import { t as translate } from "../../../i18n";
import type { WorkspaceLogic } from "./WorkspaceLogic";
import { deriveListFlags, type PoseListState } from "./poseListState";

type RangeKeys = [string, string] | [string, string, boolean];

type ReconcileState = PoseListState & {
  rangeOn: boolean;
  rk: RangeKeys | null;
};

const RANGE_RESET_TOAST_MS = 4000;

// With poses present and nothing selected, the screen falls back to the last pose the user was on. This runs after a
// commit instead of during render so a render never schedules work.
export function reconcileSelection(screen: WorkspaceLogic): void {
  const state: ReconcileState = screen.state;
  if (state.sel >= 0) {
    screen._lastSel = state.sel;
    return;
  }
  const { running, n, st } = deriveListFlags(state);
  const settled = !state.p0sel && !state.selMode && state.mode === "all" && !running && st !== "sending"
    && !state.preflight;
  if (n === 0 || !settled || screen._autoSelT) return;
  const linkedIndex = state.linked >= 0 && state.linked < n ? state.linked : -1;
  const remembered = Math.max(0, Math.min(n - 1, screen._lastSel ?? 0));
  const want = linkedIndex >= 0 ? linkedIndex : remembered;
  screen._autoSelT = screen.tm(() => {
    screen._autoSelT = null;
    const current: ReconcileState = screen.state;
    const stillEmpty = current.sel < 0 && current.seq.length > 0 && !current.p0sel && !current.selMode
      && current.mode === "all";
    if (stillEmpty) screen.setState({ sel: Math.min(want, current.seq.length - 1) });
  }, 0);
}

// A range remembers its endpoints by pose key, so deleting or moving poses leaves rs/re stale until this realigns them.
export function reconcileRange(screen: WorkspaceLogic): void {
  const state: ReconcileState = screen.state;
  const { running, seq, n } = deriveListFlags(state);
  if (!state.rangeOn || !state.rk || screen._grpPrev || running || screen._rkFix) return;
  const [startKey, endKey, toEnd] = state.rk;
  const start = seq.indexOf(startKey);
  const end = toEnd ? n - 1 : seq.indexOf(endKey);
  if (start < 0 || end < 0) {
    screen._rkFix = true;
    screen.tm(() => {
      screen._rkFix = false;
      screen.setState({ rangeOn: false, rk: null });
      screen.showToast(
        translate("robot.workspace.messages.aRangeEndpointWasDeletedPlaybackResetToAll"),
        null,
        "",
        null,
        "",
        RANGE_RESET_TOAST_MS,
      );
    }, 0);
  } else if (start !== state.rs || end !== state.re) {
    screen._rkFix = true;
    screen.tm(() => {
      screen._rkFix = false;
      screen.setState({ rs: start, re: end });
    }, 0);
  }
}
