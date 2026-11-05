# Block Lab block catalog (บล็อกที่จะใช้)

> รวบรวม 2026-11-02 · ผู้ใช้แจ้งว่ายังไม่ 100% และจะมีเพิ่ม · ใช้คู่กับ `85-block-lab-requirements.md` (BL-4x) และ `90-open-questions.md` (B1–B11)

## แหล่ง

| แหล่ง | เนื้อหา | วิธีอ่าน |
|---|---|---|
| ชีต PDF `[School Bot] Fuction - Block Mixly.pdf` (Downloads) | 1 หน้า letter แนวนอน คอลัมน์: test · ประเภท · ชื่อ EN · ชื่อไทย · lib / analog / digital / digital+analog · หมวด | `pdftotext` ได้แค่ชื่อ EN (ไทยและเครื่องหมายติ๊กหาย) จึง render ด้วย pdf.js แล้วซูมทีละช่วง (ไม่มี `pdftoppm` ในเครื่อง) |
| สเปก MD `3-Channel RGB LED Module.md` (Downloads) | โค้ด C++ ที่ต้องการต่ออุปกรณ์ + ตาราง field ของ RGBSetup, RGBOpen, Active Buzzer + รูปบล็อก 5 รูป (base64) | อ่านตรง · รูปถอดเป็น PNG |
| mockup `BlocksPaneA2` | ตาราง 33 อุปกรณ์จาก `sensor-list.md` (ชื่อสั้น, ขาตัวอย่าง, โหมดอ่าน) + กลุ่มย่อย 7 กลุ่ม + บล็อกตัวอย่างต่อหมวด | อ่าน source |
| Mixly | stock ใน `C:\src\sb-app-mixly\resources\app\src\board\arduino_avr\` (`SB/`) · แพ็กเกจ ZLTech ใน `C:\Users\jabja\Downloads\mixly(ver.thai)\mixly\resources\app\src\board\arduino_avr\libraries\ThirdParty\001-Lib_ZLTech_UNO_AR_V2.0.9\` (`ZL/`: `block/ZLTech.js` = ZB, `generator/ZLTech.js` = ZG, `ZLTech.xml` = ZX, `libraries/`, `language/`) | subagent `mixly-source-scout` ไล่โค้ด · ตรวจซ้ำ 3 จุดด้วยมือแล้ว |

## ข้อค้นพบหลัก

1. **โค้ดใน MD ส่วนใหญ่ตรงกับแพ็กเกจ ZLTech ไม่ใช่บล็อก stock ของ Mixly** (DHT `dht.h`/`readTemperature(2)`, อัลตราโซนิก `INPUT_PULLUP`, MPU6050 แบบ Wire register, Gesture `APDS9960.h`, Color `Adafruit_TCS34725`, IR `decode_results`, Passive buzzer `tone(pin,f,dur)`) ค่า default ใน `ZX` ตรงกับ MD (DHT ขา 2 `ZX:666`, อัลตราโซนิก A2/A3 `ZX:675`, IR ขา 2 `ZX:910`)
2. แพ็กเกจ ZLTech มีเฉพาะใน build ไทยใน Downloads ไม่อยู่ใน sb-app-mixly และไม่อยู่ใน snapshot ของ engine · `import-source.ps1` จับแค่โฟลเดอร์ `generators|others` แต่ ZL ใช้ `generator/` (เอกพจน์) และ `engines/mixly/generator.go:76` กรอง `/generators/` เหมือนกัน
3. RGB (`display_rgb_module`) เป็นบล็อกใหม่ใน working tree ของ sb-app-mixly ที่ยังไม่ถูก import เข้า snapshot (`generators/actuator.js` และ `index.html` hash ไม่ตรง manifest · generator อื่น 20 ไฟล์ตรง)
4. **engine ใช้บางบล็อกไม่ได้:** generator `controls_tone`/`controls_notone` (`actuator.js:96,119`), `ir_recv*` (`communicate.js:8,67,101`) เรียก `Mixly.Editor.blockEditor` ซึ่ง `engines/mixly/src/bootstrap.js` ไม่มี (อ่านจากโค้ด ยังไม่ได้รัน) · ถ้าใช้บล็อก ZL แทนจะไม่ติด
5. Library: `Adafruit_TCS34725` **ไม่มี** ใน `C:\src\sb-app-mixly\arduino-cli\libraries` (มีใน `ZL/libraries/ZLTech_Adafruit_TCS34725`) · `dht.h` มี 3 ชุด (`ZLTech_dht`, `ZLTech_mixly`, `yfrobot`) arduino-cli อาจเลือกผิดตัว · `arduino-cli.yaml` ของ sb-app-mixly ชี้ `D:/mixly/...`
6. ไม่มี "RoboX" ใน Mixly หรือ ZL เลย · ใกล้สุดคือชุด ZLTech "开拓者1" (`ZLKPZAR_SETUP` ZB:2809) แต่ไม่มีหลักฐานว่าเป็นตัวเดียวกัน
7. timer ชนบน AVR: `tone()`, IRremote และ servo โหมด Timer2ServoPwm ใช้ Timer2 ร่วมกัน (Mixly เตือนเองด้วย `IR_AND_TONE_WARNING`)

## A. อุปกรณ์ (ชีต 33 ตัว + RoboX)

⏸ = ไม่ทำใน v1 (ผู้ใช้ตัดสิน 2026-11-02: แถว test แดงและ RoboX จำไว้สำหรับอนาคต) · หมวดยึดชีต (B1)

คอลัมน์ "อ่าน" จากชีต: D = digital, A = analog, D+A = ติ๊กทั้งสอง, D/A = ติ๊กช่อง digital+analog, lib · test: ✓ ผ่าน, ✗ แดง, – ไม่มีติ๊ก · ช่อง "Mixly ที่ใกล้สุด" = บล็อกที่โค้ดตรง MD มากที่สุด

| # | อุปกรณ์ (EN) | ชื่อไทยในชีต | ชื่อสั้น mockup | อ่าน | หมวดชีต | กลุ่มย่อย mockup | test | Mixly ที่ใกล้สุด | ตรง MD |
|---|---|---|---|---|---|---|---|---|---|
| 1 | 3-Channel RGB LED Module | โมดูล RGB LED 3 ช่อง | ไฟ RGB | lib | sensor | แสงและสี | ✓ | `display_rgb_module` (ใหม่ ยังไม่ import) + `RGB_color_rgb` | เกือบ: `setBrightness` อยู่ก่อน `setPixelColor`, ไม่แปลง 0–100% |
| 2 | Active Buzzer | บัซเซอร์แบบมีวงจรขับในตัว | บัซเซอร์ | D (Output) | sensor | เสียง | ✓ | `inout_digital_write2` หรือ ZL `yf_buzzer` | ตรง |
| 3 | Button Module | โมดูลปุ่มกด | ปุ่มกด | D | sensor | ปุ่มและการกด | ✓ | `inout_digital_read2` | ตรง (ZL `yf_btn` ใช้ `INPUT_PULLUP` ต่าง) |
| 4 | Touch Sensor | เซนเซอร์สัมผัส | สัมผัส | D | sensor | ปุ่มและการกด | ✓ | `inout_digital_read2` | ตรง |
| 5 ⏸ | Flame Sensor | เซนเซอร์ตรวจจับเปลวไฟ | เปลวไฟ | D+A | sensor | แสงและสี | ✗ | digital/analog read | digital ตรง · analog เพิ่ม `pinMode(A0, INPUT)` |
| 6 | Magnetic Reed Switch | สวิตช์แม่เหล็ก | แม่เหล็ก | D | sensor | ชน เอียง สั่น | ✓ | `inout_digital_read2` | ตรง |
| 7 ⏸ | Collision Switch | สวิตช์ตรวจจับการชน | ชน | D | sensor | ชน เอียง สั่น | ✗ | `inout_digital_read2` | ตรง |
| 8 | IR Obstacle Avoidance Sensor | เซนเซอร์อินฟราเรดตรวจจับสิ่งกีดขวาง | สิ่งกีดขวาง | D+A | sensor | ระยะและสิ่งกีดขวาง | ✓ | digital/analog read | เหมือนข้อ 5 |
| 9 | Tilt Switch | สวิตช์ตรวจจับการเอียง | เอียง | D+A | sensor | ชน เอียง สั่น | ✓ | digital/analog read | เหมือนข้อ 5 |
| 10 ⏸ | Vibration Sensor | เซนเซอร์ตรวจจับแรงสั่นสะเทือน | สั่น | D (MD บอก "หรือ analog") | sensor | ชน เอียง สั่น | ✗ | `inout_digital_read2` | ตรง |
| 11 | Sound Sensor | เซนเซอร์ตรวจจับเสียง | เสียง | D (mockup ใช้ analog) | sensor | เสียง | ✓ | `inout_digital_read2` | ตรง |
| 12 | Light Sensor | เซนเซอร์ตรวจจับแสง | แสง | D (mockup ใช้ analog) | sensor | แสงและสี | ✓ | `inout_digital_read2` | ตรง |
| 13 ⏸ | Thermistor Temperature Sensor | เซนเซอร์วัดอุณหภูมิแบบเทอร์มิสเตอร์ | อุณหภูมิ | A (MD บอก D หรือ A) | sensor | อากาศและสิ่งแวดล้อม | ✗ | `inout_analog_read` (ค่าดิบ) · stock `NTC_TEMP` ได้ °C | ตรงถ้าเป็นค่าดิบ |
| 14 | PIR Motion Sensor | เซนเซอร์ตรวจจับความเคลื่อนไหว | เคลื่อนไหว | D+A | sensor | ระยะและสิ่งกีดขวาง | ✓ | digital/analog read | เหมือนข้อ 5 |
| 15 | Dual-Channel Line Tracking Sensor | เซนเซอร์เดินตามเส้นแบบคู่ | ตามเส้น | D (2 ขา) | sensor | แสงและสี | ✓ | `inout_digital_read2` ×2 | ตรง |
| 16 | Relay Module | โมดูลรีเลย์ | รีเลย์ | D (Output) | sensor | สั่งงาน | ✓ | `inout_digital_write2` / ZL `yf_relay` | ตรง (MD เขียน `digitalRead` ผิด) |
| 17 | Fan Module | โมดูลพัดลม | พัดลม | D (Output, 2 ขา L9110) | sensor | สั่งงาน | ✓ | stock `Motor_8833` (2 ขา + PWM) / ZL `yf_fan` (1 ขา) | ไม่ตรง (MD ยังถามเรื่อง PWM) |
| 18 | Rotary Potentiometer | โพเทนชิโอมิเตอร์แบบหมุน | ลูกบิด | A | sensor | ปุ่มและการกด | ✓ | `inout_analog_read` | ตรง |
| 19 | Soil Moisture Sensor | เซนเซอร์วัดความชื้นในดิน | ความชื้นดิน | A (ช่อง D สีแดง) | sensor | อากาศและสิ่งแวดล้อม | ✓ | `inout_analog_read` | ตรง |
| 20 | Water Level Sensor | เซนเซอร์วัดระดับน้ำ | ระดับน้ำ | A | sensor | อากาศและสิ่งแวดล้อม | ✓ | `inout_analog_read` | ตรง |
| 21 | Grayscale Sensor Module | โมดูลเซนเซอร์ตรวจจับระดับสีเทา | ระดับเทา | A (ช่อง D สีแดง) | sensor | แสงและสี | ✓ | `inout_analog_read` | ตรง |
| 22 | MQ-7 Carbon Monoxide Sensor | เซนเซอร์ตรวจจับก๊าซคาร์บอนมอนอกไซด์ | ก๊าซ CO | D+A | sensor | อากาศและสิ่งแวดล้อม | ✓ | digital/analog read | เหมือนข้อ 5 |
| 23 | MQ-3 Alcohol Sensor | เซนเซอร์ตรวจจับแอลกอฮอล์ | แอลกอฮอล์ | D+A | sensor | อากาศและสิ่งแวดล้อม | ✓ | digital/analog read | เหมือนข้อ 5 |
| 24 ⏸ | Joystick Module | โมดูลจอยสติ๊ก | จอยสติ๊ก | D/A (แถวไฮไลต์ส้ม) | **remote control** | ปุ่มและการกด | ✗ | ZL `yf_joystick_d` / `yf_joystick_a` (แกนละบล็อก) | stock ไม่มี |
| 25 | DHT11 Temperature & Humidity Sensor | เซนเซอร์วัดอุณหภูมิและความชื้น | อุณหภูมิ/ชื้น | lib | sensor | อากาศและสิ่งแวดล้อม | ✓ | ZL `ZLTech_DHT11_TP` | ตรง (stock `DHT` ใช้ `DHT.h` ต่าง) |
| 26 | Ultrasonic Distance Sensor | เซนเซอร์อัลตราโซนิกวัดระยะทาง | ระยะ | D | sensor | ระยะและสิ่งกีดขวาง | ✓ | ZL `yf_ultrasonic_04` | ตรง (stock `chaoshengbo2` ใช้ `INPUT` + `delay(10)`) |
| 27 | OLED Display Module | โมดูลจอแสดงผล OLED | จอ OLED | lib | **Display** | สั่งงาน | ✓ | stock `oled_init` + `oled_page` + `oled_set_EN_Font` + `oled_print` | declare/setup ตรง · `page1()/page2()` ใน MD มาจากฟังก์ชันของผู้ใช้ |
| 28 | MPU6050 Accelerometer & Gyroscope | เซนเซอร์วัดความเร่งและไจโรสโคป | ความเร่ง | lib | sensor | ชน เอียง สั่น | ✓ | ZL `ZLTech_ZAccelInit/Read/Get` | ตรง (ZL ป้าย "มุม"/"ความเร่ง" สลับกันที่ ZG:418-429) |
| 29 | Gesture Sensor | เซนเซอร์ตรวจจับท่าทาง | ท่าทาง | lib | sensor | ชน เอียง สั่น | ✓ | ZL `ZLTech_ZGestureInit/InitColor/Ges/Color` | API ตรง แต่ Init กับ InitColor ใช้ key เดียวกัน เปิดสองโหมดพร้อมกันไม่ได้ |
| 30 | Color Sensor | เซนเซอร์ตรวจจับสี | สี | lib | sensor | แสงและสี | ✓ | ZL `ZLTech_ZTcsInit/Get/Led` | ตรง · **library ขาดใน sb-app-mixly** |
| 31 | Infrared Remote Control | รีโมทคอนโทรลอินฟราเรด | รีโมต | lib | **remote control** | ระยะและสิ่งกีดขวาง | ✓ | ZL `yf_ir_recv` + `yf_ir_get` + `yf_ir_val` | ตรง (`decode(decode_results*)` deprecated ใน IRremote 3.7.1 แต่ compile ได้) |
| 32 | Passive Buzzer | บัซเซอร์แบบไม่มีวงจรขับในตัว | บัซเซอร์ | D (ชีตเขียนประเภท Input) | sensor | เสียง | ✓ | ZL `yf_buzzer2` (tone f) / `yf_buzzer3` (tone f,dur) | ตรง (stock `controls_tone` ใช้ใน engine ไม่ได้) |
| 33 | 9g Servo Motor | เซอร์โวมอเตอร์ 9 กรัม | เซอร์โว | lib (Output) | **servo - motor** | สั่งงาน | – | stock `servo_move1` | MD ใช้ไม่ได้ (คัดลอกจาก passive buzzer) |
| 34 ⏸ | Setup RoboX | – | – | – (ประเภท Setup, ไฮไลต์ส้ม) | – | – | – | ไม่พบ | รอสเปก |

ข้อสังเกตต่อแถว:
- ชีตใส่ RGB เป็น "Input" และ Passive Buzzer เป็น "Input" น่าจะพิมพ์ผิด (เป็น Output)
- โหมดอ่านในชีตกับ mockup ไม่ตรงกันหลายตัว (เสียง, แสง: ชีต D แต่ mockup A · เปลวไฟ, สิ่งกีดขวาง, เอียง, เคลื่อนไหว: ชีต D+A แต่ mockup D) ให้ยึดชีต
- ขาที่ mockup ใช้ (`2`, `A0` ฯลฯ) เป็นค่าตัวอย่างชั่วคราว ชนกันได้

## B. บล็อกพื้นฐาน (ชีต)

ทุกบล็อก stock ด้านล่างมี generator ใน snapshot ของ engine และมีข้อความไทยใน `th.js` แล้ว

| ชื่อในชีต | หมวดชีต | หมวดที่เสนอ | Mixly | ต้องทำใหม่ | หมายเหตุ |
|---|---|---|---|---|---|
| setup | เริ่มต้น | เริ่มต้น | `base_setup` | — | hat "เริ่มต้น" ลบไม่ได้ |
| loop | เริ่มต้น | เริ่มต้น | ไม่มีบล็อก (บล็อกนอก setup ถูกห่อเป็น `void loop()` ที่ `common/blockly-core/arduino.js:724`) | **ใหม่** `sb_loop` (แผนเดิม D) | hat "วนซ้ำ" |
| wait | ควบคุม | ควบคุม | `controls_delay` | — | `delay(ms)` |
| if | ควบคุม | ควบคุม | `controls_if` | — | มี else if/else |
| while ("white") | ควบคุม | ควบคุม | `controls_whileUntil` (WHILE) | — | |
| repeat until | ควบคุม | ควบคุม | `controls_whileUntil` (UNTIL) | — | `while(!(c))` |
| forever | ควบคุม | ควบคุม | ไม่มี | **ใหม่** (หรือ preset ของ whileUntil + true) | `controls_end_program` คือ `while(true);` เป็นการหยุด ไม่ใช่วนซ้ำ |
| count | ควบคุม | ควบคุม | `controls_for` | — | `for(int i=a;i<=b;i=i+step)` |
| repeat | ควบคุม | ควบคุม | ไม่มีบล็อก "ทำซ้ำ N ครั้ง" ใน AVR (`controls_repeat` ถูกอ้างแค่ใน `blocks/control.js:161` ไม่มีนิยาม/toolbox) | **ใหม่** (หรือ preset ของ `controls_for`) | |
| break / continue | ควบคุม | ควบคุม | `controls_flow_statements` | — | |
| wait until | ควบคุม | ควบคุม | ไม่มี | **ใหม่** (หรือ whileUntil UNTIL body ว่าง) | |
| `>` `<` | ตรรกะ | ตรรกะ | `logic_compare` | — | dropdown มี `=` `≠` `≥` `≤` ด้วย จะซ่อนไหม |
| and / or / not | ตรรกะ | ตรรกะ | `logic_operation`, `logic_negate` | — | |
| `+ − × ÷` | math | คณิต | `math_arithmetic` | — | |
| random | math | คณิต | `math_random_int` | — | `random(a,b)` ไม่รวมขอบบน (Scratch รวม) |
| join | ข้อความ | ข้อความ | `text_join` | — | |
| letter of | ข้อความ | ข้อความ | `text_char_at` | — | 0-based (Scratch 1-based) |
| length of ("leght of") | ข้อความ | ข้อความ | `text_length` | — | |
| contains | ข้อความ | ข้อความ | `String_indexOf` | อาจต้องใหม่ | Mixly คืน index (−1 = ไม่พบ) ไม่ใช่ boolean |
| mod | ข้อความ | **คณิต** (B1) | `math_arithmetic` (`%`) | — | `(long)(a) % (long)(b)` |
| round | ข้อความ | **คณิต** (B1) | `math_to_int` | — | |
| "of" | ข้อความ | คณิต | ไม่ชัด | ยืนยันกับทีม | น่าจะเป็น "abs/sqrt/… of" แบบ Scratch (`math_single`/`math_trig`) |
| ตัวแปรไปใช้ต่อ | ตัวแปร | ตัวแปร | `variables_get` | — | |
| Set | ตัวแปร | ตัวแปร | `variables_set` | — | ต้องประกาศตัวแปร (`variables_declare`) ด้วย |
| Change | ตัวแปร | ตัวแปร | ไม่มีตรงตัว (`variables_change` เป็น type cast) | **ใหม่** หรือใช้ `variables_operation` (`x = x + v`) | |
| รายการ / ฟังก์ชัน | — | รายการ / ฟังก์ชัน | `procedures_*` / list blocks | — | ผู้ใช้สร้างเองแล้วบันทึก (หมวดเริ่มด้วยปุ่มสร้าง แบบ Scratch) · ฟังก์ชันอยู่ใน v1 · รายการยืนยันตอนทำ (B2) |

บล็อกที่ mockup มีแต่ชีตไม่มี — **อยู่ใน v1 ทั้งหมด** (ผู้ใช้ตัดสิน B2): `=`, จริง/เท็จ, แปลงค่า 0–255, จำกัดค่า 0–255, หยุดโปรแกรม, สร้างตัวแปร, ฟังก์ชัน (นิยาม/เรียก/คืนค่า/ค่าที่รับ), Serial print / ข้อความที่ได้รับ / มีข้อความเข้ามา, LED เปิด/ปิด/ตั้งความสว่าง/ค่าสี, "LED บนบอร์ด (ขา 13)"

## C. Library ที่ต้องมี

| library | ที่มีอยู่ (`C:\src\sb-app-mixly\arduino-cli\libraries`, gitignored) | ใช้กับ |
|---|---|---|
| Adafruit_NeoPixel | ✓ 1.12.0 | RGB |
| dht (`dht.h`) | `ZLTech_dht` ✓ แต่มี `dht.h` ซ้ำใน `ZLTech_mixly`, `yfrobot` | DHT11 |
| U8g2 | ✓ 2.34.22 | OLED |
| IRremote | ✓ 3.7.1 | IR |
| APDS9960 (`APDS9960.h`) | `ZLTech_APDS9960` (ใน ZL) | Gesture |
| Adafruit_TCS34725 | **ไม่มี** · มีใน `ZL/libraries/ZLTech_Adafruit_TCS34725` | Color |
| Servo | ✓ 1.2.1 | Servo |
| Wire | ✓ ใน AVR core 1.8.3 | MPU6050, OLED, Gesture, Color |

ต้องตรวจ license ของทุก library ก่อนแจกจ่าย (ZLTech ไม่มีไฟล์ license · grbl เป็น GPL แต่ไม่จำเป็นต้องใช้)

## D. ร่างแก้สเปก MD (Claude เสนอ ให้ทีมบล็อกรีวิว · B7)

ผู้ใช้ให้ร่างแก้ตามที่เห็นสมควรแล้วส่งทีมรีวิว (2026-11-02) จนกว่าทีมยืนยัน ข้อเหล่านี้มีสถานะ "เสนอ"

| # | อุปกรณ์/เรื่อง | ในสเปก | เสนอแก้ | เหตุผล |
|---|---|---|---|---|
| 1 | 9g Servo | เนื้อหาเป็นสำเนาของ Passive Buzzer | บล็อก "เซอร์โว ขา [▼] หมุนไป [90] องศา" → `#include <Servo.h>` · `Servo servo_N;` · `servo_N.attach(N);` ใน setup · `servo_N.write(deg);` (ตามแบบ stock `servo_move1` แต่ไม่ใส่ `delay` เอง) | ของเดิมใช้ไม่ได้ · ใช้ Servo library มาตรฐาน · การรอให้เด็กใส่บล็อก "รอ" เอง |
| 2 | Relay | `pinMode OUTPUT` + `digitalRead` | `digitalWrite(pin, HIGH/LOW)` | อุปกรณ์ส่งออกต้องเขียนค่า |
| 3 | Fan (L9110 2 ขา) | `pinMode` 2 ขา + `digitalRead` ×2 + "check PWM??" | บล็อก "พัดลม ขา INA [▼] ขา INB [▼] หมุน [ตามเข็ม▼] แรง [0–100]%" → `analogWrite(INA, map(speed,0,100,0,255)); digitalWrite(INB, LOW);` (กลับทิศสลับขา) · บล็อก "หยุดพัดลม" → ทั้งสองขา LOW · ขา INA ต้องเป็นขา PWM ของ UNO (3, 5, 6, 9, 10, 11) | L9110 คุมความเร็วด้วย PWM ขาเดียว ทิศด้วยอีกขา · dropdown กรองเฉพาะขา PWM |
| 4 | ตัวอย่างทุกตัว | ใช้ขา `0` ทั้งที่ field เป็น "GPIO 2" | ถือว่า `0` เป็น placeholder: โค้ดใช้ขาที่เลือกจริง · ไม่ให้เลือกขา 0/1 (เป็น RX/TX ของ Serial บน UNO) | ขา 0/1 ชนกับการ upload และ Serial |
| 5 | อุปกรณ์รับค่า (digital) | `digitalRead` อยู่ใน `setup()` | บล็อกค่า (มน) ใช้ได้ทุกที่ · `pinMode(pin, INPUT)` ใส่ใน setup ให้อัตโนมัติครั้งเดียว | สเปกเขียนเพื่อแสดงรูปโค้ดเท่านั้น |
| 6 | อุปกรณ์ digital+analog | "หรือ analogRead(A0)" | dropdown "แบบอ่าน [ดิจิทัล▼/แอนะล็อก]" ในบล็อกเดียว (B6) · แอนะล็อก → ขา A0–A5 เท่านั้น · ดิจิทัล → `digitalRead` | ตามที่ผู้ใช้เลือก |
| 7 | RGB brightness | ตาราง 0–100% แต่ตัวอย่าง 255 และโค้ด `setBrightness(20)` · อยู่ในบล็อก RGBOpen | **ย้ายไปอยู่ในบล็อก RGBSetup** (0–100% → `setBrightness` ครั้งเดียวใน setup) · RGBOpen เหลือ pixel + color | `setBrightness` ของ NeoPixel มีผลทั้งเส้นและทำให้สีเพี้ยนสะสมเมื่อเรียกซ้ำ ตั้งต่อดวงไม่ได้จริง (rules review) |
| 7b | ขา PWM, ขา 13, ขาซ้ำ, tone+IR | สเปกไม่ได้ระบุ | PWM (พัดลม, ความสว่าง LED) ใช้ขา 5/6 เท่านั้น · ขา 13 เฉพาะ LED · ห้ามใช้ขาเดียวกันหลายแบบ · ห้ามใช้ buzzer แบบ tone กับรีโมต IR พร้อมกัน | ขา 3/9/10/11 ตายเมื่อมี servo หรือ tone/IR (พัดลม REVERSE จะหมุนเต็มกำลัง) · ขา 13 กะพริบตอนรีเซ็ต · tone ทำให้ IR อ่านไม่ได้ (hardware-safety review) |
| 8 | RGB pixel | "ตำแหน่งของหลอดไฟ" เริ่ม 0 | ให้เด็กนับจาก 1 (generator ลบ 1) | สอดคล้องกับ B14 (นับแบบ Scratch) · ต้องให้ทีมยืนยัน |
| 9 | Active Buzzer | Output รับ HIGH/LOW หรือ 0/1 | ช่องเป็น dropdown "เปิด/ปิด" → `HIGH`/`LOW` | ค่า 0/1 กับ HIGH/LOW ได้ผลเหมือนกัน เด็กเข้าใจง่ายกว่า |
| 10 | Passive Buzzer | มี `digitalWrite(A4,HIGH)` + `tone(pin,f)` + `tone(pin,f,dur)` | 2 บล็อก: "เล่นโน้ต [ความถี่/โน้ต▼]" (`tone(pin,f)`) และ "เล่นโน้ต [..] นาน [ms]" (`tone(pin,f,dur)`) + "หยุดเสียง" (`noTone`) · ตัด `digitalWrite` | buzzer แบบ passive ต้องใช้คลื่น · ใช้ ZL `yf_buzzer2/3` (stock `controls_tone` ใช้ใน engine ไม่ได้) |
| 11 | IR remote | `decode()` อยู่ใน `setup()` + ค่าปุ่ม `0xFFA25D` | "เริ่มรับรีโมต ขา [▼]" (setup: `enableIRIn`) · "ได้รับปุ่มใหม่ไหม" (เงื่อนไข: `decode`+เก็บค่า+`resume` ใน loop) · "ปุ่มที่กดคือ [ปุ่ม▼]" (เทียบรหัส) | ต้องอ่านซ้ำใน loop · dropdown ชื่อปุ่มแทนรหัส hex |
| 12 | MPU6050 | ค่าเก็บใน global ต้องเรียก record ก่อนอ่าน | บล็อกค่าเดียว "ค่าความเร่ง/การหมุน แกน [X▼]" ที่เรียก `recordAccelRegisters()`/`recordGyroRegisters()` ให้เองก่อนคืนค่า | เด็กไม่ต้องรู้ลำดับ · แก้ป้ายสลับของ ZL (ZG:418-429) ไปพร้อมกัน |
| 13 | Gesture | ต้องเปิด gesture + light พร้อมกัน | setup เดียวเปิดทั้งสอง (`enableGestureSensor(true)` + `enableLightSensor(true)`) ทำใน overlay | ZL ใช้ key ชนกัน เปิดพร้อมกันไม่ได้ |
| 14 | OLED | `page1()/page2()` | บล็อก "แสดงข้อความ [..] ที่บรรทัด [1▼]" + "ล้างจอ" · generator วาดใน `firstPage/nextPage` ให้เอง | ฟังก์ชัน page ในสเปกมาจากโปรแกรมตัวอย่าง ไม่ใช่บล็อก |
| 15 | DHT11 | `#include "dht.h"` + `DHT_2.readTemperature(2)` | คงตามสเปก (ZL `ZLTech_DHT11_TP`) + เพิ่ม "ความชื้น" ใน dropdown · bundle เฉพาะ `ZLTech_dht` เพื่อไม่ให้ `dht.h` ชนกัน | ตรงกับ ZL ที่ทีมต้องการ |
| 16 | Ultrasonic | Echo `INPUT_PULLUP` | คงตามสเปก (ZL `yf_ultrasonic_04`) | ตรงกับ ZL |

## E. ผลต่อ engine และ REQ

- ต้องนำแพ็กเกจ ZLTech (block defs ใช้อ้างอิง, generator, xml, libraries) เข้ามาใน engine ผ่านทางที่ตรวจสอบได้ (importer + manifest) ไม่คัดลอกมือ และแก้ `import-source.ps1` + `generator.go:76` ให้รองรับโฟลเดอร์ `generator/` → REQ BL-49 (ใหม่)
- ต้อง import `display_rgb_module` จาก working tree ของ sb-app-mixly (ซึ่งเป็นงานแปลไทยที่ยังไม่ commit · T3)
- เพิ่ม shim `Mixly.Editor.blockEditor` ใน `bootstrap.js` หรือห้ามใช้ `controls_tone`/`ir_recv*` (ใช้ ZL แทน)
- toolchain ต้องระบุ library path ให้ arduino-cli เอง (ไม่ใช้ `arduino-cli.yaml` ที่ชี้ `D:/mixly`) และกัน `dht.h` ชนกัน
