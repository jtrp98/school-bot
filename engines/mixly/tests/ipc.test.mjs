import { test } from 'node:test';
import assert from 'node:assert/strict';
import { spawnSync } from 'node:child_process';
import { createHash } from 'node:crypto';
import { readFileSync, readdirSync } from 'node:fs';
import { fileURLToPath } from 'node:url';

const platform = process.platform === 'win32' ? 'windows' : process.platform;
const arch = process.arch === 'x64' ? 'amd64' : process.arch;
const suffix = process.platform === 'win32' ? '.exe' : '';
const binary = process.env.MIXLY_ENGINE ?? fileURLToPath(new URL(`../bin/${platform}-${arch}/mixly-engine${suffix}`, import.meta.url));
const request = (action, data = {}, request_id = 'mixly-ipc') => JSON.stringify({
  protocol: '1.0', request_id, mode: 'block-programming', action, data,
});
const schema = JSON.parse(readFileSync(new URL('../../../contracts/engine/response.schema.json', import.meta.url)));

function run(lines) {
  const child = spawnSync(binary, [], {
    input: lines.join('\n') + '\n', encoding: 'utf8', timeout: 15000, maxBuffer: 2 ** 20, windowsHide: true,
  });
  assert.ifError(child.error);
  assert.equal(child.status, 0, child.stderr);
  assert.equal(child.stderr, '');
  return child.stdout.trim().split(/\r?\n/).map(line => {
    const response = JSON.parse(line);
    assert.equal(response.protocol, '1.0');
    assert.equal(typeof response.request_id, 'string');
    assert.ok(Buffer.byteLength(response.request_id) <= 128);
    assert.ok(['success', 'error'].includes(response.status));
    assert.ok(Object.keys(response).every(key => key in schema.properties));
    if (response.status === 'error') {
      assert.equal(response.data, undefined);
      assert.equal(typeof response.error.message, 'string');
      assert.ok(response.error.message.length > 0);
      assert.ok(schema.$defs.errorCode.enum.includes(response.error.code));
    } else {
      assert.equal(response.error, undefined);
      assert.equal(typeof response.data, 'object');
      assert.ok(response.data !== null && !Array.isArray(response.data));
    }
    return response;
  });
}

test('standalone executable identifies itself and retains all 17 board families', () => {
  const [info, boards] = run([request('engine.info'), request('board.list', {}, 'boards')]);
  assert.equal(info.data.engine, 'mixly');
  assert.equal(info.data.manifest.id, 'mixly');
  assert.equal(boards.request_id, 'boards');
  assert.equal(boards.data.boards.length, 17);
  assert.ok(boards.data.boards.some(board => board.id === 'arduino_esp32'));
  assert.ok(boards.data.boards.some(board => board.id === 'micropython_nrf51822_microbit'));
});

test('compiles original .mix XML into Arduino code without Electron or source beside the exe', () => {
  const xml = readFileSync(new URL('./fixtures/avr-serial.mix', import.meta.url), 'utf8');
  const [response] = run([request('program.generate', {
    board: 'arduino_avr', platform: 'Arduino/Genuino Uno', xml,
  })]);
  assert.equal(response.status, 'success', JSON.stringify(response.error));
  assert.match(response.data.code, /Serial\.begin\(9600\)/);
  assert.match(response.data.code, /I like Mixly!/);
  assert.match(response.data.code, /for\s*\(/);
});

test('oversized and malformed requests recover with matching IDs', () => {
  const responses = run(['{', 'x'.repeat(66000), request('engine.info', {}, 'after-errors')]);
  assert.equal(responses[0].error.code, 'PROTOCOL_INVALID');
  assert.equal(responses[1].error.code, 'INVALID_REQUEST');
  assert.equal(responses[2].request_id, 'after-errors');
  assert.equal(responses[2].status, 'success');
});

test('upload and serial default to dry-run without opening a port', () => {
  const [upload, serial, flash] = run([
    request('program.upload', { board: 'arduino_avr', source: 'void setup(){} void loop(){}', port: 'no-device' }),
    request('serial.exchange', { port: 'no-device', write_base64: 'YWJj', read_bytes: 8 }),
    request('firmware.flash', { board: 'micropython_esp32', port: 'no-device' }),
  ]);
  assert.equal(upload.data.delivery, 'dry-run');
  assert.equal(upload.data.uploaded, false);
  assert.equal(upload.data.compiled, false);
  assert.equal(serial.data.delivery, 'dry-run');
  assert.equal(serial.data.written, 0);
  assert.equal(serial.data.received_base64, undefined);
  assert.equal(serial.data.device_acknowledged, false);
  assert.equal(flash.data.delivery, 'dry-run');
  assert.equal(flash.data.flashed, false);
  assert.equal(flash.data.steps, 2);
});

test('vendored files retain their pinned hashes and required licenses', () => {
  const manifest = JSON.parse(readFileSync(new URL('../source-manifest.json', import.meta.url)));
  for (const [name, expected] of Object.entries(manifest.files)) {
    const content = readFileSync(new URL(`../third_party/mixly/${name}`, import.meta.url), 'utf8').replace(/\r\n/g, '\n');
    assert.equal(createHash('sha256').update(content).digest('hex'), expected, name);
  }
  assert.ok(Object.keys(manifest.files).length > 250);
  assert.equal(readdirSync(new URL('../licenses/', import.meta.url)).filter(name => name.endsWith('.txt')).length, 11);
});
