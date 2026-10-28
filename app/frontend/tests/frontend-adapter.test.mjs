import test from 'node:test';
import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import { stripTypeScriptTypes } from 'node:module';

const source = await readFile(new URL('../src/modes/robot-control/engineAdapter.ts', import.meta.url), 'utf8');
const javascript = stripTypeScriptTypes(source, { mode: 'transform', sourceMap: false });
const { nominalProfile, validateProfile, servoItem, poseGroup, parseServoPosition, ZideTransport } = await import(
  `data:text/javascript;base64,${Buffer.from(javascript).toString('base64')}`
);
const device = { transport: 'serial', path: 'COM24', name: 'Test only' };
const hid = { transport: 'hid', path: 'fake-hid-device', name: 'Test only HID' };
const success = (data = {}) => ({ status: 'success', data });
const coded = (code, message) => Object.assign(new Error(`${code}: ${message}`), { code });
const deferred = () => {
  let resolve, reject;
  const promise = new Promise((yes, no) => { resolve = yes; reject = no; });
  return { promise, resolve, reject };
};
const flush = async () => { await Promise.resolve(); await Promise.resolve(); await Promise.resolve(); };
function fixture({ sample = false, run = async () => success(), cancel = async () => {} } = {}) {
  const calls = [], failures = [];
  const bridge = {
    run(mode, action, data) { calls.push({ mode, action, data }); return run(mode, action, data); },
    cancel() { calls.push({ action: 'cancel' }); return cancel(); },
  };
  return { calls, failures, transport: new ZideTransport(bridge, sample, error => failures.push(error)) };
}

test('calibrated six-joint profile maps explicit IDs, endpoints, reversal, timing and rounding', () => {
  const profile = nominalProfile();
  profile.servos.forEach((servo, index) => { servo.id = 10 + index; });
  profile.servos[1] = { id: 11, pwmMin: 2400, pwmMax: 600 };
  const calibrated = validateProfile(profile);
  assert.deepEqual(servoItem(calibrated, 0, -180, 0), { id: 10, pwm: 500, time_ms: 0 });
  assert.deepEqual(servoItem(calibrated, 0, 180, 9999), { id: 10, pwm: 2500, time_ms: 9999 });
  assert.deepEqual(servoItem(calibrated, 1, -135, 3000), { id: 11, pwm: 2400, time_ms: 3000 });
  assert.deepEqual(servoItem(calibrated, 1, 135, 3000), { id: 11, pwm: 600, time_ms: 3000 });
  assert.deepEqual(servoItem(calibrated, 1, 0, 3000), { id: 11, pwm: 1500, time_ms: 3000 });
  assert.equal(servoItem(calibrated, 5, 0, 1000).pwm, 500);
  assert.equal(servoItem(calibrated, 5, 100, 1000).pwm, 2500);
  assert.equal(servoItem(calibrated, 0, 1, 1000).pwm, 1506);
  const group = poseGroup(calibrated, [0, 0, 0, 0, 0, 50], [1000, 2000, 3000, 4000, 5000, 6000], 37);
  assert.equal(group.index, 37);
  assert.deepEqual(group.items.map(item => item.id), [10, 11, 12, 13, 14, 15]);
  assert.deepEqual(group.items.map(item => item.time_ms), [1000, 2000, 3000, 4000, 5000, 6000]);
  assert.deepEqual(group.items.map(item => item.pwm), [1500, 1500, 1500, 1500, 1500, 1500]);
  profile.servos[0].id = 99;
  assert.equal(calibrated.servos[0].id, 10);
});

