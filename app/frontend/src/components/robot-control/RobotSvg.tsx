import type { ReactNode } from "react";

type SvgProps = { size: number; stroke?: string; sw?: number; className?: string; children: ReactNode };

export function Svg({ size, stroke = "currentColor", sw = 2, className, children }: SvgProps) {
  return (
    <svg
      className={className ?? ""}
      width={size}
      height={size}
      viewBox="0 0 24 24"
      fill="none"
      stroke={stroke}
      strokeWidth={sw}
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
    >
      {children}
    </svg>
  );
}
