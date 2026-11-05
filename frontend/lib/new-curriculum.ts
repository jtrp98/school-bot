// lib/new-curriculum.ts
// โครงหลักสูตรใหม่ ป.1–ป.6 (เมนู "หลักสูตร (ใหม่)")
// ที่มา: Google Sheet "Course Plan" — คอลัมน์ คอนเซปการเรียนรู้ / week / เรื่อง / จำนวนคาบ
// แผนการสอนรายสัปดาห์ที่เขียนเป็นบทความแล้วอยู่ใน lib/lesson-plans/

export interface CurriculumWeek {
  week: number;
  title: string;
  periods: number;
}

export interface CurriculumGrade {
  slug: "p1" | "p2" | "p3" | "p4" | "p5" | "p6";
  grade: number;
  label: string;
  theme: string;
  highlights: string[];
  weeks: CurriculumWeek[];
}

function weeks(rows: [string, number][]): CurriculumWeek[] {
  return rows.map(([title, periods], index) => ({ week: index + 1, title, periods }));
}

export const curriculumGrades: CurriculumGrade[] = [
  {
    slug: "p1",
    grade: 1,
    label: "ป.1",
    theme: "รู้จัก AI & ใช้อย่างปลอดภัย",
    highlights: [
      "AI เรียนรู้จากข้อมูล",
      "ฝึกสอน AI ด้วยภาพ/ท่าทาง",
      "คิดคำสั่ง Prompt ให้ชัด",
      "รู้ทันข้อมูล & มิจฉาชีพ AI",
      "เริ่มบังคับหุ่นด้วย Joystick",
    ],
    weeks: weeks([
      ["ทำความรู้จัก Robot & AI [Robotics]", 2],
      ["ทำความรู้จัก Robot & AI [AI]", 2],
      ["AI ใกล้ตัว", 1],
      ["ข้อมูลดิจิทัลเชื่อถือได้แค่ไหน", 2],
      ["ความปลอดภัยของข้อมูลดิจิทัล", 2],
      // ลำดับสัปดาห์ 6–15 ปรับตามคอมเมนต์คอลัมน์ K: ย้าย Data Literacy มาก่อนหลักการทำงานของ AI,
      // เปลี่ยน From Image to Video เป็น Robot Commands และสลับ AI ผิดได้ไหม มาก่อนมิจฉาชีพ AI
      ["Data Literacy", 2],
      ["หลักการทำงานของ AI [1]", 2],
      ["หลักการทำงานของ AI [2]", 2],
      ["Image Class AI", 3],
      ["Pose Classification", 2],
      ["Robot Commands", 2],
      ["Prompt Thinking [1]", 2],
      ["Prompt Thinking [2]", 2],
      ["AI ผิดได้ไหม", 1],
      ["รู้ทันมิจฉาชีพ AI", 2],
      ["การใช้ AI ที่ถูกต้อง", 1],
      ["AI Project", 3],
      ["AI Show / AI Challenge", 2],
      ["Joystick", 2],
      ["Joystick and Robot Arm", 3],
    ]),
  },
  {
    slug: "p2",
    grade: 2,
    label: "ป.2",
    theme: "ควบคุมแขนกล Robot Arm",
    highlights: [
      "เข้าใจมุม ทิศทาง ตำแหน่ง",
      "Servo Motor & Gripper",
      "บังคับด้วย Joystick และ Zide",
      "หยิบ-วาง นับ คัดแยกสี",
      "ออกแบบแขนกลของตัวเอง",
    ],
    weeks: weeks([
      ["Case by Robot Arm", 1],
      ["Robot Arm กับแขนมนุษย์", 2],
      ["มุมของ Robot Arm", 2],
      ["ทิศทาง ตำแหน่ง", 2],
      ["Motor Servo", 2],
      ["Joystick", 1],
      ["ควบคุม Robot Arm ผ่าน Joystick", 3],
      ["Zide", 1],
      ["ทดลองใช้ Zide", 2],
      ["Control Robot Arm from Joystick Mission 1.1 [การเคลื่อนที่]", 2],
      ["Control Robot Arm from Zide Mission 1.2 [การเคลื่อนที่]", 2],
      ["Control Robot Arm from Joystick Mission 2.1 [การเคลื่อนที่ไปในเส้นทางต่าง ๆ]", 2],
      ["Control Robot Arm from Zide Mission 2.2 [การเคลื่อนที่ไปในเส้นทางต่าง ๆ]", 2],
      ["Gripper", 2],
      ["น้ำหนักและการหยิบ", 2],
      ["Pick & Place from Zide", 3],
      ["นับจำนวนและการจับกลุ่ม", 2],
      ["สีและการคัดแยก", 2],
      ["Design Your Robot Arm", 3],
      ["Show Robot Arm Challenge", 2],
    ]),
  },
  {
    slug: "p3",
    grade: 3,
    label: "ป.3",
    theme: "ปูพื้น Coding & Sensor",
    highlights: [
      "เขียนโปรแกรมด้วย Mixly",
      "Input/Output, Sequence, Loop",
      "รู้จัก Sensor ระยะ แสง IR อุณหภูมิ",
      "ตัดสินใจด้วย if/else",
      "สร้างระบบอัตโนมัติอย่างง่าย",
    ],
    weeks: weeks([
      ["Intro Coding & Control", 1],
      ["Intro MiniProject", 1],
      ["Sensor คืออะไร", 1],
      ["Intro Mixly", 2],
      ["Digital Output", 2],
      ["RGB", 2],
      ["Sequence & Timing", 2],
      ["Digital Input", 2],
      ["Loop", 2],
      ["Sound", 2],
      ["Servo Motor", 2],
      ["MiniControl Project", 3],
      ["Distance Sensor", 2],
      ["Light Sensor", 2],
      ["IR Sensor", 2],
      ["Temperature Sensor", 2],
      ["Logic if/else", 2],
      ["Sensor + Servo", 2],
      ["Automatic System", 2],
      ["MiniProject", 3],
      ["Final Challenge", 2],
    ]),
  },
  {
    slug: "p4",
    grade: 4,
    label: "ป.4",
    theme: "จาก Sensor สู่นวัตกรรม",
    highlights: [
      "Sensor + Data → Action",
      "ใช้หลาย Sensor ร่วมกัน (AND/OR)",
      "สร้าง Smart System",
      "ปัญหา → ไอเดีย → ต้นแบบ",
      "ทดสอบ ปรับปรุง นำเสนอนวัตกรรม",
    ],
    weeks: weeks([
      ["What Innovation", 1],
      ["Sensor", 1],
      ["How Sensors Sense", 2],
      ["Sensor + Data", 2],
      ["One Sensor One Action", 2],
      ["Two Sensor", 2],
      ["Sensor And Sensor", 2],
      ["Sensor Or Sensor", 2],
      ["Multi Sensor Decision", 2],
      ["Smart System", 2],
      ["Innovation Problem", 1],
      ["Idea Generation", 2],
      ["Design Your Innovation", 2],
      ["Prototype 1", 3],
      ["Prototype 2", 3],
      ["Test & Observe", 2],
      ["Improve Your Innovation", 2],
      ["Smart Your Innovation", 2],
      ["Smart Innovation Challenge", 2],
      ["Innovation Project", 3],
    ]),
  },
  {
    slug: "p5",
    grade: 5,
    label: "ป.5",
    theme: "เขียนโปรแกรมควบคุม Robot Arm",
    highlights: [
      "พิกัด มุม และการคุม Joint",
      "ลำดับการทำงาน & บันทึกท่าทาง",
      "Pick & Place อัตโนมัติด้วย Sensor",
      "คัดแยกสิ่งของ (Sorting)",
      "Debug และพิชิตภารกิจ",
    ],
    weeks: weeks([
      ["ทบทวน Robot Arm", 1],
      ["Robot Arm Coordinate", 2],
      ["Servo Control", 2],
      ["มุมของ Robot Arm", 1],
      ["ควบคุม Joint", 2],
      ["ควบคุม Gripper", 2],
      ["Sequence [ลำดับการทำงาน]", 2],
      ["Robot Arm Motion", 2],
      ["บันทึกท่าทาง", 2],
      ["Pick & Place", 3],
      ["Sensor + Robot Arm Basic", 2],
      ["Sensor Trigger", 2],
      ["Automatic Pick & Place", 2],
      ["Sorting Robot Arm", 3],
      ["Repeat Task", 1],
      ["Debug Robot Arm", 2],
      ["Robot Arm Mission [1]", 2],
      ["Robot Arm Mission [2]", 2],
      ["Mini Project Robot Arm", 3],
      ["Robot Arm Challenge", 2],
    ]),
  },
  {
    slug: "p6",
    grade: 6,
    label: "ป.6",
    theme: "ระบบอัตโนมัติในอุตสาหกรรม",
    highlights: [
      "หุ่นยนต์ & Sensor ในโรงงาน",
      "Conveyor + Sensor + Robot Arm",
      "คัดแยก & ตรวจสอบคุณภาพ",
      "สร้างสายการผลิตจำลอง",
      "แก้ปัญหา & เพิ่มประสิทธิภาพ",
    ],
    weeks: weeks([
      ["Robot in Industry", 1],
      ["Industrial Automation", 1],
      ["Robot Arm Components", 2],
      ["Robot Coordinate & Position", 2],
      ["Robot Motion", 2],
      ["Industrial Sensor", 2],
      ["Object Detection", 2],
      ["Sensor + Robot Arm Basic", 2],
      ["Pick & Pack", 2],
      ["Industrial Sorting", 2],
      ["Multiple Sensors", 2],
      ["Decision System", 2],
      ["Conveyor", 2],
      ["Conveyor + Sensor", 2],
      ["Quality Inspection", 2],
      ["Mini Product Line", 3],
      ["Troubleshooting", 2],
      ["Cycle Time & Efficiency", 2],
      ["Industrial Robot Challenge", 3],
      ["Industrial Automation Showcase", 2],
    ]),
  },
];

export function findCurriculumGrade(slug: string) {
  return curriculumGrades.find((grade) => grade.slug === slug);
}

export function totalPeriods(grade: CurriculumGrade) {
  return grade.weeks.reduce((sum, week) => sum + week.periods, 0);
}
