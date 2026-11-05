package main

import (
	"encoding/json"
	"flag"
	"os"
	"path/filepath"
	"strconv"
	"strings"
	"testing"

	"github.com/dop251/goja"
)

var updateGolden = flag.Bool("update", false, "rewrite the School Bot golden C++ files")

// Golden files pin the exact C++ each School Bot block produces. After an intended change,
// run `go test -run TestSchoolBotGolden -update` and review the .ino diff like code.
func TestSchoolBotGolden(t *testing.T) {
	board, ok := findBoard("arduino_avr")
	if !ok {
		t.Fatal("missing AVR board")
	}
	cases, err := filepath.Glob(filepath.Join("testdata", "school-bot", "*.json"))
	if err != nil || len(cases) == 0 {
		t.Fatalf("no golden cases: %v", err)
	}
	for _, input := range cases {
		golden := strings.TrimSuffix(input, ".json") + ".ino"
		t.Run(filepath.Base(golden), func(t *testing.T) {
			raw, err := os.ReadFile(input)
			if err != nil {
				t.Fatal(err)
			}
			var program struct {
				Blocks []*Block `json:"blocks"`
			}
			if err := json.Unmarshal(raw, &program); err != nil {
				t.Fatal(err)
			}
			code, err := generate(board, "Arduino/Genuino Uno", program.Blocks)
			if err != nil {
				t.Fatal(err)
			}
			if *updateGolden {
				if err := os.WriteFile(golden, []byte(code), 0o644); err != nil {
					t.Fatal(err)
				}
				return
			}
			want, err := os.ReadFile(golden)
			if err != nil {
				t.Fatalf("%v (run with -update to create it)", err)
			}
			// Git may check golden files out with CRLF on Windows.
			if code != strings.ReplaceAll(string(want), "\r\n", "\n") {
				t.Fatalf("generated C++ changed:\n--- got\n%s\n--- want\n%s", code, want)
			}
		})
	}
}

// Field values come from the page and are pasted into C++, so they must be UNO pins or fixed choices.
func TestSchoolBotFieldsAreValidated(t *testing.T) {
	board, ok := findBoard("arduino_avr")
	if !ok {
		t.Fatal("missing AVR board")
	}
	for _, block := range []*Block{
		{Type: "sb_relay", Fields: map[string]string{"PIN": "0", "STATE": "HIGH"}},
		{Type: "sb_relay", Fields: map[string]string{"PIN": "8", "STATE": "HIGH);evil("}},
		{Type: "sb_led_brightness", Fields: map[string]string{"PIN": "2"}},
		{Type: "sb_fan", Fields: map[string]string{"PIN_A": "4", "PIN_B": "7", "DIRECTION": "FORWARD"}},
		{Type: "sb_ir_button", Fields: map[string]string{"BUTTON": "0x1;evil"}},
		{Type: "sb_rgb_setup", Fields: map[string]string{"PIN": "6", "COUNT": "0"}},
		{Type: "sb_servo", Fields: map[string]string{"PIN": "13);evil(13"}},
	} {
		program := []*Block{{Type: "sb_loop", Statements: map[string]*Block{"DO": block}}}
		if code, err := generate(board, "Arduino/Genuino Uno", program); err == nil {
			t.Fatalf("%s %v accepted:\n%s", block.Type, block.Fields, code)
		}
	}
	for _, block := range []*Block{
		{Type: "sb_relay", Fields: map[string]string{"PIN": "13", "STATE": "HIGH"}},
		{Type: "sb_fan", Fields: map[string]string{"PIN_A": "9", "PIN_B": "7", "DIRECTION": "FORWARD"}},
		{Type: "sb_rgb_setup", Fields: map[string]string{"PIN": "2", "COUNT": "61", "BRIGHTNESS": "50"}},
		{Type: "sb_rgb_setup", Fields: map[string]string{"PIN": "2", "COUNT": "4", "BRIGHTNESS": "101"}},
	} {
		if code, err := generate(board, "Arduino/Genuino Uno", inLoop(block)); err == nil {
			t.Fatalf("%s %v accepted:\n%s", block.Type, block.Fields, code)
		}
	}
	if _, err := generate(board, "Arduino/Genuino Uno", inLoop(
		&Block{Type: "sb_led", Fields: map[string]string{"PIN": "13", "STATE": "HIGH"}},
	)); err != nil {
		t.Fatalf("the on-board LED must stay usable on pin 13: %v", err)
	}
	analogPin := &Block{Type: "sb_tilt", Fields: map[string]string{"PIN": "7", "MODE": "ANALOG"}}
	program := []*Block{{Type: "sb_loop", Statements: map[string]*Block{"DO": {
		Type: "serial_print", Fields: map[string]string{"serial_select": "Serial", "new_line": "println"},
		Values: map[string]*Block{"CONTENT": analogPin},
	}}}}
	if _, err := generate(board, "Arduino/Genuino Uno", program); err == nil {
		t.Fatal("analog read accepted a digital-only pin")
	}
}

