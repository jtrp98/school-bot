package main

import (
	"os"
	"path/filepath"
	"testing"

	"go.bug.st/serial/enumerator"
)

// Block Lab shows a card only for an UNO or a bridge UNO clones use; the rest are listed apart.
func TestDevicesAreSortedIntoUnoBridgeAndOther(t *testing.T) {
	for _, c := range []struct {
		port       enumerator.PortDetails
		kind, chip string
	}{
		{enumerator.PortDetails{Name: "COM3", IsUSB: true, VID: "2341", PID: "0043", SerialNumber: "857"}, "uno", ""},
		{enumerator.PortDetails{Name: "COM4", IsUSB: true, VID: "2a03", PID: "0043"}, "uno", ""},
		{enumerator.PortDetails{Name: "COM5", IsUSB: true, VID: "1a86", PID: "7523"}, "usb-serial", "CH340"},
		{enumerator.PortDetails{Name: "COM6", IsUSB: true, VID: "10C4", PID: "EA60"}, "usb-serial", "CP210x"},
		{enumerator.PortDetails{Name: "COM7", IsUSB: true, VID: "046D", PID: "C52B"}, "other", ""},
		// Without USB, IDs the OS reports are not trusted to name a board.
		{enumerator.PortDetails{Name: "COM1", VID: "2341", PID: "0043"}, "other", ""},
	} {
		device := describe(&c.port)
		if device.Port != c.port.Name || device.Kind != c.kind || device.Chip != c.chip {
			t.Errorf("%s = %+v, want %s %s", c.port.Name, device, c.kind, c.chip)
		}
		if !c.port.IsUSB && (device.VID != "" || device.PID != "") {
			t.Errorf("%s kept IDs without USB: %+v", c.port.Name, device)
		}
	}
	lower := enumerator.PortDetails{Name: "COM5", IsUSB: true, VID: "1a86", PID: "7523"}
	if device := describe(&lower); device.VID != "1A86" {
		t.Errorf("VID not upper-case: %+v", device)
	}
}

// On Linux the list comes from sysfs alone: only USB serial ports, IDs read from the USB device
// above each tty, and no /dev node opened.
func TestSysfsPortsReadUSBIDsOfACMAndUSBPorts(t *testing.T) {
	sys := t.TempDir()
	write := func(dir string, files map[string]string) {
		if err := os.MkdirAll(dir, 0o755); err != nil {
			t.Fatal(err)
		}
		for name, value := range files {
			if err := os.WriteFile(filepath.Join(dir, name), []byte(value+"\n"), 0o644); err != nil {
				t.Fatal(err)
			}
		}
	}
	link := func(target, tty string) {
		dir := filepath.Join(sys, "class", "tty", tty)
		write(dir, nil)
		if err := os.Symlink(target, filepath.Join(dir, "device")); err != nil {
			t.Skipf("symlinks unavailable: %v", err)
		}
	}
	uno := filepath.Join(sys, "devices", "usb1", "1-1")
	write(uno, map[string]string{"idVendor": "2341", "idProduct": "0043", "serial": "857"})
	write(filepath.Join(uno, "1-1.0"), nil)
	clone := filepath.Join(sys, "devices", "usb1", "1-2")
	write(clone, map[string]string{"idVendor": "1a86", "idProduct": "7523"})
	write(filepath.Join(clone, "1-2.0", "ttyUSB0"), nil)
	link(filepath.Join(uno, "1-1.0"), "ttyACM0")
	link(filepath.Join(clone, "1-2.0", "ttyUSB0"), "ttyUSB0")
	write(filepath.Join(sys, "class", "tty", "ttyS0"), nil)

	ports, err := sysfsPorts(filepath.Join(sys, "class", "tty"))
	if err != nil {
		t.Fatal(err)
	}
	if len(ports) != 2 {
		t.Fatalf("ports = %+v, want ttyACM0 and ttyUSB0 only", ports)
	}
	if got := describe(ports[0]); got.Port != "/dev/ttyACM0" || got.Kind != "uno" || got.SerialNumber != "857" {
		t.Errorf("ttyACM0 = %+v", got)
	}
	if got := describe(ports[1]); got.Port != "/dev/ttyUSB0" || got.Chip != "CH340" || got.SerialNumber != "" {
		t.Errorf("ttyUSB0 = %+v", got)
	}
}
