import { t as translate } from "../../../i18n";

import type { Joint } from "./types";
import type { WorkspaceViewContext } from "./workspaceView";

type JointReadState = {
  release: boolean;
  cur: number[];
  sel: number;
  p0sel: boolean;
  moveMs: number[] | null;
  moveFrom: number[] | null;
  holding: number | null;
  jsel: number;
  jnames: string[];
  renaming: { k: "p"; i: number } | { k: "j"; j: number } | null;
  relRead?: number[];
  valDraft: { j: number; s: string } | null;
  servoIds?: number[];
  tFlash: number | null;
  mode: string;
};

export type ReadButtonState = "done" | "disabled" | "ready";
export type KnobTone = "moving" | "limit" | "viewing" | "live";
export type DurationTone = "disabled" | "invalid" | "linked" | "linkedFlash" | "split";

export type JointView = {
  index: number;
  sid: string;
  name: string;
  defaultName: string;
  selected: boolean;
  cardLabel: string;
  renaming: boolean;
  renameLabel: string;
  renameInputLabel: string;
  servoId: string | number;
  servoIdTip: string;
  releaseMode: boolean;
  releaseLiveText: string;
  releasePoseText: string;
  releaseLiveDiffers: boolean;
  readState: ReadButtonState;
  readLabel: string;
  decreaseLabel: string;
  increaseLabel: string;
  decreasePressed: boolean;
  increasePressed: boolean;
  minLabel: string;
  maxLabel: string;
  robotMarkerPercent: number | null;
  knobTone: KnobTone;
  knobPercent: number;
  bulbPercent: number | null;
  boxTitle: string;
  boxViewing: boolean;
  valueText: string;
  unit: string;
  valueLabel: string;
  valueTipId: string;
  valueKeyTip: string;
  stepDownLabel: string;
  stepUpLabel: string;
  durationValue: number | string;
  durationTone: DurationTone;
  durationDisabled: boolean;
  durationLabel: string;
  copyLabel: string;
};

const percentOf = (joint: Joint, value: number): number => ((value - joint.min) / (joint.max - joint.min)) * 100;

function degreeRangeText(joint: Joint): string {
  if (joint.pct) return translate("robot.workspace.input.percentRange");
  return translate("robot.workspace.joints.degreeRange", { min: joint.min, max: joint.max });
}

function durationToneOf(options: {
  locked: boolean;
  invalid: boolean;
  linked: boolean;
  flashing: boolean;
}): DurationTone {
  if (options.locked) return "disabled";
  if (options.invalid) return "invalid";
  if (!options.linked) return "split";
  return options.flashing ? "linkedFlash" : "linked";
}

