//go:build windows

package main

import (
	"fmt"
	"os"

	"golang.org/x/sys/windows"
)

// The host starts the engine without the user's environment, so Windows' known folders stand
// in for %LOCALAPPDATA%, %ProgramFiles% and %USERPROFILE%, here and for arduino-cli.
func arduinoUserFolders() arduinoFolders {
	folder := func(id *windows.KNOWNFOLDERID) string {
		path, err := windows.KnownFolderPath(id, windows.KF_FLAG_DEFAULT)
		if err != nil {
			return ""
		}
		return path
	}
	profile := folder(windows.FOLDERID_Profile)
	folders := arduinoFolders{home: profile}
	// Arduino IDE 2 installs per user (%LOCALAPPDATA%\Programs) or for all users.
	folders.ideResources = windowsIDEResources(
		folder(windows.FOLDERID_UserProgramFiles), folder(windows.FOLDERID_ProgramFiles))
	for name, path := range map[string]string{
		"USERPROFILE":  profile,
		"LOCALAPPDATA": folder(windows.FOLDERID_LocalAppData),
		"APPDATA":      folder(windows.FOLDERID_RoamingAppData),
	} {
		if path != "" {
			folders.env = append(folders.env, name+"="+path)
		}
	}
	return folders
}

// %TEMP% is inside the user's own profile, so the build folder only has to be a real folder:
// a junction or link there would send the compiler's output somewhere else.
func checkPrivateDirectory(path string) error {
	info, err := os.Lstat(path)
	if err != nil {
		return err
	}
	if !info.IsDir() || info.Mode()&(os.ModeSymlink|os.ModeIrregular) != 0 {
		return fmt.Errorf("%s is not a plain folder", path)
	}
	return nil
}

// lockFile holds an exclusive lock on path until release is called or the engine exits.
func lockFile(path string) (release func(), busy bool, err error) {
	file, err := os.OpenFile(path, os.O_RDWR|os.O_CREATE, 0o600)
	if err != nil {
		return nil, false, err
	}
	handle := windows.Handle(file.Fd())
	overlapped := new(windows.Overlapped)
	flags := uint32(windows.LOCKFILE_EXCLUSIVE_LOCK | windows.LOCKFILE_FAIL_IMMEDIATELY)
	if err := windows.LockFileEx(handle, flags, 0, 1, 0, overlapped); err != nil {
		file.Close()
		if err == windows.ERROR_LOCK_VIOLATION {
			return nil, true, nil
		}
		return nil, false, err
	}
	return func() {
		_ = windows.UnlockFileEx(handle, 0, 1, 0, overlapped)
		file.Close()
	}, false, nil
}
