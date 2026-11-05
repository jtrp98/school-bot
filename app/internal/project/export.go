package project

import (
	"bytes"
	"context"
	"encoding/json"
	"fmt"
	"io"
	"os"
	"path/filepath"
	"strings"
	"time"
)

// ExportExtension is the canonical file extension for portable guest project exports.
const ExportExtension = ".schoolbot-project.json"

// ExportProject serializes an active local project into the portable .schoolbot-project.json v2 envelope.
// Internal identifiers (project_id, local_revision, cloud IDs) are omitted from the portable envelope.
func (s *LocalStore) ExportProject(ctx context.Context, projectID string) ([]byte, error) {
	doc, err := s.GetProject(ctx, projectID)
	if err != nil {
		return nil, err
	}
	contentData, err := encodeStrictContent(doc.Content)
	if err != nil {
		return nil, err
	}

	root := map[string]any{
		"schema":      2,
		"exported_at": s.now().Format(time.RFC3339),
		"mode":        doc.Mode,
		"name":        doc.Name,
		"content":     json.RawMessage(contentData),
	}
	data, err := json.MarshalIndent(root, "", "  ")
	if err != nil {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: fmt.Sprintf("failed to encode export: %v", err)}
	}
	if len(data) > 1<<20 {
		return nil, &StoreError{Code: ErrProjectTooLarge, Message: "exported project exceeds 1 MiB limit"}
	}
	return data, nil
}

// ExportProjectToFile atomically writes a portable .schoolbot-project.json export to destPath.
func (s *LocalStore) ExportProjectToFile(ctx context.Context, projectID string, destPath string) error {
	cleanDest := filepath.Clean(destPath)
	if !strings.HasSuffix(strings.ToLower(cleanDest), ExportExtension) {
		return &StoreError{
			Code:    ErrProjectInvalid,
			Message: fmt.Sprintf("export file must use %s extension", ExportExtension),
		}
	}
	data, err := s.ExportProject(ctx, projectID)
	if err != nil {
		return err
	}

	dir := filepath.Dir(cleanDest)
	if err := ensureSafeDir(dir); err != nil {
		return err
	}

	tmpFile, err := os.CreateTemp(dir, ".tmp-export-*.tmp")
	if err != nil {
		return &StoreError{Code: ErrStoreUnavailable, Message: fmt.Sprintf("failed to create export temp file: %v", err)}
	}
	tmpName := tmpFile.Name()
	defer os.Remove(tmpName)

	if _, err := tmpFile.Write(data); err != nil {
		_ = tmpFile.Close()
		return &StoreError{Code: ErrStoreUnavailable, Message: fmt.Sprintf("failed to write export temp file: %v", err)}
	}
	if err := tmpFile.Sync(); err != nil {
		_ = tmpFile.Close()
		return &StoreError{Code: ErrStoreUnavailable, Message: fmt.Sprintf("failed to sync export temp file: %v", err)}
	}
	if err := tmpFile.Close(); err != nil {
		return &StoreError{Code: ErrStoreUnavailable, Message: fmt.Sprintf("failed to close export temp file: %v", err)}
	}
	if isReparsePoint(dir) {
		return &StoreError{Code: ErrStoreUnavailable, Message: "export destination directory is a reparse point"}
	}
	if err := os.Rename(tmpName, cleanDest); err != nil {
		return &StoreError{Code: ErrStoreUnavailable, Message: fmt.Sprintf("failed to finalize export file: %v", err)}
	}
	return nil
}

