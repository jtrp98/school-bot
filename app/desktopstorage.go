package main

import (
	"context"
	"encoding/hex"
	"encoding/json"
	"errors"
	"io"
	"os"
	"path/filepath"
	"schoolbot.local/app/internal/engine"
	"schoolbot.local/app/internal/license"
	"schoolbot.local/app/internal/project"
	"time"
)

type hostEntitlementChecker struct {
	svc *DesktopService
}

func (c *hostEntitlementChecker) CheckEntitlement(ctx context.Context) error {
	if c.svc == nil || c.svc.entitlement == nil {
		return nil
	}
	proj, err := c.svc.entitlement.Evaluate(ctx)
	if err != nil {
		var licErr *license.LicenseError
		if errors.As(err, &licErr) && licErr.Code == license.ErrClockUntrusted {
			return &project.StoreError{Code: project.ErrClockUntrusted, Message: licErr.Message}
		}
		return &project.StoreError{Code: project.ErrLicenseRequired, Message: err.Error()}
	}
	if proj.Status != license.StatusValid {
		return &project.StoreError{Code: project.ErrLicenseRequired, Message: "valid license is required"}
	}
	return nil
}

func (s *DesktopService) ensureProjectStore() (*project.LocalStore, error) {
	s.mu.Lock()
	defer s.mu.Unlock()
	if s.projectStore != nil {
		return s.projectStore, nil
	}
	store, err := project.NewLocalStore(s.dataDir, &hostEntitlementChecker{svc: s})
	if err != nil {
		return nil, err
	}
	store.SetLastTrustedFunc(func() (time.Time, bool) {
		if s.entitlement == nil {
			return time.Time{}, false
		}
		tt, err := s.entitlement.TrustedTime(context.Background())
		if err != nil || !tt.Trusted || tt.CurrentUTC.IsZero() {
			return time.Time{}, false
		}
		return tt.CurrentUTC, true
	})
	s.projectStore = store
	return store, nil
}

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
			if !validDeviceID(data) {
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

func (s *DesktopService) ListProjectsV2(mode string) ([]project.ProjectMetadata, error) {
	store, err := s.ensureProjectStore()
	if err != nil {
		return nil, err
	}
	return store.ListProjects(context.Background(), mode)
}

func (s *DesktopService) CreateProjectV2(mode, name, contentJSON string) (*project.LocalProjectDocument, error) {
	store, err := s.ensureProjectStore()
	if err != nil {
		return nil, err
	}
	var content project.ProjectContent
	if err := json.Unmarshal([]byte(contentJSON), &content); err != nil {
		return nil, &project.StoreError{Code: project.ErrProjectInvalid, Message: "invalid content JSON"}
	}
	return store.CreateProject(context.Background(), project.CreateProjectInput{
		Mode:    mode,
		Name:    name,
		Content: &content,
	})
}

func (s *DesktopService) GetProjectV2(projectID string) (*project.LocalProjectDocument, error) {
	store, err := s.ensureProjectStore()
	if err != nil {
		return nil, err
	}
	return store.GetProject(context.Background(), projectID)
}

func (s *DesktopService) SaveProjectV2(projectID string, expectedRevision int64, name, contentJSON string) (*project.LocalProjectDocument, error) {
	store, err := s.ensureProjectStore()
	if err != nil {
		return nil, err
	}
	var content project.ProjectContent
	if err := json.Unmarshal([]byte(contentJSON), &content); err != nil {
		return nil, &project.StoreError{Code: project.ErrProjectInvalid, Message: "invalid content JSON"}
	}
	return store.SaveProject(context.Background(), project.SaveProjectInput{
		ProjectID:             projectID,
		ExpectedLocalRevision: expectedRevision,
		Name:                  name,
		Content:               &content,
	})
}

func (s *DesktopService) CopyProjectV2(sourceProjectID string, expectedRevision int64, name string) (*project.LocalProjectDocument, error) {
	store, err := s.ensureProjectStore()
	if err != nil {
		return nil, err
	}
	return store.CopyProject(context.Background(), project.CopyProjectInput{
		SourceProjectID:       sourceProjectID,
		ExpectedLocalRevision: expectedRevision,
		Name:                  name,
	})
}

func (s *DesktopService) DeleteProjectV2(projectID string, _ int64) (*project.TrashMetadata, error) {
	store, err := s.ensureProjectStore()
	if err != nil {
		return nil, err
	}
	return store.DeleteProject(context.Background(), projectID)
}

func (s *DesktopService) ListTrashV2(mode string) ([]project.TrashMetadata, error) {
	store, err := s.ensureProjectStore()
	if err != nil {
		return nil, err
	}
	return store.ListTrash(context.Background(), mode)
}

func (s *DesktopService) RestoreProjectV2(projectID string) (*project.LocalProjectDocument, error) {
	store, err := s.ensureProjectStore()
	if err != nil {
		return nil, err
	}
	return store.RestoreProject(context.Background(), projectID)
}

func (s *DesktopService) ExportProjectV2(projectID string) (string, error) {
	store, err := s.ensureProjectStore()
	if err != nil {
		return "", err
	}
	data, err := store.ExportProject(context.Background(), projectID)
	if err != nil {
		return "", err
	}
	return string(data), nil
}

func (s *DesktopService) ExportProjectToFileV2(projectID, filePath string) error {
	store, err := s.ensureProjectStore()
	if err != nil {
		return err
	}
	return store.ExportProjectToFile(context.Background(), projectID, filePath)
}

func (s *DesktopService) OpenExportedProjectV2(rawJSON string) (*project.LocalProjectDocument, error) {
	store, err := s.ensureProjectStore()
	if err != nil {
		return nil, err
	}
	return store.OpenExportedProject(context.Background(), []byte(rawJSON))
}

func (s *DesktopService) OpenExportedProjectFromFileV2(filePath string) (*project.LocalProjectDocument, error) {
	store, err := s.ensureProjectStore()
	if err != nil {
		return nil, err
	}
	return store.OpenExportedProjectFromFile(context.Background(), filePath)
}

func (s *DesktopService) ImportGuestProjectV2(localProjectID string, schoolID int, ownerKind string, ownerID int) (*project.CloudImportResponse, error) {
	store, err := s.ensureProjectStore()
	if err != nil {
		return nil, err
	}
	dest := project.ImportDestination{
		APIOrigin: s.apiURL,
		SchoolID:  schoolID,
		OwnerKind: ownerKind,
		OwnerID:   ownerID,
	}
	resp, _, err := store.ImportProjectToCloud(context.Background(), localProjectID, dest, func(_ context.Context, _ project.ImportDestination, req project.CloudImportRequest) (*project.CloudImportResponse, error) {
		raw, err := s.api("POST", "/api/service-school-bot/projects/import", req)
		if err != nil {
			return nil, err
		}
		var out project.CloudImportResponse
		if err := json.Unmarshal([]byte(raw), &out); err != nil {
			return nil, err
		}
		return &out, nil
	})
	return resp, err
}

