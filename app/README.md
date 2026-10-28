# Desktop application

Go + Wails **v3.0.0-beta.30** / React / TypeScript

Run/build commands: ดู `../README.md` ใช้ `../scripts/build-desktop.ps1`

Host ยังใช้ HTTP API สำหรับ local development. เป้าหมาย online คือ Host→HTTPS Gateway→gRPC SchoolBot ซึ่งยังรอ integration; frontend คงผ่าน core/client.ts และ host ไม่รับหน้าที่เป็น gRPC client. Engine JSON Lines, local storage และ hardware transport คงเดิม ดู `../docs/DEPLOYMENT.md`.

```text
main.go                         Wails window + service registration
desktopservice.go                Native bridge: engine and cloud client
desktopstorage.go                Bounded local project store and migration
internal/engine/                Framework-independent JSON contract + manager
cmd/demo-engine/                Trusted simulator executable
frontend/src/core/              One client shared by all modes
frontend/src/modes/
  robot-control/                Team 1
  coding-adventure/             Lessons and Code Lab (Team 2)
  block-programming/            Team 3
frontend/src/components/
  shared/                      Shared UI, window controls, tokens and styles
  block-programming/           Block Lab workspace and result panels
  coding-adventure/            Lesson views and Phaser playground
  robot-control/               Reusable robot inputs, hold buttons and icons
```

