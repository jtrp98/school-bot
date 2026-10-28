# School Bot · Team PoC

Starter สำหรับแบ่งทีมพัฒนา 3 Mode ไม่ใช่ Production Release

- `app/` — **Go + Wails v3 + React + TypeScript**, Desktop ทั้งหมด
- `web/` — **C# / ASP.NET Core .NET 10**, API แบบ **Clean Architecture**
- `contracts/` — Engine JSON contract ที่ทุกทีมใช้ร่วมกัน
- `engines/` — Engine แยกตาม ID แต่ละตัวมี source, dependencies, tests และ build ของตัวเอง; ดู `engines/README.md`
- `docs/` — Architecture, API, การติดตั้ง/Deploy และการปกป้องโค้ด

Engine `mixly` อยู่ใน `engines/mixly/` และ build แยกด้วย
`./scripts/build-engine.ps1 -Engine mixly -Test` รองรับตัวสร้างโค้ดจากต้นฉบับ Mixly;
ยังไม่เชื่อม UI, host routing หรือรวมใน Desktop/Installer ดูขอบเขตที่ `engines/mixly/README.md`

## Quick start (Windows / PowerShell)

ติดตั้ง Go (เวอร์ชันขั้นต่ำตามบรรทัด `go` ใน `app/go.mod`), Node.js **24 LTS**, .NET **10 SDK** และ WebView2 Runtime ก่อน
Wails Go module และ JS runtime ล็อกไว้ที่ **v3.0.0-beta.30** ซึ่งยังเป็น Beta
สคริปต์ใช้ portable Go ใน `.tools/go` ถ้ามี ไม่เช่นนั้นใช้ `go` จาก PATH (สคริปต์ไม่ตรวจเวอร์ชัน Go);
โฟลเดอร์ `.tools/` ไม่ใช่ส่วนที่ต้องส่งให้ทีม

```powershell
# รันจาก repository root: ติดตั้ง dependencies และ build Desktop
./scripts/build-desktop.ps1

# เปิด Desktop ที่ build แล้ว (ไม่ต้องเปิด API ก็ใช้สาม Mode ได้)
./scripts/start-desktop.ps1 -SkipBuild

# อีก terminal: เปิด C# API ที่ http://127.0.0.1:8080
./scripts/start-api.ps1

# ตรวจ frontend, i18n, Go, JSON IPC (smoke + ipc.test.mjs), Zide C++ tests, C# integration และ architecture tests
./scripts/verify.ps1 -SkipInstall

# ถ้า app/bin มีแอปที่กำลังเปิดอยู่ ให้ build และตรวจในโฟลเดอร์แยก (Windows)
./scripts/verify.ps1 -SkipInstall -OutputDirectory "$env:TEMP\school-bot-verify"

# Build อย่างเดียวในโฟลเดอร์แยก (ไม่แตะ app/bin ที่เปิดอยู่)
./scripts/build-desktop.ps1 -SkipInstall -OutputDirectory "$env:TEMP\school-bot-build"
```

Build ได้ `school-bot-<version>.exe` (เวอร์ชันจาก `app/frontend/package.json`) และสำเนา `school-bot.exe` ที่ installer/start script ใช้;
Windows build ฝังไอคอน `app/build/windows/appicon.ico` และ version resource ผ่าน `scripts/windows-resources` แล้วตรวจ `-verify` ให้อัตโนมัติ; สร้างไอคอนใหม่จากโลโก้ด้วย `scripts/create-windows-icon.ps1`

ถ้าเครื่องมีนโยบายห้ามรัน PowerShell scripts ให้ใช้คำสั่งด้านล่าง หรือให้ IT ใช้นโยบายที่อนุมัติแล้ว ไม่จำเป็นต้องปิดระบบรักษาความปลอดภัย
คำสั่งนี้เป็นขั้นต่ำเพื่อลองรัน ไม่ได้ฝังไอคอน/version resource และไม่ได้ตั้ง `-X main.appVersion` ใช้ `scripts/build-desktop.ps1` เมื่อจะส่งมอบ

```powershell
cd app/frontend
npm ci
npm run build
cd ..
../scripts/build-engine.ps1 -Engine zide -OutputDirectory ./bin/engines/zide
go build -trimpath -ldflags '-s -w' -o bin/demo-engine.exe ./cmd/demo-engine
go build -trimpath -tags production -ldflags '-s -w -H windowsgui' -o bin/school-bot.exe .
./bin/school-bot.exe

# อีก terminal
cd web
dotnet run --project src/SchoolBot.Api --no-launch-profile
```

## macOS (Intel / Apple Silicon)

