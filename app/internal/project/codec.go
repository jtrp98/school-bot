package project

import (
	"bytes"
	"encoding/json"
	"fmt"
	"io"
	"math"
	"time"
	"unicode/utf8"
)

func validateStrictJSONStructure(data []byte) error {
	if !utf8.Valid(data) {
		return &StoreError{Code: ErrProjectInvalid, Message: "invalid UTF-8"}
	}
	dec := json.NewDecoder(bytes.NewReader(data))
	dec.UseNumber()

	var stack []map[string]bool
	inKey := false

	for {
		tok, err := dec.Token()
		if err == io.EOF {
			break
		}
		if err != nil {
			return &StoreError{Code: ErrProjectInvalid, Message: fmt.Sprintf("invalid JSON syntax: %v", err)}
		}

		switch t := tok.(type) {
		case json.Delim:
			switch t {
			case '{':
				stack = append(stack, make(map[string]bool))
				inKey = true
			case '}':
				if len(stack) == 0 {
					return &StoreError{Code: ErrProjectInvalid, Message: "unmatched closing brace"}
				}
				stack = stack[:len(stack)-1]
				inKey = len(stack) > 0
			case '[':
				inKey = false
			case ']':
				inKey = len(stack) > 0
			}
		case string:
			if len(stack) > 0 && inKey {
				seen := stack[len(stack)-1]
				if seen[t] {
					return &StoreError{Code: ErrProjectInvalid, Message: fmt.Sprintf("duplicate key %q", t)}
				}
				seen[t] = true
				inKey = false
			} else if len(stack) > 0 {
				inKey = true
			}
		default:
			if len(stack) > 0 {
				inKey = true
			}
		}

		if len(stack) == 0 {
			var extra json.RawMessage
			if err := dec.Decode(&extra); err == nil {
				return &StoreError{Code: ErrProjectInvalid, Message: "multiple JSON documents"}
			}
			break
		}
	}
	return nil
}

func decodeStrictDocument(data []byte, expectedID string) (*LocalProjectDocument, error) {
	if len(data) > 1<<20 {
		return nil, &StoreError{Code: ErrProjectTooLarge, Message: "project document exceeds 1 MiB limit"}
	}
	if err := validateStrictJSONStructure(data); err != nil {
		return nil, err
	}

	var rawRoot map[string]json.RawMessage
	if err := json.Unmarshal(data, &rawRoot); err != nil {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: fmt.Sprintf("invalid JSON: %v", err)}
	}

	requiredRoot := []string{"schema", "project_id", "mode", "name", "local_revision", "updated_at", "content"}
	if len(rawRoot) != len(requiredRoot) {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: "unknown or missing root fields"}
	}
	for _, req := range requiredRoot {
		b, ok := rawRoot[req]
		if !ok || bytes.Equal(bytes.TrimSpace(b), []byte("null")) {
			return nil, &StoreError{Code: ErrProjectInvalid, Message: fmt.Sprintf("missing or null field %q", req)}
		}
	}

	var schema int
	if err := json.Unmarshal(rawRoot["schema"], &schema); err != nil || schema != 2 {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: "schema must be 2"}
	}

	var projectID string
	if err := json.Unmarshal(rawRoot["project_id"], &projectID); err != nil || !IsValidUUID(projectID) {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: "project_id must be valid lowercase UUID"}
	}
	if expectedID != "" && projectID != expectedID {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: fmt.Sprintf("project_id %q does not match filename UUID %q", projectID, expectedID)}
	}

	var mode string
	if err := json.Unmarshal(rawRoot["mode"], &mode); err != nil || !ValidMode(mode) {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: fmt.Sprintf("invalid mode %q", mode)}
	}

	var name string
	if err := json.Unmarshal(rawRoot["name"], &name); err != nil {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: "invalid name"}
	}
	nr := len([]rune(name))
	if nr < 1 || nr > 200 {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: "name length must be 1..200"}
	}

	var rev int64
	if err := json.Unmarshal(rawRoot["local_revision"], &rev); err != nil || rev < 1 {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: "local_revision must be >= 1"}
	}

	var updated string
	if err := json.Unmarshal(rawRoot["updated_at"], &updated); err != nil {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: "invalid updated_at"}
	}
	if _, err := time.Parse(time.RFC3339, updated); err != nil {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: "updated_at must be RFC3339"}
	}

	content, err := decodeStrictContent(mode, rawRoot["content"])
	if err != nil {
		return nil, err
	}

	doc := &LocalProjectDocument{
		Schema:        schema,
		ProjectID:     projectID,
		Mode:          mode,
		Name:          name,
		LocalRevision: rev,
		UpdatedAt:     updated,
		Content:       content,
	}
	return doc, nil
}

