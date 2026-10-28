import { t as translate } from "../../../i18n";

import { deriveListFlags, groupIndexes, groupOfKey, type PoseGroup } from "./poseListState";
import { createPoseStatus, wrapIndexes, type PoseStatus, type PoseStatusState } from "./poseStatus";
import type { WorkspaceViewContext } from "./workspaceView";

export type TileLook =
  | "emphasis" | "inRange" | "rangeIdle" | "invalid" | "selected" | "picked"
  | "reorderSource" | "reorderHeld" | "reorderLanded" | "reorder" | "normal";
export type TileFade = "faded" | "dim" | "reorderIdle" | null;
export type BarTone = "group" | "range" | "none";
export type GroupOutline = "purple" | "blue" | null;

export type GroupCardView = {
  id: string;
  name: string;
  rangeText: string;
  chevronPath: string;
  toggleLabel: string;
  footText: string;
  state: string;
  allPicked: "1" | "0" | "";
  dropInto: string;
  outline: GroupOutline;
  pointer: boolean;
  showMoreButton: boolean;
  moreLabel: string;
  menuOpen: boolean;
  showMenuItems: boolean;
  renaming: boolean;
  playLabel: string;
  playOff: boolean;
  ungroupLabel: string;
};

export type PoseCardView = {
  tapOff: boolean;
  pressed: boolean;
  label: string;
  tip: string;
  look: TileLook;
  cut: boolean;
  fade: TileFade;
  reserveMoreButton: boolean;
  selected: boolean;
  selectMode: boolean;
  pickedAttr: "1" | "0" | "";
  groupMemberAttr: "1" | "";
  dropIntoAttr: string;
  groupTag: string;
  showGroupTag: boolean;
  number: string | number;
  numberActive: boolean;
  name: string;
  code: string;
  renaming: boolean;
  renameLabel: string;
  renameInputLabel: string;
  showRenameButton: boolean;
  showPlainName: boolean;
  showRing: boolean;
  showCheck: boolean;
  ringDash: number;
  showMenuButton: boolean;
  showGoButton: boolean;
  goDisabled: boolean;
  goLabel: string;
  goAnchor: "go" | "";
  showHandle: boolean;
  showActions: boolean;
  duplicateLabel: string;
  deleteLabel: string;
};

export type PoseTileView = {
  index: number;
  key: string;
  visible: boolean;
  groupAnimation: "in" | "out" | null;
  animationDelayMs: number | null;
  slotShow: boolean;
  slotLabel: string;
  slotText: string;
  slotGrowing: boolean;
  showArrow: boolean;
  landed: boolean;
  popGroup: boolean;
  draggable: "true" | "false";
  showGroup: boolean;
  showPose: boolean;
  showBar: boolean;
  barText: string;
  barTall: boolean;
  barTone: BarTone;
  barAlignEnd: boolean;
  barPosition: "first" | "mid" | "last" | "";
  showOpenGroupHead: boolean;
  group: GroupCardView | null;
  pose: PoseCardView;
};

const GROUP_ANIMATION_STEP_IN_MS = 45;
const GROUP_ANIMATION_STEP_OUT_MS = 30;

function groupCode(sortedGroups: PoseGroup[], group: PoseGroup): string {
  return `K${String(sortedGroups.indexOf(group) + 1).padStart(4, "0")}`;
}

function tileLookOf(options: {
  status: PoseStatus;
  isRange: boolean;
  isOrder: boolean;
  selected: boolean;
  picked: boolean;
  dragging: boolean;
  held: boolean;
  landed: boolean;
}): TileLook {
  const { status, isRange, isOrder } = options;
  if (status.emphasis) return "emphasis";
  if (isRange) return status.inRange ? "inRange" : "rangeIdle";
  if (!status.valid) return "invalid";
  if (options.selected) return "selected";
  if (options.picked) return "picked";
  if (isOrder && options.dragging) return "reorderSource";
  if (isOrder && options.held) return "reorderHeld";
  if (isOrder && options.landed) return "reorderLanded";
  return isOrder ? "reorder" : "normal";
}

