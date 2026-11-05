# SchoolBot v1.0 Release Notes

**Release Version:** v1.0.0 (Build 0.1.5-rc)  
**Release Date:** 2026-11-05  
**Target Platform:** Windows 10/11 x64, macOS 12+ (Universal), Kubernetes (API/Worker)  
**Handover Status:** Final Acceptance Approved (Phase 7 Complete)

---

## 1. ข้อมูลภาพรวมการปล่อยระบบ (Release Overview)

SchoolBot v1.0 คือระบบการเรียนรู้การเขียนโปรแกรมและควบคุมหุ่นยนต์แขนกลอัจฉริยะสำหรับนักเรียนและครูในโรงเรียน เชื่อมต่อเข้ากับระบบการศึกษา SchoolBright อย่างสมบูรณ์ เพื่อยกระดับการเรียนการสอนวิทยาการคำนวณและหุ่นยนต์ (Robotics & Coding Education)

---

## 2. ฟีเจอร์หลักในเวอร์ชัน v1.0 (Key Features)

### 2.1 สตูดิโอการเรียนรู้ 3 โหมด (Multi-Mode Studio)
- **Block Lab (การเขียนโปรแกรมแบบบล็อก):**
  - ใช้ Blockly ล่าสุด รองรับการลากวางบล็อกคำสั่งควบคุมตัวละครและการเคลื่อนที่
  - ระบบตรวจสอบโครงสร้างการวนซ้ำ (Bounded Loop Validation) ป้องกันลูปไม่รู้จบและ Stack Overflow
- **Arm Lab (ห้องทดลองควบคุมแขนกลหุ่นยนต์):**
  - จำลอง 3D Kinematics บนหน้าจอ พร้อม Slider ควบคุมมุมเซอร์โว 6 แกนอิสระ
  - รองรับการเชื่อมต่อแขนกลหุ่นยนต์จริงผ่าน USB (HID และ Serial CDC)
  - กลไกความปลอดภัยระดับฮาร์ดแวร์: ตรวจสอบความเร็ว, ช่วงขีดจำกัดมุมการหมุน, และระบบตัดแรงบิดฉุกเฉินเมื่อสายหลุด (Disconnect Failsafe)
- **Code Lab (การเขียนโค้ดภาษา Python):**
  - โหมดผจญภัยเขียนโค้ดด้วยภาษา Python ในสภาพแวดล้อมที่จำกัดสิทธิ์ความปลอดภัย (Bounded DSL)

### 2.2 ระบบการสอบและการประเมินผล (Examination & Assessment)
- **ระบบทำแบบทดสอบแบบจับเวลา (Timed Attempts):**
  - จับเวลาอัตโนมัติ พร้อมกลไกส่งข้อสอบทันทีเมื่อหมดเวลา (Auto-finalization on Timeout)
  - สุ่มลำดับคำถามและตัวเลือกจากคลังข้อสอบ (Randomized Question Pool)
  - บันทึก Snapshot ของข้อสอบขณะเริ่มทำ เพื่อให้ผลการสอบคงที่แม้มีการปรับปรุงคลังข้อสอบในภายหลัง
- **ระบบตรวจข้อสอบและคำนวณคะแนนอัตโนมัติ (Auto-Grading):**
  - ตรวจและให้คะแนนทันทีเมื่อส่งข้อสอบ
  - ครูสามารถอนุมัติให้สอบซ่อมรายบุคคล (Retake Grant) หรือปรับคะแนนการประเมินแขนกลได้

### 2.3 เวิร์กโฟลว์ของครูและผู้บริหารโรงเรียน (Teacher & School Admin Workflows)
- **การจัดการบทเรียนและเกณฑ์คะแนน (Curriculum & Scoring Policies):**
  - กำหนดเกณฑ์ผ่านบทเรียน สูตรการรวมคะแนน และการเปิด-ปิดบทเรียนตามตารางสอน
  - นำเข้าแผนการสอนและคลังข้อสอบจาก Google Sheet / Excel ได้อย่างรวดเร็ว
- **การเลื่อนระดับชั้นเรียน (Classroom Level Promotion):**
  - ครูเสนอรายชื่อนักเรียนที่ผ่านเกณฑ์เพื่อเลื่อน Level ทั้งห้องเรียน
- **การส่งออกคะแนนเข้าสู่ระบบทะเบียนเดิม (Guarded Grade Export):**
  - ระบบจับคู่ช่องคะแนน SchoolBot กับช่องคะแนนเดิมของโรงเรียน (GradeSlot Mapping)
  - แสดงภาพตัวอย่างคะแนนก่อนส่งออกจริง (Score Preview Snapshot)
  - ตรวจจับคะแนนชนกัน (Conflict Detection) พร้อมกลไก Compare-And-Swap (CAS) ป้องกันการเขียนทับคะแนนที่ครูเคยกรอกไว้