func decodeStrictContent(mode string, raw json.RawMessage) (*ProjectContent, error) {
	var m map[string]json.RawMessage
	if err := json.Unmarshal(raw, &m); err != nil {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: "content must be object"}
	}

	kindRaw, ok := m["kind"]
	if !ok {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: "content missing kind"}
	}
	var kind string
	if err := json.Unmarshal(kindRaw, &kind); err != nil || kind != mode {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: fmt.Sprintf("content kind %q does not match %q", kind, mode)}
	}

	switch mode {
	case ModeCode:
		if len(m) != 2 {
			return nil, &StoreError{Code: ErrProjectInvalid, Message: "code content must contain exactly kind and source"}
		}
		srcRaw, ok := m["source"]
		if !ok || bytes.Equal(bytes.TrimSpace(srcRaw), []byte("null")) {
			return nil, &StoreError{Code: ErrProjectInvalid, Message: "missing or null source"}
		}
		var src string
		if err := json.Unmarshal(srcRaw, &src); err != nil {
			return nil, &StoreError{Code: ErrProjectInvalid, Message: "source must be string"}
		}
		if UTF16CodeUnitLength(src) > 16384 {
			return nil, &StoreError{Code: ErrProjectTooLarge, Message: "source exceeds 16384 UTF-16 units"}
		}
		return &ProjectContent{Kind: ModeCode, Source: src}, nil

	case ModeBlock:
		if len(m) != 2 {
			return nil, &StoreError{Code: ErrProjectInvalid, Message: "block content must contain exactly kind and stacks"}
		}
		stacksRaw, ok := m["stacks"]
		if !ok || bytes.Equal(bytes.TrimSpace(stacksRaw), []byte("null")) {
			return nil, &StoreError{Code: ErrProjectInvalid, Message: "missing or null stacks"}
		}
		var rawStacks []json.RawMessage
		if err := json.Unmarshal(stacksRaw, &rawStacks); err != nil {
			return nil, &StoreError{Code: ErrProjectInvalid, Message: "stacks must be array"}
		}
		stacks := make([]Stack, 0, len(rawStacks))
		for _, rs := range rawStacks {
			stk, err := decodeStrictStack(rs)
			if err != nil {
				return nil, err
			}
			stacks = append(stacks, *stk)
		}
		c := &ProjectContent{Kind: ModeBlock, Stacks: stacks}
		if err := validateBlockContent(c); err != nil {
			return nil, err
		}
		return c, nil

	case ModeArm:
		return decodeStrictArm(m)

	default:
		return nil, &StoreError{Code: ErrProjectInvalid, Message: fmt.Sprintf("unsupported mode %q", mode)}
	}
}

