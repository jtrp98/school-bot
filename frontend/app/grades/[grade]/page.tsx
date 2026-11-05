import { existsSync } from "node:fs";
import { join } from "node:path";
import Link from "next/link";
import {
  basicSessions,
  type BasicSession,
  type BasicSessionResource,
} from "@/lib/basic-sessions";
import { curriculum, toolLabel, LessonBlock } from "@/lib/curriculum-data";
import { gradeLevels } from "@/lib/grade-levels";
import { intermediateResources } from "@/lib/intermediate-resources";
import { notFound } from "next/navigation";

export function generateStaticParams() {
  return [
    ...curriculum.map((g) => ({ grade: String(g.grade) })),
    ...gradeLevels.map((level) => ({ grade: level.slug })),
  ];
}

function BlockCard({ block, index }: { block: LessonBlock; index: number }) {
  return (
    <div className="rounded-xl border border-ink/10 bg-white p-5">
      <div className="mb-2 flex items-start justify-between gap-4">
        <h3 className="font-display text-lg font-semibold leading-snug">
          {index + 1}. {block.title}
        </h3>
        <span className="shrink-0 rounded-chip bg-navy px-2.5 py-1 text-xs text-paper">
          {block.hours} ชม.
        </span>
      </div>
      <p className="mb-3 text-sm text-ink/70">{block.summary}</p>
      {block.tools.length > 0 && (
        <div className="flex flex-wrap gap-2">
          {block.tools.map((t) => (
            <span
              key={t}
              className="rounded-chip border border-ink/10 bg-paper-dim px-2 py-1 text-xs text-ink/70"
            >
              {toolLabel[t]}
            </span>
          ))}
        </div>
      )}
    </div>
  );
}

function lessonNumber(resources: BasicSessionResource[], index: number) {
  return resources.slice(0, index + 1).filter((r) => r.type === "lesson").length;
}

function CheckIcon({ className = "" }: { className?: string }) {
  return (
    <svg
      viewBox="0 0 20 20"
      fill="none"
      aria-hidden="true"
      className={className}
    >
      <circle cx="10" cy="10" r="10" fill="currentColor" fillOpacity="0.18" />
      <path
        d="m6 10.4 2.5 2.5L14 7.4"
        stroke="currentColor"
        strokeWidth="2"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
    </svg>
  );
}

function ClipboardIcon({ className = "" }: { className?: string }) {
  return (
    <svg
      viewBox="0 0 24 24"
      fill="none"
      aria-hidden="true"
      className={className}
    >
      <rect x="5" y="4" width="14" height="17" rx="3" stroke="currentColor" strokeWidth="2" />
      <path d="M9 4.5h6V3H9v1.5Z" fill="currentColor" stroke="currentColor" strokeWidth="1.5" />
      <path
        d="m9 13 2 2 4-4.5"
        stroke="currentColor"
        strokeWidth="2"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
    </svg>
  );
}

