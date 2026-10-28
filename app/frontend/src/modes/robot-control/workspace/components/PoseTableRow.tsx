import { Icon } from "../../../../components/shared/Icons";
import { t as translate } from "../../../../i18n";
import { NumericInput } from "../../../../components/robot-control/NumericInput";
import { Fragment, type ReactNode } from "react";
import type { RowFade, RowFill, RowEdge, RowNumberTone, PoseRowView } from "../rowsView";
import { T, arr } from "../tplUtil";
import type { WorkspaceViewProps } from "../viewTypes";

type Props = WorkspaceViewProps<
  "poseActions" | "jointActions" | "dom" | "isSame" | "isSplit" | "lockOff" | "isOrderMode" | "notOrderMode"
  | "gRenDraft" | "pClipN" | "relAll" | "relAllOff" | "relNew"
> & { row: PoseRowView };

const GRID_COLUMNS = {
  same: "[grid-template-columns:40px_64px_108px_repeat(6,minmax(44px,1fr))_76px_36px_100px]",
  split: "[grid-template-columns:40px_64px_108px_repeat(6,minmax(44px,1fr))_36px_100px]",
};
const MONO_FONT = "font-[family-name:ui-monospace,_Menlo,_Consolas,_monospace]";
const PLAY_ICON = "M7 4.5v15l12-7.5z";
const NAME_INPUT_CLASS = "aw-edit cursor-text [&:hover:not(:disabled)]:bg-blue-tint! "
  + "[&:hover:not(:disabled)]:border-blue-ink! focus:outline-none focus:border-solid! focus:border-blue-ink! "
  + "focus:bg-white! focus:shadow-[0_0_0_3px_var(--blue-tint)]";

const FILL: Record<RowFill, string> = {
  surface: "bg-surface",
  blueTint: "bg-blue-tint",
  okTint: "bg-ok-tint",
  purpleTint: "bg-purple-tint",
};
const EDGE: Record<RowEdge, string> = {
  none: "",
  purple: "shadow-[inset_4px_0_0_var(--purple)]",
  blue: "shadow-[inset_4px_0_0_var(--blue-ink)]",
  ok: "shadow-[inset_4px_0_0_var(--ok)]",
  dropTop: "shadow-[inset_0_4px_0_var(--blue-ink)]",
  dropBottom: "shadow-[inset_0_-4px_0_var(--blue-ink)]",
};
const FADE: Record<NonNullable<RowFade>, string> = {
  dragging: "opacity-[0.4]",
  cut: "opacity-[0.45]",
  dim: "opacity-[0.5]",
};
const CURSOR = { grab: "cursor-grab", pointer: "cursor-pointer", default: "" };
const NUMBER_TONE: Record<RowNumberTone, string> = {
  picked: "border-purple bg-purple text-white",
  pickable: "border-purple bg-surface text-purple",
  active: "border-blue-ink bg-blue-ink text-white",
  idle: "border-line-strong bg-surface text-ink",
};
const GROUP_OUTLINE = {
  purple: "outline-[3px] outline-solid outline-purple -outline-offset-3",
  blue: "outline-[3px] outline-solid outline-blue-ink -outline-offset-3",
};
const ROW_ANIMATION = {
  in: "aw-rin animate-[awRin_.26s_cubic-bezier(.2,.8,.2,1)_both] overflow-hidden motion-reduce:animate-none",
  out: "aw-rout animate-[awRout_.2s_ease-in_both] overflow-hidden pointer-events-none motion-reduce:animate-none",
};

export function gridClass(isSame: boolean): string {
  return `grid gap-[4px] items-center py-[6px] px-[10px] box-border ${isSame ? GRID_COLUMNS.same : GRID_COLUMNS.split}`;
}

function rowClass(row: PoseRowView, isSplit: boolean): string {
  const animation = row.groupAnimation ? ROW_ANIMATION[row.groupAnimation] : "";
  const height = isSplit ? "min-h-[84px]" : "min-h-[48px]";
  return `${animation} ${row.landed ? "aw-rland" : ""} ${gridClass(!isSplit)} ${height} `
    + `border-b [border-bottom-style:solid] border-b-line-strong ${FILL[row.fill]} ${EDGE[row.edge]} `
    + `${row.fade ? FADE[row.fade] : ""} ${CURSOR[row.cursor]}`;
}

