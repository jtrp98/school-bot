package main

import (
	"context"
	"encoding/json"
	"errors"
	"fmt"
	"io"
	"os"
	"os/exec"
	"path/filepath"
	"runtime"
	"sort"
	"strings"
	"time"
)

type ToolStatus struct {
	Name      string `json:"name"`
	Available bool   `json:"available"`
}

type ToolchainStatus struct {
	Engine  string        `json:"engine"`
	Tools   []ToolStatus  `json:"tools"`
	Arduino ArduinoStatus `json:"arduino"`
}

// toolPath finds the tools only the engine's toolchains folder may hold; arduino-cli is found
// by findArduinoCLI.
func toolPath(name string) (string, error) {
	switch name {
	case "python3", "mpremote", "esptool", "kflash":
	default:
		return "", fmt.Errorf("unknown tool")
	}
	executable, err := os.Executable()
	if err != nil {
		return "", err
	}
	filename := name
	if runtime.GOOS == "windows" {
		filename += ".exe"
	}
	candidate := filepath.Join(filepath.Dir(executable), "toolchains", filename)
	if info, err := os.Stat(candidate); err == nil && !info.IsDir() {
		return candidate, nil
	}
	return "", fmt.Errorf("%s is not installed in the engine's trusted toolchains directory", name)
}

func toolchainStatus() ToolchainStatus {
	arduino := arduinoStatus()
	status := ToolchainStatus{
		Engine:  "mixly",
		Tools:   []ToolStatus{{Name: "arduino-cli", Available: arduino.Found}},
		Arduino: arduino,
	}
	for _, name := range []string{"python3", "mpremote", "esptool", "kflash"} {
		_, err := toolPath(name)
		status.Tools = append(status.Tools, ToolStatus{Name: name, Available: err == nil})
	}
	return status
}

type BuildResult struct {
	Engine   string `json:"engine"`
	Board    string `json:"board"`
	Platform string `json:"platform"`
	Delivery string `json:"delivery"`
	Compiler string `json:"compiler"`
	Compiled bool   `json:"compiled"`
	Uploaded bool   `json:"uploaded"`
	Output   string `json:"output,omitempty"`
}

func buildProgram(board Board, input ProgramInput, upload bool) (BuildResult, error) {
	result := BuildResult{Engine: "mixly", Board: board.ID, Platform: input.Platform, Delivery: "dry-run"}
	if input.Transport != "" && input.Transport != "dry-run" && input.Transport != "native" {
		return result, &ProtocolError{"INVALID_INPUT", "transport must be dry-run or native"}
	}
	if upload && input.Port == "" {
		return result, &ProtocolError{"INVALID_INPUT", "upload requires a selected port"}
	}
	if len(input.Port) > 256 {
		return result, &ProtocolError{"INVALID_INPUT", "port name is too long"}
	}
	if upload {
		if err := checkUploadIDs(input, board.Language == "arduino"); err != nil {
			return result, err
		}
	}
	compiler := "python3"
	if board.Language == "arduino" {
		compiler = "arduino-cli"
		if _, err := arduinoTarget(board, input); err != nil {
			return result, &ProtocolError{"INVALID_INPUT", err.Error()}
		}
	} else if len(input.Options) != 0 {
		return result, &ProtocolError{"INVALID_INPUT", "compiler options are supported only for Arduino"}
	}
	if upload && board.Language == "python" {
		if !strings.HasPrefix(board.ID, "micropython_") {
			return result, &ProtocolError{"INVALID_INPUT", "this Python profile has no device upload operation"}
		}
		compiler = "mpremote"
	}
	result.Compiler = compiler
	if input.Transport != "native" {
		return result, nil
	}
	if compiler == "arduino-cli" {
		target, _ := arduinoTarget(board, input)
		return buildArduino(result, target, input, upload)
	}
	tool, err := toolPath(compiler)
	if err != nil {
		return result, err
	}
	if upload {
		if err := checkPort(input.Port); err != nil {
			return result, &ProtocolError{"TRANSPORT_ERROR", err.Error()}
		}
	}
	directory, err := os.MkdirTemp("", "school-bot-mixly-*")
	if err != nil {
		return result, err
	}
	defer os.RemoveAll(directory)
	source := filepath.Join(directory, "program.py")
	if err := os.WriteFile(source, []byte(input.Source), 0600); err != nil {
		return result, err
	}
	arguments := []string{"-I", "-m", "py_compile", source}
	if compiler == "mpremote" {
		arguments = []string{"connect", input.Port, "fs", "cp", source, ":main.py"}
	}
	result.Output, err = runTool(tool, arguments, directory)
	if err != nil {
		return result, err
	}
	result.Delivery = "native"
	result.Compiled = compiler != "mpremote"
	result.Uploaded = upload
	return result, nil
}

