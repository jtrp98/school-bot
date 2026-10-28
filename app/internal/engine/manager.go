package engine

import (
	"bytes"
	"context"
	"crypto/rand"
	"encoding/hex"
	"encoding/json"
	"errors"
	"fmt"
	"os"
	"os/exec"
	"path/filepath"
	"runtime"
	"sync"
	"time"
)

const (
	requestTimeout       = 5 * time.Second
	MaxRequestBytes      = 65536
	processWaitDelay     = time.Second
	maxOutputBytes       = 1 << 20
	maxDiagnosticBytes   = 8192
	maxDiagnosticExcerpt = 512
)

// A trusted registry prevents the frontend from selecting executables.
// Child processes provide crash isolation, not a security sandbox.
type Manager struct {
	slots    chan struct{}
	mu       sync.Mutex
	active   context.CancelFunc
	stopping bool
	done     chan struct{}
	timeout  time.Duration
	resolve  func(engineSpec) (string, error)
	command  func(context.Context, string) *exec.Cmd
}

type engineSpec struct{ id, relativePath string }

func engineForMode(mode string) (engineSpec, error) {
	switch mode {
	case "robot-control":
		return engineSpec{"zide", filepath.Join("engines", "zide", "zide-engine")}, nil
	case "coding-adventure", "block-programming":
		return engineSpec{"demo", "demo-engine"}, nil
	default:
		return engineSpec{}, fmt.Errorf("unsupported engine mode %q", mode)
	}
}

func ValidMode(mode string) bool {
	_, err := engineForMode(mode)
	return err == nil
}

// The webview is untrusted, so the host admits only the actions each mode's UI
// really sends; an engine action missing here is unreachable even if it exists.
var modeActions = map[string]map[string]struct{}{
	"robot-control": actionSet(
		"device.list", "board.info", "servo.set", "servo.read", "servo.torque", "robot.stop",
		"group.execute", "group.download", "group.run",
	),
	"coding-adventure":  actionSet("program.run"),
	"block-programming": actionSet("program.run"),
}

func actionSet(actions ...string) map[string]struct{} {
	set := make(map[string]struct{}, len(actions))
	for _, action := range actions {
		set[action] = struct{}{}
	}
	return set
}

func checkAction(mode, action string) error {
	if _, ok := modeActions[mode][action]; !ok {
		return fmt.Errorf("action %q is not allowed for mode %q", action, mode)
	}
	return nil
}

func locateEngine(spec engineSpec) (string, error) {
	name := spec.relativePath
	if runtime.GOOS == "windows" {
		name += ".exe"
	}
	executable, err := os.Executable()
	if err != nil {
		return "", err
	}
	// Only the directory of the running executable is trusted. The working
	// directory is set by launchers and shortcuts, so searching it would let a
	// planted engine run instead of the shipped one.
	candidate := filepath.Join(filepath.Dir(executable), name)
	if info, err := os.Stat(candidate); err == nil && !info.IsDir() {
		return filepath.Abs(candidate)
	}
	return "", fmt.Errorf("%s engine not found beside the application: run the desktop build script first", spec.id)
}

func NewManager() *Manager {
	return &Manager{
		slots:   make(chan struct{}, 1),
		timeout: requestTimeout,
		resolve: locateEngine,
		command: func(ctx context.Context, path string) *exec.Cmd {
			return exec.CommandContext(ctx, path)
		},
	}
}

func (m *Manager) Cancel() {
	m.mu.Lock()
	done := m.done
	// Navigation and shutdown can cancel concurrently with the stop sent by Robot Control.
	// Keep that stop alive; its existing request deadline still bounds the wait.
	if m.active != nil && !m.stopping {
		m.active()
	}
	m.mu.Unlock()
	// Wait without holding the lock: Execute must reap the child and release its
	// slot before a subsequent physical robot.stop can start.
	if done != nil {
		<-done
	}
}

