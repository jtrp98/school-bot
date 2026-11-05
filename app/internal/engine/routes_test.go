package engine

import (
	"encoding/json"
	"os"
	"path/filepath"
	"reflect"
	"testing"
)

func TestTrustedEngineRegistry(t *testing.T) {
	for _, c := range []struct{ mode, action, id string }{
		{"robot-control", "robot.stop", "zide"},
		{"coding-adventure", "program.run", "demo"},
		{"block-programming", "program.generate", "mixly"},
		{"block-programming", "project.validate", "mixly"},
		{"block-programming", "device.list", "mixly"},
	} {
		r, err := routeFor(c.mode, c.action)
		if err != nil || r.engine.id != c.id || filepath.IsAbs(r.engine.relativePath) {
			t.Fatalf("%s/%s: %#v %v", c.mode, c.action, r, err)
		}
	}
	for _, mode := range []string{"zide", "mixly", "../../evil", "", "Robot-Control"} {
		if ValidMode(mode) {
			t.Fatalf("accepted %q", mode)
		}
	}
}

func TestEveryModeHasActionAllowlist(t *testing.T) {
	for _, mode := range []string{"robot-control", "coding-adventure", "block-programming"} {
		if len(modeRoutes[mode]) == 0 {
			t.Fatalf("%s has no action allowlist", mode)
		}
	}
	if len(modeRoutes) != 3 {
		t.Fatalf("allowlist modes differ from registry: %d", len(modeRoutes))
	}
}

// serial.exchange writes raw bytes to any port and firmware flashing has no Block Lab use; the
// Serial monitor goes through the session routes, which take the picked board's USB IDs.
func TestBlockLabLeavesRawPortActionsUnrouted(t *testing.T) {
	for action, r := range blockLabSerialRoutes {
		if !r.session || r.accepted == nil || r.accepted["baud_rate"] || r.accepted["transport"] {
			t.Fatalf("%s route: %#v", action, r)
		}
	}
	for _, action := range []string{"serial.exchange", "toolchain.status", "firmware.flash", "board.list", "board.info"} {
		if _, err := routeFor("block-programming", action); err == nil {
			t.Fatalf("%s is routed", action)
		}
	}
}

func TestBlockLabBuildRoutes(t *testing.T) {
	windows := blockLabBuildRoutes("windows")
	for _, action := range []string{"program.compile", "program.upload"} {
		r, ok := windows[action]
		if !ok || r.engine.id != "mixly" || r.timeout != buildTimeout || r.fixed["transport"] != "native" ||
			r.fixed["board"] != "arduino_avr" || r.accepted["source"] || r.accepted["options"] || r.blocks == nil {
			t.Fatalf("%s route: %#v", action, r)
		}
	}
	if windows["program.compile"].accepted["port"] {
		t.Fatal("compile must not take a port")
	}
	if _, ok := blockLabBuildRoutes("darwin")["program.upload"]; ok {
		t.Fatal("upload is routed where cancel cannot kill avrdude")
	}
}

func TestBlockLabBuildAcceptsOnlyItsFieldsAndBlocks(t *testing.T) {
	upload := blockLabBuildRoutes("windows")["program.upload"]
	led := `{"type":"sb_led","fields":{"PIN":"13"}}`
	program := `[{"type":"sb_loop","statements":{"DO":` + led + `}}]`
	accepted := []string{
		`{"blocks":` + program + `,"port":"COM5","vid":"2341","pid":"0043"}`,
		`{"blocks":[{"type":"base_setup","next":{"type":"controls_delay","values":{"DELAY_TIME":{"type":"math_number"}}}}]}`,
	}
	for _, data := range accepted {
		if !pageDataAccepted(json.RawMessage(data), upload) {
			t.Fatalf("refused %s", data)
		}
	}
	refused := []string{
		`{"blocks":[],"source":"int main(){}"}`,
		`{"blocks":[],"Source":"int main(){}"}`,
		`{"blocks":[],"xml":"<xml/>"}`,
		`{"blocks":[],"options":{}}`,
		`{"blocks":[],"board":"micropython_esp32"}`,
		`{"port":"COM5"}`,
		`{"blocks":[{"type":"factory_include"}]}`,
		`{"blocks":[{"type":"sb_loop","statements":{"DO":{"type":"factory_function"}}}]}`,
		`{"blocks":[{"type":"sb_loop","next":{"type":"factory_declare"}}]}`,
		`{"blocks":[{"type":"sb_loop","values":{"A":null}}]}`,
		`{"blocks":[null]}`,
		`{"blocks":[{"type":"sb_collision"}]}`,
		`[]`,
	}
	for _, data := range refused {
		if pageDataAccepted(json.RawMessage(data), upload) {
			t.Fatalf("accepted %s", data)
		}
	}
	generate, _ := routeFor("block-programming", "program.generate")
	if !pageDataAccepted(json.RawMessage(`{"board":"x","blocks":[{"type":"factory_include"}]}`), generate) {
		t.Fatal("routes without an allowlist must pass data through")
	}
}

// The toolbox and the host must agree on Block Lab's blocks; deferred ones are not offered.
func TestBlockLabBlocksMatchCatalog(t *testing.T) {
	raw, err := os.ReadFile(filepath.Join("..", "..", "..", "contracts", "block-lab", "catalog.json"))
	if err != nil {
		t.Fatal(err)
	}
	var catalog struct {
		Categories []struct {
			Blocks []struct{ Type, Status string }
		}
	}
	if err := json.Unmarshal(raw, &catalog); err != nil {
		t.Fatal(err)
	}
	offered := map[string]bool{}
	for _, category := range catalog.Categories {
		for _, block := range category.Blocks {
			if block.Status != "deferred" {
				offered[block.Type] = true
			}
		}
	}
	if !reflect.DeepEqual(offered, blockLabBlocks) {
		t.Fatalf("blockLabBlocks differs from catalog.json:\ncatalog %v\nhost    %v", offered, blockLabBlocks)
	}
}

// The port list carries no board, or the engine would reject the request as unknown fields.
func TestBlockLabDeviceListHasNoFixedFields(t *testing.T) {
	r, err := routeFor("block-programming", "device.list")
	if err != nil || len(r.fixed) != 0 {
		t.Fatalf("device.list route: %#v %v", r, err)
	}
}

func TestBlockLabProgramsAreFixedToUno(t *testing.T) {
	r, _ := routeFor("block-programming", "program.generate")
	data, ok := withFixedFields(json.RawMessage(`{"board":"micropython_esp32","platform":"x","blocks":[]}`), r.fixed)
	if !ok {
		t.Fatal("object rejected")
	}
	var fields map[string]json.RawMessage
	if err := json.Unmarshal(data, &fields); err != nil {
		t.Fatal(err)
	}
	if string(fields["board"]) != `"arduino_avr"` || string(fields["platform"]) != `"Arduino/Genuino Uno"` ||
		string(fields["blocks"]) != `[]` {
		t.Fatalf("fixed fields not applied: %s", data)
	}
	for _, input := range []string{`["board"]`, `{"Board":"micropython_esp32"}`, `{"PLATFORM":"x"}`} {
		if _, ok := withFixedFields(json.RawMessage(input), r.fixed); ok {
			t.Fatalf("accepted %s", input)
		}
	}
	if data, ok := withFixedFields(json.RawMessage(`[1]`), nil); !ok || string(data) != `[1]` {
		t.Fatal("routes without fixed fields must pass data through unchanged")
	}
}
