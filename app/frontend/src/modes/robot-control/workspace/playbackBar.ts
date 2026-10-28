import type { IconName } from "../../../components/shared/Icons";
import { t as translate } from "../../../i18n";

import type { WorkspaceViewContext } from "./workspaceView";

export type PlaybackMenu = "all" | "source" | "scope";
export type RangeEdge = "start" | "end";
export type PlaybackSource = "app" | "robot";
export type PlaybackScope =
  | { kind: "all" }
  | { kind: "selectedPose" }
  | { kind: "range" }
  | { kind: "group"; groupId: string };

type PoseGroup = { id: string; name: string; keys: string[] };
type RangeKeys = [string, string] | [string, string, boolean] | null;

export type PlaybackBarState = {
  st: string;
  seq: string[];
  poses: Record<string, { name: string }>;
  groups: PoseGroup[];
  sel: number;
  p0sel: boolean;
  src?: PlaybackSource;
  mode: string;
  picking: boolean;
  rangeDone: boolean;
  rangeOn: boolean;
  rs: number;
  re: number;
  rk?: RangeKeys;
  grpRun?: string | null;
  scopeOpen: boolean;
  hoverIdx: number;
  timeEdit: number;
  renaming: unknown;
  movePick: number;
  playbackMenu?: PlaybackMenu | null;
  rangeListOpen?: RangeEdge | null;
  loops: number;
  loopInf: boolean;
  loopMenuOpen?: boolean;
  preflight: boolean;
  checked: boolean;
  preflightShakes?: number;
  robotSeq: { n: number; at: string } | null;
  synced: boolean;
  robotRun: boolean;
};

export type PlaybackChoice<T> = {
  value: T;
  icon: IconName | undefined;
  label: string;
  detail: string;
  selected: boolean;
  disabled: boolean;
};

export type RangeOption = { index: number; label: string };

// Stored indexes can point past the end after poses are deleted, so the pickers clamp to the current sequence.
export function rangeBounds(state: PlaybackBarState): { start: number; end: number } {
  const last = Math.max(state.seq.length - 1, 0);
  const clamp = (index: number) => Math.min(Math.max(index, 0), last);
  return state.rangeOn ? { start: clamp(state.rs), end: clamp(state.re) } : { start: 0, end: last };
}

const poseLabel = (index: number) => translate("robot.workspace.messages.poseValue0", { value0: index + 1 });

function rangeOptions(poseCount: number): RangeOption[] {
  return Array.from({ length: poseCount }, (_, index) => ({ index, label: poseLabel(index) }));
}

function robotMemoryDetail(state: PlaybackBarState): string {
  if (!state.robotSeq) return translate("robot.workspace.messages.neverUploaded");
  const params = { poseCount: state.robotSeq.n, uploadedAt: state.robotSeq.at };
  return state.synced
    ? translate("robot.workspace.upload.memorySummaryMatches", params)
    : translate("robot.workspace.upload.memorySummaryDiffers", params);
}

function buildSourceChoices(context: WorkspaceViewContext): PlaybackChoice<PlaybackSource>[] {
  const { S, n, IC } = context;
  const state: PlaybackBarState = S;
  const current = state.src ?? "app";
  const choice = (value: PlaybackSource, labelKey: string, detail: string, disabled: boolean) => {
    const label = IC(translate(labelKey));
    return { value, icon: label.c, label: label.t, detail, selected: current === value, disabled };
  };
  return [
    choice(
      "app",
      "robot.workspace.messages.appPoses",
      translate("robot.workspace.messages.value0PosesShownOnThisPage", { value0: n }),
      false,
    ),
    choice("robot", "robot.workspace.messages.robotMemoryPoses", robotMemoryDetail(state), !state.robotSeq),
  ];
}

