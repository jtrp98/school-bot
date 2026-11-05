# การตั้งค่า Claude สำหรับ school-bot

> สร้างเมื่อ 2026-11-02 · ไฟล์ทั้งหมดใน `.claude/` ยังไม่ได้ commit ผู้ใช้เป็นคนตัดสินใจว่าจะ commit ให้ทีมใช้ร่วมหรือเก็บไว้ใช้คนเดียว

## ใช้อะไรเมื่อไร

| งาน | ใช้ |
|---|---|
| เริ่มงาน Block Lab ทุกครั้ง | skill `block-lab-context` (โหลดบันทึก + แหล่งข้อมูล) |
| หาว่าบล็อก/หมวด/ข้อความ/generator อยู่ไหน | agent `mixly-source-scout` (อ่านอย่างเดียว ใช้ sonnet เพื่อประหยัด) |
| เขียนโค้ด Block Lab (UI, client, routing) | agent `block-lab-dev` |
| แก้ engine mixly | agent `mixly-engine-dev` |
| เพิ่มหรือแก้ action / route mode ไป engine ใหม่ | skill `engine-action-change` |
| แก้ข้อความ UI | skill `i18n-strings` |
| ดึงโค้ด Mixly จาก sb-app-mixly | skill `mixly-sync` |
| ก่อนส่งงาน | skill `verify-changes` → agent `rules-reviewer` (+ `hardware-safety-reviewer` ถ้าแตะ engine/host/serial/upload) |
| ดู UI แบบ preview ใน Browser pane | `.claude/launch.json` → `frontend-preview` (ต้อง `npm ci` ก่อน) |
| ทบทวนโค้ดทั้ง branch | `/code-review` และ `/security-review` (มีในตัว) |

## ไฟล์ที่สร้าง

```text
.claude/
  agents/   block-lab-dev, mixly-engine-dev, mixly-source-scout, rules-reviewer, hardware-safety-reviewer
  skills/   block-lab-context, verify-changes, i18n-strings, engine-action-change, mixly-sync
  notes/    บันทึกชุดนี้
  launch.json
```

memory ส่วนตัว (`~/.claude/projects/C--src-school-bot/memory/`) เก็บไว้ 3 เรื่อง: ให้ตอบเป็นภาษาไทย, บริบทการรวมโปรแกรม และลิงก์ของแผน/mockup

## แนะนำเพิ่ม (ยังไม่ได้ทำ รอผู้ใช้อนุมัติ)

1. **Permissions allowlist** (เพื่อลดการขออนุญาตสำหรับคำสั่งอ่านอย่างเดียว) เช่น `Bash(npm run typecheck)`, `Bash(npm run test:i18n)`, `Bash(npm run build)`, `Bash(go test ./...)`, `Bash(go vet ./...)`, `Bash(rtk git status*)`, `Bash(rtk git diff*)` ใส่ใน `.claude/settings.local.json` (ใช้เองคนเดียว) หรือรัน `/fewer-permission-prompts` หลังทำงานไปสักพัก
2. **Hook เตือนขนาดไฟล์**: PostToolUse บน Edit/Write ถ้าไฟล์ `.ts/.tsx/.go` เกิน 300/500 บรรทัด ให้แสดงคำเตือนแบบไม่บล็อก ช่วยเรื่องกฎขนาดไฟล์ใน AGENTS.md
3. **Hook กัน git ที่เปลี่ยน state** (แบบ `block-git.js` ใน STA) ไม่แนะนำให้บล็อกเด็ดขาด เพราะบางครั้งผู้ใช้ก็สั่งให้ commit เอง
4. **ติดตั้ง `gh` CLI** ถ้าจะเปิด PR บน `jtrp98/school-bot` จาก Claude
5. **เชื่อมกับ STA**: ถ้าทีมใช้ pipeline BA→SA→UXUI→DEV ของ software-team-agents ให้เพิ่ม school-bot เป็น target ที่สองของ root `mixly` ด้วยคำสั่ง `sta configure target --target-id school-bot --name "School Bot" --remote-url https://github.com/jtrp98/school-bot.git --root mixly` (รันจากโฟลเดอร์ที่ไม่ใช่โฟลเดอร์แม่ของ knowledge root เช่น `C:\Users\jabja`) แล้วเพิ่ม path ใน `.workflow/targets.local.yaml` · ข้อนี้เป็นการตัดสินใจของทีม

## MCP / plugins

- เชื่อมอยู่แล้ว: Claude Docs (อ่านแผน), Figma, visualize · mockup เป็น Claude Design canvas จึงอ่านผ่าน Artifact tool ได้โดยไม่ต้องใช้ Figma
- ค้นใน registry แล้วไม่พบ MCP ของ GitHub, Arduino หรือ Blockly ที่เชื่อมได้
- plugin ที่พบ (เช่น `code-review`, Engineering) ซ้ำกับ `/code-review` ที่มีในตัวอยู่แล้ว จึงยังไม่จำเป็น