- อย่าเรียก Wails/HTTP/Filesystem จาก Mode ตรง ๆ ให้ใช้ `core/client.ts`
- เมนูหลักเรียง หน้าแรก → บทเรียน → Code Lab → Block Lab → Arm Lab; Code Lab แยกจากบทเรียนและใช้ editor/Phaser เดิม โดยยังรันและบันทึกผ่าน mode `coding-adventure` เพื่อเปิดโค้ดที่บันทึกไว้เดิมได้
- เปิดบทเรียนแล้วเลือก Block Lab, Arm Lab หรือ Code Lab ก่อน; Arm Lab เปิดแผนที่บทเรียนเดิมและกลับมาเปลี่ยนโหมดได้ ส่วน Block/Code แสดงสถานะยังไม่มีบทเรียนแยกตามโหมด
- หน้าบทเรียนใช้พื้นที่ที่เหลือใต้ header โดยตรง; แผนที่ รายการด่าน และรายละเอียดปรับขนาดตามกรอบเมื่อหน้าต่างเล็กลง โดยปุ่มหลักอยู่ด้านล่างเสมอและไม่มี scrollbar ของหน้า
- การรับเหตุการณ์ปิดหน้าต่างและคำสั่งเต็มจอ/ย่อหน้าต่างเรียกผ่าน `core/client.ts` ด้วย โดย component และ hook ยังดูแล close guards, keyboard handlers และ cleanup ของตัวเอง
- `modes/` เก็บ entrypoints, state, workflows และการเรียก engine; component รับข้อมูลและ callbacks ผ่าน typed props
- `frontend/src/app/` และ `frontend/src/home/` เก็บ views, dialogs และข้อมูลประกอบของ App/Home; state และ workflow ยังอยู่ใน hooks ของ `App.tsx` และ `Home.tsx` ส่วนหน้าเชื่อมต่อ งานของฉัน และ simulator แยก view ออกจาก hook ภายใน mode เดิม
- Workspace แยกการสร้าง view data, การแก้ไขท่า, popover และค่าเริ่มต้นออกจาก `WorkspaceLogic.ts`; ตัวควบคุมยังเป็นเจ้าของ lifecycle, timers และ cleanup ส่วน keyboard/selection handlers อยู่ใน `workspaceKeyboardActions.ts` และ `workspaceSelectionActions.ts` การจำลอง playback, glide และ release อยู่ใน `workspaceSampleMotion.ts` โดยเรียกผ่านเมธอดของตัวควบคุมเพื่อรักษา native overrides
- `NativeWorkspace.tsx` เก็บ generation, pending commands, stop และ close guards ร่วมกันเพื่อรักษาลำดับ invalidate/drain/stop; `nativeWorkspaceView.ts` สร้างเฉพาะข้อมูลแสดงผลและผูก handlers คงที่ที่ตัวควบคุมสร้างไว้ การแสดงตำแหน่งยังเป็นค่าคำสั่งหรือค่าที่อ่านล่าสุดตามเดิม
- ตารางท่า การ์ดท่า และแผงข้อต่อใช้ view data จาก `rowsView.ts`, `tilesView.ts`, `jointsView.ts` และ handlers จาก `*Actions.ts` ที่อ่าน state เมื่อถูกเรียก; การเลื่อนและ focus ใช้ callback refs ผ่าน `workspaceDom.ts` ส่วนการปรับ selection/range ให้ตรงกับลำดับท่าอยู่ใน lifecycle ผ่าน `workspaceReconcile.ts`
- `components/shared/` ใช้ร่วมกันและไม่ขึ้นกับ mode; UI เฉพาะหน้า Robot Control อยู่ใน `modes/robot-control/` ส่วนที่ใช้ซ้ำอยู่ใน `components/robot-control/` แบบไฟล์ตรง ไม่มีโฟลเดอร์ย่อย
- ปุ่มกลางใช้ `Button` จาก `components/shared` และเลือกหน้าตาด้วย `variant` เช่น `<Button variant="secondary">...</Button>`; types ของปุ่มคือ `ButtonVariant` และ `ButtonSize`
- หน้าเชื่อมต่อ งานของฉัน เมนูครู และ simulator ใช้ `Button`/`IconBtn` ด้วย `variant="custom"` เมื่อขนาด โครงสร้างลูก หรือสไตล์เฉพาะหน้าไม่ตรงกับ variant มาตรฐาน โดยส่ง classes ใน TSX และไม่เพิ่ม wrapper รอบเนื้อหาปุ่ม; `Modal layout="inline"` รองรับคำยืนยันที่อยู่ใน card หรือ overlay ของหน้าเดิมโดยไม่สร้าง scrim ซ้ำ
- ชุด scenario ของ Home, งานของฉัน, การเชื่อมต่อ, เมนูครู และ workspace อยู่ใน `frontend/src/data/sampleScenarios.ts` โดยแยกชุดตามหน้าและ derive ID จากรายการของหน้านั้น; `frontend/src/data/work.ts` เป็นแหล่ง type กลาง มี `HomeWork` สำหรับข้อมูลสรุปหลาย mode และ `RobotWork` สำหรับข้อมูลท่า/หุ่นที่แก้ไขได้
- มาสคอตอยู่ใน `components/shared/SchoolBotMascot.tsx` และ `ConnectionMascot.tsx` ซึ่งเป็นภาพคนละแบบ; `Bar` แสดงเปอร์เซ็นต์พร้อม semantics ของ progressbar ส่วน `TimedProgressBar` เป็นแถบเวลาของ sample connection ที่หยุดค้างเปอร์เซ็นต์ได้ จึงแยกชื่อและ props
- Frontend ทั้ง Home และ 3 modes ใช้ Tailwind CSS ผ่าน Vite plugin โดยเขียน utility classes ใน TSX โดยตรง เช่น `flex`, `overflow-auto`, `bg-surface` ไม่มี prefix, ไฟล์ CSS แยกตาม component หรือ style map/resolver
- `frontend/src/tailwind.css` เป็น CSS entry เดียว เก็บ Tailwind imports, theme/tokens, base resets และ keyframes โดยไม่เปิด Preflight เพื่อรักษาหน้าตาเดิมและ Blockly/Phaser; imports ของฟอนต์ยังอยู่ใน `fonts.ts` และ `armlab-fonts.ts` เพื่อใช้งานแบบ offline
- เขียน conditional classes ใน `className` ด้วย template strings และเงื่อนไข `? :` โดยตรง ชื่อ selectors เดิมที่จำเป็นยังคงไว้สำหรับ states และ DOM hooks ใช้ built-in variants เช่น `hover:`/`disabled:` และ arbitrary variants เมื่อจำเป็นสำหรับ descendants หรือสถานะเฉพาะ
- ไอคอน UI กลางใช้ `react-icons/md` (Material Design) เป็นหลักผ่าน `components/shared/Icons.tsx`; ใช้ Lucide เฉพาะรูปมงกุฎที่ไม่มีตัวตรงในชุดหลัก โลโก้ มาสคอต และภาพแขนกลยังใช้ assets/rendering เดิม และไม่โหลดไอคอนจาก CDN
- ภาษาของ frontend ใช้ `i18next` + `react-i18next` ตั้งค่าที่ `frontend/src/i18n/index.ts` และเก็บคำแปลแบบ nested keys ใน `frontend/src/i18n/locales/th.json`, `en.json`, `zh.json` เช่น `t("modal.leave.title")`, `t("home.greeting", { name })` ใช้ key เดียวกันครบทุกภาษา ไม่ใช้ข้อความไทยเป็น key
- คำแปลทั้งหมดบรรจุในแอปและใช้งานออฟไลน์ได้ จำภาษาที่เลือกด้วย `school-bot:lang` ใน local storage และ fallback เป็นไทย การเปลี่ยนภาษาไม่ remount mode หรือแก้ชื่องาน ชื่อท่า ชื่อหุ่น โค้ด และไฟล์โครงงานของผู้ใช้ ส่วน diagnostics จากอุปกรณ์/engine และประวัติเดิมแสดงตามข้อความต้นทาง
- งานใน browser preview ใช้ localStorage คีย์ `school-bot:project:<mode>` และไม่อ่านคีย์ `school-bot-poc:`/`arm-poc:` ของ preview เก่า; storage ของแอป desktop ไม่เปลี่ยน
- เมื่อเพิ่มคำแปลให้ใช้ key ที่สื่อความหมายตามหมวด `nav`, `modal`, `home`, `lessons`, `blocks`, `adventure`, `robot` พร้อม interpolation แทนการต่อข้อความกับข้อมูลผู้ใช้ ตรวจด้วย `npm run test:i18n` จาก `frontend` ซึ่งตรวจ key, placeholders, การเก็บข้อมูลเมื่อสลับภาษา และ render ของ workspace/บทเรียนครบทั้งสามภาษา และสแกนว่าไม่มีอักษรไทย/จีนใน source นอกไฟล์ข้อมูลตัวอย่างที่ระบุใน `tests/i18n.test.mjs` กับไม่มีการต่อข้อความแปลด้วย `+`
- ใช้ tokens เช่น `bg-surface`, `font-body` และค่า px เพื่อรักษาขนาดเดิม (root font-size เป็น 14px); ไม่ประกอบชื่อ utility แบบ dynamic ค่าที่คำนวณขณะใช้งาน เช่น progress, ตำแหน่ง และ workspace state ยังใช้ inline styles ได้ รวมถึง rendering helper ที่ต้องรักษาลำดับ shorthand/longhand และ `!important`
- สีของ UI ใช้ semantic tokens ใน `frontend/src/tailwind.css` รวมถึงสีโปร่งใสผ่าน RGB tokens ของสีเดียวกัน ปุ่มควบคุมทิศทาง ท่าพัก เพิ่มท่า อัปโหลด เล่น และหยุดใช้ `Button`/`HoldButton` และ variants กลาง ไม่ประกอบสีปุ่มเป็น CSS strings ซ้ำใน workspace; สีสำหรับภาพวาดมาสคอต แขนกล และ Phaser แยกจากสี UI
- Native `Run` รับ Mode, Action และ JSON เท่านั้น Frontend เลือก executable ไม่ได้
- Host เริ่ม Engine ใหม่ต่อ Request: Robot Control ใช้ Zide; Adventure/Blockly ใช้ Demo Engine (ยังไม่ใช่ persistent engine pool)
- Student DSL รองรับ `hero.move_right(n)`, `hero.move_left(n)`, `hero.jump()` เท่านั้น
- Save Project เป็นหนึ่ง slot ต่อ Mode ใน OS config directory `SchoolBotPoC`
- Device ID สุ่มต่อ local installation และตรวจรูปแบบ hexadecimal ไม่อ่าน Hardware Serial Number
  เก็บไว้ใช้ activate license ภายใน host เท่านั้น; `Info()` ไม่ส่ง Device ID ไป frontend
