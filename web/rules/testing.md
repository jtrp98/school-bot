# หมวดและขอบเขต tests ของ Backend

ปัจจุบันใช้ xUnit ใน `web/tests/ServiceSchoolBot.Tests/` คง project เดิม ไม่สร้าง test project/framework เพิ่มเพียงเพื่อจัดหมวด กฎโฟลเดอร์นี้ใช้กับไฟล์ใหม่หรือไฟล์ที่อยู่ในขอบเขตงาน ไม่ต้องย้าย tests เดิมทั้งหมด

## การวางไฟล์และตั้งชื่อ

| ตำแหน่งใต้ test project | สิ่งที่ตรวจ |
|---|---|
| `Unit/<Feature>/` | Domain/Application logic โดยไม่ใช้ network หรือ DB จริง |
| `Integration/Grpc/<Feature>/` | RPC ผ่าน host/client, metadata, status/trailers และ DI |
| `Integration/Persistence/<Feature>/` | Repository/transaction กับ isolated test DB |
| `Contract/<Feature>/` | Proto parity, mapping, optional/default/error compatibility และ Gateway contract |
| `Architecture/` | Dependency direction และ layer boundaries |
| `TestSupport/<Feature>/` | Fixtures, seeds, fake clocks และ test clients ตามขอบเขตใช้ร่วมกัน |

- ชื่อไฟล์และคลาสใช้ `<Subject>Tests.cs` เช่น `AttemptEvaluatorTests.cs`, `AssessmentGrpcServiceTests.cs`; fixture ใช้ `<Feature>Fixture.cs` และชื่อ test บอกเงื่อนไขกับผลที่คาดหวัง รักษารูปแบบเดิมเมื่อแก้คลาสเดิม
- เพิ่ม `[Trait("Category", "Unit")]`, `Integration`, `Contract` หรือ `Architecture` ตามประเภทใน test class ใหม่; integration แยกได้ด้วย `[Trait("Transport", "Grpc")]` หรือ `[Trait("Storage", "PostgreSql")]` เพื่อ filter ได้ ไม่ติด label ว่า integration DB หากทดสอบเพียง Memory
- Fixture เฉพาะฟีเจอร์อยู่กับหมวดนั้นก่อน ย้ายไป `TestSupport` เมื่อมีการใช้ร่วมกันจริง ไม่สร้าง fixture กลางที่เก็บ state ของทุก tests

## เลือกสิ่งที่จะตรวจ

- Logic ใหม่/แก้ bug: ตรวจ happy path, validation และ edge/failure ที่เกี่ยวข้องกับความเสี่ยง ไม่เขียน tests ที่เพียงทวน getters, delegation หรือรายชื่อไฟล์
- RPC: ตรวจ response พร้อม status/trailers, invalid input, NotFound, auth/tenant isolation และ cancellation/deadline ตามส่วนที่เปลี่ยน อย่าใช้การเรียก service ตรงเป็นหลักฐานว่า gRPC transport ผ่าน
- Mutation: ตรวจ rollback, audit/idempotency, replay กับ payload ต่างกัน, concurrency/ETag และ tenant recheck ตาม behavior ที่แก้ ไม่เพิ่ม retry ที่อาจเขียนซ้ำ
- Proto/Gateway: ตรวจ hash ของชุด proto และ mapping ที่เปลี่ยน รวม default/optional และ error contract; หากไม่มี Gateway checkout ให้ระบุขอบเขตที่ยังตรวจไม่ได้
- Persistence: Memory tests ไม่พิสูจน์ SQL, database constraints หรือ transaction จริง ใช้ isolated DB ตาม [web/README.md](../README.md) สำหรับงานส่วนนั้น ไม่ใช้ฐานร่วม/production และไม่รัน migration ของ legacy schema เพื่อทดสอบ
- ใช้ fake clock และ deterministic synchronization แทน sleep หรือเวลาจริงเมื่อตรวจ expiry/race แยก state ต่อ test ให้รันซ้ำ/ขนานได้ตามขอบเขต fixture

## คำสั่งและการรายงาน

- จาก `web/`: `dotnet test ServiceSchoolBot.slnx --disable-build-servers -m:1` เมื่อแก้ behavior; ระหว่างพัฒนา filter เฉพาะ tests ที่เกี่ยวข้องได้ แล้วตรวจชุดที่ได้รับผลกระทบก่อนส่งมอบ
- งานย้ายคลาส/ไฟล์ให้ตรวจ build และ references; เอกสารอย่างเดียวตรวจ diff/ลิงก์ ไม่ต้องรัน tests หรือ rebuild installer
- รายงาน build, unit, integration, DB และ Gateway checks แยกตามที่รันจริง ห้ามลบ/skip/ผ่อน assertion หรือปิด auth เพื่อให้ผ่าน