type targetConfig struct {
	Key    string `json:"key"`
	Config map[string][]struct {
		Key string `json:"key"`
	} `json:"config"`
}

func arduinoTarget(board Board, input ProgramInput) (string, error) {
	raw := board.Targets[input.Platform]
	var name string
	var target targetConfig
	if err := json.Unmarshal(raw, &name); err == nil {
		target.Key = name
	} else if err := json.Unmarshal(raw, &target); err != nil {
		return "", fmt.Errorf("invalid board target metadata")
	}
	if target.Key == "" {
		return "", fmt.Errorf("board has no compiler target")
	}
	keys := make([]string, 0, len(input.Options))
	for key, selected := range input.Options {
		valid := false
		for _, option := range target.Config[key] {
			if selected == option.Key {
				valid = true
			}
		}
		if !valid {
			return "", fmt.Errorf("invalid board option %s", key)
		}
		keys = append(keys, key)
	}
	sort.Strings(keys)
	options := make([]string, 0, len(keys))
	for _, key := range keys {
		options = append(options, key+"="+input.Options[key])
	}
	if len(options) != 0 {
		target.Key += ":" + strings.Join(options, ",")
	}
	return target.Key, nil
}

// limitedOutput keeps the first limit bytes (zero: 16 KB) and drops the rest.
type limitedOutput struct {
	data      []byte
	limit     int
	truncated bool
}

func (output *limitedOutput) Write(data []byte) (int, error) {
	limit := output.limit
	if limit == 0 {
		limit = 16384
	}
	remaining := limit - len(output.data)
	if remaining < len(data) {
		output.truncated = true
	}
	if remaining > 0 {
		output.data = append(output.data, data[:min(len(data), remaining)]...)
	}
	return len(data), nil
}

// execTool runs one tool under a deadline; running past it is ENGINE_TIMEOUT. env adds to the
// engine's own environment.
func execTool(
	tool string, arguments, env []string, directory string, timeout time.Duration, stdout, stderr io.Writer,
) error {
	ctx, cancel := context.WithTimeout(context.Background(), timeout)
	defer cancel()
	command := exec.CommandContext(ctx, tool, arguments...)
	configureTool(command)
	if len(env) != 0 {
		command.Env = append(os.Environ(), env...)
	}
	command.Dir = directory
	command.WaitDelay = time.Second
	command.Stdout = stdout
	command.Stderr = stderr
	if err := runToolProcess(command); err != nil {
		if ctx.Err() != nil {
			return &ProtocolError{"ENGINE_TIMEOUT", fmt.Sprintf("%s exceeded its %s deadline", filepath.Base(tool), timeout)}
		}
		return err
	}
	return nil
}

func toolFailure(tool string, err error, output *limitedOutput) error {
	if problem := (*ProtocolError)(nil); errors.As(err, &problem) {
		return err
	}
	return fmt.Errorf("%s failed: %w; %s", filepath.Base(tool), err, string(output.data))
}

func runTool(tool string, arguments []string, directory string) (string, error) {
	output := &limitedOutput{}
	// Both streams share the same bounded writer; os/exec serialises its writes.
	if err := execTool(tool, arguments, nil, directory, 2*time.Minute, output, output); err != nil {
		return "", toolFailure(tool, err, output)
	}
	return string(output.data), nil
}

// probeTool runs a tool for what it prints on stdout, kept apart from its diagnostics so it
// stays parseable. A listing cut short would parse as a different answer, so it is an error.
func probeTool(tool string, arguments, env []string, timeout time.Duration) ([]byte, error) {
	stdout, stderr := &limitedOutput{limit: 1 << 20}, &limitedOutput{}
	if err := execTool(tool, arguments, env, os.TempDir(), timeout, stdout, stderr); err != nil {
		return nil, toolFailure(tool, err, stderr)
	}
	if stdout.truncated {
		return nil, fmt.Errorf("%s printed more than 1 MB", filepath.Base(tool))
	}
	return stdout.data, nil
}
