package project

import (
	"encoding/json"
	"fmt"
	"math"
	"time"
)

// ValidMode checks whether the mode is recognized.
func ValidMode(mode string) bool {
	return mode == ModeArm || mode == ModeCode || mode == ModeBlock
}

// UTF16CodeUnitLength calculates the length of a string in 16-bit UTF-16 code units.
func UTF16CodeUnitLength(s string) int {
	units := 0
	for _, r := range s {
		if r >= 0x10000 {
			units += 2
		} else {
			units++
		}
	}
	return units
}

// ValidateLocalProjectDocument verifies all constraints of LocalProjectDocument v2.
func ValidateLocalProjectDocument(doc *LocalProjectDocument) error {
	if doc == nil {
		return &StoreError{Code: ErrProjectInvalid, Message: "document is nil"}
	}
	if doc.Schema != 2 {
		return &StoreError{Code: ErrProjectInvalid, Message: "schema version must be 2"}
	}
	if !IsValidUUID(doc.ProjectID) {
		return &StoreError{Code: ErrProjectInvalid, Message: "project_id must be a valid lowercase UUID"}
	}
	if !ValidMode(doc.Mode) {
		return &StoreError{Code: ErrProjectInvalid, Message: fmt.Sprintf("invalid mode %q", doc.Mode)}
	}
	nameRunes := len([]rune(doc.Name))
	if nameRunes < 1 || nameRunes > 200 {
		return &StoreError{Code: ErrProjectInvalid, Message: "name length must be between 1 and 200 characters"}
	}
	if doc.LocalRevision < 1 {
		return &StoreError{Code: ErrProjectInvalid, Message: "local_revision must be >= 1"}
	}
	if doc.UpdatedAt == "" {
		return &StoreError{Code: ErrProjectInvalid, Message: "updated_at is required"}
	}
	if _, err := time.Parse(time.RFC3339, doc.UpdatedAt); err != nil {
		return &StoreError{Code: ErrProjectInvalid, Message: "updated_at must be valid RFC3339 timestamp"}
	}
	if doc.Content == nil {
		return &StoreError{Code: ErrProjectInvalid, Message: "content is required"}
	}

	data, err := json.Marshal(doc)
	if err != nil {
		return &StoreError{Code: ErrProjectInvalid, Message: fmt.Sprintf("failed to serialize document: %v", err)}
	}
	if len(data) > 1<<20 {
		return &StoreError{Code: ErrProjectTooLarge, Message: "project document exceeds 1 MiB limit"}
	}

	return ValidateProjectContent(doc.Mode, doc.Content)
}

// ValidateProjectContent validates mode-specific tagged content according to DES-003.
func ValidateProjectContent(mode string, content *ProjectContent) error {
	if content == nil {
		return &StoreError{Code: ErrProjectInvalid, Message: "content is nil"}
	}
	if content.Kind != mode {
		return &StoreError{Code: ErrProjectInvalid, Message: fmt.Sprintf("content kind %q does not match mode %q", content.Kind, mode)}
	}

	switch mode {
	case ModeCode:
		return validateCodeContent(content)
	case ModeBlock:
		return validateBlockContent(content)
	case ModeArm:
		return validateArmContent(content)
	default:
		return &StoreError{Code: ErrProjectInvalid, Message: fmt.Sprintf("unsupported mode %q", mode)}
	}
}

func validateCodeContent(c *ProjectContent) error {
	if len(c.Stacks) > 0 || len(c.Poses) > 0 || len(c.Order) > 0 || len(c.Groups) > 0 ||
		len(c.JointNames) > 0 || c.TimeMode != "" || c.Loops != 0 || c.View != "" {
		return &StoreError{Code: ErrProjectInvalid, Message: "foreign union fields present in code content"}
	}
	units := UTF16CodeUnitLength(c.Source)
	if units > 16384 {
		return &StoreError{
			Code:    ErrProjectTooLarge,
			Message: fmt.Sprintf("code source exceeds UI load limit of 16384 UTF-16 units (got %d)", units),
		}
	}
	return nil
}

func validateBlockContent(c *ProjectContent) error {
	if c.Source != "" || len(c.Poses) > 0 || len(c.Order) > 0 || len(c.Groups) > 0 ||
		len(c.JointNames) > 0 || c.TimeMode != "" || c.Loops != 0 || c.View != "" {
		return &StoreError{Code: ErrProjectInvalid, Message: "foreign union fields present in block content"}
	}

	seenIDs := make(map[string]bool)
	totalBlocks := 0

	for sIdx, stack := range c.Stacks {
		if math.IsNaN(stack.X) || math.IsInf(stack.X, 0) || math.IsNaN(stack.Y) || math.IsInf(stack.Y, 0) {
			return &StoreError{Code: ErrProjectInvalid, Message: fmt.Sprintf("stack[%d] has non-finite coordinates", sIdx)}
		}
		for _, b := range stack.Blocks {
			if err := walkBlockTree(b, seenIDs, &totalBlocks, 0); err != nil {
				return err
			}
		}
	}

	if totalBlocks > 1000 {
		return &StoreError{Code: ErrProjectTooLarge, Message: fmt.Sprintf("block count exceeds limit of 1000 (got %d)", totalBlocks)}
	}
	return nil
}

