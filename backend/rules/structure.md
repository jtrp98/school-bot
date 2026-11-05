# โครงสร้าง ชื่อ และขนาดไฟล์ Backend

ใช้กับ C# ที่เพิ่มหรือแก้ใน `backend/` รักษาโครงสร้างจริงตาม [backend/README.md](../README.md) ไม่ย้ายทั้ง repo เพียงให้ตรงตัวอย่าง

## ขอบเขตและ category

| Layer / ตำแหน่งใต้ `backend/src/` | หน้าที่ |
|---|---|
| `ServiceSchoolBot.API/GrpcServices/` | รับ/ตอบ RPC, proto mapping, trusted metadata และ error transport |
| `ServiceSchoolBot.Application/Services/Implementations/<Feature>/` | Use case, workflow และ validation ของฟีเจอร์ |
| `ServiceSchoolBot.Application/Databases/Repository/Interfaces/` | Repository contracts ตาม aggregate |
| `ServiceSchoolBot.Application/Common/Model/<Feature>/` | Application request/result ไม่ผูกกับ generated proto |
| `ServiceSchoolBot.Domain/<Feature>/` | Domain rules, records, value objects และ pure calculation |
| `ServiceSchoolBot.Infrastructure/Repositories/Databases/` | EF/database implementation ตาม aggregate |
| `ServiceSchoolBot.Infrastructure/Repositories/Storage/` | Stores/snapshots; `Memory/` สำหรับ development/tests |
| `ServiceSchoolBot.Infrastructure/Protos/ServiceSchoolBot/` | Proto ต้นฉบับของ School Bot |

- ใช้ feature เดิม เช่น `Assessment`, `Curriculum`, `Projects` ก่อนสร้างหมวดใหม่ Namespace ต้องตรงกับ layer/โฟลเดอร์ ไม่สร้าง `Helpers`, `Utils` หรือ `Common` เป็นที่รวมทุกอย่าง
- Domain ไม่อ้าง EF, ASP.NET หรือ gRPC; Application ไม่อ้าง Infrastructure/API; transport mapping อยู่ API และ persistence mapping อยู่ Infrastructure
- DI registration ใช้ extension เดิมของแต่ละ layer ไม่ยัด business logic ใน `Program.cs` และไม่สร้าง service locator

## ไฟล์ยาวและการแยกใช้ร่วมกัน

- เกิน 300 บรรทัด: ตรวจหลายหน้าที่ โค้ดซ้ำ และ dependency; เกิน 500: สรุปแนวทางแยกก่อนเพิ่มต่อ หรืออธิบายเหตุผลที่เก็บรวมตอนส่งมอบ จำนวนบรรทัดเป็นสัญญาณ ไม่ใช่ hard limit
- หากเป็นขั้นตอนย่อยที่ใช้เฉพาะคลาส ให้เริ่มจาก private method หากเป็น pure mapping/validation/calculation ที่มีหน้าที่ชัด แยกเป็นคลาสที่ตั้งชื่อตามงานได้; หาก workflow มี dependencies/side effects ของตนเองจึงแยกเป็น service
- โค้ดใช้จริงตั้งแต่สองจุดขึ้นไปและมีความหมายเดียวกันควรใช้ฟังก์ชัน/คลาสกลางใน layer ต่ำสุดที่ทั้งคู่พึ่งได้ ไม่ดึง business rules ลง `Common` เพื่อหลบ dependency และไม่สร้าง abstraction สำหรับผู้ใช้ในอนาคต
- ตัวอย่าง: proto mapping → `AssessmentProtoMapper`; domain calculation → `AttemptEvaluator`; use case ส่งงาน → `LearningSubmissionService` แทนการเพิ่ม wrapper ที่ส่งต่อเมธอดอย่างเดียว
- หนึ่งคลาสหลักต่อไฟล์เป็นค่าเริ่มต้น ไม่แยกคลาสเดียวเป็น `.Part1.cs`, `.Methods.cs`, `.Mapping.cs` หรือ partial ตามจำนวนบรรทัด หากต้องแยก ให้แยกหน้าที่และ dependencies จริง; generated/framework-required partial คงไว้ได้
- ไม่แตกฟังก์ชันจนต้องกระโดดหลายไฟล์เพื่ออ่านงานเดียว ไม่บีบหลายคำสั่งลงบรรทัดเดียว และไม่ขยาย refactor นอกขอบเขตงาน ยกเว้นเกณฑ์บรรทัดสำหรับ generated/scaffolded/vendor code
- เมื่อย้าย ต้องรักษา transaction, audit, idempotency, tenant filters, cancellation และ validation เดิม ตรวจ callers, DI, namespace, project references และ tests ให้ครบ

## การตั้งชื่อ

- ไฟล์/คลาส/โฟลเดอร์ C# ใช้ PascalCase และชื่อไฟล์ตรงคลาสหลัก เช่น `LearningSubmissionService.cs`; interfaces ใช้ `I` เช่น `IAssessmentRepository.cs`
- ใช้ suffix ให้ตรงหน้าที่: `GrpcService`, `Service`, `Repository`, `Mapper`/`ProtoMapper`, `Validator`, `Request`, `Result` ตาม pattern เดิม ไม่ใช้ชื่อ `Manager`, `Helper`, `Utils` กว้าง ๆ เมื่อระบุงานได้
- เมธอดใช้ชื่อกริยาที่บอกงาน; เมธอด async ใช้ `Async` ยกเว้น signature ที่ generated/framework กำหนด ตั้งตัวแปรตามความหมาย ไม่ใช้ตัวย่อที่ต้องเดา
- เก็บชื่อ contract เดิมที่มีผู้ใช้ ไม่เปลี่ยน public type, RPC หรือ namespace เพียงเพื่อจัดชื่อหรือ category ใหม่
