

import { t } from "../../../i18n";

export type Pose = { id: number; name: string; angles: number[]; ms: number };

export const JOINTS = [
  { sid: "S1", nameKey: "robot.joints.base" },
  { sid: "S2", nameKey: "robot.joints.shoulder" },
  { sid: "S3", nameKey: "robot.joints.elbow" },
  { sid: "S4", nameKey: "robot.joints.gripper" },
];

export const REST = [90, 70, 110, 45];

export const restPose = (): Pose => ({ id: 0, name: t("robot.simulator.messages.restPose"), angles: REST, ms: 1000 });

export const DEFAULT_MS = 1000;

export const MAX_NAME = 24;

export const clampAngle = (value: number) => Math.min(180, Math.max(0, Math.round(value)));

export const sleep = (ms: number) => new Promise((resolve) => setTimeout(resolve, ms));
