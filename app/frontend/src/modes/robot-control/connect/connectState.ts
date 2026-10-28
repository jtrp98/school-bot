
import { param } from "../../../core/debugParams";
import { SAMPLE_ROBOT_NAME } from "../../../shell";
import { type RobotApi } from "../robotContext";
import { BT, DEFAULT_NAMES, UNNAMED, USB, isScenarioId, type Disc, type ErrKey, type IdOutcome, type IdStep, type OtherInfo, type Phase, type PortInfo, type ScenarioId } from "./connectData";
export type FlowState = {
  scenario: ScenarioId;
  id: IdStep | null;
  picked: string | null;
  phase: Phase;
  err: ErrKey | null;
  tries: number;
  showOthers: boolean;
  found: boolean;
  names: Record<string, string>;

  connKey: string | null;
  since: string;
  disc: Disc | null;
  fold: boolean;
  justDisc: string | null;
  discFolded: boolean;
  unsaved: boolean;
  moving: boolean;
  nameDraft: string;
  checks: Record<number, boolean>;
  switchTo: string | null;
  port8: boolean;
  idTips: boolean;
  outcome: IdOutcome;
};

type Family = "one" | "many" | "none" | "conn";

export const familyOf = (id: ScenarioId): Family => {
  if (id === "many" || id === "id1" || id === "id3" || id === "idWait") return "many";
  if (id === "none") return "none";
  if (id === "connected" || id === "askUpload" || id === "folding" || id === "disconnected" || id === "switch")
    return "conn";
  return "one";
};

export function portsOf(s: FlowState): { robots: PortInfo[]; others: OtherInfo[] } {
  const fam = familyOf(s.scenario);
  const r7: PortInfo = { port: s.port8 ? "COM8" : "COM7", key: "COM7" };
  const r9: PortInfo = { port: "COM9", key: "COM9" };
  if (fam === "many") {
    if (s.id === "wait") return { robots: [r9], others: [BT, USB] };
    if (s.id === "unplug2") return { robots: s.outcome === "two" ? [] : [r9], others: [BT, USB] };
    return { robots: [r7, r9], others: [BT, USB] };
  }
  if (fam === "conn") return { robots: [r7, r9], others: [BT] };
  if (fam === "none" && !s.found) return { robots: [], others: [BT] };
  return { robots: [r7], others: [BT] };
}

export const nameOf = (s: FlowState, key: string) => s.names[key] || UNNAMED;

export const portOfKey = (s: FlowState, key: string) =>
  portsOf(s).robots.find((r) => r.key === key)?.port ?? key;

export const clock = () => {
  const d = new Date();
  return `${String(d.getHours()).padStart(2, "0")}:${String(d.getMinutes()).padStart(2, "0")}`;
};

const FORGET: ScenarioId[] = ["one", "scanning", "connecting", "naming", "many", "id1", "id3", "idWait"];

export function initialState(
  id: ScenarioId,
  rb: RobotApi,
  explicit: boolean,
  carry?: Pick<FlowState, "unsaved" | "moving" | "outcome">,
): FlowState {
  const fam = familyOf(id);

  const names =
    explicit && FORGET.includes(id) ? {} : { ...DEFAULT_NAMES, ...rb.names };
  const s: FlowState = {
    scenario: id,
    id: null,
    picked: fam === "many" || fam === "none" ? null : "COM7",
    phase: "idle",
    err: null,
    tries: 0,
    showOthers: fam === "many",
    found: false,
    names,
    connKey: null,
    since: "10:02",
    disc: null,
    fold: false,
    justDisc: null,
    discFolded: false,
    unsaved: carry?.unsaved ?? true,
    moving: carry?.moving ?? false,
    nameDraft: "",
    checks: {},
    switchTo: null,
    port8: false,
    idTips: false,
    outcome: carry?.outcome ?? "ok",
  };
  switch (id) {
    case "scanning":
      s.phase = "scanning";
      break;
    case "connecting":
      s.phase = "connecting";
      s.tries = 1;
      break;
    case "naming":
      s.connKey = "COM7";
      s.phase = "naming";
      s.nameDraft = SAMPLE_ROBOT_NAME;
      break;
    case "ok":
      s.connKey = "COM7";
      s.phase = "ok";
      break;
    case "id1":
      s.id = "unplug";
      break;
    case "id3":
      s.id = "found";
      s.port8 = true;
      s.picked = "COM8";
      break;
    case "idWait":
      s.id = "wait";
      s.idTips = true;
      break;
    case "busy":
      s.err = "busy";
      s.tries = 1;
      break;
    case "noReply":
      s.err = "noReply";
      s.tries = 1;
      break;
    case "fail2":
      s.err = "noReply";
      s.tries = 2;
      break;
    case "connected":
      s.connKey = "COM7";
      break;
    case "askUpload":
      s.connKey = "COM7";
      s.disc = "ask";
      break;
    case "folding":
      s.connKey = "COM7";
      s.disc = "folding";
      s.fold = true;
      break;
    case "disconnected":
      s.justDisc = "COM7";
      s.discFolded = true;
      break;
    case "switch":
      s.connKey = "COM7";
      s.picked = "COM9";
      s.switchTo = "COM9";
      break;
    default:
      break;
  }
  return s;
}

export function startState(rb: RobotApi): { state: FlowState; explicit: boolean } {
  const asked = param("scenario");
  if (isScenarioId(asked)) return { state: initialState(asked, rb, true), explicit: true };
  if (rb.conn === "connected" && rb.robot) {
    const s = initialState("connected", rb, false);
    s.connKey = rb.robot.port === "COM9" ? "COM9" : "COM7";
    return { state: s, explicit: false };
  }
  return { state: initialState("one", rb, false), explicit: false };
}
