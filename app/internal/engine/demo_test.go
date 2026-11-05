package engine

import (
	"encoding/json"
	"testing"
)

func TestDemoBoundaries(t *testing.T) {
	cases := []struct{ name, mode, action, data, code string }{
		{"valid source", "coding-adventure", "program.run", `{"source":"hero.move_right(3)\nhero.jump()"}`, ""},
		{"reject arbitrary Python", "coding-adventure", "program.run", `{"source":"import os"}`, "PROGRAM_INVALID"},
		{"robot mode belongs to Zide", "robot-control", "servo.set", `{"servo":1,"angle":90}`, "ACTION_UNKNOWN"},
		{"robot stop belongs to Zide", "robot-control", "robot.stop", `{}`, "ACTION_UNKNOWN"},
		{"Block Lab belongs to Mixly", "block-programming", "program.run", `{"source":"hero.jump()"}`, "ACTION_UNKNOWN"},
		{"bounded movement", "coding-adventure", "program.run", `{"source":"hero.move_right(99)"}`, "STEP_LIMIT"},
	}
	for _, tc := range cases {
		t.Run(tc.name, func(t *testing.T) {
			res := HandleDemo(Request{Protocol: Protocol, RequestID: "test", Mode: tc.mode, Action: tc.action, Data: json.RawMessage(tc.data)})
			if tc.code == "" {
				if res.Status != "success" {
					t.Fatalf("unexpected error: %+v", res.Error)
				}
				return
			}
			if res.Error == nil || res.Error.Code != tc.code {
				t.Fatalf("expected %s; got %+v", tc.code, res)
			}
		})
	}
}
