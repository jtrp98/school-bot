import { t as translate } from "../../../i18n";
import type { WorkspaceLogic } from "./WorkspaceLogic";
import { copyTextToClipboard } from "./clipboard";
import { selectedJointOf, sharedDuration, type PoseEditState } from "./poseEditSummary";
import { deriveWorkspaceFlags } from "./workspaceFlags";

type JointPanelState = PoseEditState & {
  jMenu: number | "all" | null;
  timeMode: "same" | "split";
  synced: boolean;
  p0sel: boolean;
};

const DEFAULT_POSE_MS = 3000;
const JOINT_COUNT = 6;
const SAME_DURATION_UNDO_MS = 6000;

// The header of the joint panel: pose menu, release/read, and the same-duration/separate-duration switch.
export function createJointPanelActions(screen: WorkspaceLogic) {
  const state = (): JointPanelState => screen.state;
  const set = (patch: Record<string, unknown>) => screen.setState(patch);

  const useSeparateDurations = () => {
    const current = state();
    if (current.timeMode === "split") return;
    const poses: Record<string, unknown> = {};
    for (const [key, pose] of Object.entries(current.poses)) {
      const ms = screen.okT(pose.t) ? Number(pose.t) : DEFAULT_POSE_MS;
      poses[key] = { ...pose, ts: Array<number>(JOINT_COUNT).fill(ms) };
    }
    set({ timeMode: "split", poses, synced: false });
    screen.addLog("time", translate("robot.workspace.messages.useIndividualServoDurations"));
  };

  const useSameDuration = () => {
    const current = state();
    if (current.timeMode !== "split") return;
    const joint = selectedJointOf(current);
    const previous = { poses: current.poses, synced: current.synced };
    const poses: Record<string, unknown> = {};
    for (const [key, pose] of Object.entries(current.poses)) {
      poses[key] = { ...pose, t: sharedDuration(screen, joint, pose) };
    }
    const { sp } = deriveWorkspaceFlags(current);
    const shown = sp ? sharedDuration(screen, joint, sp) : DEFAULT_POSE_MS;
    set({ timeMode: "same", poses, synced: false });
    screen.addLog("time", translate("robot.workspace.messages.linkAllJointDurations"));
    screen.showToast(
      translate("robot.workspace.messages.setAllJointsToValue0Ms", { value0: shown }),
      () => screen.setState({ toast: null, timeMode: "split", poses: previous.poses, synced: previous.synced }),
      translate("common.messages.undo"),
      null,
      "",
      SAME_DURATION_UNDO_MS,
    );
  };

  return {
    toggleMenu: () => set({ jMenu: state().jMenu === "all" ? null : "all" }),
    copyAllCommands: () => {
      set({ jMenu: null });
      const current = state();
      const { sp } = deriveWorkspaceFlags(current);
      const text = screen.gcodeAll();
      copyTextToClipboard(text);
      let subject = translate("robot.workspace.capture.currentPosition");
      if (current.p0sel) subject = translate("robot.workspace.messages.pose0");
      else if (sp) subject = translate("robot.workspace.messages.poseValue0", { value0: current.sel + 1 });
      const joints = Array.from({ length: JOINT_COUNT }, (_, joint) => joint);
      screen.openCopy(translate("robot.workspace.messages.copiedCommandValue0", { value0: subject }), text, joints);
    },
    askToReleaseJoints: () => {
      set({ jMenu: null });
      screen.relOpen();
    },
    toggleTimeMode: () => {
      if (state().timeMode === "split") set({ sameAsk: true });
      else useSeparateDurations();
    },
    confirmSameDuration: () => {
      set({ sameAsk: false });
      useSameDuration();
    },
    releaseJog: () => screen.endHold(),
  };
}

export type JointPanelActions = ReturnType<typeof createJointPanelActions>;
