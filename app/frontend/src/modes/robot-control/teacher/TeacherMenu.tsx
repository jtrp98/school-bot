import { Button, Modal } from "../../../components/shared";
import {
  PIN,
  RESTART_MS,
  LOG_SHOW,
  type LogLine,
  type Restart,
  KEYS,
  SCENARIOS,
  type Init,
  presetOf,
} from "./teacherData";
import { t as translate } from "../../../i18n";
import { useCallback, useEffect, useRef, useState } from "react";

import { param } from "../../../core/debugParams";
import { SAMPLE_ROBOT_NAME, useSampleData } from "../../../shell";
import { ScenarioBar } from "../../../components/shared/ScenarioBar";
import { useRobot } from "../robotContext";
import { WindowControlsSlot } from "../../../components/shared/WindowTitlebar";

const rootBaseClass =
  "tm-root [--tm-ink:var(--ink)] [--tm-mute:var(--muted)] [--tm-blue:var(--blue-ink)] " +
  "[--tm-blue-d:var(--blue-press)] [--tm-line:var(--line)] [--tm-bg:var(--bg)] " +
  "[--tm-hover:var(--blue-tint)] [--tm-font:'Noto_Sans_Thai',_'Leelawadee_UI',_sans-serif] relative " +
  "box-border [flex:1_1_auto] w-full h-[var(--app-viewport-height,_100vh)] min-w-[1280px] min-h-[720px] " +
  "overflow-hidden bg-[var(--tm-bg)] text-[var(--tm-ink)] font-[family-name:var(--tm-font)] flex " +
  "flex-col leading-[normal] text-[16px] [&_*]:box-border [&_*::before]:box-border " +
  "[&_*::after]:box-border [&_button:focus-visible]:[outline:3px_solid_var(--tm-blue)] " +
  "[&_button:focus-visible]:[outline-offset:2px] [&_button:focus-visible]:shadow-none " +
  "[&_input:focus-visible]:[outline:3px_solid_var(--tm-blue)] " +
  "[&_input:focus-visible]:[outline-offset:2px] [&_input:focus-visible]:shadow-none " +
  "[&_.tm-in:focus-visible]:outline-none [&_.tm-in:focus-visible]:border-[var(--tm-blue)] " +
  "[&_.tm-in:focus-visible]:shadow-[0_0_0_3px_var(--blue-tint)] [&.sample_.tm-cmd]:mr-[56px]";
const headerClass =
  "tm-head h-[60px] shrink-0 py-0 px-[24px] bg-[var(--tm-ink)] text-white flex items-center gap-[12px] " +
  "[&_.tm-hbtn:focus-visible]:[outline-color:var(--surface)] window-integrated-header";
const headerButtonClass =
  "tm-hbtn h-[40px] py-0 px-[14px] border-2 border-solid border-blue-line rounded-[12px] bg-transparent " +
  "text-white text-[14px] font-bold cursor-pointer [&.solid]:inline-flex [&.solid]:items-center " +
  "[&.solid]:py-0 [&.solid]:px-[16px] [&.solid]:border-[medium] [&.solid]:border-none " +
  "[&.solid]:border-current [&.solid]:bg-white [&.solid]:text-[var(--tm-ink)]";
const pinPanelClass =
  "tm-pin w-[400px] bg-white border-2 border-solid border-[var(--tm-line)] rounded-[22px] p-[28px] flex " +
  "flex-col items-center gap-[16px]";
const pinDotsClass = "tm-dots flex gap-[12px]";
const pinDotBaseClass =
  "tm-dot w-[18px] h-[18px] rounded-full border-2 border-solid border-blue-line [&.on]:border-[medium] " +
  "[&.on]:border-none [&.on]:border-current [&.on]:bg-[var(--tm-blue)] [&.bad]:border-err";
const pinMessageBaseClass = "tm-pin-msg text-[14px] font-semibold text-[var(--tm-mute)] [&.bad]:text-err-ink";
const keypadClass = "tm-keys grid [grid-template-columns:repeat(3,_84px)] gap-[10px]";
const keyClass =
  "tm-key h-[60px] rounded-[14px] border-2 border-solid border-[var(--tm-line)] border-b-4 bg-white " +
  "text-[22px] font-bold text-[var(--tm-ink)] cursor-pointer hover:bg-[var(--tm-hover)]";
