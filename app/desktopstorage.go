package main

import (
	"encoding/hex"
	"encoding/json"
	"errors"
	"io"
	"os"
	"path/filepath"
	"schoolbot.local/app/internal/engine"
)

func readBoundedFile(path string, limit int64) ([]byte, error) {
	file, err := os.Open(path)
	if err != nil {
		return nil, err
	}
	defer file.Close()
	info, err := file.Stat()
	if err != nil {
		return nil, err
	}
	if info.Size() > limit {
		return nil, errors.New("saved file exceeds size limit")
	}
	// The file can grow after Stat, so the read must have its own bound as well.
	data, err := io.ReadAll(io.LimitReader(file, limit+1))
	if err != nil {
		return nil, err
	}
	if int64(len(data)) > limit {
		return nil, errors.New("saved file exceeds size limit")
	}
	return data, nil
}

func validDeviceID(data []byte) bool {
	if len(data) != 32 {
		return false
	}
	_, err := hex.DecodeString(string(data))
	return err == nil
}

// Keep the device ID so a product rename does not consume a second license seat.
func prepareDataDirectory(config string) (string, error) {
	dir := filepath.Join(config, "SchoolBotPoC")
	if err := os.MkdirAll(dir, 0700); err != nil {
		return "", err
	}
	legacy := filepath.Join(config, "ArmStudioPoC")
	for _, name := range []string{"device-id", "robot-control.json", "coding-adventure.json", "block-programming.json"} {
		target := filepath.Join(dir, name)
		if _, err := os.Stat(target); err == nil {
			continue
		} else if !os.IsNotExist(err) {
			return "", err
		}
		file, err := os.Open(filepath.Join(legacy, name))
		if os.IsNotExist(err) {
			continue
		}
		if err != nil {
			return "", err
		}
		data, err := io.ReadAll(io.LimitReader(file, (1<<20)+1))
		_ = file.Close()
		if err != nil {
			return "", err
		}
		if len(data) > 1<<20 {
			return "", errors.New("legacy project exceeds 1 MB")
		}
		if name == "device-id" {
			if len(data) != 32 {
				return "", errors.New("invalid legacy device ID")
			}
			if _, err := hex.DecodeString(string(data)); err != nil {
				return "", errors.New("invalid legacy device ID")
			}
		} else if !json.Valid(data) {
			return "", errors.New("invalid legacy project JSON")
		}
		out, err := os.OpenFile(target, os.O_WRONLY|os.O_CREATE|os.O_EXCL, 0600)
		if os.IsExist(err) {
			continue
		}
		if err != nil {
			return "", err
		}
		_, writeErr := out.Write(data)
		closeErr := out.Close()
		if writeErr != nil {
			return "", writeErr
		}
		if closeErr != nil {
			return "", closeErr
		}
	}
	return dir, nil
}

func (s *DesktopService) SaveProject(mode, data string) error {
	if !engine.ValidMode(mode) || len(data) > 1<<20 || !json.Valid([]byte(data)) {
		return errors.New("invalid mode or project JSON (max 1 MB)")
	}
	file, err := os.CreateTemp(s.dataDir, "project-*.tmp")
	if err != nil {
		return err
	}
	name := file.Name()
	defer os.Remove(name)
	if _, err = file.WriteString(data); err != nil {
		_ = file.Close()
		return err
	}
	if err = file.Sync(); err != nil {
		_ = file.Close()
		return err
	}
	if err = file.Close(); err != nil {
		return err
	}
	return os.Rename(name, filepath.Join(s.dataDir, mode+".json"))
}
func (s *DesktopService) LoadProject(mode string) (string, error) {
	if !engine.ValidMode(mode) {
		return "", errors.New("unknown mode")
	}
	data, err := readBoundedFile(filepath.Join(s.dataDir, mode+".json"), 1<<20)
	if os.IsNotExist(err) {
		return "", nil
	}
	if err != nil {
		return "", err
	}
	if len(data) > 1<<20 || !json.Valid(data) {
		return "", errors.New("saved project is invalid")
	}
	return string(data), nil
}
