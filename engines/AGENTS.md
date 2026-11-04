# Engines

ใช้ร่วมกับกฎแม่ที่ [../AGENTS.md](../AGENTS.md) อ่าน [README.md](README.md) และ README ของ engine ที่จะเปลี่ยน

- Engine อยู่ใน `engines/<engine-id>/` แยก source, dependencies, tests, build scripts และ metadata ของตัวเอง
- ใช้ภาษาที่เหมาะกับ engine เดิมได้ เช่น C++ ไม่ต้องเขียนใหม่ด้วย Go และไม่ผูก engine ID กับ mode ID แบบหนึ่งต่อหนึ่ง
- `scripts/engines/build.*` มีหน้าที่ dispatch ไม่รวม compiler logic ของทุก engine เข้าด้วยกัน
- ไม่มีเกณฑ์จำนวนบรรทัดแบบ Frontend ให้แบ่งตามหน้าที่จริงและโครงสร้างของภาษาที่ engine นั้นใช้

## ตรวจงาน

- C++ engine: ใช้ `scripts/engines/build.ps1 -Engine zide -Test` และ IPC tests กับ binary ที่ build จาก source ล่าสุด ใช้ dry-run สำหรับการตรวจอัตโนมัติ
- ก่อน build/package ให้อ่าน [../scripts/AGENTS.md](../scripts/AGENTS.md)
