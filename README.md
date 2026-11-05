# School Bot · Team PoC

Starter สำหรับแบ่งทีมพัฒนา 3 Mode ไม่ใช่ Production Release

- `app/` — **Go + Wails v3 + React + TypeScript**, Desktop ทั้งหมด
- `service-school-bot/` — **C# / ASP.NET Core .NET 10**, API แบบ **Clean Architecture**; ดู [database-first scaffold และคำสั่ง re-scaffold](service-school-bot/README.md#ef-core-database-first-scaffold)
- `contracts/` — Engine JSON contract ที่ทุกทีมใช้ร่วมกัน
- `engines/` — Engine แยกตาม ID แต่ละตัวมี source, dependencies, tests และ build ของตัวเอง; ดู `engines/README.md`
- `docs/` — Architecture, API, การติดตั้ง/Deploy และการปกป้องโค้ด

Engine `mixly` อยู่ใน `engines/mixly/` และ build แยกด้วย
`./scripts/engines/build.ps1 -Engine mixly -Test` รองรับตัวสร้างโค้ดจากต้นฉบับ Mixly;
ยังไม่เชื่อม UI, host routing หรือรวมใน Desktop/Installer ดูขอบเขตที่ `engines/mixly/README.md`

## Labs Hardware Simulator

Run `npm run dev -- --port 5181` from `app/frontend`, then open
`http://127.0.0.1:5181/?simulator=1&view=robot-control&screen=connect` and choose Guest.
The explicit simulator supports HID/Serial profiles, poses, queues, upload/readback,
emergency torque cutoff and fault injection entirely in memory. Code/Block Labs
produce bounded visual traces. See [Phase 4 verification and remaining acceptance](docs/PHASE4-LABS.md).

## Quick start (Windows / PowerShell)

ติดตั้ง Go (เวอร์ชันขั้นต่ำตามบรรทัด `go` ใน `app/go.mod`), Node.js **24 LTS**, .NET **10 SDK** และ WebView2 Runtime ก่อน
Wails Go module และ JS runtime ล็อกไว้ที่ **v3.0.0-beta.30** ซึ่งยังเป็น Beta
สคริปต์ใช้ portable Go ใน `.tools/go` ถ้ามี ไม่เช่นนั้นใช้ `go` จาก PATH (สคริปต์ไม่ตรวจเวอร์ชัน Go);
โฟลเดอร์ `.tools/` ไม่ใช่ส่วนที่ต้องส่งให้ทีม

```powershell
# รันจาก repository root: ติดตั้ง dependencies และ build Desktop
./scripts/desktop/build.ps1

# เปิด Desktop ที่ build แล้ว (ไม่ต้องเปิด API ก็ใช้สาม Mode ได้)
./scripts/desktop/run.ps1 -SkipBuild

# อีก terminal: เปิด SchoolBot gRPC ที่ 127.0.0.1:8080 และ health HTTP ที่ 127.0.0.1:8081
./scripts/api/run.ps1

# ตรวจ frontend, i18n, Go, JSON IPC (smoke + ipc.test.mjs), Zide C++ tests, C# integration และ architecture tests
./scripts/verify.ps1 -SkipInstall

# ถ้า app/bin มีแอปที่กำลังเปิดอยู่ ให้ build และตรวจในโฟลเดอร์แยก (Windows)
./scripts/verify.ps1 -SkipInstall -OutputDirectory "$env:TEMP\school-bot-verify"

# Build อย่างเดียวในโฟลเดอร์แยก (ไม่แตะ app/bin ที่เปิดอยู่)
./scripts/desktop/build.ps1 -SkipInstall -OutputDirectory "$env:TEMP\school-bot-build"
```

Build ได้ `school-bot-<version>.exe` (เวอร์ชันจาก `app/frontend/package.json`) และสำเนา `school-bot.exe` ที่ installer/start script ใช้;
Windows build ฝังไอคอน `app/build/windows/appicon.ico` และ version resource ผ่าน `scripts/assets/windows-resources` แล้วตรวจ `-verify` ให้อัตโนมัติ; สร้างไอคอนใหม่จากโลโก้ด้วย `scripts/assets/create-windows-icon.ps1`

ดูคำสั่งและหมวดหมู่ทั้งหมดที่ [scripts/README.md](scripts/README.md)

