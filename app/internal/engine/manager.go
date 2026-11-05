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
	arm      armPorts
	session  engineSession
	building bool
	uploading bool
	simulator *hardwareSimulator
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
	manager := &Manager{
		slots:   make(chan struct{}, 1),
		timeout: requestTimeout,
		resolve: locateEngine,
		command: func(ctx context.Context, path string) *exec.Cmd {
			return exec.CommandContext(ctx, path)
		},
	}
	if os.Getenv("SCHOOL_BOT_HARDWARE_SIMULATOR") == "1" {
		manager.simulator = newHardwareSimulator(os.Getenv("SCHOOL_BOT_SIMULATOR_SCENARIO"))
	}
	return manager
}

// Cancel is what leaving a mode and closing the window ask for. It lets a robot.stop and a
// program.upload finish instead, so a page that lost its own guard (Block Lab after a crash or a
// WebView reload) cannot cut an upload off; their request deadlines bound the wait.
func (m *Manager) Cancel() { m.cancel(false) }

// StopUpload is the stop the child confirmed in Block Lab. It ends a running upload and nothing
// else: a stop that arrives after its upload ended must not end the next request, such as a
// robot motion that cancelling would not stop.
func (m *Manager) StopUpload() { m.cancel(true) }

// UploadRunning tells a closing window why it waits.
func (m *Manager) UploadRunning() bool {
	m.mu.Lock()
	defer m.mu.Unlock()
	return m.uploading
}

func (m *Manager) isBuilding() bool {
	m.mu.Lock()
	defer m.mu.Unlock()
	return m.building
}

func (m *Manager) cancel(stopUpload bool) {
	m.mu.Lock()
	if stopUpload && !m.uploading {
		m.mu.Unlock()
		return
	}
	done := m.done
	if m.active != nil && !m.stopping && (stopUpload || !m.uploading) {
		m.active()
	}
	m.mu.Unlock()
	// Wait without holding the lock: Execute must reap the child and release its
	// slot before a subsequent physical robot.stop can start.
	if done != nil {
		<-done
	}
	// Leaving a mode or closing the app also lets go of the board's Serial port. It comes after
	// the slot is free, and end() gives up waiting on a stuck process, so a robot stop that
	// waits for this cancel is not held back by the Serial monitor.
	m.session.close()
}

// Execute returns a Go error only when the caller asked for a mode or action the host never allows.
// Every failure after that point is a Response with an error code, so the frontend decides by code.
func (m *Manager) Execute(mode, action string, data json.RawMessage) (Response, error) {
	r, err := routeFor(mode, action)
	if err != nil {
		return Response{}, err
	}
	if !pageDataAccepted(data, r) {
		return hostFailure("", CodeInvalidRequest, "request data has fields or blocks this mode does not send"), nil
	}
	data, ok := withFixedFields(data, r.fixed)
	if !ok {
		return hostFailure("", CodeInvalidRequest, "request data must be a JSON object without host-owned fields"), nil
	}
	opensPort := action == "program.upload" || action == "serial.open"
	if mode == "block-programming" && opensPort && m.arm.holds(exactText(data, "port")) {
		return hostFailure("", CodeSafetyLimit, "Robot Control drove the robot arm on this port; choose the UNO's port"), nil
	}
	if r.session {
		return m.session.run(m, r.engine, mode, action, data), nil
	}
	timeout := m.timeout
	if r.timeout > 0 {
		timeout = r.timeout
	}
	m.mu.Lock()
	select {
	case m.slots <- struct{}{}:
	default:
		m.mu.Unlock()
		return hostFailure("", CodeEngineBusy, "engine is busy; stop or wait for the current request"), nil
	}
	ctx, cancel := context.WithCancel(context.Background())
	done := make(chan struct{})
	m.active = cancel
	m.stopping = action == "robot.stop"
	m.building = action == "program.compile" || action == "program.upload"
	m.uploading = action == "program.upload"
	m.done = done
	m.mu.Unlock()
	defer func() {
		cancel()
		m.mu.Lock()
		m.active = nil
		m.stopping = false
		m.building = false
		m.uploading = false
		m.done = nil
		<-m.slots
		close(done)
		m.mu.Unlock()
	}()
	if mode == "robot-control" && m.simulator != nil {
		return m.simulator.execute(ctx, action, data), nil
	}
	// avrdude needs the board's port, which an open Serial monitor holds. Closing it only once
	// this upload has the slot keeps a busy refusal from ending the session for nothing.
	if action == "program.upload" {
		m.session.close()
	}
	// The deadline starts after the session has let go of the port, which can take seconds, so
	// it does not eat into the engine's own compile and upload budget.
	ctx, stopTimer := context.WithTimeout(ctx, timeout)
	defer stopTimer()
	res := m.runEngine(ctx, timeout, r.engine, mode, action, data)
	m.arm.note(mode, action, data, res)
	return res, nil
}

func (m *Manager) runEngine(
	ctx context.Context, timeout time.Duration, spec engineSpec, mode, action string, data json.RawMessage,
) Response {
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
			return hostFailure(req.RequestID, CodeEngineTimeout, "engine did not finish within "+timeout.String())
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
