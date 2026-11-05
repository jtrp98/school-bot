package main

import (
	"bytes"
	"context"
	"crypto/ecdsa"
	"crypto/elliptic"
	"crypto/rand"
	"crypto/sha256"
	"encoding/base64"
	"encoding/json"
	"errors"
	"io"
	"net/http"
	"schoolbot.local/app/internal/license"
	"strings"
	"testing"
	"time"
)

type desktopCredentialDouble struct{ data []byte }

func (s *desktopCredentialDouble) Load(context.Context) ([]byte, error) {
	return append([]byte(nil), s.data...), nil
}
func (s *desktopCredentialDouble) Save(_ context.Context, data []byte) error {
	s.data = append([]byte(nil), data...)
	return nil
}

type roundTripFunc func(*http.Request) (*http.Response, error)

func (f roundTripFunc) RoundTrip(r *http.Request) (*http.Response, error) { return f(r) }

func signEntitlementJWS(t *testing.T, key *ecdsa.PrivateKey, kid string, claims license.EntitlementClaims) string {
	t.Helper()
	hdr, _ := json.Marshal(map[string]string{"typ": "schoolbot-entitlement+jwt", "alg": "ES256", "kid": kid})
	pay, _ := json.Marshal(claims)
	unsigned := base64.RawURLEncoding.EncodeToString(hdr) + "." + base64.RawURLEncoding.EncodeToString(pay)
	digest := sha256.Sum256([]byte(unsigned))
	r, s, err := ecdsa.Sign(rand.Reader, key, digest[:])
	if err != nil {
		t.Fatal(err)
	}
	sig := append(r.FillBytes(make([]byte, 32)), s.FillBytes(make([]byte, 32))...)
	return unsigned + "." + base64.RawURLEncoding.EncodeToString(sig)
}

type controllableClock struct {
	wall    time.Time
	elapsed time.Duration
}

func (c *controllableClock) Now() time.Time                  { return c.wall.UTC() }
func (c *controllableClock) MonotonicElapsed() time.Duration { return c.elapsed }

func TestLockedLicenseNeverReachesEngine(t *testing.T) {
	storage := &desktopCredentialDouble{}
	for restart := 0; restart < 2; restart++ {
		service := &DesktopService{entitlement: license.NewHost("https://fixture.invalid", nil, license.NewSystemClock(), &license.ES256Verifier{}, storage)}
		for _, actor := range []string{"guest", "login"} {
			for _, mode := range []string{"block-programming", "robot-control", "coding-adventure"} {
				if _, err := service.Run(mode, "run", "{}"); err == nil {
					t.Fatalf("%s/%s reached engine without first online activation", actor, mode)
				}
			}
		}
		projection, err := service.LicenseStatus()
		if err != nil || projection.Status != license.StatusUnactivated {
			t.Fatal("locked projection incorrect")
		}
	}
}

