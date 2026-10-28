package license_test

import (
	"context"
	"errors"
	"testing"
	"time"

	"schoolbot.local/app/internal/license"
)

func TestExplicitOnlineDenialInvalidatesCachedEntitlement(t *testing.T) {
	clk, store, dev, ver, key := setupFixtures(t)
	svc := license.NewEntitlementService(clk, ver, dev, store)
	ctx := context.Background()

	checkedAt := clk.Now().UTC().Unix()
	claims := license.EntitlementClaims{
		Issuer:           "schoolbot-license-authority",
		Audience:         "schoolbot-desktop",
		JTI:              "test-jti-004",
		LicenseID:        "license-k-004",
		SchoolID:         "school-s-004",
		BindingID:        "binding-k-004",
		BindingEpoch:     1,
		Confirmation:     license.Confirmation{JKT: dev.jkt},
		Modes:            []string{"block", "arm", "code"},
		IssuedAt:         checkedAt,
		NotBefore:        checkedAt,
		CheckedAt:        checkedAt,
		ExpiresAt:        checkedAt + license.OfflineLeaseSeconds,
		LicenseExpiresAt: checkedAt + 500000,
	}
	token := generateSignedToken(t, key, "test-kid-1", claims)

	_, err := svc.ProcessOnlineOutcome(ctx, license.OnlineOutcome{
		StatusCode: 200,
		Token:      token,
		KeyRef:     "key-ref-004",
	})
	if err != nil {
		t.Fatalf("activation failed: %v", err)
	}

	denials := []string{"LICENSE_INVALID", "LICENSE_INACTIVE", "BINDING_REVOKED"}
	for _, code := range denials {
		proj, dErr := svc.ProcessOnlineOutcome(ctx, license.OnlineOutcome{
			StatusCode: 403,
			ServerCode: code,
		})
		if proj.Status != license.StatusDenied || dErr == nil {
			t.Fatalf("code %s expected denied status, got proj=%+v err=%v", code, proj, dErr)
		}
		evalProj, _ := svc.Evaluate(ctx)
		if evalProj.Status != license.StatusDenied {
			t.Fatalf("cache was not invalidated: %+v", evalProj)
		}
	}
}

func TestLockedRecoveryCodes(t *testing.T) {
	clk, store, dev, ver, _ := setupFixtures(t)
	svc := license.NewEntitlementService(clk, ver, dev, store)
	ctx := context.Background()

	recoveryCodes := []string{"DEVICE_NOT_ACTIVATED", "DEVICE_PROOF_INVALID"}
	for _, code := range recoveryCodes {
		proj, err := svc.ProcessOnlineOutcome(ctx, license.OnlineOutcome{
			StatusCode: 403,
			ServerCode: code,
		})
		if proj.Status != license.StatusDenied || err == nil {
			t.Fatalf("expected denied for %s, got proj=%+v err=%v", code, proj, err)
		}
		var licErr *license.LicenseError
		if !errors.As(err, &licErr) || licErr.Code != code {
			t.Fatalf("expected error code %s, got %v", code, err)
		}
	}

	// Unknown 403 response must fail closed into denied recovery.
	unknownProj, unknownErr := svc.ProcessOnlineOutcome(ctx, license.OnlineOutcome{
		StatusCode: 403,
		ServerCode: "SOME_FUTURE_FORBIDDEN_CODE",
	})
	if unknownProj.Status != license.StatusDenied || unknownErr == nil {
		t.Fatalf("unknown 403 must fail closed into denied: %+v", unknownProj)
	}
}

func TestStoreFailurePreventsActivation(t *testing.T) {
	clk, store, dev, ver, key := setupFixtures(t)
	store.failSave = true
	svc := license.NewEntitlementService(clk, ver, dev, store)
	ctx := context.Background()

	checkedAt := clk.Now().UTC().Unix()
	claims := license.EntitlementClaims{
		Issuer:           "schoolbot-license-authority",
		Audience:         "schoolbot-desktop",
		JTI:              "test-jti-005",
		LicenseID:        "license-k-005",
		SchoolID:         "school-s-005",
		BindingID:        "binding-k-005",
		BindingEpoch:     1,
		Confirmation:     license.Confirmation{JKT: dev.jkt},
		Modes:            []string{"block", "arm", "code"},
		IssuedAt:         checkedAt,
		NotBefore:        checkedAt,
		CheckedAt:        checkedAt,
		ExpiresAt:        checkedAt + license.OfflineLeaseSeconds,
		LicenseExpiresAt: checkedAt + 500000,
	}
	token := generateSignedToken(t, key, "test-kid-1", claims)

	proj, err := svc.ProcessOnlineOutcome(ctx, license.OnlineOutcome{
		StatusCode: 200,
		Token:      token,
		KeyRef:     "key-ref-005",
	})
	if err == nil {
		t.Fatal("expected error on store failure")
	}
	var licErr *license.LicenseError
	if !errors.As(err, &licErr) || licErr.Code != license.ErrCredentialStoreFailed {
		t.Fatalf("expected CREDENTIAL_STORE_FAILED, got %v", err)
	}
	if proj.Status == license.StatusValid {
		t.Fatalf("service must not transition to valid when credential write fails")
	}
}

