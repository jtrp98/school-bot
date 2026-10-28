import type { ChangeEvent } from "react";
import { t as translate } from "../../../i18n";
import type { WorkspaceLogic } from "./WorkspaceLogic";
import { deriveWorkspaceFlags, type FlagState } from "./workspaceFlags";

type SelectedPoseState = FlagState & {
  snap: { key: string; p: unknown } | null;
  delAsk: boolean;
};

const DEFAULT_POSE_MS = 3000;
const MIN_POSE_MS = 1000;
const MAX_POSE_MS = 9000;
const DURATION_STEP_MS = 100;

// Edits of the pose shown in the selected-pose panel. Handlers read the screen state when they run.
export function createSelectedPoseActions(screen: WorkspaceLogic) {
  const state = (): SelectedPoseState => screen.state;
  const set = (patch: Record<string, unknown>) => screen.setState(patch);
  const selected = () => {
    const { sp, seq } = deriveWorkspaceFlags(state());
    return sp ? { pose: sp, key: seq[state().sel], index: state().sel } : null;
  };
  const stepDuration = (direction: 1 | -1) => {
    const target = selected();
    if (!target) return;
    const stepped = screen.num(target.pose.t, DEFAULT_POSE_MS) + direction * DURATION_STEP_MS;
    const rounded = Math.round(stepped / DURATION_STEP_MS) * DURATION_STEP_MS;
    screen.setPose(target.key, { t: direction > 0 ? Math.min(MAX_POSE_MS, rounded) : Math.max(MIN_POSE_MS, rounded) });
  };

  return {
    changeName: (event: ChangeEvent<HTMLInputElement>) => {
      const target = selected();
      if (target) screen.setPose(target.key, { name: event.target.value });
    },
    // An emptied name would leave an unlabeled card, so it falls back to the pose number.
    restoreEmptyName: () => {
      const target = selected();
      if (!target || target.pose.name.trim() !== "") return;
      screen.setPose(target.key, {
        name: translate("robot.workspace.messages.poseValue0Label", { value0: target.index + 1 }),
      });
    },
    changeDuration: (event: ChangeEvent<HTMLInputElement>) => {
      const target = selected();
      if (target) screen.setPose(target.key, { t: event.target.value });
    },
    shortenDuration: () => stepDuration(-1),
    lengthenDuration: () => stepDuration(1),
    revertToArrival: () => {
      const current = state();
      const snap = current.snap;
      if (!snap) return;
      const poses = { ...current.poses, [snap.key]: structuredClone(snap.p) };
      set({ poses, linked: -1, snap: null });
      screen.addLog("pose", translate("robot.workspace.messages.restorePoseValue0ValuesRobotStaysAtTheAdjusted", {
        value0: current.sel + 1,
      }));
    },
    duplicate: () => {
      const target = selected();
      if (!target) return;
      const name = translate("robot.workspace.messages.value0Copy", { value0: target.pose.name });
      const message = translate("robot.workspace.messages.duplicatePoseValue0Value1", {
        value0: target.index + 1,
        value1: target.pose.name,
      });
      screen.insertPose(target.index + 1, { ...structuredClone(target.pose), name }, message, target.key);
    },
    // The first press arms the button; the second one deletes.
    remove: () => {
      const target = selected();
      if (!target) return;
      if (!state().delAsk) {
        set({ delAsk: true });
        return;
      }
      screen.removeAt(target.index);
    },
  };
}

export type SelectedPoseActions = ReturnType<typeof createSelectedPoseActions>;