ถ้าเครื่องมีนโยบายห้ามรัน PowerShell scripts ให้ใช้คำสั่งด้านล่าง หรือให้ IT ใช้นโยบายที่อนุมัติแล้ว ไม่จำเป็นต้องปิดระบบรักษาความปลอดภัย
คำสั่งนี้เป็นขั้นต่ำเพื่อลองรัน ไม่ได้ฝังไอคอน/version resource และไม่ได้ตั้ง `-X main.appVersion` ใช้ `scripts/desktop/build.ps1` เมื่อจะส่งมอบ

```powershell
cd app/frontend
npm ci
npm run build
cd ..
../scripts/engines/build.ps1 -Engine zide -OutputDirectory ./bin/engines/zide
go build -trimpath -ldflags '-s -w' -o bin/demo-engine.exe ./cmd/demo-engine
go build -trimpath -tags production -ldflags '-s -w -H windowsgui' -o bin/school-bot.exe .
./bin/school-bot.exe

# อีก terminal
cd service-school-bot
dotnet run --project src/ServiceSchoolBot.API --no-launch-profile
```

## macOS (Intel / Apple Silicon)

รองรับการ Build เป็น **School Bot.app** สำหรับเครื่อง Mac ที่ใช้ macOS **15 ขึ้นไป**
ติดตั้ง Go (ตาม `app/go.mod`), Node.js 24 LTS และ Xcode Command Line Tools (`xcode-select --install`)
ติดตั้ง .NET 10 SDK เพิ่มเฉพาะเมื่อจะรัน/พัฒนา C# API หรือชุดทดสอบ API บนเครื่องนั้น

```bash
# รันจาก repository root บน Mac ไม่ต้องติดตั้ง PowerShell
bash scripts/desktop/build.sh
bash scripts/desktop/run.sh --skip-build

# อีก Terminal หากต้องการเปิด C# API
bash scripts/api/run.sh
```

- Apple Silicon → `app/bin/darwin-arm64/School Bot.app`
- Intel → `app/bin/darwin-amd64/School Bot.app`
- ZIP ของแต่ละ architecture อยู่ข้าง `.app` และรวม Demo Engine กับ C++ Zide Engine แล้ว
- Build บน Mac ตาม architecture ของ Go SDK ในเครื่อง; CI แยก Intel และ ARM64
- โปรแกรมหลักและ Engine อยู่ใน `Contents/MacOS` จึงไม่พึ่ง current working directory ตอนเปิดผ่าน Finder
- ใช้ WKWebView ของ macOS ไม่ต้องติดตั้ง WebView2
- Build Script ใช้ ad-hoc signing สำหรับพัฒนาในเครื่อง **ยังไม่ใช่ Developer ID / notarized release**
  ก่อนแจกนอกทีมต้องทำ signing/notarization ตามคู่มือ `docs/INSTALLERS.md`
- PowerShell 7 บน Mac ยังใช้ `scripts/desktop/build.ps1` และ `scripts/verify.ps1` ได้

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

SchoolBot ให้บริการ **gRPC เท่านั้น** สำหรับ Projects, Licenses, Curricula, Assessments และ Releases
HTTP เหลือ `/healthz`; ค่าปริยายคือ gRPC h2c `127.0.0.1:8080` และ health HTTP `127.0.0.1:8081`
เส้นทางของ client คือ Desktop/Web → HTTP Gateway → gRPC SchoolBot โดย browser ผ่าน same-origin BFF
สัญญา HTTP อยู่ที่ [docs/API.md](docs/API.md) และใช้ prefix `/api/service-school-bot/` โดยไม่มี v1/v2

Login ใช้ `POST /api/auth/login` ของ Gateway และส่ง `Authorization: Bearer <Gateway JWT>`
สำหรับผู้ใช้ที่ login เต็มรูป (`AccessLevel=1`); desktop ส่ง `X-Device-Entitlement` เพิ่มเมื่อเรียกข้อมูลของผู้ใช้
TAdmin (`admin_id`) เป็น staff ส่วนกลาง ส่วน `IsAdmin` เป็นแอดมินโรงเรียน
SchoolBot ไม่มี admin key, logout หรือ web-session exchange ของตัวเองแล้ว
Device challenge, activate/validate และ update check เป็น anonymous ที่ Gateway แต่ device licensing ยังตรวจ key และ signed proof

ค่าเริ่มต้นใช้ Memory Store: **ข้อมูลหายเมื่อ restart**; file store มีไว้สำหรับการทดสอบ persistence ตามข้อจำกัดใน [service-school-bot/README.md](service-school-bot/README.md)
License key สุ่มด้วย cryptographic RNG และเก็บ hash; การสร้างคืน key และรองรับ replay ตามสัญญา idempotency
Device activation คืน signed entitlement ของ stub ซึ่งยังต้องมี production signing/key management
Publish release เปิดเผย metadata เท่านั้น ไม่ดาวน์โหลด ติดตั้ง หรือตรวจ artifact signature ให้

