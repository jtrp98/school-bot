package main

import (
	"os"
	"path/filepath"
	"strings"
	"testing"
)

func TestRunRejectsOversizeAndMalformedInputBeforeManager(t *testing.T) {
	service := &DesktopService{}
	for _, data := range []string{strings.Repeat("x", 65537), "{"} {
		if _, err := service.Run("robot-control", "robot.stop", data); err == nil {
			t.Fatal("invalid input reached the manager")
		}
	}
}

func TestProjectReadBoundaries(t *testing.T) {
	service := &DesktopService{dataDir: t.TempDir()}
	path := filepath.Join(service.dataDir, "robot-control.json")
	for _, size := range []int{1 << 20, (1 << 20) + 1} {
		content := `{}` + strings.Repeat(" ", size-2)
		if err := os.WriteFile(path, []byte(content), 0600); err != nil {
			t.Fatal(err)
		}
		data, err := service.LoadProject("robot-control")
		if size == 1<<20 && (err != nil || data != content) {
			t.Fatalf("exact limit rejected: %v", err)
		}
		if size > 1<<20 && (err == nil || data != "") {
			t.Fatal("oversized project accepted")
		}
	}
}

func TestCurrentDeviceIDRequiresHex(t *testing.T) {
	for _, value := range []string{strings.Repeat("a", 32), strings.Repeat("F", 32)} {
		if !validDeviceID([]byte(value)) {
			t.Fatal("valid device ID rejected")
		}
	}
	for _, value := range []string{strings.Repeat("z", 32), strings.Repeat("a", 31), ""} {
		if validDeviceID([]byte(value)) {
			t.Fatal("invalid device ID accepted")
		}
	}
}

func TestDesktopServiceV2ProjectsTrashExportAndImport(t *testing.T) {
	service := &DesktopService{dataDir: t.TempDir()}

	// Create 2 projects in each mode (arm, code, block) and verify A does not overwrite B
	codeA, err := service.CreateProjectV2("code", "Code Project A", `{"kind":"code","source":"move_right(2)"}`)
	if err != nil {
		t.Fatalf("CreateProjectV2 code A failed: %v", err)
	}
	codeB, err := service.CreateProjectV2("code", "Code Project B", `{"kind":"code","source":"jump()"}`)
	if err != nil {
		t.Fatalf("CreateProjectV2 code B failed: %v", err)
	}
	if codeA.ProjectID == codeB.ProjectID {
		t.Fatal("expected distinct UUIDs for Code Project A and B")
	}

	blockA, err := service.CreateProjectV2("block", "Block Project A", `{"kind":"block","stacks":[]}`)
	if err != nil {
		t.Fatalf("CreateProjectV2 block A failed: %v", err)
	}
	blockB, err := service.CreateProjectV2("block", "Block Project B", `{"kind":"block","stacks":[]}`)
	if err != nil {
		t.Fatalf("CreateProjectV2 block B failed: %v", err)
	}
	if blockA.ProjectID == blockB.ProjectID {
		t.Fatal("expected distinct UUIDs for Block Project A and B")
	}

	armJSON := `{"kind":"arm","poses":[{"id":"11111111-1111-4111-8111-111111111111","name":"Home","values":[0,0,0,0,0,50],"duration_ms":1500,"joint_duration_ms":[1500,1500,1500,1500,1500,1500]}],"order":["11111111-1111-4111-8111-111111111111"],"groups":[],"joint_names":["J1","J2","J3","J4","J5","J6"],"time_mode":"same","loops":1,"view":"cards"}`
	armA, err := service.CreateProjectV2("arm", "Arm Project A", armJSON)
	if err != nil {
		t.Fatalf("CreateProjectV2 arm A failed: %v", err)
	}
	armB, err := service.CreateProjectV2("arm", "Arm Project B", armJSON)
	if err != nil {
		t.Fatalf("CreateProjectV2 arm B failed: %v", err)
	}
	if armA.ProjectID == armB.ProjectID {
		t.Fatal("expected distinct UUIDs for Arm Project A and B")
	}

	// Save A and ensure B is untouched
	updatedA, err := service.SaveProjectV2(codeA.ProjectID, 1, "Code Project A Updated", `{"kind":"code","source":"move_left(3)"}`)
	if err != nil || updatedA.LocalRevision != 2 {
		t.Fatalf("SaveProjectV2 failed: %v", err)
	}
	fetchedB, err := service.GetProjectV2(codeB.ProjectID)
	if err != nil || fetchedB.Content.Source != "jump()" || fetchedB.LocalRevision != 1 {
		t.Fatalf("saving A modified B: %+v %v", fetchedB, err)
	}

	// Export and re-open creates a new project without overwriting A or B
	exported, err := service.ExportProjectV2(codeA.ProjectID)
	if err != nil {
		t.Fatalf("ExportProjectV2 failed: %v", err)
	}
	reopened, err := service.OpenExportedProjectV2(exported)
	if err != nil {
		t.Fatalf("OpenExportedProjectV2 failed: %v", err)
	}
	if reopened.ProjectID == codeA.ProjectID || reopened.LocalRevision != 1 {
		t.Fatalf("reopened project must have a fresh UUID and revision 1: %+v", reopened)
	}

	// Delete to 30-day trash and restore
	trashMeta, err := service.DeleteProjectV2(codeB.ProjectID, 1)
	if err != nil || trashMeta.ProjectID != codeB.ProjectID {
		t.Fatalf("DeleteProjectV2 failed: %v", err)
	}
	trashList, err := service.ListTrashV2("code")
	if err != nil || len(trashList) != 1 {
		t.Fatalf("ListTrashV2 failed: %+v %v", trashList, err)
	}
	restoredB, err := service.RestoreProjectV2(codeB.ProjectID)
	if err != nil || restoredB.ProjectID != codeB.ProjectID {
		t.Fatalf("RestoreProjectV2 failed: %v", err)
	}
}

