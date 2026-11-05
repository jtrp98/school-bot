# SchoolBot API · Gateway → gRPC

Client ใช้ HTTP ที่ **Gateway** ใต้ `/api/service-school-bot/` โดยไม่มี version segment
Gateway แปลงเป็น typed gRPC ไป SchoolBot; browser เรียกผ่าน same-origin BFF
URL ในคู่มือนี้เป็นสัญญาของ module ไม่ใช่ endpoint HTTP ตรงที่ SchoolBot

**สถานะเปิดใช้งาน:** Gateway ยังไม่เรียก `AddSchoolBotModule` ใน Program ตาม gate H5/G-AUTH
Production slice ยังปิด และ `ProductionSchoolBotTransportValidator` ยังไม่ให้ trust เมื่อไม่มี transport จริง
SchoolBot ใช้ `DenyAllGatewayPeerVerifier` โดยปริยาย จึงปฏิเสธ peer จนกว่าจะเชื่อม trusted transport
ไม่ควรเปิด verifier แบบเชื่อทุก peer เพื่อใช้งาน production

## การเชื่อมต่อและสัญญา RPC

เมื่อไม่ได้ตั้ง binding เอง SchoolBot เปิด gRPC h2c ที่ `127.0.0.1:8080` และ health HTTP/1.1 ที่
`127.0.0.1:8081`; HTTP เหลือ `GET /healthz` ไม่มี business REST fallback
Proto ต้นฉบับอยู่ใน `service-school-bot/src/ServiceSchoolBot.Infrastructure/Protos/ServiceSchoolBot/`
ใช้ `package ServiceSchoolBot;` และ Gateway ต้องใช้สำเนาที่ hash ตรงทุกไบต์
field `authorization` เดิมถูก reserve ไม่ส่ง token ใน message; ตัวตนส่งผ่าน trusted metadata

| Service | สัญญาต้นฉบับ | หน้าที่ |
|---|---|---|
| `ProjectService` | [Project.proto](../service-school-bot/src/ServiceSchoolBot.Infrastructure/Protos/ServiceSchoolBot/Project.proto) | โครงงาน, revision, copy |
| `LicenseService` | [License.proto](../service-school-bot/src/ServiceSchoolBot.Infrastructure/Protos/ServiceSchoolBot/License.proto) | Device challenge, entitlement และ license admin |
| `CurriculumService` | [Curriculum.proto](../service-school-bot/src/ServiceSchoolBot.Infrastructure/Protos/ServiceSchoolBot/Curriculum.proto) | หลักสูตร, draft, publish, import |
| `AssessmentService` | [Assessment.proto](../service-school-bot/src/ServiceSchoolBot.Infrastructure/Protos/ServiceSchoolBot/Assessment.proto) | Question banks, activity policy, attempts, progress |
| `ReleaseService` | [Release.proto](../service-school-bot/src/ServiceSchoolBot.Infrastructure/Protos/ServiceSchoolBot/Release.proto) | Release metadata และ update check |

[Common.proto](../service-school-bot/src/ServiceSchoolBot.Infrastructure/Protos/ServiceSchoolBot/Common.proto) กำหนด error trailers ร่วม
JSON ของ Gateway ใช้ `snake_case`; รายละเอียดชนิดและ optional fields อยู่ใน proto และ Gateway DTOs

## Login และสิทธิ์

Login ใช้ `POST /api/auth/login` ของ Gateway จากระบบ login เดิมที่พอร์ตจาก mobile v3
คำขอที่ต้อง login ส่ง `Authorization: Bearer <Gateway JWT>` จาก full login (`AccessLevel=1`)
Temporary token (`AccessLevel=0`) ใช้ module นี้ไม่ได้
SchoolBot ไม่มี login, logout, web-session exchange หรือ admin bearer key ของตัวเอง

หน้า Admin เนื้อหาของ sb-web-helper ใช้เซสชันเดิมผ่าน BFF แล้วส่ง JWT ES256 อายุ 60 วินาทีใน
`X-SchoolBot-Staff-Assertion` ระหว่างเซิร์ฟเวอร์ Gateway ตรวจ `iss=sb-web-helper`,
`aud=school-bot-admin`, `scope=schoolbot:content` และ resolve TAdmin.Id2 จากเซสชันเป็น TAdmin.Id
scheme นี้ใช้เฉพาะ content Admin actions ที่ประกาศ policy ไว้ ไม่ใช้กับเส้นทางนักเรียน/ครูหรือ license admin
ดูค่าตั้ง signing/public key และข้อจำกัดใน [รายงาน Phase 5](phase5-implementation-2026-11-04.md)