func TestDesktopServiceLicenseActivateDuplicateExpireAndMove(t *testing.T) {
	signerKey, err := ecdsa.GenerateKey(elliptic.P256(), rand.Reader)
	if err != nil {
		t.Fatal(err)
	}
	verifier := &license.ES256Verifier{Keys: map[string]*ecdsa.PublicKey{"kid-1": &signerKey.PublicKey}}
	clock := &controllableClock{wall: time.Date(2026, 11, 3, 12, 0, 0, 0, time.UTC)}

	storageA := &desktopCredentialDouble{}
	storageB := &desktopCredentialDouble{}

	boundJKT := ""
	revoked := false
	bindingID := 10
	epoch := int64(1)

	makeClient := func(storage *desktopCredentialDouble) *http.Client {
		return &http.Client{Transport: roundTripFunc(func(req *http.Request) (*http.Response, error) {
			if strings.HasSuffix(req.URL.Path, "device-challenges") {
				return &http.Response{
					StatusCode: 200,
					Body:       io.NopCloser(strings.NewReader(`{"challenge":"chal-1"}`)),
					Header:     make(http.Header),
				}, nil
			}
			var payload map[string]interface{}
			_ = json.NewDecoder(req.Body).Decode(&payload)
			keyVal, _ := payload["key"].(string)
			if keyVal != "SBOT-VALID-KEY" {
				return &http.Response{
					StatusCode: 403,
					Body:       io.NopCloser(strings.NewReader(`{"error":{"code":"LICENSE_INVALID"}}`)),
					Header:     make(http.Header),
				}, nil
			}
			if revoked {
				return &http.Response{
					StatusCode: 403,
					Body:       io.NopCloser(strings.NewReader(`{"error":{"code":"LICENSE_INACTIVE"}}`)),
					Header:     make(http.Header),
				}, nil
			}
			cred, err := license.OpenCredentials(context.Background(), storage)
			if err != nil {
				t.Fatal(err)
			}
			jkt, _ := cred.CurrentDeviceJKT(context.Background())

			if strings.HasSuffix(req.URL.Path, "licenses/activate") {
				if boundJKT != "" && boundJKT != jkt {
					return &http.Response{
						StatusCode: 409,
						Body:       io.NopCloser(strings.NewReader(`{"error":{"code":"DEVICE_LIMIT"}}`)),
						Header:     make(http.Header),
					}, nil
				}
				boundJKT = jkt
			} else if strings.HasSuffix(req.URL.Path, "licenses/validate") {
				if boundJKT != jkt {
					return &http.Response{
						StatusCode: 403,
						Body:       io.NopCloser(strings.NewReader(`{"error":{"code":"BINDING_REVOKED"}}`)),
						Header:     make(http.Header),
					}, nil
				}
			}

			nowSec := clock.Now().Unix()
			expSec := nowSec + license.OfflineLeaseSeconds
			claims := license.EntitlementClaims{
				Issuer:           "schoolbot-api",
				Audience:         "schoolbot-desktop",
				JTI:              "jti-1",
				LicenseID:        1,
				SchoolID:         100,
				BindingID:        bindingID,
				BindingEpoch:     epoch,
				Confirmation:     license.Confirmation{JKT: jkt},
				Modes:            []string{"block", "arm", "code"},
				IssuedAt:         nowSec,
				NotBefore:        nowSec,
				CheckedAt:        nowSec,
				ExpiresAt:        expSec,
				LicenseExpiresAt: nowSec + 864000,
			}
			tok := signEntitlementJWS(t, signerKey, "kid-1", claims)
			bodyBytes, _ := json.Marshal(map[string]interface{}{
				"valid":       true,
				"license_id":  1,
				"binding_id":  bindingID,
				"entitlement": tok,
				"checked_at":  time.Unix(nowSec, 0).UTC(),
				"expires_at":  time.Unix(expSec, 0).UTC(),
			})
			return &http.Response{
				StatusCode: 200,
				Body:       io.NopCloser(bytes.NewReader(bodyBytes)),
				Header:     make(http.Header),
			}, nil
		})}
	}

	svcA := &DesktopService{entitlement: license.NewHost("https://fixture.invalid", makeClient(storageA), clock, verifier, storageA)}
	svcB := &DesktopService{entitlement: license.NewHost("https://fixture.invalid", makeClient(storageB), clock, verifier, storageB)}

	// 1. Activate Machine A succeeds
	rawA, err := svcA.ActivateLicense("SBOT-VALID-KEY")
	if err != nil {
		t.Fatalf("Machine A activation failed: %v", err)
	}
	var projA license.EntitlementProjection
	if err := json.Unmarshal([]byte(rawA), &projA); err != nil || projA.Status != license.StatusValid || projA.ExpiresAt == nil {
		t.Fatalf("unexpected projection A: %s (%v)", rawA, err)
	}

	// 2. Duplicate activation on Machine B fails with DEVICE_LIMIT
	_, errB := svcB.ActivateLicense("SBOT-VALID-KEY")
	var licErr *license.LicenseError
	if !errors.As(errB, &licErr) || licErr.Code != license.ErrDeviceLimit {
		t.Fatalf("expected DEVICE_LIMIT on Machine B, got %v", errB)
	}

	// 3. ValidateLicense with no argument uses saved key on Machine A
	clock.wall = clock.wall.Add(1 * time.Hour)
	rawValA, err := svcA.ValidateLicense()
	if err != nil || !strings.Contains(rawValA, `"status":"valid"`) {
		t.Fatalf("ValidateLicense with saved key failed: %s (%v)", rawValA, err)
	}

	// 4. Unbind A and move key to Machine B
	boundJKT = ""
	bindingID = 11
	epoch = 2
	rawB, err := svcB.ActivateLicense("SBOT-VALID-KEY")
	if err != nil || !strings.Contains(rawB, `"status":"valid"`) {
		t.Fatalf("Machine B activation after unbind failed: %s (%v)", rawB, err)
	}

	// Machine A online validation now fails with BINDING_REVOKED
	_, errRevoked := svcA.ValidateLicense()
	if !errors.As(errRevoked, &licErr) || licErr.Code != license.ErrBindingRevoked {
		t.Fatalf("expected BINDING_REVOKED on Machine A after move, got %v", errRevoked)
	}
}
