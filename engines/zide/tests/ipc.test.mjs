import { test } from 'node:test';
import assert from 'node:assert/strict';
import { spawnSync } from 'node:child_process';
import { fileURLToPath } from 'node:url';
import { loadSchema, violations } from './schema-check.mjs';

const platform = process.platform === 'win32' ? 'windows' : process.platform;
const arch = process.arch === 'x64' ? 'amd64' : process.arch;
const binary = process.env.ZIDE_ENGINE ?? fileURLToPath(new URL(`../bin/${platform}-${arch}/zide-engine${process.platform === 'win32' ? '.exe' : ''}`, import.meta.url));
const responseSchema = loadSchema(new URL('../../../contracts/engine/response.schema.json', import.meta.url));
const request = (action, data = {}, extra = {}) => ({ protocol: '1.0', request_id: 'ipc-test', mode: 'robot-control', action, data, ...extra });
const MAX_LINE = 65536;

function spawnEngine(input) {
  const child = spawnSync(binary, [], { input, encoding: 'utf8', timeout: 10000, maxBuffer: 2 ** 20, windowsHide: true });
  assert.ifError(child.error);
  assert.equal(child.status, 0, child.stderr);
  assert.equal(child.stderr, '');
  return child.stdout.trim().split(/\r?\n/).map(line => JSON.parse(line));
}
// Every response any test receives must satisfy the shared contract, not only the ones a test inspects.
function run(lines, { terminated = true } = {}) {
  const responses = spawnEngine(lines.join('\n') + (terminated ? '\n' : ''));
  for (const response of responses) assert.deepEqual(violations(responseSchema, response), [], JSON.stringify(response));
  return responses;
}
const invoke = (action, data = {}, extra = {}) => run([JSON.stringify(request(action, data, extra))])[0];
const withPadding = (object, totalBytes) => {
  const json = JSON.stringify(object);
  return json + ' '.repeat(totalBytes - Buffer.byteLength(json));
};

test('executable identifies C++ engine and supported actions', () => {
  const response = invoke('engine.info');
  assert.equal(response.status, 'success');
  assert.equal(response.data.language, 'C++');
  assert.equal(response.data.simulated, false);
  assert.equal(response.data.transport, "none");
  assert.ok(response.data.actions.includes('group.download'));
  assert.ok(!response.data.actions.includes('command.send'));
});

test('all standalone actions compile without hardware by default', () => {
  const group = { index: 2, items: [{ id: 0, pwm: 1500, time_ms: 1000 }] };
  const cases = [
    ['servo.set', { id: 0, pwm: 1500, time_ms: 1000 }, '#000P1500T1000!'],
    ['servo.set', { servo: 1, angle: 90 }, '#001P1500T1000!'],
    ['servo.read', { id: 4 }, '#004PRAD!'],
    ['servo.torque', { enabled: false }, '#255PULK!'],
    ['servo.torque', { enabled: true, id: 0 }, '#000PSCK!'],
    ['robot.stop', {}, '$DST!'],
    ['board.info', {}, '$DRS!'],
    ['group.execute', { group }, '{G0002#000P1500T1000!}'],
    ['group.download', { groups: [group] }, '<G0002#000P1500T1000!K0000N0000B000!>'],
    ['group.run', { start: 0, end: 2, times: 1 }, '$DGT:0-2,1!'],
    ['group.read', { start: 0, end: 2 }, '$PTG:0-2!'],
  ];
  for (const [action, data, command] of cases) {
    const response = invoke(action, data);
    assert.equal(response.status, 'success', JSON.stringify(response));
    assert.equal(response.request_id, 'ipc-test');
    assert.equal(response.data.simulated, true);
    assert.equal(response.data.delivery, 'dry-run');
    assert.deepEqual(response.data.commands, [command]);
    assert.equal(response.data.hid_reports[0].length, 64);
  }
});

test('dry-run stop reports a request, not a confirmed physical stop', () => {
  const { data } = invoke('robot.stop');
  assert.equal(data.stop_requested, true);
  assert.equal('stopped' in data, false);
});

test('project INI roundtrip uses groups without device I/O', () => {
  const groups = [{ index: 0, items: [{ id: 1, pwm: 1500, time_ms: 1000 }] }];
  const exported = invoke('project.export', { groups });
  assert.equal(exported.status, 'success');
  const imported = invoke('project.import', { content: exported.data.content });
  assert.equal(imported.status, 'success');
  assert.equal(imported.data.groups[0].items[0].pwm, 1500);
});

