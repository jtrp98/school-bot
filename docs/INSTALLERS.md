# Desktop installers

แพ็กเกจสำหรับผู้ใช้มี 2 แบบ เฉพาะ Desktop App:

| ระบบ | ไฟล์ | วิธีใช้ |
|---|---|---|
| Windows 10/11 x64 | `school-bot-<version>-windows-amd64-setup.exe` | เปิดไฟล์ → ติดตั้ง School Bot → เสร็จสิ้น |
| macOS 15+ Intel / Apple Silicon | `school-bot-<version>-macos-universal.pkg` | เปิดไฟล์ → Continue → Install → เปิด School Bot จาก Applications |

เวอร์ชันในชื่อไฟล์อ่านจาก `app/frontend/package.json`; อยู่ใน `dist/installers/`
แต่ละแพ็กเกจมีทั้งสาม Mode, Frontend/Media, Demo Engine และ C++ Zide Engine ในตัว ผู้ใช้ติดตั้งครั้งเดียว ไม่ต้องติดตั้ง Go, Node, .NET หรือ Qt
C# API ไม่รวมใน Installer. เป้าหมาย online คือ Desktop→HTTPS Gateway→gRPC SchoolBot; ต้องให้ integration พร้อมก่อนฝัง public Gateway URL ที่ยืนยันแล้วตอน Build. ตัวอย่าง URL ด้านล่างเป็น placeholder ไม่ใช่หลักฐานว่า gRPC พร้อม
ค่า localhost คงใช้ทดลอง HTTP API บนเครื่องนักพัฒนา ดู `DEPLOYMENT.md`; engine/local hardware ไม่เปลี่ยน

## Windows

แอปเปิดแบบ fullscreen โดยไม่มีกรอบหรือแถบชื่อ Windows
ปุ่มย่อ ออก/เข้าเต็มจอ และปิดอยู่ในแอป ใช้ F11 สลับเต็มจอได้
ปุ่มหน้าต่างรวมอยู่ใน header เดิมของแต่ละหน้า โดยไม่มีแถวชื่อ/โลโก้แยกด้านบน
นอกโหมดเต็มจอลากบริเวณชื่อแอปเพื่อย้ายหน้าต่าง
EXE, Desktop/Start Menu Shortcut และรายการแอปที่ติดตั้งใช้รูปหุ่นจากโลโก้ School Bot
(`app/build/windows/appicon.ico`; สร้างใหม่ด้วย `scripts/create-windows-icon.ps1`)
ปิดเวอร์ชันเดิมก่อนติดตั้งตัวใหม่เพื่ออัปเกรด

Setup EXE ใช้หน้าต่าง WPF ที่วาดเอง ไม่มี native title bar ใช้มุมโค้ง โลโก้ มาสคอต
และฟอนต์ IBM Plex Sans Thai แบบไม่มีหัวเดียวกับแอป โดยฝังฟอนต์ไว้ให้ใช้ได้ออฟไลน์
กดติดตั้งคลิกเดียวลงตำแหน่งเริ่มต้น ไม่แสดงตัวเลือกโฟลเดอร์หรือส่วนประกอบ
มี animation มาสคอตและวงโคจรระหว่างติดตั้ง ปิดการเคลื่อนไหวตาม Windows ClientAreaAnimation
หน้าต่างย่อให้พอดีกับ work area ของจอ และมีปุ่มย่อ/ปิดในเนื้อหาโดยไม่มีแถบ header แยก
MSI ที่ใช้ติดตั้งจริงฝังอยู่ใน EXE พร้อม .NET Runtime จึงไม่ต้องติดตั้ง .NET เพิ่ม
MSI แยกยังอยู่ใน `dist/installers/` สำหรับ IT, silent deployment, repair และ uninstall
หน้าสุดท้ายมี checkbox “เปิด School Bot หลังติดตั้ง” ซึ่งเลือกไว้ให้และยกเลิกได้
แอปเปิดเมื่อกด “เสร็จสิ้น” หลังติดตั้งสำเร็จ โดยรันด้วยสิทธิ์ของผู้ใช้ปัจจุบัน
Repair, ถอนการติดตั้ง, silent/basic UI และการติดตั้งที่ยกเลิกหรือผิดพลาดไม่เปิดแอปอัตโนมัติ
ถ้าพบเวอร์ชันเดียวกันติดตั้งอยู่แล้ว Setup EXE แสดงว่าพร้อมใช้งานและไม่เรียกติดตั้งซ้ำ
ถ้า Windows แจ้งว่าต้อง restart จะไม่เปิดแอปหรือ restart เครื่องให้อัตโนมัติ
การเปิดแอปไม่ส่งคำสั่งเคลื่อนไหวไปยังหุ่นยนต์เอง

