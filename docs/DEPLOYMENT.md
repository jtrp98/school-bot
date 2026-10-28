# API connection: development and domain

Desktop เป็นแอปที่ทำงานในเครื่องและต่ออินเทอร์เน็ตได้ ไม่ต้องติดตั้ง API ในเครื่องผู้ใช้
Go Host ส่ง License Key และ installation Device ID ไป C# API เมื่อกด Activate
การตรวจอัปเดตเรียก API เช่นกัน; Run/Save/Load ของสาม Mode ไม่เรียก API

## Development

เปิด `scripts/start-api.ps1` (Windows) หรือ `bash scripts/start-api.sh` (Mac)
API bind ที่ `127.0.0.1:8080` ตามค่าเริ่มต้น และ Desktop เรียก `http://localhost:8080`
ไม่มีการกำหนด Domain ถาวรใน Mode หรือ Frontend

## Build เมื่อ Gateway integration พร้อม

เป้าหมายคือ Desktop/Web→HTTP Gateway→gRPC SchoolBot; browser ผ่าน same-origin BFF. B ปัจจุบันยังเป็น HTTP ไม่มี Gateway/gRPC integration ที่พร้อมใช้. ต้องกำหนด RPC/HTTP mapping, trusted JWT tenant กับ opaque session และ private exposure ก่อนเปิดใช้งานจริง โดยคง Login HTTP เดิมของ Gateway.

เมื่อ integration พร้อม ให้ตั้ง public Gateway URL ที่ยืนยันแล้วก่อน Build เพื่อให้ executable บน Windows และ `.app` บน Mac มี endpoint มาในตัว
`api.example.com` เป็นตัวอย่าง ให้เปลี่ยนเป็น Domain จริงเมื่อมีแล้ว

```powershell
$env:SCHOOL_BOT_BUILD_API_URL = 'https://api.example.com'
./scripts/build-desktop.ps1
```

```bash
SCHOOL_BOT_BUILD_API_URL=https://api.example.com bash scripts/build-desktop.sh
```

Host รองรับ URL prefix แต่ต้องใช้ Gateway route ที่ยืนยันและทดสอบ mapping แล้ว; การตัด prefix แล้ว HTTP proxy ไป B ไม่ใช่ service transport เป้าหมาย. Domain ตัวอย่างข้างบนไม่ใช่ค่าที่ deploy แล้ว
ไม่ใส่ Key, Token หรือ credentials ใน URL; ค่านี้เป็น public address เท่านั้น
URL ต้องเป็น HTTPS ยกเว้น HTTP สำหรับ localhost/loopback ระหว่างพัฒนา

ลำดับค่าที่ Desktop อ่าน: `SCHOOL_BOT_API_URL` → legacy `ARM_API_URL` → URL ที่ฝังตอน Build → localhost
Runtime override เหมาะกับนักพัฒนาที่รัน executable จาก terminal; สำหรับ Mac เปิดผ่าน Finder ให้ใช้ URL ที่ฝังตอน Build
ไม่ต้อง Rebuild Frontend หรือเปลี่ยนโค้ดของทั้งสาม Mode เมื่อตั้ง Domain

## HTTP hosting ปัจจุบันสำหรับ local/PoC

คำสั่ง publish และค่า bind ด้านล่างเป็น HTTP as-built ไม่ใช่ขั้น deploy gRPC. การตั้ง trusted proxy/TLS จริงยังต้องผ่าน review และ integration proof; ห้ามใช้ส่วนนี้เปิด B เป็น public fallback.

Build จากโฟลเดอร์ `web`:

```powershell
dotnet publish src/SchoolBot.Api -c Release -o ../dist/api
```

ตั้ง `ASPNETCORE_ENVIRONMENT=Production`, `SCHOOL_BOT_ADMIN_KEY` (Secret อย่างน้อย 32 ตัวอักษร;
ถ้าไม่ตั้งจะ fallback ไปอ่าน `ARM_ADMIN_KEY` ชื่อเดิม ควรใช้ชื่อใหม่ชื่อเดียวบน Server)
และ `ASPNETCORE_URLS` ใน Server/Hosting environment
เช่น `ASPNETCORE_URLS=http://127.0.0.1:8080` เมื่อใช้ Reverse Proxy ในเครื่องเดียวกัน
ถ้าไม่ตั้ง `ASPNETCORE_URLS`, `ASPNETCORE_HTTP_PORTS` หรือ `ASPNETCORE_HTTPS_PORTS` API จะ bind `http://127.0.0.1:8080`
(image .NET มาตรฐานตั้ง `ASPNETCORE_HTTP_PORTS` ได้ตรงๆ)
Rate limiter นับตาม client IP จาก `X-Forwarded-For` และเชื่อ header นี้เฉพาะเมื่อ proxy เชื่อมต่อจาก loopback
(127.0.0.0/8, ::1) เท่านั้น ให้ Reverse Proxy บนเครื่องเดียวกันตั้ง `X-Forwarded-For` เป็น IP ของ client จริง
(เช่น nginx `proxy_set_header X-Forwarded-For $remote_addr;`) ไม่ต่อท้ายค่าที่ client ส่งมา
ถ้า proxy อยู่คนละเครื่องหรือเป็น container network, API จะไม่เชื่อ header และทุก client จะใช้โควต้าร่วมกัน
ต้องเพิ่ม trusted proxy ในโค้ดก่อนใช้รูปแบบนั้น
เป้าหมาย production ให้ client ติดต่อ HTTP Gateway และ Gateway ติดต่อ B ด้วย gRPC; host/port/health exposure ยังต้องระบุจาก deployment จริง. `GET /healthz` ของ HTTP local ไม่ยืนยัน readiness ของ Gateway/gRPC และไม่ใช่เงื่อนไขเพียงพอสำหรับแจก Desktop
Body ใหญ่กว่า 64 KB ถูกปฏิเสธ (413) และโควต้า 30 request/นาที/IP สำหรับ `/v1/licenses/*`, 60 สำหรับ `/v1/updates/check`;
ตัวเลขที่เหลือและรูปแบบ response อยู่ใน `docs/API.md`
ไม่ส่ง Admin Key ไป Desktop; License activation ไม่ต้องใช้ Admin Key

Starter ใช้ Memory Store: License และ Release หายเมื่อ restart API จึงต้องเพิ่ม persistent database
ก่อนใช้งานจริง ระบบปัจจุบันทดลอง online activation/device quota ได้ แต่ยังไม่มี signed activation token
ที่เก็บข้ามการเปิดแอป หรือการบังคับสิทธิ์แบบ Word/Excel เต็มรูปแบบ ต้องทำส่วนนี้ต่อใน Desktop Core/API
พร้อมกำหนดอายุ token และนโยบายตรวจซ้ำเมื่อออนไลน์