func walkBlockTree(b Block, seenIDs map[string]bool, total *int, depth int) error {
	*total++
	if *total > 1000 {
		return &StoreError{Code: ErrProjectTooLarge, Message: "total blocks exceed limit of 1000"}
	}
	if depth > 8 {
		return &StoreError{Code: ErrProjectInvalid, Message: "block nesting depth exceeds limit of 8"}
	}
	if !IsValidUUID(b.ID) {
		return &StoreError{Code: ErrProjectInvalid, Message: fmt.Sprintf("block id %q is not a valid UUID", b.ID)}
	}
	if seenIDs[b.ID] {
		return &StoreError{Code: ErrProjectInvalid, Message: fmt.Sprintf("duplicate block id %q", b.ID)}
	}
	seenIDs[b.ID] = true

	switch b.Type {
	case "move":
		if b.Times != 0 || len(b.Body) > 0 {
			return &StoreError{Code: ErrProjectInvalid, Message: "move block contains foreign fields"}
		}
		if b.Direction != "move_right" && b.Direction != "move_left" {
			return &StoreError{Code: ErrProjectInvalid, Message: fmt.Sprintf("invalid move direction %q", b.Direction)}
		}
		if b.Tiles < 1 || b.Tiles > 10 {
			return &StoreError{Code: ErrProjectInvalid, Message: fmt.Sprintf("move tiles %d must be in range 1..10", b.Tiles)}
		}
	case "jump":
		if b.Direction != "" || b.Tiles != 0 || b.Times != 0 || len(b.Body) > 0 {
			return &StoreError{Code: ErrProjectInvalid, Message: "jump block contains foreign fields"}
		}
	case "repeat":
		if b.Direction != "" || b.Tiles != 0 {
			return &StoreError{Code: ErrProjectInvalid, Message: "repeat block contains foreign fields"}
		}
		if b.Times < 1 || b.Times > 10 {
			return &StoreError{Code: ErrProjectInvalid, Message: fmt.Sprintf("repeat times %d must be in range 1..10", b.Times)}
		}
		for _, child := range b.Body {
			if err := walkBlockTree(child, seenIDs, total, depth+1); err != nil {
				return err
			}
		}
	default:
		return &StoreError{Code: ErrProjectInvalid, Message: fmt.Sprintf("unsupported block type %q", b.Type)}
	}
	return nil
}

