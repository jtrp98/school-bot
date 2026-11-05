package main

import (
	"bufio"
	"bytes"
	"encoding/json"
	"fmt"
	"os"
	"schoolbot.local/app/internal/engine"
)

func main() {
	scanner := bufio.NewScanner(os.Stdin)
	// Scanner needs space for the delimiter in addition to the host's largest request.
	scanner.Buffer(make([]byte, 4096), engine.MaxRequestBytes+2)
	encoder := json.NewEncoder(os.Stdout)
	for scanner.Scan() {
		if len(scanner.Bytes()) > engine.MaxRequestBytes {
			fmt.Fprintln(os.Stderr, "request exceeds 64 KB")
			os.Exit(1)
		}
		// Windows PowerShell pipelines may emit a UTF-8 BOM before the first request.
		raw := bytes.TrimPrefix(scanner.Bytes(), []byte("\xef\xbb\xbf"))
		var request engine.Request
		if err := json.Unmarshal(raw, &request); err != nil {
			fmt.Fprintln(os.Stderr, "invalid request JSON")
			os.Exit(1)
		}
		if err := encoder.Encode(engine.HandleDemo(request)); err != nil {
			os.Exit(1)
		}
	}
	if err := scanner.Err(); err != nil {
		fmt.Fprintln(os.Stderr, err)
		os.Exit(1)
	}
}
