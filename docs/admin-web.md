# หน้า Admin — sb-web-helper (เฉพาะ FE)

ใช้เมื่ออ้างอิงหรือทำงาน Frontend ของหน้า Admin ใน `sb-web-helper` ครอบคลุมหน้าจอ component, state, client-side validation, API client ที่หน้าจอเรียก และ FE tests ไม่ครอบคลุม API route handlers, server services, database/Prisma, cronjobs, deploy หรือ Backend ของ repo นี้

## ตำแหน่งบนเครื่อง

อ่าน `adminWebRoot` จาก `local.json` ที่ root ของ School Bot หากไม่มีไฟล์หรือไม่ได้กำหนด key ใช้ `C:/src/schoolbright/target/sb-web-helper` เป็นค่าเริ่มต้น ดู [ตัวอย่าง config](../service-school-bot/local.example.json)

Relative path อิง root ของ School Bot ค่าที่กำหนดแต่ใช้ไม่ได้ต้องรายงาน ไม่เลือก checkout อื่นเงียบ ๆ ค่านี้เป็น path เอกสาร/source สำหรับ agent ไม่ใช่ URL หรือ runtime config และ `local.json` ไม่เข้า Git หากไม่มี checkout ให้ระบุส่วนที่ยังตรวจไม่ได้

## อ่านต้นฉบับเฉพาะงาน FE

ทุก path ในตารางอิง `<adminWebRoot>` เปิดเฉพาะส่วนที่เกี่ยวข้อง ไม่โหลดกฎทั้ง repo ตั้งแต่เริ่ม เมื่อจะแก้ไฟล์ใน repo Admin ต้องอ่าน `AGENTS.md`, `CLAUDE.md` และกฎที่ครอบไฟล์นั้นของ repo ปลายทางก่อน

| งาน FE | ต้นฉบับที่ต้องตรวจ |
|---|---|
| โครงสร้าง ขนาดไฟล์ ชื่อ และการใช้ร่วมกัน | `.claude/rules/structure.md` |
| หน้าตา tokens, theme และ component กลาง | `DESIGN.md`, `.claude/rules/frontend.md` |
| บริบทของหน้าจอ | `PRODUCT.md` และเอกสาร feature ใต้ `_docs/` ตามแผนที่ใน `CLAUDE.md` |
| เลือกโรงเรียน | `.claude/rules/school-picker.md` |
| State, hooks และ API client | โค้ด FE ของ feature เดิมใน `src/app/pages/`, `src/hooks/`, `src/services/` และ `src/stores/` |
| FE tests และคำสั่งตรวจ | `.claude/rules/structure.md`, `CLAUDE.md`, `package.json`, test ข้างไฟล์ที่แก้ |

ไม่ import กฎ Backend ของ sb-web-helper มาเป็นกฎ BE ของ School Bot หากงานหน้าจอต้องเปลี่ยน server contract ให้รายงาน dependency นั้นและแยกขอบเขต Backend ก่อน ไม่ขยายไปแก้ server จากคำสั่ง FE อย่างเดียว

## โครงสร้างและการแยกหน้าที่

- หน้าใหม่อยู่ `src/app/pages/<domain>/<feature>/` ตาม domain ของ repo ให้หน้าประกอบ UI และแยกงานตามหน้าที่ เช่น `_components/`, `_stores/`, `_api/` และ hook ตาม pattern ของ feature นั้น ไม่สร้างหมวดว่างเผื่ออนาคต
- UI ใช้หน้าเดียวอยู่กับ feature; ใช้จริงตั้งแต่สอง feature ให้ตรวจ `src/components/<kind>/` ก่อน เช่น `card`, `modal`, `table`, `form` ไม่ใช้ path `src/components/shared/` ของเว็บครูโดยสมมติว่า Admin มีโครงสร้างเหมือนกัน
- Pure function ที่ไม่แตะ React/Next/DB อยู่ `src/lib/<topic>/<name>.ts`; state ใหม่ใช้ Zustand ตามกฎ Admin รักษา Redux เดิมที่ frozen และไม่ขยายด้วย state ใหม่ ตรวจ state lifecycle/reset กับโค้ดจริงก่อนย้าย
- ไฟล์ยาวให้ทบทวนความซ้ำ หน้าที่ และ dependencies ใช้เกณฑ์ที่ repo Admin กำหนด ไม่ยก hard limit `page.tsx` 300 บรรทัดของเว็บครูมาอ้างว่าเป็นกฎ Admin โดยอัตโนมัติ แยก component/hook/pure function ที่อ่านและใช้ซ้ำได้จริง
- ชื่อไฟล์/โฟลเดอร์ kebab-case, component default export เป็น PascalCase ตรงชื่อไฟล์, store `use-<feature>-store.ts`; ไฟล์ใหม่ไม่ใช้ `.util.ts`, `.helper.ts`, `.type.ts`, `.types.ts` ตามกฎโครงสร้าง Admin ไม่สร้าง barrel `index.ts` ใหม่ใน `src/components/` หรือ `src/lib/`
- ใช้ component กลางก่อนสร้างใหม่ หากพบโค้ดซ้ำข้ามหลายหน้าที่ต้องยุบเป็น shared ให้เสนอผู้ใช้และจำนวนผู้ใช้ตามกฎ Admin ก่อนย้ายทั้งกลุ่ม ไม่ refactor นอกขอบเขตงาน