func validateArmContent(c *ProjectContent) error {
	if c.Source != "" || len(c.Stacks) > 0 {
		return &StoreError{Code: ErrProjectInvalid, Message: "foreign union fields present in arm content"}
	}
	if c.TimeMode != "same" && c.TimeMode != "split" {
		return &StoreError{Code: ErrProjectInvalid, Message: "time_mode must be 'same' or 'split'"}
	}
	if c.Loops < 1 || c.Loops > 9999 {
		return &StoreError{Code: ErrProjectInvalid, Message: "loops must be between 1 and 9999"}
	}
	if c.View != "cards" && c.View != "table" {
		return &StoreError{Code: ErrProjectInvalid, Message: "view must be 'cards' or 'table'"}
	}
	if len(c.JointNames) != 6 {
		return &StoreError{Code: ErrProjectInvalid, Message: "joint_names must contain exactly 6 strings"}
	}
	for i, jn := range c.JointNames {
		rn := len([]rune(jn))
		if rn < 1 || rn > 200 {
			return &StoreError{Code: ErrProjectInvalid, Message: fmt.Sprintf("joint_names[%d] length must be 1..200", i)}
		}
	}

	poseMap := make(map[string]bool)
	for i, p := range c.Poses {
		if !IsValidUUID(p.ID) {
			return &StoreError{Code: ErrProjectInvalid, Message: fmt.Sprintf("pose[%d] id %q is not a valid UUID", i, p.ID)}
		}
		if poseMap[p.ID] {
			return &StoreError{Code: ErrProjectInvalid, Message: fmt.Sprintf("duplicate pose id %q", p.ID)}
		}
		poseMap[p.ID] = true

		pNameRunes := len([]rune(p.Name))
		if pNameRunes < 1 || pNameRunes > 200 {
			return &StoreError{Code: ErrProjectInvalid, Message: fmt.Sprintf("pose[%d] name must be 1..200 characters", i)}
		}
		if p.Values[0] < -180 || p.Values[0] > 180 {
			return &StoreError{Code: ErrProjectInvalid, Message: fmt.Sprintf("pose[%d] joint 0 out of bounds [-180, 180]", i)}
		}
		for j := 1; j <= 4; j++ {
			if p.Values[j] < -135 || p.Values[j] > 135 {
				return &StoreError{Code: ErrProjectInvalid, Message: fmt.Sprintf("pose[%d] joint %d out of bounds [-135, 135]", i, j)}
			}
		}
		if p.Values[5] < 0 || p.Values[5] > 100 {
			return &StoreError{Code: ErrProjectInvalid, Message: fmt.Sprintf("pose[%d] gripper joint out of bounds [0, 100]", i)}
		}
		if p.DurationMs < 1000 || p.DurationMs > 9000 {
			return &StoreError{Code: ErrProjectInvalid, Message: fmt.Sprintf("pose[%d] duration_ms must be 1000..9000", i)}
		}
		for j := 0; j < 6; j++ {
			if p.JointDurationMs[j] < 1000 || p.JointDurationMs[j] > 9000 {
				return &StoreError{Code: ErrProjectInvalid, Message: fmt.Sprintf("pose[%d] joint_duration_ms[%d] must be 1000..9000", i, j)}
			}
		}
	}

	if len(c.Order) > 10000 {
		return &StoreError{Code: ErrProjectTooLarge, Message: "order exceeds 10000 poses limit"}
	}
	if len(c.Order) != len(c.Poses) {
		return &StoreError{Code: ErrProjectInvalid, Message: "order must reference all poses exactly once"}
	}
	orderSeen := make(map[string]bool)
	for i, oID := range c.Order {
		if !IsValidUUID(oID) {
			return &StoreError{Code: ErrProjectInvalid, Message: fmt.Sprintf("order[%d] id %q is not a valid UUID", i, oID)}
		}
		if orderSeen[oID] {
			return &StoreError{Code: ErrProjectInvalid, Message: fmt.Sprintf("duplicate pose id in order: %q", oID)}
		}
		orderSeen[oID] = true
		if !poseMap[oID] {
			return &StoreError{Code: ErrProjectInvalid, Message: fmt.Sprintf("order references unknown pose id %q", oID)}
		}
	}

	groupNames := make(map[string]bool)
	groupIDs := make(map[string]bool)
	for i, g := range c.Groups {
		if !IsValidUUID(g.ID) {
			return &StoreError{Code: ErrProjectInvalid, Message: fmt.Sprintf("group[%d] id %q is not a valid UUID", i, g.ID)}
		}
		if groupIDs[g.ID] {
			return &StoreError{Code: ErrProjectInvalid, Message: fmt.Sprintf("duplicate group id %q", g.ID)}
		}
		groupIDs[g.ID] = true

		gnRunes := len([]rune(g.Name))
		if gnRunes < 1 || gnRunes > 20 {
			return &StoreError{Code: ErrProjectInvalid, Message: fmt.Sprintf("group[%d] name length must be 1..20 characters", i)}
		}
		if groupNames[g.Name] {
			return &StoreError{Code: ErrProjectInvalid, Message: fmt.Sprintf("duplicate group name %q", g.Name)}
		}
		groupNames[g.Name] = true

		if len(g.PoseIDs) < 2 {
			return &StoreError{Code: ErrProjectInvalid, Message: fmt.Sprintf("group[%d] must contain >= 2 pose_ids", i)}
		}
		for _, pid := range g.PoseIDs {
			if !orderSeen[pid] {
				return &StoreError{Code: ErrProjectInvalid, Message: fmt.Sprintf("group[%d] references pose id %q not in order", i, pid)}
			}
		}
	}

	return nil
}

// CalculateCompilationFuel computes pure compiler fuel according to the arch specification.
func CalculateCompilationFuel(stacks []Stack) int {
	fuel := 0
	var walk func([]Block)
	walk = func(blocks []Block) {
		fuel++
		if fuel > 1000 {
			return
		}
		for _, b := range blocks {
			fuel++
			if fuel > 1000 {
				return
			}
			if !b.Enabled {
				continue
			}
			if b.Type == "repeat" {
				for i := 0; i < b.Times; i++ {
					walk(b.Body)
					if fuel > 1000 {
						return
					}
				}
			}
		}
	}
	for _, stack := range stacks {
		walk(stack.Blocks)
		if fuel > 1000 {
			break
		}
	}
	return fuel
}
