import * as Blockly from "blockly/core";

// The block JSON the mixly engine generates code from (engines/mixly/docs/API.md, "Program data").
export type EngineBlock = {
  type: string;
  fields?: Record<string, string>;
  values?: Record<string, EngineBlock>;
  statements?: Record<string, EngineBlock>;
  next?: EngineBlock;
  mutation?: Record<string, string>;
  arguments?: { name: string }[];
};

// Fields the Mixly generators read but Block Lab does not show: variables are program-wide, the
// UNO has a single Serial port, and functions return a whole number like Mixly's default
// parameter type.
const FIXED_FIELDS: Record<string, Record<string, string>> = {
  variables_declare: { variables_type: "global_variate" },
  procedures_defreturn: { TYPE: "long" },
  serial_print: { serial_select: "Serial" },
  serial_readstr: { serial_select: "Serial" },
  serial_available: { serial_select: "Serial" },
};

type IfBlock = Blockly.Block & { elseifCount_?: number; elseCount_?: number };
// Blockly's procedure blocks keep their parameter names here, as Mixly's generators expect.
type ProcedureBlock = Blockly.Block & { arguments_?: string[] };

function fieldText(field: Blockly.Field) {
  // Variable fields store an ID; the generators use the variable's name.
  if (field instanceof Blockly.FieldVariable) return field.getVariable()?.getName() ?? "";
  return String(field.getValue() ?? "");
}

function mutationOf(block: Blockly.Block): Record<string, string> | undefined {
  if (block.type === "controls_if") {
    const branches = block as IfBlock;
    return { elseif: String(branches.elseifCount_ ?? 0), else: String(branches.elseCount_ ?? 0) };
  }
  if (block.type === "procedures_return") return { value: block.getInputTargetBlock("VALUE") ? "1" : "0" };
  return undefined;
}

// Blocks the child switched off are left out, and the stack carries on after them.
function firstEnabled(block: Blockly.Block | null): Blockly.Block | null {
  let current = block;
  while (current && !current.isEnabled()) current = current.getNextBlock();
  return current;
}

export function toEngineBlock(block: Blockly.Block): EngineBlock {
  const node: EngineBlock = { type: block.type };
  const fields: Record<string, string> = { ...FIXED_FIELDS[block.type] };
  const values: Record<string, EngineBlock> = {};
  const statements: Record<string, EngineBlock> = {};
  for (const input of block.inputList) {
    for (const field of input.fieldRow) {
      if (field.name) fields[field.name] = fieldText(field);
    }
    const child = firstEnabled(input.connection?.targetBlock() ?? null);
    if (!child) continue;
    if (input.type === Blockly.inputs.inputTypes.STATEMENT) statements[input.name] = toEngineBlock(child);
    else values[input.name] = toEngineBlock(child);
  }
  if (Object.keys(fields).length) node.fields = fields;
  if (Object.keys(values).length) node.values = values;
  if (Object.keys(statements).length) node.statements = statements;
  const mutation = mutationOf(block);
  if (mutation) node.mutation = mutation;
  const parameters = (block as ProcedureBlock).arguments_;
  if (block.type.startsWith("procedures_") && parameters?.length) {
    node.arguments = parameters.map(name => ({ name }));
  }
  const next = firstEnabled(block.getNextBlock());
  if (next) node.next = toEngineBlock(next);
  return node;
}

// The program is every enabled stack that starts with a hat (start, loop, function definitions);
// loose blocks are disabled by the workspace and do not run.
export function toEngineProgram(workspace: Blockly.Workspace): { blocks: EngineBlock[] } {
  const roots = workspace
    .getTopBlocks(true)
    .filter(block => block.isEnabled() && !block.previousConnection && !block.outputConnection);
  return { blocks: roots.map(toEngineBlock) };
}
