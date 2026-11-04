package updater

import (
	"context"
	"crypto/ecdsa"
	"crypto/elliptic"
	"crypto/sha256"
	"encoding/base64"
	"encoding/hex"
	"encoding/json"
	"errors"
	"fmt"
	"io"
	"math/big"
	"net/http"
	"net/url"
	"os"
	"path/filepath"
	"strconv"
	"strings"
	"sync"
	"time"
)

const (
	ErrUpdateManifestInvalid = "UPDATE_MANIFEST_INVALID"
	ErrUpdateHashMismatch    = "UPDATE_HASH_MISMATCH"
	ErrUpdateUntrustedSource = "UPDATE_UNTRUSTED_SOURCE"
	ErrUpdateNotSupported    = "UPDATE_NOT_SUPPORTED"
	ErrUpdateBusy            = "UPDATE_BUSY"
	ErrUpdateCancelled       = "UPDATE_CANCELLED"
	ErrUpdateInstallFailed   = "UPDATE_INSTALL_FAILED"
	ErrUpdateDowngrade       = "UPDATE_DOWNGRADE_REJECTED"
	ErrUpdateExpired         = "UPDATE_EXPIRED"

	MaxArtifactSizeBytes int64 = 512 * 1024 * 1024 // 512 MiB
	MaxManifestBytes     int   = 65536
)

type UpdateError struct {
	Code    string `json:"code"`
	Message string `json:"message"`
}

func (e *UpdateError) Error() string {
	return fmt.Sprintf("[%s] %s", e.Code, e.Message)
}

type ReleaseManifest struct {
	Publisher   string `json:"publisher"`
	Version     string `json:"version"`
	Channel     string `json:"channel"`
	Platform    string `json:"platform"`
	URL         string `json:"url"`
	SHA256      string `json:"sha256"`
	Size        int64  `json:"size"`
	PublishedAt string `json:"published_at"`
	ExpiresAt   string `json:"expires_at,omitempty"`
	Withdrawn   bool   `json:"withdrawn,omitempty"`
	Notes       string `json:"notes,omitempty"`
}

type UpdateCheckResponse struct {
	Available        bool             `json:"available"`
	InstallSupported bool             `json:"install_supported"`
	CurrentVersion   string           `json:"current_version"`
	Channel          string           `json:"channel"`
	Platform         string           `json:"platform"`
	CheckedAt        string           `json:"checked_at"`
	Release          *ReleaseManifest `json:"release"`
	ManifestJWS      string           `json:"manifest_jws,omitempty"`
}

type PreparedUpdate struct {
	Version     string `json:"version"`
	Channel     string `json:"channel"`
	Platform    string `json:"platform"`
	SHA256      string `json:"sha256"`
	Size        int64  `json:"size"`
	StagingPath string `json:"staging_path"`
	PreparedAt  string `json:"prepared_at"`
}

type InstallResult struct {
	Installed        bool   `json:"installed"`
	Version          string `json:"version"`
	PreviousVersion  string `json:"previous_version"`
	RecoveryRestored bool   `json:"recovery_restored,omitempty"`
	Message          string `json:"message,omitempty"`
}

type SignatureVerifier interface {
	VerifyReleaseSignature(ctx context.Context, signedData []byte, signature []byte, kid string) error
}

type ES256ReleaseVerifier struct {
	Keys map[string]*ecdsa.PublicKey
}

func (v *ES256ReleaseVerifier) VerifyReleaseSignature(_ context.Context, data, signature []byte, kid string) error {
	if v == nil || v.Keys == nil {
		return errors.New("trust store is empty")
	}
	key := v.Keys[kid]
	if key == nil || key.Curve != elliptic.P256() || len(signature) != 64 {
		return errors.New("unknown signing key or invalid signature")
	}
	digest := sha256.Sum256(data)
	r := new(big.Int).SetBytes(signature[:32])
	s := new(big.Int).SetBytes(signature[32:])
	if !ecdsa.Verify(key, digest[:], r, s) {
		return errors.New("cryptographic signature verification failed")
	}
	return nil
}

type Applier interface {
	ApplyStagedUpdate(ctx context.Context, stagingDir string, manifest ReleaseManifest) error
}

