"use client";

import Image from "next/image";
import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { useEffect, useRef, useState } from "react";
import { gradeLevels } from "@/lib/grade-levels";
import { curriculumGrades } from "@/lib/new-curriculum";

const utilityLinks = [
  { href: "/management", label: "จัดการเรียนการสอน" },
  { href: "/admin", label: "Admin หลักสูตร" },
  { href: "/robox", label: "RoboX" },
  { href: "/install", label: "ติดตั้ง" },
];

function UtilityNavItem({
  href,
  label,
  active,
  className = "",
  onClick,
}: {
  href: string;
  label: string;
  active: boolean;
  className?: string;
  onClick?: () => void;
}) {
  return (
    <Link
      href={href}
      onClick={onClick}
      aria-current={active ? "page" : undefined}
      className={`rounded-xl px-3 py-2 text-sm leading-tight transition-colors duration-200 ${
        active
          ? "bg-sky/20 text-navy"
          : "bg-white text-ink/72 hover:bg-sky/10 hover:text-navy"
      } ${className}`}
    >
      {label}
    </Link>
  );
}

function LogoutIcon() {
  return (
    <svg
      aria-hidden="true"
      viewBox="0 0 20 20"
      className="h-4 w-4"
      fill="none"
      stroke="currentColor"
      strokeWidth="1.75"
      strokeLinecap="round"
      strokeLinejoin="round"
    >
      <path d="M7.5 17.5h-3a1 1 0 0 1-1-1v-13a1 1 0 0 1 1-1h3" />
      <path d="M13 14l4-4-4-4" />
      <path d="M17 10H7.5" />
    </svg>
  );
}

interface MenuItem {
  href: string;
  label: string;
  meta: string;
  active: boolean;
}

interface MenuFooter {
  title: string;
  items: MenuItem[];
}

// รายการลิงก์ของเมนูหลักสูตร ใช้ร่วมกันทั้ง dropdown บนจอใหญ่และแผงเมนูบนมือถือ
function MenuSections({
  items,
  footer,
  onNavigate,
}: {
  items: MenuItem[];
  footer?: MenuFooter;
  onNavigate?: () => void;
}) {
  return (
    <>
      <div className="p-2">
        {items.map((item) => (
          <Link
            key={item.href}
            href={item.href}
            aria-current={item.active ? "page" : undefined}
            className={`flex items-center justify-between gap-4 rounded-xl px-3 py-2.5 text-sm transition-colors duration-200 ${
              item.active
                ? "bg-orange/18 text-navy"
                : "text-ink/78 hover:bg-sky/10 hover:text-navy"
            }`}
            onClick={onNavigate}
          >
            <span className="shrink-0 font-medium">{item.label}</span>
            <span className="truncate text-xs text-ink/45">{item.meta}</span>
          </Link>
        ))}
      </div>
      {footer ? (
        <div className="border-t border-navy/10 bg-paper-dim/70 p-2">
          <p className="px-3 pb-1 pt-1.5 text-xs font-semibold text-ink/45">{footer.title}</p>
          {footer.items.map((item) => (
            <Link
              key={item.href}
              href={item.href}
              aria-current={item.active ? "page" : undefined}
              className={`flex items-center justify-between gap-4 rounded-xl px-3 py-2 text-sm transition-colors duration-200 ${
                item.active
                  ? "bg-navy/[0.07] text-ink/80"
                  : "text-ink/55 hover:bg-navy/5 hover:text-ink/80"
              }`}
              onClick={onNavigate}
            >
              <span>{item.label}</span>
              <span className="text-xs text-ink/40">{item.meta}</span>
            </Link>
          ))}
        </div>
      ) : null}
    </>
  );
}

