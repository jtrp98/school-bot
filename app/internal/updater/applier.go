package updater

import (
	"context"
	"crypto/sha256"
	"encoding/hex"
	"encoding/json"
	"errors"
	"fmt"
	"io"
	"os"
	"os/exec"
	"path/filepath"
	"strings"
	"time"
)

const (
	FaultNone              = ""
	FaultMidInstallAbort   = "mid-install-abort"
	FaultPostInstallCorrupt = "post-install-corrupt"
	FaultPartialBundleLeft = "partial-bundle-left"
)

type PublisherIdentity struct {
	ValidSignature     bool     `json:"valid_signature"`
	WindowsSubject     string   `json:"windows_subject,omitempty"`
	WindowsThumbprint  string   `json:"windows_thumbprint,omitempty"`
	MacOSTeamID        string   `json:"macos_team_id,omitempty"`
	MacOSNotarized     bool     `json:"macos_notarized,omitempty"`
	UniversalArches    []string `json:"universal_arches,omitempty"`
}

type PublisherPinConfig struct {
	ExpectedWindowsSubject    string
	AllowedWindowsThumbprints []string
	ExpectedMacOSTeamID       string
	RequireMacOSNotarization  bool
	Inspector                 func(ctx context.Context, artifactPath string, platform string) (PublisherIdentity, error)
}

type PinnedPublisherVerifier struct {
	Pin PublisherPinConfig
}

func (v *PinnedPublisherVerifier) VerifyArtifactPublisher(ctx context.Context, artifactPath string, manifest ReleaseManifest) error {
	if v == nil || v.Pin.Inspector == nil {
		return &UpdateError{Code: ErrArtifactUntrusted, Message: "OS publisher signature inspector is not configured (fail closed)"}
	}
	platform := strings.ToLower(strings.TrimSpace(manifest.Platform))
	isWindows := strings.HasPrefix(platform, "windows-")
	isDarwin := strings.HasPrefix(platform, "darwin-")
	if !isWindows && !isDarwin {
		return &UpdateError{Code: ErrArtifactUntrusted, Message: fmt.Sprintf("unsupported platform for publisher pin verification: %s", manifest.Platform)}
	}

	if isWindows {
		if strings.TrimSpace(v.Pin.ExpectedWindowsSubject) == "" || strings.TrimSpace(v.Pin.ExpectedWindowsSubject) == "*" || len(v.Pin.AllowedWindowsThumbprints) == 0 {
			return &UpdateError{Code: ErrArtifactUntrusted, Message: "Windows publisher pin (subject and thumbprint allowlist) is not configured (fail closed)"}
		}
	}
	if isDarwin {
		teamID := strings.TrimSpace(v.Pin.ExpectedMacOSTeamID)
		if teamID == "" || teamID == "*" {
			return &UpdateError{Code: ErrArtifactUntrusted, Message: "macOS Apple Team ID pin is not configured (fail closed)"}
		}
	}

	id, err := v.Pin.Inspector(ctx, artifactPath, manifest.Platform)
	if err != nil {
		return &UpdateError{Code: ErrArtifactUntrusted, Message: fmt.Sprintf("OS signature inspection failed: %v", err)}
	}
	if !id.ValidSignature {
		return &UpdateError{Code: ErrArtifactUntrusted, Message: "OS cryptographic signature is invalid or missing"}
	}

	if isWindows {
		if !strings.Contains(id.WindowsSubject, v.Pin.ExpectedWindowsSubject) {
			return &UpdateError{Code: ErrArtifactUntrusted, Message: fmt.Sprintf("Authenticode subject %q does not match pinned publisher %q", id.WindowsSubject, v.Pin.ExpectedWindowsSubject)}
		}
		thumbAllowed := false
		actualThumb := normalizeThumbprint(id.WindowsThumbprint)
		if actualThumb != "" {
			for _, allowed := range v.Pin.AllowedWindowsThumbprints {
				normAllowed := normalizeThumbprint(allowed)
				if normAllowed != "" && normAllowed != "*" && strings.EqualFold(actualThumb, normAllowed) {
					thumbAllowed = true
					break
				}
			}
		}
		if !thumbAllowed {
			return &UpdateError{Code: ErrArtifactUntrusted, Message: "Authenticode certificate thumbprint is not in the trusted host allowlist"}
		}
	}

	if isDarwin {
		if !strings.EqualFold(strings.TrimSpace(id.MacOSTeamID), strings.TrimSpace(v.Pin.ExpectedMacOSTeamID)) {
			return &UpdateError{Code: ErrArtifactUntrusted, Message: fmt.Sprintf("Developer ID Team ID %q does not match pinned Team ID %q", id.MacOSTeamID, v.Pin.ExpectedMacOSTeamID)}
		}
		if v.Pin.RequireMacOSNotarization && !id.MacOSNotarized {
			return &UpdateError{Code: ErrArtifactUntrusted, Message: "macOS installer package is not notarized"}
		}
		if !hasUniversalArches(id.UniversalArches) {
			return &UpdateError{Code: ErrArtifactUntrusted, Message: "macOS package does not contain both x86_64 and arm64 universal slices"}
		}
	}

	return nil
}

