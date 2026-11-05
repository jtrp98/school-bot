package project

import (
	"bytes"
	"context"
	"crypto/sha256"
	"encoding/hex"
	"encoding/json"
	"fmt"
	"os"
	"path/filepath"
	"sort"
	"strings"
	"time"
)

// CanonicalizeJSONBytes parses arbitrary valid JSON bytes and re-encodes them in compact canonical form
// with object keys sorted lexicographically at every level.
func CanonicalizeJSONBytes(raw []byte) ([]byte, error) {
	dec := json.NewDecoder(bytes.NewReader(raw))
	dec.UseNumber()
	var val any
	if err := dec.Decode(&val); err != nil {
		return nil, err
	}
	var buf bytes.Buffer
	if err := writeCanonicalJSON(&buf, val); err != nil {
		return nil, err
	}
	return buf.Bytes(), nil
}

func writeCanonicalJSON(buf *bytes.Buffer, v any) error {
	switch val := v.(type) {
	case map[string]any:
		keys := make([]string, 0, len(val))
		for k := range val {
			keys = append(keys, k)
		}
		sort.Strings(keys)
		buf.WriteByte('{')
		for i, k := range keys {
			if i > 0 {
				buf.WriteByte(',')
			}
			kb, err := json.Marshal(k)
			if err != nil {
				return err
			}
			buf.Write(kb)
			buf.WriteByte(':')
			if err := writeCanonicalJSON(buf, val[k]); err != nil {
				return err
			}
		}
		buf.WriteByte('}')
		return nil
	case []any:
		buf.WriteByte('[')
		for i, elem := range val {
			if i > 0 {
				buf.WriteByte(',')
			}
			if err := writeCanonicalJSON(buf, elem); err != nil {
				return err
			}
		}
		buf.WriteByte(']')
		return nil
	case json.Number:
		buf.WriteString(val.String())
		return nil
	default:
		b, err := json.Marshal(val)
		if err != nil {
			return err
		}
		buf.Write(b)
		return nil
	}
}

// ComputeImportRequestHash computes the canonical content SHA-256 and the canonical GuestProject.Import
// request hash over {content, local_project_id, local_revision, mode, name}.
func ComputeImportRequestHash(localProjectID, mode, name string, localRevision int64, content *ProjectContent) (contentHash string, requestHash string, err error) {
	rawContent, err := encodeStrictContent(content)
	if err != nil {
		return "", "", err
	}
	canonContent, err := CanonicalizeJSONBytes(rawContent)
	if err != nil {
		return "", "", &StoreError{Code: ErrProjectInvalid, Message: fmt.Sprintf("failed to canonicalize content: %v", err)}
	}
	cSum := sha256.Sum256(canonContent)
	contentHash = hex.EncodeToString(cSum[:])

	payload := map[string]any{
		"content":          json.RawMessage(canonContent),
		"local_project_id": strings.ToLower(strings.TrimSpace(localProjectID)),
		"local_revision":   localRevision,
		"mode":             strings.ToLower(strings.TrimSpace(mode)),
		"name":             strings.TrimSpace(name),
	}
	rawPayload, err := json.Marshal(payload)
	if err != nil {
		return "", "", err
	}
	canonPayload, err := CanonicalizeJSONBytes(rawPayload)
	if err != nil {
		return "", "", err
	}
	rSum := sha256.Sum256(canonPayload)
	requestHash = hex.EncodeToString(rSum[:])
	return contentHash, requestHash, nil
}

func validateImportDestination(dest ImportDestination) error {
	if strings.TrimSpace(dest.APIOrigin) == "" {
		return &StoreError{Code: ErrProjectInvalid, Message: "destination api_origin is required"}
	}
	if dest.SchoolID <= 0 {
		return &StoreError{Code: ErrProjectInvalid, Message: "destination school_id must be > 0"}
	}
	if dest.OwnerKind != "student" && dest.OwnerKind != "employee" {
		return &StoreError{Code: ErrProjectInvalid, Message: "destination owner_kind must be 'student' or 'employee'"}
	}
	if dest.OwnerID <= 0 {
		return &StoreError{Code: ErrProjectInvalid, Message: "destination owner_id must be > 0"}
	}
	return nil
}

