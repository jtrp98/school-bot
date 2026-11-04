# School Bot API — C# Clean Architecture

```text
SchoolBot.Api ───────────→ Application ───→ Domain
       └───────────────→ Infrastructure ──→ Application
```

`Api` เป็น Composition Root ที่เลือก Infrastructure Repository
Domain ไม่มี dependency ต่อ ASP.NET, EF หรือ Infrastructure

| Layer | รับผิดชอบ |
|---|---|
| Domain | License invariants, activation/expiry/device rules, Release validation |
| Application | Use cases, store interface, DTOs; ไม่รู้ HTTP/EF |
| Infrastructure | In-memory repository (สลับเป็น PostgreSQL adapter ในอนาคต) |
| Api | HTTP routing ปัจจุบัน, JSON options, DI, admin auth, rate limiting, error mapping |

Repository interfaces อยู่ใน `Application/Databases/Repository/Interfaces/` และ implementation อยู่ใน
`Infrastructure/Repositories/Databases/` แยก Project, License, Curriculum, CurriculumImport, Assessment,
WebSession, Idempotency และ Legacy (MasterAccount, SchoolProfile, Logs, Grade) ตาม aggregate
ส่วน backing stores, snapshots, replay cipher และ PostgreSQL logout store อยู่ใน `Infrastructure/Repositories/Storage/`

Services ใช้ interface เฉพาะ aggregate; `SchoolBotUnitOfWork` เป็นเจ้าของ repository state และ transaction scope ร่วม
`ISchoolBotRepositoryScope` ใช้เฉพาะ callbacks ที่ต้องตรวจสิทธิ์และเปลี่ยนหลาย aggregate ภายใน commit เดียว
Repository เก็บ query/persistence และเรียกกฎของ service ภายใน atomic callback เดิมก่อน publish snapshot
DI ลงทะเบียนทุก repository จาก UoW เดียว จึงไม่มี lock หรือชุดตัวนับแยกต่อ repository

เป้าหมายคือ client→HTTP Gateway→gRPC SchoolBot; browser ผ่าน same-origin BFF. HTTP ที่มีอยู่และคำสั่ง local ด้านล่างไม่ยืนยัน gRPC integration. RPC/HTTP mapping และ trusted JWT tenant กับ opaque session ยังต้องออกแบบ; Login HTTP เดิมของ Gateway และ outcome decoder ไม่เปลี่ยน.

เปิด `SchoolBot.slnx` ใน IDE หรือรัน:

```powershell
dotnet run --project src/SchoolBot.Api --no-launch-profile
dotnet test SchoolBot.slnx --disable-build-servers -m:1
```

SDK เลือก Stable .NET 10 ผ่าน `global.json` (`allowPrerelease: false`)
Admin token อ่านจาก environment `SCHOOL_BOT_ADMIN_KEY`; ไม่มี Secret ใน appsettings/source
ค่าเริ่มต้น bind `127.0.0.1:8080` ไม่มี CORS สำหรับเว็บอื่น เพราะ Desktop ติดต่อผ่าน Go Host

**PoC limitation ของ v1 License/Release:** Store หายในทุก restart, lock ป้องกัน concurrent activation ได้เฉพาะ process เดียว
ก่อนหลาย Instance ให้ทำ DB transaction/unique constraints, audit, identity/RBAC, HTTPS deployment และ key/signing management
`Publish` ใน PoC ทำให้ Metadata มองเห็นได้เท่านั้น ไม่รับรองว่า Artifact/Signature ถูกต้อง
ห้ามใช้เป็นระบบจำหน่าย License หรือเผยแพร่ Updates จริงจนกว่างานเหล่านี้เสร็จ

Admin Website ยังไม่ได้สร้าง API HTTP examples อยู่ใน `src/SchoolBot.Api/SchoolBot.Api.http`

## License v2 บน stub