func decodeStrictStack(raw json.RawMessage) (*Stack, error) {
	var m map[string]json.RawMessage
	if err := json.Unmarshal(raw, &m); err != nil {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: "stack must be object"}
	}
	if len(m) != 3 {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: "stack must contain exactly x, y, blocks"}
	}
	for _, f := range []string{"x", "y", "blocks"} {
		if b, ok := m[f]; !ok || bytes.Equal(bytes.TrimSpace(b), []byte("null")) {
			return nil, &StoreError{Code: ErrProjectInvalid, Message: fmt.Sprintf("stack missing/null %s", f)}
		}
	}
	var x, y float64
	if err := json.Unmarshal(m["x"], &x); err != nil || math.IsNaN(x) || math.IsInf(x, 0) {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: "invalid stack x"}
	}
	if err := json.Unmarshal(m["y"], &y); err != nil || math.IsNaN(y) || math.IsInf(y, 0) {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: "invalid stack y"}
	}
	var rawBlocks []json.RawMessage
	if err := json.Unmarshal(m["blocks"], &rawBlocks); err != nil {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: "blocks must be array"}
	}
	blocks := make([]Block, 0, len(rawBlocks))
	for _, rb := range rawBlocks {
		blk, err := decodeStrictBlock(rb)
		if err != nil {
			return nil, err
		}
		blocks = append(blocks, *blk)
	}
	return &Stack{X: x, Y: y, Blocks: blocks}, nil
}

func decodeStrictBlock(raw json.RawMessage) (*Block, error) {
	var m map[string]json.RawMessage
	if err := json.Unmarshal(raw, &m); err != nil {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: "block must be object"}
	}
	for _, f := range []string{"id", "enabled", "type"} {
		if b, ok := m[f]; !ok || bytes.Equal(bytes.TrimSpace(b), []byte("null")) {
			return nil, &StoreError{Code: ErrProjectInvalid, Message: fmt.Sprintf("block missing/null %s", f)}
		}
	}
	var id, bType string
	var enabled bool
	if err := json.Unmarshal(m["id"], &id); err != nil || !IsValidUUID(id) {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: "block id must be UUID"}
	}
	if err := json.Unmarshal(m["enabled"], &enabled); err != nil {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: "block enabled must be boolean"}
	}
	if err := json.Unmarshal(m["type"], &bType); err != nil {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: "block type must be string"}
	}

	switch bType {
	case "move":
		if len(m) != 5 {
			return nil, &StoreError{Code: ErrProjectInvalid, Message: "move block must have exactly id, enabled, type, direction, tiles"}
		}
		var dir string
		var tiles int
		if err := json.Unmarshal(m["direction"], &dir); err != nil || (dir != "move_right" && dir != "move_left") {
			return nil, &StoreError{Code: ErrProjectInvalid, Message: "invalid move direction"}
		}
		if err := json.Unmarshal(m["tiles"], &tiles); err != nil || tiles < 1 || tiles > 10 {
			return nil, &StoreError{Code: ErrProjectInvalid, Message: "move tiles must be 1..10"}
		}
		return &Block{ID: id, Enabled: enabled, Type: bType, Direction: dir, Tiles: tiles}, nil

	case "jump":
		if len(m) != 3 {
			return nil, &StoreError{Code: ErrProjectInvalid, Message: "jump block must have exactly id, enabled, type"}
		}
		return &Block{ID: id, Enabled: enabled, Type: bType}, nil

	case "repeat":
		if len(m) != 5 {
			return nil, &StoreError{Code: ErrProjectInvalid, Message: "repeat block must have exactly id, enabled, type, times, body"}
		}
		var times int
		if err := json.Unmarshal(m["times"], &times); err != nil || times < 1 || times > 10 {
			return nil, &StoreError{Code: ErrProjectInvalid, Message: "repeat times must be 1..10"}
		}
		var rawBody []json.RawMessage
		if err := json.Unmarshal(m["body"], &rawBody); err != nil {
			return nil, &StoreError{Code: ErrProjectInvalid, Message: "repeat body must be array"}
		}
		body := make([]Block, 0, len(rawBody))
		for _, rb := range rawBody {
			child, err := decodeStrictBlock(rb)
			if err != nil {
				return nil, err
			}
			body = append(body, *child)
		}
		return &Block{ID: id, Enabled: enabled, Type: bType, Times: times, Body: body}, nil

	default:
		return nil, &StoreError{Code: ErrProjectInvalid, Message: fmt.Sprintf("unsupported block type %q", bType)}
	}
}
