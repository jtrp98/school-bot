import test from "node:test";
import assert from "node:assert/strict";
import { fileURLToPath } from "node:url";
import { createServer } from "vite";

globalThis.localStorage = { getItem: () => null, setItem() {} };
globalThis.location = { search: "" };
const server = await createServer({
  configFile: false,
  root: fileURLToPath(new URL("../", import.meta.url)),
  cacheDir: "node_modules/.vite-block-lab-tests",
  optimizeDeps: { noDiscovery: true, include: [] },
  server: { middlewareMode: true, hmr: false },
  appType: "custom",
});
let client, runBlockEngine, prepareWindowClose;
try {
  ({ client } = await server.ssrLoadModule("/src/core/client.ts"));
  ({ runBlockEngine } = await server.ssrLoadModule("/src/modes/block-programming/shell/blockEngine.ts"));
  ({ prepareWindowClose } = await server.ssrLoadModule("/src/core/windowLifecycle.ts"));
} finally {
  await server.close();
}

const tick = (ms = 0) => new Promise(resolve => setTimeout(resolve, ms));

// Block Lab's own question about a running upload goes with its page if the page crashes, so the
// window's close step itself waits for the upload instead of cancelling it.
test("closing the window waits for a running upload, and only while one runs", async () => {
  let finishUpload;
  client.run = async (_mode, action) =>
    action === "program.upload"
      ? new Promise(resolve => { finishUpload = () => resolve({ status: "success", data: { uploaded: true } }); })
      : { status: "success", data: {} };
  const notes = [];
  await runBlockEngine("program.compile", {});
  await prepareWindowClose(note => notes.push(note));
  assert.deepEqual(notes, [], "a compile does not hold the window");

  const upload = runBlockEngine("program.upload", {});
  await tick();
  let closed = false;
  const closing = prepareWindowClose(note => notes.push(note)).then(() => { closed = true; });
  await tick(20);
  assert.deepEqual([closed, notes], [false, ["blockLab.send.guard.closeWait"]]);
  finishUpload();
  await upload;
  await closing;

  notes.length = 0;
  await prepareWindowClose(note => notes.push(note));
  assert.deepEqual(notes, [], "the hold ends with the upload");
});

test("an upload that fails lets the window close again", async () => {
  client.run = async () => {
    throw new Error("upload failed");
  };
  await assert.rejects(runBlockEngine("program.upload", {}));
  const notes = [];
  await prepareWindowClose(note => notes.push(note));
  assert.deepEqual(notes, []);
});

// The host's plain cancel lets an upload finish, so the child's stop must reach it as StopUpload.
test("stopping an upload asks the host to stop it; stopping a compile cancels", async () => {
  const calls = [];
  let finish;
  client.run = () => new Promise(resolve => { finish = () => resolve({ status: "success", data: {} }); });
  client.cancel = async () => { calls.push("cancel"); finish(); };
  client.stopUpload = async () => { calls.push("stopUpload"); finish(); };
  for (const action of ["program.compile", "program.upload"]) {
    const stop = new AbortController();
    const running = runBlockEngine(action, {}, stop.signal);
    await tick();
    stop.abort();
    await running;
  }
  assert.deepEqual(calls, ["cancel", "stopUpload"]);
});
