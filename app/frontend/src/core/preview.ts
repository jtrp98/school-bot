import type { Command, EngineResponse, ModeID } from "./types";

export function runPreview(
  mode: ModeID,
  action: string,
  input: unknown,
): EngineResponse {
  const data = input as {
    source?: string;
    commands?: Command[];
    servo?: number;
    angle?: number;
  };
  let commands = data.commands;
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
