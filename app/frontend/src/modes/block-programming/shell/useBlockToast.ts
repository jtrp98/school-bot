import { useCallback, useEffect, useRef, useState } from "react";
import { useTranslation } from "react-i18next";
import * as Blockly from "blockly/core";
import type { AddedBlock } from "../blockly/addFromList";
import { watchSettingsBoxes } from "../blockly/settingsBox";

// The design keeps a note up for 9 seconds unless another change replaces it.
const TOAST_MS = 9000;
const BLOCK_TEXT_LENGTH = 40;

// group (or, for an ungrouped change, anchor) names the change the note is about: the note
// stays only while that change is the one "undo" would reverse next.
// reopenBox: the function whose settings box an input was deleted from; undo opens it again to
// show the input coming back, since restoring the function closes the box.
type Toast = {
  text: string;
  canUndo: boolean;
  group: string;
  anchor?: Blockly.Events.Abstract;
  deleted?: number;
  reopenBox?: string;
};
type BlockState = Blockly.serialization.blocks.State;

function isLatest(workspace: Blockly.WorkspaceSvg, note: Toast) {
  const stack = workspace.getUndoStack();
  const latest = stack[stack.length - 1];
  return note.group ? latest?.group === note.group : latest === note.anchor;
}

// Shadows are the editable values inside a block, not blocks the child placed.
function countBlocks(state: BlockState | undefined): number {
  if (!state) return 0;
  const children = Object.values(state.inputs ?? {}).map(input => countBlocks(input.block));
  return 1 + countBlocks(state.next?.block) + children.reduce((sum, count) => sum + count, 0);
}

// A block taken from a list (or a settings box tray) and dropped back on it is created and
// deleted in one drag: a cancelled drag, not a deletion to report.
function createdInGroup() {
  let created = { group: "", ids: new Set<string>() };
  return {
    remember(creation: Blockly.Events.BlockCreate) {
      if (creation.group !== created.group) created = { group: creation.group, ids: new Set() };
      for (const id of creation.ids ?? []) created.ids.add(id);
    },
    cancelled(deletion: Blockly.Events.BlockDelete) {
      return deletion.group === created.group && created.ids.has(deletion.blockId ?? "");
    },
  };
}

// The note in the workspace's lower left after a block is added from the list or deleted,
// with "undo" for that one change.
export function useBlockToast(workspace: Blockly.WorkspaceSvg | null) {
  const { t } = useTranslation();
  const [toast, setToast] = useState<Toast | null>(null);
  const shown = useRef<Toast | null>(null);
  const timer = useRef<number | undefined>(undefined);

  const place = useCallback((next: Toast | null) => {
    window.clearTimeout(timer.current);
    shown.current = next;
    setToast(next);
    if (!next) return;
    timer.current = window.setTimeout(() => {
      shown.current = null;
      setToast(null);
    }, TOAST_MS);
  }, []);

  useEffect(() => () => window.clearTimeout(timer.current), []);

  // An input deleted in a settings box, waiting for the change of its function block that
  // follows in the workspace: the note belongs to that step of the history.
  const boxDeletion = useRef<{ functionId: string; blockCount: number } | null>(null);

  useEffect(() => {
    if (!workspace) return;
    const created = createdInGroup();
    // Loading a project or starting a new one is not undoable (load records no undo, "new" clears
    // the undo history right after clear()), so only a child's own deletions are reported.
    const isReported = (deletion: Blockly.Events.BlockDelete) => deletion.recordUndo && !created.cancelled(deletion);
    // One "delete" can remove several stacks, each with its own event in the same group.
    const report = (deletion: Blockly.Events.BlockDelete) => {
      const before = shown.current;
      const earlier = before?.group && before.group === deletion.group ? before.deleted ?? 0 : 0;
      const deleted = earlier + countBlocks(deletion.oldJson);
      const text = t("blockLab.toast.deleted", { blockCount: deleted });
      place({ text, canUndo: true, group: deletion.group, anchor: deletion, deleted });
    };
    const reportBoxDeletion = (event: Blockly.Events.Abstract) => {
      const pending = boxDeletion.current;
      boxDeletion.current = null;
      const change = event as Blockly.Events.BlockChange;
      if (!pending || event.type !== Blockly.Events.BLOCK_CHANGE || change.blockId !== pending.functionId) return false;
      const text = t("blockLab.toast.deleted", { blockCount: pending.blockCount });
      place({ text, canUndo: true, group: event.group, anchor: event, reopenBox: pending.functionId });
      return true;
    };
    const onChange = (event: Blockly.Events.Abstract) => {
      if (event.isUiEvent) return;
      if (reportBoxDeletion(event)) return;
      if (event.type === Blockly.Events.BLOCK_CREATE) created.remember(event as Blockly.Events.BlockCreate);
      if (event.type === Blockly.Events.BLOCK_DELETE && isReported(event as Blockly.Events.BlockDelete)) {
        report(event as Blockly.Events.BlockDelete);
        return;
      }
      const note = shown.current;
      // After any other change, an undo or redo included, the note's undo would reverse the wrong step.
      if (note && !isLatest(workspace, note)) place(null);
    };
    workspace.addChangeListener(onChange);
    return () => {
      workspace.removeChangeListener(onChange);
      place(null);
    };
  }, [workspace, place, t]);

  // Deleting a function input happens in its settings box, a workspace of its own; in the main
  // history it is a change of the function block, which the note's undo reverses.
  useEffect(() => {
    if (!workspace) return;
    return watchSettingsBoxes(workspace, (functionId, box) => {
      const created = createdInGroup();
      const onBoxChange = (event: Blockly.Events.Abstract) => {
        if (event.type === Blockly.Events.BLOCK_CREATE) created.remember(event as Blockly.Events.BlockCreate);
        if (event.type !== Blockly.Events.BLOCK_DELETE) return;
        const deletion = event as Blockly.Events.BlockDelete;
        if (!deletion.recordUndo || created.cancelled(deletion)) return;
        boxDeletion.current = { functionId, blockCount: countBlocks(deletion.oldJson) };
      };
      box.addChangeListener(onBoxChange);
      return () => box.removeChangeListener(onBoxChange);
    });
  }, [workspace]);

  const added = useCallback((result: AddedBlock) => {
    const blockText = result.block.toString(BLOCK_TEXT_LENGTH, "…");
    const key = result.joined ? "blockLab.toast.addedToLoop" : "blockLab.toast.addedFree";
    place({ text: t(key, { blockText }), canUndo: true, group: result.group });
  }, [place, t]);

  const onlyOne = useCallback((type: string) => {
    if (!workspace) return;
    const text = t("blockLab.toast.onlyOne", { blockName: t(`blockLab.blocks.${type}`) });
    const stack = workspace.getUndoStack();
    place({ text, canUndo: false, group: "", anchor: stack[stack.length - 1] });
  }, [place, t, workspace]);

  const undo = useCallback(() => {
    const reopenBox = shown.current?.reopenBox;
    place(null);
    workspace?.undo(false);
    if (!reopenBox) return;
    // Blockly closes the box while it applies the undo, after this returns.
    window.setTimeout(() => {
      const icon = workspace?.getBlockById(reopenBox)?.getIcon(Blockly.icons.MutatorIcon.TYPE);
      if (icon instanceof Blockly.icons.MutatorIcon) void icon.setBubbleVisible(true);
    });
  }, [place, workspace]);

  return { toast, added, onlyOne, undo };
}

export type BlockToastState = ReturnType<typeof useBlockToast>;
