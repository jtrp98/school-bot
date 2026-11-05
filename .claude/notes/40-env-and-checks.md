# Environment และการตรวจงาน

> สถานะ: เตรียมเครื่องและรัน baseline เมื่อ 2026-11-02 (ยังไม่ได้แก้โค้ด)

## Baseline 2026-11-02 (commit `f77659f`)

| เช็ก | ผล |
|---|---|
| `npm run typecheck` | ✅ ผ่าน |
| `npm run test:i18n` | ✅ 14/14 |
| `npm run build` | ✅ (ตรวจ release assets แล้ว 129 ไฟล์ ไม่มี source map) · chunk ใหญ่: `CodeLabMode` 1.38 MB, `BlockMode` 895 KB |
| `node --test` adapter + workspace + sample-workspace | ✅ 50/50 |
| `go test ./...` (app, ใส่ `-X ...appVersion`) | ✅ ผ่าน · `internal/project` ไม่มี test |
| `go vet ./...` (app) | ✅ ผ่าน |
| `build-engine.ps1 -Engine mixly -Test` | ✅ go test/vet + IPC 5/5 |
| zide build/test | ⏭️ ไม่ได้รัน เพราะไม่มี CMake และ MSVC |
| `verify.ps1` ทั้งชุด | ⏭️ ไม่ได้รัน (ต้องมี zide และจะล้มที่ `web/`) |

## เครื่องนี้มีอะไรแล้ว

| เครื่องมือ | ต้องใช้กับ | สถานะ |
|---|---|---|
| Node.js | frontend, tests | ✅ v24.13.0 (npm 11.6.2) |
| .NET SDK | installer WPF / `web/` | ✅ 10.0.302 |
| Go ≥ 1.25 (app), ≥ 1.26 (mixly) | host, demo-engine, mixly | ✅ go1.26.10 portable ที่ `.tools/go` (ตรวจ SHA256 แล้ว) · ไม่อยู่ใน PATH สคริปต์จะหาเอง · module cache อยู่ที่ `.tools/gopath`, `.tools/gocache` |
| CMake + MSVC C++17 + Windows SDK | zide | ❌ ไม่มี `cmake` (ยังไม่ได้ตรวจ MSVC) |
| `app/frontend/node_modules` | typecheck/build/i18n test | ✅ `npm ci` แล้ว (89 packages, 0 vulnerabilities) |
| PowerShell 7 (`pwsh`) | สคริปต์ | ❌ มีแค่ Windows PowerShell 5.1 |

## Gotchas ที่เจอตอนรันจริง

1. **PowerShell 5.1 ล้มเมื่อ module cache ยังว่าง:** สคริปต์ตั้ง `$ErrorActionPreference = 'Stop'` พอ `go` พิมพ์ `go: downloading ...` ออก stderr ก็กลายเป็น `NativeCommandError` วิธีแก้คือรัน `go test ./...` ใน Git Bash ก่อน (ตั้ง PATH เป็น `.tools/go/bin` + `GOPATH`/`GOCACHE` เป็น `.tools/...`) ให้ cache ครบ หรือติดตั้ง PowerShell 7
2. **`-OutputDirectory` แบบ relative ของ mixly ซ้อน path:** `engines/mixly/scripts/build.ps1` resolve path หลัง `Push-Location` เข้าโฟลเดอร์ engine ทำให้ `./engines/mixly/bin/check` ไปจบที่ `engines/mixly/engines/mixly/bin/check` ให้ใช้ path แบบ absolute หรือไม่ใส่ (ค่าเริ่มต้นคือ `engines/mixly/bin/windows-amd64`) · README ของ mixly เขียนตัวอย่างแบบ relative ไว้ ซึ่งไม่ตรงกับพฤติกรรมจริง
3. **`go test` ใน `app` สร้าง `go.work.sum` ที่ root:** ไฟล์นี้ไม่อยู่ใน repo และไม่ได้ ignore ทุกครั้งที่ go รันจะเกิดใหม่ ต้องให้ทีมตัดสินว่าจะ commit หรือเพิ่มใน `.gitignore` (ตอน baseline ลบทิ้งไปแล้ว)
4. ใน Git Bash ให้ใช้ `GOPATH='C:\src\school-bot\.tools/gopath'` (backslash) จะได้ cache ชุดเดียวกับที่สคริปต์ PowerShell ใช้
| arduino-cli + AVR core | compile/upload Block Lab | ⚠️ มีใน `C:\src\sb-app-mixly\arduino-cli` (4.2 GB, เป็น runtime ของ Electron) แต่ยังไม่มีใน `toolchains/` ของ mixly engine |
| gh CLI | PR บน GitHub | ❌ ไม่มี |
| Python | — | ไม่มี (สคริปต์ของ repo ไม่ได้ใช้) |

