# เมทริกซ์ความเข้ากันได้ของการยืนยันตัวตนและไดเรกทอรีระบบเดิม (Legacy Auth & Directory Compatibility Matrix)

เอกสารบันทึกข้อเท็จจริงและหลักฐานเปรียบเทียบพฤติกรรมการยืนยันตัวตน การแปลงไดเรกทอรี และการแยกเขตข้อมูลระหว่างโรงเรียน (Tenant Isolation) จากระบบเดิม เพื่อใช้เป็นเกณฑ์ความเข้ากันได้สำหรับ School Bot  
แหล่งอ้างอิงโค้ดต้นทาง: `sb-web-academic-dev-1` (ระบบ Legacy ASP.NET ดั้งเดิม), `sb-api-gateway` (พอร์ต Web และ Mobile บน Gateway), และ `sb-tech-upgrade-academic` (ไมโครเซอร์วิส Academic)

---

## 1. ตารางเมทริกซ์เปรียบเทียบความเข้ากันได้ (Compatibility Matrix)

| หมวดหมู่ | ระบบ Legacy ดั้งเดิม (`sb-web-academic-dev-1`) | พอร์ต Web บน Gateway (`sb-api-gateway`) | พอร์ต Mobile บน Gateway (`sb-api-gateway`) | กฎยืนยันแล้วของ School Bot (Confirmed Rule / Fail-Closed) | สถานะรอการตัดสินใจ (Pending C1–C10 & TR-AUTH) |
|---|---|---|---|---|---|
| **การแยกข้อมูลระหว่างโรงเรียน (Tenant Isolation)** | `a.nCompany == schoolId` (`Default.aspx.cs:100`); เข้าร่วมตาราง `TCompanies` ด้วย `nCompany` | `u.NCompany == request.SchoolId` (`WebLoginService.cs:64`); ตรวจ `company.IsActive` และ `!company.CDel` (:196) | `t.NCompany == company` (`TusersRepository.cs:106`); เข้าร่วม `Tcompanies` | **Strict Tenant Boundary:** `SchoolId` ของคำขอต้องตรงกับ `NCompany` ของบัญชีและ `SchoolId` ของโปรไฟล์อย่างเคร่งครัด ข้อมูลข้ามโรงเรียนต้องถูกปฏิเสธแบบ fail-closed (`403 FORBIDDEN` / `401 UNAUTHORIZED`) ป้องกันข้อบกพร่องของ Gateway `IsInactiveEmployeeAsync` (:209) ที่ละเลยการกรอง `SchoolId` | **ยืนยันแล้ว**: ปฏิเสธการเข้าถึงข้ามโรงเรียนทุกกรณี |
| **ตัวตนบัญชี vs ข้อมูลโปรไฟล์ (Account vs Profile Identity)** | `q1.sID` (PK บัญชี Master); `q1.nSystemID` (ใช้เป็น `user_id` ในโทเค็น, :218); `sEmp == q1.nSystemID` (:157) เทียบกับ `sEmp == q1.sID` (:160) | `userId = user.NSystemId ?? user.SId` (`WebLoginService.cs:272`); แต่ `IsInactiveEmployeeAsync` ใช้ `e.SEmp == user.SId` (:209) | ใช้ Master `sID` ตรงกับ School DB `TUser.SId` (`AuthService.cs:210`, `TusersRepository.cs:36-41`) | **No ID Assumption:** ห้ามสมมติว่า `AccountId == ProfileId` หรือ `SystemId == ProfileId` โดยเด็ดขาด การผูกโยงต้องผ่านการพิสูจน์โดยไดเรกทอรีที่เชื่อถือได้ (Authoritative Directory Link) เท่านั้น หากไม่มีการพิสูจน์หรือ `SystemId` เป็น null หรือ 0 ต้องปฏิเสธแบบ fail-closed ด้วย `503 AUTH_NOT_CONFIGURED` | **Pending (C10 & TR-AUTH-03..05)**: รอการพิสูจน์โครงสร้างความสัมพันธ์ระหว่างตารางจริง |
| **บทบาทและสิทธิ์ผู้ดูแลระบบ (Role & is_admin)** | ตรวจสอบสมาชิกกลุ่ม "แอดมิน/ผู้บริหาร" ฝั่งเซิร์ฟเวอร์ (`Default.aspx.cs:193-240`); Superadmin: `(UserID == 0 && AdminID != null) ? 1 : 0` (`bypassgarde.aspx.cs:64`) | กำหนดค่าคงที่ `isAdmin: 0` (`WebLoginService.cs:278`); ส่วน handoff ส่งต่อ claim `IsAdmin` จากโทเค็นขาเข้า (:302-305) | ออก claims สำหรับ mobile โดยไม่มีการตรวจสอบกลุ่ม `is_admin`; โทเค็น mobile ไม่มี `CType` | **Decisions-035 Authority:** ยึดตามมติ decisions-035 (มีลำดับเหนือ C11) ตรวจสอบสมาชิกกลุ่มแอดมิน/ผู้บริหารฝั่งเซิร์ฟเวอร์ตาม `Default.aspx.cs:193-240` และคง superadmin ตาม `bypassgarde.aspx.cs:55-75` ห้ามเชื่อถือ body, header หรือ boolean assertion ขาเข้า หากข้อมูลกลุ่มหรือ mapping ไม่พร้อม ต้อง fail-closed ด้วย `503 AUTH_NOT_CONFIGURED` | **ปิดเฉพาะ C11**: คงค้าง `TR-AUTH-02-rest` (ตัวตรวจ login `TAdmin`), `TR-AUTH-03` และ implementation proof |
| **การตรวจสอบรหัสผ่านและ Verifier (Password Hash & Verifier)** | `FormsAuthentication.HashPasswordForStoringInConfigFile(pwd, "SHA1")` ได้ Uppercase 40-char hex (`Default.aspx.cs:83-85, 124`); Plaintext หรือ `ddMMyyyy` ภายใต้ server culture (`en-us`/`th-th`) (:93-96, 131) | `UserPasswordRepository.HashSha1`: `Convert.ToHexString(SHA1.HashData(...))` เปรียบเทียบด้วย `StringComparison.Ordinal` (`WebLoginService.cs:222`); Plaintext / `ddMMyyyy` date candidates (:225-231) | Modern PBKDF2 `v2:{salt}:{hash}` (SHA256, 100k iterations, hash 32 bytes) (`PasswordHasherService.cs:45-66`); Legacy SHA1 เทียบด้วย `OrdinalIgnoreCase` (:78-83); Plaintext / `ddMMyyyy` / `yyyyMMdd` โดย `ToString()` ไม่ระบุ culture (`AuthService.cs:105-108`) | **Bounded Verifier & Fail-Closed:** จำกัดความยาวรหัสผ่านที่ 1..1024 ตัวอักษรตาม DES-004 ไม่ trim รหัสผ่าน; ปฏิเสธค่าว่าง, เกินขนาด หรือผิดรูปแบบทันที ไม่เลือก PBKDF2, date candidates หรือ culture ใดเป็นเป้าหมายล่วงหน้าของ School Bot หากเจ้าของระบบยังไม่ได้รับรอง | **Pending (C1, C2, C3, C4)**: C1 ทางเลือก verifier, C2 ขนาดตัวพิมพ์ hex, C3 การยอมรับ `v2:`, C4 รูปแบบวันเกิดและ culture |
| **การจัดการชื่อผู้ใช้ (Username Normalization: Input, Column Trim, Case)** | `a.username.Trim() == username` (`Default.aspx.cs:100` ตัดช่องว่างที่ column ในคำสั่ง query) | `u.Username.Trim() == request.Username.Trim()` (`WebLoginService.cs:64` ตัดช่องว่างทั้งสองฝั่ง); case ขึ้นกับ collation ของฐานข้อมูล | `t.Username == username` (`TusersRepository.cs:108` ไม่ตัดช่องว่าง); case ขึ้นกับ collation ของฐานข้อมูล (ยังไม่ยืนยัน / unproven DB collation) | **No Unilateral Normalization:** ไม่แปลงเป็น lowercase หรือ Unicode normalize เองล่วงหน้า ในการ admission ต้องใช้ trusted selector ตาม normalization ของ upstream ที่เลือก | **Pending (C5)**: การ trim และ case sensitivity/collation รอการยืนยันจากข้อเท็จจริงของระบบ |
| **ประเภทบัญชี (Account Category: cType)** | บังคับ `a.cType == "1"` สำหรับการเข้าสู่ระบบผ่านเว็บ (`Default.aspx.cs:100`) | บังคับ `u.CType == '1'` สำหรับบุคลากรผ่านเว็บ (`WebLoginService.cs:64`) | ไม่กรอง `CType` ใน query (`TusersRepository.cs:106`); แยกเงื่อนไข `user.CType == "0"` ใน `AuthService.cs:209` | **Server Category Authority:** รับเฉพาะค่าที่ยืนยันจากเซิร์ฟเวอร์เท่านั้น ('0' = นักเรียน, '1' = บุคลากร); บัญชีที่ `cType` เป็น null หรือค่าอื่นที่ไม่รู้จัก ต้องปฏิเสธแบบ fail-closed ทันที ไม่ยอมรับการกำหนดประเภทจาก body หรือ header | **ยืนยันการปฏิเสธค่าผิดปกติ**: ไม่มีการอ้างอิง decisions-048 ที่ไม่มีอยู่ในชุดสัญญา |
| **การลบบัญชี (Account Deletion: CDel)** | บังคับ `a.cDel == null` (`Default.aspx.cs:100`); ไม่รับทั้ง '0' และ '1' | บังคับ `u.CDel == null` (`WebLoginService.cs:64`); บริษัท `!company.CDel` (:196) | `t.CDel != '1'` (`TusersRepository.cs:107`); ยอมรับทั้ง null และ '0' | **Fail-Closed Deletion:** บัญชีที่มีเครื่องหมายการลบ (`CDel == '1'` หรือ `cDel == true`) ต้องถูกปฏิเสธทันที ไม่สามารถเข้าสู่ระบบหรือออก session ได้ | **Pending (C6)**: ความต่างของความหมายระหว่าง `null` (web) กับ `!='1'` (mobile) ยังรอการตัดสิน |
| **บัญชีชื่อผู้ใช้ซ้ำ (Duplicate Usernames Cardinality)** | `OrderByDescending(o => o.sID).FirstOrDefault()` (`Default.aspx.cs:115`) | `OrderByDescending(u => u.SId).FirstOrDefaultAsync(ct)` (`WebLoginService.cs:65`) | `orderby t.SId descending ... FirstOrDefaultAsync()` (`TusersRepository.cs:108`) | **Strict Cardinality Admission:** ในการ admission เข้า School Bot ต้องพบบัญชี active ที่มีสิทธิ์เพียงบัญชีเดียวเท่านั้น หากพบบัญชี active ซ้ำซ้อน, บัญชีหลายประเภทปนกัน (mixed category), มีเฉพาะบัญชีที่ถูกลบ หรือประเภทบัญชีไม่ถูกต้อง ต้องปฏิเสธแบบ fail-closed ทันที ห้ามใช้พฤติกรรมการเลือก ID สูงสุด (`highest SId`) จาก `FirstOrDefault` มาเป็นนโยบายการ admission | **แยก Observed Port ออกจาก Admission**: การเรียง `highest SId` เป็นเพียงพฤติกรรมที่สังเกตได้จาก port เดิม ไม่ใช่นโยบายเป้าหมาย |
| **สถานะการล็อกและสถานะการทำงาน (Lockout & Active Status)** | `WorkStatus in (2, 3)` -> 401 "No Permission" (`Default.aspx.cs:168`); โรงเรียนไม่ active -> 405 (`Default.aspx.cs:175`) | ตรวจ `AccountLockedUntil > now` (`WebLoginService.cs:70`); `WorkStatus in (2, 3)` (:204, 214) | `user.IsAccountLocked()` (`AuthService.cs:149`) | **Strict State Checking:** ปฏิเสธบัญชีที่ถูกล็อก (`AccountLocked == true` หรือ `AccountLockedUntil > now`) และปฏิเสธบุคลากรที่พ้นสภาพ (`WorkStatus` 2 = ลาออก, 3 = เลิกจ้าง) ห้ามออก session ให้กับสถานะที่ไม่ active | **Pending (C7 & C9)**: C7 การบันทึกตัวนับ lockout กลับ Master DB และ C9 เกณฑ์ active ของนักเรียน ยังรอคำยืนยัน |