func decodeStrictExportEnvelope(data []byte) (*ExportedProjectEnvelope, error) {
	if len(data) > 1<<20 {
		return nil, &StoreError{Code: ErrProjectTooLarge, Message: "exported project file exceeds 1 MiB limit"}
	}
	if err := validateStrictJSONStructure(data); err != nil {
		return nil, err
	}

	var rawRoot map[string]json.RawMessage
	if err := json.Unmarshal(data, &rawRoot); err != nil {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: fmt.Sprintf("invalid JSON: %v", err)}
	}

	required := []string{"schema", "exported_at", "mode", "name", "content"}
	if len(rawRoot) != len(required) {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: "unknown or missing export envelope fields"}
	}
	for _, k := range required {
		b, ok := rawRoot[k]
		if !ok || bytes.Equal(bytes.TrimSpace(b), []byte("null")) {
			return nil, &StoreError{Code: ErrProjectInvalid, Message: fmt.Sprintf("missing or null field %q", k)}
		}
	}

	var schema int
	if err := json.Unmarshal(rawRoot["schema"], &schema); err != nil || schema != 2 {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: "export schema must be 2"}
	}

	var exportedAt string
	if err := json.Unmarshal(rawRoot["exported_at"], &exportedAt); err != nil {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: "invalid exported_at"}
	}
	if _, err := time.Parse(time.RFC3339, exportedAt); err != nil {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: "exported_at must be RFC3339"}
	}

	var mode string
	if err := json.Unmarshal(rawRoot["mode"], &mode); err != nil || !ValidMode(mode) {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: fmt.Sprintf("invalid mode %q", mode)}
	}

	var name string
	if err := json.Unmarshal(rawRoot["name"], &name); err != nil {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: "invalid name"}
	}
	if nr := len([]rune(name)); nr < 1 || nr > 200 {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: "name length must be 1..200"}
	}

	content, err := decodeStrictContent(mode, rawRoot["content"])
	if err != nil {
		return nil, err
	}

	return &ExportedProjectEnvelope{
		Schema:     schema,
		ExportedAt: exportedAt,
		Mode:       mode,
		Name:       name,
		Content:    content,
	}, nil
}

// OpenExportedProject validates a portable .schoolbot-project.json payload and imports it as a new local project
// with a fresh UUIDv4 project_id and local_revision = 1, never overwriting existing local projects.
func (s *LocalStore) OpenExportedProject(ctx context.Context, data []byte) (*LocalProjectDocument, error) {
	if err := s.entitlement.CheckEntitlement(ctx); err != nil {
		return nil, err
	}
	env, err := decodeStrictExportEnvelope(data)
	if err != nil {
		return nil, err
	}
	return s.CreateProject(ctx, CreateProjectInput{
		Mode:    env.Mode,
		Name:    env.Name,
		Content: env.Content,
	})
}

// OpenExportedProjectFromFile reads a .schoolbot-project.json file and imports it as a new local project.
func (s *LocalStore) OpenExportedProjectFromFile(ctx context.Context, srcPath string) (*LocalProjectDocument, error) {
	if err := s.entitlement.CheckEntitlement(ctx); err != nil {
		return nil, err
	}
	cleanSrc := filepath.Clean(srcPath)
	fi, err := os.Lstat(cleanSrc)
	if os.IsNotExist(err) {
		return nil, &StoreError{Code: ErrNotFound, Message: "export file not found"}
	}
	if err != nil {
		return nil, &StoreError{Code: ErrStoreUnavailable, Message: err.Error()}
	}
	if fi.Mode()&os.ModeSymlink != 0 || isReparsePoint(cleanSrc) {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: "symlinks are not permitted"}
	}
	if fi.Size() > 1<<20 {
		return nil, &StoreError{Code: ErrProjectTooLarge, Message: "exported project file exceeds 1 MiB limit"}
	}
	file, err := os.Open(cleanSrc)
	if err != nil {
		return nil, &StoreError{Code: ErrStoreUnavailable, Message: err.Error()}
	}
	data, err := io.ReadAll(io.LimitReader(file, (1<<20)+1))
	_ = file.Close()
	if err != nil {
		return nil, &StoreError{Code: ErrStoreUnavailable, Message: err.Error()}
	}
	return s.OpenExportedProject(ctx, data)
}
