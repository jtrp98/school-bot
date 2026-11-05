import Link from "next/link";
import FoundationBreadcrumb from "@/components/FoundationBreadcrumb";
import { foundationCourses } from "@/lib/foundation-data";
import { buildFoundationSequence } from "@/lib/foundation-navigation";

const pdfUrl = encodeURI("/pdf/RoboX/องค์ประกอบของหุ่นยนต์ RoboX.pdf");

const highlights = [
  "โครงสร้างและชิ้นส่วนหลักของ RoboX",
  "ระบบเซอร์โวและการเคลื่อนไหวของแขนกล",
  "ระบบควบคุมและการเชื่อมต่อใช้งาน",
  "เซนเซอร์ที่เกี่ยวข้องกับการเรียนการสอน",
];

export default function RoboXComponentsPage() {
  const course = foundationCourses.find((item) => item.slug === "robotics");
  const sequence = course ? buildFoundationSequence(course) : [];
  const currentIndex = sequence.findIndex(
    (item) => item.href === "/foundation/robotics/robox-components",
  );
  const prev = sequence[currentIndex - 1];
  const next = sequence[currentIndex + 1];
  const courseHref = "/foundation/robotics";

  return (
    <div className="max-w-6xl mx-auto px-6 py-10">
      <FoundationBreadcrumb
        items={[
          { label: "คอร์สปรับพื้นฐาน", href: "/foundation" },
          { label: "Foundation to Robotics", href: courseHref },
          { label: "องค์ประกอบของ RoboX" },
        ]}
      />
      <div className="mb-6 flex flex-wrap items-start justify-between gap-4">
        <div className="max-w-2xl">
          <h1 className="font-display text-3xl font-semibold text-ink">
            องค์ประกอบของ RoboX
          </h1>
          <p className="mt-2 max-w-2xl text-sm text-ink/70">
            เปิดดูเอกสารอ้างอิงเรื่องโครงสร้าง เซอร์โว ระบบควบคุม และ Sensor
            ของ RoboX ได้ในหน้าเดียว เหมาะสำหรับเปิดประกอบการสอนหรือทบทวนก่อนขึ้นคาบ
          </p>
        </div>

        <div className="flex flex-wrap items-center gap-2">
          <Link
            href="/foundation/robotics"
            className="inline-flex items-center rounded-chip border border-ink/15 bg-white px-4 py-2 text-sm text-ink/70 transition-colors hover:bg-ink/5"
          >
            กลับไปหน้า Robotics
          </Link>
          <a
            href={pdfUrl}
            target="_blank"
            rel="noreferrer"
            className="inline-flex items-center rounded-chip border border-sky/30 bg-sky/10 px-4 py-2 text-sm text-sky-dark transition-colors hover:border-sky hover:bg-sky/20"
          >
            เปิดเต็มหน้า
          </a>
          <a
            href={pdfUrl}
            download="องค์ประกอบของหุ่นยนต์ RoboX.pdf"
            className="inline-flex items-center rounded-chip bg-orange px-4 py-2 text-sm font-medium text-navy-dark transition-colors hover:bg-orange-dark"
          >
            ดาวน์โหลด PDF
          </a>
        </div>
      </div>

      <div className="grid gap-4 lg:grid-cols-[minmax(0,1fr)_280px]">
        <section className="overflow-hidden rounded-2xl border border-ink/10 bg-white shadow-sm">
          <div className="flex flex-wrap items-center justify-between gap-2 border-b border-ink/10 bg-paper-dim px-4 py-3">
            <div>
              <p className="font-display text-base font-semibold text-ink">
                เอกสารองค์ประกอบของหุ่นยนต์ RoboX
              </p>
              <p className="text-xs text-ink/50">
                แสดงไฟล์จาก public/pdf ภายในหน้าเว็บ
              </p>
            </div>
            <span className="rounded-chip bg-white px-2.5 py-1 text-xs text-ink/60">
              PDF Viewer
            </span>
          </div>

          <iframe
            title="องค์ประกอบของ RoboX"
            src={`${pdfUrl}#toolbar=1&navpanes=0&view=FitH`}
            className="h-[78vh] w-full bg-white"
          />
        </section>

        <aside className="space-y-4">
          <div className="rounded-2xl border border-ink/10 bg-white p-5 shadow-sm">
            <h2 className="font-display text-lg font-semibold text-ink">
              ใช้เอกสารนี้เมื่อไร
            </h2>
            <ul className="mt-3 space-y-2 text-sm text-ink/70">
              {highlights.map((item) => (
                <li key={item} className="flex gap-2">
                  <span className="mt-1.5 h-1.5 w-1.5 shrink-0 rounded-full bg-sky" />
                  <span>{item}</span>
                </li>
              ))}
            </ul>
          </div>

          <div className="rounded-2xl border border-orange/20 bg-orange-light/20 p-5">
            <h2 className="font-display text-lg font-semibold text-navy-dark">
              เอกสารประกอบการสอน
            </h2>
            <p className="mt-2 text-sm text-ink/70">
              ใช้หน้านี้เปิดเอกสารอ้างอิงควบคู่กับคาบเรียนใน Foundation to Robotics
              เพื่อทบทวนภาพรวมของโครงสร้าง เซอร์โว ระบบควบคุม และ Sensor ก่อนเข้าสู่บทเรียนถัดไป
            </p>
          </div>
        </aside>
      </div>

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
              <span className="font-medium">หน้า Foundation to Robotics</span>
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
              <span className="font-medium">ดูภาพรวม Foundation to Robotics</span>
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
