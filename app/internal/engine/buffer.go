package engine

import (
	"bytes"
	"strings"
	"unicode/utf8"
)

// boundedBuffer keeps the first limit bytes and drops the rest without failing the write, because
// an error here would break the child's pipe and turn a verbose engine into a crash.
// It deliberately does not embed bytes.Buffer: the promoted ReadFrom would let io.Copy bypass the limit.
type boundedBuffer struct {
	data      bytes.Buffer
	limit     int
	truncated bool
}

func (b *boundedBuffer) Write(p []byte) (int, error) {
	kept := p
	if room := max(b.limit-b.data.Len(), 0); room < len(p) {
		b.truncated = true
		kept = p[:room]
	}
	b.data.Write(kept)
	return len(p), nil
}

func (b *boundedBuffer) Bytes() []byte {
	return b.data.Bytes()
}

func (b *boundedBuffer) excerpt(limit int) string {
	text := strings.ToValidUTF8(strings.Join(strings.Fields(b.data.String()), " "), "")
	if len(text) <= limit {
		return text
	}
	cut := limit
	for cut > 0 && !utf8.RuneStart(text[cut]) {
		cut--
	}
	return text[:cut] + "…"
}