const navClass =
  "tm-nav w-[260px] shrink-0 py-[20px] px-[14px] bg-white border-r-2 [border-right-style:solid] " +
  "border-r-[var(--tm-line)] flex flex-col gap-[4px]";
const tabClass =
  "tm-tab flex items-center gap-[10px] h-[60px] py-0 px-[12px] rounded-[12px] text-[16px] font-bold " +
  "cursor-pointer text-left text-[var(--tm-ink)] border-2 border-solid border-transparent bg-white " +
  "hover:bg-[var(--tm-hover)] [&[aria-current='true']]:border-[var(--tm-blue)] " +
  "[&[aria-current='true']]:bg-[var(--tm-hover)]";
const tabSubClass = "tm-tab-sub text-[12px] font-medium text-[var(--tm-mute)]";
const mainClass = "tm-main [flex:1_1_0] min-w-0 py-[24px] px-[32px] flex flex-col gap-[16px] overflow-hidden";
const headClass =
  "tm-h flex flex-col gap-[2px] [&_h1]:m-0 [&_h1]:text-[24px] [&_h1]:font-bold [&_span]:text-[14px] " +
  "[&_span]:text-[var(--tm-mute)]";
const cardClass =
  "tm-card bg-white border-2 border-solid border-[var(--tm-line)] rounded-[16px] p-[18px] flex flex-col " +
  "gap-[10px]";
const cardTitleClass = "tm-card-ttl text-[17px] font-bold";
const cardDescClass = "tm-card-desc text-[14px] text-[var(--tm-mute)] leading-[1.5]";
const actionButtonBaseClass =
  "tm-ob [align-self:flex-start] h-[44px] py-0 px-[18px] border-2 border-solid border-line-strong " +
  "border-b-4 rounded-[12px] bg-white text-[var(--tm-ink)] text-[15px] font-bold cursor-pointer " +
  "[&:hover:not(:disabled)]:bg-[var(--tm-hover)] [&:hover:not(:disabled)]:border-[var(--tm-blue)] " +
  "disabled:opacity-[0.55] disabled:cursor-not-allowed [&.red]:border-err-line [&.red]:text-err-ink";
const askClass = "tm-ask flex items-center gap-[8px] p-[10px] rounded-[10px] bg-err-tint";
const askTextClass = "tm-ask-q [flex:1_1_0] text-[14px] font-semibold text-err-ink";
const askNoClass =
  "tm-ask-no h-[36px] py-0 px-[12px] border-2 border-solid border-line-strong rounded-[9px] bg-white " +
  "text-[var(--tm-ink)] text-[14px] font-bold cursor-pointer";
const askYesClass =
  "tm-ask-yes h-[36px] py-0 px-[12px] border-[medium] border-none border-current border-b-[3px] " +
  "[border-bottom-style:solid] border-b-err-ink rounded-[9px] bg-err text-white text-[14px] font-bold " +
  "cursor-pointer";
const consoleClass =
  "tm-con [flex:1_1_0] min-h-0 bg-ink rounded-[16px] p-[16px] flex flex-col gap-[10px] " +
  "[&_.tm-hbtn:focus-visible]:[outline-color:var(--surface)] " +
  "[&_button:focus-visible]:[outline-color:var(--surface)] " +
  "[&_input:focus-visible]:[outline-color:var(--surface)]";
const clearButtonClass =
  "tm-clr h-[30px] py-0 px-[10px] border-1 border-solid border-blue-press rounded-[8px] bg-transparent " +
  "text-line text-[12px] cursor-pointer disabled:opacity-[0.5] disabled:cursor-not-allowed";
const logClass =
  "tm-log [flex:1_1_0] min-h-0 overflow-hidden flex flex-col justify-end gap-[2px] " +
  "font-[family-name:Consolas,_'Courier_New',_monospace] text-[13px] leading-[1.6] " +
  "[&_.i]:text-blue-line [&_.o]:text-ok-line [&_.s]:text-star [&_.e]:text-blue-line";
const commandInputClass =
  "tm-in [flex:1_1_0] h-[40px] py-0 px-[12px] border-2 border-solid border-blue-press rounded-[10px] " +
  "bg-[var(--tm-ink)] text-white font-[family-name:Consolas,_monospace] text-[14px] focus:outline-none " +
  "focus:border-[var(--tm-blue)] focus:shadow-[0_0_0_3px_var(--blue-tint)] disabled:opacity-[0.55]";
