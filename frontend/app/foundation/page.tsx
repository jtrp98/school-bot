import Link from "next/link";
import FoundationBreadcrumb from "@/components/FoundationBreadcrumb";
import { foundationCourses } from "@/lib/foundation-data";

function totalHours(course: (typeof foundationCourses)[number]) {
  return course.modules.reduce((sum, m) => sum + m.hours, 0);
}

export default function FoundationHub() {
  return (
    <div className="max-w-4xl mx-auto px-6 py-12">
      <FoundationBreadcrumb items={[{ label: "คอร์สปรับพื้นฐาน" }]} />
      <h1 className="font-display text-3xl font-semibold mb-2">คอร์สปรับพื้นฐาน</h1>
      <p className="text-ink/70 text-sm mb-10 max-w-2xl">
        แยกจากหลักสูตรหลัก ป.1–6 ใช้สำหรับเด็กที่ยังไม่เคยเรียนเนื้อหาก่อนหน้า
        (เช่นปีแรกที่โรงเรียนเริ่มใช้หลักสูตรนี้ หรือนักเรียนย้ายเข้าใหม่)
        ครูเลือกใช้เฉพาะคอร์สที่จำเป็นก่อนเข้าเนื้อหาประจำชั้นด้านล่างได้ ไม่ต้องเรียนครบทุกคอร์ส
      </p>

      <div className="grid sm:grid-cols-2 gap-4">
        {foundationCourses.map((c) => {
          const disabled = c.status !== "ready" && c.status !== "draft";
          const hours = totalHours(c);
          const card = (
            <div
              className={`h-full bg-white rounded-xl border p-5 transition-all ${
                disabled
                  ? "border-ink/10 opacity-60"
                  : "border-ink/10 hover:border-sky hover:shadow-md"
              }`}
            >
              <div className="flex items-start justify-between gap-2 mb-2">
                <h2 className="font-display text-lg font-semibold">{c.title}</h2>
                {c.status === "coming-soon" ? (
                  <span className="shrink-0 text-xs bg-paper-dim text-ink/50 px-2 py-1 rounded-chip">
                    เร็วๆ นี้
                  </span>
                ) : c.status === "draft" ? (
                  <span className="shrink-0 text-xs bg-orange-light/40 text-orange-dark px-2 py-1 rounded-chip">
                    กำลังจัดทำ
                  </span>
                ) : (
                  <span className="shrink-0 text-xs bg-sky/20 text-sky-dark px-2 py-1 rounded-chip">
                    {hours} ชม. · {c.modules.length} บท
                  </span>
                )}
              </div>
              <p className="text-sm text-ink/60">{c.subtitle}</p>
            </div>
          );
          return disabled ? (
            <div key={c.slug}>{card}</div>
          ) : (
            <Link key={c.slug} href={`/foundation/${c.slug}`}>
              {card}
            </Link>
          );
        })}
      </div>
    </div>
  );
}