type RecoveryReporter interface {
	ReportFailedInstall(ctx context.Context, version string, reason error)
}

type Config struct {
	BaseURL          string
	DataDir          string
	CurrentVersion   string
	Channel          string
	Platform         string
	ExpectedPub      string
	AllowedHosts     []string
	AllowLocalHTTP   bool
	Client           *http.Client
	Verifier         SignatureVerifier
	Applier          Applier
	RecoveryReporter RecoveryReporter
	NowFunc          func() time.Time
}

type Client struct {
	mu           sync.Mutex
	cfg          Config
	activeCancel context.CancelFunc
}

func NewClient(cfg Config) *Client {
	if cfg.Channel == "" {
		cfg.Channel = "stable"
	}
	if cfg.ExpectedPub == "" {
		cfg.ExpectedPub = "SchoolBright"
	}
	if cfg.Client == nil {
		cfg.Client = &http.Client{Timeout: 30 * time.Second}
	}
	if cfg.NowFunc == nil {
		cfg.NowFunc = func() time.Time { return time.Now().UTC() }
	}
	cfg.BaseURL = strings.TrimRight(cfg.BaseURL, "/")
	return &Client{cfg: cfg}
}

type jwsHeader struct {
	Typ string `json:"typ"`
	Alg string `json:"alg"`
	Kid string `json:"kid"`
}

func (c *Client) VerifyManifestJWS(ctx context.Context, rawJWS string) (*ReleaseManifest, error) {
	if len(rawJWS) == 0 || len(rawJWS) > MaxManifestBytes {
		return nil, &UpdateError{Code: ErrUpdateManifestInvalid, Message: "manifest JWS size is invalid"}
	}
	if c.cfg.Verifier == nil {
		return nil, &UpdateError{Code: ErrUpdateManifestInvalid, Message: "release signature verifier is not configured"}
	}
	parts := strings.Split(rawJWS, ".")
	if len(parts) != 3 {
		return nil, &UpdateError{Code: ErrUpdateManifestInvalid, Message: "malformed JWS compact serialization"}
	}
	headerBytes, err := base64.RawURLEncoding.DecodeString(parts[0])
	if err != nil {
		return nil, &UpdateError{Code: ErrUpdateManifestInvalid, Message: "invalid JWS header encoding"}
	}
	var hdr jwsHeader
	if err := json.Unmarshal(headerBytes, &hdr); err != nil {
		return nil, &UpdateError{Code: ErrUpdateManifestInvalid, Message: "invalid JWS header JSON"}
	}
	if hdr.Typ != "schoolbot-release+jws" || hdr.Alg != "ES256" || strings.TrimSpace(hdr.Kid) == "" {
		return nil, &UpdateError{Code: ErrUpdateManifestInvalid, Message: "unsupported JWS header typ, alg, or kid"}
	}

	payloadBytes, err := base64.RawURLEncoding.DecodeString(parts[1])
	if err != nil {
		return nil, &UpdateError{Code: ErrUpdateManifestInvalid, Message: "invalid JWS payload encoding"}
	}
	sigBytes, err := base64.RawURLEncoding.DecodeString(parts[2])
	if err != nil {
		return nil, &UpdateError{Code: ErrUpdateManifestInvalid, Message: "invalid JWS signature encoding"}
	}

	signedData := []byte(parts[0] + "." + parts[1])
	if err := c.cfg.Verifier.VerifyReleaseSignature(ctx, signedData, sigBytes, hdr.Kid); err != nil {
		return nil, &UpdateError{Code: ErrUpdateManifestInvalid, Message: "release manifest signature verification failed"}
	}

	var manifest ReleaseManifest
	if err := json.Unmarshal(payloadBytes, &manifest); err != nil {
		return nil, &UpdateError{Code: ErrUpdateManifestInvalid, Message: "invalid release manifest JSON"}
	}
	if err := c.validateManifestFields(&manifest); err != nil {
		return nil, err
	}
	return &manifest, nil
}