Gateway ใช้ claims `PostalCode` เป็นโรงเรียนและ `Sid` เป็นผู้ใช้ แล้ว lookup `TUsers.cType` จากคู่นี้
ค่า `"0"` เป็น student; ค่าอื่นเป็น employee ไม่รับ tenant จาก body/query/header ของ client มาแทน JWT
การสร้าง license ของ staff ส่วนกลางมี `school_id` เพื่อระบุโรงเรียนเป้าหมาย ซึ่งไม่ใช่การ override tenant ของผู้ใช้

| ตัวตน | ความหมาย |
|---|---|
| `admin_id` | TAdmin ส่วนกลาง; channel `staff_admin` มี `content_author`, `content_approver`, `license_admin`; four-eyes ใช้ `admin:{admin_id}` |
| `IsAdmin` | แอดมินของโรงเรียน ไม่ใช่ staff ส่วนกลาง |
| JWT + `X-Device-Entitlement` ที่ตรวจผ่าน | Desktop student/employee; ตรวจ school/license/current binding/epoch/device thumbprint/expiry |
| Employee ไม่มี entitlement | `school_web` พร้อม school/profile/license ที่ใช้งานได้ |
| Student ไม่มี entitlement | ไม่ได้สิทธิ์ desktop |

Gateway ส่ง `x-school-id`, `x-user-id`, `x-user-type`, `x-admin-id`, `x-is-admin`, `x-employee-id`
และ `x-device-entitlement` ตามข้อมูลที่มี พร้อม `x-caller=api-gateway` และ `x-request-id`
SchoolBot เชื่อ metadata นี้เฉพาะ trusted peer ไม่ตรวจ JWT ของ client ซ้ำ
School/profile/license/binding ที่เกี่ยวข้องถูกตรวจซ้ำภายใน curriculum commit ผ่าน `RecheckAsync`

Device challenge, activate/validate และ update check เป็น **anonymous ที่ Gateway**
การไม่มี JWT ไม่ยกเว้นการตรวจ license key และ signed device proof

## Routes ของ Gateway

ทุก path ในตารางต่อท้าย prefix **`/api/service-school-bot`**

