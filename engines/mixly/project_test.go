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
	blocks := []*Block{{Type: "logic_negate"}}
	current := blocks[0]
	for index := 0; index < 33; index++ {
		current.Values = map[string]*Block{"BOOL": {Type: "logic_negate"}}
		current = current.Values["BOOL"]
	}
	if err := validateBlocks(blocks, "arduino"); err == nil {
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

// Text reaches the C++ as typed, so nothing a block carries may start a new line of it, wherever
// the block sits: inside a value, a statement or further down a next chain.
func TestBlockTextCannotStartANewLineOfCode(t *testing.T) {
	leak := "x\n#include \"C:/Users/teacher/secret.txt\""
	printFields := map[string]string{"serial_select": "Serial", "new_line": "println"}
	waitFields := map[string]string{"UNIT": "delay"}
	for name, block := range map[string]*Block{
		"field":           {Type: "text", Fields: map[string]string{"TEXT": leak}},
		"carriage return": {Type: "text", Fields: map[string]string{"TEXT": "x\r#include <x>"}},
		"delete":          {Type: "text", Fields: map[string]string{"TEXT": "x\x7f"}},
		"comment":         {Type: "math_number", Fields: map[string]string{"NUM": "1"}, Comment: leak},
		"comment splice":  {Type: "math_number", Fields: map[string]string{"NUM": "1"}, Comment: `next line \  `},
		"mutation":        {Type: "procedures_callnoreturn", Mutation: map[string]string{"name": leak}},
		"argument":        {Type: "procedures_defnoreturn", Arguments: []Argument{{Name: "x", Type: "int\n#x"}}},
	} {
		for place, program := range map[string][]*Block{
			"top":       {block},
			"value":     {{Type: "serial_print", Fields: printFields, Values: map[string]*Block{"CONTENT": block}}},
			"statement": {{Type: "sb_loop", Statements: map[string]*Block{"DO": block}}},
			"next":      {{Type: "sb_loop", Next: &Block{Type: "controls_delay", Fields: waitFields, Next: block}}},
		} {
			if err := validateBlocks(program, "arduino"); err == nil {
				t.Errorf("%s in %s accepted", name, place)
			}
		}
	}
	if err := validateBlocks([]*Block{{Type: "text", Fields: map[string]string{"TEXT": `สวัสดี "a" \ b`},
		Comment: `a \ b`}}, "arduino"); err != nil {
		t.Fatalf("printable text refused: %v", err)
	}
}

// Each case would otherwise put its own C++ on the line the generator writes.
func TestArduinoPastedFieldsTakeOnlyTheValuesTheirBlocksOffer(t *testing.T) {
	for name, block := range map[string]*Block{
		"number":        {Type: "math_number", Fields: map[string]string{"NUM": "1); while (true"}},
		"no number":     {Type: "math_number"},
		"delay unit":    {Type: "controls_delay", Fields: map[string]string{"UNIT": "exit(0); delay"}},
		"rounding":      {Type: "math_to_int", Fields: map[string]string{"OP": "free"}},
		"variable type": {Type: "variables_declare", Fields: map[string]string{"VAR": "x", "TYPE": "int y; int"}},
		"variable name": {Type: "variables_get", Fields: map[string]string{"VAR": "a,b"}},
		"function name": {Type: "procedures_callnoreturn", Fields: map[string]string{"NAME": "f,g"}},
		"return type":   {Type: "procedures_defreturn", Fields: map[string]string{"NAME": "f", "TYPE": "long g(); long"}},
		"serial port":   {Type: "serial_available", Fields: map[string]string{"serial_select": "Serial1"}},
		"print":         {Type: "serial_print", Fields: map[string]string{"serial_select": "Serial", "new_line": "end"}},
		"argument type": {Type: "procedures_defnoreturn", Arguments: []Argument{{Name: "x", Type: "int y, int"}}},
		"argument name": {Type: "procedures_defnoreturn", Arguments: []Argument{{Name: "x,y"}}},
	} {
		if err := validateBlocks([]*Block{block}, "arduino"); err == nil {
			t.Errorf("%s accepted", name)
		}
	}
	for name, block := range map[string]*Block{
		"decimal":       {Type: "math_number", Fields: map[string]string{"NUM": "-1.5"}},
		"tiny":          {Type: "math_number", Fields: map[string]string{"NUM": "1e-7"}},
		"thai variable": {Type: "variables_declare", Fields: map[string]string{"VAR": "คะแนน", "TYPE": "String"}},
		"no return":     {Type: "procedures_defnoreturn", Fields: map[string]string{"NAME": "ทักทาย"}, Arguments: []Argument{{Name: "x"}}},
	} {
		if err := validateBlocks([]*Block{block}, "arduino"); err != nil {
			t.Errorf("%s refused: %v", name, err)
		}
	}
}

// A quote or backslash in a text block stays inside the C++ string.
func TestTextQuotesAndBackslashesStayInsideTheString(t *testing.T) {
	board, _ := findBoard("arduino_avr")
	code, err := generate(board, "Arduino/Genuino Uno", []*Block{{
		Type: "serial_print", Fields: map[string]string{"serial_select": "Serial", "new_line": "println"},
		Values: map[string]*Block{"CONTENT": {Type: "text", Fields: map[string]string{"TEXT": `a"); evil("\`}}},
	}})
	if err != nil || !strings.Contains(code, `Serial.println("a\"); evil(\"\\");`) {
		t.Fatalf("%s: %v", code, err)
	}
}

// A child's loop can hold a long row of blocks; only blocks inside blocks count as nesting.
func TestLongRowOfBlocksGenerates(t *testing.T) {
	wait := func(next *Block) *Block {
		return &Block{Type: "controls_delay", Fields: map[string]string{"UNIT": "delay"}, Next: next,
			Values: map[string]*Block{"DELAY_TIME": {Type: "math_number", Fields: map[string]string{"NUM": "1"}}}}
	}
	var row *Block
	for index := 0; index < 255; index++ {
		row = wait(row)
	}
	program := []*Block{{Type: "base_setup"}, {Type: "sb_loop", Statements: map[string]*Block{"DO": row}}}
	if err := validateBlocks(program, "arduino"); err != nil {
		t.Fatal(err)
	}
	board, _ := findBoard("arduino_avr")
	code, err := generate(board, "Arduino/Genuino Uno", program)
	if err != nil || strings.Count(code, "delay(1);") != 255 {
		t.Fatalf("%d delays: %v", strings.Count(code, "delay(1);"), err)
	}
}
