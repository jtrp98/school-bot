package main

import (
	"encoding/json"
	"errors"
	"os"
	"path/filepath"
	"runtime"
	"slices"
	"strings"
	"testing"
	"time"
)

// The engine runs the arduino-cli inside Arduino IDE 2 when its own toolchains folder has none,
// with the IDE's settings, and never one from anywhere else.
func TestArduinoCLIIsTheEnginesOwnOrArduinoIDEs(t *testing.T) {
	root := t.TempDir()
	engineDir := filepath.Join(root, "engine")
	home := filepath.Join(root, "home")
	userPrograms := filepath.Join(root, "programs")
	folders := arduinoFolders{
		ideResources: []string{filepath.Join(userPrograms, "Arduino IDE", "resources"), "relative", `\\server\share`},
		home:         home,
		env:          []string{"HOME=" + home},
	}
	touch := func(path string) {
		if err := os.MkdirAll(filepath.Dir(path), 0o755); err != nil {
			t.Fatal(err)
		}
		if err := os.WriteFile(path, nil, 0o644); err != nil {
			t.Fatal(err)
		}
	}
	find := func() (arduinoCLI, error) {
		return firstArduinoCLI(arduinoCLICandidates(engineDir, "arduino-cli.exe", folders))
	}

	if _, err := find(); err != errArduinoCLIMissing {
		t.Fatalf("nothing installed: %v", err)
	}
	ideResources := filepath.Join(folders.ideResources[0], "app", "lib", "backend", "resources")
	ide := filepath.Join(ideResources, "arduino-cli.exe")
	touch(ide)
	cli, err := find()
	if err != nil || cli.path != ide || cli.source != "arduino-ide" || cli.config != "" {
		t.Fatalf("IDE without settings = %+v, %v", cli, err)
	}
	if !slices.Equal(cli.env, []string{arduinoQuiet, "HOME=" + home}) {
		t.Fatalf("arduino-cli environment = %v", cli.env)
	}
	settings := filepath.Join(home, ".arduinoIDE", "arduino-cli.yaml")
	touch(settings)
	if cli, _ := find(); cli.config != settings {
		t.Fatalf("IDE settings not used: %+v", cli)
	}
	own := filepath.Join(engineDir, "toolchains", "arduino-cli.exe")
	touch(own)
	if cli, _ := find(); cli.path != own || cli.source != "toolchains" || cli.config != "" {
		t.Fatalf("toolchains CLI did not win: %+v", cli)
	}
	for _, candidate := range arduinoCLICandidates(engineDir, "arduino-cli.exe", folders) {
		if !filepath.IsAbs(candidate.path) || strings.HasPrefix(candidate.path, `\\`) {
			t.Errorf("candidate outside a local absolute folder: %s", candidate.path)
		}
	}
}

// Arduino IDE 2's .exe installer uses "Arduino IDE" and its .msi (winget) "arduino-ide"; the
// per-user install is still searched before the all-users one.
func TestWindowsIDEFoldersCoverBothInstallers(t *testing.T) {
	root := t.TempDir()
	user, machine := filepath.Join(root, "user"), filepath.Join(root, "machine")
	resources := windowsIDEResources(user, "", machine)
	want := []string{
		filepath.Join(user, "Arduino IDE", "resources"), filepath.Join(user, "arduino-ide", "resources"),
		filepath.Join(machine, "Arduino IDE", "resources"), filepath.Join(machine, "arduino-ide", "resources"),
	}
	if !slices.Equal(resources, want) {
		t.Fatalf("IDE folders = %v", resources)
	}
	folders := arduinoFolders{ideResources: resources}
	msi := filepath.Join(machine, "arduino-ide", "resources", "app", "lib", "backend", "resources", "arduino-cli.exe")
	if err := os.MkdirAll(filepath.Dir(msi), 0o755); err != nil {
		t.Fatal(err)
	}
	if err := os.WriteFile(msi, nil, 0o644); err != nil {
		t.Fatal(err)
	}
	cli, err := firstArduinoCLI(arduinoCLICandidates(filepath.Join(root, "engine"), "arduino-cli.exe", folders))
	if err != nil || cli.path != msi || cli.source != "arduino-ide" {
		t.Fatalf("MSI install not found: %+v, %v", cli, err)
	}
}

