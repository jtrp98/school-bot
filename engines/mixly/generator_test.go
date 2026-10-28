package main

import (
	"strings"
	"testing"
)

func TestArduinoGeneration(t *testing.T) {
	board, ok := findBoard("arduino_avr")
	if !ok {
		t.Fatal("missing AVR board")
	}
	code, err := generate(board, "Arduino/Genuino Uno", []*Block{{
		Type: "inout_digital_write2",
		Values: map[string]*Block{
			"PIN":  {Type: "math_number", Fields: map[string]string{"NUM": "13"}},
			"STAT": {Type: "inout_highlow", Fields: map[string]string{"BOOL": "HIGH"}},
		},
	}})
	if err != nil || !strings.Contains(code, "digitalWrite(13,HIGH)") {
		t.Fatalf("%s: %v", code, err)
	}
}

func TestEveryBoardGenerator(t *testing.T) {
	boards, err := catalog()
	if err != nil || len(boards) < 15 {
		t.Fatalf("incomplete catalog: %d, %v", len(boards), err)
	}
	for _, board := range boards {
		t.Run(board.ID, func(t *testing.T) {
			platform := ""
			for name := range board.Targets {
				if platform == "" || name < platform {
					platform = name
				}
			}
			code, err := generate(board, platform, []*Block{{
				Type: "math_number", Fields: map[string]string{"NUM": "42"},
			}})
			if err != nil || !strings.Contains(code, "42") {
				t.Fatalf("%s: %v", code, err)
			}
		})
	}
}
