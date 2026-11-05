import * as Blockly from "blockly/core";

// A function's settings box (Blockly's mutator): the tray of "input" blocks on the left and, on
// the white part, the "inputs" container that lists the function's inputs in order.
export const INPUT_TYPE = "procedures_mutatorarg";
const CONTAINER_TYPE = "procedures_mutatorcontainer";

// The box of a block, while it is open.
export function openSettingsBox(workspace: Blockly.WorkspaceSvg, blockId: string) {
  const icon = workspace.getBlockById(blockId)?.getIcon(Blockly.icons.MutatorIcon.TYPE);
  if (!(icon instanceof Blockly.icons.MutatorIcon) || !icon.bubbleIsVisible()) return null;
  return icon.getWorkspace() ?? null;
}

// Calls `onOpen` each time a settings box opens; what it returns runs when that box closes, or
// when the returned stop is called. Each box is a workspace of its own, made on opening.
export function watchSettingsBoxes(
  workspace: Blockly.WorkspaceSvg,
  onOpen: (functionId: string, box: Blockly.WorkspaceSvg) => (() => void) | void,
) {
  const closers = new Map<string, () => void>();
  const close = (functionId: string) => {
    closers.get(functionId)?.();
    closers.delete(functionId);
  };
  const onBubble = (event: Blockly.Events.Abstract) => {
    if (event.type !== Blockly.Events.BUBBLE_OPEN) return;
    const bubble = event as Blockly.Events.BubbleOpen;
    if (bubble.bubbleType !== "mutator" || !bubble.blockId) return;
    close(bubble.blockId);
    const box = bubble.isOpen ? openSettingsBox(workspace, bubble.blockId) : null;
    const closer = box ? onOpen(bubble.blockId, box) : undefined;
    if (closer) closers.set(bubble.blockId, closer);
  };
  workspace.addChangeListener(onBubble);
  return () => {
    workspace.removeChangeListener(onBubble);
    for (const functionId of [...closers.keys()]) close(functionId);
  };
}

// The box workspace an element on screen belongs to, among the boxes open now.
export function settingsBoxAt(workspace: Blockly.WorkspaceSvg, element: Element) {
  for (const block of workspace.getAllBlocks(false)) {
    const box = block.hasIcon(Blockly.icons.MutatorIcon.TYPE) ? openSettingsBox(workspace, block.id) : null;
    if (box?.getParentSvg().contains(element)) return box;
  }
  return null;
}

// The placed input (not one in the tray) an element on screen is part of.
export function placedInputAt(box: Blockly.WorkspaceSvg, element: Element) {
  const id = element.closest("[data-id]")?.getAttribute("data-id");
  const block = id ? box.getBlockById(id) : null;
  return block?.type === INPUT_TYPE ? block : null;
}

// A tap on an input in the tray adds it after the last input, as dragging it there would.
export function addInputFromTray(box: Blockly.WorkspaceSvg, source: Blockly.BlockSvg) {
  const container = box.getTopBlocks(false).find(block => block.type === CONTAINER_TYPE);
  let end = container?.getInput("STACK")?.connection ?? null;
  const state = Blockly.serialization.blocks.save(source, { addCoordinates: false });
  if (!end || !state) return;
  for (let last = end.targetBlock(); last?.nextConnection; last = last.getNextBlock()) end = last.nextConnection;
  Blockly.Events.setGroup(true);
  const added = Blockly.serialization.blocks.append(state, box, { recordUndo: true });
  if (added.previousConnection) end.connect(added.previousConnection);
  Blockly.Events.setGroup(false);
}

// "Delete this input": the inputs after it move up to close the gap.
export function deleteInput(block: Blockly.BlockSvg) {
  Blockly.Events.setGroup(true);
  block.dispose(true, true);
  Blockly.Events.setGroup(false);
}