func (c *Client) validateManifestFields(m *ReleaseManifest) error {
	if m == nil {
		return &UpdateError{Code: ErrUpdateManifestInvalid, Message: "release manifest is nil"}
	}
	if m.Withdrawn {
		return &UpdateError{Code: ErrUpdateManifestInvalid, Message: "release has been withdrawn"}
	}
	if m.Publisher != c.cfg.ExpectedPub {
		return &UpdateError{Code: ErrUpdateManifestInvalid, Message: "release publisher mismatch"}
	}
	if strings.TrimSpace(m.Version) == "" {
		return &UpdateError{Code: ErrUpdateManifestInvalid, Message: "release version is required"}
	}
	if c.cfg.Channel != "" && m.Channel != c.cfg.Channel {
		return &UpdateError{Code: ErrUpdateManifestInvalid, Message: "release channel mismatch"}
	}
	if c.cfg.Platform != "" && m.Platform != c.cfg.Platform {
		return &UpdateError{Code: ErrUpdateManifestInvalid, Message: "release platform mismatch"}
	}
	if len(m.SHA256) != 64 {
		return &UpdateError{Code: ErrUpdateManifestInvalid, Message: "release sha256 must be 64 hex characters"}
	}
	if _, err := hex.DecodeString(m.SHA256); err != nil {
		return &UpdateError{Code: ErrUpdateManifestInvalid, Message: "release sha256 is not valid hex"}
	}
	if m.Size <= 0 || m.Size > MaxArtifactSizeBytes {
		return &UpdateError{Code: ErrUpdateManifestInvalid, Message: "release artifact size is out of bounds"}
	}
	if _, err := time.Parse(time.RFC3339, m.PublishedAt); err != nil {
		return &UpdateError{Code: ErrUpdateManifestInvalid, Message: "release published_at must be RFC3339"}
	}
	if m.ExpiresAt != "" {
		exp, err := time.Parse(time.RFC3339, m.ExpiresAt)
		if err != nil {
			return &UpdateError{Code: ErrUpdateManifestInvalid, Message: "release expires_at must be RFC3339"}
		}
		if !c.cfg.NowFunc().Before(exp) {
			return &UpdateError{Code: ErrUpdateExpired, Message: "release manifest has expired"}
		}
	}
	if err := c.validateArtifactURL(m.URL); err != nil {
		return err
	}
	if c.cfg.CurrentVersion != "" && c.cfg.CurrentVersion != "0.0.0" {
		if compareSemver(m.Version, c.cfg.CurrentVersion) <= 0 {
			return &UpdateError{Code: ErrUpdateDowngrade, Message: fmt.Sprintf("release version %s is not newer than current version %s", m.Version, c.cfg.CurrentVersion)}
		}
	}
	return nil
}

func (c *Client) validateArtifactURL(rawURL string) error {
	parsed, err := url.Parse(rawURL)
	if err != nil || parsed.Host == "" || parsed.User != nil || parsed.Fragment != "" {
		return &UpdateError{Code: ErrUpdateUntrustedSource, Message: "invalid release artifact URL"}
	}
	hostname := strings.ToLower(parsed.Hostname())
	isLocal := hostname == "localhost" || hostname == "127.0.0.1" || hostname == "::1"
	if parsed.Scheme != "https" && !(c.cfg.AllowLocalHTTP && parsed.Scheme == "http" && isLocal) {
		return &UpdateError{Code: ErrUpdateUntrustedSource, Message: "release artifact URL must use HTTPS"}
	}
	if len(c.cfg.AllowedHosts) > 0 {
		allowed := false
		for _, h := range c.cfg.AllowedHosts {
			if strings.EqualFold(strings.TrimSpace(h), hostname) {
				allowed = true
				break
			}
		}
		if !allowed {
			return &UpdateError{Code: ErrUpdateUntrustedSource, Message: "release artifact host is not in the trusted allowlist"}
		}
	}
	return nil
}

