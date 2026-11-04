package license_test

import (
	"bufio"
	"bytes"
	"context"
	"crypto/ecdsa"
	"crypto/elliptic"
	"encoding/base64"
	"encoding/json"

	"io"
	"math/big"
	"net/http"
	"os"
	"os/exec"
	"path/filepath"
	"strings"
	"sync"
	"testing"
	"time"

	"schoolbot.local/app/internal/license"
)

type hostServer struct {
	t        *testing.T
	input    io.WriteCloser
	output   *bufio.Scanner
	mu       sync.Mutex
	key      string
	verifier *license.ES256Verifier
}

func buildHostServer(t *testing.T) string {
	t.Helper()
	artifacts := t.TempDir()
	config := filepath.Join(artifacts, "NuGet.Config")
	if err := os.WriteFile(config, []byte(`<configuration><packageSources><clear /></packageSources></configuration>`), 0600); err != nil {
		t.Fatal(err)
	}
	project, err := filepath.Abs("testdata/hostserver/HostServer.csproj")
	if err != nil {
		t.Fatal(err)
	}
	ctx, cancel := context.WithTimeout(context.Background(), 3*time.Minute)
	defer cancel()
	command := exec.CommandContext(ctx, "dotnet", "build", project, "--artifacts-path", artifacts, "--disable-build-servers", "-m:1", "-v:q", "-p:NuGetAudit=false", "-p:RestoreConfigFile="+config)
	command.Dir = filepath.Join(filepath.Dir(project), "../../../../../web")
	assets, err := os.ReadFile(filepath.Join(command.Dir, "src/ServiceSchoolBot.Infrastructure/obj/project.assets.json"))
	if err != nil {
		t.Fatal(err)
	}
	var restore struct {
		PackageFolders map[string]json.RawMessage `json:"packageFolders"`
	}
	if err = json.Unmarshal(assets, &restore); err != nil {
		t.Fatal(err)
	}
	packages := os.Getenv("NUGET_PACKAGES")
	if packages == "" {
		for folder := range restore.PackageFolders {
			if _, err := os.Stat(filepath.Join(folder, "npgsql", "10.0.2")); err == nil {
				packages = folder
				break
			}
		}
	}
	command.Env = append(os.Environ(), "DOTNET_CLI_TELEMETRY_OPTOUT=1", "DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1", "DOTNET_NOLOGO=1", "DOTNET_CLI_HOME="+artifacts, "NUGET_HTTP_CACHE_PATH="+artifacts, "NUGET_PACKAGES="+packages)
	output, err := command.CombinedOutput()
	if err != nil {
		t.Fatalf("host server fixture build: %v\n%s", err, output)
	}
	return filepath.Join(artifacts, "bin", "HostServer", "debug", "HostServer.dll")
}

func startHostServer(t *testing.T, binary, backend string) *hostServer {
	t.Helper()
	artifacts := t.TempDir()
	command := exec.Command("dotnet", binary, backend, artifacts)
	command.Env = append(os.Environ(), "DOTNET_CLI_TELEMETRY_OPTOUT=1", "DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1", "DOTNET_NOLOGO=1", "DOTNET_CLI_HOME="+artifacts, "NUGET_HTTP_CACHE_PATH="+artifacts)
	input, err := command.StdinPipe()
	if err != nil {
		t.Fatal(err)
	}
	output, err := command.StdoutPipe()
	if err != nil {
		t.Fatal(err)
	}
	var diagnostics bytes.Buffer
	command.Stderr = &diagnostics
	if err = command.Start(); err != nil {
		t.Fatal(err)
	}
	server := &hostServer{t: t, input: input, output: bufio.NewScanner(output)}
	server.output.Buffer(make([]byte, 4096), 1<<20)
	t.Cleanup(func() {
		input.Close()
		done := make(chan error, 1)
		go func() { done <- command.Wait() }()
		select {
		case err := <-done:
			if err != nil {
				t.Errorf("server fixture exit: %v", err)
			}
		case <-time.After(5 * time.Second):
			command.Process.Kill()
			<-done
			t.Error("server fixture shutdown timed out")
		}
	})
	if !server.output.Scan() {
		t.Fatalf("server startup failed: %s", diagnostics.String())
	}
	var ready struct {
		Key string
		Kid string
		X   string
		Y   string
	}
	if err = json.Unmarshal(server.output.Bytes(), &ready); err != nil {
		t.Fatal("invalid fixture startup")
	}
	x, _ := base64.StdEncoding.DecodeString(ready.X)
	y, _ := base64.StdEncoding.DecodeString(ready.Y)
	public := &ecdsa.PublicKey{Curve: elliptic.P256(), X: new(big.Int).SetBytes(x), Y: new(big.Int).SetBytes(y)}
	server.key = ready.Key
	server.verifier = &license.ES256Verifier{Keys: map[string]*ecdsa.PublicKey{ready.Kid: public}}
	return server
}

