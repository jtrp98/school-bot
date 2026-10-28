package engine

import (
	"encoding/json"
	"fmt"
	"regexp"
	"strconv"
	"strings"
)

var instruction = regexp.MustCompile(`^hero\.(move_right|move_left|jump)\(([0-9]*)\)$`)

// Parse a bounded DSL so student source never reaches eval, a shell, or a compiler.
func HandleDemo(req Request) Response {
	res := Response{Protocol: Protocol, RequestID: req.RequestID, Status: "error"}
	fail := func(code, message string) Response {
		res.Error = &Error{Code: code, Message: message}
		return res
	}
	if req.Protocol != Protocol || req.RequestID == "" {
		return fail("PROTOCOL_INVALID", "Expected protocol 1.0 and request_id")
	}
	switch req.Mode + "/" + req.Action {
	case "coding-adventure/program.run", "block-programming/program.run":
		var program Program
		if err := json.Unmarshal(req.Data, &program); err != nil {
			return fail("INVALID_INPUT", "Invalid program")
		}
		commands := program.Commands
		if req.Mode == "coding-adventure" {
			var err error
			commands, err = ParseSource(program.Source)
			if err != nil {
				return fail("PROGRAM_INVALID", err.Error())
			}
		}
		if len(commands) == 0 || len(commands) > 64 {
			return fail("STEP_LIMIT", "Use between 1 and 64 commands")
		}
		steps := 0
		for _, command := range commands {
			switch command.Op {
			case "move_right", "move_left":
				if command.Value < 1 || command.Value > 10 {
					return fail("STEP_LIMIT", "Movement must be between 1 and 10 tiles")
				}
				steps += command.Value
			case "jump":
				steps++
			default:
				return fail("COMMAND_UNKNOWN", "Only move_right, move_left and jump are supported")
			}
		}
		if steps > 100 {
			return fail("STEP_LIMIT", "Program exceeds 100 steps")
		}
		res.Data = map[string]any{"simulated": true, "commands": commands, "steps": steps}
	default:
		return fail("ACTION_UNKNOWN", "Unknown mode/action")
	}
	res.Status = "success"
	return res
}

func ParseSource(source string) ([]Command, error) {
	if len(source) > 16384 {
		return nil, fmt.Errorf("source is too large")
	}
	commands := []Command{}
	for index, line := range strings.Split(source, "\n") {
		line = strings.TrimSpace(line)
		if line == "" || strings.HasPrefix(line, "#") {
			continue
		}
		match := instruction.FindStringSubmatch(line)
		if match == nil {
			return nil, fmt.Errorf("line %d: use hero.move_right(n), hero.move_left(n), or hero.jump()", index+1)
		}
		command := Command{Op: match[1]}
		if command.Op == "jump" {
			if match[2] != "" {
				return nil, fmt.Errorf("line %d: jump takes no argument", index+1)
			}
		} else {
			value, err := strconv.Atoi(match[2])
			if err != nil {
				return nil, fmt.Errorf("line %d: movement needs an integer", index+1)
			}
			command.Value = value
		}
		commands = append(commands, command)
		if len(commands) > 64 {
			return nil, fmt.Errorf("program exceeds 64 commands")
		}
	}
	return commands, nil
}
