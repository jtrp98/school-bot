# School Bot API — C# Clean Architecture

กฎ Backend อยู่ที่ [AGENTS.md](AGENTS.md) และแยกรายละเอียดโครงสร้าง/gRPC/tests ใน `rules/` ให้อ่านตามขอบเขตงาน
ตำแหน่ง Gateway checkout สำหรับ agent ตั้งด้วย `gatewayRoot` ใน `local.json` ที่ root ของ School Bot
คัดลอก [service-school-bot/local.example.json](local.example.json) ไปเป็น `local.json` ที่ root เพื่อปรับค่า; หากไม่กำหนด ใช้ `C:/src/schoolbright/target/sb-api-gateway`
Relative path อิง root ของ School Bot ไฟล์ `local.json` ถูก git-ignore และไม่ใช่ config ของ API runtime
ตัวอย่างเดียวกันมี `teacherWebRoot` สำหรับ [งานเว็บครู](../docs/teacher-web.md) ใช้เมื่อทำงานเชื่อม Frontend/BFF กับ Gateway และ School Bot
และ `adminWebRoot` สำหรับ [หน้า Admin FE](../docs/admin-web.md) ใน `sb-web-helper` ใช้อ้างอิงเฉพาะ Frontend

```text
ServiceSchoolBot.API ───────────→ Application ───→ Domain
       └───────────────→ Infrastructure ──→ Application
```

`Api` เป็น Composition Root ที่เลือก Infrastructure Repository; Domain ไม่มี dependency ต่อ ASP.NET, EF หรือ Infrastructure

| Layer | รับผิดชอบ |
|---|---|
| Domain | License invariants, activation/expiry/device rules, Release validation |
| Application | Use cases, repository interfaces และ DTOs; ไม่รู้ HTTP/EF |
| Infrastructure | Repository อ่านเขียนผ่าน DbContext ที่ scaffold แยกตามฐาน; memory/file ใช้เฉพาะโหมดพัฒนาและทดสอบ |
| Api | gRPC services, trusted Gateway identity, DI, error trailers และ health endpoint |

Repository interfaces อยู่ใน `Application/Databases/Repository/Interfaces/` และ implementation อยู่ใน
`Infrastructure/Repositories/Databases/` แยก Project, License, Curriculum, CurriculumImport, Assessment,
WebSession, Idempotency และ Legacy (MasterAccount, SchoolProfile, Logs, Grade) ตาม aggregate
ส่วน backing stores, snapshots และ replay cipher อยู่ใน `Infrastructure/Repositories/Storage/`
Services ใช้ interface เฉพาะ aggregate; `SchoolBotUnitOfWork` เป็นเจ้าของ DbContext และ transaction scope ร่วม
`ISchoolBotRepositoryScope` ใช้เฉพาะ callbacks ที่ตรวจสิทธิ์และเปลี่ยนหลาย aggregate ภายใน commit เดียว
DI ลงทะเบียน repository แบบ scoped จาก UoW เดียว ใช้ identity ของ PostgreSQL แทนตัวนับใน memory

Repository เดิมรับ DbContext โดยตรง: Project, License, WebSession, Curriculum, CurriculumImport,
Assessment และ Idempotency ใช้ `SchoolBotDbContext`; MasterAccount ใช้ Master context,
SchoolProfile ใช้ School context, Logs ใช้ Logs context และ Grade ใช้ PGGrade context
ไม่มี adapter หรือ repository ชุด `Postgres` ซ้อนอีกชั้น

ค่าเริ่มต้นใช้ PostgreSQL และไม่มี fallback ไป memory เมื่อเชื่อมต่อไม่ได้
โหมดจำลองต้องตั้ง `SchoolBot__Storage=Memory` พร้อม `ASPNETCORE_ENVIRONMENT=Development` เท่านั้น
implementation จำลองเดิมอยู่ใน `Repositories/Storage/Memory/` เพื่อรองรับ development และ tests
services/principal resolvers ที่ใช้ repository เป็น scoped; replay/nonce caches เดิมแชร์ต่อ host แยกจาก DbContext

