import test from 'node:test';
import assert from 'node:assert/strict';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { createRequire } from 'node:module';

const frontend = new URL('../', import.meta.url);
const require = createRequire(new URL('package.json', frontend));
const { createServer } = await import(pathToFileURL(require.resolve('vite')).href);
globalThis.location = { search: '' };
globalThis.window = { setTimeout, clearTimeout, setInterval, clearInterval, addEventListener() {}, removeEventListener() {} };
globalThis.document = { querySelector() { return null; }, querySelectorAll() { return []; }, removeEventListener() {} };
const vite = await createServer({
  configFile: false,
  root: fileURLToPath(frontend),
  server: { middlewareMode: true, hmr: false },
  appType: 'custom',
});
let NativeWorkspace, nominalProfile, prepareWindowClose, registerWindowCloseGuard;
try {
  ({ NativeWorkspace } = await vite.ssrLoadModule('/src/modes/robot-control/workspace/NativeWorkspace.tsx'));
  ({ nominalProfile } = await vite.ssrLoadModule('/src/modes/robot-control/engineAdapter.ts'));
  ({ prepareWindowClose, registerWindowCloseGuard } = await vite.ssrLoadModule('/src/core/windowLifecycle.ts'));
} finally { await vite.close(); }

const deferred = () => {
  let resolve, reject;
  const promise = new Promise((yes, no) => { resolve = yes; reject = no; });
  return { promise, resolve, reject };
};
const flush = async () => { await Promise.resolve(); await Promise.resolve(); await Promise.resolve(); };
function fixture({ count = 2, connected = true, run = async () => ({}), stop = async () => {} } = {}) {
  const calls = [], messages = [], glides = [], intervals = [], timeouts = [];
  const robotApi = {
    conn: connected ? 'connected' : 'off', sample: false,
    robot: connected ? { name: 'Test arm', port: 'COM24', profile: nominalProfile() } : null,
    busy: () => false,
    run(action, data) { calls.push({ action, data }); return run(action, data); },
    stop(relock) { calls.push({ action: 'robot.stop', relock }); return stop(relock); },
  };
  const workspace = new NativeWorkspace({ robotApi, conn: connected ? 'connected' : 'off', bar: 'A', sample4c: 'none' });
  workspace.setState = patch => { workspace.state = { ...workspace.state, ...(typeof patch === 'function' ? patch(workspace.state) : patch) }; };
  workspace.showToast = message => messages.push(message);
  workspace.addLog = (_kind, message) => messages.push(message);
  workspace.iv = callback => { intervals.push(callback); return intervals.length; };
  workspace.tm = (callback, ms) => { timeouts.push({ callback, ms }); return timeouts.length; };
  workspace.clearAll = () => { intervals.length = 0; timeouts.length = 0; };
  workspace.glide = (values, times) => glides.push({ values, times });
  workspace.freezeGlide = () => {};
  workspace.endHold = () => {};
  workspace.endGroupRun = () => {};
  const poses = {}, seq = [];
  for (let i = 0; i < count; i++) {
    const key = `pose-${i}`; seq.push(key);
    poses[key] = { name: `Pose ${i}`, v: [i, 0, 0, 0, 0, 50], t: 1000, ts: [1000, 1000, 1000, 1000, 1000, 1000] };
  }
  workspace.setState({ poses, seq, sel: count ? 0 : -1, st: connected ? 'ready' : 'disconnected', loops: 1, cur: [0, 0, 0, 0, 0, 0], savedSnap: '', savedAt: '' });
  return { workspace, robotApi, calls, messages, glides, intervals, timeouts };
}

test('native mount and disconnected actions never start automatic motion', async () => {
  const f = fixture({ connected: false });
  f.workspace.afterMount();
  await f.workspace.playNative();
  await f.workspace.uploadNative();
  f.workspace.syncTo();
  await flush();
  assert.deepEqual(f.calls, []);
  assert.deepEqual(f.glides, []);
  assert.equal(f.workspace.state.linked, -1);
  assert.equal(f.workspace.state.synced, false);
  f.workspace.beforeUnmount();
});

