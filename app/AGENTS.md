# Desktop Host (Go + Wails)

ใช้ร่วมกับกฎแม่ที่ [../AGENTS.md](../AGENTS.md) กฎ Desktop UI อยู่ใน [frontend/AGENTS.md](frontend/AGENTS.md) ส่วนงาน IPC engines อยู่ใน [../engines/AGENTS.md](../engines/AGENTS.md)

## หน้าที่ของ Host และการจัดการหน้าต่าง
- ขอบเขต: Go Host (`app/`) ทำหน้าที่เป็น bridge ระหว่าง Wails กับ OS/Engines ส่วน C# API อยู่ใน `backend/`
- **Trusted Engine Routing**: Host เป็นผู้เลือก engine binary จาก routing table ที่กำหนดไว้เท่านั้น ห้ามรับ executable path จาก frontend หรือ project data
- **Window Management**: ดูแลหน้าต่าง frameless/fullscreen และควบคุมปุ่มย่อ/ขยาย/ปิดใน header แอป พร้อมทั้งรักษา close guards
- **Engine Process Lifecycle**: จัดการเปิด/ปิด process ของ engine ผ่าน JSON Lines UTF-8 (stdin/stdout) พร้อมทั้งส่ง cancellation และ drain คิวงานก่อนหยุดอย่างปลอดภัย
- **Why-only Comments**: คอมเมนต์เฉพาะเหตุผลความปลอดภัยของ OS/Process, hardware timing หรือ workaround ทางเทคนิคเท่านั้น

## คำสั่งตรวจงาน
- รัน `go test ./...` และ `go vet ./...` จากโฟลเดอร์ `app`
- ก่อน build หรือ package Desktop ให้อ่าน [../scripts/AGENTS.md](../scripts/AGENTS.md)
