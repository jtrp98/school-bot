import { param } from "./debugParams";
import { EngineError } from "./engineError";

// Browser preview only: made-up answers for Block Lab's program.compile and program.upload.
// Nothing is compiled and nothing reaches a board; the delays only let the cards be seen.
// `?send=success|compile-fail|upload-fail|no-toolchain|timeout|nodevice` picks the outcome.
type Outcome = "success" | "compile-fail" | "upload-fail" | "no-toolchain" | "timeout" | "nodevice";

const OUTCOMES: readonly Outcome[] = [
  "success",
  "compile-fail",
  "upload-fail",
  "no-toolchain",
  "timeout",
  "nodevice",
];
const outcome: Outcome = OUTCOMES.find(known => known === param("send")) ?? "success";

const COMPILE_MS = 2500;
const UPLOAD_MS = 6000;

// The host accepts only these fields from the page and fixes board, platform and transport.
const FIELDS: Record<string, readonly string[]> = {
  "program.compile": ["blocks"],
  "program.upload": ["blocks", "port", "vid", "pid"],
};

// Each waiting request, with whether it is an upload: as on the host, a plain cancel lets an
// upload finish and only the child's confirmed stop ends one.
const pending = new Map<() => void, boolean>();

const wait = (ms: number, upload: boolean) =>
  new Promise<void>((resolve, reject) => {
    const cancel = () => {
      window.clearTimeout(timer);
      pending.delete(cancel);
      reject(new EngineError("ENGINE_CANCELLED", "preview: request cancelled"));
    };
    const timer = window.setTimeout(() => {
      pending.delete(cancel);
      resolve();
    }, ms);
    pending.set(cancel, upload);
  });

// Stands in for the host's Cancel (upload false) and StopUpload (upload true).
export function cancelPreviewBuilds(upload: boolean) {
  for (const [cancel, isUpload] of [...pending]) {
    if (isUpload === upload) cancel();
  }
}

function checkFields(action: string, input: unknown) {
  const allowed = FIELDS[action];
  const data = typeof input === "object" && input !== null ? (input as Record<string, unknown>) : null;
  if (!data || !Array.isArray(data.blocks)) {
    throw new EngineError("INVALID_REQUEST", "preview: data.blocks must be an array");
  }
  const extra = Object.keys(data).find(key => !allowed.includes(key));
  if (extra) throw new EngineError("INVALID_REQUEST", `preview: field ${extra} is not accepted`);
  if (action === "program.upload" && allowed.some(key => key !== "blocks" && typeof data[key] !== "string")) {
    throw new EngineError("INVALID_REQUEST", "preview: port, vid and pid must be strings");
  }
}

const result = (uploaded: boolean, output: string) => ({
  engine: "preview",
  board: "arduino_avr",
  platform: "Arduino/Genuino Uno",
  compiler: "preview",
  delivery: "preview",
  compiled: true,
  uploaded,
  output,
  simulated: true,
});

export const isPreviewBuild = (action: string) => action in FIELDS;

export async function previewBuild(action: string, input: unknown) {
  checkFields(action, input);
  if (action === "program.compile") {
    await wait(COMPILE_MS, false);
    if (outcome === "no-toolchain") {
      throw new EngineError("TOOLCHAIN_MISSING", "preview: no arduino-cli with the arduino:avr core");
    }
    if (outcome === "compile-fail") {
      throw new EngineError("COMPILE_FAILED", "preview: sketch.ino:12:3: error: expected ';' before '}' token");
    }
    return result(false, "preview: nothing was compiled");
  }
  await wait(UPLOAD_MS, true);
  if (outcome === "nodevice") throw new EngineError("TRANSPORT_ERROR", "preview: the chosen port is gone");
  if (outcome === "timeout") throw new EngineError("ENGINE_TIMEOUT", "preview: the build took longer than allowed");
  if (outcome === "upload-fail") {
    throw new EngineError("UPLOAD_FAILED", "preview: avrdude: stk500_recv(): programmer is not responding");
  }
  return result(true, "preview: nothing was sent to a board");
}
