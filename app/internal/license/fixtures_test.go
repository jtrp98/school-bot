package license_test

import (
	"context"
	"crypto/ecdsa"
	"crypto/elliptic"
	"crypto/rand"
	"crypto/sha256"
	"encoding/base64"
	"encoding/json"
	"errors"
	"math/big"
	"testing"
	"time"

	"schoolbot.local/app/internal/license"
)

type testClock struct {
	wall    time.Time
	elapsed time.Duration
}

func (c *testClock) Now() time.Time {
	return c.wall
}

func (c *testClock) MonotonicElapsed() time.Duration {
	return c.elapsed
}

func (c *testClock) Advance(d time.Duration) {
	c.wall = c.wall.Add(d)
	c.elapsed += d
}

func (c *testClock) SetWall(t time.Time) {
	c.wall = t
}

type testStore struct {
	record   *license.GenerationRecord
	failSave bool
}

func (s *testStore) Load(_ context.Context) (*license.GenerationRecord, error) {
	if s.record == nil {
		return nil, nil
	}
	cp := *s.record
	return &cp, nil
}

func (s *testStore) Save(_ context.Context, record *license.GenerationRecord) error {
	if s.failSave {
		return errors.New("simulated disk write failure")
	}
	if record == nil {
		s.record = nil
		return nil
	}
	cp := *record
	s.record = &cp
	return nil
}

type testDevice struct {
	jkt string
}

func (d *testDevice) CurrentDeviceJKT(_ context.Context) (string, error) {
	return d.jkt, nil
}

type testVerifier struct {
	key *ecdsa.PrivateKey
}

func (v *testVerifier) VerifySignature(_ context.Context, signedData []byte, signature []byte, kid string) error {
	if kid != "test-kid-1" {
		return errors.New("unknown kid")
	}
	if len(signature) != 64 {
		return errors.New("invalid signature length")
	}
	r := new(big.Int).SetBytes(signature[:32])
	s := new(big.Int).SetBytes(signature[32:])
	h := sha256.Sum256(signedData)
	if !ecdsa.Verify(&v.key.PublicKey, h[:], r, s) {
		return errors.New("signature verification failed")
	}
	return nil
}

func generateSignedToken(t *testing.T, key *ecdsa.PrivateKey, kid string, claims license.EntitlementClaims) string {
	t.Helper()
	header := map[string]string{
		"typ": "schoolbot-entitlement+jwt",
		"alg": "ES256",
		"kid": kid,
	}
	headerJSON, err := json.Marshal(header)
	if err != nil {
		t.Fatalf("failed to marshal header: %v", err)
	}
	claimsJSON, err := json.Marshal(claims)
	if err != nil {
		t.Fatalf("failed to marshal claims: %v", err)
	}

	headerB64 := base64.RawURLEncoding.EncodeToString(headerJSON)
	claimsB64 := base64.RawURLEncoding.EncodeToString(claimsJSON)
	signedData := []byte(headerB64 + "." + claimsB64)

	h := sha256.Sum256(signedData)
	r, s, err := ecdsa.Sign(rand.Reader, key, h[:])
	if err != nil {
		t.Fatalf("failed to sign token: %v", err)
	}
	rBytes := r.Bytes()
	sBytes := s.Bytes()
	sig := make([]byte, 64)
	copy(sig[32-len(rBytes):32], rBytes)
	copy(sig[64-len(sBytes):64], sBytes)
	sigB64 := base64.RawURLEncoding.EncodeToString(sig)

	return headerB64 + "." + claimsB64 + "." + sigB64
}

func setupFixtures(t *testing.T) (*testClock, *testStore, *testDevice, *testVerifier, *ecdsa.PrivateKey) {
	t.Helper()
	key, err := ecdsa.GenerateKey(elliptic.P256(), rand.Reader)
	if err != nil {
		t.Fatalf("failed to generate key: %v", err)
	}
	clk := &testClock{
		wall:    time.Date(2026, 10, 26, 12, 0, 0, 0, time.UTC),
		elapsed: 10 * time.Second,
	}
	store := &testStore{}
	dev := &testDevice{jkt: "test-device-thumbprint-001"}
	ver := &testVerifier{key: key}
	return clk, store, dev, ver, key
}