| Method | Path | ขอบเขต |
|---|---|---|
| GET / POST | `/projects` | List/create โครงงานของ desktop owner |
| GET / PUT / DELETE | `/projects/{id}` | Read/update/delete โครงงานของ owner |
| POST | `/projects/{id}/copies` | Copy จาก revision ที่ระบุ |
| POST | `/device-challenges` | Anonymous: challenge สำหรับ public JWK ของเครื่อง |
| POST | `/licenses/activate` | Anonymous: activate/renew เครื่องด้วย key + proof |
| POST | `/licenses/validate` | Anonymous: ตรวจ binding และต่อ lease ด้วย proof |
| GET / POST | `/admin/licenses` | Staff ส่วนกลาง list/create; list รองรับ `school_id` |
| POST | `/admin/licenses/{id}/revoke` | Revoke license |
| POST | `/admin/licenses/{id}/unbind` | ถอด binding; รองรับ `reason`, `next_device_jkt` |
| GET | `/curricula` | Published list: `level_id`, `recommended_grade`, `subject_key`, `mode`, `limit`, `cursor` |
| GET | `/curricula/{id}` | Published detail |
| GET / POST | `/admin/curricula` | Admin list/create; list รองรับ `level_id`, `lifecycle`, `limit`, `cursor` |
| GET | `/admin/levels` | ระดับหลักสูตรจริงสำหรับ content Admin |
| GET / PUT | `/admin/curricula/{id}` | Admin detail/update metadata ของ draft |
| PUT | `/admin/curricula/{id}/content` | เปลี่ยนเนื้อหา draft |
| POST | `/admin/curricula/{id}/publish` | Publish โดย approver คนละคนพร้อม `approval_ref` |
| POST | `/admin/curricula/{id}/approve-criteria` | อนุมัติเกณฑ์ด้วยตัวตน approver + four-eyes, `approval_ref`, ETag และ idempotency key |
| POST | `/admin/curricula/{id}/import` | Import normalized rows เข้า draft |
| GET / POST | `/admin/question-banks` | List/create bank revision |
| GET / PUT | `/admin/question-banks/{id}` | Detail/update draft |
| POST | `/admin/question-banks/{id}/publish` | Publish แบบ four-eyes |
| GET | `/course-runs/{id}/activities/{activityId}` | อ่าน activity policy/window |
| GET | `/attempts` | ประวัติ: ต้องมี `course_run_id`; รองรับ `activity_id`, `state`, `limit`, `cursor` |
| GET | `/attempts/{id}` | Attempt snapshot ตาม reveal policy |
| GET | `/progress` | `course_run_id`, `curriculum_id`, `mode=block\|arm\|code` |
| GET | `/learning/curricula/{id}` | Published lesson/mission/criteria/activity catalog พร้อม ID สำหรับเรียน/ส่งงาน |
| GET | `/learning/curricula/{id}/course-runs` | รอบเรียนที่ตัวตนนี้เข้าถึงได้ |
| GET | `/course-runs/{id}` | รอบเรียนและ ETag |
| GET / POST | `/submissions` | รายการงาน/ส่ง project revision; ไม่รับคะแนนจาก client |
| GET | `/submissions/{id}` | สถานะ/ผล และหลักฐานงานตามสิทธิ์ |
| POST | `/submissions/{id}/assessments` | ครูตรวจ rubric หรือเพิ่ม correction พร้อม reason |
| POST | `/course-runs/{id}/activities/{activityId}/attempts` | เริ่ม attempt พร้อม snapshot/สุ่มคำถามและตัวเลือก |
| PUT | `/attempts/{id}/answers` | บันทึกคำตอบก่อน server deadline |
| POST | `/attempts/{id}/submit` | ส่ง/ตรวจข้อสอบ; เมื่อหมดเวลาใช้คำตอบที่บันทึกไว้แล้ว |
| PUT | `/course-runs/{id}/activities/{activityId}/policy` | ครูกำหนดเวลา/หน้าต่างสอบ |
| POST | `/course-runs/{id}/activities/{activityId}/retakes` | สิทธิ์สอบซ่อมรายคนพร้อมเหตุผล |
| PUT | `/course-runs/{id}/scoring-policy` | น้ำหนักกิจกรรมรวม 10,000 bps ก่อน activate |
| POST | `/course-runs/{id}/activate` | Freeze scoring snapshot และเปิดรอบเรียน |
| GET | `/course-runs/{id}/learners/{learnerId}/total` | คะแนนรวมตาม frozen policy |
| POST | `/course-runs/{id}/level-proposals` | สรุปทั้งห้องจาก roster/term authority |
| GET | `/level-proposals/{id}` | อ่านข้อเสนอและ snapshot |
| POST | `/level-proposals/{id}/confirm` หรือ `/decline` | ตัดสินด้วย ETag และ snapshot hash |
| GET / PUT | `/course-runs/{id}/grade-mapping` | ช่องเกรดเดิม/ตั้ง mapping ตามสิทธิ์ admin โรงเรียน |
| DELETE | `/course-runs/{id}/grade-mapping/{mappingId}` | ยุติ mapping ที่ยังไม่มี export ยืนยัน |
| POST | `/course-runs/{id}/score-previews` | สร้าง preview อายุ 15 นาที |
| GET | `/score-previews/{id}` | อ่าน preview พร้อม snapshot hash/ETag |
| POST | `/score-previews/{id}/confirm` | ยืนยัน source snapshot แล้วสร้าง export/outbox |
| GET | `/score-exports/{id}` | สถานะ export/receipt/conflict |
| POST | `/score-exports/{id}/process` | ดำเนินการต่อผ่าน verified destination และสิทธิ์เดิม |
| POST | `/score-exports/{id}/resolutions` | เลือก keep/replace รายคนเพื่อสร้าง preview ใหม่; ไม่เขียนเกรดจนยืนยัน snapshot ใหม่ |
| GET / POST | `/admin/releases` | Staff ส่วนกลาง list/create release metadata |
| POST | `/admin/releases/{id}/publish` | Publish release metadata |
| GET | `/updates/check?version=0.1.0&channel=stable&platform=windows-amd64` | Anonymous: published release ที่ใหม่กว่า |

HTTP routes เดิมของ SchoolBot รวม logout และ web-session exchange ถูกลบแล้ว
ไม่มี fallback หรือ version segment; ใช้ Gateway routes ตามตารางเท่านั้น
Question-bank mutation ใช้ staff roles; assessment/progress ยังต้องเชื่อม `IAssessmentAuthority`
สำหรับ roster/School DB ซึ่งค่าปริยายคืน `AUTH_NOT_CONFIGURED`

## ETag, idempotency และขนาดคำขอ

