package updater

import (
	"context"
	"crypto/ecdsa"
	"crypto/elliptic"
	"crypto/rand"
	"crypto/sha256"
	"encoding/hex"
	"encoding/json"
	"errors"
	"net/http"
	"net/http/httptest"
	"os"
	"path/filepath"
	"testing"
	"time"
)

func TestPublisherPinFailClosedAndRejectsOtherPublisher(t *testing.T) {
	ctx := context.Background()
	winManifest := ReleaseManifest{
		Publisher: "SchoolBright",
		Version:   "1.2.0",
		Channel:   "stable",
		Platform:  "windows-amd64",
	}

	// 1. Fail closed when pin is empty or wildcard
	emptyVerifier := &PinnedPublisherVerifier{
		Pin: PublisherPinConfig{
			ExpectedWindowsSubject:    "*",
			AllowedWindowsThumbprints: []string{"AABBCC"},
			Inspector: func(_ context.Context, _ string, _ string) (PublisherIdentity, error) {
				return PublisherIdentity{ValidSignature: true, WindowsSubject: "CN=SchoolBright", WindowsThumbprint: "AABBCC"}, nil
			},
		},
	}
	if err := emptyVerifier.VerifyArtifactPublisher(ctx, "dummy.msi", winManifest); err == nil {
		t.Fatal("expected wildcard subject pin to fail closed")
	}

	// 2. Reject valid signature from another publisher or wrong thumbprint
	strictWinVerifier := &PinnedPublisherVerifier{
		Pin: PublisherPinConfig{
			ExpectedWindowsSubject:    "O=School Bright Co., Ltd.",
			AllowedWindowsThumbprints: []string{"11223344556677889900AABBCCDDEEFF00112233"},
			Inspector: func(_ context.Context, _ string, _ string) (PublisherIdentity, error) {
				return PublisherIdentity{
					ValidSignature:    true,
					WindowsSubject:    "O=Other Publisher Inc.",
					WindowsThumbprint: "11223344556677889900AABBCCDDEEFF00112233",
				}, nil
			},
		},
	}
	err := strictWinVerifier.VerifyArtifactPublisher(ctx, "dummy.msi", winManifest)
	var ue *UpdateError
	if !errors.As(err, &ue) || ue.Code != ErrArtifactUntrusted {
		t.Fatalf("expected ARTIFACT_UNTRUSTED for wrong publisher subject, got %v", err)
	}

	// 3. Accept matching Windows subject + thumbprint
	strictWinVerifier.Pin.Inspector = func(_ context.Context, _ string, _ string) (PublisherIdentity, error) {
		return PublisherIdentity{
			ValidSignature:    true,
			WindowsSubject:    "CN=School Bot Release, O=School Bright Co., Ltd., C=TH",
			WindowsThumbprint: "11:22:33:44:55:66:77:88:99:00:aa:bb:cc:dd:ee:ff:00:11:22:33",
		}, nil
	}
	if err := strictWinVerifier.VerifyArtifactPublisher(ctx, "dummy.msi", winManifest); err != nil {
		t.Fatalf("expected pinned Windows publisher to pass: %v", err)
	}

	// 4. macOS Team ID pin + notarization + universal arch check
	macManifest := ReleaseManifest{
		Publisher: "SchoolBright",
		Version:   "1.2.0",
		Channel:   "stable",
		Platform:  "darwin-arm64",
	}
	macVerifier := &PinnedPublisherVerifier{
		Pin: PublisherPinConfig{
			ExpectedMacOSTeamID:      "SBTEAM9988",
			RequireMacOSNotarization: true,
			Inspector: func(_ context.Context, _ string, _ string) (PublisherIdentity, error) {
				return PublisherIdentity{
					ValidSignature:  true,
					MacOSTeamID:     "OTHERTEAM1",
					MacOSNotarized:  true,
					UniversalArches: []string{"x86_64", "arm64"},
				}, nil
			},
		},
	}
	if err := macVerifier.VerifyArtifactPublisher(ctx, "dummy.pkg", macManifest); err == nil {
		t.Fatal("expected mismatched Apple Team ID to be rejected")
	}

	macVerifier.Pin.Inspector = func(_ context.Context, _ string, _ string) (PublisherIdentity, error) {
		return PublisherIdentity{
			ValidSignature:  true,
			MacOSTeamID:     "SBTEAM9988",
			MacOSNotarized:  true,
			UniversalArches: []string{"arm64"}, // missing x86_64
		}, nil
	}
	if err := macVerifier.VerifyArtifactPublisher(ctx, "dummy.pkg", macManifest); err == nil {
		t.Fatal("expected thin single-arch PKG to be rejected when universal is required")
	}

	macVerifier.Pin.Inspector = func(_ context.Context, _ string, _ string) (PublisherIdentity, error) {
		return PublisherIdentity{
			ValidSignature:  true,
			MacOSTeamID:     "SBTEAM9988",
			MacOSNotarized:  true,
			UniversalArches: []string{"x86_64", "arm64"},
		}, nil
	}
	if err := macVerifier.VerifyArtifactPublisher(ctx, "dummy.pkg", macManifest); err != nil {
		t.Fatalf("expected valid notarized universal PKG to pass: %v", err)
	}
}

