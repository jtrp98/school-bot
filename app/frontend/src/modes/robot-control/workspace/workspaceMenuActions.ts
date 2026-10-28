import type { KeyboardEvent, MouseEvent } from "react";
import { t as translate, type Language } from "../../../i18n";
import type { WorkspaceLogic } from "./WorkspaceLogic";

export type LeaveTarget = "work" | "new" | "teacher" | "home";

type PoseGroup = { id: string; name: string; keys: string[]; open: boolean };

type Pose = { name: string };

type GroupMenuState = { gid: string; x: number; y: number };
type PoseMenuState = { i: number; x: number; y: number };

type MenuState = {
  seq: string[];
  poses: Record<string, Pose>;
  groups: PoseGroup[];
  view: "cards" | "table";
  ctx: GroupMenuState | null;
  pctx: PoseMenuState | null;
  menuOpen: boolean;
  projDraft: string;
  projName: string;
  saveAsMode: boolean;
  saveAfterName: boolean;
  projRenaming: boolean;
  netMeas: boolean;
  netSt: "ok" | "slow" | "off";
};

const POSE_DURATION_MS = 3000;
const JOINT_COUNT = 6;
const LEAVE_TARGET_TITLE_KEYS: Record<LeaveTarget, string> = {
  work: "robot.workspace.messages.myProjects",
  new: "home.create.title",
  teacher: "robot.teacher.title",
  home: "nav.home",
};
const TABLE_RENAME_FOCUS_DELAY_MS = 30;
const POSE_ZERO_MENU_GAP = 6;

