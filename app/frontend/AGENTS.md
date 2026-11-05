# Desktop Frontend (React + TypeScript)

ใช้ร่วมกับกฎแม่ที่ [../AGENTS.md](../AGENTS.md) และ [../../AGENTS.md](../../AGENTS.md) ขอบเขตไฟล์นี้คือ `app/frontend/`

> **คำเตือน**: โฟลเดอร์นี้คือ Desktop UI ห้ามนำโครงสร้าง Next.js ของ Teacher Web (`frontend/`) หรือ Admin Web มาปนเปื้อน

## สถาปัตยกรรมและการเชื่อมต่อ
- Frontend ติดต่อ Host ผ่าน `app/frontend/src/core/client.ts` เท่านั้น ห้ามเรียก OS, filesystem, shell, executable หรือ database โดยตรง
- รักษา TypeScript strict mode: ห้ามปิด `noUnusedLocals` และ `noUnusedParameters` และห้ามใช้ `any` ในโค้ดใหม่
- **การแยกชั้น (Separation of Concerns)**:
  - **TSX**: รับผิดชอบ Layout, HTML structure, Tailwind className ตามสถานะ
  - **Hook / Module**: รับผิดชอบ State, Business logic, Derived flags และ Event handlers
  - **View-Model Builder**: สร้าง Pure Data สำหรับแสดงผล ห้ามมี side effect หรือ assign ค่าภายนอกระหว่าง render
- **ข้อห้ามด้าน Styling**: ห้ามประกอบ CSS string หรือ dynamic classes ใน `.ts` แล้วส่งให้ TSX แปลง ให้เขียน Tailwind classes ใน TSX โดยตรง

## ขนาดไฟล์ (300 / 500 บรรทัด)
- **เกิน 300 บรรทัด**: สัญญาณเตือนให้ทบทวนหน้าที่ซ้ำซ้อน และประเมินการแยก component/hook
- **เกิน 500 บรรทัด**: ต้องวางแผนแยกหน้าที่ก่อนเพิ่มโค้ดต่อ หรือสรุปเหตุผลความจำเป็นที่ต้องรวมในรายงานการส่งมอบ
- **ข้อห้าม**: ห้ามแตกไฟล์เพียงเพื่อลดจำนวนบรรทัด ห้ามย่อคำสั่งลงบรรทัดเดียวเพื่อหลบเกณฑ์ และไม่ refactor นอกขอบเขตงาน
- **ข้อยกเว้น**: Generated bindings, vendored code, mock scenarios และ JSON คำแปล

## UI, tokens และการรองรับภาษา (i18n)
- ใช้ Shared components ที่มีอยู่ใน `src/components/shared/` ก่อนสร้างใหม่ (เช่น `Button variant="secondary"`, `Modal`)
- **ห้าม Hardcode ข้อความ UI**: ทุกข้อความปุ่ม หัวข้อ dialog, tooltip, toast ต้องมี key ใน `src/i18n/locales/th.json`, `en.json`, `zh.json` เสมอ
- **i18n Best Practices**: ห้ามต่อ string แปลภาษาด้วยเครื่องหมาย `+` ให้ใช้ interpolation key เต็มประโยค
- **ห้ามแปลข้อมูลดิบ**: ชื่องาน, ชื่อท่า, ชื่อหุ่น, โค้ดผู้ใช้, ค่าเซนเซอร์ และ diagnostics จากฮาร์ดแวร์ ห้ามแปลงหรือแปลภาษา
- **Sample vs Real Hardware**: Handler จำลองผล (เช่น `setTimeout` จำลอง upload) ใช้ได้เฉพาะ Sample mode และต้องแยกจาก path อุปกรณ์จริงเด็ดขาด

## คำสั่งตรวจงาน
- รัน `npm run typecheck` และ `npm run build` จาก `app/frontend`
- เมื่อเพิ่มหรือแก้ข้อความ UI/i18n ให้รัน `npm run test:i18n`
- เมื่อแก้พฤติกรรมหุ่นยนต์ ให้รัน tests ใน `app/frontend/tests/`
