package engine

import (
	"encoding/json"
	"runtime"
	"testing"
)

// A clone UNO and the arm's controller can look the same, so a port Robot Control drove takes no
// Block Lab upload until a port list shows its cable came out.
func TestBlockLabUploadSkipsThePortRobotControlDrove(t *testing.T) {
	if runtime.GOOS != "windows" {
		t.Skip("program.upload is routed on Windows only")
	}
	manager, started := helperManager(t)
	upload := func() string {
		data := `{"blocks":[{"type":"base_setup"}],"port":"COM5","vid":"1A86","pid":"7523"}`
		response, err := manager.Execute("block-programming", "program.upload", json.RawMessage(data))
		if err != nil {
			t.Fatal(err)
		}
		if response.Status == "success" {
			<-started
			return "success"
		}
		return response.Error.Code
	}
	run := func(mode, action, data string) {
		if response, err := manager.Execute(mode, action, json.RawMessage(data)); err != nil || response.Status != "success" {
			t.Fatalf("%s/%s: %#v %v", mode, action, response, err)
		}
		<-started
	}
	if got := upload(); got != "success" {
		t.Fatalf("upload before Robot Control: %s", got)
	}
	run("robot-control", "servo.set", `{"transport":"serial","port":"com5","baud":115200}`)
	if got := upload(); got != CodeSafetyLimit {
		t.Fatalf("upload to the arm's port: %s", got)
	}
	run("block-programming", "device.list", `{"helper":"listed-com5"}`)
	if got := upload(); got != CodeSafetyLimit {
		t.Fatalf("upload while the arm's port is still listed: %s", got)
	}
	run("block-programming", "device.list", `{"helper":"listed-com7"}`)
	if got := upload(); got != "success" {
		t.Fatalf("upload after the arm's port left the list: %s", got)
	}
}
