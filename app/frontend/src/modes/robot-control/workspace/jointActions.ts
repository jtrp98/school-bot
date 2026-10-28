import type { ChangeEvent, KeyboardEvent, MouseEvent } from "react";
import { t as translate } from "../../../i18n";
import type { WorkspaceLogic } from "./WorkspaceLogic";
import { copyTextToClipboard } from "./clipboard";
import { deriveWorkspaceFlags, type FlagState } from "./workspaceFlags";

type JointActionState = FlagState & {
  jsel: number;
  holding: number | null;
  jnames: string[];
  valDraft: { j: number; s: string } | null;
  reading: boolean;
  relRead?: number[];
  cur: number[];
  timeMode: "same" | "split";
};

const LAST_JOINT = 5;
const TIME_FLASH_MS = 700;
const READ_UNDO_TOAST_MS = 5000;

// Sample mode copies the displayed joint value into the pose. NativeWorkspace overrides readJoint to ask the servo,
// so this path never runs against hardware.
export function readJointInSample(screen: WorkspaceLogic, joint: number, event?: { stopPropagation(): void }): void {
  event?.stopPropagation();
  const state: JointActionState = screen.state;
  if (!state.release) {
    screen.relOpen();
    return;
  }
  const { sp, seq } = deriveWorkspaceFlags(state);
  if (!sp || state.p0sel || state.reading) return;
  const key = seq[state.sel];
  const previous = sp.v.slice();
  const index = state.sel;
  const spec = screen.SV[joint];
  const value = Math.round(state.cur[joint]);
  const was = screen.num(previous[joint], 0);
  const next = previous.slice();
  next[joint] = value;
  screen.setPose(key, { v: next });
  screen.relPoke();
  screen.setState({ relRead: (state.relRead || []).concat([joint]) });
  screen.addLog("pose", translate("robot.workspace.messages.saveValue0Value1ToPoseValue2", {
    value0: spec.sid,
    value1: state.jnames[joint],
    value2: index + 1,
  }));
  const undo = () => {
    screen.setPose(key, { v: previous });
    const remaining = (screen.state.relRead || []).filter((read: number) => read !== joint);
    screen.setState({ toast: null, relRead: remaining });
  };
  screen.showToast(
    translate("robot.workspace.messages.savedValue0Value1AsValue2InPoseValue3Was", {
      value0: spec.sid,
      value1: state.jnames[joint],
      value2: screen.fmt(joint, value),
      value3: index + 1,
      value4: screen.fmt(joint, was),
    }),
    undo,
    translate("common.messages.undo"),
    null,
    "",
    READ_UNDO_TOAST_MS,
  );
}

// Handlers read the screen state when they run, so one instance serves every render without stale closures.
export function createJointActions(screen: WorkspaceLogic) {
  const state = (): JointActionState => screen.state;
  const set = (patch: Record<string, unknown>) => screen.setState(patch);

  // With nothing to edit yet, touching a joint draws attention to "Add pose" instead of doing nothing.
  const nudgeWhenSequenceIsEmpty = () => {
    const { sp, n, busy } = deriveWorkspaceFlags(state());
    if (!sp && n === 0 && !busy) screen.nudgeAdd();
  };

  const stepFromKey = (joint: number, event: KeyboardEvent<HTMLElement>) => {
    screen.stepVal(joint, event.key === "ArrowRight" ? 1 : -1, event.shiftKey);
  };

  return {
    pickArmDot: (joint: number) => {
      if (state().holding === null) set({ jsel: joint });
    },
    selectJoint: (joint: number) => {
      nudgeWhenSequenceIsEmpty();
      if (state().holding === null) set({ jsel: joint });
    },
    clickValueBox: (joint: number, event: MouseEvent<HTMLElement>) => {
      event.stopPropagation();
      nudgeWhenSequenceIsEmpty();
      if (state().holding === null && state().jsel !== joint) set({ jsel: joint });
    },
    startRename: (joint: number, event: MouseEvent<HTMLElement>) => {
      event.stopPropagation();
      const current = state();
      if (current.holding === null && current.jsel !== joint) set({ jsel: joint });
      set({ renaming: { k: "j", j: joint }, renameDraft: current.jnames[joint] });
    },
    keyOnCard: (joint: number, event: KeyboardEvent<HTMLElement>) => {
      const target = event.target as HTMLElement;
      if (target.tagName === "INPUT" || target.tagName === "BUTTON") return;
      if (event.key === "ArrowRight" || event.key === "ArrowLeft") {
        event.preventDefault();
        if (state().jsel !== joint) set({ jsel: joint });
        if (!deriveWorkspaceFlags(state()).jogOff) stepFromKey(joint, event);
      } else if (event.key === "ArrowUp" || event.key === "ArrowDown") {
        event.preventDefault();
        const next = Math.max(0, Math.min(LAST_JOINT, joint + (event.key === "ArrowDown" ? 1 : -1)));
        set({ jsel: next });
        screen.dom.focusJointCard(next);
      } else if (event.key === "Enter" || event.key === " ") {
        event.preventDefault();
        set({ jsel: joint });
      }
    },
    editValue: (joint: number, event: ChangeEvent<HTMLInputElement>) => {
      set({ valDraft: { j: joint, s: event.target.value } });
    },
    commitValue: (joint: number) => {
      const draft = state().valDraft;
      if (draft && draft.j === joint) screen.typeVal(joint, draft.s);
    },
    keyOnValue: (joint: number, event: KeyboardEvent<HTMLInputElement>) => {
      if (event.key === "ArrowRight" || event.key === "ArrowLeft") {
        event.preventDefault();
        event.stopPropagation();
        stepFromKey(joint, event);
        return;
      }
      if (event.key === "ArrowUp" || event.key === "ArrowDown") {
        event.stopPropagation();
        return;
      }
      if (event.key === "Enter") {
        event.currentTarget.blur();
      } else if (event.key === "Escape") {
        event.stopPropagation();
        set({ valDraft: null });
        event.currentTarget.blur();
        screen.stopAllHandler();
      }
    },
    step: (joint: number, direction: 1 | -1, event: MouseEvent<HTMLElement>) => {
      event.stopPropagation();
      screen.stepVal(joint, direction, event.shiftKey);
    },
    startHold: (joint: number, direction: 1 | -1) => {
      screen.holdDir = direction;
      screen.startHold(joint, direction);
    },
    changeDuration: (joint: number, event: ChangeEvent<HTMLInputElement>) => {
      const current = state();
      const { sp, seq } = deriveWorkspaceFlags(current);
      if (!sp) return;
      const key = seq[current.sel];
      if (current.timeMode !== "split") {
        screen.setPose(key, { t: event.target.value });
        clearTimeout(screen._tfT);
        screen.setState({ tFlash: joint });
        screen._tfT = screen.tm(() => screen.setState({ tFlash: null }), TIME_FLASH_MS);
        return;
      }
      const ts = sp.ts.slice();
      ts[joint] = event.target.value;
      screen.setPose(key, { ts });
    },
    copyCommand: (joint: number, event: MouseEvent<HTMLElement>) => {
      event.stopPropagation();
      set({ jMenu: null });
      const text = screen.gcodeOne(joint);
      copyTextToClipboard(text);
      const who = `${screen.SV[joint].sid} ${state().jnames[joint]}`;
      screen.openCopy(translate("robot.workspace.messages.copiedValue0Commands", { value0: who }), text, [joint]);
    },
    readJoint: (joint: number, event: MouseEvent<HTMLElement>) => screen.readJoint(joint, event),
  };
}

export type JointActions = ReturnType<typeof createJointActions>;
