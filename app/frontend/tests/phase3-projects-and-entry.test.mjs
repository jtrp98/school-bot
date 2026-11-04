import test from "node:test";
import assert from "node:assert/strict";
import { fileURLToPath, pathToFileURL } from "node:url";
import { createRequire } from "node:module";

const frontend = new URL("../", import.meta.url);
const require = createRequire(new URL("package.json", frontend));
const { createServer } = await import(pathToFileURL(require.resolve("vite")).href);

globalThis.location = { search: "?preview=1" };
globalThis.window = {
  setTimeout,
  clearTimeout,
  setInterval,
  clearInterval,
  addEventListener() {},
  removeEventListener() {},
};
globalThis.document = {
  documentElement: { lang: "th" },
  querySelector() {
    return null;
  },
  querySelectorAll() {
    return [];
  },
  removeEventListener() {},
};

const vite = await createServer({
  configFile: false,
  root: fileURLToPath(frontend),
  server: { middlewareMode: true, hmr: false },
  appType: "custom",
});

let projectStoreV2;
let restoreNativeProject;
let createHostEntryAdapter;
let allowsEntry;
let registerWindowCloseGuard;
let i18nModule;

try {
  projectStoreV2 = await vite.ssrLoadModule("/src/core/projectStoreV2.ts");
  ({ restoreNativeProject } = await vite.ssrLoadModule(
    "/src/modes/robot-control/workspace/projectStore.ts",
  ));
  ({ createHostEntryAdapter } = await vite.ssrLoadModule("/src/app/entry/hostEntry.ts"));
  ({ allowsEntry } = await vite.ssrLoadModule("/src/app/entry/types.ts"));
  ({ registerWindowCloseGuard } = await vite.ssrLoadModule("/src/core/windowLifecycle.ts"));
  i18nModule = await vite.ssrLoadModule("/src/i18n/index.ts");
} finally {
  await vite.close();
}

const {
  InMemoryProjectLibraryBackend,
  serializeArmProject,
  deserializeArmProject,
  serializeCodeProject,
  deserializeCodeProject,
  serializeBlockProject,
  deserializeBlockProject,
  safeLoadWithFallback,
  safeSavePreservingState,
  sanitizeExportFileName,
} = projectStoreV2;

test("FE-004: Arm mode adapter preserves poses/groups and strips/rejects connected or verified state", () => {
  const armState = {
    poses: {
      p1: { name: "Home", v: [0, -20, 35, 10, 0, 50], t: 1500, ts: [1500, 1500, 1500, 1500, 1500, 1500] },
      p2: { name: "Grip", v: [15, -10, 40, 20, 5, 80], t: 2000, ts: [2000, 2000, 2000, 2000, 2000, 2000] },
    },
    seq: ["p1", "p2"],
    groups: [{ id: "g1", name: "Pick Sequence", keys: ["p1", "p2"], open: true }],
    counter: 3,
    projName: "Arm Pick Demo",
    timeMode: "split",
    view: "table",
    loops: 3,
  };

  const content = serializeArmProject(armState);
  assert.equal(content.kind, "arm");
  assert.equal(content.poses.length, 2);
  assert.equal(content.groups.length, 1);
  assert.equal("connected" in content, false);
  assert.equal("verified" in content, false);

  const restored = restoreNativeProject(content);
  assert.deepEqual(restored.seq, ["p1", "p2"]);
  assert.deepEqual(restored.poses.p1.v, [0, -20, 35, 10, 0, 50]);
  assert.deepEqual(restored.poses.p2.v, [15, -10, 40, 20, 5, 80]);
  assert.equal(restored.groups.length, 1);
  assert.deepEqual(restored.groups[0].keys, ["p1", "p2"]);
  assert.equal(restored.linked, -1);
  assert.equal(restored.synced, false);

  const injectedConnected = structuredClone(content);
  injectedConnected.connected = true;
  assert.throws(
    () => deserializeArmProject(injectedConnected),
    (err) => err.code === "PROJECT_RUNTIME_STATE_FORBIDDEN",
  );

  const injectedVerified = structuredClone(content);
  injectedVerified.poses[0].verified = true;
  assert.throws(
    () => deserializeArmProject(injectedVerified),
    (err) => err.code === "PROJECT_RUNTIME_STATE_FORBIDDEN",
  );
});