PostgreSQL ลงทะเบียน `SchoolAssessmentAuthority` สำหรับ assignment/room/term/explicit enrollment และ
`PostgresGradeDestination` สำหรับช่องเกรดเดิม พร้อม worker resolver ที่ตรวจครูผู้ยืนยันใหม่ทุกครั้ง
ตั้ง `SchoolBot__LegacyAuthority__TimeZoneId` ให้ตรง timestamp ของ School DB; history ที่ซ้ำ ย้าย หรือสถานะกำกวมยังถูกปฏิเสธ
โค้ดเขียนคะแนน/คำนวณ/receipt เพิ่มแล้วแต่ยังไม่รันตามคำสั่ง code-only และยังต้องมี `GradeExportWriteAuthorization`
จาก composition ที่ทบทวนหลักฐานจริงก่อน confirm/apply; ไม่มี config boolean สำหรับข้ามการรับรองสูตรหรือ coexistence

การเขียน SchoolBot ใช้ transaction รวม mutation/audit/idempotency; ผลที่เตรียมไม่สำเร็จ rollback ทั้งหน่วย
schema ปัจจุบันมีเฉพาะ primary keys จึงใช้ PostgreSQL advisory transaction lock ร่วมสำหรับ mutations
เพื่อกัน race ข้าม instance การเพิ่ม unique constraints และแบ่ง lock รองรับ throughput สูงเป็นงานต่อไป
ETag ใช้เวลาความละเอียด microsecond ให้ตรง PostgreSQL และอ่านข้อมูลใหม่ทุกหน่วย transaction
Curriculum/QuestionBank replay ต้องตั้ง `SCHOOL_BOT_REPLAY_KEY` เป็น base64 ของกุญแจ 32 bytes ที่คงเดิมข้าม restart
Project create ที่มี idempotency key ใช้ durable commit/replay และ release metadata ใช้ repository-backed service แล้ว
Project update/copy และ replay/nonce caches ของ License เดิมยังมีส่วนที่เป็นข้อมูลระดับ process
`TestLicenseTokenSigner` ยังคงเป็น signer ของ PoC ตามสัญญาเดิม

Curriculum import ยังปฏิเสธการเขียนจริง เพราะ scaffold ของฐานปัจจุบันไม่มี `SchoolBotLesson.ImportSource`
ต้องรัน migration เดิมและ re-scaffold ก่อน ไม่ทิ้งข้อมูล provenance หรือแก้ schema อัตโนมัติ
SchoolProfile ไม่อนุมานสิทธิ์ admin จากประเภทบุคลากร; สิทธิ์ต้องมาจาก trusted principal/role ที่มีหลักฐาน

## Transport และตัวตน

Client → HTTP Gateway → gRPC SchoolBot; browser ผ่าน same-origin BFF
HTTP ของธุรกิจอยู่ที่ Gateway ใต้ `/api/service-school-bot/` โดยไม่มี v1/v2
SchoolBot มี typed RPC 5 services: `ProjectService`, `LicenseService`, `CurriculumService`,
`AssessmentService` และ `ReleaseService`; proto ต้นฉบับอยู่ใน
`src/ServiceSchoolBot.Infrastructure/Protos/ServiceSchoolBot/` และ Gateway ต้องใช้สำเนาที่ hash ตรงทุกไบต์
ดู routes, headers, ETag และ error contract ใน [คู่มือ API](../docs/API.md)

เปิด `ServiceSchoolBot.slnx` ใน IDE หรือรันจาก `service-school-bot/`:

```powershell
dotnet run --project src/ServiceSchoolBot.API --no-launch-profile
dotnet test ServiceSchoolBot.slnx --disable-build-servers -m:1
```

