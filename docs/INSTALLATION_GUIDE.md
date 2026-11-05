# SchoolBot v1.0 — คู่มือการติดตั้งระบบ (Installation Guide)

> เอกสารชุดส่งมอบโครงการ (Handover Package) · SchoolBot Target: `target/school-bot/` · Version: v1.0.0 (Build 0.1.5)  
> วันที่ตรวจรับ: 2026-11-05 · สถานะ: ผ่านการตรวจรับความพร้อม (Verified & Release Ready)

---

## 1. ข้อมูลภาพรวมและข้อกำหนดของระบบ (System Requirements)

SchoolBot รองรับการใช้งาน 2 ส่วนหลัก:
1. **Desktop Client (แอปพลิเคชันฝั่งเครื่องผู้เรียนและห้องปฏิบัติการ)**: รันบนระบบปฏิบัติการ Windows และ macOS
2. **Backend API Services & Worker (บริการระบบฝั่งเซิร์ฟเวอร์)**: รันบนระบบคลัสเตอร์ Kubernetes / Docker Container

### 1.1 ความต้องการระบบขั้นต่ำสำหรับ Desktop Client

| รายการ | ฝั่ง Windows | ฝั่ง macOS |
|---|---|---|
| **ระบบปฏิบัติการ** | Windows 10 (64-bit) Build 19045+ หรือ Windows 11 (22H2 / 24H2 / 25H2) | macOS 12 Monterey, macOS 13 Ventura, macOS 14 Sonoma, macOS 15+ Sequoia |
| **สถาปัตยกรรม CPU** | x86_64 / amd64 | Universal Binary (Intel x86_64 และ Apple Silicon arm64) |
| **หน่วยความจำ (RAM)** | ขั้นต่ำ 4 GB (แนะนำ 8 GB) | ขั้นต่ำ 4 GB (แนะนำ 8 GB) |
| **พื้นที่ว่างในฮาร์ดดิสก์** | 500 MB สำหรับติดตั้งแอป และ 1 GB สำหรับแคชและโครงงาน | 500 MB สำหรับแอปพลิเคชัน และ 1 GB สำหรับโครงงาน |
| **คอมโพเนนต์จำเป็น** | Microsoft Edge WebView2 Runtime (Evergreen Bootstrapper) | WebKit (ติดตั้งมาพร้อมกับระบบปฏิบัติการ macOS แล้ว) |
| **การเชื่อมต่อฮาร์ดแวร์** | พอร์ต USB 2.0 / 3.0 สำหรับเชื่อมต่อแขนกลหุ่นยนต์ (HID หรือ Serial CDC) | พอร์ต USB / Type-C พร้อม Adapter สำหรับเชื่อมต่อแขนกลหุ่นยนต์ |

---

## 2. ขั้นตอนการติดตั้ง Desktop Client บน Windows

### 2.1 ไฟล์ตัวติดตั้งที่จัดเตรียมไว้ในชุด Release Package (`dist/installers/`)
- `school-bot-0.1.5-windows-amd64.msi`: Windows Installer มาตรฐาน (รองรับ Per-User Installation, Silent Install, MajorUpgrade)
- `school-bot-0.1.5-windows-amd64-setup.exe`: ตัวติดตั้งพร้อม Bootstrapper ที่ตรวจสอบและดาวน์โหลด WebView2 Runtime อัตโนมัติ

