package main

import (
	"debug/elf"
	"errors"
	"io/fs"
	"os"
	"path/filepath"
	"strings"
)

// arduinoWorkspace is the per-user sketch and build folder kept between requests: engines are
// started per request, and without it every upload would compile U8g2's fonts from scratch.
// arduino-cli rebuilds only what changed. The lock keeps a second School Bot from compiling
// into it, or replacing the program, between this compile and its upload.
type arduinoWorkspace struct {
	sketch string
	build  string
	// Present while a compile runs, so one that was killed (by its deadline, or with the engine
	// when the host cancels) is noticed by the next compile.
	marker  string
	release func()
}

// dropUnfinishedBuild removes what a killed compile may have cut short: object files whose ELF
// section table is incomplete, with their dependency files, and the archives, which arduino-cli
// rebuilds from the objects. A cut-short object is newer than its source, so arduino-cli would
// link it on every later compile. Finished objects stay, so a slow PC that ran out of time keeps
// what it compiled and gets further on the next try.
func dropUnfinishedBuild(build string) error {
	return filepath.WalkDir(build, func(path string, entry fs.DirEntry, err error) error {
		if err != nil || entry.IsDir() {
			return err
		}
		switch filepath.Ext(path) {
		case ".a":
			return os.Remove(path)
		case ".o":
			if object, err := elf.Open(path); err == nil {
				return object.Close()
			}
			if err := os.Remove(strings.TrimSuffix(path, ".o") + ".d"); err != nil && !errors.Is(err, fs.ErrNotExist) {
				return err
			}
			return os.Remove(path)
		}
		return nil
	})
}

func openArduinoWorkspace(temp, fqbn string) (arduinoWorkspace, error) {
	name := unsafeInFQBN.ReplaceAllString(fqbn, "_")
	root := filepath.Join(temp, "school-bot-mixly")
	base := filepath.Join(root, name)
	for _, directory := range []string{root, base} {
		if err := os.Mkdir(directory, 0o700); err != nil && !errors.Is(err, os.ErrExist) {
			return arduinoWorkspace{}, err
		}
		if err := checkPrivateDirectory(directory); err != nil {
			return arduinoWorkspace{}, err
		}
	}
	release, busy, err := lockFile(filepath.Join(base, "lock"))
	if err != nil {
		return arduinoWorkspace{}, err
	}
	if busy {
		return arduinoWorkspace{}, &ProtocolError{"ENGINE_BUSY", "another School Bot compile or upload is running"}
	}
	workspace := arduinoWorkspace{
		sketch:  filepath.Join(base, "program"),
		build:   filepath.Join(base, "build"),
		marker:  filepath.Join(base, "compiling"),
		release: release,
	}
	for _, directory := range []string{workspace.sketch, workspace.build} {
		if err := os.Mkdir(directory, 0o700); err != nil && !errors.Is(err, os.ErrExist) {
			release()
			return arduinoWorkspace{}, err
		}
		if err := checkPrivateDirectory(directory); err != nil {
			release()
			return arduinoWorkspace{}, err
		}
	}
	return workspace, nil
}
