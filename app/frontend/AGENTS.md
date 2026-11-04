# Frontend / React + TypeScript

ใช้ร่วมกับกฎแม่ที่ [../AGENTS.md](../AGENTS.md) และ [../../AGENTS.md](../../AGENTS.md) ขอบเขตไฟล์นี้คือ `app/frontend/`

ไฟล์นี้ใช้กับ Desktop Frontend งานเว็บครูใน repo `sb-tech-upgrade-web-system` หรือการเชื่อมกับเว็บครูให้อ่าน [แนวทางเว็บครู](../../docs/teacher-web.md) และ resolve `teacherWebRoot` จาก `local.json` ไม่ใช้โครงสร้าง Next.js ของเว็บครูมาบังคับ Desktop

- รักษา TypeScript strict mode, `noUnusedLocals` และ `noUnusedParameters` ห้ามปิดกฎเพื่อให้ build ผ่าน
- Frontend เรียกผ่าน `app/frontend/src/core/client.ts` ไม่เรียก filesystem, shell, executable หรือ database โดยตรง

## ขนาดไฟล์และการแยกหน้าที่

- เกณฑ์ในไฟล์นี้ใช้กับ Frontend (`app/frontend/`) ส่วน Backend ใช้เกณฑ์และแนวทางแยกหน้าที่ใน `web/AGENTS.md`
- เมื่อเพิ่มหรือแก้ไฟล์ source ที่เกิน 300 บรรทัด ต้องวิเคราะห์ก่อนว่ามีหลายหน้าที่ โค้ดหรือ JSX ซ้ำ หรือส่วนที่แยกขอบเขตได้ชัดเจนหรือไม่ จำนวนบรรทัดเป็นสัญญาณให้ทบทวน ไม่ใช่ข้อสรุปว่าโค้ดรก
- หากเกิน 500 บรรทัด ต้องสรุปแนวทางแบ่งหน้าที่ก่อนเพิ่มโค้ดต่อ แล้วดำเนินการตามขอบเขตงานที่ได้รับ หากควรเก็บรวม ให้แจ้งเหตุผลที่ชัดเจนในการส่งมอบ ไม่ใส่รายงานการตัดสินใจชั่วคราวไว้ในคอมเมนต์
- แม้ไม่ถึง 300 บรรทัดก็ควรแยกเมื่อมีโค้ดซ้ำหรือส่วนที่มีหน้าที่ชัดเจน ประเมินความซับซ้อน ความซ้ำ และความยาวของบรรทัดร่วมด้วย โดยเฉพาะ TSX ที่มี Tailwind classes ยาว
- แยก UI เป็น component และแยก state/workflow เป็น hook หรือ module ตามหน้าที่ ส่วนที่ใช้ร่วมกันอยู่ใน `app/frontend/src/components/` ตามหมวดเดิม ส่วนเฉพาะหน้าอยู่กับ mode
- ไม่แตกไฟล์เพียงเพื่อลดจำนวนบรรทัด ไม่ย่อหลายคำสั่งไว้บรรทัดเดียวเพื่อหลบเกณฑ์ และไม่ขยายไป refactor ไฟล์นอกขอบเขตงานเพียงเพราะเกินเกณฑ์ ต้องรักษาพฤติกรรม dependencies, lifecycle และ cleanup เดิม
- ยกเว้นเกณฑ์จำนวนบรรทัดสำหรับ generated code, bindings ที่สร้างอัตโนมัติ, vendored code และ JSON คำแปล

## UI และภาษา

- ใช้ components, tokens, assets และ design skill ของ School Bright ที่มีอยู่เมื่อแก้ UI ไม่ออกแบบหน้าตาใหม่ระหว่างงาน cleanup
- ก่อนเพิ่มหรือแก้ UI ต้องตรวจและใช้ components ที่มีอยู่ใน `app/frontend/src/components/` ก่อน เช่น `Button variant="secondary"`, `Field` และ `Modal` ไม่เขียน UI ซ้ำเมื่อ component เดิมรองรับ; หากต้องเพิ่ม component ให้จัดตามขอบเขตการใช้ร่วมกัน โดย UI เฉพาะหน้าอยู่กับ mode เดิม
- ทุกครั้งที่เพิ่มหรือแก้ข้อความ UI ต้องเพิ่มหรืออัปเดตคำแปลใน `app/frontend/src/i18n/locales/th.json`, `en.json`, `zh.json` ให้ครบทุกภาษาภายในงานเดียวกัน รวมถึงข้อความปุ่ม หัวข้อ dialog, placeholder, tooltip, toast และ accessibility labels ห้าม hardcode ข้อความ UI ใน TS/TSX
- ใช้ i18n key ที่สื่อความหมาย เช่น `modal.leave.title` ผ่าน `t()` หรือ hook ของ i18n ที่มีอยู่ ตรวจและใช้ key เดิมก่อนสร้างใหม่ พร้อมรักษา key และ interpolation parameters ให้ตรงกันทุกภาษา ไม่ใช้ข้อความไทยเป็น key และไม่ต่อข้อความแปลกับข้อมูลผู้ใช้โดยตรง
key ใหม่ต้องจัดตามโครงสร้างหน้าและส่วนของ UI เช่น `robot.connect.status.title` ไม่ใช้ประโยคข้อความเป็นชื่อ key และตั้งชื่อ interpolation ตามความหมายของข้อมูล เช่น `robotName`, `poseCount` ไม่ใช้ `value0` หรือ `value1`; คง key เก่าไว้ได้และทยอยปรับเฉพาะส่วนที่อยู่ในขอบเขตงาน
- ชื่องาน ชื่อท่า ชื่อหุ่น โค้ดและข้อมูลที่ผู้ใช้กรอก รวมถึง diagnostics จากอุปกรณ์/engine และประวัติต้นทาง ไม่ใช่ข้อความ UI คงที่ ห้ามแปลหรือแก้ค่าข้อมูลเหล่านี้เพื่อเปลี่ยนภาษา
- ใช้ไอคอนหุ่น School Bot จาก assets เดิม ไม่แทนด้วยตัวอักษร SB และไม่เพิ่มแถวโลโก้ซ้ำกับ header เดิม

