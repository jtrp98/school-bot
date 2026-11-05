package engine

import (
	"context"
	"encoding/json"
	"os"
	"os/exec"
	"runtime"
	"strings"
	"sync/atomic"
	"testing"
	"time"
)

// TestSessionHelperProcess plays a mixly engine that answers every line until stdin closes, as
// the real one does; COM9 is a port that is not connected.
func TestSessionHelperProcess(t *testing.T) {
	if os.Getenv("SCHOOL_BOT_SESSION_TEST_HELPER") != "1" {
		return
	}
	decoder := json.NewDecoder(os.Stdin)
	encoder := json.NewEncoder(os.Stdout)
	for {
		var req Request
		if err := decoder.Decode(&req); err != nil {
			os.Exit(0)
		}
		var fields map[string]any
		_ = json.Unmarshal(req.Data, &fields)
		response := Response{Protocol: Protocol, RequestID: req.RequestID, Status: "success",
			Data: map[string]any{"action": req.Action, "process": os.Getpid(), "transport": fields["transport"]}}
		if req.Action == "program.compile" {
			time.Sleep(time.Second)
		}
		if req.Action == "serial.open" && strings.Contains(string(req.Data), "COM9") {
			response.Status, response.Data = "error", nil
			response.Error = &Error{Code: "TRANSPORT_ERROR", Message: "the selected port is not connected"}
		}
		if err := encoder.Encode(response); err != nil {
			os.Exit(3)
		}
	}
}

func sessionManager(t *testing.T) (*Manager, *atomic.Int32) {
	t.Helper()
	var started atomic.Int32
	manager := NewManager()
	manager.resolve = func(engineSpec) (string, error) { return os.Args[0], nil }
	manager.command = func(ctx context.Context, path string) *exec.Cmd {
		started.Add(1)
		cmd := exec.CommandContext(ctx, path, "-test.run=^TestSessionHelperProcess$")
		cmd.Env = []string{"SCHOOL_BOT_SESSION_TEST_HELPER=1"}
		return cmd
	}
	t.Cleanup(manager.Cancel)
	return manager, &started
}

func sessionCall(t *testing.T, manager *Manager, mode, action, data string) Response {
	t.Helper()
	response, err := manager.Execute(mode, action, json.RawMessage(data))
	if err != nil {
		t.Fatalf("%s: %v", action, err)
	}
	return response
}

func dataField(response Response, name string) any {
	fields, _ := response.Data.(map[string]any)
	return fields[name]
}

// One engine keeps the board's port between Serial requests; nothing starts until a port opens,
// and an upload, a failed open or leaving the mode ends it.
func TestBlockLabSerialSessionKeepsOneEngine(t *testing.T) {
	manager, started := sessionManager(t)
	open := `{"port":"COM5","vid":"1A86","pid":"7523"}`
	if read := sessionCall(t, manager, "block-programming", "serial.read", `{}`); dataField(read, "open") != false {
		t.Fatalf("read with no session: %#v", read)
	}
	if write := sessionCall(t, manager, "block-programming", "serial.write", `{"write_base64":"aGk="}`); write.Error == nil ||
		write.Error.Code != "TRANSPORT_ERROR" {
		t.Fatalf("write with no session: %#v", write)
	}
	if started.Load() != 0 {
		t.Fatal("an engine started without a port to open")
	}
	if opened := sessionCall(t, manager, "block-programming", "serial.open", open); opened.Status != "success" ||
		dataField(opened, "transport") != "native" {
		t.Fatalf("open: %#v", opened)
	}
	first := sessionCall(t, manager, "block-programming", "serial.read", `{}`)
	second := sessionCall(t, manager, "block-programming", "serial.write", `{"write_base64":"aGk="}`)
	if dataField(first, "process") == nil || dataField(first, "process") != dataField(second, "process") || started.Load() != 1 {
		t.Fatalf("session requests ran in %d engines: %#v %#v", started.Load(), first, second)
	}
	if bad := sessionCall(t, manager, "block-programming", "serial.read", `{"baud_rate":115200}`); bad.Error == nil ||
		bad.Error.Code != CodeInvalidRequest {
		t.Fatalf("read with a field the page never sends: %#v", bad)
	}
	if runtime.GOOS == "windows" {
		upload := `{"blocks":[{"type":"base_setup"}],"port":"COM5","vid":"1A86","pid":"7523"}`
		sessionCall(t, manager, "block-programming", "program.upload", upload)
		if read := sessionCall(t, manager, "block-programming", "serial.read", `{}`); dataField(read, "open") != false {
			t.Fatalf("the session outlived an upload: %#v", read)
		}
		sessionCall(t, manager, "block-programming", "serial.open", open)
	}
	manager.Cancel()
	if read := sessionCall(t, manager, "block-programming", "serial.read", `{}`); dataField(read, "open") != false {
		t.Fatalf("the session outlived leaving the mode: %#v", read)
	}
	before := started.Load()
	missing := `{"port":"COM9","vid":"1A86","pid":"7523"}`
	if failed := sessionCall(t, manager, "block-programming", "serial.open", missing); failed.Error == nil {
		t.Fatalf("open of a missing port: %#v", failed)
	}
	if read := sessionCall(t, manager, "block-programming", "serial.read", `{}`); dataField(read, "open") != false ||
		started.Load() != before+1 {
		t.Fatalf("a failed open left its engine running: %#v", read)
	}
}

// A compile holds the board for its upload, so no Serial monitor opens meanwhile; a session
// nobody reads any more ends by itself.
func TestBlockLabSerialWaitsForBuildsAndEndsWhenIdle(t *testing.T) {
	manager, started := sessionManager(t)
	open := `{"port":"COM5","vid":"1A86","pid":"7523"}`
	compiled := make(chan struct{})
	go func() {
		defer close(compiled)
		sessionCall(t, manager, "block-programming", "program.compile", `{"blocks":[{"type":"base_setup"}]}`)
	}()
	deadline := time.Now().Add(2 * time.Second)
	for started.Load() == 0 && time.Now().Before(deadline) {
		time.Sleep(10 * time.Millisecond)
	}
	if busy := sessionCall(t, manager, "block-programming", "serial.open", open); busy.Error == nil ||
		busy.Error.Code != CodeEngineBusy {
		t.Fatalf("open during a compile: %#v", busy)
	}
	<-compiled
	previous := sessionIdleTimeout
	sessionIdleTimeout = 100 * time.Millisecond
	t.Cleanup(func() { sessionIdleTimeout = previous })
	sessionCall(t, manager, "block-programming", "serial.open", open)
	time.Sleep(500 * time.Millisecond)
	if read := sessionCall(t, manager, "block-programming", "serial.read", `{}`); dataField(read, "open") != false {
		t.Fatalf("an idle session kept running: %#v", read)
	}
}

func TestBlockLabSerialSkipsThePortRobotControlDrove(t *testing.T) {
	manager, started := sessionManager(t)
	sessionCall(t, manager, "robot-control", "servo.set", `{"transport":"serial","port":"COM5","baud":115200}`)
	opened := sessionCall(t, manager, "block-programming", "serial.open", `{"port":"com5","vid":"1A86","pid":"7523"}`)
	if opened.Error == nil || opened.Error.Code != CodeSafetyLimit || started.Load() != 1 {
		t.Fatalf("open on the arm's port: %#v (%d engines)", opened, started.Load())
	}
}
