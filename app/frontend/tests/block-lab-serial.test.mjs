import test from "node:test";
import assert from "node:assert/strict";
import { fileURLToPath } from "node:url";
import { createServer } from "vite";

const server = await createServer({
  configFile: false,
  root: fileURLToPath(new URL("../", import.meta.url)),
  cacheDir: "node_modules/.vite-block-lab-tests",
  optimizeDeps: { noDiscovery: true, include: [] },
  server: { middlewareMode: true, hmr: false },
  appType: "custom",
});
let addReceived, addLine, emptyLog, endPartialLine, unreadCount, elapsedText, toBase64, fromBase64, MAX_LOG_LINES;
try {
  ({ addReceived, addLine, emptyLog, endPartialLine, unreadCount, elapsedText, toBase64, fromBase64, MAX_LOG_LINES } =
    await server.ssrLoadModule("/src/modes/block-programming/shell/serialLog.ts"));
} finally {
  await server.close();
}

const texts = log => log.lines.map(line => line.text ?? line.key);

test("board text that arrives in pieces becomes whole lines, Serial.println's \\r\\n included", () => {
  let log = addReceived(emptyLog(0), "สวัส", 1000);
  assert.deepEqual([texts(log), log.partial], [[], "สวัส"]);
  log = addReceived(log, "ดี\r\nกดแล้ว\r\nลูกบิด = ", 2000);
  assert.deepEqual([texts(log), log.partial], [["สวัสดี", "กดแล้ว"], "ลูกบิด = "]);
  log = addLine(log, { kind: "sent", time: 3000, text: "hi" });
  assert.deepEqual(log.lines.map(line => line.id), [1, 2, 3]);
  assert.equal(elapsedText(log, 65_000), "1:05");
  assert.equal(new TextDecoder().decode(fromBase64(toBase64("ส่ง\n"))), "ส่ง\n");
});

test("text cut off when reading stopped stays its own line instead of joining the next reading", () => {
  let log = addReceived(emptyLog(0), "สวัสดี\r\nสวัสด", 1000);
  log = endPartialLine(log, 2000);
  assert.deepEqual([texts(log), log.partial, log.lines[1].time], [["สวัสดี", "สวัสด"], "", 2000]);
  assert.equal(endPartialLine(log, 3000), log);
  log = addReceived(log, "สวัสดี\r\n", 4000);
  assert.deepEqual(texts(log), ["สวัสดี", "สวัสด", "สวัสดี"]);
});

test("the Output tab counts new board and app lines, not what the child sent", () => {
  let log = addReceived(emptyLog(0), "a\r\nb\r\n", 0);
  const seenUpTo = log.nextId;
  log = addLine(log, { kind: "sent", time: 1, text: "hi" });
  log = addReceived(log, "c\r\n", 2);
  log = addLine(log, { kind: "system", time: 3, key: "blockLab.terminal.log.opened" });
  assert.equal(unreadCount(log, seenUpTo), 2);
  assert.equal(unreadCount(addReceived(emptyLog(4), "x\r\n", 4), seenUpTo), 1, "a log started over is all new");
});

test("the log keeps the newest 200 lines and says older ones were dropped", () => {
  const many = Array.from({ length: MAX_LOG_LINES + 5 }, (_, index) => `line ${index}\n`).join("");
  const log = addReceived(emptyLog(0), many, 0);
  assert.equal(log.lines.length, MAX_LOG_LINES);
  assert.equal(log.lines[0].text, "line 5");
  assert.equal(log.trimmed, true);
  const endless = addReceived(emptyLog(0), "x".repeat(1500), 0);
  assert.deepEqual([endless.lines.length, endless.partial], [1, ""]);
});
