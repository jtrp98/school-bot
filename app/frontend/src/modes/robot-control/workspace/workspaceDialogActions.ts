import { t as translate } from "../../../i18n";
import type { WorkspaceLogic } from "./WorkspaceLogic";
import type { CopyPopoverState } from "./workspacePopovers";

type PoseSnapshot = {
  name: string;
  v: number[];
  t: number;
  ts: number[];
};

type PoseGroup = { id: string; name: string; keys: string[]; open: boolean };

type StoredToast = { act?: () => void; act2?: () => void } | null;

type ProjectState = {
  poses: Record<string, PoseSnapshot>;
  seq: string[];
  groups: PoseGroup[];
  counter: number;
  sel: number;
  linked: number;
  rangeOn: boolean;
};

const BACKUP_DURATION_MS = 3000;

// One instance serves every render; handlers read the screen state when they run, so they never act on stale values.
export function createDialogActions(screen: WorkspaceLogic) {
  const set = (patch: Record<string, unknown>) => screen.setState(patch);
  const project = (): ProjectState => screen.state;
  const undoLabel = () => translate("common.messages.undo");

  return {
    undoMove: () => screen.undoMove(),
    closeMessagePopover: () => set({ msgPop: null }),
    closeKeyPopover: () => set({ keyPop: null }),
    runToastPrimary: () => {
      const toast: StoredToast = screen.state.toast;
      toast?.act?.();
    },
    runToastSecondary: () => {
      const toast: StoredToast = screen.state.toast;
      toast?.act2?.();
    },
    toggleCopyDetails: () => {
      const copy: CopyPopoverState | null = screen.state.copyPop;
      if (copy) set({ copyPop: { ...copy, open: !copy.open } });
    },
    closeCopy: () => screen.closeCopy(),

    cancelReleaseConfirm: () => set({ relAsk: false }),
    confirmRelease: () => screen.relStart(),
    keepJointsReleased: () => screen.relCancelEnd(),

    cancelLoopFromBoard: () => set({ robInfAsk: false }),
    confirmLoopFromBoard: () => {
      screen._robInfOk = true;
      set({ robInfAsk: false });
      screen.tm(() => screen.playSrc(), 0);
    },

    dismissRecovery: () => set({ recoverAsk: false }),
    // Reachable only from the "recover" sample scenario: it adds demo poses and never talks to the robot.
    recoverSampleBackup: () => {
      const state = project();
      const key = "p" + state.counter;
      const backupPose: PoseSnapshot = {
        name: translate("robot.workspace.messages.backupPose"),
        v: [20, -30, 40, 10, 0, 60],
        t: BACKUP_DURATION_MS,
        ts: Array<number>(6).fill(BACKUP_DURATION_MS),
      };
      const group: PoseGroup = {
        id: "gR",
        name: translate("robot.workspace.messages.pickUp"),
        keys: [state.seq[1], state.seq[2]],
        open: false,
      };
      set({
        recoverAsk: false,
        poses: { ...state.poses, [key]: backupPose },
        seq: state.seq.concat([key]),
        counter: state.counter + 1,
        groups: state.groups.concat([group]),
      });
      screen.showToast(
        translate("robot.workspace.messages.recoveredThe1042BackupPressSaveToStore"),
        null,
        "",
        null,
        "",
        4000,
      );
    },

    closeMerge: () => set({ mergeOpen: false }),
    pickMergeFolder: (folderIndex: number) => set({ fdFolder: folderIndex, mWork: null }),
    pickMergeFile: (projectId: string) => set({ mWork: projectId }),
    openMergeFile: (projectId: string) => {
      set({ mWork: projectId });
      screen.tm(() => screen.mergeAll(), 0);
    },
    confirmMerge: () => screen.mergeAll(),

    stayOnPage: () => set({ leaveAsk: null }),
    stopRobot: () => screen.stopAllHandler(),
    leaveWithoutSaving: () => screen.leaveNav(),
    saveAndLeave: () => screen.leaveSaveNav(),

    cancelClear: () => set({ clearAsk: false }),
    confirmClear: () => {
      const state = project();
      const previous: Partial<ProjectState> = {
        poses: state.poses,
        seq: state.seq,
        groups: state.groups,
        sel: state.sel,
        linked: state.linked,
        rangeOn: state.rangeOn,
      };
      const poseCount = state.seq.length;
      set({
        clearAsk: false,
        poses: {},
        seq: [],
        groups: [],
        sel: -1,
        linked: -1,
        rangeOn: false,
        mode: "all",
        synced: false,
        picks: [],
        selMode: false,
      });
      screen.addLog("pose", translate("robot.workspace.messages.startOverDeleteAllValue0Poses", { value0: poseCount }));
      screen.showToast(
        translate("robot.workspace.messages.allPosesDeletedReadyToStartOver"),
        () => set({ toast: null, ...previous }),
        undoLabel(),
        null,
        "",
        6000,
      );
    },
  };
}

export type DialogActions = ReturnType<typeof createDialogActions>;
