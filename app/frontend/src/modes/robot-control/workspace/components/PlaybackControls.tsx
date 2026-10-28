import { Button } from "../../../../components/shared/Button";
import { t as translate } from "../../../../i18n";
import type { ReactNode } from "react";
import { T } from "../tplUtil";
import type { WorkspaceViewProps } from "../viewTypes";
import { NowPlaybackBar } from "./NowPlaybackBar";
import { PlaybackOptions } from "./PlaybackOptions";

type Props = WorkspaceViewProps;

const ATTENTION_RING_CLASS = "aw-ring-or [--rc:var(--orange-rgb)] animate-[awAttn_2.4s_ease-in-out_infinite] "
  + "motion-reduce:animate-none motion-reduce:shadow-[0_0_0_4px_rgba(var(--rc),.5)]";

// Alternating two keyframe names restarts the shake on every blocked tap without remounting the dialog.
function shakeClass(shakes: number): string {
  if (!shakes) return "";
  return shakes % 2
    ? "aw-shake animate-[awShake_.45s_ease-in-out_1] motion-reduce:animate-none"
    : "aw-shake2 animate-[awShake2_.45s_ease-in-out_1] motion-reduce:animate-none";
}

export function PlaybackControls({ v }: Props): ReactNode {
  const { preflight } = v.playbackBar;
  const actions = v.playbackActions;
  return (
    <div role="region" aria-label={translate("common.messages.playAndStop")} data-bar6="1" className={"relative shrink-0 box-border flex flex-wrap [align-items:stretch] gap-[10px] py-[10px] px-[12px] bg-white border-2 border-solid border-line rounded-[18px]"} >
      {v.playbackBar.showTopProgress ? (
        <div role="progressbar" aria-label={translate("common.messages.playbackProgress")} aria-valuemin={0}
          aria-valuemax={100} aria-valuenow={v.runPct}
          className="absolute left-0 right-0 top-[-2px] h-[4px] bg-line-strong z-[2]">
          <div className="h-full bg-blue-ink [transition:width_0.1s_linear]" style={{ width: `${v.runPct}%` }} />
        </div>
      ) : null}
      {v.barNow ? (
        <>
        <NowPlaybackBar v={v} />
        </>
      ) : null}
      {v.barNew ? (
        <>
        <PlaybackOptions v={v} />
        {preflight.open ? (
          <>
          <div onClick={actions.cancelPreflight} aria-hidden="true" className={"fixed inset-0 z-[29] bg-transparent"}  />
          <div role="alertdialog" aria-label={translate("common.messages.beforeTheRobotMoves")} className={`${shakeClass(preflight.shakes)} [cursor:default] absolute right-[236px] bottom-[68px] w-[330px] box-border py-[12px] px-[14px] flex flex-col gap-[8px] bg-white border-2 border-solid border-line-strong rounded-[14px] shadow-pop z-[30] text-left [white-space:normal]`} onClick={v.f6StripClick} >
            <span className={"text-[15px] font-bold text-ink"} >{translate("common.messages.beforeTheRobotMoves")}</span>
            <span className={"text-[13px] leading-[1.5] text-muted"} >{translate("robot.workspace.messages.checkTheAreaAroundTheRobotBeforePlayback")}</span>
            <label
              onClick={v.stop}
              className={`flex items-center gap-[10px] py-[8px] px-[10px] rounded-[10px] text-[14px] font-semibold
                cursor-pointer text-ink border-2 border-solid
                ${preflight.checked ? "bg-blue-tint border-blue-line" : "bg-surface border-line-strong"}`}
            >
              <input type="checkbox" checked={preflight.checked} onChange={actions.togglePreflightCheck}
                className={"w-[20px] h-[20px] shrink-0 [accent-color:var(--blue-ink)] cursor-pointer"} />
              {translate("common.messages.areaAroundRobotIsClearOfHandsAndObjects")}
            </label>
            <span className={"text-[12px] leading-[1.5] text-muted"} >
              {T(preflight.hint)}
            </span>
          </div>
          </>
        ) : null}
        </>
      ) : null}
      <div data-b6div="1" className={"w-[2px] bg-line rounded-[2px] my-0 mx-[6px]"}  />
      <div data-b6stop="1" className={"w-[168px] [flex:0_0_auto] min-w-0 flex flex-col [align-items:stretch] justify-center gap-[2px]"} >
        {v.isDisc ? (
          <>
          <span className={"text-[11px] text-err-ink"} >{translate("robot.workspace.messages.unavailableWhileDisconnected")}</span>
          </>
        ) : null}
        <Button type="button" data-pop="stop" onClick={v.stopAll} disabled={v.isDisc} variant="danger" className={"relative z-[31] h-[56px]! min-h-0! px-[26px]! rounded-[16px]! text-[20px]! font-[family-name:inherit]!"}>
          <svg width="22" height="22" viewBox="0 0 24 24" aria-hidden="true">
            <rect x="4" y="4" width="16" height="16" rx="3" fill="currentColor" />
          </svg>
          <span>{translate("common.stop")}</span>
        </Button>
      </div>
      <div data-b6playw="1" className={"[flex:1.4_1_0] min-w-0 flex w-[223px]"} >
        <Button type="button" data-b6play="1"  onClick={v.playSlot} onPointerDown={v.playDown} onPointerUp={v.playUp} disabled={v.playSlotOff} title={v.playTip} variant={v.slotIsPause ? "secondary" : "blue"} className={`relative z-[31] w-[120px] h-[52px]! min-h-0! px-[8px]! rounded-[16px]! self-center font-[family-name:inherit]! ${preflight.open ? ATTENTION_RING_CLASS : ""} ${String(v.playSlotLabel).length > 8 ? "text-[14px]!" : "text-[18px]!"}`}>
          {v.slotIsPause ? (
            <>
            <svg width="18" height="18" viewBox="0 0 24 24" aria-hidden="true">
              <rect x="6" y="5" width="4" height="14" rx="1" fill="currentColor" />
              <rect x="14" y="5" width="4" height="14" rx="1" fill="currentColor" />
            </svg>
            </>
          ) : null}
          {v.slotIsPlay ? (
            <>
            <svg width="18" height="18" viewBox="0 0 24 24" aria-hidden="true">
              <path d="M7 4.5v15l12-7.5z" fill="currentColor" />
            </svg>
            </>
          ) : null}
          <span>
            {T(v.playSlotLabel)}
          </span>
        </Button>
      </div>
    </div>
  );
}
