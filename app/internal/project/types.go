package project

import (
	"fmt"
)

// Modes supported by SchoolBot.
const (
	ModeArm   = "arm"
	ModeCode  = "code"
	ModeBlock = "block"
)

// Controlled error codes matching DES-003 and project-api-001.
const (
	ErrProjectInvalid       = "PROJECT_INVALID"
	ErrProjectTooLarge      = "PROJECT_TOO_LARGE"
	ErrProjectNotFound      = "NOT_FOUND"
	ErrNotFound             = "NOT_FOUND"
	ErrProjectExists        = "PROJECT_EXISTS"
	ErrStateConflict        = "STATE_CONFLICT"
	ErrVersionConflict      = "VERSION_CONFLICT"
	ErrPreconditionRequired = "PRECONDITION_REQUIRED"
	ErrStoreUnavailable     = "STORE_UNAVAILABLE"
	ErrEntitlementRequired  = "LICENSE_REQUIRED"
	ErrLicenseRequired      = "LICENSE_REQUIRED"
	ErrMigrationUnsupported = "MIGRATION_UNSUPPORTED"
)

// StoreError represents a controlled local store error.
type StoreError struct {
	Code                 string `json:"code"`
	Message              string `json:"message"`
	CurrentLocalRevision int64  `json:"current_local_revision,omitempty"`
}

func (e *StoreError) Error() string {
	if e.CurrentLocalRevision > 0 {
		return fmt.Sprintf("[%s] %s (current_local_revision: %d)", e.Code, e.Message, e.CurrentLocalRevision)
	}
	return fmt.Sprintf("[%s] %s", e.Code, e.Message)
}

// LocalProjectDocument is the v2 project document stored locally.
type LocalProjectDocument struct {
	Schema        int             `json:"schema"`
	ProjectID     string          `json:"project_id"`
	Mode          string          `json:"mode"`
	Name          string          `json:"name"`
	LocalRevision int64           `json:"local_revision"`
	UpdatedAt     string          `json:"updated_at"`
	Content       *ProjectContent `json:"content"`
}

// ProjectMetadata represents project summary used in listing and derived index.
type ProjectMetadata struct {
	ProjectID     string `json:"project_id"`
	Mode          string `json:"mode"`
	Name          string `json:"name"`
	LocalRevision int64  `json:"local_revision"`
	UpdatedAt     string `json:"updated_at"`
}

// ProjectContent is the tagged union of project content.
type ProjectContent struct {
	Kind string `json:"kind"`

	// For kind == "code"
	Source string `json:"source,omitempty"`

	// For kind == "block"
	Stacks []Stack `json:"stacks,omitempty"`

	// For kind == "arm"
	Poses      []Pose      `json:"poses,omitempty"`
	Order      []string    `json:"order,omitempty"`
	Groups     []PoseGroup `json:"groups,omitempty"`
	JointNames []string    `json:"joint_names,omitempty"`
	TimeMode   string      `json:"time_mode,omitempty"`
	Loops      int         `json:"loops,omitempty"`
	View       string      `json:"view,omitempty"`
}

// Stack holds a group of connected blocks in block programming.
type Stack struct {
	X      float64 `json:"x"`
	Y      float64 `json:"y"`
	Blocks []Block `json:"blocks"`
}

// Block represents a single block AST element in block programming.
type Block struct {
	ID        string  `json:"id"`
	Enabled   bool    `json:"enabled"`
	Type      string  `json:"type"`
	Direction string  `json:"direction,omitempty"`
	Tiles     int     `json:"tiles,omitempty"`
	Times     int     `json:"times,omitempty"`
	Body      []Block `json:"body,omitempty"`
}

// Pose represents an arm posture in arm lab.
type Pose struct {
	ID              string     `json:"id"`
	Name            string     `json:"name"`
	Values          [6]float64 `json:"values"`
	DurationMs      int        `json:"duration_ms"`
	JointDurationMs [6]int     `json:"joint_duration_ms"`
}

// PoseGroup groups multiple ordered poses under arm lab.
type PoseGroup struct {
	ID      string   `json:"id"`
	Name    string   `json:"name"`
	PoseIDs []string `json:"pose_ids"`
}

// MigrationReceipt records completion of legacy slot migration.
type MigrationReceipt struct {
	Schema     int    `json:"schema"`
	SourceApp  string `json:"source_app"`
	Mode       string `json:"mode"`
	SourceHash string `json:"source_hash"`
	ProjectID  string `json:"project_id"`
}

// MigrationResult summarizes the outcome of migrating one slot.
type MigrationResult struct {
	Mode       string `json:"mode"`
	SourceApp  string `json:"source_app"`
	ProjectID  string `json:"project_id,omitempty"`
	Status     string `json:"status"`
	ErrorCode  string `json:"error_code,omitempty"`
	Message    string `json:"message,omitempty"`
	SourceHash string `json:"source_hash,omitempty"`
}

// CreateProjectInput holds parameters for creating a new project.
type CreateProjectInput struct {
	Mode    string
	Name    string
	Content *ProjectContent
}

// SaveProjectInput holds parameters for updating an existing project.
type SaveProjectInput struct {
	ProjectID             string
	ExpectedLocalRevision int64
	Name                  string
	Content               *ProjectContent
}

// CopyProjectInput holds parameters for making a project copy.
type CopyProjectInput struct {
	SourceProjectID       string
	ExpectedLocalRevision int64
	Name                  string
}
