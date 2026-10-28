import { Button } from "../../../../components/shared/Button";
import { t as translate } from "../../../../i18n";
import { Fragment, type ReactNode } from "react";
import { IMG, T, arr, sx } from "../tplUtil";
import type { WorkspaceViewProps } from "../viewTypes";

type Props = WorkspaceViewProps<
  "robotName" | "robotPort" | "nativeHardware" | "nativeConnected" | "showBaud" | "hideBaud"
  | "toggleBaud" | "baudOpen" | "nativeProfile" | "yes6" | "navTo" | "switch6Tip" | "switch6Style"
  | "f6StripClick" | "f6StripStyle" | "f6Prev" | "f6PrevOff" | "f6BtnStyle" | "f6Name" | "f6No"
  | "f6SyncStyle" | "f6Sync" | "f6Next" | "f6NextOff" | "hdrGoShow" | "hdrGoCls" | "goSel" | "goSelOff"
  | "hdrGoTip" | "hdrGoStyle" | "hdrGoLabel" | "hasFar" | "farCancel" | "farHead" | "farGo" | "hdrP0Show"
  | "usePose0" | "lockOff" | "goHome" | "homeOff" | "toggleBoxMenu" | "boxMenuOpen"
  | "goBox" | "armDots" | "showArmTag" | "armTag" | "jointActions"
>;

const DOT_BASE = "absolute p-0 box-border rounded-full font-[family-name:inherit] font-bold leading-none flex "
  + "items-center justify-center cursor-pointer [transition:width_0.15s,height_0.15s,margin_0.15s]";
const DOT_SELECTED = "w-[36px] h-[36px] mt-[-18px] ml-[-18px] text-[13px] z-[4] border-[3px] border-solid "
  + "border-surface bg-blue-ink text-white shadow-[0_0_0_2px_var(--blue-ink),0_3px_8px_rgba(var(--ink-rgb),0.35)]";
const DOT_IDLE = "w-[24px] h-[24px] mt-[-12px] ml-[-12px] text-[11px] z-[2] border-2 border-solid border-blue-line "
  + "bg-surface text-blue-press shadow-[0_1px_4px_rgba(var(--ink-rgb),0.35)]";

