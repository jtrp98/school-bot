package main

import (
	"encoding/base64"
	"fmt"
	"time"

	"go.bug.st/serial"
)

type SerialInput struct {
	Port        string `json:"port"`
	BaudRate    int    `json:"baud_rate,omitempty"`
	WriteBase64 string `json:"write_base64,omitempty"`
	ReadBytes   int    `json:"read_bytes,omitempty"`
	TimeoutMS   int    `json:"timeout_ms,omitempty"`
	Transport   string `json:"transport,omitempty"`
}

type SerialResult struct {
	Engine    string `json:"engine"`
	Delivery  string `json:"delivery"`
	Written   int    `json:"written"`
	Received  string `json:"received_base64,omitempty"`
	DeviceAck bool   `json:"device_acknowledged"`
}

type Devices struct {
	Engine string   `json:"engine"`
	Ports  []string `json:"ports"`
}

func listDevices() (Devices, error) {
	ports, err := serial.GetPortsList()
	if ports == nil {
		ports = []string{}
	}
	return Devices{"mixly", ports}, err
}

func checkPort(name string) error {
	devices, err := listDevices()
	if err != nil {
		return err
	}
	for _, port := range devices.Ports {
		if port == name {
			return nil
		}
	}
	return fmt.Errorf("selected serial port is not available")
}

func exchangeSerial(input SerialInput) (SerialResult, error) {
	result := SerialResult{Engine: "mixly", Delivery: "dry-run"}
	if input.Port == "" || len(input.Port) > 256 {
		return result, &ProtocolError{"INVALID_INPUT", "select a serial port"}
	}
	if input.Transport != "" && input.Transport != "dry-run" && input.Transport != "native" {
		return result, &ProtocolError{"INVALID_INPUT", "transport must be dry-run or native"}
	}
	if input.BaudRate == 0 {
		input.BaudRate = 115200
	}
	if input.TimeoutMS == 0 {
		input.TimeoutMS = 500
	}
	if input.BaudRate < 50 || input.BaudRate > 3000000 || input.TimeoutMS < 1 || input.TimeoutMS > 2000 ||
		input.ReadBytes < 0 || input.ReadBytes > 8192 {
		return result, &ProtocolError{"SAFETY_LIMIT", "serial parameters exceed engine limits"}
	}
	data, err := base64.StdEncoding.DecodeString(input.WriteBase64)
	if err != nil || len(data) > 1024 {
		return result, &ProtocolError{"INVALID_INPUT", "write_base64 must encode at most 1024 bytes"}
	}
	if input.Transport != "native" {
		return result, nil
	}
	if err := checkPort(input.Port); err != nil {
		return result, &ProtocolError{"TRANSPORT_ERROR", err.Error()}
	}
	port, err := serial.Open(input.Port, &serial.Mode{
		BaudRate: input.BaudRate, DataBits: 8, Parity: serial.NoParity, StopBits: serial.OneStopBit,
		InitialStatusBits: &serial.ModemOutputBits{DTR: false, RTS: false},
	})
	if err != nil {
		return result, &ProtocolError{"TRANSPORT_ERROR", err.Error()}
	}
	defer port.Close()
	if err := port.SetReadTimeout(time.Duration(input.TimeoutMS) * time.Millisecond); err != nil {
		return result, &ProtocolError{"TRANSPORT_ERROR", err.Error()}
	}
	if len(data) > 0 {
		result.Written, err = port.Write(data)
		if err != nil || result.Written != len(data) {
			return result, &ProtocolError{"TRANSPORT_ERROR", "serial write was incomplete"}
		}
	}
	if input.ReadBytes > 0 {
		buffer := make([]byte, input.ReadBytes)
		count, err := port.Read(buffer)
		if err != nil {
			return result, &ProtocolError{"TRANSPORT_ERROR", err.Error()}
		}
		result.Received = base64.StdEncoding.EncodeToString(buffer[:count])
	}
	result.Delivery = "native"
	return result, nil
}