func TestWindowsMSIRollbackAndRepairRecovery(t *testing.T) {
	priv, err := ecdsa.GenerateKey(elliptic.P256(), rand.Reader)
	if err != nil {
		t.Fatal(err)
	}
	installDir := filepath.Join(t.TempDir(), "School Bot")
	if err := os.MkdirAll(filepath.Join(installDir, "engines", "zide"), 0755); err != nil {
		t.Fatal(err)
	}
	v1Files := map[string][]byte{
		"school-bot.exe":                              []byte("school-bot-win-v0.1.5"),
		"demo-engine.exe":                             []byte("demo-engine-win-v0.1.5"),
		filepath.Join("engines", "zide", "zide-engine.exe"): []byte("zide-engine-win-v0.1.5"),
		filepath.Join("engines", "zide", "engine.json"):     []byte(`{"id":"zide","version":"0.1.5"}`),
	}
	for rel, content := range v1Files {
		if err := os.WriteFile(filepath.Join(installDir, rel), content, 0755); err != nil {
			t.Fatal(err)
		}
	}

	msiPayload := []byte("signed-msi-candidate-v0.1.6")
	msiSum := sha256.Sum256(msiPayload)
	var srv *httptest.Server
	srv = httptest.NewServer(http.HandlerFunc(func(w http.ResponseWriter, _ *http.Request) {
		_, _ = w.Write(msiPayload)
	}))
	defer srv.Close()

	dataDir := t.TempDir()
	repairCalled := false
	applier := &OSInstallerApplier{
		DataDir:        dataDir,
		InstallDir:     installDir,
		CurrentVersion: "0.1.5",
		Platform:       "windows-amd64",
		FaultPoint:     FaultMidInstallAbort,
		NowFunc:        func() time.Time { return time.Date(2026, 11, 4, 8, 0, 0, 0, time.UTC) },
		MSIHook: func(_ context.Context, _ string, dir string, fault string) error {
			// Simulate partial overwrite during MSI execution before abort
			_ = os.WriteFile(filepath.Join(dir, "school-bot.exe"), []byte("partial-v0.1.6-broken"), 0755)
			_ = os.WriteFile(filepath.Join(dir, "partial-update.marker"), []byte("dirty"), 0600)
			return errors.New("MSI action InjectMidUpdateFailure returned 1603 (" + fault + ")")
		},
		MSIRepairHook: func(_ context.Context, dir string) error {
			repairCalled = true
			return os.WriteFile(filepath.Join(dir, "school-bot.exe"), v1Files["school-bot.exe"], 0755)
		},
	}

	client := NewClient(Config{
		BaseURL:        srv.URL,
		DataDir:        dataDir,
		CurrentVersion: "0.1.5",
		Channel:        "stable",
		Platform:       "windows-amd64",
		ExpectedPub:    "SchoolBright",
		AllowLocalHTTP: true,
		Verifier: &ES256ReleaseVerifier{
			Keys: map[string]*ecdsa.PublicKey{"rel-2026": &priv.PublicKey},
		},
		Applier: applier,
		NowFunc: func() time.Time { return time.Date(2026, 11, 4, 8, 0, 0, 0, time.UTC) },
	})

	manifest := ReleaseManifest{
		ReleaseID:   20,
		Publisher:   "SchoolBright",
		Version:     "0.1.6",
		Channel:     "stable",
		Platform:    "windows-amd64",
		PackageKind: "windows-msi",
		MinOS:       "10.0.26200",
		URL:         srv.URL + "/school-bot-0.1.6-windows-amd64.msi",
		SHA256:      hex.EncodeToString(msiSum[:]),
		Size:        int64(len(msiPayload)),
		PublishedAt: "2026-11-04T00:00:00Z",
	}
	jws := signTestManifest(t, priv, "rel-2026", "schoolbot-release+jws", manifest)

	if _, err := client.PrepareUpdate(context.Background(), jws); err != nil {
		t.Fatalf("PrepareUpdate failed: %v", err)
	}

	res, err := client.InstallPreparedUpdate(context.Background(), "0.1.6", false, true)
	if err == nil || res == nil || !res.RecoveryRestored || res.Version != "0.1.5" {
		t.Fatalf("expected failed install to restore 0.1.5, got res=%+v err=%v", res, err)
	}
	if !repairCalled {
		t.Fatal("expected MSI repair hook to be invoked when rollback drift was detected")
	}

	// Verify all v0.1.5 files are intact and no partial marker remains
	for rel, wantBytes := range v1Files {
		got, readErr := os.ReadFile(filepath.Join(installDir, rel))
		if readErr != nil || string(got) != string(wantBytes) {
			t.Fatalf("file %s was not restored to v0.1.5: err=%v", rel, readErr)
		}
	}
	if _, statErr := os.Stat(filepath.Join(installDir, "partial-update.marker")); !os.IsNotExist(statErr) {
		t.Fatal("partial-update.marker must be cleaned up")
	}

	receiptBytes, err := os.ReadFile(filepath.Join(dataDir, "updates", "recovery-receipt.json"))
	if err != nil {
		t.Fatalf("missing recovery-receipt.json: %v", err)
	}
	var receipt RecoveryReceipt
	if err := json.Unmarshal(receiptBytes, &receipt); err != nil {
		t.Fatal(err)
	}
	if receipt.RestoredVersion != "0.1.5" || receipt.FailedVersion != "0.1.6" || receipt.HalfInstalledRemaining {
		t.Fatalf("unexpected recovery receipt: %+v", receipt)
	}
}

