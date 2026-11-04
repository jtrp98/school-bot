# สคริปต์ของ School Bot

รันจากราก repository คำสั่ง PowerShell ใช้บน Windows ส่วน `.sh` ใช้บน macOS ตามที่แต่ละสคริปต์รองรับ

| งาน | คำสั่ง |
|---|---|
| เปิด API | `./scripts/api/run.ps1` หรือ `bash scripts/api/run.sh` |
| สร้าง EF models จากฐานข้อมูล | `./scripts/api/scaffold-databases.ps1 -Database Master` — อ่าน metadata แบบ read-only; ตั้งค่าตาม [คู่มือ API](../web/README.md#ef-core-database-first-scaffold) |
| Build Desktop | `./scripts/desktop/build.ps1` หรือ `bash scripts/desktop/build.sh` |
| เปิด Desktop | `./scripts/desktop/run.ps1 -SkipBuild` หรือ `bash scripts/desktop/run.sh --skip-build` |
| Package Desktop และ installer | `./scripts/desktop/package.ps1` หรือ `bash scripts/desktop/package.sh` |
| Build/test engine | `./scripts/engines/build.ps1 -Engine zide -Test` หรือ `bash scripts/engines/build.sh zide --test` |
| ตรวจ third-party notices | `node scripts/checks/third-party-notices.mjs` |
| ตรวจทั้งระบบ | `./scripts/verify.ps1` — build แอปและตรวจ frontend, Go, engine, API |
| ส่ง source ให้ทีม | `./scripts/package-source.ps1` |

`installer/` เก็บขั้นเตรียม tools/branding และ build ตัวติดตั้งที่ `desktop/package.ps1` เรียกใช้
`installer/test-installation.ps1` ติดตั้ง ซ่อม และถอน MSI จริง ใช้ใน CI หรือเครื่องทดสอบที่อนุญาตให้ติดตั้งเท่านั้น

`assets/` เก็บตัวสร้างไอคอนและ Windows version resources; `lib/` เป็น helper กลาง; `tests/` เก็บการตรวจสคริปต์

การตรวจที่ไม่ติดตั้งหรือเชื่อมฐานข้อมูลจริง:

```powershell
./scripts/tests/source-package.test.ps1
./scripts/tests/scaffold-databases.test.ps1
./scripts/tests/installer-package.test.ps1 -InstallerPath ./dist/installers/<package>.msi
```
