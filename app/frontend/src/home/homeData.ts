import { HOME_SCENARIOS as scenarios, type HomeScenarioId as ScenarioId } from "../data/sampleScenarios";
export { scenarios };
export type { ScenarioId };
import { t as translate } from "../i18n";

import type { ModeID } from "../core/types";
import { type IconName } from "../components/shared/Icons";

import type { HomeWork as Work, WorkKind as Kind } from "../data/work";
export type { HomeWork as Work } from "../data/work";

export const workTitle = (work: Work): string => work.id === work.mode ? translate(work.kind === "arm" ? "common.messages.savedArmPoses" : "common.messages.savedBlockProject") : work.name;

const MODE_OF: Record<Kind, ModeID> = {
  arm: "robot-control",
  block: "block-programming",
};

export const KIND_LABEL: Record<Kind, string> = { arm: "Arm Lab", block: "Block Lab" };

export const workKinds: { mode: ModeID; kind: Kind; name: string; meta: string }[] = [
  {
    mode: "robot-control",
    kind: "arm",
    name: "ท่าแขนกลที่บันทึกไว้",
    meta: "common.messages.savedOnThisDevice",
  },
  {
    mode: "block-programming",
    kind: "block",
    name: "งานบล็อกที่บันทึกไว้",
    meta: "common.messages.savedOnThisDevice",
  },
];

const sampleWork = (
  id: string,
  kind: Kind,
  name: string,
  meta: string,
  saved: string,
  a?: number[],
  bg?: string,
): Work => ({
  id,
  kind,
  mode: MODE_OF[kind],
  name,
  meta,
  saved,
  a,
  bg: bg ?? "var(--orange-tint)",
});

const S_PICK = sampleWork("s1", "arm", "แขนหยิบกล่อง", "robot.workspace.messages.4Poses", "home.saved.today", [0, -40, 30, 0], "var(--blue-tint)");

const S_CAR = sampleWork("s2", "block", "รถเลี้ยวหลบกล่อง", "common.messages.12Blocks", "home.saved.yesterday");

const S_WAVE = sampleWork("s3", "arm", "โบกมือทักทาย", "common.messages.6Poses", "home.saved.friday", [0, 10, -60, -30], "var(--purple-tint)");

const S_BALL = sampleWork("s4", "arm", "ส่งลูกบอล", "common.messages.5Poses", "home.saved.friday", [0, 30, 30, 20], "var(--purple-tint)");

const S_SORT = sampleWork("s5", "arm", "เรียงบล็อกสี", "common.messages.9Poses", "home.saved.monday", [0, -70, 60, 40], "var(--ok-tint)");

const S_DANCE = sampleWork("s6", "arm", "เต้นตามเพลง", "common.messages.12Poses", "home.saved.wednesday", [0, -20, -40, 60], "var(--err-tint)");

type Folder = { icon: IconName; path: string; count: string; works: Work[] };

export const FOLDERS: Folder[] = [
  { icon: "folder", path: "เอกสาร › SchoolBot", count: "common.messages.5Projects", works: [S_PICK, S_CAR, S_WAVE] },
  { icon: "folder", path: "Download › SchoolBot", count: "common.messages.3Projects", works: [S_BALL, S_SORT, S_DANCE] },
  { icon: "usb", path: "USB (E:) › งานห้อง 4-2", count: "common.messages.2Projects", works: [S_WAVE, S_CAR] },
];

export const OTHER_FOLDER: Folder = {
  icon: "folder",
  path: "Desktop › หุ่นยนต์",
  count: "common.messages.noProjectsYet",
  works: [],
};

export type HeroKind = "first" | "continue" | "done";

export const HEROES: Record<HeroKind, { title: string; sub: string; btn: string; pct: number | null }> = {
  first: {
    title: "home.greeting",
    sub: "home.hero.first.description",
    btn: "home.hero.first.action",
    pct: null,
  },
  continue: {
    title: "home.greeting",
    sub: "home.hero.continue.description",
    btn: "home.hero.continue.action",
    pct: 60,
  },
  done: {
    title: "home.hero.complete.title",
    sub: "common.messages.tryTheNextUnitOrCreateYourOwnProject",
    btn: "home.hero.complete.action",
    pct: 100,
  },
};

export const scenarioIds = scenarios.map((s) => s.id);

export const heroOf = (s: ScenarioId): HeroKind =>
  s === "first" ? "first" : s === "done" ? "done" : "continue";