function barTextOf(options: {
  collapsedHead: boolean;
  covered: boolean;
  status: PoseStatus;
  preview: number[];
  groupLast: number;
  picking: boolean;
  rangeDone: boolean;
  hovering: boolean;
  isRange: boolean;
}): string {
  const { status, preview } = options;
  const lastPosition = preview.length - 1;
  const endsHere = options.collapsedHead && options.covered && preview.length > 1 && status.order > 0
    && preview[lastPosition] === options.groupLast;
  if (endsHere) return translate("robot.workspace.messages.end");
  if (!options.covered) return "";
  if (options.isRange && options.picking && !options.hovering) return translate("robot.workspace.playback.selectEnd");
  if (options.isRange && !options.picking && !options.rangeDone) {
    return translate("robot.workspace.playback.confirmStart");
  }
  if (preview.length === 1) return translate("robot.workspace.playback.onlyThisPose");
  if (status.order === 0) return translate("robot.workspace.playback.startMarker");
  if (status.order === lastPosition) return translate("robot.workspace.messages.end");
  return translate("robot.workspace.playback.orderNumber", { number: status.order + 1 });
}

function tileLabelOf(index: number, name: string, isRange: boolean, isOrder: boolean): string {
  const params = { number: index + 1, name };
  if (isRange) return translate("robot.workspace.sequence.rangeTileLabel", params);
  if (isOrder) return translate("robot.workspace.sequence.dragTileLabel", params);
  return translate("robot.workspace.sequence.viewTileLabel", params);
}