// CheckUpdates performs a strictly metadata-only check (AC-030-01..03, AC-030-06).
// It never downloads, stages, or executes any binary payload.
func (c *Client) CheckUpdates(ctx context.Context) (UpdateCheckResponse, error) {
	nowStr := c.cfg.NowFunc().UTC().Format(time.RFC3339)
	baseResp := UpdateCheckResponse{
		Available:        false,
		InstallSupported: false,
		CurrentVersion:   c.cfg.CurrentVersion,
		Channel:          c.cfg.Channel,
		Platform:         c.cfg.Platform,
		CheckedAt:        nowStr,
		Release:          nil,
	}

	if err := c.validateArtifactURL(c.cfg.BaseURL); err != nil {
		return baseResp, err
	}

	query := url.Values{
		"version":  {c.cfg.CurrentVersion},
		"channel":  {c.cfg.Channel},
		"platform": {c.cfg.Platform},
	}
	endpoint := c.cfg.BaseURL + "/v1/updates/check?" + query.Encode()
	req, err := http.NewRequestWithContext(ctx, http.MethodGet, endpoint, nil)
	if err != nil {
		return baseResp, err
	}

	client := *c.cfg.Client
	client.CheckRedirect = func(*http.Request, []*http.Request) error {
		return errors.New("API redirects are not allowed")
	}

	res, err := client.Do(req)
	if err != nil {
		return baseResp, errors.New("API unavailable; check your internet connection or API address. Local learning remains available")
	}
	defer res.Body.Close()

	body, err := io.ReadAll(io.LimitReader(res.Body, int64(MaxManifestBytes)+1))
	if err != nil || len(body) > MaxManifestBytes {
		return baseResp, errors.New("invalid API response")
	}
	if res.StatusCode >= 400 {
		return baseResp, fmt.Errorf("API %d: %s", res.StatusCode, http.StatusText(res.StatusCode))
	}
	if !json.Valid(body) {
		return baseResp, errors.New("invalid API response")
	}

	var wire struct {
		Available        bool             `json:"available"`
		InstallSupported bool             `json:"install_supported"`
		Release          *ReleaseManifest `json:"release"`
		ManifestJWS      string           `json:"manifest_jws"`
	}
	if err := json.Unmarshal(body, &wire); err != nil {
		return baseResp, errors.New("invalid API response")
	}

	if !wire.Available || (wire.Release == nil && wire.ManifestJWS == "") {
		return baseResp, nil
	}

	var verified *ReleaseManifest
	if wire.ManifestJWS != "" {
		m, err := c.VerifyManifestJWS(ctx, wire.ManifestJWS)
		if err != nil {
			return baseResp, err
		}
		verified = m
	} else if wire.Release != nil {
		if c.cfg.Verifier != nil {
			return baseResp, &UpdateError{Code: ErrUpdateManifestInvalid, Message: "signed manifest_jws is required when verifier is configured"}
		}
		verified = wire.Release
	}

	baseResp.Available = true
	baseResp.InstallSupported = c.cfg.Applier != nil && wire.InstallSupported
	baseResp.Release = verified
	baseResp.ManifestJWS = wire.ManifestJWS
	return baseResp, nil
}

// CancelPrepare cancels an in-flight PrepareUpdate download.
func (c *Client) CancelPrepare() {
	c.mu.Lock()
	cancel := c.activeCancel
	c.mu.Unlock()
	if cancel != nil {
		cancel()
	}
}

