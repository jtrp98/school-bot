import type { ReactNode } from "react";
import Link from "next/link";
import { notFound } from "next/navigation";
import FoundationBreadcrumb from "@/components/FoundationBreadcrumb";
import LessonToc, { type TocItem } from "@/components/curriculum/LessonToc";
import ProtectedArticle from "@/components/curriculum/ProtectedArticle";
import { gradeThemes, sectionTone } from "@/lib/grade-theme";
import { findLessonPlan, lessonHref, lessonPlans } from "@/lib/lesson-plans";
import type { FlowStep, LessonGuide, LessonPlan, LessonSession, Worksheet } from "@/lib/lesson-plans/types";
import { findCurriculumGrade } from "@/lib/new-curriculum";

export function generateStaticParams() {
  return lessonPlans.map((plan) => ({ grade: plan.gradeSlug, week: `week-${plan.week}` }));
}

function parseWeek(value: string) {
  const match = /^week-(\d+)$/.exec(value);
  return match ? Number(match[1]) : null;
}

/*
 * ลำดับชั้นของเนื้อหา
 * ระดับ 1 หัวข้อใหญ่  — SectionHeading  เลขกล่องสีวนตามลำดับหัวข้อ + เส้นคั่นเต็มความกว้าง (1, 2, 3…)
 * ระดับ 2 หัวข้อรอง   — SubHeading      แถบฟ้าด้านซ้าย + เลขย่อย (1.1, 1.2…)
 * ระดับ 3 หัวข้อย่อย  — ItemHeading     ตัวหนาสีกรม ขนาดเท่าเนื้อหา
 * เนื้อหา             — Body            15px สีเทาเข้ม
 * ข้อย่อย             — BulletList      จุดสีฟ้า / QuestionList สำหรับคำถาม
 */

function SectionHeading({
  id,
  number,
  eyebrow,
  title,
}: {
  id: string;
  number: number;
  eyebrow: string;
  title: string;
}) {
  const tone = sectionTone(number);
  return (
    <div id={id} className="mb-7 flex scroll-mt-24 items-center gap-4 border-b-2 border-ink/10 pb-4">
      <span
        className={`flex h-11 w-11 shrink-0 -rotate-3 items-center justify-center rounded-xl font-display text-xl font-semibold text-navy-dark shadow-sm ${tone.tile}`}
      >
        {number}
      </span>
      <div>
        <p className={`text-xs font-semibold uppercase tracking-[0.12em] ${tone.text}`}>{eyebrow}</p>
        <h2 className="font-display text-2xl font-semibold leading-tight text-ink sm:text-[1.75rem]">
          {title}
        </h2>
      </div>
    </div>
  );
}

function SubHeading({
  id,
  number,
  title,
  meta,
}: {
  id?: string;
  number: string;
  title: string;
  meta?: ReactNode;
}) {
  // ใช้สีเดียวกับหัวข้อใหญ่ที่หัวข้อรองนี้สังกัด (เลขก่อนจุด)
  const tone = sectionTone(parseInt(number, 10));
  return (
    <div
      id={id}
      className={`mb-4 flex scroll-mt-24 flex-wrap items-center justify-between gap-x-4 gap-y-2 border-l-4 pl-3 ${tone.border}`}
    >
      <h3 className="font-display text-xl font-semibold leading-snug text-ink">
        <span className={`mr-2 ${tone.text}`}>{number}</span>
        {title}
      </h3>
      {meta ? <div className="flex flex-wrap gap-2">{meta}</div> : null}
    </div>
  );
}

function ItemHeading({ children, icon }: { children: ReactNode; icon?: string }) {
  return (
    <h4 className="font-display text-base font-semibold leading-snug text-ink">
      {icon ? (
        <span aria-hidden="true" className="mr-2">
          {icon}
        </span>
      ) : null}
      {children}
    </h4>
  );
}

