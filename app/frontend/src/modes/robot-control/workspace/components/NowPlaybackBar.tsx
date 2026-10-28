import { Icon } from "../../../../components/shared/Icons";
import { t as translate } from "../../../../i18n";
import { Fragment, type ReactNode } from "react";
import { T, arr, sx } from "../tplUtil";
import type { WorkspaceViewProps } from "../viewTypes";

type Props = WorkspaceViewProps<
  "variantStyle" | "vPreflight" | "playbackBar" | "playbackActions" | "vControls" | "srcOpts"
  | "srcNoteStyle" | "srcNote" | "srcIsApp" | "scopeToggle" | "scopeOff" | "scopeOpen" | "scopeTip"
  | "scopeStyle" | "scopeLabel" | "scopeCaret" | "redoShow" | "redoPick" | "redoTip" | "redoLabel"
  | "scopeClearShow" | "scopeClear" | "scopeClearLabel" | "scopeMenuOpen" | "scopeIsAll"
  | "scopeItemAllStyle" | "scopeAllLabel" | "rangeOnFlag" | "scopeItemRangeStyle"
  | "summary" | "summaryHintStyle" | "summaryHintIc" | "summaryHint" | "vRunning" | "runTitle" | "runSub"
  | "runPct" | "vStopped" | "stoppedTitle"
>;

export function NowPlaybackBar({ v }: Props): ReactNode {
  const actions = v.playbackActions;
  return (
    <div {...sx(v.variantStyle)}>
      {v.vPreflight ? (
        <>
        <label className={"flex items-center gap-[10px] text-[15px] text-orange-deep cursor-pointer grow"} >
          <input type="checkbox" checked={v.playbackBar.preflight.checked} onChange={actions.togglePreflightCheck} className={"w-[22px] h-[22px] [accent-color:var(--blue-ink)]"}  />
          <span>
            <strong>{translate("common.messages.beforeTheRobotMoves")}</strong>
            {" "}{translate("robot.workspace.messages.checkedNoHandsOrObstaclesAroundTheRobot")}
          </span>
        </label>
        <button type="button" onClick={actions.cancelPreflight} className={"inline-flex items-center justify-center leading-[1.2] h-[46px] py-0 px-[14px] border-2 border-solid border-orange-line border-b-4 rounded-[12px] bg-white text-ink font-[family-name:inherit] text-[14px] font-semibold cursor-pointer"} >{translate("common.cancel")}</button>
        </>
      ) : null}
      {v.vControls ? (
        <>
        <span role="radiogroup" aria-label={translate("robot.workspace.messages.playbackSource")} className={"flex flex-col gap-[2px] shrink-0"} >
          <span className={"flex gap-[2px] p-[3px] rounded-[10px] bg-sunken"} >
            {arr(v.srcOpts).map((o, $i0) => (
              <Fragment key={$i0}>
                <button type="button" role="radio" aria-checked={o.on} onClick={o.pick} disabled={o.off} title={o.tip} {...sx(o.style)}>
                  <Icon name={o.ic} className={"inline-block size-[1.15em] align-[-.2em] mr-[.3em]"} />
                  {T(o.label)}
                </button>
              </Fragment>
            ))}
          </span>
          <span {...sx(v.srcNoteStyle)}>
            {T(v.srcNote)}
          </span>
        </span>
        {v.srcIsApp ? (
          <>
          <span className={"relative flex items-center gap-[2px] shrink-0"} >
            <button type="button" onClick={v.scopeToggle} disabled={v.scopeOff} aria-haspopup="menu" aria-expanded={v.scopeOpen} title={v.scopeTip} {...sx(v.scopeStyle)}>
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.4" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
                <path d="M7 4.5v15l12-7.5z" />
              </svg>
              {T(v.scopeLabel)}
              {v.scopeCaret ? (
                <>
                <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="3" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
                  <path d="m6 9 6 6 6-6" />
                </svg>
                </>
              ) : null}
            </button>
            {v.redoShow ? (
              <>
              <button type="button" onClick={v.redoPick} title={v.redoTip} className={"h-[34px] py-0 px-[12px] shrink-0 ml-[4px] border-2 border-solid border-blue-line border-b-4 rounded-[10px] bg-white text-blue-ink font-[family-name:inherit] text-[13px] font-bold whitespace-nowrap cursor-pointer flex items-center gap-[4px]"} >
                {"↺ "}
                {T(v.redoLabel)}
              </button>
              </>
            ) : null}
            {v.scopeClearShow ? (
              <>
              <button type="button" onClick={v.scopeClear} aria-label={v.scopeClearLabel} title={v.scopeClearLabel} className={"w-[30px] h-[30px] shrink-0 p-0 border-2 border-solid border-line-strong rounded-full bg-white text-muted font-[family-name:inherit] text-[14px] font-bold cursor-pointer leading-none"} >
                ✕
              </button>
              </>
            ) : null}
            {v.scopeMenuOpen ? (
              <>
              <div role="menu" aria-label={translate("robot.workspace.messages.choosePosesToPlay")} className={"absolute bottom-[calc(100%_+_8px)] left-0 w-[230px] box-border p-[6px] flex flex-col gap-[4px] bg-white border-2 border-solid border-line-strong rounded-[12px] shadow-pop z-[30]"} >
                <button type="button" role="menuitemradio" aria-checked={v.scopeIsAll} onClick={actions.scopeAll} {...sx(v.scopeItemAllStyle)}>
                  {T(v.scopeAllLabel)}
                </button>
                <button type="button" role="menuitemradio" aria-checked={v.rangeOnFlag} onClick={actions.scopePick} {...sx(v.scopeItemRangeStyle)}>
                  {translate("robot.workspace.messages.chooseRange")}{" "}
                  <span className={"font-normal text-muted"} >{translate("robot.workspace.messages.tapTheStartPoseThenTheEndPose")}</span>
                </button>
              </div>
              </>
            ) : null}
          </span>
          </>
        ) : null}
        <span className={"text-[17px] font-bold"} >
          {T(v.summary)}
        </span>
        <span {...sx(v.summaryHintStyle)}>
          <Icon name={v.summaryHintIc} className={"inline-block size-[1.15em] align-[-.2em] mr-[.3em]"} />
          {T(v.summaryHint)}
        </span>
        </>
      ) : null}
      {v.vRunning ? (
        <>
        <div className={"flex flex-col gap-[6px] grow"} >
          <div className={"flex items-center gap-[10px]"} >
            <span className={"text-[17px] font-bold text-blue-press"} >
              {T(v.runTitle)}
            </span>
            <span className={"text-[14px]"} >
              {T(v.runSub)}
            </span>
          </div>
          <div className={"h-[10px] rounded-full bg-blue-tint overflow-hidden"} >
            <div {...sx(`height: 100%; width: ${v.runPct ?? ""}%; border-radius: 999px; background: var(--blue-ink); transition: width 0.1s linear`)} />
          </div>
        </div>
        </>
      ) : null}
      {v.vStopped ? (
        <>
        <svg width="28" height="28" viewBox="0 0 24 24" aria-hidden="true">
          <rect x="4" y="4" width="16" height="16" rx="3" fill="var(--muted)" />
        </svg>
        <span className={"text-[17px] font-bold grow"} >
          {T(v.stoppedTitle)}
        </span>
        </>
      ) : null}
    </div>
  );
}
