import type { WorkspaceLogic } from "./WorkspaceLogic";
import { deriveListFlags, type PoseListState } from "./poseListState";
import type { FlagPose } from "./workspaceFlags";

export type PoseStatusState = PoseListState & {
  pos: number;
  elapsed: number;
  steps: number[];
  src: string;
  robotRun: boolean;
};

export type PoseStatus = {
  order: number;
  inRange: boolean;
  valid: boolean;
  emphasis: boolean;
  showRing: boolean;
  showCheck: boolean;
  ringDash: number;
  dim: boolean;
};

const DEFAULT_POSE_MS = 3000;
const RING_CIRCUMFERENCE = 57;

// Indexes from `from` to `to`, wrapping past the end of the sequence.
export function wrapIndexes(count: number, from: number, to: number): number[] {
  const indexes: number[] = [];
  if (!count) return indexes;
  for (let index = from, visited = 0; visited < count; index = (index + 1) % count, visited++) {
    indexes.push(index);
    if (index === to) break;
  }
  return indexes;
}

// What every pose card and row shows about playback: which poses a range or run covers, and where playback is.
export function createPoseStatus(screen: WorkspaceLogic, state: PoseStatusState) {
  const { st, running, isRange, isOrder, seq, n } = deriveListFlags(state);
  const order: number[] = screen.order();
  const loopLength = Math.max(order.length, 1);
  const inLoop = state.pos % loopLength;
  const current = order.length ? order[inLoop] : -1;
  const currentMs = current >= 0 ? screen.dur(state.poses[seq[current]]) : DEFAULT_POSE_MS;
  const hover = isRange && state.hoverIdx >= 0 && state.hoverIdx < n ? state.hoverIdx : -1;
  const chosenRange = state.rangeDone ? wrapIndexes(n, state.rs, state.re) : [];
  let hoveredRange = state.rangeDone ? chosenRange : [hover];
  if (state.picking) hoveredRange = wrapIndexes(n, state.rs, hover);
  let preview = order;
  if (isRange) {
    if (hover >= 0) preview = hoveredRange;
    else preview = state.picking ? [state.rs] : chosenRange;
  }
  const playVisible = !isOrder && state.src !== "robot" && !state.robotRun && (running || !!state.preflight);
  const currentStep = running && state.steps && state.pos < state.steps.length ? state.steps[state.pos] : -1;
  const playing = st === "playing" || st === "paused";

  const read = (index: number, pose: FlagPose): PoseStatus => {
    const position = preview.indexOf(index);
    const inRange = position >= 0;
    const isCurrent = playing && inRange && index === current;
    const finished = playing && inRange && !isCurrent && position < inLoop;
    return {
      order: position,
      inRange,
      valid: screen.poseOk(pose),
      emphasis: isCurrent,
      showRing: isCurrent,
      showCheck: finished,
      ringDash: isCurrent ? Math.round((state.elapsed / currentMs) * RING_CIRCUMFERENCE) : 0,
      dim: ((isRange && (state.picking || hover >= 0)) || running || playVisible) && !inRange,
    };
  };

  return { order, loopLength, inLoop, current, currentMs, hover, preview, playVisible, currentStep, read };
}

export type PoseStatusReader = ReturnType<typeof createPoseStatus>;