test("FE-004: Code and Block mode adapters validate schema limits and preserve existing state on failure", async () => {
  const validCode = serializeCodeProject({
    stageId: 2,
    code: "hero.move_right(2)\nhero.jump()\n",
  });
  assert.equal(validCode.kind, "code");
  const restoredCode = deserializeCodeProject(validCode);
  assert.equal(restoredCode.stageId, 2);
  assert.equal(restoredCode.code, "hero.move_right(2)\nhero.jump()\n");

  assert.throws(
    () => serializeCodeProject({ stageId: 1, code: "a".repeat(16385) }),
    (err) => err.code === "PROJECT_CODE_TOO_LARGE",
  );

  const validBlock = serializeBlockProject({
    stageId: 4,
    workspace: {
      blocks: {
        blocks: [{ type: "arm_jump", id: "b1" }],
      },
    },
    commands: [{ op: "move_right", value: 2 }, { op: "jump" }],
  });
  assert.equal(validBlock.kind, "block");
  const restoredBlock = deserializeBlockProject(validBlock);
  assert.equal(restoredBlock.stageId, 4);
  assert.deepEqual(restoredBlock.commands, [{ op: "move_right", value: 2 }, { op: "jump" }]);

  let deepBlock = { type: "arm_jump", id: "leaf" };
  for (let i = 0; i < 8; i++) {
    deepBlock = {
      type: "arm_repeat",
      id: `r_${i}`,
      inputs: { DO: { block: deepBlock } },
    };
  }
  assert.throws(
    () =>
      serializeBlockProject({
        stageId: 1,
        workspace: { blocks: [deepBlock] },
        commands: [{ op: "jump" }],
      }),
    (err) => err.code === "PROJECT_BLOCK_LIMIT",
  );

  const currentWorkspaceState = { stageId: 3, code: "hero.move_right(1)" };
  const failedLoad = await safeLoadWithFallback(async () => {
    return deserializeCodeProject({ kind: "code", language: "python-dsl", source: "" });
  }, currentWorkspaceState);
  assert.equal(failedLoad.ok, false);
  assert.deepEqual(failedLoad.state, currentWorkspaceState);

  const failedSave = await safeSavePreservingState(async () => {
    throw new Error("Simulated disk write failure");
  }, currentWorkspaceState);
  assert.equal(failedSave.ok, false);
  assert.deepEqual(failedSave.state, currentWorkspaceState);
});

test("FE-003: Multi-project library supports 2+ projects per mode without Project A overwriting Project B", async () => {
  const backend = new InMemoryProjectLibraryBackend();

  const armA = await backend.createProject(
    "guest",
    "Arm Project A",
    "arm",
    serializeArmProject({
      poses: { p1: { name: "Pose A", v: [10, 0, 0, 0, 0, 50], t: 1000, ts: [1000, 1000, 1000, 1000, 1000, 1000] } },
      seq: ["p1"],
    }),
  );
  const armB = await backend.createProject(
    "guest",
    "Arm Project B",
    "arm",
    serializeArmProject({
      poses: { p1: { name: "Pose B", v: [45, 0, 0, 0, 0, 50], t: 2000, ts: [2000, 2000, 2000, 2000, 2000, 2000] } },
      seq: ["p1"],
    }),
  );

  const codeA = await backend.createProject(
    "guest",
    "Code Project A",
    "code",
    serializeCodeProject({ stageId: 1, code: "hero.move_right(1)" }),
  );
  const codeB = await backend.createProject(
    "guest",
    "Code Project B",
    "code",
    serializeCodeProject({ stageId: 2, code: "hero.move_left(3)" }),
  );

  const blockA = await backend.createProject(
    "guest",
    "Block Project A",
    "block",
    serializeBlockProject({ stageId: 1, workspace: { blocks: [] }, commands: [{ op: "jump" }] }),
  );
  const blockB = await backend.createProject(
    "guest",
    "Block Project B",
    "block",
    serializeBlockProject({ stageId: 3, workspace: { blocks: [] }, commands: [{ op: "wait", value: 2 }] }),
  );

  const updatedCodeA = await backend.saveProject(
    "guest",
    codeA.project_id,
    1,
    "Code Project A v2",
    "code",
    serializeCodeProject({ stageId: 5, code: "hero.jump()\nhero.interact()" }),
  );
  assert.equal(updatedCodeA.local_revision, 2);

  await assert.rejects(
    () =>
      backend.saveProject(
        "guest",
        codeA.project_id,
        1,
        "Stale Save",
        "code",
        serializeCodeProject({ stageId: 1, code: "hero.wait(1)" }),
      ),
    (err) => err.code === "PROJECT_REVISION_CONFLICT",
  );

  const loadedArmA = await backend.getProject("guest", armA.project_id);
  const loadedArmB = await backend.getProject("guest", armB.project_id);
  const loadedCodeA = await backend.getProject("guest", codeA.project_id);
  const loadedCodeB = await backend.getProject("guest", codeB.project_id);
  const loadedBlockA = await backend.getProject("guest", blockA.project_id);
  const loadedBlockB = await backend.getProject("guest", blockB.project_id);

  assert.equal(loadedArmA.content.poses[0].name, "Pose A");
  assert.equal(loadedArmB.content.poses[0].name, "Pose B");
  assert.equal(loadedCodeA.content.source, "hero.jump()\nhero.interact()");
  assert.equal(loadedCodeB.content.source, "hero.move_left(3)");
  assert.deepEqual(loadedBlockA.content.compiled_preview.commands, [{ op: "jump" }]);
  assert.deepEqual(loadedBlockB.content.compiled_preview.commands, [{ op: "wait", value: 2 }]);
});

