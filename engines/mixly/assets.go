package main

import "embed"

//go:embed third_party/mixly
var upstream embed.FS

//go:embed src/bootstrap.js
var bootstrap string

//go:embed src/school-bot-arduino.js
var schoolBotArduino string

//go:embed src/school-bot-devices.js
var schoolBotDevices string

//go:embed src/school-bot-modules.js
var schoolBotModules string