รองรับการ Build เป็น **School Bot.app** สำหรับเครื่อง Mac ที่ใช้ macOS **15 ขึ้นไป**
ติดตั้ง Go (ตาม `app/go.mod`), Node.js 24 LTS และ Xcode Command Line Tools (`xcode-select --install`)
ติดตั้ง .NET 10 SDK เพิ่มเฉพาะเมื่อจะรัน/พัฒนา C# API หรือชุดทดสอบ API บนเครื่องนั้น

```bash
# รันจาก repository root บน Mac ไม่ต้องติดตั้ง PowerShell
bash scripts/build-desktop.sh
bash scripts/start-desktop.sh --skip-build

# อีก Terminal หากต้องการเปิด C# API
bash scripts/start-api.sh
```

- Apple Silicon → `app/bin/darwin-arm64/School Bot.app`
- Intel → `app/bin/darwin-amd64/School Bot.app`
- ZIP ของแต่ละ architecture อยู่ข้าง `.app` และรวม Demo Engine กับ C++ Zide Engine แล้ว
- Build บน Mac ตาม architecture ของ Go SDK ในเครื่อง; CI แยก Intel และ ARM64
- โปรแกรมหลักและ Engine อยู่ใน `Contents/MacOS` จึงไม่พึ่ง current working directory ตอนเปิดผ่าน Finder
- ใช้ WKWebView ของ macOS ไม่ต้องติดตั้ง WebView2
- Build Script ใช้ ad-hoc signing สำหรับพัฒนาในเครื่อง **ยังไม่ใช่ Developer ID / notarized release**
  ก่อนแจกนอกทีมต้องทำ signing/notarization ตามคู่มือ `docs/INSTALLERS.md`
- PowerShell 7 บน Mac ยังใช้ `scripts/build-desktop.ps1` และ `scripts/verify.ps1` ได้

## สิ่งที่ทดลองได้

| ส่วน | ทำแล้วใน starter | ยังไม่ใช่ขอบเขตนี้ |
|---|---|---|
| Robot Control | Native Arm Lab ผ่าน C++ Zide: ค้นหา HID/Serial บน Windows, โปรไฟล์ 6 ข้อ, ส่งท่า/เล่นลำดับ/อัปโหลด/อ่าน servo/หยุด, Save/Load; preview แยกจำลอง | Hardware acceptance, macOS USB, firmware upgrade, calibrated home/fold presets |
| Coding Adventure | Phaser scene, movement/jump playback, textarea editor, bounded DSL, Save/Load | Python เต็ม, levels, grading, language sandbox |
| Block Programming | Blockly, move/jump/repeat, bounded compilation, JSON engine trace, Save/Load | Variables, functions, conditions, full Scratch runtime |
| Engine Manager | Native child process, JSON Lines, request matching, 5-second timeout, bounded output, cancellation | Dynamic engine installer, OS sandbox, process-tree containment |
| API | License create/list/activate/validate/revoke, device limits, release draft/publish/check | Database, admin website, user accounts, signing pipeline |
| Updates | ตรวจ metadata จาก API ผ่าน Host | Download, signature verification, installation, rollback |

Robot Control แบบ Native ส่งคำสั่งไป Hardware ได้เมื่อเลือกอุปกรณ์และยืนยันโปรไฟล์ ส่วน Browser Preview เป็นจำลอง **ยังต้องทดสอบหุ่นจริงก่อนใช้งานกับนักเรียน**; DSL ไม่ใช่ Python และไม่มีการ `eval` Source นักเรียน

## API / License

API ใช้ Memory Store: **ข้อมูลหายเมื่อ restart** และไม่มี Seed Key ที่ใช้ได้จริง
Admin endpoints ปิดไว้หากไม่กำหนด `SCHOOL_BOT_ADMIN_KEY` อย่างน้อย 32 ตัวอักษร

```powershell
# ตั้ง token สำหรับ local dev ใน terminal นี้ ห้าม commit token
$env:SCHOOL_BOT_ADMIN_KEY = [Convert]::ToHexString([Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
./scripts/start-api.ps1
```

ส่ง Header `Authorization: Bearer <token ของคุณ>` ผ่าน HTTP client
ตัวอย่างอยู่ที่ `web/src/SchoolBot.Api/SchoolBot.Api.http` และ `docs/API.md`
API เก็บเฉพาะ SHA256 ของ License Key ที่สุ่มด้วย cryptographic RNG; คืน Key เต็มครั้งเดียวตอนสร้าง
Desktop ส่ง Key ไป Activate แต่ **ไม่บันทึก Key** ใน PoC ไม่มี Offline License Token หรือ License Enforcement
Local Learning ไม่ถูกปิดเมื่อ API ล่ม

