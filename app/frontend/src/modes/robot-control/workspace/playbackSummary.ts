import { t as translate } from "../../../i18n";
import type { WorkspaceLogic } from "./WorkspaceLogic";
import { deriveListFlags, type PoseGroup } from "./poseListState";
import { wrapIndexes, type PoseStatusState } from "./poseStatus";
import type { createPoseStatus } from "./poseStatus";

type RobotSequence = { n: number; ms: number; names?: string; at?: string };

export type PlaybackSummaryState = PoseStatusState & {
  rangeOn: boolean;
  loops: number;
  loopInf: boolean;
  lap: number;
  robotSeq: RobotSequence | null;
  checked: boolean;
  grpRun: string | null;
};

export type PlaySlotKind = "none" | "pause" | "resume" | "run" | "start";
export type PlaySlot = { label: string; off: boolean; pause: boolean; kind: PlaySlotKind };

function seconds(ms: number): string {
  return (ms / 1000).toFixed(1);
}

function loopPhrase(state: PlaybackSummaryState): string {
  if (state.loopInf) return translate("robot.workspace.messages.loopForever");
  if (state.loops > 1) return translate("robot.workspace.playback.repeatTimes", { count: state.loops });
  return translate("robot.workspace.playback.oneRound");
}

function invalidPoseIndexes(screen: WorkspaceLogic, state: PlaybackSummaryState): number[] {
  return state.seq
    .map((key, index) => (screen.poseOk(state.poses[key]) ? -1 : index))
    .filter((index) => index >= 0);
}

// What the play slot does right now. The label and the action come from one rule so the button never says one thing
// and does another.
export function derivePlaySlot(screen: WorkspaceLogic, state: PlaybackSummaryState): PlaySlot {
  const { st, busy, noSeq, isOrder, isRange } = deriveListFlags(state);
  const compact = (screen.props.bar || "now") !== "now";
  const anyInvalid = invalidPoseIndexes(screen, state).length > 0;
  let slot: PlaySlot;
  if (st === "preparing") {
    slot = { label: translate("robot.workspace.messages.preparing"), off: true, pause: false, kind: "none" };
  } else if (st === "playing") {
    const label = compact
      ? translate("robot.workspace.messages.pause")
      : translate("robot.workspace.messages.pausePlayback");
    slot = { label, off: false, pause: true, kind: "pause" };
  } else if (st === "paused") {
    slot = { label: translate("robot.workspace.messages.resume"), off: false, pause: false, kind: "resume" };
  } else {
    const off = state.src === "robot"
      ? !state.robotSeq || busy
      : noSeq || busy || anyInvalid || isOrder || (isRange && !state.rangeDone && !state.picking);
    slot = { label: translate("common.play"), off, pause: false, kind: "run" };
  }
  if (state.preflight) {
    slot = {
      label: translate("robot.workspace.messages.startPlayback"),
      off: !state.checked,
      pause: false,
      kind: "start",
    };
  }
  return slot;
}

function summaryOf(state: PlaybackSummaryState, status: ReturnType<typeof createPoseStatus>, noSeq: boolean): string {
  const { order } = status;
  const { isRange, n, running } = deriveListFlags(state);
  const loop = loopPhrase(state);
  if (state.robotRun) return translate("robot.workspace.messages.robotIsPlayingFromItsMemory");
  if (state.src === "robot" && state.robotSeq && !running) {
    return translate("robot.workspace.messages.willPlayValue0PosesFromRobotMemory", { value0: state.robotSeq.n });
  }
  if (noSeq) return translate("robot.workspace.messages.noPosesToPlay");
  if (isRange) {
    if (state.rangeDone) {
      const covered = wrapIndexes(n, state.rs, state.re).map((index) => index + 1).join(" → ");
      return translate("robot.workspace.messages.rangeSelectedWillPlayPosesValue0", { value0: covered });
    }
    return state.picking
      ? translate("robot.workspace.messages.startSelectedPoseValue0", { value0: state.rs + 1 })
      : translate("robot.workspace.messages.noRangeSelected");
  }
  if (!state.rangeOn) {
    return translate("robot.workspace.messages.willPlayAllValue0PosesValue1", { value0: n, value1: loop });
  }
  if (order.length === 1) {
    return translate("robot.workspace.messages.willPlayOnlyPoseValue0Value1", { value0: order[0] + 1, value1: loop });
  }
  return translate("robot.workspace.messages.willPlayPosesValue0Value1Value2", {
    value0: order[0] + 1,
    value1: order[order.length - 1] + 1,
    value2: loop,
  });
}

