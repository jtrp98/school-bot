import { Icon } from "../../../../components/shared/Icons";
import { t as translate } from "../../../../i18n";
import { NumericInput } from "../../../../components/robot-control/NumericInput";
import { HoldButton } from "../../../../components/robot-control/HoldButton";
import type { ReactNode } from "react";
import type { DurationTone, JointView, KnobTone, ReadButtonState } from "../jointsView";
import { T } from "../tplUtil";
import type { WorkspaceViewProps } from "../viewTypes";

type Props = WorkspaceViewProps<
  | "jointActions" | "dom" | "jEmptyCls" | "lockOff" | "jogAria" | "jogDis" | "isSame" | "isSplit" | "tmOff"
  | "tmTip" | "chainLabel" | "hasSelectedPose" | "renameDraft" | "poseActions" | "jointPanelActions"
> & { joint: JointView };

const ICON_CLASS = "inline-block size-[1.15em] align-[-.2em] mr-[.3em]";
const JOG_BUTTON_CLASS = "h-[40px]! min-h-0! w-[48px] shrink-0 touch-none gap-[4px]! p-0! rounded-[14px]! "
  + "text-[15px]! font-[family-name:inherit]!";
const VALUE_HOVER_CLASS = "aw-val hover:bg-blue-tint hover:text-blue-ink hover:border-b-blue-ink! "
  + "focus-within:bg-blue-tint focus-within:text-blue-ink focus-within:border-b-blue-ink!";
const BAR_STRIPES = "bg-[repeating-linear-gradient(45deg,_#F3C2C2_0_4px,_#FBE3E3_4px_8px)]";
const BARE_INPUT_CLASS = "box-border border-none border-current outline-none bg-transparent "
  + "font-[family-name:inherit] font-bold "
  + "text-center [font-variant-numeric:tabular-nums] text-inherit";

const READ_BUTTON_TONE: Record<ReadButtonState, string> = {
  done: "border-none border-current bg-ok text-white",
  disabled: "border-2 border-solid border-line bg-sunken text-faint cursor-not-allowed",
  ready: "border-none border-current border-b-[3px] [border-bottom-style:solid] border-b-blue-press "
    + "bg-blue-ink text-white "
    + "cursor-pointer",
};

const KNOB_TONE: Record<KnobTone, string> = {
  moving: "bg-line border-surface shadow-[0_0_0_1px_var(--faint)]",
  limit: "bg-err border-surface shadow-[0_0_0_2px_var(--err-ink)]",
  viewing: "bg-surface border-blue-ink",
  live: "bg-orange border-surface shadow-[0_0_0_1px_var(--orange-press)]",
};

const DURATION_BOX_TONE: Record<DurationTone, string> = {
  disabled: "border-off bg-sunken text-faint",
  invalid: "border-err bg-err-tint text-err-ink",
  linked: "border-blue-ink bg-blue-tint text-ink",
  linkedFlash: "border-blue-ink bg-blue-line text-ink",
  split: "border-line-strong bg-surface text-ink",
};

function cardClass(selected: boolean, emptyClass: string): string {
  const tone = selected
    ? "border-blue-ink bg-blue-tint shadow-[inset_0_0_0_1px_var(--blue-ink),0_0_0_3px_var(--blue-tint)]"
    : "border-line bg-surface";
  return `aw-jcard focus-visible:[outline:3px_solid_var(--blue-ink)] focus-visible:[outline-offset:2px] `
    + `[&.aw-jempty_button:disabled]:pointer-events-none [&.aw-jempty_input:disabled]:pointer-events-none `
    + `[&.aw-jempty_.aw-valbox_*]:pointer-events-none ${emptyClass} relative flex-[1_1_56px] min-h-[56px] box-border `
    + `py-[6px] px-[12px] rounded-[14px] flex items-center gap-[10px] cursor-pointer border-2 border-solid ${tone}`;
}

function badgeClass(selected: boolean): string {
  const tone = selected
    ? "border-blue-ink bg-blue-ink text-white"
    : "border-blue-line bg-surface text-blue-press";
  return `w-[36px] h-[30px] shrink-0 rounded-[9px] flex items-center justify-center font-bold text-[13px] `
    + `border-2 border-solid ${tone}`;
}

function valueBoxClass(viewing: boolean): string {
  const tone = viewing ? "border-dashed border-blue-line text-blue-ink" : "border-solid border-line-strong text-ink";
  return `aw-valbox [&:hover_.aw-step]:opacity-[1] [&:focus-within_.aw-step]:opacity-[1] `
    + `[&:focus-within_.aw-valtip]:block relative w-[100px] h-[40px] shrink-0 box-border rounded-[10px] flex flex-col `
    + `items-center justify-center leading-[1.15] bg-surface border-2 ${tone}`;
}

