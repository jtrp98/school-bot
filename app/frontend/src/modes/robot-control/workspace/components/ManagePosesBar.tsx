import { Icon } from "../../../../components/shared/Icons";
import { t as translate } from "../../../../i18n";
import type { ReactNode } from "react";
import { T } from "../tplUtil";
import type { WorkspaceViewProps } from "../viewTypes";
import { TableToggleButton } from "./TableToggleButton";
import { ToolbarButton } from "./ToolbarButton";

type Props = WorkspaceViewProps<"toolbar" | "toolbarActions" | "relEnd">;

const GROUP_NAME_INPUT_CLASS = [
  "aw-edit box-border h-[36px] w-[220px] min-w-[120px] shrink-[1] cursor-text rounded-[10px] border-2 border-solid",
  "border-blue-ink bg-white px-[10px] py-0 font-[family-name:inherit] text-[14px] font-bold text-ink",
  "focus:shadow-[0_0_0_3px_var(--blue-tint)] focus:outline-none",
].join(" ");

const BAR_CLASS = [
  "box-border flex min-w-0 [flex:1_1_auto] items-center gap-[8px] rounded-[12px] border-2 border-solid pr-[5px] pl-[12px]",
].join(" ");

export function ManagePosesBar({ v }: Props): ReactNode {
  const { manageBar, releaseBar, tableToggle, groupSelection } = v.toolbar;
  const actions = v.toolbarActions;
  return (
    <>
      {manageBar ? (
        <div data-sel6ok="1" className={`${BAR_CLASS} h-[46px] border-blue-line bg-blue-tint`}>
          <b className="text-[14px] whitespace-nowrap text-blue-press">
            {translate("robot.workspace.messages.managePoses")}
          </b>
          <span
            className={[
              "min-w-0 [flex:1_1_auto] overflow-hidden text-[12px] text-ellipsis whitespace-nowrap",
              manageBar.emphasized ? "font-bold text-purple-ink" : "font-semibold text-blue-press",
            ].join(" ")}
          >
            {T(manageBar.text)}
          </span>
          {manageBar.naming ? (
            <input
              type="text"
              className={GROUP_NAME_INPUT_CLASS}
              value={manageBar.draft}
              onChange={(event) => actions.setGroupDraft(event.target.value)}
              onKeyDown={(event) => actions.handleGroupNameKey(event, groupSelection)}
              autoFocus
              maxLength={20}
              placeholder={translate("robot.workspace.messages.namePoseGroup")}
              aria-label={translate("robot.workspace.messages.newPoseGroupName")}
            />
          ) : null}
          <ToolbarButton
            variant="blue"
            onClick={() => actions.makeGroup(groupSelection)}
            disabled={manageBar.makeDisabled}
            title={manageBar.makeTip}
            className={manageBar.makeDisabled ? "" : "bg-purple! border-purple! border-b-purple-ink!"}
          >
            <svg
              width="15"
              height="15"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2.2"
              strokeLinecap="round"
              strokeLinejoin="round"
              aria-hidden="true"
              className="mr-[6px] inline-block align-[-.2em]"
            >
              <path d="M21 8l-9-5-9 5v8l9 5 9-5z" />
              <path d="M3 8l9 5 9-5M12 13v8" />
            </svg>
            {T(manageBar.makeLabel)}
          </ToolbarButton>
          <span aria-hidden="true" className="mx-[2px] my-0 h-[26px] w-[2px] shrink-0 rounded-[2px] bg-blue-line" />
          <ToolbarButton variant="secondary" data-cancel6="1" onClick={actions.cancelManagingPoses}>
            {translate("common.cancel")}
          </ToolbarButton>
          <ToolbarButton variant="blue" data-exit6="1" onClick={actions.finishManagingPoses}>
            {translate("robot.workspace.messages.save")}
          </ToolbarButton>
        </div>
      ) : null}
      {releaseBar ? (
        <div className={`${BAR_CLASS} h-[44px] border-orange bg-orange-tint pr-[4px] text-orange-deep`}>
          <Icon name="lock" className="mr-[.3em] inline-block size-[1.15em] align-[-.2em]" />
          <b className="text-[14px] whitespace-nowrap">{translate("robot.workspace.messages.jointsReleased")}</b>
          <span className="min-w-0 [flex:1_1_auto] overflow-hidden text-[12px] font-semibold text-ellipsis whitespace-nowrap">
            {translate("robot.workspace.messages.positionTheSelectedPoseByHandThenPressRead")}
          </span>
          <ToolbarButton
            variant="secondary"
            data-relock6="1"
            onClick={v.relEnd}
            className="border-orange! border-b-orange-press! text-orange-deep!"
          >
            {translate("robot.workspace.messages.lockJoints")}
          </ToolbarButton>
          <TableToggleButton toggle={tableToggle} onToggle={actions.toggleTableFullscreen} describeState={false} />
        </div>
      ) : null}
    </>
  );
}
