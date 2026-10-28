//go:build !windows

package main

import (
	"os/exec"
	"runtime"
)

// macOS provides WKWebView, so no separate runtime installation is needed.
func ensureDesktopRuntime() error { return nil }

func reportStartupError(err error) {
	if runtime.GOOS == "darwin" {
		// Pass diagnostics as argv so quotes or newlines cannot become AppleScript code.
		script := "on run argv\n display dialog (item 1 of argv) with title \"School Bot\" " +
			"buttons {\"OK\"} default button 1 with icon stop\nend run"
		_ = exec.Command("/usr/bin/osascript", "-e", script, err.Error()).Run()
	}
}