function ResourceCard({
  resource,
  number,
}: {
  resource: BasicSessionResource;
  number: number;
}) {
  const href = encodeURI(resource.pdfPath);
  const isTest = resource.type === "test";
  const slidesPath = resource.slidesPdfPath;
  const slidesAvailable = slidesPath
    ? existsSync(join(process.cwd(), "public", slidesPath.slice(1)))
    : false;

  if (isTest) {
    return (
      <div className="flex flex-wrap items-center justify-between gap-4 rounded-2xl border-2 border-dashed border-orange/60 bg-gradient-to-r from-orange/15 to-[#FFF4CA]/60 px-5 py-4">
        <div className="flex min-w-0 items-center gap-4">
          <span className="flex h-12 w-12 shrink-0 items-center justify-center rounded-2xl bg-orange text-navy-dark shadow-sm">
            <ClipboardIcon className="h-6 w-6" />
          </span>
          <div className="min-w-0">
            <p className="text-xs font-semibold uppercase tracking-[0.08em] text-orange-dark">
              จุดเช็กความเข้าใจ
            </p>
            <h3 className="font-display text-lg font-semibold text-ink">
              แบบทดสอบ · {resource.periodLabel}
            </h3>
          </div>
        </div>
        <a
          href={href}
          target="_blank"
          rel="noreferrer"
          className="inline-flex items-center gap-2 rounded-chip bg-orange px-5 py-2.5 text-sm font-semibold text-navy-dark shadow-sm transition-all hover:-translate-y-0.5 hover:bg-orange-dark hover:shadow-md"
        >
          ทำแบบทดสอบ
          <span aria-hidden="true">↗</span>
        </a>
      </div>
    );
  }

  return (
    <article className="group rounded-3xl border border-sky/30 bg-white p-6 shadow-sm transition-all hover:-translate-y-0.5 hover:border-sky hover:shadow-lg hover:shadow-sky/20">
      <div className="mb-4 flex flex-wrap items-center gap-2">
        <span className="rounded-chip bg-navy px-3 py-1 text-xs font-semibold text-paper">
          {resource.periodLabel}
        </span>
        <span className="rounded-chip bg-sky/20 px-3 py-1 text-xs font-semibold text-sky-dark">
          คาบเรียน
        </span>
        {resource.unitTitle ? (
          <span className="text-xs text-ink/50">หน่วยการเรียนรู้ที่ {number}</span>
        ) : null}
      </div>

      <h3 className="font-display text-xl font-semibold leading-snug text-ink sm:text-2xl">
        {resource.unitTitle ?? resource.title}
      </h3>

      {resource.topics && resource.topics.length > 0 ? (
        <ul className="mt-4 grid gap-x-6 gap-y-2.5 text-sm leading-6 text-ink/75 md:grid-cols-2">
          {resource.topics.map((topic) => (
            <li key={topic} className="flex items-start gap-2.5">
              <CheckIcon className="mt-1 h-4 w-4 shrink-0 text-sky-dark" />
              <span>{topic}</span>
            </li>
          ))}
        </ul>
      ) : null}

      <div className="mt-6 flex flex-wrap items-center gap-3">
        <a
          href={href}
          target="_blank"
          rel="noreferrer"
          className="inline-flex items-center gap-2 rounded-chip bg-navy px-5 py-2.5 text-sm font-semibold text-paper shadow-sm transition-all hover:bg-navy-light hover:shadow-md"
        >
          ดูแผนการเรียน
          <span aria-hidden="true">↗</span>
        </a>
        {slidesPath ? (
          slidesAvailable ? (
            <a
              href={slidesPath}
              target="_blank"
              rel="noreferrer"
              className="inline-flex items-center gap-2 rounded-chip bg-[#FFD44F] px-5 py-2.5 text-sm font-semibold text-navy-dark shadow-sm transition-all hover:bg-[#F5C534] hover:shadow-md"
            >
              ดูสไลด์
              <span aria-hidden="true">↗</span>
            </a>
          ) : (
            <span className="inline-flex items-center gap-2">
              <button
                type="button"
                disabled
                className="cursor-not-allowed rounded-chip border border-[#E9D99B] bg-[#FFF4CA] px-5 py-2.5 text-sm font-semibold text-navy/60"
              >
                ดูสไลด์
              </button>
              <span className="text-xs text-ink/55">รอไฟล์ PDF</span>
            </span>
          )
        ) : null}
      </div>
    </article>
  );
}

