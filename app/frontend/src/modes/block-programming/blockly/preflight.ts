import * as Blockly from "blockly/core";

// What the check before sending finds, in the order the list shows it: a missing value, a block
// that is not part of the program, then wiring the UNO cannot run (from the mixly engine).
export type PreflightProblem =
  | { kind: "emptySlot"; blockId: string; blockName: string }
  | { kind: "loose"; blockId: string; blockName: string }
  | { kind: "pinShared"; blockId?: string; pin: string; uses: string[] }
  | { kind: "timer2"; blockId?: string }
  | { kind: "engine"; detail?: string };

// project.validate's answer (engines/mixly/docs/API.md, "School Bot blocks").
export type EngineProblem = { code: string; pin?: string; uses?: string[] };

// A function's "return" may stay empty: that returns nothing.
const OPTIONAL_VALUES: Record<string, string[]> = { procedures_return: ["VALUE"] };
const PIN_FIELDS = ["PIN", "PIN_A", "PIN_B", "TRIG", "ECHO"];
// The blocks that take Timer2 in the engine (school-bot-devices.js, school-bot-modules.js).
const TIMER2_TYPES = ["sb_buzzer_tone", "sb_buzzer_tone_for", "sb_buzzer_stop", "sb_ir_received"];
const NAME_LENGTH = 30;

// The words on the block's first row ("if", "LED pin 2"), which is how a child names it.
function blockName(block: Blockly.Block) {
  const words = (block.inputList[0]?.fieldRow ?? [])
    .filter(field => !(field instanceof Blockly.FieldImage))
    .map(field => field.getText().trim())
    .filter(Boolean);
  return words.length ? words.join(" ") : block.toString(NAME_LENGTH);
}

// The stacks that run, as toEngineProgram sends them: hats and function definitions.
function programRoots(workspace: Blockly.Workspace) {
  return workspace
    .getTopBlocks(true)
    .filter(block => block.isEnabled() && !block.previousConnection && !block.outputConnection);
}

// Every block that runs, parents first. A switched-off block and what it holds are left out,
// and the stack carries on after it, as in the code.
function runningBlocks(workspace: Blockly.Workspace) {
  const found: Blockly.Block[] = [];
  const visit = (block: Blockly.Block | null) => {
    for (let current = block; current; current = current.getNextBlock()) {
      if (!current.isEnabled()) continue;
      found.push(current);
      for (const input of current.inputList) visit(input.connection?.targetBlock() ?? null);
    }
  };
  for (const root of programRoots(workspace)) visit(root);
  return found;
}

function hasEmptySlot(block: Blockly.Block) {
  return block.inputList.some(input => {
    if (input.type !== Blockly.inputs.inputTypes.VALUE || !input.connection) return false;
    if (OPTIONAL_VALUES[block.type]?.includes(input.name)) return false;
    // A switched-off value is left out of the code, so its slot is as empty as a bare one.
    return !input.connection.targetBlock()?.isEnabled();
  });
}

// Blocks lying on their own. One the child switched off is parked on purpose and is not listed.
function looseBlocks(workspace: Blockly.Workspace) {
  return workspace
    .getTopBlocks(true)
    .filter(block => block.previousConnection || block.outputConnection)
    .filter(block => !block.hasDisabledReason(Blockly.constants.MANUALLY_DISABLED));
}

export function findBlockProblems(workspace: Blockly.Workspace): PreflightProblem[] {
  const empty = runningBlocks(workspace)
    .filter(hasEmptySlot)
    .map(block => ({ kind: "emptySlot" as const, blockId: block.id, blockName: blockName(block) }));
  const loose = looseBlocks(workspace)
    .map(block => ({ kind: "loose" as const, blockId: block.id, blockName: blockName(block) }));
  return [...empty, ...loose];
}

// The engine names a pin or a timer, not a block; "show me" goes to the first block that uses it.
export function fromEngineProblems(workspace: Blockly.Workspace, problems: EngineProblem[]): PreflightProblem[] {
  const blocks = runningBlocks(workspace);
  return problems.map((problem): PreflightProblem => {
    if (problem.code === "PIN_SHARED" && problem.pin) {
      const pin = problem.pin;
      const user = blocks.find(block => PIN_FIELDS.some(name => block.getField(name)?.getValue() === pin));
      return { kind: "pinShared", blockId: user?.id, pin, uses: problem.uses ?? [] };
    }
    if (problem.code === "TIMER2_SHARED") {
      return { kind: "timer2", blockId: blocks.find(block => TIMER2_TYPES.includes(block.type))?.id };
    }
    return { kind: "engine", detail: problem.code };
  });
}

export function problemBlockId(problem: PreflightProblem) {
  return problem.kind === "engine" ? undefined : problem.blockId;
}
