import { t as translate } from "../../../i18n";

import type { Joint } from "./types";

// Where each servo sits on the arm photo, as percentages of its width and height.
const DOT_POSITIONS: ReadonlyArray<readonly [number, number]> = [
  [40.2, 82.5],
  [42.0, 61.8],
  [56.1, 25.9],
  [43.8, 6.7],
  [33.2, 13.3],
  [20.0, 25.1],
];
const LAST_JOINT = 5;

export type ArmDotView = { index: number; sid: string; on: boolean; label: string; x: number; y: number };
export type ArmTagView = { text: string; x: number; y: number };

export function buildArmStage(joints: Joint[], selected: number | undefined, displayNames: string[]) {
  const armDots = joints.map((joint, index): ArmDotView => ({
    index,
    sid: joint.sid,
    on: selected === index,
    label: translate("robot.workspace.messages.selectValue0Value1", { value0: joint.sid, value1: displayNames[index] }),
    x: DOT_POSITIONS[index][0],
    y: DOT_POSITIONS[index][1],
  }));
  const tagged = Math.max(0, Math.min(LAST_JOINT, selected ?? 0));
  const armTag: ArmTagView = {
    text: `${joints[tagged].sid} ${displayNames[tagged]}`,
    x: DOT_POSITIONS[tagged][0],
    y: DOT_POSITIONS[tagged][1],
  };
  return { armDots, armTag };
}
