# Backend / C# gRPC API

ใช้ร่วมกับกฎแม่ที่ [../AGENTS.md](../AGENTS.md) อ่าน [README.md](README.md) ก่อนแก้ API

- Clean Architecture: Domain ไม่ขึ้นกับ Application/Infrastructure/API; Application ไม่ขึ้นกับ Infrastructure/API หรือ gRPC generated types
- HTTP business endpoints อยู่ที่ Gateway; SchoolBot เปิด typed gRPC และ health ตาม contract เดิม ไม่เพิ่ม REST ซ้ำ
- ไฟล์เกิน 300 บรรทัดให้ทบทวนหน้าที่/ความซ้ำ เกิน 500 ให้ตัดสินใจแยกหน้าที่หรืออธิบายเหตุผลที่เก็บรวม ดูเกณฑ์และข้อยกเว้นในกฎโครงสร้าง
- ใช้ของเดิมก่อน เช่น `SchoolBotGrpcRunner`, `SchoolBotGrpcMetadata` และ proto mappers ไม่เพิ่ม helper/service ซ้อนโดยไม่มีผู้ใช้จริง

## อ่านเฉพาะงานที่เกี่ยวข้อง

| งาน | กฎรายละเอียด |
|---|---|
| เพิ่ม/ย้าย/แก้ C# และจัดขอบเขตคลาส | [โครงสร้าง ชื่อไฟล์ และการใช้ร่วมกัน](rules/structure.md) |
| RPC, proto, metadata, auth, error, Gateway | [gRPC และ Gateway](rules/grpc.md) |
| เพิ่ม/แก้ tests หรือเตรียมตรวจ behavior | [หมวดและขอบเขต tests](rules/testing.md) |

ไม่ต้องเปิดรายละเอียดทุกไฟล์ตั้งแต่เริ่ม กฎเหล่านี้ใช้เมื่อทำงานในขอบเขตนั้น หากแก้ behavior ให้อ่านกฎ tests ด้วย

## ตำแหน่ง Gateway บนเครื่อง

อ่าน `gatewayRoot` จาก `local.json` ที่ root ของ School Bot หากไม่มีไฟล์หรือไม่ได้กำหนด key ใช้ `C:/src/schoolbright/target/sb-api-gateway` เป็นค่าเริ่มต้น Relative path ให้อิง root ของ School Bot; ค่าที่กำหนดแต่ใช้ไม่ได้ต้องรายงาน ไม่เงียบกลับไปใช้ repo อื่น ดูตัวอย่าง [local.example.json](local.example.json)

ค่านี้เป็นตำแหน่งเอกสาร/source สำหรับ agent ไม่ใช่ URL หรือ configuration ของ API runtime อ่านเฉพาะ handbook/contract ที่เกี่ยวข้องจาก Gateway ไม่ import กฎการทำงานทั้ง repo มาใช้กับ School Bot หากไม่มี checkout ให้อ่าน contract ใน School Bot ต่อและระบุสิ่งที่ยังตรวจข้าม repo ไม่ได้

เมื่องานเกี่ยวข้องกับ Frontend/BFF ของเว็บครู ให้อ่าน [แนวทางเว็บครู](../docs/teacher-web.md) และใช้ `teacherWebRoot` จาก `local.json` เดียวกัน

## ตรวจงาน

- C# API: รัน `dotnet test ServiceSchoolBot.slnx --disable-build-servers -m:1` จาก `backend` เมื่อเปลี่ยน behavior