function buildScopeChoices(context: WorkspaceViewContext): PlaybackChoice<PlaybackScope>[] {
  const { S, n, seq, isRange, isRobot, grpScope, IC } = context;
  const state: PlaybackBarState = S;
  if (isRobot) return [];
  const activeGroup: PoseGroup | null | undefined = grpScope;
  const singlePose = state.rangeOn && state.rs === state.re && state.rs === state.sel && !activeGroup;
  const choice = (value: PlaybackScope, labelText: string, detail: string, selected: boolean) => {
    const label = IC(labelText);
    return { value, icon: label.c, label: label.t, detail, selected, disabled: false };
  };
  const choices = [
    choice(
      { kind: "all" },
      translate("robot.workspace.messages.all"),
      translate("robot.workspace.messages.value0PosesLabel", { value0: n }),
      !state.rangeOn && !isRange,
    ),
  ];
  if (!isRange && state.sel >= 0 && !state.p0sel && state.sel < n) {
    const poseName = state.poses[seq[state.sel]].name;
    choices.push(choice(
      { kind: "selectedPose" },
      translate("robot.workspace.messages.selectedPoseOnly"),
      translate("robot.workspace.messages.poseValue0Value1LabelStatus", { value0: state.sel + 1, value1: poseName }),
      singlePose,
    ));
  }
  choices.push(choice(
    { kind: "range" },
    isRange
      ? translate("robot.workspace.messages.cancelRangeSelectionLabel")
      : translate("robot.workspace.messages.chooseRange"),
    isRange
      ? translate("robot.workspace.messages.backToPlayingAll")
      : translate("robot.workspace.messages.tapStartThenEndSameCardOnePose"),
    (state.rangeOn && !activeGroup && !singlePose) || isRange,
  ));
  for (const group of state.groups) {
    const indexes = group.keys.map((key) => seq.indexOf(key));
    choices.push(choice(
      { kind: "group", groupId: group.id },
      translate("robot.workspace.messages.poseGroupValue0Label", { value0: group.name }),
      translate("robot.workspace.messages.posesValue0Value1", {
        value0: indexes[0] + 1,
        value1: indexes[indexes.length - 1] + 1,
      }),
      !!activeGroup && activeGroup.id === group.id,
    ));
  }
  return choices;
}

export function buildPlaybackBarView(context: WorkspaceViewContext) {
  const { S, n, running, lockOff, isRange, isRobot, playScopeLabel, wrapOrd, IC } = context;
  const { loopInfEff, vRunNow, barMode } = context;
  const state: PlaybackBarState = S;
  const openMenu = state.playbackMenu ?? null;
  const { start, end } = rangeBounds(state);
  const controlsLocked = running || !!state.preflight;
  const scopeLabel = IC(playScopeLabel);
  const sourceIcon: IconName = isRobot ? "robot" : "laptop";
  const wrapOrder: number[] = end < start ? wrapOrd(start, end) : [];
  return {
    controlsLocked,
    showTopProgress: vRunNow && !state.robotRun && barMode !== "now",
    robotPoseCount: state.robotSeq
      ? translate("robot.workspace.messages.value0Poses", { value0: state.robotSeq.n })
      : "",
    range: {
      disabled: controlsLocked || n < 2,
      active: !!state.rangeOn,
      openEdge: running ? null : state.rangeListOpen ?? null,
      start,
      end,
      startLabel: poseLabel(start),
      endLabel: poseLabel(end),
      options: rangeOptions(n),
      wrapOrder: wrapOrder.map((index) => index + 1).join(" → "),
    },
    menu: {
      open: openMenu,
      disabled: controlsLocked,
      sourceIcon,
      sourceLabel: isRobot
        ? translate("robot.workspace.messages.inRobotMemory")
        : translate("robot.workspace.messages.inApp"),
      sourceHighlighted: openMenu === "source" || isRobot,
      scopeIcon: scopeLabel.c,
      scopeLabel: scopeLabel.t,
      scopeTip: isRobot
        ? translate("robot.workspace.messages.robotMemorySupportsFullSequencePlaybackOnly")
        : translate("robot.workspace.messages.chooseWhichPosesToPlay"),
      scopeDisabled: controlsLocked || isRobot,
      scopeFixedToRobot: isRobot,
      scopeHighlighted: openMenu === "scope" || !!state.rangeOn || isRange,
      showSourceChoices: openMenu === "all" || openMenu === "source",
      showScopeChoices: openMenu === "all" || openMenu === "scope",
      showDivider: openMenu === "all",
      showRobotScopeNote: isRobot && openMenu === "all",
      sourceChoices: buildSourceChoices(context),
      scopeChoices: buildScopeChoices(context),
    },
    loop: {
      count: state.loops,
      forever: loopInfEff,
      disabled: lockOff,
      menuOpen: !!state.loopMenuOpen && !lockOff,
      label: loopInfEff ? translate("robot.workspace.messages.loopForever") : translate("common.messages.times"),
      tip: loopInfEff
        ? translate("robot.workspace.messages.loopForeverTapToChooseRepeatCount")
        : translate("robot.workspace.messages.chooseRepeatCountOrLoopForever"),
      foreverDetail: state.src === "robot"
        ? translate("robot.workspace.messages.stopWithTheScreenSStopButtonWhileConnected")
        : translate("robot.workspace.messages.doesNotStopAutomaticallyPressStop"),
    },
    preflight: {
      open: !!state.preflight,
      checked: !!state.checked,
      shakes: state.preflightShakes ?? 0,
      hint: state.checked
        ? translate("robot.workspace.messages.readyPressTheBlueStartPlaybackButtonTapElsewhere")
        : translate("robot.workspace.messages.tickTheCheckboxToEnableStartPlaybackTapElsewhere"),
    },
  };
}

export type PlaybackBarView = ReturnType<typeof buildPlaybackBarView>;