Response ผ่าน SchoolBot pipeline ตั้ง `Cache-Control: no-store` และ `X-Request-Id`
คำขอที่มี UUID `X-Request-Id` ถูกต้องจะใช้ค่าเดิม มิฉะนั้น Gateway สร้างใหม่
Authentication middleware อาจปฏิเสธ JWT ก่อนถึง pipeline จึงไม่รับประกัน SchoolBot envelope/headers ในกรณีนั้น

ETag เป็น opaque string ที่มี quotes; ส่งกลับใน `If-Match` โดยไม่แก้รูปแบบ
Project update/delete/copy, curriculum metadata/content/publish และ question-bank update/publish ใช้ concurrency precondition
License unbind/revoke รับ `If-Match` แบบ optional ตาม use case; stale tag ถูกปฏิเสธ
Missing precondition ที่บังคับคืน 428 `PRECONDITION_REQUIRED`; stale คืน 412 `VERSION_CONFLICT`
และบางกรณีมี `error.details.current_etag` หลักสูตร/bank ที่ publish แล้วแก้ draft เดิมไม่ได้

ส่ง `Idempotency-Key` เมื่อ retry operation ที่รองรับ; Gateway รองรับ `X-Request-Key` เป็น fallback
Project create/copy, license create/activate และ curriculum/bank mutations มี replay ตามขอบเขตของ service
payload ต่างบน key เดิมคืน `IDEMPOTENCY_CONFLICT`; หมด replay window คืน `IDEMPOTENCY_EXPIRED`
License replay window 15 นาทีและเก็บใน process; curriculum/bank ledger อายุ 24 ชั่วโมงพร้อม snapshot/AES-GCM
ไม่รับรอง cross-process/restart สำหรับ replay ของทุก service ดูข้อจำกัดใน [service-school-bot/README.md](../service-school-bot/README.md)

| กลุ่ม | เพดาน body ที่ Gateway |
|---|---:|
| Device/control/release/license | 64 KiB |
| Project | 1 MiB |
| Curriculum/question-bank | 2 MiB |

เกินเพดานคืน 413 `PAYLOAD_TOO_LARGE`; malformed JSON/ชนิด field ผิดของ typed body คืน 400 `INVALID_REQUEST`
Typed JSON endpoint ที่ Content-Type ไม่ใช่ JSON อาจไม่ match และคืน 404; device/raw-body endpoint มีกฎ parsing ของตัวเอง
อย่าเหมารวมว่า endpoint ทุกตัวคืน 415 หรือใช้ binding rules เดียวกัน

## License และ device proof

สร้างผ่าน `POST /api/service-school-bot/admin/licenses` ด้วย JWT ของ TAdmin:

```json
{
  "school_id": 100,
  "school_name": "Demo school",
  "device_limit": 1,
  "expires_at": "2027-10-16T00:00:00Z"
}
```

201 คืน `{ "license": { ... }, "key": "SBOT-..." }`; key สุ่มและเก็บ hash
list คืน metadata/activated count โดยไม่คืน plaintext key, key hash หรือ device identifiers
`device_limit` รองรับ 1 เครื่อง; ใช้ unbind เพื่อย้ายเครื่อง ไม่ใช้ payload เดิม `device_id`

ขอ challenge ด้วย `{ "device_jwk": { "kty": "EC", "crv": "P-256", "x": "...", "y": "..." } }`
activate ส่ง `key`, `device_jwk`, `challenge`, `proof`; validate ส่ง `key`, `binding_id`, `challenge`, `proof`
Proof ใช้ ES256 และ hash ของ canonical JSON ที่ตัด `proof` ออก พร้อม challenge, method และ idempotency key
ค่า path ภายใน signed proof ตรงกับ HTTP path: `/api/service-school-bot/licenses/activate` หรือ
`/api/service-school-bot/licenses/validate` ทั้ง Desktop และตัวตรวจ proof ใช้ค่าเดียวกัน ไม่มี alias หรือ path แปลงเวอร์ชัน

สำเร็จคืน `valid`, `license_id`, `binding_id`, `entitlement`, `checked_at`, `expires_at`
Desktop ส่ง entitlement กลับใน `X-Device-Entitlement` คู่กับ Gateway JWT เมื่อใช้ข้อมูลผู้ใช้
SchoolBot ตรวจ token กับ binding ปัจจุบัน; key/entitlement ไม่ใช่ user login
Signer ปัจจุบันเป็น `TestLicenseTokenSigner` ต้องมี production signing/key management ก่อนใช้งานจริง