export function buildPlaybackSummary(
  screen: WorkspaceLogic,
  state: PlaybackSummaryState,
  status: ReturnType<typeof createPoseStatus>,
) {
  const flags = deriveListFlags(state);
  const { st, running, isRange, isOrder, seq, noSeq } = flags;
  const { order, preview, hover, inLoop, loopLength, current, currentMs } = status;
  const badIdx = invalidPoseIndexes(screen, state);
  const anyBad = badIdx.length > 0;
  const loopInfEff = !!state.loopInf;
  const isRobot = state.src === "robot";
  const poseMs = (index: number) => screen.dur(state.poses[seq[index]]);
  const totalOrderSeconds = () => {
    const orderDurationMs = order.reduce((sum, index) => sum + poseMs(index), 0);
    return seconds(orderDurationMs * (loopInfEff ? 1 : state.loops));
  };
  const unit = loopInfEff
    ? translate("robot.workspace.playback.secondsPerLoop")
    : translate("robot.workspace.messages.seconds");

  let summaryHint = "";
  let hintBad = false;
  if (anyBad) {
    summaryHint = translate("robot.workspace.messages.poseValue0HasInvalidValuesFixTheRedFields", {
      value0: badIdx.map((index) => index + 1).join(", "),
    });
    hintBad = true;
  } else if (isOrder && state.movePick >= 0) {
    summaryHint = translate("robot.workspace.messages.tapPasteHereToMoveOrTheOriginalPose");
  } else if (isOrder && state.dragFrom >= 0) {
    summaryHint = state.dropGap >= 0
      ? translate("robot.workspace.messages.releaseToPlaceValue0AsPoseValue1", {
        value0: state.poses[seq[state.dragFrom]].name,
        value1: state.dragFrom < state.dropGap ? state.dropGap : state.dropGap + 1,
      })
      : translate("robot.workspace.messages.dragBetweenPosesToRevealAnInsertionTarget");
  } else if (isOrder) {
    summaryHint = translate("robot.workspace.messages.reorderModeDragAPoseToANewPosition");
  } else if (isRange && state.picking && hover >= 0) {
    summaryHint = translate("robot.workspace.messages.tapPoseValue0WillPlayValue1", {
      value0: hover + 1,
      value1: preview.map((index) => index + 1).join(" → "),
    });
  } else if (isRange && state.picking) {
    summaryHint = translate("robot.workspace.messages.tapTheEndPoseOrThisPoseOnlyTap");
  } else if (isRange && state.rangeDone) {
    summaryHint = translate("robot.workspace.messages.pressPlayRangeTapAnotherPoseToReselectOr");
  } else if (isRange) {
    summaryHint = translate("robot.workspace.messages.tapTheStartPoseIfTheEndComesBefore");
  } else if (state.rangeOn && !noSeq) {
    summaryHint = translate("robot.workspace.messages.value0SequenceValue1TotalValue2Value3", {
      value0: state.grpRun ? translate("robot.workspace.groups.runningGroupPrefix", { name: state.grpRun }) : "",
      value1: order.map((index) => index + 1).join(" → "),
      value2: totalOrderSeconds(),
      value3: unit,
    });
  } else if (!noSeq) {
    summaryHint = translate("robot.workspace.messages.totalValue0Value1", {
      value0: totalOrderSeconds(),
      value1: unit,
    });
  }
  const summary = summaryOf(state, status, noSeq);
  if (state.src === "robot" && state.robotSeq && !running) {
    summaryHint = translate("robot.workspace.messages.value0Value1Seconds", {
      value0: state.robotSeq.names || "",
      value1: seconds(state.robotSeq.ms),
    });
  }
  if (state.robotRun) summaryHint = translate("robot.workspace.messages.youCanPressStopAllAtAnyTime");

  const groupScope: PoseGroup | null | undefined = state.rangeOn
    ? state.groups.find((group) => {
      const indexes = group.keys.map((key) => seq.indexOf(key));
      return indexes[0] === state.rs && indexes[indexes.length - 1] === state.re;
    })
    : null;
  const playScopeLabel = scopeLabelOf(state, flags, groupScope);
  const loopNumber = loopInfEff ? state.lap || 0 : Math.min(Math.floor(state.pos / loopLength), state.loops - 1);
  const runTitleNow = playbackTitle(st, current, inLoop, loopLength);
  const progress = loopInfEff
    ? translate("robot.workspace.playback.roundProgress", { number: loopNumber + 1 })
    : translate("robot.workspace.playback.timesProgress", { current: loopNumber + 1, total: state.loops });
  const runSubNow = st === "preparing"
    ? translate("robot.workspace.messages.checkConnectionLabel")
    : translate("robot.workspace.messages.value0ThisPoseValue1Ms", { value0: progress, value1: currentMs });
  const stepMs = (index: number) => screen.dur(state.poses[seq[index]]);
  const totalMs = Math.max(state.steps.reduce((sum, index) => sum + stepMs(index), 0), 1);
  const doneMs = state.steps.slice(0, state.pos).reduce((sum, index) => sum + stepMs(index), 0) + state.elapsed;
  const preflightView = state.preflight;
  const runningView = !preflightView && running;
  const stoppedView = !preflightView && st === "stopped";
  const slot = derivePlaySlot(screen, state);

  return {
    badIdx,
    anyBad,
    summary,
    summaryHint,
    hintBad,
    barMode: screen.props.bar || "now",
    isRobot,
    grpScope: groupScope,
    playScopeLabel,
    loopInfEff,
    vRunNow: running || !!state.robotRun,
    runTitleNow,
    runSubNow,
    totalMs,
    doneMs,
    vPreflight: preflightView,
    vRunning: runningView,
    vStopped: stoppedView,
    vControls: !preflightView && !runningView && !stoppedView,
    slotLabel: slot.label,
    slotOff: slot.off,
    slotPause: slot.pause,
  };
}

function playbackTitle(status: string, current: number, inLoop: number, loopLength: number): string {
  if (status === "preparing") return translate("robot.workspace.messages.preparing");
  if (status === "paused") return translate("robot.workspace.playback.pausedAtPose", { number: current + 1 });
  return translate("robot.workspace.playback.playingPoseProgress", {
    number: current + 1,
    round: inLoop + 1,
    rounds: loopLength,
  });
}

function scopeLabelOf(
  state: PlaybackSummaryState,
  flags: ReturnType<typeof deriveListFlags>,
  groupScope: PoseGroup | null | undefined,
): string {
  if (state.src === "robot") return translate("robot.workspace.messages.fullSequence");
  if (flags.isRange) return translate("robot.workspace.messages.selectingRange");
  if (!state.rangeOn) return translate("robot.workspace.messages.all");
  if (groupScope) return `📦 ${groupScope.name}`;
  if (state.rs === state.re) return translate("robot.workspace.playback.scopeSinglePose", { number: state.rs + 1 });
  return translate("robot.workspace.playback.scopeRange", { from: state.rs + 1, to: state.re + 1 });
}
