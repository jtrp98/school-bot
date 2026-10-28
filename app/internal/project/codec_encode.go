package project

import (
	"encoding/json"
	"fmt"
)

func encodeStrictDocument(doc *LocalProjectDocument) ([]byte, error) {
	if doc == nil {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: "nil document"}
	}
	contentData, err := encodeStrictContent(doc.Content)
	if err != nil {
		return nil, err
	}

	root := map[string]any{
		"schema":         doc.Schema,
		"project_id":     doc.ProjectID,
		"mode":           doc.Mode,
		"name":           doc.Name,
		"local_revision": doc.LocalRevision,
		"updated_at":     doc.UpdatedAt,
		"content":        json.RawMessage(contentData),
	}
	return json.MarshalIndent(root, "", "  ")
}

func encodeStrictContent(c *ProjectContent) ([]byte, error) {
	if c == nil {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: "nil content"}
	}
	switch c.Kind {
	case ModeCode:
		return json.Marshal(map[string]any{
			"kind":   ModeCode,
			"source": c.Source,
		})
	case ModeBlock:
		stks := c.Stacks
		if stks == nil {
			stks = []Stack{}
		}
		type encBlock struct {
			ID        string     `json:"id"`
			Enabled   bool       `json:"enabled"`
			Type      string     `json:"type"`
			Direction *string    `json:"direction,omitempty"`
			Tiles     *int       `json:"tiles,omitempty"`
			Times     *int       `json:"times,omitempty"`
			Body      []encBlock `json:"body,omitempty"`
		}
		type encStack struct {
			X      float64    `json:"x"`
			Y      float64    `json:"y"`
			Blocks []encBlock `json:"blocks"`
		}
		var convertBlocks func([]Block) []encBlock
		convertBlocks = func(blks []Block) []encBlock {
			if blks == nil {
				return []encBlock{}
			}
			res := make([]encBlock, len(blks))
			for i, b := range blks {
				eb := encBlock{ID: b.ID, Enabled: b.Enabled, Type: b.Type}
				switch b.Type {
				case "move":
					eb.Direction = &b.Direction
					eb.Tiles = &b.Tiles
				case "repeat":
					eb.Times = &b.Times
					eb.Body = convertBlocks(b.Body)
				}
				res[i] = eb
			}
			return res
		}
		convertedStacks := make([]encStack, len(stks))
		for i, s := range stks {
			convertedStacks[i] = encStack{
				X:      s.X,
				Y:      s.Y,
				Blocks: convertBlocks(s.Blocks),
			}
		}
		return json.Marshal(map[string]any{
			"kind":   ModeBlock,
			"stacks": convertedStacks,
		})
	case ModeArm:
		poses := c.Poses
		if poses == nil {
			poses = []Pose{}
		}
		order := c.Order
		if order == nil {
			order = []string{}
		}
		groups := c.Groups
		if groups == nil {
			groups = []PoseGroup{}
		}
		return json.Marshal(map[string]any{
			"kind":        ModeArm,
			"poses":       poses,
			"order":       order,
			"groups":      groups,
			"joint_names": c.JointNames,
			"time_mode":   c.TimeMode,
			"loops":       c.Loops,
			"view":        c.View,
		})
	default:
		return nil, &StoreError{Code: ErrProjectInvalid, Message: fmt.Sprintf("unsupported kind %q", c.Kind)}
	}
}

func (doc *LocalProjectDocument) MarshalJSON() ([]byte, error) {
	return encodeStrictDocument(doc)
}

func (doc *LocalProjectDocument) UnmarshalJSON(data []byte) error {
	d, err := decodeStrictDocument(data, "")
	if err != nil {
		return err
	}
	*doc = *d
	return nil
}