Gateway มี module/mapping แล้ว แต่ **ยังไม่ต่อเข้า Program ของ Gateway** และยังไม่ผ่าน H5/G-AUTH
SchoolBot ใช้ `DenyAllGatewayPeerVerifier` และ Gateway ใช้ `ProductionSchoolBotTransportValidator` ที่ปฏิเสธเมื่อยังไม่มี trusted transport
จึงยังเปิดใช้ production ไม่ได้ และไม่มี HTTP fallback ที่ SchoolBot

Desktop ใน `app/` ยังใช้ URL/path HTTP เดิม ต้องย้ายให้ชี้ Gateway พร้อม JWT และ entitlement ก่อนใช้ flow นี้จริง
การตั้ง base URL อย่างเดียวไม่เปลี่ยน path เดิม ตัวอย่าง Build ด้านล่างสาธิตการฝัง URL เท่านั้น

```powershell
# สำหรับ Build แจกทีม: ฝัง public API URL ใน executable (ตัวอย่าง Domain เท่านั้น)
$env:SCHOOL_BOT_BUILD_API_URL = 'https://api.example.com'
./scripts/desktop/build.ps1 -SkipInstall
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
| API | `service-school-bot/ServiceSchoolBot.slnx` และ 4 layers ใน `service-school-bot/src/` |

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
./scripts/desktop/package.ps1
# ถ้า Build แอปล่าสุดไว้แล้ว
./scripts/desktop/package.ps1 -SkipBuild
# เมื่อ app/bin มีแอปที่เปิดอยู่ ให้ build และแพ็กจากโฟลเดอร์แยก
./scripts/desktop/package.ps1 -PayloadDirectory "$env:TEMP\school-bot-payload"
```

```bash
# macOS: สร้าง .pkg ไฟล์เดียวสำหรับ Intel + Apple Silicon (ต้องรันบน Mac)
bash scripts/desktop/package.sh
```

Installer อยู่ใน `dist/installers/` พร้อมไฟล์ SHA256 ดูขั้นตอนติดตั้งใน `docs/INSTALLERS.md`
Windows MSI มี Shortcut, Repair และถอนผ่าน Settings > Apps / msiexec ได้
Setup EXE วาดหน้าต่างไร้กรอบ มุมโค้ง ฟอนต์ไม่มีหัวและมาสคอต School Bot ที่เคลื่อนไหวได้
มีปุ่มติดตั้งคลิกเดียวลงตำแหน่งเริ่มต้น ไม่ต้องเลือกโฟลเดอร์หรือส่วนประกอบ
หน้าสุดท้ายมี checkbox เปิดแอปหลังติดตั้ง ซึ่งยกเลิกได้; silent install, Repair และ uninstall ไม่เปิดแอป
หากเครื่องไม่มี WebView2 แอปจะขอให้ติดตั้งผ่านอินเทอร์เน็ตตอนเปิดครั้งแรก
Mac Installer ลง `School Bot.app` ใน `/Applications`
Installer นี้ยังเป็น development build ที่ไม่ลงนาม ต้องทำ Windows signing / Mac Developer ID + notarization ก่อนแจกภายนอก
API ใน `service-school-bot/` Deploy แยกบน Server ไม่รวมใน Desktop Installer

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
Host ยังอ่าน `SCHOOL_BOT_API_URL` และ fallback `ARM_API_URL`; ค่าปริยายและ path เดิมของ desktop ยังต้องย้ายตามสัญญา Gateway ข้างต้น

## Verification / limits

รันชุดตรวจด้วย `scripts/verify.ps1`; ขอบเขต Hardware/transport อยู่ที่ `engines/zide/docs/INTEGRATION.md`
ยังต้องทดสอบหุ่นจริง, macOS และการอัปเกรด Installer ล่าสุดบนเครื่องทดสอบ
300 MB, startup 5 วินาที และ 30 FPS ยังเป็นเป้าหมาย ไม่ใช่ผล benchmark
การ Build หรือ Browser Preview ไม่ใช่การพิสูจน์ WebView2/macOS หรือ Hardware Safety

ดู `THIRD-PARTY-NOTICES.md` เรื่อง MIT/Dependencies; starter นี้ไม่ได้ประกาศว่า Source ของ School Bot ต้องเป็น MIT
