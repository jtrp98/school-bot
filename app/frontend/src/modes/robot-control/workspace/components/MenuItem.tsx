import { Icon, type IconName } from "../../../../components/shared/Icons";
import type { ComponentPropsWithoutRef, ReactNode } from "react";

type MenuItemProps = Omit<ComponentPropsWithoutRef<"button">, "children" | "role"> & {
  icon?: IconName;
  shortcut?: string;
  detail?: string;
  tone?: "default" | "danger" | "muted";
  size?: "lg" | "md";
  children: ReactNode;
};

const SIZE_CLASS = {
  lg: "h-[40px] rounded-[9px] text-[15px]",
  md: "h-[36px] rounded-[8px] text-[14px]",
} as const;

const TONE_CLASS = { default: "text-ink", danger: "text-err-ink", muted: "text-muted" } as const;

export function MenuItem({
  icon,
  shortcut,
  detail,
  tone = "default",
  size = "lg",
  children,
  ...rest
}: MenuItemProps): ReactNode {
  return (
    <button
      type="button"
      role="menuitem"
      {...rest}
      className={[
        "flex w-full cursor-pointer items-center gap-[10px] border-none bg-white px-[10px] text-left",
        "font-[family-name:inherit] font-semibold hover:bg-blue-tint!",
        SIZE_CLASS[size],
        TONE_CLASS[tone],
      ].join(" ")}
    >
      {icon ? <Icon name={icon} className="mr-[.3em] inline-block size-[1.15em] align-[-.2em]" /> : null}
      {children}
      {detail ? <span className="ml-auto text-[12px] text-faint">{detail}</span> : null}
      {shortcut ? (
        <kbd
          className={[
            "ml-auto rounded-[5px] border-1 border-solid border-line border-b-2 bg-sunken px-[6px] py-[1px]",
            "font-[family-name:inherit] text-[11px] font-bold text-muted",
          ].join(" ")}
        >
          {shortcut}
        </kbd>
      ) : null}
    </button>
  );
}