test('upload covers all sequence rows in bounded chunks and never starts playback', async () => {
  const f = fixture({ count: 9 });
  f.workspace.setState({ rangeOn: true, rs: 1, re: 2 });
  await f.workspace.uploadNative();
  assert.deepEqual(f.calls.map(call => call.action), ['group.download', 'group.download', 'group.download']);
  assert.deepEqual(f.calls.map(call => call.data.groups.length), [4, 4, 1]);
  assert.deepEqual(f.calls.flatMap(call => call.data.groups.map(group => group.index)), [0, 1, 2, 3, 4, 5, 6, 7, 8]);
  assert.equal(f.workspace.state.uploadProgress, 9);
  assert.equal(f.workspace.state.robotSeq.n, 9);
  assert.equal(f.workspace.state.synced, false, 'OS writes cannot establish verified board contents');
  assert.equal(f.workspace.state.st, 'ready');
  assert.deepEqual(f.glides, []);
});

test('upload failure stops later chunks and never claims verified or complete board contents', async () => {
  let requests = 0;
  const f = fixture({ count: 9, run: async () => { if (++requests === 2) throw new Error('partial write'); return {}; } });
  await f.workspace.uploadNative();
  assert.equal(f.calls.length, 2);
  assert.equal(f.workspace.state.uploadProgress, 4);
  assert.equal(f.workspace.state.robotSeq, null);
  assert.equal(f.workspace.state.synced, false);
  assert.equal(f.workspace.state.st, 'ready');
  assert.ok(f.messages.some(message => message.includes('partial write')));
});

test('stop during a pending upload invalidates the remaining upload sequence', async () => {
  const pending = deferred();
  const f = fixture({ count: 9, run: () => pending.promise });
  const upload = f.workspace.uploadNative();
  assert.equal(f.calls.length, 1);
  await f.workspace.stopNative();
  pending.resolve({});
  await upload;
  assert.deepEqual(f.calls.map(call => call.action), ['group.download', 'robot.stop']);
  assert.equal(f.workspace.state.st, 'stopped');
  assert.equal(f.workspace.state.robotSeq, null);
  assert.equal(f.workspace.state.uploadProgress, 0);
});

test('late movement completion after stop cannot create playback timers or another pose request', async () => {
  const pending = deferred();
  const f = fixture({ run: () => pending.promise });
  const play = f.workspace.playNative();
  assert.equal(f.calls[0].action, 'group.execute');
  await f.workspace.stopNative();
  pending.resolve({});
  await play;
  await flush();
  assert.deepEqual(f.calls.map(call => call.action), ['group.execute', 'robot.stop']);
  assert.equal(f.intervals.length, 0);
  assert.equal(f.timeouts.length, 0);
  assert.deepEqual(f.glides, []);
  assert.equal(f.workspace.state.st, 'stopped');
  assert.equal(f.workspace.state.linked, -1);
});

test('late playback failure after stop cannot overwrite the stopped state', async () => {
  const pending = deferred();
  const f = fixture({ run: () => pending.promise });
  const play = f.workspace.playNative();
  await f.workspace.stopNative();
  pending.reject(new Error('cancelled old request'));
  await play;
  assert.equal(f.workspace.state.st, 'stopped');
  assert.ok(!f.messages.some(message => message.includes('cancelled old request')));
});

test('a playback timer already queued before stop cannot send the next pose afterward', async () => {
  const f = fixture();
  await f.workspace.playNative();
  assert.equal(f.intervals.length, 1);
  const queued = f.intervals[0];
  await f.workspace.stopNative();
  queued();
  await flush();
  assert.deepEqual(f.calls.map(call => call.action), ['group.execute', 'robot.stop']);
  assert.equal(f.workspace.state.st, 'stopped');
});

test('stop discards pending joint edits and late completion cannot flush the next servo', async () => {
  const pending = deferred();
  const f = fixture({ run: () => pending.promise });
  f.workspace.pending.set(0, { value: 5, key: 'pose-0' });
  f.workspace.pending.set(1, { value: 10, key: 'pose-0' });
  const flushJoints = f.workspace.flushJoints();
  assert.equal(f.calls[0].action, 'servo.set');
  await f.workspace.stopNative();
  pending.resolve({});
  await flushJoints;
  assert.deepEqual(f.calls.map(call => call.action), ['servo.set', 'robot.stop']);
  assert.equal(f.workspace.pending.size, 0);
  assert.deepEqual(f.workspace.state.cur, [0, 0, 0, 0, 0, 0]);
  assert.equal(f.workspace.state.st, 'stopped');
});