License และ binding ใช้ authoritative snapshot จาก backing store ทุกครั้งที่อ่านหรือเขียน
Memory store แชร์ lock ของ store object; file store แชร์ lock ตาม absolute directory ภายใน process เดียว
การตรวจ current binding, epoch, identity, expiry และ ETag เกิดภายใต้ lock เดียวกับการ publish
Validate และ activate เครื่องเดิมแก้เฉพาะเวลา lease และสร้าง signed response ก่อน publish

File store เผยแพร่ License และ binding พร้อมกันใน `licenses_snapshot.json` ด้วยการแทนที่ไฟล์ครั้งเดียว
หากยังไม่มี snapshot จะอ่าน `licenses.json` และ `bindings.json` เดิม แล้วเขียน snapshot เมื่อ commit สำเร็จครั้งแรก
เมื่อมี snapshot แล้วจะไม่อ่านไฟล์ legacy กลับมาทับ generation ปัจจุบัน

หลักประกันนี้ครอบคลุมหนึ่ง process ที่มีหลาย adapter/store object เท่านั้น ไม่ครอบคลุมหลาย process หรือ PostgreSQL
Audit และ idempotency result ยังไม่รวมใน snapshot; replay ของ service ยังไม่รองรับข้าม instance/restart
Admin authorization ยังรับ context ณ request และยังไม่มี authoritative re-check ก่อน commit
จึงยังใช้ผลบน stub นี้รับรอง production ไม่ได้

## Curriculum idempotency บน stub

Curriculum ทั้ง create, metadata, content และ publish ใช้ `IIdempotentSchoolBotCommit` ผ่าน repository และ `SchoolBotUnitOfWork`
ตรวจสิทธิ์ซ้ำและทำ mutation/audit/result ภายใต้ lock ของ backing store หน่วยเดียว
Memory แชร์ store object; File แชร์ absolute directory ภายใน process เดียวและแทนที่
`curricula_snapshot.json` ครั้งเดียวพร้อมสมาชิก `idempotency` เดิมที่ไม่มีสมาชิกนี้อ่านเป็น ledger ว่าง
ทุก mutation อ่าน snapshot สดก่อนตรวจ preconditions รวมผู้เรียก adapter โดยตรงที่ไม่มี request key

ขอบเขต key คือ (SchoolId, Principal, Operation, RequestKey) โดยส่วนกลางใช้ SchoolId=NULL
และ Principal=staff_admin:{PrincipalId} จาก session คำขอซ้อนรอ lock แล้ว replay ผลเดิมเมื่อ hash ตรง
หรือคืน IDEMPOTENCY_CONFLICT เมื่อ hash ต่าง ตรวจสิทธิ์ปัจจุบันก่อนเปิด replay และตรวจ stale ETag เฉพาะ mutation ใหม่
คำขอภายในที่ไม่ส่ง key ใช้ UUID ภายในหน่วย commit และไม่รับประกัน replay ให้ผู้เรียก
Projects และ License ยังคงใช้เส้นทางเดิม

ผล replay เข้ารหัสด้วย AES-GCM รวม status/body/resource/ETag อายุ 24 ชั่วโมงจากเวลา commit
retry ไม่ต่ออายุ เมื่อ now >= ReplayUntil คืน IDEMPOTENCY_EXPIRED และคง tuple กันทำซ้ำ
ผลเข้ารหัสที่เปิดไม่ได้คืน STATE_CONFLICT โดยไม่รัน mutation ซ้ำ
`SCHOOL_BOT_REPLAY_KEY` รับกุญแจ 32 bytes ในรูป base64 จาก environment ของ server และต้องคงเดิมเมื่อเปิด file store ใหม่
ไม่เก็บกุญแจลง snapshot หากไม่กำหนดใช้กุญแจสุ่มเฉพาะ process สำหรับ stub เท่านั้น
จึงกู้ replay หลัง restart process ไม่ได้ในกรณีนั้น การจัดการ/หมุนกุญแจและ custody จริงยังต้องผ่านเงื่อนไข production

