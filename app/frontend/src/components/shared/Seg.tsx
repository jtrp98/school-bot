import { t as translate } from "../../i18n";
import type { ReactNode } from "react";

const segClass = [
  "sb-seg inline-flex p-[4px] gap-[4px] bg-sunken rounded-control border-2 border-solid border-line",
  "[&_button]:min-h-[44px] [&_button]:py-0 [&_button]:px-[16px] [&_button]:border-[medium]",
  "[&_button]:border-none [&_button]:border-current [&_button]:rounded-[8px]",
  "[&_button]:bg-transparent [&_button]:not-italic [&_button]:[font-variant:normal]",
  "[&_button]:font-bold [&_button]:[font-stretch:normal] [&_button]:text-[15px]",
  "[&_button]:leading-none [&_button]:font-body [&_button]:text-muted [&_button]:cursor-pointer",
  "[&_button:hover]:bg-blue-tint [&_button:hover]:text-blue-ink [&_button.sb-is-hover]:bg-blue-tint",
  "[&_button.sb-is-hover]:text-blue-ink [&_button[aria-pressed=true]]:bg-surface",
  "[&_button[aria-pressed=true]]:text-blue-ink [&_button[aria-pressed=true]]:shadow-pop",
  "[&_button.sb-is-on]:bg-surface [&_button.sb-is-on]:text-blue-ink [&_button.sb-is-on]:shadow-pop",
  "[&_button:focus-visible]:outline-none [&_button:focus-visible]:shadow-[var(--focus)]",
  "[&_button.sb-is-focus]:outline-none [&_button.sb-is-focus]:shadow-[var(--focus)]",
  "[&_button:disabled]:opacity-[.5] [&_button:disabled]:cursor-not-allowed",
  "[&_button.sb-is-disabled]:opacity-[.5] [&_button.sb-is-disabled]:cursor-not-allowed",
].join(" ");

export function Seg<T extends string>({
  value,
  options,
  onChange,
  label,
}: {
  value: T;
  options: { value: T; label: ReactNode; disabled?: boolean }[];
  onChange: (next: T) => void;
  label?: string;
}) {
  return (
    <div className={segClass} role="group" aria-label={label}>
      {options.map((o) => (
        <button
          key={o.value}
          type="button"
          aria-pressed={o.value === value}
          disabled={o.disabled}
          className={o.value === value ? "sb-is-on" : undefined}
          onClick={() => onChange(o.value)}
        >
          {typeof o.label === "string" ? translate(o.label) : o.label}
        </button>
      ))}
    </div>
  );
}
