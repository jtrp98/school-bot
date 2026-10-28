package engine

import (
	"context"
	"encoding/json"
	"fmt"
	"os"
	"os/exec"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
	"time"
	"unicode/utf8"
)

func TestTrustedEngineRegistry(t *testing.T) {
	for mode, id := range map[string]string{"robot-control": "zide", "coding-adventure": "demo", "block-programming": "demo"} {
		spec, err := engineForMode(mode)
		if err != nil || spec.id != id || filepath.IsAbs(spec.relativePath) {
			t.Fatalf("%s: %#v %v", mode, spec, err)
		}
	}
	for _, mode := range []string{"zide", "../../evil", "", "Robot-Control"} {
		if _, err := engineForMode(mode); err == nil {
			t.Fatalf("accepted %q", mode)
		}
	}
}

func TestEveryModeHasActionAllowlist(t *testing.T) {
	for _, mode := range []string{"robot-control", "coding-adventure", "block-programming"} {
		if len(modeActions[mode]) == 0 {
			t.Fatalf("%s has no action allowlist", mode)
		}
	}
	if len(modeActions) != 3 {
		t.Fatalf("allowlist modes differ from registry: %d", len(modeActions))
	}
}

func TestActionAllowlist(t *testing.T) {
	manager, started := helperManager(t)
	rejected := []struct{ mode, action string }{
		{"robot-control", "command.send"},
		{"robot-control", "engine.info"},
		{"robot-control", "project.import"},
		{"robot-control", ""},
		{"robot-control", "program.run"},
		{"coding-adventure", "servo.set"},
		{"block-programming", "robot.stop"},
		{"unknown-mode", "robot.stop"},
		{"", "robot.stop"},
	}
	for _, c := range rejected {
		_, err := manager.Execute(c.mode, c.action, json.RawMessage(`{}`))
		if err == nil || (!strings.Contains(err.Error(), "not allowed") && !strings.Contains(err.Error(), "unsupported engine mode")) {
			t.Fatalf("%s/%s: expected rejection, got %v", c.mode, c.action, err)
		}
	}
	select {
	case <-started:
		t.Fatal("rejected action started an engine process")
	default:
	}
	allowed := []struct{ mode, action string }{
		{"robot-control", "device.list"},
		{"robot-control", "robot.stop"},
		{"robot-control", "group.download"},
		{"coding-adventure", "program.run"},
		{"block-programming", "program.run"},
	}
	for _, c := range allowed {
		if _, err := manager.Execute(c.mode, c.action, json.RawMessage(`{}`)); err != nil {
			t.Fatalf("%s/%s: %v", c.mode, c.action, err)
		}
		<-started
	}
}

func TestLocateEngineIgnoresWorkingDirectory(t *testing.T) {
	t.Chdir(t.TempDir())
	spec, _ := engineForMode("robot-control")
	name := spec.relativePath
	if runtime.GOOS == "windows" {
		name += ".exe"
	}
	planted := filepath.Join("bin", name)
	if err := os.MkdirAll(filepath.Dir(planted), 0700); err != nil {
		t.Fatal(err)
	}
	if err := os.WriteFile(planted, []byte("test fixture"), 0700); err != nil {
		t.Fatal(err)
	}
	if got, err := locateEngine(spec); err == nil {
		t.Fatalf("engine resolved from the working directory: %q", got)
	}
}

func helperManager(t *testing.T) (*Manager, <-chan struct{}) {
	t.Helper()
	started := make(chan struct{}, 4)
	manager := NewManager()
	manager.resolve = func(engineSpec) (string, error) { return os.Args[0], nil }
	manager.command = func(ctx context.Context, path string) *exec.Cmd {
		started <- struct{}{}
		cmd := exec.CommandContext(ctx, path, "-test.run=^TestManagerHelperProcess$")
		cmd.Env = []string{"SCHOOL_BOT_ENGINE_TEST_HELPER=1"}
		return cmd
	}
	return manager, started
}

func TestManagerHelperProcess(t *testing.T) {
	if os.Getenv("SCHOOL_BOT_ENGINE_TEST_HELPER") != "1" {
		return
	}
	var req Request
	if err := json.NewDecoder(os.Stdin).Decode(&req); err != nil {
		os.Exit(2)
	}
	helper := struct{ Helper string }{}
	_ = json.Unmarshal(req.Data, &helper)
	response := Response{Protocol: Protocol, RequestID: req.RequestID, Status: "success"}
	switch helper.Helper {
	case "stop-wait":
		time.Sleep(200 * time.Millisecond)
	case "wait":
		time.Sleep(30 * time.Second)
	case "mismatch":
		response.RequestID = "other-request"
	case "stderr-exit":
		fmt.Fprint(os.Stderr, "boom:\n  bad firmware\n")
		os.Exit(1)
	case "noisy":
		_, _ = os.Stderr.WriteString(strings.Repeat("e", 200000))
	case "huge-output":
		_, _ = os.Stdout.WriteString(strings.Repeat("x", 2<<20) + "\n")
		os.Exit(0)
	case "engine-error":
		response.Status = "error"
		response.Error = &Error{Code: "INVALID_INPUT", Message: "pwm must be an integer"}
	case "unknown-code":
		response.Status = "error"
		response.Error = &Error{Code: "MADE_UP", Message: "x"}
	case "data-array":
		response.Data = []int{1}
	case "error-with-data":
		response.Status = "error"
		response.Error = &Error{Code: "INVALID_INPUT", Message: "x"}
		response.Data = map[string]any{"a": 1}
	}
	if err := json.NewEncoder(os.Stdout).Encode(response); err != nil {
		os.Exit(3)
	}
	os.Exit(0)
}

