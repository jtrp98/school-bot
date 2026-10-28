import { Icon } from "../../../../components/shared/Icons";
import { t as translate } from "../../../../i18n";
import { Fragment, type ReactNode } from "react";
import { T, arr } from "../tplUtil";
import type { WorkspaceViewProps } from "../viewTypes";
import { JointCard } from "./JointCard";
import { SelectedPose } from "./SelectedPose";

type Props = WorkspaceViewProps;

const ICON_CLASS = "inline-block size-[1.15em] align-[-.2em] mr-[.3em]";
const RELEASE_ACTIVE_CLASS = "p-[8px] m-[-8px] rounded-[16px] bg-orange-tint outline-2 outline-solid outline-orange "
  + "[--rc:255,138,0] animate-[awAttn_2.4s_ease-in-out_infinite]";

function readAllClass(unavailable: boolean): string {
  const tone = unavailable
    ? "border-b-line-strong bg-off text-faint cursor-not-allowed"
    : "border-b-blue-press bg-blue-ink text-white cursor-pointer";
  return `h-[40px] py-0 px-[14px] rounded-[12px] font-[family-name:inherit] text-[15px] font-bold whitespace-nowrap `
    + `border-none border-current border-b-4 [border-bottom-style:solid] ${tone}`;
}

function durationToggleClass(linked: boolean, locked: boolean): string {
  const tone = linked
    ? "border-blue-ink bg-blue-tint text-blue-ink"
    : "border-line-strong bg-surface text-muted";
  return `inline-flex items-center gap-[6px] h-[40px] py-0 px-[12px] box-border rounded-[12px] `
    + `font-[family-name:inherit] text-[14px] font-bold whitespace-nowrap border-2 border-solid ${tone} `
    + (locked ? "opacity-50 cursor-not-allowed" : "cursor-pointer");
}

function menuButtonClass(open: boolean): string {
  const tone = open ? "bg-blue-tint text-blue-ink" : "bg-transparent text-muted";
  return `flex items-center justify-center shrink-0 mr-[14px] w-[40px] h-[40px] p-0 box-border rounded-[12px] `
    + `font-[family-name:inherit] cursor-pointer border-none border-current ${tone}`;
}