function Body({ children, className = "" }: { children: ReactNode; className?: string }) {
  return <p className={`text-[15px] leading-7 text-ink/80 ${className}`}>{children}</p>;
}

function BulletList({ items, className = "" }: { items: string[]; className?: string }) {
  return (
    <ul className={`space-y-2 ${className}`}>
      {items.map((item) => (
        <li key={item} className="flex gap-3 text-[15px] leading-7 text-ink/80">
          <span aria-hidden="true" className="mt-[11px] h-1.5 w-1.5 shrink-0 rounded-full bg-sky" />
          {item}
        </li>
      ))}
    </ul>
  );
}

function QuestionList({ items }: { items: string[] }) {
  return (
    <ul className="mt-3 space-y-1.5 border-l-2 border-orange/30 pl-3">
      {items.map((item) => (
        <li key={item} className="flex gap-2 text-sm leading-6 text-ink/75">
          <span aria-hidden="true" className="font-semibold text-orange-dark">
            ?
          </span>
          {item}
        </li>
      ))}
    </ul>
  );
}

function Chip({ children, tone = "sky" }: { children: ReactNode; tone?: "sky" | "navy" }) {
  return (
    <span
      className={`rounded-chip px-3 py-1 text-xs font-semibold ${
        tone === "navy" ? "bg-sky/20 text-[#1B6E9A]" : "bg-ink/5 text-ink/70"
      }`}
    >
      {children}
    </span>
  );
}

function Flow({ steps, tone = "navy" }: { steps: FlowStep[]; tone?: "navy" | "sky" }) {
  return (
    <ol className="flex flex-wrap items-stretch gap-2">
      {steps.map((step, index) => (
        <li key={`${step.label}-${index}`} className="flex items-center gap-2">
          <div
            className={`rounded-2xl px-4 py-2.5 ${
              tone === "navy" ? "bg-[#E6F5FC] text-ink ring-2 ring-sky/60" : "bg-white text-ink ring-1 ring-sky/40"
            }`}
          >
            <p className="font-display text-base font-semibold leading-tight">{step.label}</p>
            {step.sub ? (
              <p className={`text-xs ${"text-ink/55"}`}>
                {step.sub}
              </p>
            ) : null}
          </div>
          {index < steps.length - 1 ? (
            <span aria-hidden="true" className="text-lg font-bold text-orange-dark">
              →
            </span>
          ) : null}
        </li>
      ))}
    </ol>
  );
}

function Callout({ label, tone, children }: { label: string; tone: "orange" | "sky"; children: ReactNode }) {
  return (
    <p
      className={`rounded-xl border-l-4 px-4 py-2.5 text-sm leading-6 text-ink/80 ${
        tone === "orange" ? "border-orange bg-orange/10" : "border-sky bg-sky/10"
      }`}
    >
      <span className={`font-semibold ${tone === "orange" ? "text-orange-dark" : "text-sky-dark"}`}>
        {label} ·{" "}
      </span>
      {children}
    </p>
  );
}

function WorksheetIcon({ className = "" }: { className?: string }) {
  return (
    <svg viewBox="0 0 24 24" fill="none" aria-hidden="true" className={className}>
      <path d="M7 3h7l5 5v12a1 1 0 0 1-1 1H7a1 1 0 0 1-1-1V4a1 1 0 0 1 1-1Z" stroke="currentColor" strokeWidth="2" strokeLinejoin="round" />
      <path d="M14 3v5h5M9 13h6M9 17h4" stroke="currentColor" strokeWidth="2" strokeLinecap="round" />
    </svg>
  );
}

