# Host (Go) และ engines

> สถานะ: สรุปจากโค้ด 2026-11-02 · อ้างอิง path:line ตาม commit `f77659f`

## Go host (`app/`)

| ไฟล์ | หน้าที่ |
|---|---|
| `main.go` (70) | เช็ก WebView2 ก่อน → `NewDesktopService` → หน้าต่าง frameless เปิดแบบ fullscreen ที่ (0,0) · ปุ่มปิด/Alt+F4 ไม่ปิดทันที แต่ยิง event `school-bot:close-requested` ให้ frontend ตรวจ guard ก่อน จากนั้นจึงเรียก `CloseWindow` |
| `desktopservice.go` (193) | method ที่ frontend เรียกได้: `CloseWindow`, `Info`, `Run(mode, action, data)`, `CancelRun`, `CheckUpdates`, `ActivateLicense` · `api()` timeout 5 s, ห้าม redirect, body ≤ 64 KB, HTTP ใช้ได้เฉพาะ loopback |
| `desktopstorage.go` (141) | `SaveProject/LoadProject(mode)` → `<UserConfigDir>/SchoolBotPoC/<mode>.json` ≤ 1 MB เขียนแบบ temp+sync+rename · ครั้งแรกคัดลอกจาก `ArmStudioPoC` (ไม่ทับไฟล์ที่มีอยู่) · `device-id` สุ่ม 16 byte hex |
| `runtime_windows.go` | ถ้าไม่มี WebView2 จะถามแล้วเรียก bootstrapper |
| `internal/engine/manager.go` (254) | routing `engineForMode` (`:43`), allowlist `modeActions` (`:60`), `Execute` (`:133`), `Cancel` (`:115`) |
| `internal/engine/protocol.go` | `Protocol = "1.0"`, error codes `ENGINE_BUSY/CANCELLED/FAILED/TIMEOUT` |
| `internal/engine/demo.go` | engine ของ `coding-adventure/program.run` และ `block-programming/program.run` (`commands: [{op: move_right|move_left|jump, value}]`) |
| `cmd/demo-engine/main.go` | ไบนารี demo-engine ที่ห่อ `HandleDemo` |
| `internal/project/*`, `internal/license/*` | **ไม่มีใคร import** (ดู 00-overview) |

### Engine Manager

- spawn process ต่อ 1 request ไม่ใช้ shell · input ≤ 64 KB, stdout ≤ 1 MB, stderr ≤ 8 KB (ส่วนที่เกินถูกทิ้งเพื่อไม่ให้ pipe เต็มจน engine ค้าง)
- `requestTimeout = 5 * time.Second` (`manager.go:20`) · รับได้ครั้งละ 1 request ถ้าชนจะได้ `ENGINE_BUSY`
- `Cancel` รอให้ child ปิดแล้วคืน slot · ถ้า `robot.stop` กำลังทำงาน จะปล่อยให้ทำจนจบ (`manager.go:125,151`) · `WaitDelay` 1 s
- host ตรวจ response กับ `contracts/engine/response.schema.json` · frontend แยกกรณีด้วย `error.code` ไม่ดูจากข้อความ

> **อัปเดต 2026-11-02:** routing ย้ายไป `app/internal/engine/routes.go` (`modeRoutes` ตาม action) และ `block-programming` route `program.generate`/`project.validate` ไป mixly แล้ว (board บังคับ UNO) · action native ยังไม่ route ดูเงื่อนไขที่ BL-54 ในบันทึก 85

### ถ้าจะ route Block Lab ไป mixly ต้องเพิ่ม

1. `engineForMode("block-programming")` → `engines/mixly/mixly-engine` และเพิ่ม action ลง allowlist (เช่น `board.info`, `program.generate`, `program.compile`, `program.upload`, `device.list`, `serial.exchange`)
2. deadline แยกตาม action เพราะ compile/upload ใช้นาน ≤ 2 นาที และอาจต้องมี progress หรือปุ่มยกเลิกที่ UI · ห้ามแค่ขยาย 5 s ทั้งระบบ
3. serial monitor แบบต่อเนื่องยังไม่มีใน contract ตอนนี้เป็น request/response ทีละครั้ง
4. build/package ต้องคัดลอก `mixly-engine` + `toolchains/` (arduino-cli, cores) + notices ไปไว้ข้าง exe (`scripts/build-desktop.ps1:22-33` ทำแค่ zide/demo)
5. ทำตาม skill `engine-action-change` และให้ `hardware-safety-reviewer` รีวิว