Desktop ต่ออินเทอร์เน็ตได้สำหรับ Activate License และตรวจอัปเดต ส่วนสาม Mode ทำงานในเครื่อง
ระหว่างพัฒนา API รันที่ `http://localhost:8080` ตามเดิม. เป้าหมายใช้งานจริงคือ Desktop/Web→HTTP Gateway→gRPC SchoolBot โดยเว็บผ่าน same-origin BFF; ไม่เปิด B เป็น public fallback. B ปัจจุบันยังเป็น HTTP; ต้องทำ RPC/HTTP mapping และ trusted tenant/opaque-session boundary ก่อนใช้งานผ่าน Gateway. ตัวอย่าง Build ด้านล่างสาธิตการฝัง URL เท่านั้น ไม่ยืนยันว่า Gateway integration พร้อม

```powershell
# สำหรับ Build แจกทีม: ฝัง public API URL ใน executable (ตัวอย่าง Domain เท่านั้น)
$env:SCHOOL_BOT_BUILD_API_URL = 'https://api.example.com'
./scripts/build-desktop.ps1 -SkipInstall
```

ผู้รับแอปไม่ต้องตั้ง URL เอง ส่วนผู้พัฒนายัง Override ตอนเปิดด้วย `$env:SCHOOL_BOT_API_URL` ได้
ถ้าไม่ตั้งค่าใดเลยใช้ `http://localhost:8080`; HTTP อนุญาตเฉพาะ loopback
ดูขั้นตอนแยก Development/Domain และคำสั่ง macOS ใน `docs/DEPLOYMENT.md`

## แบ่งงานทีม

| ทีม | จุดเริ่มต้น |
|---|---|
| Mode 1 Robot | `app/frontend/src/modes/robot-control/` |
| Mode 2 Adventure | `app/frontend/src/modes/coding-adventure/`, `app/frontend/src/components/coding-adventure/` |
| Mode 3 Blocks | `app/frontend/src/modes/block-programming/` |
| Desktop Core | `app/desktopservice.go`, `app/internal/engine/`, `app/frontend/src/core/` |
| API | `web/SchoolBot.slnx` และ 4 layers ใน `web/src/` |

อ่าน `docs/ARCHITECTURE.md` และ `engines/README.md` ก่อนแก้ shared contract
Mode ใช้ `app/frontend/src/core/client.ts` ร่วมกัน; การเพิ่ม Action ต้องปรับ contract, engine และ frontend types ให้ตรงกัน

## Browser preview (สำหรับทำ UI เท่านั้น)

```powershell
cd app/frontend
npm ci
npm run dev
# เปิด http://127.0.0.1:5173/?preview=1
```

`?preview=1` ใช้ Simulator แยกและ localStorage สำหรับข้อมูลโครงงาน **ไม่ใช่ native IPC**
ไม่มี Key/Token เก็บใน localStorage หน้า Preview ไม่ติดต่อ License API
Browser ไม่ใส่ parameter จะไม่แอบ fallback เป็น Mock และแจ้งว่า Native Bridge ใช้ไม่ได้
Desktop ใช้ Go Process และ filesystem จริง

## ส่งต่อให้ทีม

สำหรับผู้ใช้ทั่วไป ใช้ **Desktop Installer**: Windows `-setup.exe` และ macOS Universal `.pkg`
ทั้งสองแบบรวมแอปสาม Mode กับ Engine แล้ว ไม่ต้องติดตั้ง Go/Node/.NET ที่เครื่องผู้ใช้

```powershell
# Windows: Build แอปและสร้าง setup EXE พร้อม MSI สำหรับ IT
./scripts/package-desktop.ps1
# ถ้า Build แอปล่าสุดไว้แล้ว
./scripts/package-desktop.ps1 -SkipBuild
# เมื่อ app/bin มีแอปที่เปิดอยู่ ให้ build และแพ็กจากโฟลเดอร์แยก
./scripts/package-desktop.ps1 -PayloadDirectory "$env:TEMP\school-bot-payload"
```

```bash
# macOS: สร้าง .pkg ไฟล์เดียวสำหรับ Intel + Apple Silicon (ต้องรันบน Mac)
bash scripts/package-desktop.sh
```