SDK เลือก Stable .NET 10 ผ่าน `global.json` (`allowPrerelease: false`)
เมื่อไม่ได้ตั้ง binding เอง Kestrel เปิด gRPC h2c ที่ `127.0.0.1:8080` และ health HTTP/1.1 ที่
`127.0.0.1:8081`; HTTP เหลือ `/healthz` ไม่มี business REST หรือไฟล์ `.http` เดิม
Gateway ตรวจ JWT จาก `POST /api/auth/login` เพียงจุดเดียว ใช้ full-login token (`AccessLevel=1`);
temporary token (`AccessLevel=0`) ถูกปฏิเสธ Gateway หา `TUsers.cType` จากโรงเรียน/ผู้ใช้ใน JWT แล้วส่ง metadata ตัวตน
TAdmin (`admin_id`) ได้ `staff_admin` พร้อมทุก role ส่วน `IsAdmin` หมายถึงแอดมินโรงเรียน
Desktop ส่ง `X-Device-Entitlement`; SchoolBot ตรวจ signature, expiry, school, current binding, epoch และ device thumbprint
พนักงานที่ไม่มี entitlement ใช้ `school_web`; นักเรียนต้องมี desktop entitlement
SchoolBot ไม่มี login/logout, opaque session token หรือ web-session exchange ของตัวเองแล้ว

Gateway มี `AddSchoolBotModule` และ gRPC repositories แต่ยังไม่ต่อเข้า Program ตาม gate H5/G-AUTH
`DenyAllGatewayPeerVerifier` ปฏิเสธ peer ทุกตัวโดยปริยาย และ `ProductionSchoolBotTransportValidator`
ไม่เปิด production trust เมื่อยังไม่มี transport จริง Tests เปลี่ยน verifier/transport เฉพาะ host ของการทดสอบ
Desktop ใน `app/` ใช้เส้นทาง Gateway พร้อม native JWT และ entitlement แล้ว; การเปิดใช้จริงยังต้องผ่าน transport/authority gates
หน้า Admin เนื้อหาใน sb-web-helper ใช้ login เดิมแล้วส่ง JWT assertion อายุ 60 วินาทีจากเซิร์ฟเวอร์ ดู [Phase 5](../docs/phase5-implementation-2026-11-04.md)

## License ผ่าน gRPC บน stub

ส่วนนี้อธิบายโหมด Memory ที่เลือกอย่างชัดเจนใน Development; release metadata ใช้ repository/backing store ตามโหมดที่เลือก
License และ binding ใช้ authoritative snapshot จาก backing store ทุกครั้งที่อ่านหรือเขียน
Memory แชร์ lock ของ store object; file store แชร์ lock ตาม absolute directory ภายใน process เดียว
การตรวจ current binding, epoch, identity, expiry และ ETag อยู่ใต้ lock เดียวกับการ publish
Validate/activate เครื่องเดิมแก้เฉพาะเวลา lease และสร้าง signed response ก่อน publish
File store เผยแพร่ License/binding พร้อมกันใน `licenses_snapshot.json` ด้วยการแทนที่ครั้งเดียว
หากยังไม่มี snapshot จะอ่าน `licenses.json`/`bindings.json` เดิม แล้วเขียน snapshot เมื่อ commit สำเร็จครั้งแรก
เมื่อมี snapshot แล้วจะไม่อ่านไฟล์ legacy กลับมาทับ generation ปัจจุบัน

Device challenge และ activate/validate ไม่ต้องมี JWT แต่ยังตรวจ key และ signed device proof
ค่า path ที่เซ็นยังเป็น canonical `/v2/licenses/activate` หรือ `/v2/licenses/validate` ภายใน proof
ไม่ใช่ URL HTTP ที่ต้องเรียก; HTTP จริงอยู่ใต้ Gateway prefix ที่ไม่มี version ห้ามเปลี่ยน canonical path เพราะย้าย route
หลักประกันของ store ครอบคลุมหนึ่ง process หลาย adapters/store objects ไม่ครอบคลุมหลาย process หรือ PostgreSQL
Audit/idempotency result ของ License ยังไม่รวมใน snapshot; replay ไม่รองรับข้าม instance/restart
Admin authorization รับ context ณ request และยังไม่มี authoritative re-check ก่อน commit
`TestLicenseTokenSigner` เป็น signer ของ stub ต้องมี production key/signing management ก่อนแจก entitlement จริง
Publish release เปิดเผย metadata เท่านั้น ไม่ตรวจ artifact/signature หรืออนุมัติการติดตั้ง