export function buildJointsView(context: WorkspaceViewContext): JointView[] {
  const { screen, S, displayNames, st, sp, linked, lockOff, isSplit, isSame } = context;
  const state: JointReadState = S;
  const holdDirection: number = screen.holdDir || 0;
  const moving = st === "moving" || st === "playing" || !!state.moveMs;
  const viewing = !!sp && !linked && !moving && !state.release;
  const pose = sp && !state.p0sel ? sp : null;

  return (screen.SV as Joint[]).map((joint, index) => {
    const sameAsRobot = !!sp && Math.round(state.cur[index]) === screen.num(sp.v[index], 0);
    let shown = state.cur[index];
    if (!state.release && sp && !moving) shown = screen.num(sp.v[index], 0);
    const atLimit = shown <= joint.min || shown >= joint.max;
    const holdingThis = state.holding === index;
    const readDone = (state.relRead || []).includes(index) && sameAsRobot;
    const readDisabled = !sp || state.p0sel || sameAsRobot;
    const robotAway = !!sp && viewing && screen.num(sp.v[index], 0) !== state.cur[index];
    const name = displayNames[index];
    let durationValue: number | string = "";
    if (sp) durationValue = isSplit ? sp.ts[index] : sp.t;
    let knobTone: KnobTone = "live";
    if (state.moveMs && state.moveFrom && state.moveFrom[index] !== state.cur[index]) knobTone = "moving";
    else if (atLimit) knobTone = "limit";
    else if (viewing) knobTone = "viewing";
    let readState: ReadButtonState = "ready";
    if (readDone) readState = "done";
    else if (readDisabled) readState = "disabled";
    const jogLabel = (directionKey: string) => translate("robot.workspace.joints.jogLabel", {
      joint: `${joint.sid} ${name}`,
      direction: translate(directionKey),
    });
    return {
      index,
      sid: joint.sid,
      name,
      defaultName: translate(joint.nameKey),
      selected: state.jsel === index,
      cardLabel: translate("robot.workspace.messages.value0Value1Value2LeftRightToMoveUpDown", {
        value0: joint.sid,
        value1: name,
        value2: screen.fmt(index, shown),
      }),
      renaming: !!state.renaming && state.renaming.k === "j" && state.renaming.j === index,
      renameLabel: translate("robot.workspace.messages.renameValue0Value1", { value0: joint.sid, value1: name }),
      renameInputLabel: translate("robot.workspace.messages.newNameForValue0LeaveBlankToKeepValue1", {
        value0: joint.sid,
        value1: translate(joint.nameKey),
      }),
      servoId: String((state.servoIds || [1, 2, 3, 4, 5, 6])[index]),
      servoIdTip: translate("robot.workspace.messages.servoValue0BusIdChangeInTeacherMenuRobot", {
        value0: joint.sid,
      }),
      releaseMode: state.release,
      releaseLiveText: translate("robot.workspace.messages.nowValue0", {
        value0: screen.fmt(index, Math.round(state.cur[index])),
      }),
      releasePoseText: pose
        ? translate("robot.workspace.messages.inPoseValue0Value1", {
          value0: state.sel + 1,
          value1: screen.fmt(index, screen.num(pose.v[index], 0)),
        })
        : translate("robot.workspace.messages.noPoseSelected"),
      releaseLiveDiffers: !!sp && !sameAsRobot,
      readState,
      readLabel: readDone
        ? translate("robot.workspace.messages.readComplete")
        : translate("robot.workspace.messages.readValue"),
      decreaseLabel: jogLabel(joint.left),
      increaseLabel: jogLabel(joint.right),
      decreasePressed: holdingThis && holdDirection < 0,
      increasePressed: holdingThis && holdDirection > 0,
      minLabel: joint.pct ? "0%" : `${joint.min}°`,
      maxLabel: joint.pct ? "100%" : `+${joint.max}°`,
      robotMarkerPercent: robotAway ? percentOf(joint, state.cur[index]) : null,
      knobTone,
      knobPercent: percentOf(joint, knobTone === "moving" ? (state.moveFrom as number[])[index] : shown),
      bulbPercent: state.moveMs && state.moveFrom && state.moveFrom[index] !== state.cur[index]
        ? percentOf(joint, state.cur[index])
        : null,
      boxTitle: joint.pct ? translate("robot.workspace.messages.opening") : translate("robot.workspace.messages.angle"),
      boxViewing: viewing,
      valueText: state.valDraft && state.valDraft.j === index
        ? state.valDraft.s
        : String(Math.round(screen.num(shown, 0))),
      unit: joint.pct ? "%" : "°",
      valueLabel: translate("robot.workspace.messages.value0Value1EnterValue2", {
        value0: joint.sid,
        value1: name,
        value2: degreeRangeText(joint),
      }),
      valueTipId: `valtip-v4b-${index}`,
      valueKeyTip: joint.pct
        ? translate("robot.workspace.messages.adjustBy5Shift20EnterToFinish")
        : translate("robot.workspace.messages.adjustBy1Shift10EnterToFinish"),
      stepDownLabel: translate(
        joint.pct ? "robot.workspace.joints.decreaseByPercent" : "robot.workspace.joints.decreaseByDegree",
        { joint: joint.sid },
      ),
      stepUpLabel: translate(
        joint.pct ? "robot.workspace.joints.increaseByPercent" : "robot.workspace.joints.increaseByDegree",
        { joint: joint.sid },
      ),
      durationValue,
      durationTone: durationToneOf({
        locked: lockOff || !sp,
        invalid: !screen.okT(durationValue),
        linked: isSame,
        flashing: state.tFlash != null && state.tFlash !== index,
      }),
      durationDisabled: lockOff || !sp || state.mode !== "all",
      durationLabel: translate("robot.workspace.joints.durationMsLabel", {
        joint: joint.sid,
        duration: isSame
          ? translate("robot.workspace.input.linkedDuration")
          : translate("common.messages.moveDuration"),
      }),
      copyLabel: translate("robot.workspace.messages.copyValue0Value1Command", { value0: joint.sid, value1: name }),
    };
  });
}
