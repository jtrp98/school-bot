import { createContext, useContext } from "react";
import type { ModeID } from "./core/types";
import { preview } from "./core/client";
import { param } from "./core/debugParams";

export type View = "home" | "code-lab" | ModeID;
export type { Language as Lang } from "./i18n";
import type { Language as Lang } from "./i18n";

// No profile / progress service exists yet, so these are fixed placeholders.
export const STUDENT_NAME = "น้องมายด์";
export const STUDENT_CLASS = "ป.4/2";
export const WEEK_STARS = 0;

// Names shown only while sample data is on; real robots and projects use the user's own names.
export const SAMPLE_ROBOT_NAME = "หุ่นกลุ่ม 3";
export const SAMPLE_ROBOT_NAME_SECOND = "หุ่นกลุ่ม 5";
export const SAMPLE_WORK_NAME = "แขนหยิบกล่อง";
export const SAMPLE_WORK_NAME_ROOM2 = "แขนหยิบกล่อง ห้อง 2";

export const SAMPLE_STARS = 9;

/** sample data is allowed only for the browser preview or an explicit ?scenario= deep link */
export const useSampleData = () => preview || !!param("scenario");

export type ShellApi = {

  go: (view: View, screen?: string, confirmed?: boolean) => void;
  menuOpen: boolean;
  openMenu: () => void;
  closeMenu: () => void;
  profileOpen: boolean;
  openProfile: () => void;
  closeProfile: () => void;
  openSettings: () => void;
  lang: Lang;
  setLang: (lang: Lang) => void;

  setArmScreen: (screen: string) => void;
};

const noop = () => undefined;
const defaults: ShellApi = {
  go: noop,
  menuOpen: false,
  openMenu: noop,
  closeMenu: noop,
  profileOpen: false,
  openProfile: noop,
  closeProfile: noop,
  openSettings: noop,
  lang: "th",
  setLang: noop,
  setArmScreen: noop,
};

export const ShellContext = createContext<ShellApi>(defaults);
export const useShell = () => useContext(ShellContext);