## Curriculum idempotency บน stub

Create, metadata, content และ publish ใช้ `IIdempotentSchoolBotCommit` ผ่าน repository/UoW
ตรวจสิทธิ์ซ้ำและทำ mutation/audit/result ภายใต้ lock ของ backing store หน่วยเดียว
`GatewayPrincipalResolver` สร้าง `RecheckAsync` ให้ตรวจ school/profile/license/binding ที่เกี่ยวข้องภายใน transaction เดิม
Memory แชร์ store object; File แชร์ absolute directory ภายใน process เดียวและแทนที่ `curricula_snapshot.json`
ครั้งเดียวพร้อมสมาชิก `idempotency`; snapshot เดิมที่ไม่มีสมาชิกนี้อ่านเป็น ledger ว่าง
ทุก mutation อ่าน snapshot สดก่อนตรวจ preconditions รวมผู้เรียก adapter โดยตรงที่ไม่มี request key

ขอบเขต key คือ (SchoolId, Principal, Operation, RequestKey) โดยส่วนกลางใช้ SchoolId=NULL
และ Principal=`staff_admin:{PrincipalId}` ซึ่ง principal ของ TAdmin คือ `admin:{admin_id}`
คำขอซ้อนรอ lock แล้ว replay เมื่อ hash ตรง หรือคืน `IDEMPOTENCY_CONFLICT` เมื่อ hash ต่าง
ตรวจสิทธิ์ปัจจุบันก่อน replay และตรวจ stale ETag เฉพาะ mutation ใหม่
คำขอภายในที่ไม่ส่ง key ใช้ UUID ภายใน commit ไม่รับประกัน replay ให้ผู้เรียก
Projects/License ใช้ idempotency ของ service แยกต่างหาก

ผล replay เข้ารหัส AES-GCM รวม status/body/resource/ETag อายุ 24 ชั่วโมงจาก commit; retry ไม่ต่ออายุ
เมื่อ now >= ReplayUntil คืน `IDEMPOTENCY_EXPIRED` และคง tuple กันทำซ้ำ
ผลเข้ารหัสที่เปิดไม่ได้คืน `STATE_CONFLICT` โดยไม่รัน mutation ซ้ำ
`SCHOOL_BOT_REPLAY_KEY` รับกุญแจ 32 bytes แบบ base64 จาก environment และต้องคงเดิมเมื่อเปิด file store ใหม่
ไม่เก็บกุญแจลง snapshot; หากไม่กำหนดใช้กุญแจสุ่มเฉพาะ process สำหรับ stub จึงกู้ replay หลัง restart ไม่ได้
การจัดการ/หมุนกุญแจและ custody จริงยังต้องผ่านเงื่อนไข production
Atomicity นี้ครอบคลุมหลาย adapters ใน process เดียว ไม่ครอบคลุมหลาย OS processes, PostgreSQL หรือ power loss
ผลบน stub ไม่ปิด DBA-003/G-CORE หรือ H5/G-AUTH

## Course Plan importer บน stub

