//go:build !linux

package main

import "go.bug.st/serial/enumerator"

func portDetails() ([]*enumerator.PortDetails, error) {
	return enumerator.GetDetailedPortsList()
}