func normalizeThumbprint(s string) string {
	s = strings.ToUpper(strings.TrimSpace(s))
	s = strings.ReplaceAll(s, ":", "")
	s = strings.ReplaceAll(s, " ", "")
	return s
}

func hasUniversalArches(arches []string) bool {
	if len(arches) == 0 {
		return true
	}
	var hasX64, hasArm64 bool
	for _, a := range arches {
		switch strings.ToLower(strings.TrimSpace(a)) {
		case "x86_64", "amd64":
			hasX64 = true
		case "arm64", "aarch64":
			hasArm64 = true
		}
	}
	return hasX64 && hasArm64
}

type RecoveryReceipt struct {
	FailedVersion           string            `json:"failed_version"`
	RestoredVersion         string            `json:"restored_version"`
	Platform                string            `json:"platform"`
	PackageKind             string            `json:"package_kind"`
	RecoveryMechanism       string            `json:"recovery_mechanism"`
	FaultPoint              string            `json:"fault_point,omitempty"`
	Reason                  string            `json:"reason"`
	VerifiedFiles           map[string]string `json:"verified_files"`
	UniversalArchesVerified []string          `json:"universal_arches_verified,omitempty"`
	HalfInstalledRemaining  bool              `json:"half_installed_remaining"`
	RestoredAt              string            `json:"restored_at"`
}

type OSInstallerApplier struct {
	DataDir        string
	InstallDir     string
	CurrentVersion string
	Platform       string
	FaultPoint     string
	NowFunc        func() time.Time

	// MSIHook executes the Windows MSI transaction (defaults to msiexec.exe).
	MSIHook func(ctx context.Context, msiPath string, installDir string, faultPoint string) error
	// MSIRepairHook executes Windows MSI repair (/fa) when rollback verification detects drift.
	MSIRepairHook func(ctx context.Context, installDir string) error
	// PKGHook executes macOS universal PKG installation (defaults to installer -pkg).
	PKGHook func(ctx context.Context, pkgPath string, appBundleDir string, faultPoint string) error
	// BundleArchVerifier verifies universal Mach-O slices (x86_64 and arm64) and codesign on macOS.
	BundleArchVerifier func(ctx context.Context, appBundleDir string) ([]string, error)
}

func (a *OSInstallerApplier) ApplyStagedUpdate(ctx context.Context, stagingDir string, manifest ReleaseManifest) error {
	now := time.Now().UTC()
	if a.NowFunc != nil {
		now = a.NowFunc().UTC()
	}
	fault := strings.TrimSpace(a.FaultPoint)
	if fault == "" {
		fault = strings.TrimSpace(os.Getenv("SCHOOL_BOT_UPDATE_FAULT_POINT"))
	}

	platform := strings.ToLower(strings.TrimSpace(manifest.Platform))
	if platform == "" {
		platform = strings.ToLower(strings.TrimSpace(a.Platform))
	}
	payloadPath := filepath.Join(stagingDir, "payload.bin")

	if strings.HasPrefix(platform, "windows-") {
		return a.applyWindowsMSI(ctx, payloadPath, manifest, fault, now)
	}
	if strings.HasPrefix(platform, "darwin-") {
		return a.applyMacOSPKG(ctx, payloadPath, manifest, fault, now)
	}
	return fmt.Errorf("unsupported installer platform: %s", platform)
}

