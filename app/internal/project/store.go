package project

import (
	"context"
	"encoding/json"
	"fmt"
	"io"
	"os"
	"path/filepath"
	"strings"
	"sync"
)

// LocalStore manages local v2 project documents with concurrency control and durability.
type LocalStore struct {
	dataDir     string
	projectsDir string
	receiptsDir string
	locksDir    string
	entitlement EntitlementChecker

	mu    sync.RWMutex
	index map[string]ProjectMetadata
}

// NewLocalStore initializes the project storage directory and rebuilds the derived index.
func NewLocalStore(dataDir string, entitlement EntitlementChecker) (*LocalStore, error) {
	if dataDir == "" {
		return nil, &StoreError{Code: ErrStoreUnavailable, Message: "data directory path cannot be empty"}
	}
	if entitlement == nil {
		return nil, &StoreError{Code: ErrStoreUnavailable, Message: "entitlement checker is required"}
	}

	cleanDir := filepath.Clean(dataDir)
	projectsDir := filepath.Join(cleanDir, "projects")
	receiptsDir := filepath.Join(cleanDir, "receipts")
	locksDir := filepath.Join(cleanDir, "locks")

	if err := ensureSafeDir(cleanDir); err != nil {
		return nil, err
	}
	if err := ensureSafeDir(projectsDir); err != nil {
		return nil, err
	}
	if err := ensureSafeDir(receiptsDir); err != nil {
		return nil, err
	}
	if err := ensureSafeDir(locksDir); err != nil {
		return nil, err
	}

	store := &LocalStore{
		dataDir:     cleanDir,
		projectsDir: projectsDir,
		receiptsDir: receiptsDir,
		locksDir:    locksDir,
		entitlement: entitlement,
		index:       make(map[string]ProjectMetadata),
	}

	if err := store.rebuildIndex(); err != nil {
		return nil, err
	}

	return store, nil
}

// DataDir returns the base data directory.
func (s *LocalStore) DataDir() string {
	return s.dataDir
}

// ReceiptsDir returns the directory containing migration receipts.
func (s *LocalStore) ReceiptsDir() string {
	return s.receiptsDir
}

func (s *LocalStore) resolveProjectPath(projectID string) (string, error) {
	if !IsValidUUID(projectID) {
		return "", &StoreError{Code: ErrProjectInvalid, Message: fmt.Sprintf("invalid project_id %q", projectID)}
	}
	target := filepath.Join(s.projectsDir, projectID+".json")
	if err := verifyContainment(s.projectsDir, target); err != nil {
		return "", err
	}
	return target, nil
}

func (s *LocalStore) lockTwoProjects(ctx context.Context, id1, id2 string) (func(), error) {
	first, second := id1, id2
	if first > second {
		first, second = second, first
	}
	u1, err := s.lockProject(ctx, first)
	if err != nil {
		return nil, err
	}
	u2, err := s.lockProject(ctx, second)
	if err != nil {
		u1()
		return nil, err
	}
	return func() {
		u2()
		u1()
	}, nil
}

func (s *LocalStore) readProjectFile(path string) (*LocalProjectDocument, error) {
	if err := verifyContainment(s.projectsDir, path); err != nil {
		return nil, err
	}
	fi, err := os.Lstat(path)
	if os.IsNotExist(err) {
		return nil, &StoreError{Code: ErrNotFound, Message: "project not found"}
	}
	if err != nil {
		return nil, &StoreError{Code: ErrStoreUnavailable, Message: err.Error()}
	}
	if fi.Mode()&os.ModeSymlink != 0 || isReparsePoint(path) {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: "symlinks are not permitted"}
	}
	if fi.Size() > 1<<20 {
		return nil, &StoreError{Code: ErrProjectTooLarge, Message: "saved project file exceeds 1 MiB limit"}
	}

	file, err := os.Open(path)
	if err != nil {
		return nil, &StoreError{Code: ErrStoreUnavailable, Message: err.Error()}
	}
	data, err := io.ReadAll(io.LimitReader(file, (1<<20)+1))
	_ = file.Close()
	if err != nil {
		return nil, &StoreError{Code: ErrStoreUnavailable, Message: err.Error()}
	}
	if len(data) > 1<<20 {
		return nil, &StoreError{Code: ErrProjectTooLarge, Message: "saved project file exceeds 1 MiB limit"}
	}

	expectedID := strings.TrimSuffix(filepath.Base(path), ".json")
	docPtr, err := decodeStrictDocument(data, expectedID)
	if err != nil {
		return nil, err
	}
	doc := *docPtr
	return &doc, nil
}

