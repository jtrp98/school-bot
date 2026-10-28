package main

import (
	"bytes"
	"context"
	"crypto/rand"
	"encoding/hex"
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
	"strings"
	"time"
)

// Build scripts inject the version from app/frontend/package.json, the only place it is written.
// A plain `go build` or `go test` keeps this unreleased value.
var appVersion = unreleasedVersion

const unreleasedVersion = "0.0.0"

// This URL is embedded in distributed binaries, so it must not contain secrets.
var defaultAPIURL = "http://localhost:8080"

type DesktopService struct {
	manager     *engine.Manager
	apiURL      string
	dataDir     string
	deviceID    string
	closeWindow func()
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
		config = os.TempDir()
	}
	dir, err := prepareDataDirectory(config)
	if err != nil {
		return nil, err
	}
	deviceFile := filepath.Join(dir, "device-id")
	device, err := readBoundedFile(deviceFile, 32)
	if err != nil || !validDeviceID(device) {
		value := make([]byte, 16)
		if _, err := rand.Read(value); err != nil {
			return nil, err
		}
		device = []byte(hex.EncodeToString(value))
		if err := os.WriteFile(deviceFile, device, 0600); err != nil {
			return nil, err
		}
	}
	return &DesktopService{
		manager:  engine.NewManager(),
		apiURL:   strings.TrimRight(base, "/"),
		dataDir:  dir,
		deviceID: string(device),
	}, nil
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
	return s.manager.Execute(mode, action, json.RawMessage(data))
}
func (s *DesktopService) CancelRun() { s.manager.Cancel() }

func (s *DesktopService) CheckUpdates() (string, error) {
	query := url.Values{
		"version":  {appVersion},
		"channel":  {"stable"},
		"platform": {runtime.GOOS + "-" + runtime.GOARCH},
	}
	return s.api("GET", "/v1/updates/check?"+query.Encode(), nil)
}

func (s *DesktopService) ActivateLicense(key string) (string, error) {
	key = strings.TrimSpace(key)
	if key == "" || len(key) > 256 {
		return "", errors.New("invalid key")
	}
	return s.api("POST", "/v1/licenses/activate", map[string]string{"key": key, "device_id": s.deviceID})
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
