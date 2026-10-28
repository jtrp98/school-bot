package main

import (
	_ "embed"
	"encoding/json"
	"errors"
	"fmt"
	"sort"
)

//go:embed engine.json
var manifest json.RawMessage

type ProgramInput struct {
	Board     string            `json:"board"`
	Platform  string            `json:"platform,omitempty"`
	Options   map[string]string `json:"options,omitempty"`
	XML       string            `json:"xml,omitempty"`
	Blocks    []*Block          `json:"blocks,omitempty"`
	Source    string            `json:"source,omitempty"`
	Port      string            `json:"port,omitempty"`
	Transport string            `json:"transport,omitempty"`
}

type GeneratedProgram struct {
	Engine   string `json:"engine"`
	Board    string `json:"board"`
	Platform string `json:"platform"`
	Language string `json:"language"`
	Code     string `json:"code"`
}

var actions = []string{
	"engine.info", "board.list", "board.info", "project.validate", "program.generate",
	"toolchain.status", "program.compile", "program.upload", "device.list", "serial.exchange",
	"firmware.flash",
}

func dispatch(request Request) Response {
	switch request.Action {
	case "engine.info", "board.list", "toolchain.status", "device.list":
		if err := decode(request.Data, &struct{}{}); err != nil {
			return failure(request.RequestID, "INVALID_INPUT", "this action requires empty data")
		}
		switch request.Action {
		case "engine.info":
			return success(request.RequestID, struct {
				Engine   string          `json:"engine"`
				Manifest json.RawMessage `json:"manifest"`
				Actions  []string        `json:"actions"`
			}{"mixly", manifest, actions})
		case "board.list":
			boards, err := catalog()
			if err != nil {
				return failure(request.RequestID, "ENGINE_FAILED", err.Error())
			}
			return success(request.RequestID, struct {
				Engine string  `json:"engine"`
				Boards []Board `json:"boards"`
			}{"mixly", boards})
		case "toolchain.status":
			return success(request.RequestID, toolchainStatus())
		case "device.list":
			devices, err := listDevices()
			if err != nil {
				return failure(request.RequestID, "TRANSPORT_ERROR", err.Error())
			}
			return success(request.RequestID, devices)
		}
	case "board.info":
		var input struct {
			Board string `json:"board"`
		}
		if err := decode(request.Data, &input); err != nil {
			return failure(request.RequestID, "INVALID_INPUT", err.Error())
		}
		board, ok := findBoard(input.Board)
		if !ok {
			return failure(request.RequestID, "INVALID_INPUT", "unknown board")
		}
		return success(request.RequestID, board)
	case "project.validate", "program.generate", "program.compile", "program.upload":
		var input ProgramInput
		if err := decode(request.Data, &input); err != nil {
			return failure(request.RequestID, "INVALID_INPUT", err.Error())
		}
		board, ok := findBoard(input.Board)
		if !ok {
			return failure(request.RequestID, "INVALID_INPUT", "unknown board")
		}
		if input.Transport == "native" && request.Action == "program.upload" &&
			len(board.Targets) > 1 && input.Platform == "" {
			return failure(request.RequestID, "INVALID_INPUT", "native upload requires an explicit platform")
		}
		if err := selectPlatform(board, &input); err != nil {
			return failure(request.RequestID, "INVALID_INPUT", err.Error())
		}
		if len(input.Options) > 0 {
			if board.Language != "arduino" {
				return failure(request.RequestID, "INVALID_INPUT", "board options are supported only for Arduino")
			}
			if _, err := arduinoTarget(board, input); err != nil {
				return failure(request.RequestID, "INVALID_INPUT", err.Error())
			}
		}
		if input.Source != "" {
			if request.Action != "program.compile" && request.Action != "program.upload" {
				return failure(request.RequestID, "INVALID_INPUT", "source is accepted only for compile or upload")
			}
			if input.XML != "" || input.Blocks != nil {
				return failure(request.RequestID, "INVALID_INPUT", "choose source, xml or blocks")
			}
		} else {
			if input.XML != "" && input.Blocks != nil {
				return failure(request.RequestID, "INVALID_INPUT", "choose xml or blocks")
			}
			if input.XML != "" {
				blocks, err := parseProject(input.XML)
				if err != nil {
					return failure(request.RequestID, "PROJECT_INVALID", err.Error())
				}
				input.Blocks = blocks
			}
			if err := validateBlocks(input.Blocks); err != nil {
				return failure(request.RequestID, "PROJECT_INVALID", err.Error())
			}
			code, err := generate(board, input.Platform, input.Blocks)
			if err != nil {
				return failure(request.RequestID, "PROGRAM_INVALID", err.Error())
			}
			input.Source = code
		}
		if request.Action == "program.generate" || request.Action == "project.validate" {
			return success(request.RequestID, GeneratedProgram{
				"mixly", board.ID, input.Platform, board.Language, input.Source,
			})
		}
		result, err := buildProgram(board, input, request.Action == "program.upload")
		if err != nil {
			return actionFailure(request.RequestID, err)
		}
		return success(request.RequestID, result)
	case "serial.exchange":
		var input SerialInput
		if err := decode(request.Data, &input); err != nil {
			return failure(request.RequestID, "INVALID_INPUT", err.Error())
		}
		result, err := exchangeSerial(input)
		if err != nil {
			return actionFailure(request.RequestID, err)
		}
		return success(request.RequestID, result)
	case "firmware.flash":
		var input FirmwareInput
		if err := decode(request.Data, &input); err != nil {
			return failure(request.RequestID, "INVALID_INPUT", err.Error())
		}
		result, err := flashFirmware(input)
		if err != nil {
			return actionFailure(request.RequestID, err)
		}
		return success(request.RequestID, result)
	}
	return failure(request.RequestID, "ACTION_UNKNOWN", "unknown Mixly action")
}

func actionFailure(id string, err error) Response {
	var problem *ProtocolError
	if errors.As(err, &problem) {
		return failure(id, problem.Code, problem.Message)
	}
	return failure(id, "ENGINE_FAILED", err.Error())
}

func selectPlatform(board Board, input *ProgramInput) error {
	if input.Platform == "" {
		platforms := make([]string, 0, len(board.Targets))
		for platform := range board.Targets {
			platforms = append(platforms, platform)
		}
		sort.Strings(platforms)
		if len(platforms) > 0 {
			input.Platform = platforms[0]
		}
	}
	if len(board.Targets) > 0 {
		if _, ok := board.Targets[input.Platform]; !ok {
			return fmt.Errorf("unknown platform for board %s", board.ID)
		}
	} else if input.Platform != "" {
		return fmt.Errorf("this board does not accept a platform")
	}
	return nil
}
