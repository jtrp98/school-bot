# เว็บครู — sb-tech-upgrade-web-system

ใช้เมื่ออ่านต้นแบบ FE ของเว็บครูหรือทำงานเชื่อมเว็บครู → BFF → Gateway → School Bot ไฟล์นี้เป็นแผนที่ไปยังต้นฉบับ ไม่แทนกฎของ repo เว็บครู และไม่ใช้โครงสร้าง Next.js มาบังคับ Desktop Frontend ใน `app/frontend/`

## ตำแหน่งบนเครื่อง

อ่าน `teacherWebRoot` จาก `local.json` ที่ root ของ School Bot หากไม่มีไฟล์หรือไม่ได้กำหนด key ใช้ `C:/src/schoolbright/target/sb-tech-upgrade-web-system` เป็นค่าเริ่มต้น ดู [ตัวอย่าง config](../backend/local.example.json)

Relative path อิง root ของ School Bot ค่าที่กำหนดแต่ใช้ไม่ได้ต้องรายงาน ไม่เลือก checkout อื่นเงียบ ๆ ค่านี้เป็น path เอกสาร/source สำหรับ agent ไม่ใช่ URL หรือ runtime config และ `local.json` ไม่เข้า Git หากไม่มี checkout ให้ระบุส่วนที่ยังตรวจข้าม repo ไม่ได้

## อ่านต้นฉบับตามงาน

ทุก path ในตารางอิง `<teacherWebRoot>` อ่านเฉพาะงานที่เกี่ยวข้อง ไม่โหลดทั้งชุดตั้งแต่เริ่ม เมื่อจะแก้ไฟล์ใน repo เว็บครู ต้องอ่านและปฏิบัติตาม `AGENTS.md`, `CLAUDE.md` และ `_docs/policy/project-policy.md` ของ repo นั้นก่อน

| งาน | ต้นฉบับที่ต้องตรวจ |
|---|---|
| โครงสร้าง ขนาดไฟล์ ชื่อ และการใช้ร่วมกัน | `.claude/rules/structure.md` |
| UI, i18n, tokens และ component กลาง | `DESIGN.md`, `.claude/rules/frontend.md`, `_docs/upgrade/ux-phase-1/07-component-catalog.md` |
| State และ workflow | `.claude/rules/state.md` |
| Loading/error states | `.claude/rules/loading.md` |
| BFF, API client และ upstream | `.claude/rules/bff-routes.md`, `src/server/upstream/registry.ts` |
| Auth/session | `.claude/rules/auth.md` |
| Tests และ project policy checks | `.claude/rules/testing.md`, `package.json`, `tests/project/` |

## แนวทาง FE ของเว็บครู

- `page.tsx` เป็นตัวประกอบหน้า ไม่เกิน 300 บรรทัดตามกฎเว็บครู แยก UI → `_components`, state ร่วม → `_stores`, workflow/effects → `_hooks`, HTTP client → `_api`, pure calculation/format → `_utils` ไม่แบ่งไฟล์เพียงให้จำนวนบรรทัดผ่าน
- ใช้หน้าเดียวให้อยู่กับหน้า หน้าย่อยตั้งแต่สองหน้าใช้ร่วมให้วาง `_shared/` ระดับที่ครอบผู้ใช้จริง ข้าม feature ใช้ `src/components/shared/` หรือ `src/services/` ตามหน้าที่ โค้ดกลางไม่ import ย้อนจากโฟลเดอร์หน้า
- ชื่อไฟล์/โฟลเดอร์ใช้ kebab-case ตามข้อยกเว้น App Router; component export ใช้ PascalCase, hook `use-<name>.ts`, store `use-<feature>-store.ts`, API `<resource>-api.ts` และ types `<resource>.types.ts` ตรวจชื่อและ export pattern จากต้นฉบับก่อนเพิ่ม
- ใช้ component กลางและ token เดิม ปรับด้วย props/slots ที่สื่อความหมาย ไม่ copy UI หรือใส่สี/ระยะเฉพาะหน้าทับมาตรฐาน ตรวจ implementation จริงเทียบ catalog เพราะบางรายการอาจอยู่ระหว่างอัปเกรด
- ใช้ Zustand สำหรับ state ร่วมตามกฎเว็บครู `useState` ใช้กับ UI ชั่วคราวของ component เดียว Store ไม่ fetch เอง ให้ hook เรียก `_api` แล้วอัปเดต store รักษาการ reset, selector และ hydration ตามต้นฉบับ
- ข้อความ UI ผ่าน `useTranslations()` กับ `messages/th.json`/`en.json` ของเว็บครู Loading ใช้ skeleton เฉพาะส่วนที่รอ พร้อม error/empty states และ accessibility ไม่คัดระบบ i18n สามภาษาหรือ host bridge ของ Desktop ไปใช้

## เชื่อมกับ School Bot และ Gateway

- Browser เรียก same-origin BFF ใต้ `src/app/api/` ใช้ upstream registry และ proxy helpers เดิม ไม่เรียก service gRPC หรือ URL backend ตรงจาก browser และไม่วาง secrets ใน client
- ตรวจ `src/server/upstream/registry.ts` ก่อนเพิ่ม alias/route ไม่เดาชื่อ upstream หรือถือว่า School Bot ลงทะเบียนแล้ว ใช้ Gateway HTTP contract จาก [API.md](API.md) และ [กฎ gRPC](../backend/rules/grpc.md)
- รักษา auth/session ของเว็บครู และตรวจการส่ง JWT/tenant/entitlement ตาม contract ของ Gateway ห้ามใช้ identity จาก request body หรือเพิ่ม auth bypass เพื่อให้หน้าเรียกได้
- งานข้าม repo ต้องตรวจ caller, BFF route, Gateway mapping และ RPC ที่เปลี่ยนให้ครบ แก้ repo ปลายทางตามขอบเขตงานและกฎของ repo นั้น ไม่อ้างว่า end-to-end ผ่านจากการทดสอบเพียงฝั่งเดียว

## Tests และการตรวจงาน

- Tests ของเว็บครูอยู่ `tests/` mirror path ของ `src/` เช่น `src/.../_utils/score.ts` → `tests/.../_utils/score.test.ts` ไม่วางข้าง source ใช้ Vitest/React Testing Library และ fixtures/reset state ตามต้นฉบับ
- Pure logic ตรวจ unit; component ตรวจ interaction และ loading/error/empty; store ตรวจ transitions/reset; BFF ตรวจ status/body, auth และ upstream failure ตามส่วนที่แก้ รักษา project tests เรื่องชื่อไฟล์ โครงสร้าง state และ loading ไม่เพิ่ม allowlist เพื่อหลบกฎ
- รันคำสั่งของเว็บครูจาก `<teacherWebRoot>` ตาม `package.json`: `bun run type-check`, lint ไฟล์ที่แก้ด้วย `bunx eslint <files>`, `bun run test` แบบเลือกชุดที่เกี่ยวข้อง และ `bun run docs:check` เมื่อแก้เอกสาร/กฎ ตรวจ build เมื่อขอบเขตงานต้องใช้
- งาน UI ต้องตรวจ browser ตามกฎเว็บครูบน localhost พร้อม keyboard, responsive และ console errors รายงาน unit/component/browser/Gateway checks แยกตามที่ตรวจจริง เอกสารอ้างอิงอย่างเดียวไม่ต้องรัน product tests