## ลำดับเตรียมเครื่อง

1. ✅ **Frontend:** `cd app/frontend` → `npm ci`
2. ✅ **Go portable:** go1.26.10 windows-amd64 จาก go.dev แตกไปที่ `C:\src\school-bot\.tools\go` (`.tools/` อยู่ใน gitignore) ใช้ได้ทั้ง app (≥1.25) และ mixly (≥1.26)
3. ✅ **ตรวจ host:** ต้อง `npm run build` ก่อน (go:embed ต้องการ `frontend/dist`) แล้ว `cd app` → `go test ./...`, `go vet ./...`
4. ✅ **ตรวจ mixly:** `./scripts/build-engine.ps1 -Engine mixly -Test` (ถ้าจะใส่ `-OutputDirectory` ให้ใช้ path แบบ absolute)
5. ⏳ **Zide (เมื่อจำเป็น):** ติดตั้ง CMake + Visual Studio Build Tools (C++ workload + Windows SDK, หลาย GB) แล้วรัน `./scripts/build-engine.ps1 -Engine zide -Test` ต้องใช้เมื่อแตะ Arm Lab/zide หรือรัน `verify.ps1`
6. **Preview UI:** `.claude/launch.json` มี config ชื่อ `frontend-preview` (`npm --prefix app/frontend run dev` ที่ port 5173) เปิดแล้วไปที่ `http://127.0.0.1:5173/?preview=1`

## คำสั่งตรวจตามส่วนที่แก้

ดูตารางเต็มใน skill `.claude/skills/verify-changes/SKILL.md` สรุปสั้นๆ:

- frontend: `npm run typecheck` + `npm run build` · ข้อความ UI: `npm run test:i18n` · robot: `node --test app/frontend/tests/frontend-*.test.mjs`
- Go host: `go test ./...` + `go vet ./...` (ใน `app`)
- engines: `./scripts/build-engine.ps1 -Engine <id> -Test`
- integration: `./scripts/verify.ps1 -SkipInstall -OutputDirectory "$env:TEMP\school-bot-verify"` **จะล้มที่ขั้นสุดท้าย** เพราะไม่มี `web/`

## Build/CI

- `scripts/build-desktop.ps1`: build zide → `engines/zide/` และ demo-engine → build frontend → `go build -tags production` + ฝังไอคอน/version resource (`scripts/windows-resources`) ได้ไฟล์ `school-bot-<version>.exe` และสำเนา `school-bot.exe` · **ยังไม่ bundle mixly**
- `scripts/verify.ps1`: build-desktop → i18n test → zide `-Test` → `node --test` (adapter, workspace, zide ipc) → `go test`/`go vet` → smoke ของ demo-engine (adventure + block) และ zide dry-run → `dotnet test` ใน `web/`
- `scripts/package-desktop.ps1`: setup EXE (WPF ใน `app/installer`) + MSI (WiX, `app/build/windows/*.wxs`) · `package-source.ps1` ใช้แพ็ก source ส่งทีม
- `.github/workflows/verify.yml`: matrix Windows/macOS รัน `verify.ps1`, แพ็ก installer และ `test-installer.ps1` ถ้า CI รันบน repo นี้ จะล้มเพราะไม่มี `web/` เหมือนกัน (ควรเช็กผลจริงใน GitHub)
- ห้าม build ทับ `app/bin` ตอนที่แอปเปิดอยู่ ให้ใช้ `-OutputDirectory` และอย่าปิด process ของผู้ใช้เอง

## ดูหน้า Block Lab ใน browser preview (พบ 2026-11-02)

- `main.tsx` ใช้ `unavailableEntryAdapter` จึงติดหน้า "ตรวจสอบสิทธิ์การใช้งาน" ทุกครั้งใน `?preview=1` เข้า Labs ไม่ได้
- วิธีที่ใช้ตรวจหน้าตา: สร้าง harness ชั่วคราวที่ root ของ `app/frontend` (เช่น `tmp-block-lab-check.html` + `.tsx` ที่ import `./src/tailwind.css`, `./src/components/shared/fonts` แล้ว render `BlockMode`) เปิด `http://127.0.0.1:5173/tmp-block-lab-check.html?preview=1&scenario=block-sheet` แล้ว**ลบทิ้งหลังตรวจ** (Vite build ใช้แค่ `index.html` จึงไม่ติดไปกับ release แต่ห้ามทิ้งไว้ใน repo)
- `?scenario=block-sheet` = แผ่นตัวอย่างบล็อก (5 ชนิด × 4 สถานะ + 16 หมวด) ของ renderer Block Lab
