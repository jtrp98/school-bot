import { Icon } from "../../../../components/shared/Icons";
import { t as translate } from "../../../../i18n";
import { Fragment, type ReactNode } from "react";
import type { PlaybackBarView, RangeEdge } from "../playbackBar";
import type { PlaybackBarActions } from "../playbackBarActions";
import { T, arr, sx } from "../tplUtil";
import type { WorkspaceViewProps } from "../viewTypes";
import { PlaybackLoopControls } from "./PlaybackLoopControls";
import { PlaybackPickerMenu } from "./PlaybackPickerMenu";

type Props = WorkspaceViewProps<
  "barA" | "barC" | "srcOpts" | "send" | "sendOff" | "sendTip" | "up6Style" | "srcIsApp" | "srcIsRobotTab"
  | "playbackBar" | "playbackActions"
>;

const ICON_CLASS = "inline-block size-[1.15em] align-[-.2em] mr-[.3em]";

function pickerButtonClass(highlighted: boolean, fixedToRobot = false): string {
  const base = "inline-flex items-center gap-[6px] h-[44px] py-0 px-[14px] rounded-[12px] font-[family-name:inherit] "
    + "text-[15px] font-bold whitespace-nowrap border-2 border-solid border-b-4";
  if (fixedToRobot) return `${base} border-off bg-sunken text-faint cursor-not-allowed`;
  return highlighted
    ? `${base} cursor-pointer border-blue-ink bg-blue-tint text-blue-ink`
    : `${base} cursor-pointer border-line-strong bg-surface text-ink`;
}

type RangeSelectProps = {
  edge: RangeEdge;
  ariaLabel: string;
  range: PlaybackBarView["range"];
  actions: PlaybackBarActions;
};

function RangeSelect({ edge, ariaLabel, range, actions }: RangeSelectProps): ReactNode {
  const open = range.openEdge === edge;
  const selectedIndex = edge === "start" ? range.start : range.end;
  const pick = edge === "start" ? actions.pickRangeStart : actions.pickRangeEnd;
  const tone = range.disabled
    ? "border-off bg-sunken text-faint cursor-not-allowed"
    : range.active
      ? "border-blue-ink bg-blue-tint text-blue-ink cursor-pointer"
      : "border-line-strong bg-surface text-ink cursor-pointer";
  return (
    <span className="relative inline-flex">
      <button
        type="button"
        aria-haspopup="listbox"
        aria-expanded={open}
        aria-label={ariaLabel}
        onClick={() => actions.toggleRangeList(edge)}
        disabled={range.disabled}
        className={`h-[38px] min-w-[92px] box-border py-0 pr-[12px] pl-[14px] flex items-center justify-between gap-[8px]
          whitespace-nowrap rounded-[10px] font-[family-name:inherit] text-[14px] font-bold border-2 border-solid border-b-4
          ${tone}`}
      >
        <span>{T(edge === "start" ? range.startLabel : range.endLabel)}</span>
        <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="3.2"
          strokeLinecap="round" strokeLinejoin="round" aria-hidden="true" className="shrink-0">
          <path d="m6 9 6 6 6-6" />
        </svg>
      </button>
      {open ? (
        <div
          role="listbox"
          aria-label={ariaLabel}
          className="absolute bottom-[46px] left-0 min-w-full max-h-[252px] overflow-y-auto box-border p-[6px] flex flex-col
            gap-[2px] bg-surface border-2 border-solid border-line-strong rounded-[12px]
            shadow-[0_10px_24px_rgba(var(--ink-rgb),0.2)] z-[40]"
        >
          {range.options.map((option) => {
            const selected = option.index === selectedIndex;
            return (
              <button
                key={option.index}
                type="button"
                role="option"
                aria-selected={selected}
                onClick={() => pick(option.index)}
                className={`aw-menuitem hover:bg-blue-tint! h-[40px] w-full box-border py-0 pr-[12px] pl-[10px] border-none
                  rounded-[9px] flex items-center gap-[8px] font-[family-name:inherit] text-[15px] font-semibold text-left
                  whitespace-nowrap cursor-pointer ${selected ? "bg-blue-tint text-blue-ink" : "bg-surface text-ink"}`}
              >
                <span className="w-[16px] shrink-0">{selected ? "✓" : ""}</span>
                {T(option.label)}
              </button>
            );
          })}
        </div>
      ) : null}
    </span>
  );
}