### 2.2 การติดตั้งแบบ Interactive (ผู้ใช้งานทั่วไป)
1. ดับเบิลคลิกที่ไฟล์ `school-bot-0.1.5-windows-amd64-setup.exe` หรือ `school-bot-0.1.5-windows-amd64.msi`
2. ระบบจะทำการตรวจสอบการมีอยู่ของ WebView2 Runtime หากยังไม่มี ระบบ Setup Bootstrapper จะทำการดาวน์โหลดและติดตั้งให้อัตโนมัติ
3. หน้าจอ Wizard จะแสดงขั้นตอนการติดตั้ง โดยค่าเริ่มต้นจะติดตั้งลงในไดเรกทอรีของผู้ใช้:
   `%LOCALAPPDATA%\Programs\School Bot\`
4. เมื่อการติดตั้งเสร็จสิ้น ไอคอนของ SchoolBot จะปรากฏบน Desktop และใน Start Menu
5. ผู้เรียนหรือครูสามารถเปิดโปรแกรม SchoolBot เข้าสู่ระบบหรือใช้งานโหมดออฟไลน์ได้ทันที

### 2.3 การติดตั้งแบบอัตโนมัติในห้องปฏิบัติการคอมพิวเตอร์ (Silent / Unattended Install)
สำหรับผู้ดูแลระบบเครือข่ายของโรงเรียน สามารถติดตั้ง SchoolBot บนเครื่องคอมพิวเตอร์จำนวนมากผ่านคำสั่ง PowerShell หรือ Active Directory Group Policy (GPO):

```powershell
# ติดตั้งแบบเงียบ (Silent Installation) ไม่แสดง UI
msiexec.exe /i "school-bot-0.1.5-windows-amd64.msi" /qn /norestart

# ติดตั้งพร้อมบันทึก Log การทำงาน
msiexec.exe /i "school-bot-0.1.5-windows-amd64.msi" /qn /L*v "install-schoolbot.log"
```

### 2.4 กลไกการอัปเกรดและย้อนกลับเวอร์ชัน (Upgrade & Rollback)
- **การอัปเกรด (MajorUpgrade)**: ระบบตั้งเวลาตารางติดตั้งแบบ `Schedule="afterInstallExecute"` เพื่อความปลอดภัย หากการอัปเกรดเกิดข้อผิดพลาดระหว่างทาง Windows Installer จะทำการ Rollback คืนสถานะเวอร์ชันเดิมให้ทันทีโดยไม่ทำให้โปรแกรมเสียหาย
- **การป้องกัน Downgrade**: ป้องกันการลงทับด้วยเวอร์ชันที่เก่ากว่า (จะคืน Exit Code 1603 เพื่อความปลอดภัยของข้อมูล)
- **การซ่อมแซมแอปพลิเคชัน (Self-Repair)**:
  ```powershell
  msiexec.exe /fa "school-bot-0.1.5-windows-amd64.msi" /qn
  ```
- **การถอนการติดตั้ง (Uninstall)**:
  ```powershell
  msiexec.exe /x "school-bot-0.1.5-windows-amd64.msi" /qn
  ```
  *หมายเหตุ:* ข้อมูลโครงงานและประวัติของผู้เรียนใน `%APPDATA%\SchoolBotPoC` จะยังคงถูกรักษาไว้ ไม่ถูกลบขณะถอนการติดตั้ง

---

## 3. ขั้นตอนการติดตั้ง Desktop Client บน macOS

### 3.1 ไฟล์ตัวติดตั้ง
- `school-bot-0.1.5-universal.pkg`: macOS Installer Package สำหรับทั้ง Intel (x86_64) และ Apple Silicon (M1/M2/M3/M4)
- `School Bot.app`: แอปพลิเคชันบันเดิล

### 3.2 การติดตั้ง
1. ดับเบิลคลิกไฟล์ `.pkg` และปฏิบัติตามคำแนะนำบนหน้าจอ
2. ตัวติดตั้งจะนำแอปพลิเคชันไปไว้ที่ `/Applications/School Bot.app`
3. ในกรณีที่องค์กรจัดการเครื่องผ่าน MDM (Jamf, Kandji, Mosyle) สามารถส่งไฟล์ `.pkg` ไปติดตั้งแบบ Silent ได้ทันที:
   ```bash
   sudo installer -pkg "school-bot-0.1.5-universal.pkg" -target /
   ```

---

## 4. ขั้นตอนการติดตั้ง Backend API & Worker บน Kubernetes

ระบบเซิร์ฟเวอร์ SchoolBot ประกอบด้วย 2 deployment หลัก:
1. `service-school-bot`: รัน `ServiceSchoolBot.API` ให้บริการ gRPC ผ่านพอร์ต 8080 (h2c) และ health probe ผ่านพอร์ต 8081 (`/healthz`)
2. `school-bot-grade-worker`: รัน Background Worker ประมวลผลและส่งออกเกรด/คะแนนไปยังระบบเดิมอย่างปลอดภัย

### 4.1 ข้อกำหนดและ Pre-requisites
- ฐานข้อมูล PostgreSQL 15.19+ (สร้างตารางตามสคริปต์ `build/db/schoolbot_db.sql`)
- Kubernetes Cluster 1.28+ หรือ K3s
- การเชื่อมต่อเครือข่ายภายในระหว่าง `sb-api-gateway` และ `service-school-bot`

### 4.2 การตั้งค่า Secret Configuration
สร้าง Secret ใน Kubernetes สำหรับเก็บ Connection Strings (อ้างอิงตาม `deploy/base/school-bot-grpc.yaml`):

```bash
kubectl create secret generic school-bot-db-secrets \
  --from-literal=SCHOOL_BOT_DB_CONNECTION_STRING="Host=pg-host;Port=5432;Database=SchoolBotDB;Username=sb_app;Password=secret" \
  --from-literal=MASTER_DB_CONNECTION_STRING="Host=pg-host;Port=5432;Database=JabjaiMasterSingleDB;Username=sb_ro;Password=secret" \
  --from-literal=SCHOOL_DB_CONNECTION_STRING="Host=pg-host;Port=5432;Database=JabjaiSchoolSingleDB;Username=sb_ro;Password=secret" \
  --from-literal=LOGS_DB_CONNECTION_STRING="Host=pg-host;Port=5432;Database=JabjaiSchoolLogs;Username=sb_log;Password=secret" \
  --from-literal=PGGRADE_DB_CONNECTION_STRING="Host=pg-host;Port=5432;Database=PGGradeDB;Username=sb_grade;Password=secret"
