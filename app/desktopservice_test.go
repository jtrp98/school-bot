package main

import (
	"encoding/json"
	"net/http"
	"net/http/httptest"
	"os"
	"path/filepath"
	"strings"
	"testing"
)

func TestAPIEndpointPolicy(t *testing.T) {
	for _, value := range []string{"http://localhost:8080", "http://127.0.0.1:8080", "http://[::1]:8080", "https://api.example.com", "https://api.example.com/school-bot"} {
		if err := validateAPIURL(value); err != nil {
			t.Errorf("valid endpoint rejected: %s: %v", value, err)
		}
	}
	for _, value := range []string{"http://api.example.com", "https://user:password@api.example.com", "https://api.example.com?key=secret", "https://api.example.com#fragment", "https://", "file:///tmp/api"} {
		if err := validateAPIURL(value); err == nil {
			t.Errorf("unsafe endpoint accepted: %s", value)
		}
	}
}

func TestOnlineActivationContractWithoutPersistingKey(t *testing.T) {
	calls := 0
	server := httptest.NewServer(http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		calls++
		var body map[string]string
		if err := json.NewDecoder(r.Body).Decode(&body); err != nil {
			t.Error(err)
		}
		if r.Method != http.MethodPost || r.URL.Path != "/v1/licenses/activate" || r.URL.RawQuery != "" || body["key"] != "SBOT-test" || body["device_id"] != "test-device" {
			t.Error("activation must POST key and device ID in JSON")
		}
		_, _ = w.Write([]byte(`{"valid":true,"license_id":"test-license","school":"Demo school","expires_at":"2027-10-16T00:00:00Z","offline_entitlement":false}`))
	}))
	defer server.Close()
	s := &DesktopService{apiURL: server.URL, deviceID: "test-device", dataDir: t.TempDir()}
	if _, err := s.ActivateLicense(" \t"); err == nil {
		t.Fatal("empty key accepted")
	}
	result, err := s.ActivateLicense(" SBOT-test ")
	if err != nil || !strings.Contains(result, `"valid":true`) || calls != 1 {
		t.Fatalf("online activation failed: %s, calls %d, %v", result, calls, err)
	}
	files, err := os.ReadDir(s.dataDir)
	if err != nil || len(files) != 0 {
		t.Fatal("activation must not persist the raw key", err)
	}
}

func TestRenamePreservesLegacyProjectsAndDeviceID(t *testing.T) {
	root := t.TempDir()
	legacy := filepath.Join(root, "ArmStudioPoC")
	if err := os.MkdirAll(legacy, 0700); err != nil {
		t.Fatal(err)
	}
	files := map[string]string{"device-id": strings.Repeat("a", 32), "robot-control.json": `{"schema":1,"angles":[90,70,110,45]}`, "coding-adventure.json": `{"schema":1,"source":"hero.jump()"}`}
	for name, data := range files {
		if err := os.WriteFile(filepath.Join(legacy, name), []byte(data), 0600); err != nil {
			t.Fatal(err)
		}
	}
	dir, err := prepareDataDirectory(root)
	if err != nil {
		t.Fatal(err)
	}
	if filepath.Base(dir) != "SchoolBotPoC" {
		t.Fatal("wrong renamed directory", dir)
	}
	for name, expected := range files {
		for _, parent := range []string{legacy, dir} {
			actual, err := os.ReadFile(filepath.Join(parent, name))
			if err != nil || string(actual) != expected {
				t.Fatalf("file not preserved: %s %v", name, err)
			}
		}
	}
	newer := `{"schema":1,"source":"hero.move_right(2)"}`
	if err := os.WriteFile(filepath.Join(dir, "coding-adventure.json"), []byte(newer), 0600); err != nil {
		t.Fatal(err)
	}
	if _, err := prepareDataDirectory(root); err != nil {
		t.Fatal(err)
	}
	actual, err := os.ReadFile(filepath.Join(dir, "coding-adventure.json"))
	if err != nil || string(actual) != newer {
		t.Fatal("migration overwrote a newer project", err)
	}
}