// PrepareUpdate verifies the JWS manifest, downloads the payload over HTTPS, verifies size and SHA-256,
// and stages the verified artifact under updates/staging/<version>/ without executing it (AC-030-04, AC-030-08).
func (c *Client) PrepareUpdate(ctx context.Context, manifestJWS string) (*PreparedUpdate, error) {
	manifest, err := c.VerifyManifestJWS(ctx, manifestJWS)
	if err != nil {
		return nil, err
	}

	c.mu.Lock()
	if c.activeCancel != nil {
		c.mu.Unlock()
		return nil, &UpdateError{Code: ErrUpdateBusy, Message: "another update preparation is already in progress"}
	}
	prepCtx, cancel := context.WithCancel(ctx)
	c.activeCancel = cancel
	c.mu.Unlock()

	defer func() {
		cancel()
		c.mu.Lock()
		c.activeCancel = nil
		c.mu.Unlock()
	}()

	stagingRoot := filepath.Join(c.cfg.DataDir, "updates", "staging")
	versionDir := filepath.Join(stagingRoot, sanitizeVersionDir(manifest.Version))
	_ = os.RemoveAll(versionDir)
	if err := os.MkdirAll(versionDir, 0700); err != nil {
		return nil, err
	}

	cleanupOnFail := true
	defer func() {
		if cleanupOnFail {
			_ = os.RemoveAll(versionDir)
		}
	}()

	req, err := http.NewRequestWithContext(prepCtx, http.MethodGet, manifest.URL, nil)
	if err != nil {
		return nil, &UpdateError{Code: ErrUpdateUntrustedSource, Message: err.Error()}
	}
	client := *c.cfg.Client
	client.CheckRedirect = func(*http.Request, []*http.Request) error {
		return &UpdateError{Code: ErrUpdateUntrustedSource, Message: "redirects are not allowed for artifact downloads"}
	}

	res, err := client.Do(req)
	if err != nil {
		if errors.Is(prepCtx.Err(), context.Canceled) {
			return nil, &UpdateError{Code: ErrUpdateCancelled, Message: "update download was cancelled"}
		}
		return nil, err
	}
	defer res.Body.Close()

	if res.StatusCode != http.StatusOK {
		return nil, &UpdateError{Code: ErrUpdateUntrustedSource, Message: fmt.Sprintf("artifact download failed with status %d", res.StatusCode)}
	}

	tmpPayload := filepath.Join(versionDir, "payload.bin.tmp")
	outFile, err := os.OpenFile(tmpPayload, os.O_CREATE|os.O_WRONLY|os.O_TRUNC, 0600)
	if err != nil {
		return nil, err
	}

	hasher := sha256.New()
	limited := io.LimitReader(res.Body, manifest.Size+1)
	written, copyErr := io.Copy(io.MultiWriter(outFile, hasher), limited)
	syncErr := outFile.Sync()
	closeErr := outFile.Close()

	if errors.Is(prepCtx.Err(), context.Canceled) {
		return nil, &UpdateError{Code: ErrUpdateCancelled, Message: "update preparation was cancelled"}
	}
	if copyErr != nil {
		return nil, copyErr
	}
	if syncErr != nil {
		return nil, syncErr
	}
	if closeErr != nil {
		return nil, closeErr
	}
	if written != manifest.Size {
		return nil, &UpdateError{Code: ErrUpdateHashMismatch, Message: fmt.Sprintf("artifact size mismatch: got %d bytes, expected %d", written, manifest.Size)}
	}

	actualHash := hex.EncodeToString(hasher.Sum(nil))
	if !strings.EqualFold(actualHash, manifest.SHA256) {
		return nil, &UpdateError{Code: ErrUpdateHashMismatch, Message: "artifact SHA-256 digest does not match signed manifest"}
	}

	finalPayload := filepath.Join(versionDir, "payload.bin")
	if err := os.Rename(tmpPayload, finalPayload); err != nil {
		return nil, err
	}
	if err := os.WriteFile(filepath.Join(versionDir, "manifest.jws"), []byte(manifestJWS), 0600); err != nil {
		return nil, err
	}

	prepared := &PreparedUpdate{
		Version:     manifest.Version,
		Channel:     manifest.Channel,
		Platform:    manifest.Platform,
		SHA256:      strings.ToLower(manifest.SHA256),
		Size:        manifest.Size,
		StagingPath: versionDir,
		PreparedAt:  c.cfg.NowFunc().UTC().Format(time.RFC3339),
	}
	stateBytes, err := json.Marshal(prepared)
	if err != nil {
		return nil, err
	}
	if err := os.WriteFile(filepath.Join(versionDir, "state.json"), stateBytes, 0600); err != nil {
		return nil, err
	}

	cleanupOnFail = false
	return prepared, nil
}

