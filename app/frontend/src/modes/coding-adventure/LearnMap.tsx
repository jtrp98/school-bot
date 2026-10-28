import { t, speechLanguage } from "../../i18n";
import { t as translate } from "../../i18n";
import { LearnMapView } from "../../components/coding-adventure/LearnMapView";
import { useCallback, useEffect, useRef, useState } from "react";

import { param } from "../../core/debugParams";
import { SAMPLE_ROBOT_NAME } from "../../shell";
import { type IconName } from "../../components/shared/Icons";

import { ScenarioBar, type Scenario } from "../../components/shared/ScenarioBar";

type Conn = "off" | "connecting" | "connected" | "lost";
type Area = "arm" | "block" | "quiz";
export type Stage = {
  t: string;
  short?: string;
  ic: IconName;
  area: Area;
  kind: string;
  goal: string;
  how: string;
  lab?: boolean;
  steps?: string[];
};

const BASE = [3, 2, 3, 3, 2, 3, 3];
const ST: Stage[] = [
  {
    t: "common.messages.meetTheRobotArm",
    ic: "book",
    area: "arm",
    kind: "common.messages.learn",
    goal: "common.messages.exploreThePartsOfARobotArm",
    how: "common.messages.readThenTapContinue",
  },
  {
    t: "common.messages.jointsS0S5",
    ic: "quiz",
    area: "quiz",
    kind: "common.messages.quiz",
    goal: "common.messages.learnTheNamesOfTheRobotArmSJoints",
    how: "common.messages.readTheQuestionAndChooseTheCorrectAnswer",
  },
  {
    t: "common.messages.matchTheJoints",
    ic: "game",
    area: "quiz",
    kind: "common.messages.game",
    goal: "common.messages.discoverWhatEachJointDoes",
    how: "common.messages.matchEachJointToItsName",
  },
  {
    t: "common.messages.moveJointS1ToRaiseTheArm",
    short: "common.messages.moveJointS1",
    ic: "arm",
    area: "arm",
    kind: "common.messages.realRobotPractice",
    goal: "common.messages.tryCommandingTheRobotArmToLift",
    how: "common.messages.followTheStepsWithYourGroupSRobot",
    lab: true,
    steps: [
      "common.messages.connectYourGroupSRobot",
      "common.messages.selectPose1ThenPressOnS1",
      "common.messages.checkWhetherTheArmLifts",
      "common.messages.saveAsANewPose",
    ],
  },
  {
    t: "common.messages.buildBlocksToLiftAnObject",
    ic: "block",
    area: "block",
    kind: "common.messages.buildBlocks",
    goal: "common.messages.useBlocksToMakeTheArmLiftAnObject",
    how: "common.messages.connectTheBlocksThenSendThemToTheRobot",
  },
  {
    t: "common.messages.pickUpABox",
    ic: "box",
    area: "arm",
    kind: "common.messages.robotMission",
    goal: "common.messages.makeTheArmPickUpAndPlaceABox",
    how: "common.messages.followTheMissionAndCheckThatTheRobotPicks",
    lab: true,
  },
  {
    t: "common.messages.bossLevel",
    ic: "crown",
    area: "quiz",
    kind: "common.messages.bossLevel",
    goal: "common.messages.useEverythingYouVeLearnedInThisUnit",
    how: "common.messages.completeTheFinalMission",
  },
];

type StageState = "done" | "current" | "locked";
type PanelState = StageState | "complete";

type Init = {
  cur: number;
  view: "map" | "list";
  sel: number | null;
  earned: Record<number, number>;
  celeb: boolean;
  run: boolean;
  conn: Conn;
  noRobot: boolean;
};
const SCENARIOS: Scenario[] = [
  { id: "map4", label: "common.messages.mapLevel4" },
  { id: "list", label: "lessons.views.list" },
  { id: "done2", label: "common.messages.selectACompletedLevel" },
  { id: "locked6", label: "common.messages.selectALockedLevel" },
  { id: "run5", label: "common.messages.missionInProgress" },
  { id: "celebrate", label: "common.messages.levelComplete" },
  { id: "complete", label: "common.messages.unitCompleteLabel" },
  { id: "start", label: "common.messages.startUnit" },
  { id: "noRobot", label: "common.messages.robotDisconnected" },
];
const base: Init = {
  cur: 4,
  view: "map",
  sel: null,
  earned: {},
  celeb: false,
  run: false,
  conn: "connected",
  noRobot: false,
};
function presetOf(id: string): Init {
  switch (id) {
    case "list": return { ...base, view: "list" };
    case "done2": return { ...base, sel: 2 };
    case "locked6": return { ...base, sel: 6 };
    case "run5": return { ...base, cur: 5, run: true };
    case "celebrate": return { ...base, cur: 5, celeb: true, earned: { 4: 3 } };
    case "complete": return { ...base, cur: 8 };
    case "start": return { ...base, cur: 1 };
    case "noRobot": return { ...base, conn: "off", noRobot: true };
    default: return base;
  }
}

type LearnMapOptions = { onChooseMode?: () => void };

