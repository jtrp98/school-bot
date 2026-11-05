import { EngineError } from "../../../core/engineError";

// The two steps of sending: program.compile touches no port, program.upload writes the board.
export type SendStep = "compile" | "upload";

export type SendFailure =
  | "compile"
  | "upload"
  | "compileTimeout"
  | "timeout"
  | "toolchain"
  | "busy"
  | "engine"
  | "uploadEngine"
  | "robotPort";

export type SendState =
  | { status: "idle" }
  | { status: "noBoard"; chosen: boolean }
  | { status: "compiling" }
  | { status: "uploading"; confirmingStop: boolean }
  | { status: "stopping" }
  | { status: "cancelled"; step: SendStep }
  | { status: "sent" }
  // `output` is the tool's own text for the teacher, shown as sent and never translated.
  | { status: "failed"; failure: SendFailure; output: string };

type Outcome = Extract<SendState, { status: "noBoard" | "cancelled" | "failed" }>;

// A build is running on the host: compile, upload, or an upload being stopped.
export const isBuilding = (state: SendState) =>
  state.status === "compiling" || state.status === "uploading" || state.status === "stopping";

// What the engine answers for program.compile and program.upload (engines/mixly/docs/API.md).
export type BuildData = {
  compiled?: unknown;
  uploaded?: unknown;
  output?: unknown;
};

export const outputOf = (data: BuildData | undefined) => (typeof data?.output === "string" ? data.output : "");

const failed = (failure: SendFailure, output: string): Outcome => ({ status: "failed", failure, output });

// Codes the host and engine give a build. Any other code is shown as a failure of the step that
// was running, with the code and its text for the teacher.
export function outcomeOf(error: unknown, step: SendStep): Outcome {
  const code = error instanceof EngineError ? error.code : "";
  const output = error instanceof Error ? error.message : String(error);
  switch (code) {
    case "TOOLCHAIN_MISSING":
      return failed("toolchain", output);
    case "COMPILE_FAILED":
      return failed("compile", output);
    case "UPLOAD_FAILED":
      return failed("upload", output);
    // Compiling touches no cable, so its timeout says nothing about USB. A deadline that ends
    // an upload may stop avrdude half way, so that one says the program may be incomplete.
    case "ENGINE_TIMEOUT":
      return failed(step === "compile" ? "compileTimeout" : "timeout", output);
    // The host's one engine slot was taken, so this step never started.
    case "ENGINE_BUSY":
      return failed("busy", output);
    // The engine crashed or answered out of contract: nothing about the blocks to check. During
    // an upload it may have crashed while writing the board.
    case "ENGINE_FAILED":
      return failed(step === "upload" ? "uploadEngine" : "engine", output);
    // The host refuses to upload to a port Robot Control drove (app/internal/engine/armports.go).
    case "SAFETY_LIMIT":
      return failed("robotPort", output);
    // The port is gone, is not an UNO, or its USB IDs changed since the board was chosen.
    case "TRANSPORT_ERROR":
      return { status: "noBoard", chosen: true };
    case "ENGINE_CANCELLED":
      return { status: "cancelled", step };
    default:
      return failed(step, output);
  }
}
