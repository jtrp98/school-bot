//go:build !windows

package main

import "os/exec"

func configureTool(command *exec.Cmd) {}

func runToolProcess(command *exec.Cmd) error { return command.Run() }