// Execute returns a Go error only when the caller asked for a mode or action the host never allows.
// Every failure after that point is a Response with an error code, so the frontend decides by code.
func (m *Manager) Execute(mode, action string, data json.RawMessage) (Response, error) {
	spec, err := engineForMode(mode)
	if err != nil {
		return Response{}, err
	}
	if err := checkAction(mode, action); err != nil {
		return Response{}, err
	}
	m.mu.Lock()
	select {
	case m.slots <- struct{}{}:
	default:
		m.mu.Unlock()
		return hostFailure("", CodeEngineBusy, "engine is busy; stop or wait for the current request"), nil
	}
	ctx, cancel := context.WithTimeout(context.Background(), m.timeout)
	done := make(chan struct{})
	m.active = cancel
	m.stopping = action == "robot.stop"
	m.done = done
	m.mu.Unlock()
	defer func() {
		cancel()
		m.mu.Lock()
		m.active = nil
		m.stopping = false
		m.done = nil
		<-m.slots
		close(done)
		m.mu.Unlock()
	}()
	return m.runEngine(ctx, spec, mode, action, data), nil
}

func (m *Manager) runEngine(ctx context.Context, spec engineSpec, mode, action string, data json.RawMessage) Response {
	path, err := m.resolve(spec)
	if err != nil {
		return hostFailure("", CodeEngineFailed, err.Error())
	}
	id := make([]byte, 16)
	if _, err := rand.Read(id); err != nil {
		return hostFailure("", CodeEngineFailed, "could not create a request ID")
	}
	req := Request{Protocol: Protocol, RequestID: hex.EncodeToString(id), Mode: mode, Action: action, Data: data}
	payload, err := json.Marshal(req)
	if err != nil {
		return hostFailure(req.RequestID, CodeEngineFailed, "could not encode the request")
	}
	if len(payload) > MaxRequestBytes {
		return hostFailure(req.RequestID, CodeInvalidRequest, "request exceeds 64 KB")
	}
	cmd := m.command(ctx, path)
	configureCommand(cmd)
	cmd.Stdin = bytes.NewReader(append(payload, '\n'))
	cmd.Env = append(cmd.Env, engineEnvironment()...)
	// A descendant can inherit the pipes after the engine exits or is killed; bound the drain before stop can run.
	cmd.WaitDelay = processWaitDelay
	out := boundedBuffer{limit: maxOutputBytes}
	diagnostic := boundedBuffer{limit: maxDiagnosticBytes}
	cmd.Stdout = &out
	cmd.Stderr = &diagnostic
	if err := cmd.Run(); err != nil {
		switch {
		case errors.Is(ctx.Err(), context.Canceled):
			return hostFailure(req.RequestID, CodeEngineCancelled, "engine request was cancelled")
		case errors.Is(ctx.Err(), context.DeadlineExceeded):
			return hostFailure(req.RequestID, CodeEngineTimeout, "engine did not finish within "+m.timeout.String())
		}
		return engineFailure(req.RequestID, fmt.Sprintf("engine failed: %v", err), &diagnostic)
	}
	if out.truncated {
		return engineFailure(req.RequestID, "engine output exceeds 1 MB", &diagnostic)
	}
	var res Response
	if err := json.Unmarshal(bytes.TrimSpace(out.Bytes()), &res); err != nil {
		return engineFailure(req.RequestID, "engine returned malformed JSON", &diagnostic)
	}
	if problem := contractProblem(res, req.RequestID); problem != "" {
		return engineFailure(req.RequestID, problem, &diagnostic)
	}
	return res
}

// contractProblem is empty when res matches contracts/engine/response.schema.json for this request.
func contractProblem(res Response, requestID string) string {
	if res.Protocol != Protocol || res.RequestID != requestID {
		return "engine response contract mismatch"
	}
	if res.Data != nil {
		if _, ok := res.Data.(map[string]any); !ok {
			return "engine response data is not an object"
		}
	}
	switch res.Status {
	case "success":
		if res.Error != nil {
			return "engine response contract mismatch"
		}
	case "error":
		if res.Error == nil || res.Error.Message == "" || res.Data != nil {
			return "engine error has no details"
		}
		if !knownErrorCodes[res.Error.Code] {
			return "engine returned an unknown error code"
		}
	default:
		return "engine response contract mismatch"
	}
	return ""
}

func hostFailure(requestID, code, message string) Response {
	return Response{Protocol: Protocol, RequestID: requestID, Status: "error", Error: &Error{Code: code, Message: message}}
}

// engineFailure keeps the engine's own stderr, bounded, because the exit status alone hides the cause.
func engineFailure(requestID, summary string, diagnostic *boundedBuffer) Response {
	if detail := diagnostic.excerpt(maxDiagnosticExcerpt); detail != "" {
		summary += "; stderr: " + detail
	}
	return hostFailure(requestID, CodeEngineFailed, summary)
}
