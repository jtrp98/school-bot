# กฎร่วม School Bot

คำสั่งล่าสุดของผู้ใช้มีลำดับเหนือกฎในไฟล์นี้ อ่าน `README.md` และเอกสารของส่วนที่จะเปลี่ยนก่อนทำงาน หากเอกสารไม่ตรงกับโค้ด ให้ตรวจโค้ดและรายงานข้อแตกต่าง

กฎร่วมอยู่ที่นี่ กฎเฉพาะอยู่ใน `AGENTS.md` ของแต่ละส่วน ก่อนแก้ไฟล์ให้อ่านกฎตามเส้นทางของไฟล์นั้นเท่านั้น ไม่ต้องอ่านทุกส่วนตั้งแต่เริ่ม หากงานข้ามส่วนให้อ่านเฉพาะส่วนที่เกี่ยวข้อง

| ส่วน | กฎเฉพาะ |
|---|---|
| Desktop / Go + Wails | [app/AGENTS.md](app/AGENTS.md) |
| Desktop Frontend / React + TypeScript | [app/frontend/AGENTS.md](app/frontend/AGENTS.md) และกฎแม่ใน `app/` |
| เว็บครู / Next.js ใน repo แยก | [แนวทางอ้างอิงเว็บครู](docs/teacher-web.md) อ่านเมื่อทำงานเว็บครูหรือเชื่อม School Bot กับเว็บครู |
| หน้า Admin / FE ใน sb-web-helper | [แนวทางอ้างอิง Admin FE](docs/admin-web.md) อ่านเฉพาะงานหน้าจอ Admin |
| Backend / C# API | [service-school-bot/AGENTS.md](service-school-bot/AGENTS.md) |
| Engines | [engines/AGENTS.md](engines/AGENTS.md) |
| Scripts / build / package / release | [scripts/AGENTS.md](scripts/AGENTS.md) |

แต่ละ `CLAUDE.md` มีเพียง `@AGENTS.md` เพื่อนำเข้ากฎในโฟลเดอร์เดียวกัน แก้กฎที่ `AGENTS.md` ของส่วนนั้น ไม่คัดลอกกฎหรือ import กฎลูกทั้งหมดไว้ที่ root

ก่อนรัน build/package/release ให้อ่าน `scripts/AGENTS.md` แม้ไม่ได้แก้ตัวสคริปต์

## การแก้โค้ด

- แก้ให้จบตามขอบเขตงาน รักษาพฤติกรรมส่วนที่ไม่เกี่ยวข้อง และไม่ทับงานของผู้ใช้หรือ agent อื่น
- เลือกวิธีที่เรียบง่าย ใช้ของที่มีอยู่ก่อน ไม่เพิ่ม abstraction, dependency หรือ framework เพื่อรองรับอนาคตที่ยังไม่มีความต้องการจริง
- ลบ dead code เมื่อพิสูจน์ได้ว่าไม่มีผู้ใช้ รวมถึง imports, variables, functions, components, styles และ branches ที่เข้าไม่ถึง
- ตรวจผู้เรียกทั้ง repository ก่อนลบ ระวัง Wails bindings, React lifecycle, dynamic imports, protocol actions และ compatibility ที่ยังรองรับ
- ไม่เก็บโค้ดเก่าไว้ในคอมเมนต์ ไม่สร้างไฟล์สำเนา `.bak`, `-old`, `-copy` หรือโฟลเดอร์ทดลองค้างไว้
- แต่ละส่วนใช้เกณฑ์ขนาดไฟล์ตามกฎของส่วนนั้น แยกตามหน้าที่จริงหรือโค้ดที่ใช้ร่วมกัน ไม่แตกไฟล์เพียงเพื่อลดจำนวนบรรทัด

## คอมเมนต์และเอกสาร

