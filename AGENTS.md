# กฎการทำงาน School Bot

กฎนี้ใช้กับโค้ดทั้งหมดใน repository อ่าน `README.md` และเอกสารของส่วนที่จะเปลี่ยนก่อนทำงาน
คำสั่งล่าสุดของผู้ใช้มีลำดับเหนือกฎในไฟล์นี้ หากข้อมูลเอกสารไม่ตรงกับโค้ด ให้ตรวจโค้ดและรายงานข้อแตกต่าง

ไฟล์นี้เป็นแหล่งกฎร่วมเพียงที่เดียวของโครงการ `CLAUDE.md` มีแค่ `@AGENTS.md` เพื่อนำเข้าไฟล์นี้ เมื่อต้องเพิ่มหรือแก้กฎให้แก้ที่นี่เท่านั้น ไม่คัดลอกกฎไปไว้ใน `CLAUDE.md`

## การแก้โค้ด

- แก้ให้จบตามขอบเขตงาน รักษาพฤติกรรมส่วนที่ไม่เกี่ยวข้อง และไม่ทับงานของผู้ใช้หรือ agent อื่น
- เลือกวิธีที่เรียบง่าย ใช้ของที่มีอยู่ก่อน ไม่เพิ่ม abstraction, dependency หรือ framework เพื่อรองรับอนาคตที่ยังไม่มีความต้องการจริง
- ลบ dead code เมื่อพิสูจน์ได้ว่าไม่มีผู้ใช้ รวมถึง imports, variables, functions, components, styles และ branches ที่เข้าไม่ถึง
- ตรวจผู้เรียกทั้ง repository ก่อนลบ ระวัง Wails bindings, React lifecycle, dynamic imports, protocol actions และ compatibility ที่ยังรองรับ
- ไม่เก็บโค้ดเก่าไว้ในคอมเมนต์ ไม่สร้างไฟล์สำเนา `.bak`, `-old`, `-copy` หรือโฟลเดอร์ทดลองค้างไว้
- รักษา TypeScript strict mode, `noUnusedLocals` และ `noUnusedParameters` ห้ามปิดกฎเพื่อให้ build ผ่าน

## ขนาดไฟล์และการแยกหน้าที่

- เมื่อเพิ่มหรือแก้ไฟล์ source ที่เกิน 300 บรรทัด ต้องวิเคราะห์ก่อนว่ามีหลายหน้าที่ โค้ดหรือ JSX ซ้ำ หรือส่วนที่แยกขอบเขตได้ชัดเจนหรือไม่ จำนวนบรรทัดเป็นสัญญาณให้ทบทวน ไม่ใช่ข้อสรุปว่าโค้ดรก
- หากเกิน 500 บรรทัด ต้องสรุปแนวทางแบ่งหน้าที่ก่อนเพิ่มโค้ดต่อ แล้วดำเนินการตามขอบเขตงานที่ได้รับ หากควรเก็บรวม ให้แจ้งเหตุผลที่ชัดเจนในการส่งมอบ ไม่ใส่รายงานการตัดสินใจชั่วคราวไว้ในคอมเมนต์
- แม้ไม่ถึง 300 บรรทัดก็ควรแยกเมื่อมีโค้ดซ้ำหรือส่วนที่มีหน้าที่ชัดเจน ประเมินความซับซ้อน ความซ้ำ และความยาวของบรรทัดร่วมด้วย โดยเฉพาะ TSX ที่มี Tailwind classes ยาว
- แยก UI เป็น component และแยก state/workflow เป็น hook หรือ module ตามหน้าที่ ส่วนที่ใช้ร่วมกันอยู่ใน `app/frontend/src/components/` ตามหมวดเดิม ส่วนเฉพาะหน้าอยู่กับ mode; โค้ด backend/engine ให้แยกตามโครงสร้างและภาษาของส่วนนั้น ไม่ใช้รูปแบบ React component
- ไม่แตกไฟล์เพียงเพื่อลดจำนวนบรรทัด ไม่ย่อหลายคำสั่งไว้บรรทัดเดียวเพื่อหลบเกณฑ์ และไม่ขยายไป refactor ไฟล์นอกขอบเขตงานเพียงเพราะเกินเกณฑ์ ต้องรักษาพฤติกรรม dependencies, lifecycle และ cleanup เดิม
- ยกเว้นเกณฑ์จำนวนบรรทัดสำหรับ generated code, bindings ที่สร้างอัตโนมัติ, vendored code และ JSON คำแปล

## คอมเมนต์และเอกสาร