func errorCode(t *testing.T, response Response, err error) string {
	t.Helper()
	if err != nil {
		t.Fatalf("expected an error response, got Go error %v", err)
	}
	if response.Status != "error" || response.Error == nil || response.Error.Message == "" {
		t.Fatalf("expected an error response, got %#v", response)
	}
	return response.Error.Code
}

func TestCancelReleasesBusySlotBeforeReturning(t *testing.T) {
	manager, started := helperManager(t)
	finished := make(chan Response, 1)
	go func() {
		response, _ := manager.Execute("robot-control", "board.info", json.RawMessage(`{"helper":"wait"}`))
		finished <- response
	}()
	select {
	case <-started:
	case <-time.After(3 * time.Second):
		t.Fatal("request never started")
	}
	response, err := manager.Execute("coding-adventure", "program.run", json.RawMessage(`{}`))
	if code := errorCode(t, response, err); code != CodeEngineBusy {
		t.Fatalf("expected shared busy slot, got %s", code)
	}
	manager.Cancel()
	cancelled := <-finished
	if code := errorCode(t, cancelled, nil); code != CodeEngineCancelled {
		t.Fatalf("expected canceled child, got %s", code)
	}
	if response, err := manager.Execute("robot-control", "robot.stop", json.RawMessage(`{}`)); err != nil || response.Status != "success" {
		t.Fatalf("stop after Cancel: %#v %v", response, err)
	}
	manager.Cancel()
}

func TestTimeoutIsReportedAsEngineTimeout(t *testing.T) {
	manager, _ := helperManager(t)
	manager.timeout = 300 * time.Millisecond
	response, err := manager.Execute("robot-control", "board.info", json.RawMessage(`{"helper":"wait"}`))
	if code := errorCode(t, response, err); code != CodeEngineTimeout {
		t.Fatalf("expected %s, got %s", CodeEngineTimeout, code)
	}
}

func TestEngineErrorResponseIsForwardedUnchanged(t *testing.T) {
	manager, _ := helperManager(t)
	response, err := manager.Execute("robot-control", "servo.set", json.RawMessage(`{"helper":"engine-error"}`))
	if code := errorCode(t, response, err); code != "INVALID_INPUT" || response.Error.Message != "pwm must be an integer" {
		t.Fatalf("engine error changed: %#v", response.Error)
	}
}

func TestContractViolationsBecomeEngineFailed(t *testing.T) {
	manager, _ := helperManager(t)
	for helper, want := range map[string]string{
		"mismatch":        "contract mismatch",
		"unknown-code":    "unknown error code",
		"data-array":      "not an object",
		"error-with-data": "no details",
	} {
		response, err := manager.Execute("robot-control", "board.info", json.RawMessage(`{"helper":"`+helper+`"}`))
		if code := errorCode(t, response, err); code != CodeEngineFailed || !strings.Contains(response.Error.Message, want) {
			t.Fatalf("%s: %#v", helper, response.Error)
		}
		if response.RequestID == "" {
			t.Fatalf("%s: host failure lost the request ID", helper)
		}
	}
}

func TestEngineFailureCarriesBoundedStderr(t *testing.T) {
	manager, _ := helperManager(t)
	response, err := manager.Execute("robot-control", "board.info", json.RawMessage(`{"helper":"stderr-exit"}`))
	if code := errorCode(t, response, err); code != CodeEngineFailed {
		t.Fatalf("expected %s, got %s", CodeEngineFailed, code)
	}
	if !strings.Contains(response.Error.Message, "stderr: boom: bad firmware") {
		t.Fatalf("stderr missing from %q", response.Error.Message)
	}
}

// A verbose engine must not be killed by a full pipe: the buffers drop the excess instead of failing writes.
func TestVerboseEngineDoesNotBreakItsPipes(t *testing.T) {
	manager, _ := helperManager(t)
	response, err := manager.Execute("robot-control", "board.info", json.RawMessage(`{"helper":"noisy"}`))
	if err != nil || response.Status != "success" {
		t.Fatalf("200 KB of stderr broke the request: %#v %v", response, err)
	}
	response, err = manager.Execute("robot-control", "board.info", json.RawMessage(`{"helper":"huge-output"}`))
	if code := errorCode(t, response, err); code != CodeEngineFailed || !strings.Contains(response.Error.Message, "exceeds 1 MB") {
		t.Fatalf("oversize stdout: %#v %v", response.Error, err)
	}
}

