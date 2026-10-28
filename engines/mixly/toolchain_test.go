package main

import (
	"context"
	"os"
	"os/exec"
	"strings"
	"testing"
	"time"
)

func TestToolSubprocessHelper(t *testing.T) {
	if os.Getenv("MIXLY_TEST_TOOL") != "1" {
		return
	}
	if os.Getenv("MIXLY_TEST_SLEEP") == "1" {
		time.Sleep(10 * time.Second)
	}
	_, _ = os.Stdout.WriteString("tool-output")
	os.Exit(0)
}

func TestNativeToolOutputAndCancellation(t *testing.T) {
	executable, err := os.Executable()
	if err != nil {
		t.Fatal(err)
	}
	for _, sleeping := range []bool{false, true} {
		name := "output"
		if sleeping {
			name = "cancellation"
		}
		t.Run(name, func(t *testing.T) {
			timeout := 10 * time.Second
			if sleeping {
				timeout = time.Second
			}
			ctx, cancel := context.WithTimeout(context.Background(), timeout)
			defer cancel()
			command := exec.CommandContext(ctx, executable, "-test.run=^TestToolSubprocessHelper$")
			command.Env = append(os.Environ(), "MIXLY_TEST_TOOL=1")
			if sleeping {
				command.Env = append(command.Env, "MIXLY_TEST_SLEEP=1")
			}
			configureTool(command)
			var output strings.Builder
			command.Stdout = &output
			command.Stderr = &output
			started := time.Now()
			err := runToolProcess(command)
			if sleeping {
				if err == nil || ctx.Err() == nil || time.Since(started) > 5*time.Second {
					t.Fatalf("cancellation did not terminate the helper: %v", err)
				}
			} else if err != nil || output.String() != "tool-output" {
				t.Fatalf("%q: %v", output.String(), err)
			}
		})
	}
}

func TestFirmwareDryRunUsesOriginalPresets(t *testing.T) {
	for _, board := range []string{
		"micropython_esp32", "micropython_esp32c3", "micropython_esp32c3_mixgocar",
		"micropython_esp32s2", "micropython_k210_mixgoai",
	} {
		t.Run(board, func(t *testing.T) {
			result, err := flashFirmware(FirmwareInput{Board: board, Port: "no-device"})
			if err != nil || result.Steps < 1 || result.Delivery != "dry-run" || result.Flashed {
				t.Fatalf("%+v: %v", result, err)
			}
		})
	}
}