`POST /api/service-school-bot/admin/curricula/{id}/import` ที่ Gateway รับแถว normalize เข้า draft revision ที่มีอยู่
ใช้ `staff_admin` + `content_author` และ `Idempotency-Key` โดยไม่ใช้ `If-Match`
แต่ละแถวมี `source_row_key`, `source_position`, `source_approved` (true/false/null) และ `lesson`
นำเข้าใหม่ได้ `created`; identity/hash เดิมได้ `unchanged`; ชนเนื้อหาเดิมได้ `409 STATE_CONFLICT` ทั้ง batch
ค้นก่อนเขียนและ commit Lesson/Activity/audit/ledger พร้อมกัน การเปลี่ยนลำดับไม่เปลี่ยน hash และไม่ย้ายบทเรียนเดิม
normalized DTO ไม่มีคอลัมน์ O; admin detail คืน `lessons[].import_source` ซึ่งเป็นที่มา/การอนุมัติต้นทาง ไม่ใช่การ publish
Public detail คงรูปเดิม; content PUT รักษาที่มาตาม LessonKey ไม่ให้ client ตั้งหรือล้างเอง
File adapter เปิดใหม่ใน process เดิม replay ได้เมื่อคงกุญแจเข้ารหัส ยังไม่ครอบคลุม PostgreSQL/หลาย process, manifest หรือเนื้อหาจริง

## คลังข้อสอบและการอ่าน assessment บน stub

Gateway `/api/service-school-bot/admin/question-banks` รองรับ create/list/detail/PUT draft/publish แบบ four-eyes
create/PUT ต้องมี `content_author`; publish ต้องมี `content_approver` และ `approval_ref` ไม่เกิน 1,000 ตัวอักษร
PUT/publish ใช้ `If-Match`; mutations ใช้ helper idempotency เดียวกับหลักสูตร
bank/audit/ledger commit ใน `curricula_snapshot.json` พร้อมกัน โดยคง format เดิม; collection ใหม่ที่ว่างละ key ออก
`GET /api/service-school-bot/course-runs/{id}/activities/{activity_id}` อ่าน policy/window ที่กำหนดไว้จริง
`GET /api/service-school-bot/attempts` และ `attempts/{id}` อ่านประวัติ/เฉลยจาก snapshot เดิมของ Attempt
เรียงข้อ/ตัวเลือกตาม snapshot และซ่อนคะแนน/เฉลยตาม reveal policy
การ publish bank revision ใหม่ไม่เปลี่ยน Activity, CourseRun หรือ Attempt และไม่มี worker เปลี่ยนผลระหว่างอ่าน

`GatewayAssessmentAuthResolver` รับ Gateway identity ผ่าน curriculum principal แล้วแยก employee/learner
ส่วน `IAssessmentAuthority` สำหรับ roster/School DB ยังเป็น `UnconfiguredAssessmentAuthority`
ซึ่งปฏิเสธด้วย `AUTH_NOT_CONFIGURED` จนกว่าจะเชื่อม authority จริง
Tests ใช้ authority fake และตรวจ memory/file หลาย adapters ใน process เดียว
ผลนี้ไม่รับรอง production authority, PostgreSQL, หลาย process หรือการเริ่ม/submit Attempt จริง

## CourseRun และ progress บน stub

`GET /api/service-school-bot/progress?course_run_id=…` อ่านหลักสูตรฉบับที่ CourseRun เลือก แล้วคืน Level,
Activity และ Mission ตามลำดับเดิม ไม่ใช้ระดับชั้นของห้องแทน Level ของหลักสูตร
รองรับ `curriculum_id` และ `mode=block|arm|code`; เมื่อระบุทั้ง run/curriculum ต้องตรงกัน
Progress รับเฉพาะ desktop student ที่ profile/License binding ยังใช้ได้ และตรวจ roster/ห้อง/ปี/เทอมผ่าน authority จริง
การอ่านไม่สร้าง Attempt, Submission, audit หรือ idempotency result และไม่สั่งอุปกรณ์
ผลที่ยังไม่มีคืน `null`; stub ยังไม่มี Submission/Assessment store จึงไม่มี project result ให้รวม
Attempt เลือกผลที่ตรวจเสร็จแล้วและซ่อนคะแนนตาม reveal policy; Test รวมสอบซ่อมใช้ best
Tests ใช้ seed/fake authority เฉพาะ test ไม่ใช่ผลตรวจ UI, School DB, PostgreSQL, หลาย process หรือ hardware

## Session storage ที่คงไว้