function ResourceTimeline({ resources }: { resources: BasicSessionResource[] }) {
  return (
    <ol className="relative space-y-5 before:absolute before:bottom-4 before:left-[19px] before:top-4 before:w-0.5 before:rounded-full before:bg-gradient-to-b before:from-sky before:via-sky/40 before:to-orange/60 sm:before:left-[23px]">
      {resources.map((resource, index) => {
        const isTest = resource.type === "test";
        const number = lessonNumber(resources, index);
        return (
          <li key={resource.slug} className="relative pl-12 sm:pl-16">
            <span
              aria-hidden="true"
              className={`absolute left-0 top-4 flex h-10 w-10 items-center justify-center rounded-full border-4 border-white text-sm font-bold shadow-md sm:h-12 sm:w-12 ${
                isTest ? "bg-orange text-navy-dark" : "bg-navy text-paper"
              }`}
            >
              {isTest ? <ClipboardIcon className="h-5 w-5" /> : number}
            </span>
            <ResourceCard resource={resource} number={number} />
          </li>
        );
      })}
    </ol>
  );
}

const sessionThemes = [
  {
    // Basic 1 — orange
    banner: "from-orange to-[#FFB988]",
    soft: "border-orange/40 bg-orange/10 hover:border-orange hover:bg-orange/20",
    badge: "bg-orange text-navy-dark",
    eyebrow: "text-orange-dark",
  },
  {
    // Basic 2 — sky
    banner: "from-sky to-[#9ADCF7]",
    soft: "border-sky/40 bg-sky/10 hover:border-sky hover:bg-sky/20",
    badge: "bg-sky text-navy-dark",
    eyebrow: "text-sky-dark",
  },
];

function countResources(resources: BasicSessionResource[]) {
  const lessons = resources.filter((r) => r.type === "lesson").length;
  return { lessons, tests: resources.length - lessons };
}

function SessionSection({
  session,
  themeIndex,
}: {
  session: BasicSession;
  themeIndex: number;
}) {
  const theme = sessionThemes[themeIndex % sessionThemes.length];
  const { lessons, tests } = countResources(session.resources);

  return (
    <section id={session.slug} className="scroll-mt-24">
      <div
        className={`relative mb-6 overflow-hidden rounded-3xl bg-gradient-to-br ${theme.banner} px-6 py-5 text-navy-dark shadow-sm`}
      >
        <div
          aria-hidden="true"
          className="absolute -right-10 -top-12 h-44 w-44 rounded-full bg-white/30"
        />
        <div
          aria-hidden="true"
          className="absolute -bottom-16 right-24 h-36 w-36 rounded-full bg-white/20"
        />
        <div className="relative flex flex-wrap items-end justify-between gap-4">
          <div>
            <p className="text-sm font-semibold uppercase tracking-[0.1em] text-navy/70">
              {session.subtitle}
            </p>
            <h2 className="font-display text-2xl font-semibold sm:text-3xl">
              {session.title}
            </h2>
          </div>
          <div className="flex flex-wrap gap-2 text-sm font-semibold">
            <span className="rounded-chip bg-white px-4 py-1.5 shadow-sm">
              {lessons} ชุดคาบเรียน
            </span>
            <span className="rounded-chip bg-navy px-4 py-1.5 text-paper shadow-sm">
              {tests} แบบทดสอบ
            </span>
          </div>
        </div>
      </div>

      <ResourceTimeline resources={session.resources} />
    </section>
  );
}

