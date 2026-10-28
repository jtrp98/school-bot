import { Button } from "../../../../components/shared/Button";
import { Icon } from "../../../../components/shared/Icons";
import { t as translate } from "../../../../i18n";
import type { ReactNode } from "react";
import { T } from "../tplUtil";
import type { WorkspaceViewProps } from "../viewTypes";
import { MergeProjectDialog } from "./MergeProjectDialog";

type Props = WorkspaceViewProps<"projectDialogs" | "dialogActions">;

// The scrim stops above the bottom bar on purpose: the Stop button must stay reachable while a dialog is open.
function StopBarSafeDialog({
  label,
  width,
  children,
}: {
  label: string;
  width: "w-[520px]" | "w-[540px]";
  children: ReactNode;
}): ReactNode {
  return (
    <div
      className={[
        "absolute top-0 right-0 bottom-[76px] left-0 z-[70] flex items-center justify-center",
        "bg-[rgba(var(--ink-rgb),_0.35)]",
      ].join(" ")}
    >
      <div
        role="alertdialog"
        aria-label={label}
        className={`${width} box-border flex flex-col gap-[12px] rounded-[20px] bg-white px-[26px] py-[24px] shadow-pop`}
      >
        {children}
      </div>
    </div>
  );
}

export function ProjectDialogs({ v }: Props): ReactNode {
  const { recoveryOpen, merge, leave, clearConfirm } = v.projectDialogs;
  const actions = v.dialogActions;
  return (
    <>
      {recoveryOpen ? (
        <StopBarSafeDialog label={translate("robot.workspace.messages.recoverProject")} width="w-[540px]">
          <span className="text-[20px] font-bold">
            <Icon name="buoy" className="mr-[.3em] inline-block size-[1.15em] align-[-.2em]" />{" "}
            {translate("robot.workspace.messages.unsavedProjectFound")}
          </span>
          <span className="text-[15px] leading-[1.6] text-muted">
            {translate("robot.workspace.messages.theAppClosedBeforeSavingAnAutomaticBackupFrom")}
          </span>
          <div className="flex gap-[10px] text-[13px]">
            <span className="[flex:1_1_0] rounded-[12px] bg-sunken px-[12px] py-[10px]">
              <b>{translate("robot.workspace.messages.savedAt1020")}</b>
              <br />
              {translate("robot.workspace.messages.4Poses")}
            </span>
            <span className="[flex:1_1_0] rounded-[12px] border-2 border-solid border-blue-line bg-blue-tint px-[12px] py-[10px]">
              <b>{translate("robot.workspace.messages.backupAt1042")}</b>
              <br />
              {translate("robot.workspace.messages.5Poses1Group")}
            </span>
          </div>
          <div className="mt-[6px] flex justify-end gap-[10px]">
            <Button variant="secondary" size="md" onClick={actions.dismissRecovery}>
              {translate("robot.workspace.messages.openSavedVersion")}
            </Button>
            <Button variant="blue" size="md" icon="buoy" onClick={actions.recoverSampleBackup}>
              {translate("robot.workspace.messages.recoverBackup")}
            </Button>
          </div>
        </StopBarSafeDialog>
      ) : null}
      {merge ? <MergeProjectDialog merge={merge} actions={actions} /> : null}
      {leave ? (
        <StopBarSafeDialog label={leave.title} width="w-[520px]">
          <span className="text-[20px] font-bold">{T(leave.title)}</span>
          <span className="text-[15px] leading-[1.6] text-muted">{T(leave.body)}</span>
          <div className="mt-[6px] flex justify-end gap-[10px]">
            <Button variant="secondary" size="md" onClick={actions.stayOnPage}>
              {translate("robot.workspace.messages.stayOnThisPage")}
            </Button>
            {leave.mode === "busy" ? (
              <Button variant="danger" size="md" onClick={actions.stopRobot}>
                {translate("robot.workspace.messages.stopTheRobotFirst")}
              </Button>
            ) : null}
            {leave.mode === "unsaved" ? (
              <>
                <Button variant="secondary" size="md" onClick={actions.leaveWithoutSaving}>
                  {translate("robot.workspace.messages.donTSave")}
                </Button>
                <Button variant="blue" size="md" icon="save" onClick={actions.saveAndLeave}>
                  {translate("common.save")}
                </Button>
              </>
            ) : null}
            {leave.mode === "go" ? (
              <Button variant="blue" size="md" onClick={actions.leaveWithoutSaving}>
                {T(leave.goLabel)}
              </Button>
            ) : null}
          </div>
        </StopBarSafeDialog>
      ) : null}
      {clearConfirm ? (
        <StopBarSafeDialog label={translate("robot.workspace.messages.confirmStartOver")} width="w-[520px]">
          <span className="font-[family-name:'Mitr',_'Noto_Sans_Thai',_sans-serif] text-[22px] font-medium">
            {translate("robot.workspace.messages.deleteAllPoses")} {T(clearConfirm.poseCount)}{" "}
            {translate("robot.workspace.messages.posesAndStartOver")}
          </span>
          <span className="text-[15px] leading-[1.55] text-muted">
            {translate("robot.workspace.messages.poseGroupsAndNamesWillAlsoBeRemovedRobot")}
          </span>
          <div className="mt-[6px] flex justify-end gap-[10px]">
            <Button variant="secondary" size="md" onClick={actions.cancelClear}>
              {translate("common.cancel")}
            </Button>
            <Button variant="danger" size="md" onClick={actions.confirmClear}>
              {translate("robot.workspace.messages.deleteAll")}
            </Button>
          </div>
        </StopBarSafeDialog>
      ) : null}
    </>
  );
}
