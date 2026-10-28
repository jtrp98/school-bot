import { t as translate } from "../../../i18n";

export type LogType = "conn" | "pose" | "move" | "order" | "play" | "stop" | "err" | "send" | "time";
export type LogFilter = "all" | "pose" | "play" | "stop" | "err";

type LogEntry = { time: string; type: string; text: string };
type LogState = { log: LogEntry[]; logFilter: string };

const FILTER_LABEL_KEYS: Array<[LogFilter, string]> = [
  ["all", "robot.workspace.messages.all"],
  ["pose", "common.messages.poses"],
  ["play", "common.play"],
  ["stop", "common.stop"],
  ["err", "robot.workspace.messages.error"],
];
const TAG_LABEL_KEYS: Record<LogType, string> = {
  conn: "robot.connect.action",
  pose: "common.messages.poses",
  move: "robot.workspace.messages.move",
  order: "robot.workspace.messages.order",
  play: "common.play",
  stop: "common.stop",
  err: "robot.workspace.messages.error",
  send: "robot.workspace.messages.upload",
  time: "robot.workspace.messages.duration",
};
const POSE_FILTER_TYPES = ["order", "move", "time"];

export type LogItemView = { time: string; text: string; tag: string; tone: LogType };

const isLogType = (type: string): type is LogType => type in TAG_LABEL_KEYS;

export function buildLogView(state: LogState) {
  const logFilters = FILTER_LABEL_KEYS.map(([value, labelKey]) => ({
    value,
    label: translate(labelKey),
    on: state.logFilter === value,
  }));
  const shown = state.log.slice().reverse().filter((entry) => state.logFilter === "all"
    || entry.type === state.logFilter
    || (state.logFilter === "pose" && POSE_FILTER_TYPES.includes(entry.type)));
  const logItems = shown.map((entry): LogItemView => {
    const tone: LogType = isLogType(entry.type) ? entry.type : "conn";
    return { time: entry.time, text: entry.text, tag: translate(TAG_LABEL_KEYS[tone]), tone };
  });
  return { logFilters, logItems };
}