ความคืบหน้ามาจาก Windows Installer record callbacks ไม่ใช้ timer จำลองเปอร์เซ็นต์
การยกเลิกแจ้ง MSI แล้วรอ transaction/rollback จบ ไม่ kill process
ถ้าไฟล์กำลังใช้งาน Setup EXE หยุดพร้อมแจ้งให้ปิดแอปแล้วลองใหม่ โดยไม่ปิดแอปแทนผู้ใช้
แพ็กเกจที่ตรวจ SHA256 แล้วและ log เก็บใน `%LOCALAPPDATA%\SchoolBot\InstallerCache\<hash>\`
เพื่อให้ repair หา source ได้ภายหลัง; ไม่ลบ cache นี้ระหว่างจบการติดตั้ง
ตัวติดตั้งไม่เปิด HTTP server หรือ listener

```powershell
# Developer terminal ที่ repository root
# ตั้งค่านี้เมื่อมี Domain แล้ว (example.com เป็นเพียงตัวอย่าง)
# $env:SCHOOL_BOT_BUILD_API_URL = 'https://api.example.com'
./scripts/package-desktop.ps1

# ภาษา installer: ค่าเริ่มต้นเป็นไทย เปลี่ยนเป็น en หรือ zh ได้ (ไม่เปลี่ยนภาษาที่บันทึกไว้ในแอป)
./scripts/package-desktop.ps1 -Language en

# ถ้าแอปล่าสุด Build ไว้แล้ว ไม่เปลี่ยนโค้ด/URL
./scripts/package-desktop.ps1 -SkipBuild

# Build และแพ็กจากโฟลเดอร์แยก เมื่อ app/bin มีแอปที่กำลังเปิดอยู่
./scripts/package-desktop.ps1 -PayloadDirectory "$env:TEMP\school-bot-payload"

