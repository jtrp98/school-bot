package engine

import (
	"encoding/json"
	"testing"
	"time"
)

func TestCancelDoesNotInterruptAnActiveRobotStop(t *testing.T) {
	manager, started := helperManager(t)
	finished := make(chan Response, 1)
	go func() {
		response, _ := manager.Execute("robot-control", "robot.stop", json.RawMessage(`{"helper":"stop-wait"}`))
		finished <- response
	}()
	select {
	case <-started:
	case <-time.After(3 * time.Second):
		t.Fatal("stop never started")
	}
	manager.Cancel()
	select {
	case response := <-finished:
		if response.Status != "success" {
			t.Fatalf("navigation cancelled the stop: %#v", response.Error)
		}
	case <-time.After(3 * time.Second):
		t.Fatal("Cancel returned before the stop drained")
	}
}

func TestProtectedStopStillHasADeadline(t *testing.T) {
	manager, started := helperManager(t)
	manager.timeout = 300 * time.Millisecond
	finished := make(chan Response, 1)
	go func() {
		response, _ := manager.Execute("robot-control", "robot.stop", json.RawMessage(`{"helper":"wait"}`))
		finished <- response
	}()
	select {
	case <-started:
	case <-time.After(3 * time.Second):
		t.Fatal("stop never started")
	}
	manager.Cancel()
	if code := errorCode(t, <-finished, nil); code != CodeEngineTimeout {
		t.Fatalf("protected stop lost its deadline: %s", code)
	}
}
