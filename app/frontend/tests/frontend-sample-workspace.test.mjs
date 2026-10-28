import test from "node:test";
import assert from "node:assert/strict";
import { createRequire } from "node:module";
import { fileURLToPath, pathToFileURL } from "node:url";

const frontend = new URL("../", import.meta.url);
const require = createRequire(new URL("package.json", frontend));
const { createServer } = await import(pathToFileURL(require.resolve("vite")).href);
globalThis.location = { search: "" };
globalThis.document = { removeEventListener() {} };
globalThis.window = { addEventListener() {}, removeEventListener() {} };
const vite = await createServer({
  configFile: false,
  root: fileURLToPath(frontend),
  server: { middlewareMode: true, hmr: false },
  appType: "custom",
});
let WorkspaceLogic, NativeWorkspace, createWorkspaceKeyboardActions, createWorkspaceSelectionActions;
try {
  ({ WorkspaceLogic } = await vite.ssrLoadModule("/src/modes/robot-control/workspace/WorkspaceLogic.ts"));
  ({ NativeWorkspace } = await vite.ssrLoadModule("/src/modes/robot-control/workspace/NativeWorkspace.tsx"));
  ({ createWorkspaceKeyboardActions } = await vite.ssrLoadModule(
    "/src/modes/robot-control/workspace/workspaceKeyboardActions.ts",
  ));
  ({ createWorkspaceSelectionActions } = await vite.ssrLoadModule(
    "/src/modes/robot-control/workspace/workspaceSelectionActions.ts",
  ));
} finally {
  await vite.close();
}

function fixture(context) {
  let clock = 0;
  let nextId = 1;
  const timers = new Map();
  const schedule = (callback, delay, repeat = false) => {
    const id = nextId++;
    timers.set(id, { callback, delay, due: clock + delay, repeat });
    return id;
  };
  context.mock.method(globalThis, "clearTimeout", id => timers.delete(id));
  context.mock.method(globalThis, "clearInterval", id => timers.delete(id));
  context.mock.method(Date, "now", () => clock);
  window.setTimeout = (callback, delay) => schedule(callback, delay);
  window.setInterval = (callback, delay) => schedule(callback, delay, true);
  const workspace = new WorkspaceLogic({ sample4c: "none" });
  workspace.setState = patch => {
    const next = typeof patch === "function" ? patch(workspace.state, workspace.props) : patch;
    workspace.state = { ...workspace.state, ...next };
  };
  const pose = (name, value) => ({ name, v: [value, 0, 0, 0, 0, 50], t: 1000, ts: Array(6).fill(1000) });
  workspace.setState({
    poses: { first: pose("First", 10), second: pose("Second", 20) },
    seq: ["first", "second"],
    sel: 0,
    linked: 0,
    log: [],
  });
  const advance = duration => {
    const until = clock + duration;
    for (;;) {
      const next = [...timers].filter(([, timer]) => timer.due <= until)
        .sort((left, right) => left[1].due - right[1].due)[0];
      if (!next) break;
      const [id, timer] = next;
      clock = timer.due;
      if (timer.repeat) timer.due += timer.delay;
      else timers.delete(id);
      timer.callback();
    }
    clock = until;
  };
  return { workspace, advance, timers };
}

test("sample playback preserves preparation, split duration, sequence completion and selected snapshot", context => {
  const { workspace, advance } = fixture(context);
  workspace.state.timeMode = "split";
  workspace.state.poses.first.ts[1] = 2000;
  workspace.state.sel = 1;
  workspace.play();
  assert.equal(workspace.state.st, "preparing");
  advance(899);
  assert.equal(workspace.state.st, "preparing");
  advance(1);
  assert.equal(workspace.state.st, "playing");
  assert.deepEqual(workspace.state.moveMs, [1000, 2000, 1000, 1000, 1000, 1000]);
  advance(1900);
  assert.equal(workspace.state.pos, 0);
  advance(100);
  assert.equal(workspace.state.pos, 1);
  assert.equal(workspace.state.cur[0], 20);
  advance(1000);
  assert.equal(workspace.state.st, "done");
  assert.equal(workspace.state.linked, 1);
  assert.equal(workspace.state.snap.key, "second");
});

