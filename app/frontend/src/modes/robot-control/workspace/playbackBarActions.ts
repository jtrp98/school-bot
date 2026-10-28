import { t as translate } from "../../../i18n";

import {
  rangeBounds,
  type PlaybackBarState,
  type PlaybackMenu,
  type PlaybackScope,
  type PlaybackSource,
  type RangeEdge,
} from "./playbackBar";

type PlaybackBarPatch = Partial<PlaybackBarState> & { toast?: null };

export type PlaybackBarHost = {
  getState(): PlaybackBarState;
  setState(patch: PlaybackBarPatch): void;
  showUndoToast(text: string, undo: () => void, durationMs: number): void;
  endGroupRun(): void;
  forgetGroupRun(): void;
};

// Handlers read the host state when they run, so one instance serves every render without stale closures.
export function createPlaybackBarActions(host: PlaybackBarHost) {
  const set = (patch: PlaybackBarPatch) => host.setState(patch);

  const applyRange = (start: number, end: number) => {
    const { seq } = host.getState();
    const last = seq.length - 1;
    const reset = { grpRun: null, mode: "all", picking: false, rangeDone: false };
    host.forgetGroupRun();
    if (start === 0 && end >= last) set({ ...reset, rangeOn: false, rk: null });
    else set({ ...reset, rangeOn: true, rs: start, re: end, rk: [seq[start], seq[end], end === last] });
  };

  const scopeAll = () => set({ rangeOn: false, scopeOpen: false, mode: "all", picking: false });

  const scopePick = () => set({
    mode: "range",
    picking: false,
    rangeDone: false,
    rangeOn: false,
    scopeOpen: false,
    timeEdit: -1,
    renaming: null,
    movePick: -1,
  });

  const rangeCancel = () => {
    const state = host.getState();
    const restore = state.mode !== "range" && state.rangeOn
      ? { rangeOn: true, rs: state.rs, re: state.re, rk: state.rk }
      : null;
    set({ rangeOn: false, mode: "all", picking: false, rangeDone: false, scopeOpen: false, hoverIdx: -1 });
    if (!restore) return;
    host.showUndoToast(
      translate("robot.workspace.messages.rangeClearedPlayAllValue0Poses", { value0: state.seq.length }),
      () => set({ toast: null, ...restore }),
      5000,
    );
  };

  const playSelectedPose = () => {
    const { sel, seq } = host.getState();
    set({ rangeOn: true, mode: "all", picking: false, rs: sel, re: sel, rk: [seq[sel], seq[sel]] });
  };

  const playGroup = (groupId: string) => {
    const { groups, seq } = host.getState();
    const group = groups.find((candidate) => candidate.id === groupId);
    if (!group) return;
    const first = group.keys[0];
    const last = group.keys[group.keys.length - 1];
    set({
      rangeOn: true,
      mode: "all",
      picking: false,
      rs: seq.indexOf(first),
      re: seq.indexOf(last),
      rk: [first, last],
    });
  };

  const runScope = (scope: PlaybackScope) => {
    if (scope.kind === "all") scopeAll();
    else if (scope.kind === "selectedPose") playSelectedPose();
    else if (scope.kind === "group") playGroup(scope.groupId);
    else if (host.getState().mode === "range") rangeCancel();
    else scopePick();
  };

  return {
    applyRange,
    scopeAll,
    scopePick,
    rangeCancel,
    pickRangeStart: (index: number) => {
      set({ rangeListOpen: null });
      applyRange(index, rangeBounds(host.getState()).end);
    },
    pickRangeEnd: (index: number) => {
      set({ rangeListOpen: null });
      applyRange(rangeBounds(host.getState()).start, index);
    },
    toggleRangeList: (edge: RangeEdge) => {
      const open = host.getState().rangeListOpen;
      set({ rangeListOpen: open === edge ? null : edge, playbackMenu: null });
    },
    closeRangeList: () => set({ rangeListOpen: null }),
    toggleMenu: (menu: PlaybackMenu) => {
      const open = host.getState().playbackMenu;
      set({ playbackMenu: open === menu ? null : menu });
    },
    closeMenu: () => set({ playbackMenu: null }),
    pickSource: (source: PlaybackSource) => {
      const state = host.getState();
      const toRobot = source === "robot";
      set({
        src: source,
        playbackMenu: state.playbackMenu === "all" ? "all" : null,
        rangeOn: toRobot ? false : state.rangeOn,
        mode: toRobot ? "all" : state.mode,
      });
    },
    pickScope: (scope: PlaybackScope) => {
      runScope(scope);
      set({ playbackMenu: null });
    },
    toggleLoopMenu: () => set({ loopMenuOpen: !host.getState().loopMenuOpen }),
    closeLoopMenu: () => set({ loopMenuOpen: false }),
    chooseLoopCount: () => set({ loopInf: false, loopMenuOpen: false }),
    chooseLoopForever: () => set({ loopInf: true, loopMenuOpen: false }),
    decreaseLoops: () => set({ loops: Math.max(1, host.getState().loops - 1) }),
    increaseLoops: () => set({ loops: Math.min(9, host.getState().loops + 1) }),
    togglePreflightCheck: () => set({ checked: !host.getState().checked }),
    cancelPreflight: () => {
      set({ preflight: false });
      host.endGroupRun();
    },
  };
}

export type PlaybackBarActions = ReturnType<typeof createPlaybackBarActions>;
