import { useCallback, useEffect, useRef, useState } from "react";
import type * as Blockly from "blockly/core";
import type { BoardLink } from "../connect/boardDevices";
import type { BoardConnection } from "../connect/useBoardConnection";
import { toEngineProgram } from "../blockly/engineProgram";
import { runBlockEngine } from "./blockEngine";
import { isBuilding, outcomeOf, outputOf, type BuildData, type SendState, type SendStep } from "./sendOutcome";
import type { BoardSerial } from "./useBoardSerial";
import type { Preflight } from "./usePreflight";
import type { UnsavedReason } from "./useBlockProject";

// What the child tried to do while a send was running: start over, leave, close the app, or
// open the board screen.
export type SendGuardReason = UnsavedReason | "board";

type Job = { abort: AbortController; step: SendStep };
type Program = ReturnType<typeof toEngineProgram>;

const IDLE: SendState = { status: "idle" };
// "Cancelled, nothing was sent" is a short note that closes by itself.
const CANCELLED_NOTE_MS = 4000;

// "Send to board": the check before sending, a fresh look for the chosen board, then compile
// (no port) and upload (writes the board). Only an upload the engine reports as done counts as
// sent. A running upload is never cancelled by leaving, a new job or closing the window: those
// wait behind the guard dialog, and only the child's confirmed stop cancels it.
export function useSendToBoard(
  workspace: Blockly.WorkspaceSvg | null,
  board: BoardConnection,
  preflight: Preflight,
  serial: Pick<BoardSerial, "pause" | "resume">,
) {
  const [state, setState] = useState<SendState>(IDLE);
  const [guard, setGuard] = useState<SendGuardReason | null>(null);
  const job = useRef<Job | null>(null);
  const pressed = useRef(false);
  const alive = useRef(true);
  const { checkBoard, pausePolling } = board;
  const { check } = preflight;
  const { pause: pauseSerial, resume: resumeSerial } = serial;

  useEffect(() => {
    alive.current = true;
    return () => {
      alive.current = false;
    };
  }, []);

  const show = useCallback((next: SendState) => {
    if (alive.current) setState(next);
  }, []);

  const busy = isBuilding(state);
  useEffect(() => pausePolling(busy), [busy, pausePolling]);

  useEffect(() => {
    if (state.status !== "cancelled" || state.step !== "compile") return;
    const timer = window.setTimeout(() => setState(IDLE), CANCELLED_NOTE_MS);
    return () => window.clearTimeout(timer);
  }, [state]);

  // Answers whether the board now holds the program.
  const build = useCallback(async (program: Program, target: BoardLink) => {
    const current: Job = { abort: new AbortController(), step: "compile" };
    const { signal } = current.abort;
    job.current = current;
    show({ status: "compiling" });
    try {
      const compiled = await runBlockEngine<BuildData>("program.compile", program, signal);
      // A cancel while compiling has already said that nothing was sent.
      if (signal.aborted) return false;
      if (compiled.data?.compiled !== true) {
        show({ status: "failed", failure: "compile", output: outputOf(compiled.data) });
        return false;
      }
      current.step = "upload";
      show({ status: "uploading", confirmingStop: false });
      const upload = { ...program, port: target.port, vid: target.vid, pid: target.pid };
      const sent = await runBlockEngine<BuildData>("program.upload", upload, signal);
      if (sent.data?.uploaded === true) {
        show({ status: "sent" });
        return true;
      }
      if (signal.aborted) show({ status: "cancelled", step: "upload" });
      else show({ status: "failed", failure: "upload", output: outputOf(sent.data) });
      return false;
    } catch (error) {
      if (current.step === "compile" && signal.aborted) return false;
      // A stopped upload may end in any error; the child asked for the stop, so it says stopped.
      show(signal.aborted ? { status: "cancelled", step: current.step } : outcomeOf(error, current.step));
      return false;
    } finally {
      job.current = null;
    }
  }, [show]);

  const start = useCallback(async () => {
    if (!workspace || pressed.current) return;
    pressed.current = true;
    try {
      show(IDLE);
      if (!(await check())) return;
      const found = await checkBoard();
      const target = found.kind === "ready" ? found.board : null;
      // The host uploads only to the board with these USB IDs, so a board without them is not sent to.
      if (!target?.vid || !target.pid) {
        show({ status: "noBoard", chosen: found.kind === "missing" || found.kind === "ready" });
        return;
      }
      // avrdude needs the port the Serial monitor holds. Reading goes on after a send that worked,
      // so the child sees what the new program prints, but only on that same board, looked up
      // again: during the send the list was paused, and a clone's bridge in that socket could now
      // be another device, such as the robot arm's controller.
      const wasReading = await pauseSerial();
      let reopenOn: BoardLink | null = null;
      try {
        if ((await build(toEngineProgram(workspace), target)) && wasReading) {
          const again = await checkBoard();
          const same = again.kind === "ready" && again.board.key === target.key && again.board.port === target.port;
          if (same) reopenOn = again.board;
        }
      } finally {
        resumeSerial(reopenOn ?? target, reopenOn !== null);
      }
    } finally {
      pressed.current = false;
    }
  }, [workspace, check, checkBoard, build, show, pauseSerial, resumeSerial]);

  // Compiling touches no port, so it stops at once. Stopping an upload asks first.
  const cancel = useCallback(() => {
    const current = job.current;
    if (!current || current.abort.signal.aborted) return;
    if (current.step === "upload") {
      setState(now => (now.status === "uploading" ? { status: "uploading", confirmingStop: true } : now));
      return;
    }
    current.abort.abort();
    show({ status: "cancelled", step: "compile" });
  }, [show]);

  const keepSending = useCallback(() => {
    setState(now => (now.status === "uploading" ? { status: "uploading", confirmingStop: false } : now));
  }, []);

  const stopSending = useCallback(() => {
    const current = job.current;
    if (!current || current.step !== "upload" || current.abort.signal.aborted) return;
    current.abort.abort();
    show({ status: "stopping" });
  }, [show]);

  const close = useCallback(() => {
    setState(now => (isBuilding(now) ? now : IDLE));
  }, []);

  // Asked by every way out of the job. From the check before sending until the send ends (a
  // cancelled compile has nothing left to protect) it shows the guard dialog and answers true,
  // so the caller does not go ahead. It already holds before the upload starts: a window closed
  // during the check would otherwise close in the gap before the upload's own close guard.
  const holdBack = useCallback((reason: SendGuardReason) => {
    const current = job.current;
    if (!pressed.current || (current?.step === "compile" && current.abort.signal.aborted)) return false;
    setGuard(reason);
    return true;
  }, []);

  const closeGuard = useCallback(() => setGuard(null), []);

  return {
    state,
    busy,
    guard,
    start,
    cancel,
    keepSending,
    stopSending,
    close,
    holdBack,
    closeGuard,
  };
}

export type SendToBoard = ReturnType<typeof useSendToBoard>;
