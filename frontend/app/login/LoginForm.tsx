"use client";

import Image from "next/image";
import { useRouter, useSearchParams } from "next/navigation";
import { useState } from "react";

export default function LoginForm() {
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
      const res = await fetch("/api/login", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ username, password }),
      });

      if (!res.ok) {
        const data = await res.json().catch(() => null);
        setError(data?.error || "เข้าสู่ระบบไม่สำเร็จ");
        setLoading(false);
        return;
      }

      const from = searchParams.get("from") || "/";
      router.push(from);
      router.refresh();
    } catch {
      setError("เกิดข้อผิดพลาด กรุณาลองใหม่อีกครั้ง");
      setLoading(false);
    }
  }

  return (
    <div className="w-full max-w-sm rounded-2xl border border-navy/10 bg-white p-8 shadow-[0_20px_60px_-38px_rgba(11,32,69,0.45)]">
      <div className="mb-6 flex flex-col items-center gap-3 text-center">
        <Image
          src="/brand/logo.png"
          alt="School Bot"
          width={626}
          height={100}
          priority
          className="h-10 w-auto"
        />
        <p className="font-body text-sm text-ink/70">
          เข้าสู่ระบบเพื่อดูคู่มือครู Robotics &amp; AI
        </p>
      </div>

      <form onSubmit={handleSubmit} className="flex flex-col gap-4">
        <div className="flex flex-col gap-1.5">
          <label htmlFor="username" className="font-body text-sm font-medium text-ink">
            ชื่อผู้ใช้
          </label>
          <input
            id="username"
            name="username"
            type="text"
            autoComplete="username"
            required
            value={username}
            onChange={(event) => setUsername(event.target.value)}
            className="rounded-lg border border-ink/15 bg-white px-3 py-2 font-body text-ink outline-none focus:border-sky focus:ring-2 focus:ring-sky/40"
          />
        </div>

        <div className="flex flex-col gap-1.5">
          <label htmlFor="password" className="font-body text-sm font-medium text-ink">
            รหัสผ่าน
          </label>
          <div className="relative">
            <input
              id="password"
              name="password"
              type={showPassword ? "text" : "password"}
              autoComplete="current-password"
              required
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              className="w-full rounded-lg border border-ink/15 bg-white px-3 py-2 pr-12 font-body text-ink outline-none focus:border-sky focus:ring-2 focus:ring-sky/40"
            />
            <button
              type="button"
              aria-label={showPassword ? "ซ่อนรหัสผ่าน" : "แสดงรหัสผ่าน"}
              aria-pressed={showPassword}
              onClick={() => setShowPassword((value) => !value)}
              className="absolute inset-y-0 right-2 inline-flex items-center rounded-lg px-2 text-ink/55 transition-colors hover:text-navy focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-sky/40"
            >
              {showPassword ? (
                <svg
                  aria-hidden="true"
                  viewBox="0 0 20 20"
                  className="h-5 w-5"
                  fill="none"
                  stroke="currentColor"
                  strokeWidth="1.7"
                  strokeLinecap="round"
                  strokeLinejoin="round"
                >
                  <path d="M2.5 2.5 17.5 17.5" />
                  <path d="M8.75 8.95a2 2 0 0 0 2.3 2.3" />
                  <path d="M6.2 6.2A11.2 11.2 0 0 0 1.75 10c1.63 2.73 4.69 5.5 8.25 5.5 1.56 0 3-.53 4.24-1.33" />
                  <path d="M8.2 4.78A8.8 8.8 0 0 1 10 4.5c3.56 0 6.62 2.77 8.25 5.5a14.3 14.3 0 0 1-2.17 2.64" />
                </svg>
              ) : (
                <svg
                  aria-hidden="true"
                  viewBox="0 0 20 20"
                  className="h-5 w-5"
                  fill="none"
                  stroke="currentColor"
                  strokeWidth="1.7"
                  strokeLinecap="round"
                  strokeLinejoin="round"
                >
                  <path d="M1.75 10C3.38 7.27 6.44 4.5 10 4.5s6.62 2.77 8.25 5.5c-1.63 2.73-4.69 5.5-8.25 5.5S3.38 12.73 1.75 10Z" />
                  <circle cx="10" cy="10" r="2.25" />
                </svg>
              )}
            </button>
          </div>
        </div>

        {error && (
          <p role="alert" className="rounded-lg bg-alert/10 px-3 py-2 text-sm text-alert-dark">
            {error}
          </p>
        )}

        <button
          type="submit"
          disabled={loading}
          className="mt-2 rounded-chip bg-orange px-4 py-2.5 font-display font-semibold text-navy-dark transition-colors hover:bg-orange-dark disabled:opacity-60"
        >
          {loading ? "กำลังเข้าสู่ระบบ..." : "เข้าสู่ระบบ"}
        </button>
      </form>
    </div>
  );
}
