package engine

import (
	"bytes"
	"encoding/json"
)

// blockLabBlocks are the block types of contracts/block-lab/catalog.json that the toolbox
// offers (deferred blocks excluded); a test keeps the two equal. The engine checks the fields
// these blocks' generators paste into the C++, but other upstream Mixly blocks paste field text
// unchecked, so compile and upload take only these.
var blockLabBlocks = setOf(
	"base_setup", "sb_loop",
	"controls_delay", "controls_if", "controls_whileUntil", "sb_forever", "controls_for", "sb_repeat",
	"controls_flow_statements", "sb_wait_until", "controls_end_program",
	"logic_compare", "logic_operation", "logic_negate", "logic_boolean",
	"math_number", "math_arithmetic", "sb_random", "math_to_int", "base_map", "math_constrain",
	"text", "text_join", "sb_text_letter", "text_length", "sb_text_contains",
	"variables_declare", "variables_get", "variables_set", "sb_variable_change",
	"procedures_defnoreturn", "procedures_defreturn", "procedures_callnoreturn", "procedures_callreturn",
	"procedures_return",
	"sb_servo", "sb_button", "sb_touch", "sb_potentiometer", "sb_reed_switch", "sb_tilt", "sb_mpu6050",
	"sb_gesture_is", "sb_gesture_light", "sb_rgb_setup", "sb_rgb_set", "sb_rgb_color", "sb_rgb_named_color",
	"sb_light", "sb_line_tracker", "sb_grayscale", "sb_color_sensor", "sb_active_buzzer", "sb_sound",
	"sb_buzzer_tone", "sb_buzzer_tone_for", "sb_buzzer_stop", "sb_ir_obstacle", "sb_pir", "sb_ultrasonic",
	"sb_soil_moisture", "sb_water_level", "sb_mq7", "sb_mq3", "sb_dht11", "sb_relay", "sb_fan",
	"sb_fan_stop", "sb_oled_print", "sb_oled_clear", "sb_led", "sb_led_brightness",
	"serial_print", "serial_readstr", "serial_available", "sb_ir_received", "sb_ir_button",
)

func setOf(items ...string) map[string]bool {
	set := make(map[string]bool, len(items))
	for _, item := range items {
		set[item] = true
	}
	return set
}

// pageBlock mirrors the links of the mixly engine's Block. Go decodes field names the same
// case-insensitive way here and in the engine, so both walk the same tree.
type pageBlock struct {
	Type       string                `json:"type"`
	Values     map[string]*pageBlock `json:"values"`
	Statements map[string]*pageBlock `json:"statements"`
	Next       *pageBlock            `json:"next"`
}

// pageDataAccepted reports whether data uses only the route's accepted fields, spelled exactly,
// and holds only allowed blocks. Exact spelling matters because the engine's decoder would also
// read "Source" as the raw C++ field the route leaves out.
func pageDataAccepted(data json.RawMessage, r route) bool {
	if r.accepted == nil {
		return true
	}
	fields := map[string]json.RawMessage{}
	if trimmed := bytes.TrimSpace(data); len(trimmed) != 0 && !bytes.Equal(trimmed, []byte("null")) {
		if err := json.Unmarshal(trimmed, &fields); err != nil || fields == nil {
			return false
		}
	}
	for name := range fields {
		if !r.accepted[name] {
			return false
		}
	}
	if r.blocks == nil {
		return true
	}
	var blocks []*pageBlock
	if err := json.Unmarshal(fields["blocks"], &blocks); err != nil {
		return false
	}
	var allowed func(*pageBlock) bool
	allowed = func(block *pageBlock) bool {
		for ; block != nil; block = block.Next {
			if !r.blocks[block.Type] {
				return false
			}
			for _, children := range []map[string]*pageBlock{block.Values, block.Statements} {
				for _, child := range children {
					if child == nil || !allowed(child) {
						return false
					}
				}
			}
		}
		return true
	}
	for _, block := range blocks {
		if block == nil || !allowed(block) {
			return false
		}
	}
	return true
}
