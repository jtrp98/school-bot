# บันทึกโปรเจค school-bot (สำหรับคนและ Claude)

> เริ่มเขียนเมื่อ 2026-11-02 ช่วงเตรียมงาน (ยังไม่แก้โค้ด) · บันทึกเป็น snapshot ถ้าโค้ดเปลี่ยน ให้ตรวจโค้ดก่อนแล้วแก้บันทึก · กฎของ repo อยู่ที่ `AGENTS.md` (บันทึกนี้ไม่ได้แทนกฎ)

## สารบัญ

| ไฟล์ | เนื้อหา |
|---|---|
| [00-overview.md](00-overview.md) | โปรเจคคืออะไร, สถาปัตยกรรม, repo ที่เกี่ยวข้อง, เอกสารที่ไม่ตรงโค้ด, จุดระวัง |
| [10-host-and-engines.md](10-host-and-engines.md) | Go host, Engine Manager, Zide, Mixly, contracts |
| [20-frontend.md](20-frontend.md) | โครง frontend, design tokens, shared components, 3 modes |
| [40-env-and-checks.md](40-env-and-checks.md) | เครื่องมือที่มี/ขาด, ลำดับเตรียมเครื่อง, คำสั่งตรวจ, build/CI |
| [50-block-lab-plan-extract.md](50-block-lab-plan-extract.md) | สรุปแผน Block Lab เดิม (A–F, เฟส, test, 11 ข้อที่ต้องตัดสิน) + แมปเข้า school-bot |
| [60-sb-app-mixly.md](60-sb-app-mixly.md) | Electron Mixly เดิม: โครงสร้าง, งานค้าง, สิ่งที่ school-bot ยังไม่มี |
| [70-block-lab-ui-spec.md](70-block-lab-ui-spec.md) | สเปก UI จาก mockup Ver.1 (หน้าจอ, 16 หมวด, ข้อความ, การแมปกับ components) |
| [71-block-lab-design-system.md](71-block-lab-design-system.md) | design system ของ Block Lab: palette, สีหมวด, ตัวอักษร, layout, รูปทรงบล็อก, การทำใน Blockly, ทางเลือกการวาง token |
| [72-block-lab-block-catalog.md](72-block-lab-block-catalog.md) | รายการบล็อกที่จะใช้ (จากชีต PDF + สเปกบล็อกใหม่ .md) แมปกับ Mixly และ mockup |
| [99-handoff.md](99-handoff.md) | **อ่านก่อน** ถ้ารับงานต่อจาก session อื่น: สถานะ · งานถัดไป · กฎที่ผู้ใช้ย้ำ · วิธีตรวจ |
| [73-block-lab-a2-gaps.md](73-block-lab-a2-gaps.md) | ส่วนที่ยังไม่ตรง mockup A2 · ไฟล์ต้นฉบับของ mockup อยู่ที่ `.claude/design/block-lab/` |
| [80-block-lab-roadmap.md](80-block-lab-roadmap.md) | roadmap Block Lab ใน school-bot |
| [85-block-lab-requirements.md](85-block-lab-requirements.md) | REQ ของ Block Lab v1 พร้อมแหล่ง สถานะ และเกณฑ์ผ่าน |
| [90-open-questions.md](90-open-questions.md) | เรื่องที่ต้องตัดสินใจและคำถามค้าง |
| [95-claude-setup.md](95-claude-setup.md) | agents/skills ที่ตั้งไว้ วิธีใช้ และข้อแนะนำเพิ่ม |

## ศัพท์

| คำ | ความหมาย |
|---|---|
| Arm Lab | mode `robot-control` สำหรับคุมหุ่นแขนกล ผ่าน engine Zide (C++) |
| Block Lab | mode `block-programming` สำหรับต่อบล็อกเขียน Arduino UNO แบบ Mixly ปลายทางคือ engine mixly |
| Block Studio (ประถม) | mockup อีกชุดสำหรับเด็กประถม ใช้บล็อกคุมแขนกล ไม่ใช่ UNO |
| engine | process แยก คุยด้วย JSON Lines ผ่าน stdin/stdout โดย host เป็นผู้เรียกตาม routing ที่เชื่อถือได้ |
| preview | เปิด frontend ใน browser ด้วย `?preview=1` เป็นการจำลอง ไม่ได้คุยกับ native |
| dry-run | ค่าเริ่มต้นของ transport ใน engine ตรวจ input และคืนแผนงาน แต่ไม่แตะอุปกรณ์ |
| `.mix` | ไฟล์โปรเจค Mixly (Blockly XML) |
| toolbox / หมวด (category) | แถบรายการบล็อกด้านซ้าย |
| generator | JS ของ Mixly ที่แปลงบล็อกเป็นโค้ด Arduino C++ (รันใน goja ภายใน engine) |
| STA | software-team-agents: framework ทีม agent ของผู้ใช้ (`sta` CLI) และ knowledge root `mixly` |
| F1/F2/A–E | รหัสงานจากแผนเดิม ดูบันทึก 50 |
| SCR-xx / R17 / R21 / OQxx | รหัสอ้างอิงใน mockup ดูบันทึก 70 |
