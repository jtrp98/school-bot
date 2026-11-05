package main

import (
	"encoding/json"
	"fmt"
	"io/fs"
	"path"
	"regexp"
	"strings"
	"time"

	"github.com/dop251/goja"
)

var scriptSource = regexp.MustCompile(`(?i)<script[^>]+src=["']([^"']+)["']`)
var htmlComment = regexp.MustCompile(`(?s)<!--.*?-->`)

type Block struct {
	Type       string            `json:"type"`
	Fields     map[string]string `json:"fields,omitempty"`
	Values     map[string]*Block `json:"values,omitempty"`
	Statements map[string]*Block `json:"statements,omitempty"`
	Next       *Block            `json:"next,omitempty"`
	Mutation   map[string]string `json:"mutation,omitempty"`
	Comment    string            `json:"comment,omitempty"`
	Disabled   bool              `json:"disabled,omitempty"`
	Arguments  []Argument        `json:"arguments,omitempty"`
}

type Argument struct {
	Name string `json:"name"`
	Type string `json:"type,omitempty"`
}

// ProgramProblem is wiring the School Bot generators refuse because it compiles but fails on
// the UNO. Codes and uses are stable names that Block Lab words for children.
type ProgramProblem struct {
	Code string   `json:"code"`
	Pin  string   `json:"pin,omitempty"`
	Uses []string `json:"uses"`
}

type ProgramProblems []ProgramProblem

func (problems ProgramProblems) Error() string {
	reasons := make([]string, len(problems))
	for i, problem := range problems {
		switch problem.Code {
		case "PIN_SHARED":
			reasons[i] = "pin " + problem.Pin + " is used as " + strings.Join(problem.Uses, " and as ")
		case "TIMER2_SHARED":
			reasons[i] = "the buzzer tone and the IR receiver cannot be used in one program on the UNO"
		default:
			reasons[i] = problem.Code
		}
	}
	return strings.Join(reasons, "; ")
}

// Only the School Bot overlay (Arduino UNO) records problems; other boards have none.
func wiringProblems(vm *goja.Runtime) (ProgramProblems, error) {
	value, err := vm.RunString(
		"JSON.stringify((Blockly.Arduino && Blockly.Arduino.schoolBotProblems) || [])")
	if err != nil {
		return nil, err
	}
	var problems ProgramProblems
	if err := json.Unmarshal([]byte(value.String()), &problems); err != nil {
		return nil, err
	}
	return problems, nil
}

func generate(board Board, platform string, blocks []*Block) (string, error) {
	vm := goja.New()
	// Blockly generates a row of blocks recursively, two calls per block, so a 512-block row needs
	// over 1024 frames; validateBlocks' block count is what bounds the depth.
	vm.SetMaxCallStackSize(4096)
	timer := time.AfterFunc(3*time.Second, func() { vm.Interrupt("generation deadline exceeded") })
	defer timer.Stop()
	if err := loadGenerators(vm, board, platform); err != nil {
		return "", err
	}
	encoded, err := json.Marshal(blocks)
	if err != nil {
		return "", err
	}
	if err := vm.Set("programJSON", string(encoded)); err != nil {
		return "", err
	}
	if err := vm.Set("programLanguage", board.Language); err != nil {
		return "", err
	}
	if _, err := vm.RunString("prepareGenerator(programLanguage)"); err != nil {
		return "", err
	}
	value, err := vm.RunString("generateProgram(JSON.parse(programJSON), programLanguage)")
	if err != nil {
		return "", err
	}
	problems, err := wiringProblems(vm)
	if err != nil {
		return "", err
	}
	if len(problems) > 0 {
		return "", problems
	}
	code := value.String()
	if len(code) > 131072 {
		return "", fmt.Errorf("generated source exceeds 128 KB")
	}
	return code, nil
}

// loadGenerators runs the trusted embedded scripts that define the board's generators in vm.
func loadGenerators(vm *goja.Runtime, board Board, platform string) error {
	if err := vm.Set("platformName", platform); err != nil {
		return err
	}
	var target targetConfig
	var selectedKey string
	if raw := board.Targets[platform]; len(raw) > 0 {
		if json.Unmarshal(raw, &selectedKey) != nil && json.Unmarshal(raw, &target) == nil {
			selectedKey = target.Key
		}
	}
	if err := vm.Set("selectedBoardKey", selectedKey); err != nil {
		return err
	}
	if _, err := vm.RunString(bootstrap); err != nil {
		return err
	}
	load := func(file string) error {
		source, err := upstream.ReadFile("third_party/mixly/" + file)
		if err != nil {
			return err
		}
		_, err = vm.RunScript(file, string(source))
		return err
	}
	if err := load("board/common/blockly-core/blockly_compressed.js"); err != nil {
		return err
	}
	page, err := upstream.ReadFile("third_party/mixly/" + board.Index)
	if err != nil {
		return err
	}
	for _, match := range scriptSource.FindAllStringSubmatch(htmlComment.ReplaceAllString(string(page), ""), -1) {
		file := path.Clean(path.Join(path.Dir(board.Index), match[1]))
		if !strings.HasPrefix(file, "board/") || strings.Contains(file, ":") {
			continue
		}
		if !strings.Contains(file, "/generators/") && !strings.Contains(file, "/others/") &&
			!strings.HasSuffix(file, "profile.js") && !coreGenerator(file) {
			continue
		}
		if err := load(file); err != nil {
			if _, statErr := fs.Stat(upstream, "third_party/mixly/"+file); statErr != nil {
				return fmt.Errorf("missing upstream generator %s", file)
			}
			return fmt.Errorf("load %s: %w", file, err)
		}
	}
	if _, err := vm.RunString("if (typeof profile !== 'undefined') profile.default = profile[platformName] || profile.default;"); err != nil {
		return err
	}
	// School Bot's own blocks live outside third_party so upstream imports never overwrite them.
	if board.ID == "arduino_avr" {
		if _, err := vm.RunScript("school-bot-arduino.js", schoolBotArduino); err != nil {
			return err
		}
		if _, err := vm.RunScript("school-bot-devices.js", schoolBotDevices); err != nil {
			return err
		}
		if _, err := vm.RunScript("school-bot-modules.js", schoolBotModules); err != nil {
			return err
		}
	}
	return nil
}

func coreGenerator(file string) bool {
	if path.Dir(file) != "board/common/blockly-core" {
		return false
	}
	switch path.Base(file) {
	case "arduino.js", "python_compressed.js", "names.js", "variables.js", "procedures.js":
		return true
	}
	return false
}
