import type { ButtonHTMLAttributes, ReactNode } from "react";
import { Icon, type IconName } from "./Icons";

const iconBtnBaseClass = [
  "sb-iconbtn inline-flex h-[48px] w-[48px] min-w-[44px] items-center justify-center rounded-control",
  "p-0 text-ink cursor-pointer [transition:transform_.08s,background_.12s] enabled:hover:bg-blue-tint",
  "enabled:hover:border-blue focus-visible:outline-none focus-visible:shadow-[var(--focus)]",
  "enabled:active:[transform:translateY(2px)] enabled:active:border-b-2 disabled:bg-off",
  "disabled:border-off disabled:text-faint disabled:cursor-not-allowed",
].join(" ");

type IconBtnProps = Omit<ButtonHTMLAttributes<HTMLButtonElement>, "children"> & {
  label: string;
  flat?: boolean;
  loading?: boolean;
  size?: number;
} & (
  | { variant: "custom"; children: ReactNode; icon?: never }
  | { variant?: never; children?: never; icon: IconName }
);

export function IconBtn({
  icon,
  label,
  flat,
  loading,
  className,
  size = 24,
  type = "button",
  variant,
  children,
  ...rest
}: IconBtnProps) {
  const flatClass = flat
    ? "sb-iconbtn-flat border-current border-none bg-transparent"
    : "border-2 border-b-4 border-solid border-line-strong bg-surface";
  const loadingClass = loading ? "sb-is-loading pointer-events-none" : "";
  const buttonClass = variant === "custom"
    ? className
    : `${iconBtnBaseClass} ${flatClass} ${loadingClass} ${className ?? ""}`;
  return (
    <button
      type={type}
      aria-label={label}
      title={label}
      className={buttonClass}
      {...rest}
    >
      {children ?? (
        <Icon
          name={loading ? "loader" : icon}
          size={size}
          className={loading ? "sb-spin animate-[sbSpin_1s_linear_infinite] motion-reduce:animate-none" : undefined}
        />
      )}
    </button>
  );
}
