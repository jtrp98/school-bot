package main

import (
	"bufio"
	"encoding/json"
	"fmt"
	"io"
	"os"
)

func serve(input io.Reader, output io.Writer) error {
	reader := bufio.NewReaderSize(input, 4096)
	encoder := json.NewEncoder(output)
	for {
		line := make([]byte, 0, 4096)
		oversized := false
		for {
			part, err := reader.ReadSlice('\n')
			if len(line)+len(part) > maxRequestBytes+1 {
				oversized = true
			}
			if !oversized {
				line = append(line, part...)
			}
			if err == bufio.ErrBufferFull {
				continue
			}
			if err != nil && err != io.EOF {
				return err
			}
			if len(line) == 0 && !oversized && err == io.EOF {
				return nil
			}
			if len(line) > 0 && line[len(line)-1] == '\n' {
				line = line[:len(line)-1]
			}
			response := failure("", "INVALID_REQUEST", "request exceeds 64 KB")
			if !oversized && len(line) <= maxRequestBytes {
				response = handleLine(line)
			}
			if err := encoder.Encode(response); err != nil {
				return err
			}
			break
		}
	}
}

func main() {
	if err := serve(os.Stdin, os.Stdout); err != nil {
		fmt.Fprintln(os.Stderr, err)
		os.Exit(1)
	}
}
