# ภาพรวม school-bot

> สถานะ: สรุปจากการอ่านโค้ดจริง 2026-11-02 (branch `gun/adjust-block-lab`, commit `f77659f`) · ยังไม่แก้โค้ด · ถ้าโค้ดขัดกับบันทึกนี้ ให้เชื่อโค้ดแล้วแก้บันทึก

## โปรเจคนี้คืออะไร

ทีมรวม 2 โปรแกรมเดิมเป็นแอป desktop ตัวเดียวชื่อ **School Bot** แล้วปรับ UX/UI เป็นแบบ School Bright

| โปรแกรมเดิม | ใน school-bot | engine |
|---|---|---|
| Arm Lab (หุ่นแขนกล, ต้นทาง `sb-app-zide`) | Mode `robot-control` | `engines/zide` (C++17) |
| Block Lab (Mixly สำหรับ Arduino UNO, ต้นทาง `C:\src\sb-app-mixly`) | Mode `block-programming` (ตอนนี้ยังเป็น PoC) | `engines/mixly` (Go + goja) **ยังไม่ต่อกับ host** |
| — | Mode `coding-adventure` (เกม Phaser + DSL) | `demo-engine` |

- remote: `https://github.com/jtrp98/school-bot.git` · branch หลัก `main`
- งานของผู้ใช้คือ **Block Lab ทั้งหมด** (ยืนยัน 2026-11-02) ดูบันทึก 50, 70 และ 80
- `sb-app-mixly` เป็นต้นแบบไว้ศึกษาเท่านั้น ไม่แก้และไม่ออกเวอร์ชันต่อ งานจริงทำใน school-bot

## สถาปัตยกรรม

```text
React 19 + Tailwind 4 (app/frontend, ฝังใน exe ด้วย go:embed frontend/dist)
  │  core/client.ts → Call.ByName("main.DesktopService.<Method>")   (?preview=1 → core/preview.ts จำลอง)
  ▼
Go DesktopService (app/desktopservice.go, Wails v3.0.0-beta.30, frameless + fullscreen)
  ├─ SaveProject/LoadProject → <UserConfigDir>/SchoolBotPoC/<mode>.json (≤1 MB, temp+sync+rename)
  ├─ CheckUpdates / ActivateLicense → HTTP API (SCHOOL_BOT_API_URL, ค่าเริ่มต้น http://localhost:8080)
  └─ Run(mode, action, json) → internal/engine Manager (process ละ request, 5 s, 1 request ต่อครั้ง)
        ├─ robot-control      → engines/zide/zide-engine(.exe)   (allowlist 9 actions)
        ├─ coding-adventure   → demo-engine(.exe)                (program.run)
        └─ block-programming  → demo-engine(.exe)                (program.run)
```

- ตาราง routing กับ action allowlist อยู่ที่ `app/internal/engine/manager.go:43-68` webview ไม่ได้รับความไว้ใจ จึงส่ง action ที่ไม่อยู่ใน allowlist ไม่ได้
- โปรโตคอลเป็น JSON Lines 1.0: `protocol, request_id, mode, action, data` และใช้ schema กลางใน `contracts/engine/`
- หาไบนารี engine จากโฟลเดอร์ข้าง exe เท่านั้น ไม่ค้นจาก cwd จึงต้องรันจากโฟลเดอร์ build output

## repo และโฟลเดอร์ที่เกี่ยวข้อง

| ที่ | ใช้ทำอะไร | สิทธิ์ |
|---|---|---|
| `C:\src\school-bot` | repo หลัก (งานนี้) | เขียนได้ตามงาน |
| `C:\src\sb-app-mixly` | ต้นแบบ Electron Mixly + หน้า login School Bot ใช้ศึกษาและเป็นแหล่งคำแปล/นิยามบล็อก · มีงานแปลไทยที่ยังไม่ commit 11 ไฟล์ (2026-11-02) | อ่านอย่างเดียว |
| `C:\src\sb-app-mixly-knowledge` | STA knowledge root `mixly` (target `sb-app-mixly` อย่างเดียว) | อ่าน |
| `C:\Users\jabja\Downloads\mixly(ver.thai)` | Mixly ภาษาไทยครบ + runtime/arduino-cli | อ่าน |

