package project

import (
	"context"
	"crypto/sha256"
	"encoding/hex"
	"encoding/json"
	"fmt"
	"io"
	"os"
	"path/filepath"
	"reflect"
	"time"
)

// MigrateLegacyProjects inspects SchoolBotPoC first, then ArmStudioPoC,
// converting valid legacy slots without overwriting newer projects or deleting sources.
func MigrateLegacyProjects(configDir string, store *LocalStore) ([]MigrationResult, error) {
	if store == nil {
		return nil, &StoreError{Code: ErrStoreUnavailable, Message: "store is nil"}
	}
	cleanConfig := filepath.Clean(configDir)

	slots := []struct {
		mode     string
		filename string
	}{
		{mode: ModeArm, filename: "robot-control.json"},
		{mode: ModeCode, filename: "coding-adventure.json"},
		{mode: ModeBlock, filename: "block-programming.json"},
	}

	apps := []string{"SchoolBotPoC", "ArmStudioPoC"}
	var results []MigrationResult

	for _, slot := range slots {
		var foundApp string
		var foundPath string

		primaryPath := filepath.Join(cleanConfig, apps[0], slot.filename)
		fi, err := os.Lstat(primaryPath)
		if err == nil && fi.Mode().IsRegular() {
			foundApp = apps[0]
			foundPath = primaryPath
		} else if os.IsNotExist(err) {
			secondaryPath := filepath.Join(cleanConfig, apps[1], slot.filename)
			sfi, serr := os.Lstat(secondaryPath)
			if serr == nil && sfi.Mode().IsRegular() {
				foundApp = apps[1]
				foundPath = secondaryPath
			}
		} else if err != nil {
			foundApp = apps[0]
			foundPath = primaryPath
		}

		if foundPath == "" {
			continue
		}

		res := migrateSlot(foundApp, slot.mode, slot.filename, foundPath, store)
		results = append(results, res)
	}

	return results, nil
}

func migrateSlot(sourceApp, mode, filename, sourcePath string, store *LocalStore) MigrationResult {
	file, err := os.Open(sourcePath)
	if err != nil {
		return MigrationResult{
			Mode: mode, SourceApp: sourceApp,
			Status: "error", ErrorCode: ErrStoreUnavailable, Message: err.Error(),
		}
	}
	data, err := io.ReadAll(io.LimitReader(file, (1<<20)+1))
	_ = file.Close()
	if err != nil {
		return MigrationResult{
			Mode: mode, SourceApp: sourceApp,
			Status: "error", ErrorCode: ErrStoreUnavailable, Message: err.Error(),
		}
	}
	if len(data) > 1<<20 {
		return MigrationResult{
			Mode: mode, SourceApp: sourceApp,
			Status: "unsupported", ErrorCode: ErrMigrationUnsupported,
			Message: "legacy project exceeds 1 MiB size limit",
		}
	}
	if !json.Valid(data) {
		return MigrationResult{
			Mode: mode, SourceApp: sourceApp,
			Status: "unsupported", ErrorCode: ErrMigrationUnsupported,
			Message: "legacy project contains invalid JSON",
		}
	}

	h := sha256.Sum256(data)
	sourceHash := hex.EncodeToString(h[:])
	projectID := DeterministicUUID(SchoolBotNamespace, fmt.Sprintf("%s:%s:%s", sourceApp, mode, filename))

	unlock, err := store.lockProject(context.Background(), projectID)
	if err != nil {
		return MigrationResult{
			Mode: mode, SourceApp: sourceApp, ProjectID: projectID,
			Status: "error", ErrorCode: ErrStoreUnavailable, Message: err.Error(),
		}
	}
	defer unlock()

	receiptPath := filepath.Join(store.ReceiptsDir(), fmt.Sprintf("receipt_%s_%s.json", sourceApp, mode))
	hasValidReceipt := false
	var validReceipt MigrationReceipt

	if receiptData, rErr := os.ReadFile(receiptPath); rErr == nil {
		if json.Unmarshal(receiptData, &validReceipt) == nil &&
			validReceipt.Schema == 1 &&
			validReceipt.SourceApp == sourceApp &&
			validReceipt.Mode == mode &&
			validReceipt.ProjectID == projectID {
			hasValidReceipt = true
		} else {
			return MigrationResult{
				Mode: mode, SourceApp: sourceApp, ProjectID: projectID,
				Status: "error", ErrorCode: ErrStateConflict,
				Message: "corrupted or mismatched migration receipt",
				SourceHash: sourceHash,
			}
		}
	}

	projectPath, pErr := store.resolveProjectPath(projectID)
	if pErr != nil {
		return MigrationResult{
			Mode: mode, SourceApp: sourceApp, ProjectID: projectID,
			Status: "error", ErrorCode: ErrStoreUnavailable, Message: pErr.Error(),
		}
	}
	existing, getErr := store.readProjectFile(projectPath)
	if hasValidReceipt {
		if getErr != nil || existing == nil {
			return MigrationResult{
				Mode: mode, SourceApp: sourceApp, ProjectID: projectID,
				Status: "error", ErrorCode: ErrStateConflict,
				Message: "receipt exists but project file is missing",
				SourceHash: sourceHash,
			}
		}
		if validReceipt.SourceHash == sourceHash {
			return MigrationResult{
				Mode: mode, SourceApp: sourceApp, ProjectID: projectID,
				Status: "skipped_up_to_date", SourceHash: sourceHash,
			}
		}
		return MigrationResult{
			Mode: mode, SourceApp: sourceApp, ProjectID: projectID,
			Status: "skipped_existing", SourceHash: sourceHash,
		}
	}

	content, name, hasWarning, err := convertLegacyContent(mode, data, projectID)
	if err != nil {
		return MigrationResult{
			Mode: mode, SourceApp: sourceApp,
			Status: "unsupported", ErrorCode: ErrMigrationUnsupported,
			Message: err.Error(), SourceHash: sourceHash,
		}
	}

	warnMsg := ""
	if hasWarning {
		warnMsg = "some invalid groups were skipped"
	}

	if existing != nil {
		if existing.LocalRevision == 1 && existing.Mode == mode &&
			existing.Name == name && reflect.DeepEqual(existing.Content, content) {
			rErr := writeReceiptAtomic(store.ReceiptsDir(), receiptPath, sourceApp, mode, sourceHash, projectID)
			if rErr != nil {
				return MigrationResult{
					Mode: mode, SourceApp: sourceApp, ProjectID: projectID,
					Status: "error", ErrorCode: ErrStoreUnavailable,
					Message: rErr.Error(), SourceHash: sourceHash,
				}
			}
			return MigrationResult{
				Mode: mode, SourceApp: sourceApp, ProjectID: projectID,
				Status: "migrated", SourceHash: sourceHash, Message: warnMsg,
			}
		}
		return MigrationResult{
			Mode: mode, SourceApp: sourceApp, ProjectID: projectID,
			Status: "error", ErrorCode: ErrStateConflict,
			Message: "project exists without receipt and differs from legacy conversion",
			SourceHash: sourceHash,
		}
	}

	doc := &LocalProjectDocument{
		Schema:        2,
		ProjectID:     projectID,
		Mode:          mode,
		Name:          name,
		LocalRevision: 1,
		UpdatedAt:     time.Now().UTC().Format(time.RFC3339),
		Content:       content,
	}

	if err := ValidateLocalProjectDocument(doc); err != nil {
		return MigrationResult{
			Mode: mode, SourceApp: sourceApp,
			Status: "unsupported", ErrorCode: ErrMigrationUnsupported,
			Message: err.Error(), SourceHash: sourceHash,
		}
	}

	if err := store.importProjectLocked(context.Background(), doc); err != nil {
		return MigrationResult{
			Mode: mode, SourceApp: sourceApp,
			Status: "error", ErrorCode: ErrStoreUnavailable,
			Message: err.Error(), SourceHash: sourceHash,
		}
	}

	rErr := writeReceiptAtomic(store.ReceiptsDir(), receiptPath, sourceApp, mode, sourceHash, projectID)
	if rErr != nil {
		return MigrationResult{
			Mode: mode, SourceApp: sourceApp, ProjectID: projectID,
			Status: "error", ErrorCode: ErrStoreUnavailable,
			Message: rErr.Error(), SourceHash: sourceHash,
		}
	}

	return MigrationResult{
		Mode: mode, SourceApp: sourceApp, ProjectID: projectID,
		Status: "migrated", SourceHash: sourceHash, Message: warnMsg,
	}
}

