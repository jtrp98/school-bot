package main

import (
	"bytes"
	"encoding/json"
	"fmt"
	"io"
	"unicode/utf8"
)

const protocol = "1.0"
const maxRequestBytes = 65536

type Request struct {
	Protocol  string          `json:"protocol"`
	RequestID string          `json:"request_id"`
	Mode      string          `json:"mode"`
	Action    string          `json:"action"`
	Data      json.RawMessage `json:"data"`
}

type Response struct {
	Protocol  string          `json:"protocol"`
	RequestID string          `json:"request_id"`
	Status    string          `json:"status"`
	Data      json.RawMessage `json:"data,omitempty"`
	Error     *ProtocolError  `json:"error,omitempty"`
}

type ProtocolError struct {
	Code    string `json:"code"`
	Message string `json:"message"`
}

func (err *ProtocolError) Error() string { return err.Message }

func failure(id, code, message string) Response {
	return Response{Protocol: protocol, RequestID: id, Status: "error", Error: &ProtocolError{code, message}}
}

func success[T interface{}](id string, data T) Response {
	encoded, err := json.Marshal(data)
	if err != nil {
		return failure(id, "ENGINE_FAILED", "could not encode engine response")
	}
	return Response{Protocol: protocol, RequestID: id, Status: "success", Data: encoded}
}

func decode[T interface{}](data []byte, target *T) error {
	decoder := json.NewDecoder(bytes.NewReader(data))
	decoder.DisallowUnknownFields()
	if err := decoder.Decode(target); err != nil {
		return err
	}
	if err := decoder.Decode(new(json.RawMessage)); err != io.EOF {
		return fmt.Errorf("expected one JSON object")
	}
	return nil
}

func handleLine(line []byte) Response {
	if !utf8.Valid(line) {
		return failure("", "PROTOCOL_INVALID", "expected UTF-8 JSON")
	}
	var request Request
	if err := decode(line, &request); err != nil {
		return failure("", "PROTOCOL_INVALID", "invalid request envelope")
	}
	if request.Protocol != protocol || len(request.RequestID) == 0 || len(request.RequestID) > 128 {
		return failure("", "PROTOCOL_INVALID", "expected protocol 1.0 and a request_id of 1..128 bytes")
	}
	if request.Mode != "block-programming" && request.Mode != "coding-adventure" {
		return failure(request.RequestID, "INVALID_REQUEST", "unsupported mode")
	}
	if len(request.Action) == 0 || len(request.Action) > 128 {
		return failure(request.RequestID, "INVALID_REQUEST", "invalid action")
	}
	data := bytes.TrimSpace(request.Data)
	if len(data) == 0 || data[0] != '{' {
		return failure(request.RequestID, "INVALID_INPUT", "data must be an object")
	}
	return dispatch(request)
}
