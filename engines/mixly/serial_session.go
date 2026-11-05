package main

import (
	"encoding/base64"
	"errors"
	"io"
	"sync"
	"time"

	"go.bug.st/serial"
)

// A serial session keeps one board's port open across the requests of a long-lived engine
// process, so Block Lab reads what the program prints as it prints it. Opening the port for each
// read would restart an UNO every time (its reset follows DTR) and lose what came in between.
const (
	sessionBaudRate = 9600
	// Kept until read; when a page stops reading, the oldest bytes go first.
	sessionBufferBytes = 64 << 10
	sessionReadBytes   = 16 << 10
	sessionWriteBytes  = 256
	sessionReadTimeout = 200 * time.Millisecond
	// Opening the port raises DTR and restarts an UNO. The line carries noise in the first
	// milliseconds of that reset, which would show as broken characters before the program's
	// first line; the bootloader then waits about a second before the program prints.
	sessionSettle = 250 * time.Millisecond
)

type SerialOpenInput struct {
	Port     string `json:"port"`
	VID      string `json:"vid"`
	PID      string `json:"pid"`
	BaudRate int    `json:"baud_rate,omitempty"`
	// Only "native" opens the port; anything else answers as a dry run, like the other device actions.
	Transport string `json:"transport,omitempty"`
}

type SerialWriteInput struct {
	WriteBase64 string `json:"write_base64"`
}

type SerialState struct {
	Engine   string `json:"engine"`
	Open     bool   `json:"open"`
	Port     string `json:"port,omitempty"`
	BaudRate int    `json:"baud_rate,omitempty"`
	Received string `json:"received_base64,omitempty"`
	// Older bytes were dropped because nobody read them in time.
	Dropped bool `json:"dropped,omitempty"`
	// Why reading stopped by itself, such as the cable coming out.
	Problem  string `json:"problem,omitempty"`
	Written  int    `json:"written,omitempty"`
	Delivery string `json:"delivery,omitempty"`
}

type serialSession struct {
	mu       sync.Mutex
	port     io.ReadWriteCloser
	name     string
	baudRate int
	buffer   []byte
	dropped  bool
	problem  string
	closing  bool
	done     chan struct{}
	// Bytes read before this are the reset's noise and are not kept.
	settleUntil time.Time
}

var boardSerial serialSession

// start reads port into the buffer until it fails or the session closes it, dropping what
// arrives during settle.
func (s *serialSession) start(port io.ReadWriteCloser, name string, baudRate int, settle time.Duration) {
	s.port, s.name, s.baudRate = port, name, baudRate
	s.buffer, s.dropped, s.problem, s.closing = nil, false, "", false
	s.settleUntil = time.Now().Add(settle)
	s.done = make(chan struct{})
	go s.pump(port, s.done)
}

func (s *serialSession) pump(port io.ReadCloser, done chan struct{}) {
	defer close(done)
	chunk := make([]byte, 1024)
	for {
		count, err := port.Read(chunk)
		s.mu.Lock()
		if count > 0 && !time.Now().Before(s.settleUntil) {
			s.buffer = append(s.buffer, chunk[:count]...)
			if overflow := len(s.buffer) - sessionBufferBytes; overflow > 0 {
				s.buffer = append([]byte(nil), s.buffer[overflow:]...)
				s.dropped = true
			}
		}
		if err != nil {
			// A port that failed by itself (the cable came out) is let go at once, so the handle
			// of an unplugged bridge is not held until the next open.
			if !s.closing {
				s.problem = err.Error()
				_ = port.Close()
			}
			s.mu.Unlock()
			return
		}
		s.mu.Unlock()
	}
}

func (s *serialSession) state() SerialState {
	return SerialState{Engine: "mixly", Open: s.port != nil && s.problem == "", Port: s.name, BaudRate: s.baudRate}
}

func (s *serialSession) read() SerialState {
	s.mu.Lock()
	defer s.mu.Unlock()
	state := s.state()
	count := min(len(s.buffer), sessionReadBytes)
	if count > 0 {
		state.Received = base64.StdEncoding.EncodeToString(s.buffer[:count])
		s.buffer = s.buffer[count:]
	}
	state.Dropped, state.Problem = s.dropped, s.problem
	s.dropped = false
	return state
}

