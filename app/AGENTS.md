# Desktop / Go + Wails

ใช้ร่วมกับกฎแม่ที่ [../AGENTS.md](../AGENTS.md) กฎ Frontend อยู่ใน [frontend/AGENTS.md](frontend/AGENTS.md) อ่านเฉพาะเมื่อทำงานส่วนนั้น ส่วนงาน engine อ่าน [../engines/AGENTS.md](../engines/AGENTS.md)

## Host และหน้าต่างแอป

- Desktop อยู่ใน `app/`: Go + Wails และ React/TypeScript ส่วน C# API อยู่ใน `web/`
- Host เป็นผู้เลือก engine จาก trusted routing เท่านั้น ห้ามรับ executable path จาก frontend หรือ project file
- รักษาการเปิดแบบ fullscreen/frameless และปุ่มหน้าต่างใน header ของแอป รวมถึง close guards

## ตรวจงาน

- Go host/engine manager: รัน `go test ./...` และ `go vet ./...` จาก `app` โดยใช้ SDK ตาม repository
- ก่อน build/package Desktop หรือ installer ให้อ่าน [../scripts/AGENTS.md](../scripts/AGENTS.md)
