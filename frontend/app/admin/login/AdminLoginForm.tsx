"use client";

import Image from "next/image";
import Link from "next/link";
import { useRouter, useSearchParams } from "next/navigation";
import { useState } from "react";
import { MdAdminPanelSettings, MdArrowBack } from "react-icons/md";

export default function AdminLoginForm() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  async function handleSubmit(event: React.FormEvent) {
    event.preventDefault();
    setError(null);
    setLoading(true);

    try {
      const res = await fetch("/api/admin/login", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ username, password }),
      });

      if (!res.ok) {
        const data = await res.json().catch(() => null);
        setError(data?.error || "เข้าสู่ระบบผู้ดูแลระบบไม่สำเร็จ");
        setLoading(false);
        return;
      }

      const from = searchParams.get("from") || "/admin";
      router.push(from);
      router.refresh();
    } catch {
      setError("เกิดข้อผิดพลาดในการเชื่อมต่อ กรุณาลองใหม่อีกครั้ง");
      setLoading(false);
    }
  }

  return (
    <div className="w-full max-w-md rounded-3xl border border-navy/15 bg-white p-8 shadow-[0_20px_60px_-20px_rgba(11,32,69,0.35)]">
      <div className="mb-6 flex flex-col items-center gap-3 text-center">
        <div className="flex h-14 w-14 items-center justify-center rounded-2xl bg-navy text-orange shadow-md">
          <MdAdminPanelSettings className="text-3xl" />
        </div>
        <div className="space-y-1">
          <h1 className="font-display text-xl font-bold text-navy">
            เข้าสู่ระบบผู้ดูแลระบบ
          </h1>
          <p className="font-body text-xs text-ink/70">
            School Bot Content Admin Console (TAdmin)
          </p>
        </div>
      </div>

      <form onSubmit={handleSubmit} className="flex flex-col gap-4">
        {error && (
          <div role="alert" className="rounded-xl border border-red-200 bg-red-50 p-3 text-xs text-red-700">
            {error}
          </div>
        )}

        <div className="flex flex-col gap-1.5">
          <label htmlFor="admin-username" className="font-body text-xs font-semibold text-ink/80">
            ชื่อผู้ใช้งานผู้ดูแลระบบ
          </label>
          <input
            id="admin-username"
            name="username"
            type="text"
            autoComplete="username"
            required
            value={username}
            onChange={(e) => setUsername(e.target.value)}
            placeholder="ชื่อผู้ใช้ TAdmin"
            className="rounded-xl border border-navy/20 bg-white px-3.5 py-2.5 text-sm text-ink outline-none transition focus:border-orange focus:ring-2 focus:ring-orange/20"
          />
        </div>

        <div className="flex flex-col gap-1.5">
          <label htmlFor="admin-password" className="font-body text-xs font-semibold text-ink/80">
            รหัสผ่าน
          </label>
          <div className="relative">
            <input
              id="admin-password"
              name="password"
              type={showPassword ? "text" : "password"}
              autoComplete="current-password"
              required
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="รหัสผ่าน"
              className="w-full rounded-xl border border-navy/20 bg-white px-3.5 py-2.5 pr-12 text-sm text-ink outline-none transition focus:border-orange focus:ring-2 focus:ring-orange/20"
            />
            <button
              type="button"
              aria-label={showPassword ? "ซ่อนรหัสผ่าน" : "แสดงรหัสผ่าน"}
              onClick={() => setShowPassword((v) => !v)}
              className="absolute inset-y-0 right-2 inline-flex items-center px-2 text-xs text-ink/50 hover:text-navy"
            >
              {showPassword ? "ซ่อน" : "แสดง"}
            </button>
          </div>
        </div>

        <button
          type="submit"
          disabled={loading}
          className="mt-2 inline-flex w-full items-center justify-center gap-2 rounded-xl bg-orange py-3 text-sm font-semibold text-white shadow-sm transition hover:bg-orange/90 disabled:cursor-not-allowed disabled:opacity-50"
        >
          {loading ? (
            <span className="inline-block h-4 w-4 animate-spin rounded-full border-2 border-white border-t-transparent" />
          ) : (
            "เข้าสู่ระบบ Admin"
          )}
        </button>

        <div className="mt-4 flex items-center justify-between border-t border-navy/10 pt-4 text-xs text-ink/60">
          <Link
            href="/"
            className="inline-flex items-center gap-1 hover:text-navy transition"
          >
            <MdArrowBack />
            <span>กลับหน้าหลัก</span>
          </Link>
          <Link
            href="/login"
            className="hover:text-orange transition font-medium"
          >
            เข้าสู่ระบบครูผู้สอน (TUser)
          </Link>
        </div>
      </form>
    </div>
  );
}
