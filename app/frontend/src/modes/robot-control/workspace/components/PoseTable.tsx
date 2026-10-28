import { Icon } from "../../../../components/shared/Icons";
import { t as translate } from "../../../../i18n";
import { Fragment, type ReactNode } from "react";
import { T, arr } from "../tplUtil";
import type { WorkspaceViewProps } from "../viewTypes";
import { PoseTableRow, gridClass } from "./PoseTableRow";

type Props = WorkspaceViewProps<
  "servoHeads" | "isSame" | "jsel6" | "relFlag6" | "p0Off" | "p0Cells" | "p0m6Open" | "lockOff"
  | "pasteTopShow" | "pClipN" | "noSeq" | "rows" | "gRenDraft"
  | "isOrderMode" | "isSplit" | "notOrderMode" | "relAll" | "relAllOff" | "relNew" | "addRowShow" | "addCls"
  | "addPose" | "addOff" | "addRowNo" | "dom" | "poseActions" | "jointActions"
>;

const MONO_FONT = "font-[family-name:ui-monospace,_Menlo,_Consolas,_monospace]";
const ADD_ROW_CLASS = `aw-addfirst disabled:border-line-strong! disabled:bg-sunken! disabled:text-faint!
  disabled:cursor-not-allowed! [&:disabled>span:first-child]:bg-line! [transition:background-color_.12s,border-color_.12s]
  [&:hover:not(:disabled)]:bg-blue-tint! [&:hover:not(:disabled)]:border-blue-ink!
  [&:hover:not(:disabled)]:border-solid! [&:focus-visible:not(:disabled)]:bg-blue-tint!
  [&:focus-visible:not(:disabled)]:border-blue-ink! [&:focus-visible:not(:disabled)]:border-solid!
  focus-visible:[outline:3px_solid_var(--blue-tint)] focus-visible:[outline-offset:2px]
  [&:active:not(:disabled)]:bg-blue-ink! [&:active:not(:disabled)]:text-white!
  [&:active:not(:disabled)]:border-blue-ink! [&:active:not(:disabled)]:border-b-blue-press!
  [&:active:not(:disabled)]:border-b-4! [&:active:not(:disabled)>span:first-child]:bg-white!
  [&:active:not(:disabled)>span:first-child]:text-blue-ink! [&:active:not(:disabled)]:[transform:translateY(1px)]
  [&.aw-ring]:[animation-name:awAttnIn] motion-reduce:[&.aw-ring]:animate-none
  motion-reduce:[&.aw-ring]:shadow-[inset_0_0_0_3px_rgba(var(--rc),.5)]
  w-full h-[44px] box-border rounded-[12px] flex items-center justify-center gap-[8px] font-[family-name:inherit]
  text-[15px] font-bold border-2 border-dashed`;
const PASTE_ROW_CLASS = "h-[30px] w-full box-border border-2 border-dashed border-blue-ink rounded-[8px] bg-blue-tint "
  + "text-blue-ink font-[family-name:inherit] text-[13px] font-bold cursor-pointer";

