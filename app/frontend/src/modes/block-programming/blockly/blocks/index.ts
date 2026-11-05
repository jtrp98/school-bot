import * as Blockly from "blockly/core";
// Registers Blockly's standard blocks with their mutators: Block Lab keeps its procedure
// blocks and the "if" mutator, and redefines the rest below to match the Mixly generators.
import "blockly/blocks";
import { registerBlockLabFields } from "../fieldMenu";
import { isPlacedInProgram } from "../hatFields";
import { INPUT_TYPE } from "../settingsBox";
import type { BlockJson } from "./builders";
import { CORE_BLOCKS } from "./coreBlocks";
import { DEVICE_BLOCKS } from "./deviceBlocks";

export const BLOCK_LAB_BLOCKS: readonly BlockJson[] = [...CORE_BLOCKS, ...DEVICE_BLOCKS];

// The start and loop hats: a new program starts with both, and a program holds each at most once.
export const REQUIRED_BLOCKS = ["base_setup", "sb_loop"] as const;
export const isRequiredBlock = (type: string) => (REQUIRED_BLOCKS as readonly string[]).includes(type);

let defined = false;

// The input block of a function's settings box says how to use it without dragging: a tap in
// the tray adds it, and a long press on a placed one opens "delete this input".
function explainFunctionInput() {
  const definition = Blockly.Blocks[INPUT_TYPE] as { init: (this: Blockly.Block) => void };
  const init = definition.init;
  definition.init = function () {
    init.call(this);
    this.setTooltip(() =>
      this.isInFlyout ? Blockly.Msg.SB_FUNCTION_INPUT_TRAY_TIP : Blockly.Msg.SB_FUNCTION_INPUT_PLACED_TIP,
    );
  };
}

export function defineBlockLabBlocks() {
  if (defined) return;
  defined = true;
  registerBlockLabFields();
  explainFunctionInput();
  for (const json of BLOCK_LAB_BLOCKS) {
    const required = isRequiredBlock(json.type);
    // Assigned directly because several types replace Blockly's standard definitions, which
    // defineBlocksWithJsonArray reports as an accidental overwrite.
    Blockly.Blocks[json.type] = {
      init(this: Blockly.Block) {
        this.jsonInit(json);
        if (!required) return;
        // A program holds each hat once, as a second setup() would not compile: no copy or duplicate
        // (its menu offers "duplicate" only switched off, contextMenu.ts). It can be deleted;
        // the list then offers it again.
        const hat = this as Blockly.BlockSvg;
        hat.isCopyable = () => false;
        hat.isDuplicatable = () => false;
        if (!this.workspace.isFlyout) return;
        this.contextMenu = false;
        if (!isPlacedInProgram(this)) return;
        // While the program holds this hat, the list shows it as placed and it cannot be dragged out.
        this.setMovable(false);
        this.setTooltip(() => Blockly.Msg.SB_HAT_PLACED_TIP);
        hat.addClass("sbPlaced");
      },
    };
  }
}