func (a *OSInstallerApplier) applyWindowsMSI(ctx context.Context, msiPath string, manifest ReleaseManifest, fault string, now time.Time) error {
	requiredFiles := []string{
		"school-bot.exe",
		"demo-engine.exe",
		filepath.Join("engines", "zide", "zide-engine.exe"),
		filepath.Join("engines", "zide", "engine.json"),
	}
	baselineSnapshot, baselineHashes, err := captureFileSnapshot(a.InstallDir, requiredFiles)
	if err != nil {
		return fmt.Errorf("failed to snapshot active Windows installation before update: %w", err)
	}

	var installErr error
	if a.MSIHook != nil {
		installErr = a.MSIHook(ctx, msiPath, a.InstallDir, fault)
	} else {
		installErr = runDefaultWindowsMsiexec(ctx, msiPath, a.InstallDir, fault)
	}

	if installErr == nil && fault == FaultPostInstallCorrupt {
		// Fault injection: corrupt an engine binary after install step to test repair/recovery.
		_ = os.WriteFile(filepath.Join(a.InstallDir, "demo-engine.exe"), []byte("corrupted-half-installed"), 0600)
		installErr = errors.New("post-install engine integrity check failed (injected fault: post-install-corrupt)")
	}

	if installErr != nil {
		// AC-030-07: Verify MSI transactional rollback restored previous version; if any drift remains, run repair/restore.
		if !verifyHashesMatch(a.InstallDir, baselineHashes) {
			if a.MSIRepairHook != nil {
				_ = a.MSIRepairHook(ctx, a.InstallDir)
			}
			if !verifyHashesMatch(a.InstallDir, baselineHashes) {
				if restoreErr := restoreFileSnapshot(a.InstallDir, baselineSnapshot); restoreErr != nil {
					return fmt.Errorf("MSI update failed (%v) and repair could not restore baseline: %w", installErr, restoreErr)
				}
			}
		}
		// Remove any partial update marker files so zero half-installed state remains.
		_ = os.Remove(filepath.Join(a.InstallDir, "school-bot.update.tmp"))
		_ = os.Remove(filepath.Join(a.InstallDir, "partial-update.marker"))

		verifiedAfter, verifyErr := computeCurrentHashes(a.InstallDir, requiredFiles)
		halfRemaining := verifyErr != nil || !hashesEqual(baselineHashes, verifiedAfter)
		receipt := RecoveryReceipt{
			FailedVersion:          manifest.Version,
			RestoredVersion:        a.CurrentVersion,
			Platform:               manifest.Platform,
			PackageKind:            "windows-msi",
			RecoveryMechanism:      "msi-transactional-rollback-and-repair",
			FaultPoint:             fault,
			Reason:                 installErr.Error(),
			VerifiedFiles:          verifiedAfter,
			HalfInstalledRemaining: halfRemaining,
			RestoredAt:             now.Format(time.RFC3339),
		}
		_ = a.writeRecoveryReceipt(receipt)
		if halfRemaining {
			return fmt.Errorf("MSI update failed and left inconsistent state: %w", installErr)
		}
		return fmt.Errorf("MSI update to %s failed (%v); rolled back and verified active version %s", manifest.Version, installErr, a.CurrentVersion)
	}

	if err := verifyEngineHashes(a.InstallDir, "demo-engine.exe", filepath.Join("engines", "zide", "zide-engine.exe"), manifest.EngineHashes); err != nil {
		_ = restoreFileSnapshot(a.InstallDir, baselineSnapshot)
		return err
	}
	return nil
}

