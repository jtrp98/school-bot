package engine

import (
	"bufio"
	"bytes"
	"context"
	"crypto/rand"
	"encoding/hex"
	"encoding/json"
	"io"
	"sync"
	"time"
)

// Block Lab's Serial monitor keeps the board's port open between requests (engines/mixly
// docs/API.md, "Serial session"), so its requests go to one engine process that stays running
// instead of a process per request. The session has its own lock, not the request slot: reads
// come several times a second and must neither wait behind a two-minute compile nor make the
// port list busy.
type engineSession struct {
	mu         sync.Mutex
	stop       context.CancelFunc
	stdin      io.WriteCloser
	lines      *bufio.Reader
	exited     chan struct{}
	diagnostic *boundedBuffer
	open       bool
	// Ends a session nobody reads any more: a page that reloaded or crashed, or a serial.open
	// that arrived after the cancel of leaving Block Lab, would otherwise hold the port.
	idle      *time.Timer
	idleRound int
}

// The longest answer is a read of 16 KB as base64 in its envelope.
const sessionLineBytes = 64 << 10

// The page reads every 300 ms while the Serial monitor is open. A variable so tests need not wait.
var sessionIdleTimeout = 15 * time.Second

func sessionReply(data any) Response {
	return Response{Protocol: Protocol, Status: "success", Data: data}
}

// run sends one session request, starting the engine for serial.open.
func (s *engineSession) run(m *Manager, spec engineSpec, mode, action string, data json.RawMessage) Response {
	s.mu.Lock()
	defer s.mu.Unlock()
	// Checked under the session lock: an upload marks itself building before it closes the
	// session, which waits for this lock, so a port opened here is closed again before avrdude.
	if action == "serial.open" && m.isBuilding() {
		return hostFailure("", CodeEngineBusy, "a compile or upload is using the board; wait for it to finish")
	}
	if s.stdin == nil {
		switch action {
		case "serial.open":
			if problem := s.start(m, spec); problem != nil {
				return *problem
			}
		case "serial.write":
			return hostFailure("", "TRANSPORT_ERROR", "the board's serial port is not open")
		default:
			return sessionReply(map[string]any{"engine": spec.id, "open": false})
		}
	}
	res := s.request(mode, action, data)
	switch {
	case res.Status != "success" && action == "serial.open" && !s.open,
		res.Error != nil && (res.Error.Code == CodeEngineFailed || res.Error.Code == CodeEngineTimeout),
		action == "serial.close":
		s.end()
	case action == "serial.open":
		s.open = true
	}
	if s.stdin != nil {
		s.watchIdle()
	}
	return res
}

func (s *engineSession) watchIdle() {
	s.idleRound++
	round := s.idleRound
	if s.idle != nil {
		s.idle.Stop()
	}
	s.idle = time.AfterFunc(sessionIdleTimeout, func() {
		s.mu.Lock()
		defer s.mu.Unlock()
		// A request that came in meanwhile started a new round.
		if round == s.idleRound {
			s.end()
		}
	})
}

func (s *engineSession) start(m *Manager, spec engineSpec) *Response {
	path, err := m.resolve(spec)
	if err != nil {
		failure := hostFailure("", CodeEngineFailed, err.Error())
		return &failure
	}
	ctx, stop := context.WithCancel(context.Background())
	cmd := m.command(ctx, path)
	configureCommand(cmd)
	cmd.Env = append(cmd.Env, engineEnvironment()...)
	cmd.WaitDelay = processWaitDelay
	diagnostic := &boundedBuffer{limit: maxDiagnosticBytes}
	cmd.Stderr = diagnostic
	stdin, err := cmd.StdinPipe()
	var stdout io.ReadCloser
	if err == nil {
		stdout, err = cmd.StdoutPipe()
	}
	if err == nil {
		err = cmd.Start()
	}
	if err != nil {
		stop()
		failure := hostFailure("", CodeEngineFailed, "could not start the serial session: "+err.Error())
		return &failure
	}
	s.stop, s.stdin, s.exited, s.diagnostic, s.open = stop, stdin, make(chan struct{}), diagnostic, false
	s.lines = bufio.NewReaderSize(stdout, sessionLineBytes)
	exited := s.exited
	go func() {
		_ = cmd.Wait()
		close(exited)
	}()
	return nil
}

func (s *engineSession) request(mode, action string, data json.RawMessage) Response {
	id := make([]byte, 16)
	if _, err := rand.Read(id); err != nil {
		return hostFailure("", CodeEngineFailed, "could not create a request ID")
	}
	req := Request{Protocol: Protocol, RequestID: hex.EncodeToString(id), Mode: mode, Action: action, Data: data}
	payload, err := json.Marshal(req)
	if err != nil || len(payload) > MaxRequestBytes {
		return hostFailure(req.RequestID, CodeInvalidRequest, "request exceeds 64 KB")
	}
	if _, err := s.stdin.Write(append(payload, '\n')); err != nil {
		return s.stopped(req.RequestID)
	}
	answer := make(chan []byte, 1)
	// The reader is handed over: end() clears s.lines while this goroutine may still be waiting.
	lines := s.lines
	go func() {
		line, err := lines.ReadSlice('\n')
		if err != nil {
			line = nil
		}
		answer <- bytes.Clone(line)
	}()
	timer := time.NewTimer(requestTimeout)
	defer timer.Stop()
	var line []byte
	select {
	case line = <-answer:
	case <-timer.C:
		// The reader goroutine ends when end() kills the process and its pipe closes.
		return hostFailure(req.RequestID, CodeEngineTimeout, "serial session did not answer within "+requestTimeout.String())
	}
	if line == nil {
		return s.stopped(req.RequestID)
	}
	var res Response
	if err := json.Unmarshal(bytes.TrimSpace(line), &res); err != nil {
		return hostFailure(req.RequestID, CodeEngineFailed, "serial session returned malformed JSON")
	}
	if problem := contractProblem(res, req.RequestID); problem != "" {
		return hostFailure(req.RequestID, CodeEngineFailed, problem)
	}
	return res
}

// stopped reports an engine that went away, with what it wrote to stderr once it has exited.
func (s *engineSession) stopped(requestID string) Response {
	select {
	case <-s.exited:
		return engineFailure(requestID, "the serial session stopped", s.diagnostic)
	case <-time.After(processWaitDelay):
		return hostFailure(requestID, CodeEngineFailed, "the serial session stopped")
	}
}

// end closes the engine's stdin, so it releases the port and exits, and kills it if it does
// not exit within processWaitDelay. It gives up waiting after that too: a process stuck in a
// serial driver must not keep Cancel, and the robot stop behind it, waiting. Call with s.mu held.
func (s *engineSession) end() {
	if s.idle != nil {
		s.idle.Stop()
		s.idle = nil
	}
	if s.stdin == nil {
		return
	}
	_ = s.stdin.Close()
	select {
	case <-s.exited:
	case <-time.After(processWaitDelay):
		s.stop()
		select {
		case <-s.exited:
		case <-time.After(processWaitDelay):
		}
	}
	s.stop()
	s.stdin, s.lines, s.stop, s.exited, s.diagnostic, s.open = nil, nil, nil, nil, nil, false
}

// close ends the session before anything else may need the board's port.
func (s *engineSession) close() {
	s.mu.Lock()
	defer s.mu.Unlock()
	s.end()
}
