import { t as translate } from "../../../i18n";

import { deriveListFlags, groupIndexes, groupOfKey } from "./poseListState";
import { createPoseStatus, type PoseStatusState } from "./poseStatus";
import type { GroupOutline } from "./tilesView";
import type { WorkspaceViewContext } from "./workspaceView";

export type RowFill = "surface" | "blueTint" | "okTint" | "purpleTint";
export type RowEdge = "none" | "purple" | "blue" | "ok" | "dropTop" | "dropBottom";
export type RowFade = "dragging" | "cut" | "dim" | null;
export type RowNumberTone = "picked" | "pickable" | "active" | "idle";

export type RowCellView = {
  joint: number;
  value: number | string;
  invalid: boolean;
  label: string;
  duration: number | string;
  durationInvalid: boolean;
  durationLabel: string;
};

export type RowGroupView = {
  id: string;
  name: string;
  code: string;
  label: string;
  foldIcon: string;
  foldLabel: string;
  foldDisabled: boolean;
  playLabel: string;
  playOff: boolean;
  renaming: boolean;
  dropInto: string;
  outline: GroupOutline;
  dragAttr: "true" | "false";
};

export type RowReleaseCellView = { text: string; differs: boolean };

export type PoseRowView = {
  index: number;
  key: string;
  visible: boolean;
  showGroupHead: boolean;
  group: RowGroupView | null;
  inGroup: boolean;
  draggable: "true" | "false";
  fill: RowFill;
  edge: RowEdge;
  fade: RowFade;
  cursor: "grab" | "pointer" | "default";
  groupAnimation: "in" | "out" | null;
  animationDelayMs: number | null;
  landed: boolean;
  selectedAttr: "1" | "0";
  numberText: string | number;
  numberTone: RowNumberTone;
  pressed: boolean;
  tapOff: boolean;
  label: string;
  code: string;
  name: string;
  nameLabel: string;
  duration: number | string;
  durationInvalid: boolean;
  durationLabel: string;
  cells: RowCellView[];
  showGoButton: boolean;
  goDisabled: boolean;
  goLabel: string;
  goAnchor: "go" | "";
  upDisabled: boolean;
  downDisabled: boolean;
  upLabel: string;
  downLabel: string;
  showDuplicate: boolean;
  duplicateLabel: string;
  removeLabel: string;
  releaseRow: RowReleaseCellView[] | null;
  showPasteBelow: boolean;
};

const GROUP_ANIMATION_STEP_IN_MS = 45;
const GROUP_ANIMATION_STEP_OUT_MS = 30;

function rowLabelOf(index: number, isRange: boolean): string {
  const params = { number: index + 1 };
  return isRange
    ? translate("robot.workspace.sequence.rowRangeLabel", params)
    : translate("robot.workspace.sequence.rowViewLabel", params);
}

type RowState = PoseStatusState & { t6full: boolean };

