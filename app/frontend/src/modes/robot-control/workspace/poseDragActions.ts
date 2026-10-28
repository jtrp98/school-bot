import type { DragEvent, MouseEvent } from "react";
import type { WorkspaceLogic } from "./WorkspaceLogic";
import { deriveListFlags, groupOfKey, type PoseGroup, type PoseListState } from "./poseListState";

type Axis = "x" | "y";

// Handlers read the screen state when they run, so one instance serves every render without stale closures.
export function createPoseDragActions(screen: WorkspaceLogic) {
  const state = (): PoseListState => screen.state;
  const set = (patch: Record<string, unknown>) => screen.setState(patch);

  const dropHere = () => {
    const { dragFrom, dropGap } = state();
    set({ dragFrom: -1, dropGap: -1 });
    if (dragFrom >= 0 && dropGap >= 0) screen.moveTo(dragFrom, dropGap);
  };

  // Dragging only works through state, so a WebView that rejects dataTransfer writes still reorders.
  const startTransfer = (index: number, event: DragEvent<HTMLElement>) => {
    try {
      event.dataTransfer.effectAllowed = "move";
      event.dataTransfer.setData("text/plain", String(index));
    } catch {
      // The drag state below is enough to reorder when the WebView rejects the transfer data.
    }
  };

  const gapAt = (index: number, event: DragEvent<HTMLElement>, axis: Axis): number => {
    const rect = event.currentTarget.getBoundingClientRect();
    const pointer = axis === "x" ? event.clientX : event.clientY;
    const midpoint = axis === "x" ? rect.left + rect.width / 2 : rect.top + rect.height / 2;
    return pointer > midpoint ? index + 1 : index;
  };

  const trackGap = (index: number, event: DragEvent<HTMLElement>, axis: Axis) => {
    const current = state();
    const gap = gapAt(index, event, axis);
    const allowed = gap !== current.dragFrom && gap !== current.dragFrom + 1;
    const next = allowed ? gap : -1;
    if (next !== current.dropGap) set({ dropGap: next });
  };

  const overWhileDragging = (index: number, event: DragEvent<HTMLElement>, axis: Axis, group: PoseGroup | null) => {
    if (!deriveListFlags(state()).dragOn) return;
    event.preventDefault();
    if (state().selMode) {
      screen.over6(event, index, group, axis);
      return;
    }
    trackGap(index, event, axis);
  };

  const dropOnList = (event: DragEvent<HTMLElement>) => {
    if (!deriveListFlags(state()).isOrder) return;
    event.preventDefault();
    if (state().selMode) {
      screen.drop6();
      return;
    }
    dropHere();
  };

  const collapsedHeadOf = (index: number): PoseGroup | null => {
    const current = state();
    const key = current.seq[index];
    const group = groupOfKey(current, key);
    const collapsed = !!group && !group.open && (!deriveListFlags(current).isOrder || current.selMode);
    return group && collapsed && group.keys[0] === key ? group : null;
  };

  return {
    startTileDrag: (index: number, event: DragEvent<HTMLElement>) => {
      if (!deriveListFlags(state()).isOrder) return;
      startTransfer(index, event);
      const head = state().selMode ? collapsedHeadOf(index) : null;
      set({ dragFrom: index, dropGap: -1, movePick: -1, dragG6: head ? head.id : null, dropInto6: null });
    },
    overTile: (index: number, event: DragEvent<HTMLElement>) => {
      overWhileDragging(index, event, "x", collapsedHeadOf(index));
    },
    dropOnTile: dropOnList,
    startRowDrag: (index: number, event: DragEvent<HTMLElement>) => {
      if (!deriveListFlags(state()).isOrder) return;
      startTransfer(index, event);
      set({ dragFrom: index, dropGap: -1, movePick: -1, dragG6: null, dropInto6: null });
    },
    overRow: (index: number, event: DragEvent<HTMLElement>) => overWhileDragging(index, event, "y", null),
    dropOnRow: dropOnList,
    startGroupRowDrag: (index: number, groupId: string, event: DragEvent<HTMLElement>) => {
      const current = state();
      const { isOrder } = deriveListFlags(current);
      const group = current.groups.find((candidate) => candidate.id === groupId);
      if (!isOrder || !current.selMode || !group) return;
      startTransfer(index, event);
      set({ dragFrom: index, dropGap: -1, movePick: -1, dragG6: group.id, dropInto6: null });
    },
    overGroupRow: (index: number, groupId: string, event: DragEvent<HTMLElement>) => {
      const current = state();
      if (!deriveListFlags(current).dragOn || !current.selMode) return;
      event.preventDefault();
      screen.over6(event, index, current.groups.find((candidate) => candidate.id === groupId) ?? null, "y");
    },
    dropOnGroupRow: (event: DragEvent<HTMLElement>) => {
      if (!deriveListFlags(state()).isOrder) return;
      event.preventDefault();
      screen.drop6();
    },
    endDrag: () => set({ dragFrom: -1, dropGap: -1, dragG6: null, dropInto6: null }),
    // The gap before a pose accepts a drop while pasting, or while moving or dragging a pose.
    pasteOrMoveBefore: (index: number) => {
      const current = state();
      if (deriveListFlags(current).pasteMode) screen.pPaste(index);
      else screen.moveTo(current.movePick, index);
    },
    allowDropInGap: (event: DragEvent<HTMLElement>) => {
      if (deriveListFlags(state()).dragOn) event.preventDefault();
    },
    dropInGap: (event: DragEvent<HTMLElement>) => {
      event.preventDefault();
      dropHere();
    },
    openPoseMenu: (index: number, event: MouseEvent<HTMLElement>) => {
      event.preventDefault();
      event.stopPropagation();
      const current = state();
      const { running } = deriveListFlags(current);
      const blocked = running || current.st === "moving" || current.st === "sending" || !!current.preflight
        || current.selMode || current.mode !== "all";
      if (blocked) return;
      const point = screen.dom.pointInRoot(event.clientX, event.clientY);
      set({ pctx: { i: index, x: point.x, y: point.y }, ctx: null, gMenu: null });
    },
  };
}

export type PoseDragActions = ReturnType<typeof createPoseDragActions>;