หลักประกัน atomicity นี้ครอบคลุมหลาย adapters ใน process เดียว ไม่ครอบคลุมหลาย OS processes,
PostgreSQL, power loss หรือ Gateway จริง การตรวจบน stub ไม่ปิด DBA-003/G-CORE หรือ H5

## Course Plan importer บน stub

`POST /v2/admin/curricula/{id}/import` รับแถวที่ normalize แล้วเข้า draft revision ที่มีอยู่
ใช้สิทธิ์ `staff_admin` + `content_author` และ `Idempotency-Key` โดยไม่ใช้ `If-Match`
แต่ละแถวต้องมี `source_row_key`, `source_position`, `source_approved` (true/false/null) และ `lesson`
นำเข้าใหม่ได้ `created`; identity และ hash เดิมได้ `unchanged`; ชนเนื้อหาเดิมได้ `409 STATE_CONFLICT` ทั้ง batch
ค้นก่อนเขียนและ commit Lesson/Activity/audit/ledger ในหน่วยเดียวกันผ่าน helper เดิม
การเปลี่ยนลำดับไม่เปลี่ยน hash และไม่ย้ายบทเรียนเดิม; normalized DTO ไม่มีคอลัมน์ O

Admin detail คืน `lessons[].import_source` พร้อมที่มาและสถานะอนุมัติจากต้นทางซึ่งไม่ใช่การ publish
Public detail คงรูปเดิม; content PUT รักษาที่มาตาม LessonKey และไม่ให้ client ตั้งหรือล้างที่มาเอง
File adapter เปิดใหม่ใน process เดิม replay ผลเดิมได้เมื่อคงกุญแจเข้ารหัส
หลักประกันนี้ยังไม่ครอบคลุม PostgreSQL/หลาย process, manifest หรือเนื้อหาจริง และ Gateway

## คลังข้อสอบและการอ่าน assessment บน stub

`/v2/admin/question-banks` รองรับ create, list, detail, PUT draft และ publish แบบ four-eyes
create/PUT ต้องมี `content_author`; publish ต้องมี `content_approver` พร้อม `approval_ref` ไม่เกิน 1,000 ตัวอักษร
PUT/publish ใช้ `If-Match`; ทุก mutation ใช้ helper idempotency เดียวกับหลักสูตร
bank, audit และ ledger commit ใน `curricula_snapshot.json` พร้อมกัน โดยคง format และไฟล์เดิม
collection ใหม่ที่ไม่มีข้อมูลละ key ออก จึงรักษารูป snapshot ของหลักสูตรเดิม

`GET /v2/course-runs/{id}/activities/{activity_id}` อ่าน policy และ window ที่กำหนดไว้จริง
`GET /v2/attempts` และ `GET /v2/attempts/{id}` อ่านประวัติและเฉลยจาก Snapshot เดิมของ Attempt
เรียงข้อและตัวเลือกตาม snapshot; ซ่อนคะแนนและเฉลยตาม reveal policy
การเผยแพร่ bank revision ใหม่ไม่เปลี่ยน Activity, CourseRun หรือ Attempt และไม่มี worker เปลี่ยนผลระหว่างอ่าน

assessment ใช้ `IAssessmentAuthResolver` และ `IAssessmentAuthority` ที่ต้องเชื่อม trusted session/School DB
ค่าเริ่มต้นปฏิเสธด้วย `AUTH_NOT_CONFIGURED`; ไม่มี production token หรือ roster จำลอง
self-check ลงทะเบียน authority/session fake เฉพาะ test และตรวจหนึ่ง host สอง services/adapters
ทั้ง memory store object เดียวและ file store หลาย objects บน directory เดียว รวม reader ที่สามและ reopen
ผลนี้ไม่รับรอง Gateway/gRPC, PostgreSQL, หลาย process, หรือการเริ่ม/submit Attempt จริง
## CourseRun และ progress บน stub

