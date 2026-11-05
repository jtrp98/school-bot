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
let parseDeviceList, followLink, returnedBoard, startUnplugSearch, advanceUnplugSearch, boardCards, mascotLine;
try {
  const connectDir = "/src/modes/block-programming/connect";
  ({ parseDeviceList, followLink, returnedBoard } = await server.ssrLoadModule(`${connectDir}/boardDevices.ts`));
  ({ startUnplugSearch, advanceUnplugSearch } = await server.ssrLoadModule(`${connectDir}/unplugSearch.ts`));
  ({ boardCards, mascotLine } = await server.ssrLoadModule(`${connectDir}/connectView.ts`));
} finally {
  await server.close();
}

const UNO = { usb: true, vid: "2341", pid: "0043", kind: "uno" };
const CP210X = { usb: true, vid: "10C4", pid: "EA60", serial_number: "0001", kind: "usb-serial", chip: "CP210x" };
const CH340 = { usb: true, vid: "1A86", pid: "7523", kind: "usb-serial", chip: "CH340" };
const list = (...devices) => parseDeviceList({ devices });
const connect = device => ({ ...device, since: 1, lost: false });

test("a serial number shared by two boards keys them by port and USB ids", () => {
  const [uno, first, second, clone, serialPort] = list(
    { ...UNO, port: "COM3", serial_number: "857" },
    { ...CP210X, port: "COM11" },
    { ...CP210X, port: "COM12" },
    { ...CH340, port: "COM5" },
    { port: "COM1", usb: false, kind: "other" },
  );
  assert.deepEqual([uno.key, uno.followsAcrossPorts], ["2341:0043:857", true]);
  assert.deepEqual([first.key, first.followsAcrossPorts], ["10C4:EA60@COM11", false]);
  assert.equal(second.key, "10C4:EA60@COM12");
  assert.deepEqual([clone.key, clone.followsAcrossPorts], ["1A86:7523@COM5", false]);
  assert.equal(serialPort.key, "COM1");
});

test("the chosen board follows its key change and only then any socket", () => {
  const [alone] = list({ ...CP210X, port: "COM11" });
  const link = connect(alone);
  assert.equal(link.followsAcrossPorts, true);
  // A second clone with the same serial: ours is now only ours in COM11.
  const shared = followLink(link, list({ ...CP210X, port: "COM11" }, { ...CP210X, port: "COM12" }));
  assert.deepEqual([shared.key, shared.followsAcrossPorts, shared.lost], ["10C4:EA60@COM11", false, false]);
  // Its cable out, the other clone is not taken for it, even once its serial is unique again.
  const lost = followLink(shared, list({ ...CP210X, port: "COM12" }));
  assert.equal(lost.lost, true);
  // Back in its socket it still waits for the child to choose its card: the other clone could
  // have been moved there.
  const back = followLink(lost, list({ ...CP210X, port: "COM11" }, { ...CP210X, port: "COM12" }));
  assert.deepEqual([back.port, back.lost], ["COM11", true]);
});

test("a clone without a serial that comes back is chosen again, a UNO with one is not", () => {
  const clone = connect(list({ ...CH340, port: "COM5" })[0]);
  const cloneLost = followLink(clone, list());
  const cloneBack = followLink(cloneLost, list({ ...CH340, port: "COM5" }));
  assert.equal(cloneBack.lost, true, "the robot arm's CH340 could be in that socket now");
  const uno = connect(list({ ...UNO, port: "COM3", serial_number: "857" })[0]);
  const unoBack = followLink(followLink(uno, list()), list({ ...UNO, port: "COM3", serial_number: "857" }));
  assert.equal(unoBack.lost, false);
});

test("a clone back in its socket shows one card to confirm, not a ghost beside it", () => {
  const lost = followLink(connect(list({ ...CH340, port: "COM5" })[0]), list());
  const screenWith = boards => ({
    conn: { link: lost, names: {}, failure: null, devices: boards },
    boards,
    returned: returnedBoard(lost, boards),
    pickedBoard: undefined,
    search: { state: null },
    disconnected: null,
    cameBack: false,
    phase: "idle",
  });
  const waiting = screenWith([]);
  assert.deepEqual(boardCards(waiting).map(card => [card.id, card.tag]), [["lost:1A86:7523@COM5", "lost"]]);
  assert.equal(mascotLine(waiting), "lost");
  const back = screenWith(list({ ...CH340, port: "COM5" }, { ...CH340, port: "COM6" }));
  assert.deepEqual(boardCards(back).map(card => [card.id, card.status, card.tag]), [
    ["1A86:7523@COM5", "free", "returned"],
    ["1A86:7523@COM6", "free", null],
  ]);
  assert.equal(mascotLine(back), "returned");
});

test("another gadget on the board's port is not the board coming back", () => {
  const link = connect(list({ ...CH340, port: "COM5" })[0]);
  const lost = followLink(link, list({ ...UNO, port: "COM5", serial_number: "857" }));
  assert.equal(lost.lost, true);
  const moved = followLink(link, list({ ...UNO, port: "COM3", serial_number: "857" }));
  assert.equal(moved.lost, true, "a clone without a serial is ours only in the same socket");
  const uno = connect(list({ ...UNO, port: "COM3", serial_number: "857" })[0]);
  const unoMoved = followLink(uno, list({ ...UNO, port: "COM8", serial_number: "857" }));
  assert.deepEqual([unoMoved.port, unoMoved.lost], ["COM8", false]);
});

test("unplugging finds the board by its vanished port, even in a new socket", () => {
  const boards = list({ ...UNO, port: "COM3", serial_number: "857" }, { ...CH340, port: "COM5" });
  let search = startUnplugSearch(boards);
  search = advanceUnplugSearch(search, boards.slice(1));
  assert.deepEqual(search, { step: "plugBack", oursPort: "COM3", remaining: ["COM5"] });
  const replugged = list({ ...CH340, port: "COM5" }, { ...UNO, port: "COM8", serial_number: "857" });
  search = advanceUnplugSearch(search, replugged);
  assert.deepEqual(search, { step: "found", port: "COM8", portChanged: true });
  const both = advanceUnplugSearch(startUnplugSearch(boards), []);
  assert.deepEqual(both, { step: "tooMany", vanished: 2 });
});
