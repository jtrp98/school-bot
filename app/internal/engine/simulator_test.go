package engine

import (
	"context"
	"encoding/json"
	"os/exec"
	"reflect"
	"testing"
	"time"
)

func simulatorCall(t *testing.T, simulator *hardwareSimulator, action string, data map[string]interface{}) Response {
	t.Helper()
	if action != "device.list" {
		data["transport"] = "serial"
		data["port"] = simulatorTarget
	}
	raw, err := json.Marshal(data)
	if err != nil {
		t.Fatal(err)
	}
	response := simulator.execute(context.Background(), action, raw)
	if problem := contractProblem(response, "simulator"); problem != "" {
		t.Fatal(problem)
	}
	return response
}

func simulatorPose(index, pwm int) simulatorGroup {
	group := simulatorGroup{Index: index}
	for id := 0; id < 6; id++ {
		group.Items = append(group.Items, simulatorServo{ID: id, PWM: pwm, TimeMS: 1000})
	}
	return group
}

func simulatorSuccess(t *testing.T, response Response) map[string]interface{} {
	t.Helper()
	if response.Status != "success" {
		t.Fatalf("expected success: %+v", response)
	}
	data := response.Data.(map[string]interface{})
	if data["simulated"] != true || data["delivery"] == "written" {
		t.Fatal("simulator claimed production delivery")
	}
	return data
}

func TestSimulatorOptInUsesExistingRouteWithoutNativeProcess(t *testing.T) {
	t.Setenv("SCHOOL_BOT_HARDWARE_SIMULATOR", "")
	if NewManager().simulator != nil {
		t.Fatal("simulator must be opt-in")
	}
	t.Setenv("SCHOOL_BOT_HARDWARE_SIMULATOR", "true")
	if NewManager().simulator != nil {
		t.Fatal("only explicit 1 opts in")
	}
	t.Setenv("SCHOOL_BOT_HARDWARE_SIMULATOR", "1")
	t.Setenv("SCHOOL_BOT_SIMULATOR_SCENARIO", "normal")
	manager := NewManager()
	manager.resolve = func(engineSpec) (string, error) { t.Fatal("simulator resolved executable"); return "", nil }
	manager.command = func(context.Context, string) *exec.Cmd { t.Fatal("simulator opened process"); return nil }
	response, err := manager.Execute("robot-control", "device.list", json.RawMessage(`{}`))
	if err != nil {
		t.Fatal(err)
	}
	data := simulatorSuccess(t, response)
	devices := data["devices"].([]map[string]string)
	if len(devices) != 2 || devices[0]["path"] != simulatorTarget {
		t.Fatal(data)
	}
	for _, transport := range []string{"hid", "serial"} {
		key := "device"
		if transport == "serial" {
			key = "port"
		}
		raw, _ := json.Marshal(map[string]string{"transport": transport, key: simulatorTarget})
		response, err = manager.Execute("robot-control", "board.info", raw)
		if err != nil {
			t.Fatal(err)
		}
		simulatorSuccess(t, response)
	}
	response, err = manager.Execute("robot-control", "board.info", json.RawMessage(`{"transport":"serial","port":"COM1"}`))
	if err != nil || response.Error == nil || response.Error.Code != "INVALID_INPUT" {
		t.Fatal("simulator must never fall through to real COM port", response, err)
	}
}

