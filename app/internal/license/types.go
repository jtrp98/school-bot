package license

import (
	"fmt"
	"time"
)

// Standard entitlement statuses exposed to the application.
const (
	StatusUnactivated    = "unactivated"
	StatusValid          = "valid"
	StatusExpired        = "expired"
	StatusDenied         = "denied"
	StatusClockUntrusted = "clock_untrusted"
)

// Standard controlled error codes.
const (
	ErrInvalidEntitlement    = "INVALID_ENTITLEMENT"
	ErrCredentialStoreFailed = "CREDENTIAL_STORE_FAILED"
	ErrClockUntrusted        = "CLOCK_UNTRUSTED"
	ErrDeviceNotActivated    = "DEVICE_NOT_ACTIVATED"
	ErrDeviceProofInvalid    = "DEVICE_PROOF_INVALID"
	ErrLicenseInvalid        = "LICENSE_INVALID"
	ErrLicenseInactive       = "LICENSE_INACTIVE"
	ErrBindingRevoked        = "BINDING_REVOKED"
	ErrStoreUnavailable      = "STORE_UNAVAILABLE"
	ErrConfigUnavailable     = "CONFIG_UNAVAILABLE"
	ErrVerifierUnavailable   = "VERIFIER_UNAVAILABLE"
)

// OfflineLeaseSeconds defines the standard 3-day duration (3 * 24 * 3600).
const OfflineLeaseSeconds int64 = 259200

// LicenseError represents a controlled domain error.
type LicenseError struct {
	Code    string `json:"code"`
	Message string `json:"message"`
}

func (e *LicenseError) Error() string {
	return fmt.Sprintf("[%s] %s", e.Code, e.Message)
}

// EntitlementProjection is the strict UI-facing state without internal claims or tokens.
type EntitlementProjection struct {
	Status    string  `json:"status"`
	ExpiresAt *string `json:"expires_at,omitempty"`
}

// EntitlementClaims matches the verified JWS claims envelope.
type EntitlementClaims struct {
	Issuer           string       `json:"iss"`
	Audience         string       `json:"aud"`
	JTI              string       `json:"jti"`
	LicenseID        string       `json:"license_id"`
	SchoolID         string       `json:"school_id"`
	BindingID        string       `json:"binding_id"`
	BindingEpoch     int64        `json:"binding_epoch"`
	Confirmation     Confirmation `json:"cnf"`
	Modes            []string     `json:"modes"`
	IssuedAt         int64        `json:"iat"`
	NotBefore        int64        `json:"nbf"`
	CheckedAt        int64        `json:"checked_at"`
	ExpiresAt        int64        `json:"exp"`
	LicenseExpiresAt int64        `json:"license_expires_at"`
}

// Confirmation holds the device JWK thumbprint.
type Confirmation struct {
	JKT string `json:"jkt"`
}

// ClockAnchor holds verified server time, observed wall time, and high-water mark.
type ClockAnchor struct {
	ServerTimeUTC   int64 `json:"server_time_utc"`
	ObservedWallUTC int64 `json:"observed_wall_utc"`
	MaxEffectiveUTC int64 `json:"max_effective_utc"`
}

// GenerationRecord is the atomic persistence unit for local entitlement.
type GenerationRecord struct {
	Generation  int64       `json:"generation"`
	Token       string      `json:"token"`
	KeyRef      string      `json:"key_ref"`
	ClockAnchor ClockAnchor `json:"clock_anchor"`
	Status      string      `json:"status"`
}

// OnlineOutcome captures raw response information from the cloud check.
type OnlineOutcome struct {
	StatusCode int
	ServerCode string
	Token      string
	KeyRef     string
	NetworkErr error
}

// TrustedTimeProjection is consumed by host subsystems (such as trash lifecycle).
type TrustedTimeProjection struct {
	CurrentUTC time.Time `json:"current_utc"`
	Trusted    bool      `json:"trusted"`
	ErrorCode  string    `json:"error_code,omitempty"`
}
