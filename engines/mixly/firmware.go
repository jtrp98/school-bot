package main

import (
	"encoding/json"
	"fmt"
	"os"
	"path/filepath"
	"strings"
	"unicode"
)

type FirmwareInput struct {
	Board     string `json:"board"`
	Platform  string `json:"platform,omitempty"`
	Port      string `json:"port"`
	Transport string `json:"transport,omitempty"`
}

type FlashResult struct {
	Engine   string `json:"engine"`
	Board    string `json:"board"`
	Delivery string `json:"delivery"`
	Flashed  bool   `json:"flashed"`
	Steps    int    `json:"steps"`
}

func flashFirmware(input FirmwareInput) (FlashResult, error) {
	result := FlashResult{Engine: "mixly", Board: input.Board, Delivery: "dry-run"}
	board, ok := findBoard(input.Board)
	if !ok {
		return result, &ProtocolError{"INVALID_INPUT", "unknown board"}
	}
	if input.Transport == "native" && len(board.Targets) > 1 && input.Platform == "" {
		return result, &ProtocolError{"INVALID_INPUT", "native flashing requires an explicit platform"}
	}
	program := ProgramInput{Platform: input.Platform}
	if err := selectPlatform(board, &program); err != nil {
		return result, &ProtocolError{"INVALID_INPUT", err.Error()}
	}
	if input.Port == "" || len(input.Port) > 256 ||
		(input.Transport != "" && input.Transport != "dry-run" && input.Transport != "native") {
		return result, &ProtocolError{"INVALID_INPUT", "select a port and transport dry-run or native"}
	}
	var burn map[string]json.RawMessage
	if json.Unmarshal(board.Config["burn"], &burn) != nil {
		return result, &ProtocolError{"INVALID_INPUT", "this board has no firmware flashing preset"}
	}
	var burnType string
	_ = json.Unmarshal(burn["type"], &burnType)
	if burnType != "command" {
		return result, &ProtocolError{"INVALID_INPUT", "volume-based firmware installation is not supported by this engine"}
	}
	var key string
	_ = json.Unmarshal(board.Targets[program.Platform], &key)
	var preset struct {
		Command string `json:"command"`
	}
	_ = json.Unmarshal(board.Config["burn"], &preset)
	if raw := burn[key]; len(raw) > 0 {
		if err := json.Unmarshal(raw, &preset); err != nil {
			return result, err
		}
	}
	if preset.Command == "" {
		return result, &ProtocolError{"INVALID_INPUT", "selected platform has no firmware flashing preset"}
	}
	type step struct {
		tool string
		args []string
	}
	steps := []step{}
	for _, command := range strings.Split(preset.Command, "&&") {
		tokens, err := presetTokens(command)
		if err != nil || len(tokens) == 0 {
			return result, fmt.Errorf("invalid bundled firmware recipe")
		}
		name := strings.Trim(tokens[0], "{}")
		if name != "esptool" && name != "kflash" {
			return result, fmt.Errorf("unsupported bundled firmware tool %s", name)
		}
		steps = append(steps, step{name, tokens[1:]})
	}
	result.Steps = len(steps)
	if input.Transport != "native" {
		return result, nil
	}
	executable, err := os.Executable()
	if err != nil {
		return result, err
	}
	firmwareRoot := filepath.Join(filepath.Dir(executable), "toolchains", "firmware")
	// Resolve every executable and firmware asset before any erase operation.
	for index := range steps {
		tool, err := toolPath(steps[index].tool)
		if err != nil {
			return result, err
		}
		steps[index].tool = tool
		for position, token := range steps[index].args {
			switch {
			case token == "{com}":
				steps[index].args[position] = input.Port
			case strings.HasPrefix(token, "{indexPath}/"):
				candidate := filepath.Join(firmwareRoot, board.ID, strings.TrimPrefix(token, "{indexPath}/"))
				resolved, err := filepath.EvalSymlinks(candidate)
				if err != nil {
					return result, fmt.Errorf("required firmware asset is missing: %s", filepath.Base(candidate))
				}
				relative, err := filepath.Rel(firmwareRoot, resolved)
				if err != nil || filepath.IsAbs(relative) || relative == ".." ||
					strings.HasPrefix(relative, ".."+string(filepath.Separator)) {
					return result, fmt.Errorf("firmware asset escapes the trusted firmware directory")
				}
				info, err := os.Stat(resolved)
				if err != nil || !info.Mode().IsRegular() {
					return result, fmt.Errorf("firmware asset is not a regular file")
				}
				steps[index].args[position] = resolved
			case strings.ContainsAny(token, "{}"):
				return result, fmt.Errorf("unresolved bundled firmware argument")
			}
		}
	}
	if err := checkPort(input.Port); err != nil {
		return result, &ProtocolError{"TRANSPORT_ERROR", err.Error()}
	}
	for _, step := range steps {
		if _, err := runTool(step.tool, step.args, firmwareRoot); err != nil {
			return result, err
		}
	}
	result.Delivery = "native"
	result.Flashed = true
	return result, nil
}

func presetTokens(command string) ([]string, error) {
	tokens := []string{}
	var token strings.Builder
	var quote rune
	for _, character := range command {
		switch {
		case quote != 0:
			if character == quote {
				quote = 0
			} else {
				token.WriteRune(character)
			}
		case character == '"' || character == '\'':
			quote = character
		case unicode.IsSpace(character):
			if token.Len() > 0 {
				tokens = append(tokens, token.String())
				token.Reset()
			}
		default:
			token.WriteRune(character)
		}
	}
	if quote != 0 {
		return nil, fmt.Errorf("unclosed preset quote")
	}
	if token.Len() > 0 {
		tokens = append(tokens, token.String())
	}
	return tokens, nil
}
