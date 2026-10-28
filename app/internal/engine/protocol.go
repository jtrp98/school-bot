package engine

import "encoding/json"

const Protocol = "1.0"

// Error codes of contracts/engine/response.schema.json. Callers decide by code, never by message text.
const (
	CodeInvalidRequest  = "INVALID_REQUEST"
	CodeEngineBusy      = "ENGINE_BUSY"
	CodeEngineCancelled = "ENGINE_CANCELLED"
	CodeEngineFailed    = "ENGINE_FAILED"
	CodeEngineTimeout   = "ENGINE_TIMEOUT"
)

// knownErrorCodes must stay equal to the enum in the response schema; a test compares them.
var knownErrorCodes = map[string]bool{
	"PROTOCOL_INVALID":  true,
	CodeInvalidRequest:  true,
	"INVALID_INPUT":     true,
	"ACTION_UNKNOWN":    true,
	"SAFETY_LIMIT":      true,
	"TRANSPORT_ERROR":   true,
	"DEVICE_TIMEOUT":    true,
	"PROJECT_INVALID":   true,
	CodeEngineBusy:      true,
	CodeEngineCancelled: true,
	CodeEngineFailed:    true,
	CodeEngineTimeout:   true,
	"PROGRAM_INVALID":   true,
	"COMMAND_UNKNOWN":   true,
	"STEP_LIMIT":        true,
}

type Request struct {
	Protocol  string          `json:"protocol"`
	RequestID string          `json:"request_id"`
	Mode      string          `json:"mode"`
	Action    string          `json:"action"`
	Data      json.RawMessage `json:"data"`
}

type Response struct {
	Protocol  string `json:"protocol"`
	RequestID string `json:"request_id"`
	Status    string `json:"status"`
	Data      any    `json:"data,omitempty"`
	Error     *Error `json:"error,omitempty"`
}

type Error struct {
	Code    string `json:"code"`
	Message string `json:"message"`
}

type Command struct {
	Op    string `json:"op"`
	Value int    `json:"value,omitempty"`
}

type Program struct {
	Source   string    `json:"source,omitempty"`
	Commands []Command `json:"commands,omitempty"`
}
