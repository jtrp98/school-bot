import Link from "next/link";
import FoundationBreadcrumb from "@/components/FoundationBreadcrumb";
import { gradeThemes } from "@/lib/grade-theme";
import { lessonPlans } from "@/lib/lesson-plans";
import { curriculumGrades, totalPeriods } from "@/lib/new-curriculum";

export default function CurriculumOverviewPage() {
  return (
    <div className="mx-auto max-w-5xl px-4 py-8 sm:px-6 sm:py-10">
      <FoundationBreadcrumb items={[{ label: "หลักสูตร (ใหม่)" }]} />

      <header className="relative mb-10 overflow-hidden rounded-3xl bg-gradient-to-br from-sky-light/70 via-white to-[#FFF3E8] px-6 py-7 shadow-sm ring-1 ring-sky/30 sm:px-8 sm:py-8">
        <div aria-hidden="true" className="absolute -left-20 -bottom-24 h-64 w-64 rounded-full bg-orange/20 blur-3xl" />
        <div aria-hidden="true" className="absolute -right-16 -top-20 h-72 w-72 rounded-full bg-sky/40 blur-3xl" />
        <span aria-hidden="true" className="absolute right-[12%] top-[22%] hidden h-5 w-5 rounded-full bg-orange sm:block" />
        <span aria-hidden="true" className="absolute bottom-[20%] right-[22%] hidden h-4 w-4 rotate-12 rounded-md bg-[#FFD44F] sm:block" />
        <span aria-hidden="true" className="absolute right-[6%] bottom-[28%] hidden h-6 w-6 rounded-full border-4 border-sky sm:block" />
        <div className="relative max-w-3xl">
          <span className="inline-flex items-center gap-2 rounded-chip bg-white/90 px-4 py-1.5 text-sm font-semibold text-navy shadow-sm ring-1 ring-orange/30">
            <span className="h-2 w-2 rounded-full bg-orange" />
            หลักสูตร School Bot
          </span>
          <h1 className="mt-3 font-display text-3xl font-semibold leading-tight text-navy sm:text-4xl">
            หลักสูตร ป.1–ป.6
          </h1>
          <p className="mt-2 text-sm leading-6 text-ink/75 sm:text-base">
            โครงหลักสูตรรายสัปดาห์ของแต่ละชั้น พร้อมจำนวนคาบเรียน
            เปิดอ่านแผนการสอนเป็นบทความได้ทันทีในสัปดาห์ที่เนื้อหาพร้อมแล้ว
          </p>
        </div>
      </header>

      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {curriculumGrades.map((grade) => {
          const theme = gradeThemes[grade.slug];
          const ready = lessonPlans.filter((plan) => plan.gradeSlug === grade.slug).length;

          return (
            <Link
              key={grade.slug}
              href={`/curriculum/${grade.slug}`}
              className={`group flex h-full flex-col rounded-3xl border-2 bg-white bg-gradient-to-b to-white to-55% p-6 shadow-sm transition-all duration-200 hover:-translate-y-1 hover:shadow-xl motion-reduce:transition-none motion-reduce:hover:translate-y-0 ${theme.card}`}
            >
              <div className="flex items-center justify-between gap-3">
                <span className={`rounded-2xl px-3.5 py-1.5 font-display text-lg font-semibold text-navy-dark shadow-sm transition-transform group-hover:-rotate-6 group-hover:scale-110 motion-reduce:transform-none ${theme.tile}`}>
                  {grade.label}
                </span>
                {ready > 0 ? (
                  <span className={`rounded-chip bg-white px-3 py-1 text-xs font-semibold shadow-sm ring-1 ${theme.badge}`}>
                    แผนพร้อมใช้ {ready} สัปดาห์
                  </span>
                ) : null}
              </div>
              <h2 className="mt-4 font-display text-xl font-semibold leading-snug text-navy">
                {grade.theme}
              </h2>
              <ul className="mb-5 mt-3 space-y-1.5 text-sm leading-6 text-ink/70">
                {grade.highlights.map((item) => (
                  <li key={item} className="flex gap-2">
                    <span aria-hidden="true" className={`mt-2.5 h-1.5 w-1.5 shrink-0 rounded-full ${theme.dot}`} />
                    {item}
                  </li>
                ))}
              </ul>
              <div className={`mt-auto flex items-center justify-between gap-3 border-t border-dashed pt-4 text-sm ${theme.line}`}>
                <span className="text-ink/55">
                  {grade.weeks.length} สัปดาห์ · {totalPeriods(grade)} คาบ
                </span>
                <span className={`font-semibold transition-transform group-hover:translate-x-0.5 ${theme.accent}`}>
                  ดูโครงหลักสูตร →
                </span>
              </div>
            </Link>
          );
        })}
      </div>
    </div>
  );
}
