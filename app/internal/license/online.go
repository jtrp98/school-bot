package license

import (
	"context"
	"strings"
	"time"
)

// ProcessOnlineOutcome evaluates server check results and applies state transitions.
func (s *EntitlementService) ProcessOnlineOutcome(ctx context.Context, outcome OnlineOutcome) (EntitlementProjection, error) {
	s.mu.Lock()
	defer s.mu.Unlock()

	if outcome.StatusCode == 200 {
		return s.handleOnlineActive(ctx, outcome)
	}

	if outcome.StatusCode == 403 {
		return s.handleOnlineForbidden(ctx, outcome)
	}

	// Network failures, 5xx, or 429 allow continuing under an existing valid lease without renewing.
	if outcome.NetworkErr != nil || outcome.StatusCode >= 500 || outcome.StatusCode == 429 {
		return s.handleOnlineUnavailable(ctx)
	}

	// Any other 4xx status denies activation.
	s.cachedStatus = StatusDenied
	return EntitlementProjection{Status: StatusDenied}, &LicenseError{
		Code:    ErrLicenseInvalid,
		Message: "license server rejected request",
	}
}

func (s *EntitlementService) handleOnlineActive(ctx context.Context, outcome OnlineOutcome) (EntitlementProjection, error) {
	if s.device == nil {
		return EntitlementProjection{Status: s.cachedStatus}, &LicenseError{
			Code:    ErrConfigUnavailable,
			Message: "device verifier is unavailable",
		}
	}
	expectedJKT, err := s.device.CurrentDeviceJKT(ctx)
	if err != nil || strings.TrimSpace(expectedJKT) == "" {
		return EntitlementProjection{Status: s.cachedStatus}, &LicenseError{
			Code:    ErrDeviceProofInvalid,
			Message: "unable to resolve verified device thumbprint",
		}
	}

	claims, err := VerifyEntitlementToken(ctx, outcome.Token, expectedJKT, s.verifier)
	if err != nil {
		return EntitlementProjection{Status: s.cachedStatus}, err
	}

	if s.store == nil {
		return EntitlementProjection{Status: s.cachedStatus}, &LicenseError{
			Code:    ErrStoreUnavailable,
			Message: "credential store is unavailable",
		}
	}

	if s.clock == nil {
		return EntitlementProjection{Status: s.cachedStatus}, &LicenseError{
			Code:    ErrConfigUnavailable,
			Message: "clock implementation is missing",
		}
	}

	wallNow := s.clock.Now().UTC().Unix()
	effectiveServer := claims.CheckedAt
	maxEffective := effectiveServer
	if wallNow > maxEffective {
		maxEffective = wallNow
	}
	if s.currentRecord != nil && s.currentRecord.ClockAnchor.MaxEffectiveUTC > maxEffective {
		maxEffective = s.currentRecord.ClockAnchor.MaxEffectiveUTC
	}

	nextGen := int64(1)
	if s.currentRecord != nil {
		nextGen = s.currentRecord.Generation + 1
	}

	newRecord := &GenerationRecord{
		Generation: nextGen,
		Token:      outcome.Token,
		KeyRef:     outcome.KeyRef,
		ClockAnchor: ClockAnchor{
			ServerTimeUTC:   claims.CheckedAt,
			ObservedWallUTC: wallNow,
			MaxEffectiveUTC: maxEffective,
		},
		Status: StatusValid,
	}

	if err := s.store.Save(ctx, newRecord); err != nil {
		return EntitlementProjection{Status: s.cachedStatus}, &LicenseError{
			Code:    ErrCredentialStoreFailed,
			Message: "failed to persist entitlement generation",
		}
	}

	s.currentRecord = newRecord
	s.currentClaims = claims
	s.cachedStatus = StatusValid
	s.sessionBaseMono = s.clock.MonotonicElapsed()
	s.hasSessionBase = true

	expStr := time.Unix(claims.ExpiresAt, 0).UTC().Format(time.RFC3339)
	return EntitlementProjection{
		Status:    StatusValid,
		ExpiresAt: &expStr,
	}, nil
}

func (s *EntitlementService) handleOnlineForbidden(ctx context.Context, outcome OnlineOutcome) (EntitlementProjection, error) {
	code := strings.TrimSpace(outcome.ServerCode)
	switch code {
	case "LICENSE_INVALID", "LICENSE_INACTIVE", "BINDING_REVOKED":
		s.invalidateLocalCache(ctx, StatusDenied)
		return EntitlementProjection{Status: StatusDenied}, &LicenseError{
			Code:    code,
			Message: "license is no longer active",
		}
	case "DEVICE_NOT_ACTIVATED", "DEVICE_PROOF_INVALID":
		s.invalidateLocalCache(ctx, StatusDenied)
		return EntitlementProjection{Status: StatusDenied}, &LicenseError{
			Code:    code,
			Message: "device is not activated or proof invalid",
		}
	default:
		// Unknown 403 response must fail closed into denied recovery.
		s.invalidateLocalCache(ctx, StatusDenied)
		return EntitlementProjection{Status: StatusDenied}, &LicenseError{
			Code:    ErrLicenseInvalid,
			Message: "unknown denial response from license server",
		}
	}
}

func (s *EntitlementService) handleOnlineUnavailable(ctx context.Context) (EntitlementProjection, error) {
	proj, err := s.evaluateLocked(ctx)
	if err == nil && proj.Status == StatusValid {
		return proj, nil
	}
	return proj, &LicenseError{
		Code:    ErrStoreUnavailable,
		Message: "license server unreachable and local entitlement is expired or unactivated",
	}
}

func (s *EntitlementService) invalidateLocalCache(ctx context.Context, status string) {
	s.cachedStatus = status
	s.currentClaims = nil
	if s.store != nil {
		nextGen := int64(1)
		var anchor ClockAnchor
		if s.currentRecord != nil {
			nextGen = s.currentRecord.Generation + 1
			anchor = s.currentRecord.ClockAnchor
		}
		s.currentRecord = &GenerationRecord{
			Generation:  nextGen,
			Token:       "",
			KeyRef:      "",
			ClockAnchor: anchor,
			Status:      status,
		}
		_ = s.store.Save(ctx, s.currentRecord)
	}
}