function DropdownMenu({
  label,
  active,
  items,
  footer,
}: {
  label: string;
  active: boolean;
  items: MenuItem[];
  footer?: MenuFooter;
}) {
  const [open, setOpen] = useState(false);
  const menuRef = useRef<HTMLDivElement | null>(null);

  useEffect(() => {
    if (!open) {
      return;
    }

    function handlePointerDown(event: MouseEvent | TouchEvent) {
      if (!menuRef.current?.contains(event.target as Node)) {
        setOpen(false);
      }
    }

    function handleEscape(event: KeyboardEvent) {
      if (event.key === "Escape") {
        setOpen(false);
      }
    }

    document.addEventListener("mousedown", handlePointerDown);
    document.addEventListener("touchstart", handlePointerDown);
    document.addEventListener("keydown", handleEscape);

    return () => {
      document.removeEventListener("mousedown", handlePointerDown);
      document.removeEventListener("touchstart", handlePointerDown);
      document.removeEventListener("keydown", handleEscape);
    };
  }, [open]);

  return (
    <div
      ref={menuRef}
      className="relative"
      onMouseEnter={() => setOpen(true)}
      onMouseLeave={() => setOpen(false)}
    >
      <button
        type="button"
        aria-haspopup="menu"
        aria-expanded={open}
        onClick={() => setOpen((value) => !value)}
        className={`flex items-center justify-between gap-2 rounded-xl px-3 py-2 text-sm transition-colors duration-200 ${
          active
            ? "bg-orange/25 text-navy"
            : "bg-sky/15 text-navy hover:bg-sky/25"
        }`}
      >
        <span>{label}</span>
        <svg
          aria-hidden="true"
          viewBox="0 0 20 20"
          className={`h-4 w-4 transition-transform duration-200 ${open ? "rotate-180" : ""}`}
          fill="none"
          stroke="currentColor"
          strokeWidth="1.8"
          strokeLinecap="round"
          strokeLinejoin="round"
        >
          <path d="M5 7.5 10 12.5l5-5" />
        </svg>
      </button>

      {open ? (
        <div className="absolute left-0 top-full z-50 w-max min-w-[220px] pt-2">
          <div className="overflow-hidden rounded-2xl border border-navy/10 bg-white shadow-[0_20px_45px_-22px_rgba(15,23,42,0.35)]">
            <MenuSections items={items} footer={footer} onNavigate={() => setOpen(false)} />
          </div>
        </div>
      ) : null}
    </div>
  );
}

function useCurriculumMenu(activeLevel?: string) {
  const pathname = usePathname();
  const activeGrade = curriculumGrades.find(
    (grade) =>
      pathname === `/curriculum/${grade.slug}` ||
      pathname.startsWith(`/curriculum/${grade.slug}/`),
  );
  const legacyLevel = gradeLevels.find((level) => level.slug === activeLevel);

  return {
    label: activeGrade
      ? `หลักสูตร (ใหม่): ${activeGrade.label}`
      : legacyLevel
        ? `หลักสูตรเก่า: ${legacyLevel.label}`
        : "หลักสูตร (ใหม่)",
    active:
      pathname === "/curriculum" || pathname.startsWith("/curriculum/") || Boolean(activeLevel),
    items: [
      {
        href: "/curriculum",
        label: "ภาพรวม ป.1–ป.6",
        meta: "ทุกชั้น",
        active: pathname === "/curriculum",
      },
      ...curriculumGrades.map((grade) => ({
        href: `/curriculum/${grade.slug}`,
        label: grade.label,
        meta: grade.theme,
        active: activeGrade?.slug === grade.slug,
      })),
    ],
    footer: {
      title: "หลักสูตรเก่า",
      items: gradeLevels.map((level) => ({
        href: `/grades/${level.slug}`,
        label: level.label,
        meta: `ป.${level.grades[0]}-${level.grades[level.grades.length - 1]}`,
        active: activeLevel === level.slug,
      })),
    },
  };
}

