<div align="center">

# 🤖 School Bot — คู่มือครู RoboX

### เว็บคู่มือครูแบบ All-in-One สำหรับหลักสูตร Robotics & AI

*แผนการสอน · คอร์สปรับพื้นฐาน · วิธีติดตั้ง · คลังใบงาน — เปิดแล้วสอนได้เลย ไม่ต้องเตรียมเอง*

<br/>

![Next.js](https://img.shields.io/badge/Next.js-14.2-000000?style=for-the-badge&logo=next.js&logoColor=white)
![TypeScript](https://img.shields.io/badge/TypeScript-5.5-3178C6?style=for-the-badge&logo=typescript&logoColor=white)
![TailwindCSS](https://img.shields.io/badge/Tailwind_CSS-3.4-38BDF8?style=for-the-badge&logo=tailwindcss&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-ready-2496ED?style=for-the-badge&logo=docker&logoColor=white)

</div>

<br/>

## 📖 นี่คือโปรเจกต์อะไร

**School Bot** คือเว็บแอปสำหรับ **ครูประถมศึกษา ป.1–6** ที่สอนหลักสูตร Robotics & AI โดยใช้หุ่นยนต์ **Robo X** ของ School Bot

ปัญหาที่แก้: ครูต้อง**เปิดเว็บก่อนหรือระหว่างสอน** เพื่อดูแผนการสอน วิธีติดตั้งโปรแกรม และใบงาน — ต้องหาสิ่งที่ต้องการได้ภายใน **5 วินาที** เพราะกำลังยืนสอนอยู่หน้าห้อง ไม่มีเวลาอ่านอะไรยาว ๆ

> 🎯 **เป้าหมายความสำเร็จ:** ครูใช้จริงในห้องเรียน และรู้สึกมั่นใจขณะสอน

<br/>

## ✨ ฟีเจอร์หลัก

<table>
<tr>
<td width="50%" valign="top">

### 🔐 ระบบล็อกอินคุ้มครองเนื้อหา
เนื้อหาแผนการสอนทั้งหมดถูกป้องกันด้วย `middleware.ts` ที่เช็ก session cookie ทุกหน้า ยกเว้น `/login` และ `/api/login` — ผู้ใช้ที่ไม่ผ่านการยืนยันตัวตนจะถูกเด้งกลับหน้า login โดยอัตโนมัติ

</td>
<td width="50%" valign="top">

### 📚 แบ่งระดับชั้นชัดเจน
หลักสูตรแบ่งเป็น 3 ระดับ:
- 🔵 **Basic** (ป.1–2)
- 🟠 **Intermediate** (ป.3–4)
- ⚫ **Advance** (ป.5–6)

แต่ละระดับมีสี badge และธีมของตัวเอง

</td>
</tr>
<tr>
<td width="50%" valign="top">

### 🧩 คอร์สปรับพื้นฐาน (Foundation)
เนื้อหาแยกเป็น 3 สาย: **Robotics · AI · Scratch** เขียนเป็นไฟล์ Markdown ล้วน ๆ ใน `content/` แล้ว render เป็นหน้าเว็บสวย ๆ ให้อัตโนมัติ — เพิ่มบทเรียนใหม่แค่เพิ่มไฟล์ `.md`

</td>
<td width="50%" valign="top">

### 📝 คลังใบงาน & วิธีติดตั้ง
หน้า `/exercises` และ `/install` รวมทุกอย่างที่ครูต้องใช้ก่อนเริ่มคาบเรียน ให้เปิดเจอในคลิกเดียว

</td>
</tr>
</table>

<br/>

## 🎨 ดีไซน์ที่ไม่เหมือนใคร

โทนสีทั้งหมดดึงมาจากโลโก้ School Bot โดยตรง เพื่อให้แบรนด์เป็นหนึ่งเดียวกันทั้งเว็บ

<div align="center">

| สี | Hex | ใช้ทำอะไร |
|---|---|---|
| 🟠 **Orange** (มาสคอต) | `#F8945C` | ปุ่ม, badge, จุดเน้นความสดใส |
| 🔵 **Sky Blue** (มาสคอต) | `#68C8F2` | ลิงก์, การ์ดระดับ Basic |
| 🔷 **Navy** | `#0B2045` | ข้อความหลัก, พื้นหลังเข้ม |
| 🤍 **Paper** | `#FFFBF5` | พื้นหลังหลัก อ่านสบายตา |

</div>

**หลักการออกแบบ:**

- ✅ **Clarity first** — ครูต้องหาของที่ต้องการได้ใน 5 วินาที
- ✅ **Confident but warm** — ดูมืออาชีพ แต่ไม่เย็นชา เข้ากับบรรยากาศห้องเรียนเด็ก
- ✅ **Content is the hero** — UI ต้องส่งเสริมเนื้อหา ไม่แย่งซีน
- ✅ **Tablet-first & Accessible** — รองรับ WCAG 2.1 AA, contrast ขั้นต่ำ 4.5:1, ใช้งานได้ทั้งจอโปรเจกเตอร์และ iPad พร้อม Reduced-motion support

ตัวอักษรใช้ฟอนต์ไทยอ่านง่าย: **Mitr** (หัวข้อ) + **Sarabun** (เนื้อหา)

<br/>

## 🗂️ โครงสร้างโปรเจกต์

```
sb-web-school-robot/
├── 📁 app/                      # Next.js App Router
│   ├── page.tsx                 # 🏠 หน้าแรก — สรุปหลักสูตรทุกระดับ
│   ├── login/                   # 🔐 หน้าล็อกอิน
│   ├── grades/[grade]/          # 🎒 แผนการสอนแยกตามชั้นปี
│   ├── foundation/[slug]/       # 🧱 คอร์สปรับพื้นฐาน (robotics/ai/scratch)
│   ├── exercises/               # 📝 คลังใบงาน
│   ├── install/                 # 💾 วิธีติดตั้งโปรแกรม
│   ├── robox/                   # 🤖 ข้อมูลหุ่นยนต์ Robo X
│   └── api/login/               # ⚙️ API ยืนยันตัวตน
│
├── 📁 components/               # Nav, Breadcrumb ฯลฯ
├── 📁 lib/                      # ตรรกะ + ข้อมูลหลักสูตรทั้งหมด (TypeScript)
├── 📁 content/                  # 📄 เนื้อหาบทเรียนเป็นไฟล์ Markdown
│   └── foundation/{robotics,ai,scratch}/
├── 📁 public/                   # ไฟล์ static (รูป, ไอคอน)
├── middleware.ts                # 🛡️ ตรวจสอบสิทธิ์เข้าถึงทุกหน้า
├── Dockerfile                   # 🐳 Build image สำหรับ production
└── tailwind.config.ts           # 🎨 โทนสีและฟอนต์ของแบรนด์
```

<br/>

## 🚀 เริ่มต้นใช้งาน (Local Development)

### สิ่งที่ต้องมี

- **Node.js** 20 ขึ้นไป
- **npm**

### ขั้นตอน

```bash
# 1️⃣ ติดตั้ง dependencies
npm install

# 2️⃣ รันเซิร์ฟเวอร์สำหรับพัฒนา
npm run dev
```

เปิดเบราว์เซอร์ไปที่ **http://localhost:3000** 🎉

### คำสั่งอื่น ๆ ที่มีให้ใช้

| คำสั่ง | ทำหน้าที่อะไร |
|---|---|
| `npm run dev` | 🛠️ รันโหมดพัฒนา (hot reload) |
| `npm run build` | 📦 Build เวอร์ชัน production |
| `npm run start` | ▶️ รันเวอร์ชันที่ build แล้ว |
| `npm run lint` | 🔍 ตรวจสอบคุณภาพโค้ด |

<br/>

## 🔑 ตัวแปรสภาพแวดล้อม (Environment Variables)

ระบบล็อกอินอ่านค่าจาก environment variables เหล่านี้ (มีค่า default ให้ใช้ตอน dev แต่ **ควรตั้งค่าเองเสมอบน production**):

```bash
SITE_USERNAME=Teacher          # ชื่อผู้ใช้สำหรับเข้าเว็บ
SITE_PASSWORD=SB2026           # รหัสผ่านสำหรับเข้าเว็บ
SESSION_SECRET=your-own-secret # secret สำหรับเซ็น session token
```

> ⚠️ **สำคัญ:** ค่า default ในโค้ดมีไว้เพื่อความสะดวกตอนพัฒนาเท่านั้น ห้ามใช้ค่า default บน production เด็ดขาด!

<br/>

## 🐳 Deploy ด้วย Docker

โปรเจกต์นี้มี `Dockerfile` แบบ **multi-stage build** พร้อมใช้งานจริง (เบา, ปลอดภัย, รันด้วย non-root user)

```bash
# 🏗️ Build image
docker build -t school-bot-web .

# ▶️ รัน container
docker run -p 3000:3000 \
  -e SITE_USERNAME=Teacher \
  -e SITE_PASSWORD=your-secure-password \
  -e SESSION_SECRET=your-secure-secret \
  school-bot-web
```

จากนั้นเปิด **http://localhost:3000** ได้เลย ✅

<details>
<summary>📋 <b>Docker build ทำงานยังไง (คลิกเพื่อดูรายละเอียด)</b></summary>

<br/>

Dockerfile แบ่งเป็น 3 stage เพื่อให้ image สุดท้ายเล็กและปลอดภัยที่สุด:

1. **`deps`** — ติดตั้ง dependencies ด้วย `npm ci` (ใช้ `package-lock.json` เพื่อความ deterministic)
2. **`builder`** — build แอปด้วย Next.js `output: "standalone"` เพื่อดึงเฉพาะไฟล์ที่จำเป็นจริง ๆ
3. **`runner`** — image สุดท้าย รันด้วย user `nextjs` (non-root) เพื่อความปลอดภัย

</details>

<br/>

## 🏗️ เทคโนโลยีที่ใช้

<div align="center">

| หมวด | เทคโนโลยี |
|---|---|
| **Framework** | [Next.js 14](https://nextjs.org/) (App Router) |
| **ภาษา** | [TypeScript](https://www.typescriptlang.org/) |
| **สไตล์** | [Tailwind CSS](https://tailwindcss.com/) + `@tailwindcss/typography` |
| **เนื้อหา** | Markdown (`marked`) render จากไฟล์ `.md` |
| **Deploy** | Docker (multi-stage, standalone output) |

</div>

<br/>

## 🧭 แนวทางที่ *ไม่ต้องการ* ให้เว็บนี้เป็น

เพื่อรักษาตัวตนของแบรนด์ School Bot ให้ชัดเจน:

- ❌ ไม่เหมือน Google Classroom หรือ LMS ที่ดูน่าเบื่อซับซ้อน
- ❌ ไม่เหมือนเว็บราชการไทยที่ดูล้าสมัย
- ❌ ไม่ใช้สีสดจัดจนดูเป็นของเล่นแบบ EdTech Startup ทั่วไป
- ❌ ไม่ใช้ Dark mode ที่ดู dramatic เกินไปสำหรับบรรยากาศห้องเรียน

<br/>

---

<div align="center">

สร้างด้วย ❤️ เพื่อครูไทยที่กำลังพาเด็ก ๆ ก้าวเข้าสู่โลกของ **Robotics & AI** 🤖✨

</div>
