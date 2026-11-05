# sb-app-mixly (Block Lab เดิม, Electron)

> สถานะ: สำรวจแบบอ่านอย่างเดียวเมื่อ 2026-11-02 · repo `C:\src\sb-app-mixly` · origin `Jabjai-Corporation/sb-app-mixly` · branch `main` (มี 2 commit)

## สถานะ git ตอนนี้

- มี **11 ไฟล์ที่แก้แล้วแต่ยังไม่ commit**: `board/arduino_avr/blocks/{actuator,display,ethernet,text,tools}.js`, `board/common/mixly/common/{code,interface,nav}.js`, `board/common/msg/th.js` (361 KB), `mixly-sw/msg/th.json`, `mixly-sw/templete/setting-div.html`
- ดูเหมือนเป็นงาน F1 (merge ภาษาไทยจาก `Downloads\mixly(ver.thai)`) ที่ session อื่นทำค้างไว้ ต้องถามผู้ใช้ว่าจะ commit หรือไม่ก่อน sync เข้า school-bot
- runtime ที่ใหญ่ (`Mixly2.exe`, `arduino-cli/` 4.2 GB, `mixpyBuild/`, `Arduino IDE/`, `node_modules/`) อยู่ใน `.gitignore` (commit `3a3142b`)

## โครงสร้าง (`resources/app/src/`)

| ที่ | หน้าที่ |
|---|---|
| `main.js` | Electron main เปิด `login/index.html` (หน้า login School Bot) |
| `login/` | หน้า login ของ School Bot |
| `mixly-sw/` | หน้าเลือกบอร์ด, setting และ `msg/*.json` · ภาษาเริ่มต้น `th` |
| `board/common/blockly-core/` | Blockly fork ของ Mixly (`blockly_compressed.js`) |
| `board/common/mixly/common/` | `nav.js` (เมนูภาษาแบบ dropdown), `interface.js` (popup), `code.js` |
| `board/common/msg/{en,th,zh-hans,zh-hant}.js` | ข้อความบล็อก `Blockly.MIXLY_*` + `Blockly.Msg.*` |
| `board/arduino_avr/` | `config.json` (board: UNO = `arduino:avr:uno`, Nano, Pro Mini ฯลฯ + burn/upload/serial/lib), `index.html` (**toolbox XML 33 `<category>`**), `blocks/*.js` 19 ไฟล์ (actuator, blynk, communicate, control, display, ethernet, factory, inout, lists, logic, math, pins, procedures, scoop, sensor, serial, storage, text, tools, variables), `generators/`, `media/`, `wiki/` |
| `sample/` | ตัวอย่าง `.mix` 430 ไฟล์ (นับด้วย `find -iname "*.mix"`; แผนเดิมนับรวมได้ 432) โดยอยู่ใน `sample/Arduino AVR/` 221 ไฟล์ |

## สิ่งที่ school-bot ยังไม่มี ถ้าจะทำ Block Lab ใน React

1. block definitions (`board/arduino_avr/blocks/*.js` + common) เขียนด้วย API ของ Blockly fork เดิมและใช้ global `Blockly.MIXLY_*`, `profile`
2. toolbox (หมวด/ลำดับ) อยู่ใน `index.html`
3. ข้อความ msg ทั้ง 4 ภาษา ภาษาไทยมี ~1,800 key
4. media/รูปบล็อก
5. ตัวประกอบ `setup()/loop()` ใช้ generator (ส่วนนี้ engine มีแล้ว)

ทางเลือกในการนำเข้าเป็นเรื่องที่ต้องตัดสินใจ ดู `90-open-questions.md` (Q1)

## License

- Mixly ใช้ Apache-2.0 (`engines/mixly/licenses/MIXLY-LICENSE.txt`) · Blockly ใช้ Apache-2.0 · lib ของ ZLTech ไม่มีไฟล์ license และ lib ย่อยบางตัว (เช่น grbl) เป็น GPL ต้องเช็กก่อนแจกจ่าย
