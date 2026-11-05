//go:build linux

package main

import "go.bug.st/serial/enumerator"

// go.bug.st/serial's Linux enumerator opens every /dev/ttyS* to tell real ports from
// placeholders, which resets the line settings under any program using one. The UNO and its
// clones are never ttyS, so only sysfs is read.
func portDetails() ([]*enumerator.PortDetails, error) {
	return sysfsPorts("/sys/class/tty")
}
