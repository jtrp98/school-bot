import { t as translate } from "../../i18n";
import type { ReactNode } from "react";
import { Icon } from "../shared/Icons";
import { Button, IconBtn, Stars } from "../shared";
import type { useLearnMapScreen } from "../../modes/coding-adventure/LearnMap";
import { useLessonContentFit } from "../../modes/coding-adventure/useLessonContentFit";
import { SchoolBotMascot } from "../shared/SchoolBotMascot";
import { KindTag } from "./LessonKindTag";
import { shortOf } from "./lessonPresentation";

const SpeakerIcon = () => (
  <svg
    className={"icon shrink-0"}
    width={24}
    height={24}
    viewBox="0 0 24 24"
    fill="none"
    stroke="currentColor"
    strokeWidth={2}
    strokeLinecap="round"
    strokeLinejoin="round"
    aria-hidden="true"
  >
    <path d="M4 10v4h4l5 4V6L8 10z" />
    <path d="M16 9a4 4 0 0 1 0 6M18.5 6.5a8 8 0 0 1 0 11" />
  </svg>
);

const celebClass =
  "lm-celeb flex items-center gap-[12px] py-[12px] px-[14px] bg-ok-tint border-2 border-solid border-ok " +
  "rounded-[18px] shrink-0 animate-[lmPop_0.45s_cubic-bezier(0.2,_1.4,_0.4,_1)_both] " +
  "motion-reduce:animate-none";
const celebIconClass =
  "lm-celeb-ic w-[44px] h-[44px] shrink-0 rounded-[50%] bg-ok text-white flex items-center " +
  "justify-center";
const celebTextClass = "sb-grow [flex:1] min-w-0 sb-col flex flex-col gap-0";
const celebTitleClass =
  "lm-celeb-t not-italic [font-variant:normal] font-extrabold [font-stretch:normal] text-[20px] " +
  "leading-[1.3] font-heading text-ok-ink";
const celebSubClass =
  "lm-celeb-s not-italic [font-variant:normal] font-semibold [font-stretch:normal] text-[16px] " +
  "leading-[1.4] font-body";
const completeClass =
  "lm-complete [flex:1] flex flex-col items-center justify-center text-center gap-[14px] py-[6px] px-0 " +
  "[&_.t]:not-italic [&_.t]:[font-variant:normal] [&_.t]:font-extrabold [&_.t]:[font-stretch:normal] " +
  "[&_.t]:text-[28px] [&_.t]:leading-[1.3] [&_.t]:font-heading [&_.s]:not-italic " +
  "[&_.s]:[font-variant:normal] [&_.s]:font-medium [&_.s]:[font-stretch:normal] [&_.s]:text-[19px] " +
  "[&_.s]:leading-[1.5] [&_.s]:font-body [&_.s]:text-muted";
const bigButtonClass = "lm-bigbtn [&.lm-bigbtn]:min-h-[64px] [&.lm-bigbtn]:text-[22px] [&.lm-bigbtn]:rounded-[14px]";
const lockChipClass =
  "lm-lockchip inline-flex items-center gap-[6px] min-h-[36px] py-0 px-[14px] rounded-full bg-sunken " +
  "border-2 border-dashed border-line-strong text-muted not-italic [font-variant:normal] font-extrabold " +
  "[font-stretch:normal] text-[15px] leading-none font-body";
const strongTextClass = "sb-grow [flex:1] min-w-0 sb-strong font-bold sb-muted text-muted text-[17px]";
const strongLabelClass = "sb-strong font-bold sb-muted text-muted text-[17px]";
const titleClass =
  "lm-pt not-italic [font-variant:normal] font-extrabold [font-stretch:normal] text-[28px] " +
  "leading-[1.3] font-heading";
const lockBoxClass =
  "lm-lockbox flex items-center gap-[14px] p-[14px] bg-bg border-2 border-dashed border-line-strong " +
  "rounded-[20px]";
const labelClass =
  "lm-lab not-italic [font-variant:normal] font-extrabold [font-stretch:normal] text-[15px] " +
  "leading-[1.3] font-body text-muted mb-[4px]";
const goalClass =
  "lm-goal not-italic [font-variant:normal] font-medium [font-stretch:normal] text-[19px] leading-[1.5] " +
  "font-body";