test('invalid profiles and invalid pose values fail before any command can be constructed', () => {
  for (const mutate of [
    p => { p.name = ''; }, p => { p.servos.pop(); }, p => { p.servos[1].id = 0; },
    p => { p.servos[0].id = -1; }, p => { p.servos[0].id = 255; },
    p => { p.servos[0].id = 1.5; }, p => { p.servos[0].pwmMin = 499; },
    p => { p.servos[0].pwmMax = 2501; }, p => { p.servos[0].pwmMax = p.servos[0].pwmMin; },
    p => { p.servos[0].pwmMin = NaN; }, p => { p.servos[0].pwmMax = '2500'; },
  ]) {
    const profile = nominalProfile(); mutate(profile);
    assert.throws(() => validateProfile(profile));
  }
  for (const invalid of [null, undefined, {}, { name: 'Arm', servos: null }]) assert.throws(() => validateProfile(invalid));
  const profile = validateProfile(nominalProfile());
  for (const [joint, value, time] of [[-1, 0, 1000], [6, 0, 1000], [0, 181, 1000], [1, -136, 1000], [5, 101, 1000], [0, NaN, 1000], [0, 0, -1], [0, 0, 10000], [0, 0, 1000.5]])
    assert.throws(() => servoItem(profile, joint, value, time));
  assert.throws(() => poseGroup(profile, [0], [1000], 0));
  assert.throws(() => poseGroup(profile, Array(6), Array(6).fill(1000), 0));
  assert.throws(() => poseGroup(profile, [0, 0, 0, 0, 0, 0], [1000, 1000, 1000, 1000, 1000, 1000], 10000));
});

test('sensor parsing requires the selected servo ID and valid calibrated PWM, including reverse endpoints', () => {
  const profile = nominalProfile();
  profile.servos[1] = { id: 17, pwmMin: 2400, pwmMax: 600 };
  const reply = received => ({ exchange: { received } });
  assert.equal(parseServoPosition(profile, 1, reply('#017P2400!')), -135);
  assert.equal(parseServoPosition(profile, 1, reply('#017PRAD=600!')), 135);
  assert.equal(parseServoPosition(profile, 1, reply('noise\r\n#001P1700!#017PRAD=1500!\r\n')), 0);
  assert.equal(parseServoPosition(profile, 1, reply('#017P2400!#017P1500!')), 0);
  assert.throws(() => parseServoPosition(profile, 1, reply('#001P1500!')));
  assert.throws(() => parseServoPosition(profile, 1, reply('#017PRAD!')));
  assert.throws(() => parseServoPosition(profile, 1, reply('#017P2500!')));
  assert.throws(() => parseServoPosition(profile, 1, reply('')));
  assert.throws(() => parseServoPosition(profile, 1, {}));
});

test('sample mode never reaches the native bridge, including discovery and stop', async () => {
  const { transport, calls } = fixture({ sample: true });
  await assert.rejects(transport.discover());
  await assert.rejects(transport.probe(device));
  await assert.rejects(transport.command(device, 'servo.set', { id: 0, pwm: 1500 }));
  await assert.rejects(transport.stop(device));
  assert.deepEqual(calls, []);
  assert.equal(transport.idle, true);
});

test('probe only reads board information and requires a real-device response', async () => {
  const real = fixture({ run: async () => success({ simulated: false }) });
  await real.transport.probe(device);
  assert.deepEqual(real.calls, [{ mode: 'robot-control', action: 'board.info', data: { transport: 'serial', port: 'COM24', baud: 115200, read_ms: 1000 } }]);
  const simulated = fixture({ run: async () => success({ simulated: true }) });
  await assert.rejects(simulated.transport.probe(hid));
  assert.equal(simulated.calls[0].action, 'board.info');
  assert.equal(simulated.calls[0].data.device, hid.path);
  assert.equal(simulated.calls[0].data.transport, 'hid');
  assert.equal('port' in simulated.calls[0].data, false);
});

test('discovery filters malformed devices without moving any servos', async () => {
  const f = fixture({ run: async () => success({ devices: [device, hid, null, { transport: 'wifi', path: 'x' }, { transport: 'serial', path: '' }] }) });
  assert.deepEqual(await f.transport.discover(), [device, hid]);
  assert.deepEqual(f.calls.map(call => call.action), ['device.list']);
});

