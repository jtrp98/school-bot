import type { KeyboardEvent } from "react";
import { t as translate } from "../../../i18n";
import type { WorkspaceLogic } from "./WorkspaceLogic";
import type { GroupSelection, SequenceMode } from "./workspaceToolbarFields";

type PoseGroup = { id: string; name: string; keys: string[]; open: boolean };

type ToolbarState = {
  seq: string[];
  poses: Record<string, unknown>;
  groups: PoseGroup[];
  sel: number;
  linked: number;
  counter: number;
  rs: number;
  re: number;
  rangeOn: boolean;
  rk?: unknown;
  grpNaming: boolean;
  grpDraft: string;
  t6full: boolean;
};

// Everything the user can still undo or cancel while managing poses.
type ManageSnapshot = Pick<ToolbarState, "seq" | "poses" | "groups" | "sel" | "linked" | "counter">;

const TOAST_SHORT_MS = 2500;
const TOAST_MEDIUM_MS = 4000;
const TOAST_LONG_MS = 6000;

export function createToolbarActions(screen: WorkspaceLogic) {
  const set = (patch: Record<string, unknown>) => screen.setState(patch);
  const state = (): ToolbarState => screen.state;
  const undoLabel = () => translate("common.messages.undo");
  // Kept in the closure, not on the screen, so it disappears together with this workspace.
  let manageSnapshot: ManageSnapshot | null = null;

  const makeGroup = (selection: GroupSelection) => {
    const current = state();
    if (!selection.ok) return;
    const keys = selection.indices.map((index) => current.seq[index]);
    const previousSeq = current.seq.slice();
    const insertAt = selection.indices[0];
    const gathered = () => {
      const picked = new Set(keys);
      const rest = previousSeq.filter((key) => !picked.has(key));
      rest.splice(insertAt, 0, ...keys);
      return rest;
    };
    const nextSeq = selection.contiguous ? previousSeq : gathered();
    const reordered = nextSeq.some((key, index) => key !== previousSeq[index]);
    const remap = (index: number) => (index < 0 ? index : nextSeq.indexOf(previousSeq[index]));
    const moveInfo = reordered
      ? { sel: remap(current.sel), linked: remap(current.linked), synced: false }
      : {};

    const extended = selection.extendedGroup;
    if (extended) {
      set({
        groups: current.groups.map((group) => (group.id === extended.id ? { ...group, keys } : group)),
        seq: nextSeq,
        picks: [],
        ...moveInfo,
      });
      screen.addLog(
        "order",
        translate("robot.workspace.toolbar.logAddToGroup", {
          context: reordered ? "pulled" : undefined,
          name: extended.name,
          from: insertAt + 1,
          to: insertAt + keys.length,
        }),
      );
      screen.showToast(
        translate("robot.workspace.messages.addedValue0PosesToValue1NowValue2PosesPress", {
          value0: selection.singlesCount,
          value1: extended.name,
          value2: keys.length,
        }),
        null,
        "",
        null,
        "",
        TOAST_MEDIUM_MS,
      );
      return;
    }
    if (!current.grpNaming) {
      set({ grpNaming: true, grpDraft: "" });
      return;
    }
    const name = current.grpDraft.trim();
    if (!name || current.groups.some((group) => group.name === name)) return;
    const group: PoseGroup = { id: "g" + Date.now(), name, keys, open: false };
    set({
      groups: screen.fixGroups(nextSeq, current.groups).concat([group]),
      seq: nextSeq,
      picks: [],
      grpNaming: false,
      grpDraft: "",
      ...moveInfo,
    });
    const pulled = reordered ? "pulled" : undefined;
    screen.addLog(
      "order",
      translate("robot.workspace.toolbar.logCreateGroup", {
        context: pulled,
        name,
        count: keys.length,
        after: insertAt + 1,
      }),
    );
    screen.showToast(
      translate("robot.workspace.toolbar.toastGroupCreated", { context: pulled, name, after: insertAt + 1 }),
      null,
      "",
      null,
      "",
      TOAST_MEDIUM_MS,
    );
  };

  return {
    showCards: () => set({ view: "cards" }),
    showTable: () => set({ view: "table" }),
    pickSequenceMode: (mode: SequenceMode) => {
      if (mode === "range") set({ mode, rs: 0, re: Math.max(state().seq.length - 1, 0), picking: false });
      else set({ mode, picking: false });
    },
    finishClipboardMode: () => screen.pDone(),
    toggleTableFullscreen: () => set({ t6full: !state().t6full }),

    saveRangeAsGroup: () => {
      const current = state();
      const first = current.rs;
      const last = current.re;
      const keys = current.seq.slice(first, last + 1);
      const rangeLabel = { value0: first + 1, value1: last + 1 };
      let name = translate("robot.workspace.messages.rangePosesValue0Value1", rangeLabel);
      let suffix = 2;
      while (current.groups.some((group) => group.name === name)) {
        name = translate("robot.workspace.messages.rangePosesValue0Value1Value2", { ...rangeLabel, value2: suffix++ });
      }
      const group: PoseGroup = { id: "g" + Date.now(), name, keys, open: false };
      const previous = { groups: current.groups, rangeOn: current.rangeOn, rs: current.rs, re: current.re, rk: current.rk };
      set({
        groups: current.groups.concat([group]),
        mode: "all",
        picking: false,
        rangeDone: false,
        hoverIdx: -1,
        rangeOn: true,
        rs: first,
        re: last,
        rk: [keys[0], keys[keys.length - 1]],
        gMenu: group.id,
        gRen: group.id,
        gRenDraft: name,
        gFocus: group.id,
      });
      screen.addLog("order", translate("robot.workspace.messages.savePosesValue0Value1AsAGroup", rangeLabel));
      screen.showToast(
        translate("robot.workspace.messages.savedAsAGroupNameItNowThenSelect"),
        () => set({ toast: null, gMenu: null, gRen: null, ...previous }),
        undoLabel(),
        null,
        "",
        TOAST_LONG_MS,
      );
    },

    startManagingPoses: () => {
      const { seq, poses, groups, sel, linked, counter } = state();
      manageSnapshot = { seq: seq.slice(), poses, groups, sel, linked, counter };
      set({
        selMode: true,
        mode: "order",
        picks: [],
        grpNaming: false,
        timeEdit: -1,
        renaming: null,
        movePick: -1,
        scopeOpen: false,
      });
    },
    cancelManagingPoses: () => {
      const snapshot = manageSnapshot;
      manageSnapshot = null;
      set({
        selMode: false,
        mode: "all",
        picks: [],
        grpNaming: false,
        movePick: -1,
        renaming: null,
        msgPop: null,
        landed: null,
        moved: null,
        ...snapshot,
      });
      if (!snapshot) return;
      screen.showToast(
        translate("robot.workspace.messages.cancelPoseManagementAndRestoreThePreviousState"),
        null,
        "",
        null,
        "",
        TOAST_SHORT_MS,
      );
    },
    finishManagingPoses: () => {
      const snapshot = manageSnapshot;
      manageSnapshot = null;
      set({ selMode: false, mode: "all", picks: [], grpNaming: false, movePick: -1, renaming: null, msgPop: null });
      if (!snapshot) return;
      const current = state();
      const moved = current.seq.filter((key, index) => snapshot.seq[index] !== key).length;
      const created = current.groups.filter((group) => !snapshot.groups.some((old) => old.id === group.id)).length;
      const edited = current.groups.filter((group) => {
        const old = snapshot.groups.find((candidate) => candidate.id === group.id);
        return !!old && old.keys.join() !== group.keys.join();
      }).length;
      if (!moved && !created && !edited) return;
      const summary: string[] = [];
      if (moved) summary.push(translate("robot.workspace.messages.moveValue0Poses", { value0: moved }));
      if (created) summary.push(translate("robot.workspace.messages.createValue0Groups", { value0: created }));
      if (edited) summary.push(translate("robot.workspace.messages.editValue0Groups", { value0: edited }));
      const changes = summary.join(" · ");
      screen.addLog("order", translate("robot.workspace.messages.poseManagementCompleteValue0", { value0: changes }));
      screen.showToast(
        translate("robot.workspace.messages.poseManagementCompleteValue0Label", { value0: changes }),
        () => {
          set({ toast: null, ...snapshot });
          screen.addLog("order", translate("robot.workspace.messages.undoPoseManagement"));
        },
        undoLabel(),
        null,
        "",
        TOAST_LONG_MS,
      );
    },

    setGroupDraft: (draft: string) => set({ grpDraft: draft }),
    handleGroupNameKey: (event: KeyboardEvent<HTMLInputElement>, selection: GroupSelection) => {
      if (event.key === "Enter") makeGroup(selection);
      else if (event.key === "Escape") set({ grpNaming: false });
    },
    makeGroup,
  };
}

export type ToolbarActions = ReturnType<typeof createToolbarActions>;