function BasicPage() {
  const totals = basicSessions.reduce(
    (acc, session) => {
      const c = countResources(session.resources);
      return { lessons: acc.lessons + c.lessons, tests: acc.tests + c.tests };
    },
    { lessons: 0, tests: 0 },
  );

  return (
    <div className="mx-auto max-w-4xl px-4 py-8 sm:px-6 sm:py-10">
      <header className="relative mb-12 overflow-hidden rounded-3xl bg-gradient-to-br from-sky-light via-white to-orange-light/60 px-6 py-7 shadow-sm ring-1 ring-sky/30 sm:px-8 sm:py-8">
        <div
          aria-hidden="true"
          className="absolute -right-16 -top-20 h-72 w-72 rounded-full bg-sky/30 blur-2xl"
        />
        <div
          aria-hidden="true"
          className="absolute -bottom-24 left-1/3 h-64 w-64 rounded-full bg-orange/25 blur-2xl"
        />
        <div className="relative max-w-3xl">
          <span className="inline-flex items-center gap-2 rounded-chip bg-white/90 px-4 py-1.5 text-sm font-semibold text-navy shadow-sm">
            <span className="h-2 w-2 rounded-full bg-orange" />
            หลักสูตร School Bot
          </span>
          <h1 className="mt-3 font-display text-3xl font-semibold leading-tight text-navy sm:text-4xl">
            Basic
          </h1>
          <p className="mt-2 text-sm leading-6 text-ink/75 sm:text-base">
            เริ่มต้นเรียนรู้หุ่นยนต์และ AI ผ่านแผนการเรียน 2 session
            เรียงตามคาบเรียน พร้อมแบบทดสอบและสไลด์ให้เปิดใช้ได้ทันที
          </p>

          <div className="mt-5 flex flex-wrap gap-2 text-sm font-semibold">
            <span className="inline-flex items-center gap-2 rounded-chip bg-navy px-4 py-2 text-paper shadow-sm">
              <span className="h-2.5 w-2.5 rounded-full bg-sky" />
              {totals.lessons} ชุดคาบเรียน
            </span>
            <span className="inline-flex items-center gap-2 rounded-chip bg-orange px-4 py-2 text-navy-dark shadow-sm">
              <ClipboardIcon className="h-4 w-4" />
              {totals.tests} แบบทดสอบ
            </span>
          </div>
        </div>
      </header>

      <div className="space-y-16">
        {basicSessions.map((session, index) => (
          <SessionSection key={session.slug} session={session} themeIndex={index} />
        ))}
      </div>
    </div>
  );
}

function IntermediatePage() {
  const grades = curriculum.filter((item) => item.grade === 3 || item.grade === 4);

  return (
    <div className="mx-auto max-w-6xl px-6 py-12">
      <div className="mb-8">
        <p className="mb-1 text-sm font-medium text-alert">หลักสูตร School Bot</p>
        <h1 className="font-display text-3xl font-semibold text-ink">
          Intermediate
        </h1>
        <p className="mt-2 max-w-3xl text-sm leading-6 text-ink/70">
          รวมแผนการเรียนของชุด Intermediate จากไฟล์ PDF ในโฟลเดอร์
          public/pdf/Intermediate โดยเรียงตามลำดับคาบเรียน
        </p>
      </div>

      <section className="rounded-3xl border border-ink/10 bg-white p-6 shadow-sm">
        <div className="mb-6 flex flex-wrap items-start justify-between gap-4">
          <div>
            <p className="text-sm font-medium text-ink/50">Session เดียว</p>
            <h2 className="mt-1 font-display text-2xl font-semibold text-ink">
              Intermediate
            </h2>
            <p className="mt-2 max-w-2xl text-sm leading-6 text-ink/70">
              เปิดดูเอกสารแต่ละช่วงคาบเรียนได้ทันทีจากรายการด้านล่าง
            </p>
          </div>
          <div className="flex flex-wrap gap-2 text-xs text-ink/60">
            <span className="rounded-chip bg-orange/15 px-3 py-1.5 text-orange-dark">
              {intermediateResources.length} ชุดแผนการเรียน
            </span>
            <span className="rounded-chip bg-paper-dim px-3 py-1.5">
              {intermediateResources.length} ไฟล์ PDF
            </span>
          </div>
        </div>

        <ResourceTimeline resources={intermediateResources} />
      </section>

      <section className="mt-12 rounded-3xl border border-ink/10 bg-paper px-6 py-5">
        <div className="flex flex-wrap items-center justify-between gap-4">
          <div>
            <h2 className="font-display text-xl font-semibold text-ink">
              แผนรายชั้น ป.3–ป.4
            </h2>
            <p className="mt-1 text-sm text-ink/65">
              ถ้าต้องการดูโครงสร้างตามระดับชั้น แยกต่อได้ที่หน้า ป.3 และ ป.4
            </p>
          </div>
          <div className="flex flex-wrap gap-2">
            {grades.map((grade) => (
              <Link
                key={grade.grade}
                href={`/grades/${grade.grade}`}
                className="rounded-chip border border-ink/15 bg-white px-4 py-2 text-sm text-ink/75 transition-colors hover:bg-ink/5"
              >
                {grade.label}
              </Link>
            ))}
          </div>
        </div>
      </section>
    </div>
  );
}

