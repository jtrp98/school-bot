package main

import (
	"encoding/base64"
	"errors"
	"strings"
	"sync"
	"testing"
	"time"
)

// fakePort hands out queued chunks, answers an idle read with 0 bytes like a read timeout, and
// fails reads once closed or unplugged.
type fakePort struct {
	mu      sync.Mutex
	chunks  [][]byte
	written []byte
	failure error
	closed  bool
}

func (port *fakePort) Read(buffer []byte) (int, error) {
	port.mu.Lock()
	defer port.mu.Unlock()
	if port.failure != nil {
		return 0, port.failure
	}
	if len(port.chunks) == 0 {
		port.mu.Unlock()
		time.Sleep(time.Millisecond)
		port.mu.Lock()
		return 0, nil
	}
	count := copy(buffer, port.chunks[0])
	port.chunks = port.chunks[1:]
	return count, nil
}

func (port *fakePort) Write(data []byte) (int, error) {
	port.mu.Lock()
	defer port.mu.Unlock()
	port.written = append(port.written, data...)
	return len(data), nil
}

func (port *fakePort) Close() error {
	port.mu.Lock()
	port.closed = true
	port.mu.Unlock()
	port.fail(errors.New("port closed"))
	return nil
}

func (port *fakePort) fail(err error) {
	port.mu.Lock()
	defer port.mu.Unlock()
	port.failure = err
}

func (port *fakePort) push(text string) {
	port.mu.Lock()
	defer port.mu.Unlock()
	port.chunks = append(port.chunks, []byte(text))
}

func readUntil(t *testing.T, session *serialSession, want func(SerialState) bool) SerialState {
	t.Helper()
	deadline := time.Now().Add(2 * time.Second)
	var received []byte
	for time.Now().Before(deadline) {
		state := session.read()
		data, _ := base64.StdEncoding.DecodeString(state.Received)
		received = append(received, data...)
		state.Received = string(received)
		if want(state) {
			return state
		}
		time.Sleep(time.Millisecond)
	}
	t.Fatalf("session never reached the expected state; received %q", received)
	return SerialState{}
}

func TestSerialSessionReadsWhatTheBoardPrints(t *testing.T) {
	var session serialSession
	port := &fakePort{}
	session.mu.Lock()
	session.start(port, "COM3", 9600, 0)
	session.mu.Unlock()
	port.push("hello ")
	port.push("board\n")
	state := readUntil(t, &session, func(state SerialState) bool { return state.Received == "hello board\n" })
	if !state.Open || state.Port != "COM3" || state.BaudRate != 9600 {
		t.Fatalf("%+v", state)
	}
	if _, err := session.write([]byte("hi\n")); err != nil || string(port.written) != "hi\n" {
		t.Fatalf("write: %q %v", port.written, err)
	}
	// The cable comes out: reading stops by itself and says why.
	port.fail(errors.New("device unplugged"))
	state = readUntil(t, &session, func(state SerialState) bool { return state.Problem != "" })
	port.mu.Lock()
	released := port.closed
	port.mu.Unlock()
	if state.Open || !strings.Contains(state.Problem, "unplugged") || !released {
		t.Fatalf("%+v", state)
	}
	if _, err := session.write([]byte("x")); protocolCode(err) != "TRANSPORT_ERROR" {
		t.Fatalf("write after unplug: %v", err)
	}
	if closed := session.close(); closed.Open || closed.Port != "" {
		t.Fatalf("%+v", closed)
	}
}

// The noise of the UNO's reset right after opening never reaches the page; the program's
// first line, printed after the bootloader, does.
func TestSerialSessionDropsTheResetNoise(t *testing.T) {
	var session serialSession
	port := &fakePort{}
	port.push("\x00\x80\x1ef\x00\x80\x9e")
	session.mu.Lock()
	session.start(port, "COM3", 9600, 50*time.Millisecond)
	session.mu.Unlock()
	time.Sleep(80 * time.Millisecond)
	port.push("hello\r\n")
	readUntil(t, &session, func(state SerialState) bool { return state.Received == "hello\r\n" })
	session.close()
}

// A page that stops reading loses the oldest output, and is told so, instead of the engine
// growing without bound.
func TestSerialSessionKeepsTheNewestOutput(t *testing.T) {
	var session serialSession
	port := &fakePort{}
	for index := 0; index < sessionBufferBytes/1024+8; index++ {
		port.push(strings.Repeat(string(rune('a'+index%26)), 1024))
	}
	port.push("end")
	session.mu.Lock()
	session.start(port, "COM3", 9600, 0)
	session.mu.Unlock()
	deadline := time.Now().Add(2 * time.Second)
	for {
		port.mu.Lock()
		left := len(port.chunks)
		port.mu.Unlock()
		if left == 0 || time.Now().After(deadline) {
			break
		}
		time.Sleep(time.Millisecond)
	}
	time.Sleep(10 * time.Millisecond)
	first := session.read()
	data, _ := base64.StdEncoding.DecodeString(first.Received)
	if !first.Dropped || len(data) != sessionReadBytes {
		t.Fatalf("dropped=%v read=%d", first.Dropped, len(data))
	}
	var rest []byte
	for state := session.read(); state.Received != ""; state = session.read() {
		chunk, _ := base64.StdEncoding.DecodeString(state.Received)
		rest = append(rest, chunk...)
		if state.Dropped {
			t.Fatal("drop reported twice")
		}
	}
	if total := len(data) + len(rest); total != sessionBufferBytes || !strings.HasSuffix(string(rest), "end") {
		t.Fatalf("kept %d bytes ending %q", total, string(rest[len(rest)-3:]))
	}
	session.close()
}

func TestSerialSessionRequestsAreChecked(t *testing.T) {
	boardSerial.close()
	for name, c := range map[string]struct {
		input SerialOpenInput
		code  string
	}{
		"no port":       {SerialOpenInput{VID: "1A86", PID: "7523"}, "INVALID_INPUT"},
		"no usb ids":    {SerialOpenInput{Port: "COM3"}, "INVALID_INPUT"},
		"baud":          {SerialOpenInput{Port: "COM3", VID: "1A86", PID: "7523", BaudRate: 5000000}, "SAFETY_LIMIT"},
		"not connected": {SerialOpenInput{Port: "COM-test-missing", VID: "1A86", PID: "7523", Transport: "native"}, "TRANSPORT_ERROR"},
		"transport":     {SerialOpenInput{Port: "COM3", VID: "1A86", PID: "7523", Transport: "usb"}, "INVALID_INPUT"},
	} {
		if _, err := openBoardSerial(c.input); protocolCode(err) != c.code {
			t.Errorf("%s: %v", name, err)
		}
	}
	// Without native transport nothing is opened, even for a board that is plugged in.
	if state, err := openBoardSerial(SerialOpenInput{Port: "COM3", VID: "1A86", PID: "7523"}); err != nil ||
		state.Open || state.Delivery != "dry-run" {
		t.Fatalf("open without native transport: %+v %v", state, err)
	}
	if state := boardSerial.read(); state.Open || state.Received != "" {
		t.Fatalf("read with no session: %+v", state)
	}
	for _, data := range []string{"", "!", base64.StdEncoding.EncodeToString(make([]byte, 257))} {
		if _, err := writeBoardSerial(SerialWriteInput{WriteBase64: data}); protocolCode(err) != "INVALID_INPUT" {
			t.Errorf("write %.12q: %v", data, err)
		}
	}
	if _, err := writeBoardSerial(SerialWriteInput{WriteBase64: "aGk="}); protocolCode(err) != "TRANSPORT_ERROR" {
		t.Errorf("write with no session: %v", err)
	}
}