## UI, forms และ API client

- อ่าน `DESIGN.md` และ `.claude/rules/frontend.md` ก่อนแก้ UI ใช้ `src/components/ui/` และ `compat/` ตามหน้าที่เดิม ไม่เพิ่ม Ant Design กลับมา ใช้ token/theme จากระบบเดิม ไม่ hardcode สีหรือสร้าง design system ใหม่
- ใช้ของกลาง เช่น `HeaderBar`, `CardSectionHeader`, `SummaryCard`, `StatusModal`, `SchoolPicker` และ `DataTable` จาก path ในกฎ Admin ไม่ copy component ของเว็บครูหรือ Desktop มาทั้งชุด
- Forms ใช้ react-hook-form และ Zod ตาม pattern เดิม รักษา confirm → ส่งคำขอ → success/error ผ่าน `StatusModal`; ปุ่มป้องกัน submit ซ้ำ และต้องแสดง loading/error/empty states กับ keyboard/focus ที่เหมาะสม
- ใช้ข้อความ ภาษา และรูปแบบข้อมูลของ feature กับ `src/locales/` ตามระบบ Admin จริง ไม่สมมติว่าใช้ `next-intl`, `messages/` หรือชุดภาษาของเว็บครู/Desktop
- หน้าจอเรียกผ่าน client/service เดิมของ Admin และ HTTP contract ที่ได้รับอนุมัติ ไม่เรียกฐานข้อมูล/secret หรือ gRPC ตรงจาก browser ไม่เพิ่มหรือแก้ API route handler จากขอบเขต FE นี้
- รักษา permission/tenant context ของ UI และ contract ที่ได้รับ ไม่ใช้การซ่อนปุ่มเป็นหลักฐานว่า Backend ตรวจสิทธิ์แล้ว ไม่แสดงผลสำเร็จเมื่อคำขอล้มเหลว

## FE tests และการตรวจงาน

- Admin ใช้ `bun:test` และ test อยู่ข้าง source เป็น `<name>.test.ts` ตามกฎ repo ไม่ยก Vitest หรือ `tests/` mirror path ของเว็บครูมาใช้กับ Admin
- ตรวจ pure functions/schema/state transitions และข้อผิดพลาดที่เกี่ยวข้องกับงาน ใช้ fixtures/mocks ตาม pattern เดิม ไม่เรียก mutation จริงหรือแก้ Backend เพื่อเตรียม FE tests
- จาก `<adminWebRoot>` ใช้ `bun test <path>`, `bunx eslint <files>` และ `bun run typecheck` ตาม `package.json` ไม่สลับเป็น `type-check` ของเว็บครู ตรวจ error เดิมเทียบส่วนที่เพิ่ม ไม่อ้างว่าทั้ง repo ผ่านจากการตรวจเพียงไฟล์เดียว
- งาน UI ตรวจ browser ตามกฎ `verify` ของ Admin พร้อม responsive, keyboard/focus, permission และ loading/error/empty states; `bun run build` ใช้เมื่อจำเป็นและต้องไม่ชน dev server ที่ใช้ `.next` เดียวกัน
- เอกสารอ้างอิงอย่างเดียวตรวจ JSON, path และลิงก์ ไม่ต้องรัน product tests รายงานผล FE checks แยกจาก Backend/Gateway checks ที่ไม่ได้รัน