test('native slider edits honor the pose common duration instead of a hardcoded jog duration', async () => {
  const f = fixture();
  f.workspace.state.poses['pose-0'].t = 3000;
  f.workspace.state.poses['pose-0'].ts = [1000, 2000, 4000, 5000, 6000, 7000];
  f.workspace.setState({ timeMode: 'same', linked: 0 });
  f.workspace.setAbs(1, 30);
  clearTimeout(f.workspace.jointTimer);
  await f.workspace.flushJoints();
  assert.equal(f.calls.length, 1);
  assert.equal(f.calls[0].action, 'servo.set');
  assert.equal(f.calls[0].data.id, 1);
  assert.equal(f.calls[0].data.time_ms, 3000);
  assert.equal(f.workspace.state.poses['pose-0'].v[1], 30);
});

test('native slider edits honor the selected joint split duration and original pose key', async () => {
  const f = fixture();
  f.workspace.state.poses['pose-0'].t = 3000;
  f.workspace.state.poses['pose-0'].ts = [1000, 2000, 7000, 4000, 5000, 6000];
  f.workspace.setState({ timeMode: 'split', linked: 0 });
  f.workspace.setAbs(2, 45);
  clearTimeout(f.workspace.jointTimer);
  f.workspace.setState({ sel: 1 });
  await f.workspace.flushJoints();
  assert.equal(f.calls.length, 1);
  assert.equal(f.calls[0].action, 'servo.set');
  assert.equal(f.calls[0].data.id, 2);
  assert.equal(f.calls[0].data.time_ms, 7000, 'queued edit uses its own pose duration after selection changes');
  assert.equal(f.workspace.state.poses['pose-0'].v[2], 45);
});

test('workspace exit stops a long slider movement even after its transport write resolved', async () => {
  const f = fixture();
  f.workspace.state.poses['pose-0'].ts = [1000, 2000, 7000, 4000, 5000, 6000];
  f.workspace.setState({ timeMode: 'split', linked: 0 });
  f.workspace.setAbs(2, 45);
  clearTimeout(f.workspace.jointTimer);
  await f.workspace.flushJoints();
  assert.equal(f.calls[0].data.time_ms, 7000);
  assert.equal(f.workspace.state.st, 'ready');
  assert.equal(f.workspace.sending, false);
  assert.equal(f.workspace.pending.size, 0);
  f.workspace.beforeUnmount();
  await flush();
  assert.deepEqual(f.calls.map(call => call.action), ['servo.set', 'robot.stop']);
  assert.equal(f.workspace._dead, true);
});

test('exiting an untouched disconnected workspace does not send a hardware stop', async () => {
  const f = fixture({ connected: false });
  f.workspace.beforeUnmount();
  await flush();
  assert.deepEqual(f.calls, []);
  assert.equal(f.workspace._dead, true);
});

test('stale callbacks cannot start upload, move or sensor read during active playback', async () => {
  const f = fixture();
  f.workspace.setState({ st: 'playing', steps: [0, 1], pos: 0 });
  await f.workspace.uploadNative();
  await f.workspace.readNative([0]);
  await f.workspace.move([0, 0, 0, 0, 0, 50], [3000, 3000, 3000, 3000, 3000, 3000], 'stale callback', {});
  await f.workspace.torque(false);
  assert.deepEqual(f.calls, []);
  assert.equal(f.workspace.state.st, 'playing');
});

test('disconnect while a movement is pending invalidates completion and prevents next motion', async () => {
  const pending = deferred();
  const f = fixture({ run: () => pending.promise });
  const play = f.workspace.playNative();
  f.robotApi.conn = 'lost';
  f.workspace.applyConn('lost');
  pending.resolve({});
  await play;
  assert.equal(f.workspace.state.st, 'disconnected');
  assert.equal(f.calls.length, 1);
  assert.deepEqual(f.glides, []);
  assert.equal(f.intervals.length, 0);
});

