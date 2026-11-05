package engine

import (
	"bytes"
	"context"
	"encoding/hex"
	"encoding/json"
	"errors"
	"fmt"
	"sync"
	"time"
)

const simulatorTarget = "simulator:school-bot"

// A simulator is a separate, opt-in host session. It never delegates to a native transport.
type hardwareSimulator struct {
	mu        sync.Mutex
	now       func() time.Time
	scenario  string
	positions [6]int
	torque    [6]bool
	groups    map[int]simulatorGroup
	runUntil  time.Time
	lastGroup int
}

func newHardwareSimulator(scenario string) *hardwareSimulator {
	simulator := &hardwareSimulator{now: time.Now, scenario: scenario, groups: map[int]simulatorGroup{}}
	for index := range simulator.positions {
		simulator.positions[index] = 1500
		simulator.torque[index] = true
	}
	return simulator
}

func (s *hardwareSimulator) execute(ctx context.Context, action string, raw json.RawMessage) Response {
	s.mu.Lock()
	defer s.mu.Unlock()
	fail := func(code, message string) Response { return hostFailure("simulator", code, message) }
	fields, err := simulatorObject(raw)
	if err != nil {
		return fail("INVALID_INPUT", "invalid or oversized simulator request")
	}
	payload, encodeErr := json.Marshal(Request{Protocol: Protocol, RequestID: "simulator", Mode: "robot-control", Action: action, Data: raw})
	if encodeErr != nil || len(payload) > MaxRequestBytes {
		return fail(CodeInvalidRequest, "request exceeds 64 KB")
	}
	result := map[string]interface{}{
		"engine": "zide", "language": "Go", "simulated": true, "transport": "none",
	}
	success := func() Response {
		return Response{Protocol: Protocol, RequestID: "simulator", Status: "success", Data: result}
	}
	if action == "device.list" {
		if len(fields) != 0 {
			return fail("INVALID_INPUT", "device.list requires empty data")
		}
		devices := []map[string]string{}
		if s.scenario != "disconnect" {
			for _, transport := range []string{"hid", "serial"} {
				devices = append(devices, map[string]string{
					"transport": transport, "path": simulatorTarget, "name": "Hardware Simulator / Virtual Port",
				})
			}
		}
		result["devices"] = devices
		return success()
	}
	if err := validateSimulatorTarget(fields); err != nil {
		return fail("INVALID_INPUT", err.Error())
	}
	transport := ""
	json.Unmarshal(fields["transport"], &transport)
	result["transport"] = transport
	result["delivery"] = "dry-run"
	result["acknowledged"] = false
	var commands []string
	var commit func()
	switch action {
	case "board.info", "robot.stop":
		err = simulatorKeys(fields)
		if action == "board.info" {
			commands = []string{"$DRS!"}
		} else {
			commands = []string{"$DST!"}
			result["stop_requested"] = true
			commit = func() { s.runUntil = time.Time{} }
		}
	case "servo.read", "servo.set", "servo.torque":
		commands, commit, err = s.planServo(action, fields, result)
	case "group.execute", "group.download":
		err = simulatorKeys(fields, "group", "groups")
		if err == nil {
			var groups []simulatorGroup
			groups, err = simulatorGroups(fields, action == "group.download")
			if err == nil {
				for _, group := range groups {
					commands = append(commands, simulatorGroupCommand(group, action == "group.download"))
				}
				commit = func() {
					for _, group := range groups {
						if action == "group.download" {
							s.groups[group.Index] = group
						} else {
							s.applyGroup(group)
						}
					}
				}
			}
		}
	case "group.run":
		commands, commit, err = s.planRun(fields, result)
	default:
		return fail("ACTION_UNKNOWN", "unsupported simulator action")
	}
	if err != nil {
		var safety simulatorSafetyError
		if errors.As(err, &safety) {
			return fail("SAFETY_LIMIT", safety.Error())
		}
		return fail("INVALID_INPUT", err.Error())
	}
	if action == "servo.set" || action == "group.execute" || action == "group.download" || action == "group.run" {
		for _, enabled := range s.torque {
			if !enabled {
				return fail("INVALID_INPUT", "lock simulator torque before moving, uploading or running")
			}
		}
	}
	if ctx.Err() != nil {
		return fail(CodeEngineCancelled, "simulator request cancelled")
	}
	if s.scenario == "disconnect" || (s.scenario == "stop-error" && action == "robot.stop") {
		s.groups = map[int]simulatorGroup{}
		s.runUntil = time.Time{}
		return fail("TRANSPORT_ERROR", "simulator disconnected; delivery unknown; do not retry movement")
	}
	if s.scenario == "timeout" {
		s.groups = map[int]simulatorGroup{}
		return fail("DEVICE_TIMEOUT", "simulator scripted timeout; delivery unknown")
	}
	if s.scenario != "" && s.scenario != "normal" && s.scenario != "stop-error" {
		return fail("INVALID_INPUT", "unknown simulator scenario")
	}
	// A rejected input cannot advance playback or commit even part of a pose/upload batch.
	if !s.runUntil.IsZero() && !s.now().Before(s.runUntil) {
		s.applyGroup(s.groups[s.lastGroup])
		s.runUntil = time.Time{}
	}
	if commit != nil {
		commit()
	}
	received := ""
	if action == "board.info" {
		received = "SIMULATOR ZIDE"
	}
	if action == "servo.read" {
		id, _ := simulatorInteger(fields, "id", 0, 5, nil)
		received = fmt.Sprintf("#%03dP%04d!", id, s.positions[id])
	}
	written := 0
	for _, command := range commands {
		written += len(command)
	}
	result["commands"] = commands
	result["exchange"] = map[string]interface{}{
		"received": received, "received_hex": hex.EncodeToString([]byte(received)),
		"bytes_written": written, "bytes_read": len(received),
	}
	return success()
}

func validateSimulatorTarget(fields map[string]json.RawMessage) error {
	var transport, target string
	if json.Unmarshal(fields["transport"], &transport) != nil || (transport != "hid" && transport != "serial") {
		return errors.New("simulator requires explicit hid or serial transport")
	}
	key, forbidden := "device", "port"
	if transport == "serial" {
		key, forbidden = "port", "device"
	}
	if _, exists := fields[forbidden]; exists {
		return errors.New("target field does not match transport")
	}
	if json.Unmarshal(fields[key], &target) != nil || target != simulatorTarget {
		return errors.New("simulator target must come from device.list")
	}
	for key, bounds := range map[string][3]int{"baud": {9600, 921600, 115200}, "read_ms": {1, 1500, 1000}} {
		value, err := simulatorInteger(fields, key, bounds[0], bounds[1], &bounds[2])
		if err != nil {
			return err
		}
		if key == "baud" {
			allowed := false
			for _, baud := range []int{9600, 19200, 38400, 57600, 115200, 128000, 230400, 256000, 460800, 512000, 921600} {
				allowed = allowed || baud == value
			}
			if !allowed {
				return errors.New("unsupported baud")
			}
		}
	}
	return nil
}

func simulatorBool(raw json.RawMessage) (bool, error) {
	var value bool
	if bytes.Equal(raw, []byte("null")) || json.Unmarshal(raw, &value) != nil {
		return false, errors.New("enabled must be boolean")
	}
	return value, nil
}
