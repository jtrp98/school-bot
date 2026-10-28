import { Icon } from "../../../../components/shared/Icons";
import { t as translate } from "../../../../i18n";
import type { ReactNode } from "react";
import type { PoseCardView, PoseTileView, TileLook } from "../tilesView";
import { T } from "../tplUtil";
import type { WorkspaceViewProps } from "../viewTypes";
import { GroupCard } from "./GroupCard";

type Props = WorkspaceViewProps<
  "poseActions" | "dom" | "gRenDraft" | "noClip" | "lockOff" | "renameDraft"
> & { tile: PoseTileView };

const ICON_CLASS = "inline-block size-[1.15em] align-[-.2em] mr-[.3em]";
const LANDED_CLASS = "aw-landed [&>button]:[--rc:var(--orange-rgb)] "
  + "[&>button]:animate-[awAttn_1.2s_ease-in-out_1_both] "
  + "motion-reduce:[&>button]:animate-none "
  + "motion-reduce:[&>button]:shadow-[0_0_0_4px_rgba(var(--rc),.5)]";
const NAME_BUTTON_CLASS = "aw-name [&:hover:not(:disabled)]:bg-blue-tint! [&:hover:not(:disabled)]:text-blue-ink! "
  + "[&:hover:not(:disabled)]:border-b-blue-ink!";
const ACTION_BUTTON_CLASS = "absolute right-[6px] min-w-[30px] h-[30px] p-0 flex items-center justify-center gap-[4px] "
  + "rounded-full border-2 border-solid border-line bg-white font-[family-name:inherit] text-[12px] font-bold "
  + "whitespace-nowrap cursor-pointer z-[6] [transition:background_0.12s,color_0.12s]";
const TILE_BASE = "h-[80px] w-[172px] shrink-0 box-border py-0 pl-[10px] rounded-[14px] flex items-center gap-[8px] "
  + "font-[family-name:inherit] text-left [transition:background_0.2s,opacity_0.2s]";

const TILE_LOOK: Record<TileLook, { dashed: boolean; classes: string }> = {
  emphasis: { dashed: false, classes: "border-[3px] border-blue-ink bg-blue-tint" },
  inRange: { dashed: false, classes: "border-[3px] border-blue-ink bg-blue-tint" },
  rangeIdle: { dashed: true, classes: "border-2 border-line-strong bg-surface" },
  invalid: { dashed: false, classes: "border-[3px] border-err bg-err-tint" },
  selected: {
    dashed: false,
    classes: "border-[3px] border-blue-ink bg-blue-tint shadow-[0_0_0_3px_var(--blue-tint)]",
  },
  picked: { dashed: false, classes: "border-[3px] border-purple bg-purple-tint" },
  reorderSource: { dashed: true, classes: "border-2 border-blue-ink bg-blue-tint" },
  reorderHeld: {
    dashed: false,
    classes: "border-[3px] border-blue-ink bg-blue-tint shadow-[0_6px_14px_rgba(var(--ink-rgb),0.28)]",
  },
  reorderLanded: { dashed: false, classes: "border-[3px] border-ok bg-ok-tint" },
  reorder: { dashed: true, classes: "border-2 border-faint bg-surface" },
  normal: { dashed: false, classes: "border-2 border-line border-b-4 bg-surface" },
};

const TILE_FADE = { faded: "opacity-[0.4]", dim: "opacity-[0.45]", reorderIdle: "opacity-[0.6]" };

function tileClass(pose: PoseCardView): string {
  const look = TILE_LOOK[pose.look];
  const border = pose.cut || look.dashed ? "border-dashed" : "border-solid";
  const fade = pose.fade ? TILE_FADE[pose.fade] : "";
  const cursor = pose.look === "reorder" ? "cursor-grab" : "cursor-pointer";
  const padding = pose.reserveMoreButton ? "pr-[42px]" : "pr-[10px]";
  return `${TILE_BASE} ${padding} ${border} ${look.classes} ${fade} ${cursor}`;
}

function goButtonClass(disabled: boolean): string {
  const tone = disabled
    ? "text-line-strong bg-sunken border-off cursor-not-allowed"
    : "text-blue-ink bg-blue-tint border-blue-line border-b-4 cursor-pointer";
  return `w-[36px] h-[36px] shrink-0 box-border rounded-full flex items-center justify-center `
    + `border-2 border-solid ${tone}`;
}

