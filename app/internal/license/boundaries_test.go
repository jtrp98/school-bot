package license_test

import (
	"context"
	"testing"
	"time"

	"schoolbot.local/app/internal/license"
)

func TestClockBoundariesAndExpiry(t *testing.T) {
	clk, store, dev, ver, key := setupFixtures(t)
	svc := license.NewEntitlementService(clk, ver, dev, store)
	ctx := context.Background()

	checkedAt := clk.Now().UTC().Unix()
	licenseExp := checkedAt + 1000000
	expectedExp := checkedAt + license.OfflineLeaseSeconds

	claims := license.EntitlementClaims{
		Issuer:           "schoolbot-license-authority",
		Audience:         "schoolbot-desktop",
		JTI:              "test-jti-001",
		LicenseID:        "license-k-001",
		SchoolID:         "school-s-001",
		BindingID:        "binding-k-001",
		BindingEpoch:     1,
		Confirmation:     license.Confirmation{JKT: dev.jkt},
		Modes:            []string{"block", "arm", "code"},
		IssuedAt:         checkedAt,
		NotBefore:        checkedAt,
		CheckedAt:        checkedAt,
		ExpiresAt:        expectedExp,
		LicenseExpiresAt: licenseExp,
	}
	token := generateSignedToken(t, key, "test-kid-1", claims)

	proj, err := svc.ProcessOnlineOutcome(ctx, license.OnlineOutcome{
		StatusCode: 200,
		Token:      token,
		KeyRef:     "key-ref-001",
	})
	if err != nil || proj.Status != license.StatusValid {
		t.Fatalf("expected valid status, got proj=%+v err=%v", proj, err)
	}

	// Boundary 1: exp - 1s -> must be valid
	timeUntilExpMinus1 := time.Duration(expectedExp-clk.Now().UTC().Unix()-1) * time.Second
	clk.Advance(timeUntilExpMinus1)
	proj, err = svc.Evaluate(ctx)
	if err != nil || proj.Status != license.StatusValid {
		t.Fatalf("boundary exp - 1s expected valid, got proj=%+v err=%v", proj, err)
	}

	// Boundary 2: exactly at exp -> must be expired (no grace)
	clk.Advance(1 * time.Second)
	proj, err = svc.Evaluate(ctx)
	if proj.Status != license.StatusExpired {
		t.Fatalf("boundary at exp expected expired, got proj=%+v err=%v", proj, err)
	}

	// Boundary 3: exp + 1s -> must remain expired
	clk.Advance(1 * time.Second)
	proj, err = svc.Evaluate(ctx)
	if proj.Status != license.StatusExpired {
		t.Fatalf("boundary exp + 1s expected expired, got proj=%+v err=%v", proj, err)
	}
}

func TestReopenDoesNotExtendExpiry(t *testing.T) {
	clk, store, dev, ver, key := setupFixtures(t)
	svc := license.NewEntitlementService(clk, ver, dev, store)
	ctx := context.Background()

	checkedAt := clk.Now().UTC().Unix()
	expectedExp := checkedAt + license.OfflineLeaseSeconds
	claims := license.EntitlementClaims{
		Issuer:           "schoolbot-license-authority",
		Audience:         "schoolbot-desktop",
		JTI:              "test-jti-002",
		LicenseID:        "license-k-002",
		SchoolID:         "school-s-002",
		BindingID:        "binding-k-002",
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
		KeyRef:     "key-ref-002",
	})
	if err != nil || proj.Status != license.StatusValid {
		t.Fatalf("initial activation failed: %+v", err)
	}
	originalExp := *proj.ExpiresAt

	// Simulate app closing and reopening with new process baseline
	clk.Advance(24 * time.Hour)
	newClk := &testClock{
		wall:    clk.Now(),
		elapsed: 5 * time.Second,
	}
	reopenedSvc := license.NewEntitlementService(newClk, ver, dev, store)
	reopenProj, err := reopenedSvc.Resume(ctx)
	if err != nil || reopenProj.Status != license.StatusValid {
		t.Fatalf("reopened session failed: %+v, err=%v", reopenProj, err)
	}
	if *reopenProj.ExpiresAt != originalExp {
		t.Fatalf("reopen must not extend exp: original=%s, reopened=%s", originalExp, *reopenProj.ExpiresAt)
	}
}

func TestClockRollbackEntersUntrusted(t *testing.T) {
	clk, store, dev, ver, key := setupFixtures(t)
	svc := license.NewEntitlementService(clk, ver, dev, store)
	ctx := context.Background()

	checkedAt := clk.Now().UTC().Unix()
	claims := license.EntitlementClaims{
		Issuer:           "schoolbot-license-authority",
		Audience:         "schoolbot-desktop",
		JTI:              "test-jti-003",
		LicenseID:        "license-k-003",
		SchoolID:         "school-s-003",
		BindingID:        "binding-k-003",
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
		KeyRef:     "key-ref-003",
	})
	if err != nil {
		t.Fatalf("activation failed: %v", err)
	}

	// Move wall clock backward by 1 hour (rollback)
	clk.SetWall(clk.Now().Add(-1 * time.Hour))
	proj, err := svc.Evaluate(ctx)
	if proj.Status != license.StatusClockUntrusted {
		t.Fatalf("expected CLOCK_UNTRUSTED on rollback, got proj=%+v err=%v", proj, err)
	}

	// Subsystem trusted time must also fail closed
	tt, ttErr := svc.TrustedTime(ctx)
	if tt.Trusted || ttErr == nil {
		t.Fatalf("expected trusted time failure on rollback, got tt=%+v err=%v", tt, ttErr)
	}
}
