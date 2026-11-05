import Link from "next/link";
import FoundationBreadcrumb from "@/components/FoundationBreadcrumb";
import { foundationCourses } from "@/lib/foundation-data";
import { buildFoundationSequence } from "@/lib/foundation-navigation";
import { renderMarkdownFile } from "@/lib/markdown";
import { notFound } from "next/navigation";

export function generateStaticParams() {
  const params: { slug: string; moduleSlug: string }[] = [];
  for (const c of foundationCourses) {
    for (const m of c.modules) {
      params.push({ slug: c.slug, moduleSlug: m.slug });
    }
  }
  return params;
}

export default function FoundationModulePage({
  params,
}: {
  params: { slug: string; moduleSlug: string };
}) {
  const course = foundationCourses.find((c) => c.slug === params.slug);
  if (!course) return notFound();

  const index = course.modules.findIndex((m) => m.slug === params.moduleSlug);
  if (index === -1) return notFound();

  const mod = course.modules[index];
  const html = renderMarkdownFile(mod.contentFile);
  const sequence = buildFoundationSequence(course);
  const currentHref = `/foundation/${course.slug}/${mod.slug}`;
  const sequenceIndex = sequence.findIndex((item) => item.href === currentHref);
  const prev = sequence[sequenceIndex - 1];
  const next = sequence[sequenceIndex + 1];
  const displayNumber = sequenceIndex + 1;
  const courseHref = `/foundation/${course.slug}`;

  return (
    <div className="max-w-3xl mx-auto px-6 py-12">
      <FoundationBreadcrumb
        items={[
          { label: "คอร์สปรับพื้นฐาน", href: "/foundation" },
          { label: course.title, href: courseHref },
          { label: mod.title },
        ]}
      />

      <div className="flex items-start justify-between gap-4 mt-4 mb-8">
        <h1 className="font-display text-2xl font-semibold leading-snug">
          {displayNumber}. {mod.title}
        </h1>
        <span className="shrink-0 text-xs bg-navy text-paper px-2.5 py-1 rounded-chip mt-1">
          {mod.hours} คาบ
        </span>
      </div>

      <article
        className="prose prose-sm sm:prose-base max-w-none prose-headings:font-display prose-headings:font-semibold prose-table:text-sm"
        dangerouslySetInnerHTML={{ __html: html }}
      />

      <div className="mt-16 grid gap-3 border-t border-ink/10 pt-8 md:grid-cols-2">
        {prev ? (
          <Link
            href={prev.href}
            className="group rounded-2xl border border-ink/10 bg-white px-5 py-4 transition-colors hover:border-ink/20 hover:bg-paper-dim/50"
          >
            <p className="text-xs text-ink/45">ย้อนกลับ</p>
            <div className="mt-1 flex items-center gap-2 text-sm text-ink/75">
              <span
                aria-hidden="true"
                className="transition-transform group-hover:-translate-x-0.5"
              >
                ←
              </span>
              <span className="font-medium">{prev.title}</span>
            </div>
          </Link>
        ) : (
          <Link
            href={courseHref}
            className="group rounded-2xl border border-ink/10 bg-white px-5 py-4 transition-colors hover:border-ink/20 hover:bg-paper-dim/50"
          >
            <p className="text-xs text-ink/45">ย้อนกลับ</p>
            <div className="mt-1 flex items-center gap-2 text-sm text-ink/75">
              <span
                aria-hidden="true"
                className="transition-transform group-hover:-translate-x-0.5"
              >
                ←
              </span>
              <span className="font-medium">หน้า {course.title}</span>
            </div>
          </Link>
        )}
        {next ? (
          <Link
            href={next.href}
            className="group rounded-2xl border border-ink/10 bg-white px-5 py-4 transition-colors hover:border-sky/30 hover:bg-sky/5"
          >
            <p className="text-xs text-ink/45">บทถัดไป</p>
            <div className="mt-1 flex items-center justify-between gap-3 text-sm text-ink">
              <span className="font-medium">{next.title}</span>
              <span
                aria-hidden="true"
                className="text-ink/50 transition-transform group-hover:translate-x-0.5"
              >
                →
              </span>
            </div>
          </Link>
        ) : (
          <Link
            href={courseHref}
            className="group rounded-2xl border border-ink/10 bg-white px-5 py-4 transition-colors hover:border-sky/30 hover:bg-sky/5"
          >
            <p className="text-xs text-ink/45">ถัดไป</p>
            <div className="mt-1 flex items-center justify-between gap-3 text-sm text-ink">
              <span className="font-medium">ดูภาพรวม {course.title}</span>
              <span
                aria-hidden="true"
                className="text-ink/50 transition-transform group-hover:translate-x-0.5"
              >
                →
              </span>
            </div>
          </Link>
        )}
      </div>
    </div>
  );
}
