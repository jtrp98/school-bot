import type { ComponentPropsWithRef } from "react";
import { Icon, type IconName } from "./Icons";

export type ButtonVariant = "primary" | "blue" | "secondary" | "danger" | "ok" | "ghost" | "custom";
export type ButtonSize = "lg" | "md" | "sm";

export type ButtonProps = ComponentPropsWithRef<"button"> & {
  variant?: ButtonVariant;
  size?: ButtonSize;
  icon?: IconName;
  iconRight?: IconName;
  loading?: boolean;
  block?: boolean;
};

const baseClass = [
  "sb-btn relative inline-flex items-center justify-center gap-[10px] rounded-control border-2",
  "border-solid py-0 font-heading font-bold leading-[1.1] whitespace-nowrap no-underline select-none",
  "cursor-pointer [transition:transform_.08s_ease,filter_.12s_ease,background_.12s_ease]",
  "focus-visible:outline-none focus-visible:shadow-[var(--focus)]",
  "enabled:active:[transform:translateY(3px)] enabled:active:border-b-2 enabled:active:brightness-[.95]",
  "disabled:bg-off disabled:text-faint disabled:border-off disabled:border-b-line-strong",
  "disabled:cursor-not-allowed disabled:[filter:none] disabled:transform-none aria-disabled:bg-off",
  "aria-disabled:text-faint aria-disabled:border-line-strong aria-disabled:cursor-not-allowed",
  "aria-pressed:border-t-4! aria-pressed:border-b-0! aria-pressed:translate-y-[2px]",
].join(" ");

export function Button({
  variant = "primary",
  size = "lg",
  icon,
  iconRight,
  loading,
  block,
  className,
  children,
  disabled,
  type = "button",
  ...rest
}: ButtonProps) {
  if (variant === "custom") {
    return (
      <button type={type} className={className} disabled={disabled} aria-busy={loading || undefined} {...rest}>
        {icon && <Icon name={icon} />}
        {children}
        {iconRight && <Icon name={iconRight} />}
      </button>
    );
  }
  const px = size === "lg" ? 22 : 20;
  const sizeClass = size === "lg"
    ? "min-h-[56px] px-[24px] text-[18px]"
    : size === "md"
      ? "sb-btn-md min-h-[48px] px-[20px] text-[17px]"
      : "sb-btn-sm min-h-[44px] px-[16px] text-[16px]";
  const borderClass = variant === "ghost" ? "border-b-2" : size === "lg" ? "border-b-[5px]" : "border-b-4";
  const colorClass = variant === "primary"
    ? size === "sm"
      ? "bg-orange-press border-orange-press border-b-orange-deep text-white enabled:hover:brightness-[1.07]"
      : "bg-orange border-orange border-b-orange-press text-white enabled:hover:brightness-[1.07]"
    : variant === "blue"
      ? "bg-blue-ink border-blue-ink border-b-blue-press text-white enabled:hover:brightness-[1.07]"
      : variant === "secondary"
        ? "bg-surface border-line-strong text-ink enabled:hover:bg-blue-tint enabled:hover:border-blue"
        : variant === "danger"
          ? "bg-err border-err border-b-err-press text-white enabled:hover:brightness-[1.07]"
          : variant === "ok"
            ? "bg-ok border-ok border-b-ok-ink text-white enabled:hover:brightness-[1.07]"
            : "bg-transparent border-transparent text-blue-ink enabled:hover:bg-blue-tint disabled:bg-transparent disabled:border-transparent";
  const variantClasses = `sb-btn-${variant} ${sizeClass} ${borderClass} ${colorClass}`;
  const blockClass = block ? "sb-btn-block flex w-full" : "";
  const loadingClass = loading
    ? "sb-is-loading pointer-events-none [filter:none] [&_.sb-ico:not(.sb-spin)]:hidden"
    : "";
  return (
    <button
      type={type}
      className={`${baseClass} ${variantClasses} ${blockClass} ${loadingClass} ${className ?? ""}`}
      disabled={disabled}
      aria-busy={loading || undefined}
      {...rest}
    >
      {loading ? (
        <Icon
          name="loader"
          size={px}
          className="sb-spin animate-[sbSpin_1s_linear_infinite] motion-reduce:animate-none"
        />
      ) : icon ? (
        <Icon name={icon} size={px} />
      ) : null}
      {children != null && <span>{children}</span>}
      {iconRight && <Icon name={iconRight} size={px} />}
    </button>
  );
}
