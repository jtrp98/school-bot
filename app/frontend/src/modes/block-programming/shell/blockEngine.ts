import { client } from "../../../core/client";
import { EngineError } from "../../../core/engineError";
import type { EngineResponse } from "../../../core/types";
import { registerWindowCloseGuard } from "../../../core/windowLifecycle";

type BlockEngineAction =
  | "program.generate"
  | "project.validate"
  | "device.list"
  | "program.compile"
  | "program.upload"
  | "serial.open"
  | "serial.read"
  | "serial.write"
  | "serial.close";

let queue: Promise<unknown> = Promise.resolve();
let generation = 0;
let leaving = false;

// The host runs one engine request at a time and answers a second with ENGINE_BUSY, so Block
// Lab's own calls (the C++ tab's code, the check before sending, the board list, compile and
// upload) wait for each other instead.
// A call made with a signal can be cancelled: while it waits it is dropped, and while it runs
// the host request is cancelled. The host cancels whatever runs, so cancelling only while this
// call is the one running keeps it from stopping someone else's request.
export function runBlockEngine<TData>(
  action: BlockEngineAction,
  data: unknown,
  signal?: AbortSignal,
): Promise<EngineResponse<TData>> {
  const queuedIn = generation;
  const next = queue.then(async () => {
    if (leaving || queuedIn !== generation) {
      throw new EngineError("ENGINE_CANCELLED", "Block Lab closed before the request ran");
    }
    if (signal?.aborted) throw new EngineError("ENGINE_CANCELLED", "cancelled before the request ran");
    const running = runOnHost<TData>(action, data, signal);
    if (action !== "program.upload") return running;
    // Half an upload leaves the board without a whole program, so the host's cancel lets an upload
    // finish. Block Lab asks about closing itself, but its question goes with its page if the page
    // crashes, so the window also waits here and says why; the host ends an upload by its deadline.
    const release = registerWindowCloseGuard(() => running.then(ignore, ignore), "blockLab.send.guard.closeWait");
    try {
      return await running;
    } finally {
      release();
    }
  });
  queue = next.catch(() => undefined);
  return next;
}

const ignore = () => undefined;

async function runOnHost<TData>(action: BlockEngineAction, data: unknown, signal?: AbortSignal) {
  if (!signal) return client.run<TData>("block-programming", action, data);
  // The host's plain cancel lets an upload finish; an upload stops only through stopUpload.
  const cancelHost = () => {
    const stop = action === "program.upload" ? client.stopUpload() : client.cancel();
    stop.catch(error => console.warn("Unable to cancel the Block Lab request", error));
  };
  signal.addEventListener("abort", cancelHost, { once: true });
  try {
    return await client.run<TData>("block-programming", action, data);
  } finally {
    signal.removeEventListener("abort", cancelHost);
  }
}

export function openBlockEngine() {
  leaving = false;
}

// The shell's leaveMode cancels the running request and only then unmounts Block Lab. Calls
// still waiting, and any a timer adds in between, are refused from here on, so none takes the
// host's single slot after the child moves on, where a robot stop may need it.
export function leaveBlockEngine() {
  generation++;
  leaving = true;
}

// A window close can still fail and keep Block Lab open, so it drops only the calls queued now.
export function dropQueuedBlockEngineCalls() {
  generation++;
}
