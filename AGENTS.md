# กฎร่วม School Bot (Root Directives)

คำสั่งล่าสุดของผู้ใช้มีลำดับสูงสุดเหนือกฎทุกข้อ อ่าน `README.md` และเอกสารของส่วนที่เกี่ยวข้องก่อนทำงาน หากเอกสารไม่ตรงกับโค้ดจริง ให้ยึดโค้ดจริงและรายงานข้อแตกต่าง

## สารบัญกฎเฉพาะส่วน (Rule Hierarchy)
อ่านเฉพาะกฎตามเส้นทางของไฟล์ที่ทำงาน ไม่อ่านทุกส่วนตั้งแต่เริ่ม:

| ส่วนของระบบ | ขอบเขตหน้าที่ | กฎเฉพาะส่วน |
|---|---|---|
| **1. Desktop Host** | Go + Wails host, window controls, engine lifecycle | [app/AGENTS.md](app/AGENTS.md) |
| **2. Desktop Frontend** | React + TypeScript, robot UI, 3D/Phaser views | [app/frontend/AGENTS.md](app/frontend/AGENTS.md) |
| **3. Teacher Web** | Next.js 14 App Router, แผนการสอน RoboX ป.1–6 | [frontend/AGENTS.md](frontend/AGENTS.md) *(อ้างอิงภายนอก: [docs/teacher-web.md](docs/teacher-web.md))* |
| **4. Engines** | C++ IPC engines, หุ่นยนต์ RoboX, simulators | [engines/AGENTS.md](engines/AGENTS.md) |
| **5. Backend API** | C# gRPC microservice, Clean Architecture | [backend/AGENTS.md](backend/AGENTS.md) |
| **6. Admin Web FE** | School Bot Admin Console (รวมใน frontend/app/admin) | [docs/admin-web.md](docs/admin-web.md) |
| **7. Build & Scripts** | Script ตรวจสอบ, packaging, build installer | [scripts/AGENTS.md](scripts/AGENTS.md) |

> **Single Source of Truth**: ทุกไฟล์ `CLAUDE.md` มีเพียง `@AGENTS.md` เท่านั้น ไม่คัดลอกกฎซ้ำซ้อนหรือ import กฎลูกทั้งหมดไว้ที่ root

---

## พฤติกรรม AI และการรายงานผล (AI Directives)
- **กระชับขั้นสุด**: ตอบตรงประเด็น ไม่อารัมภบท ไม่ทวนคำสั่งซ้ำ ไม่แสดงความเห็นส่วนตัวที่ไม่เกี่ยวกับงาน
- **โครงสร้างการรายงาน**: สรุปผลลัพธ์เป็นภาษาไทยสั้นๆ ชัดเจน 3 ส่วนเสมอ:
  1. **สิ่งที่แก้**: ระบุไฟล์และจุดสำคัญที่แก้ไข
  2. **ผลการตรวจ**: คำสั่งที่รันและผลลัพธ์ (Build / Test / Lint)
  3. **ข้อจำกัดหรือบล็อกเกอร์**: ระบุสิ่งที่ยังไม่ได้ตรวจหรือติดขัด (ถ้ามี)
- **รายงานตามจริง**: ห้ามอ้างว่างานสำเร็จหรือผ่านการตรวจ หากยังไม่ได้รันคำสั่งจริง หรือถูก sandbox/toolchain ขวาง

---