func writeReceiptAtomic(receiptsDir, receiptPath, sourceApp, mode, sourceHash, projectID string) error {
	receipt := MigrationReceipt{
		Schema:     1,
		SourceApp:  sourceApp,
		Mode:       mode,
		SourceHash: sourceHash,
		ProjectID:  projectID,
	}
	receiptBytes, err := json.MarshalIndent(receipt, "", "  ")
	if err != nil {
		return err
	}

	tmpFile, err := os.CreateTemp(receiptsDir, ".tmp-receipt-*.tmp")
	if err != nil {
		return err
	}
	tmpName := tmpFile.Name()
	defer os.Remove(tmpName)

	if _, err := tmpFile.Write(receiptBytes); err != nil {
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
	return os.Rename(tmpName, receiptPath)
}

func convertLegacyContent(mode string, data []byte, projectID string) (*ProjectContent, string, bool, error) {
	switch mode {
	case ModeCode:
		c, n, err := convertLegacyCode(data)
		return c, n, false, err
	case ModeArm:
		return convertLegacyArm(data, projectID)
	case ModeBlock:
		c, n, err := convertLegacyBlock(data)
		return c, n, false, err
	default:
		return nil, "", false, fmt.Errorf("unsupported mode %q", mode)
	}
}

func convertLegacyCode(data []byte) (*ProjectContent, string, error) {
	var payload struct {
		Schema *int    `json:"schema"`
		Source *string `json:"source"`
	}
	if err := json.Unmarshal(data, &payload); err != nil {
		return nil, "", fmt.Errorf("invalid code payload: %v", err)
	}
	if payload.Schema == nil || *payload.Schema != 1 || payload.Source == nil {
		return nil, "", fmt.Errorf("legacy code schema must be 1 with source string")
	}
	units := UTF16CodeUnitLength(*payload.Source)
	if units > 16384 {
		return nil, "", fmt.Errorf("source exceeds 16384 UTF-16 code units (got %d)", units)
	}
	return &ProjectContent{
		Kind:   ModeCode,
		Source: *payload.Source,
	}, "Coding Adventure", nil
}
