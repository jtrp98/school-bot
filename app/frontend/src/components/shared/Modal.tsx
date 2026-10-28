import type { ReactNode } from "react";
import { Icon, type IconName } from "./Icons";

const modalPanelClass = [
  "sb-modal flex max-w-[calc(100%_-_32px)] flex-col gap-[16px] rounded-modal border-2 border-solid",
  "border-line bg-surface p-[28px] shadow-pop",
].join(" ");

type InlineDialogProps = {
  layout: "inline";
  label: string;
  className?: string;
  children?: ReactNode;
  role?: "dialog" | "alertdialog";
};

type StandardModalProps = {
  layout?: "standard";
  title: ReactNode;
  icon?: IconName;
  tone?: "warn" | "ok" | "err" | "info";
  width?: number;
  children?: ReactNode;
  actions?: ReactNode;
  fixed?: boolean;
  soft?: boolean;
  titleId?: string;
};

export function Modal(props: StandardModalProps | InlineDialogProps) {
  if (props.layout === "inline") {
    return (
      <div className={props.className} role={props.role ?? "alertdialog"} aria-label={props.label}>
        {props.children}
      </div>
    );
  }
  return <StandardModal {...props} />;
}

function StandardModal({
  title,
  icon,
  tone = "warn",
  width = 560,
  children,
  actions,
  fixed,
  soft,
  titleId = "sb-modal-title",
}: StandardModalProps) {
  return (
    <div
      className={`sb-scrim inset-0 flex items-center justify-center ${
        fixed ? "fixed z-[100]" : "absolute z-40"
      } ${
        soft ? "sb-scrim-soft bg-[rgba(var(--bg-rgb),.82)]" : "bg-[rgba(var(--ink-rgb),.45)]"
      }`}
    >
      <div
        className={modalPanelClass}
        role="dialog"
        aria-modal="true"
        aria-labelledby={titleId}
        style={{ width }}
      >
        {icon && (
          <span
            className={`flex h-[56px] w-[56px] items-center justify-center rounded-[50%] ${
              tone === "warn"
                ? "bg-orange-tint text-orange-deep"
                : tone === "ok"
                  ? "bg-ok-tint text-ok-ink"
                  : tone === "err"
                    ? "bg-err-tint text-err-ink"
                    : "bg-blue-tint text-blue-ink"
            }`}
          >
            <Icon name={icon} size={30} />
          </span>
        )}
        <div className="sb-h2 font-heading text-[24px] font-extrabold leading-[1.25]" id={titleId}>
          {title}
        </div>
        {children}
        {actions && (
          <div className="sb-row mt-[4px] flex items-center justify-end gap-[12px]">{actions}</div>
        )}
      </div>
    </div>
  );
}