## Release และ update check

Create release รับ `version`, `channel`, `platform`, `url`, `sha256`, `signature`
รองรับ platform `windows-amd64`, `darwin-amd64`, `darwin-arm64`; channel `stable`/`beta`
version เป็น `x.y.z` ไม่มี prerelease/build metadata, whitespace หรือศูนย์นำหน้า
URL เป็น HTTPS ไม่มี userinfo ไม่เกิน 2,048 ตัวอักษร; signature ไม่เกิน 4,096 ตัวอักษร
API ตรวจรูปแบบ metadata ไม่ดาวน์โหลด artifact หรือตรวจ artifact signature

Update check คืน `{ "available": false, "release": null, "install_supported": false }` เมื่อไม่มีรุ่นใหม่
ถ้ามี จะคืน published release ล่าสุดที่ channel/platform ตรงและใหม่กว่า version ที่ส่งมา
Metadata ไม่ใช่การอนุมัติดาวน์โหลด/ติดตั้ง และยังไม่มี install/recovery/rollback flow

## Attempt mutations ผ่าน gRPC

`AssessmentService` รองรับ `StartAttempt`, `SaveAttemptAnswers` และ `SubmitAttempt` เพิ่มจาก read RPC เดิม
ยังไม่มี HTTP Gateway routes หรือ Desktop client สำหรับ RPC ใหม่เหล่านี้

- ทุก mutation ต้องมี idempotency key เป็น UUID ที่ไม่ว่าง; การบันทึกและส่งตรวจต้องมี `if_match`
- เริ่มสอบต้องเป็นนักเรียน Desktop ในห้องที่ได้รับสิทธิ์ มี License และ CourseRun ที่ active
  พร้อม `CurrentPolicyId`; snapshot ตรึงคำถาม เฉลย ลำดับตัวเลือก policy/window และ evaluator `choice-v1`
- `deadline_at` ใช้ค่าที่เร็วที่สุดระหว่างเวลาจำกัดกับเวลาปิด รับคำตอบเฉพาะก่อน deadline
  PostgreSQL อ่าน `clock_timestamp()` ภายใน transaction หลังได้ advisory lock; fake ใช้ TimeProvider
- เมื่อถึง deadline จะ commit `expired` และตรวจจากคำตอบที่บันทึกไว้ แล้วคืน `409 ATTEMPT_EXPIRED`
  โดย `submitted_at` คง null; คำตอบที่แนบมากับคำขอช้าไม่ถูกนำมาคิดคะแนน
- GET/list/progress และการเริ่มสอบครั้งถัดไปกู้ผลที่หมดเวลาผ่าน evaluator เดียวกัน
  evaluator ที่ไม่รู้จักคืน `503 GRADER_UNAVAILABLE` และ rollback โดยไม่แทนคะแนนด้วยศูนย์
- การส่งซ้ำใช้ ledger เดิม; terminal state ไม่ถูกตรวจหรือสร้าง audit ซ้ำ และคืนเฉลยตาม reveal policy

ยังต้องเพิ่ม bounded background sweep, การอนุญาตสอบซ่อม, Teacher API, Offline submission sync และ
การเชื่อมคะแนนไป PGGrade ก่อนตรวจรับ Phase 5 ทั้งระบบ การทดสอบ memory/file ไม่ยืนยัน
PostgreSQL concurrency หรือการเชื่อม Gateway จริง
License snapshot ของ memory/file ถูกอ่านก่อน curriculum lock เพื่อหลีกเลี่ยงการล็อกซ้อน
จึงไม่รับรอง race ระหว่างเพิกถอน License กับ mutation; PostgreSQL ตรวจข้อมูลซ้ำใน transaction เดิม

## Errors และ trust boundary

Business/binding error ที่ผ่าน pipeline ใช้รูปแบบ:

```json
{
  "error": {
    "code": "VERSION_CONFLICT",
    "message": "Concurrency token mismatch.",
    "request_id": "00000000-0000-4000-8000-000000000001",
    "details": { "current_etag": "\"opaque-etag\"" }
  }
}
```

`details` มีเฉพาะเมื่อมีข้อมูล เช่น current ETag หรือ import row
SchoolBot ส่ง `sb-error-bin` และ `schoolbot-error-bin` พร้อม `http_status`; Gateway ตรวจ consistency และ trusted peer ก่อนแปลง