export default function Nav() {
  const pathname = usePathname();
  const router = useRouter();
  const [mobileOpen, setMobileOpen] = useState(false);

  async function handleLogout() {
    await fetch("/api/login", { method: "DELETE" });
    router.push("/login");
    router.refresh();
  }

  const activeLevel = gradeLevels.find(
    (level) =>
      pathname === `/grades/${level.slug}` ||
      level.grades.some((grade) => pathname === `/grades/${grade}`),
  )?.slug;
  const curriculumMenu = useCurriculumMenu(activeLevel);

  useEffect(() => {
    if (!mobileOpen) {
      return;
    }

    function handleEscape(event: KeyboardEvent) {
      if (event.key === "Escape") {
        setMobileOpen(false);
      }
    }

    document.addEventListener("keydown", handleEscape);
    return () => document.removeEventListener("keydown", handleEscape);
  }, [mobileOpen]);

  if (pathname === "/login") {
    return null;
  }

  return (
    <header className="sticky top-0 z-40 border-b border-navy/10 bg-white/95 backdrop-blur supports-[backdrop-filter]:bg-white/90">
      <div className="mx-auto max-w-6xl px-4 py-3 sm:px-6">
        <div className="flex items-center justify-between gap-3">
          <Link href="/" className="inline-flex shrink-0 items-center" onClick={() => setMobileOpen(false)}>
            <span className="relative block h-[26px] w-[166px] shrink-0 overflow-hidden sm:h-[29px] sm:w-48 lg:h-[37px] lg:w-[240px]">
              <Image
                src="/brand/logo.png"
                alt="School Bot"
                fill
                sizes="(min-width: 1024px) 240px, (min-width: 640px) 192px, 166px"
                priority
                className="object-cover object-center"
              />
            </span>
          </Link>

          <div className="hidden items-center gap-2 lg:flex">
            <DropdownMenu {...curriculumMenu} />

            {utilityLinks.map((link) => (
              <UtilityNavItem
                key={link.href}
                href={link.href}
                label={link.label}
                active={pathname === link.href}
              />
            ))}

            <button
              type="button"
              onClick={handleLogout}
              className="inline-flex shrink-0 items-center gap-2 rounded-xl border border-navy/15 bg-white px-3 py-2 text-sm text-navy transition-colors duration-200 hover:bg-paper-dim"
            >
              <LogoutIcon />
              <span>ออกจากระบบ</span>
            </button>
          </div>

          <button
            type="button"
            aria-expanded={mobileOpen}
            aria-controls="mobile-menu"
            aria-label={mobileOpen ? "ปิดเมนู" : "เปิดเมนู"}
            onClick={() => setMobileOpen((value) => !value)}
            className={`flex h-11 w-11 items-center justify-center rounded-xl text-navy transition-colors duration-200 lg:hidden ${
              mobileOpen ? "bg-orange/25" : "bg-sky/15 hover:bg-sky/25"
            }`}
          >
            <svg
              aria-hidden="true"
              viewBox="0 0 24 24"
              className="h-6 w-6"
              fill="none"
              stroke="currentColor"
              strokeWidth="1.8"
              strokeLinecap="round"
            >
              {mobileOpen ? <path d="M6 6l12 12M18 6 6 18" /> : <path d="M4 7h16M4 12h16M4 17h16" />}
            </svg>
          </button>
        </div>

        {mobileOpen ? (
          <nav
            id="mobile-menu"
            aria-label="เมนูหลัก"
            className="-mx-4 mt-3 max-h-[calc(100dvh-4.5rem)] overflow-y-auto border-t border-navy/10 px-4 pb-4 pt-3 sm:-mx-6 sm:px-6 lg:hidden"
          >
            <p className="px-1 text-xs font-semibold text-ink/45">หลักสูตร (ใหม่)</p>
            <div className="mt-2 overflow-hidden rounded-2xl border border-navy/10 bg-white">
              <MenuSections
                items={curriculumMenu.items}
                footer={curriculumMenu.footer}
                onNavigate={() => setMobileOpen(false)}
              />
            </div>

            <div className="mt-3 grid grid-cols-2 gap-2">
              {utilityLinks.map((link) => (
                <UtilityNavItem
                  key={link.href}
                  href={link.href}
                  label={link.label}
                  active={pathname === link.href}
                  className="border border-navy/10 py-3 text-center"
                  onClick={() => setMobileOpen(false)}
                />
              ))}
            </div>

            <button
              type="button"
              onClick={handleLogout}
              className="mt-3 inline-flex w-full items-center justify-center gap-2 rounded-xl border border-navy/15 bg-white px-3 py-3 text-sm text-navy transition-colors duration-200 hover:bg-paper-dim"
            >
              <LogoutIcon />
              <span>ออกจากระบบ</span>
            </button>
          </nav>
        ) : null}
      </div>
    </header>
  );
}
