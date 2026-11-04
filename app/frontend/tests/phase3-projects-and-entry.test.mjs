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
let normalizePublicEntitlement;
let parseHostLicenseError;
let EntryScreen;
let allowsEntry;
let registerWindowCloseGuard;
let i18nModule;
let React;
let renderToStaticMarkup;

try {
  React = (await import(pathToFileURL(require.resolve("react")).href)).default;
  ({ renderToStaticMarkup } = await import(pathToFileURL(require.resolve("react-dom/server")).href));
  projectStoreV2 = await vite.ssrLoadModule("/src/core/projectStoreV2.ts");
  ({ restoreNativeProject } = await vite.ssrLoadModule(
    "/src/modes/robot-control/workspace/projectStore.ts",
  ));
  ({
    createHostEntryAdapter,
    normalizePublicEntitlement,
    parseHostLicenseError,
  } = await vite.ssrLoadModule("/src/app/entry/hostEntry.ts"));
  ({ EntryScreen } = await vite.ssrLoadModule("/src/app/entry/EntryScreen.tsx"));
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

test("FE-001: License UI entry gate, key activation, duplicate/invalid/expired/migrated states, and expiration display", async () => {
  await i18nModule.default.changeLanguage("th");
  const { t } = i18nModule;

  const renderScreen = (status, overrides = {}) =>
    renderToStaticMarkup(
      React.createElement(EntryScreen, {
        status,
        busy: false,
        recovering: false,
        error: "",
        loginAvailable: true,
        offlineAvailable: true,
        onActivate: async () => {},
        onValidate: async () => {},
        onOffline: async () => {},
        onGuest: async () => {},
        onRetryRecovery: async () => {},
        onLogin: async () => {},
        ...overrides,
      }),
    );

  // 1. Unactivated state (AC-009-01, AC-009-02): locks Guest & Login entry, renders License Key input
  let latestStatus = { status: "unactivated" };
  let hostBackendState = "unactivated";
  const adapter = createHostEntryAdapter({
    licenseStatus: async () => {
      if (hostBackendState === "unactivated") {
        return JSON.stringify({ status: "unactivated" });
      }
      return JSON.stringify({ status: "valid", expires_at: "2026-11-07T12:00:00Z" });
    },
    activateLicense: async (key) => {
      if (key === "SBOT-DUPLICATE") {
        throw new Error("[DEVICE_LIMIT] license server rejected request");
      }
      if (key === "SBOT-INVALID") {
        throw new Error("[LICENSE_INVALID] license is no longer active");
      }
      if (key === "SBOT-EXPIRED") {
        throw new Error("[LICENSE_INACTIVE] license is no longer active");
      }
      if (key === "SBOT-OFFLINE") {
        throw new Error(
          "[STORE_UNAVAILABLE] license server unreachable and local entitlement is expired or unactivated",
        );
      }
      hostBackendState = "valid";
      return JSON.stringify({ status: "valid", expires_at: "2026-11-07T12:00:00Z" });
    },
    validateLicense: async () => {
      if (hostBackendState === "moved") {
        throw new Error("[BINDING_REVOKED] license is no longer active");
      }
      if (hostBackendState === "offline_during_valid") {
        throw new Error(
          "[STORE_UNAVAILABLE] license server unreachable and local entitlement is expired or unactivated",
        );
      }
      if (hostBackendState === "lease_expired") {
        return JSON.stringify({ status: "expired", expires_at: "2026-11-07T12:00:00Z" });
      }
      return JSON.stringify({ status: "valid", expires_at: "2026-11-10T12:00:00Z" });
    },
  });

  adapter.subscribe((next) => {
    latestStatus = next;
  });

  latestStatus = await adapter.readStatus();
  assert.equal(allowsEntry(latestStatus), false);
  const unactivatedHtml = renderScreen(latestStatus);
  assert.ok(unactivatedHtml.includes('id="entry-license-key"'));
  assert.ok(unactivatedHtml.includes(t("entry.activation.title")));
  assert.ok(!unactivatedHtml.includes(t("entry.choice.guest")));
  assert.ok(!unactivatedHtml.includes(t("entry.choice.login")));

  // 2. Duplicate key (DEVICE_LIMIT), Invalid key (LICENSE_INVALID), Inactive key (LICENSE_INACTIVE) (AC-028-04)
  for (const [badKey, expectedCode] of [
    ["SBOT-DUPLICATE", "DEVICE_LIMIT"],
    ["SBOT-INVALID", "LICENSE_INVALID"],
    ["SBOT-EXPIRED", "LICENSE_INACTIVE"],
    ["SBOT-OFFLINE", "network"],
  ]) {
    await adapter.activate(badKey);
    assert.equal(allowsEntry(latestStatus), false);
    assert.equal(latestStatus.status, "unactivated");
    assert.equal(latestStatus.error, expectedCode);
    const errHtml = renderScreen(latestStatus);
    assert.ok(errHtml.includes(t(`entry.errors.${expectedCode}`)), `Missing error alert for ${expectedCode}`);
    assert.ok(
      errHtml.includes('id="entry-license-key"'),
      `Key input must remain available after ${expectedCode} failure`,
    );
    assert.ok(!errHtml.includes(t("entry.choice.guest")));
  }

  // 3. Successful activation normalizes Go snake_case expires_at -> expiresAt and unlocks Guest/Login
  await adapter.activate("SBOT-VALID-KEY-2026");
  assert.equal(latestStatus.status, "valid");
  assert.equal(latestStatus.expiresAt, "2026-11-07T12:00:00Z");
  assert.equal(allowsEntry(latestStatus), true);
  const validHtml = renderScreen(latestStatus);
  assert.ok(validHtml.includes(t("entry.choice.title")));
  assert.ok(validHtml.includes(t("entry.choice.guest")));
  assert.ok(validHtml.includes(t("entry.choice.login")));
  assert.ok(!validHtml.includes(t("entry.license.expiryUnknown")));

  // 4. Expiring soon warning when remaining time <= 24h
  const nowMs = Date.parse("2026-11-07T00:00:00Z");
  const soonStatus = normalizePublicEntitlement(
    { status: "valid", expires_at: "2026-11-07T12:00:00Z" },
    nowMs,
  );
  assert.equal(soonStatus.expiringSoon, true);
  const soonHtml = renderScreen(soonStatus);
  assert.ok(soonHtml.includes(t("entry.license.expiringSoon")));

  // 5. Network failure during active lease keeps offline-valid until original expiresAt
  hostBackendState = "offline_during_valid";
  await adapter.validate();
  assert.equal(latestStatus.status, "offline-valid");
  assert.equal(latestStatus.expiresAt, "2026-11-07T12:00:00Z");
  assert.equal(latestStatus.error, "network");
  assert.equal(allowsEntry(latestStatus), true);

  // 6. Lease expiration locks UI and offers retry / reactivate
  hostBackendState = "lease_expired";
  await adapter.validate();
  assert.equal(latestStatus.status, "expired");
  assert.equal(allowsEntry(latestStatus), false);
  const expiredHtml = renderScreen(latestStatus);
  assert.ok(expiredHtml.includes(t("entry.states.expired")));
  assert.ok(expiredHtml.includes(t("entry.lock.retry")));
  assert.ok(expiredHtml.includes(t("entry.lock.reactivate")));
  assert.ok(!expiredHtml.includes(t("entry.choice.guest")));

  // 7. Key migration / unbind revokes old device online (BINDING_REVOKED) and locks UI
  hostBackendState = "moved";
  await adapter.validate();
  assert.equal(latestStatus.status, "denied");
  assert.equal(latestStatus.error, "BINDING_REVOKED");
  assert.equal(allowsEntry(latestStatus), false);
  const movedHtml = renderScreen(latestStatus);
  assert.ok(movedHtml.includes(t("entry.errors.BINDING_REVOKED")));
  assert.ok(movedHtml.includes(t("entry.lock.reactivate")));
  assert.ok(!movedHtml.includes(t("entry.choice.guest")));

  // 8. Clock rollback maps to clock_untrusted state
  const clockUntrusted = parseHostLicenseError(new Error("[CLOCK_UNTRUSTED] wall clock rollback"), latestStatus);
  assert.deepEqual(clockUntrusted, { status: "clock_untrusted" });
  assert.equal(allowsEntry(clockUntrusted), false);
  const clockHtml = renderScreen(clockUntrusted);
  assert.ok(clockHtml.includes(t("entry.states.clock_untrusted")));
});
