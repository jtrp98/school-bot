import Link from "next/link";
import FoundationBreadcrumb from "@/components/FoundationBreadcrumb";
import { foundationCourses } from "@/lib/foundation-data";
import { excerptFromMarkdownFile } from "@/lib/markdown";
import { notFound } from "next/navigation";

export function generateStaticParams() {
  return foundationCourses
    .filter((c) => c.modules.length > 0)
    .map((c) => ({ slug: c.slug }));
}

export default function FoundationCoursePage({
  params,
}: {
  params: { slug: string };
}) {
  const course = foundationCourses.find((c) => c.slug === params.slug);
  if (!course || course.modules.length === 0) return notFound();

  const totalHours = course.modules.reduce((s, m) => s + m.hours, 0);
  const showRoboXComponentsCard = course.slug === "robotics";

  return (
    <div className="max-w-3xl mx-auto px-6 py-12">
      <FoundationBreadcrumb
        items={[
          { label: "คอร์สปรับพื้นฐาน", href: "/foundation" },
          { label: course.title },
        ]}
      />
      <div className="mb-2 flex flex-wrap items-start justify-between gap-3">
        <h1 className="font-display text-3xl font-semibold">{course.title}</h1>
      </div>
      <p className="text-ink/70 text-sm mb-2">{course.subtitle}</p>
      <p className="text-xs text-ink/40 mb-8">
        {course.sourceNote} · รวม {totalHours} คาบ ({course.modules.length} บท)
      </p>

      <div className="grid gap-3">
        {showRoboXComponentsCard ? (
          <Link
            href="/foundation/robotics/robox-components"
            className="block bg-white rounded-xl border border-sky/30 p-5 hover:border-sky hover:shadow-md transition-all"
          >
            <div className="flex items-start justify-between gap-4 mb-1.5">
              <h3 className="font-display text-base font-semibold leading-snug">
                1. องค์ประกอบของ RoboX
              </h3>
              <span className="shrink-0 text-xs bg-sky text-navy-dark px-2.5 py-1 rounded-chip">
                PDF
              </span>
            </div>
            <p className="text-sm text-ink/60">
              เปิดเอกสารอ้างอิงเรื่องโครงสร้าง เซอร์โว ระบบควบคุม และ Sensor ของ RoboX
              ได้ในหน้าเดียว พร้อมปุ่มเปิดเต็มหน้าและดาวน์โหลด
            </p>
          </Link>
        ) : null}
        {course.modules.map((m, i) => (
          <Link
            key={m.slug}
            href={`/foundation/${course.slug}/${m.slug}`}
            className="block bg-white rounded-xl border border-ink/10 p-5 hover:border-sky hover:shadow-md transition-all"
          >
            <div className="flex items-start justify-between gap-4 mb-1.5">
              <h3 className="font-display text-base font-semibold leading-snug">
                {i + 1 + (showRoboXComponentsCard ? 1 : 0)}. {m.title}
              </h3>
              <span className="shrink-0 text-xs bg-navy text-paper px-2.5 py-1 rounded-chip">
                {m.hours} คาบ
              </span>
            </div>
            <p className="text-sm text-ink/60">
              {excerptFromMarkdownFile(m.contentFile)}
            </p>
          </Link>
        ))}
      </div>
    </div>
  );
}
