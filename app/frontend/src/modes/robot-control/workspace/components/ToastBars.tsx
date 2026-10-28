import { Icon } from "../../../../components/shared/Icons";
import { t as translate } from "../../../../i18n";
import type { ComponentPropsWithoutRef, ReactNode } from "react";
import { T } from "../tplUtil";
import type { WorkspaceViewProps } from "../viewTypes";

type Props = WorkspaceViewProps<"dialogs" | "dialogActions">;

const FLOATING_BAR_CLASS = [
  "absolute left-[50%] bottom-[100px] [transform:translateX(-50%)] rounded-[12px] bg-ink text-[14px] text-white",
  "shadow-pop",
].join(" ");

const TOAST_BAR_CLASS = [
  FLOATING_BAR_CLASS,
  "flex items-center gap-[12px] pt-[8px] pr-[8px] pb-[8px] pl-[16px] font-semibold whitespace-nowrap",
].join(" ");

function BarButton(props: ComponentPropsWithoutRef<"button">): ReactNode {
  return (
    <button
      type="button"
      {...props}
      className={[
        "h-[32px] cursor-pointer rounded-[8px] border-2 border-solid border-blue-line bg-transparent px-[12px]",
        "font-[family-name:inherit] text-[13px] font-bold text-white",
      ].join(" ")}
    />
  );
}

export function ToastBars({ v }: Props): ReactNode {
  const { movedText, toast, copyPopover } = v.dialogs;
  const actions = v.dialogActions;
  return (
    <>
      {movedText !== null ? (
        <div role="status" className={`${TOAST_BAR_CLASS} z-[35]`}>
          <span>
            {"✓ "}
            {T(movedText)}
          </span>
          <BarButton onClick={actions.undoMove}>{translate("common.messages.undo")}</BarButton>
        </div>
      ) : null}
      {copyPopover ? (
        <div
          data-cpbox="1"
          role="status"
          className={[
            FLOATING_BAR_CLASS,
            "z-[37] box-border w-[620px] max-w-[calc(100%_-_32px)] pt-[12px] pr-[12px] pb-[12px] pl-[16px]",
          ].join(" ")}
        >
          <div className="flex items-center gap-[8px]">
            <span className="[flex:1_1_0] font-bold">{T(copyPopover.title)}</span>
            <BarButton onClick={actions.toggleCopyDetails}>
              {copyPopover.open
                ? translate("robot.workspace.messages.hideExplanation")
                : translate("robot.workspace.messages.showExplanation")}
            </BarButton>
            <button
              type="button"
              aria-label={translate("common.messages.close")}
              onClick={actions.closeCopy}
              className="h-[32px] w-[32px] cursor-pointer rounded-[8px] border-0 bg-transparent text-[18px] text-line-strong"
            >
              ✕
            </button>
          </div>
          <code
            className={[
              "mt-[8px] block rounded-[8px] bg-[rgba(var(--surface-rgb),_0.08)] px-[10px] py-[8px]",
              "font-[family-name:ui-monospace,_Menlo,_Consolas,_monospace] text-[13px] leading-[1.5] text-blue-tint",
              "[overflow-wrap:anywhere] [user-select:all] [white-space:normal]",
            ].join(" ")}
          >
            {T(copyPopover.code)}
          </code>
          {copyPopover.open ? (
            <>
              <div className={`mt-[10px] grid gap-[6px] ${copyPopover.rows.length > 1 ? "grid-cols-3" : "grid-cols-1"}`}>
                {copyPopover.rows.map((row) => (
                  <div
                    key={row.sid}
                    className="rounded-[8px] bg-[rgba(var(--surface-rgb),_0.06)] px-[8px] py-[6px] text-[13px] leading-[1.45]"
                  >
                    <div className="font-bold">
                      {T(row.sid)} {T(row.name)}
                    </div>
                    <div className="font-[family-name:ui-monospace,_Menlo,_Consolas,_monospace] text-line-strong">
                      {T(row.id)}
                      {" · P"}
                      {T(row.p)}
                      {" · T"}
                      {T(row.t)}
                    </div>
                  </div>
                ))}
              </div>
              <div className="mt-[8px] text-[12px] text-line-strong">
                {translate("robot.workspace.messages.jointPPosition5002500TDurationMsTap")}
              </div>
            </>
          ) : null}
        </div>
      ) : null}
      {toast ? (
        <div role="status" className={`${TOAST_BAR_CLASS} z-[36]`}>
          <span>
            <Icon name={toast.icon} className="mr-[.3em] inline-block size-[1.15em] align-[-.2em]" />
            {T(toast.text)}
          </span>
          {toast.primaryLabel !== null ? (
            <BarButton onClick={actions.runToastPrimary}>{T(toast.primaryLabel)}</BarButton>
          ) : null}
          {toast.secondaryLabel !== null ? (
            <BarButton onClick={actions.runToastSecondary}>{T(toast.secondaryLabel)}</BarButton>
          ) : null}
        </div>
      ) : null}
    </>
  );
}
