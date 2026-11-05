import test from "node:test";
import assert from "node:assert/strict";
import { fileURLToPath, pathToFileURL } from "node:url";
import { createRequire } from "node:module";

const root = new URL("../", import.meta.url);
const require = createRequire(new URL("package.json", root));
const { createServer } = await import(pathToFileURL(require.resolve("vite")).href);
const vite = await createServer({ configFile: false, root: fileURLToPath(root),
  server: { middlewareMode: true, hmr: false }, appType: "custom" });
const { OfflineLearning } = await vite.ssrLoadModule("/src/learning/offlineLearning.ts");
test.after(() => vite.close());

function storage() {
  const values = new Map();
  return { getItem: key => values.get(key) ?? null, setItem: (key, value) => values.set(key, value) };
}
const input = { name: "Jump", mode: "code", content: { kind: "code", source: "hero.jump()" }, missionId: 3, criteriaId: 3 };

test("offline work survives reopening, uses stable keys, and never uploads a local score", async () => {
  const disk = storage();
  const calls = [];
  let id = 0;
  const gateway = { request: async request => {
    calls.push(request);
    return { data: request.operation === "project" ? { project_revision_id: 41 } : { id: 91, state: "queued" }, etag: "tag" };
  } };
  const first = new OfflineLearning("school11/student301", disk, gateway, () => `key${++id}`);
  first.enqueue(input);
  const reopened = new OfflineLearning("school11/student301", disk, gateway);
  await Promise.all([reopened.sync(), reopened.sync()]);
  assert.equal(calls.length, 2);
  assert.equal(calls[0].key, "key2");
  assert.equal(calls[1].key, "key1");
  assert.deepEqual(calls[1].body, { mission_id: 3, criteria_id: 3, project_revision_id: 41 });
  assert.equal(reopened.read().pending.length, 0);
  assert.equal(reopened.read().results[0].state, "queued");
  assert.equal(new OfflineLearning("school12/student301", disk, gateway).read().results.length, 0);
});

test("lost submission response preserves project receipt and replays the same submission key", async () => {
  const disk = storage();
  let fail = true;
  let sequence = 0;
  const calls = [];
  const store = new OfflineLearning("school11/student301", disk, { request: async request => {
    calls.push(request);
    if (request.operation === "project") return { data: { project_revision_id: 7 } };
    if (fail) throw new Error("network");
    return { data: { id: 19, state: "queued" } };
  } }, () => `key${++sequence}`);
  store.enqueue(input);
  await assert.rejects(store.sync(), /network/);
  assert.equal(store.read().pending[0].projectRevisionId, 7);
  fail = false;
  await store.sync();
  assert.equal(calls.filter(c => c.operation === "project").length, 1);
  assert.equal(calls[1].key, calls[2].key);
  assert.equal(store.read().pending.length, 0);
});

test("work queued during synchronization is retained and processed", async () => {
  const disk = storage();
  let count = 0;
  let sequence = 0;
  const store = new OfflineLearning("scope", disk, { request: async request => {
    if (request.operation === "project") {
      if (++count === 1) store.enqueue({ ...input, name: "Second" });
      return { data: { project_revision_id: count } };
    }
    return { data: { id: count, state: "awaiting_teacher" } };
  } }, () => `key${++sequence}`);
  store.enqueue(input);
  await store.sync();
  assert.equal(store.read().results.length, 2);
  assert.equal(store.read().pending.length, 0);
});

test("failed persistence never silently claims that work is queued", () => {
  const store = new OfflineLearning("scope", { getItem: () => null, setItem: () => { throw new Error("quota"); } },
    { request: () => { throw new Error("must not request"); } }, () => "id");
  assert.throws(() => store.enqueue(input), /quota/);
});
