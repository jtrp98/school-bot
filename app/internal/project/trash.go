package project

import (
	"bytes"
	"context"
	"encoding/json"
	"fmt"
	"io"
	"os"
	"path/filepath"
	"sort"
	"strings"
	"time"
)

const trashRetention = 30 * 24 * time.Hour

// TrashDir returns the directory containing soft-deleted guest projects.
func (s *LocalStore) TrashDir() string {
	return s.trashDir
}

func (s *LocalStore) resolveTrashPath(projectID string) (string, error) {
	if !IsValidUUID(projectID) {
		return "", &StoreError{Code: ErrProjectInvalid, Message: fmt.Sprintf("invalid project_id %q", projectID)}
	}
	target := filepath.Join(s.trashDir, projectID+".json")
	if err := verifyContainment(s.trashDir, target); err != nil {
		return "", err
	}
	return target, nil
}

func (s *LocalStore) removeFromIndex(projectID string) {
	s.mu.Lock()
	defer s.mu.Unlock()
	delete(s.index, projectID)
	_ = s.persistIndexLocked()
}

func encodeTrashEnvelope(env *TrashEnvelope) ([]byte, error) {
	if env == nil || env.Project == nil {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: "invalid trash envelope"}
	}
	projBytes, err := encodeStrictDocument(env.Project)
	if err != nil {
		return nil, err
	}
	root := map[string]any{
		"schema":     1,
		"project":    json.RawMessage(projBytes),
		"deleted_at": env.DeletedAt,
		"expires_at": env.ExpiresAt,
	}
	return json.MarshalIndent(root, "", "  ")
}

func decodeStrictTrashEnvelope(data []byte, expectedID string) (*TrashEnvelope, error) {
	if len(data) > (1<<20)+4096 {
		return nil, &StoreError{Code: ErrProjectTooLarge, Message: "trash file exceeds size limit"}
	}
	if err := validateStrictJSONStructure(data); err != nil {
		return nil, err
	}
	var rawRoot map[string]json.RawMessage
	if err := json.Unmarshal(data, &rawRoot); err != nil {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: fmt.Sprintf("invalid trash JSON: %v", err)}
	}
	required := []string{"schema", "project", "deleted_at", "expires_at"}
	if len(rawRoot) != len(required) {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: "unknown or missing trash envelope fields"}
	}
	for _, k := range required {
		b, ok := rawRoot[k]
		if !ok || bytes.Equal(bytes.TrimSpace(b), []byte("null")) {
			return nil, &StoreError{Code: ErrProjectInvalid, Message: fmt.Sprintf("missing or null trash field %q", k)}
		}
	}
	var schema int
	if err := json.Unmarshal(rawRoot["schema"], &schema); err != nil || schema != 1 {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: "trash schema must be 1"}
	}
	var deletedAt, expiresAt string
	if err := json.Unmarshal(rawRoot["deleted_at"], &deletedAt); err != nil {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: "invalid deleted_at"}
	}
	if _, err := time.Parse(time.RFC3339, deletedAt); err != nil {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: "deleted_at must be RFC3339"}
	}
	if err := json.Unmarshal(rawRoot["expires_at"], &expiresAt); err != nil {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: "invalid expires_at"}
	}
	if _, err := time.Parse(time.RFC3339, expiresAt); err != nil {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: "expires_at must be RFC3339"}
	}
	doc, err := decodeStrictDocument(rawRoot["project"], expectedID)
	if err != nil {
		return nil, err
	}
	return &TrashEnvelope{
		Schema:    schema,
		Project:   doc,
		DeletedAt: deletedAt,
		ExpiresAt: expiresAt,
	}, nil
}

func (s *LocalStore) readTrashFile(path string) (*TrashEnvelope, error) {
	if err := verifyContainment(s.trashDir, path); err != nil {
		return nil, err
	}
	fi, err := os.Lstat(path)
	if os.IsNotExist(err) {
		return nil, &StoreError{Code: ErrNotFound, Message: "trash item not found"}
	}
	if err != nil {
		return nil, &StoreError{Code: ErrStoreUnavailable, Message: err.Error()}
	}
	if fi.Mode()&os.ModeSymlink != 0 || isReparsePoint(path) {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: "symlinks are not permitted"}
	}
	if fi.Size() > (1<<20)+4096 {
		return nil, &StoreError{Code: ErrProjectTooLarge, Message: "trash file exceeds size limit"}
	}
	file, err := os.Open(path)
	if err != nil {
		return nil, &StoreError{Code: ErrStoreUnavailable, Message: err.Error()}
	}
	data, err := io.ReadAll(io.LimitReader(file, (1<<20)+4097))
	_ = file.Close()
	if err != nil {
		return nil, &StoreError{Code: ErrStoreUnavailable, Message: err.Error()}
	}
	expectedID := strings.TrimSuffix(filepath.Base(path), ".json")
	return decodeStrictTrashEnvelope(data, expectedID)
}

