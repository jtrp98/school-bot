//go:build windows

package project

import (
	"os"
	"syscall"

	"golang.org/x/sys/windows"
)

func isReparsePoint(path string) bool {
	fi, err := os.Lstat(path)
	if err != nil {
		return false
	}
	if fi.Mode()&os.ModeSymlink != 0 {
		return true
	}
	if sys, ok := fi.Sys().(*syscall.Win32FileAttributeData); ok {
		if sys.FileAttributes&windows.FILE_ATTRIBUTE_REPARSE_POINT != 0 {
			return true
		}
	}
	return false
}
