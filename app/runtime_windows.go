package main

import (
	"context"
	"errors"
	"fmt"
	"os"
	"os/exec"
	"path/filepath"
	"strings"
	"time"

	"golang.org/x/sys/windows"
	"golang.org/x/sys/windows/registry"
)

const messageBoxYes = 6

func hasWebView2() bool {
	const key = `Software\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}`
	for _, hive := range []registry.Key{registry.LOCAL_MACHINE, registry.CURRENT_USER} {
		for _, view := range []uint32{registry.WOW64_32KEY, registry.WOW64_64KEY} {
			entry, err := registry.OpenKey(hive, key, registry.QUERY_VALUE|view)
			if err != nil {
				continue
			}
			version, _, err := entry.GetStringValue("pv")
			_ = entry.Close()
			if err == nil && strings.TrimSpace(version) != "" && version != "0.0.0.0" {
				return true
			}
		}
	}
	return false
}

func runtimeMessage(message string, flags uint32) int32 {
	text, _ := windows.UTF16PtrFromString(message)
	title, _ := windows.UTF16PtrFromString("School Bot")
	result, _ := windows.MessageBox(0, text, title, flags)
	return result
}

func reportStartupError(err error) {
	runtimeMessage(err.Error(), windows.MB_OK|windows.MB_ICONERROR)
}

// Install prerequisites outside the MSI transaction. Running a second installer
// inside an MSI custom action would interfere with Windows Installer servicing.
func ensureDesktopRuntime() error {
	if hasWebView2() {
		return nil
	}
	executable, err := os.Executable()
	if err != nil {
		return err
	}
	bootstrapper := filepath.Join(filepath.Dir(executable), "MicrosoftEdgeWebview2Setup.exe")
	if _, err := os.Stat(bootstrapper); err != nil {
		return errors.New("School Bot requires Microsoft WebView2. " +
			"Install the Evergreen Runtime from https://developer.microsoft.com/microsoft-edge/webview2, " +
			"then open School Bot again.")
	}
	message := "School Bot needs Microsoft WebView2 to display the app. Install it now? " +
		"An internet connection is required. Microsoft Setup will show the installation progress."
	if runtimeMessage(message, windows.MB_YESNO|windows.MB_ICONINFORMATION) != messageBoxYes {
		return errRuntimeCancelled
	}
	ctx, cancel := context.WithTimeout(context.Background(), 10*time.Minute)
	defer cancel()
	err = exec.CommandContext(ctx, bootstrapper, "/install").Run()
	if !hasWebView2() {
		return fmt.Errorf("Microsoft WebView2 could not be installed. "+
			"Check your internet connection or contact your IT administrator, "+
			"then open School Bot again: %v", err)
	}
	return nil
}