---

## 2. การสืบย้อนกลับไปยังบรรทัดโค้ดต้นทาง (Source-to-Line Traceability)

1. `Default.aspx.cs:83-85, 93-96, 98-118, 122-136, 157-175, 193-240` (`C:\src\sb-web-academic-dev-1\jabjaiuniversity\FingerprintPayment\Default.aspx.cs`)
2. `bypassgarde.aspx.cs:55-75` (`C:\src\sb-web-academic-dev-1\jabjaiuniversity\FingerprintPayment\bypassgarde.aspx.cs`)
3. `WebLoginService.cs:58-75, 195-245, 270-310` (`C:\src\schoolbright\target\sb-api-gateway\src\ApiGateway.Infrastructure\Authentication\WebLoginService.cs`)
4. `UserPasswordRepository.cs:48-49` (`C:\src\schoolbright\target\sb-api-gateway\src\ApiGateway.Infrastructure\Databases\Repository\UserPasswordRepository.cs`)
5. `TusersRepository.cs:36-41, 103-134` (`C:\src\schoolbright\target\sb-api-gateway\src\ApiGateway.Infrastructure\Repositories\Databases\TusersRepository.cs`)
6. `AuthService.cs:102-146, 209-238` (`C:\src\schoolbright\target\sb-api-gateway\src\ApiGateway.Application\Services\Implementations\AuthService.cs`)
7. `PasswordHasherService.cs:27-87` (`C:\src\schoolbright\target\sb-api-gateway\src\ApiGateway.Infrastructure\Common\Authentication\PasswordHasherService.cs`)
8. `TeacherRepository.cs:24-35, 111-122` (`C:\src\schoolbright\target\sb-tech-upgrade-academic\src\ServiceAcademic.Infrastructure\Repositories\Databases\TeacherRepository.cs`)