function SessionBlock({
  session,
  number,
  worksheets = [],
}: {
  session: LessonSession;
  number: string;
  worksheets?: Worksheet[];
}) {
  return (
    <div>
      <SubHeading
        id={`session-${session.number}`}
        number={number}
        title={`คาบที่ ${session.number}: ${session.title}`}
        meta={<Chip>{session.minutes} นาที</Chip>}
      />
      <div className="mb-5 rounded-2xl bg-paper-dim px-4 py-3">
        <p className="text-xs font-semibold text-navy/70">เป้าหมายของคาบ</p>
        <Body>{session.goal}</Body>
      </div>

      <ol className="relative space-y-5 before:absolute before:bottom-3 before:left-[23px] before:top-3 before:w-0.5 before:rounded-full before:bg-gradient-to-b before:from-orange/60 before:via-orange/25 before:to-orange/10 sm:before:left-[27px]">
        {session.steps.map((step) => (
          <li key={step.time} className="relative flex gap-3 sm:gap-4">
            <span className="relative z-10 flex h-12 w-12 shrink-0 flex-col items-center justify-center rounded-2xl border-4 border-white bg-[#FFEBDD] text-center text-orange-dark shadow-sm sm:h-14 sm:w-14">
              <span className="text-[11px] font-bold leading-none sm:text-xs">{step.time}</span>
              <span className="text-[10px] leading-tight text-orange-dark/70">นาที</span>
            </span>
            <div className="min-w-0 flex-1 rounded-2xl border border-ink/10 bg-white p-4">
              <ItemHeading>{step.phase}</ItemHeading>
              <Body className="mt-1">{step.activity}</Body>
              {step.teacher || step.expected ? (
                <dl className="mt-3 grid gap-2 text-sm leading-6 sm:grid-cols-2">
                  {step.teacher ? (
                    <div className="rounded-xl bg-paper-dim px-3 py-2">
                      <dt className="text-xs font-semibold text-navy/70">บทบาทครู</dt>
                      <dd className="text-ink/75">{step.teacher}</dd>
                    </div>
                  ) : null}
                  {step.expected ? (
                    <div className="rounded-xl bg-[#E3F6F2] px-3 py-2">
                      <dt className="text-xs font-semibold text-[#11786A]">สิ่งที่คาดหวังจากนักเรียน</dt>
                      <dd className="text-ink/75">{step.expected}</dd>
                    </div>
                  ) : null}
                </dl>
              ) : null}
              {step.worksheet
                ? (() => {
                    const sheet = worksheets.find((item) => item.id === step.worksheet?.id);
                    return sheet ? (
                      <a
                        href={sheet.file}
                        target="_blank"
                        rel="noreferrer"
                        className="mt-3 flex items-center gap-2.5 rounded-xl border border-orange/50 bg-orange/10 px-3 py-2 text-sm font-semibold text-navy transition-colors hover:bg-orange/20"
                      >
                        <WorksheetIcon className="h-5 w-5 shrink-0 text-orange-dark" />
                        <span className="min-w-0 flex-1">
                          ใช้ใบงาน {sheet.title}
                          {step.worksheet?.part ? ` · ${step.worksheet.part}` : ""}
                        </span>
                        <span className="shrink-0 text-orange-dark">เปิด PDF ↗</span>
                      </a>
                    ) : null;
                  })()
                : null}
              {step.note ? (
                <div className="mt-3">
                  <Callout label="ครูควรรู้" tone="orange">
                    {step.note}
                  </Callout>
                </div>
              ) : null}
            </div>
          </li>
        ))}
      </ol>
    </div>
  );
}

