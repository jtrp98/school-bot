import { t as translate } from "../../../i18n";
import type { WorkspaceLogic } from "./WorkspaceLogic";
import type { FlagPose } from "./workspaceFlags";
import { deriveWorkspaceFlags, type FlagState } from "./workspaceFlags";

export type PoseEditState = FlagState & {
  cur: number[];
  snap: { key: string; p: FlagPose } | null;
  jsel: number;
};

const DEFAULT_POSE_MS = 3000;
const LAST_JOINT = 5;

// The one duration a pose gets when all joints share it: the selected joint's, or the longest when none is selected.
export function sharedDuration(screen: WorkspaceLogic, selectedJoint: number, pose: FlagPose): number {
  const valid = pose.ts.map((ms) => (screen.okT(ms) ? Number(ms) : DEFAULT_POSE_MS));
  return selectedJoint >= 0 ? valid[selectedJoint] : Math.max(...valid);
}

export function selectedJointOf(state: { jsel: number }): number {
  return state.jsel >= 0 && state.jsel <= LAST_JOINT ? state.jsel : -1;
}

// What changed since the robot reached the selected pose, and the duration the selected pose will use.
export function derivePoseEditSummary(screen: WorkspaceLogic, state: PoseEditState) {
  const { sp, seq } = deriveWorkspaceFlags(state);
  const changes: string[] = [];
  if (sp && state.snap && state.snap.key === seq[state.sel]) {
    const original = state.snap.p;
    sp.v.forEach((value, joint) => {
      if (screen.num(value, 0) === screen.num(original.v[joint], 0)) return;
      const from = screen.fmt(joint, original.v[joint]);
      changes.push(`${screen.SV[joint].sid} ${from}→${screen.fmt(joint, value)}`);
    });
    if (String(sp.t) !== String(original.t) || sp.ts.join() !== original.ts.join()) {
      changes.push(translate("robot.workspace.messages.duration"));
    }
    if (sp.name !== original.name) changes.push(translate("robot.workspace.messages.name"));
  }
  const selectedJoint = selectedJointOf(state);
  return {
    ms: sp ? screen.dur(sp) : 0,
    changed: changes.length > 0,
    changeText: changes.length > 0
      ? translate("robot.workspace.messages.changedValue0", { value0: changes.join(" · ") })
      : "",
    robotWhere: state.linked >= 0
      ? translate("robot.workspace.messages.robotIsAtPoseValue0", { value0: state.linked + 1 })
      : translate("robot.workspace.messages.robotIsElsewhereNoMatchingPose"),
    tOk: sp ? screen.okT(sp.t) : true,
    tN: sp ? screen.num(sp.t, 0) : 0,
    samePick: (pose: FlagPose) => sharedDuration(screen, selectedJoint, pose),
    sameWhy: selectedJoint >= 0
      ? translate("robot.workspace.messages.forEachPoseUseTheSelectedJointSDuration", {
        value0: screen.SV[selectedJoint].sid,
        value1: translate(screen.SV[selectedJoint].nameKey),
      })
      : translate("robot.workspace.messages.noJointIsSelectedUseEachPoseSLongest"),
  };
}
