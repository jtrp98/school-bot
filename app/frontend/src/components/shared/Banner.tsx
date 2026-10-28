import type { ReactNode } from "react";
import { Icon, type IconName } from "./Icons";

const bannerBaseClass = "sb-banner flex items-start gap-[14px] rounded-card border-2 border-solid px-[16px] py-[14px]";

export type BannerKind = "info" | "warn" | "err" | "ok";
const bannerIcon: Record<BannerKind, IconName> = { info: "info", warn: "warn", err: "warn", ok: "check" };

function bannerToneClass(kind: BannerKind): string {
  return kind === "info"
    ? "bg-blue-tint border-blue-line"
    : kind === "warn"
      ? "bg-orange-tint border-orange-line"
      : kind === "err"
        ? "bg-err-tint border-err-line"
        : "bg-ok-tint border-ok-line";
}

function bannerTextClass(kind: BannerKind): string {
  return kind === "info"
    ? "text-blue-ink"
    : kind === "warn"
      ? "text-orange-deep"
      : kind === "err"
        ? "text-err-ink"
        : "text-ok-ink";
}

export function Banner({ kind = "info", icon, title, children, actions, className }: {
  kind?: BannerKind;
  icon?: IconName;
  title: ReactNode;
  children?: ReactNode;
  actions?: ReactNode;
  className?: string;
}) {
  return (
    <div
      className={`${bannerBaseClass} sb-banner-${kind} ${bannerToneClass(kind)} ${className ?? ""}`}
      role={kind === "err" ? "alert" : "status"}
    >
      <span className={bannerTextClass(kind)}>
        <Icon name={icon ?? bannerIcon[kind]} size={28} />
      </span>
      <div className="sb-grow sb-col flex min-w-0 flex-1 flex-col gap-[4px]">
        <div className="sb-h3 font-heading text-[18px] font-bold leading-[1.3]">{title}</div>
        {children && <div className="sb-t-sm font-body text-[14px] font-normal leading-[1.45]">{children}</div>}
      </div>
      {actions && <div className="sb-row flex shrink-0 items-center gap-[12px]">{actions}</div>}
    </div>
  );
}
