package engine

import (
	"os"
	"runtime"
)

func engineEnvironment() []string {
	// Inheriting the host environment would expose license/API secrets to every engine.
	// Windows needs its system directory to load runtime DLLs; Unix only needs a writable temp directory.
	if runtime.GOOS == "windows" {
		return []string{
			"SYSTEMROOT=" + os.Getenv("SYSTEMROOT"),
			"WINDIR=" + os.Getenv("WINDIR"),
			"TEMP=" + os.TempDir(),
			"TMP=" + os.TempDir(),
		}
	}
	return []string{"TMPDIR=" + os.TempDir()}
}
