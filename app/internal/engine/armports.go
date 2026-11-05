package engine

import (
	"encoding/json"
	"strings"
	"sync"
)

// The robot arm's controller and Block Lab's clone UNOs can use the same CH340 bridge, which
// has no serial number, so a port list cannot tell them apart. The host remembers the serial
// ports Robot Control drove and refuses Block Lab uploads and Serial monitors on them, so a wrong
// pick never overwrites the arm's firmware or types into its controller. A port is forgotten once
// a Block Lab port list no longer shows it: its cable came out, and whatever is plugged in next is
// chosen again on the board screen.
type armPorts struct {
	mu    sync.Mutex
	ports map[string]bool
}

// Windows COM names ignore case.
func armPortKey(port string) string { return strings.ToUpper(port) }

func (a *armPorts) holds(port string) bool {
	a.mu.Lock()
	defer a.mu.Unlock()
	return port != "" && a.ports[armPortKey(port)]
}

// note updates the ports from a request the host just ran and the engine's answer.
func (a *armPorts) note(mode, action string, data json.RawMessage, res Response) {
	if res.Status != "success" {
		return
	}
	a.mu.Lock()
	defer a.mu.Unlock()
	switch {
	case mode == "robot-control":
		if exactText(data, "transport") == "serial" {
			if port := exactText(data, "port"); port != "" {
				if a.ports == nil {
					a.ports = map[string]bool{}
				}
				a.ports[armPortKey(port)] = true
			}
		}
	case mode == "block-programming" && action == "device.list":
		listed := listedPorts(res.Data)
		for port := range a.ports {
			if !listed[port] {
				delete(a.ports, port)
			}
		}
	}
}

// exactText reads a string field spelled exactly as the engines read it.
func exactText(data json.RawMessage, name string) string {
	var fields map[string]json.RawMessage
	if json.Unmarshal(data, &fields) != nil {
		return ""
	}
	var text string
	if json.Unmarshal(fields[name], &text) != nil {
		return ""
	}
	return text
}

// listedPorts reads the ports of a mixly device.list answer (engines/mixly/docs/API.md).
func listedPorts(data any) map[string]bool {
	listed := map[string]bool{}
	fields, _ := data.(map[string]any)
	devices, _ := fields["devices"].([]any)
	for _, device := range devices {
		entry, _ := device.(map[string]any)
		if port, ok := entry["port"].(string); ok {
			listed[armPortKey(port)] = true
		}
	}
	return listed
}
