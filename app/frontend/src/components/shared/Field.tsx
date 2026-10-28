import type { InputHTMLAttributes, ReactNode } from "react";

const labelClass = "sb-label mb-[6px] block font-body text-[15px] font-bold leading-[1.3] text-ink";
const inputBaseClass = [
  "sb-input min-h-[52px] w-full rounded-control border-2 border-solid bg-surface px-[16px] py-0",
  "font-body text-[18px] font-medium leading-[1.2] text-ink transition-[border-color] duration-[.12s]",
  "placeholder:text-faint focus:outline-none disabled:cursor-not-allowed disabled:border-off",
  "disabled:bg-off disabled:text-faint",
].join(" ");
const hintBaseClass = "sb-hint mt-[6px] font-body text-[13px] leading-[1.45]";

export function Field({ id, label, hint, error, className, ...rest }: InputHTMLAttributes<HTMLInputElement> & {
  id: string;
  label?: string;
  hint?: ReactNode;
  error?: boolean;
}) {
  const stateClass = error
    ? "sb-is-err border-err shadow-[0_0_0_4px_var(--err-tint)]"
    : "border-line-strong enabled:hover:border-blue focus:border-blue focus:shadow-[0_0_0_4px_var(--blue-tint)]";
  const hintClass = error ? "sb-err font-semibold text-err-ink" : "font-medium text-muted";
  return (
    <div>
      {label && <label className={labelClass} htmlFor={id}>{label}</label>}
      <input
        id={id}
        type="text"
        className={`${inputBaseClass} ${stateClass} ${className ?? ""}`}
        aria-invalid={error || undefined}
        {...rest}
      />
      {hint && <div className={`${hintBaseClass} ${hintClass}`}>{hint}</div>}
    </div>
  );
}
