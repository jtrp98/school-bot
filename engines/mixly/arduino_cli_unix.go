//go:build !windows

package main

import (
	"errors"
	"fmt"
	"os"
	"os/user"
	"path/filepath"
	"runtime"
	"syscall"
)

// The host starts the engine without HOME, so the account database names the home folder,
// here and for arduino-cli (which keeps its data in ~/Library/Arduino15 on macOS).
func arduinoUserFolders() arduinoFolders {
	home := ""
	if current, err := user.Current(); err == nil {
		home = current.HomeDir
	}
	folders := arduinoFolders{home: home}
	if runtime.GOOS == "darwin" {
		folders.ideResources = []string{filepath.Join("/Applications", "Arduino IDE.app", "Contents", "Resources")}
		if home != "" {
			folders.ideResources = append(folders.ideResources,
				filepath.Join(home, "Applications", "Arduino IDE.app", "Contents", "Resources"))
		}
	}
	if home != "" {
		folders.env = []string{"HOME=" + home}
	}
	return folders
}

// The temporary folder can be shared by every account (/tmp on Linux), where another user could
// create the build folder first and swap the program that gets uploaded. Only a real folder this
// account owns, closed to everyone else, is used.
func checkPrivateDirectory(path string) error {
	info, err := os.Lstat(path)
	if err != nil {
		return err
	}
	stat, ok := info.Sys().(*syscall.Stat_t)
	if !info.IsDir() || info.Mode()&os.ModeSymlink != 0 || !ok ||
		int(stat.Uid) != os.Getuid() || info.Mode().Perm()&0o077 != 0 {
		return fmt.Errorf("%s is not a private folder of this account", path)
	}
	return nil
}

// lockFile holds an exclusive lock on path until release is called or the engine exits.
func lockFile(path string) (release func(), busy bool, err error) {
	file, err := os.OpenFile(path, os.O_RDWR|os.O_CREATE, 0o600)
	if err != nil {
		return nil, false, err
	}
	if err := syscall.Flock(int(file.Fd()), syscall.LOCK_EX|syscall.LOCK_NB); err != nil {
		file.Close()
		if errors.Is(err, syscall.EWOULDBLOCK) {
			return nil, true, nil
		}
		return nil, false, err
	}
	return func() { file.Close() }, false, nil
}