export function PlaybackOptions({ v }: Props): ReactNode {
  const bar = v.playbackBar;
  const actions = v.playbackActions;
  const { range, menu } = bar;
  return (
    <div data-b6row="1" className="flex items-center gap-[12px] [flex:1_1_0] min-w-0">
      <span data-b6a="1" className="relative flex gap-[6px] shrink-0">
        {v.barA ? (
          <span
            data-b6b="1"
            className={`flex items-center gap-[8px] text-[14px] font-semibold whitespace-nowrap
              ${bar.controlsLocked ? "text-faint" : "text-ink"}`}
          >
            <span data-b6lab="1">{translate("robot.workspace.messages.playFrom")}</span>
            <span role="tablist" aria-label={translate("robot.workspace.messages.playbackSource")}
              className="flex gap-[2px] p-[3px] rounded-[12px] bg-sunken">
              {arr(v.srcOpts).map((o, $i0) => (
                <Fragment key={$i0}>
                  <button type="button" role="tab" aria-selected={o.on} onClick={o.pick} disabled={o.off} title={o.tip}
                    {...sx(o.tabStyle)}>
                    <Icon name={o.ic} className={ICON_CLASS} />
                    {T(o.tab)}
                  </button>
                </Fragment>
              ))}
            </span>
            <button type="button" data-up6="1" onClick={v.send} disabled={v.sendOff} title={v.sendTip}
              {...sx(v.up6Style)}>
              {translate("robot.workspace.messages.uploadToBoard")}
            </button>
            <span data-b6br="1" aria-hidden="true" />
            {v.srcIsApp ? (
              <>
                <span data-b6lab="1">{translate("robot.workspace.messages.from")}</span>
                <RangeSelect edge="start" ariaLabel={translate("robot.workspace.messages.firstPoseToPlay")}
                  range={range} actions={actions} />
                <span data-b6lab="1">{translate("robot.workspace.messages.to")}</span>
                <RangeSelect edge="end" ariaLabel={translate("robot.workspace.messages.lastPoseToPlay")}
                  range={range} actions={actions} />
                {range.wrapOrder ? (
                  <span className="text-[13px] font-bold text-blue-ink whitespace-nowrap">
                    {translate("common.play")}{" "}
                    {T(range.wrapOrder)}
                  </span>
                ) : null}
              </>
            ) : null}
            {v.srcIsRobotTab ? (
              <span className="text-[13px] text-muted">
                <Icon name="lock" className={ICON_CLASS} />
                {" "}{translate("robot.workspace.messages.playTheFullRobotMemorySequence")}
                {bar.robotPoseCount ? " " : null}
                {T(bar.robotPoseCount)}
              </span>
            ) : null}
          </span>
        ) : null}
        {v.barC ? (
          <>
            <button type="button" onClick={() => actions.toggleMenu("source")} disabled={menu.disabled}
              aria-haspopup="menu" aria-expanded={menu.open === "source"}
              className={pickerButtonClass(menu.sourceHighlighted)}>
              <Icon name={menu.sourceIcon} className={ICON_CLASS} />
              {T(menu.sourceLabel)}
              {" ▾"}
            </button>
            <button type="button" onClick={() => actions.toggleMenu("scope")} disabled={menu.scopeDisabled}
              aria-haspopup="menu" aria-expanded={menu.open === "scope"} title={menu.scopeTip}
              className={pickerButtonClass(menu.scopeHighlighted, menu.scopeFixedToRobot)}>
              <Icon name={menu.scopeIcon} className={ICON_CLASS} />
              {T(menu.scopeLabel)}
              {menu.scopeFixedToRobot ? null : T(menu.open === "scope" ? " ▴" : " ▾")}
            </button>
          </>
        ) : null}
        {menu.open ? <PlaybackPickerMenu menu={menu} actions={actions} /> : null}
      </span>
      <PlaybackLoopControls loop={bar.loop} actions={actions} />
      <div className="[flex:1_1_0] min-w-0" />
    </div>
  );
}