func TestProjectStorageAndModeBoundary(t *testing.T) {
	s := &DesktopService{dataDir: t.TempDir()}
	if err := s.SaveProject("../../escape", `{}`); err == nil {
		t.Fatal("unsafe mode accepted")
	}
	if err := s.SaveProject("coding-adventure", `not-json`); err == nil {
		t.Fatal("invalid project accepted")
	}
	for _, value := range []string{`{"schema":1,"source":"hero.jump()"}`, `{"schema":1,"source":"hero.move_right(2)"}`} {
		if err := s.SaveProject("coding-adventure", value); err != nil {
			t.Fatal(err)
		}
		actual, err := s.LoadProject("coding-adventure")
		if err != nil || actual != value {
			t.Fatalf("save/load failed: %s %v", actual, err)
		}
	}
}

func TestAppVersionMatchesManifest(t *testing.T) {
	if appVersion == unreleasedVersion {
		t.Skip("set by build scripts; use -ldflags -X schoolbot.local/app.appVersion=<manifest version>")
	}
	manifest, err := os.ReadFile(filepath.Join("frontend", "package.json"))
	if err != nil {
		t.Fatal(err)
	}
	var parsed struct {
		Version string `json:"version"`
	}
	if err := json.Unmarshal(manifest, &parsed); err != nil {
		t.Fatal(err)
	}
	if appVersion != parsed.Version {
		t.Fatalf("host version %q differs from package.json %q", appVersion, parsed.Version)
	}
}

func TestHostAPIContractAndOfflineFailure(t *testing.T) {
	server := httptest.NewServer(http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		if r.URL.Path != "/v1/updates/check" || r.URL.Query().Get("version") != appVersion {
			t.Error("update contract mismatch")
		}
		_, _ = w.Write([]byte(`{"available":false,"release":null,"install_supported":false}`))
	}))
	defer server.Close()
	s := &DesktopService{apiURL: server.URL, dataDir: t.TempDir()}
	result, err := s.CheckUpdates()
	if err != nil || !strings.Contains(result, `"available":false`) {
		t.Fatalf("API call failed: %s %v", result, err)
	}
	server.Close()
	if _, err = s.CheckUpdates(); err == nil {
		t.Fatal("expected offline error")
	}
	if err = s.SaveProject("robot-control", `{"schema":1,"angles":[90,90,90,90]}`); err != nil {
		t.Fatal("offline storage should remain available", err)
	}
}

func TestAPIErrorStatusIsReportedWhateverTheBodyLooksLike(t *testing.T) {
	cases := []struct {
		name   string
		status int
		body   string
		want   string
	}{
		{"rate limited with error body", http.StatusTooManyRequests,
			`{"error":{"code":"RATE_LIMITED","message":"Too many requests."}}`, "API 429: Too many requests."},
		{"rate limited with empty body", http.StatusTooManyRequests, "", "API 429: Too Many Requests"},
		{"bad request with empty body", http.StatusBadRequest, "", "API 400: Bad Request"},
		{"proxy HTML error", http.StatusBadGateway, "<html>bad gateway</html>", "API 502: Bad Gateway"},
		{"error JSON without message", http.StatusForbidden, `{"error":{}}`, "API 403: Forbidden"},
	}
	for _, tc := range cases {
		t.Run(tc.name, func(t *testing.T) {
			server := httptest.NewServer(http.HandlerFunc(func(w http.ResponseWriter, _ *http.Request) {
				w.WriteHeader(tc.status)
				_, _ = w.Write([]byte(tc.body))
			}))
			defer server.Close()
			s := &DesktopService{apiURL: server.URL, dataDir: t.TempDir()}
			_, err := s.CheckUpdates()
			if err == nil || err.Error() != tc.want {
				t.Fatalf("got %v, want %q", err, tc.want)
			}
		})
	}
}

func TestSuccessfulNonJSONAPIResponseIsRejected(t *testing.T) {
	server := httptest.NewServer(http.HandlerFunc(func(w http.ResponseWriter, _ *http.Request) {
		_, _ = w.Write([]byte("not json"))
	}))
	defer server.Close()
	s := &DesktopService{apiURL: server.URL, dataDir: t.TempDir()}
	if _, err := s.CheckUpdates(); err == nil || err.Error() != "invalid API response" {
		t.Fatalf("expected invalid API response, got %v", err)
	}
}
