package engine

import (
	"bytes"
	"encoding/json"
	"fmt"
	"path/filepath"
	"runtime"
	"strings"
	"time"
)

type engineSpec struct{ id, relativePath string }

var (
	zideEngine  = engineSpec{"zide", filepath.Join("engines", "zide", "zide-engine")}
	demoEngine  = engineSpec{"demo", "demo-engine"}
	mixlyEngine = engineSpec{"mixly", filepath.Join("engines", "mixly", "mixly-engine")}
)

// Block Lab targets the Arduino UNO only, so the host fixes the board instead of trusting the page.
var (
	unoProgram = map[string]string{"board": "arduino_avr", "platform": "Arduino/Genuino Uno"}
	unoBuild   = map[string]string{"board": "arduino_avr", "platform": "Arduino/Genuino Uno", "transport": "native"}
)

// Compiling and uploading may take up to two minutes; the engine's own tool deadlines
// (compile 110 s alone, 75 s + upload 30 s) end first so the page gets the tool's reason, not this one.
const buildTimeout = 2 * time.Minute

type route struct {
	engine engineSpec
	// Request data fields the host sets itself, overriding whatever the page sent.
	fixed map[string]string
	// When set, the only fields the page may send.
	accepted map[string]bool
	// When set, the only block types data.blocks may hold.
	blocks map[string]bool
	// When set, replaces the manager's default request deadline.
	timeout time.Duration
	// Sent to the mode's long-lived serial session (session.go) instead of a process per request.
	session bool
}

func routes(engine engineSpec, fixed map[string]string, actions ...string) map[string]route {
	set := make(map[string]route, len(actions))
	for _, action := range actions {
		set[action] = route{engine: engine, fixed: fixed}
	}
	return set
}

// The webview is untrusted, so the host admits only the actions each mode's UI
// really sends; an engine action missing here is unreachable even if it exists.
// Routing is per action because a mode may use more than one engine.
var modeRoutes = map[string]map[string]route{
	"robot-control": routes(zideEngine, nil,
		"device.list", "board.info", "servo.set", "servo.read", "servo.torque", "robot.stop",
		"group.execute", "group.download", "group.run",
	),
	"coding-adventure": routes(demoEngine, nil, "program.run"),
	// Block Lab gets code generation, the port list (which opens no port), compile/upload
	// limited to its own blocks, and the Serial monitor of the board the child picked.
	// serial.exchange stays unrouted: it would write raw bytes to any port, the robot arm's
	// included.
	"block-programming": withRoutes(
		routes(mixlyEngine, unoProgram, "project.validate", "program.generate"),
		// device.list takes no board, and the engine rejects fields it does not know.
		routes(mixlyEngine, nil, "device.list"),
		blockLabBuildRoutes(runtime.GOOS),
		blockLabSerialRoutes,
	),
}

// The engine opens a port only with transport native, which the host sets, and only a listed
// UNO or clone bridge with the picked board's USB IDs, at the 9600 baud Block Lab programs use
// (the page cannot choose another). The host refuses a port Robot Control drove (armports.go).
var (
	nativeSerial         = map[string]string{"transport": "native"}
	blockLabSerialRoutes = map[string]route{
		"serial.open": {
			engine: mixlyEngine, fixed: nativeSerial, accepted: setOf("port", "vid", "pid"), session: true,
		},
		"serial.read":  {engine: mixlyEngine, accepted: setOf(), session: true},
		"serial.write": {engine: mixlyEngine, accepted: setOf("write_base64"), session: true},
		"serial.close": {engine: mixlyEngine, accepted: setOf(), session: true},
	}
)

// Compile and upload run arduino-cli and avrdude on this machine. The page sends Block Lab
// blocks, never C++ source, and the engine refuses field values those blocks do not offer where
// their generators paste the field into the C++. For upload the page also sends the port with
// the USB IDs of the board the child picked; the engine re-lists the ports and uploads only to
// that UNO or UNO-clone bridge.
func blockLabBuildRoutes(goos string) map[string]route {
	set := map[string]route{
		"program.compile": {
			engine:   mixlyEngine,
			fixed:    unoBuild,
			accepted: setOf("blocks"),
			blocks:   blockLabBlocks,
			timeout:  buildTimeout,
		},
	}
	// Elsewhere the engine kills only its direct child on cancel, so avrdude could keep writing
	// the board after the page was told the upload stopped.
	if goos == "windows" {
		set["program.upload"] = route{
			engine:   mixlyEngine,
			fixed:    unoBuild,
			accepted: setOf("blocks", "port", "vid", "pid"),
			blocks:   blockLabBlocks,
			timeout:  buildTimeout,
		}
	}
	return set
}

func withRoutes(sets ...map[string]route) map[string]route {
	all := map[string]route{}
	for _, set := range sets {
		for action, r := range set {
			all[action] = r
		}
	}
	return all
}

func ValidMode(mode string) bool {
	_, ok := modeRoutes[mode]
	return ok
}

func routeFor(mode, action string) (route, error) {
	actions, ok := modeRoutes[mode]
	if !ok {
		return route{}, fmt.Errorf("unsupported engine mode %q", mode)
	}
	r, ok := actions[action]
	if !ok {
		return route{}, fmt.Errorf("action %q is not allowed for mode %q", action, mode)
	}
	return r, nil
}

// withFixedFields returns data with the route's fixed fields set; ok is false when data is not
// an object or spells a fixed field in another case, which Go's case-insensitive decoding in
// the engine could otherwise let compete with the fixed value.
func withFixedFields(data json.RawMessage, fixed map[string]string) (json.RawMessage, bool) {
	if len(fixed) == 0 {
		return data, true
	}
	fields := map[string]json.RawMessage{}
	if trimmed := bytes.TrimSpace(data); len(trimmed) != 0 && !bytes.Equal(trimmed, []byte("null")) {
		if err := json.Unmarshal(trimmed, &fields); err != nil || fields == nil {
			return nil, false
		}
	}
	for name := range fields {
		for fixedName := range fixed {
			if name != fixedName && strings.EqualFold(name, fixedName) {
				return nil, false
			}
		}
	}
	for name, value := range fixed {
		encoded, _ := json.Marshal(value)
		fields[name] = encoded
	}
	out, err := json.Marshal(fields)
	return out, err == nil
}
