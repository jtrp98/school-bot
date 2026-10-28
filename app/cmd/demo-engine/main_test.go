package main

import (
	"bytes"
	"encoding/json"
	"os"
	"os/exec"
	"strings"
	"testing"

	"schoolbot.local/app/internal/engine"
)

func TestDemoHelperProcess(t *testing.T) {
	if os.Getenv("SCHOOL_BOT_DEMO_TEST") == "1" {
		main()
		os.Exit(0)
	}
}

func TestDemoLineBoundary(t *testing.T) {
	request := `{"protocol":"1.0","request_id":"boundary","mode":"coding-adventure",` +
		`"action":"program.run","data":{"source":"hero.jump()"}}`
	for _, size := range []int{engine.MaxRequestBytes, engine.MaxRequestBytes + 1} {
		for _, newline := range []string{"", "\n"} {
			cmd := exec.Command(os.Args[0], "-test.run=^TestDemoHelperProcess$")
			cmd.Env = append(os.Environ(), "SCHOOL_BOT_DEMO_TEST=1")
			cmd.Stdin = strings.NewReader(request + strings.Repeat(" ", size-len(request)) + newline)
			var diagnostic bytes.Buffer
			cmd.Stderr = &diagnostic
			output, err := cmd.Output()
			if size > engine.MaxRequestBytes {
				if err == nil || diagnostic.Len() == 0 {
					t.Fatal("oversized line did not fail with a diagnostic")
				}
				continue
			}
			var response engine.Response
			if err != nil || json.Unmarshal(output, &response) != nil || response.Status != "success" {
				t.Fatalf("exact limit failed: %s %v %s", output, err, diagnostic.String())
			}
		}
	}
}