export function PoseTable({ v }: Props): ReactNode {
  const addTone = v.addOff
    ? "border-line-strong bg-sunken text-faint cursor-not-allowed"
    : "border-blue-ink bg-blue-tint text-blue-ink cursor-pointer";
  return (
    <div
      role="table"
      aria-label={translate("robot.workspace.messages.poseSequenceTableAnglePerServo")}
      className="border-2 border-solid border-line rounded-[14px] overflow-hidden flex flex-col [flex:1_1_0]
        min-h-0"
    >
      <div role="row" className={`${gridClass(v.isSame)} bg-blue-tint text-[12px] text-muted font-semibold`}>
        <span role="columnheader">{translate("robot.workspace.messages.order")}</span>
        <span role="columnheader" title={translate("robot.workspace.messages.poseCodeKGroupCodeGIsShownIn")}>
          {translate("robot.workspace.messages.poseCode")}
        </span>
        <span role="columnheader" className={"flex items-center gap-[4px]"} >
          {translate("common.messages.poseName")}{" "}
        </span>
        {arr(v.servoHeads).map((head) => (
          <Fragment key={head.sid}>
            <span role="columnheader" className={"flex flex-col items-center leading-[1.25]"} >
              <span
                className="text-[13px] font-bold text-ink whitespace-nowrap overflow-hidden text-ellipsis
                  max-w-full"
              >
                {T(head.sid)}
              </span>
            </span>
          </Fragment>
        ))}
        {v.isSame ? (
          <span role="columnheader" className={"text-center"} >{translate("robot.workspace.messages.durationMs")}</span>
        ) : null}
        <span role="columnheader" className={"text-center"} >{translate("common.play")}</span>
        <span role="columnheader" />
      </div>
      <div className={"[flex:1_1_0] min-h-0 overflow-y-auto flex flex-col"} >
        <div
          ref={v.dom.setTableScroll}
          data-js6={v.jsel6}
          data-rel6={v.relFlag6}
          className="[flex:1_1_0] min-h-0 overflow-y-auto"
        >
          <div
            role="row"
            className={`${gridClass(v.isSame)} min-h-[44px] bg-sunken border-b [border-bottom-style:solid]
              border-b-line-strong`}
          >
            <span role="cell">
              <button
                type="button"
                onClick={v.poseActions.selectPoseZero}
                disabled={v.p0Off}
                aria-label={translate("robot.workspace.messages.selectPose0")}
                className="w-[36px] h-[32px] rounded-[9px] border-2 border-solid border-faint bg-white
                  font-[family-name:inherit] text-[13px] font-bold text-ink [cursor:default]"
              >
                0
              </button>
            </span>
            <span
              role="cell"
              className={`text-[13px] font-bold [font-variant-numeric:tabular-nums] ${MONO_FONT} text-muted`}
            >
              G0000
            </span>
            <span role="cell" className={"flex flex-col leading-[1.3]"} >
              <span className={"text-[14px] font-bold"} >
                {translate("robot.workspace.messages.uprightArm")}{" "}
                <Icon name="lock" className={"inline-block size-[1.15em] align-[-.2em] mr-[.3em]"} />
              </span>
              <span className={"text-[11px] text-muted"} >
                {translate("robot.workspace.messages.initialPoseReadOnly")}
              </span>
            </span>
            {arr(v.p0Cells).map((cell, joint) => (
              <Fragment key={joint}>
                <span
                  role="cell"
                  className="text-center text-[15px] font-semibold [font-variant-numeric:tabular-nums] text-muted"
                >
                  {T(cell.v)}
                </span>
              </Fragment>
            ))}
            {v.isSame ? (
              <span role="cell" className={"text-center text-[14px] text-muted"} >
                2000
              </span>
            ) : null}
            <span role="cell" />
            <span role="cell" className={"flex justify-end"} >
              <button
                type="button"
                onClick={v.p0m6Open}
                aria-haspopup="menu"
                disabled={v.lockOff}
                className="inline-flex items-center justify-center leading-[1.2] h-[32px] py-0 px-[10px] border-2
                  border-solid border-blue-line rounded-[9px] bg-white text-blue-ink
                  font-[family-name:inherit] text-[12px] font-bold cursor-pointer whitespace-nowrap"
              >
                {translate("robot.workspace.messages.useThisPoseLabel")}
              </button>
            </span>
          </div>
          {v.pasteTopShow ? (
            <div className={"py-[2px] px-[10px]"} >
              <button
                type="button"
                onClick={v.poseActions.pasteBeforeFirst}
                aria-label={translate("robot.workspace.messages.pasteAtStartValue0Poses", {value0: v.pClipN ?? ""})}
                className={PASTE_ROW_CLASS}
              >
                {translate("robot.workspace.messages.pasteAtStart")}
                {T(v.pClipN)}
                {" "}{translate("robot.workspace.messages.poses")}
              </button>
            </div>
          ) : null}
          {v.noSeq ? (
            <div className={"pt-[16px] px-[16px] pb-[4px] text-center text-muted text-[14px]"} >
              {translate("robot.workspace.messages.noPosesInThisSequence")}
            </div>
          ) : null}
          {arr(v.rows).map((row) => (
            <PoseTableRow key={row.key} v={v} row={row} />
          ))}
          {v.addRowShow ? (
            <div className={"py-[8px] px-[10px]"} >
              <button
                type="button"
                className={`${ADD_ROW_CLASS} ${addTone} ${v.addCls ?? ""}`}
                onClick={v.addPose}
                disabled={v.addOff}
                aria-label={translate("robot.workspace.messages.appendPoseValue0ToTheSequence", {value0: v.addRowNo ?? ""})}
              >
                <svg
                  width="18"
                  height="18"
                  viewBox="0 0 24 24"
                  fill="none"
                  stroke="currentColor"
                  strokeWidth="3"
                  strokeLinecap="round"
                  aria-hidden="true"
                >
                  <path d="M12 5v14M5 12h14" />
                </svg>
                <span>{translate("common.messages.addPose")}</span>
              </button>
            </div>
          ) : null}
        </div>
      </div>
    </div>
  );
}
