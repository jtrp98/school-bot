import { Icon } from "../../../../components/shared/Icons";
import { t as translate } from "../../../../i18n";
import type { ReactNode } from "react";
import type { PlaybackBarView, PlaybackChoice } from "../playbackBar";
import type { PlaybackBarActions } from "../playbackBarActions";
import { T } from "../tplUtil";

type Props = {
  menu: PlaybackBarView["menu"];
  actions: PlaybackBarActions;
};

const SECTION_TITLE_CLASS = "pt-[4px] px-[10px] pb-[2px] text-[12px] font-bold text-muted";

function choiceTone<Value>(choice: PlaybackChoice<Value>): string {
  if (choice.disabled) return "cursor-not-allowed bg-surface text-faint";
  return choice.selected ? "cursor-pointer bg-blue-tint text-blue-ink" : "cursor-pointer bg-surface text-ink";
}

type ChoiceButtonProps<Value> = { choice: PlaybackChoice<Value>; onPick: (value: Value) => void };

function ChoiceButton<Value>({ choice, onPick }: ChoiceButtonProps<Value>): ReactNode {
  return (
    <button
      type="button"
      role="menuitemradio"
      aria-checked={choice.selected}
      onClick={() => onPick(choice.value)}
      disabled={choice.disabled}
      className={`flex items-start gap-[10px] py-[8px] px-[10px] border-none rounded-[9px] font-[family-name:inherit]
        text-[14px] font-bold text-left ${choiceTone(choice)}`}
    >
      <span>{choice.selected ? "●" : "○"}</span>
      <span className="flex flex-col items-start">
        <span>
          <Icon name={choice.icon} className="inline-block size-[1.15em] align-[-.2em] mr-[.3em]" />
          {T(choice.label)}
        </span>
        <span className="text-[12px] font-medium text-muted">{T(choice.detail)}</span>
      </span>
    </button>
  );
}

export function PlaybackPickerMenu({ menu, actions }: Props): ReactNode {
  return (
    <div
      role="menu"
      aria-label={translate("robot.workspace.messages.chooseWhatToPlay")}
      className={`absolute bottom-[calc(100%_+_18px)] w-[300px] box-border p-[8px] flex flex-col gap-[2px] bg-surface
        border-2 border-solid border-line-strong rounded-[14px] shadow-[0_10px_24px_rgba(var(--ink-rgb),0.2)] z-[40]
        ${menu.open === "scope" ? "left-[120px]" : "left-0"}`}
    >
      {menu.showSourceChoices ? (
        <>
          <span className={SECTION_TITLE_CLASS}>{translate("robot.workspace.messages.playFrom")}</span>
          {menu.sourceChoices.map((choice) => (
            <ChoiceButton key={choice.value} choice={choice} onPick={actions.pickSource} />
          ))}
        </>
      ) : null}
      {menu.showDivider ? <span className="h-[2px] bg-sunken my-[6px] mx-[4px]" /> : null}
      {menu.showScopeChoices ? (
        <>
          <span className={SECTION_TITLE_CLASS}>{translate("robot.workspace.messages.playbackRange")}</span>
          {menu.showRobotScopeNote ? (
            <span className="py-[6px] px-[10px] text-[13px] text-faint">
              {translate("robot.workspace.messages.robotMemorySupportsFullSequencePlaybackOnly")}
            </span>
          ) : null}
          {menu.scopeChoices.map((choice) => (
            <ChoiceButton
              key={choice.value.kind === "group" ? `group-${choice.value.groupId}` : choice.value.kind}
              choice={choice}
              onPick={actions.pickScope}
            />
          ))}
        </>
      ) : null}
    </div>
  );
}
