package license

import (
	"context"
)

// GenerationStore manages atomic persistence of local entitlement state.
type GenerationStore interface {
	Load(ctx context.Context) (*GenerationRecord, error)
	Save(ctx context.Context, record *GenerationRecord) error
}

// MemoryGenerationStore provides an in-memory store implementation for testing.
type MemoryGenerationStore struct {
	record *GenerationRecord
}

// NewMemoryGenerationStore constructs an empty memory store.
func NewMemoryGenerationStore() *MemoryGenerationStore {
	return &MemoryGenerationStore{}
}

// Load retrieves the current persisted generation.
func (m *MemoryGenerationStore) Load(_ context.Context) (*GenerationRecord, error) {
	if m.record == nil {
		return nil, nil
	}
	// Return a copy to prevent accidental mutation.
	copyRec := *m.record
	return &copyRec, nil
}

// Save stores the generation record atomically.
func (m *MemoryGenerationStore) Save(_ context.Context, record *GenerationRecord) error {
	if record == nil {
		m.record = nil
		return nil
	}
	copyRec := *record
	m.record = &copyRec
	return nil
}
