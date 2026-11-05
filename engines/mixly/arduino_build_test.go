package main

import (
	"encoding/binary"
	"errors"
	"os"
	"path/filepath"
	"regexp"
	"slices"
	"strings"
	"testing"
	"time"
)

func protocolCode(err error) string {
	var problem *ProtocolError
	if errors.As(err, &problem) {
		return problem.Code
	}
	return ""
}

// Upload goes only to the board the child picked: listed now, an UNO or a clone's USB bridge,
// with the same USB IDs. Anything else is a port problem, never a reason to try the upload.
func TestUploadPortMustStillBeThePickedBoard(t *testing.T) {
	devices := []Device{
		{Port: "COM3", USB: true, VID: "2341", PID: "0043", Kind: "uno"},
		{Port: "COM5", USB: true, VID: "1A86", PID: "7523", Kind: "usb-serial", Chip: "CH340"},
		{Port: "COM7", USB: true, VID: "046D", PID: "C52B", Kind: "other"},
		{Port: "COM1", Kind: "other"},
	}
	for _, c := range []struct {
		port, vid, pid string
		ok             bool
	}{
		{"COM3", "2341", "0043", true},
		{"COM5", "1a86", "7523", true},
		{"COM5", "2341", "0043", false},
		{"COM7", "046D", "C52B", false},
		{"COM1", "", "", false},
		{"COM9", "2341", "0043", false},
	} {
		_, err := matchBoardPort(devices, c.port, c.vid, c.pid)
		if c.ok != (err == nil) || (err != nil && protocolCode(err) != "TRANSPORT_ERROR") {
			t.Errorf("%s %s:%s: %v", c.port, c.vid, c.pid, err)
		}
	}
	if got := uploadSpeedArguments(devices[1]); len(got) != 2 || got[1] != "upload.speed=117647" {
		t.Errorf("CH340 upload speed: %v", got)
	}
	if got := uploadSpeedArguments(devices[0]); got != nil {
		t.Errorf("genuine UNO keeps its own speed: %v", got)
	}
	for _, c := range []struct {
		vid, pid string
		arduino  bool
		ok       bool
	}{
		{"2341", "0043", true, true},
		{"1a86", "7523", true, true},
		{"", "0043", true, false},
		{"2341", "43", true, false},
		{"2341", "004G", true, false},
		{"", "", false, true},
		{"2341", "0043", false, false},
	} {
		err := checkUploadIDs(ProgramInput{VID: c.vid, PID: c.pid}, c.arduino)
		if c.ok != (err == nil) || (err != nil && protocolCode(err) != "INVALID_INPUT") {
			t.Errorf("%q:%q arduino=%v: %v", c.vid, c.pid, c.arduino, err)
		}
	}
}

// Libraries come only from real folders beside the engine; an install without them is broken.
func TestBundledLibrariesAreTheFoldersBesideTheEngine(t *testing.T) {
	engineDir := t.TempDir()
	if _, err := bundledLibraries(engineDir); err == nil {
		t.Fatal("missing libraries folder accepted")
	}
	root := filepath.Join(engineDir, "libraries")
	for _, name := range []string{"U8g2", "Servo"} {
		if err := os.MkdirAll(filepath.Join(root, name), 0o755); err != nil {
			t.Fatal(err)
		}
	}
	if err := os.WriteFile(filepath.Join(root, "notes.txt"), nil, 0o644); err != nil {
		t.Fatal(err)
	}
	libraries, err := bundledLibraries(engineDir)
	want := []string{filepath.Join(root, "Servo"), filepath.Join(root, "U8g2")}
	if err != nil || !slices.Equal(libraries, want) {
		t.Fatalf("%v: %v", libraries, err)
	}
}

// The shipped libraries folder must provide every header the School Bot generators include,
// with its license texts.
func TestShippedLibrariesProvideTheGeneratorsHeaders(t *testing.T) {
	include := regexp.MustCompile(`#include [<"]([^>"]+)[>"]`)
	headers := map[string]bool{}
	for _, script := range []string{schoolBotArduino, schoolBotDevices, schoolBotModules} {
		for _, match := range include.FindAllStringSubmatch(script, -1) {
			headers[match[1]] = true
		}
	}
	delete(headers, "Wire.h") // part of the Arduino AVR core
	if len(headers) != 7 {
		t.Fatalf("generators include %v; update the bundled libraries with them", headers)
	}
	found := map[string]bool{}
	err := filepath.WalkDir("libraries", func(path string, entry os.DirEntry, err error) error {
		if err == nil && !entry.IsDir() {
			found[entry.Name()] = true
		}
		return err
	})
	if err != nil {
		t.Fatal(err)
	}
	for header := range headers {
		if !found[header] {
			t.Errorf("no bundled library provides %s", header)
		}
	}
	for _, license := range []string{
		"Servo/LICENSE.txt", "Adafruit_NeoPixel/COPYING", "U8g2/LICENSE", "IRremote/LICENSE",
		"ZLTech_Adafruit_TCS34725/license.txt",
	} {
		if _, err := os.Stat(filepath.Join("libraries", license)); err != nil {
			t.Errorf("license missing: %v", err)
		}
	}
}