test("sample stop cancels preparation and freezes a pending visual glide", context => {
  const { workspace, advance, timers } = fixture(context);
  workspace.play();
  workspace.stopAll();
  advance(2000);
  assert.equal(workspace.state.st, "stopped");
  assert.equal(timers.size, 0);
  workspace.glide([100, 0, 0, 0, 0, 50], Array(6).fill(1000));
  advance(500);
  workspace.stopAll();
  assert.equal(workspace.state.cur[0], 50);
  advance(1000);
  assert.equal(workspace.state.cur[0], 50);
  assert.equal(workspace.state.moveMs, null);
});

test("sample hold repeats only until release and unmount cancels every registered timer", context => {
  const { workspace, advance, timers } = fixture(context);
  workspace.beginHold(0, 1);
  assert.equal(workspace.state.cur[0], 3);
  advance(140);
  assert.equal(workspace.state.cur[0], 6);
  workspace.endHold();
  advance(500);
  assert.equal(workspace.state.cur[0], 6);
  assert.equal(workspace.state.holding, null);
  workspace.relStart();
  workspace.beforeUnmount();
  advance(130000);
  assert.equal(timers.size, 0);
  assert.equal(workspace._dead, true);
  assert.equal(workspace.state.relIdle, false);
});

test("sample release drifts within limits and countdown cancellation keeps joints released", context => {
  const { workspace, advance } = fixture(context);
  workspace.relStart();
  advance(2000);
  assert.deepEqual(workspace.state.cur, [0, 15, 15, 10, 0, 0]);
  workspace.relEndAsk();
  advance(1000);
  assert.equal(workspace.state.relCount, 2);
  workspace.relCancelEnd();
  advance(3000);
  assert.equal(workspace.state.release, true);
  assert.equal(workspace.state.relCount, 0);
  workspace.relEndAsk();
  advance(3000);
  assert.equal(workspace.state.release, false);
  assert.deepEqual(workspace.state.relRead, []);
});

test("keyboard actions keep input shortcuts and dispatch Escape through the current stop override", context => {
  const { workspace } = fixture(context);
  const actions = createWorkspaceKeyboardActions(workspace);
  let stops = 0;
  let copies = 0;
  workspace.stopAll = () => stops++;
  workspace.pCopy = () => copies++;
  const event = (key, target = null) => ({ key, target, ctrlKey: true, preventDefault() {} });
  actions.shortcut(event("c", { tagName: "INPUT", selectionStart: 0, selectionEnd: 1 }));
  assert.equal(copies, 0);
  actions.shortcut(event("c", { tagName: "INPUT", selectionStart: 1, selectionEnd: 1 }));
  assert.equal(copies, 1);
  workspace.setState({ menuOpen: true });
  actions.escape(event("Escape"));
  assert.equal(workspace.state.menuOpen, false);
  assert.equal(stops, 0);
  workspace.setState({ st: "playing" });
  actions.escape(event("Escape"));
  assert.equal(stops, 1);
  actions.escape(event("Escape", { tagName: "INPUT" }));
  assert.equal(stops, 1);
});

test("selection gestures use the registered track and allow stop controls during multi-select", context => {
  const { workspace } = fixture(context);
  const actions = createWorkspaceSelectionActions(workspace);
  const stopButton = { closest: () => ({}) };
  let blocked = 0;
  workspace.state.selMode = true;
  actions.guard({ target: { closest: () => stopButton }, preventDefault: () => blocked++ });
  assert.equal(blocked, 0);
  const track = { scrollLeft: 0, contains: target => target === track };
  workspace.dom.setCardsTrack(track);
  actions.wheel({ target: track, deltaY: 40, deltaX: 0, preventDefault: () => blocked++ });
  assert.equal(track.scrollLeft, 40);
  assert.equal(blocked, 1);
});

test("native view handlers stay stable across renders and never expose sample presets", () => {
  const calls = [];
  const workspace = new NativeWorkspace({ robotApi: {
    conn: "off",
    sample: false,
    robot: null,
    busy: () => false,
    run: async () => calls.push("run"),
    stop: async () => calls.push("stop"),
  } });
  workspace.showToast = () => {};
  const first = workspace.renderVals();
  const second = workspace.renderVals();
  for (const name of ["goHome", "goBox", "goPose0", "send", "readBack", "relAll", "relNew", "netTest"]) {
    assert.equal(first[name], second[name], name);
  }
  first.goHome();
  first.goBox();
  first.goPose0();
  assert.deepEqual(calls, []);
});