test('unmount sends stop for active movement and discards its later completion', async () => {
  const pending = deferred();
  const f = fixture({ run: () => pending.promise });
  const play = f.workspace.playNative();
  f.workspace.beforeUnmount();
  pending.resolve({});
  await play;
  assert.equal(f.workspace._dead, true);
  assert.deepEqual(f.calls.map(call => call.action), ['group.execute', 'robot.stop']);
  assert.deepEqual(f.glides, []);
  assert.equal(f.intervals.length, 0);
});

test('torque release requires an explicit action and does not run simulated drift or guess sensor values', async () => {
  const f = fixture();
  const original = [...f.workspace.state.cur];
  await f.workspace.torque(false);
  assert.deepEqual(f.calls, [{ action: 'servo.torque', data: { enabled: false } }]);
  assert.equal(f.workspace.state.release, true);
  assert.deepEqual(f.workspace.state.cur, original);
  assert.equal(f.intervals.length, 0);
  assert.equal(f.timeouts.length, 0);
  await f.workspace.torque(true);
  assert.deepEqual(f.calls[1], { action: 'servo.torque', data: { enabled: true } });
  assert.equal(f.workspace.state.release, false);
  assert.deepEqual(f.workspace.state.cur, original);
});

test('disconnected rendering is safe and guessed home, box and pose-zero actions never move hardware', async () => {
  const f = fixture({ connected: false });
  const view = f.workspace.renderVals();
  view.goHome(); view.goBox(); view.goPose0();
  await flush();
  assert.deepEqual(f.calls, []);
  assert.equal(view.nativeConnected, false);
  assert.equal(f.workspace.state.linked, -1);
});

test('pause controls cannot silently pause only the UI while hardware continues', async () => {
  const f = fixture();
  f.workspace.setState({ st: 'playing', steps: [0, 1], pos: 0 });
  const view = f.workspace.renderVals();
  view.playSlot(); view.playDown(); view.playUp();
  await flush();
  assert.equal(f.workspace.state.st, 'playing');
  assert.deepEqual(f.calls, []);
  assert.ok(f.messages.some(message => message.includes('ไม่รองรับ')));
});

test('position reads use measured response IDs and commit all joints atomically', async () => {
  let reads = 0;
  const last = deferred();
  const f = fixture({ run: async (action, data) => {
    assert.equal(action, 'servo.read');
    ++reads;
    if (reads === 6) return last.promise;
    return { exchange: { received: `#${String(data.id).padStart(3, '0')}P1500!` } };
  } });
  const before = structuredClone(f.workspace.state.poses);
  const reading = f.workspace.readNative([0, 1, 2, 3, 4, 5]);
  for (let i = 0; i < 12; i++) await flush();
  assert.equal(reads, 6);
  assert.deepEqual(f.workspace.state.poses, before, 'partial sensor responses do not modify the saved pose');
  last.resolve({ exchange: { received: '#005PRAD=2500!' } });
  await reading;
  assert.deepEqual(f.calls.map(call => call.action), Array(6).fill('servo.read'));
  assert.deepEqual(f.calls.map(call => call.data.id), [0, 1, 2, 3, 4, 5]);
  assert.deepEqual(f.workspace.state.poses['pose-0'].v, [0, 0, 0, 0, 0, 100]);
  assert.deepEqual(f.workspace.state.cur, [0, 0, 0, 0, 0, 100]);
  assert.equal(f.workspace.state.reading, false);
  assert.deepEqual(f.glides, []);
});

test('missing sensor data rejects the entire read without saving guessed or partial positions', async () => {
  const f = fixture({ run: async (_action, data) => ({ exchange: { received: data.id === 1 ? '' : '#000P1600!' } }) });
  const poses = structuredClone(f.workspace.state.poses), cur = [...f.workspace.state.cur];
  await f.workspace.readNative([0, 1, 2, 3, 4, 5]);
  assert.equal(f.calls.length, 2);
  assert.deepEqual(f.workspace.state.poses, poses);
  assert.deepEqual(f.workspace.state.cur, cur);
  assert.equal(f.workspace.state.reading, false);
  assert.equal(f.workspace.state.linked, -1);
});

