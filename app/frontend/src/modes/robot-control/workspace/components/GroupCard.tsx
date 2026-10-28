import { Icon } from "../../../../components/shared/Icons";
import { t as translate } from "../../../../i18n";
import type { ReactNode } from "react";
import type { GroupCardView, PoseTileView } from "../tilesView";
import { T } from "../tplUtil";
import type { WorkspaceViewProps } from "../viewTypes";

type Props = WorkspaceViewProps<"poseActions" | "gRenDraft" | "noClip"> & { tile: PoseTileView; group: GroupCardView };

const ICON_CLASS = "inline-block size-[1.15em] align-[-.2em] mr-[.3em]";
const GROUP_POP_CLASS = "aw-gpop animate-[awGpop_.24s_cubic-bezier(.2,.8,.2,1)_both] motion-reduce:animate-none";
const OUTLINE_CLASS = {
  purple: "outline-[3px] outline-solid outline-purple outline-offset-2",
  blue: "outline-[3px] outline-solid outline-blue-ink outline-offset-2",
};
const MENU_BUTTON_CLASS = "aw-gbtn [&:hover:not(:disabled)]:bg-purple-tint! [&:hover:not(:disabled)]:border-purple! "
  + "[flex:1_1_0] h-[40px] py-0 border-2 border-solid rounded-[10px] bg-white font-[family-name:inherit] "
  + "font-bold cursor-pointer whitespace-nowrap";
const NAME_INPUT_CLASS = "aw-edit cursor-text [&:hover:not(:disabled)]:bg-blue-tint! "
  + "[&:hover:not(:disabled)]:border-blue-ink! focus:outline-none focus:border-solid! focus:border-blue-ink! "
  + "focus:bg-white! focus:shadow-[0_0_0_3px_var(--blue-tint)] [flex:1_1_0] min-w-0 h-[36px] box-border py-0 "
  + "px-[8px] border-2 border-solid border-purple rounded-[9px] font-[family-name:inherit] text-[14px] font-bold "
  + "text-ink";

function cardClass(group: GroupCardView, popped: boolean): string {
  const outline = group.outline ? OUTLINE_CLASS[group.outline] : "";
  return `aw-gcard hover:shadow-pop ${popped ? GROUP_POP_CLASS : ""} relative h-[80px] w-[204px] shrink-0 box-border `
    + `py-0 px-[10px] rounded-[14px] flex items-center gap-[8px] border-[3px] border-solid `
    + `border-purple bg-purple-tint `
    + `shadow-[4px_4px_0_var(--purple-line)] ${group.pointer ? "cursor-pointer" : ""} ${outline}`;
}

