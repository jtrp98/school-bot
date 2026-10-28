import type { ChangeEvent, FocusEvent, KeyboardEvent, MouseEvent } from "react";
import type { WorkspaceLogic } from "./WorkspaceLogic";
import { deriveListFlags, type PoseListState } from "./poseListState";

const GO_POPOVER_DELAY_MS = 60;
const LAST_JOINT = 5;

// Handlers read the screen state when they run, so one instance serves every render without stale closures.
export function createPoseRowActions(screen: WorkspaceLogic) {
  const state = (): PoseListState => screen.state;

  // Typing in a cell of a pose the robot is not at first asks the user to move the robot there.
  const askToMoveFirst = (index: number, input: HTMLInputElement, previous: number | string | null): boolean => {
    const current = state();
    const { isRange, isOrder } = deriveListFlags(current);
    if (current.selMode || isRange || isOrder || current.linked === index || current.p0sel) return false;
    if (previous !== null) input.value = String(previous);
    if (screen.state.keyPop) screen.setState({ keyPop: null });
    if (current.sel !== index) screen.poseActions.tapPose(index);
    screen.tm(() => screen.msgPopOpen("go"), GO_POPOVER_DELAY_MS);
    return true;
  };

  const poseKey = (index: number) => state().seq[index];

  return {
    changeName: (index: number, event: ChangeEvent<HTMLInputElement>) => {
      screen.setPose(poseKey(index), { name: event.target.value });
    },
    changeCell: (index: number, joint: number, event: ChangeEvent<HTMLInputElement>) => {
      const pose = state().poses[poseKey(index)];
      if (askToMoveFirst(index, event.currentTarget, pose.v[joint])) return;
      const values = pose.v.slice();
      values[joint] = event.target.value;
      screen.setPose(poseKey(index), { v: values });
    },
    changeCellDuration: (index: number, joint: number, event: ChangeEvent<HTMLInputElement>) => {
      const durations = state().poses[poseKey(index)].ts.slice();
      durations[joint] = event.target.value;
      screen.setPose(poseKey(index), { ts: durations });
    },
    changeDuration: (index: number, event: ChangeEvent<HTMLInputElement>) => {
      screen.setPose(poseKey(index), { t: event.target.value });
    },
    showCellHint: (event: FocusEvent<HTMLInputElement>) => screen.keyPopOpen(event.currentTarget),
    keyOnCell: (index: number, joint: number, event: KeyboardEvent<HTMLInputElement>) => {
      const { key } = event;
      if (screen.state.keyPop) screen.setState({ keyPop: null });
      const vertical = key === "ArrowUp" || key === "ArrowDown";
      if (vertical && askToMoveFirst(index, event.currentTarget, null)) {
        event.preventDefault();
        return;
      }
      if (key === "ArrowLeft" || key === "ArrowRight") {
        event.preventDefault();
        const next = joint + (key === "ArrowRight" ? 1 : -1);
        if (next >= 0 && next <= LAST_JOINT) screen.dom.focusCell(poseKey(index), next);
      } else if (vertical) {
        event.preventDefault();
        const pose = state().poses[poseKey(index)];
        const spec = screen.SV[joint];
        const typed = screen.num(event.currentTarget.value, screen.num(pose.v[joint], 0));
        const step = spec.pct ? (event.shiftKey ? 20 : 5) : event.shiftKey ? 10 : 1;
        const value = Math.max(spec.min, Math.min(spec.max, Math.round(typed) + (key === "ArrowUp" ? step : -step)));
        const values = pose.v.slice();
        values[joint] = value;
        screen.setPose(poseKey(index), { v: values });
      } else if (key === "Enter") {
        event.currentTarget.blur();
      }
    },
    tapRow: (index: number, event: MouseEvent<HTMLElement>) => {
      const target = event.target as HTMLElement;
      if (target.closest("input,button,textarea,select,a,[role=button]")) return;
      screen.poseActions.tapPose(index);
    },
    moveRowUp: (index: number) => screen.moveTo(index, index - 1),
    moveRowDown: (index: number) => screen.moveTo(index, index + 2),
    pasteAfterRow: (index: number) => screen.pPaste(index + 1),
  };
}

export type PoseRowActions = ReturnType<typeof createPoseRowActions>;
