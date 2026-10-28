import { Icon } from "../../../../components/shared/Icons";
import { IconBtn } from "../../../../components/shared/IconBtn";
import { t as translate } from "../../../../i18n";
import type { ReactNode } from "react";
import { T } from "../tplUtil";
import type { WorkspaceViewProps } from "../viewTypes";
import { ManagePosesBar } from "./ManagePosesBar";
import { TableToggleButton } from "./TableToggleButton";
import { ToolbarButton } from "./ToolbarButton";

type Props = WorkspaceViewProps<
  | "toolbar" | "toolbarActions" | "playbackActions" | "addPose" | "addOff" | "redoPick" | "redoOff" | "redoTip"
  | "redoLabel" | "relEnd"
>;

const SEGMENT_CLASS = [
  "inline-flex h-[34px] cursor-pointer items-center justify-center rounded-[9px] border-none px-[12px]",
  "font-[family-name:inherit] text-[14px] leading-[1.2] font-bold whitespace-nowrap",
].join(" ");

const SEGMENT_GROUP_CLASS = "gap-[4px] rounded-[12px] bg-blue-tint p-[4px]";

function segmentTone(selected: boolean): string {
  return selected ? "bg-surface text-blue-ink shadow-[0_1px_0_var(--line-strong)]" : "bg-transparent text-muted";
}

const RANGE_TIP_KEYS = [
  "robot.workspace.messages.howToChooseAPlaybackRange",
  "robot.workspace.messages.tapStartPoseTapEndPose",
  "robot.workspace.messages.playbackMovesRightFromStartToEnd",
  "robot.workspace.messages.ifTheEndIsBeforeTheStartPlaybackWraps",
  "robot.workspace.messages.tapTheSamePoseTwiceToPlayOnlyThat",
  "robot.workspace.messages.theRangeStaysSelectedWhileYouEditPosesPress",
] as const;