- คอมเมนต์ต้องอธิบาย **why** เช่น เหตุผลของข้อจำกัด ความปลอดภัย หรือ workaround ที่อ่านจากโค้ดอย่างเดียวไม่ชัด
- ไม่ใส่ task ID, ticket ID, ชื่อรอบงาน, วันที่แก้, ประวัติการสนทนา, รายงานความคืบหน้า หรือหัวข้อแบ่งโค้ดที่ไม่จำเป็นในคอมเมนต์
- ไม่เขียนคอมเมนต์อธิบายซ้ำว่าโค้ดทำอะไร ให้ใช้ชื่อและโครงสร้างโค้ดที่ชัดเจนแทน
- เก็บ compiler/build directives, license, copyright และ attribution ที่จำเป็น ห้ามลบทิ้งเพียงเพราะมีรูปแบบเป็นคอมเมนต์
- ไม่แก้ vendored code ใน `third_party/` เพื่อจัดรูปแบบหรือลบคอมเมนต์ ต้องมีเหตุผลเฉพาะของงาน
- อัปเดตเอกสารเดิมเมื่อ contract, วิธี build หรือพฤติกรรมเปลี่ยน ไม่สร้างเอกสารซ้ำหรือบันทึก task/verification ชั่วคราวโดยไม่จำเป็น
- เมื่อลบหรือย้ายไฟล์ ต้องแก้ references และตรวจลิงก์ที่เกี่ยวข้อง

## สัญญาระหว่างส่วนและอุปกรณ์

- ใช้ JSON Lines UTF-8: stdout สำหรับ protocol เท่านั้น diagnostics ไป stderr ต้องรักษา request matching, bounds, timeout และ cancellation
- การเปลี่ยน action/payload ต้องปรับ engine validation, contract/types, ผู้เรียก และ tests ที่เกี่ยวข้องให้ตรงกัน
- แยก preview/sample จาก native hardware ให้ชัด Sample mode ต้องไม่ส่งคำสั่งไปอุปกรณ์
- ห้ามทดสอบการเคลื่อนไหวจริง อัปโหลด firmware หรือเปลี่ยนสถานะอุปกรณ์โดยไม่มีคำสั่งจากผู้ใช้ให้ทดสอบสิ่งนั้น
- การ cancel process ไม่เท่ากับ physical stop ต้องรักษาการ drain งานเดิมก่อนส่งคำสั่งหยุด และกัน callbacks เก่าส่งการเคลื่อนไหวต่อ
- OS write สำเร็จไม่ยืนยันตำแหน่งหรือการหยุดจริง ห้ามนำค่าจำลองมาแสดงเป็น sensor measurement หรือใช้ preset ที่ยังไม่ calibrated กับ hardware

## ไฟล์และการตรวจงาน

- ไม่ปิด process ของผู้ใช้ ติดตั้งทับ หรือถอนการติดตั้งเพื่อทดสอบโดยพลการ
- ก่อนลบไฟล์/โฟลเดอร์ให้ตรวจ absolute path ว่าอยู่ในขอบเขตงาน บน Windows ใช้ PowerShell `-LiteralPath` และตรวจ junction/symlink ก่อน recursive deletion
- เก็บ source, user projects, SDK/dependencies ที่ยังใช้ และ release ที่ผู้ใช้กำลังทดสอบ ลบเฉพาะสิ่งที่ตรวจแล้วว่าไม่จำเป็นตามขอบเขตคำสั่ง
- ไม่ใช้ git ที่เปลี่ยน state เช่น add, commit, reset, checkout, branch, merge หรือ push เว้นแต่ผู้ใช้สั่งชัดเจน
- รัน checks ตามส่วนที่เปลี่ยน ไม่เพิ่ม tests ที่เพียงทวน implementation หรือรัน build ใหญ่ซ้ำโดยไม่มีเหตุผล
- งาน integration/release ใช้ `scripts/verify.ps1` ตามความเหมาะสม โดยตรวจ output และ process ที่เปิดอยู่ก่อน เพราะสคริปต์นี้ build desktop ด้วย
- เอกสารหรือคอมเมนต์อย่างเดียวให้ตรวจ diff/references ตามความจำเป็น ไม่ต้อง rebuild installer
- จะ release app, ตัดรอบเวอร์ชัน หรือสร้าง release artifacts/installer สำหรับ release ก็ต่อเมื่อผู้ใช้สั่งชัดเจนเท่านั้น หากยังไม่มีคำสั่งระบุเวอร์ชัน ให้ใช้ 1.0.0 เป็นค่าเริ่มต้น
- สรุปภาษาไทยแบบกระชับว่าแก้อะไร ตรวจอะไรผ่าน และมีข้อจำกัดอะไร แยกผล build/tests จากการทดสอบ UI หรือ hardware จริง
- หาก checks ถูก sandbox/toolchain ขวาง ให้รายงานตามจริง ไม่อ้างว่าผ่าน และระบุเมื่อ EXE/installer ยังไม่ได้ rebuild