```

> [!IMPORTANT]
> `PGGRADE_DB_CONNECTION_STRING` ถูกจำกัดให้เข้าถึงได้เฉพาะ Pod `school-bot-grade-worker` เท่านั้น โดย `service-school-bot` ทั่วไปจะไม่ได้รับ Environment Variable นี้ เพื่อเป็นไปตามหลักการ Least Privilege (Process Isolation SEC-001-002)

### 4.3 การตรวจสอบความพร้อมก่อน Deploy (Preflight Check)
ก่อนดำเนินการ Deploy บน Kubernetes ให้รันสคริปต์ตรวจสอบความถูกต้องของ manifest และการแยกสิทธิ์:

```powershell
& "scripts/api/deploy-preflight.ps1" -ManifestPath "deploy/base/school-bot-grpc.yaml"
```

### 4.4 การติดตั้งผ่าน Kustomize
```bash
# สำหรับ Development Environment
kubectl apply -k deploy/overlays/dev

# สำหรับ Production Environment
kubectl apply -k deploy/overlays/prod
```

### 4.5 การตรวจสอบสถานะการทำงาน (Health Check)
```bash
# ตรวจสอบสถานะ Pod
kubectl get pods -l app=school-bot

# ตรวจสอบ Health Probe
kubectl exec -it deploy/service-school-bot -- curl -s http://localhost:8081/healthz
# ผลลัพธ์ที่ถูกต้อง: {"status":"ok","service":"school-bot-api","poc":true,"storage":"postgresql"}
```

### 4.6 การสำรองและกู้คืนฐานข้อมูล (Backup & Restore)
รันสคริปต์จัดการสำรองข้อมูลเฉพาะฐานข้อมูล SchoolBot:

```powershell
# สำรองข้อมูล
& "scripts/api/backup-restore.ps1" -Action Backup -BackupPath "backups/schoolbot-v1.0.dump"

# ตรวจสอบไฟล์สำรอง
& "scripts/api/backup-restore.ps1" -Action Validate -BackupPath "backups/schoolbot-v1.0.dump"

# กู้คืนข้อมูล (เมื่อจำเป็น)
& "scripts/api/backup-restore.ps1" -Action Restore -BackupPath "backups/schoolbot-v1.0.dump"
```
