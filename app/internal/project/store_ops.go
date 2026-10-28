package project

import (
	"context"
	"fmt"
	"os"
	"sort"
	"time"
)

// CreateProject validates input, generates a unique project ID, and atomically writes the document.
func (s *LocalStore) CreateProject(ctx context.Context, input CreateProjectInput) (*LocalProjectDocument, error) {
	if err := s.entitlement.CheckEntitlement(ctx); err != nil {
		return nil, err
	}
	if !ValidMode(input.Mode) {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: fmt.Sprintf("invalid mode %q", input.Mode)}
	}
	if input.Content == nil {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: "content is required"}
	}

	newID, err := NewUUIDv4()
	if err != nil {
		return nil, &StoreError{Code: ErrStoreUnavailable, Message: fmt.Sprintf("failed to generate UUID: %v", err)}
	}

	doc := &LocalProjectDocument{
		Schema:        2,
		ProjectID:     newID,
		Mode:          input.Mode,
		Name:          input.Name,
		LocalRevision: 1,
		UpdatedAt:     time.Now().UTC().Format(time.RFC3339),
		Content:       input.Content,
	}

	if err := ValidateLocalProjectDocument(doc); err != nil {
		return nil, err
	}

	path, err := s.resolveProjectPath(newID)
	if err != nil {
		return nil, err
	}

	unlock, err := s.lockProject(ctx, newID)
	if err != nil {
		return nil, err
	}
	defer unlock()

	if _, err := os.Stat(path); err == nil {
		return nil, &StoreError{Code: ErrProjectExists, Message: "project with generated ID already exists"}
	}

	if err := s.entitlement.CheckEntitlement(ctx); err != nil {
		return nil, err
	}

	if err := s.atomicWriteDocument(path, doc); err != nil {
		return nil, err
	}

	s.updateIndex(doc)
	return doc, nil
}

// GetProject loads and validates a project document by its UUID.
func (s *LocalStore) GetProject(ctx context.Context, projectID string) (*LocalProjectDocument, error) {
	if err := s.entitlement.CheckEntitlement(ctx); err != nil {
		return nil, err
	}
	path, err := s.resolveProjectPath(projectID)
	if err != nil {
		return nil, err
	}

	unlock, err := s.lockProject(ctx, projectID)
	if err != nil {
		return nil, err
	}
	defer unlock()

	return s.readProjectFile(path)
}

// ListProjects returns project metadata optionally filtered by mode.
func (s *LocalStore) ListProjects(ctx context.Context, mode string) ([]ProjectMetadata, error) {
	if err := s.entitlement.CheckEntitlement(ctx); err != nil {
		return nil, err
	}
	if mode != "" && !ValidMode(mode) {
		return nil, &StoreError{Code: ErrProjectInvalid, Message: fmt.Sprintf("invalid mode %q", mode)}
	}

	_ = s.rebuildIndex()

	s.mu.RLock()
	defer s.mu.RUnlock()

	var results []ProjectMetadata
	for _, meta := range s.index {
		if mode == "" || meta.Mode == mode {
			results = append(results, meta)
		}
	}

	sort.Slice(results, func(i, j int) bool {
		if results[i].UpdatedAt == results[j].UpdatedAt {
			return results[i].Name < results[j].Name
		}
		return results[i].UpdatedAt > results[j].UpdatedAt
	})

	return results, nil
}

