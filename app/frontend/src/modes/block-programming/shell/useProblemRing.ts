import { useEffect, useState } from "react";
import * as Blockly from "blockly/core";

export type RingBox = { left: number; top: number; width: number; height: number };

// Where a block is inside the workspace's box, in screen pixels: the block alone, without the
// blocks joined below it.
function measure(workspace: Blockly.WorkspaceSvg, block: Blockly.BlockSvg): RingBox {
  const origin = workspace.getInjectionDiv().getBoundingClientRect();
  const corner = Blockly.utils.svgMath.wsToScreenCoordinates(workspace, block.getRelativeToSurfaceXY());
  // getHeightWidth() would count the stack below too; width and height are the block's own.
  return {
    left: corner.x - origin.left,
    top: corner.y - origin.top,
    width: block.width * workspace.scale,
    height: block.height * workspace.scale,
  };
}

// The block the check before sending pointed at with "show me" stays ringed until the child
// changes anything in the workspace; the ring follows scrolling and zooming.
export function useProblemRing(workspace: Blockly.WorkspaceSvg | null, blockId: string | null, onDone: () => void) {
  const [box, setBox] = useState<RingBox | null>(null);

  useEffect(() => {
    const block = blockId ? workspace?.getBlockById(blockId) : null;
    if (!workspace || !block) {
      setBox(null);
      return;
    }
    setBox(measure(workspace, block));
    const onChange = (event: Blockly.Events.Abstract) => {
      if (!event.isUiEvent) onDone();
      else if (event.type === Blockly.Events.VIEWPORT_CHANGE) setBox(measure(workspace, block));
    };
    workspace.addChangeListener(onChange);
    return () => workspace.removeChangeListener(onChange);
  }, [workspace, blockId, onDone]);

  return box;
}
