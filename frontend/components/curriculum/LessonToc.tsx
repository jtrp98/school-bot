"use client";

import { useEffect, useState } from "react";

export interface TocItem {
  id: string;
  label: string;
  children?: TocItem[];
}

function flatten(items: TocItem[]): string[] {
  return items.flatMap((item) => [item.id, ...flatten(item.children ?? [])]);
}

// สารบัญด้านขวา — ไฮไลต์หัวข้อที่กำลังอ่านอยู่ตามตำแหน่งการเลื่อน
export default function LessonToc({ items }: { items: TocItem[] }) {
  const [activeId, setActiveId] = useState(items[0]?.id);

  useEffect(() => {
    const ids = flatten(items);

    function update() {
      // หัวข้อสุดท้ายที่ขอบบนเลื่อนผ่านเส้นอ่าน (ใต้แถบเมนู) คือหัวข้อที่กำลังอ่าน
      const line = 140;
      let current = ids[0];
      for (const id of ids) {
        const el = document.getElementById(id);
        if (el && el.getBoundingClientRect().top <= line) current = id;
      }
      const atBottom =
        window.innerHeight + window.scrollY >= document.documentElement.scrollHeight - 4;
      setActiveId(atBottom ? ids[ids.length - 1] : current);
    }

    update();
    window.addEventListener("scroll", update, { passive: true });
    window.addEventListener("resize", update);
    return () => {
      window.removeEventListener("scroll", update);
      window.removeEventListener("resize", update);
    };
  }, [items]);

  function linkClass(id: string, nested: boolean) {
    const active = id === activeId;
    return `block border-l-2 py-1.5 leading-snug transition-colors ${
      nested ? "pl-6 text-[13px]" : "pl-3 text-sm font-medium"
    } ${
      active
        ? "border-orange font-semibold text-ink"
        : `border-transparent hover:border-orange/50 hover:text-ink ${nested ? "text-ink/50" : "text-ink/75"}`
    }`;
  }

  return (
    <nav aria-label="สารบัญ">
      <p className="mb-3 text-xs font-semibold uppercase tracking-[0.1em] text-ink/45">สารบัญ</p>
      <ul className="border-l border-ink/10">
        {items.map((item) => (
          <li key={item.id} className="-ml-px">
            <a href={`#${item.id}`} className={linkClass(item.id, false)}>
              {item.label}
            </a>
            {item.children?.length ? (
              <ul>
                {item.children.map((child) => (
                  <li key={child.id}>
                    <a href={`#${child.id}`} className={linkClass(child.id, true)}>
                      {child.label}
                    </a>
                  </li>
                ))}
              </ul>
            ) : null}
          </li>
        ))}
      </ul>
      <a
        href="#top"
        className="mt-4 inline-flex items-center gap-1.5 text-xs text-ink/50 transition-colors hover:text-orange-dark"
      >
        <span aria-hidden="true">↑</span> กลับขึ้นด้านบน
      </a>
    </nav>
  );
}