export function GroupCard({ v, tile, group }: Props): ReactNode {
  const actions = v.poseActions;
  const pose = tile.pose;
  return (
    <div
      className={cardClass(group, tile.popGroup)}
      data-gst6={group.state}
      data-gpk6={group.allPicked}
      data-gin6={group.dropInto}
      onContextMenu={(event) => actions.openGroupMenu(group.id, event)}
      onClick={() => actions.tapGroupCard(group.id)}
      onMouseEnter={() => actions.hoverGroup(group.id)}
      onMouseLeave={actions.clearHover}
    >
      {pose.selectMode ? (
        <span data-gtop6="1">
          <span data-chk6={group.allPicked} aria-hidden="true">✓</span>
          <span data-hdl6="1" aria-hidden="true">⠿</span>
        </span>
      ) : null}
      <div data-gr1="1">
        <button
          type="button"
          data-gbadge6="1"
          onClick={(event) => actions.toggleGroupFromCard(group.id, event)}
          aria-label={group.toggleLabel}
          title={group.toggleLabel}
        >
          {T(group.rangeText)}
          <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="3"
            strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
            <path d={group.chevronPath} />
          </svg>
        </button>
        <span className="[flex:1_1_auto]" />
        {!pose.selectMode ? (
          <button
            type="button"
            data-gplay6="1"
            onClick={(event) => actions.playGroup(group.id, event)}
            disabled={group.playOff}
            aria-label={group.playLabel}
            title={group.playLabel}
          >
            <svg width="12" height="12" viewBox="0 0 24 24" aria-hidden="true">
              <path d="M7 4.5v15l12-7.5z" fill="currentColor" />
            </svg>
          </button>
        ) : null}
      </div>
      <span data-gname6="1" title={group.name}>{T(group.name)}</span>
      <div data-gr3="1">
        <span data-gfoot6="1">
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.2"
            strokeLinecap="round" strokeLinejoin="round" aria-hidden="true" className="shrink-0">
            <path d="M21 8l-9-5-9 5v8l9 5 9-5z" />
            <path d="M3 8l9 5 9-5M12 13v8" />
          </svg>
          {T(group.footText)}
        </span>
        <span className="[flex:1_1_auto]" />
        {group.showMoreButton ? (
          <button
            type="button"
            data-gmore6="1"
            onClick={(event) => actions.toggleGroupMenu(group.id, event)}
            aria-haspopup="true"
            aria-label={group.moreLabel}
            title={translate("robot.workspace.messages.viewRenameCopyCutUngroupDelete")}
          >
            ⋯
          </button>
        ) : null}
      </div>
      {group.menuOpen ? (
        <div
          role="menu"
          aria-label={group.moreLabel}
          className={`${GROUP_POP_CLASS} absolute top-[-3px] bottom-[-3px] left-[-3px] w-[400px] z-[9] shadow-pop
            box-border py-0 px-[8px] rounded-[14px] border-[3px] border-solid border-purple bg-white flex
            items-center gap-[6px]`}
        >
          {group.renaming ? (
            <>
              <input
                type="text"
                className={NAME_INPUT_CLASS}
                value={v.gRenDraft ?? ""}
                onChange={actions.editGroupNameDraft}
                onKeyDown={(event) => actions.keyOnGroupName(group.id, event)}
                autoFocus={true}
                maxLength={20}
                aria-label={translate("robot.workspace.messages.newPoseGroupName")}
              />
              <button
                type="button"
                onClick={(event) => actions.commitGroupName(group.id, event)}
                aria-label={translate("robot.workspace.messages.savePoseGroupName")}
                className="w-[36px] h-[36px] shrink-0 p-0 border-none border-current border-b-[3px]
                  [border-bottom-style:solid] border-b-purple-ink rounded-[9px] bg-purple text-white
                  font-[family-name:inherit] text-[15px] font-bold cursor-pointer"
              >
                ✓
              </button>
            </>
          ) : null}
          {group.showMenuItems ? (
            <>
              <button
                type="button"
                role="menuitem"
                className={`${MENU_BUTTON_CLASS} px-[2px] border-purple-line text-purple-ink text-[12px]`}
                onClick={(event) => actions.startGroupRename(group.id, event)}
              >
                {translate("robot.workspace.messages.nameLabel")}
              </button>
              <button
                type="button"
                role="menuitem"
                className={`${MENU_BUTTON_CLASS} px-[2px] border-purple-line text-purple-ink text-[12px]`}
                onClick={(event) => actions.copyGroup(group.id, event)}
                title={translate("robot.workspace.messages.copyPoseGroupCtrlC")}
              >
                <Icon name="copy" className={ICON_CLASS} />
                {" "}{translate("robot.workspace.messages.copy")}
              </button>
              <button
                type="button"
                role="menuitem"
                className={`${MENU_BUTTON_CLASS} px-[2px] border-purple-line text-purple-ink text-[12px]`}
                onClick={(event) => actions.pasteAfterGroup(group.id, event)}
                disabled={v.noClip}
                title={translate("robot.workspace.messages.pasteAfterThisGroupCtrlV")}
              >
                <Icon name="paste" className={ICON_CLASS} />
                {" "}{translate("robot.workspace.messages.paste")}
              </button>
              <button
                type="button"
                role="menuitem"
                className={`${MENU_BUTTON_CLASS} px-[2px] border-err-line text-err-ink text-[12px]`}
                onClick={(event) => actions.deleteGroup(group.id, event)}
                title={translate("robot.workspace.messages.deleteGroupAndItsPosesDelete")}
              >
                <Icon name="trash" className={ICON_CLASS} />
                {" "}{translate("common.delete")}
              </button>
              <button
                type="button"
                role="menuitem"
                className={`${MENU_BUTTON_CLASS} px-[4px] border-purple-line text-purple-ink text-[13px]`}
                onClick={(event) => actions.ungroupFromCard(group.id, event)}
                title={group.ungroupLabel}
              >
                <Icon name="cut" className={ICON_CLASS} />
                {" "}{translate("robot.workspace.messages.ungroup")}
              </button>
            </>
          ) : null}
          <button
            type="button"
            onClick={actions.closeGroupCardMenu}
            aria-label={translate("common.messages.close")}
            className="w-[28px] h-[28px] shrink-0 p-0 border-none border-current rounded-full bg-sunken
              text-muted font-[family-name:inherit] text-[12px] font-bold cursor-pointer"
          >
            ✕
          </button>
        </div>
      ) : null}
    </div>
  );
}