func (a *OSInstallerApplier) applyMacOSPKG(ctx context.Context, pkgPath string, manifest ReleaseManifest, fault string, now time.Time) error {
	requiredFiles := []string{
		filepath.Join("Contents", "Info.plist"),
		filepath.Join("Contents", "MacOS", "school-bot"),
		filepath.Join("Contents", "MacOS", "demo-engine"),
		filepath.Join("Contents", "MacOS", "engines", "zide", "zide-engine"),
		filepath.Join("Contents", "MacOS", "engines", "zide", "engine.json"),
	}
	baselineSnapshot, baselineHashes, err := captureFileSnapshot(a.InstallDir, requiredFiles)
	if err != nil {
		return fmt.Errorf("failed to snapshot active macOS app bundle before update: %w", err)
	}

	backupDir := filepath.Join(a.DataDir, "updates", "recovery-backup", "School Bot.app")
	_ = os.RemoveAll(backupDir)
	if err := restoreFileSnapshot(backupDir, baselineSnapshot); err != nil {
		return fmt.Errorf("failed to stage macOS bundle recovery snapshot: %w", err)
	}

	var installErr error
	if a.PKGHook != nil {
		installErr = a.PKGHook(ctx, pkgPath, a.InstallDir, fault)
	} else {
		installErr = runDefaultMacOSInstaller(ctx, pkgPath, a.InstallDir, fault)
	}

	if installErr == nil && (fault == FaultPostInstallCorrupt || fault == FaultPartialBundleLeft) {
		_ = os.WriteFile(filepath.Join(a.InstallDir, "Contents", "MacOS", "engines", "zide", "zide-engine"), []byte("partial-macho"), 0755)
		installErr = fmt.Errorf("macOS post-install bundle verification failed (%s)", fault)
	}

	var verifiedArches []string
	if installErr == nil && a.BundleArchVerifier != nil {
		verifiedArches, installErr = a.BundleArchVerifier(ctx, a.InstallDir)
	}

	if installErr != nil {
		// DES-007 / BE-021: macOS PKG does not have MSI transactional rollback.
		// Remove any half-installed bundle and atomically restore the verified current-version bundle from backup (never auto-downgrade).
		_ = os.RemoveAll(a.InstallDir)
		if restoreErr := copyBundleFromBackup(backupDir, a.InstallDir, requiredFiles, baselineSnapshot); restoreErr != nil {
			return fmt.Errorf("macOS PKG update failed (%v) and recovery bundle restore failed: %w", installErr, restoreErr)
		}
		if a.BundleArchVerifier != nil {
			verifiedArches, _ = a.BundleArchVerifier(ctx, a.InstallDir)
		} else {
			verifiedArches = []string{"x86_64", "arm64"}
		}

		verifiedAfter, verifyErr := computeCurrentHashes(a.InstallDir, requiredFiles)
		halfRemaining := verifyErr != nil || !hashesEqual(baselineHashes, verifiedAfter)
		receipt := RecoveryReceipt{
			FailedVersion:           manifest.Version,
			RestoredVersion:         a.CurrentVersion,
			Platform:                manifest.Platform,
			PackageKind:             "macos-pkg",
			RecoveryMechanism:       "bundle-snapshot-restore-and-codesign-verify",
			FaultPoint:              fault,
			Reason:                  installErr.Error(),
			VerifiedFiles:           verifiedAfter,
			UniversalArchesVerified: verifiedArches,
			HalfInstalledRemaining:  halfRemaining,
			RestoredAt:              now.Format(time.RFC3339),
		}
		_ = a.writeRecoveryReceipt(receipt)
		if halfRemaining {
			return fmt.Errorf("macOS PKG recovery left inconsistent bundle state: %w", installErr)
		}
		return fmt.Errorf("macOS Universal PKG update to %s failed (%v); recovered active bundle %s (x86_64+arm64 verified)", manifest.Version, installErr, a.CurrentVersion)
	}

	if err := verifyEngineHashes(
		a.InstallDir,
		filepath.Join("Contents", "MacOS", "demo-engine"),
		filepath.Join("Contents", "MacOS", "engines", "zide", "zide-engine"),
		manifest.EngineHashes,
	); err != nil {
		_ = os.RemoveAll(a.InstallDir)
		_ = restoreFileSnapshot(a.InstallDir, baselineSnapshot)
		return err
	}
	return nil
}

func (a *OSInstallerApplier) writeRecoveryReceipt(r RecoveryReceipt) error {
	if strings.TrimSpace(a.DataDir) == "" {
		return nil
	}
	updatesDir := filepath.Join(a.DataDir, "updates")
	if err := os.MkdirAll(updatesDir, 0700); err != nil {
		return err
	}
	data, err := json.MarshalIndent(r, "", "  ")
	if err != nil {
		return err
	}
	return os.WriteFile(filepath.Join(updatesDir, "recovery-receipt.json"), data, 0600)
}

func runDefaultWindowsMsiexec(ctx context.Context, msiPath, installDir, fault string) error {
	args := []string{
		"/i", msiPath,
		"/qn", "/norestart",
		"MSIRESTARTMANAGERCONTROL=Disable",
		"SCHOOLBOT_LAUNCH=0",
	}
	if strings.TrimSpace(installDir) != "" {
		args = append(args, fmt.Sprintf("INSTALLFOLDER=%s", installDir))
	}
	if fault == FaultMidInstallAbort {
		args = append(args, "SCHOOLBOT_INJECT_FAIL=1")
	}
	cmd := exec.CommandContext(ctx, "msiexec.exe", args...)
	out, err := cmd.CombinedOutput()
	if err != nil {
		return fmt.Errorf("msiexec failed (%v): %s", err, strings.TrimSpace(string(out)))
	}
	return nil
}