- คอมเมนต์ต้องอธิบาย **why** เช่น เหตุผลของข้อจำกัด ความปลอดภัย หรือ workaround ที่อ่านจากโค้ดอย่างเดียวไม่ชัด
- ไม่ใส่ task ID, ticket ID, ชื่อรอบงาน, วันที่แก้, ประวัติการสนทนา, รายงานความคืบหน้า หรือหัวข้อแบ่งโค้ดที่ไม่จำเป็นในคอมเมนต์
- ไม่เขียนคอมเมนต์อธิบายซ้ำว่าโค้ดทำอะไร ให้ใช้ชื่อและโครงสร้างโค้ดที่ชัดเจนแทน
- เก็บ compiler/build directives, license, copyright และ attribution ที่จำเป็น ห้ามลบทิ้งเพียงเพราะมีรูปแบบเป็นคอมเมนต์
- ไม่แก้ vendored code ใน `third_party/` เพื่อจัดรูปแบบหรือลบคอมเมนต์ ต้องมีเหตุผลเฉพาะของงาน
- อัปเดตเอกสารเดิมเมื่อ contract, วิธี build หรือพฤติกรรมเปลี่ยน ไม่สร้างเอกสารซ้ำหรือบันทึก task/verification ชั่วคราวโดยไม่จำเป็น
- เมื่อลบหรือย้ายไฟล์ ต้องแก้ references และตรวจลิงก์ที่เกี่ยวข้อง

## ขอบเขตระบบและหลาย engine

- Desktop อยู่ใน `app/`: Go + Wails และ React/TypeScript ส่วน C# API อยู่ใน `web/`
- Frontend เรียกผ่าน `app/frontend/src/core/client.ts` ไม่เรียก filesystem, shell, executable หรือ database โดยตรง
- Host เป็นผู้เลือก engine จาก trusted routing เท่านั้น ห้ามรับ executable path จาก frontend หรือ project file
- Engine อยู่ใน `engines/<engine-id>/` แยก source, dependencies, tests, build scripts และ metadata ของตัวเอง
- ใช้ภาษาที่เหมาะกับ engine เดิมได้ เช่น C++ ไม่ต้องเขียนใหม่ด้วย Go และไม่ผูก engine ID กับ mode ID แบบหนึ่งต่อหนึ่ง
- Root build-engine scripts มีหน้าที่ dispatch ไม่รวม compiler logic ของทุก engine เข้าด้วยกัน
- ใช้ JSON Lines UTF-8: stdout สำหรับ protocol เท่านั้น diagnostics ไป stderr ต้องรักษา request matching, bounds, timeout และ cancellation
- การเปลี่ยน action/payload ต้องปรับ engine validation, contract/types, ผู้เรียก และ tests ที่เกี่ยวข้องให้ตรงกัน
- Installer เดียวรวม desktop และ engines ที่ใช้งาน แต่ API ฝั่ง server deploy แยก ไม่บรรจุใน desktop installer
- C# รักษาทิศทาง Clean Architecture: Domain ไม่ขึ้นกับ Application, Infrastructure หรือ ASP.NET; Application ไม่ขึ้นกับ Infrastructure/API

## UI และหุ่นยนต์

- ใช้ components, tokens, assets และ design skill ของ School Bright ที่มีอยู่เมื่อแก้ UI ไม่ออกแบบหน้าตาใหม่ระหว่างงาน cleanup
- ก่อนเพิ่มหรือแก้ UI ต้องตรวจและใช้ components ที่มีอยู่ใน `app/frontend/src/components/` ก่อน เช่น `Button variant="secondary"`, `Field` และ `Modal` ไม่เขียน UI ซ้ำเมื่อ component เดิมรองรับ; หากต้องเพิ่ม component ให้จัดตามขอบเขตการใช้ร่วมกัน โดย UI เฉพาะหน้าอยู่กับ mode เดิม
- ทุกครั้งที่เพิ่มหรือแก้ข้อความ UI ต้องเพิ่มหรืออัปเดตคำแปลใน `app/frontend/src/i18n/locales/th.json`, `en.json`, `zh.json` ให้ครบทุกภาษาภายในงานเดียวกัน รวมถึงข้อความปุ่ม หัวข้อ dialog, placeholder, tooltip, toast และ accessibility labels ห้าม hardcode ข้อความ UI ใน TS/TSX
- ใช้ i18n key ที่สื่อความหมาย เช่น `modal.leave.title` ผ่าน `t()` หรือ hook ของ i18n ที่มีอยู่ ตรวจและใช้ key เดิมก่อนสร้างใหม่ พร้อมรักษา key และ interpolation parameters ให้ตรงกันทุกภาษา ไม่ใช้ข้อความไทยเป็น key และไม่ต่อข้อความแปลกับข้อมูลผู้ใช้โดยตรง

