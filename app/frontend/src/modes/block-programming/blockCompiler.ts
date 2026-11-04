import type * as Blockly from "blockly/core";
import type { Command } from "../../core/types";

export function compile(workspace: Blockly.Workspace): Command[] {
  const commands: Command[] = [];
  let fuel = 0;
  function walk(start: Blockly.Block | null, depth: number) {
    if (++fuel > 1000)
      throw new Error("Program exceeds the block compilation budget.");
    if (depth > 8) throw new Error("Use at most 8 nested repeats.");
    for (let block = start; block; block = block.getNextBlock()) {
      if (++fuel > 1000)
        throw new Error("Program exceeds the block compilation budget.");
      if (!block.isEnabled()) continue;
      if (block.type === "arm_move") {
        const op = String(block.getFieldValue("DIRECTION"));
        const value = Number(block.getFieldValue("TILES"));
        if (
          !["move_right", "move_left"].includes(op) ||
          !Number.isInteger(value) ||
          value < 1 ||
          value > 10
        )
          throw new Error("Invalid movement");
        commands.push({ op: op as Command["op"], value });
      } else if (block.type === "arm_jump") commands.push({ op: "jump" });
      else if (block.type === "arm_repeat") {
        const times = Number(block.getFieldValue("TIMES"));
        if (!Number.isInteger(times) || times < 1 || times > 10)
          throw new Error("Repeat must be 1–10.");
        for (let i = 0; i < times; i++)
          walk(block.getInputTargetBlock("DO"), depth + 1);
      } else throw new Error("Unsupported block type.");
      if (commands.length > 64) throw new Error("Program exceeds 64 commands.");
    }
  }
  const roots = workspace.getTopBlocks(true);
  if (roots.length > 1)
    throw new Error("Connect your blocks into one sequence.");
  walk(roots[0] ?? null, 0);
  return commands;
}

