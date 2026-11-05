"use client";

import type { ReactNode, SyntheticEvent } from "react";

// ลดการคัดลอกเนื้อหาแผนการสอน: ปิดการเลือกข้อความ คัดลอก ลาก คลิกขวา และซ่อนตอนสั่งพิมพ์
// เป็นเพียงการป้องกันขั้นพื้นฐาน ไม่สามารถกันผู้ที่ตั้งใจดึงข้อมูลได้ 100%
export default function ProtectedArticle({ children }: { children: ReactNode }) {
  function block(event: SyntheticEvent) {
    event.preventDefault();
  }

  return (
    <>
      <div
        className="select-none print:hidden"
        onCopy={block}
        onCut={block}
        onDragStart={block}
        onContextMenu={block}
      >
        {children}
      </div>
      <p className="hidden print:block">
        เนื้อหาแผนการสอนนี้เปิดดูได้บนเว็บ School Bot เท่านั้น
      </p>
    </>
  );
}
