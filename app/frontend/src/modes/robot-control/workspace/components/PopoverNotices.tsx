import { t as translate } from "../../../../i18n";
import type { CSSProperties, ReactNode } from "react";
import { T } from "../tplUtil";
import type { MessagePopoverView } from "../workspaceDialogFields";
import type { WorkspaceViewProps } from "../viewTypes";

type Props = WorkspaceViewProps<"dialogs" | "dialogActions">;

const ATTENTION_RING_CLASS = [
  "[--rc:var(--orange-rgb)] animate-[awAttn_2.4s_ease-in-out_infinite]",
  "motion-reduce:animate-none motion-reduce:shadow-[0_0_0_4px_rgba(var(--rc),.5)]",
].join(" ");

const POPOVER_PANEL_CLASS = [
  "fixed box-border flex w-[330px] cursor-pointer flex-col gap-[8px] rounded-[14px] border-2 border-solid",
  "bg-surface px-[14px] py-[12px] text-left whitespace-normal shadow-[0_10px_24px_rgba(var(--ink-rgb),0.2)]",
].join(" ");

// Two animation names alternate so every repeated tap restarts the shake.
function shakeClass(shakes: number): string {
  if (!shakes) return "";
  return shakes % 2
    ? "animate-[awShake_.45s_ease-in-out_1] motion-reduce:animate-none"
    : "animate-[awShake2_.45s_ease-in-out_1] motion-reduce:animate-none";
}

function MessagePopover({ popover, onClose }: { popover: MessagePopoverView; onClose: () => void }): ReactNode {
  const isNetwork = popover.kind === "net";
  const ringStyle: CSSProperties = {
    left: popover.anchor.left - 2,
    top: popover.anchor.top - 2,
    width: popover.anchor.width + 4,
    height: popover.anchor.height + 4,
  };
  const panelStyle: CSSProperties = popover.opensUp
    ? { left: popover.x, bottom: popover.y }
    : { left: popover.x, top: popover.y };
  return (
    <>
      <div
        aria-hidden="true"
        className={`pointer-events-none fixed rounded-[14px] ${isNetwork ? "z-[71]" : "z-[36]"} ${ATTENTION_RING_CLASS}`}
        style={ringStyle}
      />
      <div
        role="alertdialog"
        aria-label={popover.title}
        onClick={onClose}
        className={[
          POPOVER_PANEL_CLASS,
          popover.red ? "border-err" : "border-line-strong",
          isNetwork ? "z-[72]" : "z-[38]",
          shakeClass(popover.shakes),
        ].join(" ")}
        style={panelStyle}
      >
        <span className={`text-[15px] font-bold ${popover.red ? "text-err-ink" : "text-ink"}`}>
          {T(popover.title)}
        </span>
        <span className="text-[13px] leading-[1.5] text-muted [white-space:pre-line]">{T(popover.body)}</span>
        <span className="flex items-center justify-between gap-[8px]">
          <span className="text-[12px] leading-[1.5] text-muted">
            {translate("robot.workspace.messages.tapAnywhereToClose")}
          </span>
        </span>
      </div>
    </>
  );
}

export function PopoverNotices({ v }: Props): ReactNode {
  const { messagePopover, keyPopover } = v.dialogs;
  return (
    <>
      {keyPopover ? (
        <>
          <div onClick={v.dialogActions.closeKeyPopover} aria-hidden="true" className="fixed inset-0 z-[37] bg-transparent" />
          <div
            role="dialog"
            aria-label={translate("robot.workspace.messages.tableShortcuts")}
            onClick={v.dialogActions.closeKeyPopover}
            className={`${POPOVER_PANEL_CLASS} z-[38] border-line-strong`}
            style={{ left: keyPopover.x, top: keyPopover.y }}
          >
            <span className="text-[15px] font-bold text-ink">
              {translate("robot.workspace.messages.tableShortcuts")}
            </span>
            <span className="text-[13px] leading-[1.5] text-muted">
              {translate("robot.workspace.messages.adjustBy1Shift10ChangeJointEnterTo")}
            </span>
            <span className="text-[12px] leading-[1.5] text-muted">
              {translate("robot.workspace.messages.shownOnceTapAnywhereToClose")}
            </span>
          </div>
        </>
      ) : null}
      {messagePopover ? (
        <MessagePopover popover={messagePopover} onClose={v.dialogActions.closeMessagePopover} />
      ) : null}
    </>
  );
}
