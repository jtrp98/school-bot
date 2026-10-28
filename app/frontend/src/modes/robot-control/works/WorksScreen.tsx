import { WorksView } from "./WorksView";
import {
  type Work,
  SEED,
  DEFAULT_ROBOT,
  type ScenarioId,
  scenarioIds,
  type Toast,
  type NameCheck,
  checkName,
  uniqueName,
  type Start,
  build,
} from "./worksData";

import { useEffect, useRef, useState, type KeyboardEvent } from "react";

import { param } from "../../../core/debugParams";
import { t as translate } from "../../../i18n";
import { SAMPLE_WORK_NAME_ROOM2, useSampleData } from "../../../shell";

import { useRobot } from "../robotContext";

// Keep sample works outside the component so navigation does not reset the list.
let kept: Work[] | null = null;

export function useWorksScreen() {
  const { go, conn, robot, connectTo, disconnect, workName, setWorkName } = useRobot();
  const sample = useSampleData();
  const asked = param("scenario") as ScenarioId | undefined;
  const [scenario, setScenario] = useState<ScenarioId>(
    asked && scenarioIds.includes(asked) ? asked : "list",
  );
  const [start] = useState(() => (kept ? build("list") : build(scenario)));
  const [works, setWorks] = useState<Work[]>(() => kept ?? start.works);
  const [naming, setNaming] = useState<string | null>(start.naming);
  const [missing, setMissing] = useState<string[]>(start.missing);
  const [renameId, setRenameId] = useState<string | null>(start.renameId);
  const [renameVal, setRenameVal] = useState(
    start.renameId ? (SEED.find((w) => w.id === start.renameId)?.name ?? "") : "",
  );
  const [q, setQ] = useState(start.q);
  const restoreRef = useRef<(w: Work, at: number) => void>(() => {});
  const withUndo = (s: Start): Toast | null => {
    const t = s.trashed;
    return s.toast && t ? { ...s.toast, undo: () => restoreRef.current(t.work, t.at) } : s.toast;
  };
  const [toast, setToast] = useState<Toast | null>(() => withUndo(start));
  const [usb, setUsb] = useState(start.usb);
  const worksRef = useRef<Work[]>([]);
  const timers = useRef<number[]>([]);
  const toastTimer = useRef<number | undefined>(undefined);

  worksRef.current = works;
  restoreRef.current = (w, at) => {
    const cur = worksRef.current;
    const i = Math.min(at, cur.length);
    commit([...cur.slice(0, i), w, ...cur.slice(i)]);
  };
  const live = conn === "connected";
  const robotName = robot?.name ?? DEFAULT_ROBOT;

  useEffect(() => {
    if (asked === "robotOff") disconnect();
  }, []);
  useEffect(
    () => () => {
      timers.current.forEach((t) => window.clearTimeout(t));
      window.clearTimeout(toastTimer.current);
    },
    [],
  );
  useEffect(() => {
    if (toast && !toast.sticky) {
      window.clearTimeout(toastTimer.current);
      toastTimer.current = window.setTimeout(() => setToast(null), 5000);
    }
  }, [toast]);

  const later = (fn: () => void, ms: number) => {
    timers.current.push(window.setTimeout(fn, ms));
  };
  const commit = (next: Work[]) => {
    kept = next;
    setWorks(next);
  };
  const say = (text: string, undo?: () => void) => setToast({ text, undo });

  function applyScenario(id: ScenarioId) {
    const s = build(id);
    setScenario(id);
    kept = s.works;
    setWorks(s.works);
    setNaming(s.naming);
    setMissing(s.missing);
    setRenameId(s.renameId);
    setRenameVal(s.renameId ? (SEED.find((w) => w.id === s.renameId)?.name ?? "") : "");
    setQ(s.q);
    setToast(withUndo(s));
    setUsb(s.usb);
    if (id === "robotOff") disconnect();
    else if (conn === "off") connectTo({ name: DEFAULT_ROBOT, port: "COM7" });
  }

  function openWork(w: Work) {
    setWorkName(w.name);
    go("workspace");
  }
  function createWork() {
    if (naming == null || checkName(naming, works) !== "ok") return;
    const name = naming.trim();
    const w: Work = {
      id: `n${Date.now()}`,
      name,
      robot: robotName,
      poses: 0,
      groups: 0,
      saved: translate("robot.works.messages.savedJustCreated"),
      a: [0, 0, 0, 0],
      bg: "var(--blue-tint)",
      fresh: true,
    };
    commit([w, ...works]);
    setNaming(null);
    setWorkName(name);
    go("workspace");
  }
  function startRename(w: Work) {
    setNaming(null);
    setRenameId(w.id);
    setRenameVal(w.name);
  }
  function finishRename(save: boolean) {
    const w = works.find((x) => x.id === renameId);
    if (!w || !save) return setRenameId(null);
    const name = renameVal.trim();
    if (name === w.name) return setRenameId(null);
    if (checkName(name, works.filter((x) => x.id !== w.id)) !== "ok") return;
    commit(works.map((x) => (x.id === w.id ? { ...x, name } : x)));
    if (w.name === workName) setWorkName(name);
    setRenameId(null);
  }
  function copyWork(w: Work) {
    const name = uniqueName(`${w.name} 2`, works);
    const copy: Work = {
      ...w,
      id: `c${Date.now()}`,
      name,
      tag: undefined,
      saved: translate("robot.works.messages.savedJustDuplicated"),
      fromUsb: false,
      fresh: true,
    };
    const at = works.findIndex((x) => x.id === w.id);
    commit([...works.slice(0, at + 1), copy, ...works.slice(at + 1)]);
    say(translate("robot.works.messages.toastDuplicated", { name: w.name }));
  }
  function removeWork(w: Work, verb: "trash" | "list") {
    const at = works.findIndex((x) => x.id === w.id);
    commit(works.filter((x) => x.id !== w.id));
    if (renameId === w.id) setRenameId(null);
    const restore = () => restoreRef.current(w, at);
    say(
      translate(
        verb === "trash" ? "robot.works.messages.toastTrashed" : "robot.works.messages.toastRemoved",
        { name: w.name },
      ),
      restore,
    );
  }
  function locate(w: Work) {
    setMissing((m) => m.filter((id) => id !== w.id));
    commit(
      worksRef.current.map((x) =>
        x.id === w.id
          ? { ...x, saved: translate("robot.works.messages.savedFoundOnUsb"), fromUsb: true }
          : x,
      ),
    );
    say(translate("robot.works.messages.toastLocated", { name: w.name }));
  }
  function openUsb() {
    if (usb) return;
    setUsb(true);
    later(() => {
      const cur = worksRef.current;
      const w: Work = {
        id: `u${Date.now()}`,
        name: uniqueName(SAMPLE_WORK_NAME_ROOM2, cur),
        robot: robotName,
        poses: 7,
        groups: 1,
        saved: translate("robot.works.messages.savedCopiedFromUsb"),
        fromUsb: true,
        a: [0, -30, 50, 10],
        bg: "var(--blue-tint)",
        fresh: true,
        tag: translate("robot.works.messages.tagFromUsb"),
      };
      setUsb(false);
      commit([w, ...cur]);
      say(translate("robot.works.messages.toastUsbCopied"));
    }, 1500);
  }

  const needle = q.trim();
  const shown = works
    .filter((w) => !needle || w.name.toLowerCase().includes(needle.toLowerCase()))
    .slice()
    .sort((a, b) => Number(b.robot === robotName) - Number(a.robot === robotName));
  const hasWorks = works.length > 0;
  const nameState: NameCheck = naming == null ? "empty" : checkName(naming, works);
  const dn = naming?.trim() ?? "";
  const renameState = renameId ? checkName(renameVal, works.filter((x) => x.id !== renameId)) : "ok";

  const pathHintKeys: Record<NameCheck, string> = {
    empty: "robot.works.messages.pathHintEmpty",
    bad: "robot.works.messages.pathHintBad",
    dup: "robot.works.messages.pathHintDup",
    ok: "robot.works.messages.pathHintOk",
  };
  const pathText = translate(pathHintKeys[nameState], { name: dn });

  const chipCls = conn === "connected" ? "" : conn === "off" ? "is-off" : conn === "lost" ? "is-lost" : "is-busy";
  const chipStatusKeys: Record<typeof conn, string> = {
    connected: "robot.works.messages.chipConnected",
    off: "robot.works.messages.chipOff",
    lost: "robot.works.messages.chipLost",
    connecting: "robot.works.messages.chipConnecting",
  };
  const chipSt = translate(chipStatusKeys[conn]);

  const onNamingKey = (event: KeyboardEvent<HTMLInputElement>) => {
    if (event.key === "Enter") {
      event.preventDefault();
      createWork();
    } else if (event.key === "Escape") {
      event.stopPropagation();
      setNaming(null);
    }
  };
  const onRenameKey = (event: KeyboardEvent<HTMLInputElement>) => {
    if (event.key === "Enter") {
      event.preventDefault();
      finishRename(true);
    } else if (event.key === "Escape") {
      event.stopPropagation();
      finishRename(false);
    }
  };

  return {
    go,
    robot,
    workName,
    sample,
    scenario,
    naming,
    setNaming,
    missing,
    renameId,
    setRenameId,
    renameVal,
    setRenameVal,
    q,
    setQ,
    toast,
    setToast,
    usb,
    live,
    robotName,
    say,
    applyScenario,
    openWork,
    createWork,
    startRename,
    finishRename,
    copyWork,
    removeWork,
    locate,
    openUsb,
    needle,
    shown,
    hasWorks,
    nameState,
    renameState,
    pathText,
    chipCls,
    chipSt,
    onNamingKey,
    onRenameKey,
  };
}

export default function WorksScreen() { return <WorksView v={useWorksScreen()} />; }
