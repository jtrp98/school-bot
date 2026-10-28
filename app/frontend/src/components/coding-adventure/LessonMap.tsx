import { t as translate } from "../../i18n";
import type { CSSProperties } from "react";
import { Icon } from "../shared/Icons";
import { Stars } from "../shared";
import type { useLearnMapScreen } from "../../modes/coding-adventure/LearnMap";
import { useLessonContentFit } from "../../modes/coding-adventure/useLessonContentFit";
import { SchoolBotMascot } from "../shared/SchoolBotMascot";
import { KindTag } from "./LessonKindTag";
import { shortOf } from "./lessonPresentation";

const B: [number, number][] = [
  [92, 466], [205, 384], [328, 480], [435, 312], [558, 420], [640, 240], [736, 84],
];

const MOS: [number, number][] = [
  [-56, -210], [-56, -210], [-56, -210], [-170, -100], [85, -40], [-175, -100], [-205, -45],
];

const Y0 = Math.min(...B.map((b) => b[1]));

const Y1 = Math.max(...B.map((b) => b[1]));

const PAD_T = 64;

const PAD_B = 112;

const tileBaseClass =
  "lm-tile relative flex items-center justify-center box-border [transition:transform_0.08s_ease,_filter_0.12s_ease] " +
  "motion-reduce:[transition:none]";

const doneTileClass =
  "lm-t-done text-white bg-ok border-b-[6px] [border-bottom-style:solid] border-b-ok-ink rounded-[50%]";

const currentTileClass =
  "lm-t-cur text-white bg-blue border-b-8 [border-bottom-style:solid] border-b-blue-press rounded-[28px] " +
  "shadow-[0_0_0_6px_rgba(var(--blue-rgb),_0.32)] " +
  "[&.lm-t-sel]:shadow-[0_0_0_4px_var(--surface),_0_0_0_8px_var(--blue-ink)]";

const nextTileClass =
  "lm-t-lock rounded-[50%] lm-t-next bg-white border-[3px] border-dashed border-blue text-blue-ink";

const lockedTileClass =
  "lm-t-lock bg-sunken border-[3px] border-dashed border-line-strong rounded-[50%] text-muted";

const selTileClass = "lm-t-sel shadow-[0_0_0_4px_var(--surface),_0_0_0_8px_var(--blue)]";

const nodeButtonClass =
  "lm-nd absolute w-[160px] ml-[-80px] flex flex-col items-center gap-[12px] p-0 " +
  "border-[0] border-none border-current " +
  "bg-transparent bg-none font-[family-name:inherit] text-[inherit] cursor-pointer z-[2] focus-visible:outline-none " +
  "[&:focus-visible_.lm-tile]:shadow-[var(--focus)] [&:hover_.lm-tile]:[filter:brightness(1.06)] " +
  "[&:active_.lm-tile]:[transform:translateY(2px)]";

const platClass =
  "lm-plat absolute left-[50%] bottom-[-18px] rounded-[50%] bg-[rgba(var(--surface-rgb),_0.8)] " +
  "shadow-pop";

const numClass =
  "lm-num absolute left-[-8px] top-[-8px] w-[30px] h-[30px] rounded-[50%] bg-ink border-2 border-solid border-white " +
  "text-white not-italic [font-variant:normal] font-extrabold [font-stretch:normal] text-[15px] leading-none " +
  "font-heading flex items-center justify-center";

const checkClass =
  "lm-chk absolute right-[-8px] top-[-8px] w-[30px] h-[30px] rounded-[50%] bg-white border-[3px] border-solid " +
  "border-ok text-ok flex items-center justify-center box-border";

const pillBaseClass =
  "lm-pill flex flex-col items-center gap-[3px] max-w-[152px] pt-[5px] px-[12px] pb-[6px] rounded-[14px] text-center " +
  "bg-white border-2 border-solid border-ok-line shadow-pop not-italic [font-variant:normal] font-bold " +
  "[font-stretch:normal] text-[17px] leading-[1.3] font-body [&.cur]:border-blue " +
  "[&.lock]:bg-[rgba(var(--surface-rgb),_0.92)] " +
  "[&.lock]:border-line-strong [&.lock]:text-muted";

