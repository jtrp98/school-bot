# แผน Block Lab เดิม (2026-10-27) — สรุปและการแมปเข้า school-bot

> แหล่ง: https://claude.ai/artifact/RQuHLhmr28fNWhGApeR9eq · สรุปเมื่อ 2026-11-02 · อ่านต้นฉบับซ้ำก่อนใช้ตัดสินใจ

แผนต้นฉบับเขียนสำหรับ repo Electron `C:\src\sb-app-mixly` (Mixly 2, Blockly 6.20210701, Electron 19) ไม่ได้เขียนสำหรับ school-bot
path ในแผนทั้งหมดอิง `resources/app/src/` ของ sb-app-mixly (ส่วนใหญ่อยู่ใต้ `board/common/mixly/common/`) เลขบรรทัดอิงชุด Downloads ซึ่งแผนระบุว่าตรงกับ repo ยกเว้น `main.js:98`
เอกสารมีแผนภาพฝังหนึ่งรูป "เส้นทางจาก login ถึงบอร์ด · กล่องสี = จุดที่ต้องแก้" (`node/4e6b684d-65b8`) ซึ่งไม่อยู่ในข้อความที่สรุปนี้

## เป้าหมายและขอบเขต

เป้าหมาย: ทำให้ Mixly เป็นโปรแกรมเขียนบล็อกที่เด็กใช้ง่าย สำหรับ **Arduino UNO อย่างเดียว** ด้วยการปรับ UX, ออกแบบ UI ใหม่ และซ่อนฟังก์ชันที่ไม่ใช้ โดย **ไม่ลบโค้ดเดิม**

ขอบเขต
- บอร์ด: Arduino UNO (`board/arduino_avr`) + lib ZLTech (`libraries/ThirdParty/001-Lib_ZLTech_UNO_AR_V2.0.9`)
- บล็อก: ตามชีต "[School Bot] Fuction" แท็บ Mixly — ใช้ 120 · ใช้+แก้ข้อความ 11 · รอเคลียร์ 11 · ซ่อน 139 (นับเป็นแถว)
- กติกาบล็อกใหม่: "เริ่มต้น" = `setup()`, "วนซ้ำ" = `loop()` บล็อกที่ไม่ได้ต่อกับสองก้อนนี้ไม่ทำงาน
- UI: ตาม mockup "School Bot – Block Lab"
- นอกขอบเขต: ESP32, ESP8266, MicroPython, Python และแท็บ Zide / Block Mixly ในชีต

หลักการที่ทุกงานต้องทำตาม
- **ซ่อน ไม่ลบ**: ไฟล์บล็อก ตัวสร้างโค้ด บอร์ดอื่น และไลบรารีอยู่ครบ ซ่อนผ่านรายการตั้งค่าที่เดียว เปิดกลับได้ด้วยการแก้รายการ
- **ไฟล์เก่าต้องเปิดได้**: `.mix` ที่มีบล็อกที่ถูกซ่อนต้องโหลดและสร้างโค้ดได้
- **แก้ข้อความด้วย key ใหม่**: key ภาษาที่ใช้ร่วมกับบล็อกอื่นห้ามแก้ค่าเดิมตรงๆ
- **ทุกขั้นต้องตรวจได้**: มีชุดตรวจอัตโนมัติ + ทดสอบบน UNO จริงก่อนปิดงาน
- **ฐานโค้ด**: ทำบน `C:\src\sb-app-mixly` หลังรวมงานภาษาไทยจากชุด Downloads เสร็จแล้วเท่านั้น

ภาพรวมระบบตามแผน: เข้าระบบ → หน้าบอร์ด → สร้าง toolbox → พื้นที่ต่อบล็อก → สร้างโค้ด → คอมไพล์ → บอร์ด
- toolbox อ่านจาก XML ใน `board/arduino_avr/index.html:66-3688` + `ZLTech.xml`
- บล็อก 1 ตัว = หน้าตา (`blocks/*.js`) + ข้อความ (`msg/th.js`) + ตัวสร้างโค้ด (`generators/*.js`) + รายการใน toolbox
- "ตะกร้าโค้ด" = `definitions_` (หัวไฟล์), `setups_` (setup), ค่าที่คืน (loop) ประกอบใน `blockly-core/arduino.js` คีย์ซ้ำเขียนทับกัน โค้ดจึงไม่ซ้ำ
- Blockly 6.20210701 มี renderer `zelos`, `Blockly.Theme`, `maxInstances` อยู่แล้ว

## ลำดับเฟส

| เฟส | งาน | ขึ้นกับ | เกณฑ์ผ่าน |
| --- | --- | --- | --- |
| 0 เตรียมฐาน | F1 รวมภาษาไทยแล้ว commit · F2 นำ lib ZLTech เข้า repo + แก้ `.gitignore` · ทำให้แอปรันจาก repo ได้ (คัดลอก `Mixly2.exe`, `node_modules`, `arduino-cli`, `mixpyBuild` จากชุด Downloads) · สร้าง harness + เก็บ baseline T0 ก่อนแก้โค้ดใดๆ | — | T0 เก็บแล้ว, F1 commit แล้ว |
| 1 งานอิสระ (ทำพร้อมกันได้) | A เหลือ UNO · B รายการซ่อน · C แก้ข้อความ (เฉพาะแถวที่ตัดสินใจแล้ว) | เฟส 0 | A → T6 · B → T1, T5 · C → T3 |
| 2 เริ่มต้น/วนซ้ำ | D | baseline T0 | T2, T4, T5 ตรง baseline |
| 3 UI ตาม mockup | E1 เริ่มจาก prototype ธีม + renderer `zelos` แล้วค่อยแถบหมวดและแผงล่าง | D + คำตอบเรื่องหมวด | T7 |
| 4 งานเล็กจากโน้ต | F3 เตือนเซฟ และหลายหน้าต่าง | คำตอบคำถาม F3 | T9 |
| 5 บอร์ดจริง | T8 + ให้ครูลองใช้ | ทุกเฟส | T8 |

E2 (ตัวจำลอง, เลือกอุปกรณ์ตามชื่อ, stage) เป็นโครงการแยก ไม่อยู่ในลำดับนี้ แผนระบุว่าถ้าเริ่มผ่าน pipeline ของ workspace ให้เริ่มที่ `business-analyst` โดยใช้เอกสารนี้เป็นข้อมูลตั้งต้น

## งานแต่ละชุด

### งาน A: จำกัดให้เหลือ Arduino UNO

ปิดทางเข้าบอร์ดอื่น 4 จุด และให้ login พาเข้าหน้าต่อบล็อก UNO ตรง โดยไม่ลบโฟลเดอร์บอร์ดใด

| จุด | ไฟล์ | วิธีแก้ |
| --- | --- | --- |
| การ์ดบอร์ดหน้าแรก | `boards.json:2-41` | ตั้ง `env.electron:false` ให้ ESP32, Python ESP32, ESP8266 (ไม่ลบรายการ) · ห้ามใช้ `USER.boardIgnore` เพราะปุ่มรีเซ็ตล้างได้ |
| บอร์ด ThirdParty (RP2040, STM32) | `mixly-sw/common/board-manager.js:1010-1032` | ครอบ loop ด้วย flag ปิด |
| บอร์ดคลาวด์ + การ์ด "เพิ่ม" | `sw-config.json:3-5`, `board-manager.js:928, 1033-1045` | ปิด URL คลาวด์ · การ์ดเพิ่มมีปุ่มธีม/ตั้งค่า/รีเซ็ต ต้องย้ายปุ่มก่อนซ่อน |
| dropdown รุ่นบอร์ด | `board/arduino_avr/config.json:2-55` | เหลือรายการเดียว `"Arduino/Genuino Uno": "arduino:avr:uno"` (ย้ายรุ่นอื่นไปไฟล์สำรอง) · บล็อกขาของ Nano/Mega หายเองผ่าน `m-show` |