func (s *LocalStore) atomicWriteTrash(targetPath string, env *TrashEnvelope) error {
	data, err := encodeTrashEnvelope(env)
	if err != nil {
		return err
	}
	dir := filepath.Dir(targetPath)
	if isReparsePoint(dir) {
		return &StoreError{Code: ErrStoreUnavailable, Message: "trash directory is a reparse point"}
	}
	tmpFile, err := os.CreateTemp(dir, ".tmp-trash-*.tmp")
	if err != nil {
		return &StoreError{Code: ErrStoreUnavailable, Message: fmt.Sprintf("failed to create trash temp file: %v", err)}
	}
	tmpName := tmpFile.Name()
	defer os.Remove(tmpName)

	if _, err := tmpFile.Write(data); err != nil {
		_ = tmpFile.Close()
		return &StoreError{Code: ErrStoreUnavailable, Message: fmt.Sprintf("failed to write trash temp file: %v", err)}
	}
	if err := tmpFile.Sync(); err != nil {
		_ = tmpFile.Close()
		return &StoreError{Code: ErrStoreUnavailable, Message: fmt.Sprintf("failed to sync trash temp file: %v", err)}
	}
	if err := tmpFile.Close(); err != nil {
		return &StoreError{Code: ErrStoreUnavailable, Message: fmt.Sprintf("failed to close trash temp file: %v", err)}
	}
	if isReparsePoint(dir) {
		return &StoreError{Code: ErrStoreUnavailable, Message: "trash directory became a reparse point"}
	}
	if err := os.Rename(tmpName, targetPath); err != nil {
		return &StoreError{Code: ErrStoreUnavailable, Message: fmt.Sprintf("failed to replace trash file: %v", err)}
	}
	return nil
}

// DeleteProject soft-deletes an active local project into the 30-day trash.
func (s *LocalStore) DeleteProject(ctx context.Context, projectID string) (*TrashMetadata, error) {
	if err := s.entitlement.CheckEntitlement(ctx); err != nil {
		return nil, err
	}
	projectPath, err := s.resolveProjectPath(projectID)
	if err != nil {
		return nil, err
	}
	trashPath, err := s.resolveTrashPath(projectID)
	if err != nil {
		return nil, err
	}

	unlock, err := s.lockProject(ctx, projectID)
	if err != nil {
		return nil, err
	}
	defer unlock()

	doc, err := s.readProjectFile(projectPath)
	if err != nil {
		return nil, err
	}

	if err := s.entitlement.CheckEntitlement(ctx); err != nil {
		return nil, err
	}

	now := s.now()
	deletedAt := now.Format(time.RFC3339)
	expiresAt := now.Add(trashRetention).Format(time.RFC3339)

	env := &TrashEnvelope{
		Schema:    1,
		Project:   doc,
		DeletedAt: deletedAt,
		ExpiresAt: expiresAt,
	}
	if err := s.atomicWriteTrash(trashPath, env); err != nil {
		return nil, err
	}
	if err := os.Remove(projectPath); err != nil && !os.IsNotExist(err) {
		_ = os.Remove(trashPath)
		return nil, &StoreError{Code: ErrStoreUnavailable, Message: fmt.Sprintf("failed to remove active project after trash write: %v", err)}
	}

	s.removeFromIndex(projectID)
	return &TrashMetadata{
		ProjectID:     doc.ProjectID,
		Mode:          doc.Mode,
		Name:          doc.Name,
		LocalRevision: doc.LocalRevision,
		UpdatedAt:     doc.UpdatedAt,
		DeletedAt:     deletedAt,
		ExpiresAt:     expiresAt,
	}, nil
}

