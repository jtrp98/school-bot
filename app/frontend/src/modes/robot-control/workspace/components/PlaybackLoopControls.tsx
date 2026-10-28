import { t as translate } from "../../../../i18n";
import type { ReactNode } from "react";
import type { PlaybackBarView } from "../playbackBar";
import type { PlaybackBarActions } from "../playbackBarActions";
import { T } from "../tplUtil";

type Props = {
  loop: PlaybackBarView["loop"];
  actions: PlaybackBarActions;
};

const MENU_ITEM_CLASS = "flex flex-col items-start gap-[2px] text-left w-full box-border py-[8px] px-[10px] "
  + "rounded-[10px] font-[family-name:inherit] cursor-pointer border-2 border-solid";
const UNSELECTED_ITEM_CLASS = "border-off bg-surface text-ink";

function menuButtonTone(loop: PlaybackBarView["loop"]): string {
  if (loop.disabled) return "border-off bg-sunken text-faint cursor-not-allowed font-semibold";
  return loop.forever
    ? "border-blue-ink bg-blue-tint text-blue-ink cursor-pointer font-bold"
    : "border-line bg-surface text-ink cursor-pointer font-semibold";
}

export function PlaybackLoopControls({ loop, actions }: Props): ReactNode {
  const stepTone = loop.disabled
    ? "border-off bg-sunken text-faint cursor-not-allowed"
    : "border-line bg-surface text-ink cursor-pointer";
  const stepClass = `inline-flex items-center justify-center leading-[1.2] w-[34px] h-[34px] rounded-[10px]
    font-[family-name:inherit] text-[18px] font-bold border-2 border-solid ${stepTone}`;
  return (
    <div className="flex items-center gap-[6px] text-[14px] shrink-0 relative">
      <span className={`font-semibold ${loop.disabled ? "text-faint" : "text-ink"}`}>
        {translate("common.messages.repeat")}
      </span>
      {loop.forever ? null : (
        <>
          <button type="button" aria-label={translate("common.messages.fewerRepetitions")}
            onClick={actions.decreaseLoops} disabled={loop.disabled} className={stepClass}>
            −
          </button>
          <span className={`min-w-[22px] text-center font-bold text-[17px] ${loop.disabled ? "text-faint" : "text-ink"}`}>
            {T(loop.count)}
          </span>
          <button type="button" aria-label={translate("common.messages.moreRepetitions")}
            onClick={actions.increaseLoops} disabled={loop.disabled} className={stepClass}>
            +
          </button>
        </>
      )}
      <span className="relative inline-flex">
        <button
          type="button"
          aria-haspopup="menu"
          aria-expanded={loop.menuOpen}
          aria-label={translate("robot.workspace.messages.repeatModeValue0", { value0: loop.label })}
          title={loop.tip}
          onClick={actions.toggleLoopMenu}
          disabled={loop.disabled}
          className={`inline-flex items-center gap-[4px] h-[36px] py-0 px-[10px] rounded-[10px] font-[family-name:inherit]
            text-[14px] whitespace-nowrap border-2 border-solid border-b-4 ${menuButtonTone(loop)}`}
        >
          <span>{T(loop.label)}</span>
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="3"
            strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
            <path d="m6 9 6 6 6-6" />
          </svg>
        </button>
        {loop.menuOpen ? (
          <>
            <div onClick={actions.closeLoopMenu} aria-hidden="true"
              className="fixed inset-0 z-[29] bg-transparent left-0 top-0" />
            <div role="menu" aria-label={translate("robot.workspace.messages.repeatMode")}
              className="absolute left-0 bottom-[calc(100%_+_8px)] w-[260px] box-border p-[8px] flex flex-col gap-[6px]
                bg-white border-2 border-solid border-line-strong rounded-[14px] shadow-pop z-[30] [white-space:normal]">
              <button type="button" role="menuitem" onClick={actions.chooseLoopCount}
                className={`${MENU_ITEM_CLASS} ${loop.forever
                  ? UNSELECTED_ITEM_CLASS
                  : "border-blue-ink bg-blue-tint text-blue-press"}`}>
                <span className="text-[14px] font-bold">{translate("robot.workspace.messages.chooseRepeatCount")}</span>
                <span className="text-[12px] text-muted leading-[1.4]">
                  {translate("robot.workspace.messages.adjustWith19Times")}
                </span>
              </button>
              <button type="button" role="menuitem" onClick={actions.chooseLoopForever}
                className={`${MENU_ITEM_CLASS} ${loop.forever
                  ? "border-blue-ink bg-blue-tint text-blue-ink"
                  : UNSELECTED_ITEM_CLASS}`}>
                <span className="text-[14px] font-bold">{translate("robot.workspace.messages.loopForever")}</span>
                <span className="text-[12px] text-muted leading-[1.4]">{T(loop.foreverDetail)}</span>
              </button>
            </div>
          </>
        ) : null}
      </span>
    </div>
  );
}