function AdvanceComingSoonPage() {
  return (
    <div className="mx-auto max-w-4xl px-6 py-16">
      <section className="rounded-3xl border border-navy/10 bg-white p-8 text-center shadow-sm">
        <div className="mx-auto max-w-2xl">
          <span className="inline-flex rounded-chip bg-navy px-3 py-1 text-sm font-medium text-paper">
            Advance
          </span>
          <h1 className="mt-4 font-display text-3xl font-semibold text-ink">
            บทเรียนระดับ Advance กำลังจัดเตรียม
          </h1>
          <p className="mt-3 text-base leading-7 text-ink/70">
            เนื้อหาชุดนี้จะเปิดให้ใช้งานในเร็ว ๆ นี้
            ระหว่างนี้สามารถใช้บทเรียนในระดับ Basic และ Intermediate ไปก่อนได้
          </p>

          <div className="mt-8 flex flex-wrap justify-center gap-3">
            <Link
              href="/grades/basic"
              className="rounded-chip bg-orange px-5 py-2.5 text-sm font-medium text-navy-dark transition-colors hover:bg-orange-dark"
            >
              ไปที่ Basic
            </Link>
            <Link
              href="/grades/intermediate"
              className="rounded-chip border border-sky/35 bg-sky/10 px-5 py-2.5 text-sm font-medium text-sky-dark transition-colors hover:bg-sky/20"
            >
              ไปที่ Intermediate
            </Link>
          </div>
        </div>
      </section>
    </div>
  );
}

function LevelPage({ levelSlug }: { levelSlug: string }) {
  const level = gradeLevels.find((item) => item.slug === levelSlug);
  if (!level) return notFound();

  if (level.slug === "basic") {
    return <BasicPage />;
  }

  if (level.slug === "intermediate") {
    return <IntermediatePage />;
  }

  if (level.slug === "advance") {
    return <AdvanceComingSoonPage />;
  }

  const grades = curriculum.filter((item) => level.grades.includes(item.grade));
  const needsBasicSessions = grades.some(
    (grade) => grade.recommendedFoundationCourses.length > 0,
  );

  return (
    <div className="mx-auto max-w-4xl px-6 py-12">
      <div className="mb-8">
        <p className="mb-1 text-sm font-medium text-alert">หลักสูตร School Bot</p>
        <h1 className="mb-2 font-display text-3xl font-semibold">{level.label}</h1>
        <p className="text-sm text-ink/70">
          ช่วงชั้น {grades.map((grade) => grade.label).join("–")}
        </p>
      </div>

      {needsBasicSessions && (
        <div className="mb-10 rounded-xl border border-sky/40 bg-sky/10 p-5">
          <p className="mb-2 text-sm font-medium text-navy">
            ถ้ายังไม่มีพื้นฐาน แนะนำให้เริ่มที่ Basic 1 / Basic 2
          </p>
          <p className="mb-3 text-xs text-ink/60">
            เหมาะสำหรับห้องเรียนที่เพิ่งเริ่มใช้หลักสูตร หรืออยากเปิดแผนพื้นฐานแบบเรียงคาบเรียนจาก PDF ก่อน
          </p>
          <Link
            href="/grades/basic"
            className="inline-flex items-center rounded-chip border border-sky-dark/40 bg-white px-3 py-1.5 text-sm text-navy transition-colors hover:bg-sky/20"
          >
            ดู Basic 1 / Basic 2 →
          </Link>
        </div>
      )}

      <div className="space-y-10">
        {grades.map((grade) => (
          <section key={grade.grade}>
            <div className="mb-4 flex items-baseline justify-between gap-3">
              <h2 className="font-display text-2xl font-semibold">{grade.label}</h2>
              <span className="text-xs text-ink/50">{grade.totalHours} ชม./ปี</span>
            </div>
            <div className="grid gap-3">
              {grade.coreBlocks.map((block, index) => (
                <BlockCard block={block} index={index} key={block.title} />
              ))}
            </div>

            {grade.extensionActivities.length > 0 && (
              <div className="mt-6">
                <h3 className="mb-3 font-display text-lg font-semibold">
                  กิจกรรมเสริม {grade.label}
                </h3>
                <div className="grid gap-3">
                  {grade.extensionActivities.map((block, index) => (
                    <BlockCard block={block} index={index} key={block.title} />
                  ))}
                </div>
              </div>
            )}
          </section>
        ))}
      </div>
    </div>
  );
}

