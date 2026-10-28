package engine

import (
	"context"
	"encoding/json"
	"os"
	"os/exec"
	"path/filepath"
	"strings"
	"testing"
	"time"
)

func TestMalformedRequestDoesNotStartEngine(t *testing.T) {
	manager, started := helperManager(t)
	response, err := manager.Execute("robot-control", "board.info", json.RawMessage(`{`))
	if code := errorCode(t, response, err); code != CodeEngineFailed {
		t.Fatalf("unexpected code: %s", code)
	}
	select {
	case <-started:
		t.Fatal("malformed input started the engine")
	default:
	}
}

func TestEngineEnvironmentDoesNotInheritSecrets(t *testing.T) {
	t.Setenv("SCHOOL_BOT_ADMIN_KEY", "secret")
	t.Setenv("ARM_ADMIN_KEY", "legacy-secret")
	t.Setenv("UNRELATED_SECRET", "other-secret")
	for _, value := range engineEnvironment() {
		if strings.Contains(value, "secret") || strings.HasSuffix(value, "=") {
			t.Fatalf("unexpected environment entry: %s", strings.SplitN(value, "=", 2)[0])
		}
	}
}

func TestManagerRequestEnvelopeBoundary(t *testing.T) {
	manager, started := helperManager(t)
	data := json.RawMessage(`{"pad":""}`)
	request := Request{
		Protocol:  Protocol,
		RequestID: strings.Repeat("a", 32),
		Mode:      "robot-control",
		Action:    "board.info",
		Data:      data,
	}
	payload, err := json.Marshal(request)
	if err != nil {
		t.Fatal(err)
	}
	for _, extra := range []int{0, 1} {
		padding := strings.Repeat("a", MaxRequestBytes-len(payload)+extra)
		response, err := manager.Execute("robot-control", "board.info", json.RawMessage(`{"pad":"`+padding+`"}`))
		if extra == 0 {
			if err != nil || response.Status != "success" {
				t.Fatalf("64 KB envelope rejected: %#v %v", response.Error, err)
			}
			<-started
		} else if code := errorCode(t, response, err); code != CodeInvalidRequest {
			t.Fatalf("oversized envelope: %s", code)
		}
	}
	select {
	case <-started:
		t.Fatal("oversized envelope started the engine")
	default:
	}
}

func TestPipeHolderHelper(t *testing.T) {
	switch os.Getenv("SCHOOL_BOT_PIPE_HELPER") {
	case "child":
		if err := os.WriteFile(os.Getenv("SCHOOL_BOT_PIPE_MARKER"), []byte("ready"), 0600); err != nil {
			os.Exit(2)
		}
		time.Sleep(4 * time.Second)
		os.Exit(0)
	case "parent":
		var request Request
		if json.NewDecoder(os.Stdin).Decode(&request) != nil {
			os.Exit(2)
		}
		child := exec.Command(os.Args[0], "-test.run=^TestPipeHolderHelper$")
		child.Env = append(os.Environ(), "SCHOOL_BOT_PIPE_HELPER=child")
		child.Stdout = os.Stdout
		child.Stderr = os.Stderr
		if child.Start() != nil {
			os.Exit(3)
		}
		if os.Getenv("SCHOOL_BOT_PIPE_EXIT") == "1" {
			os.Exit(0)
		}
		time.Sleep(30 * time.Second)
		os.Exit(0)
	}
}

func TestWaitDelayBoundsInheritedPipeDrain(t *testing.T) {
	for _, operation := range []string{"cancel", "timeout", "exit"} {
		t.Run(operation, func(t *testing.T) {
			marker := filepath.Join(t.TempDir(), "ready")
			manager := NewManager()
			manager.resolve = func(engineSpec) (string, error) { return os.Args[0], nil }
			manager.command = func(ctx context.Context, path string) *exec.Cmd {
				cmd := exec.CommandContext(ctx, path, "-test.run=^TestPipeHolderHelper$")
				cmd.Env = []string{"SCHOOL_BOT_PIPE_HELPER=parent", "SCHOOL_BOT_PIPE_MARKER=" + marker}
				if operation == "exit" {
					cmd.Env = append(cmd.Env, "SCHOOL_BOT_PIPE_EXIT=1")
				}
				return cmd
			}
			if operation == "timeout" {
				manager.timeout = 1500 * time.Millisecond
			}
			finished := make(chan Response, 1)
			go func() {
				response, _ := manager.Execute("robot-control", "board.info", json.RawMessage(`{}`))
				finished <- response
			}()
			defer manager.Cancel()
			readyDeadline := time.Now().Add(3 * time.Second)
			for {
				if _, err := os.Stat(marker); err == nil {
					break
				}
				if time.Now().After(readyDeadline) {
					t.Fatal("pipe holder never started")
				}
				time.Sleep(10 * time.Millisecond)
			}
			cancelled := make(chan struct{})
			if operation == "cancel" {
				go func() {
					manager.Cancel()
					close(cancelled)
				}()
			}
			select {
			case response := <-finished:
				want := CodeEngineFailed
				if operation == "cancel" {
					want = CodeEngineCancelled
					select {
					case <-cancelled:
					case <-time.After(time.Second):
						t.Fatal("Cancel did not return after drain")
					}
				} else if operation == "timeout" {
					want = CodeEngineTimeout
				}
				if code := errorCode(t, response, nil); code != want {
					t.Fatalf("expected %s, got %s", want, code)
				}
			case <-time.After(3 * time.Second):
				t.Fatal("inherited pipes kept the engine busy beyond WaitDelay")
			}
		})
	}
}