// The host starts the engine with almost no environment, so the folders must not come from it.
func TestArduinoFoldersDoNotComeFromTheEnvironment(t *testing.T) {
	for _, name := range []string{"LOCALAPPDATA", "ProgramFiles", "USERPROFILE", "APPDATA", "HOME"} {
		t.Setenv(name, "")
	}
	folders := arduinoUserFolders()
	if !filepath.IsAbs(folders.home) {
		t.Fatalf("home = %q", folders.home)
	}
	if runtime.GOOS != "linux" && len(folders.ideResources) == 0 {
		t.Fatal("no Arduino IDE folders")
	}
	for _, resources := range folders.ideResources {
		if !filepath.IsAbs(resources) {
			t.Errorf("relative IDE folder %q", resources)
		}
	}
	if len(folders.env) == 0 {
		t.Fatal("arduino-cli would start without the user's folders")
	}
}

func TestDataFolderIsReadFromBothConfigDumpFormats(t *testing.T) {
	data := filepath.Join(t.TempDir(), "Arduino15")
	directories := map[string]any{"directories": map[string]string{"data": data}}
	older, _ := json.Marshal(directories)
	current, _ := json.Marshal(map[string]any{"config": directories})
	for _, c := range []struct {
		name, dump string
		fails      bool
	}{
		{name: "0.x", dump: string(older)},
		{name: "1.x", dump: string(current)},
		{name: "relative", dump: `{"directories":{"data":"Arduino15"}}`, fails: true},
		{name: "missing", dump: `{}`, fails: true},
		{name: "not JSON", dump: `Error: unknown`, fails: true},
	} {
		got, err := arduinoDataDirectory([]byte(c.dump))
		if (err != nil) != c.fails || (!c.fails && got != data) {
			t.Errorf("%s: got %q, %v", c.name, got, err)
		}
	}
	// arduino-cli 1.x leaves defaults out of the dump; the status then asks `config get`.
	unset := `{"config":{"updater":{"enable_notification":false}}}`
	if _, err := arduinoDataDirectory([]byte(unset)); !errors.Is(err, errArduinoDataUnset) {
		t.Errorf("1.x dump without the data folder: %v", err)
	}
	value, _ := json.Marshal(data)
	if got, err := arduinoDataValue(value); err != nil || got != data {
		t.Errorf("config get: got %q, %v", got, err)
	}
	for _, bad := range []string{`"Arduino15"`, `{"data":"x"}`, ``} {
		if _, err := arduinoDataValue([]byte(bad)); err == nil {
			t.Errorf("config get %q accepted", bad)
		}
	}
}

func TestAVRCoreIsInstalledWhenBoardsManagerPutItOnDisk(t *testing.T) {
	data := t.TempDir()
	if avrCoreInstalled(data) {
		t.Fatal("empty data folder has the core")
	}
	core := filepath.Join(data, "packages", "arduino", "hardware", "avr", "1.8.6")
	if err := os.MkdirAll(core, 0o755); err != nil {
		t.Fatal(err)
	}
	if avrCoreInstalled(data) {
		t.Fatal("a folder without platform.txt counted as installed")
	}
	if err := os.WriteFile(filepath.Join(core, "platform.txt"), nil, 0o644); err != nil {
		t.Fatal(err)
	}
	if !avrCoreInstalled(data) {
		t.Fatal("installed core not found")
	}
}

// A probe keeps only stdout, so arduino-cli's notices on stderr cannot break its JSON, and a
// probe that runs too long comes back as ENGINE_TIMEOUT.
func TestProbeKeepsStdoutAndTimesOut(t *testing.T) {
	executable, err := os.Executable()
	if err != nil {
		t.Fatal(err)
	}
	helper := []string{"-test.run=^TestToolSubprocessHelper$"}
	output, err := probeTool(executable, helper, []string{"MIXLY_TEST_TOOL=1", "MIXLY_TEST_STDERR=1"}, 10*time.Second)
	if err != nil || string(output) != "tool-output" {
		t.Fatalf("%q: %v", output, err)
	}
	_, err = probeTool(executable, helper, []string{"MIXLY_TEST_TOOL=1", "MIXLY_TEST_SLEEP=1"}, time.Second)
	var problem *ProtocolError
	if !errors.As(err, &problem) || problem.Code != "ENGINE_TIMEOUT" {
		t.Fatalf("slow probe: %v", err)
	}
}
