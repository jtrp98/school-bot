package license

import (
	"context"
	"sync"
	"time"
)

// EntitlementService coordinates state transitions, online checks, and clock boundaries.
type EntitlementService struct {
	mu       sync.Mutex
	clock    Clock
	verifier SignatureVerifier
	device   DeviceVerifier
	store    GenerationStore

	currentRecord   *GenerationRecord
	currentClaims   *EntitlementClaims
	cachedStatus    string
	sessionBaseMono time.Duration
	hasSessionBase  bool
}

// NewEntitlementService constructs an unactivated service instance.
func NewEntitlementService(clock Clock, verifier SignatureVerifier, device DeviceVerifier, store GenerationStore) *EntitlementService {
	return &EntitlementService{
		clock:        clock,
		verifier:     verifier,
		device:       device,
		store:        store,
		cachedStatus: StatusUnactivated,
	}
}

// Evaluate determines current entitlement status from local state and clock progression.
func (s *EntitlementService) Evaluate(ctx context.Context) (EntitlementProjection, error) {
	s.mu.Lock()
	defer s.mu.Unlock()
	return s.evaluateLocked(ctx)
}

// Resume handles startup and session wake events by verifying clock progression before advancing.
func (s *EntitlementService) Resume(ctx context.Context) (EntitlementProjection, error) {
	return s.Evaluate(ctx)
}

// CurrentStatus returns the currently cached state name.
func (s *EntitlementService) CurrentStatus() string {
	s.mu.Lock()
	defer s.mu.Unlock()
	return s.cachedStatus
}

// TrustedTime returns verified time for dependent subsystems such as trash lifecycle.
func (s *EntitlementService) TrustedTime(ctx context.Context) (TrustedTimeProjection, error) {
	s.mu.Lock()
	defer s.mu.Unlock()

	if s.clock == nil {
		return TrustedTimeProjection{
			CurrentUTC: time.Time{},
			Trusted:    false,
			ErrorCode:  ErrConfigUnavailable,
		}, &LicenseError{
			Code:    ErrConfigUnavailable,
			Message: "clock implementation is missing",
		}
	}

	s.ensureSessionBase()
	monoElapsed := s.clock.MonotonicElapsed() - s.sessionBaseMono
	wallNow := s.clock.Now().UTC().Unix()

	var anchor *ClockAnchor
	if s.currentRecord != nil {
		anchor = &s.currentRecord.ClockAnchor
	}
	effectiveTime, err := EvaluateClock(wallNow, monoElapsed, anchor)
	if err != nil {
		s.cachedStatus = StatusClockUntrusted
		return TrustedTimeProjection{
			CurrentUTC: time.Time{},
			Trusted:    false,
			ErrorCode:  ErrClockUntrusted,
		}, err
	}

	return TrustedTimeProjection{
		CurrentUTC: time.Unix(effectiveTime, 0).UTC(),
		Trusted:    true,
	}, nil
}

func (s *EntitlementService) ensureSessionBase() {
	if !s.hasSessionBase && s.clock != nil {
		s.sessionBaseMono = s.clock.MonotonicElapsed()
		s.hasSessionBase = true
	}
}

func (s *EntitlementService) evaluateLocked(ctx context.Context) (EntitlementProjection, error) {
	if s.store == nil {
		s.cachedStatus = StatusUnactivated
		return EntitlementProjection{Status: StatusUnactivated}, nil
	}

	if s.currentRecord == nil {
		rec, err := s.store.Load(ctx)
		if err != nil {
			return EntitlementProjection{Status: s.cachedStatus}, &LicenseError{
				Code:    ErrStoreUnavailable,
				Message: "failed to load credential store",
			}
		}
		s.currentRecord = rec
	}

	if s.currentRecord == nil || s.currentRecord.Status == StatusUnactivated {
		s.cachedStatus = StatusUnactivated
		return EntitlementProjection{Status: StatusUnactivated}, nil
	}

	if s.currentRecord.Status == StatusDenied {
		s.cachedStatus = StatusDenied
		return EntitlementProjection{Status: StatusDenied}, &LicenseError{
			Code:    ErrLicenseInvalid,
			Message: "cached entitlement is marked denied",
		}
	}

	if s.currentRecord.Token == "" {
		s.cachedStatus = StatusUnactivated
		return EntitlementProjection{Status: StatusUnactivated}, nil
	}

	if s.clock == nil {
		return EntitlementProjection{Status: s.cachedStatus}, &LicenseError{
			Code:    ErrConfigUnavailable,
			Message: "clock implementation is missing",
		}
	}

	s.ensureSessionBase()
	monoElapsed := s.clock.MonotonicElapsed() - s.sessionBaseMono
	wallNow := s.clock.Now().UTC().Unix()

	effectiveTime, err := EvaluateClock(wallNow, monoElapsed, &s.currentRecord.ClockAnchor)
	if err != nil {
		s.cachedStatus = StatusClockUntrusted
		return EntitlementProjection{Status: StatusClockUntrusted}, err
	}

	if s.currentClaims == nil {
		if s.device == nil {
			return EntitlementProjection{Status: s.cachedStatus}, &LicenseError{
				Code:    ErrConfigUnavailable,
				Message: "device verifier is unavailable",
			}
		}
		expectedJKT, err := s.device.CurrentDeviceJKT(ctx)
		if err != nil {
			return EntitlementProjection{Status: s.cachedStatus}, &LicenseError{
				Code:    ErrDeviceProofInvalid,
				Message: "failed to obtain device thumbprint",
			}
		}
		claims, err := VerifyEntitlementToken(ctx, s.currentRecord.Token, expectedJKT, s.verifier)
		if err != nil {
			s.cachedStatus = StatusDenied
			return EntitlementProjection{Status: StatusDenied}, err
		}
		s.currentClaims = claims
	}

	expStr := time.Unix(s.currentClaims.ExpiresAt, 0).UTC().Format(time.RFC3339)

	// Boundary check: at or past exp means expired without grace.
	if effectiveTime >= s.currentClaims.ExpiresAt {
		s.cachedStatus = StatusExpired
		return EntitlementProjection{
			Status:    StatusExpired,
			ExpiresAt: &expStr,
		}, nil
	}

	// Advance and persist high-water mark before granting valid operation access.
	s.currentRecord.ClockAnchor.ObservedWallUTC = wallNow
	s.currentRecord.ClockAnchor.MaxEffectiveUTC = effectiveTime
	s.sessionBaseMono = s.clock.MonotonicElapsed()
	_ = s.store.Save(ctx, s.currentRecord)

	s.cachedStatus = StatusValid
	return EntitlementProjection{
		Status:    StatusValid,
		ExpiresAt: &expStr,
	}, nil
}