export function buildTilesView(context: WorkspaceViewContext): PoseTileView[] {
  const { screen, S } = context;
  const state: PoseStatusState = S;
  const flags = deriveListFlags(state);
  const status = createPoseStatus(screen, state);
  const { isRange, isOrder, running, busy, isDisc, seq, n, pasteMode, dragOn } = flags;
  const sortedGroups = state.groups.slice().sort((a, b) => seq.indexOf(a.keys[0]) - seq.indexOf(b.keys[0]));
  const coverShown = isRange || status.playVisible;

  return seq.map((key, index): PoseTileView => {
    const pose = state.poses[key];
    const read = status.read(index, pose);
    const group = groupOfKey(state, key);
    const indexes = group ? groupIndexes(state, group) : [];
    const groupLast = indexes[indexes.length - 1];
    const isHead = !!group && group.keys[0] === key;
    const collapsed = !!group && !group.open && (!isOrder || state.selMode);
    const selected = !running && !state.preflight && !isRange && !isOrder && !state.selMode
      && state.sel === index && !state.p0sel;
    const picked = state.selMode && state.picks.includes(key);
    const idle = !running && !state.preflight && !!group && !group.open && !isOrder && isHead;
    const groupHasSelection = idle && state.sel >= 0 && !!group && group.keys.includes(seq[state.sel])
      && !state.p0sel;
    const groupPlaying = !!group && !group.open && !isOrder && isHead && status.currentStep >= 0
      && group.keys.includes(seq[status.currentStep]);
    const animation = group && state.gAnim && state.gAnim.id === group.id ? state.gAnim : null;
    const groupPosition = group ? group.keys.indexOf(key) : 0;
    const covered = coverShown && read.inRange;
    const groupAllPicked = !!group && group.keys.every((groupKey) => state.picks.includes(groupKey));
    const focused = !!group && state.gFocus === group.id;
    const openGroupMember = !!group && group.open && !collapsed && !isOrder;
    const showOpenGroupHead = !!group && group.open && isHead && !isOrder && !covered;
    const look = tileLookOf({
      status: read,
      isRange,
      isOrder,
      selected,
      picked,
      dragging: state.dragFrom === index,
      held: state.movePick === index,
      landed: !!state.landed && state.landed.key === key,
    });
    const cut = !!state.pClip && !!state.pClip.keys && state.pClip.keys.includes(key);
    let fade: TileFade = null;
    if (cut || state.dragFrom === index) fade = "faded";
    else if (read.dim) fade = "dim";
    else if (look === "reorder" && state.movePick >= 0) fade = "reorderIdle";
    const inRangeText = wrapIndexes(n, state.rs, index).map((position) => position + 1).join(" → ");
    const placeIndex = (dragOn ? state.dragFrom : state.movePick) < index ? index : index + 1;
    const heldPoseName = (state.poses[seq[state.movePick]] || { name: "" }).name;
    const actionsVisible = !isRange && !isOrder && !running && !state.selMode && state.timeEdit !== index;
    const goDisabled = busy || !read.valid || state.linked === index;
    let outline: GroupOutline = null;
    if (groupHasSelection || groupPlaying) outline = "blue";
    else if (state.selMode && groupAllPicked) outline = "purple";
    else if (isRange && !!group && status.preview.includes(indexes[0])) outline = "blue";
    else if (focused && !isRange && !state.selMode) outline = "purple";
    const showGroup = collapsed && isHead;
    let groupAnimation: PoseTileView["groupAnimation"] = null;
    let animationDelayMs: number | null = null;
    if (animation) groupAnimation = animation.mode === "in" ? "in" : "out";
    if (animation && group) {
      animationDelayMs = animation.mode === "in"
        ? groupPosition * GROUP_ANIMATION_STEP_IN_MS
        : (group.keys.length - 1 - groupPosition) * GROUP_ANIMATION_STEP_OUT_MS;
    }
    let slotLabel = "";
    if (pasteMode) {
      slotLabel = translate("robot.workspace.messages.pasteHereAsPoseValue0", { value0: index + 1 });
    } else if (state.movePick >= 0) {
      slotLabel = translate("robot.workspace.messages.placePoseValue0AtPositionValue1", {
        value0: heldPoseName,
        value1: index > state.movePick ? index : index + 1,
      });
    }
    let barTone: BarTone = "none";
    if (openGroupMember) barTone = "group";
    else if (covered) barTone = "range";
    let barPosition: PoseTileView["barPosition"] = "";
    if (group && openGroupMember) {
      if (group.keys[0] === key) barPosition = "first";
      else if (group.keys[group.keys.length - 1] === key) barPosition = "last";
      else barPosition = "mid";
    }
    let groupState = "";
    if (!state.selMode) {
      if (groupPlaying) groupState = "play";
      else if (isRange) groupState = status.preview.includes(indexes[0]) ? "in" : "out";
      else if (focused || groupHasSelection) groupState = "sel";
    }
    const selectionMark = picked ? "1" : "0";
    const groupSelectionMark = groupAllPicked ? "1" : "0";
    const selectionNumber = picked ? "✓" : "";
    let tip = "";
    if (isRange) {
      tip = state.picking
        ? translate("robot.workspace.messages.tapToEndHereWillPlayValue0", { value0: inRangeText })
        : translate("robot.workspace.messages.tapToStartAtPoseValue0", { value0: index + 1 });
    }

    return {
      index,
      key,
      visible: !(collapsed && !isHead),
      groupAnimation,
      animationDelayMs,
      slotShow: pasteMode
        || (isOrder && state.movePick >= 0 && index !== state.movePick && index !== state.movePick + 1)
        || (dragOn && state.dropGap === index),
      slotLabel,
      slotText: pasteMode
        ? translate("robot.workspace.messages.pasteHereLabel")
        : translate("robot.workspace.messages.placeAsPoseValue0", { value0: placeIndex }),
      slotGrowing: dragOn,
      showArrow: index > 0 && !pasteMode
        && !(isOrder && state.movePick >= 0 && index !== state.movePick && index !== state.movePick + 1)
        && !(dragOn && state.dropGap === index),
      landed: !!state.landed && state.landed.key === key,
      popGroup: !!animation && animation.mode === "pop",
      draggable: isOrder ? "true" : "false",
      showGroup,
      showPose: !(collapsed && isHead),
      showBar: !showOpenGroupHead,
      barText: barTextOf({
        collapsedHead: collapsed && isHead,
        covered,
        status: read,
        preview: status.preview,
        groupLast,
        picking: state.picking,
        rangeDone: state.rangeDone,
        hovering: status.hover >= 0,
        isRange,
      }),
      barTall: !!group && group.open && !isOrder && !covered,
      barTone,
      barAlignEnd: read.order === status.preview.length - 1 && status.preview.length > 1,
      barPosition,
      showOpenGroupHead,
      group: group && {
        id: group.id,
        name: group.name,
        rangeText: indexes.length > 1 ? `${indexes[0] + 1}–${groupLast + 1}` : String(indexes[0] + 1),
        chevronPath: group.open ? "M6 15l6-6 6 6" : "M6 9l6 6 6-6",
        toggleLabel: group.open
          ? translate("robot.workspace.messages.collapseGroupValue0", { value0: group.name })
          : translate("robot.workspace.messages.viewPosesInValue0", { value0: group.name }),
        footText: groupHasSelection
          ? translate("robot.workspace.messages.poseValue0", { value0: state.sel + 1 })
          : groupCode(sortedGroups, group),
        state: groupState,
        allPicked: state.selMode ? groupSelectionMark : "",
        dropInto: state.dropInto6 === group.id ? "1" : "",
        outline,
        pointer: isRange || state.selMode,
        showMoreButton: !state.selMode && !running && (focused || groupHasSelection),
        moreLabel: translate("robot.workspace.messages.manageGroupValue0", { value0: group.name }),
        menuOpen: state.gMenu === group.id,
        showMenuItems: state.gMenu === group.id && state.gRen !== group.id,
        renaming: state.gRen === group.id,
        playLabel: translate("robot.workspace.messages.playOnlyGroupValue0", { value0: group.name }),
        playOff: running || busy || isDisc || state.selMode,
        ungroupLabel: translate("robot.workspace.messages.ungroupValue0KeepAllPoses", { value0: group.name }),
      },
      pose: {
        tapOff: running,
        pressed: isRange ? read.inRange : selected,
        label: tileLabelOf(index, pose.name, isRange, isOrder),
        tip,
        look,
        cut,
        fade,
        reserveMoreButton: !isRange && !isOrder && !running,
        selected,
        selectMode: state.selMode,
        pickedAttr: state.selMode ? selectionMark : "",
        groupMemberAttr: state.selMode && group ? "1" : "",
        dropIntoAttr: group && state.dropInto6 === group.id ? "1" : "",
        groupTag: state.selMode && group ? group.name : "",
        showGroupTag: state.selMode && !!group,
        number: state.selMode ? selectionNumber : index + 1,
        numberActive: read.emphasis || selected || (isRange && read.inRange),
        name: pose.name,
        code: `G${String(index + 1).padStart(4, "0")}`,
        renaming: !!state.renaming && state.renaming.k === "p" && state.renaming.i === index,
        renameLabel: translate("robot.workspace.messages.renamePoseValue0Value1", {
          value0: index + 1,
          value1: pose.name,
        }),
        renameInputLabel: translate("robot.workspace.messages.newNameForPoseValue0", { value0: index + 1 }),
        showRenameButton: selected && !isRange && !isOrder && !running,
        showPlainName: !(selected && !isRange && !isOrder && !running),
        showRing: read.showRing && !state.selMode,
        showCheck: read.showCheck && !state.selMode,
        ringDash: read.ringDash,
        showMenuButton: state.sel === index && !state.selMode && !running && !isRange,
        showGoButton: !(state.selMode || isRange || isOrder),
        goDisabled,
        goLabel: state.linked === index
          ? translate("robot.workspace.messages.robotIsAlreadyAtPoseValue0", { value0: index + 1 })
          : translate("robot.workspace.messages.moveRobotToPoseValue0Value1", { value0: index + 1, value1: pose.name }),
        goAnchor: index === state.sel ? "go" : "",
        showHandle: isOrder && !state.selMode,
        showActions: actionsVisible,
        duplicateLabel: translate("robot.workspace.messages.duplicatePoseValue0Value1AfterPoseValue2", {
          value0: index + 1,
          value1: pose.name,
          value2: index + 1,
        }),
        deleteLabel: state.delAsk
          ? translate("robot.workspace.messages.confirmDeletingPoseValue0Value1", {
            value0: index + 1,
            value1: pose.name,
          })
          : translate("robot.workspace.messages.deletePoseValue0Value1", { value0: index + 1, value1: pose.name }),
      },
    };
  });
}
