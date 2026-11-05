import * as Blockly from "blockly/core";

// What "delete all" removes, as Blockly picks it: every stack that can be deleted, and the
// blocks inside one that cannot.
export function deletableBlocks(workspace: Blockly.WorkspaceSvg) {
  const found: Blockly.BlockSvg[] = [];
  const visit = (block: Blockly.BlockSvg) => {
    if (block.isDeletable()) found.push(...block.getDescendants(false));
    else block.getChildren(false).forEach(visit);
  };
  workspace.getTopBlocks(true).forEach(visit);
  return found;
}

// Blockly counts the editable values inside a block as blocks; the menu counts what the child
// placed, as the "deleted" note does.
export const placedCount = (blocks: readonly Blockly.BlockSvg[]) => blocks.filter(block => !block.isShadow()).length;

type ConfirmDeleteAll = (blockCount: number, deleteAll: () => void) => void;
let confirmDeleteAll: ConfirmDeleteAll | null = null;

// Block Lab's own dialog asks before deleting every block, in place of the browser's confirm box
// Blockly would show. The shell sets it while the workspace is on screen.
export function setDeleteAllConfirm(confirm: ConfirmDeleteAll | null) {
  confirmDeleteAll = confirm;
}

// The workspace menu's "delete all blocks", in one step of the history.
export function deleteAllBlocks(workspace: Blockly.WorkspaceSvg) {
  workspace.cancelCurrentGesture();
  // Picked again on "delete": what the dialog counted may have changed behind it.
  const remove = () => {
    Blockly.Events.setGroup(true);
    for (const block of deletableBlocks(workspace)) {
      if (!block.isDeadOrDying()) block.dispose(false, true);
    }
    Blockly.Events.setGroup(false);
  };
  const count = placedCount(deletableBlocks(workspace));
  if (count < 2) remove();
  else confirmDeleteAll?.(count, remove);
}
