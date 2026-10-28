import type { ReactNode } from "react";
import { Icon, type IconName } from "./Icons";

const pillBaseClass = [
  "sb-pill inline-flex min-h-[28px] items-center gap-[6px] rounded-full px-[12px] py-[2px]",
  "font-body text-[13px] font-bold leading-[1.2] whitespace-nowrap",
].join(" ");

export type PillTone = "ok" | "off" | "warn" | "err" | "blue";

const spinClass = "sb-spin animate-[sbSpin_1s_linear_infinite] motion-reduce:animate-none";

function pillToneClass(tone: PillTone): string {
  return tone === "ok"
    ? "bg-ok-tint text-ok-ink"
    : tone === "off"
      ? "bg-off text-muted"
      : tone === "warn"
        ? "bg-orange-tint text-orange-deep"
        : tone === "err"
          ? "bg-err-tint text-err-ink"
          : "bg-blue-tint text-blue-ink";
}

export function Pill({ tone = "off", icon, dot, children, className }: {
  tone?: PillTone;
  icon?: IconName;
  dot?: boolean;
  children: ReactNode;
  className?: string;
}) {
  return (
    <span className={`${pillBaseClass} sb-pill-${tone} ${pillToneClass(tone)} ${className ?? ""}`}>
      {dot ? <i className="sb-dot inline-block h-[10px] w-[10px] shrink-0 rounded-[50%] bg-current" /> : icon ? (
        <Icon
          name={icon}
          size={14}
          className={icon === "loader" ? spinClass : undefined}
        />
      ) : null}
      {children}
    </span>
  );
}
