import test from "node:test";
import assert from "node:assert/strict";
import { readFile, readdir } from "node:fs/promises";
import { fileURLToPath } from "node:url";
import { createServer } from "vite";
import React from "react";
import { renderToStaticMarkup } from "react-dom/server";

const storage = new Map([["school-bot:lang", "en"]]);
globalThis.localStorage = { getItem: key => storage.get(key) ?? null, setItem: (key, value) => storage.set(key, value) };
globalThis.location = { search: "" };
globalThis.window = { setTimeout: () => 0, clearTimeout() {}, setInterval: () => 0, clearInterval() {}, addEventListener() {}, removeEventListener() {} };
globalThis.document = { documentElement: { lang: "" }, querySelector: () => null, querySelectorAll: () => [], addEventListener() {}, removeEventListener() {} };
const server = await createServer({ configFile: false, root: fileURLToPath(new URL("../", import.meta.url)), cacheDir: "node_modules/.vite-i18n-tests", optimizeDeps: { noDiscovery: true, include: [] }, server: { middlewareMode: true, hmr: false }, appType: "custom" });
let i18n, t, WorkspaceLogic, Template, scenarios, LearnMap, LessonsModeView, errorText, EngineError, RichText;
try {
  ({ default: i18n, t } = await server.ssrLoadModule("/src/i18n/index.ts"));
  ({ WorkspaceLogic } = await server.ssrLoadModule("/src/modes/robot-control/workspace/WorkspaceLogic.ts"));
  ({ Template } = await server.ssrLoadModule("/src/modes/robot-control/workspace/template.tsx"));
  ({ SCENARIO_LIST: scenarios } = await server.ssrLoadModule("/src/modes/robot-control/workspace/scenarios.ts"));
  ({ default: LearnMap } = await server.ssrLoadModule("/src/modes/coding-adventure/LearnMap.tsx"));
  ({ LessonsModeView } = await server.ssrLoadModule("/src/modes/coding-adventure/LessonsMode.tsx"));
  errorText = await server.ssrLoadModule("/src/modes/robot-control/engineErrorText.ts");
  ({ EngineError } = await server.ssrLoadModule("/src/core/engineError.ts"));
  ({ RichText } = await server.ssrLoadModule("/src/components/shared/RichText.tsx"));
} finally { await server.close(); }

const flatten = (object, prefix = "") => Object.entries(object).flatMap(([key, value]) => {
  const path = prefix ? `${prefix}.${key}` : key;
  return typeof value === "string" ? [[path, value]] : flatten(value, path);
});
const catalogs = Object.fromEntries(await Promise.all(["th", "en", "zh"].map(async language => [language,
  Object.fromEntries(flatten(JSON.parse(await readFile(new URL(`../src/i18n/locales/${language}.json`, import.meta.url), "utf8"))))
])));
const placeholders = text => [...new Set([...text.matchAll(/\{\{\s*([^},]+)\s*\}\}/g)].map(match => match[1].trim()))].sort();
const assertNoKeys = (html, context = "") => {
  const leaked = [...html.matchAll(/(?:robot|common|modal|nav|home|lessons|shell|adventure|blocks|profile|window|settings|language)\.[a-zA-Z][\w.]*/g)].find(match => catalogs.th[match[0]] !== undefined);
  assert.equal(leaked?.[0], undefined, context);
};

test("startup restores the saved language and switching persists it", async () => {
  assert.equal(i18n.resolvedLanguage, "en");
  assert.equal(t("nav.home"), "Home");
  await i18n.changeLanguage("zh");
  assert.equal(t("nav.home"), "首页");
  assert.equal(document.documentElement.lang, "zh");
  assert.equal(storage.get("school-bot:lang"), "zh");
});

