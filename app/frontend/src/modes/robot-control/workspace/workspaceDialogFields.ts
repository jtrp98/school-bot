import type { IconName } from "../../../components/shared/Icons";
import { t as translate } from "../../../i18n";
import type { WorkspaceFieldsContext } from "./workspaceFields";
import type {
  CopyPopoverState,
  KeyPopoverState,
  MessagePopoverState,
} from "./workspacePopovers";

export type ToastView = {
  text: string;
  icon: IconName | undefined;
  primaryLabel: string | null;
  secondaryLabel: string | null;
};

export type MessagePopoverView = Omit<MessagePopoverState, "title" | "body"> & {
  title: string;
  body: string;
};

export type DialogsView = {
  movedText: string | null;
  messagePopover: MessagePopoverView | null;
  keyPopover: KeyPopoverState | null;
  copyPopover: CopyPopoverState | null;
  toast: ToastView | null;
  releaseConfirmOpen: boolean;
  releaseCountdown: number | null;
  releaseCountdownSteps: boolean[];
  loopFromBoardConfirmOpen: boolean;
};

export type MergeFolderView = { icon: string; name: string; selected: boolean };

export type MergeFileView = { id: string; name: string; date: string; size: string; selected: boolean };

export type MergeDialogView = {
  breadcrumb: string;
  folderName: string;
  folders: MergeFolderView[];
  files: MergeFileView[];
  selectedFileName: string;
};

export type LeaveDialogView = {
  mode: "busy" | "unsaved" | "go";
  title: string;
  body: string;
  goLabel: string;
};

export type ProjectDialogsView = {
  recoveryOpen: boolean;
  merge: MergeDialogView | null;
  leave: LeaveDialogView | null;
  clearConfirm: { poseCount: number } | null;
};

type MergeSourceProject = { id: string; name: string; poses: unknown[] };

type StoredToast = {
  text: string;
  act?: () => void;
  actLabel?: string;
  act2?: () => void;
  act2Label?: string;
};

type MovedPose = { name: string; from: number; to: number };
type LeaveRequest = { kind: string; to: string };

// File names, dates and folders of the simulated project picker; they are sample data, not UI text.
const MERGE_FOLDERS: { icon: string; name?: string; nameKey?: string; projectIds: string[] }[] = [
  { icon: "⬇️", name: "Download", projectIds: ["w2", "w3", "w4"] },
  { icon: "📄", nameKey: "robot.workspace.messages.documents", projectIds: ["w4"] },
  { icon: "🖥️", name: "Desktop", projectIds: [] },
  { icon: "🔌", name: "USB (E:)", projectIds: ["w3"] },
];
const MERGE_DATES: Record<string, string> = {
  w2: "14/10/2569 14:05",
  w3: "28/9/2569 09:40",
  w4: "25/9/2569 13:15",
};

function buildMergeDialog(
  state: { fdFolder?: number; mWork: string | null },
  projects: MergeSourceProject[],
): MergeDialogView {
  const folderIndex = state.fdFolder ?? 0;
  const folders = MERGE_FOLDERS.map((folder, index) => ({
    icon: folder.icon,
    name: folder.nameKey ? translate(folder.nameKey) : (folder.name ?? ""),
    selected: index === folderIndex,
  }));
  const current = folders[folderIndex];
  const visible = projects.filter((project) => MERGE_FOLDERS[folderIndex].projectIds.includes(project.id));
  const files = visible.map((project) => ({
    id: project.id,
    name: project.name + ".ini",
    date: MERGE_DATES[project.id] ?? "",
    size: project.poses.length + " KB",
    selected: state.mWork === project.id,
  }));
  return {
    breadcrumb: translate("robot.workspace.messages.thisPcValue0Schoolbot", { value0: current.name }),
    folderName: current.name,
    folders,
    files,
    selectedFileName: files.find((file) => file.selected)?.name ?? "",
  };
}

function buildLeaveDialog(
  request: LeaveRequest,
  projectName: string,
  busy: boolean,
  dirty: boolean,
): LeaveDialogView {
  const goLabel = translate("robot.workspace.messages.goToValue0", { value0: request.to });
  if (busy) {
    return {
      mode: "busy",
      title: translate("robot.workspace.messages.robotIsStillMoving"),
      body: translate("robot.workspace.messages.stopTheRobotBeforeOpeningValue0PlaybackMustStop", {
        value0: request.to,
      }),
      goLabel,
    };
  }
  if (dirty) {
    return {
      mode: "unsaved",
      title: translate("robot.workspace.save.unsavedProject"),
      body: translate("robot.workspace.messages.projectValue0HasUnsavedChangesSaveBeforeOpeningValue1", {
        value0: projectName || translate("robot.workspace.save.untitled"),
        value1: request.to,
      }),
      goLabel,
    };
  }
  return {
    mode: "go",
    title: translate("robot.workspace.position.robotStopped"),
    body: translate("robot.workspace.messages.goToValue0Now", { value0: request.to }),
    goLabel,
  };
}

export function buildDialogFields(context: WorkspaceFieldsContext) {
  const { screen, S, IC, n, leaveBusyNow, leaveDirty } = context;
  const toast: StoredToast | null = S.toast;
  const moved: MovedPose | null = S.moved;
  const messagePopover: MessagePopoverState | null = S.msgPop;
  const leaveRequest: LeaveRequest | null = S.leaveAsk;
  const copyPopover: CopyPopoverState | null = S.copyPop;
  const keyPopover: KeyPopoverState | null = S.keyPop;
  const countdown: number = S.relCount;
  const toastText = toast ? IC(translate(toast.text)) : null;

  const dialogs: DialogsView = {
    movedText: moved
      ? translate("robot.workspace.messages.moveValue0FromValue1Value2", {
        value0: moved.name,
        value1: moved.from + 1,
        value2: moved.to + 1,
      })
      : null,
    messagePopover: messagePopover
      ? { ...messagePopover, title: translate(messagePopover.title), body: translate(messagePopover.body) }
      : null,
    keyPopover,
    copyPopover,
    toast: toast && toastText
      ? {
        text: toastText.t,
        icon: toastText.c,
        primaryLabel: toast.act ? translate(toast.actLabel ?? "") : null,
        secondaryLabel: toast.act2 ? translate(toast.act2Label ?? "") : null,
      }
      : null,
    releaseConfirmOpen: !!S.relAsk,
    releaseCountdown: countdown > 0 ? countdown : null,
    releaseCountdownSteps: [1, 2, 3].map((step) => step <= 4 - countdown),
    loopFromBoardConfirmOpen: !!S.robInfAsk,
  };

  const projectDialogs: ProjectDialogsView = {
    recoveryOpen: !!S.recoverAsk,
    merge: S.mergeOpen ? buildMergeDialog(S, screen.MW) : null,
    leave: leaveRequest ? buildLeaveDialog(leaveRequest, S.projName, leaveBusyNow, leaveDirty) : null,
    clearConfirm: S.clearAsk ? { poseCount: n } : null,
  };

  return { dialogs, projectDialogs, dialogActions: screen.dialogActions };
}
