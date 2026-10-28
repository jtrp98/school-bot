package project

import (
	"bytes"
	"encoding/json"
	"fmt"
)

func decodeStrictArm(m map[string]json.RawMessage) (*ProjectContent, error) {
	required := []string{"kind", "poses", "order", "groups", "joint_names", "time_mode", "loops", "view"}
	if len(m) != len(required) {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: "arm content fields mismatch"}
	}
	for _, req := range required {
		if b, ok := m[req]; !ok || bytes.Equal(bytes.TrimSpace(b), []byte("null")) {
			return nil, &StoreError{Code: ErrProjectInvalid, Message: fmt.Sprintf("arm missing/null %s", req)}
		}
	}

	var rawPoses []json.RawMessage
	if err := json.Unmarshal(m["poses"], &rawPoses); err != nil {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: "poses must be array"}
	}
	poses := make([]Pose, 0, len(rawPoses))
	for _, rp := range rawPoses {
		p, err := decodeStrictPose(rp)
		if err != nil {
			return nil, err
		}
		poses = append(poses, *p)
	}

	var order []string
	if err := json.Unmarshal(m["order"], &order); err != nil {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: "order must be array of strings"}
	}
	if order == nil {
		order = []string{}
	}

	var rawGroups []json.RawMessage
	if err := json.Unmarshal(m["groups"], &rawGroups); err != nil {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: "groups must be array"}
	}
	groups := make([]PoseGroup, 0, len(rawGroups))
	for _, rg := range rawGroups {
		g, err := decodeStrictGroup(rg)
		if err != nil {
			return nil, err
		}
		groups = append(groups, *g)
	}

	var rawJNames []string
	if err := json.Unmarshal(m["joint_names"], &rawJNames); err != nil || len(rawJNames) != 6 {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: "joint_names must be array of length 6"}
	}

	var timeMode, view string
	var loops int
	if err := json.Unmarshal(m["time_mode"], &timeMode); err != nil || (timeMode != "same" && timeMode != "split") {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: "invalid time_mode"}
	}
	if err := json.Unmarshal(m["view"], &view); err != nil || (view != "cards" && view != "table") {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: "invalid view"}
	}
	if err := json.Unmarshal(m["loops"], &loops); err != nil || loops < 1 || loops > 9999 {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: "invalid loops"}
	}

	c := &ProjectContent{
		Kind:       ModeArm,
		Poses:      poses,
		Order:      order,
		Groups:     groups,
		JointNames: rawJNames,
		TimeMode:   timeMode,
		Loops:      loops,
		View:       view,
	}
	if err := validateArmContent(c); err != nil {
		return nil, err
	}
	return c, nil
}

func decodeStrictPose(raw json.RawMessage) (*Pose, error) {
	var m map[string]json.RawMessage
	if err := json.Unmarshal(raw, &m); err != nil || len(m) != 5 {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: "pose must contain exactly id, name, values, duration_ms, joint_duration_ms"}
	}
	for _, f := range []string{"id", "name", "values", "duration_ms", "joint_duration_ms"} {
		if b, ok := m[f]; !ok || bytes.Equal(bytes.TrimSpace(b), []byte("null")) {
			return nil, &StoreError{Code: ErrProjectInvalid, Message: fmt.Sprintf("pose missing/null %s", f)}
		}
	}
	var id, name string
	var dur int
	if err := json.Unmarshal(m["id"], &id); err != nil || !IsValidUUID(id) {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: "pose id must be UUID"}
	}
	if err := json.Unmarshal(m["name"], &name); err != nil {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: "pose name must be string"}
	}
	if err := json.Unmarshal(m["duration_ms"], &dur); err != nil || dur < 1000 || dur > 9000 {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: "pose duration_ms must be 1000..9000"}
	}

	var rawVals []float64
	if err := json.Unmarshal(m["values"], &rawVals); err != nil || len(rawVals) != 6 {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: "pose values must be array of length 6"}
	}
	var rawJDur []int
	if err := json.Unmarshal(m["joint_duration_ms"], &rawJDur); err != nil || len(rawJDur) != 6 {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: "pose joint_duration_ms must be array of length 6"}
	}

	var vals [6]float64
	var jDur [6]int
	copy(vals[:], rawVals)
	copy(jDur[:], rawJDur)

	return &Pose{
		ID:              id,
		Name:            name,
		Values:          vals,
		DurationMs:      dur,
		JointDurationMs: jDur,
	}, nil
}

func decodeStrictGroup(raw json.RawMessage) (*PoseGroup, error) {
	var m map[string]json.RawMessage
	if err := json.Unmarshal(raw, &m); err != nil || len(m) != 3 {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: "group must contain exactly id, name, pose_ids"}
	}
	for _, f := range []string{"id", "name", "pose_ids"} {
		if b, ok := m[f]; !ok || bytes.Equal(bytes.TrimSpace(b), []byte("null")) {
			return nil, &StoreError{Code: ErrProjectInvalid, Message: fmt.Sprintf("group missing/null %s", f)}
		}
	}
	var id, name string
	var pids []string
	if err := json.Unmarshal(m["id"], &id); err != nil || !IsValidUUID(id) {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: "group id must be UUID"}
	}
	if err := json.Unmarshal(m["name"], &name); err != nil {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: "group name must be string"}
	}
	if err := json.Unmarshal(m["pose_ids"], &pids); err != nil || len(pids) < 2 {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: "group pose_ids must have at least 2 items"}
	}
	return &PoseGroup{ID: id, Name: name, PoseIDs: pids}, nil
}
