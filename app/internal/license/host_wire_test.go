package license_test

import (
	"bytes"
	"context"
	"crypto/ecdsa"
	"crypto/elliptic"
	"crypto/sha256"
	"encoding/base64"
	"encoding/hex"
	"encoding/json"
	"errors"
	"io"
	"math/big"
	"net/http"
	"strings"
	"testing"
	"time"

	"schoolbot.local/app/internal/license"
)

type transportFunc func(*http.Request) (*http.Response, error)

func (f transportFunc) RoundTrip(request *http.Request) (*http.Response, error) {
	return f(request)
}

func TestHostWireAndSecureGenerationSelfCheck(t *testing.T) {
	clock, _, _, verifier, key := setupFixtures(t)
	storage := &blobNamespace{}
	online, denied := true, false
	checkedAt := clock.wall.Unix()
	client := &http.Client{Transport: transportFunc(func(request *http.Request) (*http.Response, error) {
		if !online {
			return nil, io.ErrClosedPipe
		}
		data, err := io.ReadAll(request.Body)
		if err != nil {
			t.Fatal(err)
		}
		var body map[string]json.RawMessage
		if err := json.Unmarshal(data, &body); err != nil {
			t.Fatal(err)
		}
		var response []byte
		status := 200
		if strings.HasSuffix(request.URL.Path, "device-challenges") {
			response = []byte(`{"challenge":"test-challenge"}`)
		} else if denied {
			status = 403
			response = []byte(`{"error":{"code":"LICENSE_INACTIVE"}}`)
		} else {
			var jwk struct {
				X string
				Y string
			}
			if request.URL.Path == "/v2/licenses/activate" {
				if err := json.Unmarshal(body["device_jwk"], &jwk); err != nil {
					t.Fatal(err)
				}
			}
			var proof string
			if err := json.Unmarshal(body["proof"], &proof); err != nil {
				t.Fatal(err)
			}
			parts := strings.Split(proof, ".")
			if len(parts) != 3 {
				t.Fatal("invalid proof")
			}
			if jwk.X != "" {
				x, _ := base64.RawURLEncoding.DecodeString(jwk.X)
				y, _ := base64.RawURLEncoding.DecodeString(jwk.Y)
				public := &ecdsa.PublicKey{Curve: elliptic.P256(), X: new(big.Int).SetBytes(x), Y: new(big.Int).SetBytes(y)}
				digest := sha256.Sum256([]byte(parts[0] + "." + parts[1]))
				signature, _ := base64.RawURLEncoding.DecodeString(parts[2])
				if len(signature) != 64 || !ecdsa.Verify(public, digest[:], new(big.Int).SetBytes(signature[:32]), new(big.Int).SetBytes(signature[32:])) {
					t.Fatal("invalid device proof signature")
				}
			}
			delete(body, "proof")
			canonical, err := json.Marshal(body)
			if err != nil {
				t.Fatal(err)
			}
			hash := sha256.Sum256(canonical)
			payload, _ := base64.RawURLEncoding.DecodeString(parts[1])
			var claims struct {
				BodyHash string `json:"body_hash"`
				Path     string `json:"path"`
				Key      string `json:"idempotency_key"`
			}
			if json.Unmarshal(payload, &claims) != nil || claims.BodyHash != hex.EncodeToString(hash[:]) || claims.Path != request.URL.Path || claims.Key != request.Header.Get("Idempotency-Key") {
				t.Fatal("proof is not bound to request")
			}
			credentials, err := license.OpenCredentials(context.Background(), storage)
			if err != nil {
				t.Fatal(err)
			}
			jkt, _ := credentials.CurrentDeviceJKT(context.Background())
			entitlement := license.EntitlementClaims{
				Issuer:           "schoolbot-api",
				Audience:         "schoolbot-desktop",
				JTI:              "test-lease",
				LicenseID:        1,
				SchoolID:         100,
				BindingID:        1,
				BindingEpoch:     1,
				Confirmation:     license.Confirmation{JKT: jkt},
				Modes:            []string{"block", "arm", "code"},
				IssuedAt:         checkedAt,
				NotBefore:        checkedAt,
				CheckedAt:        checkedAt,
				ExpiresAt:        checkedAt + 259200,
				LicenseExpiresAt: checkedAt + 864000,
			}
			response, err = json.Marshal(map[string]interface{}{
				"valid":       true,
				"license_id":  1,
				"binding_id":  1,
				"checked_at":  time.Unix(checkedAt, 0).UTC(),
				"expires_at":  time.Unix(checkedAt+259200, 0).UTC(),
				"entitlement": generateSignedToken(t, key, "test-kid-1", entitlement),
			})
			if err != nil {
				t.Fatal(err)
			}
		}
		return &http.Response{StatusCode: status, Body: io.NopCloser(bytes.NewReader(response)), Header: make(http.Header)}, nil
	})}
	recreate := func() *license.Host {
		return license.NewHost("https://fixture.invalid", client, clock, verifier, storage)
	}
	host := recreate()
	projection, err := host.Check(context.Background(), "SBOT-test", false)
	if err != nil || projection.Status != license.StatusValid {
		t.Fatalf("signed activation failed: %v", err)
	}
	online = false
	clock.Advance(24 * time.Hour)
	host = recreate()
	if err := host.Authorize(context.Background(), "robot-control"); err != nil {
		t.Fatal(err)
	}
	online = true
	checkedAt = clock.wall.Unix()
	if _, err := host.Check(context.Background(), "SBOT-test", true); err != nil {
		t.Fatal(err)
	}
	denied = true
	projection, err = host.Check(context.Background(), "SBOT-test", true)
	var failure *license.LicenseError
	if !errors.As(err, &failure) || failure.Code != license.ErrLicenseInactive || projection.Status != license.StatusDenied {
		t.Fatal("denial not enforced")
	}
	online = false
	host = recreate()
	if err := host.Authorize(context.Background(), "robot-control"); err == nil {
		t.Fatal("restart bypassed persisted denial")
	}
}