function GuideBlock({ guide, number }: { guide: LessonGuide; number: string }) {
  return (
    <div>
      <SubHeading id={guide.id} number={number} title={guide.title} />
      {guide.intro ? <Body className="mb-4">{guide.intro}</Body> : null}

      <div className="grid gap-3 sm:grid-cols-2">
        {guide.blocks.map((block) => (
          <div
            key={block.heading}
            className={`rounded-2xl border border-ink/10 bg-white p-4 ${
              block.flow && block.flow.length > 3 ? "sm:col-span-2" : ""
            }`}
          >
            <ItemHeading icon={block.icon}>{block.heading}</ItemHeading>
            {block.text ? <Body className="mt-1">{block.text}</Body> : null}
            {block.compare ? (
              <p className="mt-2 text-sm leading-6 text-ink/65">
                <span className="mr-1.5 rounded-chip bg-paper-dim px-2 py-0.5 text-xs font-semibold text-ink/60">
                  เทียบกับ
                </span>
                {block.compare}
              </p>
            ) : null}
            {block.flow ? (
              <div className="mt-3">
                <Flow steps={block.flow} tone="sky" />
              </div>
            ) : null}
            {block.questions ? <QuestionList items={block.questions} /> : null}
          </div>
        ))}
      </div>

      {guide.tip ? (
        <div className="mt-4">
          <Callout label="เคล็ดลับ" tone="sky">
            {guide.tip}
          </Callout>
        </div>
      ) : null}
    </div>
  );
}

interface Section {
  id: string;
  eyebrow: string;
  title: string;
  subs: { id?: string; title: string }[];
}

function buildSections(plan: LessonPlan): Section[] {
  const sections: Section[] = [
    { id: "objectives", eyebrow: "Learning Objectives", title: "จุดประสงค์การเรียนรู้", subs: [] },
    {
      id: "standards",
      eyebrow: "Standards",
      title: "มาตรฐานและตัวชี้วัด",
      subs: [
        { id: "standards-indicators", title: "ตัวชี้วัด" },
        { id: "standards-integration", title: "บูรณาการกับกลุ่มสาระอื่น" },
      ],
    },
    {
      id: "content",
      eyebrow: "Content",
      title: "สาระการเรียนรู้",
      subs: [
        { id: "content-learning", title: "ความรู้ ทักษะ และเจตคติ" },
        ...(plan.vocabulary?.length ? [{ id: "content-vocabulary", title: "คำศัพท์สำคัญ" }] : []),
        { id: "content-materials", title: "สื่อและแหล่งการเรียนรู้" },
      ],
    },
    {
      id: "steps",
      eyebrow: "Lesson Flow",
      title: "ขั้นตอนการจัดการเรียนรู้",
      subs: plan.sessions.map((session) => ({
        id: `session-${session.number}`,
        title: `คาบที่ ${session.number}: ${session.title}`,
      })),
    },
  ];

  if (plan.guides?.length) {
    sections.push({
      id: "guides",
      eyebrow: "Activity Guide",
      title: "รายละเอียดกิจกรรม",
      subs: [
        ...plan.guides.map((guide) => ({ id: guide.id, title: guide.title })),
        ...(plan.questions?.length ? [{ id: "questions", title: "คำถามกระตุ้นการคิด" }] : []),
      ],
    });
  }

  sections.push(
    { id: "assessment", eyebrow: "Assessment", title: "การวัดและประเมินผล", subs: [] },
    {
      id: "takeaways",
      eyebrow: "Key Message",
      title: "สรุปท้ายหน่วย",
      subs: [
        { id: "takeaways-key", title: "สาระที่ผู้เรียนควรได้รับ" },
        { id: "takeaways-bridge", title: `เชื่อมต่อสู่ ${plan.bridge.nextLabel}` },
      ],
    },
  );

  return sections;
}

