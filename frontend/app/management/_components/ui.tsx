"use client";

import React from "react";

export function Card({
  title,
  icon,
  description,
  children,
}: {
  title: string;
  icon?: React.ReactNode;
  description?: string;
  children: React.ReactNode;
}) {
  return (
    <div className="rounded-2xl border border-navy/10 bg-white p-6 shadow-sm transition duration-200">
      <div className="mb-4 flex items-center gap-3 border-b border-navy/10 pb-4">
        {icon && <span className="text-2xl text-orange">{icon}</span>}
        <div>
          <h2 className="font-display text-lg font-bold text-navy">{title}</h2>
          {description && <p className="text-xs text-ink/60">{description}</p>}
        </div>
      </div>
      <div>{children}</div>
    </div>
  );
}

export function Button({
  content,
  onClick,
  disabled = false,
  pending = false,
  variant = "primary",
  icon,
  iconOnly = false,
  title,
  "aria-label": ariaLabel,
}: {
  content?: string;
  onClick?: () => void;
  disabled?: boolean;
  pending?: boolean;
  variant?: "primary" | "secondary-outline" | "ghost" | "danger";
  icon?: React.ReactNode;
  iconOnly?: boolean;
  title?: string;
  "aria-label"?: string;
}) {
  let baseStyle =
    "inline-flex items-center justify-center gap-2 rounded-xl text-sm font-medium transition duration-150 disabled:opacity-50 disabled:cursor-not-allowed";

  let variantStyle = "bg-orange text-white hover:bg-orange/90 shadow-sm";
  if (variant === "secondary-outline") {
    variantStyle = "border border-navy/20 bg-white text-navy hover:bg-paper-dim";
  } else if (variant === "ghost") {
    variantStyle = "text-ink/70 hover:bg-sky/15 hover:text-navy";
  } else if (variant === "danger") {
    variantStyle = "bg-red-500 text-white hover:bg-red-600";
  }

  const paddingStyle = iconOnly ? "p-2 h-9 w-9" : "px-4 py-2.5";

  return (
    <button
      type="button"
      title={title}
      aria-label={ariaLabel ?? content}
      disabled={disabled || pending}
      onClick={onClick}
      className={`${baseStyle} ${variantStyle} ${paddingStyle}`}
    >
      {pending ? (
        <span className="inline-block h-4 w-4 animate-spin rounded-full border-2 border-current border-t-transparent" />
      ) : (
        icon
      )}
      {!iconOnly && content}
    </button>
  );
}

export function Input({
  value,
  onChange,
  type = "text",
  placeholder,
  multiline = false,
  maxLength,
  disabled = false,
  "aria-label": ariaLabel,
}: {
  value: string;
  onChange: (val: string) => void;
  type?: string;
  placeholder?: string;
  multiline?: boolean;
  maxLength?: number;
  disabled?: boolean;
  "aria-label"?: string;
}) {
  const commonClasses =
    "w-full rounded-xl border border-navy/20 bg-white px-3 py-2 text-sm text-ink placeholder:text-ink/40 focus:border-orange focus:outline-none focus:ring-1 focus:ring-orange disabled:bg-paper-dim disabled:opacity-60";

  if (multiline) {
    return (
      <textarea
        value={value}
        onChange={(e) => onChange(e.target.value)}
        placeholder={placeholder}
        maxLength={maxLength}
        disabled={disabled}
        aria-label={ariaLabel}
        rows={3}
        className={commonClasses}
      />
    );
  }

  return (
    <input
      type={type}
      value={value}
      onChange={(e) => onChange(e.target.value)}
      placeholder={placeholder}
      maxLength={maxLength}
      disabled={disabled}
      aria-label={ariaLabel}
      className={commonClasses}
    />
  );
}

export function NativeSelect({
  value,
  onChange,
  children,
  disabled = false,
  "aria-label": ariaLabel,
}: {
  value: string | number;
  onChange: (e: React.ChangeEvent<HTMLSelectElement>) => void;
  children: React.ReactNode;
  disabled?: boolean;
  "aria-label"?: string;
}) {
  return (
    <select
      value={value}
      onChange={onChange}
      disabled={disabled}
      aria-label={ariaLabel}
      className="w-full rounded-xl border border-navy/20 bg-white px-3 py-2 text-sm text-ink focus:border-orange focus:outline-none focus:ring-1 focus:ring-orange disabled:bg-paper-dim disabled:opacity-60"
    >
      {children}
    </select>
  );
}

export function DataTable<T>({
  rows,
  rowKey,
  columns,
  loading = false,
  emptyText = "ไม่มีข้อมูล",
}: {
  rows: T[];
  rowKey: (item: T) => string | number;
  columns: {
    id: string;
    header: string;
    cell: (item: T) => React.ReactNode;
  }[];
  loading?: boolean;
  emptyText?: string;
}) {
  return (
    <div className="overflow-x-auto rounded-xl border border-navy/10 bg-white">
      <table className="min-w-full divide-y divide-navy/10 text-left text-sm">
        <thead className="bg-paper-dim text-xs font-semibold uppercase text-ink/60">
          <tr>
            {columns.map((col) => (
              <th key={col.id} className="px-4 py-3 font-medium">
                {col.header}
              </th>
            ))}
          </tr>
        </thead>
        <tbody className="divide-y divide-navy/5">
          {loading ? (
            <tr>
              <td colSpan={columns.length} className="px-4 py-6 text-center text-ink/50">
                <span className="inline-block h-5 w-5 animate-spin rounded-full border-2 border-orange border-t-transparent" />
              </td>
            </tr>
          ) : rows.length === 0 ? (
            <tr>
              <td colSpan={columns.length} className="px-4 py-6 text-center text-ink/50">
                {emptyText}
              </td>
            </tr>
          ) : (
            rows.map((row) => (
              <tr key={rowKey(row)} className="hover:bg-paper-dim/40 transition">
                {columns.map((col) => (
                  <td key={col.id} className="px-4 py-3 text-ink/80">
                    {col.cell(row)}
                  </td>
                ))}
              </tr>
            ))
          )}
        </tbody>
      </table>
    </div>
  );
}

export function ApiErrorText({ error }: { error?: unknown }) {
  if (!error) return null;
  const message =
    error instanceof Error
      ? (error as any).messageTh || error.message
      : typeof error === "string"
      ? error
      : "เกิดข้อผิดพลาดในการโหลดข้อมูล";

  return (
    <div className="rounded-xl border border-red-200 bg-red-50 p-3 text-xs text-red-700">
      {message}
    </div>
  );
}
