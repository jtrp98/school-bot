package engine

import (
	"encoding/json"
	"runtime"
	"testing"
	"time"
)

// Leaving a mode or closing the window lets an upload finish, so a page that lost its own guard
// cannot leave the board half written; only Block Lab's confirmed stop ends it.
func TestCancelLetsAnUploadFinishUnlessStopped(t *testing.T) {
	if runtime.GOOS != "windows" {
		t.Skip("program.upload is routed on Windows only")
	}
	manager, started := helperManager(t)
	upload := func(port string) <-chan Response {
		finished := make(chan Response, 1)
		go func() {
			data := `{"blocks":[{"type":"base_setup"}],"port":"` + port + `","vid":"1A86","pid":"7523"}`
			response, _ := manager.Execute("block-programming", "program.upload", json.RawMessage(data))
			finished <- response
		}()
		select {
		case <-started:
		case <-time.After(3 * time.Second):
			t.Fatal("upload never started")
		}
		return finished
	}
	finished := upload("COM8")
	manager.Cancel()
	manager.mu.Lock()
	running := manager.done != nil
	manager.mu.Unlock()
	if running {
		t.Fatal("Cancel returned before the upload finished")
	}
	if response := <-finished; response.Status != "success" {
		t.Fatalf("Cancel cut the upload off: %#v", response)
	}
	finished = upload("COM9")
	// The command is created before it starts; the stop has to end a process that runs.
	time.Sleep(500 * time.Millisecond)
	if !manager.UploadRunning() {
		t.Fatal("UploadRunning is false during an upload")
	}
	start := time.Now()
	manager.StopUpload()
	if code := errorCode(t, <-finished, nil); code != CodeEngineCancelled || time.Since(start) > 10*time.Second {
		t.Fatalf("StopUpload: %s after %v", code, time.Since(start))
	}
	if manager.UploadRunning() {
		t.Fatal("UploadRunning is still true after the upload ended")
	}
}

// A stop that comes after its upload ended must not end the next request instead.
func TestStopUploadLeavesOtherRequestsRunning(t *testing.T) {
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
	manager.StopUpload()
	select {
	case response := <-finished:
		t.Fatalf("StopUpload ended a request that is not an upload: %#v", response)
	case <-time.After(300 * time.Millisecond):
	}
	manager.Cancel()
	if code := errorCode(t, <-finished, nil); code != CodeEngineCancelled {
		t.Fatalf("Cancel after StopUpload: %s", code)
	}
}