func TestNetworkErrorHandling(t *testing.T) {
	clk, store, dev, ver, key := setupFixtures(t)
	svc := license.NewEntitlementService(clk, ver, dev, store)
	ctx := context.Background()

	checkedAt := clk.Now().UTC().Unix()
	expectedExp := checkedAt + license.OfflineLeaseSeconds
	claims := license.EntitlementClaims{
		Issuer:           "schoolbot-license-authority",
		Audience:         "schoolbot-desktop",
		JTI:              "test-jti-006",
		LicenseID:        "license-k-006",
		SchoolID:         "school-s-006",
		BindingID:        "binding-k-006",
		BindingEpoch:     1,
		Confirmation:     license.Confirmation{JKT: dev.jkt},
		Modes:            []string{"block", "arm", "code"},
		IssuedAt:         checkedAt,
		NotBefore:        checkedAt,
		CheckedAt:        checkedAt,
		ExpiresAt:        expectedExp,
		LicenseExpiresAt: checkedAt + 500000,
	}
	token := generateSignedToken(t, key, "test-kid-1", claims)

	proj, err := svc.ProcessOnlineOutcome(ctx, license.OnlineOutcome{
		StatusCode: 200,
		Token:      token,
		KeyRef:     "key-ref-006",
	})
	if err != nil || proj.Status != license.StatusValid {
		t.Fatalf("initial activation failed: %v", err)
	}
	initialExp := *proj.ExpiresAt

	// 1 day passes: network error occurs during recheck attempt
	clk.Advance(24 * time.Hour)
	netProj, netErr := svc.ProcessOnlineOutcome(ctx, license.OnlineOutcome{
		StatusCode: 0,
		NetworkErr: errors.New("dial tcp: connection refused"),
	})
	if netErr != nil || netProj.Status != license.StatusValid {
		t.Fatalf("network failure during active lease should allow remaining valid lease: proj=%+v, err=%v", netProj, netErr)
	}
	if *netProj.ExpiresAt != initialExp {
		t.Fatalf("network failure must not renew or extend expiry: initial=%s, got=%s", initialExp, *netProj.ExpiresAt)
	}

	// Advance beyond expiry: network failure should no longer grant valid status
	clk.Advance(48 * time.Hour)
	expiredNetProj, expiredNetErr := svc.ProcessOnlineOutcome(ctx, license.OnlineOutcome{
		StatusCode: 503,
		NetworkErr: errors.New("service unavailable"),
	})
	if expiredNetErr == nil || expiredNetProj.Status == license.StatusValid {
		t.Fatalf("network failure after expiry must not unlock host: proj=%+v, err=%v", expiredNetProj, expiredNetErr)
	}
}

func TestSignatureAndClaimsValidation(t *testing.T) {
	clk, store, dev, ver, key := setupFixtures(t)
	svc := license.NewEntitlementService(clk, ver, dev, store)
	ctx := context.Background()

	checkedAt := clk.Now().UTC().Unix()
	baseClaims := license.EntitlementClaims{
		Issuer:           "schoolbot-license-authority",
		Audience:         "schoolbot-desktop",
		JTI:              "test-jti-007",
		LicenseID:        "license-k-007",
		SchoolID:         "school-s-007",
		BindingID:        "binding-k-007",
		BindingEpoch:     1,
		Confirmation:     license.Confirmation{JKT: dev.jkt},
		Modes:            []string{"block", "arm", "code"},
		IssuedAt:         checkedAt,
		NotBefore:        checkedAt,
		CheckedAt:        checkedAt,
		ExpiresAt:        checkedAt + license.OfflineLeaseSeconds,
		LicenseExpiresAt: checkedAt + 500000,
	}

	// Test 1: Unknown key ID (kid) fails closed
	unknownKidToken := generateSignedToken(t, key, "untrusted-kid", baseClaims)
	_, err := svc.ProcessOnlineOutcome(ctx, license.OnlineOutcome{
		StatusCode: 200,
		Token:      unknownKidToken,
		KeyRef:     "key-ref-007",
	})
	if err == nil {
		t.Fatal("unknown kid must fail verification")
	}

	// Test 2: Device JKT mismatch fails verification
	mismatchedClaims := baseClaims
	mismatchedClaims.Confirmation.JKT = "different-device-thumbprint"
	mismatchedToken := generateSignedToken(t, key, "test-kid-1", mismatchedClaims)
	_, err = svc.ProcessOnlineOutcome(ctx, license.OnlineOutcome{
		StatusCode: 200,
		Token:      mismatchedToken,
		KeyRef:     "key-ref-007",
	})
	if err == nil {
		t.Fatal("device JKT mismatch must fail verification")
	}

	// Test 3: Invalid expiry calculation fails verification
	badExpClaims := baseClaims
	badExpClaims.ExpiresAt = checkedAt + 9999999 // Exceeds canonical 3-day window
	badExpToken := generateSignedToken(t, key, "test-kid-1", badExpClaims)
	_, err = svc.ProcessOnlineOutcome(ctx, license.OnlineOutcome{
		StatusCode: 200,
		Token:      badExpToken,
		KeyRef:     "key-ref-007",
	})
	if err == nil {
		t.Fatal("invalid exp calculation must fail verification")
	}

	// Test 4: Malformed token string
	_, err = svc.ProcessOnlineOutcome(ctx, license.OnlineOutcome{
		StatusCode: 200,
		Token:      "not-a-valid-jwt-token",
		KeyRef:     "key-ref-007",
	})
	if err == nil {
		t.Fatal("malformed token must fail verification")
	}
}