func (s *LocalStore) atomicWriteDocument(targetPath string, doc *LocalProjectDocument) error {
	data, err := encodeStrictDocument(doc)
	if err != nil {
		return &StoreError{Code: ErrProjectInvalid, Message: fmt.Sprintf("failed to serialize document: %v", err)}
	}
	if len(data) > 1<<20 {
		return &StoreError{Code: ErrProjectTooLarge, Message: "serialized document exceeds 1 MiB limit"}
	}

	dir := filepath.Dir(targetPath)
	if isReparsePoint(dir) {
		return &StoreError{Code: ErrStoreUnavailable, Message: "target directory is a reparse point"}
	}

	tmpFile, err := os.CreateTemp(dir, ".tmp-project-*.tmp")
	if err != nil {
		return &StoreError{Code: ErrStoreUnavailable, Message: fmt.Sprintf("failed to create temp file: %v", err)}
	}
	tmpName := tmpFile.Name()
	defer os.Remove(tmpName)

	if _, err := tmpFile.Write(data); err != nil {
		_ = tmpFile.Close()
		return &StoreError{Code: ErrStoreUnavailable, Message: fmt.Sprintf("failed to write temp file: %v", err)}
	}
	if err := tmpFile.Sync(); err != nil {
		_ = tmpFile.Close()
		return &StoreError{Code: ErrStoreUnavailable, Message: fmt.Sprintf("failed to sync temp file: %v", err)}
	}
	if err := tmpFile.Close(); err != nil {
		return &StoreError{Code: ErrStoreUnavailable, Message: fmt.Sprintf("failed to close temp file: %v", err)}
	}

	if isReparsePoint(dir) {
		return &StoreError{Code: ErrStoreUnavailable, Message: "target directory became a reparse point"}
	}

	if err := os.Rename(tmpName, targetPath); err != nil {
		return &StoreError{Code: ErrStoreUnavailable, Message: fmt.Sprintf("failed to replace project file: %v", err)}
	}

	return nil
}

func (s *LocalStore) updateIndex(doc *LocalProjectDocument) {
	s.mu.Lock()
	defer s.mu.Unlock()

	s.index[doc.ProjectID] = ProjectMetadata{
		ProjectID:     doc.ProjectID,
		Mode:          doc.Mode,
		Name:          doc.Name,
		LocalRevision: doc.LocalRevision,
		UpdatedAt:     doc.UpdatedAt,
	}
	_ = s.persistIndexLocked()
}

func (s *LocalStore) rebuildIndex() error {
	if isReparsePoint(s.projectsDir) {
		return &StoreError{Code: ErrStoreUnavailable, Message: "projects directory is a reparse point"}
	}
	entries, err := os.ReadDir(s.projectsDir)
	if err != nil {
		return &StoreError{Code: ErrStoreUnavailable, Message: fmt.Sprintf("failed to read projects dir: %v", err)}
	}

	s.mu.Lock()
	defer s.mu.Unlock()

	s.index = make(map[string]ProjectMetadata)
	for _, entry := range entries {
		if entry.IsDir() || strings.HasPrefix(entry.Name(), ".") || entry.Name() == "index.json" {
			continue
		}
		if !strings.HasSuffix(entry.Name(), ".json") {
			continue
		}
		projectID := strings.TrimSuffix(entry.Name(), ".json")
		if !IsValidUUID(projectID) {
			continue
		}
		fullPath := filepath.Join(s.projectsDir, entry.Name())
		doc, err := s.readProjectFile(fullPath)
		if err == nil && doc != nil && doc.ProjectID == projectID {
			s.index[doc.ProjectID] = ProjectMetadata{
				ProjectID:     doc.ProjectID,
				Mode:          doc.Mode,
				Name:          doc.Name,
				LocalRevision: doc.LocalRevision,
				UpdatedAt:     doc.UpdatedAt,
			}
		}
	}

	return s.persistIndexLocked()
}

func (s *LocalStore) persistIndexLocked() error {
	data, err := json.MarshalIndent(s.index, "", "  ")
	if err != nil {
		return err
	}
	indexPath := filepath.Join(s.projectsDir, "index.json")
	tmpFile, err := os.CreateTemp(s.projectsDir, ".tmp-index-*.tmp")
	if err != nil {
		return err
	}
	tmpName := tmpFile.Name()
	defer os.Remove(tmpName)

	if _, err := tmpFile.Write(data); err != nil {
		_ = tmpFile.Close()
		return err
	}
	if err := tmpFile.Sync(); err != nil {
		_ = tmpFile.Close()
		return err
	}
	if err := tmpFile.Close(); err != nil {
		return err
	}

	return os.Rename(tmpName, indexPath)
}
