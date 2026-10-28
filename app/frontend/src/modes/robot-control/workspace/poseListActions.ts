import type { ChangeEvent, KeyboardEvent, MouseEvent } from "react";
import { t as translate } from "../../../i18n";
import type { WorkspaceLogic } from "./WorkspaceLogic";
import { deriveListFlags, groupOfKey, type PoseListState } from "./poseListState";

const DEFAULT_FAR_MOVE_DEGREES = 30;

// Handlers read the screen state when they run, so one instance serves every render without stale closures.
export function createPoseListActions(screen: WorkspaceLogic) {
  const state = (): PoseListState => screen.state;
  const set = (patch: Record<string, unknown>) => screen.setState(patch);

  // A move to the selected pose that is longer than the safety threshold waits for the user's confirmation.
  const moveToSelectedPose = () => {
    screen.tm(() => {
      const target = screen.selPose();
      if (!target) return;
      const difference = screen.maxDiff(screen.state.cur, target.v);
      if (difference.m > (screen.props.farDeg ?? DEFAULT_FAR_MOVE_DEGREES)) {
        screen.setState({
          far: { j: difference.j, dir: 0, maxd: Math.round(difference.m), sid: screen.SV[difference.j].sid },
        });
      } else {
        screen.syncTo();
      }
    }, 0);
  };

  const goToPose = (index: number, event: MouseEvent<HTMLElement> | KeyboardEvent<HTMLElement>) => {
    event.stopPropagation();
    const current = state();
    const pose = current.poses[current.seq[index]];
    if (deriveListFlags(current).busy || current.linked === index || !screen.poseOk(pose)) return;
    if (current.gFocus) set({ gFocus: null });
    screen.select(index);
    moveToSelectedPose();
  };

  const tapPose = (index: number) => {
    const current = state();
    const { running, isOrder, isRange } = deriveListFlags(current);
    if (running) return;
    if (isOrder && !current.selMode) return;
    if (current.selMode) {
      const key = current.seq[index];
      if (current.grpNaming) return;
      const group = groupOfKey(current, key);
      if (group) {
        screen.togglePickGroup(group);
        return;
      }
      const picks = current.picks.includes(key)
        ? current.picks.filter((pick) => pick !== key)
        : [...current.picks, key];
      set({ picks, grpNaming: false });
      return;
    }
    if (isRange) {
      if (!current.picking) set({ rs: index, re: index, picking: true, rangeDone: false, hoverIdx: -1 });
      else set({ re: index, picking: false, rangeDone: true, hoverIdx: -1 });
      return;
    }
    if (current.gFocus) set({ gFocus: null });
    screen.select(index);
  };

  const duplicate = (index: number) => {
    const current = state();
    const key = current.seq[index];
    const pose = current.poses[key];
    const copy = {
      ...structuredClone(pose),
      name: translate("robot.workspace.messages.value0Copy", { value0: pose.name }),
    };
    const message = translate("robot.workspace.messages.duplicatePoseValue0Value1", {
      value0: index + 1,
      value1: pose.name,
    });
    screen.insertPose(index + 1, copy, message, key);
  };

  const startRename = (index: number, resetMenu: boolean) => {
    const current = state();
    const name = current.poses[current.seq[index]].name;
    const patch = { renaming: { k: "p", i: index }, renameDraft: name, delAsk: false };
    set(resetMenu ? { ...patch, menuIdx: -1 } : patch);
  };

  const isActivationKey = (event: KeyboardEvent<HTMLElement>) => event.key === "Enter" || event.key === " ";

  return {
    tapPose,
    goToPose,
    goToPoseFromKey: (index: number, event: KeyboardEvent<HTMLElement>) => {
      if (!isActivationKey(event)) return;
      event.preventDefault();
      goToPose(index, event);
    },
    hoverPose: (index: number) => {
      const current = state();
      if (deriveListFlags(current).isRange && current.hoverIdx !== index) set({ hoverIdx: index });
    },
    selectPoseZero: () => {
      screen.logEditOf(state());
      set({ p0sel: true, sel: -1, snap: null, far: null });
    },
    pasteBeforeFirst: () => screen.pPaste(0),
    clearHover: () => {
      if (state().hoverIdx !== -1) set({ hoverIdx: -1 });
    },
    editRenameDraft: (event: ChangeEvent<HTMLInputElement>) => set({ renameDraft: event.target.value }),
    keyOnRenameDraft: (event: KeyboardEvent<HTMLInputElement>) => {
      if (event.key === "Enter") {
        event.preventDefault();
        screen.commitRename();
      } else if (event.key === "Escape") {
        event.preventDefault();
        set({ renaming: null });
      }
    },
    commitRename: () => screen.commitRename(),
    deleteTilePose: (index: number, event: MouseEvent<HTMLElement>) => {
      event.stopPropagation();
      set({ menuIdx: -1 });
      screen.removeAt(index);
    },
    deleteRowPose: (index: number) => screen.removeAt(index),
    duplicateTilePose: (index: number, event: MouseEvent<HTMLElement>) => {
      event.stopPropagation();
      duplicate(index);
    },
    duplicateRowPose: duplicate,
    renameTilePose: (index: number, event: MouseEvent<HTMLElement>) => {
      event.stopPropagation();
      if (state().holding === null && !deriveListFlags(state()).lockOff) startRename(index, true);
    },
    renameTilePoseFromKey: (index: number, event: KeyboardEvent<HTMLElement>) => {
      if (!isActivationKey(event)) return;
      event.preventDefault();
      event.stopPropagation();
      if (!deriveListFlags(state()).lockOff) startRename(index, false);
    },
  };
}

export type PoseListActions = ReturnType<typeof createPoseListActions>;