const nowClass =
  "lm-now bg-blue-ink text-white not-italic [font-variant:normal] font-extrabold [font-stretch:normal] " +
  "text-[15px] leading-[1.4] font-body py-[1px] px-[12px] rounded-full";

const nextLabelClass =
  "lm-nextl not-italic [font-variant:normal] font-bold [font-stretch:normal] " +
  "text-[14px] leading-[1.3] font-body text-blue-ink";

const mascotClass = "lm-mascot absolute z-[3] pointer-events-none w-[112px] h-[160px]";

const bubbleBaseClass =
  "lm-bubble [text-wrap:balance] absolute w-[170px] py-[8px] px-[12px] bg-white border-2 border-solid " +
  "border-blue-line " +
  "rounded-[16px] not-italic [font-variant:normal] font-extrabold [font-stretch:normal] text-[17px] leading-[1.35] " +
  "font-body text-center text-ink shadow-pop box-border after:[content:''] after:absolute after:left-[50%] " +
  "after:bottom-[-9px] after:w-[14px] after:h-[14px] after:ml-[-7px] after:bg-white after:border-r-2 " +
  "after:[border-right-style:solid] after:border-r-blue-line after:border-b-2 after:[border-bottom-style:solid] " +
  "after:border-b-blue-line after:[transform:rotate(45deg)] [&.side::after]:left-auto [&.side::after]:right-[-9px] " +
  "[&.side::after]:bottom-auto [&.side::after]:top-[50%] [&.side::after]:mt-[-7px] [&.side::after]:mx-0 " +
  "[&.side::after]:mb-0 [&.side::after]:[transform:rotate(-45deg)]";

const listClass =
  "lm-list w-full shrink-0 pt-[18px] px-[20px] pb-[24px] flex flex-col gap-[10px]";

const rowBaseClass =
  "lm-row flex items-center gap-[14px] w-full min-h-[72px] py-[10px] px-[16px] border-2 border-solid border-line " +
  "border-b-4 rounded-card bg-white text-ink font-[family-name:inherit] text-left cursor-pointer box-border shrink-0 " +
  "hover:bg-blue-tint hover:border-blue focus-visible:outline-none focus-visible:shadow-[var(--focus)] " +
  "[&.sel]:bg-blue-tint [&.sel]:border-blue [&.sel]:shadow-[0_0_0_2px_var(--blue)] [&_.lm-tag]:min-h-[28px] " +
  "[&_.lm-tag]:text-[14px] [&_.lm-tag]:[align-self:flex-start] [&_.lm-tag]:py-0 [&_.lm-tag]:px-[10px]";

const rtBaseClass =
  "lm-rt w-[52px] h-[52px] shrink-0 flex items-center justify-center box-border text-white [&.done]:bg-ok " +
  "[&.done]:rounded-[50%] [&.cur]:bg-blue [&.cur]:rounded-[16px] [&.lock]:bg-sunken [&.lock]:text-muted " +
  "[&.lock]:border-[3px] [&.lock]:border-dashed [&.lock]:border-line-strong [&.lock]:rounded-[50%]";

const rowTitleClass =
  "lm-row-t not-italic [font-variant:normal] font-bold [font-stretch:normal] text-[18px] leading-[1.3] font-body";

const rowNowClass =
  "lm-now bg-blue-ink text-white not-italic [font-variant:normal] font-extrabold [font-stretch:normal] " +
  "leading-[1.4] font-body rounded-full py-[3px] px-[14px] text-[16px]";

type Props = Pick<ReturnType<typeof useLearnMapScreen>,
  "cur"
  | "view"
  | "celeb"
  | "starsOf"
  | "statusOf"
  | "selNow"
  | "pick"
  | "ST"
