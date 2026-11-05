package license

import (
	"bytes"
	"encoding/json"
	"fmt"
	"unicode/utf16"
)

// Match the existing server's canonical string escaping before hashing the proof body.
func canonicalBody(body interface{}) ([]byte, error) {
	data, err := json.Marshal(body)
	if err != nil {
		return nil, err
	}
	var output bytes.Buffer
	for index := 0; index < len(data); index++ {
		if data[index] != '"' {
			output.WriteByte(data[index])
			continue
		}
		start := index
		index++
		for index < len(data) {
			if data[index] == '\\' {
				index += 2
				continue
			}
			if data[index] == '"' {
				break
			}
			index++
		}
		var value string
		if err := json.Unmarshal(data[start:index+1], &value); err != nil {
			return nil, err
		}
		output.WriteByte('"')
		for _, char := range value {
			switch char {
			case '\\':
				output.WriteString(`\\`)
			case '\b':
				output.WriteString(`\b`)
			case '\f':
				output.WriteString(`\f`)
			case '\n':
				output.WriteString(`\n`)
			case '\r':
				output.WriteString(`\r`)
			case '\t':
				output.WriteString(`\t`)
			default:
				if char < 32 || char >= 127 || char == '"' || char == '\'' || char == '+' || char == '`' || char == '&' || char == '<' || char == '>' {
					for _, unit := range utf16.Encode([]rune{char}) {
						fmt.Fprintf(&output, "\\u%04X", unit)
					}
				} else {
					output.WriteRune(char)
				}
			}
		}
		output.WriteByte('"')
	}
	return output.Bytes(), nil
}