ข้ามหน้าเลือกบอร์ด
- `login/login.html:362` ส่งไป `../index.html` ให้เปลี่ยนเป็น `../board/arduino_avr/index.html?thirdPartyBoard=false&boardIndex=./board/arduino_avr/index.html&boardType=Arduino%20AVR&boardImg=./board/arduino_avr/media/uno_compressed.png`
- ต้องมี `boardIndex` เสมอ ไม่งั้น error ที่ `editor.js:86` และ `boards.js:172-177`
- ปุ่มกลับหน้าแรก (`nav.js:791` → `interface.js:203`, `electron/loader.js:29`) ต้องชี้ไป login หรือซ่อน
- ดับเบิลคลิก `.mix`: `main.js:210-211` ส่ง `open-file` แต่มีแค่หน้าเลือกบอร์ดที่รับ (`mixly-sw/common/events.js:57-110`) ตอนนี้เสียอยู่แล้ว ต้องต่อให้หน้า UNO รับ
- `.mix` ที่บันทึกเป็น Nano ยังเปิดได้ เพราะ `boards.js:194-195` ข้ามรุ่นที่ไม่มีในรายการ

เกณฑ์/เทสต์: T6 (ด้วยมือ)

### งาน B: ซ่อนบล็อกตามชีต โดยไม่ลบโค้ด

รายการซ่อนอยู่ที่เดียวใน `board/arduino_avr/config.json` แล้วกรอง toolbox ทุกครั้งที่สร้างใหม่ ไม่แก้ `index.html` หรือ `ZLTech.xml` ทีละ tag
เหตุผล: toolbox ถูกสร้างใหม่จาก `Env.defaultXML` ทุกครั้งใน `Boards.updateCategories` (`boards.js:438-478`) ทั้งตอนเปิดหน้า เปลี่ยนรุ่น เปิด `.mix` และโหลด lib ZLTech จุดเดียวจึงครอบคลุมทั้ง Mixly และ ZLTech

ขั้นตอน
- เพิ่ม `"toolboxHide": {"categories": [...], "blocks": [...]}` ใน `config.json` (อ่านเป็น `BOARD.toolboxHide` จาก `mixly.js:402`)
- ใน `Boards.selectCategories` (`boards.js:480-496`) ก่อน `return xmlDom.html()`: ลบ `category` ที่ id อยู่ในรายการ · ลบเฉพาะ `category > block` (ลูกตรง) ที่ type อยู่ในรายการ ห้ามแตะบล็อกใน `value` / `statement` / `shadow` · ตัดหมวดที่ว่างหลังกรอง (ไม่มี block, category, label และไม่มี `custom`) เพราะ Blockly ไม่ตัดให้
- รายการจากชีต (คอลัมน์ "ตัดสินใจ = ซ่อน" ในไฟล์ mapping)
  - ซ่อนทั้งหมวด: `catSensor`, `catActuator_voice`, `cat4Digitdisplay`, `catLCD`, `catTFT`, `catMatrix`, `catI2C`, `catSPI`, `catRFID`, `catStorageSD`, `catStorageEEPROM`, `catBlynk`, `ZLTech_SYS`, `ZLTech_DIS`, `mixly_Sentry2` (+ `catIR`, `catFactory`, `catTools`, `catNetwork` ถ้าตัดสินใจซ่อน)
  - ซ่อนรายบล็อก: ที่เหลือในหมวด IN/OUT, Control, Math, Logic, Text, Serial, motor, pioneer, บัส, อินพุต, เอาต์พุต
- เก็บไฟล์บล็อกและ `<script>` ทุกตัวใน `index.html:21-60` และ `ZLTech.xml:25-26` ไว้ ไฟล์ `.mix` ที่มีบล็อกที่ซ่อนจึงยังเปิดได้ (ตรวจที่ `mfile.js:348-355`)

วิธีที่ตัดทิ้ง: `m-show` / `m-hide` (ผูกรุ่นบอร์ด และต้องแก้ ~139 tag ใน 2 ไฟล์) · `hidden="true"` (ยังค้นเจอใน `toolbox-searcher.js:63-68` และทิ้งหมวดแม่ว่าง)

ข้อควรระวัง
- หมวด Variable / Function สร้างบล็อกด้วยฟังก์ชัน (`variables.js:103`, `procedures.js:184`) ซ่อนรายบล็อกไม่ได้ (ชีตใช้ทั้งหมด จึงไม่กระทบ)
- lib ZLTech ไม่เข้า git: `resources/app/src/.gitignore:4,8` ไม่เก็บ `board/**/libraries/ThirdParty/` และ `board/ThirdParty/`
- ZLTech โหลดตอน runtime ในโหมด Electron เท่านั้น (`boards.js:452-464`, `lib-manager.js:72-250`)

เกณฑ์/เทสต์: T1, T5

### งาน C: แก้ข้อความบล็อก

ส่วนใหญ่แก้ใน `board/common/msg/th.js` อย่างเดียว ต้องแก้ไฟล์บล็อกใน 3 กรณี: คำที่ใช้ร่วมเยอะ ข้อความเขียนตายตัวในโค้ด และคำต่อท้ายบล็อก

กติกา
- key ที่ใช้เฉพาะบล็อกเป้าหมาย แก้ใน `th.js` ได้เลย
- key ที่ใช้ร่วม ให้สร้าง key ใหม่ขึ้นต้น `SB_` ในหมวด "School Bot" ท้าย `th.js` (และ `en.js`) แล้วให้บล็อกใช้ key ใหม่ ไม่ลบ key เดิม
- ห้ามเปลี่ยนชื่อ field/input หรือค่า dropdown เช่น `"HIGH"`/`"LOW"` เพราะ generator และ `.mix` เก่าอ่านค่าเหล่านี้
- `....` ในชีต = ช่องค่าที่มีอยู่แล้ว ไม่เพิ่มช่องใหม่

รายบล็อก (สถานะ "ใช้+แก้ข้อความ")

