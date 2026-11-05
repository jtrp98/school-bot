package main

import (
	"errors"
	"fmt"
	"os"
	"os/exec"
	"path/filepath"
	"regexp"
	"strings"
	"time"
)

// The host gives each request two minutes: generation (3 s) + the tools must end first, so the
// page gets the tool's own reason rather than the host's timeout. A first compile with U8g2 takes
// over half a minute even on a fast PC, so compile alone gets the most; Block Lab compiles before
// it uploads, which leaves the upload's own compile mostly cached.
const (
	arduinoCompileOnlyTimeout = 110 * time.Second
	arduinoCompileTimeout     = 75 * time.Second
	arduinoUploadTimeout      = 30 * time.Second
)

var (
	usbID        = regexp.MustCompile(`^[0-9A-Fa-f]{4}$`)
	unsafeInFQBN = regexp.MustCompile(`[^A-Za-z0-9]+`)
)

// checkUploadIDs: an Arduino upload names the board the child picked by its USB IDs, so a port
// that now belongs to another device (the robot arm's, say) is refused. Other uploads take none.
func checkUploadIDs(input ProgramInput, arduino bool) error {
	if !arduino {
		if input.VID != "" || input.PID != "" {
			return &ProtocolError{"INVALID_INPUT", "vid and pid are accepted only for Arduino upload"}
		}
		return nil
	}
	if !usbID.MatchString(input.VID) || !usbID.MatchString(input.PID) {
		return &ProtocolError{"INVALID_INPUT", "Arduino upload requires the board's USB vid and pid (four hex digits each)"}
	}
	return nil
}

// matchBoardPort finds port in the current device list and accepts it only as an UNO or a
// USB-serial bridge UNO clones use, with the same USB IDs as when the board was picked.
func matchBoardPort(devices []Device, port, vid, pid string) (Device, error) {
	for _, device := range devices {
		if device.Port != port {
			continue
		}
		if device.Kind != "uno" && device.Kind != "usb-serial" {
			return Device{}, &ProtocolError{"TRANSPORT_ERROR", "the selected port is not an Arduino UNO or a USB-serial bridge"}
		}
		if !strings.EqualFold(device.VID, vid) || !strings.EqualFold(device.PID, pid) {
			return Device{}, &ProtocolError{"TRANSPORT_ERROR", "the device on the selected port is not the board that was picked"}
		}
		return device, nil
	}
	return Device{}, &ProtocolError{"TRANSPORT_ERROR", "the selected port is not connected"}
}

func checkBoardPort(port, vid, pid string) (Device, error) {
	devices, err := listDevices()
	if err != nil {
		return Device{}, &ProtocolError{"TRANSPORT_ERROR", err.Error()}
	}
	return matchBoardPort(devices.Devices, port, vid, pid)
}

// The UNO bootloader runs at 16 MHz / (8 × 17) = 117647 baud, not the 115200 it is named for.
// The UNO's own USB chip makes the same rate from the same divider, but a CH34x makes an exact
// 115200: 2.1 % off, which a clone's ceramic resonator can push past what the UART tolerates,
// so pages fail mid-transfer ("programmer is not responding"). Asking the CH34x for the
// bootloader's real rate removes the mismatch.
var bootloaderRateChips = map[string]bool{"CH340": true, "CH341": true, "CH9102": true}

func uploadSpeedArguments(device Device) []string {
	if bootloaderRateChips[device.Chip] {
		return []string{"--upload-property", "upload.speed=117647"}
	}
	return nil
}

// bundledLibraries lists the Arduino libraries shipped in the libraries folder beside the
// engine, which compiles pass with --library so they win over any the teacher installed. Only
// that folder is read, never one a request names. A missing folder is a broken install.
func bundledLibraries(engineDir string) ([]string, error) {
	root := filepath.Join(engineDir, "libraries")
	entries, err := os.ReadDir(root)
	if err != nil {
		return nil, fmt.Errorf("the engine's bundled Arduino libraries are missing: %w", err)
	}
	var libraries []string
	for _, entry := range entries {
		// Only real folders: a link could hand the compiler sources from outside the install.
		if entry.IsDir() {
			libraries = append(libraries, filepath.Join(root, entry.Name()))
		}
	}
	if len(libraries) == 0 {
		return nil, fmt.Errorf("the engine's bundled Arduino libraries folder is empty: %s", root)
	}
	return libraries, nil
}