// InstallPreparedUpdate re-verifies the staged JWS and recomputes SHA-256 to prevent TOCTOU tampering,
// enforces stop/close-guard confirmation, and invokes the configured Applier (AC-030-05..08).
func (c *Client) InstallPreparedUpdate(ctx context.Context, version string, hasActiveWork bool, userConfirmed bool) (*InstallResult, error) {
	if hasActiveWork {
		return nil, &UpdateError{Code: ErrUpdateBusy, Message: "cannot install update while robot or project work is active"}
	}
	if !userConfirmed {
		return nil, &UpdateError{Code: ErrUpdateCancelled, Message: "user confirmation is required before installing update"}
	}
	if c.cfg.Applier == nil {
		return nil, &UpdateError{Code: ErrUpdateNotSupported, Message: "update installation is not supported on this build"}
	}

	versionDir := filepath.Join(c.cfg.DataDir, "updates", "staging", sanitizeVersionDir(version))
	jwsBytes, err := os.ReadFile(filepath.Join(versionDir, "manifest.jws"))
	if err != nil {
		return nil, &UpdateError{Code: ErrUpdateManifestInvalid, Message: "staged update manifest is missing"}
	}

	manifest, err := c.VerifyManifestJWS(ctx, string(jwsBytes))
	if err != nil {
		_ = os.RemoveAll(versionDir)
		return nil, err
	}
	if manifest.Version != version {
		_ = os.RemoveAll(versionDir)
		return nil, &UpdateError{Code: ErrUpdateManifestInvalid, Message: "staged update version mismatch"}
	}

	payloadPath := filepath.Join(versionDir, "payload.bin")
	file, err := os.Open(payloadPath)
	if err != nil {
		_ = os.RemoveAll(versionDir)
		return nil, &UpdateError{Code: ErrUpdateHashMismatch, Message: "staged payload is missing"}
	}
	hasher := sha256.New()
	written, copyErr := io.Copy(hasher, io.LimitReader(file, manifest.Size+1))
	_ = file.Close()
	if copyErr != nil || written != manifest.Size {
		_ = os.RemoveAll(versionDir)
		return nil, &UpdateError{Code: ErrUpdateHashMismatch, Message: "staged payload size tampered before install"}
	}
	actualHash := hex.EncodeToString(hasher.Sum(nil))
	if !strings.EqualFold(actualHash, manifest.SHA256) {
		_ = os.RemoveAll(versionDir)
		return nil, &UpdateError{Code: ErrUpdateHashMismatch, Message: "staged payload SHA-256 tampered before install"}
	}

	if err := c.cfg.Applier.ApplyStagedUpdate(ctx, versionDir, *manifest); err != nil {
		if c.cfg.RecoveryReporter != nil {
			c.cfg.RecoveryReporter.ReportFailedInstall(ctx, manifest.Version, err)
		}
		return &InstallResult{
			Installed:        false,
			Version:          c.cfg.CurrentVersion,
			PreviousVersion:  c.cfg.CurrentVersion,
			RecoveryRestored: true,
			Message:          err.Error(),
		}, &UpdateError{Code: ErrUpdateInstallFailed, Message: fmt.Sprintf("update installation failed; current version %s remains active: %v", c.cfg.CurrentVersion, err)}
	}

	return &InstallResult{
		Installed:       true,
		Version:         manifest.Version,
		PreviousVersion: c.cfg.CurrentVersion,
	}, nil
}

func sanitizeVersionDir(version string) string {
	var b strings.Builder
	for _, r := range version {
		if (r >= 'a' && r <= 'z') || (r >= 'A' && r <= 'Z') || (r >= '0' && r <= '9') || r == '.' || r == '-' || r == '_' {
			b.WriteRune(r)
		} else {
			b.WriteRune('_')
		}
	}
	res := strings.Trim(b.String(), "._")
	if res == "" {
		return "unknown"
	}
	return res
}

func compareSemver(a, b string) int {
	pa := parseSemverParts(a)
	pb := parseSemverParts(b)
	for i := 0; i < 3; i++ {
		if pa[i] > pb[i] {
			return 1
		}
		if pa[i] < pb[i] {
			return -1
		}
	}
	return strings.Compare(a, b)
}

func parseSemverParts(v string) [3]int {
	v = strings.TrimPrefix(strings.TrimSpace(v), "v")
	if idx := strings.IndexAny(v, "-+"); idx >= 0 {
		v = v[:idx]
	}
	parts := strings.Split(v, ".")
	var out [3]int
	for i := 0; i < 3 && i < len(parts); i++ {
		n, _ := strconv.Atoi(parts[i])
		out[i] = n
	}
	return out
}