export function JointPanel({ v }: Props): ReactNode {
  const panel = v.jointPanelActions;
  return (
    <section
      aria-label={translate("common.messages.moveTheArm")}
      className="grow min-w-0 bg-white border-2 border-solid border-line rounded-[20px] py-[10px] px-[12px]
        box-border flex gap-[16px]"
    >
      <div className={`grow min-w-0 flex flex-col gap-[4px] ${v.relOn ? RELEASE_ACTIVE_CLASS : ""}`}>
        <div className={"relative flex items-center justify-between gap-[8px] min-h-[40px]"} >
          {v.relOn ? (
            <>
            <div role="status" className={"absolute inset-0 z-[6] bg-orange-tint flex items-center gap-[8px]"} >
              <span
                className="inline-flex items-center gap-[6px] h-[34px] py-0 px-[12px] rounded-full bg-white
                  border-2 border-solid border-orange text-orange-deep text-[15px] font-bold
                  whitespace-nowrap"
              >
                <Icon name="lock" className={ICON_CLASS} />
                {translate("robot.workspace.messages.jointsReleasedPositionTheArmByHand")}
              </span>
              <button
                type="button"
                onClick={v.relEnd}
                className="h-[40px] py-0 px-[14px] rounded-[12px] border-2 border-solid border-orange border-b-4
                  [border-bottom-style:solid] border-b-orange-press bg-white text-orange-deep
                  font-[family-name:inherit] text-[15px] font-bold cursor-pointer whitespace-nowrap"
              >
                {translate("robot.workspace.messages.lockJoints")}
              </button>
              <span className={"[flex:1_1_0]"}  />
              <button
                type="button"
                onClick={v.relAll}
                disabled={v.relAllOff}
                title={v.relAllTip}
                className={readAllClass(v.relAllOff)}
              >
                {T(v.relAllLabel)}
              </button>
              <button
                type="button"
                onClick={v.relNew}
                className="h-[40px] py-0 px-[14px] rounded-[12px] border-2 border-solid border-blue-line
                  border-b-4 bg-white text-blue-ink font-[family-name:inherit] text-[15px] font-bold
                  cursor-pointer whitespace-nowrap"
              >
                {translate("robot.workspace.messages.addAsANewPoseAfterThisOne")}
              </button>
            </div>
            </>
          ) : null}
          <span className={"flex items-center gap-[6px] min-w-0"} >
            <span
              className="font-[family-name:'Mitr',_'Noto_Sans_Thai',_sans-serif] text-[17px] font-medium
                leading-[30px]"
            >
              {translate("robot.workspace.joints.title")}
            </span>
            <span
              className="aw-tipwrap [&:hover_.aw-tip]:block [&:focus-within_.aw-tip]:block relative inline-flex
                w-[22px] h-[22px] rounded-full items-center justify-center text-muted [cursor:help]"
              tabIndex={0}
              aria-describedby="armcap-tip-v4b"
            >
              <svg
                width="18"
                height="18"
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
                id="armcap-tip-v4b"
                role="tooltip"
                className="aw-tip hidden absolute top-[calc(100%_+_6px)] left-[-8px] w-[240px] box-border bg-ink
                  text-white text-[13px] font-semibold leading-[1.5] py-[8px] px-[12px] rounded-[8px]
                  z-[40]"
              >
                {translate("robot.workspace.messages.tapANumberToChooseAJointUseOn")}{" "}
                {T(v.jointNote)}
              </span>
            </span>
            {v.jointLockShow ? (
              <>
              <span
                className="inline-flex items-center text-[13px] font-bold text-orange-deep bg-orange-tint
                  border-1 border-solid border-orange-line rounded-[8px] py-[3px] px-[8px]
                  whitespace-nowrap overflow-hidden text-ellipsis min-w-0"
              >
                <Icon name={v.jointTitleIc} className={ICON_CLASS} />
                {T(v.jointTitle)}
              </span>
              </>
            ) : null}
          </span>
          <div className={"flex items-center gap-[10px] shrink-0"} >
            {v.timeSegShow ? (
              <>
              <span className={"relative inline-flex"} >
                <button
                  type="button"
                  onClick={panel.toggleTimeMode}
                  disabled={v.tmOff}
                  aria-pressed={v.isSame}
                  title={v.tmTip}
                  className={durationToggleClass(v.isSame, v.tmOff)}
                >
                  {v.isSame ? (
                    <>
                    <svg
                      width="18"
                      height="18"
                      viewBox="0 0 24 24"
                      fill="none"
                      stroke="currentColor"
                      strokeWidth="2.4"
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      aria-hidden="true"
                    >
                      <path d="M10 13a5 5 0 0 0 7.5.5l3-3a5 5 0 0 0-7-7l-1.7 1.7" />
                      <path d="M14 11a5 5 0 0 0-7.5-.5l-3 3a5 5 0 0 0 7 7l1.7-1.7" />
                    </svg>
                    </>
                  ) : null}
                  {v.isSplit ? (
                    <>
                    <svg
                      width="18"
                      height="18"
                      viewBox="0 0 24 24"
                      fill="none"
                      stroke="currentColor"
                      strokeWidth="2.4"
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      aria-hidden="true"
                    >
                      <path d="M18.8 12.7l1.7-1.7a5 5 0 0 0-7-7l-1.7 1.7" />
                      <path d="M5.2 11.3l-1.7 1.7a5 5 0 0 0 7 7l1.7-1.7" />
                      <path d="M8 2v3M2 8h3M16 22v-3M22 16h-3" />
                    </svg>
                    </>
                  ) : null}
                  <span>
                    {T(v.chainLabel)}
                  </span>
                </button>
                {v.sameAskShow ? (
                  <>
                  <div
                    role="alertdialog"
                    aria-label={translate("robot.workspace.messages.useTheSameDurationForAllJoints")}
                    onClick={v.sameAskNo}
                    className="cursor-pointer absolute top-[46px] right-0 w-[330px] box-border py-[12px]
                      px-[14px] flex flex-col gap-[8px] bg-white border-2 border-solid
                      border-line-strong rounded-[14px] shadow-pop z-[45] text-left [white-space:normal]"
                  >
                    <span className={"text-[15px] font-bold text-ink"} >
                      {translate("robot.workspace.messages.setTheSameDurationForAllJoints")}
                    </span>
                    <span className={"text-[13px] leading-[1.5] text-muted"} >
                      {translate("robot.workspace.messages.all6JointsInEachPoseUseTheSame")}
                    </span>
                    <span className={"flex flex-wrap gap-[4px]"} >
                      {arr(v.sameAskChips).map((r, $i0) => (
                        <Fragment key={$i0}>
                          <span
                            className="inline-flex gap-[4px] py-[2px] px-[8px] rounded-full bg-blue-tint
                              text-[12px] font-bold text-blue-ink"
                          >
                            {T(r.t)}
                            {" · "}
                            {T(r.v)}
                          </span>
                        </Fragment>
                      ))}
                      <span className={"text-[12px] text-muted [align-self:center]"} >
                        {T(v.sameAskMore)}
                      </span>
                    </span>
                    <span className={"text-[12px] leading-[1.5] text-muted"} >
                      {T(v.sameAskWhy)}
                    </span>
                    <span className={"text-[12px] text-faint"} >
                      {translate("robot.workspace.messages.tapAnywhereToClose")}
                    </span>
                    <span className={"flex gap-[8px] justify-end"} >
                      <button
                        type="button"
                        onClick={panel.confirmSameDuration}
                        className="h-[36px] py-0 px-[14px] border-[medium] border-none border-current
                          border-b-[3px] [border-bottom-style:solid] border-b-blue-press rounded-[10px]
                          bg-blue-ink text-white font-[family-name:inherit] text-[13px] font-bold
                          cursor-pointer"
                      >
                        {translate("robot.workspace.messages.setEqual")}
                      </button>
                    </span>
                  </div>
                  </>
                ) : null}
              </span>
              </>
            ) : null}
            <div className={"relative shrink-0"} >
              <button
                type="button"
                onClick={panel.toggleMenu}
                aria-haspopup="menu"
                aria-expanded={v.hdrMenuOpen}
                aria-label={translate("robot.workspace.messages.poseMenuCopyCommandsReleaseJoints")}
                title={translate("robot.workspace.messages.copyPoseCommandsReleaseJoints")}
                className={menuButtonClass(v.hdrMenuOpen)}
              >
                <Icon name="dots" className={"inline-block size-[1.5em] align-[-.2em] "} />
              </button>
              {v.hdrMenuOpen ? (
                <>
                <div
                  role="menu"
                  aria-label={translate("robot.workspace.messages.poseMenu")}
                  className="aw-jmenu absolute right-0 z-[40] w-[290px] box-border p-[6px] rounded-[14px]
                    border-2 border-solid border-line-strong bg-white shadow-pop flex flex-col gap-[2px]
                    [&_button]:flex [&_button]:items-start [&_button]:gap-[10px] [&_button]:w-full
                    [&_button]:box-border [&_button]:py-[8px] [&_button]:px-[10px]
                    [&_button]:border-[medium] [&_button]:border-none border-current
                    [&_button]:border-current [&_button]:rounded-[10px] [&_button]:bg-white
                    [&_button]:text-ink [&_button]:font-[family-name:inherit] [&_button]:text-left
                    [&_button]:cursor-pointer [&_button:hover:not(:disabled)]:bg-blue-tint
                    [&_button:disabled]:text-faint [&_button:disabled]:cursor-not-allowed
                    [&_.aw-i]:mt-[2px] [&_.aw-i]:mx-0 [&_.aw-i]:mb-0 [&_.aw-i]:w-[1.3em]
                    [&_.aw-i]:h-[1.3em] [&_.aw-i]:text-blue-ink [&_button:disabled_.aw-i]:text-faint
                    top-[calc(100%_+_6px)]"
                >
                  <button type="button" role="menuitem" onClick={panel.copyAllCommands}>
                    <Icon name="copy" className={ICON_CLASS} />
                    <span className={"text-[15px] font-normal"} >
                      {translate("robot.workspace.messages.copyAllPoseCommands")}
                    </span>
                  </button>
                  <button type="button" role="menuitem" onClick={panel.askToReleaseJoints} disabled={v.readOff}>
                    <Icon name="lock" className={ICON_CLASS} />
                    <span className={"text-[15px] font-normal"} >
                      {translate("robot.workspace.messages.releaseJoints")}
                    </span>
                  </button>
                </div>
                </>
              ) : null}
            </div>
          </div>
        </div>
        {arr(v.joints).map((joint) => (
          <Fragment key={joint.index}>
            <JointCard v={v} joint={joint} />
          </Fragment>
        ))}
      </div>
      <SelectedPose v={v} />
    </section>
  );
}