export default function LessonPlanPage({ params }: { params: { grade: string; week: string } }) {
  const grade = findCurriculumGrade(params.grade);
  const weekNumber = parseWeek(params.week);
  if (!grade || weekNumber === null) return notFound();

  const plan = findLessonPlan(grade.slug, weekNumber);
  const week = grade.weeks.find((item) => item.week === weekNumber);
  if (!plan || !week) return notFound();

  const theme = gradeThemes[grade.slug];
  const gradePlans = lessonPlans.filter((item) => item.gradeSlug === grade.slug);
  const index = gradePlans.indexOf(plan);
  const prev = gradePlans[index - 1];
  const next = gradePlans[index + 1];

  const sections = buildSections(plan);
  const sectionNumber = (id: string) => sections.findIndex((section) => section.id === id) + 1;
  const subNumber = (id: string, i: number) => `${sectionNumber(id)}.${i + 1}`;

  const toc: TocItem[] = [
    { id: "overview", label: "ภาพรวมหน่วย" },
    ...sections.map((section, i) => ({
      id: section.id,
      label: `${i + 1}. ${section.title}`,
      children: section.subs
        .filter((sub) => sub.id)
        .map((sub, j) => ({ id: sub.id as string, label: `${i + 1}.${j + 1} ${sub.title}` })),
    })),
  ];

  return (
    <div id="top" className="mx-auto max-w-6xl px-4 py-8 sm:px-6 sm:py-10">
      <FoundationBreadcrumb
        items={[
          { label: "หลักสูตร (ใหม่)", href: "/curriculum" },
          { label: grade.label, href: `/curriculum/${grade.slug}` },
          { label: `สัปดาห์ที่ ${week.week}` },
        ]}
      />

      <div className="lg:grid lg:grid-cols-[minmax(0,1fr)_15rem] lg:gap-12">
        <div className="min-w-0 max-w-3xl">
          <ProtectedArticle>
            <article>
              <header
                id="overview"
                className={`relative scroll-mt-24 overflow-hidden rounded-3xl bg-gradient-to-br from-white via-white to-[#FFF3E8] px-6 py-7 shadow-sm ring-1 ${theme.ring} sm:px-8 sm:py-8`}
              >
                <div
                  aria-hidden="true"
                  className={`absolute -right-16 -top-20 h-72 w-72 rounded-full opacity-40 blur-3xl ${theme.tile}`}
                />
                <div
                  aria-hidden="true"
                  className="absolute -bottom-24 left-1/4 h-56 w-56 rounded-full bg-orange/15 blur-3xl"
                />
                <div className="relative">
                  <p className="flex flex-wrap items-center gap-2 text-sm font-semibold text-navy/70">
                    <span className={`rounded-lg px-2.5 py-0.5 font-display text-navy-dark ${theme.tile}`}>
                      {grade.label}
                    </span>
                    แผนการจัดการเรียนรู้ · สัปดาห์ที่ {week.week} · {plan.unitLabel}
                  </p>
                  <h1 className="mt-2 font-display text-3xl font-semibold leading-tight text-navy sm:text-4xl">
                    {plan.title}
                  </h1>
                  <p className="mt-1 font-display text-lg text-ink/70">{plan.subtitle}</p>
                  <div className="mt-4 flex flex-wrap gap-2 text-sm font-semibold">
                    <span className="rounded-chip bg-white px-4 py-1.5 text-ink shadow-sm">{week.periods} คาบ</span>
                    <span className="rounded-chip bg-white px-4 py-1.5 text-ink shadow-sm">
                      คาบละ {plan.minutesPerPeriod} นาที
                    </span>
                    <span className="rounded-chip bg-white px-4 py-1.5 text-ink shadow-sm">
                      เรื่อง: {week.title}
                    </span>
                    {plan.worksheets?.map((sheet) => (
                      <a
                        key={sheet.id}
                        href={sheet.file}
                        target="_blank"
                        rel="noreferrer"
                        title={sheet.usage}
                        className="inline-flex items-center gap-2 rounded-chip bg-orange py-1.5 pl-2 pr-4 text-[15px] font-bold text-navy-dark shadow-md shadow-orange/40 ring-2 ring-white transition-all hover:-translate-y-0.5 hover:bg-orange-dark hover:shadow-lg"
                      >
                        <span className="flex h-7 w-7 items-center justify-center rounded-full bg-white text-orange-dark">
                          <WorksheetIcon className="h-4 w-4" />
                        </span>
                        ใบงาน: {sheet.title}
                        <span className="rounded-chip bg-navy-dark/15 px-1.5 py-0.5 text-[11px] font-bold leading-none">PDF</span>
                        <span aria-hidden="true">↗</span>
                      </a>
                    ))}
                  </div>
                </div>
              </header>

              <div className="mt-8 space-y-4 text-[17px] leading-8 text-ink/85">
                {plan.summary.map((paragraph) => (
                  <p key={paragraph}>{paragraph}</p>
                ))}
              </div>

              <nav
                aria-label="ในบทนี้"
                className="mt-8 flex flex-wrap gap-2 border-y border-ink/10 py-4 text-sm lg:hidden"
              >
                <span className="mr-1 py-1 text-ink/50">ในบทนี้:</span>
                {sections.map((section, i) => (
                  <a
                    key={section.id}
                    href={`#${section.id}`}
                    className="rounded-chip bg-white px-3 py-1 text-ink/70 ring-1 ring-ink/10 transition-colors hover:bg-orange/10 hover:text-ink"
                  >
                    {i + 1}. {section.title}
                  </a>
                ))}
              </nav>

              <div className="mt-14 space-y-16">
                <section>
                  <SectionHeading
                    id="objectives"
                    number={sectionNumber("objectives")}
                    eyebrow="Learning Objectives"
                    title="จุดประสงค์การเรียนรู้"
                  />
                  <Body className="mb-4">เมื่อจบหน่วยนี้ นักเรียนสามารถ</Body>
                  <ol className="space-y-3">
                    {plan.objectives.map((objective, i) => (
                      <li key={objective} className="flex gap-3 text-[15px] leading-7 text-ink/80">
                        <span className="mt-0.5 flex h-6 w-6 shrink-0 items-center justify-center rounded-full bg-sky text-xs font-bold text-navy-dark">
                          {i + 1}
                        </span>
                        {objective}
                      </li>
                    ))}
                  </ol>
                </section>

                <section>
                  <SectionHeading
                    id="standards"
                    number={sectionNumber("standards")}
                    eyebrow="Standards"
                    title="มาตรฐานและตัวชี้วัด"
                  />
                  <SubHeading
                    id="standards-indicators"
                    number={subNumber("standards", 0)}
                    title="ตัวชี้วัด"
                  />
                  {plan.standards.every((item) => item.code.startsWith("ว 4.2")) ? (
                    <Body className="mb-3">สาระที่ 4 เทคโนโลยี · มาตรฐาน ว 4.2</Body>
                  ) : null}
                  <div className="space-y-3">
                    {plan.standards.map((standard) => (
                      <div key={standard.code} className="rounded-2xl border border-ink/10 bg-white p-4">
                        <Chip tone="navy">{standard.code}</Chip>
                        <div className="mt-2">
                          <ItemHeading>{standard.indicator}</ItemHeading>
                        </div>
                        <p className="mt-1 text-sm leading-6 text-ink/65">ในหน่วยนี้: {standard.core}</p>
                      </div>
                    ))}
                  </div>

                  <div className="mt-10">
                    <SubHeading
                      id="standards-integration"
                      number={subNumber("standards", 1)}
                      title="บูรณาการกับกลุ่มสาระอื่น"
                    />
                    <div className="space-y-3">
                      {plan.integrations.map((item) => (
                        <div key={item.subject}>
                          <ItemHeading>{item.subject}</ItemHeading>
                          <Body>{item.detail}</Body>
                        </div>
                      ))}
                    </div>
                  </div>
                </section>

                <section>
                  <SectionHeading
                    id="content"
                    number={sectionNumber("content")}
                    eyebrow="Content"
                    title="สาระการเรียนรู้"
                  />
                  <SubHeading
                    id="content-learning"
                    number={subNumber("content", 0)}
                    title="ความรู้ ทักษะ และเจตคติ"
                  />
                  <div className="grid gap-3 sm:grid-cols-3">
                    {[
                      { label: "ความรู้", text: plan.learning.knowledge, tone: "bg-[#E6F5FC] ring-1 ring-sky/40" },
                      { label: "ทักษะ / กระบวนการ", text: plan.learning.skills, tone: "bg-[#FFEEE3] ring-1 ring-orange/40" },
                      { label: "เจตคติ", text: plan.learning.attitude, tone: "bg-[#FFF6D9] ring-1 ring-[#FFD44F]/60" },
                    ].map((item) => (
                      <div key={item.label} className={`rounded-2xl p-4 ${item.tone}`}>
                        <ItemHeading>{item.label}</ItemHeading>
                        <p className="mt-1.5 text-sm leading-6 text-ink/80">{item.text}</p>
                      </div>
                    ))}
                  </div>

                  {plan.vocabulary?.length ? (
                    <div className="mt-10">
                      <SubHeading
                        id="content-vocabulary"
                        number={subNumber("content", 1)}
                        title="คำศัพท์สำคัญ"
                      />
                      <dl className="grid gap-2 sm:grid-cols-2">
                        {plan.vocabulary.map((item) => (
                          <div
                            key={item.term}
                            className="flex flex-wrap items-baseline gap-x-2 rounded-xl border border-ink/10 bg-white px-3 py-2"
                          >
                            <dt className="font-display text-base font-semibold text-ink">{item.term}</dt>
                            <dd className="text-sm leading-6 text-ink/75">{item.meaning}</dd>
                          </div>
                        ))}
                      </dl>
                    </div>
                  ) : null}

                  <div className="mt-10">
                    <SubHeading
                      id="content-materials"
                      number={subNumber("content", plan.vocabulary?.length ? 2 : 1)}
                      title="สื่อและแหล่งการเรียนรู้"
                    />
                    <BulletList items={plan.materials} />
                  </div>
                </section>

                <section>
                  <SectionHeading
                    id="steps"
                    number={sectionNumber("steps")}
                    eyebrow="Lesson Flow"
                    title="ขั้นตอนการจัดการเรียนรู้"
                  />
                  <div className="space-y-12">
                    {plan.sessions.map((session, i) => (
                      <SessionBlock
                        key={session.number}
                        session={session}
                        number={subNumber("steps", i)}
                        worksheets={plan.worksheets}
                      />
                    ))}
                  </div>
                </section>

                {plan.guides?.length ? (
                  <section>
                    <SectionHeading
                      id="guides"
                      number={sectionNumber("guides")}
                      eyebrow="Activity Guide"
                      title="รายละเอียดกิจกรรม"
                    />
                    <div className="space-y-12">
                      {plan.guides.map((guide, i) => (
                        <GuideBlock key={guide.id} guide={guide} number={subNumber("guides", i)} />
                      ))}
                      {plan.questions?.length ? (
                        <div>
                          <SubHeading
                            id="questions"
                            number={subNumber("guides", plan.guides.length)}
                            title="คำถามกระตุ้นการคิด"
                          />
                          <BulletList items={plan.questions} />
                        </div>
                      ) : null}
                    </div>
                  </section>
                ) : null}

                <section>
                  <SectionHeading
                    id="assessment"
                    number={sectionNumber("assessment")}
                    eyebrow="Assessment"
                    title="การวัดและประเมินผล"
                  />
                  <div className="space-y-3">
                    {plan.assessment.map((item) => (
                      <div
                        key={item.topic}
                        className="grid gap-1 rounded-2xl border border-ink/10 bg-white p-4 sm:grid-cols-[1fr_1.2fr] sm:gap-4"
                      >
                        <div>
                          <ItemHeading>{item.topic}</ItemHeading>
                          <p className="mt-0.5 text-sm text-ink/60">ประเมินจาก {item.method}</p>
                        </div>
                        <Body>{item.criteria}</Body>
                      </div>
                    ))}
                  </div>
                  {plan.assessmentNote ? (
                    <p className="mt-3 text-sm leading-6 text-ink/60">* {plan.assessmentNote}</p>
                  ) : null}
                </section>

                <section>
                  <SectionHeading
                    id="takeaways"
                    number={sectionNumber("takeaways")}
                    eyebrow="Key Message"
                    title="สรุปท้ายหน่วย"
                  />
                  <SubHeading
                    id="takeaways-key"
                    number={subNumber("takeaways", 0)}
                    title="สาระที่ผู้เรียนควรได้รับ"
                  />
                  <ul className="space-y-3 rounded-3xl bg-gradient-to-br from-[#E6F5FC] to-[#E3F6F2] p-6 text-ink ring-1 ring-sky/40 sm:p-7">
                    {plan.takeaways.map((item) => (
                      <li key={item} className="flex gap-3 text-[15px] leading-7">
                        <span aria-hidden="true" className="mt-2.5 h-2 w-2 shrink-0 rounded-full bg-sky-dark" />
                        {item}
                      </li>
                    ))}
                  </ul>

                  <div className="mt-10">
                    <SubHeading
                      id="takeaways-bridge"
                      number={subNumber("takeaways", 1)}
                      title={`เชื่อมต่อสู่ ${plan.bridge.nextLabel}`}
                    />
                    <div className="space-y-3 rounded-3xl border-2 border-dashed border-orange/60 bg-gradient-to-r from-orange/10 to-[#FFF8F1] p-5 sm:p-6">
                      <div>
                        <ItemHeading>ครูปิดบทเรียนด้วยคำถาม</ItemHeading>
                        <Body>{plan.bridge.question}</Body>
                      </div>
                      <div>
                        <ItemHeading>จากนั้นเกริ่นว่า</ItemHeading>
                        <Body>{plan.bridge.teaser}</Body>
                      </div>
                    </div>
                  </div>
                </section>
              </div>
            </article>
          </ProtectedArticle>

          <div className="mt-16 grid gap-3 border-t border-ink/10 pt-8 md:grid-cols-2">
            <Link
              href={prev ? lessonHref(grade.slug, prev.week) : `/curriculum/${grade.slug}`}
              className="group rounded-2xl border border-ink/10 bg-white px-5 py-4 transition-colors hover:border-ink/20 hover:bg-paper-dim/50"
            >
              <p className="text-xs text-ink/45">ย้อนกลับ</p>
              <div className="mt-1 flex items-center gap-2 text-sm text-ink/75">
                <span aria-hidden="true" className="transition-transform group-hover:-translate-x-0.5">
                  ←
                </span>
                <span className="font-medium">
                  {prev ? `${prev.unitLabel}: ${prev.title}` : `โครงหลักสูตร ${grade.label}`}
                </span>
              </div>
            </Link>
            <Link
              href={next ? lessonHref(grade.slug, next.week) : `/curriculum/${grade.slug}`}
              className="group rounded-2xl border border-ink/10 bg-white px-5 py-4 transition-colors hover:border-orange/30 hover:bg-orange/5"
            >
              <p className="text-xs text-ink/45">{next ? "บทถัดไป" : "ถัดไป"}</p>
              <div className="mt-1 flex items-center justify-between gap-3 text-sm text-ink">
                <span className="font-medium">
                  {next ? `${next.unitLabel}: ${next.title}` : `กลับไปโครงหลักสูตร ${grade.label}`}
                </span>
                <span aria-hidden="true" className="text-ink/50 transition-transform group-hover:translate-x-0.5">
                  →
                </span>
              </div>
            </Link>
          </div>
        </div>

        <aside className="hidden lg:block">
          <div className="sticky top-24 max-h-[calc(100vh-7rem)] overflow-y-auto pb-6">
            <LessonToc items={toc} />
          </div>
        </aside>
      </div>
    </div>
  );
}
