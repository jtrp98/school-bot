import { previewDeviceList } from "./previewBoards";
import { cancelPreviewBuilds, isPreviewBuild, previewBuild } from "./previewSend";
import { closePreviewSerial, isPreviewSerial, previewSerial } from "./previewSerial";
import type { Command, EngineResponse, ModeID } from "./types";

async function runPreviewBuild(action: string, input: unknown): Promise<EngineResponse<unknown>> {
  // Simulated compile/upload answers; the browser preview has no toolchain and no board.
  return {
    protocol: "1.0",
    request_id: crypto.randomUUID(),
    status: "success",
    data: await previewBuild(action, input),
  };
}

// The preview's stand-in for the host's cancel, which also ends the Serial session.
export function cancelPreview() {
  cancelPreviewBuilds(false);
  closePreviewSerial();
}

export function stopPreviewUpload() {
  cancelPreviewBuilds(true);
}

export function runPreview(
  mode: ModeID,
  action: string,
  input: unknown,
): EngineResponse<unknown> | Promise<EngineResponse<unknown>> {
  if (mode === "block-programming" && isPreviewBuild(action)) return runPreviewBuild(action, input);
  if (mode === "block-programming" && isPreviewSerial(action)) {
    // Simulated Serial monitor; the browser preview has no board.
    return { protocol: "1.0", request_id: crypto.randomUUID(), status: "success", data: previewSerial(action, input) };
  }
  if (mode === "block-programming" && action === "device.list") {
    // Simulated sample ports; the browser preview has no engine and no serial access.
    return {
      protocol: "1.0",
      request_id: crypto.randomUUID(),
      status: "success",
      data: previewDeviceList(),
    };
  }
  const data = input as {
    source?: string;
    servo?: number;
    angle?: number;
  };
  let commands: Command[] | undefined;
  if (mode === "coding-adventure") {
    commands = [];
    for (const [index, line] of (data.source ?? "").split("\n").entries()) {
      const text = line.trim();
      if (!text || text.startsWith("#")) continue;
      const match = /^hero\.(move_right|move_left|jump)\(([0-9]*)\)$/.exec(
        text,
      );
      if (!match || (match[1] === "jump" ? match[2] !== "" : match[2] === ""))
        throw new Error(
          `Line ${index + 1}: use the supported hero commands. This is not Python.`,
        );
      commands.push({
        op: match[1] as Command["op"],
        ...(match[1] === "jump" ? {} : { value: Number(match[2]) }),
      });
    }
  }
  if (commands) {
    let steps = 0;
    if (!commands.length || commands.length > 64)
      throw new Error("Use 1–64 commands.");
    for (const command of commands) {
      if (!["move_right", "move_left", "jump"].includes(command.op))
        throw new Error("Unsupported command");
      if (
        command.op !== "jump" &&
        (!Number.isInteger(command.value) ||
          command.value! < 1 ||
          command.value! > 10)
      )
        throw new Error("Movement must be 1–10 tiles.");
      steps += command.op === "jump" ? 1 : command.value!;
    }
    if (steps > 100) throw new Error("Program exceeds 100 steps.");
    return {
      protocol: "1.0",
      request_id: crypto.randomUUID(),
      status: "success",
      data: { commands, steps, simulated: true },
    };
  }
  if (mode !== "robot-control" || !["servo.set", "robot.stop"].includes(action))
    throw new Error("Unknown mode/action");
  if (
    action === "servo.set" &&
    (!Number.isInteger(data.servo) ||
      data.servo! < 1 ||
      data.servo! > 4 ||
      !Number.isInteger(data.angle) ||
      data.angle! < 0 ||
      data.angle! > 180)
  )
    throw new Error("Servo safety limit exceeded");
  return {
    protocol: "1.0",
    request_id: crypto.randomUUID(),
    status: "success",
    data: { simulated: true },
  };
}