### 2.4 สถาปัตยกรรมทำงานแบบออฟไลน์ (Offline-First Architecture)
- ผู้เรียนสามารถสร้าง บันทึก และเปิดโครงงานได้ต่อเนื่องแม้ไม่มีสัญญาณอินเทอร์เน็ต
- โครงงานออฟไลน์ถูกบันทึกอย่างปลอดภัยในเครื่อง พร้อมระบบกู้คืนโครงงานและถังขยะ (Guest Trash Recovery)
- ระบบจะทำการซิงก์ข้อมูลขึ้นคลาวด์อัตโนมัติเมื่ออุปกรณ์เชื่อมต่ออินเทอร์เน็ต

### 2.5 การรองรับหลายภาษา (Localization / i18n)
- รองรับภาษาไทย (th), ภาษาอังกฤษ (en), และภาษาจีน (zh) ครบถ้วนทุกหน้าจอ

---

## 3. ความปลอดภัยและการปฏิบัติตามมาตรฐาน (Security & Compliance)

1. **สถาปัตยกรรมแยกส่วน (Process Isolation):**
   - การเข้าถึงฐานข้อมูลคะแนนเดิม (`PGGradeDB`) ถูกแยกสิทธิ์เฉพาะ Pod `school-bot-grade-worker` ไม่เปิดเผยให้ API ทั่วไป
2. **การเข้ารหัสและยืนยันตัวตนเครื่อง (Cryptographic Device Binding):**
   - ผูกสิทธิ์การใช้งานแอปพลิเคชันเข้ากับ Public Key Thumbprint (JKT) ของเครื่อง ป้องกันการคัดลอก License Key ไปใช้ในเครื่องที่ไม่ได้รับอนุญาต
3. **ระบบอัปเดตที่ปลอดภัย (Secure Atomic Updater):**
   - ตัวอัปเดตในเครื่องตรวจสอบความถูกต้องของลายเซ็นใบรับรองดิจิทัล (Publisher Certificate Pinning)
   - สลับไฟล์ไบนารีแบบ Atomic หากมีข้อผิดพลาดจะ Rollback คืนเวอร์ชันเดิมทันที
4. **ความสะอาดของชุดเผยแพร่ (Clean Packaging):**
   - ตัวติดตั้งจริงผ่านการตรวจสอบ ปราศจากไฟล์ทดสอบ, Source Map, PDB หรือ Private Keys ใดๆ

---

## 4. รายการไฟล์ชุดเผยแพร่และค่าตรวจสอบ (Release Artifacts & Hashes)

| รายการไฟล์ | พาธ / แหล่งที่อยู่ | SHA-256 Checksum |
|---|---|---|
| **Windows MSI Installer** | `dist/installers/school-bot-0.1.5-windows-amd64.msi` | `d5822a4f8d17896678e8903bfdce79c47033bca659427613cbec688295451133` |
| **Windows Setup Bootstrapper** | `dist/installers/school-bot-0.1.5-windows-amd64-setup.exe` | `c6e7e01da22d004dd83e06dffac715d5d4e70d167e628fe45621bad7f15789d4` |
| **SchoolBot Host Executable** | `dist/payload/school-bot.exe` | `e486102d29201ce12b7d053cbd50eec94bacd50e5841b9dd8dc818ce5614dcb7` |
| **Demo DSL Engine** | `dist/payload/demo-engine.exe` | `f084d19084f8939a3749368d90e184888a76ea1a4d03d43d8178acc18af016ad` |
| **Zide C++ Robotics Engine** | `dist/payload/engines/zide/zide-engine.exe` | `3c4ab9ceb482c3d50310c653edf22c5133189e63b996082ece9710f4e593ed24` |
| **WebView2 Bootstrapper** | `dist/installers/MicrosoftEdgeWebview2Setup.exe` | `baa78c71c5615533a1ee0c4d2ca2ea3fd24dae01965f9033ac530a61b5aa93a0` |

---

## 5. แผนการนำขึ้นระบบจริงและลำดับขั้นตอน (Deployment Runbook)

ลำดับขั้นตอนการ Rollout ตามมาตรฐาน `plan/release-runbook-001.md`:
1. **ฐานข้อมูล (Database):** รันสคริปต์ `schoolbot_db.sql` บน PostgreSQL 15.19 ฐานข้อมูล `SchoolBotDB` แยกขาดจากฐานเดิม
2. **ระบบบริการหลังบ้าน (Backend):** ติดตั้ง `ServiceSchoolBot.API` และ `school-bot-grade-worker` บน Kubernetes Cluster
3. **เกตเวย์ (API Gateway):** เปิดเส้นทาง HTTP to gRPC บน `sb-api-gateway`
4. **เว็บสำหรับบุคลากร (Web Admin & Teacher):** อัปเดตเมนู SchoolBot บนระบบ SchoolBright Web
5. **แอปพลิเคชันผู้เรียน (Desktop Clients):** แจกจ่ายตัวติดตั้ง `school-bot-0.1.5-windows-amd64.msi` ให้กับห้องคอมพิวเตอร์โรงเรียน
