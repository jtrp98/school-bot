// What the Output tab lists. Text from the board and text the child sent are data, shown as
// they came; the app's own notes are catalog keys, so they follow the language.
export type NewLine =
  | { kind: "board" | "sent"; time: number; text: string }
  | { kind: "error"; time: number; key: string; message?: string; code: string }
  | { kind: "system"; time: number; key: string };

export type LogLine = NewLine & { id: number };

export type SerialLog = {
  lines: LogLine[];
  // Board text after the last line break; shown as the last line until it ends.
  partial: string;
  // Older lines were dropped to keep the newest MAX_LOG_LINES.
  trimmed: boolean;
  nextId: number;
  // When the log began, for the elapsed times beside each line.
  startedAt: number;
};

export const MAX_LOG_LINES = 200;
// A board that prints without ever ending a line would otherwise grow one line without bound.
const MAX_PARTIAL_CHARS = 1000;

export const emptyLog = (now: number): SerialLog => ({
  lines: [],
  partial: "",
  trimmed: false,
  nextId: 1,
  startedAt: now,
});

function append(log: SerialLog, added: readonly NewLine[]): SerialLog {
  const numbered: LogLine[] = added.map((line, index) => ({ ...line, id: log.nextId + index }));
  const all = [...log.lines, ...numbered];
  const overflow = Math.max(0, all.length - MAX_LOG_LINES);
  return {
    ...log,
    lines: overflow ? all.slice(overflow) : all,
    trimmed: log.trimmed || overflow > 0,
    nextId: log.nextId + added.length,
  };
}

// Board text arrives in pieces; Serial.println ends each line with "\r\n".
export function addReceived(log: SerialLog, text: string, time: number): SerialLog {
  const parts = (log.partial + text).split("\n");
  let partial = parts.pop() ?? "";
  if (partial.length > MAX_PARTIAL_CHARS) {
    parts.push(partial);
    partial = "";
  }
  const added = parts.map(part => ({ kind: "board" as const, time, text: part.replace(/\r$/, "") }));
  return { ...append(log, added), partial };
}

// Reading stopped mid-line: opening the port again restarts the board's program, so that text
// is never finished and stays a line of its own instead of joining the next first line.
export function endPartialLine(log: SerialLog, time: number): SerialLog {
  if (!log.partial) return log;
  const ended = append(log, [{ kind: "board", time, text: log.partial.replace(/\r$/, "") }]);
  return { ...ended, partial: "" };
}

export const addLine = (log: SerialLog, line: NewLine): SerialLog => append(log, [line]);

// Lines from `fromId` on that the child has not seen: the board's and the app's, not their own.
// A log started over since then numbers from 1 again, so all of it is new.
export function unreadCount(log: SerialLog, fromId: number): number {
  const from = fromId > log.nextId ? 1 : fromId;
  return log.lines.filter(line => line.id >= from && line.kind !== "sent").length;
}

// m:ss since the log began, h:mm:ss after an hour.
export function elapsedText(log: SerialLog, time: number): string {
  const seconds = Math.max(0, Math.floor((time - log.startedAt) / 1000));
  const hours = Math.floor(seconds / 3600);
  const minutes = Math.floor((seconds % 3600) / 60);
  const rest = String(seconds % 60).padStart(2, "0");
  return hours ? `${hours}:${String(minutes).padStart(2, "0")}:${rest}` : `${minutes}:${rest}`;
}

export function toBase64(text: string): string {
  let binary = "";
  for (const byte of new TextEncoder().encode(text)) binary += String.fromCharCode(byte);
  return btoa(binary);
}

export function fromBase64(data: string): Uint8Array {
  return Uint8Array.from(atob(data), character => character.charCodeAt(0));
}
