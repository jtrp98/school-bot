import { CONNECT_SCENARIOS as SCENARIOS, type ConnectScenarioId as ScenarioId } from "../../../data/sampleScenarios";
export { SCENARIOS };
export type { ScenarioId };
import { SAMPLE_ROBOT_NAME, SAMPLE_ROBOT_NAME_SECOND } from "../../../shell";

export const isScenarioId = (v: string | undefined): v is ScenarioId =>
  !!v && SCENARIOS.some((s) => s.id === v);

export type IdOutcome = "ok" | "two" | "timeout" | "portChanged";
export const ID_OUTCOMES: { value: IdOutcome; label: string }[] = [
  { value: "ok", label: "common.messages.found" },
  { value: "two", label: "common.messages.tooManyCablesUnplugged" },
  { value: "timeout", label: "timeout" },
  { value: "portChanged", label: "common.messages.changePort" },
];

export type IdStep = "unplug" | "unplug2" | "wait" | "found";
export type Phase = "idle" | "scanning" | "connecting" | "naming" | "ok";
export type Disc = "stopping" | "ask" | "uploading" | "folding";
export type ErrKey = "busy" | "noReply";

export type PortInfo = { port: string; key: string };
export type OtherInfo = { port: string; name: string };

export const BT: OtherInfo = { port: "COM3", name: "Bluetooth" };
export const USB: OtherInfo = { port: "COM4", name: "อุปกรณ์ USB อื่น" };

export const DEFAULT_NAMES: Record<string, string> = {
  COM7: SAMPLE_ROBOT_NAME,
  COM9: SAMPLE_ROBOT_NAME_SECOND,
};
export const UNNAMED = "หุ่นแขนกล";

export const BAUD: Record<string, string> = {
  COM7: "115200",
  COM8: "115200",
  COM9: "9600",
};

export const ERR_TEXT: Record<ErrKey, { title: string; body: string; code: string }> = {
  busy: {
    title: "common.messages.anotherProgramIsUsingThisRobot",
    body: "common.messages.closeCodeLabOrOtherProgramsConnectedToThe",
    code: "E-BUSY",
  },
  noReply: {
    title: "common.messages.cableDetectedButRobotIsNotResponding",
    body: "common.messages.connectThePowerAdapterAndCheckTheBoardLight",
    code: "E-NOREPLY",
  },
};

export const DEEP_STEPS: Record<ErrKey, [string, string][]> = {
  busy: [
    ["common.messages.closeAllCodeLabWindows", "common.messages.checkTheTaskbarAndCloseCodeLab"],
    ["common.messages.closeOtherArmStudioWindows", "common.messages.youMayHaveOpenedTwoWindowsAccidentally"],
    ["common.messages.unplugAndReconnectUsb", "common.messages.wait3SecondsBeforeRetrying"],
    ["common.messages.switchTheRobotOffAndOn", "common.messages.theSwitchIsBesideTheBoard"],
  ],
  noReply: [
    ["common.messages.isTheBoardLightOn", "common.messages.ifNotFirmlyConnectThePowerAdapter"],
    ["common.messages.isTheRobotSwitchedOn", "common.messages.theSwitchBesideTheBoardMustBeOn"],
    ["common.messages.tryADifferentUsbPort", "common.messages.somePortsDoNotProvideEnoughPower"],
    ["common.messages.isTheCorrectPortSelected", "common.messages.chooseTheRobotArmCardNotAnotherDevice"],
  ],
};

export const HELP_TIPS = [
  "common.messages.connectUsbFirmlyAtBothTheComputerAndRobot",
  "common.messages.connectTheRobotPowerAdapterAndCheckTheBoard",
  "common.messages.closeCodeLabFirstOnlyOneProgramCanUse",
];

export const WAIT_TIPS = [
  "common.messages.fullyInsertBothCableEndsThenWait3Seconds",
  "common.messages.tryAnotherUsbPortThenPressSearchAgain",
  "common.messages.theBoardLightMustBeOnIfNotCheck",
];

export const MS = {
  scan: 1100,
  connect: 1300,
  goBack: 2100,
  idStep: 2600,
  stopping: 900,
  uploading: 1500,
  folding: 2500,
};
