package project

import (
	"bytes"
	"context"
	"encoding/json"
	"errors"
	"os"
	"path/filepath"
	"testing"
	"time"
)

func sampleCodeContent(src string) *ProjectContent {
	return &ProjectContent{
		Kind:   ModeCode,
		Source: src,
	}
}

func sampleBlockContent(tiles int) *ProjectContent {
	b1, _ := NewUUIDv4()
	b2, _ := NewUUIDv4()
	return &ProjectContent{
		Kind: ModeBlock,
		Stacks: []Stack{
			{
				X: 12,
				Y: 24,
				Blocks: []Block{
					{ID: b1, Enabled: true, Type: "move", Direction: "move_right", Tiles: tiles},
					{ID: b2, Enabled: true, Type: "jump"},
				},
			},
		},
	}
}

func sampleArmContent() *ProjectContent {
	p1, _ := NewUUIDv4()
	p2, _ := NewUUIDv4()
	g1, _ := NewUUIDv4()
	return &ProjectContent{
		Kind: ModeArm,
		Poses: []Pose{
			{
				ID:              p1,
				Name:            "Home",
				Values:          [6]float64{0, 15, -15, 0, 10, 50},
				DurationMs:      1500,
				JointDurationMs: [6]int{1500, 1500, 1500, 1500, 1500, 1500},
			},
			{
				ID:              p2,
				Name:            "Pick",
				Values:          [6]float64{45, 30, -20, 10, -5, 80},
				DurationMs:      2000,
				JointDurationMs: [6]int{2000, 2000, 2000, 2000, 2000, 2000},
			},
		},
		Order: []string{p1, p2},
		Groups: []PoseGroup{
			{
				ID:      g1,
				Name:    "Cycle 1",
				PoseIDs: []string{p1, p2},
			},
		},
		JointNames: []string{"Base", "Shoulder", "Elbow", "Wrist Pitch", "Wrist Roll", "Gripper"},
		TimeMode:   "same",
		Loops:      2,
		View:       "cards",
	}
}

func assertStoreErrorCode(t *testing.T, err error, expectedCode string) {
	t.Helper()
	if err == nil {
		t.Fatalf("expected error with code %q, got nil", expectedCode)
	}
	var se *StoreError
	if !errors.As(err, &se) {
		t.Fatalf("expected *StoreError, got %T (%v)", err, err)
	}
	if se.Code != expectedCode {
		t.Fatalf("expected error code %q, got %q (%s)", expectedCode, se.Code, se.Message)
	}
}

