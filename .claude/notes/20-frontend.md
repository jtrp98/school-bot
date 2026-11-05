# Frontend (`app/frontend`)

> สถานะ: สรุปจากโค้ด 2026-11-02 · React 19.3 + Tailwind 4.3 + Vite 8.3 + TS 7.0 (strict) · เวอร์ชันแอปอ่านจาก `package.json` (0.1.5)

## โครงสร้าง

| ที่ | หน้าที่ |
|---|---|
| `src/main.tsx`, `App.tsx` (208), `shell.tsx` | state ของ view/screen, การนำทาง, guard ตอนออกจากหน้า (`App.tsx:130`) · `robot-control` เป็นเต็มจอเมื่อไม่ได้อยู่หน้า works (`App.tsx:172`) |
| `src/app/AppView.tsx` (417) | layout หลัก แสดง mode ที่เลือก (`:257-274`) · Block Lab อยู่ใน `pageBodyClass` (หน้าแบบมี padding ไม่ใช่เต็มจอ) |
| `src/app/appData.tsx` | lazy import ของแต่ละ mode และรายการเมนู: `nav.lessons` (coding-adventure), `"Block Lab"`, `"Arm Lab"` (ชื่อแบรนด์ไม่แปล) |
| `src/app/entry/*` | หน้าเข้าแอปและสถานะ license (`useEntry`, `EntryGate`) · **ไม่ได้ต่อเข้าแอป** ตั้งแต่ 2026-11-02 (AUTH-1 ใน 90) |
| `src/app/useWindowClose.ts`, `CloseDialog.tsx`, `core/windowLifecycle.ts` | close guard ที่ต่อกับ event `school-bot:close-requested` |
| `src/core/client.ts` | **ทางเดียว**ที่คุยกับ host: `run`, `cancel`, `save`, `load`, `info`, `updates`, `activate`, หน้าต่าง · ถ้าเปิดด้วย `?preview=1` จะใช้ `core/preview.ts` กับ localStorage แทน |
| `src/core/types.ts` | `ModeID`, `Command`, `EngineResponse` · `core/engineError.ts` เก็บ `EngineError(code)` |
| `src/home/*` | หน้า Home: การ์ด Arm/Block, โปรเจคตัวอย่าง (`homeData.ts`) |
| `src/i18n/*` | ดู skill `i18n-strings` · th/en/zh มีอย่างละ 1,633 key · fallback `th` · เก็บภาษาใน `school-bot:lang` |
| `src/components/shared/*` | ระบบ UI กลาง (ตารางด้านล่าง) |
| `src/tailwind.css` | design tokens |
| `scripts/copy-media.mjs` | คัดลอก media ของ Blockly ไป `public/blockly-media/` (gitignored) ตอน build |
| `scripts/check-release-assets.mjs` | ตรวจ dist ก่อนปล่อย (ห้ามมี source map ฯลฯ) |
| `tests/*.mjs` | `i18n.test.mjs` (`npm run test:i18n`) · `frontend-adapter/workspace/sample-workspace.test.mjs` (`node --test`, ใช้ `stripTypeScriptTypes` ไม่ต้องมี engine) |
| `reports/*` | หลักฐานงาน close-guard และ entry preview เก่า (ไม่ใช่โค้ดแอป) |

## Design tokens (School Bright)

- สีมาจาก `--orange` (brand), `--blue`, `--ok #1f8a4c`, `--err #d3302f`, `--ink`, `--muted`, `--line`, `--sunken`, `--purple`, `--award`, `--star`, `--cyan` และแต่ละสีมี variant `-tint/-line/-ink/-press`
- ฟอนต์: heading ใช้ `Plus Jakarta Sans` + `IBM Plex Sans Thai` · body ใช้ `IBM Plex Sans Thai` ก่อน · มี `Mitr` และ `Noto Sans Thai` ติดตั้งไว้ด้วย
- radius: control 10 px, card 16 px, modal 20 px · มีเงา `shadow-card` และ `shadow-pop`

## Shared components

| component | ใช้ทำอะไร |
|---|---|
| `Button` | variant: `primary`, `blue`, `secondary`, `danger`, `ok`, `ghost`, `custom` |
| `IconBtn` | ปุ่มไอคอน (`icon: IconName`) หรือ `variant="custom"` |
| `Icons` (`Icon`) | ชุดไอคอน SVG ตามชื่อ |
| `Modal` | dialog มาตรฐานหรือแบบ inline |
| `Field` | input + label + hint + error |
| `Seg` | segmented control (generic `T`) |
| `Pill` | ป้ายสถานะ (`tone`, `dot`, `icon`) |
| `Banner` | แถบแจ้ง info/warn/err พร้อม actions |
| `Toast` | toast ที่มีปุ่ม action ได้ เช่น "เลิกทำ" |
| `Bar`, `TimedProgressBar` | progress |
| `AppBar`, `WindowTitlebar` (250), `WindowControlsSlot` | แถบบนและปุ่มหน้าต่างของแอป frameless |
| `RobotChip` | ชิปแสดงชื่อและสถานะหุ่นบน title bar |
| `ConnectionMascot`, `SchoolBotMascot` | มาสคอต (ห้ามใช้ตัวอักษร SB แทน) |
| `ScenarioBar` | แถบสลับ sample scenario |
| `SearchBox`, `Stars`, `TagKind` (`arm`/`block`/`quiz`), `RichText` (ข้อความ i18n ที่มี tag), `ArmFigure` | อื่นๆ |

