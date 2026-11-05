const pdfUrl = encodeURI("/pdf/RoboX/องค์ประกอบของหุ่นยนต์ RoboX.pdf");

const sections = [
  {
    title: "โครงสร้างของแขนหุ่นยนต์",
    points: [
      "ส่วนฐาน (Base): รองรับน้ำหนักแขนทั้งหมด ต้องแข็งแรง",
      "ลิงค์ (Link/Arm): แขนที่เชื่อมต่อข้อต่อแต่ละตัว",
      "ข้อต่อ (Joint): จุดที่แขนสามารถขยับได้",
      "มือจับ (Gripper): สามารถหุบและกางได้",
    ],
  },
  {
    title: "เซอร์โวควบคุมการขยับ",
    points: [
      "มีทั้งหมด 6 ตัว (Servo ID1–ID6)",
      "ควบคุมตำแหน่งได้แม่นยำ หมุนได้ 0–180 องศา",
      "บอกตำแหน่งปัจจุบันของแขนได้",
    ],
  },
  {
    title: "ระบบควบคุม (Control System)",
    points: [
      "บอร์ดหลัก xArm AI (ชิป MM32F103CBT6) ขับเคลื่อนเซอร์โว เก็บและเรียกใช้ Action Group แบบออฟไลน์ได้",
      "บอร์ดขยาย ESP32 (Controller CoreX) รับคำสั่งจากการเขียนโปรแกรม มีไฟ RGB เซ็นเซอร์เสียง และอินฟราเรดในตัว",
      "ทำงานร่วมกันแบบสถาปัตยกรรมสองสมอง ขยายฟังก์ชันได้ง่าย",
    ],
  },
  {
    title: "Sensor และอุปกรณ์เสริม",
    points: [
      "เซ็นเซอร์สี อัลตราโซนิก สัมผัส แสง และอุณหภูมิ-ความชื้น",
      "โมดูลกล้อง AI อัจฉริยะ (WonderCam) และโมดูลโต้ตอบด้วยเสียง AI (WonderEcho)",
      "อุปกรณ์เสริมประกอบการสอนกว่า 20 ชิ้น เช่น บล็อก EVA ป้าย และแผนที่",
    ],
  },
];

export default function RoboXPage() {
  return (
    <div className="mx-auto max-w-6xl px-6 py-10">
      <div className="mb-8 flex flex-wrap items-start justify-between gap-4">
        <div className="max-w-2xl">
          <h1 className="font-display text-3xl font-semibold text-ink">
            ข้อมูลเบื้องต้นของ RoboX
          </h1>
          <p className="mt-2 text-sm text-ink/70">
            ภาพรวมโครงสร้าง เซอร์โว ระบบควบคุม และ Sensor ของหุ่นยนต์ RoboX
            ก่อนเริ่มสอน — เปิดอ่านเอกสารฉบับเต็มได้ในหน้านี้โดยไม่ต้องดาวน์โหลด
          </p>
        </div>

        <div className="flex flex-wrap items-center gap-2">
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

      <div className="mb-10 grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        {sections.map((section) => (
          <div
            key={section.title}
            className="rounded-2xl border border-ink/10 bg-white p-5 shadow-sm"
          >
            <h2 className="font-display text-base font-semibold text-ink">
              {section.title}
            </h2>
            <ul className="mt-3 space-y-2 text-sm text-ink/70">
              {section.points.map((point) => (
                <li key={point} className="flex gap-2">
                  <span className="mt-1.5 h-1.5 w-1.5 shrink-0 rounded-full bg-sky" />
                  <span>{point}</span>
                </li>
              ))}
            </ul>
          </div>
        ))}
      </div>

      <section className="overflow-hidden rounded-2xl border border-ink/10 bg-white shadow-sm">
        <div className="flex flex-wrap items-center justify-between gap-2 border-b border-ink/10 bg-paper-dim px-4 py-3">
          <div>
            <p className="font-display text-base font-semibold text-ink">
              เอกสาร: องค์ประกอบของหุ่นยนต์ RoboX
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
          title="ข้อมูลเบื้องต้นของ RoboX"
          src={`${pdfUrl}#toolbar=1&navpanes=0&view=FitH`}
          className="h-[80vh] w-full bg-white"
        />
      </section>
    </div>
  );
}
