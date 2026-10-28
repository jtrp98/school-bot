package license

import (
	"context"
	"encoding/base64"
	"encoding/json"
	"strings"
)

// SignatureVerifier executes cryptographic verification on JWS payloads.
type SignatureVerifier interface {
	VerifySignature(ctx context.Context, signedData []byte, signature []byte, kid string) error
}

// DeviceVerifier supplies the trusted device JWK thumbprint.
type DeviceVerifier interface {
	CurrentDeviceJKT(ctx context.Context) (string, error)
}

type jwsHeader struct {
	Typ string `json:"typ"`
	Alg string `json:"alg"`
	Kid string `json:"kid"`
}

// VerifyEntitlementToken decodes, parses, and validates the entitlement JWS token.
// Verification enforces strict token type, ES256 algorithm, known kid, audience,
// device confirmation thumbprint, and exact 3-day expiry formula.
func VerifyEntitlementToken(ctx context.Context, rawToken string, expectedJKT string, verifier SignatureVerifier) (*EntitlementClaims, error) {
	if verifier == nil {
		return nil, &LicenseError{
			Code:    ErrVerifierUnavailable,
			Message: "signature verifier is unavailable",
		}
	}
	parts := strings.Split(rawToken, ".")
	if len(parts) != 3 {
		return nil, &LicenseError{
			Code:    ErrInvalidEntitlement,
			Message: "malformed JWS token structure",
		}
	}

	headerBytes, err := base64.RawURLEncoding.DecodeString(parts[0])
	if err != nil {
		return nil, &LicenseError{
			Code:    ErrInvalidEntitlement,
			Message: "invalid JWS header encoding",
		}
	}
	var header jwsHeader
	if err := json.Unmarshal(headerBytes, &header); err != nil {
		return nil, &LicenseError{
			Code:    ErrInvalidEntitlement,
			Message: "invalid JWS header JSON",
		}
	}

	if header.Typ != "schoolbot-entitlement+jwt" || header.Alg != "ES256" || strings.TrimSpace(header.Kid) == "" {
		return nil, &LicenseError{
			Code:    ErrInvalidEntitlement,
			Message: "unsupported or missing JWS header fields",
		}
	}

	payloadBytes, err := base64.RawURLEncoding.DecodeString(parts[1])
	if err != nil {
		return nil, &LicenseError{
			Code:    ErrInvalidEntitlement,
			Message: "invalid JWS payload encoding",
		}
	}
	var claims EntitlementClaims
	if err := json.Unmarshal(payloadBytes, &claims); err != nil {
		return nil, &LicenseError{
			Code:    ErrInvalidEntitlement,
			Message: "invalid JWS claims JSON",
		}
	}

	sigBytes, err := base64.RawURLEncoding.DecodeString(parts[2])
	if err != nil {
		return nil, &LicenseError{
			Code:    ErrInvalidEntitlement,
			Message: "invalid JWS signature encoding",
		}
	}

	signedData := []byte(parts[0] + "." + parts[1])
	if err := verifier.VerifySignature(ctx, signedData, sigBytes, header.Kid); err != nil {
		return nil, &LicenseError{
			Code:    ErrInvalidEntitlement,
			Message: "cryptographic signature verification failed",
		}
	}

	if err := ValidateClaims(&claims, expectedJKT); err != nil {
		return nil, err
	}

	return &claims, nil
}

// ValidateClaims checks semantic boundaries and exact expiry calculation.
func ValidateClaims(claims *EntitlementClaims, expectedJKT string) error {
	if claims == nil {
		return &LicenseError{
			Code:    ErrInvalidEntitlement,
			Message: "missing entitlement claims",
		}
	}
	if claims.Audience != "schoolbot-desktop" ||
		strings.TrimSpace(claims.JTI) == "" ||
		strings.TrimSpace(claims.LicenseID) == "" ||
		strings.TrimSpace(claims.SchoolID) == "" ||
		strings.TrimSpace(claims.BindingID) == "" {
		return &LicenseError{
			Code:    ErrInvalidEntitlement,
			Message: "claims missing mandatory identification fields",
		}
	}

	// Device binding must match verified device identity, preventing cross-machine token reuse.
	if expectedJKT == "" || claims.Confirmation.JKT != expectedJKT {
		return &LicenseError{
			Code:    ErrInvalidEntitlement,
			Message: "device confirmation thumbprint mismatch",
		}
	}

	if claims.CheckedAt <= 0 || claims.LicenseExpiresAt <= 0 {
		return &LicenseError{
			Code:    ErrInvalidEntitlement,
			Message: "timestamps must be strictly positive epoch seconds",
		}
	}
	if claims.IssuedAt != claims.CheckedAt || claims.NotBefore != claims.CheckedAt {
		return &LicenseError{
			Code:    ErrInvalidEntitlement,
			Message: "iat and nbf must match checked_at exactly",
		}
	}

	// Exact offline expiry calculation: min(checked_at + 259200, license_expires_at).
	expectedExp := claims.CheckedAt + OfflineLeaseSeconds
	if claims.LicenseExpiresAt < expectedExp {
		expectedExp = claims.LicenseExpiresAt
	}
	if claims.ExpiresAt != expectedExp {
		return &LicenseError{
			Code:    ErrInvalidEntitlement,
			Message: "exp does not match canonical formula",
		}
	}
	if claims.ExpiresAt <= claims.CheckedAt {
		return &LicenseError{
			Code:    ErrInvalidEntitlement,
			Message: "exp must be after checked_at",
		}
	}

	return nil
}
