import type { BoardDevice } from "./boardDevices";

// Finding "our" board among several by unplugging its cable: the card that vanishes is ours,
// and plugging it back shows where it returned. It compares port names, not board keys: a key
// can change when a clone with a shared serial leaves the list, a port name cannot.
export type UnplugSearch =
  | { step: "unplug"; ports: readonly string[] }
  | { step: "plugBack"; oursPort: string; remaining: readonly string[] }
  | { step: "tooMany"; vanished: number }
  | { step: "found"; port: string; portChanged: boolean };

const portsOf = (boards: readonly BoardDevice[]) => boards.map(board => board.port);

export const startUnplugSearch = (boards: readonly BoardDevice[]): UnplugSearch => ({
  step: "unplug",
  ports: portsOf(boards),
});

export function advanceUnplugSearch(search: UnplugSearch, boards: readonly BoardDevice[]): UnplugSearch {
  const current = portsOf(boards);
  if (search.step === "unplug") {
    const vanished = search.ports.filter(port => !current.includes(port));
    if (vanished.length === 0) return search;
    if (vanished.length > 1) return { step: "tooMany", vanished: vanished.length };
    return { step: "plugBack", oursPort: vanished[0], remaining: current };
  }
  if (search.step !== "plugBack") return search;
  if (current.includes(search.oursPort)) return { step: "found", port: search.oursPort, portChanged: false };
  // Plugged into another socket, the board comes back under a new port name, so the one new
  // card is ours. Two new cards at once could be either.
  const added = current.filter(port => !search.remaining.includes(port));
  if (added.length !== 1) return search;
  return { step: "found", port: added[0], portChanged: true };
}