export function ArmStage({ v }: Props): ReactNode {
  return (
    <div data-arm6="1" className={"[flex:1_1_0] min-h-0 flex flex-col gap-[8px]"} >
      <div className={"flex items-center gap-[8px] h-[52px] shrink-0 box-border pt-0 pr-[8px] pb-0 pl-[12px] border-2 border-solid border-line rounded-[16px] bg-white"} >
        <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="var(--blue-ink)" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true" className={"shrink-0"} >
          <rect x="5" y="8" width="14" height="11" rx="3" />
          <path d="M12 8V4" />
          <circle cx="12" cy="3" r="1" />
          <circle cx="9.5" cy="13" r="1.2" />
          <circle cx="14.5" cy="13" r="1.2" />
        </svg>
        <span className={"text-[15px] font-bold whitespace-nowrap"} >
          {T(v.robotName)}
          {" "}
          <span className={"font-medium text-muted"} >
            · {T(v.robotPort || "COM7")}
          </span>
        </span>
        <span className={"inline-flex items-center gap-[5px] text-[13px] font-semibold text-ok-ink whitespace-nowrap"} >
          <span className={"w-[9px] h-[9px] rounded-full bg-ok"}  />
          {v.nativeHardware ? (v.nativeConnected ? translate("robot.workspace.stage.lastDetected") : translate("common.messages.notConnected")) : translate("robot.workspace.messages.connected")}
        </span>
        <span className={"[flex:1_1_0] min-w-[8px]"}  />
        <span className={"relative inline-flex"} >
          <button type="button" aria-label={translate("robot.workspace.messages.connectionDetails")} aria-describedby="baud-tip-v4b" onMouseEnter={v.showBaud} onMouseLeave={v.hideBaud} onFocus={v.showBaud} onBlur={v.hideBaud} onClick={v.toggleBaud} className={"w-[36px] h-[36px] p-0 border-[medium] border-none border-current rounded-[8px] bg-transparent flex items-center justify-center [cursor:help]"} >
            <span className={"w-[20px] h-[20px] box-border rounded-full border-2 border-solid border-muted text-muted font-[family-name:Georgia,_serif] text-[12px] font-bold flex items-center justify-center"} >
              i
            </span>
          </button>
          {v.baudOpen ? (
            <>
            <span id="baud-tip-v4b" role="tooltip" className={"absolute top-[40px] right-[-48px] whitespace-nowrap bg-ink text-white text-[13px] font-semibold leading-[1.5] py-[6px] px-[10px] rounded-[8px] z-[40]"} >
              {v.nativeHardware ? translate("robot.workspace.messages.profileValue0DisplayedValuesAreNotMeasuredAngles", {value0: v.nativeProfile}) : translate("robot.workspace.messages.usbCable115200Baud")}
            </span>
            </>
          ) : null}
        </span>
        {v.yes6 ? (
          <>
          <a href="#" onClick={(e: any) => { e.preventDefault(); v.navTo("connect"); }} title={v.switch6Tip} {...sx(v.switch6Style)}>{translate("robot.workspace.messages.changeRobot")}</a>
          </>
        ) : null}
      </div>
      <div data-stagecard6="1" className={"relative [flex:1_1_0] min-h-0 box-border rounded-[22px] bg-[radial-gradient(120%_90%_at_50%_30%,_#F2F8FF_0%,_#DCEBFB_70%,_#CFE2F8_100%)] flex flex-col items-center justify-center pt-[8px] px-[8px] pb-[78px] overflow-hidden"} >
        <div onClick={v.f6StripClick} {...sx(v.f6StripStyle)}>
          <button type="button" onClick={v.f6Prev} disabled={v.f6PrevOff} aria-label={translate("common.messages.previousPose")} {...sx(v.f6BtnStyle)}>
            ‹
          </button>
          <span className={"flex flex-col items-center min-w-[200px] leading-[1.2] py-0 px-[4px]"} >
            <b className={"text-[20px] whitespace-nowrap max-w-[280px] overflow-hidden text-ellipsis"} >
              {T(v.f6Name)}
            </b>
            <span className={"text-[12px] font-semibold text-muted whitespace-nowrap"} >
              {T(v.f6No)}
              {" · "}
              <span {...sx(v.f6SyncStyle)}>
                {T(v.f6Sync)}
              </span>
            </span>
          </span>
          <button type="button" onClick={v.f6Next} disabled={v.f6NextOff} aria-label={translate("common.messages.nextPose")} {...sx(v.f6BtnStyle)}>
            ›
          </button>
        </div>
        <div className={"absolute top-[10px] right-[10px] z-[40] flex flex-col items-end gap-[8px]"} >
          <span className={"relative flex flex-col items-end gap-[8px]"} >
            {v.hdrGoShow ? (
              <>
              <button type="button" className={v.hdrGoCls} onClick={v.goSel} disabled={v.goSelOff} title={v.hdrGoTip} {...sx(v.hdrGoStyle)}>
                {T(v.hdrGoLabel)}
              </button>
              </>
            ) : null}
            {v.hasFar ? (
              <>
              <div onClick={v.farCancel} aria-hidden="true" className={"fixed inset-0 z-[29] bg-transparent"}  />
              <div role="alertdialog" aria-label={translate("robot.workspace.messages.confirmALargeRobotMovement")} className={"absolute right-0 top-full mt-[8px] w-[330px] box-border py-[12px] px-[14px] flex flex-col gap-[8px] bg-white border-2 border-solid border-orange rounded-[14px] shadow-pop z-[30] text-left [white-space:normal]"} >
                <span className={"text-[15px] font-bold text-orange-deep"} >
                  {T(v.farHead)}
                </span>
                <span className={"text-[13px] leading-[1.5] text-muted"} >{translate("robot.workspace.messages.checkAroundTheRobotNoHandsOrObstacles")}</span>
                <button type="button" onClick={v.farGo} className={"inline-flex items-center justify-center leading-[1.2] h-[40px] py-0 px-[14px] border-[medium] border-none border-current border-b-4 [border-bottom-style:solid] border-b-orange-press rounded-[10px] bg-orange text-white font-[family-name:inherit] text-[14px] font-bold cursor-pointer"} >{translate("robot.workspace.messages.moveNow")}</button>
                <span className={"text-[12px] leading-[1.5] text-muted"} >{translate("robot.workspace.messages.tapElsewhereToCancel")}</span>
              </div>
              </>
            ) : null}
            {v.hdrP0Show ? (
              <>
              <button type="button" onClick={v.usePose0} disabled={v.lockOff} title={translate("robot.workspace.messages.copyPose0ToTheEndOfTheSequence")} {...sx(v.hdrGoStyle)}>{translate("robot.workspace.messages.usePose0AsANewPose")}</button>
              </>
            ) : null}
            <span className={"flex items-center gap-[2px] order-[-1]"} >
              <Button type="button" onClick={v.goHome} disabled={v.homeOff} title={translate("common.messages.restPoseTheRobotSPowerOnPose")} variant="primary" className="min-h-0! h-[30px]! px-[10px]! rounded-[10px_4px_4px_10px]! text-[13px]! font-[family-name:inherit]!">
                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.4" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
                  <path d="M4 11 12 4l8 7" />
                  <path d="M6 10v9h12v-9" />
                </svg>{translate("common.messages.returnToRestPose")}</Button>
              <Button type="button" onClick={v.toggleBoxMenu} disabled={v.homeOff} aria-label={translate("robot.workspace.messages.otherPoses")} aria-haspopup="menu" aria-expanded={v.boxMenuOpen} title={translate("robot.workspace.messages.otherPoses")} variant="primary" aria-pressed={v.boxMenuOpen} className="min-h-0! h-[30px]! w-[28px] p-0! rounded-[4px_10px_10px_4px]! text-[13px]! font-[family-name:inherit]!">
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="3" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
                  <path d="m6 9 6 6 6-6" />
                </svg>
              </Button>
            </span>
          </span>
          {v.boxMenuOpen ? (
            <>
            <div role="menu" aria-label={translate("robot.workspace.messages.otherPoses")} className={"absolute top-[40px] right-0 w-[248px] box-border p-[10px] flex flex-col gap-[8px] bg-white border-2 border-solid border-line-strong rounded-[12px] shadow-pop z-[20]"} >
              <span className={"text-[13px] font-bold text-ink"} >{translate("robot.workspace.messages.foldArmIntoBox")}</span>
              <span className={"text-[12px] text-muted leading-[1.4]"} >{translate("robot.workspace.messages.theRobotWillFoldDownForStorageClearThe")}</span>
              <div className={"grid [grid-template-columns:repeat(2,_minmax(0,_1fr))] gap-[6px]"} >
                <button type="button" onClick={v.toggleBoxMenu} className={"h-[34px] border-2 border-solid border-line rounded-[9px] bg-white text-muted font-[family-name:inherit] text-[13px] font-bold cursor-pointer"} >{translate("common.cancel")}</button>
                <button type="button" role="menuitem" onClick={v.goBox} className={"inline-flex items-center justify-center gap-[4px] h-[34px] border-2 border-solid border-blue-ink rounded-[9px] bg-white text-blue-ink font-[family-name:inherit] text-[13px] font-bold cursor-pointer"} >
                  <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.4" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
                    <path d="M3 9h18v11H3z" />
                    <path d="M12 3v9M8 8l4 4 4-4" />
                  </svg>{translate("robot.workspace.messages.foldIntoBox")}</button>
              </div>
            </div>
            </>
          ) : null}
        </div>
        <div role="group" aria-label={translate("robot.workspace.messages.robotArmWithServosS0ToS5TapA")} className={"relative w-full [aspect-ratio:990_/_912]"} >
          <div aria-hidden="true" className={"absolute left-[6%] top-[74%] w-[92%] h-[22%] [transform:rotate(-4deg)] rounded-[50%] bg-[radial-gradient(closest-side,_rgba(var(--blue-press-rgb),_0.22),_rgba(var(--blue-press-rgb),_0.08)_60%,_transparent)] pointer-events-none"}  />
          <img src={IMG.armPhoto} alt={translate("robot.workspace.messages.physicalRobotArm")} width="300" height="284" className={"block w-full h-full"}  />
          {arr(v.armDots).map((d, $i0) => (
            <Fragment key={$i0}>
              <button
                type="button"
                className={`aw-dot [&:not(.aw-dot-on):hover]:bg-blue-tint! ${d.on ? "aw-dot-on" : ""} ${DOT_BASE}
                  ${d.on ? DOT_SELECTED : DOT_IDLE}`}
                style={{ left: `${d.x}%`, top: `${d.y}%` }}
                onClick={() => v.jointActions.pickArmDot(d.index)}
                aria-label={d.label}
                aria-pressed={d.on}
              >
                {T(d.sid)}
              </button>
            </Fragment>
          ))}
          {v.showArmTag ? (
            <>
            <span
              aria-hidden="true"
              className="absolute [transform:translateY(-50%)] max-w-[130px] whitespace-nowrap overflow-hidden
                text-ellipsis py-[3px] px-[8px] rounded-[8px] bg-ink text-white text-[12px] font-bold leading-[1.4]
                pointer-events-none z-[3] shadow-[0_2px_6px_rgba(var(--ink-rgb),0.3)]"
              style={{ left: `calc(${v.armTag.x}% + 22px)`, top: `${v.armTag.y}%` }}
            >
              {T(v.armTag.text)}
            </span>
            </>
          ) : null}
        </div>
      </div>
    </div>
  );
}