func TestMacOSUniversalPKGRecoveryOnIntelAndARM(t *testing.T) {
	priv, err := ecdsa.GenerateKey(elliptic.P256(), rand.Reader)
	if err != nil {
		t.Fatal(err)
	}

	for _, platform := range []string{"darwin-amd64", "darwin-arm64"} {
		t.Run(platform, func(t *testing.T) {
			bundleDir := filepath.Join(t.TempDir(), "Applications", "School Bot.app")
			if err := os.MkdirAll(filepath.Join(bundleDir, "Contents", "MacOS", "engines", "zide"), 0755); err != nil {
				t.Fatal(err)
			}
			v1Bundle := map[string][]byte{
				filepath.Join("Contents", "Info.plist"):                            []byte("<plist>0.1.5</plist>"),
				filepath.Join("Contents", "MacOS", "school-bot"):                   []byte("macho-universal-x86_64-arm64-school-bot-0.1.5"),
				filepath.Join("Contents", "MacOS", "demo-engine"):                  []byte("macho-universal-x86_64-arm64-demo-engine-0.1.5"),
				filepath.Join("Contents", "MacOS", "engines", "zide", "zide-engine"): []byte("macho-universal-x86_64-arm64-zide-engine-0.1.5"),
				filepath.Join("Contents", "MacOS", "engines", "zide", "engine.json"): []byte(`{"id":"zide","version":"0.1.5"}`),
			}
			for rel, content := range v1Bundle {
				if err := os.WriteFile(filepath.Join(bundleDir, rel), content, 0755); err != nil {
					t.Fatal(err)
				}
			}

			pkgPayload := []byte("signed-universal-macos-pkg-v0.1.6")
			pkgSum := sha256.Sum256(pkgPayload)
			var srv *httptest.Server
			srv = httptest.NewServer(http.HandlerFunc(func(w http.ResponseWriter, _ *http.Request) {
				_, _ = w.Write(pkgPayload)
			}))
			defer srv.Close()

			dataDir := t.TempDir()
			applier := &OSInstallerApplier{
				DataDir:        dataDir,
				InstallDir:     bundleDir,
				CurrentVersion: "0.1.5",
				Platform:       platform,
				FaultPoint:     FaultPartialBundleLeft,
				NowFunc:        func() time.Time { return time.Date(2026, 11, 4, 8, 30, 0, 0, time.UTC) },
				PKGHook: func(_ context.Context, _ string, targetBundle string, _ string) error {
					// Simulate partial bundle extraction mid-install
					_ = os.WriteFile(filepath.Join(targetBundle, "Contents", "MacOS", "school-bot"), []byte("corrupted-half-extracted"), 0755)
					return errors.New("installer: package extraction interrupted mid-way")
				},
				BundleArchVerifier: func(_ context.Context, targetBundle string) ([]string, error) {
					data, err := os.ReadFile(filepath.Join(targetBundle, "Contents", "MacOS", "school-bot"))
					if err != nil || string(data) != string(v1Bundle[filepath.Join("Contents", "MacOS", "school-bot")]) {
						return nil, errors.New("bundle universal slice verification failed")
					}
					return []string{"x86_64", "arm64"}, nil
				},
			}

			client := NewClient(Config{
				BaseURL:        srv.URL,
				DataDir:        dataDir,
				CurrentVersion: "0.1.5",
				Channel:        "stable",
				Platform:       platform,
				ExpectedPub:    "SchoolBright",
				AllowLocalHTTP: true,
				Verifier: &ES256ReleaseVerifier{
					Keys: map[string]*ecdsa.PublicKey{"rel-2026": &priv.PublicKey},
				},
				Applier: applier,
				NowFunc: func() time.Time { return time.Date(2026, 11, 4, 8, 30, 0, 0, time.UTC) },
			})

			manifest := ReleaseManifest{
				ReleaseID:   21,
				Publisher:   "SchoolBright",
				Version:     "0.1.6",
				Channel:     "stable",
				Platform:    platform,
				PackageKind: "macos-pkg",
				MinOS:       "15.0",
				URL:         srv.URL + "/school-bot-0.1.6-macos-universal.pkg",
				SHA256:      hex.EncodeToString(pkgSum[:]),
				Size:        int64(len(pkgPayload)),
				PublishedAt: "2026-11-04T00:00:00Z",
			}
			jws := signTestManifest(t, priv, "rel-2026", "schoolbot-release+jws", manifest)

			if _, err := client.PrepareUpdate(context.Background(), jws); err != nil {
				t.Fatalf("PrepareUpdate failed on %s: %v", platform, err)
			}

			res, err := client.InstallPreparedUpdate(context.Background(), "0.1.6", false, true)
			if err == nil || res == nil || !res.RecoveryRestored || res.Version != "0.1.5" {
				t.Fatalf("expected macOS recovery on %s to restore 0.1.5, got res=%+v err=%v", platform, res, err)
			}

			for rel, wantBytes := range v1Bundle {
				got, readErr := os.ReadFile(filepath.Join(bundleDir, rel))
				if readErr != nil || string(got) != string(wantBytes) {
					t.Fatalf("bundle file %s was not restored on %s", rel, platform)
				}
			}

			receiptBytes, err := os.ReadFile(filepath.Join(dataDir, "updates", "recovery-receipt.json"))
			if err != nil {
				t.Fatalf("missing macOS recovery receipt on %s: %v", platform, err)
			}
			var receipt RecoveryReceipt
			if err := json.Unmarshal(receiptBytes, &receipt); err != nil {
				t.Fatal(err)
			}
			if receipt.RecoveryMechanism != "bundle-snapshot-restore-and-codesign-verify" ||
				receipt.HalfInstalledRemaining || len(receipt.UniversalArchesVerified) != 2 {
				t.Fatalf("unexpected macOS recovery receipt on %s: %+v", platform, receipt)
			}
		})
	}
}
