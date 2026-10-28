import { t as translate } from "../i18n";
import type { LogicProps } from "../modes/robot-control/workspace/WorkspaceLogic";

export const HOME_SCENARIOS = [
  { id: "continue", label: "common.messages.continueLearning" },
  { id: "first", label: "common.messages.newLearner" },
  { id: "done", label: "common.messages.unitComplete" },
  { id: "empty", label: "common.messages.noProjectsYet" },
  { id: "sidebar", label: "common.messages.expandedMenu" },
  { id: "folders", label: "home.messages.chooseFolder" },
  { id: "profile", label: "profile.menu" },
  { id: "newwork", label: "common.messages.newProjectChooseType" },
] as const;

export type HomeScenarioId = (typeof HOME_SCENARIOS)[number]["id"];

export const WORKS_SCENARIOS = [
  { id: "list", label: "robot.works.scenario.list" },
  { id: "naming", label: "robot.works.messages.nameYourNewProject" },
  { id: "dupName", label: "robot.works.scenario.dupName" },
  { id: "badName", label: "robot.works.scenario.badName" },
  { id: "missing", label: "robot.works.messages.fileNotFound" },
  { id: "empty", label: "common.messages.noProjectsYet" },
  { id: "noResult", label: "robot.works.scenario.noResult" },
  { id: "rename", label: "robot.workspace.messages.rename" },
  { id: "deleted", label: "robot.works.scenario.deleted" },
  { id: "robotOff", label: "robot.works.scenario.robotOff" },
  { id: "openingUsb", label: "robot.works.messages.openingAUsbFile" },
] as const;

export type WorksScenarioId = (typeof WORKS_SCENARIOS)[number]["id"];

export const CONNECT_SCENARIOS = [
  { id: "one", label: "robot.connect.messages.1RobotFound" },
  { id: "scanning", label: "common.messages.searching" },
  { id: "connecting", label: "common.messages.connecting" },
  { id: "naming", label: "common.messages.firstTimeNaming" },
  { id: "ok", label: "common.messages.connectedSuccessfully" },
  { id: "many", label: "common.messages.multipleRobotsFound" },
  { id: "id1", label: "common.messages.unplugCableStep1" },
  { id: "id3", label: "common.messages.unplugCableFound" },
  { id: "idWait", label: "common.messages.unplugCableTimeout" },
  { id: "none", label: "common.messages.robotNotFound" },
  { id: "busy", label: "Error · busy" },
  { id: "noReply", label: "common.messages.errorNoRobotResponse" },
  { id: "fail2", label: "common.messages.secondFailedConnection" },
  { id: "connected", label: "common.messages.returnWhileConnected" },
  { id: "askUpload", label: "common.messages.askToUploadBeforeDisconnecting" },
  { id: "folding", label: "common.messages.foldingArm" },
  { id: "disconnected", label: "common.messages.disconnected" },
  { id: "switch", label: "common.messages.switchRobot" },
] as const;

export type ConnectScenarioId = (typeof CONNECT_SCENARIOS)[number]["id"];

export const TEACHER_SCENARIOS = [
  { id: "pin", label: "robot.teacher.messages.enterTeacherPin" },
  { id: "pinErr", label: "robot.teacher.scenario.pinErr" },
  { id: "fix", label: "robot.teacher.messages.troubleshooting" },
  { id: "restarting", label: "robot.teacher.scenario.restarting" },
  { id: "restarted", label: "robot.teacher.scenario.restarted" },
  { id: "resetAsk", label: "robot.teacher.scenario.resetAsk" },
  { id: "console", label: "robot.teacher.messages.commandConsole" },
  { id: "robotOff", label: "robot.teacher.scenario.robotOff" },
] as const;

export type TeacherScenarioId = (typeof TEACHER_SCENARIOS)[number]["id"];


export type ScenarioConn = "off" | "connected" | "lost";
export type Scenario = {
  id: string;
  label: string;
  props?: Partial<LogicProps>;

  init?: Record<string, unknown>;

  boot?: string;
  conn?: ScenarioConn;
};

const READY = { demo: "ready" };
const LINKED = { demo: "linked" };

