"use client";

import React, { useEffect } from "react";
import { MdCheckCircle, MdError, MdHelpOutline } from "react-icons/md";
import type { ModalType } from "../_hooks/use-content-management";

export interface StatusModalProps {
  open: boolean;
  type: ModalType;
  title: string;
  message: string;
  loading?: boolean;
  onClose: () => void;
  onConfirm?: () => void;
}

export function StatusModal({
  open,
  type,
  title,
  message,
  loading = false,
  onClose,
  onConfirm,
}: StatusModalProps) {
  useEffect(() => {
    if (!open) return;
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === "Escape" && !loading) {
        onClose();
      }
    };
    window.addEventListener("keydown", handleKeyDown);
    return () => window.removeEventListener("keydown", handleKeyDown);
  }, [open, loading, onClose]);

  if (!open) return null;

  return (
    <div
      role="dialog"
      aria-modal="true"
      aria-labelledby="status-modal-title"
      className="fixed inset-0 z-50 flex items-center justify-center p-4"
    >
      <div
        className="fixed inset-0 bg-navy/50 backdrop-blur-sm transition-opacity"
        onClick={() => !loading && onClose()}
      />

      <div className="relative z-10 w-full max-w-md rounded-2xl border border-navy/15 bg-white p-6 shadow-2xl transition-all">
        <div className="flex items-start gap-4">
          <div className="shrink-0 pt-0.5">
            {type === "success" && <MdCheckCircle className="text-3xl text-emerald-500" />}
            {type === "error" && <MdError className="text-3xl text-red-500" />}
            {type === "confirm" && <MdHelpOutline className="text-3xl text-orange" />}
          </div>
          <div className="flex-1 space-y-1">
            <h3 id="status-modal-title" className="font-display text-base font-bold text-navy">
              {title}
            </h3>
            <p className="font-body text-xs leading-relaxed text-ink/75">{message}</p>
          </div>
        </div>

        <div className="mt-6 flex justify-end gap-3 border-t border-navy/10 pt-4">
          {type === "confirm" ? (
            <>
              <button
                type="button"
                disabled={loading}
                onClick={onClose}
                className="rounded-xl border border-navy/20 bg-white px-4 py-2 text-xs font-semibold text-navy hover:bg-paper-dim disabled:opacity-50 transition"
              >
                ยกเลิก
              </button>
              <button
                type="button"
                disabled={loading}
                onClick={onConfirm}
                className="inline-flex items-center gap-2 rounded-xl bg-orange px-4 py-2 text-xs font-semibold text-white shadow-sm hover:bg-orange/90 disabled:opacity-50 transition"
              >
                {loading && (
                  <span className="inline-block h-3.5 w-3.5 animate-spin rounded-full border-2 border-white border-t-transparent" />
                )}
                <span>ยืนยัน</span>
              </button>
            </>
          ) : (
            <button
              type="button"
              onClick={onClose}
              className="rounded-xl bg-navy px-4 py-2 text-xs font-semibold text-white hover:bg-navy/90 transition"
            >
              ตกลง
            </button>
          )}
        </div>
      </div>
    </div>
  );
}
