package main

import (
	"strings"
	"testing"
)

func TestMixProjectUsesConnectedBlockInsteadOfShadow(t *testing.T) {
	project := `<xml xmlns="https://developers.google.com/blockly/xml"><block type="inout_digital_write2">
  <value name="PIN"><shadow type="math_number"><field name="NUM">1</field></shadow>
    <block type="math_number"><field name="NUM">13</field></block></value>
  <value name="STAT"><shadow type="inout_highlow"><field name="BOOL">HIGH</field></shadow></value>
  <next><block type="inout_digital_write2">
    <value name="PIN"><block type="math_number"><field name="NUM">13</field></block></value>
    <value name="STAT"><block type="inout_highlow"><field name="BOOL">LOW</field></block></value>
  </block></next>
</block></xml>`
	blocks, err := parseProject(project)
	if err != nil {
		t.Fatal(err)
	}
	board, _ := findBoard("arduino_avr")
	code, err := generate(board, "Arduino/Genuino Uno", blocks)
	if err != nil || !strings.Contains(code, "digitalWrite(13,HIGH)") || !strings.Contains(code, "digitalWrite(13,LOW)") ||
		strings.Count(code, "pinMode(13, OUTPUT)") != 1 || strings.Contains(code, "digitalWrite(1,") {
		t.Fatalf("%s: %v", code, err)
	}
}

func TestXMLAndBlockLimits(t *testing.T) {
	for _, project := range []string{
		`<!DOCTYPE xml [<!ENTITY external SYSTEM "file:///secret">]><xml/>`,
		`<xml><block type="math_number"><value name="NUM"/></block></xml>`,
		strings.Repeat("<xml>", 97) + strings.Repeat("</xml>", 97),
		`<xml><block></xml>`,
		`<xml/><xml/>`,
	} {
		if _, err := parseProject(project); err == nil {
			t.Fatalf("accepted invalid XML: %.80s", project)
		}
	}
	blocks := []*Block{{Type: "math_number"}}
	current := blocks[0]
	for index := 0; index < 33; index++ {
		current.Next = &Block{Type: "math_number"}
		current = current.Next
	}
	if err := validateBlocks(blocks); err == nil {
		t.Fatal("accepted deep block tree")
	}
}

func TestXMLDeclarationAndProcedureArguments(t *testing.T) {
	project := `<?xml version="1.0" encoding="UTF-8"?><xml><variables><variable id="x">x</variable></variables>
<block type="procedures_defnoreturn"><mutation><arg name="x" type="int"/></mutation>
<field name="NAME">my_function</field><field name="TYPE">void</field></block></xml>`
	blocks, err := parseProject(project)
	if err != nil {
		t.Fatal(err)
	}
	board, _ := findBoard("arduino_avr")
	code, err := generate(board, "Arduino/Genuino Uno", blocks)
	if err != nil || !strings.Contains(code, "void my_function(int x)") {
		t.Fatalf("%s: %v", code, err)
	}
}

func TestGenerationStateIsNotSharedBetweenRequests(t *testing.T) {
	board, _ := findBoard("arduino_avr")
	first := []*Block{{Type: "inout_digital_write2", Values: map[string]*Block{
		"PIN":  {Type: "math_number", Fields: map[string]string{"NUM": "13"}},
		"STAT": {Type: "inout_highlow", Fields: map[string]string{"BOOL": "HIGH"}},
	}}}
	if _, err := generate(board, "Arduino/Genuino Uno", first); err != nil {
		t.Fatal(err)
	}
	code, err := generate(board, "Arduino/Genuino Uno", []*Block{{Type: "math_number", Fields: map[string]string{"NUM": "42"}}})
	if err != nil || strings.Contains(code, "pinMode") || strings.Contains(code, "digitalWrite") {
		t.Fatalf("%s: %v", code, err)
	}
}
