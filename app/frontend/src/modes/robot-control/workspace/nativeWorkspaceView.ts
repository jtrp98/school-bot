import { t as translate } from "../../../i18n";
import type { RobotApi } from "../robotContext";
import type { WorkspaceView } from "./viewTypes";

export type NativeViewActions = {
  unsupportedPreset(): void;
  send(): void;
  readAll(): void;
  readNew(): void;
  pauseUnsupported(): void;
  ignorePress(): void;
  netTest(): void;
};

type NativeViewData = {
  robot: RobotApi["robot"];
  connection: RobotApi["conn"];
  connected: boolean;
  status: string;
  linked: boolean;
  savedAt: string;
  dirty: boolean;
};

function saveStateText(data: NativeViewData): string {
  if (!data.savedAt) return translate("robot.workspace.messages.projectNotYetSavedLocally");
  return translate(data.dirty ? "robot.workspace.save.nativeDirty" : "robot.workspace.save.nativeClean", {
    savedAt: data.savedAt,
  });
}

export function buildNativeWorkspaceView(view: WorkspaceView, data: NativeViewData, actions: NativeViewActions) {
  const playing = data.status === "playing" || data.status === "paused";
  return {
    ...view,
    nativeHardware: true,
    robotName: data.robot?.name || translate("robot.workspace.messages.noRobotSelected"),
    robotPort: data.robot?.port || translate("robot.workspace.messages.notConnected"),
    nativeConnected: data.connected,
    nativeProfile: data.robot?.profile?.name || translate("robot.workspace.messages.profileNotConfirmed"),
    nativeLost: data.connection === "lost",
    xyzNote: translate("robot.workspace.messages.commandBasedPreviewNotMeasuredPositions"),
    goHome: actions.unsupportedPreset,
    goBox: actions.unsupportedPreset,
    goPose0: actions.unsupportedPreset,
    send: actions.send,
    readBack: actions.readAll,
    relAll: actions.readAll,
    relNew: actions.readNew,
    joints: view.joints.map((joint, index) => ({
      ...joint,
      releaseLiveText: translate("robot.workspace.messages.commandedLastReadValues"),
      servoId: data.robot?.profile?.servos[index]?.id ?? "—",
      servoIdTip: translate("robot.workspace.messages.idsFromTheConfirmedConnectionProfile"),
    })),
    ...(playing ? {
      playSlot: actions.pauseUnsupported,
      playDown: actions.ignorePress,
      playUp: actions.ignorePress,
      playSlotLabel: translate("robot.workspace.messages.pauseUnsupported"),
      playSlotOff: true,
    } : {}),
    ...(data.status === "stopped" ? {
      stoppedTitle: translate("robot.workspace.messages.stopCommandSentVerifyTheRobotHasPhysicallyStopped"),
    } : {}),
    f6Sync: translate(data.linked
      ? "robot.workspace.messages.poseCommandsSentPositionsHaveNotBeenReadBack"
      : "robot.workspace.messages.actualPositionsUnknownSendAPoseBeforeAdjusting"),
    saveStateText: saveStateText(data),
    netTest: actions.netTest,
  };
}