test('rejected requests carry the documented error code and no payload', () => {
  const cases = [
    ['servo.set', {}, {}, 'INVALID_INPUT'],
    ['servo.set', { id: 0, pwm: 2501 }, {}, 'INVALID_INPUT'],
    ['servo.set', { id: 0, pwm: 2500, bias: 1 }, {}, 'SAFETY_LIMIT'],
    ['servo.set', { id: 0, pwm: 500, bias: -1 }, {}, 'SAFETY_LIMIT'],
    ['servo.set', { id: 0.1, pwm: 1500 }, {}, 'INVALID_INPUT'],
    ['servo.set', { id: 0, pwm: '1500' }, {}, 'INVALID_INPUT'],
    ['servo.set', { servo: 1 }, {}, 'INVALID_INPUT'],
    ['servo.set', { servo: 1, angle: 90, pwm: 1500 }, {}, 'INVALID_INPUT'],
    ['servo.read', { id: 255 }, {}, 'INVALID_INPUT'],
    ['robot.stop', { transport: 'tcp' }, {}, 'INVALID_INPUT'],
    ['robot.stop', { transport: 5 }, {}, 'INVALID_INPUT'],
    ['robot.stop', { read_ms: 2000 }, {}, 'INVALID_INPUT'],
    ['robot.stop', { transport: 'serial' }, {}, 'INVALID_INPUT'],
    ['robot.stop', { arbitrary: 'unknown' }, {}, 'INVALID_INPUT'],
    ['command.send', { command: '$DRS!' }, {}, 'INVALID_INPUT'],
    ['robot.stop', { command: '$RST!' }, {}, 'INVALID_INPUT'],
    ['group.execute', { groups: [{ index: 0, items: [] }] }, {}, 'INVALID_INPUT'],
    ['group.execute', { group: { index: 0, items: [{ id: 0, pwm: 2500, bias: 1, time_ms: 0 }] } }, {}, 'SAFETY_LIMIT'],
    ['group.run', { start: 3, end: 1, times: 1 }, {}, 'INVALID_INPUT'],
    ['group.run', { start: 0, end: 1, times: 0 }, {}, 'INVALID_INPUT'],
    ['project.import', {}, {}, 'INVALID_INPUT'],
    ['project.import', { content: 5 }, {}, 'INVALID_INPUT'],
    ['project.import', { content: 'not an ini project' }, {}, 'PROJECT_INVALID'],
    ['project.export', {}, {}, 'INVALID_INPUT'],
    ['project.export', { groups: [] }, {}, 'PROJECT_INVALID'],
    ['project.export', { groups: [5] }, {}, 'PROJECT_INVALID'],
    ['robot.stop', {}, { protocol: '2.0' }, 'PROTOCOL_INVALID'],
    ['robot.stop', {}, { protocol: 1 }, 'PROTOCOL_INVALID'],
    ['robot.stop', {}, { mode: 'coding-adventure' }, 'PROTOCOL_INVALID'],
    ['robot.stop', {}, { request_id: '' }, 'PROTOCOL_INVALID'],
    ['robot.stop', {}, { extra: true }, 'PROTOCOL_INVALID'],
    ['robot.stop', null, {}, 'INVALID_INPUT'],
    ['unknown.action', {}, {}, 'ACTION_UNKNOWN'],
  ];
  for (const [action, data, extra, code] of cases) {
    const response = invoke(action, data, extra);
    const label = JSON.stringify({ action, data, extra });
    assert.equal(response.status, 'error', label);
    assert.equal(response.error.code, code, `${label} → ${response.error.message}`);
    assert.doesNotMatch(response.error.message, /json\.exception/, label);
    assert.equal(response.data, undefined, label);
  }
  const overflow = JSON.stringify(request('servo.set', { id: 0, pwm: 1500 })).replace('"pwm":1500', '"pwm":1500,"bias":18446744073709551615');
  assert.equal(run([overflow])[0].error.code, 'INVALID_INPUT');
});

test('non-object requests answer INVALID_REQUEST or PROTOCOL_INVALID with an empty id', () => {
  const [syntax, nothing, array] = run(['{', 'null', '[]']);
  assert.equal(syntax.error.code, 'INVALID_REQUEST');
  assert.equal(nothing.error.code, 'PROTOCOL_INVALID');
  assert.equal(array.error.code, 'PROTOCOL_INVALID');
  for (const response of [syntax, nothing, array]) assert.equal(response.request_id, '');
});

