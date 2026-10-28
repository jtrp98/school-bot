import { Button } from "../../../../components/shared/Button";
import { t as translate } from "../../../../i18n";
import { NumericInput } from "../../../../components/robot-control/NumericInput";
import type { ReactNode } from "react";
import { T } from "../tplUtil";
import type { WorkspaceViewProps } from "../viewTypes";

type Props = WorkspaceViewProps<
  "showSelPanel" | "selNone" | "selIsPose0" | "goPose0" | "goOff"
  | "usePose0" | "lockOff" | "selIsPose" | "selNo" | "selName" | "showLinkBox" | "linkHead" | "linkDesc"
  | "isSame" | "selT" | "selTValid" | "selTHint" | "isSplit" | "selDurText" | "goSel" | "goSelOff" | "goSelLinked"
  | "goSelLabel" | "showRevert" | "selChanged" | "changeText" | "selDeleteConfirm" | "delLabel"
  | "selectedPoseActions"
>;

const OUTLINE_BUTTON_BASE = "inline-flex items-center justify-center leading-[1.2] "
  + "border-2 border-solid rounded-[10px] "
  + "font-[family-name:inherit] text-[13px] font-bold cursor-pointer h-[36px]";

function cardClass(hasPose: boolean, linked: boolean): string {
  let border = "border-line";
  if (hasPose) border = linked ? "border-ok" : "border-blue-line";
  return `border-2 border-solid ${border} rounded-[16px] p-[10px] flex flex-col gap-[8px] bg-surface`;
}

function linkBoxClass(linked: boolean): string {
  const tone = linked ? "bg-ok-tint text-ok-ink" : "bg-blue-tint text-blue-press";
  return `flex flex-col gap-[2px] rounded-[12px] py-[8px] px-[10px] ${tone}`;
}

function durationInputClass(valid: boolean): string {
  const tone = valid ? "border-line-strong bg-surface" : "border-err bg-err-tint";
  return `h-[36px] w-[62px] min-w-0 box-border rounded-[10px] py-0 px-[6px] font-[family-name:inherit] text-[16px] `
    + `font-bold text-center [font-variant-numeric:tabular-nums] text-ink border-2 border-solid ${tone}`;
}

