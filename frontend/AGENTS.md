# กฎเฉพาะ Frontend (Teacher Web) — Next.js

โฟลเดอร์นี้คือ **Teacher Web** ของระบบ School Bot สำหรับครูประถมศึกษา ป.1–6 ที่สอนหลักสูตร Robotics & AI (หุ่นยนต์ Robo X)

## ขอบเขตและหน้าที่
- แผนการสอนและใบงานทุกระดับชั้น ป.1–ป.6 (`app/curriculum/`)
- คอร์สปรับพื้นฐาน Robotics, AI, Scratch (`app/foundation/`)
- คู่มือและรายละเอียดหุ่นยนต์ RoboX (`app/robox/`)
- ระบบจัดการรอบการเรียนและส่งคะแนนเข้า SchoolBright (`app/management/`)

## แนวทางการพัฒนา
- ใช้ **Next.js 14 App Router**, **TypeScript**, **Tailwind CSS**, และ **Zustand**
- ธีมสีแบรนด์: Navy `#0B2045`, Orange `#F8945C`, Sky Blue `#68C8F2`, Paper `#FFFBF5`
- ฟอนต์: Mitr (หัวข้อ) และ Sarabun (เนื้อหา)
- ไม่คัดลอก UI หรือ dependencies ขนาดใหญ่จากภายนอกเข้ามาโดยไม่จำเป็น
- การเชื่อมต่อ Backend ให้ผ่าน `lib/api/learning-api.ts` ไปยัง endpoints ของ School Bot Backend (`/api/service-school-bot/...`)

## การตรวจสอบงาน
- Build: `npm run build`
- Dev server: `npm run dev`
- Lint: `npm run lint`