function numberClass(active: boolean): string {
  return `w-[24px] h-[24px] shrink-0 rounded-full flex items-center justify-center text-[12px] font-bold ${
    active ? "bg-blue-ink text-white" : "bg-blue-tint text-ink"}`;
}

function barClass(tile: PoseTileView): string {
  const tones = { group: "bg-purple", range: "bg-blue-ink", none: "bg-transparent" };
  const tone = tones[tile.barTone];
  return `${tile.barTall ? "h-[26px]" : "h-[18px]"} shrink-0 box-border px-[10px] rounded-full mx-[2px] my-0 `
    + `text-[12px] font-bold leading-[18px] whitespace-nowrap overflow-hidden text-white `
    + `${tile.barAlignEnd ? "text-right" : "text-left"} ${tone}`;
}

export function PoseCard({ v, tile }: Props): ReactNode {
  const actions = v.poseActions;
  const pose = tile.pose;
  const index = tile.index;
  const openHeadGroup = tile.showOpenGroupHead ? tile.group : null;
  return (
    <div
      draggable={tile.draggable}
      onDragStart={(event) => actions.startTileDrag(index, event)}
      onDragOver={(event) => actions.overTile(index, event)}
      onDrop={actions.dropOnTile}
      onDragEnd={actions.endDrag}
      onContextMenu={(event) => actions.openPoseMenu(index, event)}
      className={`aw-card [&_.aw-del]:opacity-0 [&_.aw-dup]:opacity-0 [&:hover_.aw-dup]:opacity-[1]
        [&:hover_.aw-del]:opacity-[1] ${tile.landed ? LANDED_CLASS : ""} relative flex flex-col gap-[4px]`}
    >
      {tile.showGroup && tile.group ? <GroupCard v={v} tile={tile} group={tile.group} /> : null}
      {tile.showPose ? (
        <>
          <button
            type="button"
            data-pose-index={pose.number}
            onClick={() => actions.tapPose(index)}
            onMouseEnter={() => actions.hoverPose(index)}
            onMouseLeave={actions.clearHover}
            disabled={pose.tapOff}
            aria-pressed={pose.pressed}
            aria-label={pose.label}
            title={pose.tip}
            data-tile={tile.key}
            data-pk6={pose.pickedAttr}
            data-gm6={pose.groupMemberAttr}
            data-gin6={pose.dropIntoAttr}
            ref={v.dom.tileRef(tile.key)}
            className={tileClass(pose)}
          >
            {pose.selectMode ? (
              <>
                <span data-chk6={pose.pickedAttr} aria-hidden="true">✓</span>
                <span data-hdl6="1" aria-hidden="true">⠿</span>
                {pose.showGroupTag ? (
                  <span
                    data-gtag6="1"
                    title={translate("robot.workspace.messages.inGroupValue0TapToSelectTheGroup", {
                      value0: pose.groupTag,
                    })}
                  >
                    {"📦 "}
                    {T(pose.groupTag)}
                  </span>
                ) : null}
              </>
            ) : (
              <span className={numberClass(pose.numberActive)}>{T(pose.number)}</span>
            )}
            {pose.showMenuButton ? (
              <span
                role="button"
                tabIndex={0}
                data-more6="1"
                onClick={(event) => actions.openPoseMenu(index, event)}
                aria-label={translate("robot.workspace.messages.thisPoseSMenu")}
                title={translate("robot.workspace.messages.duplicateDeleteCopyRename")}
                className="w-[28px] h-[28px] box-border rounded-[8px] border-2 border-solid border-line bg-white
                  text-muted text-[15px] font-extrabold flex items-center justify-center cursor-pointer"
              >
                ⋯
              </span>
            ) : null}
            {pose.showGoButton ? (
              <span
                role="button"
                tabIndex={0}
                data-pop={pose.goAnchor}
                onClick={(event) => actions.goToPose(index, event)}
                aria-disabled={pose.goDisabled}
                aria-label={pose.goLabel}
                title={pose.goLabel}
                className={goButtonClass(pose.goDisabled)}
              >
                <svg width="16" height="16" viewBox="0 0 24 24" aria-hidden="true">
                  <path d="M7 4.5v15l12-7.5z" fill="currentColor" />
                </svg>
              </span>
            ) : null}
            <span className="flex flex-col items-start [flex:1_1_0] min-w-0">
              {pose.showRenameButton ? (
                <span
                  role="button"
                  tabIndex={0}
                  className={`${NAME_BUTTON_CLASS} text-[14px] font-bold whitespace-nowrap overflow-hidden
                    text-ellipsis max-w-full text-ink py-[1px] px-[4px] ml-[-4px] rounded-[6px] cursor-text`}
                  onClick={(event) => actions.renameTilePose(index, event)}
                  onKeyDown={(event) => actions.renameTilePoseFromKey(index, event)}
                  title={translate("robot.workspace.messages.tapToRename")}
                  aria-label={pose.renameLabel}
                >
                  {T(pose.name)}
                </span>
              ) : null}
              {pose.showPlainName ? (
                <span
                  className="text-[14px] font-bold whitespace-nowrap overflow-hidden text-ellipsis max-w-full
                    text-ink"
                >
                  {T(pose.name)}
                </span>
              ) : null}
              <span className="text-[12px] whitespace-nowrap flex items-center gap-[4px]">
                {pose.showRing ? (
                  <svg width="14" height="14" viewBox="0 0 22 22" aria-hidden="true" className="shrink-0">
                    <circle cx="11" cy="11" r="9" fill="none" stroke="var(--line-strong)" strokeWidth="3" />
                    <circle
                      cx="11"
                      cy="11"
                      r="9"
                      fill="none"
                      stroke="var(--blue-ink)"
                      strokeWidth="3"
                      strokeLinecap="round"
                      strokeDasharray={`${pose.ringDash} 57`}
                      transform="rotate(-90 11 11)"
                    />
                  </svg>
                ) : null}
                {pose.showCheck ? (
                  <svg
                    width="13"
                    height="13"
                    viewBox="0 0 24 24"
                    fill="none"
                    stroke="var(--ok)"
                    strokeWidth="3.4"
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    aria-hidden="true"
                    className="shrink-0"
                  >
                    <path d="m5 12 5 5 9-10" />
                  </svg>
                ) : null}
                <span
                  className="font-[family-name:ui-monospace,_Menlo,_Consolas,_monospace] text-[11px] font-bold
                    text-faint mr-[4px]"
                >
                  {T(pose.code)}
                </span>
              </span>
              {pose.showHandle ? (
                <svg
                  width="18"
                  height="18"
                  viewBox="0 0 24 24"
                  fill="var(--muted)"
                  aria-label={translate("robot.workspace.messages.dragToMove")}
                  className="shrink-0 [cursor:grab]"
                >
                  <circle cx="9" cy="6" r="1.6" />
                  <circle cx="15" cy="6" r="1.6" />
                  <circle cx="9" cy="12" r="1.6" />
                  <circle cx="15" cy="12" r="1.6" />
                  <circle cx="9" cy="18" r="1.6" />
                  <circle cx="15" cy="18" r="1.6" />
                </svg>
              ) : null}
            </span>
          </button>
          {pose.showActions ? (
            <>
              <button
                type="button"
                className={`aw-dup [&.aw-del-on]:opacity-[1] focus-visible:opacity-[1] [&_.aw-dup-lbl]:hidden
                  [&:hover:not(:disabled)]:bg-blue-ink! [&:hover:not(:disabled)]:border-blue-ink!
                  [&:hover:not(:disabled)]:text-white! focus-visible:bg-blue-ink! focus-visible:border-blue-ink!
                  focus-visible:text-white! ${pose.selected ? "aw-del-on" : ""} ${ACTION_BUTTON_CLASS} top-[44px]
                  text-blue-ink`}
                onClick={(event) => actions.duplicateTilePose(index, event)}
                disabled={v.lockOff}
                aria-label={pose.duplicateLabel}
                title={pose.duplicateLabel}
              >
                <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.4"
                  strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
                  <rect x="8" y="8" width="12" height="12" rx="2" />
                  <path d="M16 8V5a1 1 0 0 0-1-1H5a1 1 0 0 0-1 1v10a1 1 0 0 0 1 1h3" />
                </svg>
                <span className="aw-dup-lbl">{translate("robot.workspace.messages.duplicate")}</span>
              </button>
              <button
                type="button"
                className={`aw-del [&.aw-del-on]:opacity-[1] focus-visible:opacity-[1] [&_.aw-del-lbl]:hidden
                  [&:hover:not(:disabled)]:bg-err! [&:hover:not(:disabled)]:border-err!
                  [&:hover:not(:disabled)]:text-white! focus-visible:bg-err! focus-visible:border-err!
                  focus-visible:text-white! ${pose.selected ? "aw-del-on" : ""} ${ACTION_BUTTON_CLASS} top-[6px]
                  text-muted`}
                onClick={(event) => actions.deleteTilePose(index, event)}
                disabled={v.lockOff}
                aria-label={pose.deleteLabel}
                title={pose.deleteLabel}
              >
                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="3"
                  strokeLinecap="round" aria-hidden="true" className="shrink-0">
                  <path d="M6 6l12 12M18 6 6 18" />
                </svg>
                <span className="aw-del-lbl">{translate("common.messages.deleteThisPose")}</span>
              </button>
            </>
          ) : null}
          {pose.renaming ? (
            <input
              type="text"
              className="aw-edit cursor-text [&:hover:not(:disabled)]:bg-blue-tint!
                [&:hover:not(:disabled)]:border-blue-ink! focus:outline-none focus:border-solid!
                focus:border-blue-ink! focus:bg-white! focus:shadow-[0_0_0_3px_var(--blue-tint)]
                absolute left-[80px] right-[8px] top-[10px] h-[30px] z-[8] box-border border-2
                border-solid border-blue-ink rounded-[8px] py-0 px-[8px] font-[family-name:inherit]
                text-[14px] font-bold text-ink bg-white"
              value={v.renameDraft ?? ""}
              onChange={actions.editRenameDraft}
              onKeyDown={actions.keyOnRenameDraft}
              onBlur={actions.commitRename}
              autoFocus={true}
              maxLength={24}
              aria-label={pose.renameInputLabel}
            />
          ) : null}
        </>
      ) : null}
      {tile.showBar ? (
        <div data-gbar6={tile.barPosition} className={barClass(tile)}>
          {T(tile.barText)}
        </div>
      ) : null}
      {openHeadGroup ? (
        <div
          data-gbar6="first"
          className="h-[26px] shrink-0 box-border my-0 mx-[2px] pt-0 pr-[3px] pb-0 pl-[10px] rounded-full
            bg-purple text-white flex items-center gap-[4px]"
        >
          <span className="[flex:1_1_0] min-w-0 text-[12px] font-bold whitespace-nowrap overflow-hidden text-ellipsis">
            <Icon name="box" className={ICON_CLASS} />
            {" "}
            {T(openHeadGroup.name)}
          </span>
          <button
            type="button"
            className="aw-gbar-btn hover:bg-white! hover:text-purple-ink! h-[20px] py-0 px-[8px] border-none
              border-current rounded-full bg-[rgba(var(--surface-rgb),0.22)] text-white
              font-[family-name:inherit] text-[12px] font-bold cursor-pointer whitespace-nowrap"
            onClick={(event) => actions.toggleGroupFromCard(openHeadGroup.id, event)}
            aria-label={openHeadGroup.toggleLabel}
          >
            {translate("robot.workspace.messages.hide")}
          </button>
          <button
            type="button"
            className="aw-gbar-btn hover:bg-white! hover:text-purple-ink! h-[20px] py-0 px-[8px] border-none
              border-current rounded-full bg-[rgba(var(--surface-rgb),0.22)] text-white
              font-[family-name:inherit] text-[12px] font-bold cursor-pointer whitespace-nowrap"
            onClick={(event) => actions.ungroupFromCard(openHeadGroup.id, event)}
            aria-label={openHeadGroup.ungroupLabel}
          >
            <Icon name="cut" className={ICON_CLASS} />
            {" "}{translate("robot.workspace.messages.ungroup")}
          </button>
        </div>
      ) : null}
    </div>
  );
}
