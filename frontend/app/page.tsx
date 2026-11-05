import Image from "next/image";
import Link from "next/link";
import { lessonPlans } from "@/lib/lesson-plans";
import { gradeThemes } from "@/lib/grade-theme";
import { curriculumGrades, totalPeriods } from "@/lib/new-curriculum";

const quickLinks = [
  { href: "/curriculum", icon: "M4 5.5A1.5 1.5 0 0 1 5.5 4H11v15H5.5A1.5 1.5 0 0 0 4 20.5v-15ZM20 5.5A1.5 1.5 0 0 0 18.5 4H13v15h5.5a1.5 1.5 0 0 1 1.5 1.5v-15Z", tile: "bg-sky", title: "หลักสูตร ป.1–ป.6", description: "โครงหลักสูตรรายสัปดาห์และแผนการสอน" },
  { href: "/robox", icon: "M9 4h6M12 4v3m-5 3h10a2 2 0 0 1 2 2v5a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2v-5a2 2 0 0 1 2-2Zm2.5 4h.01M14.5 14h.01", tile: "bg-[#5FD0BF]", title: "รู้จัก RoboX", description: "องค์ประกอบ เซอร์โว และระบบควบคุม" },
  { href: "/install", icon: "M12 4v11m0 0-4-4m4 4 4-4M5 19h14", tile: "bg-[#FFD44F]", title: "วิธีติดตั้งโปรแกรม", description: "คู่มือ App, PC, Scratch และ Python Editor" },
];

const featurePills = [
  { label: "แผนการสอน", dot: "bg-sky-dark", style: "bg-sky/25 text-navy" },
  { label: "ใบงาน", dot: "bg-orange-dark", style: "bg-orange/25 text-navy" },
  { label: "คู่มือโปรแกรม", dot: "bg-[#E0A800]", style: "bg-[#FFD44F]/50 text-navy" },
  { label: "สื่อประกอบการสอน", dot: "bg-[#2E9E5B]", style: "bg-[#C9F2D6] text-navy" },
];

function ArrowIcon() {
  return (
    <svg aria-hidden="true" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" className="h-5 w-5 shrink-0">
      <path d="M5 12h14m-6-6 6 6-6 6" />
    </svg>
  );
}