func (s *hostServer) call(actor int, path, requestKey string, body json.RawMessage) (int, []byte) {
	s.t.Helper()
	s.mu.Lock()
	defer s.mu.Unlock()
	request, err := json.Marshal(map[string]interface{}{"actor": actor, "path": path, "request_key": requestKey, "body": body})
	if err != nil {
		s.t.Fatal(err)
	}
	if _, err = s.input.Write(append(request, '\n')); err != nil {
		s.t.Fatal(err)
	}
	if !s.output.Scan() {
		s.t.Fatal("server fixture stopped")
	}
	var result struct {
		Status int
		Body   json.RawMessage
	}
	if json.Unmarshal(s.output.Bytes(), &result) != nil {
		s.t.Fatal("invalid server fixture response")
	}
	return result.Status, result.Body
}

func (s *hostServer) control(path string, value interface{}) []byte {
	s.t.Helper()
	body, err := json.Marshal(value)
	if err != nil {
		s.t.Fatal(err)
	}
	status, result := s.call(0, "/fixture/"+path, "", body)
	if status != 200 {
		s.t.Fatalf("fixture control %s status %d", path, status)
	}
	return result
}

type networkSwitch struct {
	server *hostServer
	actor  int
	online bool
	calls  int
}

func (n *networkSwitch) RoundTrip(request *http.Request) (*http.Response, error) {
	if !n.online {
		return nil, io.ErrClosedPipe
	}
	n.calls++
	body, err := io.ReadAll(request.Body)
	if err != nil {
		return nil, err
	}
	status, data := n.server.call(n.actor, request.URL.Path, request.Header.Get("Idempotency-Key"), body)
	return &http.Response{StatusCode: status, Body: io.NopCloser(bytes.NewReader(data)), Header: make(http.Header), Request: request}, nil
}

type blobNamespace struct{ data []byte }

func (s *blobNamespace) Load(context.Context) ([]byte, error) { return bytes.Clone(s.data), nil }
func (s *blobNamespace) Save(_ context.Context, data []byte) error {
	s.data = bytes.Clone(data)
	return nil
}

type hostComponent struct {
	host    *license.Host
	network *networkSwitch
	storage *blobNamespace
	clock   *testClock
	server  *hostServer
}

func newHostComponent(server *hostServer, actor int) *hostComponent {
	component := &hostComponent{server: server, network: &networkSwitch{server: server, actor: actor}, storage: &blobNamespace{}, clock: &testClock{wall: time.Date(2026, 11, 3, 0, 0, 0, 0, time.UTC)}}
	component.restart()
	return component
}

func (c *hostComponent) restart() {
	c.clock.elapsed = 0
	c.host = license.NewHost("https://fixture.invalid", &http.Client{Transport: c.network}, c.clock, c.server.verifier, c.storage)
}

func (c *hostComponent) at(seconds int64) {
	c.clock.wall = time.Unix(seconds, 0).UTC()
	c.clock.elapsed = 0
	c.restart()
}

func (c *hostComponent) claims(t *testing.T) *license.EntitlementClaims {
	t.Helper()
	credentials, err := license.OpenCredentials(context.Background(), c.storage)
	if err != nil {
		t.Fatal(err)
	}
	record, err := credentials.Load(context.Background())
	if err != nil || record == nil {
		t.Fatal("missing accepted record")
	}
	jkt, err := credentials.CurrentDeviceJKT(context.Background())
	if err != nil {
		t.Fatal(err)
	}
	claims, err := license.VerifyEntitlementToken(context.Background(), record.Token, jkt, c.server.verifier)
	if err != nil {
		t.Fatal("accepted record failed signature verification")
	}
	return claims
}

func assertModes(t *testing.T, c *hostComponent, allowed bool) {
	t.Helper()
	for _, actor := range []string{"guest", "login"} {
		for _, mode := range []string{"block-programming", "robot-control", "coding-adventure"} {
			err := c.host.Authorize(context.Background(), mode)
			if (err == nil) != allowed {
				t.Fatalf("%s/%s allowed=%v error=%v", actor, mode, allowed, err)
			}
			if !allowed && !strings.Contains(err.Error(), "online") && !strings.Contains(err.Error(), "denied") {
				t.Fatalf("locked status needs online recovery: %v", err)
			}
		}
	}
}
