export type FlagPose = {
  name: string;
  v: Array<number | string>;
  t: number | string;
  ts: Array<number | string>;
};

export type FlagState = {
  st: string;
  seq: string[];
  poses: Record<string, FlagPose>;
  sel: number;
  linked: number;
  p0sel: boolean;
  mode: string;
  preflight: boolean;
  far: unknown;
  release: boolean;
};

export type WorkspaceFlags = ReturnType<typeof deriveWorkspaceFlags>;

// One definition of "what the arm is doing" for the view builders and for handlers that run later, so a
// handler decides with the same rules the screen used when it enabled or disabled the control.
export function deriveWorkspaceFlags(state: FlagState) {
  const { st } = state;
  const isDisc = st === "disconnected" || st === "connecting";
  const running = st === "preparing" || st === "playing" || st === "paused";
  const lockOff = running || st === "moving" || st === "sending" || isDisc || !!state.preflight;
  const busy = lockOff || !!state.far || !!state.release;
  const seq = state.seq;
  const sp: FlagPose | null = state.sel >= 0 ? state.poses[seq[state.sel]] : null;
  const linked = !!sp && state.linked === state.sel;
  const selectingOrReordering = state.mode === "range" || state.mode === "order";
  const needGo = !!sp && !linked && !state.p0sel && !selectingOrReordering;
  const jogOff = busy || !sp || state.p0sel || selectingOrReordering || needGo;
  return {
    st,
    isDisc,
    running,
    busy,
    seq,
    n: seq.length,
    noSeq: seq.length === 0,
    sp,
    linked,
    needGo,
    jogOff,
    lockOff,
  };
}
