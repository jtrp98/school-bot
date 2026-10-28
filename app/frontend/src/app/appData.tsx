import { t as translate } from "../i18n";

import { Component, lazy, type ReactNode } from "react";

import { param } from "../core/debugParams";

import { type IconName } from "../components/shared/Icons";

import { STUDENT_NAME, type View } from "../shell";

const noticeClass = [
  "notice whitespace-pre-wrap [overflow-wrap:anywhere] border-2 border-solid border-blue-line",
  "bg-blue-tint text-blue-press rounded-control py-[12px] px-[16px] mt-0 mx-0 mb-[20px] text-[13px]",
  "[&.error]:text-err-ink [&.error]:bg-err-tint [&.error]:border-err-line error",
].join(" ");

export const Robot = lazy(() => import("../modes/robot-control/RobotMode"));

export const Lessons = lazy(() => import("../modes/coding-adventure/LessonsMode"));

export const CodeLab = lazy(() => import("../modes/coding-adventure/CodeLabMode"));

export const Blocks = lazy(() => import("../modes/block-programming/BlockMode"));

type NavItem = { view: View; label: string; icon: IconName };

export const nav: NavItem[] = [
  { view: "home", label: "nav.home", icon: "home" },
  { view: "coding-adventure", label: "nav.lessons", icon: "book" },
  { view: "code-lab", label: "nav.codeLab", icon: "terminal" },
  { view: "block-programming", label: "Block Lab", icon: "block" },
  { view: "robot-control", label: "Arm Lab", icon: "arm" },
];

export const titles: Record<View, string> = {
  home: "nav.home",
  "coding-adventure": "nav.lessons",
  "code-lab": "nav.codeLab",
  "block-programming": "Block Lab",
  "robot-control": "Arm Lab",
};

export const titleIcons: Partial<Record<View, IconName>> = {
  "code-lab": "terminal",
  "block-programming": "block",
  "robot-control": "arm",
};

const views: View[] = [
  "home",
  "coding-adventure",
  "code-lab",
  "block-programming",
  "robot-control",
];

export const avatarLetter = Array.from(STUDENT_NAME)[0];

export function initialView(): View {
  const asked = param("view") as View | undefined;
  return asked && views.includes(asked) ? asked : "home";
}

export function syncUrl(screen?: string) {
  try {
    const url = new URL(location.href);
    url.searchParams.delete("scenario");
    if (screen) url.searchParams.set("screen", screen);
    else url.searchParams.delete("screen");
    history.replaceState(null, "", url);
  } catch {
    // Navigation state still works when the WebView refuses history updates.
  }
}

export class ModeBoundary extends Component<
  { children: ReactNode },
  { error: string }
> {
  state = { error: "" };
  static getDerivedStateFromError(error: Error) {
    return { error: error.message };
  }
  render() {
    return this.state.error ? (
      <div role="alert" className={noticeClass}>
        {translate("shell.messages.modeLoadFailed", { error: this.state.error })}
      </div>
    ) : (
      this.props.children
    );
  }
}