func TestSimulatorUploadReadFiniteRunStopTorque(t *testing.T) {
	simulator := newHardwareSimulator("normal")
	clock := time.Unix(100, 0)
	simulator.now = func() time.Time { return clock }
	for _, groups := range [][]simulatorGroup{
		{simulatorPose(0, 1600), simulatorPose(1, 1700), simulatorPose(2, 1800), simulatorPose(3, 1900)},
		{simulatorPose(4, 2000)},
	} {
		simulatorSuccess(t, simulatorCall(t, simulator, "group.download", map[string]interface{}{"groups": groups}))
	}
	if len(simulator.groups) != 5 {
		t.Fatal("4+1 upload did not persist")
	}
	simulatorSuccess(t, simulatorCall(t, simulator, "group.run", map[string]interface{}{"start": 0, "end": 4, "times": 2}))
	if simulator.runUntil.Sub(clock) != 10*time.Second {
		t.Fatal("finite run schedule incorrect")
	}
	clock = clock.Add(10 * time.Second)
	for id := 0; id < 6; id++ {
		data := simulatorSuccess(t, simulatorCall(t, simulator, "servo.read", map[string]interface{}{"id": id}))
		if data["exchange"].(map[string]interface{})["received"] == "" || simulator.positions[id] != 2000 {
			t.Fatal("read did not use complete simulated pose")
		}
	}
	simulatorSuccess(t, simulatorCall(t, simulator, "servo.torque", map[string]interface{}{"enabled": false}))
	before := simulator.positions
	response := simulatorCall(t, simulator, "group.execute", map[string]interface{}{"group": simulatorPose(0, 1500)})
	if response.Status != "error" || simulator.positions != before {
		t.Fatal("unlocked torque allowed movement")
	}
	stop := simulatorSuccess(t, simulatorCall(t, simulator, "robot.stop", map[string]interface{}{}))
	if stop["stop_requested"] != true || stop["stopped"] != nil || simulator.torque[0] {
		t.Fatal("stop claimed physical stop or relocked without intent")
	}
	simulatorSuccess(t, simulatorCall(t, simulator, "servo.torque", map[string]interface{}{"enabled": true}))
	simulatorSuccess(t, simulatorCall(t, simulator, "group.execute", map[string]interface{}{"group": simulatorPose(0, 1500)}))
}

func TestSimulatorRejectedInputCommitsNothing(t *testing.T) {
	simulator := newHardwareSimulator("normal")
	valid := simulatorPose(0, 1700)
	duplicate := simulatorPose(1, 1800)
	duplicate.Items[5].ID = 0
	missing := simulatorPose(1, 1800)
	missing.Items = missing.Items[:5]
	bad := simulatorPose(1, 1800)
	bad.Items[5].PWM = 2501
	cases := []struct {
		action string
		data   map[string]interface{}
	}{
		{"group.download", map[string]interface{}{"groups": []simulatorGroup{valid, duplicate}}},
		{"group.download", map[string]interface{}{"groups": []simulatorGroup{valid, missing}}},
		{"group.download", map[string]interface{}{"groups": []simulatorGroup{valid, bad}}},
		{"group.download", map[string]interface{}{"groups": []simulatorGroup{valid, valid}}},
		{"group.download", map[string]interface{}{"groups": []simulatorGroup{valid, valid, valid, valid, valid}}},
		{"group.run", map[string]interface{}{"start": 0, "end": 1, "times": 1}},
		{"group.run", map[string]interface{}{"start": 0, "end": 0, "times": 0}},
		{"servo.set", map[string]interface{}{"id": 0, "pwm": 2500, "bias": 1}},
		{"servo.set", map[string]interface{}{"id": 0, "pwm": 1500, "time_ms": 10000}},
		{"servo.set", map[string]interface{}{"id": 0.5, "pwm": 1500}},
		{"servo.set", map[string]interface{}{"id": 0, "pwm": 1500, "enabled": true}},
		{"servo.torque", map[string]interface{}{"enabled": nil}},
	}
	for _, test := range cases {
		positions, torque := simulator.positions, simulator.torque
		response := simulatorCall(t, simulator, test.action, test.data)
		if response.Status != "error" || len(simulator.groups) != 0 || simulator.positions != positions || simulator.torque != torque {
			t.Fatalf("rejection mutated state: %s %+v", test.action, response)
		}
	}
}

