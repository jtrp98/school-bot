# Block Lab design system (สำหรับ implement)

> แหล่ง: canvas "School Bot – Block Lab" หน้า Ver.1 (https://claude.ai/artifact/YYgSHL2aCtVbkDTgBQBzTV) เวอร์ชัน `1793595782-4ee6` อ่านเมื่อ 2026-11-02
> ใช้คู่กับ `70-block-lab-ui-spec.md` (หน้าจอ, flow, ข้อความ UI) · บันทึกนี้เน้นค่าที่ต้องเอาไปทำ token, Blockly theme/renderer และ component
> ค่าทั้งหมดอ่านจาก source ของ board โดยตรง ถ้า canvas เปลี่ยนให้อ่านซ้ำ

## สถานะของแหล่ง

| board | ใช้เป็นหลักไหม | หมายเหตุ |
|---|---|---|
| `Main-A2`, `WindowBarA2`, `BlocksPaneA2`, `CatRailA2`, `BlockA2`, `BlockSheetA2`, `ConnectUnoA2`, `ContextMenuA2` | ใช้ | ชุดล่าสุด แก้ล่าสุด 2026-11-02 |
| `CodeEditorA2`, `TerminalA2`, `PreflightDialogA2`, `CardsA2`, `SimPaneA2`, `StagePaneA2`, `MascotA2` | ใช้ | ชิ้นส่วนของ A2 (ไม่อยู่ในภาพที่ผู้ใช้ชี้ แต่ Main-A2 import ไปใช้) |
| `Spec.dc.html` (F) | อ้างอิงเท่านั้น | รายการหมวดเป็นชุดเก่า · focus เป็น outline ส้ม `#B85E00` ขัดกับ A2 · ใช้ได้แค่หลักการ 9 หัวข้อ |
| `ContextMenuA2` | ต้นแบบแยก | ยังไม่ได้ต่อเข้ากับ `BlocksPaneA2` (โมเดลบล็อกจำลอง 12 ก้อน) ผู้ออกแบบบอกว่า "ยังไม่ได้ render ตรวจ" |
| หน้า "Achive" | ไม่ใช้ | board รุ่น A–E, 1D |

mockup อ้าง "CI §11" และ "brand §1–6" ของ School Bright ซึ่งไม่มีไฟล์อยู่ใน repo หรือในเครื่อง และไม่มี Design System artifact ที่ผู้ใช้เปิดได้ ค่าในบันทึกนี้จึงมาจาก mockup อย่างเดียว

### ที่เปลี่ยนหลังเขียนบันทึก 70

- **2026-11-02 (ผู้ใช้สั่ง · R9/R17):** ย้ายปุ่ม **หยุดทั้งหมด** จากแถวแท็บลิ้นชักไปไว้ที่หัวแผงตัวจำลองใน `Main-A2` (ต่อจากปุ่มเริ่ม/หยุดจำลอง, ■ 46×46 ขอบ/ไอคอน `#D32F2F` พื้นขาว, z 40 จึงกดได้แม้มี dialog) · ตอนแผงขวาซ่อน (ค่าเริ่มต้นของ A2) ปุ่มนี้ลอยแทน ตำแหน่งที่แน่นอนต้องดูใน `Main-A2` ตอนทำจริง
- เอาชิป "ปัญหา N ข้อ" ออกจากลิ้นชัก ผลตรวจ R17 แสดงเป็น popup ตอนกดเริ่มจำลองหรือส่งลงบอร์ดเท่านั้น ถ้าส่งไม่ได้ ปุ่มส่งต้องบอกเหตุผลผ่าน tooltip + aria
- แถวขวาของลิ้นชักจึงเหลือแค่ [ส่งลงบอร์ด] [ยุบ/ขยาย]
- แถบหมวด **ไม่มี scrollbar** (รอบ 8 · OQ94) ใช้ปุ่ม "เลื่อนขึ้น/เลื่อนลง" เต็มความกว้าง 112 สูง 44 แทน ปุ่มโผล่ตามตำแหน่งที่เลื่อนได้ ส่วนรายการบล็อกใช้ขอบจางไล่ระดับเป็นสัญญาณว่ามีต่อ (บันทึก 70 ที่เขียนว่า "scrollbar 14 เห็นตลอด" ผิด)

## 1. Palette (โทนน้ำเงินเย็นของ CI)

นับจาก 7 board หลัก: `#14243D` 125 ครั้ง · `#DCE3EC` 103 · `#5A6377` 91 · `#1866C9` 90 · `#EAF2FD` 46 · `#C6D6EA` 37 · `#F4F6F8` 36 · `#D32F2F` 22 · `#B07F00` 22 · `#2E7D32` 20 · `#0F4C99` 15 · `#FFF4CC` 14

| token ที่เสนอ (ชื่อ semantic) | ค่า | ใช้ที่ | var เดิมใน `tailwind.css` |
|---|---|---|---|
| `text-primary` | `#14243D` | ตัวอักษรทั้งหมด รวมตัวอักษรบนบล็อก | `--ink` `#1F2430` |
| `text-secondary` | `#5A6377` | หัวกลุ่ม, คำอธิบาย, สถานะปิดใช้ | `--muted` `#5b6270` |
| `action-primary` | `#1866C9` | ปุ่มหลัก (ส่งลงบอร์ด), selected, focus, เส้นจุดต่อ | `--blue` `#1490E0` |
| `action-primary-ink` | `#0F4C99` | ตัวอักษรบนพื้น subtle (ปุ่มเลื่อนแถบหมวด) | `--blue-press` |
| `action-primary-subtle` | `#EAF2FD` | hover, พื้นปุ่มเลื่อน | `--blue-tint` `#e4f2fc` |
| `ring-subtle` | `#D0EBFD` | วงอ่อน | ไม่มี |
| `border` | `#DCE3EC` | เส้นขอบ, ราง rail, จุดตาราง | `--line` `#e6e0d8` |
| `border-strong` | `#C6D6EA` | เส้นแบ่งกลุ่ม, ขอบปุ่มเลื่อน | `--line-strong` `#cfc7bc` |
| `surface` | `#FFFFFF` | แผง, การ์ด | `--surface` ตรง |
| `background` | `#F4F7FB` | พื้นหลังหน้า, พื้นที่ทำงาน | `--bg` `#FBF9F6` |
| `sunken` / `disabled` | `#F4F6F8` | บล็อกปิดใช้, พื้นยุบ | `--sunken` `#f3efe9` |
| `danger` | `#D32F2F` | หยุดทั้งหมด, error | `--err` `#d3302f` (เกือบตรง) |
| `warning-strong` | `#B07F00` | ขอบบล็อกเตือน (ประ), ข้อความเตือน | ไม่มี (ใช้ส้ม) |
| `warning-subtle` | `#FFF4CC` | พื้นแถบเตือน | ไม่มี |
| `success` | `#2E7D32` | สถานะบันทึกแล้ว, ผ่าน | `--ok` `#1f8a4c` |
| scrim | `rgba(20,36,61,.35)` | ฉากหลัง dialog | Modal ใช้ ink .45 |

ข้อสังเกต: Arm Lab ใช้ `#1866C9`/`#EAF2FD`/`#DCE3EC`/`#C6D6EA` แบบ hardcode อยู่แล้วบางจุด (`tailwind.css:389-527`, `SimulatorStage.tsx`, `PoseCards.tsx`, `ConnectionMascot.tsx`) แปลว่าแอปกำลังอยู่ระหว่างย้ายไปโทน CI

## 2. สีหมวดบล็อก 16 หมวด (DSG-13 ค่าชั่วคราว)

ตัวอักษรบนบล็อกทุกหมวด = `#14243D` 16px/700 (ยกเลิกตัวขาวแล้ว OQ135) · contrast ตัวเข้มบนพื้นหมวดต่ำสุด 6.57:1 (Car Control) · ขอบหมวดบนพื้น `#F4F7FB` ต่ำสุด 3.76:1 (เริ่มต้น)

| # | id ที่เสนอ | ชื่อบนแถบ | กลุ่ม | fill | edge | ไอคอน (path ใน `CatRailA2`) | MVP |
|---|---|---|---|---|---|---|---|
| 1 | `start` | เริ่มต้น | พื้นฐาน | `#FFD23F` | `#9C7A00` | flag | มีบล็อก |
| 2 | `control` | ควบคุม | พื้นฐาน | `#FFAB3D` | `#A8610A` | repeat | มีบล็อก (หมวดตั้งต้น) |
| 3 | `logic` | ตรรกะ | พื้นฐาน | `#62D4C8` | `#137A70` | diamond | มีบล็อก |
| 4 | `math` | คณิต | พื้นฐาน | `#6CCB5F` | `#2F7A26` | บวก/คูณ | มีบล็อก |
| 5 | `text` | ข้อความ | พื้นฐาน | `#B79CFF` | `#5F3FC2` | type | ว่าง (แต่ชีตบล็อกมี join/letter/length/contains) |
| 6 | `variable` | ตัวแปร | ข้อมูล | `#FF8F70` | `#B03A1C` | box | มีบล็อก |
| 7 | `list` | รายการ | ข้อมูล | `#FF86A8` | `#B02C57` | list | ว่าง |
| 8 | `function` | ฟังก์ชัน | ข้อมูล | `#F08AD3` | `#9A2F85` | braces | มีบล็อก (กล่องเฟืองแบบ Mixly) |
| 9 | `arm` | Arm Control | School Bot | `#C9E04A` | `#6E7F0B` | แขนกล | ว่าง |
| 10 | `car` | Car Control | School Bot | `#D58CF2` | `#8239A8` | car | ว่าง |
| 11 | `motor` | มอเตอร์ | School Bot | `#5BAEFF` | `#1D63B8` | rotate | ว่าง (ชี้ไป Sensor › สั่งงาน) |
| 12 | `sensor` | Sensor | School Bot | `#5CCBEA` | `#17789A` | wifi | อุปกรณ์ 33 ตัว |
| 13 | `display` | Display | School Bot | `#7FE0A8` | `#1F8050` | monitor | อุปกรณ์ |
| 14 | `serial` | Serial | School Bot | `#8FA6FF` | `#3D4FC0` | message | มีบล็อก |
| 15 | `remote` | Remote Control | School Bot | `#D2A679` | `#7A4E1F` | remote | ว่าง (ชี้ไป Sensor) |
| 16 | `character` | ตัวละคร | พิเศษ | `#FFBE94` | `#A4521C` | smile | ไม่แสดงใน A2 |

- ชื่อหมวดบนแถบปนไทย/อังกฤษ (Arm Control, Sensor, Display ฯลฯ) ตามที่ mockup เขียน ต้องทำเป็น i18n key ทั้ง 3 ภาษา
- dropdown บนบล็อก = สีหมวดซ้อน `#14243D` 12% (ผสม sRGB) ตัว/ลูกศรเข้ม
- ไอคอนควบคุมทึบบนแถวหัวบล็อก: เฟือง = ขาวทึบบนวงกลม 28 สี edge ของหมวด · ? = วงกลมขาวขอบ 2px + เครื่องหมายสี edge · ปิดใช้ = `#5A6377`

## 3. ตัวอักษร

| ใช้ที่ | ฟอนต์ / ขนาด / น้ำหนัก |
|---|---|
| ทั้งแอป | IBM Plex Sans Thai 400/500/600/700 (ตรงกับ `--f-body`) |
| ตัวอักษรบนบล็อก, ชื่อหมวด, ปุ่ม, เนื้อหา | 16px · บล็อกและหมวด 700 · line-height 1.6 |
| ค่าในช่องบล็อก | 16px/600 |
| หัวกลุ่มแถบหมวด, ปุ่มเลื่อน, โค้ด | 14px/700 (โค้ดใช้ mono `Consolas, ui-monospace`) |
| หัวข้อใหญ่ใน Spec/แผ่นตัวอย่าง | 20–28px/700 (ไม่ใช่หน้าจอแอป) |

ความต่าง: root ของแอปใช้ 14px (`tailwind.css` `:root font-size:14px`) แต่ mockup ใช้ 16px เป็นค่าพื้นฐาน และไม่มี token mono

## 4. Layout, ระยะ, radius, เงา, ชั้น, motion

- จอ 1440×900 = แถบหน้าต่าง 60 + พื้นที่ 840 · กรอบ content x24 y72 กว้าง 1392 สูง 812 · ระยะระหว่างแผงแนวตั้ง 12 · ระหว่างแผงซ้าย–ขวา 24
- แผง: พื้น surface · radius 16 · ขอบ 2px วาดเป็นชั้นทับ (ไม่กินพื้นที่ เพื่อไม่ให้พิกัดลากเปลี่ยน)
- แผงบล็อก 516 (ลิ้นชักขยาย) / 740 (ยุบ) · ลิ้นชัก 284 / 60 · แถบหมวด 112 (รายการกว้าง 104 + margin 4) · รายการบล็อกลอย 340 (184 เมื่อแผงแคบกว่า 1000) พื้น surface 90%
- พื้นที่ต่อบล็อก: พื้นขาว + จุด `#DCE3EC` 1.3px ห่าง 24 · ซูม 50–200% ทีละ 25%
- radius: control/ปุ่ม 12 · การ์ด/แผง/toast/popover 16 · รายการหมวด 12 · ชิปไอคอนหมวด 9 (26×26) · pill 999 · บล็อก 7
- เงา: เล็ก `0 1px 3px rgba(20,36,61,.18)` (หมวดที่เลือก) · float `0 8px 20px rgba(20,36,61,.2)` (ชุดปุ่ม, popover, บล็อกที่ลาก) · dialog `0 16px 40px rgba(20,36,61,.3)`
- ชั้น z: base 0 · backdrop 20 · dialog 30 · popover/tooltip/หยุดทั้งหมด 40 · toast 50 · (z-stop 70 ในแถบหน้าต่าง)
- motion: dur-move 0.2s ease-out · dur-press 0.1s · popover เข้า 0.2s จาก translateY(-4px) · ต้องรองรับ `prefers-reduced-motion` (ไม่ขยาย ไม่เลื่อน ไม่จาง)
- focus: วง 2 ชั้น ขาว 2px + `#1866C9` 3px (ตรงแนวคิด `--focus` เดิม แต่สีต่าง) · ปุ่มในราง outline 3px `#1866C9` offset −3
- เป้ากด ≥ 44 (ตรง `--tap`) · ปุ่มควบคุมพื้นที่ทำงาน 44×44 ไอคอน 24 stroke 2.4 · ปุ่มในแถบลิ้นชักสูง 46

## 5. รูปทรงบล็อก (ค่าจาก `BlockA2.K()`)

| ค่า | ความหมาย | ค่า |
|---|---|---|
| `R` | มุม | 7 |
| `NX` / `ND` | ตำแหน่ง/ความลึกรอยบาก | 14 / 5 (รอยบาก `l4,5 h16 l4,-5` กว้างรวม 24) |
| `A` | ย่อหน้าปาก c-block | 18 |
| `HD` | ความสูงโดม hat | 22 · กว้าง `min(132, 0.62·w)` (ค่าประมาณจากสายตา OQ108) |
| `SH` | ความสูงช่องค่า | 28 |
| `PX` / `IG` | padding แนวนอน / ช่องว่างระหว่างชิ้น | 12 / 8 |
| ขอบบล็อก | ค่าเริ่มต้น | 1.5px (ปรับได้ 1–3 ใน Tweak) |

- ชนิด: **hat** (เริ่มต้น / วนซ้ำ / ฟังก์ชัน; หัวแบนไม่มีโดมใช้ในกล่องเฟือง) · **c-block** (ทำซ้ำ / ถ้า) · **stack** · **value** (pill) · **boolean** (หกเหลี่ยม; ช่องหกเหลี่ยมรับเฉพาะหกเหลี่ยม)
- สถานะ: ปกติ · ยังไม่ได้ต่อ/ปิดใช้ (`#F4F6F8` ขอบประ `#5A6377` 2px `6 4`) · ghost (`#C9CFD8` ขอบ `#AEB6C2`) · เตือน (ขอบประ `#B07F00` 2.5px `6 3`) · ย่อ · คอมเมนต์ (R21)
- ช่อง: ค่า = แคปซูลขาว ตัวเข้ม 16/600 · เงื่อนไข = หกเหลี่ยมว่าง · dropdown อุปกรณ์ = พื้นสีหมวด+12% เข้ม · ช่องสี = ตัวอย่าง 16×16 ขอบ 2px `#14243D` + ชื่อสีหรือค่า R G B · ช่องติ๊ก 28×28 · เป้ากดช่องอุปกรณ์ 44×44

## 6. นำไปทำใน Blockly 13

| ส่วนของ design | กลไกใน Blockly | หมายเหตุ |
|---|---|---|
| สีหมวด fill/edge + ตัวเข้ม | `Blockly.Theme` → `blockStyles` (`colourPrimary` = fill, `colourTertiary` = edge) + `categoryStyles` | ตัวอักษรเข้มต้องตั้งผ่าน CSS ของ renderer (ค่าเริ่มต้นของ Blockly เป็นตัวขาว) |
| รูปทรง hat โดม, มุม 7, รอยบาก 24×5, ปาก 18, ช่อง 28 | custom renderer สืบจาก `geras`/`thrasos` + `ConstantProvider` (`CORNER_RADIUS`, `NOTCH_WIDTH`, `NOTCH_HEIGHT`, `NOTCH_OFFSET_LEFT`, `STATEMENT_INPUT_NOTCH_OFFSET`, `START_HAT_HEIGHT/WIDTH`, `FIELD_*`) | boolean หกเหลี่ยม/value pill ต้องใช้ shape แบบ `zelos` ควรทดลองว่าสืบจาก `zelos` แล้วปรับง่ายกว่าหรือไม่ |
| แถบหมวด 112 + รายการบล็อกลอย | toolbox แบบ custom (React เป็นเจ้าของแถบ) + flyout ของ Blockly | `CatRailA2` เป็น React component ได้ตรงๆ (ปุ่ม, roving tabindex, ปุ่มเลื่อน) |
| พื้นจุด, ซูม 50–200%, ชุดปุ่มเลิกทำ/ซูม | `grid` options + `zoom` options + ปุ่ม React เรียก API ของ workspace | ปุ่มเป็น `IconBtn` เดิม |
| เมนูคลิกขวา R21 | `ContextMenuRegistry` + UI เมนูของเรา | กดค้าง 0.5 s และ Alt+M/Shift+F10 ต้องทำเอง |
| ค้นหาบล็อก, กระเป๋า (backpack) | plugin ของ Blockly มีให้ (`@blockly/plugin-workspace-search`, `@blockly/workspace-backpack`) | เพิ่ม dependency ต้องตัดสินก่อน (AGENTS.md: ไม่เพิ่มถ้าไม่จำเป็น) |
| ทางเลือกไม่ต้องลาก (WCAG 2.5.7) | keyboard navigation ของ Blockly (`@blockly/keyboard-navigation`) หรือทำเอง | ต้องตัดสิน |
| ช่องสี R G B, popover อุปกรณ์ | custom field + popover React | — |

## 7. ทางเลือกการวาง token ในแอป

**ผู้ใช้เลือกทาง (ข) 2026-11-02:** theme เฉพาะ Block Lab · ใช้ radius 12 และ base font 16 ตาม mockup · ไม่มีเอกสาร CI จริง บันทึกนี้จึงเป็นแหล่ง token

ความขัดกันหลัก: mockup ใช้โทนน้ำเงินเย็นของ CI แต่ `tailwind.css` ทั้งแอปเป็นโทนอุ่น (ส้ม/ครีม, `--color-brand` = ส้ม)

| ทาง | ทำอย่างไร | ผลกระทบ |
|---|---|---|
| (ก) เปลี่ยนทั้งแอปเป็น CI | แก้ค่าใน `:root` ของ `tailwind.css` | Arm Lab, Home, Adventure เปลี่ยนหน้าตาทั้งหมด อยู่นอกขอบเขต Block Lab ต้องเป็นการตัดสินของทีม |
| (ข) theme เฉพาะ Block Lab (เสนอ) | ประกาศค่า CI ใน selector ของ mode เช่น `[data-mode="block-programming"]` ทับ var เดิม (`--ink`, `--muted`, `--blue`, `--line`, `--bg` …) + เพิ่ม token ใหม่ที่ไม่มี (`warning-*`, `ring-subtle`, สีหมวด 16 คู่, mono, motion) | component เดิมใช้ต่อได้ทันทีเพราะอ่าน var เดียวกัน · เมื่อทีมย้ายทั้งแอปไป CI ก็ยก block นี้ขึ้น `:root` · ห้ามใส่สไตล์เป็น string ใน `.ts` (AGENTS.md) |
| (ค) ใช้โทนอุ่นเดิม | ปรับ mockup ให้เข้ากับ token เดิม | ขัดกับ mockup และ CI |

ไม่ว่าเลือกทางไหน ต้องเพิ่ม: สีหมวด 16 คู่ (ใช้ทั้งใน Tailwind และ Blockly theme จากแหล่งเดียว), `warning` เหลือง, font mono, motion tokens, radius control 12 (ตอนนี้ 10)

## 8. Accessibility ที่ mockup กำหนด

- ตัวอักษร ≥ 4.5:1 · สัญลักษณ์/ขอบที่ต้องเข้าใจ ≥ 3:1 (WCAG 1.4.11) · ไม่สื่อด้วยสีอย่างเดียว (หมวดว่าง = ขอบประ, พื้นที่ลบ = ไอคอน + ข้อความ)
- แถบหมวด: `role=group` ต่อกลุ่ม, ปุ่ม `aria-pressed`, roving tabindex, ↑↓ Home End, Enter/Space · หมวดว่างกดได้ ไม่ใช้ `disabled` แต่ต่อท้าย aria-label ด้วย " · ยังไม่มีบล็อก"
- ทุกการกระทำด้วยการลากต้องมีทางที่ไม่ต้องลาก · live region ประกาศ "วางแล้ว"/เพิ่ม/ลบ · Esc ปิดและคืนโฟกัส
- `prefers-reduced-motion` ทุก animation