function moveButtonClass(disabled: boolean): string {
  const tone = disabled
    ? "border-off bg-sunken text-faint cursor-not-allowed"
    : "border-blue-line border-b-[3px] bg-surface text-blue-ink cursor-pointer";
  return `inline-flex items-center justify-center w-[34px] h-[32px] p-0 rounded-[9px] font-[family-name:inherit] `
    + `text-[13px] font-bold border-2 border-solid ${tone}`;
}

function goButtonClass(disabled: boolean): string {
  const tone = disabled
    ? "text-line-strong bg-sunken border-off cursor-not-allowed"
    : "text-blue-ink bg-blue-tint border-blue-line border-b-4 cursor-pointer";
  return `w-[32px] h-[32px] box-border rounded-full flex items-center justify-center border-2 border-solid ${tone}`;
}

function cellClass(invalid: boolean): string {
  const tone = invalid ? "border-err bg-err-tint font-bold" : "border-line-strong bg-surface font-semibold";
  return `h-[34px] w-full min-w-0 box-border rounded-[8px] py-0 px-[4px] font-[family-name:inherit] text-[15px] `
    + `text-center [font-variant-numeric:tabular-nums] text-ink border-2 border-solid ${tone}`;
}

function durationClass(invalid: boolean, height: string): string {
  const tone = invalid ? "border-err bg-err-tint font-bold" : "border-line-strong bg-surface";
  return `${height} w-full min-w-0 box-border rounded-[8px] py-0 px-[4px] font-[family-name:inherit] text-[13px] `
    + `text-center [font-variant-numeric:tabular-nums] text-ink border-2 border-solid ${tone}`;
}

function releaseReadClass(differs: boolean): string {
  const tone = differs
    ? "border-b-2 [border-bottom-style:solid] border-b-orange-press bg-orange text-white cursor-pointer"
    : "bg-sunken text-faint cursor-default";
  return `h-[22px] py-0 px-[8px] rounded-[7px] font-[family-name:inherit] text-[11px] font-bold border-none `
    + `border-current ${tone}`;
}

function PlayIcon({ size }: { size: number }): ReactNode {
  return (
    <svg width={size} height={size} viewBox="0 0 24 24" aria-hidden="true">
      <path d={PLAY_ICON} fill="currentColor" />
    </svg>
  );
}