export function buildRowsView(context: WorkspaceViewContext): PoseRowView[] {
  const { screen, S } = context;
  const state: RowState = S;
  const flags = deriveListFlags(state);
  const status = createPoseStatus(screen, state);
  const { isRange, isOrder, running, busy, isDisc, seq, n, pasteMode, dragOn } = flags;
  const sortedGroups = state.groups.slice().sort((a, b) => seq.indexOf(a.keys[0]) - seq.indexOf(b.keys[0]));

  return seq.map((key, index): PoseRowView => {
    const pose = state.poses[key];
    const read = status.read(index, pose);
    const group = groupOfKey(state, key);
    const indexes = group ? groupIndexes(state, group) : [];
    const isHead = !!group && group.keys[0] === key;
    const selected = !running && !state.preflight && !isRange && !isOrder && state.sel === index && !state.p0sel;
    const picked = state.selMode && state.picks.includes(key);
    const cut = !!state.pClip && !!state.pClip.keys && state.pClip.keys.includes(key);
    const animation = group && state.gAnim && state.gAnim.id === group.id ? state.gAnim : null;
    const groupPosition = group ? group.keys.indexOf(key) : 0;
    const closedHead = !running && !state.preflight && !!group && isHead && !group.open && !isOrder;
    const groupHasSelection = closedHead && state.sel >= 0 && !!group && group.keys.includes(seq[state.sel])
      && !state.p0sel;
    const groupPlaying = !!group && isHead && !group.open && !isOrder && status.currentStep >= 0
      && group.keys.includes(seq[status.currentStep]);
    const landed = isOrder && !!state.landed && state.landed.key === key;
    const dragging = isOrder && state.dragFrom === index;
    const active = read.emphasis || (isRange && read.inRange) || selected;
    const groupAllPicked = !!group && group.keys.every((groupKey) => state.picks.includes(groupKey));
    const focused = !!group && state.gFocus === group.id;
    let fill: RowFill = "surface";
    if (dragging) fill = "blueTint";
    else if (landed) fill = "okTint";
    else if (picked) fill = "purpleTint";
    else if (active) fill = "blueTint";

    let edge: RowEdge = "none";
    if (dragOn && state.dropGap === n && index === n - 1) edge = "dropBottom";
    else if (dragOn && state.dropGap === index) edge = "dropTop";
    else if (landed) edge = "ok";
    else if (picked) edge = "purple";
    else if (selected) edge = "blue";
    else if (group) edge = "purple";
    const showGroupHead = isHead && (!isOrder || state.selMode);
    let outline: GroupOutline = null;
    if (groupHasSelection || groupPlaying) outline = "blue";
    else if (state.selMode && groupAllPicked) outline = "purple";
    else if (isRange && !!group && status.preview.includes(indexes[0])) outline = "blue";
    else if (focused && !isRange && !state.selMode) outline = "purple";

    let fade: RowFade = null;
    if (dragging) fade = "dragging";
    else if (cut) fade = "cut";
    else if (read.dim) fade = "dim";

    let numberTone: RowNumberTone = "idle";
    if (picked) numberTone = "picked";
    else if (state.selMode) numberTone = "pickable";
    else if (active) numberTone = "active";

    let animationDelayMs: number | null = null;
    let groupAnimation: PoseRowView["groupAnimation"] = null;
    if (animation && group) {
      animationDelayMs = animation.mode === "in"
        ? groupPosition * GROUP_ANIMATION_STEP_IN_MS
        : (group.keys.length - 1 - groupPosition) * GROUP_ANIMATION_STEP_OUT_MS;
    }
    if (animation?.mode === "in" || animation?.mode === "out") groupAnimation = animation.mode;
    let cursor: PoseRowView["cursor"] = running ? "default" : "pointer";
    if (isOrder) cursor = "grab";
    const goDisabled = busy || !read.valid || state.linked === index;
    const showReleaseRow = !!state.release && state.view === "table" && !!state.t6full
      && index === state.sel && !state.p0sel;
    const releaseRow = showReleaseRow
      ? screen.SV.map((_joint: unknown, joint: number) => ({
        text: screen.fmt(joint, state.cur[joint]),
        differs: Math.round(state.cur[joint]) !== screen.num(pose.v[joint], 0),
      }))
      : null;

    return {
      index,
      key,
      visible: !(group && !group.open && (!isOrder || state.selMode)),
      showGroupHead,
      group: group && {
        id: group.id,
        name: group.name,
        code: `K${String(sortedGroups.indexOf(group) + 1).padStart(4, "0")}`,
        label: translate("robot.workspace.messages.poseGroupValue0", { value0: group.name }),
        foldIcon: group.open ? "▾" : "▸",
        foldLabel: group.open
          ? translate("robot.workspace.messages.collapseGroupValue0Label", { value0: group.name })
          : translate("robot.workspace.messages.expandGroupValue0", { value0: group.name }),
        foldDisabled: running,
        playLabel: translate("robot.workspace.messages.playOnlyGroupValue0Label", { value0: group.name }),
        playOff: running || busy || isDisc || state.selMode,
        renaming: state.gRen === group.id && state.view === "table",
        dropInto: state.dropInto6 === group.id ? "1" : "",
        outline,
        dragAttr: state.selMode ? "true" : "false",
      },
      inGroup: !!group,
      draggable: isOrder ? "true" : "false",
      fill,
      edge,
      fade,
      cursor,
      groupAnimation,
      animationDelayMs,
      landed,
      selectedAttr: index === state.sel ? "1" : "0",
      numberText: picked ? "✓" : index + 1,
      numberTone,
      pressed: isRange ? read.inRange : selected,
      tapOff: running || isOrder,
      label: rowLabelOf(index, isRange),
      code: `G${String(index + 1).padStart(4, "0")}`,
      name: pose.name,
      nameLabel: translate("robot.workspace.messages.nameOfPoseValue0", { value0: index + 1 }),
      duration: pose.t,
      durationInvalid: !screen.okT(pose.t),
      durationLabel: translate("robot.workspace.messages.poseValue0MoveDurationInMilliseconds", {
        value0: index + 1,
      }),
      cells: pose.v.map((value, joint): RowCellView => {
        const spec = screen.SV[joint];
        const duration = pose.ts[joint];
        return {
          joint,
          value,
          invalid: !screen.okV(joint, value),
          label: translate(
            spec.pct
              ? "robot.workspace.sequence.cellPercentLabel"
              : "robot.workspace.sequence.cellDegreesLabel",
            { number: index + 1, joint: spec.sid },
          ),
          duration,
          durationInvalid: !screen.okT(duration),
          durationLabel: translate("robot.workspace.messages.poseValue0Value1DurationInMilliseconds", {
            value0: index + 1,
            value1: spec.sid,
          }),
        };
      }),
      showGoButton: !(state.selMode || isRange || isOrder),
      goDisabled,
      goLabel: state.linked === index
        ? translate("robot.workspace.messages.robotIsAlreadyAtPoseValue0", { value0: index + 1 })
        : translate("robot.workspace.messages.moveRobotToPoseValue0Value1", { value0: index + 1, value1: pose.name }),
      goAnchor: index === state.sel ? "go" : "",
      upDisabled: index === 0,
      downDisabled: index === n - 1,
      upLabel: translate("robot.workspace.messages.movePoseValue0Up", { value0: index + 1 }),
      downLabel: translate("robot.workspace.messages.movePoseValue0Down", { value0: index + 1 }),
      showDuplicate: !state.selMode,
      duplicateLabel: group
        ? translate("robot.workspace.sequence.rowDuplicateInGroupLabel", { number: index + 1 })
        : translate("robot.workspace.sequence.rowDuplicateLabel", { number: index + 1 }),
      removeLabel: translate("robot.workspace.messages.deletePoseValue0", { value0: index + 1 }),
      releaseRow,
      showPasteBelow: pasteMode && (!group || group.open || group.keys[group.keys.length - 1] === key),
    };
  });
}