// A mixed colour is pasted into C++ as a hex literal, so only #RRGGBB in capitals generates.
func TestSchoolBotMixedColourIsHexOnly(t *testing.T) {
	board, ok := findBoard("arduino_avr")
	if !ok {
		t.Fatal("missing AVR board")
	}
	paint := func(colour string) []*Block {
		return inLoop(&Block{
			Type:   "sb_rgb_set",
			Fields: map[string]string{"PIN": "2"},
			Values: map[string]*Block{
				"PIXEL": {Type: "math_number", Fields: map[string]string{"NUM": "1"}},
				"COLOR": {Type: "sb_rgb_named_color", Fields: map[string]string{"COLOR": colour}},
			},
		})
	}
	code, err := generate(board, "Arduino/Genuino Uno", paint("#FF8400"))
	if err != nil || !strings.Contains(code, "setPixelColor(0, 0xFF8400)") {
		t.Fatalf("mixed colour: %v\n%s", err, code)
	}
	for _, colour := range []string{"#ff8400", "#FF840", "FF8400", "#FF8400);evil(", "ORANGE"} {
		if code, err := generate(board, "Arduino/Genuino Uno", paint(colour)); err == nil {
			t.Fatalf("%q accepted:\n%s", colour, code)
		}
	}
}

// inLoop chains blocks into one loop hat body.
func inLoop(blocks ...*Block) []*Block {
	for i := len(blocks) - 2; i >= 0; i-- {
		blocks[i].Next = blocks[i+1]
	}
	return []*Block{{Type: "sb_loop", Statements: map[string]*Block{"DO": blocks[0]}}}
}

// Combinations that compile but fail silently on the UNO must not generate.
func TestSchoolBotRejectsConflictingHardware(t *testing.T) {
	board, ok := findBoard("arduino_avr")
	if !ok {
		t.Fatal("missing AVR board")
	}
	number := func(value string) *Block { return &Block{Type: "math_number", Fields: map[string]string{"NUM": value}} }
	tone := func() *Block {
		return &Block{Type: "sb_buzzer_tone", Fields: map[string]string{"PIN": "4"},
			Values: map[string]*Block{"FREQUENCY": number("440")}}
	}
	print := func(value *Block) *Block {
		return &Block{Type: "serial_print", Fields: map[string]string{"serial_select": "Serial", "new_line": "println"},
			Values: map[string]*Block{"CONTENT": value}}
	}
	for name, program := range map[string][]*Block{
		"tone and IR share Timer2": inLoop(
			tone(),
			print(&Block{Type: "sb_ir_received", Fields: map[string]string{"PIN": "11"}}),
		),
		"NeoPixel and PWM on one pin": inLoop(
			&Block{Type: "sb_rgb_set", Fields: map[string]string{"PIN": "6"}},
			&Block{Type: "sb_led_brightness", Fields: map[string]string{"PIN": "6"},
				Values: map[string]*Block{"BRIGHTNESS": number("50")}},
		),
		"relay on the I2C bus": inLoop(
			&Block{Type: "sb_relay", Fields: map[string]string{"PIN": "A4", "STATE": "HIGH"}},
			&Block{Type: "sb_oled_clear"},
		),
		"input and output on one pin": inLoop(
			print(&Block{Type: "sb_button", Fields: map[string]string{"PIN": "7"}}),
			&Block{Type: "sb_relay", Fields: map[string]string{"PIN": "7", "STATE": "LOW"}},
		),
	} {
		if code, err := generate(board, "Arduino/Genuino Uno", program); err == nil {
			t.Errorf("%s accepted:\n%s", name, code)
		}
	}
}

