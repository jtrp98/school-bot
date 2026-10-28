package main

import (
	"bytes"
	"flag"
	"fmt"
	"os"
	"path/filepath"
	"strconv"
	"strings"

	"github.com/tc-hib/winres"
	"github.com/tc-hib/winres/version"
)

func main() {
	iconPath := flag.String("icon", "", "source ICO file")
	output := flag.String("output", "", "generated Windows amd64 .syso file")
	appVersion := flag.String("version", "", "optional numeric app version, e.g. 0.1.2")
	verify := flag.String("verify", "", "verify resources in an existing Windows EXE instead of generating")
	flag.Parse()
	if err := run(*iconPath, *output, *appVersion, *verify); err != nil {
		fmt.Fprintln(os.Stderr, "Windows resources:", err)
		os.Exit(1)
	}
}

func run(iconPath, output, appVersion, verify string) error {
	if iconPath == "" || (output == "" && verify == "") {
		return fmt.Errorf("provide -icon and either -output or -verify")
	}
	file, err := os.Open(iconPath)
	if err != nil {
		return err
	}
	icon, err := winres.LoadICO(file)
	file.Close()
	if err != nil {
		return fmt.Errorf("read ICO: %w", err)
	}
	resources := &winres.ResourceSet{}
	// Wails v3 beta.30 loads group ID 3 for its native window icon. Keeping this
	// as the only group also makes it Explorer's first application icon.
	if err := resources.SetIcon(winres.ID(3), icon); err != nil {
		return err
	}
	if len(resources.Get(winres.RT_ICON, winres.ID(1), winres.LCIDNeutral)) == 0 {
		return fmt.Errorf("ICO must contain at least one image")
	}
	if appVersion != "" {
		numbers, err := numericVersion(appVersion)
		if err != nil {
			return err
		}
		info := version.Info{FileVersion: numbers, ProductVersion: numbers, Type: version.App}
		for key, value := range map[string]string{
			version.ProductName: "School Bot", version.FileDescription: "School Bot",
			version.CompanyName: "School Bot Team", version.OriginalFilename: "school-bot.exe",
			version.FileVersion: appVersion, version.ProductVersion: appVersion,
		} {
			if err := info.Set(version.LangDefault, key, value); err != nil {
				return err
			}
		}
		resources.SetVersionInfo(info)
	}
	if verify != "" {
		exe, err := os.Open(verify)
		if err != nil {
			return err
		}
		defer exe.Close()
		actual, err := winres.LoadFromEXE(exe)
		if err != nil {
			return fmt.Errorf("read PE resources: %w", err)
		}
		var mismatch error
		resources.Walk(func(kind, id winres.Identifier, lang uint16, expected []byte) bool {
			if !bytes.Equal(expected, actual.Get(kind, id, lang)) {
				mismatch = fmt.Errorf("PE resource mismatch: type %v, ID %v, language %d", kind, id, lang)
				return false
			}
			return true
		})
		if mismatch != nil {
			return mismatch
		}
		fmt.Printf("Verified %d Windows icon/version resources in %s\n", resources.Count(), verify)
		return nil
	}
	var object bytes.Buffer
	if err := resources.WriteObject(&object, winres.ArchAMD64); err != nil {
		return err
	}
	if err := os.MkdirAll(filepath.Dir(output), 0755); err != nil {
		return err
	}
	// Avoid replacing working resources with a partially written object.
	temporary, err := os.CreateTemp(filepath.Dir(output), ".windows-resources-*.tmp")
	if err != nil {
		return err
	}
	defer os.Remove(temporary.Name())
	if _, err := temporary.Write(object.Bytes()); err != nil {
		temporary.Close()
		return err
	}
	if err := temporary.Close(); err != nil {
		return err
	}
	if err := os.Rename(temporary.Name(), output); err != nil {
		return err
	}
	fmt.Printf("Generated %d Windows icon/version resources: %s\n", resources.Count(), output)
	return nil
}

func numericVersion(input string) ([4]uint16, error) {
	var output [4]uint16
	parts := strings.Split(input, ".")
	if len(parts) < 3 || len(parts) > 4 {
		return output, fmt.Errorf("version must contain three or four numeric components")
	}
	for i, part := range parts {
		n, err := strconv.ParseUint(part, 10, 16)
		if err != nil {
			return output, fmt.Errorf("invalid version %q", input)
		}
		output[i] = uint16(n)
	}
	return output, nil
}
