package main

import (
	"os"
	"path/filepath"
	"strings"
	"testing"
)

func TestRunRejectsOversizeAndMalformedInputBeforeManager(t *testing.T) {
	service := &DesktopService{}
	for _, data := range []string{strings.Repeat("x", 65537), "{"} {
		if _, err := service.Run("robot-control", "robot.stop", data); err == nil {
			t.Fatal("invalid input reached the manager")
		}
	}
}

func TestProjectReadBoundaries(t *testing.T) {
	service := &DesktopService{dataDir: t.TempDir()}
	path := filepath.Join(service.dataDir, "robot-control.json")
	for _, size := range []int{1 << 20, (1 << 20) + 1} {
		content := `{}` + strings.Repeat(" ", size-2)
		if err := os.WriteFile(path, []byte(content), 0600); err != nil {
			t.Fatal(err)
		}
		data, err := service.LoadProject("robot-control")
		if size == 1<<20 && (err != nil || data != content) {
			t.Fatalf("exact limit rejected: %v", err)
		}
		if size > 1<<20 && (err == nil || data != "") {
			t.Fatal("oversized project accepted")
		}
	}
}

func TestCurrentDeviceIDRequiresHex(t *testing.T) {
	for _, value := range []string{strings.Repeat("a", 32), strings.Repeat("F", 32)} {
		if !validDeviceID([]byte(value)) {
			t.Fatal("valid device ID rejected")
		}
	}
	for _, value := range []string{strings.Repeat("z", 32), strings.Repeat("a", 31), ""} {
		if validDeviceID([]byte(value)) {
			t.Fatal("invalid device ID accepted")
		}
	}
}