// ListTrash lists non-expired soft-deleted projects and sweeps expired items when clock is trusted.
func (s *LocalStore) ListTrash(ctx context.Context, mode string) ([]TrashMetadata, error) {
	if err := s.entitlement.CheckEntitlement(ctx); err != nil {
		return nil, err
	}
	if mode != "" && !ValidMode(mode) {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: fmt.Sprintf("invalid mode %q", mode)}
	}
	if isReparsePoint(s.trashDir) {
		return nil, &StoreError{Code: ErrStoreUnavailable, Message: "trash directory is a reparse point"}
	}

	now := s.now()
	clockTrusted := s.checkClockTrust(now) == nil

	entries, err := os.ReadDir(s.trashDir)
	if err != nil {
		return nil, &StoreError{Code: ErrStoreUnavailable, Message: fmt.Sprintf("failed to read trash dir: %v", err)}
	}

	results := make([]TrashMetadata, 0)
	for _, entry := range entries {
		if entry.IsDir() || strings.HasPrefix(entry.Name(), ".") || !strings.HasSuffix(entry.Name(), ".json") {
			continue
		}
		projectID := strings.TrimSuffix(entry.Name(), ".json")
		if !IsValidUUID(projectID) {
			continue
		}
		fullPath := filepath.Join(s.trashDir, entry.Name())
		env, rErr := s.readTrashFile(fullPath)
		if rErr != nil || env == nil || env.Project == nil {
			continue
		}
		expiresAt, pErr := time.Parse(time.RFC3339, env.ExpiresAt)
		if pErr != nil {
			continue
		}
		if clockTrusted && !now.Before(expiresAt) {
			if unlock, lErr := s.lockProject(ctx, projectID); lErr == nil {
				_ = os.Remove(fullPath)
				unlock()
			}
			continue
		}
		if mode != "" && env.Project.Mode != mode {
			continue
		}
		results = append(results, TrashMetadata{
			ProjectID:     env.Project.ProjectID,
			Mode:          env.Project.Mode,
			Name:          env.Project.Name,
			LocalRevision: env.Project.LocalRevision,
			UpdatedAt:     env.Project.UpdatedAt,
			DeletedAt:     env.DeletedAt,
			ExpiresAt:     env.ExpiresAt,
		})
	}

	sort.Slice(results, func(i, j int) bool {
		if results[i].DeletedAt == results[j].DeletedAt {
			return results[i].Name < results[j].Name
		}
		return results[i].DeletedAt > results[j].DeletedAt
	})

	return results, nil
}

// RestoreProject restores a non-expired soft-deleted project back to active local storage.
func (s *LocalStore) RestoreProject(ctx context.Context, projectID string) (*LocalProjectDocument, error) {
	if err := s.entitlement.CheckEntitlement(ctx); err != nil {
		return nil, err
	}
	now := s.now()
	if err := s.checkClockTrust(now); err != nil {
		return nil, err
	}

	projectPath, err := s.resolveProjectPath(projectID)
	if err != nil {
		return nil, err
	}
	trashPath, err := s.resolveTrashPath(projectID)
	if err != nil {
		return nil, err
	}

	unlock, err := s.lockProject(ctx, projectID)
	if err != nil {
		return nil, err
	}
	defer unlock()

	env, err := s.readTrashFile(trashPath)
	if err != nil {
		return nil, err
	}

	expiresAt, err := time.Parse(time.RFC3339, env.ExpiresAt)
	if err != nil {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: "invalid trash expires_at"}
	}
	if !now.Before(expiresAt) {
		_ = os.Remove(trashPath)
		return nil, &StoreError{Code: ErrTrashExpired, Message: "trash item has expired"}
	}

	if _, err := os.Stat(projectPath); err == nil {
		return nil, &StoreError{Code: ErrProjectExists, Message: "active project with same ID already exists"}
	}

	if err := s.entitlement.CheckEntitlement(ctx); err != nil {
		return nil, err
	}

	if err := s.atomicWriteDocument(projectPath, env.Project); err != nil {
		return nil, err
	}
	if err := os.Remove(trashPath); err != nil && !os.IsNotExist(err) {
		_ = os.Remove(projectPath)
		return nil, &StoreError{Code: ErrStoreUnavailable, Message: fmt.Sprintf("failed to remove trash file after restore: %v", err)}
	}

	s.updateIndex(env.Project)
	return env.Project, nil
}
