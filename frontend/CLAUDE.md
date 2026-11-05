@AGENTS.md

## AI Workflow (Plan first)

- ทุก session ใน repo นี้ (Claude Code / Codex / Copilot บนเครื่องของ Light) ใช้ "AI Workflow — Plan first" จาก instruction ระดับเครื่อง: Claude `~/.claude/CLAUDE.md` · Codex `~/.codex/AGENTS.md` · Copilot `%APPDATA%\Code\User\prompts\ai-workflow.instructions.md`
- สรุป: ประกาศระดับงาน S/M/L → Superpowers (Requirement → Spec → Plan → Test → Implement → Review) → Remember (claude-mem + memory) → Design (`/impeccable polish` งาน frontend) → Learn (task-observer, บทเรียนต้องให้คนอนุมัติ)
- แผน/สเปกของ repo นี้วางที่ `_docs/requirement/<initiative>/` — ห้ามใช้ `docs/superpowers/`, ไม่ auto-commit เอกสารแผน
- คำสั่งทดสอบ/verify ใช้ตามที่ไฟล์นี้หรือ README ของ repo ระบุ — ไม่พบให้ถามผู้ใช้ ห้ามเดา
- ลำดับความสำคัญ: คำสั่งผู้ใช้ > กฎของ repo นี้ > workflow ระดับเครื่อง > skill ทั่วไป
- แผนต้นทาง: sb-web-helper `_docs/requirement/overall/ai-workflow-renewal/`
