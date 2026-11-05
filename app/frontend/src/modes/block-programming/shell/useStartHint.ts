import { useEffect, useState } from "react";
import * as Blockly from "blockly/core";
import { isRequiredBlock } from "../blockly/blocks";

// The design leaves 16 between the loop hat and the hint under it.
const HINT_GAP = 16;

export type HintPlace = { left: number; top: number; scale: number };

// Under the loop hat, in workspace pixels, while the program holds nothing but the two hats.
function readPlace(workspace: Blockly.WorkspaceSvg): HintPlace | null {
  if (workspace.isDragging()) return null;
  const blocks = workspace.getAllBlocks(false);
  const loop = blocks.find(block => block.type === "sb_loop");
  if (!loop || !blocks.every(block => isRequiredBlock(block.type))) return null;
  const box = loop.getBoundingRectangle();
  const corner = new Blockly.utils.Coordinate(box.left, box.bottom + HINT_GAP);
  const onScreen = Blockly.utils.svgMath.wsToScreenCoordinates(workspace, corner);
  const origin = workspace.getParentSvg().getBoundingClientRect();
  return { left: onScreen.x - origin.left, top: onScreen.y - origin.top, scale: workspace.scale };
}

// The hint belongs to the blocks, so it follows the workspace's scroll and zoom; it hides while
// a block is dragged because Blockly reports the new place only once the block is let go.
// listInset is the room the open list takes: Blockly moves the canvas over by it without an event.
export function useStartHint(workspace: Blockly.WorkspaceSvg | null, listInset: number) {
  const [place, setPlace] = useState<HintPlace | null>(null);

  useEffect(() => {
    if (workspace) setPlace(readPlace(workspace));
  }, [workspace, listInset]);

  useEffect(() => {
    if (!workspace) return;
    const update = () => setPlace(readPlace(workspace));
    const onChange = (event: Blockly.Events.Abstract) => {
      if (event.type === Blockly.Events.BLOCK_DRAG && (event as Blockly.Events.BlockDrag).isStart) setPlace(null);
      else update();
    };
    workspace.addChangeListener(onChange);
    const resize = new ResizeObserver(update);
    resize.observe(workspace.getParentSvg());
    update();
    return () => {
      workspace.removeChangeListener(onChange);
      resize.disconnect();
      setPlace(null);
    };
  }, [workspace]);

  return place;
}