logout, web-session exchange และ PostgreSQL logout store ถูกลบแล้ว ไม่ต้องตั้งค่า logout/admin-key environment เดิม
`IWebSessionRepository` และ session records/backing storage ยังอยู่เพื่อรักษารูปแบบ snapshot
ไม่ได้ใช้เป็น authentication ของ SchoolBot; การลบ storage นี้ต้องเป็นงานเปลี่ยน snapshot แยกต่างหาก

## EF Core database-first scaffold

Legacy scaffold อยู่ใน `src/ServiceSchoolBot.Infrastructure/Entities/<Db>/` และ contexts อยู่ใน
`src/ServiceSchoolBot.Infrastructure/Common/Databases/` ตามโครง reference `sb-tech-upgrade-academic`
โดยเปลี่ยนเฉพาะ namespace ไม่คัดลอก `BaseDbContext` ที่ปรับ column types/timestamps และไม่รวม GradeSummary/GradeHistory

| DB | Context | Entity files | Entities ใน EF model |
|---|---|---:|---:|
| Master | `JabjaiMasterSingleDBPostgreSQLDBContext` | 472 | 299 |
| School | `JabjaiSchoolSingleDBPostgreSQLDBContext` | 301 | 299 |
| Logs | `JabjaiSchoolLogsPostgreSQLDBContext` | 77 | 77 |
| PGGrade | `PGGradeDBPostgreSQLDBContext` | 48 | 48 |
| SchoolBot | `SchoolBotDbContext` | 32 | 32 |

จำนวน files และ EF model ของ reference ต่างกัน เพราะมี entity files ที่ context ไม่ได้ map
legacy ชุดนี้คัดลอกจาก reference ไม่ใช่ผลยืนยัน schema ของ DB ปัจจุบัน
SchoolBot scaffold สร้างจาก live read-only metadata แล้ว มี 32 tables และ 450 mapped properties
ใน `Entities/SchoolBotDb/` และ `Common/Databases/SchoolBotDbContext.cs`
การรันเดิมถูก sandbox network ปฏิเสธ TCP ด้วย `SocketError.AccessDenied` ไม่ใช่หลักฐานว่า DB down
ภายหลัง `dotnet tool restore` และ scaffold สำเร็จด้วย runner ที่เข้าถึง NuGet และเครือข่าย DB ได้
ห้ามสร้าง model จากการเดา schema หรือเรียก migration เพื่อให้ scaffold ผ่าน

DI ลงทะเบียน legacy contexts และ `SchoolBotDbContext` แบบ scoped โดยยังไม่เปิด connection ตอนเริ่มแอป
อ่านผ่าน `IDatabaseConnectionConfig` เดิม: environment มีลำดับก่อน `ConnectionStrings:<Db>`
เมื่อ resolve context ที่ยังไม่มี connection จะคืน error ที่ระบุชื่อ setting โดยไม่ใส่ค่า
ไม่เปิด sensitive logging/error details; business repositories ใช้ context ที่ตรงฐาน ส่วน memory/file แยกเป็น development/test
การ resolve/model self-check ไม่ยืนยัน DB connectivity หรือสิทธิ์ใช้งานจริง

ตั้งค่าจาก `service-school-bot/.env.example` ใน `service-school-bot/.env` ซึ่ง gitignore แล้ว ใส่เฉพาะ DB connections ที่ใช้งาน
รักษาค่าที่มีอยู่ และไม่เก็บ connection จริงใน appsettings/source/คำสั่ง CLI
API ไม่โหลด `.env` เอง: ส่ง environment เข้า process ผ่านระบบ secrets ของเครื่องหรือ deployment เดิม
สคริปต์ scaffold โหลดเฉพาะ DB setting ที่เลือกจาก `.env` และให้ process environment มีลำดับก่อน
รองรับค่า plain หรือครอบด้วย single/double quotes; ไม่ทำ shell expansion หรือรองรับ multiline

รันจาก repository root:

