package main

import (
	"encoding/json"
	"errors"
	"fmt"
	"os"
	"path/filepath"
	"runtime"
	"strings"
	"time"
)

// arduinoCLI is the arduino-cli the engine runs, the config file it passes (empty: the CLI's own
// defaults, which are the same folders Arduino IDE 2 uses) and the environment it adds.
type arduinoCLI struct {
	path   string
	config string
	source string
	env    []string
}

// arduinoFolders are the user's folders as the OS reports them, not as the environment does.
type arduinoFolders struct {
	// Arduino IDE 2 install folders' resources directory, per-user install first.
	ideResources []string
	home         string
	// What arduino-cli needs to find the user's Arduino data and sketchbook folders.
	env []string
}

// Every arduino-cli run would otherwise also ask the internet for a newer CLI, which stalls on
// a school network that blocks it and only prints a notice nobody here acts on.
const arduinoQuiet = "ARDUINO_UPDATER_ENABLE_NOTIFICATION=false"

// Block Lab does not ship a compiler: teachers install Arduino IDE 2, which carries its own
// arduino-cli and installs the AVR core on first start. Only these fixed places are searched,
// never PATH, the working directory or anything a request names, so a request cannot choose
// what the engine runs. The engine's own toolchains folder comes first so IT can pin a CLI.
func arduinoCLICandidates(engineDir, executable string, folders arduinoFolders) []arduinoCLI {
	env := append([]string{arduinoQuiet}, folders.env...)
	toolchains := filepath.Join(engineDir, "toolchains")
	candidates := []arduinoCLI{{
		path:   filepath.Join(toolchains, executable),
		config: filepath.Join(toolchains, "arduino-cli.yaml"),
		source: "toolchains",
		env:    env,
	}}
	// The IDE keeps the CLI settings it uses (sketchbook and library folders) in the user's home.
	ideConfig := ""
	if folders.home != "" {
		ideConfig = filepath.Join(folders.home, ".arduinoIDE", "arduino-cli.yaml")
	}
	for _, resources := range folders.ideResources {
		// A redirected profile on a network share is not a place to run a compiler from.
		if !filepath.IsAbs(resources) || strings.HasPrefix(resources, `\\`) || strings.HasPrefix(resources, "//") {
			continue
		}
		candidates = append(candidates, arduinoCLI{
			path:   filepath.Join(resources, "app", "lib", "backend", "resources", executable),
			config: ideConfig,
			source: "arduino-ide",
			env:    env,
		})
	}
	return candidates
}

// Arduino IDE 2's .exe (NSIS) installer uses the folder name "Arduino IDE"; its .msi, which
// winget installs, uses "arduino-ide". Each base folder is searched for both, in order.
func windowsIDEResources(bases ...string) []string {
	var resources []string
	for _, base := range bases {
		if base == "" {
			continue
		}
		for _, name := range []string{"Arduino IDE", "arduino-ide"} {
			resources = append(resources, filepath.Join(base, name, "resources"))
		}
	}
	return resources
}

// Arduino IDE 2.2 and later keep the CLI where arduinoCLICandidates looks.
var errArduinoCLIMissing = errors.New("arduino-cli was not found: install Arduino IDE 2.2 or later")

func firstArduinoCLI(candidates []arduinoCLI) (arduinoCLI, error) {
	for _, candidate := range candidates {
		if info, err := os.Stat(candidate.path); err != nil || info.IsDir() {
			continue
		}
		if info, err := os.Stat(candidate.config); candidate.config == "" || err != nil || info.IsDir() {
			candidate.config = ""
		}
		return candidate, nil
	}
	return arduinoCLI{}, errArduinoCLIMissing
}

func findArduinoCLI() (arduinoCLI, error) {
	engine, err := os.Executable()
	if err != nil {
		return arduinoCLI{}, err
	}
	name := "arduino-cli"
	if runtime.GOOS == "windows" {
		name += ".exe"
	}
	return firstArduinoCLI(arduinoCLICandidates(filepath.Dir(engine), name, arduinoUserFolders()))
}

func (cli arduinoCLI) arguments(arguments ...string) []string {
	if cli.config != "" {
		arguments = append(arguments, "--config-file", cli.config)
	}
	return arguments
}