test('busy commands are rejected, not retained for late unexpected motion', async () => {
  const first = deferred();
  const f = fixture({ run: () => first.promise });
  const ongoing = f.transport.command(device, 'servo.set', { id: 0, pwm: 1500 });
  assert.equal(f.transport.idle, false);
  await assert.rejects(f.transport.command(device, 'servo.set', { id: 0, pwm: 1600 }));
  assert.equal(f.calls.length, 1);
  first.resolve(success({ delivery: 'written' }));
  await ongoing;
  await flush();
  assert.equal(f.transport.idle, true);
  assert.equal(f.calls.length, 1, 'rejected movement never replays');
  assert.equal(f.failures.length, 0);
});

test('stop waits for cancellation and active request drain, then sends exactly one physical stop', async () => {
  const movement = deferred(), cancellation = deferred(), physicalStop = deferred();
  const f = fixture({ cancel: () => cancellation.promise, run: (_mode, action) => action === 'robot.stop' ? physicalStop.promise : movement.promise });
  const ongoing = f.transport.command(device, 'group.execute', { group: { index: 0, items: [] } });
  const moved = assert.rejects(ongoing);
  const stopped = f.transport.stop(device);
  assert.strictEqual(f.transport.stop(device), stopped, 'parallel stop clicks share one task');
  assert.deepEqual(f.calls.map(call => call.action), ['group.execute', 'cancel']);
  await assert.rejects(f.transport.command(device, 'servo.set', { id: 0, pwm: 1600 }));
  cancellation.resolve();
  await flush();
  assert.equal(f.calls.length, 2, 'stop cannot acquire serial/HID before cancelled child drains');
  movement.reject(coded('ENGINE_CANCELLED', 'engine request was cancelled'));
  await moved;
  await flush();
  assert.deepEqual(f.calls.map(call => call.action), ['group.execute', 'cancel', 'robot.stop']);
  assert.deepEqual(f.calls[2].data, { transport: 'serial', port: 'COM24', baud: 115200, read_ms: 1 });
  assert.equal(f.transport.idle, false);
  physicalStop.resolve(success({ delivery: 'written', acknowledged: false }));
  await stopped;
  assert.equal(f.transport.idle, true);
  assert.equal(f.failures.length, 0, 'intentional cancellation is not a connection failure');
});

test('late successful command completion does not suppress the explicit stop', async () => {
  const movement = deferred();
  const f = fixture({ run: (_mode, action) => action === 'robot.stop' ? Promise.resolve(success()) : movement.promise });
  const ongoing = f.transport.command(hid, 'servo.set', { id: 0, pwm: 1500 });
  const stopped = f.transport.stop(hid);
  await flush();
  assert.deepEqual(f.calls.map(call => call.action), ['servo.set', 'cancel']);
  movement.resolve(success({ delivery: 'written' }));
  await ongoing;
  await stopped;
  assert.deepEqual(f.calls.map(call => call.action), ['servo.set', 'cancel', 'robot.stop']);
  assert.equal(f.calls[2].data.device, hid.path);
});

test('hardware errors clear active state and are reported without automatic retry', async () => {
  const f = fixture({ run: async () => { throw coded('TRANSPORT_ERROR', 'partial write'); } });
  await assert.rejects(f.transport.command(device, 'group.download', { groups: [] }), /partial write/);
  assert.equal(f.transport.idle, true);
  assert.equal(f.failures.length, 1);
  assert.deepEqual(f.calls.map(call => call.action), ['group.download']);
});