`GET /v2/progress?course_run_id=…` อ่านหลักสูตรฉบับที่ CourseRun เลือก แล้วคืน Level,
Activity และ Mission ตามลำดับเดิม ไม่ใช้ระดับชั้นของห้องแทน Level ของหลักสูตร
รองรับ `curriculum_id` และตัวกรอง `mode=block|arm|code`; เมื่อระบุทั้ง run และ curriculum ต้องตรงกัน
Progress รับเฉพาะ desktop student ที่ session/profile และ License binding ยังใช้งานได้
และตรวจ roster/ห้อง/ปี/เทอมผ่าน `IAssessmentAuthority` ซึ่งค่าเริ่มต้นปฏิเสธจนกว่าจะเชื่อม authority จริง

opaque desktop session ใช้ resolver เดียวกับการอ่านหลักสูตร ตรวจ student/employee แยกจากกัน
assessment resolver อ่าน profile จาก session ใน store ไม่รับตัวตนจาก request headers เพิ่มเติม
การอ่านไม่สร้าง Attempt, Submission, audit หรือ idempotency result และไม่สั่งอุปกรณ์
ผลที่ยังไม่มีคืน `null`; stub ปัจจุบันยังไม่มี Submission/Assessment store จึงไม่มี project result ให้รวม
Attempt ที่มีอยู่เลือกผลที่ตรวจเสร็จแล้วและซ่อนคะแนนตาม reveal policy เดิม; Test รวมสอบซ่อมใช้ best

self-check ใช้ fixture builder seed ก่อนสร้าง services หนึ่ง host สอง services/adapters ทั้ง memory/file
พร้อม reader ที่สามและ file reopen, directory/roster fake และ fake robot เฉพาะ test
ยังไม่ใช่ผลทดสอบ UI/component จริง, School DB, PostgreSQL, หลาย process หรือ hardware
## Logout v2 และ PostgreSQL

`POST /v2/auth/logout` รับ opaque Bearer token แบบ base64url 43 ตัว ใช้ SHA-256 ค้นหา session
และบันทึก revoke พร้อม audit ใน transaction เดียวของ PostgreSQL โดยไม่ใช้ memory store ของ v1
ไม่ต้องส่ง body, License, `If-Match` หรือ `Idempotency-Key`; use case logout ปัจจุบันไม่เรียก Gateway ออกไป. การเปิดรับ HTTP local นี้ยังไม่ใช่เส้น client→Gateway→gRPC SchoolBot
token หาย/ผิดรูปแบบ/ไม่พบ/หมดอายุ/ถูกถอนแล้วคืน 204; ฐานข้อมูลไม่พร้อมคืน 503 `AUTH_UNAVAILABLE`
พร้อม v2 error envelope, `request_id` UUID และ `Cache-Control: no-store`

ก่อนเรียก service/store คำขอที่มี token รูปแบบ canonical ต้องผ่าน fixed-window limit ตาม connection peer
และ concurrency limit รวมสำหรับงาน logout ของ process นั้น โดยใช้ built-in .NET rate limiters และ queue เป็น 0 ทั้งคู่
token หาย/ผิดรูปแบบยังคืน 204 โดยไม่ใช้ quota และไม่เรียก store; token canonical ที่ถูกปฏิเสธคืน
429 `RATE_LIMITED` พร้อม v2 envelope, `request_id` UUID และ `Cache-Control: no-store`
`Retry-After` ส่งเฉพาะเมื่อ lease มี metadata ที่เป็นจำนวนวินาทีไม่ติดลบในช่วง integer
เป็นเวลาลองขอ local limiter ใหม่ ไม่ยืนยันว่า DB/Gateway พร้อมหรือ reset แล้ว; concurrency rejection ไม่มีเวลาคาดเดา