func profileKeyForDestination(dest ImportDestination) string {
	raw := fmt.Sprintf("%s|%d|%s|%d", strings.ToLower(strings.TrimSpace(dest.APIOrigin)), dest.SchoolID, dest.OwnerKind, dest.OwnerID)
	sum := sha256.Sum256([]byte(raw))
	return hex.EncodeToString(sum[:16])
}

func (s *LocalStore) resolveImportReceiptPath(dest ImportDestination, localProjectID string) (string, error) {
	if err := validateImportDestination(dest); err != nil {
		return "", err
	}
	if !IsValidUUID(localProjectID) {
		return "", &StoreError{Code: ErrProjectInvalid, Message: fmt.Sprintf("invalid local_project_id %q", localProjectID)}
	}
	profDir := filepath.Join(s.importsDir, profileKeyForDestination(dest))
	if err := ensureSafeDir(profDir); err != nil {
		return "", err
	}
	if err := verifyContainment(s.importsDir, profDir); err != nil {
		return "", err
	}
	target := filepath.Join(profDir, localProjectID+".json")
	if err := verifyContainment(profDir, target); err != nil {
		return "", err
	}
	return target, nil
}

func (s *LocalStore) readImportReceipt(path string, expectedLocalID string) (*ImportReceipt, error) {
	data, err := os.ReadFile(path)
	if os.IsNotExist(err) {
		return nil, nil
	}
	if err != nil {
		return nil, &StoreError{Code: ErrStoreUnavailable, Message: err.Error()}
	}
	if err := validateStrictJSONStructure(data); err != nil {
		return nil, &StoreError{Code: ErrStateConflict, Message: "corrupted import receipt JSON"}
	}
	var receipt ImportReceipt
	if err := json.Unmarshal(data, &receipt); err != nil {
		return nil, &StoreError{Code: ErrStateConflict, Message: "invalid import receipt"}
	}
	if receipt.Schema != 1 || receipt.LocalProjectID != expectedLocalID || receipt.RequestKey != expectedLocalID {
		return nil, &StoreError{Code: ErrStateConflict, Message: "mismatched import receipt"}
	}
	if receipt.State != "pending" && receipt.State != "committed" {
		return nil, &StoreError{Code: ErrStateConflict, Message: "invalid import receipt state"}
	}
	return &receipt, nil
}

func (s *LocalStore) writeImportReceiptAtomic(path string, receipt *ImportReceipt) error {
	data, err := json.MarshalIndent(receipt, "", "  ")
	if err != nil {
		return &StoreError{Code: ErrStoreUnavailable, Message: err.Error()}
	}
	dir := filepath.Dir(path)
	if isReparsePoint(dir) {
		return &StoreError{Code: ErrStoreUnavailable, Message: "import receipt directory is a reparse point"}
	}
	tmpFile, err := os.CreateTemp(dir, ".tmp-import-receipt-*.tmp")
	if err != nil {
		return &StoreError{Code: ErrStoreUnavailable, Message: err.Error()}
	}
	tmpName := tmpFile.Name()
	defer os.Remove(tmpName)

	if _, err := tmpFile.Write(data); err != nil {
		_ = tmpFile.Close()
		return &StoreError{Code: ErrStoreUnavailable, Message: err.Error()}
	}
	if err := tmpFile.Sync(); err != nil {
		_ = tmpFile.Close()
		return &StoreError{Code: ErrStoreUnavailable, Message: err.Error()}
	}
	if err := tmpFile.Close(); err != nil {
		return &StoreError{Code: ErrStoreUnavailable, Message: err.Error()}
	}
	if err := os.Rename(tmpName, path); err != nil {
		return &StoreError{Code: ErrStoreUnavailable, Message: err.Error()}
	}
	return nil
}

// GetImportReceipt reads the persisted import receipt for a destination profile and local project ID.
func (s *LocalStore) GetImportReceipt(dest ImportDestination, localProjectID string) (*ImportReceipt, error) {
	path, err := s.resolveImportReceiptPath(dest, localProjectID)
	if err != nil {
		return nil, err
	}
	return s.readImportReceipt(path, localProjectID)
}