test('busy and cancelled requests are interruptions, every other code is a lost connection', async () => {
  for (const [code, lost] of [['ENGINE_BUSY', false], ['ENGINE_CANCELLED', false], ['ENGINE_TIMEOUT', true],
    ['ENGINE_FAILED', true], ['TRANSPORT_ERROR', true], ['DEVICE_TIMEOUT', true], ['SAFETY_LIMIT', true]]) {
    const f = fixture({ run: async () => { throw coded(code, 'diagnostic'); } });
    await assert.rejects(f.transport.command(device, 'servo.set', { id: 0, pwm: 1500 }));
    assert.equal(f.failures.length, lost ? 1 : 0, code);
    assert.equal(f.transport.idle, true, code);
  }
});

test('the decision follows the error code, not the message text', async () => {
  const text = fixture({ run: async () => { throw new Error('ENGINE_BUSY: engine is busy'); } });
  await assert.rejects(text.transport.command(device, 'servo.set', { id: 0, pwm: 1500 }));
  assert.equal(text.failures.length, 1, 'busy-looking text without a code is a failure');
  const thai = fixture({ run: async () => { throw coded('ENGINE_FAILED', 'หุ่นกำลังรับคำสั่ง'); } });
  await assert.rejects(thai.transport.command(device, 'servo.set', { id: 0, pwm: 1500 }));
  assert.equal(thai.failures.length, 1, 'Thai wording cannot hide a failed engine');
  const english = fixture({ run: async () => { throw coded('ENGINE_CANCELLED', 'anything at all'); } });
  await assert.rejects(english.transport.command(device, 'servo.set', { id: 0, pwm: 1500 }));
  assert.equal(english.failures.length, 0);
});

test('the transport busy guard carries ENGINE_BUSY and never reports a lost connection', async () => {
  const first = deferred();
  const f = fixture({ run: () => first.promise });
  const ongoing = f.transport.command(device, 'servo.set', { id: 0, pwm: 1500 });
  await assert.rejects(f.transport.command(device, 'servo.set', { id: 0, pwm: 1600 }), error => error.code === 'ENGINE_BUSY');
  first.resolve(success());
  await ongoing;
  assert.equal(f.failures.length, 0);
});

test('position reads wait for device responses while writes use the short transport window', async () => {
  const f = fixture();
  await f.transport.command(device, 'servo.read', { id: 0 });
  await f.transport.command(device, 'group.read', { start: 0, end: 1 });
  await f.transport.command(device, 'servo.set', { id: 0, pwm: 1500 });
  assert.deepEqual(f.calls.map(call => call.data.read_ms), [1000, 1000, 1]);
});

test('explicit relock follows physical stop on the captured target without movement commands', async () => {
  const f = fixture();
  await f.transport.stop(hid, true);
  assert.deepEqual(f.calls.map(call => call.action), ['cancel', 'robot.stop', 'servo.torque']);
  assert.deepEqual(f.calls[2].data, { transport: 'hid', device: hid.path, enabled: true, read_ms: 1 });
});

test('a relock requested after cleanup started is preserved by the shared stop operation', async () => {
  const physicalStop = deferred();
  const f = fixture({ run: (_mode, action) => action === 'robot.stop' ? physicalStop.promise : Promise.resolve(success()) });
  const cleanup = f.transport.stop(hid, false);
  await flush();
  assert.deepEqual(f.calls.map(call => call.action), ['cancel', 'robot.stop']);
  const requested = f.transport.stop(hid, true);
  assert.strictEqual(requested, cleanup);
  physicalStop.resolve(success());
  await requested;
  assert.deepEqual(f.calls.map(call => call.action), ['cancel', 'robot.stop', 'servo.torque']);
  assert.equal(f.calls[2].data.enabled, true);
});

test('cancellation failure prevents additional writes and remains an explicit error', async () => {
  const f = fixture({ cancel: async () => { throw new Error('cancel unavailable'); } });
  await assert.rejects(f.transport.stop(device), /cancel unavailable/);
  assert.deepEqual(f.calls.map(call => call.action), ['cancel']);
  assert.equal(f.failures.length, 1);
  assert.equal(f.transport.idle, true);
});
