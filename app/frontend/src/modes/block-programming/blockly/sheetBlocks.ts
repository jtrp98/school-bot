import * as Blockly from "blockly/core";
import { BLOCK_CATEGORIES, blockStyleName } from "./categories";

// Sample blocks for the design check sheet only. Their text reuses Blockly's own message
// catalogue (%{BKY_...}) so the sheet localises without adding app strings.
const SHEET_STACK = {
  type: "sb_sheet_stack",
  message0: "%{BKY_MATH_CHANGE_TITLE}",
  args0: [
    {
      type: "field_dropdown",
      name: "TARGET",
      options: [
        ["%{BKY_LOGIC_BOOLEAN_TRUE}", "TRUE"],
        ["%{BKY_LOGIC_BOOLEAN_FALSE}", "FALSE"],
      ],
    },
    { type: "field_number", name: "VALUE", value: 10 },
  ],
  previousStatement: null,
  nextStatement: null,
  style: blockStyleName("control"),
};

const SHEET_BLOCKS = [
  {
    type: "sb_sheet_hat",
    message0: "%{BKY_PROCEDURES_DEFNORETURN_TITLE} %{BKY_PROCEDURES_DEFNORETURN_PROCEDURE}",
    nextStatement: null,
    style: blockStyleName("start", true),
  },
  {
    type: "sb_sheet_c",
    message0: "%{BKY_CONTROLS_REPEAT_TITLE}",
    args0: [{ type: "field_number", name: "TIMES", value: 10, min: 0 }],
    message1: "%{BKY_CONTROLS_REPEAT_INPUT_DO} %1",
    args1: [{ type: "input_statement", name: "DO" }],
    previousStatement: null,
    nextStatement: null,
    style: blockStyleName("control"),
  },
  SHEET_STACK,
  {
    type: "sb_sheet_value",
    message0: "%{BKY_MATH_RANDOM_INT_TITLE}",
    args0: [
      { type: "field_number", name: "FROM", value: 1 },
      { type: "field_number", name: "TO", value: 10 },
    ],
    output: "Number",
    style: blockStyleName("math"),
  },
  {
    type: "sb_sheet_boolean",
    message0: "%1 %{BKY_LOGIC_OPERATION_AND} %2",
    args0: [
      { type: "input_value", name: "A", check: "Boolean" },
      { type: "input_value", name: "B", check: "Boolean" },
    ],
    inputsInline: true,
    output: "Boolean",
    style: blockStyleName("logic"),
  },
];

type SheetState = "normal" | "disabled" | "ghost" | "warn";
const STATES: readonly SheetState[] = ["normal", "disabled", "ghost", "warn"];
const KIND_ROWS = [
  { type: "sb_sheet_hat", height: 96 },
  { type: "sb_sheet_c", height: 132 },
  { type: "sb_sheet_stack", height: 72 },
  { type: "sb_sheet_value", height: 64 },
  { type: "sb_sheet_boolean", height: 64 },
];
const COLUMN_WIDTH = 320;
const CATEGORY_ROW_HEIGHT = 64;

export function defineSheetBlocks() {
  if (!Blockly.Blocks["sb_sheet_hat"]) Blockly.defineBlocksWithJsonArray(SHEET_BLOCKS);
}

function addBlock(
  workspace: Blockly.WorkspaceSvg,
  type: string,
  x: number,
  y: number,
  prepare?: (block: Blockly.BlockSvg) => void,
) {
  const block = workspace.newBlock(type);
  prepare?.(block);
  block.initSvg();
  block.render();
  block.moveBy(x, y);
  return block;
}

// Mirrors the mockup's block sample sheet: each block kind in each state, then one block per category.
export function fillSheet(workspace: Blockly.WorkspaceSvg) {
  let y = 0;
  for (const row of KIND_ROWS) {
    STATES.forEach((state, column) => {
      const block = addBlock(workspace, row.type, column * COLUMN_WIDTH, y, created => {
        if (state === "ghost") created.setInsertionMarker(true);
      });
      if (state === "disabled") block.setDisabledReason(true, "MANUALLY_DISABLED");
      if (state === "warn") block.addClass("sb-warn");
    });
    y += row.height;
  }
  y += 32;
  BLOCK_CATEGORIES.forEach((category, index) => {
    const x = index < 8 ? 0 : 2 * COLUMN_WIDTH;
    const rowY = y + (index % 8) * CATEGORY_ROW_HEIGHT;
    addBlock(workspace, "sb_sheet_stack", x, rowY, created => {
      created.setStyle(blockStyleName(category.id));
    });
  });
}