// Handlers read the screen state when they run, and call screen methods at call time so the NativeWorkspace
// overrides (hardware-aware stop, native test, native leave) are the ones that execute.
export function createMenuActions(screen: WorkspaceLogic) {
  const set = (patch: Record<string, unknown>) => screen.setState(patch);
  const menu = (): MenuState => screen.state;
  const groupOfMenu = (): PoseGroup | undefined => {
    const gid = menu().ctx?.gid;
    return menu().groups.find((group) => group.id === gid);
  };
  const poseIndexOfMenu = (): number => menu().pctx?.i ?? -1;
  const undoLabel = () => translate("common.messages.undo");
  const freshPoseZero = () => ({
    ...screen.POSE0,
    name: translate("robot.workspace.messages.uprightArm"),
    t: POSE_DURATION_MS,
    ts: Array<number>(JOINT_COUNT).fill(POSE_DURATION_MS),
  });

  return {
    startProjectRename: () => set({ projRenaming: true, projDraft: menu().projName || "", menuOpen: false }),
    setProjectDraft: (draft: string) => set({ projDraft: draft }),
    handleProjectNameKey: (event: KeyboardEvent<HTMLInputElement>) => {
      if (event.key === "Enter") event.currentTarget.blur();
      else if (event.key === "Escape") set({ projRenaming: false, saveAfterName: false, saveAsMode: false });
    },
    commitProjectName: () => {
      const state = menu();
      const name = (state.projDraft || "").trim();
      if (state.saveAsMode) {
        if (!state.projRenaming) return;
        if (!name) {
          set({ projRenaming: false, saveAsMode: false });
          return;
        }
        set({ projRenaming: false, saveAsMode: false, projName: name });
        set({ savedSnap: screen.snapOf(screen.state), savedAt: screen.now2() });
        screen.showToast(
          translate("robot.workspace.messages.savedAsValue0OriginalProjectValue1IsKept", {
            value0: name,
            value1: state.projName || "",
          }),
          null,
          "",
          null,
          "",
          4000,
        );
        return;
      }
      set({ projRenaming: false, saveAfterName: false, projName: name || state.projName });
      if (state.saveAfterName && name) screen.tm(() => screen.saveProj(), 0);
    },

    toggleMainMenu: () => set({ menuOpen: !menu().menuOpen }),
    closeMenus: () => set({ menuOpen: false, gMenu: null, gRen: null }),
    closeJointMenu: () => set({ jMenu: null }),
    // Always asks first for Home; the other targets skip the dialog when nothing is running or unsaved.
    leaveTo: (target: LeaveTarget, askFirst: boolean) => {
      if (askFirst || target === "home") {
        set({ menuOpen: false, leaveAsk: { kind: target, to: translate(LEAVE_TARGET_TITLE_KEYS[target]) } });
        return;
      }
      screen.navTo(target);
    },
    openMerge: () => screen.openMerge(),
    save: () => {
      set({ menuOpen: false });
      screen.saveProj();
    },
    saveAs: () => screen.saveAs(),
    openActivityLog: () => set({ menuOpen: false, logOpen: true }),
    chooseLogFilter: (filter: string) => set({ logFilter: filter }),
    askClear: () => set({ clearAsk: true, menuOpen: false }),
    setLanguage: (language: Language) => {
      set({ lang: language });
      screen.props.onLang?.(language);
    },
    openExternal: (url: string) => screen.openExternal(url),
    navTo: (kind: string) => screen.navTo(kind),
    leaveNav: () => screen.leaveNav(),
    leaveSaveNav: () => screen.leaveSaveNav(),
    openMainMenu: () => {
      set({ menuOpen: false });
      screen.props.onOpenMenu?.();
    },

    explainNetworkFromMenu: (event: MouseEvent) => {
      const state = menu();
      if (state.netMeas || state.netSt === "ok") return;
      event.stopPropagation();
      screen.netPop('[data-net="menu"]');
    },
    explainNetworkFromHeader: () => screen.netPop('[data-net="hdr"]'),

    closeGroupMenu: (event: MouseEvent) => {
      event.preventDefault();
      set({ ctx: null });
    },
    toggleGroupFromMenu: () => {
      const group = groupOfMenu();
      set({ ctx: null });
      if (group) screen.toggleGroup(group.id);
    },
    renameGroupFromMenu: () => {
      const group = groupOfMenu();
      const state = menu();
      set({
        ctx: null,
        gRen: group ? group.id : null,
        gRenDraft: group ? group.name : "",
        gMenu: group && state.view !== "table" ? group.id : null,
      });
    },
    copyGroupFromMenu: () => {
      const group = groupOfMenu();
      set({ ctx: null });
      screen.gCopy(group);
    },
    cutGroupFromMenu: () => {
      const group = groupOfMenu();
      set({ ctx: null });
      if (group) screen.pCopy(true, group.keys.slice());
    },
    ungroupFromMenu: () => {
      const group = groupOfMenu();
      if (!group) return;
      set({ ctx: null, groups: menu().groups.filter((candidate) => candidate !== group), gFocus: null });
      screen.showToast(
        translate("robot.workspace.messages.ungroupedValue0IntoIndividualPoses", { value0: group.name }),
        () => set({ groups: menu().groups.concat([group]), toast: null }),
        undoLabel(),
        null,
        "",
        5000,
      );
    },
    deleteGroupFromMenu: () => {
      const group = groupOfMenu();
      set({ ctx: null });
      screen.gDelete(group);
    },

    closePoseMenu: (event: MouseEvent) => {
      event.preventDefault();
      set({ pctx: null });
    },
    copyPoseFromMenu: () => {
      const index = poseIndexOfMenu();
      set({ pctx: null });
      screen.pCopy(false, [menu().seq[index]]);
    },
    cutPoseFromMenu: () => {
      const index = poseIndexOfMenu();
      set({ pctx: null });
      screen.pCopy(true, [menu().seq[index]]);
    },
    pasteAfterPoseFromMenu: () => {
      const index = poseIndexOfMenu();
      set({ pctx: null });
      screen.pasteClip(index + 1);
    },
    renamePoseFromMenu: () => {
      const state = menu();
      const index = poseIndexOfMenu();
      const key = state.seq[index];
      set({ pctx: null });
      if (key == null) return;
      if (state.view === "table") {
        screen.tm(() => screen.dom.focusTileInput(key), TABLE_RENAME_FOCUS_DELAY_MS);
        return;
      }
      set({ renaming: { k: "p", i: index }, renameDraft: state.poses[key].name, menuIdx: -1 });
    },
    duplicatePoseFromMenu: () => {
      const state = menu();
      const index = poseIndexOfMenu();
      const key = state.seq[index];
      const pose = state.poses[key];
      set({ pctx: null });
      if (!pose) return;
      const copy = {
        ...structuredClone(pose),
        name: translate("robot.workspace.messages.value0Copy", { value0: pose.name }),
      };
      screen.insertPose(
        index + 1,
        copy,
        translate("robot.workspace.messages.duplicatePoseValue0Value1", { value0: index + 1, value1: pose.name }),
        key,
      );
    },
    deletePoseFromMenu: () => {
      const index = poseIndexOfMenu();
      set({ pctx: null });
      if (menu().seq[index]) screen.removeAt(index);
    },

    openPoseZeroMenu: (event: MouseEvent<HTMLElement>, locked: boolean) => {
      event.stopPropagation();
      if (locked) return;
      const anchor = event.currentTarget.getBoundingClientRect();
      const root = screen.dom.root;
      if (!root) {
        set({ p0m6: { x: 0, y: 0 } });
        return;
      }
      const rootRect = root.getBoundingClientRect();
      set({ p0m6: { x: anchor.right - rootRect.left, y: anchor.bottom - rootRect.top + POSE_ZERO_MENU_GAP } });
    },
    closePoseZeroMenu: () => set({ p0m6: null }),
    insertPoseZeroFirst: () => {
      set({ p0m6: null });
      screen.insertPose(0, freshPoseZero(), translate("robot.workspace.messages.usePose0AsPose1"));
    },
    appendPoseZeroLast: () => {
      set({ p0m6: null });
      const count = menu().seq.length;
      screen.insertPose(
        count,
        screen.POSE0,
        translate("robot.workspace.messages.usePose0AsPoseValue0", { value0: count + 1 }),
      );
    },
  };
}

export type MenuActions = ReturnType<typeof createMenuActions>;