test('line protocol recovers after malformed input and matches request ids', () => {
  const responses = run(['{', JSON.stringify(request('robot.stop', {}, { request_id: 'next' })), 'null']);
  assert.equal(responses.length, 3);
  assert.equal(responses[0].error.code, 'INVALID_REQUEST');
  assert.equal(responses[1].request_id, 'next');
  assert.equal(responses[1].status, 'success');
  assert.equal(responses[2].status, 'error');
});

test('request_id is echoed up to 128 bytes and reflected as empty beyond that', () => {
  const accepted = invoke('robot.stop', {}, { request_id: 'a'.repeat(128) });
  assert.equal(accepted.status, 'success');
  assert.equal(accepted.request_id, 'a'.repeat(128));
  for (const id of ['a'.repeat(129), 'a'.repeat(200), 7, null, ['x']]) {
    const rejected = invoke('robot.stop', {}, { request_id: id });
    assert.equal(rejected.status, 'error');
    assert.equal(rejected.error.code, 'PROTOCOL_INVALID');
    assert.equal(rejected.request_id, '');
  }
  const wrongMode = invoke('robot.stop', {}, { request_id: 'b'.repeat(129), mode: 'x' });
  assert.equal(wrongMode.request_id, '');
});

test('a line of 65536 bytes is served, 65537 is rejected and the session continues', () => {
  const next = JSON.stringify(request('robot.stop', {}, { request_id: 'after' }));
  const exact = withPadding(request('robot.stop', {}, { request_id: 'exact' }), MAX_LINE);
  assert.equal(Buffer.byteLength(exact), MAX_LINE);
  const [served, following] = run([exact, next]);
  assert.equal(served.status, 'success');
  assert.equal(served.request_id, 'exact');
  assert.equal(following.request_id, 'after');

  // A complete valid request inside the oversize line must not be executed from the discarded tail.
  const over = withPadding(request('robot.stop', {}, { request_id: 'over' }), MAX_LINE + 1);
  const responses = run([over, next]);
  assert.equal(responses.length, 2);
  assert.equal(responses[0].status, 'error');
  assert.equal(responses[0].error.code, 'INVALID_REQUEST');
  assert.equal(responses[0].request_id, '');
  assert.equal(responses[1].request_id, 'after');
  assert.equal(responses[1].status, 'success');
});

test('an oversize final line without a newline is answered once and exits cleanly', () => {
  const [response, ...rest] = run(['x'.repeat(MAX_LINE + 1)], { terminated: false });
  assert.equal(rest.length, 0);
  assert.equal(response.error.code, 'INVALID_REQUEST');
  const [exact, ...others] = run([withPadding(request('robot.stop'), MAX_LINE)], { terminated: false });
  assert.equal(others.length, 0);
  assert.equal(exact.status, 'success');
  const [invalid] = run(['{'], { terminated: false });
  assert.equal(invalid.error.code, 'INVALID_REQUEST');
});

test('the response schema check rejects what the contract forbids', () => {
  const ok = { protocol: '1.0', request_id: 'x', status: 'error', error: { code: 'ENGINE_BUSY', message: 'm' } };
  assert.deepEqual(violations(responseSchema, ok), []);
  for (const bad of [
    { ...ok, error: { code: 'NOT_A_CODE', message: 'm' } },
    { ...ok, request_id: 'a'.repeat(129) },
    { ...ok, data: {} },
    { ...ok, status: 'success' },
    { protocol: '1.0', request_id: 'x', status: 'error' },
    { ...ok, extra: 1 },
  ]) assert.notDeepEqual(violations(responseSchema, bad), [], JSON.stringify(bad));
});

test('metadata and discovery reject transport options before any device access', () => {
  for (const action of ['engine.info', 'device.list', 'project.import', 'project.export']) {
    for (const data of [
      { transport: 'hid' }, { transport: 'serial', port: 'COM24' },
      { transport: 'dry-run' }, { baud: 115200 }, { device: 'fake' }, { read_ms: 1 },
    ]) {
      const response = invoke(action, data);
      assert.equal(response.status, 'error');
      assert.equal(response.error.code, 'INVALID_INPUT');
      assert.match(response.error.message, /only valid for device commands/);
    }
  }
});