export function PoseTableRow({ v, row }: Props): ReactNode {
  const actions = v.poseActions;
  const index = row.index;
  const group = row.group;
  return (
    <Fragment>
      {row.showGroupHead && group ? (
        <div
          onClick={(event) => actions.tapGroupRow(group.id, event)}
          onMouseEnter={() => actions.hoverGroup(group.id)}
          onMouseLeave={actions.clearHover}
          onContextMenu={(event) => actions.openGroupMenu(group.id, event)}
          draggable={group.dragAttr}
          onDragStart={(event) => actions.startGroupRowDrag(index, group.id, event)}
          onDragOver={(event) => actions.overGroupRow(index, group.id, event)}
          onDrop={actions.dropOnGroupRow}
          onDragEnd={actions.endDrag}
          data-gin6={group.dropInto}
          role="row"
          aria-label={group.label}
          className={`flex items-center gap-[8px] min-h-[46px] cursor-pointer py-[4px] px-[10px] box-border border-b
            [border-bottom-style:solid] border-b-purple-line shadow-[inset_4px_0_0_var(--purple)] bg-purple-tint
            ${group.outline ? GROUP_OUTLINE[group.outline] : ""}`}
        >
          <button
            type="button"
            onClick={() => actions.foldGroup(group.id)}
            disabled={group.foldDisabled}
            aria-label={group.foldLabel}
            title={group.foldLabel}
            className="w-[38px] h-[32px] shrink-0 p-0 border-2 border-solid border-purple-line rounded-[9px]
              bg-white text-purple-ink font-[family-name:inherit] text-[14px] font-bold cursor-pointer"
          >
            {T(group.foldIcon)}
          </button>
          <span className={`text-[13px] font-bold text-purple-ink ${MONO_FONT}`}>{T(group.code)}</span>
          {group.renaming ? (
            <>
              <input
                type="text"
                className={`${NAME_INPUT_CLASS} w-[200px] h-[32px] box-border py-0 px-[8px] border-2 border-solid
                  border-purple rounded-[8px] font-[family-name:inherit] text-[14px] font-bold text-ink`}
                value={v.gRenDraft ?? ""}
                onChange={actions.editGroupNameDraft}
                onKeyDown={(event) => actions.keyOnGroupName(group.id, event)}
                autoFocus={true}
                maxLength={20}
                aria-label={translate("robot.workspace.messages.newPoseGroupName")}
              />
              <button
                type="button"
                onClick={() => actions.commitGroupName(group.id)}
                aria-label={translate("robot.workspace.messages.savePoseGroupName")}
                className="h-[32px] py-0 px-[10px] border-none border-current border-b-[3px]
                  [border-bottom-style:solid] border-b-purple-ink rounded-[8px] bg-purple text-white
                  font-[family-name:inherit] text-[13px] font-bold cursor-pointer"
              >
                ✓
              </button>
            </>
          ) : (
            <button
              type="button"
              className="aw-name [&:hover:not(:disabled)]:bg-blue-tint! [&:hover:not(:disabled)]:text-blue-ink!
                [&:hover:not(:disabled)]:border-b-blue-ink! h-[34px] min-w-[160px] max-w-[280px]
                box-border py-0 px-[8px] border-2 border-solid border-transparent rounded-[8px]
                bg-transparent font-[family-name:inherit] text-[15px] font-bold text-purple-ink
                text-left cursor-text whitespace-nowrap overflow-hidden text-ellipsis"
              onClick={(event) => actions.startGroupRowRename(group.id, event)}
              title={translate("robot.workspace.messages.tapToRename")}
            >
              <Icon name="box" className="inline-block size-[1.15em] align-[-.2em] mr-[.3em]" />
              {" "}
              {T(group.name)}
            </button>
          )}
          <span className="text-[13px] text-purple-ink">{T("")}</span>
          <span className="grow" />
          <button
            type="button"
            onClick={() => actions.playGroup(group.id)}
            disabled={group.playOff}
            aria-label={group.playLabel}
            title={group.playLabel}
            className="w-[32px] h-[32px] box-border p-0 rounded-full flex items-center justify-center
              text-purple-ink bg-purple-tint border-2 border-solid border-purple-line border-b-4
              [border-bottom-style:solid] border-b-purple-line cursor-pointer"
          >
            <PlayIcon size={14} />
          </button>
          <button
            type="button"
            className="aw-gbtn [&:hover:not(:disabled)]:bg-purple-tint! [&:hover:not(:disabled)]:border-purple!
              w-[36px] h-[32px] p-0 border-2 border-solid border-purple-line rounded-[8px] bg-white
              text-purple-ink font-[family-name:inherit] text-[16px] font-bold cursor-pointer"
            onClick={(event) => actions.openGroupMenu(group.id, event)}
            disabled={group.foldDisabled}
            aria-label={translate("robot.workspace.messages.managePoseGroup")}
            title={translate("robot.workspace.messages.managePoseGroupOrRightClick")}
          >
            ⋯
          </button>
        </div>
      ) : null}
      {row.visible ? (
        <div
          role="row"
          className={rowClass(row, v.isSplit)}
          style={row.animationDelayMs === null ? undefined : { animationDelay: `${row.animationDelayMs}ms` }}
          draggable={row.draggable}
          onClick={(event) => actions.tapRow(index, event)}
          data-rsel6={row.selectedAttr}
          onMouseEnter={() => actions.hoverPose(index)}
          onMouseLeave={actions.clearHover}
          onDragStart={(event) => actions.startRowDrag(index, event)}
          onDragOver={(event) => actions.overRow(index, event)}
          onDrop={actions.dropOnRow}
          onDragEnd={actions.endDrag}
          onContextMenu={(event) => actions.openPoseMenu(index, event)}
        >
          <span role="cell" className="flex items-center gap-[2px]">
            {v.isOrderMode ? (
              <span
                aria-hidden="true"
                title={translate("robot.workspace.messages.dragToMove")}
                className="text-[18px] leading-none text-muted [cursor:grab] ml-[-6px]"
              >
                ⠿
              </span>
            ) : null}
            <button
              type="button"
              onClick={() => actions.tapPose(index)}
              disabled={row.tapOff}
              aria-pressed={row.pressed}
              aria-label={row.label}
              className={`w-[38px] h-[32px] rounded-[9px] font-[family-name:inherit] text-[13px] font-bold
                cursor-pointer border-2 border-solid ${NUMBER_TONE[row.numberTone]}`}
            >
              {T(row.numberText)}
            </button>
          </span>
          <span role="cell" className="flex flex-col gap-[2px] min-w-0">
            <span className={`text-[13px] font-bold [font-variant-numeric:tabular-nums] ${MONO_FONT} text-blue-press`}>
              {T(row.code)}
            </span>
          </span>
          <span role="cell">
            <input
              type="text"
              className={`${NAME_INPUT_CLASS} h-[34px] w-full box-border border-2 border-solid border-transparent
                rounded-[9px] py-0 px-[8px] font-[family-name:inherit] text-[14px] font-bold text-ink bg-white`}
              data-tile={row.key}
              ref={v.dom.tileRef(row.key)}
              value={row.name ?? ""}
              onChange={(event) => actions.changeName(index, event)}
              disabled={v.lockOff}
              aria-label={row.nameLabel}
              maxLength={24}
            />
          </span>
          {arr(row.cells).map((cell) => (
            <span key={cell.joint} role="cell" className="flex flex-col gap-[3px] min-w-0">
              <NumericInput
                value={cell.value ?? ""}
                onChange={(event) => actions.changeCell(index, cell.joint, event)}
                disabled={v.lockOff}
                ref={v.dom.cellRef(row.key, cell.joint)}
                onKeyDown={(event) => actions.keyOnCell(index, cell.joint, event)}
                onFocus={actions.showCellHint}
                aria-label={cell.label}
                aria-invalid={cell.invalid}
                className={cellClass(cell.invalid)}
              />
              {v.isSplit ? (
                <NumericInput
                  value={cell.duration ?? ""}
                  onChange={(event) => actions.changeCellDuration(index, cell.joint, event)}
                  disabled={v.lockOff}
                  aria-label={cell.durationLabel}
                  aria-invalid={cell.durationInvalid}
                  className={durationClass(cell.durationInvalid, "h-[30px]")}
                />
              ) : null}
            </span>
          ))}
          {v.isSame ? (
            <span role="cell">
              <NumericInput
                value={row.duration ?? ""}
                onChange={(event) => actions.changeDuration(index, event)}
                disabled={v.lockOff}
                aria-label={row.durationLabel}
                aria-invalid={row.durationInvalid}
                className={durationClass(row.durationInvalid, "h-[34px]")}
              />
            </span>
          ) : null}
          <span role="cell" className="flex justify-center">
            {row.showGoButton ? (
              <span
                role="button"
                tabIndex={0}
                data-pop={row.goAnchor}
                onClick={(event) => actions.goToPose(index, event)}
                onKeyDown={(event) => actions.goToPoseFromKey(index, event)}
                aria-disabled={row.goDisabled}
                aria-label={row.goLabel}
                title={row.goLabel}
                className={goButtonClass(row.goDisabled)}
              >
                <PlayIcon size={14} />
              </span>
            ) : null}
          </span>
          <span role="cell" className="flex gap-[6px] justify-end">
            {v.isOrderMode ? (
              <span className="flex gap-[6px] items-center">
                <button
                  type="button"
                  onClick={() => actions.moveRowUp(index)}
                  disabled={row.upDisabled}
                  aria-label={row.upLabel}
                  title={row.upLabel}
                  className={moveButtonClass(row.upDisabled)}
                >
                  ▲
                </button>
                <button
                  type="button"
                  onClick={() => actions.moveRowDown(index)}
                  disabled={row.downDisabled}
                  aria-label={row.downLabel}
                  title={row.downLabel}
                  className={moveButtonClass(row.downDisabled)}
                >
                  ▼
                </button>
              </span>
            ) : null}
            {v.notOrderMode ? (
              <span className="flex gap-[6px]">
                {row.showDuplicate ? (
                  <button
                    type="button"
                    onClick={() => actions.duplicateRowPose(index)}
                    disabled={v.lockOff}
                    aria-label={row.duplicateLabel}
                    className="inline-flex items-center justify-center leading-[1.2] h-[32px] py-0 px-[8px]
                      border-2 border-solid border-line rounded-[9px] bg-white text-blue-ink
                      font-[family-name:inherit] text-[12px] font-bold cursor-pointer"
                  >
                    {translate("robot.workspace.messages.duplicate")}
                  </button>
                ) : null}
                <button
                  type="button"
                  onClick={() => actions.deleteRowPose(index)}
                  disabled={v.lockOff}
                  aria-label={row.removeLabel}
                  className="inline-flex items-center justify-center leading-[1.2] w-[32px] h-[32px] p-0 border-2
                    border-solid border-line rounded-[9px] bg-white text-muted
                    font-[family-name:inherit] text-[16px] cursor-pointer"
                >
                  ×
                </button>
              </span>
            ) : null}
          </span>
        </div>
      ) : null}
      {row.releaseRow ? (
        <div
          role="row"
          className={`${gridClass(!v.isSplit)} min-h-[60px] bg-orange-tint border-b-2 [border-bottom-style:solid]
            border-b-orange-line shadow-[inset_4px_0_0_var(--orange)]`}
        >
          <span
            role="cell"
            className="[grid-column:span_3] flex items-center gap-[6px] text-orange-deep text-[12px] font-bold
              pl-[4px]"
          >
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.2"
              strokeLinecap="round" strokeLinejoin="round" aria-hidden="true" className="shrink-0">
              <path d="M18 11V6a2 2 0 0 0-4 0v5" />
              <path d="M14 10V4a2 2 0 0 0-4 0v6" />
              <path d="M10 10.5V6a2 2 0 0 0-4 0v8" />
              <path
                d="M18 8a2 2 0 1 1 4 0v6a8 8 0 0 1-8 8h-2c-2.8 0-4.5-.86-5.99-2.34l-3.6-3.6a2 2 0 0 1 2.83-2.82L7 15"
              />
            </svg>
            {translate("common.messages.currentRobot")}
          </span>
          {row.releaseRow.map((cell, joint) => (
            <span key={joint} role="cell" className="flex flex-col items-center gap-[3px]">
              <b className={`text-[13px] ${cell.differs ? "text-orange-deep" : "text-faint"}`}>{T(cell.text)}</b>
              <button
                type="button"
                onClick={(event) => v.jointActions.readJoint(joint, event)}
                disabled={!cell.differs}
                className={releaseReadClass(cell.differs)}
              >
                {translate("robot.workspace.messages.read")}
              </button>
            </span>
          ))}
          <span role="cell" className="[grid-column:10_/_-1] flex flex-col gap-[4px] items-end">
            <button
              type="button"
              onClick={v.relAll}
              disabled={v.relAllOff}
              className="h-[28px] py-0 px-[8px] border-none border-current border-b-[3px]
                [border-bottom-style:solid] border-b-orange-press rounded-[8px] bg-orange text-white
                font-[family-name:inherit] text-[12px] font-bold cursor-pointer whitespace-nowrap"
            >
              {translate("robot.workspace.messages.readAllJoints")}
            </button>
            <button
              type="button"
              onClick={v.relNew}
              className="h-[28px] py-0 px-[8px] border-2 border-solid border-orange border-b-[3px] rounded-[8px]
                bg-white text-orange-deep font-[family-name:inherit] text-[12px] font-bold
                cursor-pointer whitespace-nowrap"
            >
              {translate("robot.workspace.messages.newPose")}
            </button>
          </span>
        </div>
      ) : null}
      {row.showPasteBelow ? (
        <div className="py-[2px] px-[10px]">
          <button
            type="button"
            onClick={() => actions.pasteAfterRow(index)}
            aria-label={translate("robot.workspace.messages.pasteHereValue0Poses", { value0: v.pClipN ?? "" })}
            className="h-[30px] w-full box-border border-2 border-dashed border-blue-ink rounded-[8px]
              bg-blue-tint text-blue-ink font-[family-name:inherit] text-[13px] font-bold cursor-pointer"
          >
            {translate("robot.workspace.messages.pasteHere")}
            {T(v.pClipN)}
            {" "}{translate("robot.workspace.messages.poses")}
          </button>
        </div>
      ) : null}
    </Fragment>
  );
}