// ArduinoStatus says whether Block Lab can compile for the UNO: an arduino-cli was found and
// its arduino:avr core is installed. ProblemCode and Problem say why that could not be told:
// ENGINE_TIMEOUT, TOOL_FAILED or CONFIG_UNREADABLE.
type ArduinoStatus struct {
	Found       bool   `json:"found"`
	Source      string `json:"source,omitempty"`
	AVRCore     bool   `json:"avr_core"`
	ProblemCode string `json:"problem_code,omitempty"`
	Problem     string `json:"problem,omitempty"`
}

// Asking arduino-cli where its data folder is only reads its settings. The host must give
// toolchain.status a longer deadline than this.
const arduinoProbeTimeout = 10 * time.Second

// The status looks for the core on disk instead of running `core list`, which downloads the
// board and library indexes (about 60 MB) wherever they are missing, school network or not.
func arduinoStatus() ArduinoStatus {
	cli, err := findArduinoCLI()
	if err != nil {
		return ArduinoStatus{}
	}
	return arduinoStatusOf(cli)
}

func arduinoStatusOf(cli arduinoCLI) ArduinoStatus {
	status := ArduinoStatus{Found: true, Source: cli.source}
	toolFailed := func(err error) ArduinoStatus {
		status.ProblemCode = "TOOL_FAILED"
		var problem *ProtocolError
		if errors.As(err, &problem) {
			status.ProblemCode = problem.Code
		}
		status.Problem = truncate(err.Error(), 300)
		return status
	}
	dump, err := probeTool(cli.path, cli.arguments("config", "dump", "--format", "json"), cli.env, arduinoProbeTimeout)
	if err != nil {
		return toolFailed(err)
	}
	data, err := arduinoDataDirectory(dump)
	if errors.Is(err, errArduinoDataUnset) {
		// arduino-cli 1.x dumps only what a settings file or the environment set, so the
		// default data folder (Arduino IDE 2 without its own settings file) has to be asked for.
		value, probeErr := probeTool(cli.path, cli.arguments("config", "get", "directories.data", "--format", "json"),
			cli.env, arduinoProbeTimeout)
		if probeErr != nil {
			return toolFailed(probeErr)
		}
		data, err = arduinoDataValue(value)
	}
	if err != nil {
		status.ProblemCode = "CONFIG_UNREADABLE"
		status.Problem = truncate(err.Error(), 300)
		return status
	}
	status.AVRCore = avrCoreInstalled(data)
	return status
}

// arduinoDataDirectory reads `arduino-cli config dump --format json`: the settings at the top
// level in arduino-cli 0.x, under "config" in 1.x.
func arduinoDataDirectory(dump []byte) (string, error) {
	type settings struct {
		Directories struct {
			Data string `json:"data"`
		} `json:"directories"`
	}
	var parsed struct {
		settings
		Config settings `json:"config"`
	}
	if err := json.Unmarshal(dump, &parsed); err != nil {
		return "", fmt.Errorf("arduino-cli config dump printed no JSON: %s", truncate(string(dump), 120))
	}
	data := parsed.Directories.Data
	if data == "" {
		data = parsed.Config.Directories.Data
	}
	if data == "" {
		return "", errArduinoDataUnset
	}
	return absoluteDataDirectory(data)
}

var errArduinoDataUnset = errors.New("arduino-cli config dump does not name the data folder")

// arduinoDataValue reads `arduino-cli config get directories.data --format json` (1.x): one
// JSON string.
func arduinoDataValue(value []byte) (string, error) {
	var data string
	if err := json.Unmarshal(value, &data); err != nil {
		return "", fmt.Errorf("arduino-cli config get printed no JSON string: %s", truncate(string(value), 120))
	}
	return absoluteDataDirectory(data)
}

func absoluteDataDirectory(data string) (string, error) {
	if !filepath.IsAbs(data) {
		return "", fmt.Errorf("arduino-cli data folder %q is not an absolute path", truncate(data, 120))
	}
	return data, nil
}

// avrCoreInstalled is true when Boards Manager has installed some version of arduino:avr.
func avrCoreInstalled(data string) bool {
	platforms, _ := filepath.Glob(filepath.Join(data, "packages", "arduino", "hardware", "avr", "*", "platform.txt"))
	return len(platforms) != 0
}

func truncate(text string, limit int) string {
	runes := []rune(text)
	if len(runes) <= limit {
		return text
	}
	return string(runes[:limit]) + "…"
}
