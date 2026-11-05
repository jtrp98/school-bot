import type { ButtonHTMLAttributes, ComponentProps, ReactNode } from "react";
import { Tip } from "./Tip";

type ToolButtonProps = Omit<ButtonHTMLAttributes<HTMLButtonElement>, "children" | "type"> & {
  label: string;
  tip: string;
  tipSide: ComponentProps<typeof Tip>["side"];
  // An unavailable button stays focusable (aria-disabled) so its tooltip and label can say why.
  unavailable?: boolean;
  children: ReactNode;
};

// The design's borderless 44px icon button with its dark tooltip.
export function ToolButton({ label, tip, tipSide, unavailable, onClick, children, ...rest }: ToolButtonProps) {
  return (
    <button
      type="button"
      aria-label={label}
      aria-disabled={unavailable || undefined}
      onClick={unavailable ? undefined : onClick}
      className={[
        "group relative inline-flex h-[44px] w-[44px] shrink-0 cursor-pointer items-center justify-center",
        "rounded-[12px] border-0 bg-transparent p-0 text-ink hover:bg-blue-tint",
        "focus-visible:outline-[3px] focus-visible:outline-offset-2 focus-visible:outline-blue",
        "focus-visible:[outline-style:solid]",
        "aria-disabled:cursor-not-allowed aria-disabled:bg-off aria-disabled:text-muted",
        "aria-disabled:shadow-[inset_0_0_0_2px_var(--line)] aria-disabled:hover:bg-off",
      ].join(" ")}
      {...rest}
    >
      {children}
      <Tip side={tipSide}>{tip}</Tip>
    </button>
  );
}