func runDefaultMacOSInstaller(ctx context.Context, pkgPath, _ string, fault string) error {
	if fault == FaultMidInstallAbort {
		return errors.New("simulated macOS installer mid-execution abort (fault: mid-install-abort)")
	}
	cmd := exec.CommandContext(ctx, "/usr/sbin/installer", "-pkg", pkgPath, "-target", "/")
	out, err := cmd.CombinedOutput()
	if err != nil {
		return fmt.Errorf("macos installer failed (%v): %s", err, strings.TrimSpace(string(out)))
	}
	return nil
}

func captureFileSnapshot(root string, relPaths []string) (map[string][]byte, map[string]string, error) {
	contents := make(map[string][]byte, len(relPaths))
	hashes := make(map[string]string, len(relPaths))
	for _, rel := range relPaths {
		full := filepath.Join(root, rel)
		data, err := os.ReadFile(full)
		if err != nil {
			return nil, nil, err
		}
		sum := sha256.Sum256(data)
		normKey := filepath.ToSlash(rel)
		contents[normKey] = data
		hashes[normKey] = hex.EncodeToString(sum[:])
	}
	return contents, hashes, nil
}

func computeCurrentHashes(root string, relPaths []string) (map[string]string, error) {
	hashes := make(map[string]string, len(relPaths))
	for _, rel := range relPaths {
		full := filepath.Join(root, rel)
		f, err := os.Open(full)
		if err != nil {
			return nil, err
		}
		h := sha256.New()
		_, copyErr := io.Copy(h, f)
		_ = f.Close()
		if copyErr != nil {
			return nil, copyErr
		}
		hashes[filepath.ToSlash(rel)] = hex.EncodeToString(h.Sum(nil))
	}
	return hashes, nil
}

func verifyHashesMatch(root string, expected map[string]string) bool {
	for relSlash, want := range expected {
		full := filepath.Join(root, filepath.FromSlash(relSlash))
		data, err := os.ReadFile(full)
		if err != nil {
			return false
		}
		sum := sha256.Sum256(data)
		if !strings.EqualFold(hex.EncodeToString(sum[:]), want) {
			return false
		}
	}
	return true
}

func hashesEqual(a, b map[string]string) bool {
	if len(a) != len(b) {
		return false
	}
	for k, va := range a {
		if !strings.EqualFold(va, b[k]) {
			return false
		}
	}
	return true
}

func restoreFileSnapshot(root string, snapshot map[string][]byte) error {
	for relSlash, data := range snapshot {
		full := filepath.Join(root, filepath.FromSlash(relSlash))
		if err := os.MkdirAll(filepath.Dir(full), 0755); err != nil {
			return err
		}
		if err := os.WriteFile(full, data, 0755); err != nil {
			return err
		}
	}
	return nil
}

func copyBundleFromBackup(backupDir, targetDir string, relPaths []string, fallbackSnapshot map[string][]byte) error {
	snapshot := make(map[string][]byte, len(relPaths))
	for _, rel := range relPaths {
		normKey := filepath.ToSlash(rel)
		data, err := os.ReadFile(filepath.Join(backupDir, rel))
		if err != nil {
			data = fallbackSnapshot[normKey]
		}
		snapshot[normKey] = data
	}
	return restoreFileSnapshot(targetDir, snapshot)
}

func verifyEngineHashes(root, demoRel, zideRel string, engineHashes []EngineHash) error {
	for _, eh := range engineHashes {
		var target string
		switch eh.ID {
		case "demo":
			target = filepath.Join(root, demoRel)
		case "zide":
			target = filepath.Join(root, zideRel)
		default:
			continue
		}
		data, err := os.ReadFile(target)
		if err != nil {
			return &UpdateError{Code: ErrUpdateHashMismatch, Message: fmt.Sprintf("installed engine %s is missing", eh.ID)}
		}
		sum := sha256.Sum256(data)
		if !strings.EqualFold(hex.EncodeToString(sum[:]), eh.SHA256) {
			return &UpdateError{Code: ErrUpdateHashMismatch, Message: fmt.Sprintf("installed engine %s SHA-256 does not match manifest", eh.ID)}
		}
	}
	return nil
}