## Zide (`engines/zide/`, C++17, ~1,070 บรรทัด)

- actions: `engine.info`, `device.list`, `servo.set`, `servo.read`, `servo.torque`, `robot.stop` (ส่ง `$DST!` ตอบแค่ `stop_requested` ไม่ยืนยันว่าหยุดจริง), `board.info` (`$DRS!`), `group.execute/download/run/read`, `project.import/export` (INI) · ดู `docs/API.md:99-111`
- transport HID/Serial บน Windows (ใช้ hidapi เวอร์ชัน vendored) · ค่าเริ่มต้นเป็น dry-run
- build ต้องมี CMake และ toolchain C++17 พร้อม Windows SDK (`scripts/build.ps1:67-73`) **เครื่องนี้ยังไม่มี**
- tests: `tests/protocol_test.cpp`, `tests/ipc.test.mjs` (ใช้ `ZIDE_ENGINE` หรือ `bin/<os>-<arch>/`), `tests/schema-check.mjs`
- host allowlist ของ robot-control ไม่มี `group.read`, `project.import/export` แม้ engine จะรองรับ

## Mixly (`engines/mixly/`, Go ~1,500 บรรทัด + `src/bootstrap.js`)

- รัน **generator JS ต้นฉบับของ Mixly** ใน goja ไม่ได้เขียน generator ใหม่ · snapshot มาจาก `sb-app-mixly/resources/app/src` (upstream `gitee.com/mixly2/mixly2.0_src` 2.0rc3) มี 299 ไฟล์พร้อม sha256 ใน `source-manifest.json`
- **import เฉพาะ generators + board config + blockly-core บางส่วน** ไม่มี block definitions (`blocks/*.js`), toolbox (`index.html`), msg (`th.js` ฯลฯ) และ media ดังนั้น UI ของ Block Lab ต้องหาบล็อกมาจากที่อื่น
- actions (`docs/API.md:12-22`): `engine.info`, `board.list`, `board.info`, `toolchain.status`, `project.validate`, `program.generate`, `program.compile`, `program.upload`, `device.list`, `serial.exchange`, `firmware.flash`
- input: `blocks` (JSON tree) หรือ `xml` (ข้อความ `.mix`) หรือ `source` (ใช้ได้กับ compile/upload เท่านั้น) · ตัวอย่าง UNO: `board: "arduino_avr"`, `platform: "Arduino/Genuino Uno"`
- ความปลอดภัย: `transport` เริ่มเป็น `dry-run` ต้องส่ง `"native"` เองถึงจะใช้งานจริง · executable หาได้จาก `toolchains/` ข้างไบนารีเท่านั้น (`arduino-cli`, `python3`, `mpremote`, `esptool`, `kflash`) · ไม่ดาวน์โหลดอะไรเอง
- ขอบเขต: บล็อก ≤ 512, ซ้อน ≤ 32 ชั้น, generate ใช้ VM ใหม่ทุกครั้งและมีเวลา 3 s, ผลลัพธ์ ≤ 128 KB · tool ละ ≤ 2 นาที · serial เขียน ≤ 1024 / อ่าน ≤ 8192 byte, timeout 1–2000 ms, ปิด port ทุก request
- error codes: `PROTOCOL_INVALID`, `INVALID_REQUEST`, `INVALID_INPUT`, `PROJECT_INVALID`, `PROGRAM_INVALID`, `ACTION_UNKNOWN`, `SAFETY_LIMIT`, `TRANSPORT_ERROR`, `ENGINE_FAILED`, `ENGINE_TIMEOUT`
- sync จาก sb-app-mixly: `./engines/mixly/scripts/import-source.ps1 -SourceDirectory <sb-app-mixly>` แล้วตามด้วย `./scripts/build-engine.ps1 -Engine mixly -Test` · ห้ามแก้ `third_party/` ด้วยมือ
- build/test ต้องใช้ Go ≥ 1.26 และ Node (เครื่องนี้มี go1.26.10 ที่ `.tools/go` แล้ว)

## Contracts (`contracts/`)

- `contracts/engine/{manifest,request,response}.schema.json` เป็นของกลาง · `contracts/engine-*.schema.json` ที่ root เก็บไว้อ้างอิงเพื่อ compatibility
- action เป็นของแต่ละ engine ห้ามทำ enum action กลาง · mode กับ engine แยกกัน (mode เดียวใช้หลาย engine ได้)
