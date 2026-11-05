package main

import (
	"bytes"
	"context"
	"encoding/json"
	"errors"
	"fmt"
	"io"
	"net/http"
	"net/url"
	"os"
	"path/filepath"
	"runtime"
	"schoolbot.local/app/internal/engine"
	"schoolbot.local/app/internal/license"
	"schoolbot.local/app/internal/project"
	"schoolbot.local/app/internal/updater"
	"strings"
	"sync"
	"time"
)

// Build scripts inject the version from app/frontend/package.json, the only place it is written.
// A plain `go build` or `go test` keeps this unreleased value.
var appVersion = unreleasedVersion

const unreleasedVersion = "0.0.0"

// This URL is embedded in distributed binaries, so it must not contain secrets.
var defaultAPIURL = "http://localhost:8080"

type DesktopSession struct {
	Username  string `json:"username"`
	Token     string `json:"token,omitempty"`
	ExpiresAt string `json:"expires_at,omitempty"`
}

type DesktopService struct {
	mu           sync.Mutex
	manager      *engine.Manager
	entitlement  *license.Host
	projectStore *project.LocalStore
	updater      *updater.Client
	session      *DesktopSession
	apiURL       string
	dataDir      string
	closeWindow  func()
}

func (s *DesktopService) CloseWindow() error {
	if s.closeWindow == nil {
		return errors.New("desktop window is not ready")
	}
	s.closeWindow()
	return nil
}

type AppInfo struct {
	Version  string `json:"version"`
	Platform string `json:"platform"`
	APIURL   string `json:"api_url"`
	Runtime  string `json:"runtime"`
}

func NewDesktopService() (*DesktopService, error) {
	base := os.Getenv("SCHOOL_BOT_API_URL")
	if base == "" {
		base = os.Getenv("ARM_API_URL")
	}
	if base == "" {
		base = defaultAPIURL
	}
	config, err := os.UserConfigDir()
	if err != nil {
		return nil, err
	}
	dir, err := prepareDataDirectory(config)
	if err != nil {
		return nil, err
	}
	entitlement := license.NewHost(
		base, &http.Client{}, license.NewSystemClock(), &license.ES256Verifier{},
		&license.ProtectedStore{Path: filepath.Join(dir, "entitlement.dat")},
	)
	svc := &DesktopService{
		manager:     engine.NewManager(),
		entitlement: entitlement,
		apiURL:      strings.TrimRight(base, "/"),
		dataDir:     dir,
	}
	if store, err := svc.ensureProjectStore(); err == nil {
		_, _ = project.MigrateLegacyProjects(config, store)
	}
	return svc, nil
}

func (s *DesktopService) Info() AppInfo {
	return AppInfo{
		Version:  appVersion,
		Platform: runtime.GOOS + "-" + runtime.GOARCH,
		APIURL:   s.apiURL,
		Runtime:  "Wails v3 · native engine IPC",
	}
}
func (s *DesktopService) Run(mode, action, data string) (engine.Response, error) {
	if len(data) > engine.MaxRequestBytes {
		return engine.Response{}, errors.New("request exceeds 64 KB")
	}
	if !json.Valid([]byte(data)) {
		return engine.Response{}, errors.New("invalid engine JSON")
	}
	if s.entitlement == nil {
		return engine.Response{}, &license.LicenseError{Code: license.ErrDeviceNotActivated, Message: "connect online to activate this device"}
	}
	if err := s.entitlement.Authorize(context.Background(), mode); err != nil {
		return engine.Response{}, err
	}
	return s.manager.Execute(mode, action, json.RawMessage(data))
}
func (s *DesktopService) CancelRun() { s.manager.Cancel() }

func (s *DesktopService) CheckUpdates() (string, error) {
	query := url.Values{
		"version":  {appVersion},
		"channel":  {"stable"},
		"platform": {runtime.GOOS + "-" + runtime.GOARCH},
	}
	return s.api("GET", "/api/service-school-bot/updates/check?"+query.Encode(), nil)
}

func (s *DesktopService) ActivateLicense(key string) (string, error) {
	return s.checkLicense(key, false)
}

func (s *DesktopService) ValidateLicense(keys ...string) (string, error) {
	key := ""
	if len(keys) > 0 {
		key = keys[0]
	}
	if strings.TrimSpace(key) == "" && s.entitlement != nil {
		if saved, err := s.entitlement.SavedKey(context.Background()); err == nil && saved != "" {
			key = saved
		}
	}
	return s.checkLicense(key, true)
}

func (s *DesktopService) LoginSession(username, password string) (string, error) {
	username = strings.TrimSpace(username)
	if username == "" || password == "" {
		return "invalid_credentials", nil
	}
	projection, err := s.LicenseStatus()
	if err != nil || projection.Status != license.StatusValid {
		return "unavailable", nil
	}
	schoolID, deviceID, err := s.entitlement.LoginContext(context.Background())
	if err != nil {
		return "unavailable", nil
	}
	resp, err := s.api("POST", "/api/auth/login", map[string]any{
		"user": username, "password": password, "schoolId": schoolID, "imei": deviceID,
	})
	if err != nil {
		msg := err.Error()
		if strings.HasPrefix(msg, "API 401:") || strings.HasPrefix(msg, "API 403:") {
			return "invalid_credentials", nil
		}
		if strings.HasPrefix(msg, "API unavailable") {
			return "network", nil
		}
		return "unavailable", nil
	}
	session, status := parseGatewayLogin(resp, username, schoolID, time.Now().UTC())
	if session == nil {
		return status, nil
	}
	s.mu.Lock()
	s.session = session
	s.mu.Unlock()
	return "success", nil
}

