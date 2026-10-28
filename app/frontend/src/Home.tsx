import { HomeView } from "./home/HomeView";

import {
  type Work,
  workTitle,
  KIND_LABEL,
  workKinds,
  FOLDERS,
  OTHER_FOLDER,
  type HeroKind,
  HEROES,
  type ScenarioId,
  scenarioIds,
  heroOf,
} from "./home/homeData";

import { useEffect, useRef, useState } from "react";

import { client } from "./core/client";
import { t as translate } from "./i18n";
import { param } from "./core/debugParams";
import type { ModeID } from "./core/types";

import { SAMPLE_STARS, WEEK_STARS, useSampleData, useShell } from "./shell";

// No progress/profile service exists yet, so the student name / stars are fixed placeholders
// (see shell.tsx). Real works come from client.load; the sample works below are used only
// for the browser preview or an explicit ?scenario= deep link.

export function useHomeScreen() {
  const shell = useShell();
  const sample = useSampleData();
  const asked = param("scenario") as ScenarioId | undefined;
  const [scenario, setScenario] = useState<ScenarioId>(
    asked && scenarioIds.includes(asked) ? asked : "continue",
  );
  const [query, setQuery] = useState("");
  const [folder, setFolder] = useState(0);
  const [otherFolder, setOtherFolder] = useState(false);
  const [noWorks, setNoWorks] = useState(
    scenario === "first" || scenario === "empty",
  );
  const [folderOpen, setFolderOpen] = useState(scenario === "folders");
  const [kindOpen, setKindOpen] = useState(scenario === "newwork");
  const [toast, setToast] = useState("");
  const [real, setReal] = useState<Work[]>([]);
  const toastTimer = useRef<number | undefined>(undefined);

  useEffect(() => {
    let alive = true;
    Promise.all(
      workKinds.map((item) =>
        client.load(item.mode).then(
          (value): Work | undefined =>
            value
              ? {
                  ...item,
                  id: item.mode,
                  bg: item.kind === "arm" ? "var(--blue-tint)" : "var(--orange-tint)",
                  a: item.kind === "arm" ? [0, -40, 30, 0] : undefined,
                }
              : undefined,
          () => undefined,
        ),
      ),
    ).then((found) => {
      if (alive) setReal(found.filter((item): item is Work => !!item));
    });
    return () => {
      alive = false;
    };
  }, []);

  useEffect(() => {
    if (scenario === "sidebar") shell.openMenu();
    if (scenario === "profile") shell.openProfile();
  }, []);
  useEffect(() => () => window.clearTimeout(toastTimer.current), []);
  useEffect(() => {
    function onKey(event: KeyboardEvent) {
      if (event.key !== "Escape") return;
      if (kindOpen) setKindOpen(false);
      else if (folderOpen) setFolderOpen(false);
    }
    window.addEventListener("keydown", onKey);
    return () => window.removeEventListener("keydown", onKey);
  }, [kindOpen, folderOpen]);

  function say(text: string) {
    window.clearTimeout(toastTimer.current);
    setToast(text);
    toastTimer.current = window.setTimeout(() => setToast(""), 2500);
  }
  function pickScenario(id: ScenarioId) {
    setScenario(id);
    setQuery("");
    setFolder(0);
    setOtherFolder(false);
    setNoWorks(id === "first" || id === "empty");
    setFolderOpen(id === "folders");
    setKindOpen(id === "newwork");
    if (id === "sidebar") shell.openMenu();
    else if (id === "profile") shell.openProfile();
    else {
      shell.closeMenu();
      shell.closeProfile();
    }
  }
  function pickFolder(index: number) {
    setFolder(index);
    setOtherFolder(false);
    setNoWorks(false);
    setFolderOpen(false);
  }
  function browseFolder() {
    setFolderOpen(false);
    setOtherFolder(true);
    setNoWorks(false);
    say(translate("home.messages.sampleFolderPicker"));
  }

  const heroKind: HeroKind = sample ? heroOf(scenario) : "first";
  const hero = HEROES[heroKind];
  const stars = sample
    ? scenario === "first"
      ? 0
      : SAMPLE_STARS
    : WEEK_STARS;
  const folders = otherFolder ? [...FOLDERS, OTHER_FOLDER] : FOLDERS;
  const current = otherFolder ? OTHER_FOLDER : FOLDERS[folder];
  const all: Work[] = sample ? (noWorks ? [] : current.works) : real;
  const needle = query.trim().toLowerCase();
  const shown = needle
    ? all.filter((w) =>
        `${workTitle(w)} ${KIND_LABEL[w.kind]}`.toLowerCase().includes(needle),
      )
    : all;
  const startNew = (mode: ModeID) => {
    setKindOpen(false);
    shell.go(mode);
  };

  return {
    shell,
    sample,
    scenario,
    query,
    setQuery,
    folder,
    otherFolder,
    folderOpen,
    setFolderOpen,
    kindOpen,
    setKindOpen,
    toast,
    pickScenario,
    pickFolder,
    browseFolder,
    heroKind,
    hero,
    stars,
    folders,
    current,
    all,
    needle,
    shown,
    startNew,
  };
}

export default function Home() { return <HomeView v={useHomeScreen()} />; }