function SingleGradePage({ gradeValue }: { gradeValue: string }) {
  const grade = curriculum.find((item) => String(item.grade) === gradeValue);
  if (!grade) return notFound();

  const showBasicSuggestion = grade.recommendedFoundationCourses.length > 0;

  return (
    <div className="mx-auto max-w-4xl px-6 py-12">
      <div className="mb-8">
        <p className="mb-1 text-sm font-medium text-alert">หลักสูตร School Bot</p>
        <h1 className="mb-2 font-display text-3xl font-semibold">{grade.label}</h1>
        <p className="text-sm text-ink/70">รวม {grade.totalHours} ชั่วโมง/ปี</p>
        {grade.status === "draft" && (
          <p className="mt-2 inline-block rounded-chip bg-orange-light/40 px-3 py-1 text-xs text-orange-dark">
            หน้านี้ยังเป็นโครงร่าง — เนื้อหายังไม่ครบ
          </p>
        )}
      </div>

      {showBasicSuggestion && (
        <div className="mb-10 rounded-xl border border-sky/40 bg-sky/10 p-5">
          <p className="mb-2 text-sm font-medium text-navy">
            ถ้าโรงเรียนเพิ่งเริ่มใช้หลักสูตรนี้ แนะนำให้เริ่มที่ Basic 1 / Basic 2 ก่อน
          </p>
          <p className="mb-3 text-xs text-ink/60">
            เป็นชุดแผนพื้นฐานที่จัดตาม session และเรียงไฟล์ PDF ตามคาบเรียนไว้แล้ว
          </p>
          <Link
            href="/grades/basic"
            className="inline-flex items-center rounded-chip border border-sky-dark/40 bg-white px-3 py-1.5 text-sm text-navy transition-colors hover:bg-sky/20"
          >
            ไปที่หน้า Basic →
          </Link>
        </div>
      )}

      <section className="mb-10">
        <h2 className="mb-4 font-display text-xl font-semibold">
          เนื้อหาประจำชั้น {grade.label}
        </h2>
        <div className="grid gap-3">
          {grade.coreBlocks.map((block, index) => (
            <BlockCard block={block} index={index} key={block.title} />
          ))}
        </div>
      </section>

      {grade.extensionActivities.length > 0 && (
        <section>
          <h2 className="mb-4 font-display text-xl font-semibold">กิจกรรมเสริม</h2>
          <div className="grid gap-3">
            {grade.extensionActivities.map((block, index) => (
              <BlockCard block={block} index={index} key={block.title} />
            ))}
          </div>
        </section>
      )}
    </div>
  );
}

export default function GradePage({ params }: { params: { grade: string } }) {
  const isLevelPage = gradeLevels.some((item) => item.slug === params.grade);

  if (isLevelPage) {
    return <LevelPage levelSlug={params.grade} />;
  }

  return <SingleGradePage gradeValue={params.grade} />;
}