func (s *DesktopService) LogoutSession() error {
	s.mu.Lock()
	s.session = nil
	s.mu.Unlock()
	return nil
}
func (s *DesktopService) CurrentSession() *DesktopSession {
	s.mu.Lock()
	defer s.mu.Unlock()
	if s.session == nil {
		return nil
	}
	if s.session.ExpiresAt != "" {
		if exp, err := time.Parse(time.RFC3339, s.session.ExpiresAt); err == nil && !time.Now().UTC().Before(exp) {
			s.session = nil
			return nil
		}
	}
	copySession := *s.session
	copySession.Token = ""
	return &copySession
}

func (s *DesktopService) CheckUpdatesV2() (updater.UpdateCheckResponse, error) {
	return s.ensureUpdater().CheckUpdates(context.Background())
}

func (s *DesktopService) PrepareUpdateV2(manifestJWS string) (*updater.PreparedUpdate, error) {
	return s.ensureUpdater().PrepareUpdate(context.Background(), manifestJWS)
}

func (s *DesktopService) CancelPrepareUpdateV2() {
	s.ensureUpdater().CancelPrepare()
}

func (s *DesktopService) InstallPreparedUpdateV2(version string, hasActiveWork bool, userConfirmed bool) (*updater.InstallResult, error) {
	return s.ensureUpdater().InstallPreparedUpdate(context.Background(), version, hasActiveWork, userConfirmed)
}

func (s *DesktopService) ensureUpdater() *updater.Client {
	s.mu.Lock()
	defer s.mu.Unlock()
	if s.updater != nil {
		return s.updater
	}
	local := strings.HasPrefix(s.apiURL, "http://127.0.0.1") ||
		strings.HasPrefix(s.apiURL, "http://localhost") ||
		strings.HasPrefix(s.apiURL, "http://[::1]")
	s.updater = updater.NewClient(updater.Config{
		BaseURL:        s.apiURL,
		DataDir:        s.dataDir,
		CurrentVersion: appVersion,
		Channel:        "stable",
		Platform:       runtime.GOOS + "-" + runtime.GOARCH,
		ExpectedPub:    "SchoolBright",
		AllowLocalHTTP: local,
	})
	return s.updater
}

func (s *DesktopService) LicenseStatus() (license.EntitlementProjection, error) {
	if s.entitlement == nil {
		return license.EntitlementProjection{Status: license.StatusUnactivated}, nil
	}
	return s.entitlement.Evaluate(context.Background())
}

func (s *DesktopService) checkLicense(key string, validate bool) (string, error) {
	if s.entitlement == nil {
		return "", &license.LicenseError{Code: license.ErrConfigUnavailable, Message: "license host unavailable; connect online to recover"}
	}
	projection, err := s.entitlement.Check(context.Background(), key, validate)
	if err != nil {
		return "", err
	}
	data, err := json.Marshal(projection)
	return string(data), err
}
func (s *DesktopService) api(method, path string, body any) (string, error) {
	if err := validateAPIURL(s.apiURL); err != nil {
		return "", err
	}
	var err error
	var payload []byte
	if body != nil {
		payload, err = json.Marshal(body)
		if err != nil {
			return "", err
		}
	}
	ctx, cancel := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancel()
	req, err := http.NewRequestWithContext(ctx, method, s.apiURL+path, bytes.NewReader(payload))
	if err != nil {
		return "", err
	}
	req.Header.Set("Content-Type", "application/json")
	client := http.Client{
		CheckRedirect: func(_ *http.Request, _ []*http.Request) error {
			return errors.New("API redirects are not allowed")
		},
	}
	res, err := client.Do(req)
	if err != nil {
		return "", errors.New("API unavailable; check your internet connection or API address. " +
			"Local learning remains available")
	}
	defer res.Body.Close()
	data, err := io.ReadAll(io.LimitReader(res.Body, 65537))
	if err != nil || len(data) > 65536 {
		return "", errors.New("invalid API response")
	}
	if res.StatusCode >= 400 {
		return "", apiFailure(res.StatusCode, data)
	}
	if !json.Valid(data) {
		return "", errors.New("invalid API response")
	}
	return string(data), nil
}

// apiFailure checks the status before the body shape: proxies and framework defaults can answer
// 4xx/5xx with an empty or non-JSON body that must not hide the real status.
func apiFailure(status int, body []byte) error {
	var problem struct {
		Error struct {
			Message string `json:"message"`
		} `json:"error"`
	}
	if json.Unmarshal(body, &problem) == nil && problem.Error.Message != "" {
		return fmt.Errorf("API %d: %s", status, problem.Error.Message)
	}
	return fmt.Errorf("API %d: %s", status, http.StatusText(status))
}

func validateAPIURL(value string) error {
	base, err := url.Parse(value)
	if err != nil {
		return errors.New("API must use HTTPS; HTTP allowed only for local development")
	}
	local := base.Hostname() == "127.0.0.1" || base.Hostname() == "localhost" || base.Hostname() == "::1"
	secure := base.Scheme == "https" || (base.Scheme == "http" && local)
	if base.Host == "" || base.User != nil || base.RawQuery != "" || base.Fragment != "" || !secure {
		return errors.New("API must use HTTPS; HTTP allowed only for local development")
	}
	return nil
}
