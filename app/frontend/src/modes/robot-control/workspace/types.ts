import { t as translate } from "../../../i18n";

export type Joint = {
  sid: string;
  name: string;
  nameKey: string;
  hint: string;
  left: string;
  right: string;
  min: number;
  max: number;
  step: number;

  pct: boolean;
};

const JOINT_SPECS: Omit<Joint, "name">[] = [
  { sid: "S0", nameKey: "robot.joints.base", hint: "common.messages.rotateLeftRight", left: "common.messages.left", right: "common.messages.right", min: -180, max: 180, step: 3, pct: false },
  { sid: "S1", nameKey: "robot.joints.shoulder", hint: "common.messages.lowerRaise", left: "common.messages.lower", right: "common.messages.raise", min: -135, max: 135, step: 3, pct: false },
  { sid: "S2", nameKey: "robot.joints.elbow", hint: "common.messages.bendExtend", left: "robot.joints.bend", right: "common.messages.extend", min: -135, max: 135, step: 3, pct: false },
  { sid: "S3", nameKey: "robot.joints.wristBend", hint: "common.messages.bendDownUp", left: "common.messages.down", right: "common.messages.up", min: -135, max: 135, step: 3, pct: false },
  { sid: "S4", nameKey: "robot.joints.wristRotate", hint: "common.messages.rotateLeftRight", left: "common.messages.left", right: "common.messages.right", min: -135, max: 135, step: 3, pct: false },
  { sid: "S5", nameKey: "robot.joints.gripper", hint: "common.messages.closeOpenGripper", left: "common.messages.close", right: "common.open", min: 0, max: 100, step: 5, pct: true },
];

// Saved projects keep the Thai default as the "not renamed" marker, so it stays fixed whatever the UI language.
const defaultJointName = (nameKey: string) => translate(nameKey, { lng: "th" });

export const JOINTS: Joint[] = JOINT_SPECS.map((spec) => ({ ...spec, name: defaultJointName(spec.nameKey) }));
