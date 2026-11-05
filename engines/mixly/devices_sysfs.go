package main

import (
	"os"
	"path/filepath"
	"strings"

	"go.bug.st/serial/enumerator"
)

// sysfsPorts lists USB serial ports from Linux sysfs (root is /sys/class/tty) without opening
// any. It builds on every platform so its test runs where the engine is developed.
func sysfsPorts(root string) ([]*enumerator.PortDetails, error) {
	entries, err := os.ReadDir(root)
	if err != nil {
		return nil, err
	}
	ports := []*enumerator.PortDetails{}
	for _, entry := range entries {
		// An UNO is a CDC device (ttyACM); the bridges on clones are USB-serial ports (ttyUSB).
		name := entry.Name()
		if !strings.HasPrefix(name, "ttyACM") && !strings.HasPrefix(name, "ttyUSB") {
			continue
		}
		port := &enumerator.PortDetails{Name: "/dev/" + name}
		if usb, ok := usbDeviceDir(filepath.Join(root, name, "device")); ok {
			port.IsUSB = true
			port.VID = sysfsValue(usb, "idVendor")
			port.PID = sysfsValue(usb, "idProduct")
			port.SerialNumber = sysfsValue(usb, "serial")
		}
		ports = append(ports, port)
	}
	return ports, nil
}

// usbDeviceDir climbs from a tty's device to the USB device that holds its IDs: one level up
// from a CDC interface, two from a bridge's port.
func usbDeviceDir(device string) (string, bool) {
	dir, err := filepath.EvalSymlinks(device)
	if err != nil {
		return "", false
	}
	for range 3 {
		if _, err := os.Stat(filepath.Join(dir, "idVendor")); err == nil {
			return dir, true
		}
		dir = filepath.Dir(dir)
	}
	return "", false
}

// A missing file (CH340 bridges have no serial number) reads as empty.
func sysfsValue(dir, name string) string {
	value, _ := os.ReadFile(filepath.Join(dir, name))
	return strings.TrimSpace(string(value))
}