// The build folder survives between requests but belongs to one compile or upload at a time.
func TestArduinoWorkspaceIsPrivateAndLocked(t *testing.T) {
	temp := t.TempDir()
	workspace, err := openArduinoWorkspace(temp, "arduino:avr:uno")
	if err != nil {
		t.Fatal(err)
	}
	base := filepath.Join(temp, "school-bot-mixly", "arduino_avr_uno")
	if workspace.sketch != filepath.Join(base, "program") || workspace.build != filepath.Join(base, "build") {
		t.Fatalf("%+v", workspace)
	}
	if _, err := openArduinoWorkspace(temp, "arduino:avr:uno"); protocolCode(err) != "ENGINE_BUSY" {
		t.Fatalf("second user of the build folder: %v", err)
	}
	workspace.release()
	again, err := openArduinoWorkspace(temp, "arduino:avr:uno")
	if err != nil {
		t.Fatalf("released build folder: %v", err)
	}
	again.release()
}

// A compiler that exits with an error is the program's fault and keeps the tool's own words; one
// that runs past its deadline is a timeout.
func TestCompileFailureKeepsTheToolOutput(t *testing.T) {
	executable, err := os.Executable()
	if err != nil {
		t.Fatal(err)
	}
	cli := arduinoCLI{path: executable, env: []string{"MIXLY_TEST_TOOL=1", "MIXLY_TEST_EXIT=1"}}
	helper := []string{"-test.run=^TestToolSubprocessHelper$"}
	output, err := runArduinoCLI(cli, helper, 10*time.Second)
	failure := compileFailure(cli, "esp32:esp32:esp32", err, output, time.Second)
	if protocolCode(failure) != "COMPILE_FAILED" || !strings.Contains(failure.Error(), "tool-output") {
		t.Fatalf("%q: %v", output, failure)
	}
	cli.env = []string{"MIXLY_TEST_TOOL=1", "MIXLY_TEST_SLEEP=1"}
	if _, err := runArduinoCLI(cli, helper, time.Second); !isTimeout(err) {
		t.Fatalf("slow compile: %v", err)
	}
}

// elfObject is an ELF32 object header announcing sections at sectionOffset; with sections > 0 and
// nothing after the header, the file is an object a killed assembler cut short.
func elfObject(sectionOffset uint32, sections uint16) []byte {
	header := make([]byte, 52)
	copy(header, "\x7fELF\x01\x01\x01")
	binary.LittleEndian.PutUint16(header[16:], 1)  // relocatable
	binary.LittleEndian.PutUint16(header[18:], 83) // AVR
	binary.LittleEndian.PutUint32(header[20:], 1)
	binary.LittleEndian.PutUint32(header[32:], sectionOffset)
	binary.LittleEndian.PutUint16(header[40:], 52)
	binary.LittleEndian.PutUint16(header[46:], 40)
	binary.LittleEndian.PutUint16(header[48:], sections)
	return header
}

// After a killed compile only what it may have cut short is rebuilt: finished objects stay, so a
// slow PC keeps the progress it made before its deadline.
func TestUnfinishedBuildKeepsFinishedObjects(t *testing.T) {
	build := t.TempDir()
	files := map[string][]byte{
		"core/core.a":              []byte("!<arch>\n"),
		"libraries/U8g2/done.c.o":  elfObject(0, 0),
		"libraries/U8g2/done.c.d":  []byte("done.c.o: done.c"),
		"libraries/U8g2/cut.c.o":   elfObject(52, 3),
		"libraries/U8g2/cut.c.d":   []byte("cut.c.o: cut.c"),
		"sketch/program.ino.cpp.o": nil,
		"build.options.json":       []byte("{}"),
	}
	for name, data := range files {
		path := filepath.Join(build, filepath.FromSlash(name))
		if err := os.MkdirAll(filepath.Dir(path), 0o700); err != nil {
			t.Fatal(err)
		}
		if err := os.WriteFile(path, data, 0o600); err != nil {
			t.Fatal(err)
		}
	}
	if err := dropUnfinishedBuild(build); err != nil {
		t.Fatal(err)
	}
	for name := range files {
		_, err := os.Stat(filepath.Join(build, filepath.FromSlash(name)))
		kept := err == nil
		want := strings.Contains(name, "done.c") || name == "build.options.json"
		if kept != want {
			t.Errorf("%s kept=%v, want %v", name, kept, want)
		}
	}
}
