# Backend API (C# gRPC Clean Architecture)

ใช้ร่วมกับกฎแม่ที่ [../AGENTS.md](../AGENTS.md) ศึกษา [README.md](README.md) ก่อนแก้ไขโค้ด API

## สถาปัตยกรรม Clean Architecture
- **Dependency Flow**: `API` → `Application` → `Domain` และ `Infrastructure` → `Application`
  - **Domain**: Pure business invariants, license rules, pure calculation; ปราศจาก dependency ต่อ ASP.NET, EF, หรือ gRPC
  - **Application**: Use cases, workflows, repository interfaces, และ DTOs; ไม่ขึ้นกับ persistence หรือ generated protos
  - **Infrastructure**: Database context, EF repositories, storage snapshots; memory mode ใช้เฉพาะการพัฒนา/ทดสอบ
  - **API**: Typed gRPC services, trusted Gateway metadata resolver, error trailers, และ health endpoints
- **API Boundary**: School Bot ให้บริการผ่าน typed gRPC (`127.0.0.1:8080`) และ health (`127.0.0.1:8081`) ห้ามเปิด REST endpoint ซ้ำซ้อน

## Path Configuration ข้ามระบบ (`local.json`)
- Agent resolve paths จาก `local.json` ที่ root ของ repo (อิง [backend/local.example.json](local.example.json)):
  - `gatewayRoot`: ตำแหน่ง repo API Gateway (Default: `C:/src/schoolbright/target/sb-api-gateway`)
  - `teacherWebRoot`: ตำแหน่ง Teacher Web (Default: `./frontend`)
  - `adminWebRoot`: ตำแหน่ง Admin Web FE (Default: `C:/src/schoolbright/target/sb-web-helper`)
- ค่าเหล่านี้เป็น path เอกสาร/contract สำหรับ Agent เท่านั้น ไม่ใช่ runtime config

## กฎขนาดไฟล์และคอมเมนต์
- **300 / 500 บรรทัด**: เกิน 300 ให้ทบทวนหน้าที่ซ้ำซ้อน; เกิน 500 ให้แยก service/mapper หรือสรุปเหตุผลในรายงานส่งมอบ
- 1 คลาสหลักต่อ 1 ไฟล์ (PascalCase) ห้ามสร้าง partial เพียงเพื่อหลบเกณฑ์บรรทัด และห้ามใช้ชื่อกว้างๆ เช่น `Helpers`, `Utils`
- **Why-only Comments**: ห้ามเขียน XML doc บรรยายซ้ำชื่อเมธอดหรือพารามิเตอร์ ใส่คอมเมนต์เฉพาะเหตุผลทางเทคนิค ความปลอดภัย หรือ transaction invariants เท่านั้น

## กฎเฉพาะทาง (อ่านตามขอบเขตงาน)
| งานที่เกี่ยวข้อง | เอกสารข้อกำหนด |
|---|---|
| โครงสร้างคลาส, DI, namespaces, และการแบ่ง layer | [rules/structure.md](rules/structure.md) |
| RPC contract, proto sync, metadata, auth, error | [rules/grpc.md](rules/grpc.md) |
| การเขียน Unit, Integration, Persistence, Contract tests | [rules/testing.md](rules/testing.md) |

## คำสั่งตรวจงาน
- รัน `dotnet test ServiceSchoolBot.slnx --disable-build-servers -m:1` จากโฟลเดอร์ `backend` เมื่อมีการแก้พฤติกรรมโค้ด