export function useLearnMapScreen({ init, onChooseMode }: LearnMapOptions & { init: Init }) {
  const [cur, setCur] = useState(init.cur);
  const [view, setView] = useState(init.view);
  const [sel, setSel] = useState<number | null>(init.sel);
  const [earned, setEarned] = useState(init.earned);
  const [celeb, setCeleb] = useState(init.celeb);
  const [run, setRun] = useState(init.run);
  const [conn, setConn] = useState<Conn>(init.conn);
  const [noRobot, setNoRobot] = useState(init.noRobot);
  const [toast, setToast] = useState<string | null>(null);
  const toastT = useRef<number>(0);
  const connT = useRef<number>(0);

  useEffect(
    () => () => {
      window.clearTimeout(toastT.current);
      window.clearTimeout(connT.current);
    },
    [],
  );
  const say = useCallback((t: string) => {
    window.clearTimeout(toastT.current);
    setToast(t);
    toastT.current = window.setTimeout(() => setToast(null), 3500);
  }, []);

  const starsOf = (n: number) => earned[n] ?? BASE[n - 1];
  const statusOf = (n: number): StageState => (
    n < cur
      ? "done"
      : n === cur
        ? "current"
        : "locked"
  );
  const selNow = sel !== null
    ? sel
    : cur <= 7
      ? cur
      : null;
  const s = selNow !== null ? ST[selNow - 1] : null;
  const ps: PanelState = selNow === null
    ? "complete"
    : selNow > cur
      ? "locked"
      : selNow === cur
        ? "current"
        : "done";
  const info = ps === "current" || ps === "done";
  const showRun = run && info && !!s;
  const stepNow = conn === "connected" ? 1 : 0;

  let sum = 0;
  for (let n = 1; n < cur && n <= 7; n++) sum += starsOf(n);
  const total = 3 + sum;
  const doneCount = Math.min(cur - 1, 7);
  const prog = t("lessons.messages.countOf7LevelsCompleted", { count: doneCount });

  const pick = (n: number) => {
    setSel(n);
    setRun(false);
  };

  useEffect(() => {
    if (!showRun && !noRobot) return;
    const onKey = (e: KeyboardEvent) => {
      if (e.key !== "Escape") return;
      setRun(false);
      if (conn !== "connecting") setNoRobot(false);
    };
    window.addEventListener("keydown", onKey);
    return () => window.removeEventListener("keydown", onKey);
  }, [showRun, noRobot, conn]);

  const startStage = () => {
    if (s?.lab && conn !== "connected") setNoRobot(true);
    else setRun(true);
  };
  const doPrimary = () => {
    if (ps === "current" || ps === "done") startStage();
    else if (ps === "locked") setSel(null);
    else say(t("lessons.units.nextUnavailable"));
  };
  const finishRun = () => {
    if (ps === "current") {
      const next = cur + 1;
      setEarned((e) => ({ ...e, [cur]: 3 }));
      setCur(next);
      setSel(null);
      setRun(false);
      setCeleb(true);
    } else {
      setRun(false);
    }
  };
  const connectRobot = () => {
    if (conn === "connecting") return;
    setConn("connecting");
    window.clearTimeout(connT.current);
    connT.current = window.setTimeout(() => {
      setConn("connected");
      setNoRobot(false);
      say(translate("adventure.messages.robotConnectedKeepGoing", { name: SAMPLE_ROBOT_NAME }));
    }, 1400);
  };
  const speak = () => {
    if (!s) return;
    try {
      const text = `${t(s.t)}. ${t("lessons.details.learningGoal")} ${t(s.goal)}. ${
        s.steps ? s.steps.map(step => t(step)).join(" ") : t(s.how)}`;
      const u = new SpeechSynthesisUtterance(text);
      u.lang = speechLanguage();
      window.speechSynthesis.cancel();
      window.speechSynthesis.speak(u);
    } catch {
      // Reading aloud is optional; the full lesson remains visible if speech is unavailable.
    }
  };

  return {
    onChooseMode,
    cur,
    view,
    setView,
    setSel,
    celeb,
    setCeleb,
    setRun,
    conn,
    noRobot,
    setNoRobot,
    toast,
    say,
    starsOf,
    statusOf,
    selNow,
    s,
    ps,
    showRun,
    stepNow,
    total,
    prog,
    pick,
    doPrimary,
    finishRun,
    connectRobot,
    speak,
    ST,
  };
}

function LearnMapScreen(props: Parameters<typeof useLearnMapScreen>[0]) {
  return <LearnMapView {...useLearnMapScreen(props)} />;
}

export default function LearnMap({ onChooseMode }: LearnMapOptions = {}) {
  const asked = param("scenario");
  const [scn, setScn] = useState(SCENARIOS.some((x) => x.id === asked) ? (asked as string) : "map4");
  return (
    <>
      <LearnMapScreen key={scn} init={presetOf(scn)} onChooseMode={onChooseMode} />
      <ScenarioBar
        title={translate("common.messages.previewScenario")}
        items={SCENARIOS}
        value={scn}
        onChange={setScn}
      />
    </>
  );
}