func TestMultiProjectIsolationAndRestartPersistence(t *testing.T) {
	ctx := context.Background()
	dataDir := filepath.Join(t.TempDir(), "local-store")
	store, err := NewLocalStore(dataDir, InjectedEntitlement{Active: true})
	if err != nil {
		t.Fatalf("NewLocalStore failed: %v", err)
	}

	// Create 2 independent projects in each mode (arm, code, block) -> 6 projects total
	codeA, err := store.CreateProject(ctx, CreateProjectInput{
		Mode:    ModeCode,
		Name:    "Code Project A",
		Content: sampleCodeContent("move_right(2)"),
	})
	if err != nil {
		t.Fatalf("create codeA: %v", err)
	}
	codeB, err := store.CreateProject(ctx, CreateProjectInput{
		Mode:    ModeCode,
		Name:    "Code Project B",
		Content: sampleCodeContent("jump()"),
	})
	if err != nil {
		t.Fatalf("create codeB: %v", err)
	}

	blockA, err := store.CreateProject(ctx, CreateProjectInput{
		Mode:    ModeBlock,
		Name:    "Block Project A",
		Content: sampleBlockContent(3),
	})
	if err != nil {
		t.Fatalf("create blockA: %v", err)
	}
	blockB, err := store.CreateProject(ctx, CreateProjectInput{
		Mode:    ModeBlock,
		Name:    "Block Project B",
		Content: sampleBlockContent(7),
	})
	if err != nil {
		t.Fatalf("create blockB: %v", err)
	}

	armA, err := store.CreateProject(ctx, CreateProjectInput{
		Mode:    ModeArm,
		Name:    "Arm Project A",
		Content: sampleArmContent(),
	})
	if err != nil {
		t.Fatalf("create armA: %v", err)
	}
	armB, err := store.CreateProject(ctx, CreateProjectInput{
		Mode:    ModeArm,
		Name:    "Arm Project B",
		Content: sampleArmContent(),
	})
	if err != nil {
		t.Fatalf("create armB: %v", err)
	}

	// Update Project A in each mode and verify Project B is never overwritten
	codeAUpdated, err := store.SaveProject(ctx, SaveProjectInput{
		ProjectID:             codeA.ProjectID,
		ExpectedLocalRevision: 1,
		Name:                  "Code Project A v2",
		Content:               sampleCodeContent("move_left(5)"),
	})
	if err != nil {
		t.Fatalf("save codeA: %v", err)
	}
	if codeAUpdated.LocalRevision != 2 {
		t.Fatalf("expected revision 2, got %d", codeAUpdated.LocalRevision)
	}

	loadedCodeB, err := store.GetProject(ctx, codeB.ProjectID)
	if err != nil {
		t.Fatalf("get codeB: %v", err)
	}
	if loadedCodeB.Name != "Code Project B" || loadedCodeB.Content.Source != "jump()" || loadedCodeB.LocalRevision != 1 {
		t.Fatalf("codeB was unexpectedly modified: %+v", loadedCodeB)
	}

	// Stale revision conflict on codeA preserves v2
	_, err = store.SaveProject(ctx, SaveProjectInput{
		ProjectID:             codeA.ProjectID,
		ExpectedLocalRevision: 1,
		Name:                  "Stale Overwrite",
		Content:               sampleCodeContent("bad()"),
	})
	assertStoreErrorCode(t, err, ErrVersionConflict)

	// Invalid update on blockA preserves existing valid state
	_, err = store.SaveProject(ctx, SaveProjectInput{
		ProjectID:             blockA.ProjectID,
		ExpectedLocalRevision: 1,
		Name:                  "",
		Content:               sampleBlockContent(4),
	})
	assertStoreErrorCode(t, err, ErrProjectInvalid)

	loadedBlockA, err := store.GetProject(ctx, blockA.ProjectID)
	if err != nil {
		t.Fatalf("get blockA: %v", err)
	}
	if loadedBlockA.Name != "Block Project A" || loadedBlockA.LocalRevision != 1 {
		t.Fatalf("blockA state corrupted after failed save: %+v", loadedBlockA)
	}

	// Reopen store to simulate app restart
	restarted, err := NewLocalStore(dataDir, InjectedEntitlement{Active: true})
	if err != nil {
		t.Fatalf("restart NewLocalStore: %v", err)
	}
	all, err := restarted.ListProjects(ctx, "")
	if err != nil {
		t.Fatalf("ListProjects: %v", err)
	}
	if len(all) != 6 {
		t.Fatalf("expected 6 projects after restart, got %d", len(all))
	}
	reloadedArmA, err := restarted.GetProject(ctx, armA.ProjectID)
	if err != nil {
		t.Fatalf("get armA after restart: %v", err)
	}
	if len(reloadedArmA.Content.Poses) != 2 || len(reloadedArmA.Content.Groups) != 1 {
		t.Fatalf("armA poses/groups not preserved: %+v", reloadedArmA.Content)
	}
	_ = blockB
	_ = armB
}

