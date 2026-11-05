# Engines (C++ / IPC Runtimes)

ใช้ร่วมกับกฎแม่ที่ [../AGENTS.md](../AGENTS.md) ศึกษา [README.md](README.md) และ README ของแต่ละ engine ก่อนแก้ไข

## มาตรการความปลอดภัยของฮาร์ดแวร์ (Hardware Safety Invariants)
- **ห้ามสั่งขยับหุ่นยนต์จริง**: ห้ามส่งคำสั่งเคลื่อนไหว อัปโหลด firmware หรือเปลี่ยนโหมดฮาร์ดแวร์ เว้นแต่ผู้ใช้มีคำสั่งชัดเจนให้ทดสอบ
- **Sample Mode Isolation**: Sample Mode ต้องตัดการเชื่อมต่อ hardware port จริง ห้ามส่ง command packet ออกไปเด็ดขาด
- **OS Write != Physical State**: การเขียนข้อมูลลง serial/USB สำเร็จ ไม่ได้รับประกันตำแหน่งทางกายภาพ ห้ามจำลองค่า sensor
- **Stop & Drain Invariant**: เมื่อถูกยกเลิก (Cancel) ต้อง drain คำสั่งเก่าที่ค้างอยู่ใน queue ให้หมด ก่อนส่งสัญญาณหยุดฉุกเฉิน

## สัญญาและการสื่อสาร (IPC Contract)
- **JSON Lines UTF-8**: สื่อสารผ่าน standard streams — **stdout** สำหรับ protocol message เท่านั้น และ **stderr** สำหรับ diagnostics
- รักษาโครงสร้าง Envelope กลาง: `protocol`, `request_id`, `mode`, `action`, `data` และส่ง matching response กลับทุกครั้ง
- ตรวจสอบ Bounds, Timeout, Payload validation และ Error envelope ให้รัดกุมในระดับ Engine
- Mode ID (UI profiles) แยกอิสระจาก Engine ID (Implementation) ไม่ผูกกันแบบ 1:1

## การเขียนโค้ดและคอมเมนต์
- แต่ละ Engine อยู่ใน `engines/<engine-id>/` แยก source, build scripts, tests และ dependencies ของตนเอง
- **Why-only Comments**: โค้ด C++ ต้องสื่อความหมายด้วยตัวมันเอง อนุญาตให้คอมเมนต์เฉพาะ hardware timing, memory constraints หรือ technical workarounds ห้ามใส่ class/method documentation เวิ่นเว้อ
- ไม่นำเกณฑ์จำนวนบรรทัดของ Frontend มาบังคับ ให้แบ่งคลาสตามสถาปัตยกรรมของ C++

## คำสั่งตรวจงาน
- รัน `scripts/engines/build.ps1 -Engine zide -Test` และ IPC tests ร่วมกับ binary ที่ build ล่าสุด
- ก่อน build หรือ package ให้อ่าน [../scripts/AGENTS.md](../scripts/AGENTS.md)