| แถว | บล็อก | แก้ด้วย | key | ผลข้างเคียง |
| --- | --- | --- | --- | --- |
| 2 | `inout_highlow` | th.js | `MIXLY_HIGH` / `MIXLY_LOW` → "HIGH (1)" / "LOW (0)" | `inout_pulseIn`, `inout_pulseIn2` (ซ่อนอยู่แล้ว) |
| 3 | `inout_digital_write2` | th.js + key ใหม่ | `MIXLY_DIGITALWRITE_PIN` → "เขียนสัญญาณดิจิทัล ขา" · `MIXLY_STAT` (ใช้ 21 จุด) → `SB_SET_TO` = "เป็น" | บล็อก Blynk 1 ตัว (ซ่อน) |
| 4 | `inout_digital_read2` | th.js | `MIXLY_DIGITALREAD_PIN` | `inout_digital_read` (ไม่อยู่ใน toolbox) |
| 5 | `inout_analog_write` | th.js + key ใหม่ | `MIXLY_ANALOGWRITE_PIN` · `MIXLY_VALUE2` (ใช้ 10 บล็อก) → `SB_SET_TO` | บล็อก Blynk 1 ตัว (ซ่อน) |
| 6 | `inout_analog_read` | th.js | `MIXLY_ANALOGREAD_PIN` | ไม่มี |
| 7 | `inout_soft_analog_write` | th.js + key ใหม่ | `MIXLY_SOFT_ANALOGWRITE_PIN` · `MIXLY_VALUE2` → `SB_SET_TO` | ไม่มี |
| 9 | `OneButton_interrupt` | ไฟล์บล็อก | `blocks/inout.js:154` ต่อ `ONEBUTTON + " " + MIXLY_PIN` → `SB_PIN` = "ขา" (ไม่มี #) · `MIXLY_ELECLEVEL` → "เป็น" | `MIXLY_PIN` ใช้ ~100 บล็อก ห้ามแก้ค่าเดิม |
| 25 | `controls_for` | th.js | `LANG_CONTROLS_FOR_INPUT_WITH` → "กำหนดให้" | ไม่มีใน UNO |
| 52 | `base_map` | th.js | `MIXLY_MAP` → "เทียบ" | `,` และ `]` เขียนตายตัวใน `blocks/math.js:315-324` |
| 83 | `serial_begin` | th.js | `MIXLY_SERIAL_BEGIN` → "ความเร็วส่งข้อมูล" | `gps_init` (ซ่อน) |
| 84 | `serial_write` | th.js | `MIXLY_SERIAL_WRITE` → "ส่งออกข้อมูล" | ต้องคง dropdown เลือก Serial |

กรณีแก้ไฟล์บล็อก (แผนให้เก็บบรรทัดเดิมเป็น comment เหนือบรรทัดใหม่ — ขัดกับ AGENTS.md ของ school-bot ถ้าทำในฝั่ง school-bot)
- คำต่อท้าย "HIGH-LOW หรือ 1-0" ในแถว 3 และ 9: แนะนำแก้ที่ `inout_highlow` ที่เดียว แทนเพิ่ม dummy input
- ลบ # จาก "ขา #": `MIXLY_PIN` ใช้ ~100 บล็อก ต้องย้ายบล็อกที่ต้องการไปใช้ `SB_PIN` ทีละตัว หรือเปลี่ยนทั้งแอป
- บล็อก ZLTech (แถว 224–232 รอเคลียร์): ข้อความตายตัวแบบ `MixlyT("…-DG","ปุ่ม-DG")` ใน `ZLTech.js` บรรทัด 271–431 · "ขาดิจิทัล" มี key `MIXLY_DIGITAL_PIN` แล้ว (`th.js:3220`)

เกณฑ์/เทสต์: T3

### งาน D: เริ่มต้น / วนซ้ำ และบล็อกที่ไม่ได้ต่อ

แก้ 4 ชั้น: บล็อกใหม่ ตัวประกอบโค้ด ตัวตรวจบล็อกลอย และตัวแปลงไฟล์เก่า (Blockly 6 มี `maxInstances`, `zelos`, `Blockly.Theme`, `Events.disableOrphans`, `FINISHED_LOADING` แต่ Mixly ยังไม่ใช้)

- **D1 บล็อก "วนซ้ำ" ใหม่** (เช่น type `sb_loop`): นิยามใน `board/arduino_avr/blocks/control.js` แบบ `base_setup` (บรรทัด 10-25) มีช่อง `DO` ไม่มีรอยต่อบน/ล่าง · generator ใน `generators/control.js` เก็บ `statementToCode(this,'DO')` ลงตะกร้าใหม่ `loop_main_` แล้วคืน `''` · ต้องมีทั้งบล็อกและ generator ไม่งั้น `.mix` เตือน "บล็อกไม่รู้จัก" (`mfile.js:348-355`) · ข้อความ key ใหม่ `SB_LOOP` = "วนซ้ำ" + ป้ายรอง "ทำไปเรื่อยๆ" (เริ่มต้นใช้ `MIXLY_SETUP` ที่ `th.js:901` + "ทำครั้งเดียว")
- **D2 ตัวประกอบโค้ด** (`board/common/blockly-core/arduino.js`): `init` (625-653) รีเซ็ต `loop_main_` · `finish` (660-737) ตอนนี้เอาโค้ดบนสุดทุกกองไปใส่ `void loop()` (724-728) ให้เปลี่ยนเป็น `loops_begin_ + loop_main_ + loops_end_` · ย้ายตัวกำจัด `timer.run();` ซ้ำ (664-671) มาใช้กับ loop ใหม่ · ทำเป็นสวิตช์เดียว `BOARD.loopMode = "container"` ปิดแล้วกลับพฤติกรรมเดิม
- **D3 บล็อกหัวที่คืนโค้ด loop เอง** ต้องย้ายไป `loops_end_`:

  | บล็อก | ไฟล์ | โค้ด | สถานะในชีต |
  | --- | --- | --- | --- |
  | `OneButton_interrupt` | `generators/inout.js:111-123` | `buttonN.tick();` | ใช้+แก้ข้อความ |
  | `simple_timer` | `generators/control.js:188-199` | `timer.run();` | ซ่อน |
  | `IICSCAN` | `generators/tools.js:33-65` | โค้ดสแกน I2C | ไม่อยู่ในชีต (Tools) |

- **D4 บล็อกลอย = ไม่ทำงาน**: `Events.disableOrphans` ไม่พอ (ไม่ปิดบล็อกที่ไม่มีรอยต่อบน ~47 ตัวใน `arduino_avr`) · เขียน listener แบบ allowlist ราก: `base_setup`, `sb_loop`, `procedures_defnoreturn`, `procedures_defreturn` + บล็อกหัวที่ทีมเลือกเก็บ · รากอื่น → `setEnabled(false)` ทั้งสาย (Blockly ข้ามแค่ตัวที่ปิดแต่ยังสร้างโค้ดตัวถัดไป, Core:971) · ลงทะเบียนก่อน listener เดิมที่ `editor.js:63` และเช็ก `e && e.type` เพราะ `editor.js:431-434` ส่งฟังก์ชันมาแทน event · หน้าตาเส้นประทำใน E · สถานะปิดถูกบันทึกเป็น `disabled="true"` ต้องคำนวณใหม่ทุกครั้งที่เปิดไฟล์
- **D5 อย่างละ 1 ก้อน + วางให้ + ลบไม่ได้**: `maxInstances: {base_setup: 1, sb_loop: 1}` ใน `Blockly.inject` (`editor.js:132-158`) · วางสองก้อนที่ `File.newFile` (`electron/file.js:218-260`) และตอนเปิดครั้งแรก · `setDeletable(false)` · แถบหมวดแสดง "วางแล้ว" (E)
- **D6 แปลงไฟล์เก่า** (`.mix` เดิมและตัวอย่าง 432 ไฟล์มีบล็อกลอย): หลังโหลด ถ้าไม่มี `sb_loop` ให้สร้างแล้วย้ายกองบล็อกคำสั่งที่ลอยเข้าไปตามลำดับ `getTopBlocks(true)` โค้ดจึงเหมือนเดิม · เรียกทันทีหลัง `domToWorkspace` ใน event group เดียวที่ `mfile.js:357`, `blockly_helper.js:101`, `nav-events.js:47-49` · ไม่เขียนทับไฟล์จนผู้ใช้กดบันทึก · ระวังไฟล์ที่มี `<code>` แนบจะสลับไปหน้าโค้ดเองถ้าโค้ดต่าง (`mfile.js:360-366`)

เกณฑ์/เทสต์: T2, T4, T5 ต้องตรง baseline

### งาน E: UI ตาม mockup

mockup ใหญ่กว่าการปรับ Mixly มาก จึงแบ่ง E1 (ทำด้วย Blockly ที่มี) และ E2 (ฟีเจอร์ใหม่ ต้องตัดสินใจแยก) อ้างอิงหน้า Spec และ Main-A2 / SCR-16 ใน mockup

E1
- แถบหมวด 16 หมวด 4 กลุ่ม (พื้นฐาน · ข้อมูล · อุปกรณ์ · อื่นๆ) กว้าง 112 px แต่ละหมวดมีชื่อ + ไอคอน + สี
- รูปทรง: คำสั่ง = มุมมน, ค่า = มน, เงื่อนไข = หกเหลี่ยม → renderer `zelos` (Mixly ใช้ `geras`)
- สีสดตามหมวด + ตัวอักษรบนบล็อก #14243D ทุกหมวด ด้วย `Blockly.Theme` (สีและ contrast เป็นค่าชั่วคราวตาม mockup)
- เริ่มต้น / วนซ้ำ วางให้ ลบไม่ได้ อย่างละ 1 แถบหมวดแสดง "วางแล้ว"
- บล็อกลอย: พื้นเรียบ ขอบประ ไอคอน + ป้าย "ยังไม่ได้ต่อ · ไม่ทำงาน" (#5A6377 บน #F4F6F8) แทนสีเทาจาง
- เงื่อนไขที่ไม่ได้เสียบช่อง = ไม่ทำงาน · ช่องค่าว่าง = ไม่ให้ส่งลงบอร์ด + ไฮไลต์ช่อง
- พื้นที่ต่อบล็อก: ตารางจุด + ปุ่ม เลิกทำ / ทำซ้ำ / ซูม / พอดีจอ
- ลิ้นชักล่าง 2 แท็บ: Output (ข้อความจากบอร์ด + ช่องพิมพ์ส่งเข้าบอร์ด) และโค้ด Arduino C++
- แถบบน: บอร์ด Uno · COM · สถานะเชื่อมต่อ, ชื่องาน, สถานะบันทึก, ปุ่ม "ส่งลงบอร์ด" + ปุ่มหยุด
- ลาก-วางไม่กะพริบ เคารพ reduced-motion ทำได้ด้วยคีย์บอร์ด เป้ากด ≥ 44 px

E2: ตัวจำลองบอร์ด + ต่อสาย breadboard (SCR-17), stage ตัวละคร (SCR-18), บล็อกเลือกอุปกรณ์ตามชื่อแทนเลขขา ("เปิด LED แดง 1"), ผลตรวจก่อนรันแบบเต็ม (ต่อสายไม่ครบ วงจรอันตราย), มาสคอต 10 ท่า, เสียงตอนวางบล็อก

จับคู่หมวด Mixly → 16 หมวด mockup (ข้อเสนอ รอยืนยัน)

| หมวด mockup | สีพื้น / ขอบ | บล็อกจาก Mixly | mockup |
| --- | --- | --- | --- |
| เริ่มต้น | #FFD23F / #9C7A00 | `base_setup` + บล็อกวนซ้ำใหม่ | มีบล็อก |
| ควบคุม | #FFAB3D / #A8610A | Control 9 ตัว | มีบล็อก |
| ตรรกะ | #62D4C8 / #137A70 | Logic 4 ตัว | มีบล็อก |
| คณิต | #6CCB5F / #2F7A26 | Math 8 ตัว | มีบล็อก |
| ตัวแปร | #FF8F70 / #B03A1C | Variable ทั้งหมด | มีบล็อก |
| ข้อความ | #B79CFF / #5F3FC2 | Text 12 ตัว | ว่างใน MVP |
| สื่อสาร (Serial) | #8FA6FF / #3D4FC0 | Serial 6 ตัว | มีบล็อก |
| รายการ | #FF86A8 / #B02C57 | Lists 11 ตัว | ว่างใน MVP |
| ฟังก์ชัน | #F08AD3 / #9A2F85 | Function ทั้งหมด | ว่างใน MVP |
| เซอร์โว | #5BAEFF / #1D63B8 | servo 3 ตัว + เซอร์โวของ pioneer | มีบล็อก |
| บัส | #A9B8CC / #4F6482 | อุปกรณ์บัส ZLTech 7 ตัว | ว่างใน MVP |
| เสียง | #D58CF2 / #8239A8 | บัซเซอร์/ลำโพง ZLTech + `ZLKPZAR_BEEP` | มีบล็อก |
| เซนเซอร์ | #5CCBEA / #17789A | บล็อกอินพุต ZLTech 31 ตัว | มีบล็อก |
| แสดงผล | #7FE0A8 / #1F8050 | จอ OLED 20 ตัว | ว่างใน MVP |
| ไฟ | #C9E04A / #6E7F0B | ไฟ RGB 9 ตัว + `yf_led` (`ZLKPZAR_NLED` ซ่อนตามชีต) | มีบล็อก |
| ตัวละคร | #FFBE94 / #A4521C | ไม่มี (E2) | โผล่เมื่อเลือก stage |

ยังไม่มีหมวดรองรับใน mockup: บล็อกขา IN/OUT (เขียน/อ่านสัญญาณ, PWM, OneButton), แขนกล pioneer (IK, กลุ่มท่าทาง, คำสั่ง Serial คอนโทรลเลอร์), รีโมต IR/จอย PS2, มอเตอร์/รีเลย์/พัดลม ZLTech

เกณฑ์/เทสต์: T7

### งาน F: ภาษาไทย, lib ZLTech และโน้ตในชีต

เป็นฐานของทุกงาน ต้องเสร็จก่อน A–E เพราะใน repo `th.js` ไม่มีชื่อบล็อกไทย และไม่มีโฟลเดอร์ ZLTech

- **F1 รวมงานไทยจากชุด Downloads**: `board/common/msg/th.js` ฉบับเต็ม (`Blockly.MIXLY_*` ครบ 1,836 key) · helper `MixlyT` ในบล็อก 5 ไฟล์ + `nav.js` (16 ไฟล์ตาม `_th_work/mixly_th_patch.tar`) · เมนูเปลี่ยนภาษาแบบ dropdown (ตัดสินใจแล้ว) คงหน้า login + ภาษาเริ่มต้น `th` · ตรวจว่าไม่มีบล็อก "ใช้" ที่ข้อความเป็น `undefined` หรือจีน (T3)
- **F2 lib ZLTech**: คัดลอก `board/arduino_avr/libraries/ThirdParty/001-Lib_ZLTech_UNO_AR_V2.0.9/` (~9.9 MB) · แก้ `.gitignore` เช่น `!board/arduino_avr/libraries/ThirdParty/001-Lib_ZLTech_UNO_AR_V2.0.9/` · ไลบรารี C++ `libraries/ZLTech_*` ต้องอยู่ใน path ที่ arduino-cli หาเจอ (T2) · เช็ก license ZLTech และไลบรารีย่อย (เช่น grbl เป็น GPL) ก่อนแจกจ่าย
- **F3 งานจากโน้ตในชีต (แถว 288–289)**

  | เรื่อง | สภาพตอนนี้ | แนวทาง |
  | --- | --- | --- |
  | ล้างงานแล้วไม่เตือนให้เซฟ | `File.newFile` (`electron/file.js:218-260`) ถามแค่ "ล้างเลย / ขอคิดก่อน" ไม่ติดตามว่างานถูกแก้ · เปิดไฟล์/ตัวอย่างทับทันที (`file.js:270`, `example.js:113`) | ตัวติดตาม "ยังไม่บันทึก" แล้วถาม บันทึก / ไม่บันทึก / ยกเลิก · ระวังยกเลิก Save As แล้ว callback ไม่ถูกเรียก (`file.js:68-70`) |
  | อยากได้ 2 หน้าต่าง | Ctrl+Shift+N เปิดได้อยู่แล้ว (`main.js:173-175`) | ต้องแก้ autosave ทับกัน (`blockly_helper.js:34-46`), คอมไพล์ชน `testArduino`/`mixlyBuild` (`arduino-shell.js:208-210, 503-514`), `ipcMain.on('ping')` ซ้ำ (`main.js:246`) · ต้องถามว่า "2 หน้าต่าง" หมายถึงอะไร |
  | ลดบล็อกซ้ำด้วยเงื่อนไข JavaScript | "หยุดโปรแกรม" 2 ตัว (`controls_end_program`, `ZL_STOP_PROGRAM`) และ "เริ่มต้น" อยู่ 2 หมวด | เลือกตัวที่เก็บแล้วซ่อนอีกตัวด้วยรายการของ B · ต้องถามว่า "เงื่อนไข JavaScript" หมายถึงอะไร |

เกณฑ์/เทสต์: F1 → T3 · F2 → T2 · F3 → T9

ภาคผนวกในแผนมีตารางบล็อกตามชีตรายหมวด (ใช้ / ใช้+แก้ข้อความ / รอเคลียร์ / ซ่อน) ที่ list type ครบ เช่น pioneer 25 type และอินพุต ZLTech 22 type — อ่านจากต้นฉบับเมื่อต้องสร้าง toolbox allowlist ไฟล์จับคู่เต็ม `mixly-block-mapping.csv` (288 แถว) สร้างใหม่ได้จากชีต · ไม่มีแถวในชีต: `sensor_encoder_set` และ `ZLKPZAR_ZMp3` · แถว 36 ซ้ำแถว 35

## การทดสอบ

แผนระบุว่า sb-app-mixly ไม่มีเทสต์เลย (มีแค่ start/build) ต้องสร้างเทสต์ขั้นต่ำและเก็บ baseline ก่อนแก้

Harness
- หน้า HTML เล็กที่โหลด Blockly + บล็อก + generator ตามลำดับ `board/arduino_avr/index.html:7-61` รันด้วย Playwright (Chromium) หรือ DevTools ของแอป (Ctrl+Shift+I)
- stub `Mixly.JSFuncs.getPlatform`, `Mixly.Editor.blockEditor`, `Mixly.Config.USER`
- สร้างบล็อกทดสอบจาก XML ใน toolbox เพื่อให้ได้ shadow ครบ
- คอมไพล์ด้วย arduino-cli 0.21.1 + AVR core 1.8.3 จากชุด Downloads, FQBN `arduino:avr:uno` · ต้องใช้ `--config-file` ที่แก้ path เพราะ `arduino-cli.json` เก็บ path OneDrive เก่า

| รหัส | ทดสอบ | ผ่านเมื่อ | วิธี | งาน |
| --- | --- | --- | --- | --- |
| T0 | baseline ก่อนแก้ | มี `.ino` จากตัวอย่าง UNO 221 ไฟล์ + บล็อกที่ใช้ทุกตัว เก็บไว้ | อัตโนมัติ | ฐานของ T2, T5 |
| T1 | toolbox ตรงชีต | บล็อกในแต่ละหมวด = รายการ "ใช้" ในไฟล์ mapping ไม่เกินไม่ขาด · ไม่มีหมวดว่าง | อัตโนมัติ | B |
| T2 | บล็อกที่ใช้คอมไพล์ผ่าน | วางบล็อกคำสั่งใน "วนซ้ำ" / บล็อกค่าในบล็อกพาหะ → สร้างโค้ด → คอมไพล์ผ่านทุกตัว (รวม ZLTech) | อัตโนมัติ | B, C, D, F |
| T3 | ข้อความบนบล็อก | ไม่มี `undefined` ไม่มีอักษรจีน · บล็อกที่แก้ตรงคอลัมน์ C · บล็อกอื่นเหมือน baseline | อัตโนมัติ | C, F |
| T4 | กติกาเริ่มต้น/วนซ้ำ | 7 กรณี: ลอย = ปิด + ไม่มีในโค้ด · ในวนซ้ำ = อยู่ใน `loop()` · ในเริ่มต้น = อยู่ใน `setup()` · ลากเข้า/ออกสลับเปิดปิดทั้งสาย · วางซ้ำไม่ได้ · ลบไม่ได้ · `button.tick()` ยังอยู่ใน loop | อัตโนมัติ | D |
| T5 | ไฟล์เก่าโค้ดเหมือนเดิม | ตัวอย่าง UNO 221 ไฟล์ผ่านตัวแปลง → ตรง T0 (หรือต่างที่อธิบายได้) · ไฟล์ที่มีบล็อกซ่อนยังเปิดและคอมไพล์ได้ | อัตโนมัติ | B, D |
| T6 | เหลือแค่ UNO | login เข้า UNO ตรง · dropdown รุ่นเดียว · ไฟล์ Nano เปิดได้ · ดับเบิลคลิก `.mix` เปิดได้ | ด้วยมือ | A |
| T7 | หน้าตาตาม mockup | checklist Spec: contrast ≥ 4.5:1, 16 หมวด, บล็อกเส้นประ, คีย์บอร์ด, reduced-motion, เป้ากด ≥ 44 px | ด้วยมือ + ภาพหน้าจอ | E |
| T8 | บน UNO จริง | อัปโหลดและทำงานถูก: ไฟกะพริบ, ปุ่มกด, Serial, เซอร์โว, RGB, OLED, เซนเซอร์ ZLTech หมวดละ 1 โปรแกรม | ด้วยมือ | ทุกงาน |
| T9 | เตือนเซฟ / หลายหน้าต่าง | ถามก่อนล้าง/เปิดไฟล์ · ยกเลิก Save As แล้วงานไม่หาย | ด้วยมือ | F |

T1–T5 ต้องผ่านทุกครั้งที่แก้ T6–T9 ทำก่อนส่งครู ถ้าทำ harness ไม่ทันให้บันทึกส่วนที่ตรวจด้วยการอ่านโค้ดอย่างเดียว

## สิ่งที่ตัดสินใจแล้ว

- บอร์ดเดียว: Arduino UNO (`arduino_avr`, FQBN `arduino:avr:uno`) + lib ZLTech · ESP32/ESP8266/MicroPython/Python นอกขอบเขต
- ซ่อน ไม่ลบ: รายการซ่อนที่เดียว (`toolboxHide` ใน `config.json`) กรองใน `Boards.selectCategories` · ไม่ใช้ `USER.boardIgnore`, `m-show`/`m-hide`, `hidden="true"`
- `.mix` เก่าต้องเปิดและสร้างโค้ดได้ · ตัวแปลงไม่เขียนทับไฟล์จนผู้ใช้กดบันทึก
- ข้อความ: key ที่ใช้ร่วมห้ามแก้ค่าเดิม ใช้ key ใหม่ `SB_*` · ห้ามเปลี่ยนชื่อ field หรือค่า dropdown
- เริ่มต้น = `setup()`, วนซ้ำ = `loop()` · บล็อกใหม่แบบ `sb_loop` + ตะกร้า `loop_main_` · สวิตช์ `loopMode` ย้อนกลับได้ · allowlist ราก `base_setup`, `sb_loop`, `procedures_defnoreturn`, `procedures_defreturn` · ปิดทั้งสาย · อย่างละ 1 ก้อน วางให้ ลบไม่ได้
- UI แบ่ง E1/E2 · E2 เป็นโครงการแยก · E1 ใช้ `zelos` + `Blockly.Theme` ตัวอักษร #14243D
- เมนูเปลี่ยนภาษาแบบ dropdown · คง login · ภาษาเริ่มต้น `th`
- ไม่อัปเกรด Blockly 6 / Electron 19 ในรอบนี้ ทำ prototype ก่อน E1 เต็มรูป
- เครื่องมือทดสอบ: arduino-cli 0.21.1 + AVR core 1.8.3 · corpus ตัวอย่าง UNO 221 ไฟล์
- ลำดับ: เฟส 0 ก่อนเสมอ · A–E เริ่มหลัง F1 commit แล้วเท่านั้น

## เรื่องที่ยังต้องตัดสินใจ

แผนระบุ 11 ข้อ ต้องได้คำตอบก่อนเริ่มงานที่เกี่ยวข้อง

| # | คำถาม | บล็อกเฟส/งาน | ตัวเลือก |
| --- | --- | --- | --- |
| 1 | สะกด "เอาต์พุด / อินพุด" (ด) ในชีต ตั้งใจหรือพิมพ์ผิด (คำเดิม "เอาต์พุต / อินพุต") | เฟส 1 · C | ใช้ ด ตามชีต / ใช้ ต ตามเดิม |
| 2 | `[เอาต์พุด / เขียน]` ใช้คำเดียวหรือทั้งสองคำ | เฟส 1 · C | "เอาต์พุด" / "เขียน" / ทั้งสอง |
| 3 | ข้อความในแถวเขียว 135, 171, 224–232 (ทีมกำลังหาคำตอบ) | เฟส 1 · C | รอทีม |
| 4 | บล็อก/หมวดที่ไม่อยู่ในชีต: MP3 บัส (`ZLKPZAR_ZMp3`), Factory (14), Tools (8), เครือข่าย 2 ตัว, อินฟราเรด (แถว 180) | เฟส 1 · B | ซ่อน / เก็บ (รวม `catIR`, `catFactory`, `catTools`, `catNetwork`) |
| 5 | บล็อกทำงานซ้ำ "หยุดโปรแกรม" เก็บตัวไหน | เฟส 1 · B | `controls_end_program` / `ZL_STOP_PROGRAM` |
| 6 | OneButton ให้ลอยได้เหมือนหัวเหตุการณ์ หรือบังคับอยู่ในวนซ้ำ | เฟส 2 · D | ลอยได้ (เพิ่มใน allowlist ราก) / ต้องอยู่ในวนซ้ำ |
| 7 | หมวดที่ mockup บอก "ว่างใน MVP" (ข้อความ, รายการ, ฟังก์ชัน, บัส, แสดงผล) ขัดกับชีตที่เก็บบล็อกไว้ | เฟส 3 · E1 | ตาม mockup (ว่าง) / ตามชีต (มีบล็อก) |
| 8 | หมวดใหม่ที่ mockup ไม่มี: ขา IN/OUT, แขนกล pioneer, รีโมต อยู่หมวดไหน | เฟส 3 · E1 | เพิ่มหมวดใหม่ / รวมเข้าหมวดเดิม |
| 9 | ฟีเจอร์ E2 (ตัวจำลอง, เลือกอุปกรณ์ตามชื่อ, stage) อยู่ในรอบนี้หรือไม่ | E2 (นอกลำดับ) | ในรอบนี้ / โครงการแยก (แผนเอนไปทางแยก) |
| 10 | "2 หน้าต่าง" และ "เงื่อนไข JavaScript ลดบล็อกซ้ำ" หมายถึงอะไร | เฟส 4 · F3 | รอทีมชี้แจง |
| 11 | ชื่อโปรแกรม: license บังคับคงคำว่า "Mixly" หรือใช้แบบ `Mixly4XXX` | E (branding) | ผู้บริหาร/กฎหมายตัดสิน |

ข้อที่แผนเปิดไว้โดยนัย (ไม่อยู่ในรายการ 11 ข้อ)
- การจับคู่หมวด Mixly → 16 หมวด mockup เป็น "ข้อเสนอ รอยืนยัน"
- ลบ # จาก "ขา #" ทีละบล็อกด้วย `SB_PIN` หรือเปลี่ยนทั้งแอป
- คำต่อท้าย "HIGH-LOW หรือ 1-0" แก้ที่ `inout_highlow` ที่เดียว (แผนแนะนำ) หรือเพิ่ม dummy input

## ความเสี่ยง

| ความเสี่ยง | ผลถ้าเกิด | การป้องกัน | ย้อนกลับ |
| --- | --- | --- | --- |
| แก้ key ภาษาที่ใช้ร่วม | บล็อกอื่นเปลี่ยนคำตาม | key ใหม่ `SB_*` · T3 เทียบ baseline | git revert |
| บล็อกหัวที่คืนโค้ด loop เอง | โค้ดหายจาก `loop()` เงียบๆ เช่น OneButton ไม่ทำงาน | ย้ายไป `loops_end_` (D3) · T2, T4 | ปิดสวิตช์ `loopMode` |
| `.mix` เก่า | บล็อกเป็นเส้นประทั้งไฟล์ หรือโค้ดเปลี่ยน | ตัวแปลง D6 · T5 กับ 221 ไฟล์ | ไม่เขียนทับจนกดบันทึก |
| ซ่อนบล็อกผิดตัว | เด็กหาบล็อกที่ครูสอนไม่เจอ | T1 เทียบชีตทุกครั้ง | แก้ `toolboxHide` |
| lib ZLTech ไม่เข้า git | เครื่องอื่นไม่มีบล็อก ZLTech ~79 ตัว | แก้ `.gitignore` (F2) | — |
| ธีมใหม่ถูกทับ | สีกลับเป็น Classic/Dark เมื่อเปลี่ยนธีม Windows | แก้ listener `matchMedia` ใน `common/theme.js:70-77` | ปิดธีมในการตั้งค่า |
| เปลี่ยนเป็น `zelos` | บล็อกช่องเยอะ (IK ของ pioneer) กว้างเกินจอ | ตรวจภาพหน้าจอทุกบล็อก (T7) | `USER.blockRenderer` = `geras` |
| งานรวมไทยค้าง | แก้ซ้อน เกิด conflict | เริ่ม A–E หลัง F1 commit (ตอนสำรวจมีไฟล์ค้าง 11 ไฟล์) | — |
| Blockly 6 / Electron 19 เก่า | บางอย่างใน mockup ทำไม่ได้หรือยาก | prototype ก่อน E1 · ไม่อัปเกรดรอบนี้ | — |

แผนระบุว่าทุกงานปิดกลับได้ด้วยการตั้งค่า ยกเว้นการแก้ข้อความและ UI ที่ย้อนด้วย git

## อินพุตภายนอก

ตรวจในเครื่องเมื่อ 2026-11-02

| อินพุต | อ้างในแผน | สถานะในเครื่อง |
| --- | --- | --- |
| ชีต "[School Bot] Fuction" แท็บ Mixly (288 แถว, คอลัมน์ "ตัดสินใจ", คอลัมน์ C ข้อความใหม่) | ใช่ ไม่มีลิงก์ | ไม่พบไฟล์ใน `C:\Users\jabja\Downloads`, `C:\src\sb-app-mixly`, `C:\src\sb-app-mixly-knowledge` น่าจะเป็นชีตออนไลน์ |
| `mixly-block-mapping.csv` (288 แถว) | ใช่ ("สร้างใหม่ได้จากชีต") | ไม่พบในเครื่อง |
| ไฟล์ Mixly_block_shapes / School Bot Function | แผนไม่ได้อ้างชื่อนี้ | ไม่พบในเครื่อง |
| ชุด Downloads ภาษาไทย | ใช่ (ฐานของ F1, F2, runtime) | `C:\Users\jabja\Downloads\mixly(ver.thai)\mixly\` มี `Mixly2.exe`, `arduino-cli`, `Arduino IDE`, `mixlyBuild`, `mixpyBuild`, `testArduino`, `resources\` · zip ต้นทาง `C:\Users\jabja\Downloads\mixly(ver.thai).zip` (~3.55 GB) |
| `_th_work/mixly_th_patch.tar` | ใช่ (F1) | `C:\Users\jabja\Downloads\mixly(ver.thai)\mixly\_th_work\mixly_th_patch.tar` (มี `src_in.tar`, `backup_original\` ข้างกัน) |
| lib ZLTech | ใช่ (F2) | `C:\Users\jabja\Downloads\mixly(ver.thai)\mixly\resources\app\src\board\arduino_avr\libraries\ThirdParty\001-Lib_ZLTech_UNO_AR_V2.0.9\` (9.9 MB: `block/ZLTech.js`, `generator/ZLTech.js`, `language/`, `media/`, `ZLTech.xml`, `config.json`, `libraries/ZLTech_*` 12 โฟลเดอร์ รวม `ZLTech_grbl_1.1h`) · ไม่มีใน `C:\src\sb-app-mixly` |
| ตัวอย่าง `.mix` | ใช่ (221 UNO / 432 ทั้งหมด) | `C:\src\sb-app-mixly\resources\app\src\sample\Arduino AVR\` = 221 ไฟล์ · ทั้ง `sample\` = 432 ไฟล์ ตรงกับแผน |
| mockup "School Bot – Block Lab" (หน้า Spec, Main-A2, SCR-16/17/18) | ใช่ ไม่มีลิงก์ | Claude Design https://claude.ai/artifact/YYgSHL2aCtVbkDTgBQBzTV (ลิงก์จาก memory ไม่ใช่จากแผน) |
| mockup "School Bot – Block Studio (ประถม)" | ไม่ได้อ้าง | https://claude.ai/artifact/1tXuNM7Q26QfEfv5Legqrh (แขนกล ไม่ใช่ UNO) |

สถานะ `C:\src\sb-app-mixly` ตอนตรวจ: HEAD `3a3142b` มีไฟล์แก้ค้างไม่ commit 11 ไฟล์ (รวม `board/common/msg/th.js`, `nav.js`, `interface.js`, `code.js`, `blocks/{actuator,display,ethernet,text,tools}.js`, `mixly-sw/msg/th.json`, `mixly-sw/templete/setting-div.html`) จึงถือว่า F1 ยังไม่ commit และ F2 ยังไม่ทำ

## ตรวจเลขบรรทัดใน sb-app-mixly

ตรวจ 8 จุดกับ working tree ของ `C:\src\sb-app-mixly\resources\app\src` (รวมไฟล์ที่แก้ค้าง) ตรงทั้ง 8

| # | อ้างอิงในแผน | ผล |
| --- | --- | --- |
| 1 | `boards.js:438-478` `Boards.updateCategories` | ตรง (เริ่ม 438) |
| 2 | `boards.js:480-496` `Boards.selectCategories` / `return xmlDom.html()` | ตรง (480, return ที่ 495) |
| 3 | `blockly-core/arduino.js` `init` 625, `finish` 660, `timer.run();` 664-671, `void loop()` 724 | ตรงทุกจุด |
| 4 | `arduino_avr/blocks/control.js:10` `base_setup` | ตรง |
| 5 | `arduino_avr/generators/inout.js:111-123` `OneButton_interrupt` → `.tick();` | ตรง (111, tick ที่ 118) |
| 6 | `editor.js:63` addChangeListener · `editor.js:132-158` `Blockly.inject` | ตรง (63, inject ที่ 142) |
| 7 | `login/login.html:362` ไป `../index.html` | ตรง (`window.location.href = '../index.html'`) |
| 8 | `msg/th.js:901` `MIXLY_SETUP` = "เริ่มต้น" · `th.js:3220` `MIXLY_DIGITAL_PIN` | ตรง (ในไฟล์ที่แก้ค้าง) |

ตรวจเพิ่ม `mfile.js:348-357, 360-366` ก็ตรงเช่นกัน

## แมปเข้า school-bot

สภาพ school-bot ที่ใช้แมป (ตรวจแบบเร็ว)
- `engines/mixly/scripts/import-source.ps1` copy จาก sb-app-mixly เฉพาะ `board/**/(generators|others)/*.js`, `blocks/*profile.js`, `config.json`, `index*.html`, `blockly-core/{blockly_compressed,arduino,python_compressed,names,variables,procedures}.js`, `LICENSE`, `boards.json` → **ไม่มี** `blocks/*.js` (นิยามบล็อก), `msg/*.js`, toolbox ใช้งานจริง, samples และ ZLTech
- regex `/(generators|others)/` ไม่จับ `generator/ZLTech.js` ของ ZLTech (โฟลเดอร์เอกพจน์) และ lib อยู่นอก repo อยู่แล้ว · engine ยังไม่มีการส่ง libraries path ให้ arduino-cli
- engine actions: `engine.info`, `board.list` (17 families), `board.info`, `toolchain.status`, `device.list`, `project.validate`, `program.generate`, `program.compile`, `program.upload`, `serial.exchange`, `firmware.flash` · รับ `blocks` (JSON) หรือ `xml` (`.mix`) · `transport` ค่าเริ่มต้น `dry-run` · API.md ระบุ modes `block-programming`, `coding-adventure`
- engine รัน `blockly-core/arduino.js` ต้นฉบับ (`finish` ใส่ทุกกองบนสุดใน `void loop()`) และ `src/bootstrap.js` กำหนด `workspace.getTopBlocks` เอง
- host: `app/internal/engine/manager.go:47,67` route `block-programming` ไป demo และอนุญาตแค่ `program.run` · React mode มีแค่ `app/frontend/src/modes/block-programming/BlockMode.tsx` (Blockly 13.4)

ช่อง "ลงที่ไหน": (a) แก้ใน sb-app-mixly แล้ว import · (b) `engines/mixly` · (c) React Block Lab mode · (d) Go host · (e) ไม่ใช้แล้ว

| งาน | ลงที่ไหน | หมายเหตุ |
| --- | --- | --- |
| A การ์ดบอร์ด / ThirdParty / คลาวด์ (`boards.json`, `board-manager.js`, `sw-config.json`) | (e) | school-bot ไม่มีหน้าเลือกบอร์ด Mixly · frontend ส่ง `board: "arduino_avr"`, `platform: "Arduino/Genuino Uno"` คงที่ |
| A dropdown รุ่นบอร์ด (`config.json` เหลือ Uno) | (c) + (d) | ไม่ควรตัด `config.json` ใน engine เพราะ engine ใช้ config ต้นฉบับและ `coding-adventure` ก็ใช้ mixly · จำกัด board/platform ต่อ mode ที่ host · `.mix` ที่บันทึกเป็น Nano ต้องมีนโยบายว่าจะ generate เป็น Uno หรือ platform เดิม |
| A login → หน้า UNO, ปุ่มกลับหน้าแรก | (e) | ใช้ shell/header ของ school-bot |
| A ดับเบิลคลิก `.mix` | (d) | ต้องมี file association + เปิดเข้า Block Lab ใน host/installer ยังไม่มี |
| B รายการซ่อน `toolboxHide` | (c) | toolbox ใน React เป็น allowlist ของบล็อกที่ "ใช้" แทนรายการซ่อน · generator ของบล็อกที่ซ่อนยังอยู่ใน engine (b) `.mix` เก่าจึงยัง generate ได้ · ต้อง port นิยามบล็อก ~131 ตัว (120 + 11) + ZLTech จาก `blocks/*.js` Blockly 6 ไป Blockly 13 โดย field/input name ต้องตรง generator |
| B/F2 ZLTech | (a) + (b) | นำ lib เข้า sb-app-mixly (หรือ import ตรงจากชุด Downloads) · ขยาย `import-source.ps1` ให้รวม `generator/ZLTech.js` และ `libraries/ZLTech_*` · engine ต้องส่ง libraries path ให้ arduino-cli · ตรวจ license (grbl GPL) ก่อนใส่ installer + `THIRD-PARTY-NOTICES.md` |
| C ข้อความบล็อก (`th.js`, `SB_*`, `MixlyT`) | (c) | msg/blocks ไม่ได้ import · ข้อความบล็อกใน React ต้องอยู่ใน i18n `th.json`/`en.json`/`zh.json` ตาม AGENTS.md · ใช้ `th.js` ของ F1 เป็นแหล่งคำแปล · กติกาห้ามเปลี่ยน field/dropdown value ยังจำเป็นเพราะ generator ใน engine อ่านค่าเหล่านี้ |
| D1 บล็อก `sb_loop` | (c) นิยาม + (a) หรือ (b) generator | generator ใหม่ต้องรันใน engine · เลือกว่าเพิ่มใน sb-app-mixly `generators/control.js` แล้ว import หรือเพิ่มไฟล์ School Bot ของ engine เอง (ไม่แก้ `third_party/` ตรง) |
| D2 ตัวประกอบ loop (`arduino.js` `finish`, `loopMode`) | (a) หรือ (b) | engine ใช้ `arduino.js` ต้นฉบับ · ทางเลือก: patch ใน sb-app-mixly แล้ว import หรือห่อใน `src/bootstrap.js` / Go |
| D3 บล็อกหัว (`OneButton_interrupt`, `simple_timer`, `IICSCAN`) | (a) หรือ (b) | เปลี่ยน generator ชุดเดียวกับ D2 |
| D4 บล็อกลอยไม่ทำงาน (allowlist ราก) | (c) + (b) | UI listener + หน้าตาเส้นประใน React (API Blockly 13 ต่างจาก 6) · engine ควรบังคับ allowlist รากซ้ำใน `project.validate`/`program.generate` เพราะรับ `xml`/`blocks` จากภายนอกได้ |
| D5 อย่างละ 1 / วางให้ / ลบไม่ได้ | (c) | workspace options ของ Blockly 13 |
| D6 แปลง `.mix` เก่า | (c) หรือ (d) | ต้องเลือกจุดแปลงตอน import `.mix` เข้า project ของ school-bot (`app/internal/project/migration.go` มีกลไก migration) · engine รับ `xml` ได้อยู่แล้ว |
| E1 UI ตาม mockup | (c) | ใช้ components/tokens School Bright ตาม AGENTS.md · ลิ้นชักโค้ด → `program.generate` · Output → `serial.exchange` · "ส่งลงบอร์ด" → `program.upload` (`transport: "native"`) ผ่าน host · ปุ่มหยุดต้องแยกจาก cancel process |
| E2 ตัวจำลอง / stage / เลือกอุปกรณ์ตามชื่อ | (c) โครงการแยก | sample/preview ต้องไม่ส่งคำสั่งไปอุปกรณ์ |
| F1 รวมไทย (`th.js`, `MixlyT`, เมนูภาษา, login) | (a) บางส่วน + (e) | ใช้เป็นแหล่งคำแปลเท่านั้น · เมนูภาษาและ login ใช้ของ school-bot |
| F3 เตือนเซฟ | (c) + (d) | ใช้ close guards / project save ที่มีอยู่ |
| F3 หลายหน้าต่าง | (e) หรือ (d) | ปัญหา autosave/`ipcMain`/`testArduino` เป็นของ Electron · engine ลบไฟล์ชั่วคราวหลัง compile แล้ว (API.md) · ยังต้องถามความหมาย |
| F3 บล็อกซ้ำ | (c) | เลือกไว้ใน allowlist toolbox |
| T0 / T5 baseline 221 ไฟล์ | (b) | samples ไม่ได้ import · ต้องนำ corpus เข้า `engines/mixly/tests/fixtures` หรืออ้างจาก sb-app-mixly แล้วเทียบผล `program.generate` ก่อน/หลัง D |
| T1 / T3 / T4 / T7 | (c) | T3 คู่กับ `npm run test:i18n` · T4 แบ่งเป็นเทสต์ UI + เทสต์ engine |
| T2 คอมไพล์ | (b) | `program.compile` native + arduino-cli + ZLTech libs · ไม่ต้องใช้ `--config-file` แบบ OneDrive เดิม |
| T8 บอร์ดจริง | (b) + (d) | ต้องมีคำสั่งจากผู้ใช้ก่อนอัปโหลดจริงตาม AGENTS.md |
| Harness HTML + stub `Mixly.*` | (e) | engine รัน generator ใน goja อยู่แล้ว (ยังไม่ได้ตรวจว่า `bootstrap.js` stub ตัวไหนบ้าง) |
| ธีม `matchMedia` / `USER.blockRenderer` / Electron 19 | (e) | ความเสี่ยงฝั่ง Electron ไม่เกิดใน school-bot · renderer เลือกใน React |