## Mode 3 Block Programming (= "Block Lab" ปัจจุบัน)

- `modes/block-programming/BlockMode.tsx` (281) + `components/block-programming/BlockWorkspacePanel.tsx`, `BlockResultPanel.tsx`
- มี Blockly 13.4 (`blockly/core`) กับบล็อก custom 3 ตัวคือ `arm_move`, `arm_jump`, `arm_repeat` อยู่ใน flyout toolbox ไม่มีหมวด · ใช้ renderer ค่าเริ่มต้น · media อยู่ที่ `./blockly-media/`
- `compile()` แปลงบล็อกเป็น `Command[]` ในเครื่อง ได้ไม่เกิน 64 คำสั่ง, ซ้อนไม่เกิน 8 ชั้น, fuel 1000 แล้วส่ง `client.run("block-programming","program.run",{commands})` ไป demo-engine ผลลัพธ์แสดงเป็น JSON
- save/load ใช้ `{schema: 1, workspace: Blockly.serialization.workspaces.save(...)}` (JSON serialization ไม่ใช่ XML `.mix`)
- แปลข้อความบล็อกด้วยการ `setValue` ทับ FieldLabel ทุกครั้งที่สร้างบล็อก (`localizeWorkspace`) ส่วน locale ของ Blockly ตั้งจาก `blockly/msg/{th,en,zh-hans}`
- หนี้ที่เห็น: error ใน `compile()` เป็นข้อความอังกฤษ hardcode (`"Invalid movement"` ฯลฯ) และแสดงใน status โดยตรง · ใน `.tsx` มี class string ยาวที่ประกอบด้วย `join` (ยังถือว่าอยู่ใน TSX)
- สรุป: แทบทั้งหมดต้องทำใหม่ตาม mockup Block Lab สิ่งที่ใช้ต่อได้คือ pattern การ inject/cleanup Blockly ใน `useEffect`, การใช้ client save/load และตำแหน่งใน nav

## Mode 2 Coding Adventure

- `modes/coding-adventure/*` (LessonsMode, LearnMap 313, CodeLabMode, useCodeLab, useLessonMode) + `components/coding-adventure/*` (LessonMap 347, LessonDetails 373, GameCanvas ที่ใช้ Phaser)
- DSL แบบจำกัด (`hero.move_right(2)`, `hero.jump()`) parse ที่ demo-engine (`internal/engine/demo.go:65 ParseSource`) ไม่มีการ `eval`

## Mode 1 Arm Lab (`modes/robot-control/`)

- `RobotMode.tsx`, `robotContext.tsx`, `engineAdapter.ts` (ทุกคำสั่งไป zide ผ่านไฟล์นี้), `engineErrorText.ts` (แปลง error code เป็น key)
- `connect/*` คือ flow ConnectV3: ค้นอุปกรณ์ → probe → ยืนยันโปรไฟล์ 6 ข้อ → reconnect overlay · มีทั้งแบบ native (`NativeConnectScreen/View`) และแบบ sample (`ConnectScreen/View`) **mockup "เชื่อม Uno" ของ Block Lab ยกโครงมาจากชุดนี้**
- `workspace/*` (91 ไฟล์) คือหน้าจัดท่า/เล่นลำดับ: `WorkspaceLogic.ts` (660, เกิน 500) + ไฟล์ `*View.ts`, `*Actions.ts`, `*Fields.ts` ตาม pattern view-model ใน AGENTS.md · `NativeWorkspace.tsx` ทำ motion แบบ serialize, หยุดแบบ drain ก่อน แล้วกัน callback เก่าส่งคำสั่งต่อ
- `simulator/*`, `teacher/TeacherMenu.tsx` (483), `works/*` (หน้ารายการงาน)
- tests: `frontend-adapter.test.mjs`, `frontend-workspace.test.mjs`, `frontend-sample-workspace.test.mjs`
- ความเห็น i18n test: `workspaceState.ts`, `WorkspaceLogic.ts`, `ArmWorkspace.tsx` ระบุว่า "workspace is being rewritten" ส่วนนี้มีคนอื่นกำลังแก้อยู่ จึงไม่ควรแตะถ้าไม่ใช่งานของเรา

## สิ่งที่ Block Lab ใหม่ต้องเสียบเข้า

1. `appData.tsx` (lazy import + nav item) และ `AppView.tsx` (layout: mockup ใช้พื้นที่ 1440×840 + แถบหน้าต่าง 60 จึงน่าจะต้องเป็นเต็มจอแบบ robot-control)
2. `core/types.ts` (`ModeID` เดิมใช้ต่อได้) + payload/response types ของ mixly
3. `core/client.ts` (`run/cancel/save/load`) + `core/preview.ts` (ต้องจำลอง action ของ mixly ให้ preview ใช้ได้)
4. i18n namespace `blocks.*` ที่มีอยู่แล้ว
5. close guard (`useWindowClose`) ถ้ามีงานที่ยังไม่เซฟหรือกำลัง upload