# Build อย่างเดียวในโฟลเดอร์แยก แล้วแพ็กโดยไม่ build ซ้ำ
./scripts/build-desktop.ps1 -OutputDirectory "$env:TEMP\school-bot-payload"
./scripts/package-desktop.ps1 -SkipBuild -PayloadDirectory "$env:TEMP\school-bot-payload"
```

`build-desktop.ps1` สร้าง `school-bot-<version>.exe` กับสำเนา `school-bot.exe` และฝังไอคอน/version resource จาก `scripts/windows-resources`
(สั่งด้วยมือตามขั้นตอนใน `README.md` จะไม่ผ่านขั้นตอนนี้) ทั้งสองคำสั่งรองรับ Windows เท่านั้นสำหรับตัวเลือกโฟลเดอร์แยก

ก่อนแพ็ก Script ตรวจว่า `school-bot.exe` ใน payload ตรงกับ `school-bot-<version>.exe` (hash) และ version resource ของ exe ตรงกับ `package.json`; ถ้าไม่ตรงจะหยุดแทนการส่ง exe เก่า

ก่อนแพ็กทั้ง Windows และ macOS `scripts/check-notices.mjs` เทียบ Go module (`go list -deps -tags production`), npm package ที่ฝังใน dist (จาก `package-lock.json`) และไฟล์ vendored ของ Zide กับ `THIRD-PARTY-NOTICES.md` และ `engines/zide/THIRD-PARTY-NOTICES.md` แล้วหยุดแพ็กเมื่อมี dependency ที่ส่งมอบแต่ไม่อยู่ใน notices เมื่อเพิ่มหรืออัปเดต dependency ที่ส่งมอบ ต้องเพิ่มแถวใน inventory และข้อความ license จากไฟล์ของ package ที่ pin ก่อน

Build machine ต้องมี Go/Node, .NET 10 SDK และ C++17 toolchain (Visual Studio C++ + Windows SDK/CMake หรือ Zig); Script เตรียม WiX 5.0.2/WixUI ที่ล็อกเวอร์ชันไว้ใน `.tools`
และตรวจ Microsoft Authenticode ของ WebView2 Bootstrapper ก่อนบรรจุใน MSI
การแพ็กครั้งแรกต้องต่ออินเทอร์เน็ต; `-SkipInstall` ข้าม `npm ci` หากมี dependencies อยู่แล้ว
`-SkipBuild` แพ็ก binary ที่มีอยู่ จึงไม่เปลี่ยน URL ที่ฝังอยู่ใน binary นั้น

MSI ลงแบบ per-user ใน `%LOCALAPPDATA%\Programs\School Bot` โดยค่าเริ่มต้น
สร้าง Start Menu/Desktop Shortcut และรายการ School Bot ใน Settings > Apps
ไฟล์หลักอยู่ในโฟลเดอร์แอป และ Zide อยู่ใต้ `engines/zide/` พร้อม metadata/licenses ผู้ใช้ไม่ต้องวางไฟล์เอง
หากมี WebView2 แล้วเปิดแอปได้เลย; ถ้ายังไม่มี ตอนเปิดแอปครั้งแรกจะขอให้ผู้ใช้ยืนยัน
แล้วเรียก Microsoft Bootstrapper ที่มากับแอปผ่านอินเทอร์เน็ต มีหน้าต่าง Microsoft Setup แสดงความคืบหน้า
หากติดตั้ง Runtime ไม่สำเร็จหรือผู้ใช้ยกเลิก ให้เปิดแอปใหม่เพื่อลองอีกครั้ง
การติดตั้ง WebView2 เกิดหลังจบ MSI transaction จึงไม่รัน Installer ซ้อนใน MSI custom action
การถอนการติดตั้งลบเฉพาะไฟล์แอป/Shortcut/registration โดยเก็บ Projects, Device ID และ WebView2 ไว้
ปิด School Bot ก่อนติดตั้งเวอร์ชันใหม่หรือถอนการติดตั้ง
MSI ใช้ standard FilesInUse dialog สำหรับไฟล์ที่เปิดอยู่ แทนการใช้ Restart Manager ปิดแอปให้อัตโนมัติ
ตาม [Microsoft MSIRESTARTMANAGERCONTROL](https://learn.microsoft.com/en-us/windows/win32/msi/msirestartmanagercontrol)

Installer ใช้ [WiX 5.0.2](https://github.com/wixtoolset/wix/releases/tag/v5.0.2) สำหรับ MSI
และ `app/installer/` สำหรับ WPF setup shell; ดู [Microsoft WPF windows](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/windows/)
และ [Windows Installer external UI](https://learn.microsoft.com/en-us/windows/win32/msi/monitoring-an-installation-using-msisetexternaluirecord)
MSI ที่เปิดโดยตรงยังใช้ native WixUI dialogs สำหรับความคืบหน้า Repair และถอนการติดตั้ง
และ Runtime deployment ตาม [Microsoft WebView2 documentation](https://learn.microsoft.com/en-us/microsoft-edge/webview2/concepts/distribution)

`scripts/prepare-installer-brand.ps1` สร้าง bitmap จากโลโก้และมาสคอตเดิมก่อนแพ็ก
และสร้าง WiX localization จากหมวด `installer` ใน catalogs `th.json`, `en.json`, `zh.json`
จึงใช้คำแปลแหล่งเดียวกับแอป โดยข้อความมาตรฐานของ WixUI ใช้ภาษาที่เลือกตอนแพ็ก
ไฟล์ `app/build/windows/installer/hero-robot.png` เป็นสำเนา PNG ของ `hero-robot.webp` เดิมสำหรับ GDI+/Windows Installer
`scripts/build-installer-shell.ps1` อ่านเวอร์ชันจาก manifest เดิม ฝัง MSI/hash/catalog/assets
และ publish EXE แบบ self-contained single-file โดยไม่มี source หรือ debug symbols
ข้อความ license ของ runtime/fonts ฝังใน EXE และแจกไฟล์ `.NOTICES.txt` ข้างตัวติดตั้ง

MSI เป็น Windows Installer database จริงพร้อม embedded cabinet รองรับ silent install, repair และ uninstall:

```powershell
msiexec /i "school-bot-<version>-windows-amd64.msi"
# Silent deployment สำหรับบัญชีผู้ใช้ปัจจุบัน
msiexec /i "school-bot-<version>-windows-amd64.msi" /qn /norestart
# Repair / Uninstall จาก MSI เดิม หรือใช้ ProductCode ที่ Windows Installer ลงทะเบียนไว้
msiexec /fa "school-bot-<version>-windows-amd64.msi" /qn /norestart
msiexec /x "school-bot-<version>-windows-amd64.msi" /qn /norestart
```

ใช้ UpgradeCode เดิมใน `Package.wxs` เพื่อรองรับ major upgrade และป้องกัน downgrade
การตรวจเวอร์ชันครอบคลุมทุกภาษา จึงอัปเกรดจาก installer ภาษาเดิมเป็นภาษาอื่นได้
เพิ่มเวอร์ชันก่อนแจก release ใหม่; เรียก Repair เพื่อลงไฟล์ที่หายหรือเสียหายของเวอร์ชันเดิม
Starter ไม่ใช้ default EULA ตัวอย่างของ WiX; เมื่อมี EULA ของผลิตภัณฑ์ที่อนุมัติแล้วจึงเพิ่มหน้า agreement

ตัวแพ็กตรวจโครงสร้าง MSI และเงื่อนไข checkbox อัตโนมัติก่อนเขียน checksum
รันการตรวจซ้ำโดยไม่ติดตั้งหรือเปิดแอปได้ด้วย:

```powershell
./scripts/tests/installer-package.test.ps1

