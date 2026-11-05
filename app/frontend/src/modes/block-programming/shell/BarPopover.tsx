import type { KeyboardEvent, ReactNode } from "react";

// The window bar's popover panel from the design: white, 1px border, radius 16, float shadow,
// sliding in 4px from above (still under reduced motion).
export function BarPopover({
  align,
  role,
  label,
  className,
  onKeyDown,
  children,
}: {
  align: "left" | "right";
  role: "menu" | "dialog";
  label: string;
  className: string;
  onKeyDown?: (event: KeyboardEvent<HTMLDivElement>) => void;
  children: ReactNode;
}) {
  return (
    <div
      role={role}
      aria-label={label}
      tabIndex={-1}
      onKeyDown={onKeyDown}
      className={[
        "absolute top-[48px] z-[40] box-border flex flex-col rounded-[16px] border border-solid border-line",
        "bg-surface text-left shadow-[0_8px_20px_rgba(20,36,61,.2)] outline-none",
        "motion-safe:animate-[block-lab-pop-in_.2s_cubic-bezier(.2,.8,.2,1)]",
        align === "left" ? "left-0" : "right-0",
        className,
      ].join(" ")}
    >
      {children}
    </div>
  );
}
