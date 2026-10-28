import { IconBtn } from "../../../../components/shared/IconBtn";
import { t as translate } from "../../../../i18n";
import type { ReactNode } from "react";
import { IMG, T } from "../tplUtil";
import type { WorkspaceViewProps } from "../viewTypes";
import { WindowControlsSlot } from "../../../../components/shared/WindowTitlebar";
import { MainMenu, networkChipClass, networkDotClass } from "./MainMenu";

type Props = WorkspaceViewProps<"topBar" | "menuActions" | "netTest" | "saveStateText">;

const RENAME_INPUT_CLASS = [
  "aw-edit h-[22px] w-[190px] cursor-text box-border rounded-[6px] border-none border-b-2 border-b-blue-ink",
  "[border-bottom-style:solid] bg-blue-tint px-[6px] py-0 text-right font-[family-name:inherit] text-[14px]",
  "leading-[1.3] font-bold text-blue-ink focus:outline-none",
].join(" ");

const PROJECT_NAME_CLASS = [
  "block max-w-[190px] cursor-text overflow-hidden rounded-[6px] border-b-2 [border-bottom-style:solid]",
  "border-b-transparent px-[6px] py-0 text-[14px] leading-[1.3] font-bold text-ellipsis whitespace-nowrap",
  "hover:bg-blue-tint! hover:text-blue-ink! hover:border-b-blue-ink!",
].join(" ");

export function TopBar({ v }: Props): ReactNode {
  const { project, network, menuOpen } = v.topBar;
  const actions = v.menuActions;
  return (
    <header
      className={[
        "window-integrated-header aw-window-header relative z-[56] box-border flex h-[60px] min-w-0 shrink-0 items-center",
        "gap-[10px] border-b-2 [border-bottom-style:solid] border-b-line bg-white px-[24px] py-0",
      ].join(" ")}
    >
      <img
        src={IMG.schoolBot}
        alt="School Bot Studio"
        width="192"
        height="34"
        className="h-[34px] w-[192px] shrink-0 object-contain [object-position:0%_50%]"
      />
      <img src={IMG.armLab} alt="Arm Lab" width="100" height="25" className="ml-[2px] block h-[25px] w-[100px] shrink-0" />
      <div className="min-w-[8px] grow" />
      <span className="aw-window-project relative flex shrink-0 items-center gap-[6px]">
        <span className="flex flex-col items-end gap-[1px]">
          {project.renaming ? (
            <input
              type="text"
              className={RENAME_INPUT_CLASS}
              value={project.draft}
              onChange={(event) => actions.setProjectDraft(event.target.value)}
              onKeyDown={actions.handleProjectNameKey}
              onBlur={actions.commitProjectName}
              autoFocus
              maxLength={30}
              placeholder={translate("robot.workspace.messages.projectNameForExamplePickUpABox")}
              aria-label={translate("robot.works.projectName")}
            />
          ) : (
            <span
              role="button"
              tabIndex={0}
              className={PROJECT_NAME_CLASS}
              onClick={actions.startProjectRename}
              title={translate("robot.workspace.messages.tapToRenameProject")}
            >
              {T(project.name)}
            </span>
          )}
          <span
            role="status"
            className={`px-[6px] py-0 text-[11px] font-semibold whitespace-nowrap ${project.saved ? "text-ok" : "text-muted"}`}
          >
            {T(v.saveStateText)}
          </span>
        </span>
      </span>
      {network.showHeaderChip ? (
        <button
          type="button"
          data-net="hdr"
          onClick={actions.explainNetworkFromHeader}
          aria-label={translate("robot.workspace.messages.value0TapForHelp", { value0: network.chipText })}
          className={`${networkChipClass(network.state)} mr-[10px] h-[32px] cursor-pointer self-center px-[12px] text-[13px]!`}
        >
          <span aria-hidden="true" className={networkDotClass(network.state)} />
          {T(network.chipText)}
        </button>
      ) : null}
      <span className="relative inline-flex shrink-0">
        <IconBtn
          icon="menu"
          label={translate("robot.workspace.messages.menu")}
          size={20}
          aria-haspopup="menu"
          aria-expanded={menuOpen}
          onClick={actions.toggleMainMenu}
          className={`h-[40px]! w-[44px]! border-b-2! ${menuOpen ? "border-blue-ink! bg-blue-ink! text-surface!" : ""}`}
        />
        {menuOpen ? <MainMenu v={v} /> : null}
      </span>
      <WindowControlsSlot />
    </header>
  );
}
