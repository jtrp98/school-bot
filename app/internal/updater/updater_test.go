package updater

import (
	"context"
	"crypto/ecdsa"
	"crypto/elliptic"
	"crypto/rand"
	"crypto/sha256"
	"encoding/base64"
	"encoding/hex"
	"encoding/json"
	"errors"
	"net/http"
	"net/http/httptest"
	"os"
	"path/filepath"
	"sync/atomic"
	"testing"
	"time"
)

func signTestManifest(t *testing.T, key *ecdsa.PrivateKey, kid string, typ string, m ReleaseManifest) string {
	t.Helper()
	hdrBytes, err := json.Marshal(map[string]string{
		"typ": typ,
		"alg": "ES256",
		"kid": kid,
	})
	if err != nil {
		t.Fatal(err)
	}
	payloadBytes, err := json.Marshal(m)
	if err != nil {
		t.Fatal(err)
	}
	unsigned := base64.RawURLEncoding.EncodeToString(hdrBytes) + "." + base64.RawURLEncoding.EncodeToString(payloadBytes)
	digest := sha256.Sum256([]byte(unsigned))
	r, s, err := ecdsa.Sign(rand.Reader, key, digest[:])
	if err != nil {
		t.Fatal(err)
	}
	sig := append(r.FillBytes(make([]byte, 32)), s.FillBytes(make([]byte, 32))...)
	return unsigned + "." + base64.RawURLEncoding.EncodeToString(sig)
}

type fakeApplier struct {
	err    error
	called int32
}

func (f *fakeApplier) ApplyStagedUpdate(_ context.Context, _ string, _ ReleaseManifest) error {
	atomic.AddInt32(&f.called, 1)
	return f.err
}

type fakeReporter struct {
	version string
	reason  error
}

func (f *fakeReporter) ReportFailedInstall(_ context.Context, version string, reason error) {
	f.version = version
	f.reason = reason
}

func TestCheckUpdatesMetadataOnlyDoesNotDownloadArtifact(t *testing.T) {
	priv, err := ecdsa.GenerateKey(elliptic.P256(), rand.Reader)
	if err != nil {
		t.Fatal(err)
	}
	payload := []byte("signed-installer-binary-payload-v1.2.0")
	sum := sha256.Sum256(payload)
	var artifactHits int32

	var srv *httptest.Server
	srv = httptest.NewServer(http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		switch r.URL.Path {
		case "/api/service-school-bot/updates/check":
			m := ReleaseManifest{
				Publisher:   "SchoolBright",
				Version:     "1.2.0",
				Channel:     "stable",
				Platform:    "windows-amd64",
				URL:         srv.URL + "/artifacts/schoolbot-1.2.0.msi",
				SHA256:      hex.EncodeToString(sum[:]),
				Size:        int64(len(payload)),
				PublishedAt: "2026-11-01T00:00:00Z",
			}
			jws := signTestManifest(t, priv, "rel-key-1", "schoolbot-release+jws", m)
			_ = json.NewEncoder(w).Encode(map[string]any{
				"available":         true,
				"install_supported": false,
				"release":           m,
				"manifest_jws":      jws,
			})
		case "/artifacts/schoolbot-1.2.0.msi":
			atomic.AddInt32(&artifactHits, 1)
			_, _ = w.Write(payload)
		default:
			w.WriteHeader(http.StatusNotFound)
		}
	}))
	defer srv.Close()

	dataDir := t.TempDir()
	client := NewClient(Config{
		BaseURL:        srv.URL,
		DataDir:        dataDir,
		CurrentVersion: "1.0.0",
		Channel:        "stable",
		Platform:       "windows-amd64",
		ExpectedPub:    "SchoolBright",
		AllowLocalHTTP: true,
		Verifier: &ES256ReleaseVerifier{
			Keys: map[string]*ecdsa.PublicKey{"rel-key-1": &priv.PublicKey},
		},
		NowFunc: func() time.Time { return time.Date(2026, 11, 2, 0, 0, 0, 0, time.UTC) },
	})

	resp, err := client.CheckUpdates(context.Background())
	if err != nil {
		t.Fatalf("CheckUpdates failed: %v", err)
	}
	if !resp.Available || resp.InstallSupported || resp.Release == nil || resp.Release.Version != "1.2.0" {
		t.Fatalf("unexpected check response: %+v", resp)
	}
	if atomic.LoadInt32(&artifactHits) != 0 {
		t.Fatalf("CheckUpdates must be metadata-only and never download artifact, hits=%d", artifactHits)
	}
}

