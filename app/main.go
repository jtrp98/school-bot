package main

import (
	"embed"
	"errors"
	"github.com/wailsapp/wails/v3/pkg/application"
	"github.com/wailsapp/wails/v3/pkg/events"
	"log"
	"sync/atomic"
)

//go:embed all:frontend/dist
var assets embed.FS

//go:embed build/windows/appicon.png
var appIcon []byte

var errRuntimeCancelled = errors.New("WebView2 setup cancelled")

func startupFailure(err error) {
	log.Print(err)
	if !errors.Is(err, errRuntimeCancelled) {
		reportStartupError(err)
	}
}

func main() {
	if err := ensureDesktopRuntime(); err != nil {
		startupFailure(err)
		return
	}
	service, err := NewDesktopService()
	if err != nil {
		startupFailure(err)
		return
	}
	defer service.CancelRun()
	desktop := application.New(application.Options{
		Name: "School Bot", Description: "Offline-first robotics and coding PoC",
		Icon:     appIcon,
		Services: []application.Service{application.NewService(service)},
		Assets:   application.AssetOptions{Handler: application.AssetFileServerFS(assets)},
		Mac:      application.MacOptions{ApplicationShouldTerminateAfterLastWindowClosed: true},
	})
	mainWindow := desktop.Window.NewWithOptions(application.WebviewWindowOptions{
		Title: "School Bot", Width: 1200, Height: 800, MinWidth: 900, MinHeight: 640,
		URL: "/", BackgroundColour: application.NewRGB(251, 251, 253),
		Frameless: true, StartState: application.WindowStateFullscreen,
		// Wails beta.30 centres after applying StartState, which can move a
		// fullscreen window above the work area by half the taskbar height.
		InitialPosition: application.WindowXY, X: 0, Y: 0,
		Windows: application.WindowsWindow{DisableFramelessWindowDecorations: true},
	})
	// Route native Close/Alt+F4 through the same confirmation and robot-stop
	// guards as the in-app button. Closing is approved only after those finish.
	var closeApproved atomic.Bool
	mainWindow.RegisterHook(events.Common.WindowClosing, func(event *application.WindowEvent) {
		if !closeApproved.Load() {
			event.Cancel()
			desktop.Event.Emit("school-bot:close-requested")
		}
	})
	service.closeWindow = func() {
		closeApproved.Store(true)
		mainWindow.Close()
	}
	if err := desktop.Run(); err != nil {
		startupFailure(err)
	}
}