func TestArmRuntimeStateRejectedAndCodecStrictness(t *testing.T) {
	doc := &LocalProjectDocument{
		Schema:        2,
		ProjectID:     "11111111-2222-4333-8444-555555555555",
		Mode:          ModeArm,
		Name:          "Strict Arm",
		LocalRevision: 1,
		UpdatedAt:     time.Now().UTC().Format(time.RFC3339),
		Content:       sampleArmContent(),
	}
	rawBytes, err := json.Marshal(doc)
	if err != nil {
		t.Fatalf("marshal valid doc: %v", err)
	}

	// Inject runtime field "connected": true into content
	var generic map[string]any
	if err := json.Unmarshal(rawBytes, &generic); err != nil {
		t.Fatalf("unmarshal generic: %v", err)
	}
	contentMap := generic["content"].(map[string]any)
	contentMap["connected"] = true
	tamperedBytes, _ := json.Marshal(generic)

	var decoded LocalProjectDocument
	err = json.Unmarshal(tamperedBytes, &decoded)
	assertStoreErrorCode(t, err, ErrProjectInvalid)

	// Inject runtime field "verified": true into content
	delete(contentMap, "connected")
	contentMap["verified"] = true
	tamperedBytes2, _ := json.Marshal(generic)
	err = json.Unmarshal(tamperedBytes2, &decoded)
	assertStoreErrorCode(t, err, ErrProjectInvalid)
}

func TestLegacyMigrationIdempotency(t *testing.T) {
	baseDir := t.TempDir()
	configDir := filepath.Join(baseDir, "config")
	storeDir := filepath.Join(baseDir, "store")

	sbDir := filepath.Join(configDir, "SchoolBotPoC")
	if err := os.MkdirAll(sbDir, 0700); err != nil {
		t.Fatalf("mkdir SchoolBotPoC: %v", err)
	}

	codeLegacy := `{"schema":1,"source":"move_right(3)\njump()"}`
	if err := os.WriteFile(filepath.Join(sbDir, "coding-adventure.json"), []byte(codeLegacy), 0600); err != nil {
		t.Fatalf("write legacy code: %v", err)
	}

	armLegacy := `{
		"kind": "zide-workspace",
		"version": 1,
		"state": {
			"seq": ["p1", "p2"],
			"poses": {
				"p1": {"name": "Home", "v": [0, 0, 0, 0, 0, 50], "t": 1500, "ts": [1500, 1500, 1500, 1500, 1500, 1500]},
				"p2": {"name": "Lift", "v": [10, 20, -10, 0, 0, 80], "t": 2000, "ts": [2000, 2000, 2000, 2000, 2000, 2000]}
			},
			"groups": [{"id": "g1", "name": "MainGroup", "keys": ["p1", "p2"]}],
			"jnames": ["J1", "J2", "J3", "J4", "J5", "Grip"],
			"timeMode": "same",
			"view": "cards",
			"loops": 1,
			"projName": "Migrated Arm"
		}
	}`
	if err := os.WriteFile(filepath.Join(sbDir, "robot-control.json"), []byte(armLegacy), 0600); err != nil {
		t.Fatalf("write legacy arm: %v", err)
	}

	store, err := NewLocalStore(storeDir, InjectedEntitlement{Active: true})
	if err != nil {
		t.Fatalf("NewLocalStore: %v", err)
	}

	results1, err := MigrateLegacyProjects(configDir, store)
	if err != nil {
		t.Fatalf("MigrateLegacyProjects 1: %v", err)
	}
	if len(results1) != 2 {
		t.Fatalf("expected 2 migration results, got %d", len(results1))
	}
	for _, r := range results1 {
		if r.Status != "migrated" {
			t.Fatalf("expected migrated status, got %+v", r)
		}
	}

	// Second run must skip up-to-date slots without duplicating projects
	results2, err := MigrateLegacyProjects(configDir, store)
	if err != nil {
		t.Fatalf("MigrateLegacyProjects 2: %v", err)
	}
	for _, r := range results2 {
		if r.Status != "skipped_up_to_date" {
			t.Fatalf("expected skipped_up_to_date on second run, got %+v", r)
		}
	}

	list, err := store.ListProjects(context.Background(), "")
	if err != nil || len(list) != 2 {
		t.Fatalf("expected 2 migrated projects, got %d (err=%v)", len(list), err)
	}
}