// Block Lab's check before sending lists every cause at once, one entry per pin, by code.
func TestSchoolBotValidateListsWiringProblems(t *testing.T) {
	validate := func(blocks string) Response {
		data := `{"board":"arduino_avr","platform":"Arduino/Genuino Uno","blocks":` + blocks + `}`
		return handleLine(requestLine("project.validate", data))
	}
	led := func(pin string) string {
		return `{"type":"sb_led","fields":{"PIN":"` + pin + `","STATE":"HIGH"}`
	}
	button := func(pin string) string {
		return `{"type":"serial_print","fields":{"serial_select":"Serial","new_line":"println"},` +
			`"values":{"CONTENT":{"type":"sb_button","fields":{"PIN":"` + pin + `"}}}`
	}
	servo := `{"type":"sb_servo","fields":{"PIN":"7"},` +
		`"values":{"ANGLE":{"type":"math_number","fields":{"NUM":"90"}}}`
	tone := `{"type":"sb_buzzer_tone","fields":{"PIN":"4"},` +
		`"values":{"FREQUENCY":{"type":"math_number","fields":{"NUM":"440"}}}`
	ir := `{"type":"serial_print","fields":{"serial_select":"Serial","new_line":"println"},` +
		`"values":{"CONTENT":{"type":"sb_ir_received","fields":{"PIN":"11"}}}`
	loop := func(statements ...string) string {
		body := statements[len(statements)-1] + "}"
		for i := len(statements) - 2; i >= 0; i-- {
			body = statements[i] + `,"next":` + body + "}"
		}
		return `[{"type":"sb_loop","statements":{"DO":` + body + `}}]`
	}

	response := validate(loop(button("7"), led("7"), servo, tone, ir))
	if response.Status != "success" {
		t.Fatalf("validate failed: %+v", response.Error)
	}
	var check ProgramCheck
	if err := json.Unmarshal(response.Data, &check); err != nil {
		t.Fatal(err)
	}
	want := ProgramProblems{
		{Code: "PIN_SHARED", Pin: "7", Uses: []string{"input", "output", "servo"}},
		{Code: "TIMER2_SHARED", Uses: []string{"tone", "ir"}},
	}
	if got, _ := json.Marshal(check.Problems); string(got) != mustJSON(t, want) {
		t.Errorf("problems = %s, want %s", got, mustJSON(t, want))
	}

	response = validate(loop(led("7")))
	err := json.Unmarshal(response.Data, &check)
	if err != nil || check.Problems == nil || len(check.Problems) != 0 {
		t.Errorf("a program that works = %s (%v)", response.Data, err)
	}
	// Every other action refuses the same wiring before any tool runs; generate says why for
	// the C++ tab. Compile and upload stay dry-run here.
	conflict := `{"board":"arduino_avr","port":"no-device","blocks":` + loop(button("7"), led("7")) + `}`
	for _, action := range []string{"program.generate", "program.compile", "program.upload"} {
		refused := handleLine(requestLine(action, conflict))
		if refused.Error == nil || refused.Error.Code != "PROGRAM_INVALID" ||
			refused.Error.Message != "pin 7 is used as input and as output" {
			t.Errorf("%s = %+v", action, refused)
		}
	}
}

func mustJSON(t *testing.T, value any) string {
	t.Helper()
	encoded, err := json.Marshal(value)
	if err != nil {
		t.Fatal(err)
	}
	return string(encoded)
}

// The Block Lab catalog is the contract with the frontend toolbox: every block it lists,
// shipped or deferred, must have an Arduino generator on the UNO.
func TestBlockLabCatalogBlocksHaveGenerators(t *testing.T) {
	raw, err := os.ReadFile(filepath.Join("..", "..", "contracts", "block-lab", "catalog.json"))
	if err != nil {
		t.Fatal(err)
	}
	var catalogFile struct {
		Board      string `json:"board"`
		Categories []struct {
			ID     string `json:"id"`
			Blocks []struct {
				Type string `json:"type"`
			} `json:"blocks"`
		} `json:"categories"`
	}
	if err := json.Unmarshal(raw, &catalogFile); err != nil {
		t.Fatal(err)
	}
	board, ok := findBoard(catalogFile.Board)
	if !ok {
		t.Fatalf("catalog board %q not found", catalogFile.Board)
	}
	vm := goja.New()
	if err := loadGenerators(vm, board, "Arduino/Genuino Uno"); err != nil {
		t.Fatal(err)
	}
	seen := map[string]bool{}
	for _, category := range catalogFile.Categories {
		for _, block := range category.Blocks {
			if seen[block.Type] {
				t.Errorf("%s is listed twice", block.Type)
			}
			seen[block.Type] = true
			defined, err := vm.RunString("typeof Blockly.Arduino[" + strconv.Quote(block.Type) + "] === 'function'")
			if err != nil || !defined.ToBoolean() {
				t.Errorf("%s/%s has no generator", category.ID, block.Type)
			}
		}
	}
	if len(catalogFile.Categories) != 16 {
		t.Errorf("catalog has %d categories, the rail has 16", len(catalogFile.Categories))
	}
}

func TestSchoolBotBlocksStayOffOtherBoards(t *testing.T) {
	board, ok := findBoard("arduino_esp32")
	if !ok {
		t.Fatal("missing ESP32 board")
	}
	if _, err := generate(board, "", []*Block{{Type: "sb_forever"}}); err == nil {
		t.Fatal("School Bot blocks are for the UNO only")
	}
}
