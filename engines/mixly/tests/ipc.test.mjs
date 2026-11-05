import { test } from 'node:test';
import assert from 'node:assert/strict';
import { spawnSync } from 'node:child_process';
import { createHash } from 'node:crypto';
import { readFileSync, readdirSync, statSync } from 'node:fs';
import { fileURLToPath, pathToFileURL } from 'node:url';

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

test('the shipped binary generates School Bot blocks exactly as the golden C++', () => {
  const golden = new URL('../testdata/school-bot/device-outputs', import.meta.url).href;
  const { blocks } = JSON.parse(readFileSync(new URL(`${golden}.json`)));
  const [response] = run([request('program.generate', {
    board: 'arduino_avr', platform: 'Arduino/Genuino Uno', blocks,
  })]);
  assert.equal(response.status, 'success', JSON.stringify(response.error));
  assert.equal(response.data.code, readFileSync(new URL(`${golden}.ino`), 'utf8').replace(/\r\n/g, '\n'));
});

test('validate lists UNO wiring problems by code instead of failing', () => {
  const print = pin => ({
    type: 'serial_print', fields: { serial_select: 'Serial', new_line: 'println' },
    values: { CONTENT: { type: 'sb_button', fields: { PIN: pin } } },
  });
  const blocks = [{
    type: 'sb_loop',
    statements: { DO: { ...print('7'), next: { type: 'sb_led', fields: { PIN: '7', STATE: 'HIGH' } } } },
  }];
  const [check, generated] = run([
    request('project.validate', { board: 'arduino_avr', platform: 'Arduino/Genuino Uno', blocks }),
    request('program.generate', { board: 'arduino_avr', platform: 'Arduino/Genuino Uno', blocks }),
  ]);
  assert.equal(check.status, 'success', JSON.stringify(check.error));
  assert.deepEqual(check.data.problems, [{ code: 'PIN_SHARED', pin: '7', uses: ['input', 'output'] }]);
  assert.equal(generated.error.code, 'PROGRAM_INVALID');
});

test('device.list describes every port it names', () => {
  const [response] = run([request('device.list')]);
  assert.equal(response.status, 'success', JSON.stringify(response.error));
  assert.deepEqual(response.data.devices.map(device => device.port), response.data.ports);
  for (const device of response.data.devices) assert.ok(['uno', 'usb-serial', 'other'].includes(device.kind));
});

test('oversized and malformed requests recover with matching IDs', () => {
  const responses = run(['{', 'x'.repeat(66000), request('engine.info', {}, 'after-errors')]);
  assert.equal(responses[0].error.code, 'PROTOCOL_INVALID');
  assert.equal(responses[1].error.code, 'INVALID_REQUEST');
  assert.equal(responses[2].request_id, 'after-errors');
  assert.equal(responses[2].status, 'success');
});

test('Arduino upload names the picked board by USB IDs, and only upload takes them', () => {
  const uno = { board: 'arduino_avr', platform: 'Arduino/Genuino Uno' };
  const blocks = [{ type: 'sb_loop', statements: { DO: { type: 'sb_led', fields: { PIN: '13', STATE: 'HIGH' } } } }];
  const [missing, malformed, compile, planned] = run([
    request('program.upload', { ...uno, blocks, port: 'COM5' }),
    request('program.upload', { ...uno, blocks, port: 'COM5', vid: '1A86', pid: '75' }),
    request('program.compile', { ...uno, blocks, vid: '1A86', pid: '7523' }),
    request('program.upload', { ...uno, blocks, port: 'COM5', vid: '1a86', pid: '7523' }),
  ]);
  assert.equal(missing.error.code, 'INVALID_INPUT');
  assert.equal(malformed.error.code, 'INVALID_INPUT');
  assert.equal(compile.error.code, 'INVALID_INPUT');
  assert.equal(planned.status, 'success', JSON.stringify(planned.error));
  assert.deepEqual(planned.data, {
    engine: 'mixly', board: 'arduino_avr', platform: 'Arduino/Genuino Uno',
    delivery: 'dry-run', compiler: 'arduino-cli', compiled: false, uploaded: false,
  });
});

