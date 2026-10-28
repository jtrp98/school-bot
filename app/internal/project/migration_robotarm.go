package project

import (
	"encoding/json"
	"fmt"
)

var defaultJointNames = []string{
	"Joint 1", "Joint 2", "Joint 3", "Joint 4", "Joint 5", "Gripper",
}

type legacyPose struct {
	Name string     `json:"name"`
	V    [6]float64 `json:"v"`
	T    int        `json:"t"`
	Ts   [6]int     `json:"ts"`
}

type legacyGroup struct {
	ID   string   `json:"id"`
	Name string   `json:"name"`
	Keys []string `json:"keys"`
}

func convertLegacyArm(data []byte, projectID string) (*ProjectContent, string, bool, error) {
	var payload struct {
		Kind    string `json:"kind"`
		Version int    `json:"version"`
		State   *struct {
			Seq      []string              `json:"seq"`
			Poses    map[string]legacyPose `json:"poses"`
			Groups   []legacyGroup         `json:"groups"`
			JNames   []string              `json:"jnames"`
			TimeMode string                `json:"timeMode"`
			View     string                `json:"view"`
			Loops    int                   `json:"loops"`
			ProjName string                `json:"projName"`
		} `json:"state"`
	}
	if err := json.Unmarshal(data, &payload); err != nil {
		return nil, "", false, fmt.Errorf("invalid arm workspace JSON: %v", err)
	}
	if payload.Kind != "zide-workspace" || payload.Version != 1 || payload.State == nil {
		return nil, "", false, fmt.Errorf("unsupported arm workspace kind or version")
	}

	state := payload.State
	if len(state.Seq) > 10000 {
		return nil, "", false, fmt.Errorf("pose sequence exceeds 10000 limit")
	}

	seqSet := make(map[string]bool)
	for _, k := range state.Seq {
		if seqSet[k] {
			return nil, "", false, fmt.Errorf("duplicate key %q in pose sequence", k)
		}
		seqSet[k] = true
	}

	var poses []Pose
	var order []string
	keyToUUID := make(map[string]string)

	for _, k := range state.Seq {
		lp, ok := state.Poses[k]
		if !ok {
			return nil, "", false, fmt.Errorf("pose key %q not found in poses", k)
		}
		poseUUID := DeterministicUUID(SchoolBotNamespace, fmt.Sprintf("%s:pose:%s", projectID, k))
		pName := lp.Name
		if len([]rune(pName)) < 1 {
			pName = k
		}
		if len([]rune(pName)) > 200 {
			pName = string([]rune(pName)[:200])
		}

		if lp.V[0] < -180 || lp.V[0] > 180 {
			return nil, "", false, fmt.Errorf("pose %q joint 0 out of bounds", k)
		}
		for j := 1; j <= 4; j++ {
			if lp.V[j] < -135 || lp.V[j] > 135 {
				return nil, "", false, fmt.Errorf("pose %q joint %d out of bounds", k, j)
			}
		}
		if lp.V[5] < 0 || lp.V[5] > 100 {
			return nil, "", false, fmt.Errorf("pose %q gripper out of bounds", k)
		}

		if lp.T < 1000 || lp.T > 9000 {
			return nil, "", false, fmt.Errorf("pose %q duration_ms %d out of bounds", k, lp.T)
		}
		for j := 0; j < 6; j++ {
			if lp.Ts[j] < 1000 || lp.Ts[j] > 9000 {
				return nil, "", false, fmt.Errorf("pose %q joint_duration_ms[%d]=%d out of bounds", k, j, lp.Ts[j])
			}
		}

		poses = append(poses, Pose{
			ID:              poseUUID,
			Name:            pName,
			Values:          lp.V,
			DurationMs:      lp.T,
			JointDurationMs: lp.Ts,
		})
		order = append(order, poseUUID)
		keyToUUID[k] = poseUUID
	}

	hasSkippedGroups := false
	var groups []PoseGroup
	groupNames := make(map[string]bool)
	for _, lg := range state.Groups {
		gName := lg.Name
		rCount := len([]rune(gName))
		if rCount < 1 || rCount > 20 || groupNames[gName] {
			hasSkippedGroups = true
			continue
		}
		if len(lg.Keys) < 2 {
			hasSkippedGroups = true
			continue
		}
		allKeysValid := true
		var groupPoseIDs []string
		for _, k := range lg.Keys {
			u, exists := keyToUUID[k]
			if !exists {
				allKeysValid = false
				break
			}
			groupPoseIDs = append(groupPoseIDs, u)
		}
		if !allKeysValid {
			hasSkippedGroups = true
			continue
		}

		gUUID := DeterministicUUID(SchoolBotNamespace, fmt.Sprintf("%s:group:%s", projectID, lg.ID))
		groups = append(groups, PoseGroup{
			ID:      gUUID,
			Name:    gName,
			PoseIDs: groupPoseIDs,
		})
		groupNames[gName] = true
	}

	jnames := defaultJointNames
	if len(state.JNames) == 6 {
		valid := true
		for _, jn := range state.JNames {
			if len([]rune(jn)) < 1 || len([]rune(jn)) > 200 {
				valid = false
				break
			}
		}
		if valid {
			jnames = state.JNames
		}
	}

	timeMode := "same"
	if state.TimeMode == "split" {
		timeMode = "split"
	}
	view := "cards"
	if state.View == "table" {
		view = "table"
	}
	loops := state.Loops
	if loops < 1 || loops > 9999 {
		loops = 1
	}

	name := "Robot Control"
	if state.ProjName != "" && len([]rune(state.ProjName)) <= 200 {
		name = state.ProjName
	}

	content := &ProjectContent{
		Kind:       ModeArm,
		Poses:      poses,
		Order:      order,
		Groups:     groups,
		JointNames: jnames,
		TimeMode:   timeMode,
		Loops:      loops,
		View:       view,
	}

	return content, name, hasSkippedGroups, nil
}
