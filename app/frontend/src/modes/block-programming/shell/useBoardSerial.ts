import { useCallback, useEffect, useRef, useState } from "react";
import { EngineError } from "../../../core/engineError";
import type { BoardLink } from "../connect/boardDevices";
import { runBlockEngine } from "./blockEngine";
import { addLine, addReceived, emptyLog, endPartialLine, fromBase64, toBase64, type SerialLog } from "./serialLog";

// How often an open port is read; the engine keeps what arrives in between (up to 64 KB).
const READ_EVERY_MS = 300;

// closed: not reading. failed: opening or reading stopped, and why (`failure`).
type SerialStatus = "closed" | "opening" | "open" | "failed";
type SerialFailure = "unreadable" | "robotPort";

// The code a child reads out to the teacher (docs/TEACHER-SETUP.md).
export const serialFailureCode = (failure: SerialFailure) => (failure === "robotPort" ? "E-ARM" : "E-USB");

type SerialAnswer = {
  open?: unknown;
  received_base64?: unknown;
  dropped?: unknown;
  problem?: unknown;
};

const codeOf = (error: unknown) => (error instanceof EngineError ? error.code : "");

// Why serial.write failed: TRANSPORT_ERROR is the port gone, INVALID_INPUT text over the engine's
// 256 bytes; anything else (the engine stopped or answered out of contract) is the app's side.
function sendFailure(error: unknown): { key: string; code: string } {
  switch (codeOf(error)) {
    case "TRANSPORT_ERROR":
      return { key: "blockLab.terminal.log.sendFailed", code: "E-USB" };
    case "INVALID_INPUT":
      return { key: "blockLab.terminal.log.sendTooLong", code: "E-LONG" };
    default:
      return { key: "blockLab.terminal.log.sendFailedInApp", code: "E-APP" };
  }
}

// The Output tab's Serial monitor for the chosen board (engines/mixly docs/API.md, "Serial
// session"). Reading starts only when the child asks, because opening the port restarts the
// UNO's program once. It stops when the board is lost or another board is chosen, and pauses
// while a send needs the port.
export function useBoardSerial(link: BoardLink | null) {
  const [status, setStatus] = useState<SerialStatus>("closed");
  const [failure, setFailure] = useState<SerialFailure>("unreadable");
  const [log, setLog] = useState<SerialLog>(() => emptyLog(Date.now()));
  // A send holds the port from its compile to the end of its upload; reading waits.
  const [paused, setPaused] = useState(false);
  const pausedRef = useRef(false);
  const decoder = useRef(new TextDecoder());
  const reading = useRef<{ key: string; port: string } | null>(null);
  const alive = useRef(true);

  useEffect(() => {
    alive.current = true;
    return () => {
      alive.current = false;
    };
  }, []);

  // An empty log shows the failure as the whole panel; under earlier lines it is one more line.
  const fail = useCallback((reason: SerialFailure) => {
    reading.current = null;
    if (!alive.current) return;
    setFailure(reason);
    setStatus("failed");
    const now = Date.now();
    setLog(current => {
      const ended = endPartialLine(current, now);
      return ended.lines.length === 0 ? ended : addLine(ended, {
        kind: "error",
        time: now,
        key: reason === "robotPort" ? "blockLab.terminal.log.robotPort" : "blockLab.terminal.log.readFailed",
        code: serialFailureCode(reason),
      });
    });
  }, []);

  const open = useCallback(async (board: BoardLink | null = link) => {
    if (!board?.vid || !board.pid || reading.current || pausedRef.current) return;
    reading.current = { key: board.key, port: board.port };
    decoder.current = new TextDecoder();
    setStatus("opening");
    try {
      await runBlockEngine("serial.open", { port: board.port, vid: board.vid, pid: board.pid });
      if (!alive.current || reading.current?.key !== board.key) return;
      setStatus("open");
      const now = Date.now();
      // Times count from the first reading, unless earlier lines are still on the log.
      setLog(current => {
        const kept = endPartialLine(current, now);
        return addLine(kept.lines.length ? kept : emptyLog(now), {
          kind: "system",
          time: now,
          key: "blockLab.terminal.log.opened",
        });
      });
    } catch (error) {
      if (codeOf(error) === "ENGINE_CANCELLED") {
        reading.current = null;
        if (alive.current) setStatus("closed");
        return;
      }
      fail(codeOf(error) === "SAFETY_LIMIT" ? "robotPort" : "unreadable");
    }
  }, [link, fail]);

  const close = useCallback(async () => {
    const wasReading = reading.current !== null;
    reading.current = null;
    if (alive.current) setStatus(now => (now === "failed" ? now : "closed"));
    if (wasReading) await runBlockEngine("serial.close", {}).catch(() => undefined);
    return wasReading;
  }, []);

  // Before a send: stops reading and says whether it was on, so resume can turn it back on.
  const pause = useCallback(() => {
    pausedRef.current = true;
    setPaused(true);
    return close();
  }, [close]);

  const resume = useCallback((board: BoardLink, reopen: boolean) => {
    pausedRef.current = false;
    if (!alive.current) return;
    setPaused(false);
    if (reopen) void open(board);
  }, [open]);

  useEffect(() => {
    if (status !== "open") return;
    let stopped = false;
    let timer: number | undefined;
    const tick = async () => {
      try {
        const { data } = await runBlockEngine<SerialAnswer>("serial.read", {});
        if (stopped || !reading.current) return;
        const now = Date.now();
        const received = typeof data?.received_base64 === "string" ? data.received_base64 : "";
        if (received || data?.dropped === true) {
          setLog(current => {
            let next = current;
            if (data?.dropped === true) {
              next = addLine(next, { kind: "system", time: now, key: "blockLab.terminal.log.dropped" });
            }
            const text = decoder.current.decode(fromBase64(received), { stream: true });
            return text ? addReceived(next, text, now) : next;
          });
        }
        if (data?.open !== true) {
          fail("unreadable");
          return;
        }
      } catch (error) {
        if (stopped) return;
        if (codeOf(error) === "ENGINE_CANCELLED") {
          reading.current = null;
          setStatus("closed");
        } else {
          fail("unreadable");
        }
        return;
      }
      if (!stopped) timer = window.setTimeout(() => void tick(), READ_EVERY_MS);
    };
    void tick();
    return () => {
      stopped = true;
      window.clearTimeout(timer);
    };
  }, [status, fail]);

  // The port belongs to the board it was opened for: a lost cable or another board ends it.
  useEffect(() => {
    const current = reading.current;
    if (!current) return;
    if (!link || link.lost || link.key !== current.key || link.port !== current.port) void close();
  }, [link, close]);

  // Returns whether the child sees it on the log; a failed send keeps the text to try again.
  const send = useCallback(async (text: string) => {
    const now = Date.now();
    try {
      await runBlockEngine("serial.write", { write_base64: toBase64(`${text}\n`) });
      setLog(current => addLine(current, { kind: "sent", time: now, text }));
      return true;
    } catch (error) {
      const failure = sendFailure(error);
      setLog(current => addLine(current, { kind: "error", time: now, message: text, ...failure }));
      return false;
    }
  }, []);

  const clear = useCallback(() => setLog(emptyLog(Date.now())), []);

  return { status, failure, paused, log, open, close, pause, resume, send, clear };
}

export type BoardSerial = ReturnType<typeof useBoardSerial>;