key ใหม่ต้องจัดตามโครงสร้างหน้าและส่วนของ UI เช่น `robot.connect.status.title` ไม่ใช้ประโยคข้อความเป็นชื่อ key และตั้งชื่อ interpolation ตามความหมายของข้อมูล เช่น `robotName`, `poseCount` ไม่ใช้ `value0` หรือ `value1`; คง key เก่าไว้ได้และทยอยปรับเฉพาะส่วนที่อยู่ในขอบเขตงาน
- ชื่องาน ชื่อท่า ชื่อหุ่น โค้ดและข้อมูลที่ผู้ใช้กรอก รวมถึง diagnostics จากอุปกรณ์/engine และประวัติต้นทาง ไม่ใช่ข้อความ UI คงที่ ห้ามแปลหรือแก้ค่าข้อมูลเหล่านี้เพื่อเปลี่ยนภาษา
- ใช้ไอคอนหุ่น School Bot จาก assets เดิม ไม่แทนด้วยตัวอักษร SB และไม่เพิ่มแถวโลโก้ซ้ำกับ header เดิม
- รักษาการเปิดแบบ fullscreen/frameless และปุ่มหน้าต่างใน header ของแอป รวมถึง close guards
- แยก preview/sample จาก native hardware ให้ชัด Sample mode ต้องไม่ส่งคำสั่งไปอุปกรณ์
- ห้ามทดสอบการเคลื่อนไหวจริง อัปโหลด firmware หรือเปลี่ยนสถานะอุปกรณ์โดยไม่มีคำสั่งจากผู้ใช้ให้ทดสอบสิ่งนั้น
- การ cancel process ไม่เท่ากับ physical stop ต้องรักษาการ drain งานเดิมก่อนส่งคำสั่งหยุด และกัน callbacks เก่าส่งการเคลื่อนไหวต่อ
- OS write สำเร็จไม่ยืนยันตำแหน่งหรือการหยุดจริง ห้ามนำค่าจำลองมาแสดงเป็น sensor measurement หรือใช้ preset ที่ยังไม่ calibrated กับ hardware

## ชั้นของ UI, view-model และ handler

- แยกตามหน้าที่: TSX เป็นเจ้าของหน้าตา (layout, `className`, สไตล์ตามสถานะ), hook/module เป็นเจ้าของ state, derived flag และ handler; ตัวสร้าง view-model/field (`build*Fields`, `*View`) คืนเฉพาะข้อมูลและค่า derived ที่ component เอาไปแสดงหรือผูกต่อ
- ห้ามสร้างสไตล์เป็น string ใน `.ts` แล้วส่งให้ TSX แปลง (เช่น `'display: flex; ...'` ผ่าน `sx()` หรือ Tailwind class ที่ประกอบใน `.ts`) เขียนใน TSX ด้วย `className` กับตัวแปรสถานะ หรือใช้ component/variant เดิม โค้ดเดิมที่เป็นรูปแบบนี้ให้ย้ายเมื่อแก้ส่วนนั้น ไม่เพิ่ม field สไตล์ใหม่ตามรูปแบบเดิม
- Handler ที่มี side effect (เรียก host/engine, timer, DOM, เขียน `setState`) อยู่ใน action/hook ไม่ฝังใน object ที่สร้างซ้ำทุก render และห้าม assign ค่าลง object ภายนอกระหว่างสร้าง view-model ให้ลงทะเบียนใน lifecycle ที่มี cleanup
- ไม่ประกอบประโยค UI ด้วยการต่อ `t()` หลายตัวด้วย `+` หรือ ternary ซ้อนกัน เพราะลำดับคำต่างกันในแต่ละภาษา ให้ใช้ i18n key เต็มประโยคพร้อม interpolation ต่อหนึ่งเงื่อนไข และเลือก key ใน component หรือ helper เล็กๆ ที่ตั้งชื่อชัด
- ไม่ใช้ `any` ในโค้ดใหม่หรือส่วนที่แก้ ระบุ type ของ context/props/return ให้ตรง และไม่ใช้ `(x?: any)` เป็นพารามิเตอร์ตามความเคยชิน
- ตั้งชื่อ field ให้สื่อความหมาย ไม่ใช้ตัวย่อสองตัวอักษรที่อ่านไม่ออก (เช่น `pk`, `rg`, `nb`) ในโค้ดใหม่ และไม่รวมหลายหน้าที่ไว้ใน object เดียวที่ชื่อกว้างเกินไป
- ขึ้นบรรทัดให้อ่านได้ หนึ่งบรรทัดต่อหนึ่งคำสั่งหรือหนึ่ง property ไม่ยัด handler, ternary ซ้อน หรือ string ยาวเกินประมาณ 120 ตัวอักษรไว้บรรทัดเดียว ให้แยกเป็นฟังก์ชันหรือค่าที่ตั้งชื่อ
- Handler ที่จำลองผลสำเร็จ (เช่น `setTimeout` แล้วตั้งสถานะว่า upload แล้ว) ต้องใช้ใน Sample/preview เท่านั้น และแยกจาก path ที่ส่งไปอุปกรณ์จริงอย่างชัดเจน ห้ามให้ผู้ใช้เห็นว่าส่งสำเร็จทั้งที่ไม่ได้ส่ง