test("all three catalogs have matching semantic keys and interpolation parameters", () => {
  const keys = Object.keys(catalogs.th).sort();
  for (const language of ["en", "zh"]) {
    assert.deepEqual(Object.keys(catalogs[language]).sort(), keys);
    for (const key of keys) {
      assert.doesNotMatch(key, /[ก-๙\s]/);
      assert.deepEqual(placeholders(catalogs[language][key]), placeholders(catalogs.th[key]), `${language}: ${key}`);
      assert.ok(catalogs[language][key].trim(), `${language}: ${key}`);
    }
  }
});

test("every literal translation key used by the frontend exists in the catalogs", async () => {
  const root = new URL("../src/", import.meta.url);
  for (const file of await readdir(root, { recursive: true })) {
    if (!/\.tsx?$/.test(file) || file.replaceAll("\\", "/").startsWith("i18n/")) continue;
    const source = await readFile(new URL(file.replaceAll("\\", "/"), root), "utf8");
    for (const match of source.matchAll(/(?:\b(?:t|translate)\(\s*|\bi18nKey=)"([^"]+)"/g)) {
      assert.ok(catalogs.th[match[1]] !== undefined, `${file}: ${match[1]}`);
    }
  }
});

// Thai or Chinese text is allowed in source only where it is sample data for Sample mode.
const SAMPLE_DATA_FILES = {
  "shell.tsx": "placeholder student profile and the robot/project names shown only in Sample mode",
  "home/homeData.ts": "sample projects and folder paths for the Home sample scenarios",
  "modes/robot-control/works/worksData.ts": "sample project list for the Works sample scenarios",
  "modes/robot-control/connect/connectData.ts": "sample port and robot names for the Connect sample flow",
  "modes/robot-control/workspace/workspaceState.ts": "sample pose and log fixtures; workspace is being rewritten",
  "data/sampleScenarios.ts": "sample group name fixture for the workspace scenarios",
  "modes/robot-control/workspace/WorkspaceLogic.ts": "sample group name fixture; workspace is being rewritten",
  "modes/robot-control/workspace/ArmWorkspace.tsx": "sample robot name fixture; workspace is being rewritten",
};
const NON_LATIN_TEXT = /[฀-๿㐀-鿿]/;
const JOINED_TRANSLATION = /\b(?:t|translate)\(\s*"[^"]*"(?:\s*,\s*\{[^{}]*\})?\s*\)\s*\+|\+\s*(?:t|translate)\(/;
const withoutComments = source => source.replace(/\/\*[\s\S]*?\*\//g, "").replace(/^\s*\/\/.*$/gm, "");
const sourceFiles = async () => {
  const root = new URL("../src/", import.meta.url);
  const files = [];
  for (const file of await readdir(root, { recursive: true })) {
    const name = file.replaceAll("\\", "/");
    if (!/\.tsx?$/.test(name) || name.startsWith("i18n/")) continue;
    files.push({ name, code: withoutComments(await readFile(new URL(name, root), "utf8")) });
  }
  return files;
};

test("source files contain no Thai or Chinese text outside the catalogs and the listed sample data", async () => {
  const files = await sourceFiles();
  const offenders = files.filter(file => !(file.name in SAMPLE_DATA_FILES) && NON_LATIN_TEXT.test(file.code));
  assert.deepEqual(offenders.map(file => file.name), []);
  for (const name of Object.keys(SAMPLE_DATA_FILES)) {
    assert.match(files.find(file => file.name === name)?.code ?? "", NON_LATIN_TEXT, `${name} no longer needs its entry`);
  }
});

test("translated text is never joined with + because word order differs by language", async () => {
  const files = await sourceFiles();
  const offenders = files.filter(file => JOINED_TRANSLATION.test(file.code));
  assert.deepEqual(offenders.map(file => file.name), []);
});

test("workspace sentences keep selection, group names and language-specific word order", async () => {
  const groupName = "<group&{{poseCount}}>";
  const cases = [
    ["th", `เพิ่ม 1 ท่าเข้าท่าชุด “${groupName}” · ท่าที่อยู่หลังจะย้ายขึ้นมาติดท่า 1`,
      `ขยับแขน = แก้ท่า 1 ทันที (ในชุด “${groupName}”)`, "กำลังจับท่า 1 ด้วยมือ"],
    ["en", `Add 1 poses to group “${groupName}” · Following poses will move next to pose 1.`,
      `Moving the arm edits pose 1 immediately (in group “${groupName}”)`, "Positioning pose 1 by hand"],
    ["zh", `添加 1 个姿势到姿势组“${groupName}” · 后续姿势将移至姿势 1 旁。`,
      `移动机械臂即立即编辑姿势 1（在姿势组“${groupName}”）`, "正在手动调整姿势 1"],
  ];
  for (const [language, selection, heading, capture] of cases) {
    await i18n.changeLanguage(language);
    const screen = new WorkspaceLogic({ demo: "linked", conn: "connected" });
    const [first, second, , fourth] = screen.state.seq;
    Object.assign(screen.state, {
      groups: [{ id: "group", name: groupName, keys: [first, second], open: true }],
      picks: [first, second, fourth],
      sel: 0,
      linked: 0,
    });
    const before = JSON.stringify(screen.state);
    const view = screen.renderVals();
    assert.equal(view.selText, selection);
    assert.equal(view.jointTitle, heading);
    assert.equal(JSON.stringify(screen.state), before);
    screen.state.release = true;
    assert.equal(screen.renderVals().f6Sync, capture);
  }
});

test("workspace text follows save, memory, range, motion and duration state without changing actions", async () => {
  await i18n.changeLanguage("en");
  const screen = new WorkspaceLogic({ demo: "linked", conn: "connected" });
  screen.setState = patch => Object.assign(screen.state, patch);
  screen.tm = () => 0;
  const sequence = screen.state.seq.slice();
  screen.state.savedSnap = screen.snapOf(screen.state);
  assert.equal(screen.renderVals().saveStateText, "✓ Last saved 10:20");
  screen.state.savedSnap = null;
  assert.equal(screen.renderVals().saveStateText, "✓ Last saved 10:20 · New changes");
  Object.assign(screen.state, { linked: 2, sel: 0, timeMode: "split" });
  let view = screen.renderVals();
  assert.equal(view.f6Sync, "Robot is at pose 3");
  assert.equal(view.hdrGoLabel, "▶ Move robot to pose 1");
  assert.equal(view.goSelLabel, "Move robot to pose 1");
  assert.equal(view.servoHeads[0].sub, "-180…+180° · Duration ms");
  assert.equal(view.servoHeads[5].sub, "Opening % · Duration ms");
  screen.state.st = "moving";
  assert.equal(screen.renderVals().hdrGoLabel, "Robot moving…");
  Object.assign(screen.state, { st: "ready", src: "robot", synced: true,
    robotSeq: { n: 4, ms: 12000, at: "11:22" }, rangeOn: true });
  view = screen.renderVals();
  assert.equal(view.srcNote, "Robot memory: 4 poses · Uploaded 11:22 · Matches app");
  assert.match(view.sendTip, /Sends all 4 poses, not just the selected range$/);
  screen.state.synced = false;
  assert.equal(screen.renderVals().srcNote, "Robot memory: 4 poses · Uploaded 11:22 · Differs from app");
  screen.state.poses[screen.state.seq[1]].v[0] = 999;
  assert.equal(screen.renderVals().sendTip,
    "Poses 2 have invalid values. Fix the red fields before uploading.");
  const inserted = [];
  screen.insertPose = (...args) => inserted.push(args);
  Object.assign(screen.state, { seq: [], sel: -1 });
  screen.renderVals().addPose();
  assert.equal(inserted[0][2], "Add pose 1 (first pose: all servos at 0)");
  assert.deepEqual(inserted[0][1].v, [0, 0, 0, 0, 0, 0]);
  assert.deepEqual(inserted[0][1].ts, [3000, 3000, 3000, 3000, 3000, 3000]);
  screen.state.seq = sequence;
  screen.renderVals().addPose();
  assert.equal(inserted[1][2], "Add pose 5 from the current robot position");
  assert.deepEqual(inserted[1][1].v, screen.state.cur);
  const logs = [];
  screen.clearAll = () => {};
  screen.glide = () => {};
  screen.addLog = (_type, message) => logs.push(message);
  screen.state.cur = screen.POSE0.v.slice();
  screen.renderVals().goPose0();
  screen.state.cur[0] = 90;
  screen.renderVals().goPose0();
  assert.deepEqual(logs, ["Move robot to pose 0, upright arm", "Move robot to pose 0, upright arm (large movement)"]);
});

test("rich sentences style their own tag and keep tag-like user text literal", async () => {
  await i18n.changeLanguage("en");
  const html = renderToStaticMarkup(React.createElement(RichText, {
    i18nKey: "robot.connect.messages.reconnectCableFor",
    values: { name: "<bold>Arm</bold> & Co" },
    components: { bold: React.createElement("strong") },
  }));
  assert.equal(html.match(/<strong>/g)?.length, 1);
  assert.match(html, /&lt;bold&gt;Arm&lt;\/bold&gt; &amp; Co/);
  assert.match(html, /^Reconnect the USB cable for <strong>/);
});

test("engine error codes map to full translated sentences in every language", async () => {
  const keys = [...Object.values(errorText.ENGINE_ERROR_KEYS), errorText.UNEXPECTED_ENGINE_ERROR_KEY];
  for (const language of ["th", "en", "zh"]) {
    for (const key of keys) assert.ok(catalogs[language][key], `${language}: ${key}`);
  }
  for (const code of ["ENGINE_BUSY", "ENGINE_CANCELLED", "ENGINE_FAILED", "ENGINE_TIMEOUT", "TRANSPORT_ERROR", "DEVICE_TIMEOUT"]) {
    assert.ok(errorText.ENGINE_ERROR_KEYS[code], code);
  }
  await i18n.changeLanguage("en");
  const detail = "exit status 1; stderr: boom";
  assert.equal(errorText.engineErrorText(new EngineError("ENGINE_FAILED", detail)), t("robot.errors.engineFailed", { detail }));
  assert.ok(errorText.engineErrorText(new EngineError("ENGINE_FAILED", detail)).includes(detail), "diagnostic stays verbatim");
  assert.equal(errorText.engineErrorText(new EngineError("ENGINE_BUSY", "engine is busy")), t("robot.errors.engineBusy"));
  assert.equal(errorText.engineErrorText(new EngineError("PROTOCOL_INVALID", "x")), t("robot.errors.unexpected", { detail: "PROTOCOL_INVALID: x" }));
  const adapterBusy = Object.assign(new Error("robot transport is busy"), { code: "ENGINE_BUSY" });
  assert.equal(errorText.engineErrorText(adapterBusy), t("robot.errors.engineBusy"));
  assert.equal(errorText.engineErrorText(new Error("plain")), "plain");
  assert.equal(errorText.engineErrorText("text"), "text");
  await i18n.changeLanguage("th");
  assert.notEqual(errorText.engineErrorText(new EngineError("ENGINE_TIMEOUT", "x")), t("robot.errors.engineTimeout", { lng: "en" }));
});

test("interpolation keeps user content literal and React escapes it", async () => {
  await i18n.changeLanguage("en");
  const name = "<img src=x onerror=alert(1)> {{count}} หยุด";
  const message = t("home.greeting", { name });
  assert.equal(message, `Hello, ${name}`);
  const html = renderToStaticMarkup(React.createElement("span", null, message));
  assert.ok(html.includes("&lt;img"));
  assert.ok(!html.includes("<img"));
  assert.equal(t("Unrecognized device diagnostic"), "Unrecognized device diagnostic");
  assert.equal(t("modal.leave.go", { page: t("nav.lessons") }), "Go to Lessons");
});

test("language changes preserve robot project names, poses, selection and commands", async () => {
  const screen = new WorkspaceLogic({ demo: "linked", conn: "connected", workName: "หยุด" });
  screen.tm = () => 0;
  screen.iv = () => 0;
  screen.state.poses[screen.state.seq[0]].name = "บันทึก";
  const project = () => JSON.stringify({ name: screen.state.projName, poses: screen.state.poses, seq: screen.state.seq, groups: screen.state.groups, selected: screen.state.sel, values: screen.state.cur });
  const before = project();
  for (const language of ["th", "en", "zh", "th"]) {
    await i18n.changeLanguage(language);
    const view = screen.renderVals();
    assert.equal(view.topBar.project.name, "หยุด");
    assert.equal(project(), before);
    assertNoKeys(renderToStaticMarkup(React.createElement(Template, { v: view })));
  }
});

test("74 robot workspace cases render in every language without exposing translation keys", async () => {
  for (const language of ["th", "en", "zh"]) {
    await i18n.changeLanguage(language);
    for (const scenario of scenarios) {
      const screen = new WorkspaceLogic({ ...scenario.props, init: scenario.init, boot: scenario.boot, conn: scenario.conn ?? "connected" });
      screen.setState = patch => { screen.state = { ...screen.state, ...(typeof patch === "function" ? patch(screen.state) : patch) }; };
      screen.tm = () => 0;
      screen.iv = () => 0;
      screen.componentDidMount();
      for (const native of [false, true]) {
        const v = screen.renderVals();
        if (native) Object.assign(v, { nativeHardware: true, nativeConnected: true, robotName: "หุ่นของฉัน", robotPort: "COM7", nativeProfile: "โปรไฟล์ของฉัน" });
        assertNoKeys(renderToStaticMarkup(React.createElement(Template, { v })), `${language}/${scenario.id}/${native}`);
      }
      screen.componentWillUnmount();
    }
  }
});

test("all nine lesson scenarios render in all three languages", async () => {
  for (const language of ["th", "en", "zh"]) {
    await i18n.changeLanguage(language);
    for (const scenario of ["map4", "list", "done2", "locked6", "run5", "celebrate", "complete", "start", "noRobot"]) {
      location.search = `?preview=1&scenario=${scenario}`;
      const html = renderToStaticMarkup(React.createElement(LearnMap));
      assertNoKeys(html);
      assert.ok(html.includes(t("lessons.details.title")) || html.includes(t("lessons.messages.unitSummary")));
    }
  }
});

test("lessons open with mode choices and each choice shows only its own lesson content", async () => {
  location.search = "?preview=1";
  const handlers = { chooseMode() {}, backToModes() {} };
  for (const language of ["th", "en", "zh"]) {
    await i18n.changeLanguage(language);
    const picker = renderToStaticMarkup(React.createElement(LessonsModeView, { ...handlers, mode: null }));
    assert.ok(picker.includes(t("lessons.modes.title")));
    for (const mode of ["block", "arm", "code"]) {
      assert.ok(picker.includes(t(`lessons.modes.${mode}.title`)));
      const html = renderToStaticMarkup(React.createElement(LessonsModeView, { ...handlers, mode }));
      assertNoKeys(html);
      assert.ok(html.includes(t("lessons.modes.change")));
      if (mode === "arm") {
        assert.ok(html.includes(t("lessons.messages.unit2MeetTheRobotArm")));
      } else {
        assert.ok(html.includes(t("lessons.modes.unavailable", { modeName: t(`lessons.modes.${mode}.title`) })));
        assert.ok(!html.includes(t("lessons.messages.unit2MeetTheRobotArm")));
      }
    }
    assert.ok(!picker.includes(t("lessons.messages.unit2MeetTheRobotArm")));
  }
});
