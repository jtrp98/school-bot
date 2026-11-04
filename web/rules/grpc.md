# gRPC และการทำงานร่วมกับ Gateway

อ่าน [web/README.md](../README.md) และ [docs/API.md](../../docs/API.md) เป็น contract ของ School Bot ก่อน เมื่อแตะการเชื่อม Gateway ให้ resolve `gatewayRoot` ตาม [web/AGENTS.md](../AGENTS.md) แล้วอ่านเฉพาะส่วนที่เกี่ยวข้องใน `_docs/handbook/05-microservice-grpc-standard.md`; งาน route อ่าน `_docs/handbook/phase-1/01-naming-and-routing-conventions.md` เพิ่ม

Handbook เป็นแนวทางกลาง ต้องเทียบโค้ด/contract School Bot จริงด้วย ไม่เปลี่ยน ports, auth, error envelope หรือจัดโครงสร้างใหม่ทั้งหมดเพียงให้ตรงตัวอย่าง หากขัดกันให้รายงานความต่าง

## Contract และ transport

- Client → HTTP Gateway → typed gRPC SchoolBot; business HTTP อยู่ใต้ `/api/school-bot/service/` ไม่เพิ่มเวอร์ชันหรือ route สำรองเอง และไม่เปิด business REST ที่ School Bot
- รักษา binding ที่ตั้งค่าไว้: ค่าเริ่มต้น local คือ gRPC h2c `127.0.0.1:8080` และ health `127.0.0.1:8081` ไม่เปลี่ยน network exposure ระหว่าง refactor
- RPC service ต้องบาง: ตรวจ transport/identity, map request, เรียก Application, map response ใช้ `SchoolBotGrpcRunner`/metadata/error mapping เดิม Business rules ไม่อยู่ใน generated types หรือ gRPC handlers
- ส่ง `ServerCallContext.CancellationToken` ต่อถึง service/repository/DB และรักษา deadline ห้ามกลืน cancellation หรือทำ retry ของ mutation โดยไม่มี idempotency contract รองรับ
- ใช้ช่องทาง error/trailers และ HTTP mapping เดิมผ่าน `ErrorStatus`/`SchoolBotGrpcMetadata` ไม่สร้าง envelope ใหม่ ไม่แปลง error เป็นผลสำเร็จหรือเปิดเผย stack trace/secrets

## Proto และ naming

- Source of truth อยู่ `web/src/ServiceSchoolBot.Infrastructure/Protos/ServiceSchoolBot/`; หนึ่ง `.proto` ต่อ aggregate ตามเดิม เช่น `Assessment.proto`, `Project.proto`
- message/service/RPC ใช้ PascalCase, fields ใช้ snake_case; รักษา package และ `csharp_namespace` เดิม ไม่แก้ generated C# ด้วยมือ
- ห้ามเปลี่ยน/reuse field numbers; เมื่อลบ field ให้ reserved number/name รักษา optional/default semantics และความเข้ากันได้ของ client เก่า ประเมิน breaking change ก่อนเปลี่ยน type, enum หรือความหมาย
- เมื่อแก้ proto ให้ sync สำเนาที่ `<gatewayRoot>/src/ApiGateway.Infrastructure/Protos/ServiceSchoolBot/` จากต้นฉบับ ตรวจ SHA-256 ตรงกันทุกไฟล์ ไม่แก้สำเนา Gateway แยกเอง การเปลี่ยนข้าม repo ต้องอยู่ในขอบเขตงานที่ได้รับและทำตามกฎของ repo ปลายทาง
- แก้ RPC ต้องตรวจ caller, Gateway mapping/repository/controller, DTO, DI และ contract/integration tests ที่เกี่ยวข้องด้วย ไม่อ้างว่า sync หรือ end-to-end ผ่านหากยังตรวจอีกฝั่งไม่ได้

## ตัวตนและข้อมูล

- ตรวจ trusted Gateway peer ก่อนเชื่อ metadata; ใช้ resolver เดิมในการหา school/user/role ไม่รับ tenant หรือสิทธิ์จาก request body แทน trusted identity
- รักษา desktop entitlement, roster/authority และ fail-closed behavior ห้ามเปิด bypass หรือ fallback เพื่อให้เรียกได้ขณะ auth/transport ยังไม่ configured
- Tenant data ต้องกรองสิทธิ์และโรงเรียนตอนอ่าน/เขียน รักษา transaction ของ mutation + audit + idempotency และ recheck ภายใต้ commit เดียว
- Log ใช้ request/trace identifiers เท่าที่จำเป็น ไม่ log token, entitlement, secrets หรือ payload นักเรียนเต็ม ๆ