test("FE-017: Guest export (.schoolbot-project.json), re-open, 30-day trash restore/expiry, and explicit import guard", async () => {
  let currentTime = new Date("2026-03-01T10:00:00Z");
  const backend = new InMemoryProjectLibraryBackend(() => currentTime);

  const created = await backend.createProject(
    "guest",
    "My Robot Arm: Test?",
    "code",
    serializeCodeProject({ stageId: 2, code: "hero.move_right(4)" }),
  );

  const exported = await backend.exportProject("guest", created.project_id);
  assert.equal(exported.file_name, sanitizeExportFileName("My Robot Arm: Test?"));
  assert.equal(exported.file_name.endsWith(".schoolbot-project.json"), true);
  assert.equal(exported.envelope.format, "schoolbot-project");
  assert.equal(exported.envelope.schema_version, 2);

  const reopened = await backend.openExportedProject("guest", exported.raw_json);
  assert.notEqual(reopened.project_id, created.project_id);
  assert.equal(reopened.local_revision, 1);
  assert.equal(reopened.content.source, "hero.move_right(4)");

  const import1 = await backend.importGuestProject("student-101", created.project_id, "idem-key-1");
  assert.equal(import1.already_imported, false);
  assert.equal(import1.project.owner_scope, "student-101");

  const import2 = await backend.importGuestProject("student-101", created.project_id, "idem-key-1");
  assert.equal(import2.already_imported, true);
  assert.equal(import2.project.project_id, import1.project.project_id);

  const import3 = await backend.importGuestProject("student-101", created.project_id, "idem-key-2");
  assert.equal(import3.already_imported, true);
  assert.equal(import3.project.project_id, import1.project.project_id);

  const trashed = await backend.deleteProject("guest", created.project_id);
  assert.equal(trashed.project_id, created.project_id);

  currentTime = new Date("2026-03-20T10:00:00Z");
  const trashWithin30Days = await backend.listTrash("guest");
  assert.equal(trashWithin30Days.length, 1);

  const restored = await backend.restoreProject("guest", created.project_id);
  assert.equal(restored.project_id, created.project_id);
  assert.equal(restored.deleted_at, undefined);

  await backend.deleteProject("guest", reopened.project_id);
  currentTime = new Date("2026-04-21T10:00:01Z");
  await assert.rejects(
    () => backend.restoreProject("guest", reopened.project_id),
    (err) => err.code === "PROJECT_TRASH_EXPIRED",
  );
  const trashAfter30Days = await backend.listTrash("guest");
  assert.equal(trashAfter30Days.length, 0);
});

test("FE-002 & FE-005: HostEntryAdapter + close guard + i18n offline fallback and persistence", async () => {
  const events = [];
  let currentLicense = {
    status: "unactivated",
  };
  let sessionUser = "guest";

  const adapter = createHostEntryAdapter({
    licenseStatus: async () => currentLicense,
    activateLicense: async (key) => {
      events.push(`activate:${key}`);
      currentLicense = {
        status: "valid",
        expiresAt: "2026-12-31T00:00:00Z",
      };
      return currentLicense;
    },
    validateLicense: async () => {
      events.push("validate");
      return currentLicense;
    },
    loginSession: async (username, password) => {
      if (username === "student1" && password === "secret123") {
        sessionUser = username;
        events.push(`login:${username}`);
        return "success";
      }
      return "invalid_credentials";
    },
    logoutSession: async () => {
      sessionUser = "guest";
      events.push("logout");
    },
    stopAndSaveActiveWork: async () => {
      events.push("stopAndSave");
    },
  });

  const observedStatuses = [];
  const unsubscribe = adapter.subscribe((s) => observedStatuses.push(s.status));

  const initial = await adapter.readStatus();
  assert.equal(allowsEntry(initial), false);

  await adapter.activate("SB-2026-VALID-KEY");
  assert.equal(allowsEntry(currentLicense), true);
  assert.deepEqual(observedStatuses, ["valid"]);

  assert.equal(await adapter.login("student1", "wrong"), "invalid_credentials");
  assert.equal(await adapter.login("student1", "secret123"), "success");
  assert.equal(sessionUser, "student1");

  await adapter.logout();
  assert.equal(sessionUser, "guest");
  assert.equal(allowsEntry(currentLicense), true, "Logout must not revoke device license (AC-036-06)");

  await adapter.saveAndRequestStop();
  assert.equal(events.includes("stopAndSave"), true);
  unsubscribe();

  let guardCalled = false;
  const unregister = registerWindowCloseGuard(async () => {
    guardCalled = true;
  });
  unregister();
  assert.equal(guardCalled, false);

  assert.equal(i18nModule.isLanguage("th"), true);
  assert.equal(i18nModule.isLanguage("en"), true);
  assert.equal(i18nModule.isLanguage("zh"), true);
  assert.equal(i18nModule.isLanguage("fr"), false);
  await i18nModule.default.changeLanguage("fr");
  assert.equal(i18nModule.default.resolvedLanguage, "th");
  await i18nModule.default.changeLanguage("en");
  assert.equal(i18nModule.default.resolvedLanguage, "en");
  await i18nModule.default.changeLanguage("th");
});