func TestGuestExportImportAndThirtyDayTrashLifecycle(t *testing.T) {
	ctx := context.Background()
	dataDir := filepath.Join(t.TempDir(), "guest-store")
	exportDir := t.TempDir()

	currentNow := time.Date(2026, 5, 1, 10, 0, 0, 0, time.UTC)
	store, err := NewLocalStore(dataDir, InjectedEntitlement{Active: true})
	if err != nil {
		t.Fatalf("NewLocalStore: %v", err)
	}
	store.SetNowFunc(func() time.Time { return currentNow })
	store.SetLastTrustedUTC(currentNow)

	// 1. Create and export a project in each mode; re-open without overwriting original
	origArm, err := store.CreateProject(ctx, CreateProjectInput{
		Mode:    ModeArm,
		Name:    "Guest Arm Lab",
		Content: sampleArmContent(),
	})
	if err != nil {
		t.Fatalf("create origArm: %v", err)
	}

	exportPath := filepath.Join(exportDir, "arm-demo"+ExportExtension)
	if err := store.ExportProjectToFile(ctx, origArm.ProjectID, exportPath); err != nil {
		t.Fatalf("ExportProjectToFile: %v", err)
	}

	// Verify exported JSON does NOT leak project_id or local_revision
	exportedBytes, err := os.ReadFile(exportPath)
	if err != nil {
		t.Fatalf("read exported file: %v", err)
	}
	if bytes.Contains(exportedBytes, []byte("project_id")) || bytes.Contains(exportedBytes, []byte("local_revision")) {
		t.Fatalf("exported file must omit internal identifiers: %s", string(exportedBytes))
	}

	// Modify original project to revision 2 before re-opening export
	origArmRev2, err := store.SaveProject(ctx, SaveProjectInput{
		ProjectID:             origArm.ProjectID,
		ExpectedLocalRevision: 1,
		Name:                  "Guest Arm Lab Edited",
		Content:               sampleArmContent(),
	})
	if err != nil {
		t.Fatalf("save origArmRev2: %v", err)
	}

	reopenedArm, err := store.OpenExportedProjectFromFile(ctx, exportPath)
	if err != nil {
		t.Fatalf("OpenExportedProjectFromFile: %v", err)
	}
	if reopenedArm.ProjectID == origArm.ProjectID {
		t.Fatalf("reopened export must receive a fresh UUIDv4, got same ID %s", reopenedArm.ProjectID)
	}
	if reopenedArm.LocalRevision != 1 || reopenedArm.Name != "Guest Arm Lab" {
		t.Fatalf("unexpected reopened metadata: %+v", reopenedArm)
	}

	// Confirm original project is untouched at revision 2
	checkOrig, err := store.GetProject(ctx, origArm.ProjectID)
	if err != nil || checkOrig.LocalRevision != 2 || checkOrig.Name != origArmRev2.Name {
		t.Fatalf("original project was overwritten by export open: %+v", checkOrig)
	}

	// 2. Corrupt export payload must be rejected without affecting existing projects
	_, err = store.OpenExportedProject(ctx, []byte(`{"schema":2,"exported_at":"2026-05-01T10:00:00Z","mode":"code","name":"Bad","extra":1,"content":{"kind":"code","source":"x"}}`))
	assertStoreErrorCode(t, err, ErrProjectInvalid)

	// 3. Soft-delete original project into 30-day trash
	trashMeta, err := store.DeleteProject(ctx, origArm.ProjectID)
	if err != nil {
		t.Fatalf("DeleteProject: %v", err)
	}
	if trashMeta.ProjectID != origArm.ProjectID || trashMeta.LocalRevision != 2 {
		t.Fatalf("unexpected trash metadata: %+v", trashMeta)
	}

	// Active store no longer returns deleted project
	_, err = store.GetProject(ctx, origArm.ProjectID)
	assertStoreErrorCode(t, err, ErrNotFound)

	// Advance 10 days (within 30-day window) and restore
	currentNow = currentNow.Add(10 * 24 * time.Hour)
	trashList, err := store.ListTrash(ctx, ModeArm)
	if err != nil || len(trashList) != 1 {
		t.Fatalf("expected 1 item in trash at +10d, got %d (err=%v)", len(trashList), err)
	}

	restored, err := store.RestoreProject(ctx, origArm.ProjectID)
	if err != nil {
		t.Fatalf("RestoreProject within 30d: %v", err)
	}
	if restored.ProjectID != origArm.ProjectID || restored.LocalRevision != 2 || restored.Name != "Guest Arm Lab Edited" {
		t.Fatalf("restored project did not preserve identity/revision/content: %+v", restored)
	}

	// 4. Delete again, test clock rollback protection (CLOCK_UNTRUSTED), then test 30-day expiry (TRASH_EXPIRED)
	if _, err := store.DeleteProject(ctx, origArm.ProjectID); err != nil {
		t.Fatalf("DeleteProject 2: %v", err)
	}

	// Roll clock back 1 hour behind lastTrustedUTC -> CLOCK_UNTRUSTED
	trustedAnchor := currentNow
	currentNow = trustedAnchor.Add(-1 * time.Hour)
	_, err = store.RestoreProject(ctx, origArm.ProjectID)
	assertStoreErrorCode(t, err, ErrClockUntrusted)

	// Restore clock and advance past 30 days
	currentNow = trustedAnchor.Add(30*24*time.Hour + time.Second)
	_, err = store.RestoreProject(ctx, origArm.ProjectID)
	assertStoreErrorCode(t, err, ErrTrashExpired)

	// ListTrash after expiry returns empty
	trashAfterExpiry, err := store.ListTrash(ctx, "")
	if err != nil || len(trashAfterExpiry) != 0 {
		t.Fatalf("expected 0 trash items after expiry, got %d (err=%v)", len(trashAfterExpiry), err)
	}
}