---

## 3. ประเด็นความเข้ากันได้ที่ยังรอการตัดสินใจ (Pending Policies C1–C10)

ระบบไม่ตัดสินใจเลือกนโยบายทางธุรกิจแทนเจ้าของระบบ โดยรายการ C1–C10 ยังคงสถานะรอคำยืนยัน (Pending):

- **C1 (Verifier อ้างอิงของ Desktop)**: รอเลือกว่าจะใช้เฉพาะ Web (SHA1 uppercase), Mobile (PBKDF2/SHA1 insensitive) หรือแบบครอบคลุม (Superset)
- **C2 (ขนาดตัวพิมพ์ของ Hex SHA1)**: เลือกระหว่าง Ordinal ตัวใหญ่ล้วน (Web) กับ OrdinalIgnoreCase (Mobile)
- **C3 (การยอมรับ PBKDF2 `v2:`)**: เลือกว่า Desktop จะรองรับ format `v2:{salt}:{hash}` ของ mobile หรือไม่
- **C4 (รูปแบบวันเกิดและ Culture)**: พฤติกรรมต่างกันตาม Culture (`th-TH` แปลง พ.ศ. 2548 ↔ ค.ศ. 2005 ขณะที่ `en-US` ไม่แปลง) รอการกำหนด Target Culture ที่ชัดเจน
- **C5 (การตัดช่องว่างและตัวพิมพ์ชื่อผู้ใช้)**: เลือกระหว่าง `Trim()` ทั้งสองฝั่งตาม Web port หรือการเทียบตรงตัวตาม Mobile port และ Collation ของฐานข้อมูล
- **C6 (ความหมายของการลบบัญชี `CDel`)**: เลือกระหว่าง `CDel == null` อย่างเคร่งครัดตาม Web หรือ `CDel != '1'` ตาม Mobile
- **C7 (การเขียนตัวนับ Lockout และ Log)**: เลือกว่าการพยายามเข้าสู่ระบบของ School Bot จะบันทึกลง Master DB หรือไม่
- **C8 (การเข้าสู่ระบบครั้งแรกและรหัสผ่านแบบ Plaintext)**: นโยบายการจัดการ `UseEncryptPassword == false`
- **C9 (กฎการตรวจสอบสถานะ Active ของนักเรียน)**: การตรวจสอบประวัติห้องเรียนและภาคเรียน
- **C10 (การผูกโยงข้อมูลบุคลากร Employee Linkage)**: ความสัมพันธ์ระหว่าง `sEmp`, `nSystemID` และ `sID` รอการพิสูจน์ด้วยข้อมูลจริง

