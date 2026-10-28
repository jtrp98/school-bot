import type { WorkspaceLogic } from "./WorkspaceLogic";
import { deriveListFlags } from "./poseListState";
import { derivePlaySlot, type PlaybackSummaryState, type PlaySlot } from "./playbackSummary";

// Handlers read the screen state when they run, so one instance serves every render without stale closures.
export function createPlaybackSlotActions(screen: WorkspaceLogic) {
  const state = (): PlaybackSummaryState => screen.state;
  const set = (patch: Record<string, unknown>) => screen.setState(patch);

  const playRange = (start: number, end: number) => {
    const { seq } = state();
    screen._grpPrev = null;
    set({
      mode: "all",
      picking: false,
      rangeDone: false,
      hoverIdx: -1,
      rangeOn: true,
      rs: start,
      re: end,
      rk: [seq[start], seq[end]],
      src: "app",
      grpRun: null,
      selMode: false,
      picks: [],
    });
    if (screen.state.checkedOnce) {
      screen.tm(() => screen.play(), 0);
    } else {
      screen.clearAll();
      set({ preflight: true, checked: false });
    }
  };

  const run = () => {
    const current = state();
    const { isRange } = deriveListFlags(current);
    if (isRange && current.picking) {
      playRange(current.rs, current.rs);
    } else if (isRange && current.rangeDone) {
      playRange(current.rs, current.re);
    } else if (screen.isChecked(current)) {
      screen.playSrc();
    } else {
      screen.clearAll();
      set({ preflight: true, checked: false });
    }
  };

  const start = () => {
    set({ preflight: false, checkedOnce: true, checkedSnap: screen.playSnap(state()) });
    if (screen._grpPrev) screen.play();
    else screen.playSrc();
  };

  return {
    current: (): PlaySlot => derivePlaySlot(screen, state()),
    activate: () => {
      const { kind } = derivePlaySlot(screen, state());
      if (kind === "pause") {
        clearInterval(screen.timer);
        set({ st: "paused" });
      } else if (kind === "resume") {
        set({ st: "playing" });
        screen.tick();
      } else if (kind === "run") {
        run();
      } else if (kind === "start") {
        start();
      }
    },
  };
}

export type PlaybackSlotActions = ReturnType<typeof createPlaybackSlotActions>;