const sendButtonClass =
  "tm-send h-[40px] py-0 px-[18px] rounded-[10px] text-[14px] font-bold border-[medium] border-none " +
  "border-current bg-[var(--tm-blue)] text-white cursor-pointer disabled:border-1 disabled:border-solid " +
  "disabled:border-blue-press disabled:bg-transparent disabled:text-blue-line " +
  "disabled:cursor-not-allowed";
const toastClass =
  "tm-toast absolute left-[50%] bottom-[28px] [transform:translateX(-50%)] py-[12px] px-[18px] " +
  "rounded-[12px] bg-[var(--tm-ink)] text-white text-[15px] font-semibold whitespace-nowrap shadow-pop " +
  "z-[70]";

function TeacherScreen({ init, sample }: { init: Init; sample: boolean }) {
  const { conn, robot, go, from } = useRobot();
  const [locked, setLocked] = useState(init.locked);
  const [pin, setPin] = useState(init.pin);
  const [bad, setBad] = useState(init.bad);
  const [restart, setRestart] = useState<Restart>(init.restart);
  const [ask, setAsk] = useState(init.ask);
  const [log, setLog] = useState<LogLine[]>(init.log);
  const [typed, setTyped] = useState(init.typed);
  const [toast, setToast] = useState<string | null>(null);
  const toastT = useRef(0);
  const restartT = useRef(0);

  const offline = init.off || conn !== "connected";
  const port = robot?.port ?? "COM7";
  const robotName = robot?.name ?? SAMPLE_ROBOT_NAME;

  const say = useCallback((t: string) => {
    window.clearTimeout(toastT.current);
    setToast(t);
    toastT.current = window.setTimeout(() => setToast(null), 3000);
  }, []);

  const startRestart = useCallback(() => {
    setRestart("busy");
    window.clearTimeout(restartT.current);
    restartT.current = window.setTimeout(() => {
      setRestart("done");
      setLog((l) => [...l, ["s", translate("robot.teacher.messages.restartLog", { port })] as LogLine]);
    }, RESTART_MS);
  }, [port]);

  useEffect(() => {

    if (init.restart === "busy") startRestart();
    return () => {
      window.clearTimeout(toastT.current);
      window.clearTimeout(restartT.current);
    };
  }, []);

  const pinRef = useRef(init.pin);
  const setPinBoth = useCallback((v: string) => {
    pinRef.current = v;
    setPin(v);
  }, []);
  const press = useCallback(
    (k: string) => {
      if (k === "⌫") {
        setPinBoth(pinRef.current.slice(0, -1));
        setBad(false);
        return;
      }
      if (k === "") return;
      const next = (pinRef.current + k).slice(0, 4);
      if (next.length < 4) {
        setPinBoth(next);
        setBad(false);
        return;
      }
      setPinBoth("");
      if (next === PIN) {
        setLocked(false);
        setBad(false);
      } else setBad(true);
    },
    [setPinBoth],
  );

  useEffect(() => {
    if (!locked) return;
    const onKey = (e: KeyboardEvent) => {
      if (e.ctrlKey || e.metaKey || e.altKey) return;
      if (/^[0-9]$/.test(e.key)) press(e.key);
      else if (e.key === "Backspace") press("⌫");
    };
    window.addEventListener("keydown", onKey);
    return () => window.removeEventListener("keydown", onKey);
  }, [locked, press]);

  const lock = () => {
    setLocked(true);
    setPinBoth("");
    setBad(false);
    setAsk(false);
  };
  const back = () => go(from === "teacher" ? "workspace" : from);

  const send = () => {
    const c = typed.trim();
    if (!c || offline) return;
    setLog((l) => [...l, ["i", `> ${c}`] as LogLine, ["o", "< OK"] as LogLine]);
    setTyped("");
  };
  const doRestart = () => {
    if (offline || restart === "busy") return;
    startRestart();
  };
  const doReset = () => {
    setAsk(false);
    say(translate("robot.teacher.messages.resetDone"));
  };

  const statText = offline
    ? translate("robot.teacher.messages.statusOffline")
    : translate("robot.teacher.messages.statusConnected", { name: robotName, port });

  return (
    <div className={`${rootBaseClass}${sample ? " sample" : ""}`}>
      <header className={headerClass}>
        <span className={"tm-head-ttl text-[20px] font-bold"}>{translate("robot.teacher.messages.teacherMenu")}</span>
        <span className={"tm-head-stat text-[14px] text-line"}>{statText}</span>
        <div className={"tm-grow grow"} />
        {!locked && (
          <Button
            variant="custom"
            type="button"
            className={headerButtonClass}
            onClick={lock}
          >
            {translate("robot.teacher.messages.lockTeacherMenu")}
          </Button>
        )}
        <Button
          variant="custom"
          type="button"
          className={`${headerButtonClass} solid`}
          onClick={back}
        >
          {translate("robot.teacher.messages.backToStudentView")}
        </Button>
        <WindowControlsSlot />
      </header>

      {locked ? (
        <div className={"tm-pin-wrap [flex:1_1_0] min-h-0 flex items-center justify-center"}>
          <section
            className={pinPanelClass}
            aria-label={translate("robot.teacher.messages.enterTeacherPin")}
          >
            <span className={"tm-pin-ttl text-[22px] font-bold"}>
              {translate("robot.teacher.messages.enterTeacherPin")}
            </span>
            <span className={"tm-pin-sub text-[14px] text-[var(--tm-mute)] text-center"}>
              {translate("robot.teacher.messages.robotSettingsCanBeChangedHereTeachersOnly")}
            </span>
            <div
              className={pinDotsClass}
              role="img"
              aria-label={translate("robot.teacher.messages.value0Of4DigitsEntered", {value0: pin.length})}
            >
              {[0, 1, 2, 3].map((i) => (
                <span
                  key={i}
                  className={`${pinDotBaseClass}${i < pin.length ? " on" : ""}${bad ? " bad" : ""}`}
                />
              ))}
            </div>
            <span
              className={`${pinMessageBaseClass}${bad ? " bad" : ""}`}
              role={bad ? "alert" : "status"}
            >
              {bad
                ? translate("robot.teacher.messages.incorrectPinTryAgain")
                : translate("robot.teacher.messages.enter4Digits")}
            </span>
            <div className={keypadClass}>
              {KEYS.map((k, i) =>
                k === "" ? (
                  <span key={i} aria-hidden="true" />
                ) : (
                  <Button
                    variant="custom"
                    key={k}
                    type="button"
                    className={keyClass}
                    aria-label={k === "⌫" ? translate("common.delete") : k}
                    onClick={() => press(k)}
                  >
                    {k}
                  </Button>
                ),
              )}
            </div>
            {sample && <span className={"tm-pin-hint text-[12px] text-faint"}>
              {translate("robot.teacher.messages.simulationPinHint", { pin: PIN })}
            </span>}
          </section>
        </div>
      ) : (
        <div className={"tm-stage [flex:1_1_0] min-h-0 flex"}>
          <nav
            className={navClass}
            aria-label={translate("robot.teacher.messages.teacherMenuCategories")}
          >
            <Button
              variant="custom"
              type="button"
              className={tabClass}
              aria-current="true"
            >
              <span className={"tm-tab-ic text-[20px] w-[28px] text-center"}>🛠</span>
              <span className={"tm-tab-tx flex flex-col items-start leading-[1.3]"}>
                <span>{translate("robot.teacher.messages.troubleshooting")}</span>
                <span className={tabSubClass}>{translate("robot.teacher.messages.restartConsole")}</span>
              </span>
            </Button>
          </nav>

          <main className={mainClass}>
            <div className={headClass}>
              <h1>{translate("robot.teacher.messages.troubleshootingLabel")}</h1>
              <span>{translate("robot.teacher.messages.whenTheRobotFreezesOrBehavesUnexpectedly")}</span>
            </div>

            <div className={"tm-grid grid [grid-template-columns:1fr_1fr] gap-[16px]"}>
              <div className={cardClass}>
                <span className={cardTitleClass}>{translate("robot.teacher.messages.restartControlBoard")}</span>
                <span className={cardDescClass}>
                  {translate("robot.teacher.messages.useWhenTheRobotIsUnresponsiveTheArmWill")}
                </span>
                <span className={"tm-card-ok text-[14px] font-semibold text-ok-ink"} role="status">
                  {restart === "done" ? translate("robot.teacher.messages.restartCompleteRobotReconnected") : ""}
                </span>
                <Button
                  variant="custom"
                  type="button"
                  className={actionButtonBaseClass}
                  disabled={offline || restart === "busy"}
                  onClick={doRestart}
                >
                  {restart === "busy"
                    ? translate("robot.teacher.messages.restarting")
                    : translate("robot.teacher.messages.restartBoard")}
                </Button>
              </div>

              <div className={cardClass}>
                <span className={cardTitleClass}>{translate("robot.teacher.messages.restoreRobotDefaults")}</span>
                <span className={cardDescClass}>
                  {translate("robot.teacher.messages.resetRobotTimingIdsAndPowerOnGroupsTo")}
                </span>
                {ask && (
                  <Modal
                    layout="inline"
                    className={askClass}
                    label={translate("robot.teacher.messages.confirmRestoreDefaults")}
                  >
                    <span className={askTextClass}>
                      {translate("robot.teacher.messages.areYouSureThisCannotBeUndone")}
                    </span>
                    <Button
                      variant="custom"
                      type="button"
                      className={askNoClass}
                      onClick={() => setAsk(false)}
                    >{translate("common.cancel")}</Button>
                    <Button
                      variant="custom"
                      type="button"
                      className={askYesClass}
                      onClick={doReset}
                    >{translate("robot.teacher.messages.restoreNow")}</Button>
                  </Modal>
                )}
                {!ask && (
                  <Button
                    variant="custom"
                    type="button"
                    className={`${actionButtonBaseClass} red`}
                    disabled={offline}
                    onClick={() => setAsk(true)}
                  >
                    {translate("robot.teacher.messages.restoringDefaults")}
                  </Button>
                )}
              </div>
            </div>

            <section
              className={consoleClass}
              aria-label={translate("robot.teacher.messages.commandConsole")}
            >
              <div className={"tm-con-hd flex items-center gap-[10px]"}>
                <span className={"tm-con-ttl text-[15px] font-bold text-white"}>
                  {translate("robot.teacher.messages.commandConsole")}
                </span>
                <span className={"tm-con-d text-[12px] text-blue-line"}>
                  {translate("robot.teacher.messages.sendRawBoardCommandsViewResponses")}
                </span>
                <div className={"tm-grow grow"} />
                <Button
                  variant="custom"
                  type="button"
                  className={clearButtonClass}
                  onClick={() => setLog([])}
                >
                  {translate("robot.teacher.messages.clearConsole")}
                </Button>
              </div>
              <div className={logClass} role="log" aria-live="polite">
                {log.slice(-LOG_SHOW).map((l, i) => (
                  <span key={i} className={l[0]}>{l[1]}</span>
                ))}
                {offline && (
                  <span className="e">
                    {translate("robot.teacher.messages.robotDisconnectedConnectBeforeSendingCommands")}
                  </span>
                )}
              </div>
              <form
                className={"tm-cmd flex gap-[8px] m-0"}
                onSubmit={(e) => {
                  e.preventDefault();
                  send();
                }}
              >
                <input
                  className={commandInputClass}
                  type="text"
                  aria-label={translate("robot.teacher.messages.command")}
                  placeholder={translate("robot.teacher.messages.forExample1p1500t1000")}
                  value={typed}
                  autoComplete="off"
                  spellCheck={false}
                  disabled={offline}
                  onChange={(e) => setTyped(e.target.value)}
                />
                <Button
                  variant="custom"
                  type="submit"
                  className={sendButtonClass}
                  disabled={offline || !typed.trim()}
                >
                  {translate("robot.teacher.messages.send")}
                </Button>
              </form>
            </section>
          </main>
        </div>
      )}

      {toast && (
        <div className={toastClass} role="status">
          {translate(toast)}
        </div>
      )}
    </div>
  );
}

export default function TeacherMenu() {
  const sample = useSampleData();
  const asked = param("scenario");
  const [scn, setScn] = useState(SCENARIOS.some((x) => x.id === asked) ? (asked as string) : "");
  return (
    <>
      <TeacherScreen key={scn} init={presetOf(scn)} sample={sample} />
      {sample && (
        <div className={"tm-scn [&_.sb-scenario]:left-[12px] [&_.sb-scenario]:bottom-[12px]"}>
          <ScenarioBar
            title={translate("common.messages.previewScenario")}
            items={SCENARIOS}
            value={scn}
            onChange={setScn}
          />
        </div>
      )}
    </>
  );
}