## ชั้นของ UI, view-model และ handler

- แยกตามหน้าที่: TSX เป็นเจ้าของหน้าตา (layout, `className`, สไตล์ตามสถานะ), hook/module เป็นเจ้าของ state, derived flag และ handler; ตัวสร้าง view-model/field (`build*Fields`, `*View`) คืนเฉพาะข้อมูลและค่า derived ที่ component เอาไปแสดงหรือผูกต่อ
- ห้ามสร้างสไตล์เป็น string ใน `.ts` แล้วส่งให้ TSX แปลง (เช่น `'display: flex; ...'` ผ่าน `sx()` หรือ Tailwind class ที่ประกอบใน `.ts`) เขียนใน TSX ด้วย `className` กับตัวแปรสถานะ หรือใช้ component/variant เดิม โค้ดเดิมที่เป็นรูปแบบนี้ให้ย้ายเมื่อแก้ส่วนนั้น ไม่เพิ่ม field สไตล์ใหม่ตามรูปแบบเดิม
- Handler ที่มี side effect (เรียก host/engine, timer, DOM, เขียน `setState`) อยู่ใน action/hook ไม่ฝังใน object ที่สร้างซ้ำทุก render และห้าม assign ค่าลง object ภายนอกระหว่างสร้าง view-model ให้ลงทะเบียนใน lifecycle ที่มี cleanup
- ไม่ประกอบประโยค UI ด้วยการต่อ `t()` หลายตัวด้วย `+` หรือ ternary ซ้อนกัน เพราะลำดับคำต่างกันในแต่ละภาษา ให้ใช้ i18n key เต็มประโยคพร้อม interpolation ต่อหนึ่งเงื่อนไข และเลือก key ใน component หรือ helper เล็กๆ ที่ตั้งชื่อชัด
- ไม่ใช้ `any` ในโค้ดใหม่หรือส่วนที่แก้ ระบุ type ของ context/props/return ให้ตรง และไม่ใช้ `(x?: any)` เป็นพารามิเตอร์ตามความเคยชิน
- ตั้งชื่อ field ให้สื่อความหมาย ไม่ใช้ตัวย่อสองตัวอักษรที่อ่านไม่ออก (เช่น `pk`, `rg`, `nb`) ในโค้ดใหม่ และไม่รวมหลายหน้าที่ไว้ใน object เดียวที่ชื่อกว้างเกินไป
- ขึ้นบรรทัดให้อ่านได้ หนึ่งบรรทัดต่อหนึ่งคำสั่งหรือหนึ่ง property ไม่ยัด handler, ternary ซ้อน หรือ string ยาวเกินประมาณ 120 ตัวอักษรไว้บรรทัดเดียว ให้แยกเป็นฟังก์ชันหรือค่าที่ตั้งชื่อ
- Handler ที่จำลองผลสำเร็จ (เช่น `setTimeout` แล้วตั้งสถานะว่า upload แล้ว) ต้องใช้ใน Sample/preview เท่านั้น และแยกจาก path ที่ส่งไปอุปกรณ์จริงอย่างชัดเจน ห้ามให้ผู้ใช้เห็นว่าส่งสำเร็จทั้งที่ไม่ได้ส่ง

## ตรวจงาน

- Frontend: รัน `npm run typecheck` และ `npm run build` จาก `app/frontend`; ถ้าเปลี่ยน robot behavior ให้รัน adapter/workspace regression tests ใน `app/frontend/tests/`
- เมื่อเพิ่มหรือแก้ข้อความ UI, i18n keys หรือไฟล์คำแปล ให้รัน `npm run test:i18n` จาก `app/frontend` เพิ่มด้วย