## Release และการจัดการไฟล์

- Release ต้องไม่มี source maps, original sources, debug symbols, development tools, secrets หรือ private keys ใน payload เก็บ third-party notices/licenses ให้ครบ
- Frontend ที่ฝังใน binary ยังตรวจสอบย้อนหลังได้ ห้ามอ้างว่าป้องกันการคัดลอกหรือ reverse engineering ได้ทั้งหมด
- ใช้ build/package scripts ของ repository ไม่ข้ามขั้นตอนสร้าง icon/version resources หรือ release asset checks
- อ่านเวอร์ชันจาก manifests ที่มีอยู่ ไม่สร้างแหล่งเวอร์ชันซ้ำ และไม่อ้างว่า build/signing ผ่านโดยไม่มีผลตรวจจริง
- ไม่ overwrite binary ที่ผู้ใช้กำลังเปิดอยู่ ใช้ `scripts/build-desktop.ps1 -OutputDirectory <path>` และ `scripts/package-desktop.ps1 -PayloadDirectory <path>` เมื่อต้องแยก Windows output
- ไม่ปิด process ของผู้ใช้ ติดตั้งทับ หรือถอนการติดตั้งเพื่อทดสอบโดยพลการ
- ก่อนลบไฟล์/โฟลเดอร์ให้ตรวจ absolute path ว่าอยู่ในขอบเขตงาน บน Windows ใช้ PowerShell `-LiteralPath` และตรวจ junction/symlink ก่อน recursive deletion
- เก็บ source, user projects, SDK/dependencies ที่ยังใช้ และ release ที่ผู้ใช้กำลังทดสอบ ลบเฉพาะสิ่งที่ตรวจแล้วว่าไม่จำเป็นตามขอบเขตคำสั่ง
- ไม่ใช้ git ที่เปลี่ยน state เช่น add, commit, reset, checkout, branch, merge หรือ push เว้นแต่ผู้ใช้สั่งชัดเจน

## การตรวจงานและส่งมอบ

- รัน checks ตามส่วนที่เปลี่ยน ไม่เพิ่ม tests ที่เพียงทวน implementation หรือรัน build ใหญ่ซ้ำโดยไม่มีเหตุผล
- Frontend: รัน `npm run typecheck` และ `npm run build` จาก `app/frontend`; ถ้าเปลี่ยน robot behavior ให้รัน adapter/workspace regression tests ใน `app/frontend/tests/`
- เมื่อเพิ่มหรือแก้ข้อความ UI, i18n keys หรือไฟล์คำแปล ให้รัน `npm run test:i18n` จาก `app/frontend` เพิ่มด้วย
- Go host/engine manager: รัน `go test ./...` และ `go vet ./...` จาก `app` โดยใช้ SDK ตาม repository
- C++ engine: ใช้ `scripts/build-engine.ps1 -Engine zide -Test` และ IPC tests กับ binary ที่ build จาก source ล่าสุด ใช้ dry-run สำหรับการตรวจอัตโนมัติ
- C# API: รัน `dotnet test SchoolBot.slnx --disable-build-servers -m:1` จาก `web` เมื่อเปลี่ยน behavior
- งาน integration/release ใช้ `scripts/verify.ps1` ตามความเหมาะสม โดยตรวจ output และ process ที่เปิดอยู่ก่อน เพราะสคริปต์นี้ build desktop ด้วย
- เอกสารหรือคอมเมนต์อย่างเดียวให้ตรวจ diff/references ตามความจำเป็น ไม่ต้อง rebuild installer
- สรุปภาษาไทยแบบกระชับว่าแก้อะไร ตรวจอะไรผ่าน และมีข้อจำกัดอะไร แยกผล build/tests จากการทดสอบ UI หรือ hardware จริง
- หาก checks ถูก sandbox/toolchain ขวาง ให้รายงานตามจริง ไม่อ้างว่าผ่าน และระบุเมื่อ EXE/installer ยังไม่ได้ rebuild