func TestExplicitGuestToCloudImportGuard(t *testing.T) {
	ctx := context.Background()
	dataDir := filepath.Join(t.TempDir(), "import-guard-store")
	store, err := NewLocalStore(dataDir, InjectedEntitlement{Active: true})
	if err != nil {
		t.Fatalf("NewLocalStore: %v", err)
	}

	localDoc, err := store.CreateProject(ctx, CreateProjectInput{
		Mode:    ModeCode,
		Name:    "My Guest Code",
		Content: sampleCodeContent("move_right(4)"),
	})
	if err != nil {
		t.Fatalf("CreateProject: %v", err)
	}

	dest1 := ImportDestination{
		APIOrigin: "https://api.schoolbright.local",
		SchoolID:  101,
		OwnerKind: "student",
		OwnerID:   5001,
	}

	callCount := 0
	var capturedKey, capturedHash string

	// 1. Simulate network failure on first attempt -> receipt stays "pending", local project intact
	_, pendingReceipt, err := store.ImportProjectToCloud(ctx, localDoc.ProjectID, dest1, func(_ context.Context, _ ImportDestination, req CloudImportRequest) (*CloudImportResponse, error) {
		callCount++
		capturedKey = req.IdempotencyKey
		capturedHash = req.RequestHash
		return nil, &StoreError{Code: ErrStoreUnavailable, Message: "simulated network timeout"}
	})
	assertStoreErrorCode(t, err, ErrStoreUnavailable)
	if pendingReceipt == nil || pendingReceipt.State != "pending" {
		t.Fatalf("expected pending receipt after network failure, got %+v", pendingReceipt)
	}

	// Verify local project is still intact after failed import
	checkLocal, err := store.GetProject(ctx, localDoc.ProjectID)
	if err != nil || checkLocal.Name != "My Guest Code" {
		t.Fatalf("local project missing or altered after failed import: %+v, err=%v", checkLocal, err)
	}

	// 2. Retry import -> reuses exact same request_key and request_hash and transitions receipt to "committed"
	resp1, committedReceipt, err := store.ImportProjectToCloud(ctx, localDoc.ProjectID, dest1, func(_ context.Context, _ ImportDestination, req CloudImportRequest) (*CloudImportResponse, error) {
		callCount++
		if req.IdempotencyKey != capturedKey || req.RequestHash != capturedHash {
			t.Fatalf("retry did not reuse idempotency key/hash: got (%s, %s), want (%s, %s)", req.IdempotencyKey, req.RequestHash, capturedKey, capturedHash)
		}
		return &CloudImportResponse{
			Schema:            2,
			ProjectID:         901,
			ProjectRevisionID: 1801,
			Mode:              req.Mode,
			Name:              req.Name,
			Revision:          1,
			UpdateDate:        time.Now().UTC().Format(time.RFC3339),
			Content:           req.Content,
		}, nil
	})
	if err != nil {
		t.Fatalf("retry ImportProjectToCloud failed: %v", err)
	}
	if resp1.ProjectID != 901 || committedReceipt.State != "committed" {
		t.Fatalf("expected committed receipt with cloud ID 901, got resp=%+v receipt=%+v", resp1, committedReceipt)
	}

	// 3. Repeated import of same unchanged local project to same destination short-circuits without calling uploader again
	resp2, _, err := store.ImportProjectToCloud(ctx, localDoc.ProjectID, dest1, func(_ context.Context, _ ImportDestination, _ CloudImportRequest) (*CloudImportResponse, error) {
		callCount++
		t.Fatalf("uploader must not be called when receipt is already committed with matching hash")
		return nil, nil
	})
	if err != nil {
		t.Fatalf("idempotent short-circuit failed: %v", err)
	}
	if !resp2.Replayed || resp2.ProjectID != 901 || callCount != 2 {
		t.Fatalf("expected replayed cloud project 901 with callCount=2, got resp=%+v callCount=%d", resp2, callCount)
	}

	// 4. Editing local project after committed import and attempting to re-import under same local_project_id returns IDEMPOTENCY_CONFLICT
	_, err = store.SaveProject(ctx, SaveProjectInput{
		ProjectID:             localDoc.ProjectID,
		ExpectedLocalRevision: 1,
		Name:                  "My Guest Code Edited",
		Content:               sampleCodeContent("move_right(8)"),
	})
	if err != nil {
		t.Fatalf("SaveProject: %v", err)
	}
	_, _, err = store.ImportProjectToCloud(ctx, localDoc.ProjectID, dest1, func(_ context.Context, _ ImportDestination, _ CloudImportRequest) (*CloudImportResponse, error) {
		return nil, nil
	})
	assertStoreErrorCode(t, err, ErrIdempotencyConflict)

	// 5. Importing to a different owner/school destination succeeds independently
	dest2 := ImportDestination{
		APIOrigin: "https://api.schoolbright.local",
		SchoolID:  101,
		OwnerKind: "student",
		OwnerID:   5002,
	}
	respDest2, receiptDest2, err := store.ImportProjectToCloud(ctx, localDoc.ProjectID, dest2, func(_ context.Context, _ ImportDestination, req CloudImportRequest) (*CloudImportResponse, error) {
		return &CloudImportResponse{
			Schema:            2,
			ProjectID:         902,
			ProjectRevisionID: 1802,
			Mode:              req.Mode,
			Name:              req.Name,
			Revision:          1,
			UpdateDate:        time.Now().UTC().Format(time.RFC3339),
			Content:           req.Content,
		}, nil
	})
	if err != nil || respDest2.ProjectID != 902 || receiptDest2.State != "committed" {
		t.Fatalf("import to second owner failed: resp=%+v receipt=%+v err=%v", respDest2, receiptDest2, err)
	}
}
