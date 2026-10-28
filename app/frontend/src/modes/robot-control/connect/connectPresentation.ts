import { t as translate } from "../../../i18n";

import { type Disc } from "./connectData";

import { type FlowState } from "./connectState";

export function sayText(s: FlowState, n: number): [string, string] {
  if (s.disc === "stopping") return [translate("robot.connect.messages.stoppingRobotFirst"), translate("robot.connect.messages.theRobotIsMovingStopItBeforeDisconnecting")];
  if (s.disc === "folding") return [translate("robot.connect.messages.foldingArmIntoBox"), translate("robot.connect.messages.keepYourHandsAwayFromTheArm")];
  if (s.connKey && s.phase === "idle")
    return [
      translate("robot.connect.messages.robotAlreadyConnected"),
      s.switchTo
        ? translate("robot.connect.messages.anotherRobotIsSelectedPressConnectBelowToSwitch")
        : translate("robot.connect.messages.continueYourProjectOrDisconnectBeforeSwitchingToCode"),
    ];
  if (s.phase === "scanning") return [translate("robot.connect.messages.searchingForRobots"), translate("robot.connect.messages.pleaseWaitAMoment")];
  if (s.phase === "connecting") return [translate("robot.connect.messages.communicatingWithRobot"), translate("robot.connect.messages.checkingForAnActualRobotResponse")];
  if (s.phase === "naming")
    return [translate("robot.connect.messages.foundIt"), translate("robot.connect.messages.nameTheRobotBelowForExampleGroup3Robot")];
  if (s.phase === "ok") return [translate("robot.connect.messages.foundIt"), translate("robot.connect.messages.openingArmControls")];
  if (s.err && s.tries >= 2)
    return [translate("robot.connect.messages.notConnectedYetLetSCheckEachStep"), translate("robot.connect.messages.followTheRedChecklistTickEachStepThenRetry")];
  if (s.err) return [translate("robot.connect.messages.notConnectedYetLetSTryAgain"), translate("robot.connect.messages.readTheRedTroubleshootingPanelThenRetry")];
  if (s.justDisc) return [translate("robot.connect.messages.robotDisconnected"), translate("robot.connect.messages.useCodeLabOrReconnectHere")];
  if (n === 0) return [translate("robot.connect.messages.noRobotFoundYet"), translate("robot.connect.messages.connectTheCableThenSearchAgain")];
  if (s.id) return [translate("robot.connect.messages.letSFindYourRobot"), translate("robot.connect.messages.followTheBluePanelStepByStep")];
  if (n === 1) return [translate("robot.connect.messages.1RobotFound"), translate("robot.connect.messages.oneRobotIsAttachedToThisComputerPressConnect")];
  return [
    translate("robot.connect.messages.value0RobotsFoundWhichOneIsYours", {value0: n}),
    translate("robot.connect.messages.checkTheCardSRobotNameOrIdentifyIt"),
  ];
}

type IdInfo = { no: string | number; title: string; body: string; warn: boolean };

export function idInfo(s: FlowState): IdInfo | null {
  switch (s.id) {
    case "unplug":
      return { no: 1, warn: false, title: translate("robot.connect.messages.unplugYourGroupSRobotUsbCableFromThe"), body: translate("robot.connect.messages.unplugOnlyOneCableWatchWhichCardDisappears") };
    case "unplug2":
      return s.outcome === "two"
        ? {
            no: "!",
            warn: true,
            title: translate("robot.connect.messages.2CardsDisappearedTooManyCablesUnplugged"),
            body: translate("robot.connect.messages.reconnectAllCablesPressStartOverThenUnplugOnly"),
          }
        : {
            no: 2,
            warn: false,
            title: translate("robot.connect.messages.theMissingCardIsYourRobotReconnectItsCable"),
            body: translate("robot.connect.messages.useAnyUsbPortTheSystemWillDetectThe"),
          };
    case "wait":
      return {
        no: "?",
        warn: true,
        title: translate("robot.connect.messages.robotHasNotReturnedYet"),
        body: translate("robot.connect.messages.isTheCableConnectedIfTheRobotIsStill"),
      };
    case "found":
      return {
        no: 3,
        warn: false,
        title: s.port8 ? translate("robot.connect.messages.foundItOnCom8ChangingUsbPortsIsFine") : translate("robot.connect.messages.foundItYourGroupSRobotIsSelected"),
        body: translate("robot.connect.messages.pressConnectBelow"),
      };
    default:
      return null;
  }
}

export const DISC_BUSY_KEYS: Record<Exclude<Disc, "ask">, string> = {
  stopping: "robot.connect.messages.busyStopping",
  folding: "robot.connect.messages.busyFolding",
  uploading: "robot.connect.messages.busyUploading",
};