test('block text cannot start a new line of C++', () => {
  const uno = { board: 'arduino_avr', platform: 'Arduino/Genuino Uno' };
  const print = (text, comment) => [{
    type: 'serial_print', fields: { serial_select: 'Serial', new_line: 'println' }, comment,
    values: { CONTENT: { type: 'text', fields: { TEXT: text } } },
  }];
  const [field, comment, quoted] = run([
    request('program.generate', { ...uno, blocks: print('a\n#include "C:/secret.txt"') }),
    request('program.generate', { ...uno, blocks: print('a', 'note\r#include "C:/secret.txt"') }),
    request('program.generate', { ...uno, blocks: print('say "hi" \\') }),
  ]);
  assert.equal(field.error.code, 'PROJECT_INVALID');
  assert.equal(comment.error.code, 'PROJECT_INVALID');
  assert.equal(quoted.status, 'success', JSON.stringify(quoted.error));
  assert.match(quoted.data.code, /Serial\.println\("say \\"hi\\" \\\\"\);/);
});

test('fields the generators paste into the C++ take only the values their blocks offer', () => {
  const uno = { board: 'arduino_avr', platform: 'Arduino/Genuino Uno' };
  const wait = (unit, milliseconds) => [{
    type: 'controls_delay', fields: { UNIT: unit },
    values: { DELAY_TIME: { type: 'math_number', fields: { NUM: milliseconds } } },
  }];
  const [unit, number, compile, plain] = run([
    request('program.generate', { ...uno, blocks: wait('exit(0); delay', '1') }),
    request('program.generate', { ...uno, blocks: wait('delay', '1); while (true') }),
    request('program.compile', { ...uno, blocks: wait('delayMicroseconds', '1) + system("x"') }),
    request('program.generate', { ...uno, blocks: wait('delayMicroseconds', '2.5') }),
  ]);
  for (const response of [unit, number, compile]) assert.equal(response.error?.code, 'PROJECT_INVALID');
  assert.equal(plain.status, 'success', JSON.stringify(plain.error));
  assert.match(plain.data.code, /delayMicroseconds\(2\.5\);/);
});

test('a serial session answers in one process and opens only a listed board port', () => {
  const [idle, noIds, missing, dryRun, write, closed] = run([
    request('serial.read'),
    request('serial.open', { port: 'COM3' }),
    request('serial.open', { port: 'COM-test-missing', vid: '1A86', pid: '7523', transport: 'native' }),
    request('serial.open', { port: 'COM3', vid: '1A86', pid: '7523' }),
    request('serial.write', { write_base64: 'aGkK' }),
    request('serial.close'),
  ]);
  assert.deepEqual([idle.status, idle.data.open], ['success', false]);
  assert.equal(noIds.error.code, 'INVALID_INPUT');
  assert.equal(missing.error.code, 'TRANSPORT_ERROR');
  assert.deepEqual([dryRun.data.open, dryRun.data.delivery], [false, 'dry-run']);
  assert.equal(write.error.code, 'TRANSPORT_ERROR');
  assert.deepEqual([closed.status, closed.data.open], ['success', false]);
});

test('upload and serial default to dry-run without opening a port', () => {
  const [upload, serial, flash] = run([
    request('program.upload', {
      board: 'arduino_avr', source: 'void setup(){} void loop(){}', port: 'no-device', vid: '2341', pid: '0043',
    }),
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

test('the build output carries the bundled Arduino libraries beside the engine', () => {
  const libraries = new URL('libraries/', pathToFileURL(binary));
  assert.deepEqual(readdirSync(libraries).sort(), [
    'Adafruit_NeoPixel', 'IRremote', 'Servo', 'U8g2',
    'ZLTech_APDS9960', 'ZLTech_Adafruit_TCS34725', 'ZLTech_dht',
  ]);
  for (const file of ['U8g2/src/U8g2lib.h', 'ZLTech_dht/dht.h', 'Servo/LICENSE.txt', 'IRremote/LICENSE']) {
    assert.ok(statSync(new URL(file, libraries)).isFile(), file);
  }
});