```powershell
# Restore local EF tool ตาม manifest ใน service-school-bot/ ก่อนใช้งานครั้งแรก
Push-Location service-school-bot
dotnet tool restore
dotnet restore ServiceSchoolBot.slnx --disable-build-servers -m:1 -p:NuGetAudit=false
Pop-Location

# สร้าง SchoolBot จาก metadata จริง
./scripts/api/scaffold-databases.ps1 -Database SchoolBot

# สร้าง legacy ใหม่ (ตรวจ diff ก่อนและหลัง; -Force ยอมให้ทับ generated files ของ DB ที่เลือก)
./scripts/api/scaffold-databases.ps1 -Database Master -Force
./scripts/api/scaffold-databases.ps1 -Database School -Force
./scripts/api/scaffold-databases.ps1 -Database Logs -Force
./scripts/api/scaffold-databases.ps1 -Database PGGrade -Force
```

ใช้บัญชีที่ได้รับสิทธิ์อ่าน metadata เท่านั้น สคริปต์บังคับ PostgreSQL session เป็น read-only
ผ่าน connection Options และ `PGOPTIONS`, ใช้ named connection เพื่อไม่ส่ง secrets ผ่าน command line,
และ `--no-onconfiguring` เพื่อไม่ฝัง connection ใน generated code ไม่เรียก DDL, migrations, seed หรืออ่าน business rows
raw diagnostics ถูกซ่อนเพราะ provider อาจใส่ connection details ใน error
เมื่อคำสั่งล้มเหลวให้ตรวจ tool restore/build, network และสิทธิ์ metadata โดยไม่เผย connection/logs ที่มี secrets
`-Force` ไม่ลบ entity files ที่ schema ใหม่เลิกใช้ ต้องตรวจและลบเฉพาะไฟล์ generated ที่ยืนยันว่าเลิกใช้แล้ว
อย่าแก้ generated mappings ด้วยมือ ให้แยก customization เป็น partial class ตามรูปแบบ EF

ตรวจ scaffold/config โดยไม่ต่อ DB (รันจาก repository root):

```powershell
./scripts/tests/scaffold-databases.test.ps1
Push-Location service-school-bot
dotnet build tools/ServiceSchoolBot.DatabaseSelfCheck --disable-build-servers -m:1 -p:NuGetAudit=false
dotnet run --project tools/ServiceSchoolBot.DatabaseSelfCheck --no-build --no-restore
dotnet test ServiceSchoolBot.slnx --disable-build-servers -m:1 -p:NuGetAudit=false
Pop-Location
```

Self-check ตรวจ legacy และ SchoolBot models/DI รวม 32 tables/450 properties, primary keys และฟิลด์สำคัญ
พร้อม config errors และการลงทะเบียน repository แบบ scoped; script tests ใช้ mock CLI ตรวจ read-only/secret handling
ไม่เปิด connection และไม่ใช่ผลตรวจรับของ reviewer หรือการทดสอบ DB runtime

ตรวจ repository กับ PostgreSQL แยกจากฐานร่วม:

```powershell
# ใช้ฐานชั่วคราวบน loopback ชื่อ schoolbot_repo_tests ที่มี schema SchoolBot 32 ตารางแล้ว
$env:SCHOOL_BOT_REPOSITORY_TEST_CONNECTION="Host=127.0.0.1;Port=<isolated-port>;Database=schoolbot_repo_tests;Username=<test-user>;Password=<test-password>"
dotnet run --project service-school-bot/tools/ServiceSchoolBot.RepositorySelfCheck -p:NuGetAudit=false
```

คำสั่งนี้รันจาก repository root ตรวจการบันทึกข้าม DbContext, rollback, ETag, replay, audit และ tenant filters
สร้าง legacy fixtures และข้อมูลสังเคราะห์เฉพาะฐานทดสอบดังกล่าว ห้ามตั้งค่าเป็นฐานร่วม
ตัวตรวจปฏิเสธ host อื่นและชื่อฐานอื่น; ไม่อ่าน connection จาก `service-school-bot/.env` และไม่เรียก migrations
