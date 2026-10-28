import { Icon } from "../../../../components/shared/Icons";
import { t as translate } from "../../../../i18n";
import type { MouseEvent, ReactNode } from "react";
import { T } from "../tplUtil";
import type { WorkspaceViewProps } from "../viewTypes";
import { MenuItem } from "./MenuItem";

type Props = WorkspaceViewProps<"contextMenus" | "menuActions" | "playbackBar" | "playbackActions">;

const MENU_PANEL_CLASS = [
  "absolute box-border flex flex-col gap-[1px] rounded-[12px] border-2 border-solid bg-surface p-[6px]",
  "shadow-[0_10px_24px_rgba(var(--ink-rgb),0.22)]",
].join(" ");

const MENU_TITLE_CLASS = "overflow-hidden px-[10px] pt-[4px] pb-[6px] text-[12px] font-bold text-ellipsis whitespace-nowrap";

function Backdrop({ layer, onClick, onContextMenu }: {
  layer: string;
  onClick: (event: MouseEvent) => void;
  onContextMenu?: (event: MouseEvent) => void;
}): ReactNode {
  return <div onClick={onClick} onContextMenu={onContextMenu} aria-hidden="true" className={`absolute inset-0 ${layer}`} />;
}

export function Overlays({ v }: Props): ReactNode {
  const { pose0Menu, groupMenu, poseMenu } = v.contextMenus;
  const actions = v.menuActions;
  return (
    <>
      {pose0Menu ? (
        <>
          <Backdrop layer="z-[92]" onClick={actions.closePoseZeroMenu} />
          <div
            role="menu"
            aria-label={translate("robot.workspace.messages.usePose0AsANewPoseLabel")}
            className={`${MENU_PANEL_CLASS} z-[93] w-[260px] gap-[2px] border-blue-line`}
            style={{ left: pose0Menu.left, top: pose0Menu.top }}
          >
            <span className="px-[10px] pt-[4px] pb-[6px] text-[12px] font-bold text-blue-press">
              {translate("robot.workspace.messages.usePose0UprightArmAsANewPose")}
            </span>
            <MenuItem onClick={actions.insertPoseZeroFirst}>
              {translate("robot.workspace.messages.insertAsFirstPose")}
            </MenuItem>
            <MenuItem onClick={actions.appendPoseZeroLast}>
              {translate("robot.workspace.messages.appendAsLastPose")}
            </MenuItem>
          </div>
        </>
      ) : null}
      {groupMenu ? (
        <>
          <Backdrop layer="z-[90]" onClick={actions.closeGroupMenu} onContextMenu={actions.closeGroupMenu} />
          <div
            role="menu"
            aria-label={groupMenu.title}
            className={`${MENU_PANEL_CLASS} z-[91] w-[240px] border-purple-line`}
            style={{ left: groupMenu.left, top: groupMenu.top }}
          >
            <span className={`${MENU_TITLE_CLASS} text-purple-ink`}>
              <Icon name={groupMenu.icon} className="mr-[.3em] inline-block size-[1.15em] align-[-.2em]" />
              {T(groupMenu.title)}
            </span>
            <MenuItem size="md" icon="chevD" onClick={actions.toggleGroupFromMenu} disabled={groupMenu.disabled}>
              {T(groupMenu.viewLabel)}
            </MenuItem>
            <MenuItem size="md" icon="edit" onClick={actions.renameGroupFromMenu} disabled={groupMenu.disabled}>
              {translate("robot.workspace.messages.rename")}
            </MenuItem>
            <MenuItem
              size="md"
              icon="copy"
              shortcut="Ctrl+C"
              onClick={actions.copyGroupFromMenu}
              disabled={groupMenu.disabled}
            >
              {translate("robot.workspace.messages.copyPoseGroup")}
            </MenuItem>
            <MenuItem
              size="md"
              icon="cut"
              shortcut="Ctrl+X"
              onClick={actions.cutGroupFromMenu}
              disabled={groupMenu.disabled}
            >
              {translate("robot.workspace.messages.cutPoseGroup")}
            </MenuItem>
            <MenuItem size="md" icon="cut" onClick={actions.ungroupFromMenu} disabled={groupMenu.disabled}>
              {translate("robot.workspace.messages.ungroupPoses")}
            </MenuItem>
            <MenuItem
              size="md"
              icon="trash"
              tone="danger"
              shortcut="Delete"
              onClick={actions.deleteGroupFromMenu}
              disabled={groupMenu.disabled}
            >
              {translate("robot.workspace.messages.deletePoseGroup")}
            </MenuItem>
          </div>
        </>
      ) : null}
      {poseMenu ? (
        <>
          <Backdrop layer="z-[90]" onClick={actions.closePoseMenu} onContextMenu={actions.closePoseMenu} />
          <div
            role="menu"
            aria-label={poseMenu.title}
            className={`${MENU_PANEL_CLASS} z-[91] w-[240px] border-blue-line`}
            style={{ left: poseMenu.left, top: poseMenu.top }}
          >
            <span className={`${MENU_TITLE_CLASS} text-blue-press`}>{T(poseMenu.title)}</span>
            <MenuItem
              size="md"
              icon="copy"
              shortcut="Ctrl+C"
              onClick={actions.copyPoseFromMenu}
              disabled={poseMenu.disabled}
            >
              {translate("robot.workspace.messages.copyThisPose")}
            </MenuItem>
            <MenuItem
              size="md"
              icon="cut"
              shortcut="Ctrl+X"
              onClick={actions.cutPoseFromMenu}
              disabled={poseMenu.disabled}
            >
              {translate("robot.workspace.messages.cutThisPose")}
            </MenuItem>
            <MenuItem
              size="md"
              icon="paste"
              shortcut="Ctrl+V"
              onClick={actions.pasteAfterPoseFromMenu}
              disabled={poseMenu.pasteDisabled}
            >
              {translate("robot.workspace.messages.pasteAfterThisPose")}
            </MenuItem>
            <div className="mx-0 my-[4px] h-[2px] bg-sunken" />
            <MenuItem size="md" icon="edit" onClick={actions.renamePoseFromMenu} disabled={poseMenu.disabled}>
              {translate("robot.workspace.messages.rename")}
            </MenuItem>
            <MenuItem size="md" icon="copy" onClick={actions.duplicatePoseFromMenu} disabled={poseMenu.disabled}>
              {translate("robot.workspace.messages.duplicateThisPose")}
            </MenuItem>
            <MenuItem
              size="md"
              icon="trash"
              tone="danger"
              onClick={actions.deletePoseFromMenu}
              disabled={poseMenu.disabled}
            >
              {translate("common.messages.deleteThisPose")}
            </MenuItem>
          </div>
        </>
      ) : null}
      {v.contextMenus.mainMenuBackdrop ? <Backdrop layer="z-[55]" onClick={actions.closeMenus} /> : null}
      {v.playbackBar.menu.open ? <Backdrop layer="z-[35]" onClick={v.playbackActions.closeMenu} /> : null}
      {v.playbackBar.range.openEdge ? <Backdrop layer="z-[19]" onClick={v.playbackActions.closeRangeList} /> : null}
      {v.contextMenus.groupInlineMenuBackdrop ? <Backdrop layer="z-[8]" onClick={actions.closeMenus} /> : null}
      {v.contextMenus.jointMenuBackdrop ? <Backdrop layer="z-[38]" onClick={actions.closeJointMenu} /> : null}
    </>
  );
}
