import Link from "next/link";
import { notFound } from "next/navigation";
import FoundationBreadcrumb from "@/components/FoundationBreadcrumb";
import { gradeThemes } from "@/lib/grade-theme";
import { findLessonPlan, lessonHref } from "@/lib/lesson-plans";
import { curriculumGrades, findCurriculumGrade, totalPeriods } from "@/lib/new-curriculum";

export function generateStaticParams() {
  return curriculumGrades.map((grade) => ({ grade: grade.slug }));
}

export default function CurriculumGradePage({ params }: { params: { grade: string } }) {
  const grade = findCurriculumGrade(params.grade);
  if (!grade) return notFound();

  const theme = gradeThemes[grade.slug];
  const readyCount = grade.weeks.filter((week) => findLessonPlan(grade.slug, week.week)).length;

  return (
    <div className="mx-auto max-w-4xl px-4 py-8 sm:px-6 sm:py-10">
      <FoundationBreadcrumb
        items={[
          { label: "หลักสูตร (ใหม่)", href: "/curriculum" },
          { label: grade.label },
        ]}
      />

      <nav aria-label="เลือกชั้น" className="mb-6 flex flex-wrap gap-2">
        {curriculumGrades.map((item) => {
          const active = item.slug === grade.slug;
          return (
            <Link
              key={item.slug}
              href={`/curriculum/${item.slug}`}
              aria-current={active ? "page" : undefined}
              className={`inline-flex items-center gap-2 rounded-chip px-4 py-1.5 text-sm font-semibold transition-colors ${
                active
                  ? `${gradeThemes[item.slug].tile} text-navy-dark shadow-sm`
                  : `border border-ink/15 bg-white text-ink/70 ${gradeThemes[item.slug].hover} hover:text-ink`
              }`}
            >
              {active ? null : (
                <span aria-hidden="true" className={`h-2 w-2 rounded-full ${gradeThemes[item.slug].dot}`} />
              )}
              {item.label}
            </Link>
          );
        })}
      </nav>

      <header className={`relative mb-10 overflow-hidden rounded-3xl bg-gradient-to-br from-white via-white to-[#FFF3E8] px-6 py-7 shadow-sm ring-1 ${theme.ring} sm:px-8 sm:py-8`}>
        <div
          aria-hidden="true"
          className={`absolute -right-16 -top-20 h-72 w-72 rounded-full opacity-40 blur-3xl ${theme.tile}`}
        />
        <div
          aria-hidden="true"
          className="absolute -bottom-24 left-1/3 h-64 w-64 rounded-full bg-orange/20 blur-3xl"
        />
        <span aria-hidden="true" className="absolute right-[10%] top-[18%] hidden h-5 w-5 rounded-full bg-orange sm:block" />
        <span aria-hidden="true" className="absolute bottom-[22%] right-[18%] hidden h-4 w-4 rotate-12 rounded-md bg-[#FFD44F] sm:block" />
        <span aria-hidden="true" className="absolute right-[5%] bottom-[40%] hidden h-6 w-6 rounded-full border-4 border-sky sm:block" />
        <div className="relative">
          <span className={`inline-flex items-center gap-2 rounded-chip bg-white/90 px-4 py-1.5 text-sm font-semibold text-navy shadow-sm ring-1 ${theme.ring}`}>
            <span className={`h-2 w-2 rounded-full ${theme.dot}`} />
            ชั้นประถมศึกษาปีที่ {grade.grade}
          </span>
          <h1 className="mt-3 font-display text-3xl font-semibold leading-tight text-navy sm:text-4xl">
            {grade.theme}
          </h1>
          <ul className="mt-4 flex flex-wrap gap-2">
            {grade.highlights.map((item) => (
              <li
                key={item}
                className="inline-flex items-center gap-1.5 rounded-chip bg-white/80 px-3 py-1 text-sm text-ink/75 ring-1 ring-ink/10"
              >
                <span aria-hidden="true" className={`h-1.5 w-1.5 rounded-full ${theme.dot}`} />
                {item}
              </li>
            ))}
          </ul>
          <div className="mt-5 flex flex-wrap gap-2 text-sm font-semibold">
            <span className="rounded-chip bg-white px-4 py-2 text-ink shadow-sm">
              {grade.weeks.length} สัปดาห์
            </span>
            <span className="rounded-chip bg-white px-4 py-2 text-ink shadow-sm">
              {totalPeriods(grade)} คาบเรียน
            </span>
            <span className="rounded-chip bg-orange px-4 py-2 text-navy-dark shadow-sm">
              แผนการสอนพร้อมใช้ {readyCount} สัปดาห์
            </span>
          </div>
        </div>
      </header>

      <section aria-labelledby="weeks-heading">
        <h2 id="weeks-heading" className="mb-4 flex items-center gap-2.5 font-display text-xl font-semibold text-navy">
          <span aria-hidden="true" className={`h-6 w-1.5 rounded-full ${theme.tile}`} />
          โครงหลักสูตรรายสัปดาห์
        </h2>

        <ol className="overflow-hidden rounded-3xl border border-ink/10 bg-white">
          {grade.weeks.map((week) => {
            const plan = findLessonPlan(grade.slug, week.week);
            const content = (
              <>
                <span
                  className={`flex h-10 w-10 shrink-0 items-center justify-center rounded-full text-sm font-bold ${
                    plan ? `${theme.soft} ${theme.accent} ring-1 ${theme.ring}` : "bg-ink/5 text-ink/45"
                  }`}
                >
                  {week.week}
                </span>
                <div className="min-w-0 flex-1">
                  <p className={`text-xs font-semibold ${plan ? theme.accent : "text-ink/45"}`}>สัปดาห์ที่ {week.week}</p>
                  <p className="font-medium leading-snug text-ink">{week.title}</p>
                  {plan ? (
                    <p className="mt-0.5 text-sm text-ink/55">
                      {plan.unitLabel}: {plan.title} — {plan.subtitle}
                    </p>
                  ) : null}
                </div>
                <div className="flex shrink-0 flex-col items-end gap-1.5 sm:flex-row sm:items-center sm:gap-3">
                  {plan?.worksheets?.length ? (
                    <span className="rounded-chip bg-orange/20 px-3 py-1 text-xs font-semibold text-orange-dark">
                      📄 มีใบงาน
                    </span>
                  ) : null}
                  <span className="rounded-chip bg-ink/5 px-3 py-1 text-xs font-semibold text-ink/70">
                    {week.periods} คาบ
                  </span>
                  {plan ? (
                    <span className={`text-sm font-semibold ${theme.accent}`}>อ่านแผน →</span>
                  ) : (
                    <span className="text-xs text-ink/45">กำลังจัดทำ</span>
                  )}
                </div>
              </>
            );

            return (
              <li key={week.week} className="border-b border-ink/10 last:border-b-0">
                {plan ? (
                  <Link
                    href={lessonHref(grade.slug, week.week)}
                    className={`flex items-center gap-4 px-4 py-4 transition-colors ${theme.hover} sm:px-6`}
                  >
                    {content}
                  </Link>
                ) : (
                  <div className="flex items-center gap-4 px-4 py-4 sm:px-6">{content}</div>
                )}
              </li>
            );
          })}
        </ol>
      </section>
    </div>
  );
}
