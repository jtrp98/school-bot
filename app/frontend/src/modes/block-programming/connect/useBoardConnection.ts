import { useCallback, useEffect, useRef, useState } from "react";
import { EngineError } from "../../../core/engineError";
import { followLink, listBoardDevices, type BoardDevice, type BoardLink } from "./boardDevices";
import { loadBoardNames, MAX_BOARD_NAME_LENGTH, saveBoardNames, type BoardNames } from "./boardNames";

// Listing ports is cheap and opens none, but the host has one engine slot for all of Block
// Lab, so the list refreshes only every few seconds and only while someone needs it.
const POLL_MS = 3000;

type ScanFailure = { code: string; detail: string };

// The chosen board as a fresh list shows it, checked just before sending: none chosen, chosen
// but not in the list (or the list failed), back in the list but waiting to be chosen again
// (see followLink), or there, on the port it has now.
export type BoardCheck =
  | { kind: "unchosen" }
  | { kind: "missing" }
  | { kind: "unconfirmed" }
  | { kind: "ready"; board: BoardLink };

// Errors that only mean "not this time": the next refresh tries again.
const TRANSIENT = new Set(["ENGINE_BUSY", "ENGINE_CANCELLED"]);

export function useBoardConnection(screenOpen: boolean) {
  const [devices, setDevices] = useState<BoardDevice[] | null>(null);
  const [scanning, setScanning] = useState(false);
  const [failure, setFailure] = useState<ScanFailure | null>(null);
  const [link, setLink] = useState<BoardLink | null>(null);
  const [names, setNames] = useState<BoardNames>(loadBoardNames);
  // Sending pauses the list, leaving the host's single slot to the build and the upload.
  const [pollingPaused, setPollingPaused] = useState(false);
  const alive = useRef(true);
  const inFlight = useRef<Promise<BoardDevice[] | null> | null>(null);
  const linkRef = useRef(link);
  linkRef.current = link;

  // Answers with the list it read, or null when it could not read one.
  const scan = useCallback((): Promise<BoardDevice[] | null> => {
    if (inFlight.current) return inFlight.current;
    const run = (async () => {
      try {
        const found = await listBoardDevices();
        if (!alive.current) return found;
        setDevices(found);
        setFailure(null);
        setLink(current => followLink(current, found));
        return found;
      } catch (error) {
        const code = error instanceof EngineError ? error.code : "ENGINE_FAILED";
        if (!alive.current || TRANSIENT.has(code)) return null;
        const detail = error instanceof EngineError ? error.detail : String(error);
        setFailure({ code, detail });
        return null;
      } finally {
        inFlight.current = null;
      }
    })();
    inFlight.current = run;
    return run;
  }, []);

  useEffect(() => {
    alive.current = true;
    return () => {
      alive.current = false;
    };
  }, []);

  const polling = !pollingPaused && (screenOpen || link !== null);
  // Opening the screen re-lists at once, so its first view is never a stale list.
  useEffect(() => {
    if (!polling) return;
    let stopped = false;
    let timer: number | undefined;
    const tick = async () => {
      await scan();
      if (!stopped) timer = window.setTimeout(() => void tick(), POLL_MS);
    };
    void tick();
    return () => {
      stopped = true;
      window.clearTimeout(timer);
    };
  }, [polling, screenOpen, scan]);

  const rescan = useCallback(async () => {
    setScanning(true);
    await scan();
    if (alive.current) setScanning(false);
  }, [scan]);

  // Lists again now rather than trusting a list up to a few seconds old: the cable may have
  // just gone in or out, or the board may sit on another port.
  const checkBoard = useCallback(async (): Promise<BoardCheck> => {
    const found = await scan();
    const chosen = followLink(linkRef.current, found ?? []);
    if (!chosen) return { kind: "unchosen" };
    if (chosen.lost && found?.some(device => device.key === chosen.key)) return { kind: "unconfirmed" };
    if (!found || chosen.lost) return { kind: "missing" };
    return { kind: "ready", board: chosen };
  }, [scan]);

  // A second board with the same serial turns ours into a port key, and its leaving turns it
  // back. The child's name moves along, so the chip and the next connect still know the board.
  const linkedKey = useRef<{ since: number; key: string } | null>(null);
  useEffect(() => {
    const previous = linkedKey.current;
    linkedKey.current = link && { since: link.since, key: link.key };
    if (!link || !previous || previous.since !== link.since || previous.key === link.key) return;
    const name = names[previous.key];
    if (!name || names[link.key]) return;
    const next = { ...names, [link.key]: name };
    saveBoardNames(next);
    setNames(next);
  }, [link, names]);

  const connect = useCallback((device: BoardDevice) => {
    setLink({ ...device, since: Date.now(), lost: false });
  }, []);

  const disconnect = useCallback(() => setLink(null), []);

  const nameBoard = useCallback((key: string, name: string) => {
    const trimmed = name.trim().slice(0, MAX_BOARD_NAME_LENGTH);
    if (!trimmed) return;
    const next = { ...names, [key]: trimmed };
    saveBoardNames(next);
    setNames(next);
  }, [names]);

  return {
    devices,
    scanning: scanning || (devices === null && failure === null),
    failure,
    link,
    names,
    rescan,
    checkBoard,
    pausePolling: setPollingPaused,
    connect,
    disconnect,
    nameBoard,
  };
}

export type BoardConnection = ReturnType<typeof useBoardConnection>;