ตั้ง caps ผ่าน configuration/environment ก่อนเริ่มแอป (ต้องเป็นจำนวนเต็มบวก; ค่าผิดทำให้เริ่มแอปไม่สำเร็จ):

| ชื่อ | ค่าเริ่มต้น |
|---|---:|
| `SCHOOL_BOT_LOGOUT_RATE_LIMIT` | 30 คำขอที่มี token canonical ต่อ peer ต่อ window |
| `SCHOOL_BOT_LOGOUT_WINDOW_SECONDS` | 60 วินาที |
| `SCHOOL_BOT_LOGOUT_CONCURRENCY_LIMIT` | 8 งาน logout พร้อมกันต่อ process |

peer จับจาก connection ก่อนประมวลผล forwarded headers; IPv4-mapped IPv6 ใช้ bucket เดียวกับ IPv4
ไม่ใช้ `X-Forwarded-For`, header, token, account หรือ body เป็นตัวเลือก bucket; peer ที่ไม่มี address ใช้ bucket ร่วม
หลัง Gateway หลาย client อาจใช้ quota peer เดียวกัน นี่เป็น local defense ระหว่างรอ H5 trusted source
แต่ละ process/pod มี quota ของตัวเองและ restart แล้วเริ่มนับใหม่ ไม่ใช่ global cross-pod limit หรือ production SLA

ตั้ง environment `SCHOOL_BOT_DB_CONNECTION_STRING` ผ่าน secret reference ของระบบ deploy เท่านั้น
รูปแบบ placeholder: `Host=<host>;Port=<port>;Database=<database>;Username=<username>;Password=<secret>`
ไม่มีค่าเริ่มต้นหรือ fallback ฐานข้อมูล; อย่าใส่ connection string จริงใน source, logs หรือคำสั่งที่เก็บประวัติ
Npgsql 10.0.2 อ้างจาก package metadata ใน local NuGet cache และไม่เปิด connection ตอนเริ่มแอป
โค้ดไม่สร้าง schema/session/seed; ต้องเตรียม schema เดิมและสิทธิ์ DB ก่อนเปิดใช้งาน
งานนี้ยังไม่ครอบคลุม Login, persistence ของ v1, การตรวจรับ DB runtime หรือ deployment

## Web session exchange บน stub

`POST /v2/auth/web-sessions` ตรวจ trusted Gateway peer ผ่าน `ITrustedWebGatewayPeerVerifier`
ก่อนอ่าน token จาก header `JabjaiWebKey-*` เพียงหนึ่งค่า แล้วเรียก `IWebPrincipalIntrospection`
ภายใน 3 วินาที และ `IWebPrincipalDirectory` สำหรับ mapping บัญชี/profile ที่ยืนยันแล้ว
ค่าเริ่มต้นของ peer ปฏิเสธทั้งหมด และ introspection/directory คืน `AUTH_NOT_CONFIGURED`;
ต้องเชื่อม adapter จริงก่อนใช้งานผ่าน Gateway ไม่มีการเดา AccountId จาก EmployeeId

`staff_admin` ต้องมี StaffRole active; `school_web` ต้องมี License active
ทั้งสองไม่มี device binding อายุ session คือ 8 ชั่วโมง/30 นาทีตามลำดับ
และไม่เกิน source token expiry เก็บเฉพาะ SHA-256 lowercase ของ opaque token
session และ audit เผยแพร่พร้อมกันใน `curricula_snapshot.json` เพื่อใช้ atomic commit เดิม
snapshot เก่าอ่าน sessions จาก `sessions.json` จน snapshot ใหม่มี collection sessions
StaffRole ใน stub seed ผ่าน backing snapshot เท่านั้น; exchange ไม่เพิ่มหรือแก้ StaffRole/legacy

