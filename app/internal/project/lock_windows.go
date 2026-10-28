//go:build windows

package project

import (
	"context"
	"errors"
	"fmt"
	"os"
	"path/filepath"
	"sync"
	"time"

	"golang.org/x/sys/windows"
)

var (
	procLockMu sync.Mutex
	procLocks  = make(map[string]*sync.Mutex)
)

func getProcessMutex(key string) *sync.Mutex {
	procLockMu.Lock()
	defer procLockMu.Unlock()
	m, ok := procLocks[key]
	if !ok {
		m = &sync.Mutex{}
		procLocks[key] = m
	}
	return m
}

func (s *LocalStore) lockProject(ctx context.Context, projectID string) (func(), error) {
	key := filepath.Join(s.locksDir, projectID)
	m := getProcessMutex(key)
	m.Lock()

	lockPath := filepath.Join(s.locksDir, projectID+".lock")
	f, err := os.OpenFile(lockPath, os.O_CREATE|os.O_RDWR, 0600)
	if err != nil {
		m.Unlock()
		return nil, &StoreError{Code: ErrStoreUnavailable, Message: fmt.Sprintf("failed to open lock file: %v", err)}
	}

	handle := windows.Handle(f.Fd())
	var ol windows.Overlapped

	deadline := time.Now().Add(10 * time.Second)
	if d, ok := ctx.Deadline(); ok && d.Before(deadline) {
		deadline = d
	}

	locked := false
	for time.Now().Before(deadline) {
		err = windows.LockFileEx(handle, windows.LOCKFILE_EXCLUSIVE_LOCK|windows.LOCKFILE_FAIL_IMMEDIATELY, 0, 1, 0, &ol)
		if err == nil {
			locked = true
			break
		}
		if !errors.Is(err, windows.ERROR_LOCK_VIOLATION) {
			_ = f.Close()
			m.Unlock()
			return nil, &StoreError{Code: ErrStoreUnavailable, Message: fmt.Sprintf("failed to acquire lock: %v", err)}
		}
		select {
		case <-ctx.Done():
			_ = f.Close()
			m.Unlock()
			return nil, &StoreError{Code: ErrStoreUnavailable, Message: "lock acquisition cancelled"}
		case <-time.After(10 * time.Millisecond):
		}
	}

	if !locked {
		_ = f.Close()
		m.Unlock()
		return nil, &StoreError{Code: ErrStoreUnavailable, Message: "lock acquisition timed out"}
	}

	var once sync.Once
	unlock := func() {
		once.Do(func() {
			_ = windows.UnlockFileEx(handle, 0, 1, 0, &ol)
			_ = f.Close()
			m.Unlock()
		})
	}
	return unlock, nil
}