## ตัวเลขสำคัญ

- Frontend: `@school-bot/desktop-ui` 0.1.5 (เป็นแหล่งเวอร์ชันเดียวของแอป) · React 19.3.1 · Blockly 13.4.0 · Phaser 4.2.1 · i18next 26 · Vite 8.3.4 · TypeScript 7.0.2 · Tailwind 4.3.3
- Go: `app` ต้องการ go 1.25, `engines/mixly` ต้องการ go 1.26 · `go.work` รวมเฉพาะ `./app`
- i18n: th/en/zh มีอย่างละ 1,633 key
- ไฟล์ใหญ่เกิน 500 บรรทัด: `modes/robot-control/workspace/WorkspaceLogic.ts` (660)
- ไฟล์เกิน 300 บรรทัด: `NativeConnectView.tsx` 490, `ConnectView.tsx` 487, `TeacherMenu.tsx` 483, `PoseTableRow.tsx` 460, `HomeView.tsx` 443, `NativeWorkspace.tsx` 434, `ConnectBody.tsx` 433, `PoseCards.tsx` 421, `AppView.tsx` 417 ฯลฯ

## เอกสารที่ไม่ตรงกับโค้ด (ต้องรู้ไว้)

1. README, ARCHITECTURE และ AGENTS.md พูดถึง `web/` (C# API, `SchoolBot.slnx`) แต่ checkout นี้ไม่มีโฟลเดอร์ `web/` จึงทำให้ `scripts/verify.ps1:79-80` ล้มที่ขั้นสุดท้าย และ `scripts/start-api.ps1` ใช้ไม่ได้
2. `app/internal/project/` (~2,500 บรรทัด: codec, migration, validator, store, lock) กับ `app/internal/license/` (~1,300 บรรทัด) ไม่มีโค้ดของแอป import เลย ที่ใช้จริงคือ `desktopstorage.go` ซึ่งเก็บ JSON ดิบต่อ mode ดังนั้นสองแพ็กเกจนี้เป็นของรอเชื่อมหรือ dead code ต้องถามทีมก่อนลบหรือใช้
3. `.gitignore` มี `app/frontend/bindings/` แต่ frontend เรียก `Call.ByName` เองโดยไม่ใช้ bindings ที่ generate
4. `engines/mixly/engine.json` ประกาศ modes `block-programming`, `coding-adventure` แต่ host ยังไม่ route มาที่ mixly ซึ่งตรงกับคำว่า "standalone engine only"
5. `engines/mixly/README.md` ยกตัวอย่าง `-OutputDirectory ./engines/mixly/bin/check` แต่สคริปต์ resolve path หลัง `Push-Location` ทำให้ไฟล์ไปอยู่ที่ `engines/mixly/engines/mixly/bin/check` (ดูบันทึก 40)

## จุดระวังข้ามทุกส่วน

- `go test`/`go vet` ใน `app` ต้องมี `app/frontend/dist` ก่อน เพราะ `main.go` ใช้ `//go:embed all:frontend/dist` จึงต้องรัน `npm run build` ก่อน
- Block Lab ในแผน/mockup ต้อง compile และ upload นาน ~2 นาที แต่ host ให้ request ละ 5 วินาทีและรับได้ทีละ request ต้องออกแบบ deadline/progress/cancel ใหม่ก่อน route ไป mixly
- `scripts/verify.ps1` ไม่รัน `frontend-sample-workspace.test.mjs` และไม่รัน test ของ mixly
- ข้อมูลโปรเจคย้ายจาก `ArmStudioPoC` ไป `SchoolBotPoC` (`desktopstorage.go:46`) ห้ามแก้หรือทับไฟล์เดิม

## อ่านต่อ

`10-host-and-engines.md` · `20-frontend.md` · `40-env-and-checks.md` · `50-block-lab-plan-extract.md` · `60-sb-app-mixly.md` · `70-block-lab-ui-spec.md` · `80-block-lab-roadmap.md` · `90-open-questions.md` · `95-claude-setup.md`
