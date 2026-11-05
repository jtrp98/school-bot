package main

import (
	"fmt"
	"strings"

	"go.bug.st/serial/enumerator"
)

// Device is a serial port with what USB reports about it. Block Lab shows UNO boards from
// this alone. Listing reads SetupAPI and the registry on Windows, IOKit on macOS and sysfs on
// Linux and opens no port, so it never resets a board or touches a port another program (the
// robot arm) is using.
type Device struct {
	Port         string `json:"port"`
	USB          bool   `json:"usb"`
	VID          string `json:"vid,omitempty"`
	PID          string `json:"pid,omitempty"`
	SerialNumber string `json:"serial_number,omitempty"`
	// "uno" for Arduino's own UNO, "usb-serial" for a USB bridge UNO clones use (named by Chip),
	// "other" for anything else.
	Kind string `json:"kind"`
	Chip string `json:"chip,omitempty"`
}

type Devices struct {
	Engine  string   `json:"engine"`
	Ports   []string `json:"ports"`
	Devices []Device `json:"devices"`
}

// The UNO's USB IDs from the Arduino AVR core's boards.txt (uno.vid.N / uno.pid.N).
var unoUSBIDs = map[string]bool{"2341:0043": true, "2341:0001": true, "2A03:0043": true, "2341:0243": true}

// USB-to-serial bridges found on UNO clones. The same chips sit in other devices too, so a
// match means "may be an UNO", which the page says by naming the chip.
var bridgeChips = map[string]string{
	"1A86:7523": "CH340",
	"1A86:5523": "CH341",
	"1A86:55D4": "CH9102",
	"10C4:EA60": "CP210x",
	"0403:6001": "FTDI",
	"0403:6015": "FTDI",
}

func describe(port *enumerator.PortDetails) Device {
	device := Device{Port: port.Name, USB: port.IsUSB, Kind: "other"}
	if !port.IsUSB {
		return device
	}
	device.VID = strings.ToUpper(port.VID)
	device.PID = strings.ToUpper(port.PID)
	device.SerialNumber = port.SerialNumber
	id := device.VID + ":" + device.PID
	if unoUSBIDs[id] {
		device.Kind = "uno"
	} else if chip, ok := bridgeChips[id]; ok {
		device.Kind, device.Chip = "usb-serial", chip
	}
	return device
}

// A failed enumeration is an error, not a list of bare names: those would all read as "other",
// and the page would tell a child no board is plugged in while one is.
func listDevices() (result Devices, err error) {
	// The Windows enumerator indexes an empty slice when SetupAPI reports no port class, and a
	// panic would end the engine for every later request.
	defer func() {
		if recovered := recover(); recovered != nil {
			result, err = Devices{}, fmt.Errorf("listing serial ports failed: %v", recovered)
		}
	}()
	details, err := portDetails()
	if err != nil {
		return Devices{}, err
	}
	result = Devices{Engine: "mixly", Ports: []string{}, Devices: []Device{}}
	for _, port := range details {
		// macOS can report a port it could not name yet; there is nothing to pick or match.
		if port.Name == "" {
			continue
		}
		result.Ports = append(result.Ports, port.Name)
		result.Devices = append(result.Devices, describe(port))
	}
	return result, nil
}