func TestSimulatorFailureDisconnectTimeoutAndRecovery(t *testing.T) {
	for scenario, code := range map[string]string{"disconnect": "TRANSPORT_ERROR", "timeout": "DEVICE_TIMEOUT", "stop-error": "TRANSPORT_ERROR"} {
		t.Run(scenario, func(t *testing.T) {
			simulator := newHardwareSimulator("normal")
			simulatorSuccess(t, simulatorCall(t, simulator, "group.download", map[string]interface{}{"group": simulatorPose(0, 1600)}))
			simulator.scenario = scenario
			response := simulatorCall(t, simulator, "robot.stop", map[string]interface{}{})
			if response.Error == nil || response.Error.Code != code || response.Data != nil || len(simulator.groups) != 0 {
				t.Fatal(response)
			}
			if scenario == "disconnect" {
				data := simulatorSuccess(t, simulatorCall(t, simulator, "device.list", map[string]interface{}{}))
				if len(data["devices"].([]map[string]string)) != 0 {
					t.Fatal("disconnected target still discovered")
				}
			}
			simulator.scenario = "normal"
			simulatorSuccess(t, simulatorCall(t, simulator, "board.info", map[string]interface{}{}))
			response = simulatorCall(t, simulator, "group.run", map[string]interface{}{"start": 0, "end": 0, "times": 1})
			if response.Status != "error" || !reflect.DeepEqual(simulator.positions, [6]int{1500, 1500, 1500, 1500, 1500, 1500}) {
				t.Fatal("reconnect replayed upload or movement")
			}
		})
	}
}

func TestSimulatorCancelledRequestChangesNothing(t *testing.T) {
	simulator := newHardwareSimulator("normal")
	ctx, cancel := context.WithCancel(context.Background())
	cancel()
	response := simulator.execute(ctx, "servo.set", json.RawMessage(`{"transport":"serial","port":"simulator:school-bot","id":0,"pwm":1800}`))
	if response.Error == nil || response.Error.Code != CodeEngineCancelled || simulator.positions[0] != 1500 {
		t.Fatal(response)
	}
}

func TestSimulatorMaximumUploadAndRunStayBounded(t *testing.T) {
	simulator := newHardwareSimulator("normal")
	clock := time.Unix(100, 0)
	simulator.now = func() time.Time { return clock }
	for index := 0; index < 10000; index += 4 {
		groups := make([]simulatorGroup, 0, 4)
		for offset := 0; offset < 4; offset++ {
			groups = append(groups, simulatorPose(index+offset, 1800))
		}
		simulatorSuccess(t, simulatorCall(t, simulator, "group.download", map[string]interface{}{"groups": groups}))
	}
	simulatorSuccess(t, simulatorCall(t, simulator, "group.run", map[string]interface{}{"start": 0, "end": 9999, "times": 9999}))
	if len(simulator.groups) != 10000 || simulator.runUntil.Sub(clock) != 99990000*time.Second {
		t.Fatal("maximum finite run schedule incorrect")
	}
	response := simulatorCall(t, simulator, "group.download", map[string]interface{}{"group": simulatorPose(10000, 1500)})
	if response.Status != "error" || len(simulator.groups) != 10000 {
		t.Fatal("10001st pose admitted")
	}
	simulatorSuccess(t, simulatorCall(t, simulator, "robot.stop", map[string]interface{}{}))
	if !simulator.runUntil.IsZero() || simulator.positions[0] != 1500 {
		t.Fatal("stop did not discard pending simulated playback")
	}
}

func TestSimulatorBusyAndCancelUseHostAdmission(t *testing.T) {
	t.Setenv("SCHOOL_BOT_HARDWARE_SIMULATOR", "1")
	manager := NewManager()
	manager.simulator.mu.Lock()
	finished := make(chan Response, 1)
	go func() {
		response, _ := manager.Execute("robot-control", "servo.set", json.RawMessage(`{"transport":"serial","port":"simulator:school-bot","id":0,"pwm":1800}`))
		finished <- response
	}()
	deadline := time.After(3 * time.Second)
	for len(manager.slots) == 0 {
		select {
		case <-deadline:
			manager.simulator.mu.Unlock()
			t.Fatal("request did not acquire slot")
		default:
			time.Sleep(time.Millisecond)
		}
	}
	response, err := manager.Execute("robot-control", "servo.read", json.RawMessage(`{}`))
	if err != nil || response.Error == nil || response.Error.Code != CodeEngineBusy {
		manager.simulator.mu.Unlock()
		t.Fatal("overlapping request admitted", response, err)
	}
	manager.mu.Lock()
	manager.active()
	manager.mu.Unlock()
	manager.simulator.mu.Unlock()
	if response := <-finished; response.Error == nil || response.Error.Code != CodeEngineCancelled {
		t.Fatal(response)
	}
	manager.Cancel()
	if manager.simulator.positions[0] != 1500 || len(manager.slots) != 0 {
		t.Fatal("cancelled request mutated state or leaked host slot")
	}
}
