package project

import (
	"fmt"
	"os"
	"path/filepath"
	"strings"
)

func verifyContainment(root, target string) error {
	cleanRoot := filepath.Clean(root)
	cleanTarget := filepath.Clean(target)

	rel, err := filepath.Rel(cleanRoot, cleanTarget)
	if err != nil || strings.HasPrefix(rel, "..") || rel == ".." {
		return &StoreError{Code: ErrStoreUnavailable, Message: "target path escapes root directory"}
	}

	curr := cleanRoot
	for _, part := range strings.Split(rel, string(filepath.Separator)) {
		if part == "" || part == "." {
			continue
		}
		curr = filepath.Join(curr, part)
		if isReparsePoint(curr) {
			return &StoreError{Code: ErrStoreUnavailable, Message: fmt.Sprintf("reparse point detected at %s", curr)}
		}
	}
	return nil
}

func ensureSafeDir(path string) error {
	if isReparsePoint(path) {
		return &StoreError{Code: ErrStoreUnavailable, Message: fmt.Sprintf("directory %s is a reparse point", path)}
	}
	if err := os.MkdirAll(path, 0700); err != nil {
		return &StoreError{Code: ErrStoreUnavailable, Message: fmt.Sprintf("failed to create directory %s: %v", path, err)}
	}
	if isReparsePoint(path) {
		return &StoreError{Code: ErrStoreUnavailable, Message: fmt.Sprintf("directory %s is a reparse point after creation", path)}
	}
	return nil
}