# ตรวจ hash/font/progress/cancel result และเรนเดอร์ 7 สถานะ/2 ขนาด พร้อม 30 animation frames
# ไม่ติดตั้งจริง ไม่เปิดแอป และไม่มี listener; ภาพอยู่ใน dist/installer-preview/
$version = (Get-Content app/frontend/package.json -Raw | ConvertFrom-Json).version
./scripts/build-installer-shell.ps1 -InstallerPath "dist/installers/school-bot-$version-windows-amd64.msi" -BrandDirectory .tools/installer-brand/th -OutputDirectory .tools/installer-shell-preview -Preview
```

Preview/check entry points คอมไพล์เฉพาะเมื่อกำหนด `SETUP_PREVIEW`; ไม่อยู่ใน setup EXE ที่แจก
การเรนเดอร์และ unit checks ไม่ยืนยัน Windows Installer transaction จริง ต้องทดสอบ EXE บน clean VM เพิ่ม

Smoke test สำหรับ clean Windows account/CI ให้เปิด PowerShell แบบ **Run as administrator**
(สคริปต์จะหยุดก่อนติดตั้งหากไม่ได้ยกระดับสิทธิ์ และไม่รันทับ School Bot ที่ติดตั้งอยู่แล้ว):

```powershell
./scripts/test-installer.ps1
# ถ้าแพ็กจากโฟลเดอร์แยก ให้ชี้ payload เดียวกัน
./scripts/test-installer.ps1 -PayloadDirectory "$env:TEMP\school-bot-payload"
```

ทดสอบ Silent Install ลง path ที่มีช่องว่างใน workspace, checksum ของ payload, Shortcut,
Windows Installer registration, ลบ Engine ใน test installation แล้ว Repair ให้กลับมา,
เรียก Engine ที่ติดตั้งแล้ว และ Silent Uninstall พร้อมตรวจว่าไฟล์อื่น/Projects ไม่ถูกลบ
การทดสอบนี้ไม่เปิด GUI แอป; กรณีเปิดครั้งแรกบนเครื่องไม่มี Runtime ต้องลองบน clean VM เพิ่ม

## macOS

สคริปต์ใช้ Go (ตาม `app/go.mod`), Node.js 24 LTS และ Xcode Command Line Tools บน Mac
Wails v3 รองรับ Docker cross-compilation แต่ repository นี้ยังไม่ได้ตั้งค่าเส้นทางนั้น
การสร้าง `.pkg` ด้านล่างใช้ Apple `productbuild` บน macOS

```bash
# ต้องรันบน Mac ที่มี Go, Node และ Xcode Command Line Tools
# SCHOOL_BOT_BUILD_API_URL=https://api.example.com bash scripts/package-desktop.sh
bash scripts/package-desktop.sh
```

Script Build สอง architecture ด้วย Go/Clang แล้วใช้ `lipo` รวมทั้ง main executable และ Engine
เป็น Universal binary ใน `app/bin/darwin-universal/School Bot.app`
จากนั้นใช้ Apple `productbuild` ทำ `.pkg` ติดตั้งลง `/Applications` และจำกัด macOS 15 ขึ้นไป
ไม่เพิ่ม daemon, service หรือ post-install script
เมื่อมี Universal bundle ล่าสุดแล้วใช้ `bash scripts/package-desktop.sh --skip-build` ได้
`.pkg` รองรับ Intel และ Apple Silicon ในไฟล์เดียว ไม่ต้องให้ผู้ใช้เลือก architecture
ถอนแอปโดยย้าย `/Applications/School Bot.app` ไป Trash; Projects ใน Application Support ยังคงอยู่

อ้างอิง [Apple: Packaging Mac software for distribution](https://developer.apple.com/documentation/xcode/packaging-mac-software-for-distribution)

ก่อนเผยแพร่ ให้ลงนาม helper, executable และ bundle ด้วย Developer ID และ hardened runtime
จากนั้น notarize/staple และตรวจ artifact สุดท้ายบน Mac จริงก่อนคำนวณ checksum สำหรับแจก
Ad-hoc signing ใช้พัฒนาในเครื่อง ไม่ใช่ Developer ID และไม่ทำให้ผ่าน notarization
Projects/Device ID อยู่ใน `~/Library/Application Support/SchoolBotPoC` แยกจากตัวแอป

## Development build และผลตรวจ

Windows setup EXE และ MSI ยังไม่ลงนาม ส่วน Mac App ใช้ ad-hoc signing และ `.pkg` ยังไม่ลงนาม
ก่อนแจกภายนอก ต้อง Windows code signing และ Mac Developer ID Application/Installer signing + notarization
ไฟล์ `.sha256` ใช้ตรวจ checksum ไม่ใช่ลายเซ็นรับรองผู้เผยแพร่
ยังต้องทดสอบการติดตั้ง/อัปเกรดเวอร์ชันล่าสุด, การเปิดครั้งแรกบนเครื่องที่ไม่มี WebView2,
macOS build/ติดตั้งและ UI บนเครื่องจริง รวมถึง Robot Control กับหุ่นจริง
macOS hardware transport ยังไม่รองรับ; ดู `../engines/zide/docs/INTEGRATION.md`
CI เก็บ Windows setup EXE/MSI/notices และ Mac Universal PKG เป็น artifacts แยก 2 ชุดหลังตรวจผ่าน