export const WORKSPACE_SCENARIOS: Scenario[] = [
  { id: "notConnected", label: "robot.workspace.scenarios.notConnected", props: READY, conn: "off" },
  { id: "empty", label: "robot.workspace.scenarios.empty", props: { demo: "empty" } },
  { id: "ready", label: "robot.workspace.scenarios.ready", props: LINKED },
  { id: "viewing", label: "robot.workspace.scenarios.viewing", props: { demo: "viewing" } },
  { id: "far", label: "robot.workspace.scenarios.far", props: { demo: "farConfirm" } },
  { id: "table", label: "robot.workspace.scenarios.table", props: { demo: "table" } },
  { id: "split", label: "robot.workspace.scenarios.split", props: { demo: "split" } },
  {
    id: "group",
    label: "robot.workspace.scenarios.group",
    props: { ...LINKED, sample4c: "select" },
    init: { grpNaming: true, grpDraft: "หยิบของ" },
  },
  { id: "groupSaved", label: "robot.workspace.scenarios.groupSaved", props: { ...LINKED, sample4c: "group" } },
  { id: "range", label: "robot.workspace.scenarios.range", props: { demo: "range", showRange: true } },
  { id: "order", label: "robot.workspace.scenarios.order", props: { demo: "order" } },
  { id: "scope", label: "robot.workspace.scenarios.scope", props: LINKED, init: { playbackMenu: "scope" } },
  { id: "playMenu", label: "robot.workspace.scenarios.playMenu", props: LINKED, init: { playbackMenu: "all" } },
  {
    id: "playOptions",
    label: "robot.workspace.scenarios.playOptions",
    props: LINKED,
    init: { loopMenuOpen: true, loops: 3 },
  },
  { id: "safety", label: "robot.workspace.scenarios.safety", props: LINKED, init: { preflight: true, checked: false } },
  { id: "playing", label: "robot.workspace.scenarios.playing", props: { demo: "playing" }, boot: "play" },
  { id: "playingHold", label: "robot.workspace.scenarios.playingHold", props: { demo: "playing" } },
  {
    id: "stopped",
    label: "robot.workspace.scenarios.stopped",
    props: LINKED,
    init: { st: "stopped", linked: -1, snap: null },
  },
  { id: "done", label: "robot.workspace.scenarios.done", props: LINKED, init: { st: "done" } },
  { id: "lost", label: "robot.workspace.scenarios.lost", props: READY, conn: "lost" },
  {
    id: "uploading",
    label: "robot.workspace.scenarios.uploading",
    props: LINKED,
    init: { st: "sending" },
    boot: "upload",
  },
  {
    id: "uploadErr",
    label: "robot.workspace.scenarios.uploadErr",
    props: LINKED,
    init: { src: "robot", synced: false },
  },
  { id: "release", label: "robot.workspace.scenarios.release", props: { demo: "release" } },
  { id: "releaseIdle", label: "robot.workspace.scenarios.releaseIdle", props: { demo: "releaseIdle" } },
  { id: "relAsk", label: "robot.workspace.scenarios.relAsk", props: LINKED, init: { relAsk: true } },
  { id: "menu", label: "robot.workspace.scenarios.menu", props: LINKED, init: { menuOpen: true } },
  { id: "fold", label: "robot.workspace.scenarios.fold", props: LINKED, init: { boxMenu: true } },
  { id: "log", label: "robot.workspace.scenarios.log", props: { demo: "log" } },
  { id: "clearAsk", label: "robot.workspace.scenarios.clearAsk", props: LINKED, init: { clearAsk: true } },
  {
    id: "leave",
    label: "robot.workspace.scenarios.leave",
    props: LINKED,
    init: {
      leaveAsk: {
        kind: "work",
        get to() {
          return translate("robot.workspace.messages.myProjects");
        },
      },
    },
    boot: "dirty",
  },
  { id: "merge", label: "robot.workspace.scenarios.merge", props: { ...LINKED, sample4c: "merge" } },
  { id: "recover", label: "robot.workspace.scenarios.recover", props: { ...LINKED, sample4c: "recover" } },
  { id: "netSlow", label: "robot.workspace.scenarios.netSlow", props: { ...LINKED, net: "slow" }, boot: "netHdr" },
  { id: "netOff", label: "robot.workspace.scenarios.netOff", props: { ...LINKED, net: "off" }, boot: "netHdr" },
  { id: "barC", label: "robot.workspace.scenarios.barC", props: { ...LINKED, bar: "C" } },
  { id: "barNow", label: "robot.workspace.scenarios.barNow", props: { ...LINKED, bar: "now" } },
  {
    id: "extras",
    label: "robot.workspace.scenarios.extras",
    props: { ...LINKED, showLinkBox: true, showRevert: true, showArmTag: true, showSelPanel: true },
  },
];

export const WORKSPACE_SCENARIO_OPTIONS = WORKSPACE_SCENARIOS.map(({ id, label }) => ({ id, label }));

export const DEFAULT_SCENARIO: Scenario = { id: "", label: "", props: READY };

export const scenarioOf = (id: string | undefined): Scenario =>
  WORKSPACE_SCENARIOS.find((scenario) => scenario.id === id) ?? DEFAULT_SCENARIO;