## กฎการเขียนคอมเมนต์ (Why-only Comments)
- **คอมเมนต์ต้องมีเฉพาะ "Why" เท่านั้น**: บังคับใช้ในทุกภาษา (Go, TS, C++, C#)
- **Code is Self-Documenting**: ชื่อตัวแปร ฟังก์ชัน และโครงสร้างโค้ดต้องอธิบาย What และ How ด้วยตัวเอง
- **สิ่งต้องห้ามในคอมเมนต์**:
  - ห้ามใส่ Task ID, Ticket ID, Issue URL, วันที่, หรือชื่อผู้แก้ไข (Author)
  - ห้ามใส่ประวัติการสนทนา, บันทึกการตัดสินใจชั่วคราว, หรือรายงานความคืบหน้า
  - ห้ามเขียนคอมเมนต์อธิบายซ้ำว่าโค้ดทำอะไร (เช่น `// increment counter`)
- **สิ่งที่อนุญาตให้คอมเมนต์**:
  - เหตุผลเบื้องหลังของข้อจำกัดทางเทคนิค (Why)
  - ข้อควรระวังด้านความปลอดภัย (Safety & Hardware invariants)
  - Workaround ทางเทคนิคที่มีเหตุผลจำเป็น และไม่สามารถสื่อสารผ่านชื่อโค้ดได้โดยตรง
- **สงวนไว้**: เก็บ compiler directives, license, copyright และ attribution ที่จำเป็น ห้ามลบ

---

## สัญญาระบบและมาตรการความปลอดภัยฮาร์ดแวร์ (Safety & Protocol)
- **ความปลอดภัยของฮาร์ดแวร์**:
  - ห้ามสั่งขยับหุ่นยนต์จริง อัปโหลด firmware หรือเปลี่ยนสถานะฮาร์ดแวร์โดยไม่มีคำสั่งชัดเจนจากผู้ใช้
  - แยก **Sample/Preview Mode** ออกจากฮาร์ดแวร์จริงเด็ดขาด Sample Mode ต้องไม่ส่งสัญญาณหรือคำสั่งใดๆ ไปยังอุปกรณ์จริง
  - **OS Write สำเร็จ ไม่เท่ากับ สถานะจริงของฮาร์ดแวร์**: ห้ามนำค่าจำลองมาแสดงเป็นผลอ่านเซนเซอร์จริง
  - **Cancel Process ไม่เท่ากับ Physical Stop**: ต้อง drain คิวงานเดิมก่อนส่งคำสั่งหยุดฉุกเฉิน และป้องกัน callbacks เก่าส่งคำสั่งต่อ
- **โพรโตคอล IPC**: ใช้ JSON Lines UTF-8 เท่านั้น โดย stdout สำหรับ protocol และ stderr สำหรับ diagnostics; ต้องรักษา `request_id` matching, timeout, bounds, และ cancellation
- **ความปลอดภัยของสภาพแวดล้อม**:
  - ห้ามปิด process ของผู้ใช้ ห้ามติดตั้งทับ หรือถอนการติดตั้งเพื่อทดสอบโดยพลการ
  - การลบไฟล์บน Windows ให้ใช้ PowerShell `-LiteralPath` และตรวจ absolute path ให้อยู่ในขอบเขตงานเสมอ

---

## การแก้ไขโค้ดและการตรวจงาน (Engineering Standards)
- **Clean Scope**: แก้เฉพาะขอบเขตงาน รักษาพฤติกรรมเดิมส่วนที่ไม่เกี่ยวข้อง ไม่ทับงานผู้อื่น
- **Simple First**: ใช้ของเดิมที่มีก่อน ไม่เพิ่ม abstraction, framework หรือ dependencies เพื่อรองรับอนาคตที่ยังไม่ต้องการจริง
- **Dead Code**: ลบโค้ดที่ไม่ได้ใช้ (Imports, variables, functions, components) ทันทีที่พิสูจน์ได้ว่าไม่มีผู้เรียกใช้ทั้ง repo
- **ห้ามสร้างไฟล์สำเนา**: ห้ามเก็บโค้ดเก่าในคอมเมนต์ และห้ามสร้างไฟล์ `.bak`, `-old`, `-copy` ค้างไว้
- **ขนาดไฟล์**: เกณฑ์ 300/500 บรรทัดเป็นสัญญาณเตือนให้ทบทวน Separation of Concerns ห้ามแตกไฟล์เพียงเพื่อหลบเกณฑ์บรรทัด
- **Git Safety**: ห้ามใช้คำสั่ง git ที่เปลี่ยน state (add, commit, reset, checkout, branch, merge, push) เว้นแต่ผู้ใช้สั่งชัดเจน
- **Release**: จะ build installer หรือตัดรอบเวอร์ชันก็ต่อเมื่อมีคำสั่งชัดเจนเท่านั้น หากไม่ระบุเวอร์ชัน ให้ใช้ `1.0.0`