// SaveProject updates an existing project with revision conflict detection and durable write.
func (s *LocalStore) SaveProject(ctx context.Context, input SaveProjectInput) (*LocalProjectDocument, error) {
	if err := s.entitlement.CheckEntitlement(ctx); err != nil {
		return nil, err
	}
	if input.ExpectedLocalRevision < 1 {
		return nil, &StoreError{Code: ErrPreconditionRequired, Message: "expected_local_revision must be >= 1"}
	}
	path, err := s.resolveProjectPath(input.ProjectID)
	if err != nil {
		return nil, err
	}

	unlock, err := s.lockProject(ctx, input.ProjectID)
	if err != nil {
		return nil, err
	}
	defer unlock()

	existing, err := s.readProjectFile(path)
	if err != nil {
		return nil, err
	}

	if existing.LocalRevision != input.ExpectedLocalRevision {
		return nil, &StoreError{
			Code:                 ErrVersionConflict,
			Message:              fmt.Sprintf("revision conflict: expected %d but current is %d", input.ExpectedLocalRevision, existing.LocalRevision),
			CurrentLocalRevision: existing.LocalRevision,
		}
	}

	updated := &LocalProjectDocument{
		Schema:        2,
		ProjectID:     existing.ProjectID,
		Mode:          existing.Mode,
		Name:          input.Name,
		LocalRevision: existing.LocalRevision + 1,
		UpdatedAt:     time.Now().UTC().Format(time.RFC3339),
		Content:       input.Content,
	}

	if err := ValidateLocalProjectDocument(updated); err != nil {
		return nil, err
	}

	if err := s.entitlement.CheckEntitlement(ctx); err != nil {
		return nil, err
	}

	if err := s.atomicWriteDocument(path, updated); err != nil {
		return nil, err
	}

	s.updateIndex(updated)
	return updated, nil
}

// CopyProject creates a new project document copied from an existing source revision.
func (s *LocalStore) CopyProject(ctx context.Context, input CopyProjectInput) (*LocalProjectDocument, error) {
	if err := s.entitlement.CheckEntitlement(ctx); err != nil {
		return nil, err
	}
	if input.ExpectedLocalRevision < 1 {
		return nil, &StoreError{Code: ErrPreconditionRequired, Message: "expected_local_revision must be >= 1"}
	}
	sourcePath, err := s.resolveProjectPath(input.SourceProjectID)
	if err != nil {
		return nil, err
	}

	newID, err := NewUUIDv4()
	if err != nil {
		return nil, &StoreError{Code: ErrStoreUnavailable, Message: fmt.Sprintf("failed to generate UUID: %v", err)}
	}
	destPath, err := s.resolveProjectPath(newID)
	if err != nil {
		return nil, err
	}

	unlock, err := s.lockTwoProjects(ctx, input.SourceProjectID, newID)
	if err != nil {
		return nil, err
	}
	defer unlock()

	source, err := s.readProjectFile(sourcePath)
	if err != nil {
		return nil, err
	}

	if source.LocalRevision != input.ExpectedLocalRevision {
		return nil, &StoreError{
			Code:                 ErrVersionConflict,
			Message:              fmt.Sprintf("source revision conflict: expected %d but source is %d", input.ExpectedLocalRevision, source.LocalRevision),
			CurrentLocalRevision: source.LocalRevision,
		}
	}

	destDoc := &LocalProjectDocument{
		Schema:        2,
		ProjectID:     newID,
		Mode:          source.Mode,
		Name:          input.Name,
		LocalRevision: 1,
		UpdatedAt:     time.Now().UTC().Format(time.RFC3339),
		Content:       source.Content,
	}

	if err := ValidateLocalProjectDocument(destDoc); err != nil {
		return nil, err
	}

	if err := s.entitlement.CheckEntitlement(ctx); err != nil {
		return nil, err
	}

	if err := s.atomicWriteDocument(destPath, destDoc); err != nil {
		return nil, err
	}

	s.updateIndex(destDoc)
	return destDoc, nil
}

// ImportProject directly persists a prepared valid document, used by migration.
func (s *LocalStore) ImportProject(ctx context.Context, doc *LocalProjectDocument) error {
	unlock, err := s.lockProject(ctx, doc.ProjectID)
	if err != nil {
		return err
	}
	defer unlock()

	return s.importProjectLocked(ctx, doc)
}

func (s *LocalStore) importProjectLocked(ctx context.Context, doc *LocalProjectDocument) error {
	if err := s.entitlement.CheckEntitlement(ctx); err != nil {
		return err
	}
	if err := ValidateLocalProjectDocument(doc); err != nil {
		return err
	}
	path, err := s.resolveProjectPath(doc.ProjectID)
	if err != nil {
		return err
	}

	if _, err := os.Stat(path); err == nil {
		return &StoreError{Code: ErrProjectExists, Message: "project already exists at destination"}
	}

	if err := s.entitlement.CheckEntitlement(ctx); err != nil {
		return err
	}

	if err := s.atomicWriteDocument(path, doc); err != nil {
		return err
	}

	s.updateIndex(doc)
	return nil
}