>;

export function LessonMap({ cur, view, celeb, starsOf, statusOf, selNow, pick, ST }: Props) {
  const { viewportRef, contentRef, scale } = useLessonContentFit(view);

  // x is a percentage of the stage width; y is spread over the stage height (minus room for the
  // first-row pills and the top badges) so the whole route stays visible in short windows.
  const pctX = (x: number) => `${((x / 820) * 100).toFixed(3)}%`;
  const posY = (y: number) =>
    `calc(${PAD_T}px + (100% - ${PAD_T + PAD_B}px) * ${((y - Y0) / (Y1 - Y0)).toFixed(4)})`;
  const route = (
    <svg
      className={"lm-route absolute left-0 top-[64px] w-full h-[calc(100%_-_176px)] [overflow:visible]"}
      viewBox={`0 ${Y0} 820 ${Y1 - Y0}`}
      preserveAspectRatio="none"
      aria-hidden="true"
    >
      {Array.from({ length: 6 }, (_, i) => {
        const [x1, y1] = B[i];
        const [x2, y2] = B[i + 1];
        const xm = (x1 + x2) / 2;
        const d = `M${x1},${y1} C${xm},${y1} ${xm},${y2} ${x2},${y2}`;
        const done = i + 1 < cur;
        return (
          <g key={i}>
            <path
              d={d}
              fill="none"
              stroke="var(--surface)"
              strokeWidth={26}
              strokeLinecap="round"
              opacity={0.9}
              vectorEffect="non-scaling-stroke"
            />
            {done ? (
              <path
                d={d}
                fill="none"
                className={"[stroke:var(--ok)]"}
                strokeWidth={12}
                strokeLinecap="round"
                vectorEffect="non-scaling-stroke"
              />
            ) : (
              <path
                d={d}
                fill="none"
                className={"[stroke:var(--lm-route)]"}
                strokeWidth={8}
                strokeLinecap="round"
                strokeDasharray="0.1 16"
                vectorEffect="non-scaling-stroke"
              />
            )}
          </g>
        );
      })}
    </svg>
  );

  const node = (i: number) => {
    const st = ST[i];
    const n = i + 1;
    const status = statusOf(n);
    const isNext = status === "locked" && n === cur + 1;
    const size = status === "current"
      ? 100
      : status === "done"
        ? 76
        : 72;
    const tileCls = `${tileBaseClass} ${status === "done"
      ? doneTileClass
      : status === "current"
        ? currentTileClass
        : isNext
          ? nextTileClass
          : lockedTileClass} ${selNow === n ? selTileClass : ""}`;
    const aria = translate("lessons.messages.levelValue0Value1Value2ViewMission", {
      value0: n,
      value1: shortOf(st),
      value2: status === "done"
        ? translate("lessons.messages.completedValue0StarsEarned", { value0: starsOf(n) })
        : status === "current"
          ? translate("lessons.messages.currentLevel")
          : translate("lessons.messages.stillLocked"),
    });
    const style: CSSProperties = { left: pctX(B[i][0]), top: posY(B[i][1]), marginTop: -size / 2 };
    return (
      <button
        key={n}
        type="button"
        className={nodeButtonClass}
        style={style}
        aria-label={aria}
        aria-pressed={selNow === n}
        onClick={() => pick(n)}
      >
        <span className={"lm-ndw relative block"} style={{ width: size, height: size }}>
          <span
            className={platClass}
            aria-hidden="true"
            style={{
              width: Math.round(size * 1.7),
              height: Math.round(size * 0.52),
              marginLeft: -Math.round(size * 0.85),
            }}
          />
          <span className={tileCls} style={{ width: size, height: size }}>
            {status === "locked"
              ? <Icon name="lock" size={30} />
              : <Icon name={st.ic} size={status === "current" ? 46 : 38} />}
            <span className={numClass} aria-hidden="true">{n}</span>
            {status === "done" && (
              <span className={checkClass} aria-hidden="true">
                <Icon name="check" size={16} />
              </span>
            )}
          </span>
        </span>
        <span className={`${pillBaseClass} ${status === "current" ? "cur" : ""} ${status === "locked" ? "lock" : ""}`}>
          <span>{shortOf(st)}</span>
          {status === "done" && <Stars n={starsOf(n)} px={18} />}
          {status === "current" && <span className={nowClass}>{translate("lessons.messages.current")}</span>}
          {isNext && <span className={nextLabelClass}>{translate("lessons.messages.next")}</span>}
        </span>
      </button>
    );
  };

  const mascot = () => {
    const mi = Math.min(cur, 7) - 1;
    const mp = MOS[mi];
    const side = cur >= 7;
    const text = cur >= 8
      ? translate("lessons.messages.unitCompleteGreatJob")
      : celeb
        ? translate("lessons.messages.greatJobLetSKeepGoing")
        : cur === 1
          ? translate("lessons.messages.startWithLevel1")
          : translate("lessons.messages.letSContinueLevelValue0", { value0: cur });
    const pos: CSSProperties = {
      left: `calc(${pctX(B[mi][0])} + ${mp[0]}px)`,
      top: `calc(${posY(B[mi][1])} + ${mp[1]}px)`,
    };
    const bub: CSSProperties = side ? { right: 118, top: 4 } : { left: -29, bottom: 176 };
    return (
      <div className={mascotClass} aria-hidden="true" style={pos}>
        <span className={`${bubbleBaseClass} ${side ? "side" : ""}`} style={bub}>{text}</span>
        <SchoolBotMascot w={112} h={160} cheer={celeb || cur >= 8} />
      </div>
    );
  };

  const list = (
    <div ref={contentRef} className={listClass} style={{ transform: `scale(${scale})` }}>
      {ST.map((st, i) => {
        const n = i + 1;
        const status = statusOf(n);
        return (
          <button
            key={n}
            type="button"
            className={`${rowBaseClass} ${selNow === n ? "sel" : ""}`}
            aria-pressed={selNow === n}
            aria-label={translate("lessons.messages.levelValue0Value1Value2", {
              value0: n,
              value1: shortOf(st),
              value2: translate(status === "done"
                ? "lessons.messages.completed"
                : status === "current"
                  ? "lessons.messages.current"
                  : "common.messages.locked"),
            })}
            onClick={() => pick(n)}
          >
            <span className={`${rtBaseClass} ${status === "done"
              ? "done"
              : status === "current"
                ? "cur"
                : "lock"}`}>
              <Icon name={status === "locked" ? "lock" : st.ic} size={status === "locked" ? 24 : 26} />
            </span>
            <span className={"sb-grow [flex:1] min-w-0 sb-col flex flex-col gap-[3px]"} >
              <span className={rowTitleClass}>{n}. {shortOf(st)}</span>
              <KindTag s={st} />
            </span>
            <span className={"sb-row flex items-center gap-[8px]"} >
              {status === "done" ? (
                <Stars n={starsOf(n)} px={20} />
              ) : status === "current" ? (
                <span className={rowNowClass} >{translate("lessons.messages.current")}</span>
              ) : (
                <span className={"sb-strong font-bold sb-muted text-muted text-[16px]"} >
                  {translate("common.messages.locked")}
                </span>
              )}
            </span>
          </button>
        );
      })}
    </div>
  );

  return (
    <div
      ref={viewportRef}
      className="lm-mapbox relative flex flex-1 min-h-0 min-w-0 items-center justify-center overflow-hidden"
    >
      {view === "map" ? (
        <div
          ref={contentRef}
          className="lm-stage relative w-full h-full min-h-[440px] min-w-[640px] shrink-0"
          style={{ transform: `scale(${scale})` }}
        >
          {route}
          {ST.map((_, i) => node(i))}
          {mascot()}
        </div>
      ) : list}
    </div>
  );
}