const headRowClass = "sb-row flex items-center gap-[12px]";
const kindRowClass = "sb-grow [flex:1] min-w-0 sb-row flex items-center gap-[10px]";
const iconBtnClass =
  "sb-iconbtn w-[48px] h-[48px] min-w-[44px] inline-flex items-center justify-center rounded-control " +
  "bg-surface border-2 border-solid border-line-strong border-b-4 text-ink cursor-pointer p-0 " +
  "[transition:transform_.08s,background_.12s] hover:bg-blue-tint hover:border-blue " +
  "[&.sb-is-hover]:bg-blue-tint [&.sb-is-hover]:border-blue focus-visible:outline-none " +
  "focus-visible:shadow-[var(--focus)] [&.sb-is-focus]:outline-none " +
  "[&.sb-is-focus]:shadow-[var(--focus)] active:[transform:translateY(2px)] active:border-b-2 " +
  "[&.sb-is-active]:[transform:translateY(2px)] [&.sb-is-active]:border-b-2 disabled:bg-off " +
  "disabled:border-off disabled:text-faint disabled:cursor-not-allowed [&.sb-is-disabled]:bg-off " +
  "[&.sb-is-disabled]:border-off [&.sb-is-disabled]:text-faint [&.sb-is-disabled]:cursor-not-allowed " +
  "[&.sb-is-loading]:pointer-events-none";
const stepsLabelClass =
  "lm-lab not-italic [font-variant:normal] font-extrabold [font-stretch:normal] text-[15px] " +
  "leading-[1.3] font-body text-muted m-0";
const segsClass =
  "lm-segs flex gap-[5px] mb-[12px] [&_i]:[flex:1] [&_i]:h-[10px] [&_i]:rounded-full [&_i]:bg-off " +
  "[&_i.d]:bg-ok [&_i.n]:bg-blue";
const stepsListClass =
  "lm-steps [list-style:none] m-0 p-0 flex flex-col gap-[8px] [&_li]:flex [&_li]:items-center " +
  "[&_li]:gap-[12px] [&_li]:py-[8px] [&_li]:px-[12px] [&_li]:min-h-[50px] [&_li]:rounded-[14px] " +
  "[&_li]:bg-bg [&_li]:border-2 [&_li]:border-solid [&_li]:border-transparent [&_li]:not-italic " +
  "[&_li]:[font-variant:normal] [&_li]:font-semibold [&_li]:[font-stretch:normal] [&_li]:text-[17px] " +
  "[&_li]:leading-[1.4] [&_li]:font-body [&_li.now]:bg-blue-tint [&_li.now]:border-blue " +
  "[&_li.now]:font-bold";
const stepDotBaseClass =
  "lm-sd w-[30px] h-[30px] shrink-0 rounded-[50%] flex items-center justify-center not-italic " +
  "[font-variant:normal] font-extrabold [font-stretch:normal] text-[15px] leading-none font-heading " +
  "bg-white text-muted border-2 border-solid border-line-strong box-border [&.d]:bg-ok [&.d]:border-ok " +
  "[&.d]:text-white [&.n]:bg-blue-ink [&.n]:border-blue-ink [&.n]:text-white";
const stepSrClass = "lm-sr absolute w-[1px] h-[1px] overflow-hidden [clip:rect(0_0_0_0)] whitespace-nowrap";
const howClass =
  "lm-how not-italic [font-variant:normal] font-bold [font-stretch:normal] text-[19px] leading-[1.5] " +
  "font-body bg-blue-tint border-2 border-solid border-blue-line rounded-card py-[12px] px-[14px] " +
  "text-blue-ink";
const rewardClass =
  "lm-reward flex items-center gap-[10px] [&_span.t]:not-italic [&_span.t]:[font-variant:normal] " +
  "[&_span.t]:font-bold [&_span.t]:[font-stretch:normal] [&_span.t]:text-[16px] " +
  "[&_span.t]:leading-[1.3] [&_span.t]:font-body [&_span.t]:text-muted";
const panelClass =
  "lm-panel w-[clamp(320px,_34%,_460px)] shrink-0 min-h-0 min-w-0 bg-surface border-2 border-solid border-line " +
  "rounded-[24px] p-[22px] flex flex-col gap-[14px] shadow-card box-border " +
  "[@media(max-height:_800px)]:p-[16px] [@media(max-height:_800px)]:gap-[8px]";