- Robot adapter/workspace regression tests อยู่ใน `frontend/tests/frontend-*.test.mjs` และใช้ fake bridges; engine IPC tests อยู่ใน `../engines/zide/tests/`
- Engine แยก Process เพื่อ Crash Isolation; ยังไม่มี Sandbox สำหรับโค้ด arbitrary
- `Call.ByName` ใช้ service `main.DesktopService`; ถ้าย้าย Go service package ต้องแก้ client และตรวจ bridge ด้วย

การพัฒนา UI ใช้ Vite preview; หลังแก้ Go หรือทดสอบ Native ให้ build ใหม่ด้วย script
Hot reload แบบ Wails CLI/Taskfile ยังไม่ได้ตั้งค่า; การสร้าง Installer ดู `../docs/INSTALLERS.md`

macOS: `bash ../scripts/build-desktop.sh` สร้าง `bin/darwin-<arch>/School Bot.app`
พร้อม Engine ใน `Contents/MacOS` และ ZIP ที่รักษา executable permissions
ดู `../docs/INSTALLERS.md` สำหรับการแพ็กและขอบเขต signing/verification
ข้อมูลจาก `ArmStudioPoC` ถูกคัดลอกเข้า `SchoolBotPoC` แบบไม่ทับไฟล์ที่มีอยู่แล้ว
