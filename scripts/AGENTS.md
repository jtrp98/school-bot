# Scripts / Build / Package / Release

ใช้ร่วมกับกฎแม่ที่ [../AGENTS.md](../AGENTS.md) อ่าน [README.md](README.md) และกฎของส่วนที่สคริปต์จะ build หรือทดสอบ

- Installer เดียวรวม desktop และ engines ที่ใช้งาน แต่ API ฝั่ง server deploy แยก ไม่บรรจุใน desktop installer
- Release ต้องไม่มี source maps, original sources, debug symbols, development tools, secrets หรือ private keys ใน payload เก็บ third-party notices/licenses ให้ครบ
- Frontend ที่ฝังใน binary ยังตรวจสอบย้อนหลังได้ ห้ามอ้างว่าป้องกันการคัดลอกหรือ reverse engineering ได้ทั้งหมด
- ใช้ build/package scripts ของ repository ไม่ข้ามขั้นตอนสร้าง icon/version resources หรือ release asset checks
- อ่านเวอร์ชันจาก manifests ที่มีอยู่ ไม่สร้างแหล่งเวอร์ชันซ้ำ และไม่อ้างว่า build/signing ผ่านโดยไม่มีผลตรวจจริง
- ไม่ overwrite binary ที่ผู้ใช้กำลังเปิดอยู่ ใช้ `scripts/desktop/build.ps1 -OutputDirectory <path>` และ `scripts/desktop/package.ps1 -PayloadDirectory <path>` เมื่อต้องแยก Windows output

## Engine build

- เมื่อแก้ `scripts/engines/` ให้อ่าน [../engines/AGENTS.md](../engines/AGENTS.md) และ README ของ engine ที่เกี่ยวข้อง
