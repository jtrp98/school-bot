import { installGuides } from "@/lib/install-data";

export default function InstallPage() {
  return (
    <div className="mx-auto max-w-4xl px-6 py-12">
      <p className="mb-1 text-sm font-medium text-alert">ก่อนเข้าห้องเรียน</p>
      <h1 className="mb-2 font-display text-3xl font-semibold">
        วิธีติดตั้งโปรแกรม
      </h1>
      <p className="mb-4 max-w-2xl text-sm text-ink/70">
        รวมทุกโปรแกรมที่ใช้ในหลักสูตรนี้ไว้ที่นี่ — เว็บนี้เป็นแค่คู่มืออ้างอิง
        ไม่ได้รันโปรแกรมเหล่านี้ ต้องติดตั้งลงเครื่องหรือมือถือจริงก่อนเริ่มสอน
      </p>

      <div className="mb-8 flex flex-wrap gap-3">
        <a
          href="https://drive.google.com/drive/folders/1mCExIXgQAWu7y2HCThggxR9Rf-jPkqJC"
          target="_blank"
          rel="noreferrer"
          className="inline-flex items-center rounded-chip bg-orange px-5 py-2.5 font-display font-medium text-navy-dark transition-colors duration-150 hover:bg-orange-dark"
        >
          ดาวน์โหลดโปรแกรม
        </a>

        <a
          href="https://drive.google.com/drive/folders/1bwEQ0DpBxZLljv4A9OxHMzqHle1BhSpN"
          target="_blank"
          rel="noreferrer"
          className="inline-flex items-center rounded-chip border border-sky/30 bg-sky/10 px-5 py-2.5 font-display font-medium text-sky-dark transition-colors duration-150 hover:bg-sky/20"
        >
          Action Files
        </a>
      </div>

      <div className="grid gap-6">
        {installGuides.map((guide) => (
          <div
            key={guide.slug}
            id={guide.slug}
            className="rounded-xl border border-ink/10 bg-white p-6"
          >
            <div className="mb-1 flex flex-wrap items-baseline justify-between gap-2">
              <h2 className="font-display text-xl font-semibold">{guide.name}</h2>
              <span className="text-xs text-ink/50">{guide.platform}</span>
            </div>

            <p className="mb-4 text-xs text-ink/50">
              ใช้ในชั้น: {guide.usedInGrades.map((grade) => `ป.${grade}`).join(", ")}
            </p>

            <ol className="mb-4 list-inside list-decimal space-y-1.5 text-sm text-ink/80">
              {guide.steps.map((step, index) => (
                <li key={index}>{step}</li>
              ))}
            </ol>

            <div className="flex flex-wrap gap-2">
              {guide.href ? (
                <a
                  href={guide.href}
                  target="_blank"
                  rel="noreferrer"
                  className="inline-flex items-center rounded-chip bg-paper-dim px-3 py-1.5 text-xs text-ink/60 transition-colors hover:bg-paper hover:text-ink"
                >
                  {guide.downloadNote}
                </a>
              ) : (
                <span className="inline-block rounded-chip bg-paper-dim px-3 py-1.5 text-xs text-ink/60">
                  {guide.downloadNote}
                </span>
              )}

              {guide.sampleCodeHref ? (
                <a
                  href={guide.sampleCodeHref}
                  target="_blank"
                  rel="noreferrer"
                  className="inline-flex items-center rounded-chip border border-orange/30 bg-orange/10 px-3 py-1.5 text-xs font-medium text-orange-dark transition-colors hover:bg-orange/20"
                >
                  ตัวอย่างโค้ด
                </a>
              ) : null}
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
