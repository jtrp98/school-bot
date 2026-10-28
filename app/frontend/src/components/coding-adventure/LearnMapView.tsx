import { t as translate } from "../../i18n";
import { LessonMap } from "./LessonMap";
import { LessonDetails } from "./LessonDetails";
import { LessonDialogs } from "./LessonDialogs";

import { Icon } from "../shared/Icons";
import { Button, IconBtn, Seg } from "../shared";
import type { useLearnMapScreen } from "../../modes/coding-adventure/LearnMap";

type Props = ReturnType<typeof useLearnMapScreen>;

const rootClass =
  "lm-root [--lm-route:var(--blue-line)] relative flex flex-1 min-h-0 min-w-0 flex-col " +
  "overflow-hidden bg-bg text-ink box-border";

const segWrapClass =
  "lm-segwrap [&_.sb-seg_button]:min-h-[48px] [&_.sb-seg_button]:py-0 [&_.sb-seg_button]:px-[18px] " +
  "[&_.sb-seg_button]:text-[17px] [&_.sb-seg_button]:inline-flex [&_.sb-seg_button]:items-center " +
  "[&_.sb-seg_button]:gap-[8px]";

const scoreClass =
  "lm-score flex items-center gap-[8px] min-h-[52px] py-0 px-[18px] bg-surface border-2 border-solid border-line " +
  "border-b-4 rounded-card not-italic [font-variant:normal] font-extrabold [font-stretch:normal] text-[22px] " +
  "leading-none font-heading";

const mapClass =
  "lm-map [flex:1_1_0] min-w-0 min-h-0 flex flex-col bg-[linear-gradient(180deg,_#eaf4fc_0%,_#d6e9f8_100%)] " +
  "border-2 border-solid border-blue-line rounded-[24px] overflow-hidden";

const mapBarClass =
  "lm-mbar flex items-center gap-[12px] py-[14px] px-[16px] bg-[rgba(var(--surface-rgb),_0.85)] border-b-2 " +
  "[border-bottom-style:solid] border-b-blue-line shrink-0";

const unitClass =
  "lm-unit not-italic [font-variant:normal] font-extrabold [font-stretch:normal] text-[21px] leading-[1.3] " +
  "font-heading flex items-center gap-[8px] text-ink [&_.icon]:text-blue-ink";

const progClass =
  "lm-prog not-italic [font-variant:normal] font-semibold [font-stretch:normal] text-[16px] leading-[1.4] " +
  "font-body text-muted";

const pipBaseClass =
  "lm-pip block w-[24px] h-[12px] rounded-full border-2 border-solid border-blue-line bg-white box-border " +
  "[&.done]:bg-ok [&.done]:border-ok [&.cur]:bg-blue-ink [&.cur]:border-blue-ink";

export function LearnMapView({
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
  ST
}: Props) {
  return (
    <div className={rootClass}>
      <main className={
        "lm-main flex-1 min-w-0 min-h-0 py-[24px] px-[32px] flex flex-col gap-[16px] " +
        "[@media(max-height:_800px)]:p-[16px] [@media(max-height:_800px)]:gap-[12px] " +
        "[@media(max-width:_1100px)]:px-[16px]"
      }>
        <header className={"lm-head flex shrink-0 items-center gap-[16px] flex-wrap"}>
          {onChooseMode && (
            <Button variant="secondary" icon="arrowL" onClick={onChooseMode}>
              {translate("lessons.modes.change")}
            </Button>
          )}
          <div className={"lm-spacer [flex:1] min-w-[40px]"} />
          <div className={segWrapClass}>
            <Seg
              label={translate("lessons.messages.levelView")}
              value={view}
              onChange={setView}
              options={[
                { value: "map", label: (<><Icon name="map" size={22} />{translate("lessons.views.map")}</>) },
                { value: "list", label: (<><Icon name="list" size={22} />{translate("lessons.views.list")}</>) },
              ]}
            />
          </div>
          <div
            className={scoreClass}
            role="status"
            aria-label={translate("lessons.messages.value0Of24StarsEarned", {value0: total})}
          >
            <Icon name="star" size={28} className={"sb-star-on text-star"} />
            <span>{total} / 24</span>
          </div>
        </header>
        <div className={"lm-cols [flex:1] min-h-0 flex gap-[20px]"}>
          <section className={mapClass} aria-label={translate("lessons.messages.levelMap")}>
            <div className={mapBarClass}>
              <IconBtn
                icon="chevL"
                label={translate("lessons.messages.previousUnit")}
                onClick={() => say(translate("lessons.units.previousUnavailable"))}
              />
              <div className={"sb-grow [flex:1] min-w-0"}>
                <div className={unitClass}>
                  <Icon name="flag" size={24} />
                  <span>{translate("lessons.messages.unit2MeetTheRobotArm")}</span>
                </div>
                <div className={progClass}>{translate(prog)}</div>
              </div>
              <div className={"lm-pips flex gap-[5px]"} role="img" aria-label={translate(prog)}>
                {ST.map((_, i) => (
                  <span
                    key={i}
                    className={`${pipBaseClass} ${i + 1 < cur
                      ? "done"
                      : i + 1 === cur
                        ? "cur"
                        : ""}`}
                  />
                ))}
              </div>
              <IconBtn
                icon="chevR"
                label={translate("lessons.messages.nextUnit")}
                onClick={() => say(translate(cur >= 8
                  ? "lessons.units.nextUnavailable"
                  : "common.messages.completeAll7LevelsToUnlockTheNextUnit"))}
              />
            </div>
            <LessonMap
              cur={cur}
              view={view}
              celeb={celeb}
              starsOf={starsOf}
              statusOf={statusOf}
              selNow={selNow}
              pick={pick}
              ST={ST}
            />
          </section>
          <LessonDetails
            cur={cur}
            setSel={setSel}
            celeb={celeb}
            setCeleb={setCeleb}
            conn={conn}
            setNoRobot={setNoRobot}
            say={say}
            starsOf={starsOf}
            selNow={selNow}
            s={s}
            ps={ps}
            stepNow={stepNow}
            doPrimary={doPrimary}
            speak={speak}
          />
        </div>
      </main>

      <LessonDialogs
        setRun={setRun}
        conn={conn}
        noRobot={noRobot}
        setNoRobot={setNoRobot}
        toast={toast}
        s={s}
        ps={ps}
        showRun={showRun}
        finishRun={finishRun}
        connectRobot={connectRobot}
      />
    </div>
  );

}