test('stop during a sensor read discards late sensor results and clears the reading lock', async () => {
  const pending = deferred();
  const f = fixture({ run: () => pending.promise });
  const poses = structuredClone(f.workspace.state.poses);
  const reading = f.workspace.readNative([0, 1, 2, 3, 4, 5]);
  assert.equal(f.workspace.state.reading, true);
  await f.workspace.stopNative();
  pending.resolve({ exchange: { received: '#000P1600!' } });
  await reading;
  assert.deepEqual(f.workspace.state.poses, poses);
  assert.equal(f.workspace.state.st, 'stopped');
  assert.equal(f.workspace.state.reading, false);
  assert.deepEqual(f.calls.map(call => call.action), ['servo.read', 'robot.stop']);
});

test('window close waits for every asynchronous participant before allowing native teardown', async () => {
  const one = deferred(), two = deferred();
  const events = [];
  const removeOne = registerWindowCloseGuard(async () => { events.push('invalidate-one'); await one.promise; events.push('stop-one'); });
  const removeTwo = registerWindowCloseGuard(async () => { events.push('invalidate-two'); await two.promise; events.push('stop-two'); });
  try {
    let ready = false;
    const closing = prepareWindowClose().then(() => { ready = true; });
    await flush();
    assert.deepEqual(events, ['invalidate-one', 'invalidate-two']);
    assert.equal(ready, false);
    one.resolve(); await flush();
    assert.equal(ready, false, 'the second physical stop is still pending');
    two.resolve(); await closing;
    assert.equal(ready, true);
    assert.deepEqual(events, ['invalidate-one', 'invalidate-two', 'stop-one', 'stop-two']);
  } finally { removeOne(); removeTwo(); }
});

test('a failed close guard blocks closing but still waits for the other physical stop to finish', async () => {
  const pendingStop = deferred();
  const error = new Error('device stop failed');
  const removeFailed = registerWindowCloseGuard(() => { throw error; });
  let stopped = false;
  const removePending = registerWindowCloseGuard(async () => { await pendingStop.promise; stopped = true; });
  try {
    let settled = false;
    const closing = prepareWindowClose().finally(() => { settled = true; });
    const rejected = assert.rejects(closing, candidate => candidate === error);
    await flush();
    assert.equal(settled, false);
    pendingStop.resolve(); await rejected;
    assert.equal(stopped, true);
    assert.equal(settled, true);
  } finally { removeFailed(); removePending(); }
});

test('native window-close guard invalidates pending playback and waits for physical stop', async () => {
  const movement = deferred(), physicalStop = deferred();
  const f = fixture({ run: () => movement.promise, stop: () => physicalStop.promise });
  f.workspace.afterMount();
  try {
    const playing = f.workspace.playNative();
    let prepared = false;
    const closing = prepareWindowClose().then(() => { prepared = true; });
    await flush();
    assert.deepEqual(f.calls.map(call => call.action), ['group.execute', 'robot.stop']);
    assert.equal(prepared, false);
    movement.resolve({}); await playing;
    assert.deepEqual(f.glides, []);
    assert.equal(f.intervals.length, 0);
    assert.equal(prepared, false);
    physicalStop.resolve(); await closing;
    assert.equal(prepared, true);
  } finally { f.workspace.removeCloseGuard?.(); }
});

test('a released workspace close requests relock and propagates physical-stop failure', async () => {
  const f = fixture({ stop: async () => { throw new Error('cannot stop selected robot'); } });
  f.workspace.setState({ release: true });
  f.workspace.afterMount();
  try {
    await assert.rejects(prepareWindowClose(), /cannot stop selected robot/);
    assert.deepEqual(f.calls, [{ action: 'robot.stop', relock: true }]);
  } finally { f.workspace.removeCloseGuard?.(); }
});

test('unmount unregisters its window-close guard so future close does not use an obsolete robot target', async () => {
  const f = fixture();
  f.workspace.afterMount();
  f.workspace.beforeUnmount();
  await flush();
  assert.equal(f.calls.length, 1, 'unmount performs its own physical stop');
  await prepareWindowClose();
  assert.equal(f.calls.length, 1, 'the removed guard cannot stop an old target again');
});