export function SequenceToolbar({ v }: Props): ReactNode {
  const { toolbar } = v;
  const actions = v.toolbarActions;
  const { clipboard, rangeChip, manageControl } = toolbar;
  return (
    <div className="flex min-h-[44px] items-center gap-[8px] [flex-wrap:nowrap]">
      {toolbar.normalMode ? (
        <>
          <span className="font-[family-name:'Mitr',_'Noto_Sans_Thai',_sans-serif] text-[18px] font-medium whitespace-nowrap">
            {translate("robot.workspace.sequence.title")}
          </span>
          <div role="group" aria-label={translate("common.view")} className={`grid grid-cols-2 ${SEGMENT_GROUP_CLASS}`}>
            <button
              type="button"
              onClick={actions.showCards}
              aria-pressed={toolbar.viewMode === "cards"}
              className={`${SEGMENT_CLASS} ${segmentTone(toolbar.viewMode === "cards")}`}
            >
              {translate("robot.workspace.messages.cards")}
            </button>
            <button
              type="button"
              onClick={actions.showTable}
              disabled={toolbar.tableViewDisabled}
              aria-pressed={toolbar.viewMode === "table"}
              className={`${SEGMENT_CLASS} ${segmentTone(toolbar.viewMode === "table")}`}
            >
              {translate("robot.workspace.messages.table")}
            </button>
          </div>
          <div className="h-[28px] w-[2px] bg-line-strong" />
          {toolbar.modeOptions ? (
            <div
              role="group"
              aria-label={translate("robot.workspace.messages.sequenceMode")}
              className={`grid grid-cols-3 ${SEGMENT_GROUP_CLASS}`}
            >
              {toolbar.modeOptions.map((mode) => (
                <button
                  key={mode.value}
                  type="button"
                  onClick={() => actions.pickSequenceMode(mode.value)}
                  disabled={mode.disabled}
                  aria-pressed={mode.selected}
                  className={`${SEGMENT_CLASS} ${segmentTone(mode.selected)}`}
                >
                  {T(mode.label)}
                </button>
              ))}
            </div>
          ) : null}
          {clipboard ? (
            <span className="flex items-center gap-[6px]">
              <span
                role="status"
                className={[
                  "box-border flex h-[40px] items-center gap-[6px] rounded-[12px] border-2 border-solid border-blue-line",
                  "bg-blue-tint pr-[4px] pl-[12px]",
                ].join(" ")}
              >
                <span className="text-[13px] font-bold whitespace-nowrap text-blue-press">
                  <Icon name={clipboard.icon} className="mr-[.3em] inline-block size-[1.15em] align-[-.2em]" />
                  {T(clipboard.text)}
                </span>
                <ToolbarButton
                  variant="secondary"
                  height={30}
                  title="Esc"
                  aria-label={clipboard.doneLabel}
                  onClick={actions.finishClipboardMode}
                  className="text-muted!"
                >
                  {T(clipboard.doneLabel)}
                </ToolbarButton>
              </span>
            </span>
          ) : null}
          {rangeChip ? (
            <span
              className={[
                "box-border flex h-[40px] items-center gap-[4px] rounded-[12px] border-2 border-solid border-blue-line",
                "bg-blue-tint pr-[4px] pl-[10px]",
              ].join(" ")}
            >
              <span
                className={[
                  "aw-tipwrap relative flex [cursor:help] items-center gap-[6px] text-[13px] font-bold whitespace-nowrap",
                  "text-blue-press [&:focus-within_.aw-tip]:block [&:hover_.aw-tip]:block",
                ].join(" ")}
                tabIndex={0}
                aria-describedby="range-tip-v4b"
              >
                {T(rangeChip.text)}
                <svg
                  width="16"
                  height="16"
                  viewBox="0 0 24 24"
                  fill="none"
                  stroke="currentColor"
                  strokeWidth="2.2"
                  strokeLinecap="round"
                  aria-hidden="true"
                >
                  <circle cx="12" cy="12" r="9" />
                  <path d="M12 11v6M12 7.5v.5" />
                </svg>
                <span
                  id="range-tip-v4b"
                  role="tooltip"
                  className={[
                    "aw-tip absolute bottom-[calc(100%_+_10px)] left-[-10px] z-[40] box-border hidden w-[300px] rounded-[8px]",
                    "bg-ink px-[12px] py-[10px] text-[13px] leading-[1.55] font-semibold text-white [white-space:normal]",
                  ].join(" ")}
                >
                  {RANGE_TIP_KEYS.map((key, index) => (
                    <span key={key}>
                      {index > 0 ? <br /> : null}
                      {translate(key)}
                    </span>
                  ))}
                </span>
              </span>
              {rangeChip.canSaveAsGroup ? (
                <ToolbarButton
                  variant="blue"
                  height={30}
                  icon="save"
                  title={translate("robot.workspace.messages.saveThisRangeAsAGroupThenChooseIt")}
                  onClick={actions.saveRangeAsGroup}
                  className="bg-purple! border-purple! border-b-purple-ink!"
                >
                  {translate("robot.workspace.messages.saveAsPoseGroup")}
                </ToolbarButton>
              ) : null}
              <ToolbarButton
                variant="secondary"
                height={30}
                onClick={v.redoPick}
                disabled={v.redoOff}
                title={v.redoTip}
                className="border-blue-line! text-blue-ink!"
              >
                {"↺ "}
                {T(v.redoLabel)}
              </ToolbarButton>
              <IconBtn
                flat
                icon="x"
                size={16}
                label={rangeChip.cancelLabel}
                onClick={v.playbackActions.rangeCancel}
                className="h-[30px]! w-[30px]! min-w-0! shrink-0 rounded-full! text-muted!"
              />
            </span>
          ) : null}
          <div className="grow" />
          {manageControl ? (
            <span className="aw-tipwrap relative inline-flex [&:focus-within_.aw-tip]:block [&:hover_.aw-tip]:block">
              <ToolbarButton
                variant={manageControl.pressed ? "blue" : "secondary"}
                height={40}
                data-sel6ok="1"
                onClick={actions.startManagingPoses}
                disabled={manageControl.disabled}
                aria-pressed={manageControl.pressed}
                aria-describedby="order-tip-v4b"
              >
                {translate("robot.workspace.messages.managePoses")}
              </ToolbarButton>
              <span
                id="order-tip-v4b"
                role="tooltip"
                className={[
                  "aw-tip absolute right-0 bottom-[calc(100%_+_8px)] z-[40] box-border hidden w-[240px] rounded-[8px]",
                  "bg-ink px-[12px] py-[8px] text-[13px] leading-[1.5] font-semibold text-white shadow-pop",
                ].join(" ")}
              >
                {T(manageControl.tip)}
              </span>
            </span>
          ) : null}
          <ToolbarButton variant="blue" icon="plus" data-pop="add" onClick={v.addPose} disabled={v.addOff}>
            {translate("common.messages.addPose")}
          </ToolbarButton>
          {toolbar.tableToggle.visible ? (
            <TableToggleButton toggle={toolbar.tableToggle} onToggle={actions.toggleTableFullscreen} describeState />
          ) : null}
        </>
      ) : null}
      <ManagePosesBar v={v} />
    </div>
  );
}