// buildArduino compiles, and for upload sends with avrdude's read-back verify, using the
// arduino-cli findArduinoCLI picked. uploaded is set only after that verify succeeded.
func buildArduino(result BuildResult, fqbn string, input ProgramInput, upload bool) (BuildResult, error) {
	cli, err := findArduinoCLI()
	if err != nil {
		return result, &ProtocolError{"TOOLCHAIN_MISSING", err.Error()}
	}
	engine, err := os.Executable()
	if err != nil {
		return result, err
	}
	libraries, err := bundledLibraries(filepath.Dir(engine))
	if err != nil {
		return result, err
	}
	// Checked before the compile so a wrong port fails at once, and again before sending.
	if upload {
		if _, err := checkBoardPort(input.Port, input.VID, input.PID); err != nil {
			return result, err
		}
	}
	workspace, err := openArduinoWorkspace(os.TempDir(), fqbn)
	if err != nil {
		return result, err
	}
	defer workspace.release()
	if err := os.WriteFile(filepath.Join(workspace.sketch, "program.ino"), []byte(input.Source), 0o600); err != nil {
		return result, err
	}
	compile := cli.arguments("compile", "--fqbn", fqbn, "--build-path", workspace.build)
	for _, library := range libraries {
		compile = append(compile, "--library", library)
	}
	compile = append(compile, workspace.sketch)
	compileTimeout := arduinoCompileOnlyTimeout
	if upload {
		compileTimeout = arduinoCompileTimeout
	}
	if _, err := os.Stat(workspace.marker); err == nil {
		if err := dropUnfinishedBuild(workspace.build); err != nil {
			return result, err
		}
	}
	if err := os.WriteFile(workspace.marker, nil, 0o600); err != nil {
		return result, err
	}
	started := time.Now()
	output, err := runArduinoCLI(cli, compile, compileTimeout)
	result.Output = output
	// After a timeout the marker stays, and the next compile drops what this one cut short.
	if isTimeout(err) {
		return result, err
	}
	if removeErr := os.Remove(workspace.marker); removeErr != nil {
		return result, removeErr
	}
	if err != nil {
		return result, compileFailure(cli, fqbn, err, output, time.Since(started))
	}
	result.Compiled = true
	if upload {
		device, err := checkBoardPort(input.Port, input.VID, input.PID)
		if err != nil {
			return result, err
		}
		send := cli.arguments("upload", "--fqbn", fqbn, "--input-dir", workspace.build,
			"--port", input.Port, "--protocol", "serial", "--verify")
		send = append(send, uploadSpeedArguments(device)...)
		send = append(send, workspace.sketch)
		output, err := runArduinoCLI(cli, send, arduinoUploadTimeout)
		result.Output += output
		if err != nil {
			if isTimeout(err) {
				return result, &ProtocolError{"ENGINE_TIMEOUT", err.Error() +
					"; the board may hold a partial program until it is uploaded again"}
			}
			if !isExit(err) {
				return result, err
			}
			return result, &ProtocolError{"UPLOAD_FAILED", "arduino-cli upload failed (" + err.Error() + "):\n" + output}
		}
		result.Uploaded = true
	}
	result.Delivery = "native"
	return result, nil
}

func runArduinoCLI(cli arduinoCLI, arguments []string, timeout time.Duration) (string, error) {
	output := &limitedOutput{}
	// Both streams share the same bounded writer; os/exec serialises its writes.
	err := execTool(cli.path, arguments, cli.env, os.TempDir(), timeout, output, output)
	return string(output.data), err
}

// A missing core fails the compile within seconds, so only a compile that failed that fast asks
// about it: the question can take arduinoProbeTimeout, which after a compile that failed near its
// deadline would push the request past the host's two minutes.
const missingCoreFailsWithin = 30 * time.Second

// A failed compile is the child's program unless the AVR core itself is missing, which only the
// core's absence on disk can tell; asking costs a settings read only after a failure.
func compileFailure(cli arduinoCLI, fqbn string, err error, output string, took time.Duration) error {
	if !isExit(err) {
		return err
	}
	if strings.HasPrefix(fqbn, "arduino:avr:") && took < missingCoreFailsWithin {
		if status := arduinoStatusOf(cli); status.ProblemCode == "" && !status.AVRCore {
			return &ProtocolError{"TOOLCHAIN_MISSING",
				"the Arduino AVR Boards core (arduino:avr) is not installed: open Arduino IDE once to install it"}
		}
	}
	return &ProtocolError{"COMPILE_FAILED", "arduino-cli compile failed (" + err.Error() + "):\n" + output}
}

func isExit(err error) bool {
	var exit *exec.ExitError
	return errors.As(err, &exit)
}

func isTimeout(err error) bool {
	var problem *ProtocolError
	return errors.As(err, &problem) && problem.Code == "ENGINE_TIMEOUT"
}