const panelBodyClass =
  "lm-pbody flex-1 min-h-0 overflow-hidden -m-[4px]";
const panelContentClass =
  "lm-pcontent min-h-full flex flex-col gap-[16px] p-[4px] origin-top [&_>_*]:shrink-0 " +
  "[@media(max-height:_800px)]:gap-[8px]";

type Props = Pick<ReturnType<typeof useLearnMapScreen>,
  "cur"
  | "setSel"
  | "celeb"
  | "setCeleb"
  | "conn"
  | "setNoRobot"
  | "say"
  | "starsOf"
  | "selNow"
  | "s"
  | "ps"
  | "stepNow"
  | "doPrimary"
  | "speak"
>;

export function LessonDetails({
  cur,
  setSel,
  celeb,
  setCeleb,
  conn,
  setNoRobot,
  say,
  starsOf,
  selNow,
  s,
  ps,
  stepNow,
  doPrimary,
  speak
}: Props) {
  const { viewportRef, contentRef, scale } = useLessonContentFit();

  const celebSub = cur >= 8
    ? translate("common.messages.allLevelsInThisUnitCompleteYouEarned3")
    : translate("lessons.celebration.unlocked", { level: cur });
  const celebBar = celeb && cur - 1 >= 1 && (
    <div className={celebClass} role="status">
      <span className={celebIconClass} aria-hidden="true"><Icon name="check" size={24} /></span>
      <span className={celebTextClass}>
        <span className={celebTitleClass}>{translate("lessons.messages.wellDoneMissionComplete")}</span>
        <span className={celebSubClass}>{celebSub}</span>
      </span>
      <IconBtn
        icon="x"
        label={translate("lessons.messages.dismissMessage")}
        size={20}
        onClick={() => setCeleb(false)}
      />
    </div>
  );

  let body: ReactNode = null;
  let bottom: ReactNode = null;
  if (ps === "complete") {
    body = (
      <div className={completeClass}>
        <SchoolBotMascot w={120} h={171} cheer />
        <div className="t">{translate("lessons.messages.greatWorkUnitComplete")}</div>
        <div className="s">{translate("lessons.messages.tapALevelOnTheMapToPlayAgain")}</div>
      </div>
    );
    bottom = (
      <Button block iconRight="arrowR" className={bigButtonClass} onClick={doPrimary}>
        {translate("lessons.messages.goToTheNextUnit")}
      </Button>
    );
  } else if (ps === "locked" && s && selNow) {
    const msg = selNow === cur + 1
      ? translate("lessons.locked.previousLevel", { level: cur })
      : translate("lessons.locked.remainingLevels", { count: selNow - cur });
    body = (
      <>
        <div className={headRowClass}>
          <span className={lockChipClass}><Icon name="lock" size={18} />{translate("common.messages.locked")}</span>
          <span className={strongTextClass}>
            {translate("lessons.messages.levelNumber", { level: selNow })}
          </span>
        </div>
        <div className={titleClass}>{shortOf(s)}</div>
        <div className={lockBoxClass} role="status">
          <SchoolBotMascot w={70} h={100} />
          <div className={"sb-col flex flex-col gap-[4px]"} >
            <span className={"[font:800_20px/1.3_var(--f-head)]"} >
              {translate("lessons.messages.thisLevelIsLocked")}
            </span>
            <span className={"[font:500_18px/1.5_var(--f-body)] [text-wrap:balance]"} >{msg}</span>
          </div>
        </div>
        <div>
          <div className={labelClass}>{translate("lessons.messages.comingUp")}</div>
          <div className={goalClass}>{translate(s.goal)}</div>
        </div>
      </>
    );
    bottom = (
      <>
        <Button block iconRight="arrowR" className={bigButtonClass} onClick={() => setSel(null)}>
          {translate("lessons.messages.goToLevelNumber", { level: cur })}
        </Button>
        <Button block variant="secondary" size="md" icon="map" onClick={() => setSel(null)}>
          {translate("lessons.messages.backToMap")}
        </Button>
      </>
    );
  } else if (s && selNow) {
    const hasSteps = !!s.steps;
    const done = ps === "done";
    const earnedNow = done ? starsOf(selNow) : 0;
    const isDone = (i: number) => done || i < stepNow;
    body = (
      <>
        <div className={headRowClass}>
          <span className={kindRowClass} >
            <KindTag s={s} />
            <span className={strongLabelClass} >
              {translate("lessons.messages.levelNumber", { level: selNow })}
            </span>
          </span>
          <button
            type="button"
            className={iconBtnClass}
            aria-label={translate("lessons.messages.listenToTheMission")}
            title={translate("lessons.messages.listenToTheMission")}
            onClick={speak}
          >
            <SpeakerIcon />
          </button>
        </div>
        <div className={titleClass}>{translate(s.t)}</div>
        <div>
          <div className={labelClass}>{translate("lessons.details.learningGoal")}</div>
          <div className={goalClass}>{translate(s.goal)}</div>
        </div>
        {hasSteps && s.steps ? (
          <div>
            <div className={"sb-row flex items-center gap-[12px] justify-between mb-[8px]"} >
              <span className={stepsLabelClass} >{translate("lessons.details.steps")}</span>
              <span className={"[font:700_15px/1.3_var(--f-body)] text-blue-ink"} >
                {done
                  ? translate("lessons.messages.value0StepsComplete", {value0: s.steps.length})
                  : translate("lessons.messages.stepValue0OfValue1", {value0: stepNow + 1, value1: s.steps.length})}
              </span>
            </div>
            <div className={segsClass} aria-hidden="true">
              {s.steps.map((_, i) => (
                <i
                  key={i}
                  className={
                    isDone(i)
                      ? "d"
                      : !done && i === stepNow
                        ? "n"
                        : undefined
                  }
                />
              ))}
            </div>
            <ol className={stepsListClass}>
              {s.steps.map((t, i) => {
                const d = isDone(i);
                const nw = !done && i === stepNow;
                const dotState = d
                  ? "d"
                  : nw
                    ? "n"
                    : "";
                return (
                  <li key={i} className={nw ? "now" : undefined}>
                    <span
                      className={`${stepDotBaseClass} ${dotState}`}
                      aria-hidden="true"
                    >
                      {d ? <Icon name="check" size={16} /> : i + 1}
                    </span>
                    <span className={"sb-grow [flex:1] min-w-0"}>
                      {translate(t)}
                      <span className={stepSrClass}>
                        {d
                          ? <>{" "}{translate("lessons.messages.done")}</>
                          : nw
                            ? <>{" "}{translate("lessons.messages.currentStep")}</>
                            : ""}
                      </span>
                    </span>
                  </li>
                );
              })}
            </ol>
          </div>
        ) : (
          <div>
            <div className={labelClass}>{translate("lessons.details.steps")}</div>
            <div className={howClass}>{translate(s.how)}</div>
          </div>
        )}
        {s.lab && (
          <div>
            <Button
              variant="secondary"
              size="sm"
              icon="arm"
              onClick={() =>
                conn === "connected"
                  ? say(translate("lessons.actions.openArmLabHint"))
                  : setNoRobot(true)
              }
            >{translate("lessons.messages.openArmLab")}</Button>
          </div>
        )}
        <div className={rewardClass}>
          <Stars n={earnedNow} px={30} />
          <span className="t">
            {done
              ? translate("lessons.messages.value0StarsEarned", {value0: earnedNow})
              : translate("lessons.messages.earnUpTo3Stars")}
          </span>
        </div>
      </>
    );
    bottom = (
      <>
        <Button block iconRight="play" className={bigButtonClass} onClick={doPrimary}>
          {done
            ? translate("lessons.messages.playAgain")
            : hasSteps
              ? translate("common.continue")
              : translate("lessons.start")}
        </Button>
        {done && (
          <Button block variant="secondary" size="md" icon="map" onClick={() => setSel(null)}>
            {translate("lessons.messages.backToMap")}
          </Button>
        )}
      </>
    );
  }


  return (
    <aside
      className={panelClass}
      aria-label={ps === "complete" ? translate("lessons.messages.unitSummary") : translate("lessons.details.title")}
    >
      <div ref={viewportRef} className={panelBodyClass}>
        <div ref={contentRef} className={panelContentClass} style={{ transform: `scale(${scale})` }}>
          {celebBar}
          {body}
        </div>
      </div>
      <div className="lm-bottom flex flex-col gap-[10px] mt-auto shrink-0">{bottom}</div>
    </aside>
  );
}