// ImportProjectToCloud executes an explicit, idempotent guest-to-cloud project import guarded by a local receipt.
// On failure, the local project remains completely intact.
func (s *LocalStore) ImportProjectToCloud(
	ctx context.Context,
	localProjectID string,
	dest ImportDestination,
	uploader CloudImportFunc,
) (*CloudImportResponse, *ImportReceipt, error) {
	if err := s.entitlement.CheckEntitlement(ctx); err != nil {
		return nil, nil, err
	}
	if uploader == nil {
		return nil, nil, &StoreError{Code: ErrStoreUnavailable, Message: "cloud uploader is required"}
	}
	projectPath, err := s.resolveProjectPath(localProjectID)
	if err != nil {
		return nil, nil, err
	}
	receiptPath, err := s.resolveImportReceiptPath(dest, localProjectID)
	if err != nil {
		return nil, nil, err
	}

	unlock, err := s.lockProject(ctx, localProjectID)
	if err != nil {
		return nil, nil, err
	}
	defer unlock()

	doc, err := s.readProjectFile(projectPath)
	if err != nil {
		return nil, nil, err
	}

	contentHash, requestHash, err := ComputeImportRequestHash(doc.ProjectID, doc.Mode, doc.Name, doc.LocalRevision, doc.Content)
	if err != nil {
		return nil, nil, err
	}

	existingReceipt, err := s.readImportReceipt(receiptPath, localProjectID)
	if err != nil {
		return nil, nil, err
	}

	if existingReceipt != nil {
		if existingReceipt.RequestHash != requestHash {
			return nil, existingReceipt, &StoreError{
				Code:    ErrIdempotencyConflict,
				Message: "local project content or revision changed since initial import; copy as a new project to import",
			}
		}
		if existingReceipt.State == "committed" && existingReceipt.CloudProjectID != nil && existingReceipt.CloudProjectRevisionID != nil {
			committedAt := ""
			if existingReceipt.CommittedAt != nil {
				committedAt = *existingReceipt.CommittedAt
			}
			return &CloudImportResponse{
				Schema:            2,
				ProjectID:         *existingReceipt.CloudProjectID,
				ProjectRevisionID: *existingReceipt.CloudProjectRevisionID,
				Mode:              doc.Mode,
				Name:              doc.Name,
				Revision:          1,
				UpdateDate:        committedAt,
				Content:           doc.Content,
				Replayed:          true,
			}, existingReceipt, nil
		}
	} else {
		pending := &ImportReceipt{
			Schema:         1,
			LocalProjectID: doc.ProjectID,
			Destination:    dest,
			RequestKey:     doc.ProjectID,
			RequestHash:    requestHash,
			Snapshot: ImportSnapshot{
				Mode:          doc.Mode,
				Name:          doc.Name,
				LocalRevision: doc.LocalRevision,
				ContentSHA256: contentHash,
			},
			State: "pending",
		}
		if err := s.writeImportReceiptAtomic(receiptPath, pending); err != nil {
			return nil, nil, err
		}
		existingReceipt = pending
	}

	req := CloudImportRequest{
		IdempotencyKey: existingReceipt.RequestKey,
		LocalProjectID: doc.ProjectID,
		Mode:           doc.Mode,
		Name:           doc.Name,
		LocalRevision:  doc.LocalRevision,
		Content:        doc.Content,
		RequestHash:    existingReceipt.RequestHash,
	}

	resp, err := uploader(ctx, dest, req)
	if err != nil {
		return nil, existingReceipt, err
	}
	if resp == nil || resp.ProjectID <= 0 || resp.ProjectRevisionID <= 0 {
		return nil, existingReceipt, &StoreError{Code: ErrStoreUnavailable, Message: "invalid cloud import response"}
	}

	nowStr := s.now().Format(time.RFC3339)
	cloudProjID := resp.ProjectID
	cloudRevID := resp.ProjectRevisionID
	committed := &ImportReceipt{
		Schema:                 1,
		LocalProjectID:         doc.ProjectID,
		Destination:            dest,
		RequestKey:             existingReceipt.RequestKey,
		RequestHash:            existingReceipt.RequestHash,
		Snapshot:               existingReceipt.Snapshot,
		State:                  "committed",
		CloudProjectID:         &cloudProjID,
		CloudProjectRevisionID: &cloudRevID,
		CommittedAt:            &nowStr,
	}
	if err := s.writeImportReceiptAtomic(receiptPath, committed); err != nil {
		return nil, nil, err
	}
	return resp, committed, nil
}
