import * as Blockly from "blockly/core";

const FREE_SPOT_MARGIN = 40;
// The spot search walks the grid and keeps 16 clear of other stacks, as in the design.
const SPOT_STEP = 24;
const CLEARANCE = 16;
// The workspace controls in the lower right: 56 wide, 16 from the edge, 16 clear of blocks.
const CONTROLS_SPACE = 16 + 56 + 16;

function lastInStack(first: Blockly.Block) {
  let last = first;
  for (let next = last.getNextBlock(); next; next = next.getNextBlock()) last = next;
  return last;
}

// The first place in view, row by row from the top, where the block keeps clear of every other
// stack and of the controls; with no room left in view, under everything.
function freeSpot(workspace: Blockly.WorkspaceSvg, block: Blockly.BlockSvg) {
  const view = workspace.getMetricsManager().getViewMetrics(true);
  const size = block.getHeightWidth();
  const others = workspace.getTopBlocks(false)
    .filter(top => top !== block)
    .map(top => top.getBoundingRectangle());
  const clear = (x: number, y: number) => others.every(other =>
    x + size.width + CLEARANCE <= other.left
    || x >= other.right + CLEARANCE
    || y + size.height + CLEARANCE <= other.top
    || y >= other.bottom + CLEARANCE);
  const right = view.left + view.width - CONTROLS_SPACE / workspace.scale - size.width;
  const bottom = view.top + view.height - FREE_SPOT_MARGIN - size.height;
  for (let y = view.top + FREE_SPOT_MARGIN; y <= bottom; y += SPOT_STEP) {
    for (let x = view.left + FREE_SPOT_MARGIN; x <= right; x += SPOT_STEP) {
      if (clear(x, y)) return new Blockly.utils.Coordinate(x, y);
    }
  }
  const lowest = Math.max(view.top, ...others.map(other => other.bottom));
  return new Blockly.utils.Coordinate(view.left + FREE_SPOT_MARGIN, lowest + FREE_SPOT_MARGIN);
}

// joined: the block went to the end of "loop" rather than into free space. group: the Blockly
// event group of the addition, so a "block added" note can tell its own events from later ones.
export type AddedBlock = { block: Blockly.BlockSvg; joined: boolean; group: string };

// The design's way to add a block without dragging (WCAG 2.5.7): a tap on a block in the list
// copies it to the end of the "loop" stack, or, when it cannot go there, into free space in view.
export function addFromList(workspace: Blockly.WorkspaceSvg, source: Blockly.BlockSvg): AddedBlock | null {
  Blockly.Events.setGroup(true);
  const group = Blockly.Events.getGroup();
  try {
    const state = Blockly.serialization.blocks.save(source, { addCoordinates: false, addNextBlocks: false });
    if (!state) return null;
    // append() leaves creation out of the undo history by default; one undo must remove the block.
    const block = Blockly.serialization.blocks.append(state, workspace, { recordUndo: true }) as Blockly.BlockSvg;
    const loop = workspace.getTopBlocks(false).find(top => top.type === "sb_loop");
    const mouth = loop?.getInput("DO")?.connection;
    const first = mouth?.targetBlock();
    const tail = first ? lastInStack(first).nextConnection : mouth;
    const joint = block.previousConnection;
    let joined = false;
    if (tail && joint && workspace.connectionChecker.canConnect(tail, joint, false)) {
      tail.connect(joint);
      joined = true;
    } else {
      block.moveTo(freeSpot(workspace, block));
    }
    block.select();
    workspace.scrollBoundsIntoView(block.getBoundingRectangleWithoutChildren());
    return { block, joined, group };
  } finally {
    Blockly.Events.setGroup(false);
  }
}
