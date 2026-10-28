export { TEACHER_SCENARIOS as SCENARIOS } from "../../../data/sampleScenarios";

import { t } from "../../../i18n";

export const PIN = "1234";

export const RESTART_MS = 2500;

export const LOG_SHOW = 12;

type LogKind = "i" | "o" | "s";

export type LogLine = [LogKind, string];

export type Restart = "idle" | "busy" | "done";

const LOG0: LogLine[] = [
  ["i", "> #1PRAD"],
  ["o", "< #001P1500"],
  ["i", "> #255PVER"],
  ["o", "< SB-ARM v2.3.1 · 6 servo"],
];

export const KEYS = ["1", "2", "3", "4", "5", "6", "7", "8", "9", "", "0", "⌫"];

export type Init = {
  locked: boolean;
  pin: string;
  bad: boolean;
  restart: Restart;
  ask: boolean;
  log: LogLine[];
  typed: string;
  off: boolean;
};

const base: Init = { locked: false, pin: "", bad: false, restart: "idle", ask: false, log: LOG0, typed: "", off: false };

export function presetOf(id: string): Init {
  switch (id) {
    case "pin": return { ...base, locked: true, pin: "12" };
    case "pinErr": return { ...base, locked: true, bad: true };
    case "fix": return base;
    case "restarting": return { ...base, restart: "busy" };
    case "restarted": {
      const restarted = t("robot.teacher.messages.restartLog", { port: "COM7" });
      return { ...base, restart: "done", log: [...LOG0, ["s", restarted]] };
    }
    case "resetAsk": return { ...base, ask: true };
    case "console": return { ...base, log: [...LOG0, ["i", "> #1P1500T1000"], ["o", "< OK"]], typed: "#2PRAD" };
    case "robotOff": return { ...base, off: true };
    default: return { ...base, locked: true };
  }
}
