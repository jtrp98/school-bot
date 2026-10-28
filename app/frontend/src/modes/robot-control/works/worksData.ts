import { WORKS_SCENARIOS as scenarios, type WorksScenarioId as ScenarioId } from "../../../data/sampleScenarios";
export { scenarios };
export type { ScenarioId };

import { t } from "../../../i18n";
import { SAMPLE_ROBOT_NAME, SAMPLE_ROBOT_NAME_SECOND, SAMPLE_WORK_NAME } from "../../../shell";

import type { RobotWork as Work } from "../../../data/work";
export type { RobotWork as Work } from "../../../data/work";

export const SEED: Work[] = [
  {
    id: "w1",
    name: SAMPLE_WORK_NAME,
    robot: SAMPLE_ROBOT_NAME,
    poses: 4,
    groups: 1,
    saved: "วันนี้ 10:20",
    a: [0, -40, 30, 0],
    bg: "var(--blue-tint)",
  },
  {
    id: "w2",
    name: "โบกมือทักทาย",
    robot: SAMPLE_ROBOT_NAME,
    poses: 6,
    groups: 2,
    saved: "เมื่อวาน 14:05",
    a: [0, 10, -60, -30],
    bg: "var(--orange-tint)",
  },
  {
    id: "w3",
    name: "เรียงบล็อกสี",
    robot: SAMPLE_ROBOT_NAME_SECOND,
    poses: 9,
    groups: 0,
    saved: "จ. 28 ก.ย. 09:40",
    a: [0, -70, 60, 40],
    bg: "var(--ok-tint)",
  },
  {
    id: "w4",
    name: "ส่งลูกบอล",
    robot: SAMPLE_ROBOT_NAME,
    poses: 5,
    groups: 1,
    saved: "ศ. 25 ก.ย. 13:15",
    a: [0, 30, 30, 20],
    bg: "var(--purple-tint)",
  },
  {
    id: "w5",
    name: "เต้นตามเพลง",
    robot: "หุ่นกลุ่ม 1",
    poses: 12,
    groups: 3,
    saved: "พ. 23 ก.ย. 10:02",
    a: [0, -20, -40, 60],
    bg: "var(--err-tint)",
  },
];

export const DEFAULT_ROBOT = SAMPLE_ROBOT_NAME;

export const scenarioIds = scenarios.map((s) => s.id);

export type Toast = { text: string; undo?: () => void; sticky?: boolean };

const BAD_CHARS = /[\\/:*?"<>|]/;

export type NameCheck = "empty" | "bad" | "dup" | "ok";

export function checkName(raw: string, others: Work[]): NameCheck {
  const name = raw.trim();
  if (!name) return "empty";
  if (BAD_CHARS.test(name)) return "bad";
  if (others.some((w) => w.name === name)) return "dup";
  return "ok";
}

export function uniqueName(base: string, others: Work[]) {
  let name = base;
  for (let n = 2; others.some((w) => w.name === name); n++) name = `${base} ${n}`;
  return name;
}

export function metaOf(w: Work): string {
  const values = { poses: w.poses, groups: w.groups };
  if (w.poses) return t(w.groups ? "robot.works.messages.metaPosesGroups" : "robot.works.messages.metaPoses", values);
  return t(w.groups ? "robot.works.messages.metaNoPosesGroups" : "robot.works.messages.metaNoPoses", values);
}

export type Start = {
  works: Work[];
  naming: string | null;
  missing: string[];
  renameId: string | null;
  q: string;
  toast: Toast | null;

  trashed: { work: Work; at: number } | null;
  usb: boolean;
};

export function build(id: ScenarioId): Start {
  const base: Start = { works: SEED, naming: null, missing: [], renameId: null, q: "", toast: null, trashed: null, usb: false };
  switch (id) {
    case "naming":
      return { ...base, naming: "รถเก็บขยะ" };
    case "dupName":
      return { ...base, naming: "โบกมือทักทาย" };
    case "badName":
      return { ...base, naming: "รถ/เก็บขยะ" };
    case "missing":
      return { ...base, missing: ["w2"] };
    case "empty":
      return { ...base, works: [] };
    case "noResult":
      return { ...base, q: "รถไฟ" };
    case "rename":
      return { ...base, renameId: "w2" };
    case "deleted":
      return {
        ...base,
        works: SEED.filter((w) => w.id !== "w2"),
        toast: { text: t("robot.works.messages.toastTrashed", { name: SEED[1].name }), sticky: true },
        trashed: { work: SEED[1], at: 1 },
      };
    case "openingUsb":
      return { ...base, usb: true };
    default:
      return base;
  }
}