function durationBoxClass(tone: DurationTone, hasPose: boolean): string {
  return `relative w-[84px] h-[40px] shrink-0 box-border rounded-[10px] flex flex-col items-center justify-center `
    + `leading-[1.15] [transition:background_0.6s] ${hasPose ? "pr-[14px]" : "pr-0"} border-2 border-solid `
    + DURATION_BOX_TONE[tone];
}

function stepButtonClass(hasPose: boolean): string {
  return `aw-step opacity-0 [transition:opacity_0.12s] [&:hover:not(:disabled)]:text-blue-ink! disabled:opacity-0! `
    + `w-[14px] h-[22px] p-0 border-none border-current bg-transparent text-inherit `
    + `font-[family-name:inherit] text-[14px] font-bold `
    + `leading-none cursor-pointer shrink-0 ${hasPose ? "" : "pointer-events-none"}`;
}

export function JointCard({ v, joint }: Props): ReactNode {
  const actions = v.jointActions;
  const index = joint.index;
  const robotMarker = joint.robotMarkerPercent;
  return (
    <div
      className={cardClass(joint.selected, v.jEmptyCls ?? "")}
      ref={v.dom.jointCardRef(index)}
      tabIndex={0}
      data-jcard={index}
      onClick={() => actions.selectJoint(index)}
      onKeyDown={(event) => actions.keyOnCard(index, event)}
      aria-label={joint.cardLabel}
    >
      <span className={badgeClass(joint.selected)}>{T(joint.sid)}</span>
      <div
        onClick={(event) => actions.clickValueBox(index, event)}
        className="w-[104px] shrink-0 flex flex-col gap-[1px]"
      >
        {joint.renaming ? (
          <>
            <input
              type="text"
              className="aw-edit cursor-text [&:hover:not(:disabled)]:bg-blue-tint!
                [&:hover:not(:disabled)]:border-blue-ink! focus:outline-none focus:border-solid!
                focus:border-blue-ink! focus:bg-white! focus:shadow-[0_0_0_3px_var(--blue-tint)]
                h-[30px] w-full box-border border-2 border-solid border-blue-ink rounded-[8px] py-0
                px-[6px] font-[family-name:inherit] text-[14px] font-bold text-ink bg-white"
              value={v.renameDraft ?? ""}
              onChange={v.poseActions.editRenameDraft}
              onKeyDown={v.poseActions.keyOnRenameDraft}
              onBlur={v.poseActions.commitRename}
              autoFocus={true}
              maxLength={16}
              aria-label={joint.renameInputLabel}
              placeholder={joint.defaultName}
            />
            <span className="text-[10px] text-muted whitespace-nowrap">
              {translate("robot.workspace.messages.enterToSaveEscToCancel")}
            </span>
          </>
        ) : (
          <>
            <button
              type="button"
              className="aw-name [&:hover:not(:disabled)]:bg-blue-tint! [&:hover:not(:disabled)]:text-blue-ink!
                [&:hover:not(:disabled)]:border-b-blue-ink! inline-flex items-center gap-[4px]
                max-w-full py-[1px] px-[4px] ml-[-4px] border-none border-current border-b-2
                [border-bottom-style:solid] border-b-transparent rounded-[6px_6px_0_0] bg-transparent
                font-[family-name:inherit] text-[15px] font-bold text-ink cursor-text text-left"
              onClick={(event) => actions.startRename(index, event)}
              disabled={v.lockOff}
              title={translate("robot.workspace.messages.tapToRename")}
              aria-label={joint.renameLabel}
            >
              <span className="whitespace-nowrap overflow-hidden text-ellipsis min-w-0">{T(joint.name)}</span>
            </button>
            <span className="flex items-center gap-[6px] min-w-0">
              <span
                title={joint.servoIdTip}
                className="shrink-0 h-[18px] py-0 px-[6px] rounded-[5px] bg-blue-tint border-1 border-solid
                  border-line-strong text-[11px] font-bold leading-[16px] text-blue-press
                  [font-variant-numeric:tabular-nums] whitespace-nowrap"
              >
                {"ID "}
                {T(joint.servoId)}
              </span>
            </span>
          </>
        )}
      </div>
      {joint.releaseMode ? (
        <div className="[flex:1_1_0] min-w-0 flex items-center gap-[12px] pl-[4px]">
          <span className="inline-flex items-center gap-[6px] text-muted text-[14px] font-bold whitespace-nowrap">
            <Icon name="lock" className={ICON_CLASS} />
            {translate("robot.workspace.messages.adjustByHand")}
          </span>
          <span className="[flex:1_1_0]" />
          <span className="text-[14px] text-muted whitespace-nowrap">{T(joint.releasePoseText)}</span>
          <span
            className={`text-[16px] font-bold whitespace-nowrap ${
              joint.releaseLiveDiffers ? "text-orange-deep" : "text-muted"}`}
          >
            {T(joint.releaseLiveText)}
          </span>
          <button
            type="button"
            onClick={(event) => actions.readJoint(index, event)}
            disabled={joint.readState !== "ready"}
            className={`h-[34px] py-0 px-[12px] rounded-[10px] font-[family-name:inherit] text-[14px] font-bold
              whitespace-nowrap ${READ_BUTTON_TONE[joint.readState]}`}
          >
            {T(joint.readLabel)}
          </button>
        </div>
      ) : (
        <>
          <HoldButton
            aria-label={joint.decreaseLabel}
            title={joint.decreaseLabel}
            onPointerDown={() => actions.startHold(index, -1)}
            onHoldEnd={v.jointPanelActions.releaseJog}
            disabled={v.jogDis}
            aria-disabled={v.jogAria}
            variant="primary"
            aria-pressed={joint.decreasePressed}
            className={JOG_BUTTON_CLASS}
          >
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="3.5"
              strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
              <path d="m15 5-7 7 7 7" />
            </svg>
          </HoldButton>
          <div className="grow min-w-[60px] flex flex-col gap-[2px]">
            <div className="h-[12px] rounded-full bg-line-strong relative my-0 mx-[9px]">
              <div className={`absolute left-0 top-0 bottom-0 w-[5%] rounded-[999px_0_0_999px] ${BAR_STRIPES}`} />
              <div className={`absolute left-[95%] right-0 top-0 bottom-0 rounded-[0_999px_999px_0] ${BAR_STRIPES}`} />
              <div className="absolute left-[50%] top-0 bottom-0 w-[2px] ml-[-1px] bg-line-strong" />
              {robotMarker !== null ? (
                <div
                  title={translate("robot.workspace.messages.robotIsHere")}
                  className="absolute top-0 bottom-0 w-[4px] ml-[-2px] bg-muted rounded-[2px]"
                  style={{ left: `${robotMarker}%` }}
                />
              ) : null}
              <div
                className={`absolute top-[-3px] w-[18px] h-[18px] ml-[-9px] box-border rounded-full [transition:none]
                  border-[3px] border-solid ${KNOB_TONE[joint.knobTone]}`}
                style={{ left: `${joint.knobPercent}%` }}
              />
              {joint.bulbPercent !== null ? (
                <div
                  className="absolute top-[1px] w-[10px] h-[10px] ml-[-5px] box-border rounded-full bg-orange
                    shadow-[0_0_0_3px_rgba(var(--star-rgb),0.35)] animate-[awBulbOn_0.25s_ease-out_both]
                    pointer-events-none"
                  style={{ left: `${joint.bulbPercent}%` }}
                  title={translate("robot.workspace.messages.movingHere")}
                />
              ) : null}
            </div>
            <div data-lim6="1" className="flex justify-between text-[10px] text-faint py-0 px-[2px]">
              <span>{T(joint.minLabel)}</span>
              <span>0</span>
              <span>{T(joint.maxLabel)}</span>
            </div>
          </div>
          <HoldButton
            aria-label={joint.increaseLabel}
            title={joint.increaseLabel}
            onPointerDown={() => actions.startHold(index, 1)}
            onHoldEnd={v.jointPanelActions.releaseJog}
            disabled={v.jogDis}
            aria-disabled={v.jogAria}
            variant="primary"
            aria-pressed={joint.increasePressed}
            className={JOG_BUTTON_CLASS}
          >
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="3.5"
              strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
              <path d="m9 5 7 7-7 7" />
            </svg>
          </HoldButton>
          <label className={valueBoxClass(joint.boxViewing)} onClick={(event) => actions.clickValueBox(index, event)}>
            <span className="text-[10px] font-semibold whitespace-nowrap">{T(joint.boxTitle)}</span>
            <span className="flex items-center justify-center">
              <button
                type="button"
                tabIndex={-1}
                className={stepButtonClass(v.hasSelectedPose)}
                onClick={(event) => actions.step(index, -1, event)}
                disabled={v.jogDis}
                aria-disabled={v.jogAria}
                aria-label={joint.stepDownLabel}
              >
                ‹
              </button>
              <span
                className={`${v.jogAria ? "" : VALUE_HOVER_CLASS} flex items-center justify-center gap-[1px] py-0
                  px-[2px] rounded-[6px_6px_0_0] border-b-2 [border-bottom-style:solid] border-b-transparent
                  cursor-text`}
              >
                <NumericInput
                  value={joint.valueText ?? ""}
                  onChange={(event) => actions.editValue(index, event)}
                  onBlur={() => actions.commitValue(index)}
                  onKeyDown={(event) => actions.keyOnValue(index, event)}
                  disabled={v.jogDis}
                  aria-disabled={v.jogAria}
                  aria-label={joint.valueLabel}
                  aria-describedby={joint.valueTipId}
                  className={`${BARE_INPUT_CLASS} w-[40px] h-[22px] cursor-text py-0 px-[2px] text-[16px]`}
                />
                <span className="text-[13px] font-bold cursor-text">{T(joint.unit)}</span>
              </span>
              <button
                type="button"
                tabIndex={-1}
                className={stepButtonClass(v.hasSelectedPose)}
                onClick={(event) => actions.step(index, 1, event)}
                disabled={v.jogDis}
                aria-disabled={v.jogAria}
                aria-label={joint.stepUpLabel}
              >
                ›
              </button>
            </span>
            <span
              id={joint.valueTipId}
              role="tooltip"
              className="aw-valtip hidden absolute top-[calc(100%_+_6px)] right-0 whitespace-nowrap bg-ink
                text-white text-[12px] font-semibold leading-[1.5] py-[5px] px-[9px] rounded-[8px]
                z-[40] shadow-pop"
            >
              {T(joint.valueKeyTip)}
            </span>
          </label>
          <label
            data-tbox6="1"
            className={durationBoxClass(joint.durationTone, v.hasSelectedPose)}
            onClick={(event) => actions.clickValueBox(index, event)}
          >
            <span className="text-[10px] font-semibold whitespace-nowrap">
              {translate("robot.workspace.messages.durationMs")}
            </span>
            <NumericInput
              value={joint.durationValue ?? ""}
              placeholder="—"
              onChange={(event) => actions.changeDuration(index, event)}
              disabled={joint.durationDisabled}
              aria-label={joint.durationLabel}
              className={`${BARE_INPUT_CLASS} w-[52px] h-[20px] min-w-0 p-0 text-[15px] ${
                v.lockOff || !v.hasSelectedPose ? "cursor-not-allowed" : "cursor-text"}`}
            />
            <button
              type="button"
              onClick={v.jointPanelActions.toggleTimeMode}
              disabled={v.tmOff}
              aria-pressed={v.isSame}
              aria-label={v.chainLabel}
              title={v.tmTip}
              className="absolute right-[2px] top-[50%] mt-[-11px] w-[22px] h-[22px] p-0 border-none
                border-current rounded-[6px] bg-transparent flex items-center justify-center
                cursor-pointer"
            >
              {v.isSame ? (
                <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="var(--blue-ink)" strokeWidth="2.8"
                  strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
                  <path d="M10 13a5 5 0 0 0 7.5.5l3-3a5 5 0 0 0-7-7l-1.7 1.7" />
                  <path d="M14 11a5 5 0 0 0-7.5-.5l-3 3a5 5 0 0 0 7 7l1.7-1.7" />
                </svg>
              ) : null}
              {v.isSplit ? (
                <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="var(--faint)" strokeWidth="2.8"
                  strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
                  <path d="M18.8 12.7l1.7-1.7a5 5 0 0 0-7-7l-1.7 1.7" />
                  <path d="M5.2 11.3l-1.7 1.7a5 5 0 0 0 7 7l1.7-1.7" />
                </svg>
              ) : null}
            </button>
          </label>
          <div className="relative shrink-0">
            <button
              type="button"
              onClick={(event) => actions.copyCommand(index, event)}
              aria-label={joint.copyLabel}
              title={joint.copyLabel}
              className="flex items-center justify-center shrink-0 w-[40px] h-[40px] p-0 box-border
                rounded-[12px] font-[family-name:inherit] border-none border-current cursor-pointer
                bg-transparent text-muted"
            >
              <Icon name="copy" className={ICON_CLASS} />
            </button>
          </div>
        </>
      )}
    </div>
  );
}
