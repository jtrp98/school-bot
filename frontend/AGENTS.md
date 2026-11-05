# Teacher Web (Next.js 14 App Router)

ใช้ร่วมกับกฎแม่ที่ [../AGENTS.md](../AGENTS.md) ขอบเขตโฟลเดอร์นี้คือ **Teacher Web** ของหลักสูตร RoboX ป.1–6

> **บริบท**: เป็นเว็บคู่มือและแผนการสอนสำหรับครูผู้สอนในห้องเรียน เน้นความกระชับ โหลดเร็ว และค้นหาข้อมูลได้ภายใน 5 วินาที

## ขอบเขตโมดูล
- `app/curriculum/`: แผนการสอนและใบงาน ป.1–ป.6 (Basic, Intermediate, Advance)
- `app/foundation/`: คอร์สปรับพื้นฐาน Robotics, AI, Scratch
- `app/robox/`: คู่มือและสเปกหุ่นยนต์ RoboX
- `app/management/`: จัดการรอบการเรียนและส่งคะแนนเข้า SchoolBright

## สถาปัตยกรรมและการแบ่งหน้าที่ (Next.js 14)
- Stack: **Next.js 14 App Router**, **TypeScript**, **Tailwind CSS**, และ **Zustand**
- Design Tokens: Navy `#0B2045`, Orange `#F8945C`, Sky Blue `#68C8F2`, Paper `#FFFBF5` | ฟอนต์: Mitr (หัวข้อ), Sarabun (เนื้อหา)
- **การแยกชั้นในแต่ละ Route (`app/<domain>/`)**:
  - `page.tsx`: ประกอบหน้าหลัก **ห้ามเกิน 300 บรรทัด**
  - `_components/`: UI components เฉพาะหน้านั้น
  - `_stores/`: Shared state ระดับ feature ด้วย Zustand
  - `_hooks/`: Workflow, effects และ data fetch orchestration
  - `_api/`: HTTP client functions เชื่อมต่อ Backend
  - `_utils/`: Pure calculations, formatting, และ business validation
- Shared components ข้ามหน้า ให้วางไว้ที่ `src/components/` หรือ `components/` ตามโครงสร้างเดิม

## การเชื่อมต่อ Backend และความปลอดภัย
- เชื่อมต่อผ่าน `lib/api/learning-api.ts` ไปยัง School Bot Backend (`/api/service-school-bot/...`) ผ่าน Gateway
- รักษา Session cookie ใน `middleware.ts` อย่างเคร่งครัด ห้าม bypass auth เพื่อให้ทดสอบหน้าจอได้
- **Why-only Comments**: อธิบายเฉพาะเหตุผลข้อจำกัดทางเทคนิค หรือความปลอดภัยของระบบ ห้ามใส่ประวัติการแก้ไข

## คำสั่งตรวจงาน
- รัน `npm run lint` เพื่อตรวจ code style และ type issues
- รัน `npm run build` เพื่อตรวจ build errors ของ App Router
