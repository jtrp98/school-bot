//go:build darwin

package license

import (
	"bytes"
	"context"
	"crypto/sha256"
	"encoding/base64"
	"encoding/hex"
	"errors"
	"os/exec"
	"strings"
	"time"
)

type ProtectedStore struct{ Path string }

func (s *ProtectedStore) account() string {
	digest := sha256.Sum256([]byte(s.Path))
	return hex.EncodeToString(digest[:])
}

func (s *ProtectedStore) Load(ctx context.Context) ([]byte, error) {
	ctx, cancel := context.WithTimeout(ctx, 5*time.Second)
	defer cancel()
	output, err := exec.CommandContext(ctx, "/usr/bin/security", "find-generic-password", "-s", "schoolbot-entitlement", "-a", s.account(), "-w").Output()
	if err != nil {
		var exit *exec.ExitError
		if errors.As(err, &exit) && exit.ExitCode() == 44 {
			return nil, nil
		}
		return nil, errors.New("Keychain read failed")
	}
	if len(output) > 131072 {
		return nil, errors.New("invalid Keychain record")
	}
	return base64.StdEncoding.DecodeString(strings.TrimSpace(string(output)))
}

func (s *ProtectedStore) Save(ctx context.Context, data []byte) error {
	if len(data) == 0 || len(data) > 65536 {
		return errors.New("invalid credential record")
	}
	ctx, cancel := context.WithTimeout(ctx, 5*time.Second)
	defer cancel()
	// Interactive stdin keeps the credential out of process arguments; all words are fixed or base64/hex.
	command := exec.CommandContext(ctx, "/usr/bin/security", "-i")
	command.Stdin = bytes.NewBufferString("add-generic-password -U -s schoolbot-entitlement -a " + s.account() + " -w " + base64.StdEncoding.EncodeToString(data) + "\n")
	if err := command.Run(); err != nil {
		return errors.New("Keychain write failed")
	}
	persisted, err := s.Load(ctx)
	if err != nil || !bytes.Equal(persisted, data) {
		return errors.New("Keychain write was not confirmed")
	}
	return nil
}
