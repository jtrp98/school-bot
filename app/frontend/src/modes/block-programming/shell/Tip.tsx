import type { ReactNode } from "react";

// The design's tooltip (CI §11.8) instead of the browser's title: dark, shown on hover and
// keyboard focus of the enclosing `group` element, never catching the pointer.
export function Tip({ side, children }: {
  side: "below-left" | "below-right" | "above-right" | "left";
  children: ReactNode;
}) {
  const place = {
    "below-left": "top-[48px] left-0",
    "below-right": "top-[48px] right-0",
    "above-right": "bottom-[calc(100%+8px)] right-0",
    left: "right-[calc(100%+8px)] top-1/2 -translate-y-1/2",
  }[side];
  return (
    <span
      aria-hidden="true"
      className={[
        "pointer-events-none absolute z-[40] hidden rounded-[8px] bg-ink px-[12px] py-[4px] font-body text-[14px]",
        "leading-[1.6] font-medium whitespace-nowrap text-white group-hover:block group-focus-visible:block",
        place,
      ].join(" ")}
    >
      {children}
    </span>
  );
}
