package main

import (
	"bytes"
	"encoding/json"
	"strings"
	"testing"
)

func requestLine(action, data string) []byte {
	return []byte(`{"protocol":"1.0","request_id":"mixly-test","mode":"block-programming","action":"` + action + `","data":` + data + `}`)
}

func TestInvalidProtocolAndPayloads(t *testing.T) {
	cases := []struct {
		name string
		line []byte
		code string
	}{
		{"malformed", []byte("{"), "PROTOCOL_INVALID"},
		{"utf8", []byte{0xff}, "PROTOCOL_INVALID"},
		{"protocol", []byte(`{"protocol":"2.0"}`), "PROTOCOL_INVALID"},
		{"trailing JSON", append(requestLine("engine.info", "{}"), []byte("{}")...), "PROTOCOL_INVALID"},
		{"null data", requestLine("engine.info", "null"), "INVALID_INPUT"},
		{"unknown action", requestLine("shell.exec", "{}"), "ACTION_UNKNOWN"},
		{"unknown board", requestLine("board.info", `{"board":"../../evil"}`), "INVALID_INPUT"},
		{"executable", requestLine("program.compile", `{"board":"arduino_avr","executable":"evil"}`), "INVALID_INPUT"},
		{"empty blocks", requestLine("program.generate", `{"board":"arduino_avr","blocks":[]}`), "PROJECT_INVALID"},
		{"null block", requestLine("program.generate", `{"board":"arduino_avr","blocks":[null]}`), "PROJECT_INVALID"},
		{"huge mutation", requestLine("program.generate", `{"board":"arduino_avr","blocks":[{"type":"factory_function_return","mutation":{"items":"999999999"}}]}`), "PROJECT_INVALID"},
		{"unselected native platform", requestLine("program.upload", `{"board":"arduino_avr","source":"x","port":"no-device","transport":"native"}`), "INVALID_INPUT"},
		{"inherited function", requestLine("program.generate", `{"board":"arduino_avr","blocks":[{"type":"constructor"}]}`), "PROGRAM_INVALID"},
		{"init function", requestLine("program.generate", `{"board":"arduino_avr","blocks":[{"type":"init"}]}`), "PROGRAM_INVALID"},
		{"source and XML", requestLine("program.compile", `{"board":"arduino_avr","source":"x","xml":"<xml/>"}`), "INVALID_INPUT"},
		{"bad options", requestLine("program.compile", `{"board":"arduino_avr","source":"x","options":{"flags":"--evil"}}`), "INVALID_INPUT"},
		{"bad serial data", requestLine("serial.exchange", `{"port":"COM-test","write_base64":"!"}`), "INVALID_INPUT"},
		{"read limit", requestLine("serial.exchange", `{"port":"COM-test","read_bytes":8193}`), "SAFETY_LIMIT"},
	}
	for _, check := range cases {
		t.Run(check.name, func(t *testing.T) {
			response := handleLine(check.line)
			if response.Status != "error" || response.Error == nil || response.Error.Code != check.code {
				t.Fatalf("%+v", response)
			}
		})
	}
}

func TestOversizedLineDrainsBeforeNextRequest(t *testing.T) {
	input := strings.Repeat("x", maxRequestBytes+100) + "\n" + string(requestLine("engine.info", "{}")) + "\n"
	var output bytes.Buffer
	if err := serve(strings.NewReader(input), &output); err != nil {
		t.Fatal(err)
	}
	decoder := json.NewDecoder(&output)
	var first, second Response
	if err := decoder.Decode(&first); err != nil {
		t.Fatal(err)
	}
	if err := decoder.Decode(&second); err != nil {
		t.Fatal(err)
	}
	if first.Error == nil || first.Error.Code != "INVALID_REQUEST" || second.Status != "success" ||
		second.RequestID != "mixly-test" {
		t.Fatalf("%+v, %+v", first, second)
	}
}

func TestDryRunNeverReportsHardwareSuccess(t *testing.T) {
	compile := handleLine(requestLine("program.upload", `{"board":"arduino_avr","platform":"Arduino/Genuino Uno","source":"void setup(){} void loop(){}","port":"does-not-exist"}`))
	var result BuildResult
	if compile.Status != "success" || json.Unmarshal(compile.Data, &result) != nil || result.Compiled ||
		result.Uploaded || result.Delivery != "dry-run" {
		t.Fatalf("%+v, %+v", compile, result)
	}
	response := handleLine(requestLine("serial.exchange", `{"port":"does-not-exist","write_base64":"YWJj","read_bytes":32}`))
	var serial SerialResult
	if response.Status != "success" || json.Unmarshal(response.Data, &serial) != nil || serial.Written != 0 ||
		serial.Received != "" || serial.DeviceAck || serial.Delivery != "dry-run" {
		t.Fatalf("%+v, %+v", response, serial)
	}
}

func TestArduinoTargetUsesValidatedOptions(t *testing.T) {
	board, _ := findBoard("arduino_avr")
	target, err := arduinoTarget(board, ProgramInput{
		Platform: "Arduino Nano", Options: map[string]string{"cpu": "atmega328old"},
	})
	if err != nil || target != "arduino:avr:nano:cpu=atmega328old" {
		t.Fatalf("%s: %v", target, err)
	}
}
