import type { ReactNode } from "react";
import { Button } from "./Button";

const toastClass = [
  "sb-toast inline-flex items-center gap-[14px] min-h-[52px] pt-[8px] pr-[10px] pb-[8px] pl-[18px]",
  "rounded-control bg-ink text-white not-italic [font-variant:normal] font-semibold",
  "[font-stretch:normal] text-[16px] leading-[1.3] font-body shadow-pop [&_.sb-btn]:min-h-[44px]",
].join(" ");

export function Toast({
  children,
  actionLabel,
  onAction,
}: {
  children: ReactNode;
  actionLabel?: string;
  onAction?: () => void;
}) {
  return (
    <div className={toastClass} role="status">
      <span>{children}</span>
      {actionLabel && (
        <Button variant="secondary" size="sm" onClick={onAction}>
          {actionLabel}
        </Button>
      )}
    </div>
  );
}