export function SelectedPose({ v }: Props): ReactNode {
  const actions = v.selectedPoseActions;
  return (
    v.showSelPanel ? (
      <>
      <div className={"w-[300px] shrink-0 flex flex-col gap-[6px]"} >
        <span
          className="font-[family-name:'Mitr',_'Noto_Sans_Thai',_sans-serif] text-[17px] font-medium
            leading-[32px]"
        >
          {translate("common.messages.selectedPose")}
        </span>
        <div className={cardClass(v.selIsPose, v.goSelLinked)}>
          {v.selNone ? (
            <>
            <div className={"flex flex-col items-center gap-[10px] py-[18px] px-[6px] text-center"} >
              <svg
                width="44"
                height="44"
                viewBox="0 0 24 24"
                fill="none"
                stroke="var(--blue-line)"
                strokeWidth="2"
                strokeLinecap="round"
                strokeLinejoin="round"
                aria-hidden="true"
              >
                <rect x="3" y="6" width="18" height="12" rx="3" strokeDasharray="3 3" />
                <path d="M12 9v6M9 12h6" />
              </svg>
              <span className={"text-[16px] font-bold"} >{translate("robot.workspace.messages.noPoseSelected")}</span>
              <span className={"text-[13px] text-muted leading-[1.5]"} >
                {translate("common.messages.select")}{" "}
                <strong className={"text-blue-ink"} >{translate("robot.workspace.messages.addPose")}</strong>
                {" "}{translate("robot.workspace.messages.bottomRightToCreateAPoseFromTheCurrent")}
              </span>
              <span className={"text-[12px] text-muted"} >
                {translate("robot.workspace.messages.selectAPoseBeforeMovingJoints")}
              </span>
            </div>
            </>
          ) : null}
          {v.selIsPose0 ? (
            <>
            <div className={"flex items-center gap-[8px]"} >
              <span
                className="w-[28px] h-[28px] rounded-full flex items-center justify-center text-[13px] font-bold
                  bg-muted text-white shrink-0"
              >
                0
              </span>
              <span className={"flex flex-col"} >
                <span className={"text-[16px] font-bold"} >{translate("robot.workspace.messages.uprightArm")}</span>
                <span className={"text-[12px] text-muted"} >
                  {translate("robot.workspace.messages.systemInitialPoseReadOnly")}
                </span>
              </span>
            </div>
            <span className={"text-[13px] text-muted leading-[1.5] bg-sunken rounded-[10px] py-[8px] px-[10px]"} >
              {translate("robot.workspace.messages.selectUseThisPoseToAppendAnEditableCopy")}
            </span>
            <Button
              type="button"
              onClick={v.goPose0}
              disabled={v.goOff}
              variant="primary"
              className="min-h-0! h-[46px]! rounded-[12px]! text-[15px]! font-[family-name:inherit]!"
            >
              {translate("robot.workspace.messages.moveRobotToPose0")}
            </Button>
            <button
              type="button"
              onClick={v.usePose0}
              disabled={v.lockOff}
              className="inline-flex items-center justify-center leading-[1.2] h-[46px] border-2 border-solid
                border-blue-line border-b-4 rounded-[12px] bg-surface text-blue-ink
                font-[family-name:inherit] text-[15px] font-bold cursor-pointer"
            >
              {translate("robot.workspace.messages.useThisPoseLabel")}
            </button>
            </>
          ) : null}
          {v.selIsPose ? (
            <>
            <div className={"flex flex-col gap-[4px]"} >
              <label htmlFor="sel-name-4a" className={"text-[12px] font-semibold text-muted"} >
                {translate("common.messages.poseName")}
              </label>
              <div className={"flex items-center gap-[8px] min-w-0"} >
                <span
                  className="w-[28px] h-[28px] rounded-full flex items-center justify-center text-[13px]
                    font-bold bg-blue-ink text-white shrink-0"
                >
                  {T(v.selNo)}
                </span>
                <input
                  id="sel-name-4a"
                  type="text"
                  className="aw-edit cursor-text [&:hover:not(:disabled)]:bg-blue-tint!
                    [&:hover:not(:disabled)]:border-blue-ink! focus:outline-none focus:border-solid!
                    focus:border-blue-ink! focus:bg-white! focus:shadow-[0_0_0_3px_var(--blue-tint)]
                    h-[40px] grow min-w-0 box-border border-2 border-solid border-transparent
                    rounded-[10px] py-0 px-[10px] font-[family-name:inherit] text-[16px] font-bold
                    text-ink bg-blue-tint"
                  value={v.selName ?? ""}
                  onChange={actions.changeName}
                  onBlur={actions.restoreEmptyName}
                  disabled={v.lockOff}
                  maxLength={24}
                />
              </div>
            </div>
            {v.showLinkBox ? (
              <>
              <div className={linkBoxClass(v.goSelLinked)}>
                <span className={"text-[13px] font-bold"} >
                  {T(v.linkHead)}
                </span>
                <span className={"text-[12px] leading-[1.35]"} >
                  {T(v.linkDesc)}
                </span>
              </div>
              </>
            ) : null}
            {v.isSame ? (
              <>
              <div className={"flex flex-col gap-[2px] bg-sunken rounded-[12px] py-[6px] px-[10px]"} >
                <div className={"flex items-center gap-[4px]"} >
                  <label
                    htmlFor="sel-time-4a"
                    className="text-[13px] font-bold grow min-w-0 whitespace-nowrap overflow-hidden text-ellipsis"
                  >
                    {translate("common.messages.moveDuration")}{" "}
                    <span className={"font-normal text-[12px] text-muted"} >
                      {translate("robot.workspace.messages.allServos")}
                    </span>
                  </label>
                  <button
                    type="button"
                    aria-label={translate("common.messages.decreaseDurationBy100Ms")}
                    onClick={actions.shortenDuration}
                    disabled={v.lockOff}
                    className="inline-flex items-center justify-center leading-[1.2] w-[32px] h-[36px] shrink-0
                      border-2 border-solid border-line rounded-[10px] bg-white
                      font-[family-name:inherit] text-[18px] font-bold text-ink cursor-pointer"
                  >
                    −
                  </button>
                  <NumericInput
                    id="sel-time-4a"
                    value={v.selT ?? ""}
                    onChange={actions.changeDuration}
                    disabled={v.lockOff}
                    className={durationInputClass(v.selTValid)}
                  />
                  <button
                    type="button"
                    aria-label={translate("common.messages.increaseDurationBy100Ms")}
                    onClick={actions.lengthenDuration}
                    disabled={v.lockOff}
                    className="inline-flex items-center justify-center leading-[1.2] w-[32px] h-[36px] shrink-0
                      border-2 border-solid border-line rounded-[10px] bg-white
                      font-[family-name:inherit] text-[18px] font-bold text-ink cursor-pointer"
                  >
                    +
                  </button>
                  <span className={"text-[13px] font-semibold shrink-0"} >{"ms"}</span>
                </div>
                <span className={`text-[12px] font-semibold ${v.selTValid ? "text-blue-ink" : "text-err-ink"}`}>
                  {T(v.selTHint)}
                  {" "}
                  <span className={"font-normal text-muted"} >{translate("robot.workspace.messages.1000Ms1Sec")}</span>
                </span>
              </div>
              </>
            ) : null}
            {v.isSplit ? (
              <>
              <div
                className="text-[12px] text-blue-ink bg-blue-tint rounded-[10px] py-[8px] px-[10px]
                  leading-[1.45]"
              >
                {translate("robot.workspace.messages.setEachServoDurationInItsDurationMsField")}{" "}
                {T(v.selDurText)}
              </div>
              </>
            ) : null}
            <Button
              type="button"
              onClick={v.goSel}
              disabled={v.goSelOff}
              variant={v.goSelLinked ? "ok" : "primary"}
              className={`min-h-0! h-[44px]! rounded-[12px]! text-[15px]! font-[family-name:inherit]!
                ${v.goSelLinked ? "disabled:bg-ok-tint disabled:text-ok-ink disabled:border-ok-line" : ""}`}
            >
              {T(v.goSelLabel)}
            </Button>
            {v.showRevert ? (
              <>
              {v.selChanged ? (
                <>
                <div className={"flex items-center gap-[8px] bg-ok-tint rounded-[10px] py-[6px] px-[8px]"} >
                  <span className={"text-[12px] text-ok-ink grow leading-[1.35]"} >
                    {T(v.changeText)}
                  </span>
                  <button
                    type="button"
                    onClick={actions.revertToArrival}
                    disabled={v.lockOff}
                    className="inline-flex items-center justify-center gap-[4px] leading-[1.2] h-[34px] py-0
                      px-[10px] shrink-0 border-2 border-solid border-ok rounded-[9px] bg-white
                      text-ok-ink font-[family-name:inherit] text-[13px] font-bold cursor-pointer"
                  >
                    <svg
                      width="14"
                      height="14"
                      viewBox="0 0 24 24"
                      fill="none"
                      stroke="currentColor"
                      strokeWidth="2.6"
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      aria-hidden="true"
                    >
                      <path d="M9 14 4 9l5-5" />
                      <path d="M4 9h11a5 5 0 0 1 0 10h-3" />
                    </svg>{translate("robot.workspace.messages.restoreValues")}</button>
                </div>
                </>
              ) : null}
              </>
            ) : null}
            <div className={"grid [grid-template-columns:repeat(2,_minmax(0,_1fr))] gap-[8px]"} >
              <button
                type="button"
                onClick={actions.duplicate}
                disabled={v.lockOff}
                className={`${OUTLINE_BUTTON_BASE} border-line-strong bg-surface text-blue-ink`}
              >
                {translate("robot.workspace.messages.duplicateThisPose")}
              </button>
              <button
                type="button"
                onClick={actions.remove}
                disabled={v.lockOff}
                className={`${OUTLINE_BUTTON_BASE} ${
                  v.selDeleteConfirm ? "border-err bg-err-tint text-err-ink" : "border-line bg-surface text-muted"}`}
              >
                {T(v.delLabel)}
              </button>
            </div>
            </>
          ) : null}
        </div>
      </div>
      </>
    ) : null
  );
}