func (s *serialSession) write(data []byte) (SerialState, error) {
	s.mu.Lock()
	defer s.mu.Unlock()
	if s.port == nil || s.problem != "" {
		return s.state(), &ProtocolError{"TRANSPORT_ERROR", "the board's serial port is not open"}
	}
	written, err := s.port.Write(data)
	state := s.state()
	state.Written = written
	if err != nil || written != len(data) {
		return state, &ProtocolError{"TRANSPORT_ERROR", "serial write was incomplete"}
	}
	return state, nil
}

// close is safe to call with no session and waits until the reader has stopped.
func (s *serialSession) close() SerialState {
	s.mu.Lock()
	port, done := s.port, s.done
	s.closing = true
	s.mu.Unlock()
	if port != nil {
		_ = port.Close()
		<-done
	}
	s.mu.Lock()
	defer s.mu.Unlock()
	s.port, s.name, s.baudRate, s.buffer, s.problem = nil, "", 0, nil, ""
	return s.state()
}

func openBoardSerial(input SerialOpenInput) (SerialState, error) {
	if input.Port == "" || len(input.Port) > 256 {
		return SerialState{Engine: "mixly"}, &ProtocolError{"INVALID_INPUT", "select a serial port"}
	}
	if !usbID.MatchString(input.VID) || !usbID.MatchString(input.PID) {
		return SerialState{Engine: "mixly"}, &ProtocolError{"INVALID_INPUT",
			"serial.open requires the board's USB vid and pid (four hex digits each)"}
	}
	if input.BaudRate == 0 {
		input.BaudRate = sessionBaudRate
	}
	if input.BaudRate < 300 || input.BaudRate > 2000000 {
		return SerialState{Engine: "mixly"}, &ProtocolError{"SAFETY_LIMIT", "baud_rate must be 300-2000000"}
	}
	if input.Transport != "" && input.Transport != "dry-run" && input.Transport != "native" {
		return SerialState{Engine: "mixly"}, &ProtocolError{"INVALID_INPUT", "transport must be dry-run or native"}
	}
	if input.Transport != "native" {
		return SerialState{Engine: "mixly", Delivery: "dry-run"}, nil
	}
	boardSerial.mu.Lock()
	current := boardSerial.state()
	boardSerial.mu.Unlock()
	if current.Open {
		if current.Port == input.Port {
			return current, nil
		}
		return current, &ProtocolError{"INVALID_INPUT", "close the open serial port first"}
	}
	boardSerial.close()
	// The same check as upload: only the UNO or clone bridge the child picked, never a port
	// that now belongs to another device.
	if _, err := checkBoardPort(input.Port, input.VID, input.PID); err != nil {
		return SerialState{Engine: "mixly"}, err
	}
	port, err := serial.Open(input.Port, &serial.Mode{
		BaudRate: input.BaudRate, DataBits: 8, Parity: serial.NoParity, StopBits: serial.OneStopBit,
	})
	if err != nil {
		var portError *serial.PortError
		if errors.As(err, &portError) && portError.Code() == serial.PortBusy {
			return SerialState{Engine: "mixly"}, &ProtocolError{"TRANSPORT_ERROR",
				"another program is using the board's port: " + err.Error()}
		}
		return SerialState{Engine: "mixly"}, &ProtocolError{"TRANSPORT_ERROR", err.Error()}
	}
	if err := port.SetReadTimeout(sessionReadTimeout); err != nil {
		_ = port.Close()
		return SerialState{Engine: "mixly"}, &ProtocolError{"TRANSPORT_ERROR", err.Error()}
	}
	boardSerial.mu.Lock()
	defer boardSerial.mu.Unlock()
	// A read that times out answers 0 bytes and no error, which pump takes as a quiet line.
	boardSerial.start(port, input.Port, input.BaudRate, sessionSettle)
	state := boardSerial.state()
	state.Delivery = "native"
	return state, nil
}

func writeBoardSerial(input SerialWriteInput) (SerialState, error) {
	data, err := base64.StdEncoding.DecodeString(input.WriteBase64)
	if err != nil || len(data) == 0 || len(data) > sessionWriteBytes {
		return SerialState{Engine: "mixly"}, &ProtocolError{"INVALID_INPUT", "write_base64 must encode 1-256 bytes"}
	}
	return boardSerial.write(data)
}