| Status | ตัวอย่าง code/สถานการณ์ |
|---|---|
| 400 | `INVALID_REQUEST`, `INVALID_INPUT`, `INVALID_QUERY`, `INVALID_LICENSE` |
| 401 | `UNAUTHORIZED` หรือ JWT ถูก authentication middleware ปฏิเสธ |
| 403 | `CHANNEL_NOT_ALLOWED`, `STAFF_ROLE_REQUIRED`, `LICENSE_REQUIRED`, `LICENSE_INACTIVE`, `BINDING_REVOKED`, `DEVICE_PROOF_INVALID` |
| 404 | `NOT_FOUND` รวมโครงงานที่ไม่ใช่เจ้าของ/คนละ tenant |
| 409 | `DEVICE_LIMIT`, `STATE_CONFLICT`, `FOUR_EYES_REQUIRED`, `CRITERIA_NOT_APPROVED`, `IDEMPOTENCY_CONFLICT`, `IDEMPOTENCY_EXPIRED` |
| 412 / 428 | `VERSION_CONFLICT` / `PRECONDITION_REQUIRED` |
| 413 | `PAYLOAD_TOO_LARGE` |
| 503 | `AUTH_NOT_CONFIGURED`, `SERVICE_UNAVAILABLE` หรือ channel/trust adapter ยังไม่พร้อม |

Transport failure ใช้ Gateway envelope `{ code, message, messageTh, requestId, errors, results }`
client/channel ไม่พร้อมคืน 503, peer/trailer ไม่น่าเชื่อถือคืน 502, deadline คืน 504
ไม่ส่ง JWT, license key, entitlement หรือ payload นักเรียนลง logs

## ข้อจำกัดก่อนใช้งานร่วมกับ desktop

Desktop ใน `app/` ใช้ Gateway ใต้ `/api/service-school-bot/...` สำหรับ device challenge, license, update check และ project import รวมถึง path ภายใน signed device proof
หน้า Admin ใน sb-helper เรียก BFF `/api/service-school-bot/admin/...` ซึ่งส่งต่อไป Gateway ด้วย prefix เดียวกัน
การเรียกที่ต้องใช้ตัวตนยังต้องส่ง Gateway JWT และ `X-Device-Entitlement`
ไม่มี SchoolBot HTTP fallback และไม่ควรเปิด production trust ด้วย test stub เพื่อให้ desktop เก่าทำงาน

Business repositories มี PostgreSQL implementation ผ่าน scoped unit of work; ค่าเริ่มต้นไม่ fallback ไป memory
ผลทดสอบ Phase 5 ในเครื่องใช้ memory/file และ test doubles จึงยังไม่ใช่หลักฐานฐานข้อมูล production
หลักประกัน file snapshot/lock ครอบคลุม process เดียวหลาย adapters ไม่รับรองหลาย process
Session records/storage ยังอยู่เพื่อคง snapshot format แต่ไม่มี endpoint/session authentication เดิม
การเปิด production ต้องผ่าน H5/G-AUTH รวม trusted transport, authority, persistence และ signing ที่ใช้งานจริง
PostgreSQL ใช้ `SchoolAssessmentAuthority`, `PostgresGradeDestination` และ worker principal resolver แล้ว
ต้องตั้ง time zone ของ timestamp โรงเรียนอย่างชัดเจน; strict authority ไม่อนุมานสมาชิกจากประวัติซ้ำ/การย้ายที่ยังไม่ resolve
Memory ใช้ unconfigured defaults ส่วน confirm/apply ต้องมี reviewed `GradeExportWriteAuthorization` ตามเงื่อนไขเดิม
โค้ดต่อเพิ่มเติมรอบนี้ยังไม่ได้ build/test/เชื่อมฐานจริงตามคำสั่งผู้ใช้ ดู [รายละเอียดและข้อจำกัด](phase5-implementation-2026-11-04.md)

Resolution body คือ `{ "decisions": [{ "learner_id": 123, "action": "keep" }] }` โดย `action` เป็น `keep` หรือ `replace`
และต้องครบหนึ่งรายการต่อผู้เรียนที่ยังค้างใน batch ที่ชน ใช้ Idempotency-Key เดิมเมื่อ retry
รายการ applied เดิมไม่ส่งซ้ำ; ยืนยัน preview ใหม่ด้วย hash/ETag แล้วจึง supersede outbox เดิมใน transaction เดียว
กรณี `unknown` หรือ processing ที่ยังไม่ reconcile จะไม่ยอม supersede การอ่าน receipt ใช้ operation lock ก่อนตรวจผล
