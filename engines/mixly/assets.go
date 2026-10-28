package main

import "embed"

//go:embed third_party/mixly
var upstream embed.FS

//go:embed src/bootstrap.js
var bootstrap string