---

## 4. ข้อเท็จจริงภายนอกที่ยังขาด (Pending External Truth TR-AUTH-01..07)

- **TR-AUTH-01**: โครงสร้าง Master / School Database ปัจจุบัน (ได้หลักฐานเบื้องต้นแล้ว)
- **TR-AUTH-02**: เส้นทางการออก session/JWT และตัวตรวจสอบ login ของ `TAdmin` (ยังขาดข้อยุติ)
- **TR-AUTH-03**: การจัดเก็บและการอ่านข้อมูลสมาชิกกลุ่มแอดมินฝั่งเซิร์ฟเวอร์
- **TR-AUTH-04**: การเชื่อมโยงสิทธิ์ครูรายวิชากับตารางประถม/มัธยม
- **TR-AUTH-05**: นโยบายและขั้นตอนการกำหนด `staff_admin` คนแรก
- **TR-AUTH-06**: การยืนยันเส้นทาง login ของนักเรียน
- **TR-AUTH-07**: สภาพแวดล้อมเครือข่ายและการตั้งค่าโฮสต์/TLS สำหรับ private gRPC

---

## 5. ขอบเขตความปลอดภัยและกฎ Fail-Closed (Security Boundary & Fail-Closed Rules)

1. **การแยกข้อมูลระหว่างโรงเรียน (Cross-Tenant Isolation)**: ทุกการดำเนินการต้องผูกโยงกับ `SchoolId` ที่ตรวจสอบแล้ว ห้ามอ้างอิงข้อมูลข้ามโรงเรียนโดยเด็ดขาด
2. **การแยกตัวตนบัญชีและโปรไฟล์ (Identity Distinction)**: `AccountId` และ `ProfileId` อยู่คนละ namespace ห้ามสมมติว่ามีค่าเท่ากันโดยไม่มีการพิสูจน์
3. **การตรวจสอบสิทธิ์ผู้ดูแลระบบจากเซิร์ฟเวอร์ (Server Authority for Admin Roles)**: สิทธิ์ admin ต้องได้จากการตรวจสอบกลุ่มฝั่งเซิร์ฟเวอร์ตาม decisions-035 เท่านั้น ค่า assertion ที่ส่งมาจาก client หรือ boolean ใน input ไม่สามารถใช้เป็นหลักฐานได้ หากไม่มีข้อมูลกลุ่ม ต้องตอบกลับด้วย `503 AUTH_NOT_CONFIGURED`
4. **การควบคุมขอบเขตข้อมูลขาเข้า (Input Bounds Enforcement)**: รหัสผ่านต้องมีความยาว 1..1024 ตัวอักษร ไม่ตัดช่องว่าง ปฏิเสธค่าว่างและค่าที่เกินขนาดทันที