func TestBoundedBufferDropsExcessWithoutError(t *testing.T) {
	buffer := boundedBuffer{limit: 4}
	for _, chunk := range []string{"ab", "cdef", "gh"} {
		if n, err := buffer.Write([]byte(chunk)); n != len(chunk) || err != nil {
			t.Fatalf("Write(%q) = %d, %v", chunk, n, err)
		}
	}
	if string(buffer.Bytes()) != "abcd" || !buffer.truncated {
		t.Fatalf("buffer = %q truncated=%v", buffer.Bytes(), buffer.truncated)
	}
	multibyte := boundedBuffer{limit: 100}
	_, _ = multibyte.Write([]byte(strings.Repeat("สวัสดี ", 20)))
	if excerpt := multibyte.excerpt(10); !utf8.ValidString(excerpt) || len(excerpt) > 10+len("…") {
		t.Fatalf("excerpt cut inside a rune: %q", excerpt)
	}
}

func TestKnownErrorCodesMatchTheResponseSchema(t *testing.T) {
	raw, err := os.ReadFile(filepath.Join("..", "..", "..", "contracts", "engine", "response.schema.json"))
	if err != nil {
		t.Fatal(err)
	}
	var schema struct {
		Defs struct {
			ErrorCode struct {
				Enum []string `json:"enum"`
			} `json:"errorCode"`
		} `json:"$defs"`
	}
	if err := json.Unmarshal(raw, &schema); err != nil {
		t.Fatal(err)
	}
	if len(schema.Defs.ErrorCode.Enum) != len(knownErrorCodes) {
		t.Fatalf("schema lists %d codes, host knows %d", len(schema.Defs.ErrorCode.Enum), len(knownErrorCodes))
	}
	for _, code := range schema.Defs.ErrorCode.Enum {
		if !knownErrorCodes[code] {
			t.Fatalf("host does not know schema code %s", code)
		}
	}
}

func TestOversizedRequestIsRejectedBeforeStartingAnEngine(t *testing.T) {
	manager, started := helperManager(t)
	big := `{"pad":"` + strings.Repeat("a", 70000) + `"}`
	response, err := manager.Execute("robot-control", "board.info", json.RawMessage(big))
	if code := errorCode(t, response, err); code != CodeInvalidRequest {
		t.Fatalf("expected %s, got %s", CodeInvalidRequest, code)
	}
	select {
	case <-started:
		t.Fatal("oversized request started an engine process")
	default:
	}
}

func TestBuiltZideEngineDryRun(t *testing.T) {
	suffix := ""
	if runtime.GOOS == "windows" {
		suffix = ".exe"
	}
	appRoot := filepath.Join("..", "..")
	candidates := []string{filepath.Join(appRoot, "bin", "engines", "zide", "zide-engine"+suffix)}
	if runtime.GOOS == "darwin" {
		for _, target := range []string{"darwin-" + runtime.GOARCH, "darwin-universal"} {
			candidates = append(candidates, filepath.Join(appRoot, "bin", target, "School Bot.app", "Contents", "MacOS", "engines", "zide", "zide-engine"))
		}
	}
	// A verification run that built into a separate output directory points here.
	override := os.Getenv("ZIDE_ENGINE")
	if override != "" {
		candidates = []string{override}
	}
	var binary string
	for _, candidate := range candidates {
		if info, err := os.Stat(candidate); err == nil && !info.IsDir() {
			binary, _ = filepath.Abs(candidate)
			break
		}
	}
	if binary == "" && override != "" {
		t.Fatalf("ZIDE_ENGINE does not point to an engine executable: %s", override)
	}
	if binary == "" {
		t.Skip("C++ engine not built; desktop verification builds it before running this integration test")
	}
	manager := NewManager()
	// go test does not run beside the built engines, so the known output is injected.
	manager.resolve = func(spec engineSpec) (string, error) {
		if spec.id != "zide" {
			return "", fmt.Errorf("unexpected engine %s", spec.id)
		}
		return binary, nil
	}
	response, err := manager.Execute("robot-control", "servo.set", json.RawMessage(`{"transport":"dry-run","id":1,"pwm":1500,"time_ms":300}`))
	if err != nil || response.Status != "success" {
		t.Fatalf("real C++ dry-run: %#v %v", response, err)
	}
	data, ok := response.Data.(map[string]any)
	if !ok || data["engine"] != "zide" || data["delivery"] != "dry-run" {
		t.Fatalf("unexpected C++ response: %#v", response.Data)
	}
	cmd := exec.Command(binary)
	cmd.Stdin = strings.NewReader(`{"protocol":"1.0","request_id":"wrong-mode","mode":"coding-adventure","action":"servo.set","data":{"transport":"dry-run"}}` + "\n")
	output, err := cmd.Output()
	var rejected Response
	if err != nil || json.Unmarshal(output, &rejected) != nil || rejected.Status != "error" || rejected.Error == nil || !strings.Contains(rejected.Error.Message, "robot-control") {
		t.Fatalf("C++ wrong-mode response: %s %v", output, err)
	}
}
