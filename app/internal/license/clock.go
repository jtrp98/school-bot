package license

import (
	"time"
)

// Clock abstracts system time reading for wall and monotonic progression.
type Clock interface {
	Now() time.Time
	MonotonicElapsed() time.Duration
}

// SystemClock provides real OS time.
type SystemClock struct {
	start time.Time
}

// NewSystemClock initializes a real clock with process start baseline.
func NewSystemClock() *SystemClock {
	return &SystemClock{start: time.Now()}
}

// Now returns the current UTC wall time.
func (c *SystemClock) Now() time.Time {
	return time.Now().UTC()
}

// MonotonicElapsed measures monotonic duration since process baseline.
func (c *SystemClock) MonotonicElapsed() time.Duration {
	return time.Since(c.start)
}

// EvaluateClock checks current clock readings against the anchor.
// Wall clock moving forward can advance high-water mark and accelerate expiry,
// but wall clock moving backward behind previous observations triggers CLOCK_UNTRUSTED.
func EvaluateClock(wallNow int64, sessionElapsed time.Duration, anchor *ClockAnchor) (int64, error) {
	if sessionElapsed < 0 {
		return 0, &LicenseError{
			Code:    ErrClockUntrusted,
			Message: "monotonic elapsed clock reported negative duration",
		}
	}

	// An empty anchor indicates no prior established timeline.
	if anchor == nil || (anchor.ServerTimeUTC == 0 && anchor.ObservedWallUTC == 0 && anchor.MaxEffectiveUTC == 0) {
		return wallNow, nil
	}

	// Corrupt or negative anchor values invalidate trust.
	if anchor.ServerTimeUTC < 0 || anchor.ObservedWallUTC < 0 || anchor.MaxEffectiveUTC < 0 {
		return 0, &LicenseError{
			Code:    ErrClockUntrusted,
			Message: "persisted clock anchor contains invalid negative timestamp",
		}
	}

	// Detect wall clock rollback against prior recorded observation.
	if anchor.ObservedWallUTC > 0 && wallNow < anchor.ObservedWallUTC {
		return 0, &LicenseError{
			Code:    ErrClockUntrusted,
			Message: "wall clock rollback detected against recorded observation",
		}
	}

	// Session progression advances from the anchor high-water mark.
	elapsedSeconds := int64(sessionElapsed.Seconds())
	effectiveTime := anchor.MaxEffectiveUTC + elapsedSeconds
	if wallNow > effectiveTime {
		effectiveTime = wallNow
	}

	// Effective time cannot move behind the persisted high-water mark.
	if effectiveTime < anchor.MaxEffectiveUTC {
		return 0, &LicenseError{
			Code:    ErrClockUntrusted,
			Message: "effective time cannot decrease below high-water mark",
		}
	}

	return effectiveTime, nil
}
