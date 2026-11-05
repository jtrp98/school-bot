package engine

import (
	"encoding/json"
	"errors"
	"fmt"
	"time"
)

func (s *hardwareSimulator) planServo(action string, fields map[string]json.RawMessage,
	result map[string]interface{}) ([]string, func(), error) {
	allowed := []string{"id"}
	maximum := 5
	var fallback *int
	broadcast := 255
	if action == "servo.set" {
		allowed = append(allowed, "pwm", "time_ms", "bias")
	}
	if action == "servo.torque" {
		allowed = append(allowed, "enabled")
		maximum = 255
		fallback = &broadcast
	}
	if err := simulatorKeys(fields, allowed...); err != nil {
		return nil, nil, err
	}
	id, err := simulatorInteger(fields, "id", 0, maximum, fallback)
	if err != nil || (id > 5 && id != 255) {
		return nil, nil, errors.New("id is not in the simulator profile")
	}
	if action == "servo.read" {
		return []string{fmt.Sprintf("#%03dPRAD!", id)}, nil, nil
	}
	if action == "servo.torque" {
		enabled, err := simulatorBool(fields["enabled"])
		if err != nil {
			return nil, nil, err
		}
		verb := "PULK"
		if enabled {
			verb = "PSCK"
		}
		return []string{fmt.Sprintf("#%03d%s!", id, verb)}, func() {
			for index := range s.torque {
				if id == 255 || id == index {
					s.torque[index] = enabled
				}
			}
		}, nil
	}
	pwm, pwmErr := simulatorInteger(fields, "pwm", 500, 2500, nil)
	defaultTime, zero := 1000, 0
	duration, timeErr := simulatorInteger(fields, "time_ms", 0, 9999, &defaultTime)
	bias, biasErr := simulatorInteger(fields, "bias", -500, 500, &zero)
	if pwmErr != nil || timeErr != nil || biasErr != nil {
		return nil, nil, errors.New("invalid servo PWM, bias or time")
	}
	if pwm+bias < 500 || pwm+bias > 2500 {
		return nil, nil, simulatorSafetyError{}
	}
	result["id"] = id
	result["pwm"] = pwm + bias
	result["time_ms"] = duration
	return []string{fmt.Sprintf("#%03dP%04dT%04d!", id, pwm+bias, duration)}, func() {
		s.positions[id] = pwm + bias
	}, nil
}

func simulatorGroupCommand(group simulatorGroup, download bool) string {
	prefix, suffix := "{G", "}"
	if download {
		prefix, suffix = "<G", "K0000N0000B000!>"
	}
	command := fmt.Sprintf("%s%04d", prefix, group.Index)
	for _, item := range group.Items {
		command += fmt.Sprintf("#%03dP%04dT%04d!", item.ID, item.PWM, item.TimeMS)
	}
	return command + suffix
}

func (s *hardwareSimulator) applyGroup(group simulatorGroup) {
	for _, servo := range group.Items {
		s.positions[servo.ID] = servo.PWM
	}
}

func (s *hardwareSimulator) planRun(fields map[string]json.RawMessage,
	result map[string]interface{}) ([]string, func(), error) {
	if err := simulatorKeys(fields, "start", "end", "times"); err != nil {
		return nil, nil, err
	}
	start, startErr := simulatorInteger(fields, "start", 0, 9999, nil)
	end, endErr := simulatorInteger(fields, "end", start, 9999, nil)
	times, timesErr := simulatorInteger(fields, "times", 1, 9999, nil)
	if startErr != nil || endErr != nil || timesErr != nil {
		return nil, nil, errors.New("invalid finite run range or repetitions")
	}
	var totalMS int64
	for index := start; index <= end; index++ {
		group, exists := s.groups[index]
		if !exists {
			return nil, nil, errors.New("run range is not fully uploaded in this simulator session")
		}
		maximum := 0
		for _, servo := range group.Items {
			maximum = max(maximum, servo.TimeMS)
		}
		totalMS += int64(maximum)
	}
	// Compute the finite schedule once; never expand poses times repetitions into a work queue.
	duration := time.Duration(totalMS*int64(times)) * time.Millisecond
	result["runs_on_device"] = false
	return []string{fmt.Sprintf("$DGT:%d-%d,%d!", start, end, times)}, func() {
		s.runUntil = s.now().Add(duration)
		s.lastGroup = end
	}, nil
}