export default function Home() {
  const grades = curriculumGrades.map((grade) => ({
    ...grade,
    periods: totalPeriods(grade),
    ready: lessonPlans.filter((plan) => plan.gradeSlug === grade.slug).length,
    styles: gradeThemes[grade.slug],
  }));

  return (
    <div className="bg-white">
      <section className="relative overflow-hidden bg-gradient-to-b from-sky-light/70 via-white to-white">
        <div aria-hidden="true" className="absolute -left-24 top-10 h-72 w-72 rounded-full bg-orange/20 blur-3xl" />
        <div aria-hidden="true" className="absolute -right-24 -top-10 h-80 w-80 rounded-full bg-sky/40 blur-3xl" />
        <div className="mx-auto grid max-w-6xl items-center gap-4 px-6 pb-12 pt-14 md:grid-cols-[1.05fr_0.95fr] md:gap-8 md:pb-20 md:pt-20">
          <div className="relative z-10 max-w-xl">
            <span className="mb-5 inline-flex items-center gap-2 rounded-chip bg-white px-4 py-1.5 text-sm font-semibold text-navy shadow-sm ring-1 ring-orange/30">
              <span aria-hidden="true" className="h-2 w-2 rounded-full bg-orange" /> สำหรับครูผู้สอน ป.1–6
            </span>
            <h1 className="font-display text-[clamp(2.5rem,5vw,4.75rem)] font-semibold leading-[1.14] tracking-[-0.025em] text-navy">
              เริ่มสอน<br />
              <span className="inline-block bg-gradient-to-r from-orange-dark via-orange to-[#FFB62E] bg-clip-text text-transparent">
                Robotics
              </span>{" "}
              ได้เลย
            </h1>
            <p className="mt-6 max-w-[58ch] text-base leading-8 text-ink/75 md:text-lg">
              แผนการสอน ใบงาน คู่มือ และสื่อสำหรับ ป.1–6 อยู่ในที่เดียว
              เลือกชั้นที่สอน แล้วหยิบไปใช้ในห้องเรียนได้ทันที
            </p>
            <div className="mt-8 flex flex-wrap items-center gap-3">
              <Link href="/curriculum" className="inline-flex min-h-12 items-center gap-3 rounded-chip bg-orange px-6 py-3 font-display font-medium text-navy-dark shadow-[0_12px_25px_-15px_rgba(226,115,58,0.55)] transition-[background-color,transform] hover:-translate-y-0.5 hover:bg-orange-dark">
                ดูหลักสูตร ป.1–6 <ArrowIcon />
              </Link>
              <Link href="/curriculum/p1" className="inline-flex min-h-12 items-center rounded-chip border-2 border-sky bg-white px-6 py-3 font-medium text-navy transition-colors hover:bg-sky/15">
                เปิดแผนการสอน ป.1
              </Link>
            </div>
            <div className="mt-9 flex flex-wrap gap-2 text-sm font-semibold">
              {featurePills.map((pill) => (
                <span key={pill.label} className={`inline-flex items-center gap-1.5 rounded-chip px-3.5 py-1.5 ${pill.style}`}>
                  <span aria-hidden="true" className={`h-2 w-2 rounded-full ${pill.dot}`} />{pill.label}
                </span>
              ))}
            </div>
          </div>
          <div className="home-hero-stage relative mx-auto flex aspect-square w-full max-w-[550px] items-center justify-center">
            <div aria-hidden="true" className="home-hero-ring absolute inset-[9%] rounded-full bg-gradient-to-br from-[#FFE3CC] to-[#FFF5EA] ring-4 ring-white" />
            <div aria-hidden="true" className="absolute inset-[17%] rounded-full border-2 border-dashed border-sky/60" />
            <span aria-hidden="true" className="float-a absolute right-[8%] top-[15%] h-7 w-7 rounded-full bg-orange" />
            <span aria-hidden="true" className="float-b absolute bottom-[20%] left-[6%] h-5 w-5 rounded-full bg-sky" />
            <span aria-hidden="true" className="float-b absolute left-[14%] top-[14%] h-4 w-4 rotate-12 rounded-md bg-[#FFD44F]" />
            <span aria-hidden="true" className="float-a absolute bottom-[12%] right-[14%] h-6 w-6 rounded-full border-4 border-[#FFD44F]" />
                        <Image src="/brand/robox-hero.png" alt="แขนกล RoboX สีฟ้าสำหรับการเรียน Robotics" width={1254} height={1254} sizes="(min-width: 768px) 50vw, 100vw" priority className="home-hero-robot relative z-10 h-auto w-full drop-shadow-[0_24px_22px_rgba(25,95,130,0.13)]" />
          </div>
        </div>
      </section>

      <section className="mx-auto max-w-6xl px-6 py-14 md:py-20">
        <div className="mb-8 flex flex-wrap items-end justify-between gap-4">
          <div>
            <h2 className="font-display text-3xl font-semibold text-navy md:text-4xl">เลือกชั้นที่กำลังสอน</h2>
            <p className="mt-2 text-ink/65">หลักสูตร ป.1–ป.6 รายสัปดาห์ พร้อมแผนการสอนแบบบทความ</p>
          </div>
          <Link href="/curriculum" className="text-sm font-semibold text-orange-dark underline decoration-orange/60 underline-offset-4 hover:text-navy">
            ดูภาพรวมหลักสูตรทั้งหมด
          </Link>
        </div>
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {grades.map((grade) => (
            <Link key={grade.slug} href={`/curriculum/${grade.slug}`} className={`group flex h-full flex-col rounded-3xl border-2 bg-white bg-gradient-to-b to-white to-55% p-5 shadow-sm sm:p-6 transition-all duration-200 hover:-translate-y-1 hover:shadow-xl motion-reduce:transition-none motion-reduce:hover:translate-y-0 ${grade.styles.card}`}>
              <div className="flex items-start justify-between gap-3">
                <span aria-hidden="true" className={`flex h-12 min-w-12 items-center justify-center rounded-2xl px-3 font-display text-lg sm:h-14 sm:min-w-14 sm:text-xl font-semibold text-navy-dark shadow-sm transition-transform group-hover:-rotate-6 group-hover:scale-110 motion-reduce:transform-none ${grade.styles.tile}`}>
                  {grade.label}
                </span>
                {grade.ready > 0 ? (
                  <span className={`rounded-chip bg-white px-3 py-1 text-xs font-semibold shadow-sm ring-1 ${grade.styles.badge}`}>
                    แผนพร้อมใช้ {grade.ready} สัปดาห์
                  </span>
                ) : null}
              </div>
              <h3 className="mt-4 font-display text-lg sm:mt-5 sm:text-xl font-semibold leading-snug text-navy">
                <span className="sr-only">{grade.label} </span>{grade.theme}
              </h3>
              <ul className="mt-3 hidden flex-1 space-y-1.5 text-sm leading-6 text-ink/70 sm:block">
                {grade.highlights.slice(0, 3).map((item) => (
                  <li key={item} className="flex gap-2">
                    <span aria-hidden="true" className={`mt-2.5 h-1.5 w-1.5 shrink-0 rounded-full ${grade.styles.dot}`} />
                    {item}
                  </li>
                ))}
              </ul>
              <div className={`mt-4 flex items-center justify-between gap-3 border-t border-dashed pt-3 text-sm sm:mt-6 sm:pt-4 ${grade.styles.line}`}>
                <span className="text-ink/60">{grade.weeks.length} สัปดาห์ · {grade.periods} คาบ</span>
                <span className={`inline-flex items-center gap-1.5 font-semibold ${grade.styles.accent}`}>
                  ดูหลักสูตร
                  <span className="transition-transform group-hover:translate-x-1 motion-reduce:transform-none"><ArrowIcon /></span>
                </span>
              </div>
            </Link>
          ))}
        </div>
      </section>

      <section className="bg-gradient-to-b from-[#FFF4E6] to-[#FFF9F3]">
        <div className="mx-auto max-w-6xl px-6 py-14 md:py-16">
          <h2 className="font-display text-2xl font-semibold text-navy md:text-3xl">เครื่องมือที่ครูใช้บ่อย</h2>
          <div className="mt-6 grid gap-3 sm:grid-cols-3">
            {quickLinks.map((item) => (
              <Link key={item.href} href={item.href} className="group flex items-center gap-4 rounded-2xl border-2 border-transparent bg-white p-4 shadow-sm transition-all hover:-translate-y-1 hover:border-orange hover:shadow-lg motion-reduce:transition-none motion-reduce:hover:translate-y-0 sm:flex-col sm:items-stretch sm:gap-0 sm:p-5">
                <div className="flex shrink-0 items-center justify-between gap-3">
                  <span aria-hidden="true" className={`flex h-12 w-12 shrink-0 items-center justify-center rounded-2xl text-navy-dark transition-transform group-hover:rotate-6 motion-reduce:transform-none ${item.tile}`}>
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" className="h-6 w-6"><path d={item.icon} /></svg>
                  </span>
                  <span className="hidden text-orange-dark transition-transform group-hover:translate-x-1 motion-reduce:transform-none sm:block"><ArrowIcon /></span>
                </div>
                <div className="min-w-0 flex-1">
                  <h3 className="font-display text-lg font-semibold text-navy sm:mt-4">{item.title}</h3>
                  <p className="mt-1 text-sm leading-6 text-ink/65">{item.description}</p>
                </div>
                <span className="text-orange-dark sm:hidden"><ArrowIcon /></span>
              </Link>
            ))}
          </div>
        </div>
      </section>
    </div>
  );
}
