//go:build !windows

package project

import (
	"context"
	"fmt"
	"os"
	"path/filepath"
	"sync"
	"time"

	"golang.org/x/sys/unix"
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

	deadline := time.Now().Add(10 * time.Second)
	if d, ok := ctx.Deadline(); ok && d.Before(deadline) {
		deadline = d
	}

	locked := false
	for time.Now().Before(deadline) {
		err = unix.Flock(int(f.Fd()), unix.LOCK_EX|unix.LOCK_NB)
		if err == nil {
			locked = true
			break
		}
		if err != unix.EWOULDBLOCK && err != unix.EAGAIN {
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
			_ = unix.Flock(int(f.Fd()), unix.LOCK_UN)
			_ = f.Close()
			m.Unlock()
		})
	}
	return unlock, nil
}
