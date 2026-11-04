package engine

import (
	"bytes"
	"encoding/json"
	"errors"
	"fmt"
	"io"
)

type simulatorServo struct {
	ID     int `json:"id"`
	PWM    int `json:"pwm"`
	TimeMS int `json:"time_ms"`
	Bias   int `json:"bias,omitempty"`
}

type simulatorGroup struct {
	Index int              `json:"index"`
	Items []simulatorServo `json:"items"`
}

type simulatorSafetyError struct{}

func (simulatorSafetyError) Error() string { return "pwm including bias must be 500-2500" }

func simulatorObject(data json.RawMessage) (map[string]json.RawMessage, error) {
	var fields map[string]json.RawMessage
	if err := json.Unmarshal(data, &fields); err != nil || fields == nil {
		return nil, errors.New("data must be an object")
	}
	return fields, nil
}

func simulatorInteger(fields map[string]json.RawMessage, key string, low, high int, fallback *int) (int, error) {
	raw, exists := fields[key]
	if !exists && fallback != nil {
		return *fallback, nil
	}
	var value int
	if !exists || bytes.Equal(raw, []byte("null")) || json.Unmarshal(raw, &value) != nil || value < low || value > high {
		return 0, fmt.Errorf("%s must be an integer between %d and %d", key, low, high)
	}
	return value, nil
}

func simulatorKeys(fields map[string]json.RawMessage, allowed ...string) error {
	keys := actionSet(append([]string{"transport", "device", "port", "baud", "read_ms"}, allowed...)...)
	for key := range fields {
		if _, ok := keys[key]; !ok {
			return fmt.Errorf("unknown data field: %s", key)
		}
	}
	return nil
}

func simulatorDecode(raw json.RawMessage, target interface{}) error {
	decoder := json.NewDecoder(bytes.NewReader(raw))
	decoder.DisallowUnknownFields()
	if err := decoder.Decode(target); err != nil {
		return errors.New("invalid simulator data")
	}
	if decoder.Decode(new(json.RawMessage)) != io.EOF {
		return errors.New("invalid simulator data")
	}
	return nil
}

func simulatorGroups(fields map[string]json.RawMessage, download bool) ([]simulatorGroup, error) {
	group, single := fields["group"]
	groups, multiple := fields["groups"]
	if single == multiple {
		return nil, errors.New("use group or groups")
	}
	if single {
		groups = append(append([]byte{'['}, group...), ']')
	}
	var rows []json.RawMessage
	if err := json.Unmarshal(groups, &rows); err != nil || len(rows) < 1 || len(rows) > 4 || (!download && len(rows) != 1) {
		return nil, errors.New("simulator accepts one live group or 1..4 upload groups")
	}
	result := make([]simulatorGroup, 0, len(rows))
	indices := map[int]bool{}
	for _, row := range rows {
		object, err := simulatorObject(row)
		if err != nil || len(object) != 2 {
			return nil, errors.New("group requires index and items")
		}
		index, err := simulatorInteger(object, "index", 0, 9999, nil)
		if err != nil || indices[index] {
			return nil, errors.New("invalid or duplicate group index")
		}
		indices[index] = true
		var items []json.RawMessage
		if json.Unmarshal(object["items"], &items) != nil || len(items) != 6 {
			return nil, errors.New("simulator poses require all six profile IDs")
		}
		validated := simulatorGroup{Index: index}
		ids := map[int]bool{}
		for _, raw := range items {
			var servo simulatorServo
			item, itemErr := simulatorObject(raw)
			if itemErr != nil || simulatorDecode(raw, &servo) != nil {
				return nil, errors.New("invalid servo item")
			}
			id, idErr := simulatorInteger(item, "id", 0, 5, nil)
			pwm, pwmErr := simulatorInteger(item, "pwm", 500, 2500, nil)
			_, timeErr := simulatorInteger(item, "time_ms", 0, 9999, nil)
			zero := 0
			bias, biasErr := simulatorInteger(item, "bias", -500, 500, &zero)
			if idErr != nil || pwmErr != nil || timeErr != nil || biasErr != nil || ids[id] {
				return nil, errors.New("invalid servo bounds or duplicate profile ID")
			}
			if pwm+bias < 500 || pwm+bias > 2500 {
				return nil, simulatorSafetyError{}
			}
			ids[id] = true
			servo.PWM = pwm + bias
			servo.Bias = 0
			validated.Items = append(validated.Items, servo)
		}
		result = append(result, validated)
	}
	return result, nil
}
