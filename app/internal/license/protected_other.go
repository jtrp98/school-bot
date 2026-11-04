//go:build !windows && !darwin

package license

import (
	"context"
	"errors"
)

type ProtectedStore struct{ Path string }

func (s *ProtectedStore) Load(context.Context) ([]byte, error) {
	return nil, errors.New("native credential store is unavailable on this platform")
}

func (s *ProtectedStore) Save(context.Context, []byte) error {
	return errors.New("native credential store is unavailable on this platform")
}
