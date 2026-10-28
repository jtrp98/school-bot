package project

import (
	"encoding/json"
	"fmt"
)

type rawBlocklyBlock struct {
	Type   string                     `json:"type"`
	ID     string                     `json:"id"`
	X      *float64                   `json:"x,omitempty"`
	Y      *float64                   `json:"y,omitempty"`
	Fields map[string]json.RawMessage `json:"fields,omitempty"`
	Inputs map[string]struct {
		Block *rawBlocklyBlock `json:"block,omitempty"`
	} `json:"inputs,omitempty"`
	Next *struct {
		Block *rawBlocklyBlock `json:"block,omitempty"`
	} `json:"next,omitempty"`
}

func convertLegacyBlock(data []byte) (*ProjectContent, string, error) {
	var payload struct {
		Schema    *int `json:"schema"`
		Workspace *struct {
			Blocks *struct {
				Blocks []rawBlocklyBlock `json:"blocks"`
			} `json:"blocks"`
		} `json:"workspace"`
	}
	if err := json.Unmarshal(data, &payload); err != nil {
		return nil, "", fmt.Errorf("invalid block workspace JSON: %v", err)
	}
	if payload.Schema == nil || *payload.Schema != 1 || payload.Workspace == nil {
		return nil, "", fmt.Errorf("legacy block schema must be 1 with workspace")
	}

	var stacks []Stack
	if payload.Workspace.Blocks != nil {
		for _, rawTop := range payload.Workspace.Blocks.Blocks {
			x, y := 0.0, 0.0
			if rawTop.X != nil {
				x = *rawTop.X
			}
			if rawTop.Y != nil {
				y = *rawTop.Y
			}

			blocks, err := parseBlocklyChain(&rawTop, 0)
			if err != nil {
				return nil, "", err
			}
			stacks = append(stacks, Stack{
				X:      x,
				Y:      y,
				Blocks: blocks,
			})
		}
	}

	return &ProjectContent{
		Kind:   ModeBlock,
		Stacks: stacks,
	}, "Block Programming", nil
}

func parseBlocklyChain(raw *rawBlocklyBlock, depth int) ([]Block, error) {
	var chain []Block
	curr := raw

	for curr != nil {
		if depth > 8 {
			return nil, fmt.Errorf("nesting depth exceeds limit of 8")
		}
		bID := curr.ID
		if !IsValidUUID(bID) {
			bID = DeterministicUUID(SchoolBotNamespace, "block:"+curr.ID)
		}

		var converted Block
		switch curr.Type {
		case "arm_move":
			var dir string
			if dRaw, ok := curr.Fields["DIRECTION"]; ok {
				_ = json.Unmarshal(dRaw, &dir)
			}
			if dir != "move_right" && dir != "move_left" {
				return nil, fmt.Errorf("invalid move direction %q", dir)
			}
			var tiles int
			tRaw, ok := curr.Fields["TILES"]
			if !ok {
				return nil, fmt.Errorf("missing tiles field in move block")
			}
			var f float64
			if err := json.Unmarshal(tRaw, &f); err != nil {
				return nil, fmt.Errorf("invalid tiles value")
			}
			tiles = int(f)
			if tiles < 1 || tiles > 10 {
				return nil, fmt.Errorf("tiles %d must be in 1..10", tiles)
			}
			converted = Block{
				ID:        bID,
				Enabled:   true,
				Type:      "move",
				Direction: dir,
				Tiles:     tiles,
			}

		case "arm_jump":
			converted = Block{
				ID:      bID,
				Enabled: true,
				Type:    "jump",
			}

		case "arm_repeat":
			var times int
			tRaw, ok := curr.Fields["TIMES"]
			if !ok {
				return nil, fmt.Errorf("missing times field in repeat block")
			}
			var f float64
			if err := json.Unmarshal(tRaw, &f); err != nil {
				return nil, fmt.Errorf("invalid times value")
			}
			times = int(f)
			if times < 1 || times > 10 {
				return nil, fmt.Errorf("times %d must be in 1..10", times)
			}

			var body []Block
			if curr.Inputs != nil {
				if doInput, ok := curr.Inputs["DO"]; ok && doInput.Block != nil {
					nested, err := parseBlocklyChain(doInput.Block, depth+1)
					if err != nil {
						return nil, err
					}
					body = nested
				}
			}

			converted = Block{
				ID:      bID,
				Enabled: true,
				Type:    "repeat",
				Times:   times,
				Body:    body,
			}

		default:
			return nil, fmt.Errorf("unsupported block type %q", curr.Type)
		}

		chain = append(chain, converted)
		if curr.Next != nil {
			curr = curr.Next.Block
		} else {
			curr = nil
		}
	}

	return chain, nil
}