Curriculum resolver อ่าน session ใหม่จาก store และ resolve roles ใหม่ทุก request
ตรวจ session, school/employee และ StaffRole ซ้ำใน curriculum transaction ก่อน commit
named token fixtures เดิมใช้ resolver stub เฉพาะ environment Development สำหรับ regression
logout endpoint และ PostgreSQL logout registration เดิมไม่เปลี่ยน; self-check ใช้ logout store double ร่วมกับ session store

หลักประกันของ memory/file store ครอบคลุมหลาย adapter ใน process เดียวเท่านั้น
ยังไม่รับรอง PostgreSQL, หลาย process, Gateway/BFF จริง หรือการปิด H5/security gate

## Change Log

- 2026-10-23 — จำกัด logout ตาม connection peer และจำนวนงานพร้อมกันก่อนเข้า store พร้อม caps ที่ตั้งค่าได้และ v2 429
- 2026-10-23 — เพิ่ม logout v2 พร้อม PostgreSQL transaction สำหรับ session และ audit และ configuration ผ่าน environment

## EF Core database-first scaffold

Legacy scaffold อยู่ใน `src/SchoolBot.Infrastructure/Entities/<Db>/` และ contexts อยู่ใน
`src/SchoolBot.Infrastructure/Common/Databases/` ตามโครง reference `sb-tech-upgrade-academic`
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
ไม่เปิด sensitive logging/error details และไม่เปลี่ยน business repositories หรือ memory/file backing stores ให้ใช้ EF
การ resolve/model self-check ไม่ยืนยัน DB connectivity หรือสิทธิ์ใช้งานจริง

ตั้งค่าจาก `web/.env.example` ใน `web/.env` ซึ่ง gitignore แล้ว ใส่เฉพาะ DB connections ที่ใช้งาน
รักษาค่าที่มีอยู่ และไม่เก็บ connection จริงใน appsettings/source/คำสั่ง CLI
API ไม่โหลด `.env` เอง: ส่ง environment เข้า process ผ่านระบบ secrets ของเครื่องหรือ deployment เดิม
สคริปต์ scaffold โหลดเฉพาะ DB setting ที่เลือกจาก `.env` และให้ process environment มีลำดับก่อน
รองรับค่า plain หรือครอบด้วย single/double quotes; ไม่ทำ shell expansion หรือรองรับ multiline

รันจาก repository root:

```powershell
# Restore local EF tool ตาม manifest ใน web/ ก่อนใช้งานครั้งแรก
Push-Location web
dotnet tool restore
dotnet restore SchoolBot.slnx --disable-build-servers -m:1 -p:NuGetAudit=false
Pop-Location

# สร้าง SchoolBot จาก metadata จริง
./scripts/scaffold-databases.ps1 -Database SchoolBot

# สร้าง legacy ใหม่ (ตรวจ diff ก่อนและหลัง; -Force ยอมให้ทับ generated files ของ DB ที่เลือก)
./scripts/scaffold-databases.ps1 -Database Master -Force
./scripts/scaffold-databases.ps1 -Database School -Force
./scripts/scaffold-databases.ps1 -Database Logs -Force
./scripts/scaffold-databases.ps1 -Database PGGrade -Force
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
./scripts/tests/scaffold-databases.tests.ps1
Push-Location web
dotnet build tools/SchoolBot.DatabaseSelfCheck --disable-build-servers -m:1 -p:NuGetAudit=false
dotnet run --project tools/SchoolBot.DatabaseSelfCheck --no-build --no-restore
dotnet test SchoolBot.slnx --disable-build-servers -m:1 -p:NuGetAudit=false
Pop-Location
```

Self-check ตรวจ legacy และ SchoolBot models/DI รวม 32 tables/450 properties, primary keys และฟิลด์สำคัญ
พร้อม config errors และ memory backing store เดิม; script tests ใช้ mock CLI ตรวจ read-only/secret handling
ไม่เปิด connection และไม่ใช่ผลตรวจรับของ reviewer หรือการทดสอบ DB runtime