Installer อยู่ใน `dist/installers/` พร้อมไฟล์ SHA256 ดูขั้นตอนติดตั้งใน `docs/INSTALLERS.md`
Windows MSI มี Shortcut, Repair และถอนผ่าน Settings > Apps / msiexec ได้
Setup EXE วาดหน้าต่างไร้กรอบ มุมโค้ง ฟอนต์ไม่มีหัวและมาสคอต School Bot ที่เคลื่อนไหวได้
มีปุ่มติดตั้งคลิกเดียวลงตำแหน่งเริ่มต้น ไม่ต้องเลือกโฟลเดอร์หรือส่วนประกอบ
หน้าสุดท้ายมี checkbox เปิดแอปหลังติดตั้ง ซึ่งยกเลิกได้; silent install, Repair และ uninstall ไม่เปิดแอป
หากเครื่องไม่มี WebView2 แอปจะขอให้ติดตั้งผ่านอินเทอร์เน็ตตอนเปิดครั้งแรก
Mac Installer ลง `School Bot.app` ใน `/Applications`
Installer นี้ยังเป็น development build ที่ไม่ลงนาม ต้องทำ Windows signing / Mac Developer ID + notarization ก่อนแจกภายนอก
API ใน `web/` Deploy แยกบน Server ไม่รวมใน Desktop Installer

ส่ง Source พร้อม `go.mod`, `go.sum`, `package-lock.json` และ solution ทั้งชุด
สร้าง ZIP สำหรับส่งให้ทีมด้วย `./scripts/package-source.ps1` จะได้ `dist/school-bot-team-starter.zip`
ตัวแพ็ก prune `.tools`, `node_modules`, `bin`, `obj`, build output และ junction/symlink ก่อนเดินลงไป;
ตัด `.env*`, `appsettings.*.json`, key/certificate และ debug artifacts ตาม denylist และรวม `AGENTS.md`/`CLAUDE.md`
การกรองชื่อไฟล์ไม่ยืนยันว่า source ไม่มี secret ที่ฝังไว้ ต้องตรวจเนื้อหาก่อนส่งจริง
ใช้ `./scripts/package-source.ps1 -OutputDirectory <path>` เพื่อแยก ZIP และตรวจตัวแพ็กด้วย
`./scripts/tests/source-package.test.ps1`
Windows executable ที่ build แล้วอยู่ใน `app/bin/` (หรือโฟลเดอร์ที่ระบุด้วย `-OutputDirectory`) พร้อม `demo-engine.exe` และ `engines/zide/` ต้องเก็บโครงสร้างนี้ครบเมื่อใช้แบบ portable
Frontend/Phaser/Blockly/Media ฝังอยู่ใน executable แล้ว ไม่โหลดจาก CDN
MSI รวม WebView2 Bootstrapper ไว้ แอปจะเรียก Microsoft Setup เมื่อผู้ใช้ยืนยันตอนเปิดครั้งแรกหากยังขาด Runtime
Portable executable ที่ไม่มี Bootstrapper ต้องติดตั้ง WebView2 อยู่ก่อน
การเปิดแอปครั้งแรก **ยังต้องต่อเน็ต** กรณีเครื่องไม่มี WebView2
macOS มี `.app`/ZIP สำหรับพัฒนาและ Universal `.pkg` สำหรับติดตั้ง พร้อม CI ตรวจแยก Intel/ARM64
ยังต้องรัน CI/ทดสอบเครื่องจริงและทำ Developer ID signing/notarization ก่อนแจกภายนอก

การเปลี่ยนชื่อจะคัดลอก Project/Device ID เดิมจาก `ArmStudioPoC` ไป `SchoolBotPoC` เมื่อไฟล์ใหม่ยังไม่มี
ไม่แก้ไฟล์เดิมและไม่ทับ Project ใหม่ Device ID เดิมช่วยไม่ให้ใช้ License seat เพิ่ม
ใช้ `SCHOOL_BOT_API_URL` / `SCHOOL_BOT_ADMIN_KEY` สำหรับตั้งค่าใหม่ โดยยังอ่าน `ARM_API_URL` / `ARM_ADMIN_KEY` เดิมเป็น fallback ได้

## Verification / limits

รันชุดตรวจด้วย `scripts/verify.ps1`; ขอบเขต Hardware/transport อยู่ที่ `engines/zide/docs/INTEGRATION.md`
ยังต้องทดสอบหุ่นจริง, macOS และการอัปเกรด Installer ล่าสุดบนเครื่องทดสอบ
300 MB, startup 5 วินาที และ 30 FPS ยังเป็นเป้าหมาย ไม่ใช่ผล benchmark
การ Build หรือ Browser Preview ไม่ใช่การพิสูจน์ WebView2/macOS หรือ Hardware Safety

ดู `THIRD-PARTY-NOTICES.md` เรื่อง MIT/Dependencies; starter นี้ไม่ได้ประกาศว่า Source ของ School Bot ต้องเป็น MIT