func TestPrepareAndInstallUpdateVerifyTrustAndPreventTOCTOU(t *testing.T) {
	priv, err := ecdsa.GenerateKey(elliptic.P256(), rand.Reader)
	if err != nil {
		t.Fatal(err)
	}
	otherPriv, err := ecdsa.GenerateKey(elliptic.P256(), rand.Reader)
	if err != nil {
		t.Fatal(err)
	}

	payload := []byte("verified-release-binary-content-1.3.0")
	sum := sha256.Sum256(payload)

	var srv *httptest.Server
	srv = httptest.NewServer(http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		if r.URL.Path == "/artifacts/schoolbot-1.3.0.msi" {
			_, _ = w.Write(payload)
			return
		}
		w.WriteHeader(http.StatusNotFound)
	}))
	defer srv.Close()

	dataDir := t.TempDir()
	applier := &fakeApplier{}
	reporter := &fakeReporter{}
	client := NewClient(Config{
		BaseURL:        srv.URL,
		DataDir:        dataDir,
		CurrentVersion: "1.0.0",
		Channel:        "stable",
		Platform:       "windows-amd64",
		ExpectedPub:    "SchoolBright",
		AllowLocalHTTP: true,
		Verifier: &ES256ReleaseVerifier{
			Keys: map[string]*ecdsa.PublicKey{"rel-key-1": &priv.PublicKey},
		},
		Applier:          applier,
		RecoveryReporter: reporter,
		NowFunc:          func() time.Time { return time.Date(2026, 11, 2, 0, 0, 0, 0, time.UTC) },
	})

	validManifest := ReleaseManifest{
		Publisher:   "SchoolBright",
		Version:     "1.3.0",
		Channel:     "stable",
		Platform:    "windows-amd64",
		URL:         srv.URL + "/artifacts/schoolbot-1.3.0.msi",
		SHA256:      hex.EncodeToString(sum[:]),
		Size:        int64(len(payload)),
		PublishedAt: "2026-11-01T00:00:00Z",
	}

	// 1. Untrusted key rejected before download
	untrustedJWS := signTestManifest(t, otherPriv, "rel-key-1", "schoolbot-release+jws", validManifest)
	if _, err := client.PrepareUpdate(context.Background(), untrustedJWS); err == nil {
		t.Fatal("expected untrusted signature rejection")
	}

	// 2. Downgrade rejected
	downgradeManifest := validManifest
	downgradeManifest.Version = "0.9.0"
	downgradeJWS := signTestManifest(t, priv, "rel-key-1", "schoolbot-release+jws", downgradeManifest)
	if _, err := client.PrepareUpdate(context.Background(), downgradeJWS); err == nil {
		t.Fatal("expected downgrade rejection")
	}

	// 3. SHA-256 mismatch rejected and cleaned up
	badHashManifest := validManifest
	badHashManifest.SHA256 = "0000000000000000000000000000000000000000000000000000000000000000"
	badHashJWS := signTestManifest(t, priv, "rel-key-1", "schoolbot-release+jws", badHashManifest)
	if _, err := client.PrepareUpdate(context.Background(), badHashJWS); err == nil {
		t.Fatal("expected SHA-256 mismatch rejection")
	}
	if _, err := os.Stat(filepath.Join(dataDir, "updates", "staging", "1.3.0", "payload.bin")); !os.IsNotExist(err) {
		t.Fatal("mismatched artifact must not remain staged")
	}

	// 4. Valid Prepare succeeds
	validJWS := signTestManifest(t, priv, "rel-key-1", "schoolbot-release+jws", validManifest)
	prep, err := client.PrepareUpdate(context.Background(), validJWS)
	if err != nil || prep.Version != "1.3.0" {
		t.Fatalf("PrepareUpdate failed: %v", err)
	}

	// 5. Active work blocks install (AC-030-06)
	if _, err := client.InstallPreparedUpdate(context.Background(), "1.3.0", true, true); err == nil {
		t.Fatal("expected active work to block install")
	}

	// 6. TOCTOU tampering in staging is caught before applier runs (AC-030-05)
	stagedPayload := filepath.Join(prep.StagingPath, "payload.bin")
	if err := os.WriteFile(stagedPayload, []byte("tampered-bytes-of-same-len-1234567890"), 0600); err != nil {
		t.Fatal(err)
	}
	if _, err := client.InstallPreparedUpdate(context.Background(), "1.3.0", false, true); err == nil {
		t.Fatal("expected TOCTOU tampered payload to be rejected")
	}
	if atomic.LoadInt32(&applier.called) != 0 {
		t.Fatal("applier must not be called when staged file is tampered")
	}

	// 7. Re-prepare and test installer failure recovery reporting (AC-030-07)
	if _, err := client.PrepareUpdate(context.Background(), validJWS); err != nil {
		t.Fatal(err)
	}
	applier.err = errors.New("msiexec exit code 1603")
	res, err := client.InstallPreparedUpdate(context.Background(), "1.3.0", false, true)
	if err == nil || res == nil || !res.RecoveryRestored || res.Version != "1.0.0" {
		t.Fatalf("expected recovery result on failed install, got res=%+v err=%v", res, err)
	}
	if reporter.version != "1.3.0" || reporter.reason == nil {
		t.Fatalf("expected recovery reporter to record failure, got %+v", reporter)
	}

	// 8. Re-prepare and succeed
	applier.err = nil
	res, err = client.InstallPreparedUpdate(context.Background(), "1.3.0", false, true)
	if err != nil || !res.Installed || res.Version != "1.3.0" {
		t.Fatalf("expected successful install, got res=%+v err=%v", res, err)
	}
}
