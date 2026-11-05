// lib/lesson-plans/p5.ts
// แผนการสอน ป.5 สัปดาห์ที่ 1–3 (หน่วยการเรียนรู้ที่ 1–3)
// ที่มา: แผนการจัดการเรียนรู้ ป.5 (Google Docs) ปรับตามคอมเมนต์คอลัมน์ K ใน Course Plan

import type { LessonPlan } from "./types";

const logicP5 = {
  code: "ว 4.2 ป.5/1",
  indicator: "ใช้เหตุผลเชิงตรรกะในการแก้ปัญหา การอธิบายการทำงาน การคาดการณ์ผลลัพธ์จากปัญหาอย่างง่าย",
};

const programP5 = {
  code: "ว 4.2 ป.5/2",
  indicator: "ออกแบบและเขียนโปรแกรมที่มีการใช้เหตุผลเชิงตรรกะอย่างง่าย ตรวจหาข้อผิดพลาดและแก้ไข",
};

const questionP5 = {
  code: "ท 3.1 ป.5/2",
  indicator: "ตั้งคำถามและตอบคำถามเชิงเหตุผลจากเรื่องที่ฟังและดู",
};

const unit1: LessonPlan = {
  gradeSlug: "p5",
  week: 1,
  unitLabel: "หน่วยที่ 1",
  title: "ทบทวน Robot Arm",
  subtitle: "Robot Arm ทำงานร่วมกันอย่างไร",
  minutesPerPeriod: 50,
  summary: [
    "Robot Arm เป็นระบบที่หลายส่วนทำงานร่วมกัน ได้แก่ Base, Joint, Servo, Gripper และ Controller เด็กเรียนส่วนประกอบเหล่านี้มาแล้ว — ป.2 เรียน Joint มุม Servo Gripper และการสั่งงานผ่าน Zide ส่วน ป.3 เรียน Servo และ Mixly หน่วยนี้จึงใช้ช่วง Recall เช็กความรู้เดิมก่อน แล้วจัดระบบภาพรวมให้ครบด้วยแนวคิด Component → Function → Effect",
    "ผู้เรียนรู้จัก End Effector (อุปกรณ์ปลายแขน) โดย Gripper เป็นตัวอย่างหนึ่ง และเชื่อม Control Flow: Command → Controller → Servo → Joint → End Effector เปลี่ยนตำแหน่ง/ท่า เพื่อเตรียมเข้าสู่ Coordinate และ Servo Control ในหน่วยถัดไป",
  ],
  objectives: [
    "ระบุส่วนประกอบหลักของ Robot Arm พร้อมบอกหน้าที่และผลที่เกิดขึ้นเมื่อส่วนนั้นทำงานได้",
    "อธิบาย Control Flow: Command → Controller → Servo → Joint → End Effector และบอกได้ว่า Gripper เป็นตัวอย่างหนึ่งของ End Effector",
    "อธิบายความสัมพันธ์ Servo → Joint → ท่าของแขนและตำแหน่ง End Effector ได้",
    "สังเกต Robot Arm จริงหรือโมเดล แล้วระบุว่า Servo/Joint แต่ละจุดเกี่ยวกับการเคลื่อนไหวส่วนใด",
    "คาดการณ์ว่าถ้า Joint หนึ่งขยับหรือส่วนหนึ่งไม่ทำงาน จะส่งผลต่อ Robot Arm อย่างไร",
    "ใช้อุปกรณ์อย่างระมัดระวัง ไม่ฝืน Joint และปฏิบัติตามข้อตกลงด้าน Safety",
  ],
  standards: [
    { ...logicP5, core: "อธิบาย “ส่วนประกอบ → หน้าที่ → ผลที่เกิดขึ้น” ของ Robot Arm และคาดการณ์ผลเมื่อ Servo หรือ Joint เปลี่ยนการทำงาน" },
    { ...questionP5, core: "ตั้งและตอบคำถามจากการสาธิต เช่น ส่วนใดขยับ เพราะเหตุใด และ Joint ใดเกี่ยวข้อง" },
  ],
  integrations: [
    { subject: "คณิตศาสตร์", detail: "เชื่อมความรู้เดิมเรื่องมุมกับท่าของ Servo/Joint โดยไม่เน้นการคำนวณ" },
    { subject: "วิทยาศาสตร์และเทคโนโลยี", detail: "สังเกตการทำงานเป็นระบบ การคาดการณ์ผล และความปลอดภัยในการใช้อุปกรณ์" },
  ],
  learning: {
    knowledge:
      "ส่วนประกอบหลัก Base, Joint, Servo, Gripper, Controller; Component → Function → Effect; End Effector (Gripper เป็นตัวอย่างหนึ่ง); Control Flow Command → Controller → Servo → Joint → End Effector และผลเมื่อส่วนหนึ่งทำงานผิดปกติ",
    skills: "การสังเกตส่วนประกอบ การจับคู่ส่วนประกอบ–หน้าที่–ผล การเรียง Control Flow การคาดการณ์ การทดลองทีละ Joint และการวิเคราะห์ What if?",
    attitude: "รอบคอบ กล้าคาดการณ์และตรวจสอบจากผลจริง และให้ความสำคัญกับ Safety",
  },
  vocabulary: [
    { term: "Base", meaning: "ฐานของแขนกล" },
    { term: "Servo", meaning: "ตัวขับที่ทำให้ Joint หมุนไปยังมุมที่กำหนด" },
    { term: "Controller", meaning: "ตัวควบคุมที่จัดการคำสั่ง" },
    { term: "End Effector", meaning: "อุปกรณ์ปลายแขนที่ทำงานกับวัตถุ เช่น Gripper" },
  ],
  materials: [
    "Robot Arm จริงหรือโมเดล พร้อมชุดควบคุมสำหรับสาธิตการขยับ Servo/Joint ทีละจุดที่ความเร็วต่ำ",
    "ภาพ Robot Arm ที่ระบุส่วนประกอบ · Component Card · Function Card · Effect Card · ป้าย End Effector",
    "แผนภาพ Command → Controller → Servo → Joint → End Effector",
    "ใบงาน Exercise 1: Robot Arm System Map (การบ้าน)",
  ],
  sessions: [
    {
      number: 1,
      title: "Robot Arm ทำงานร่วมกันอย่างไร?",
      minutes: 50,
      goal: "นักเรียนจัดระบบส่วนประกอบของ Robot Arm ด้วย Component → Function → Effect รู้จัก End Effector อธิบาย Control Flow และคาดการณ์ผลจากการสาธิตอย่างปลอดภัย",
      steps: [
        {
          time: "0–5",
          phase: "Recall: ป.2 และ ป.3 เราเรียนอะไรมาแล้ว?",
          activity: "ดู Robot Arm แล้วช่วยกันบอกสิ่งที่เคยเรียน: Joint มุม Servo Gripper และ Zide (ป.2) · Servo และ Mixly (ป.3)",
          teacher: "ใช้ช่วงนี้เช็กความรู้เดิม ถ้าเด็กจำได้ ให้ข้ามการสอนซ้ำแล้วไปจัดระบบภาพรวมเลย",
          note: "เด็กเคยเรียนส่วนประกอบส่วนใหญ่ตอน ป.2–ป.3 แล้ว การอ้างถึงตรง ๆ ช่วยประหยัดเวลา — คาบนี้เนื้อหาเยอะ จะทำทันถ้าเด็กจำได้",
        },
        {
          time: "5–15",
          phase: "ส่วนประกอบ → หน้าที่ → ผลที่เกิดขึ้น",
          activity: "จัดความรู้เดิมเข้าภาพรวม เติมส่วนที่ยังขาด แล้วจับคู่ Component → Function → Effect พร้อมรู้จัก End Effector",
          teacher: "ถาม “ส่วนนี้ทำอะไร?” “เมื่อส่วนนี้ทำงาน อะไรในระบบเปลี่ยน?”",
        },
        {
          time: "15–22",
          phase: "Servo และ Joint",
          activity: "สังเกตการสาธิต Servo/Joint 1 จุด แล้วระบุว่า Servo ทำให้ Joint ไหนขยับ และ End Effector เปลี่ยนอย่างไร",
          teacher: "เชื่อมกับเรื่อง Servo/มุมที่เคยเรียน",
        },
        {
          time: "22–29",
          phase: "Robot Arm Control Flow",
          activity: "เรียงแผนภาพ Command → Controller → Servo → Joint → End Effector แล้วอธิบายด้วยภาษาของตนเอง",
          teacher: "ถาม “คำสั่งเริ่มจากตรงไหน?” “Controller ส่งต่อให้ใคร?” “สุดท้ายส่วนปลายเปลี่ยนอย่างไร?”",
        },
        {
          time: "29–41",
          phase: "Robot Arm System Check",
          activity: "เลือก Joint 1 จุด Predict ว่า End Effector จะเปลี่ยนอย่างไร แล้ว Test และ Observe จากนั้นสุ่ม Effect Card เช่น “Servo ไม่ขยับ” เพื่ออธิบายผล",
          teacher: "ทดลองทีละ Joint ในช่วงมุมที่ปลอดภัย",
        },
        {
          time: "41–46",
          phase: "Compare & Explain",
          activity: "เปรียบเทียบสิ่งที่คาดกับผลจริง และอธิบายว่า Joint ใดทำให้ท่าเปลี่ยน",
          teacher: "ถาม “สังเกตจากอะไร?” เพื่อย้ำการอธิบายจากหลักฐาน",
        },
        {
          time: "46–50",
          phase: "สรุปและมอบ Exercise 1",
          activity: "สรุป “ส่วนประกอบ → หน้าที่ → ผลที่เกิดขึ้น” และ Control Flow แล้วรับ Exercise 1 ไปทำเป็นการบ้าน",
          teacher: "ทิ้งคำถาม “เราจะบอกได้อย่างไรว่าปลายแขนอยู่ตรงไหน?” เข้าสู่หน่วยที่ 2",
          note: "Exercise 1 ไม่มีช่วงเวลาในตาราง จึงให้ทำเป็นการบ้าน แล้วตรวจร่วมกันช่วงต้นหน่วยที่ 2",
        },
      ],
    },
  ],
  guides: [
    {
      id: "control-flow",
      title: "Control Flow ของ Robot Arm",
      blocks: [
        {
          heading: "คำสั่งเดินทางผ่านระบบ",
          flow: [
            { label: "Command", sub: "คำสั่ง" },
            { label: "Controller", sub: "ตัวควบคุม" },
            { label: "Servo", sub: "ตัวขับ" },
            { label: "Joint", sub: "ข้อต่อ" },
            { label: "End Effector", sub: "เปลี่ยนตำแหน่ง/ท่า" },
          ],
        },
        {
          icon: "❓",
          heading: "What if?",
          text: "Servo ไม่ขยับ · Gripper ไม่เปิด–ปิด · Controller ไม่ส่งคำสั่ง",
          questions: ["ส่วนไหนจะไม่ทำงานต่อ?", "End Effector จะได้รับผลอย่างไร?"],
        },
      ],
      tip: "ไม่ลงรายละเอียด PWM, Wiring หรือ Syntax — สาธิตทีละ Joint ด้วยความเร็วต่ำ และไม่ยื่นมือเข้าเขตการเคลื่อนที่",
    },
    {
      id: "system-map",
      title: "Exercise 1 — Robot Arm System Map (การบ้าน)",
      blocks: [
        { icon: "🦾", heading: "ระบุส่วนประกอบ", text: "Base · Joint · Servo · Gripper · Controller และวงส่วนที่เป็น End Effector" },
        { icon: "➡️", heading: "เติม Control Flow", text: "Command → Controller → Servo → Joint → End Effector" },
        { icon: "❓", heading: "What if? 1 ข้อ", text: "“ถ้าส่วนนี้ไม่ทำงาน ระบบจะได้รับผลอย่างไร”" },
      ],
    },
  ],
  assessment: [
    {
      topic: "ส่วนประกอบ หน้าที่ และ Control Flow",
      method: "ถามตอบ การจับคู่การ์ด และ Exercise 1",
      criteria: "ระบุส่วนประกอบ หน้าที่ ผลที่เกิดขึ้น และอธิบาย Control Flow ได้",
    },
    {
      topic: "การสังเกต Servo/Joint (ท 3.1 ป.5/2)",
      method: "Robot Arm System Check",
      criteria: "ชี้ Servo/Joint และอธิบายว่าส่วนใดเคลื่อนไหวและส่งผลต่อ End Effector อย่างไร",
    },
    {
      topic: "การคาดการณ์ (ว 4.2 ป.5/1)",
      method: "Predict → Test → Observe และ Effect Card",
      criteria: "คาดการณ์ผลเมื่อ Joint ขยับหรือส่วนหนึ่งไม่ทำงาน และเปรียบเทียบกับผลจริง",
    },
    { topic: "การร่วมมือและ Safety", method: "สังเกตพฤติกรรม", criteria: "ไม่ฝืน Joint ไม่ยื่นมือเข้าเขตการเคลื่อนที่ และปฏิบัติตามข้อตกลง" },
  ],
  takeaways: [
    "Robot Arm ประกอบด้วย Base, Joint, Servo, Gripper และ Controller — มองแบบ “ส่วนประกอบ → หน้าที่ → ผลที่เกิดขึ้น”",
    "End Effector คือส่วนปลายที่ทำงานกับวัตถุ (Gripper เป็นตัวอย่างหนึ่ง) เมื่อ Joint เปลี่ยนท่า ตำแหน่งปลายแขนก็เปลี่ยน",
    "Control Flow: Command → Controller → Servo → Joint → End Effector",
  ],
  bridge: {
    nextLabel: "หน่วยที่ 2: Robot Arm Coordinate",
    question: "“เรารู้แล้วว่าอะไรทำให้ส่วนปลายแขนกลขยับ แล้วเราจะบอกได้อย่างไรว่ามันอยู่ตรงไหน?”",
    teaser: "“คาบหน้าเราจะใช้ค่ามุมบอกท่า และใช้พิกัดบอกตำแหน่ง”",
  },
};

const unit2: LessonPlan = {
  gradeSlug: "p5",
  week: 2,
  unitLabel: "หน่วยที่ 2",
  title: "Robot Arm Coordinate",
  subtitle: "ค่ามุมบอกท่า พิกัดบอกตำแหน่ง",
  minutesPerPeriod: 50,
  summary: [
    "Robot Arm มองได้ 2 แบบที่เชื่อมกัน: “ชุดค่ามุมของ Joint” (Joint Angle Coordinate) ใช้ค่ามุม Base / Shoulder / Elbow บอก Pose หรือท่าของแขน และ “พิกัดในพื้นที่ทำงาน” (Workspace Coordinate) ใช้จุดอ้างอิง O กับกริด X–Y บอกตำแหน่งของ Object, End Effector และ Target",
    "Home/Reference Pose เป็นท่าเริ่มหรือจุดเทียบเท่านั้น ไม่ใช่ 0° ของทุก Joint หน่วยนี้ใช้กริด 2 มิติแบบ “มองจากด้านบน” จึงเลือกท่าที่หมุน Base เป็นหลักเพื่อให้เห็นพิกัด X–Y เปลี่ยนชัด ไม่คำนวณ Forward/Inverse Kinematics และไม่ใช้แกน Z",
  ],
  objectives: [
    "อธิบายว่า Coordinate ใช้บอกท่าหรือตำแหน่งจากจุดอ้างอิง และ Home Pose ไม่ใช่ 0° ของทุก Joint",
    "แยกได้ว่าชุดค่ามุมของ Joint บอก Pose ส่วนพิกัด X–Y ใน Workspace บอก Position",
    "อธิบายได้ว่าเมื่อค่ามุม Joint เปลี่ยน Pose เปลี่ยน และตำแหน่ง End Effector อาจเปลี่ยนตาม",
    "อ่าน บันทึก และเปรียบเทียบ Pose จากค่ามุม Base / Shoulder / Elbow ได้",
    "อ่าน Workspace Grid และระบุพิกัด (X, Y) ของ Object, Target และ End Effector ได้",
    "เปลี่ยน Joint ทีละจุด บันทึกการเปลี่ยนตำแหน่ง End Effector และเปรียบเทียบกับ Target โดยไม่ใช้สูตร",
  ],
  standards: [
    { ...logicP5, core: "ใช้ค่ามุม จุดอ้างอิง และพิกัด อธิบายและคาดการณ์ว่าเมื่อ Pose เปลี่ยน ตำแหน่ง End Effector อาจเปลี่ยนอย่างไร" },
    { ...questionP5, core: "ตั้งและตอบคำถามจากภาพ แผนผัง และการสาธิต โดยอ้างอิงสิ่งที่สังเกตได้" },
  ],
  integrations: [
    { subject: "คณิตศาสตร์", detail: "ใช้ความรู้เรื่องมุม (ค 2.1 ป.4/2) อ่านค่ามุมของ Joint และใช้พิกัด (X, Y) บนกริดอย่างง่าย" },
    { subject: "การสื่อสารด้วยภาพ", detail: "Pose Card และ Workspace Grid" },
  ],
  learning: {
    knowledge:
      "Coordinate และจุดอ้างอิง, Joint Angle Coordinate บอก Pose, Home/Reference Pose เป็นจุดเทียบ, พิกัด X–Y ใน Workspace บอก Position, ความสัมพันธ์ Joint เปลี่ยน → Pose เปลี่ยน → End Effector อาจเปลี่ยนตำแหน่ง",
    skills: "อ่านและเปรียบเทียบ Pose Card อ่าน Workspace Grid ระบุพิกัด ทดลองเปลี่ยนทีละ Joint และบันทึก End Effector ก่อน–หลัง",
    attitude: "อ่านและบันทึกอย่างรอบคอบ ใช้จุดอ้างอิงเดียวกัน และปฏิบัติตาม Safety",
  },
  vocabulary: [
    { term: "Pose", meaning: "ท่าของแขน บอกด้วยชุดค่ามุมของ Joint" },
    { term: "Position", meaning: "ตำแหน่ง บอกด้วยพิกัด (X, Y)" },
    { term: "Home / Reference Pose", meaning: "ท่าเริ่มหรือจุดเทียบ" },
    { term: "Workspace", meaning: "พื้นที่ที่แขนกลเอื้อมถึงและทำงานได้" },
  ],
  materials: [
    "Robot Arm จริงหรือโมเดล — ใช้ชื่อ Joint (Base / Shoulder / Elbow) ให้ตรงกับแขนกลของโรงเรียน",
    "Pose Card และ Home/Reference Pose ด้วยค่ามุมที่ปลอดภัย",
    "Workspace Grid X–Y แบบมองจากด้านบน วางใต้แขนกล กำหนดจุด O ทิศ X–Y และขนาดช่องเหมือนกันทุกกลุ่ม",
    "Object / Target / End Effector Marker · ตาราง Angle → End Effector Position · ใบงาน Exercise 1–2",
  ],
  sessions: [
    {
      number: 1,
      title: "ชุดค่ามุมของ Joint — บอก Pose ด้วยค่ามุม",
      minutes: 50,
      goal: "นักเรียนแยกได้ว่าชุดค่ามุมของ Joint ใช้บอก Pose และอ่าน บันทึก เปรียบเทียบค่ามุม พร้อมคาดการณ์ผลต่อ End Effector ได้",
      steps: [
        { time: "0–5", phase: "เชื่อมจากหน่วยที่ 1", activity: "ตรวจ Exercise 1 สั้น ๆ แล้วลองบอกเพื่อนให้ตั้งท่าด้วยคำว่า “หมุนอีกนิด” และสังเกตว่าได้ท่าไม่เหมือนกัน", teacher: "ถาม “ต้องมีข้อมูลอะไรบ้างถึงจะได้ท่าเดียวกัน?”" },
        { time: "5–13", phase: "Coordinate และ Pose", activity: "รู้จัก Pose Card ที่ระบุค่ามุม Base / Shoulder / Elbow เช่น 90° / 60° / 120° เทียบกับ Home", teacher: "อ่านตามลำดับ Base → Shoulder → Elbow และย้ำว่า Home ไม่ใช่ 0° ของทุก Joint" },
        { time: "13–23", phase: "Pose Card Match", activity: "จับคู่ภาพ/โมเดลกับ Pose Card แล้วเปรียบเทียบกับ Home ว่า Joint ใดต่าง", teacher: "ถาม “ถ้าสลับ Base กับ Elbow ท่าจะเหมือนเดิมไหม?”" },
        { time: "23–32", phase: "Pose Compare Lab — เปลี่ยน 1 Joint", activity: "เปรียบเทียบ Pose A–B ที่ต่างกัน 1 Joint แล้วสังเกตว่า End Effector เปลี่ยนอย่างไร", teacher: "เปลี่ยนทีละ Joint ด้วยค่ามุมที่ปลอดภัย ไม่คำนวณจากสูตร" },
        { time: "32–36", phase: "Pose Quick Check", activity: "ดู Pose Card 2–3 คู่ แล้วตอบว่า Joint ใดเปลี่ยน และ End Effector น่าจะขยับไปทางใด", teacher: "ตรวจว่าแยก Pose ออกจาก Position ได้" },
        { time: "36–48", phase: "Exercise 1: Pose Coordinate Detective", activity: "อ่าน Pose Card เติมค่ามุม เปรียบเทียบ 2 Pose วง Joint ที่เปลี่ยน และคาดการณ์ทิศทาง End Effector", teacher: "ถาม “ค่ามุมนี้บอกท่าหรือตำแหน่ง?”" },
        { time: "48–50", phase: "สรุป", activity: "สรุปว่า “ชุดค่ามุมของ Joint ใช้บอก Pose”", teacher: "ถาม “บอกท่าได้แล้ว จะบอกตำแหน่งบนโต๊ะได้อย่างไร?”" },
      ],
    },
    {
      number: 2,
      title: "พิกัดใน Workspace — วัตถุและ End Effector อยู่ตรงไหน?",
      minutes: 50,
      goal: "นักเรียนใช้จุด O และกริด X–Y ระบุพิกัดของ Object, Target และ End Effector แล้วเชื่อม Pose กับตำแหน่ง End Effector ที่สังเกตได้",
      steps: [
        { time: "0–5", phase: "Recall: Pose บอกอะไรได้บ้าง?", activity: "ทบทวนว่าชุดค่ามุมบอก “ท่า” แต่ยังไม่บอกตำแหน่งบนโต๊ะ", teacher: "ถาม “จะบอกตำแหน่งให้ตรงกันได้อย่างไร?”" },
        {
          time: "5–13",
          phase: "Workspace, จุดอ้างอิง และแกน X–Y",
          activity: "รู้จักกริดแบบมองจากด้านบน จุด O แกน X–Y และฝึกอ่านพิกัดค่าบวก เช่น (2, 3) โดยอ่าน X ก่อน Y",
          teacher: "ย้ำว่ากริดนี้ “มองจากด้านบน” — ถาม “ถ้าเปลี่ยนจุดเริ่ม พิกัดจะเหมือนเดิมไหม?”",
        },
        { time: "13–22", phase: "Workspace Reading", activity: "อ่านและบันทึกพิกัด Object, Target และ End Effector แล้วสลับกันตรวจ", teacher: "ตรวจว่าอ่าน X ก่อน Y" },
        { time: "22–34", phase: "Workspace Coordinate Mission", activity: "รับ Mission Card เช่น Object = (2,4), Target = (4,1) วาง Marker ให้ตรง แล้วให้เพื่อนอ่านกลับ", teacher: "ใช้ขั้น O → X → Y → Check" },
        {
          time: "34–40",
          phase: "เชื่อม Angle → End Effector Position",
          activity: "ดู Pose A–B ที่ต่างกันที่ Base แล้วบันทึกพิกัด End Effector ก่อน–หลัง พร้อมเปรียบเทียบกับ Target",
          teacher: "ถาม “End Effector ย้ายจากพิกัดไหนไปไหน?” “ใกล้ Target ขึ้นไหม?”",
          note: "บนกริดมองจากด้านบน การขยับ Shoulder/Elbow มักเปลี่ยน “ความสูง” มากกว่าตำแหน่งบนกริด เด็กอาจงงว่าทำไมพิกัดแทบไม่เปลี่ยน — จึงเลือก Pose A–B ที่หมุน Base ให้เห็นการเปลี่ยน X–Y ชัด",
        },
        { time: "40–48", phase: "Exercise 2: Workspace Coordinate Mission", activity: "อ่านพิกัด Object / Target / End Effector แล้วดู Pose ก่อน–หลังเพื่อระบุพิกัด End Effector และ Joint ที่เปลี่ยน", teacher: "ตรวจจุด O และลำดับ X–Y" },
        { time: "48–50", phase: "Share และสรุป", activity: "ตัวแทนกลุ่มอธิบายว่า Pose ต่างจาก Position อย่างไร", teacher: "เชื่อมหน่วยที่ 3: ใช้ค่ามุมสั่ง Servo จริง" },
      ],
    },
  ],
  guides: [
    {
      id: "pose-vs-position",
      title: "Pose vs Position",
      blocks: [
        { icon: "📐", heading: "Pose — บอกด้วยค่ามุม", text: "Base / Shoulder / Elbow = 90° / 60° / 120°", compare: "บอก “ท่า” ของแขน" },
        { icon: "📍", heading: "Position — บอกด้วยพิกัด", text: "End Effector อยู่ที่ (3, 2) บนกริดที่มองจากด้านบน", compare: "บอก “ตำแหน่ง” บนโต๊ะ" },
        {
          heading: "ความสัมพันธ์",
          flow: [{ label: "มุมเปลี่ยน" }, { label: "Pose เปลี่ยน" }, { label: "Position อาจเปลี่ยน" }],
        },
      ],
      tip: "ไม่สอน Forward/Inverse Kinematics แกน Z หรือค่าติดลบ — หน่วยที่ 4 (มุมของ Robot Arm) จะเน้นต่างออกไป คือเทียบมุมที่สั่งกับมุมจริง",
    },
  ],
  assessment: [
    {
      topic: "Coordinate, Pose และ Position",
      method: "ถามตอบ Pose Card Match และ Exercise 1",
      criteria: "แยกได้ว่าค่ามุมบอก Pose พิกัดบอก Position และอธิบายบทบาทของ Home Pose ได้",
    },
    { topic: "การอ่านและเปรียบเทียบ Pose", method: "Pose Compare Lab และ Exercise 1", criteria: "อ่านค่ามุมตามลำดับ ระบุ Joint ที่เปลี่ยน และคาดการณ์ผลต่อ End Effector" },
    { topic: "การอ่านพิกัด (ท 3.1 ป.5/2)", method: "Workspace Mission และ Exercise 2", criteria: "อ่านพิกัด (X, Y) ได้ถูกต้องจากจุด O" },
    {
      topic: "เชื่อม Angle กับ Position (ว 4.2 ป.5/1)",
      method: "Pose ก่อน–หลัง และ Exercise 2",
      criteria: "บันทึก End Effector ก่อน–หลัง และอธิบาย “มุมเปลี่ยน → Pose เปลี่ยน → Position อาจเปลี่ยน” ได้",
    },
    { topic: "ความรอบคอบและ Safety", method: "สังเกตพฤติกรรม", criteria: "ใช้จุดอ้างอิงเดียวกันและปฏิบัติตาม Safety" },
  ],
  takeaways: [
    "Joint Angle Coordinate บอก Pose ส่วน Workspace Coordinate ใช้ (X, Y) บอกตำแหน่ง Object / End Effector / Target",
    "Home/Reference Pose เป็นท่าเริ่มหรือจุดเทียบ ไม่ใช่ 0° ของทุก Joint",
    "เมื่อค่ามุมเปลี่ยน Pose เปลี่ยน และตำแหน่ง End Effector อาจเปลี่ยนตาม — สังเกตได้โดยไม่ต้องใช้สูตร",
  ],
  bridge: {
    nextLabel: "หน่วยที่ 3: Servo Control",
    question: "“เรารู้มุมเป้าหมายแล้ว จะทำให้ Servo ไปมุมนั้นจริงได้อย่างไร?”",
    teaser: "“คาบหน้าเราจะเขียนโปรแกรมสั่ง Servo ไปยังมุมที่ปลอดภัย”",
  },
};

const unit3: LessonPlan = {
  gradeSlug: "p5",
  week: 3,
  unitLabel: "หน่วยที่ 3",
  title: "Servo Control",
  subtitle: "สั่งมุม Servo ด้วยโปรแกรมอย่างปลอดภัย",
  minutesPerPeriod: 50,
  summary: [
    "Servo เป็นตัวขับที่ทำให้ Joint เคลื่อนไปยังมุมเป้าหมายตามคำสั่งจากโปรแกรม หน่วยนี้เปลี่ยน “ค่ามุมที่ใช้บอก Pose” จากหน่วยที่ 2 ให้เป็น “คำสั่งที่ Servo ทำจริง” ด้วย Mixly ให้ต่อเนื่องกับ ป.3–ป.4 โดยข้อมูลสำคัญ 2 ส่วนคือ Pin/Port บอกว่าสั่ง Servo ตัวไหน และ Target Angle บอกว่าให้ไปมุมไหน",
    "ใช้ Predict → Run → Observe → Compare ทุกครั้ง เพราะมุมจริงอาจคลาดจากค่าที่สั่ง ใช้ Safe Min–Max ของแต่ละ Joint และไม่สมมติว่าทุก Joint ปลอดภัยเต็ม 0°–180° ส่วน Movement Speed เป็นคนละเรื่องกับ Target Angle — หน่วยนี้แค่สังเกต ส่วนการควบคุมความเร็วด้วย for loop เรียนในหน่วยที่ 8",
  ],
  objectives: [
    "อธิบายได้ว่าโปรแกรมใช้ Pin/Port เลือก Servo และใช้ Target Angle กำหนดมุมเป้าหมาย",
    "แยก Target Angle (ปลายทาง) กับ Movement Speed (เร็ว/ช้า) และใช้ Current เทียบ Target เพื่อคาดว่าค่ามุมต้องเพิ่มหรือลด",
    "อธิบายได้ว่า Joint แต่ละจุดมีช่วงมุมปลอดภัยต่างกัน จึงต้องใช้ค่า Safe Min–Max ของอุปกรณ์จริง",
    "ใช้ Servo–Joint–Pin Mapping วางแผน สร้าง และรันโปรแกรมใน Mixly เพื่อสั่ง Servo 1 ตัวไปยังมุมที่ปลอดภัย",
    "ใช้ Predict → Run → Observe → Compare บันทึกผลจริง และเปรียบเทียบเมื่อเปลี่ยน Target Angle",
    "ตรวจหาข้อผิดพลาดพื้นฐาน เช่น Pin ไม่ตรง มุมไม่เหมาะ หรือยังไม่ได้อัปโหลด แล้วแก้และทดสอบซ้ำ",
  ],
  standards: [
    { ...programP5, core: "เขียนโปรแกรมด้วย Mixly สั่ง Servo ไปยังมุมเป้าหมายที่ปลอดภัย ทดสอบ ตรวจหาข้อผิดพลาด และแก้ไข" },
    { ...logicP5, core: "คาดการณ์ผลจาก Servo/Pin, Current–Target Angle และ Safe Range แล้วอธิบายเหตุผลจากผลการทดสอบจริง" },
  ],
  integrations: [{ subject: "คณิตศาสตร์", detail: "ใช้ความรู้เรื่องมุม (ค 2.1 ป.4/2) อ่านและกำหนดมุม Servo" }],
  learning: {
    knowledge:
      "Servo เป็นตัวขับ Joint, Program → Controller → Servo → Joint, Pin/Port ระบุ Servo, Current Angle กับ Target Angle, Commanded Angle ≠ ผลจริงเสมอไป, Safe Min–Max, Target Angle ≠ Movement Speed และวงจร Test → Observe → Check → Fix → Retest",
    skills: "อ่าน Servo–Joint–Pin Mapping วางแผนคำสั่ง เขียนและอัปโหลดโปรแกรม ทดลองทีละ Servo บันทึก Prediction–Result และ Debug จากหลักฐาน",
    attitude: "เขียนและทดสอบอย่างรอบคอบ เปลี่ยนทีละค่า และให้ Safety มาก่อนความเร็ว",
  },
  vocabulary: [
    { term: "Pin / Port", meaning: "ช่องที่บอกว่าสั่ง Servo ตัวไหน" },
    { term: "Current Angle", meaning: "มุมปัจจุบัน" },
    { term: "Target Angle", meaning: "มุมปลายทางที่ต้องการ" },
    { term: "Safe Min–Max", meaning: "ช่วงมุมที่ Joint นั้นขยับได้อย่างปลอดภัย" },
    { term: "Movement Speed", meaning: "ความเร็วในการเคลื่อนที่ — คนละเรื่องกับ Target Angle" },
  ],
  materials: [
    "ชุด Servo/Robot Arm + Controller กลุ่มละ 1 ชุด เพราะทุกกลุ่มต้อง Upload/Run เอง (ถ้ามีไม่พอ ให้หมุนเวียนสถานี)",
    "คอมพิวเตอร์ที่ติดตั้ง Mixly (ใช้แพลตฟอร์มเดียวกับ ป.3–ป.4) · สาย USB/แหล่งจ่ายไฟ",
    "Servo–Joint–Pin Mapping ของชุดจริง · Safe Angle Card ระบุ Safe Min–Max ของแต่ละ Joint",
    "ตาราง Current → Target → Prediction → Result → Match? · ตัวอย่างเร็ว/ช้าสำหรับครูสาธิต",
    "ใบงาน Exercise 1: Servo Command Builder และ Exercise 2: Servo Control Debug Mission",
  ],
  sessions: [
    {
      number: 1,
      title: "Servo Command — สั่งมุมด้วย Mixly",
      minutes: 50,
      goal: "นักเรียนเชื่อมค่ามุมจากหน่วยที่ 2 กับคำสั่ง Servo และเขียน/รันโปรแกรมใน Mixly เพื่อสั่ง Servo 1 ตัวไปยังมุมที่ปลอดภัย",
      steps: [
        { time: "0–5", phase: "เชื่อมจากหน่วยที่ 2", activity: "ดูการสาธิตว่าเมื่อโปรแกรมส่งคำสั่ง 90° Servo จะขับ Joint ไปยังมุมเป้าหมาย", teacher: "ถาม “โปรแกรมต้องรู้อะไรอีกจึงจะสั่ง Servo ตัวถูก?”" },
        {
          time: "5–13",
          phase: "Servo รับคำสั่งจากโปรแกรมอย่างไร",
          activity: "รู้จัก Program → Controller → Servo → Joint ดู Mapping เช่น Servo A → Shoulder → Pin 5 แล้วอ่านบล็อกตัวอย่างใน Mixly",
          teacher: "Pin/Port = สั่งตัวไหน · Target Angle = ไปมุมไหน · Current เทียบ Target = ค่ามุมต้องเพิ่มหรือลด",
        },
        { time: "13–23", phase: "Build the Servo Command", activity: "วางแผนเป็นขั้น: เลือก Servo/Pin → เลือก Target → ตรวจ Safe Range → Predict → Upload/Run → Observe → Compare", teacher: "ถาม “ถ้า Pin ผิด เรากำลังสั่งตัวไหน?” “มุมนี้อยู่ใน Safe Range ไหม?”" },
        {
          time: "23–32",
          phase: "Safe Angle Test",
          activity: "เริ่มจาก Current Angle ที่กำหนด เลือก Target 2–3 ค่าใน Safe Range คาดการณ์ แล้วบันทึก Current → Target → Prediction → Result → Match?",
          teacher: "เปลี่ยนทีละค่า ทดลองทีละ Servo ถ้ามีเสียงฝืนหรือชนขอบให้หยุดทันที",
        },
        { time: "32–38", phase: "Challenge: Which Command?", activity: "เลือกคำสั่ง A/B/C ที่ “ถูก Servo + ถูก Target + ปลอดภัย” เช่น Shoulder = Pin 5, Current 40°, Safe 20°–140°, Mission 100°", teacher: "เน้นเหตุผลมากกว่าจำ Syntax" },
        { time: "38–48", phase: "Exercise 1: Servo Command Builder", activity: "เติม Pin/Port และ Target Angle บันทึก Prediction และตรวจ Safe Range", teacher: "ถาม “ถ้าผลไม่ตรง Prediction จะเช็กอะไรก่อน?”" },
        { time: "48–50", phase: "สรุป", activity: "สรุปว่า “โปรแกรมต้องรู้ Servo/Pin และ Target Angle และมุมต้องอยู่ใน Safe Range”", teacher: "ถาม “ไปมุมเดียวกัน แต่เร็วหรือช้าต่างกันได้ไหม?”" },
      ],
    },
    {
      number: 2,
      title: "Angle, Speed & Debug — ควบคุมให้ปลอดภัย",
      minutes: 50,
      goal: "นักเรียนแยก Target Angle ออกจาก Movement Speed และใช้ Test → Observe → Check → Fix → Retest แก้ข้อผิดพลาดพื้นฐานได้อย่างปลอดภัย",
      steps: [
        { time: "0–5", phase: "Recall: Angle สั่งอะไร?", activity: "ทบทวนโปรแกรมจากคาบ 1 ว่า Pin ใดถูกสั่ง Target เท่าไร อยู่ใน Safe Range ไหม", teacher: "ถาม “สั่ง 90° เหมือนกัน จะเร็วเท่ากันเสมอไหม?”" },
        {
          time: "5–13",
          phase: "Target Angle ≠ Movement Speed",
          activity: "เปรียบเทียบว่า Target Angle บอก “ปลายทาง” ส่วน Speed บอก “เร็ว/ช้า”",
          teacher: "ถ้าคำสั่ง Servo พื้นฐานไม่มี Speed ให้ครูสาธิตด้วยตัวอย่างที่เตรียมไว้ — for loop เรียนในหน่วยที่ 8",
        },
        { time: "13–22", phase: "Same Angle, Different Pace", activity: "สังเกตการสาธิต 2 แบบที่ไปมุมเดียวกันแต่เร็ว/ช้าต่างกัน แล้วบันทึก", teacher: "ถาม “ปลายทางเหมือนกันไหม?” “อะไรทำให้รู้สึกเร็ว/ช้า?”" },
        { time: "22–34", phase: "Servo Control Mission", activity: "รับ Mission Card เช่น “Servo A → 60°” ตรวจ Safe Range แก้ค่า แล้ว Predict → Run → Observe → Compare", teacher: "เน้น Angle + Safe Range เป็นแกนหลัก Speed เป็น Bonus" },
        {
          time: "34–40",
          phase: "Challenge: Why Didn’t It Move?",
          activity: "แก้สถานการณ์ 2–3 แบบ เช่น สั่ง Shoulder แต่ Elbow ขยับ · แก้ Angle แล้ว Servo ไม่ขยับ · Joint เข้าใกล้ขอบและมีเสียงฝืน",
          teacher: "ตอบเป็นหลักฐาน “สังเกตอะไร → เช็กอะไร → เพราะอะไร” ถ้ามีเสียงฝืนต้อง Stop ก่อน Debug",
        },
        { time: "40–48", phase: "Exercise 2: Servo Control Debug Mission", activity: "อ่านบล็อก Servo 3–4 สถานการณ์ เขียน Prediction หา Error แก้ 1 จุด และระบุสิ่งที่จะ Retest", teacher: "ตรวจว่าหา Error จากหลักฐานได้" },
        { time: "48–50", phase: "Share และสรุป", activity: "สรุป “Pin ให้ถูก → Angle ให้ปลอดภัย → Test → Observe → Fix”", teacher: "เชื่อมหน่วยที่ 4: สั่งมุมแล้ววัดผลจริงของแต่ละ Joint" },
      ],
    },
  ],
  guides: [
    {
      id: "servo-command",
      title: "อ่านคำสั่ง Servo แบบเหตุ–ผล",
      blocks: [
        { icon: "🔌", heading: "Pin / Port", text: "ถ้า Pin เปลี่ยน → Joint ที่ได้รับคำสั่งอาจเปลี่ยนหรือไม่ขยับ" },
        { icon: "🎯", heading: "Target Angle", text: "ถ้า Target เปลี่ยน → ปลายทางเปลี่ยน · Current เทียบ Target → ค่ามุมต้องเพิ่มหรือลด" },
        { icon: "🛡️", heading: "Safe Min–Max", text: "ใช้ค่าจากอุปกรณ์จริง ไม่สมมติว่าทุก Joint ปลอดภัย 0°–180°" },
        {
          heading: "ทำทุกครั้งที่รัน",
          flow: [{ label: "Predict" }, { label: "Run" }, { label: "Observe" }, { label: "Compare" }],
        },
      ],
    },
    {
      id: "debug-servo",
      title: "Debug จากหลักฐาน",
      blocks: [
        { icon: "🔀", heading: "สั่ง Shoulder แต่ Elbow ขยับ", compare: "เช็ก Pin / Mapping" },
        { icon: "⏸️", heading: "แก้ Angle แล้ว Servo ไม่ขยับ", compare: "เช็กว่า Upload/Run แล้วหรือยัง" },
        { icon: "⚠️", heading: "Joint เข้าใกล้ขอบ มีเสียงฝืน", compare: "Stop ก่อน แล้วเช็ก Angle กับ Safe Range" },
        {
          heading: "วงจรแก้ไข",
          flow: [{ label: "Test" }, { label: "Observe" }, { label: "Check" }, { label: "Fix" }, { label: "Retest" }],
        },
      ],
      tip: "แก้ทีละ 1 จุด และยังไม่ขยายไปเรื่อง Power/Wiring/Sensor ซึ่งเก็บไว้หน่วยที่ 16",
    },
  ],
  assessment: [
    {
      topic: "Servo, Angle, Speed และ Safe Range",
      method: "ถามตอบ Which Command? และ Exercise 1",
      criteria: "อธิบายบทบาท Pin, Target, Current และ Safe Range และแยก Angle ออกจาก Speed ได้",
    },
    {
      topic: "เขียนและรัน Servo Command (ว 4.2 ป.5/2)",
      method: "Build the Servo Command, Safe Angle Test และ Exercise 1",
      criteria: "สร้าง/แก้บล็อกให้ Servo 1 ตัวทำงานใน Safe Range และเปรียบเทียบ Prediction กับ Result ได้",
    },
    {
      topic: "Predict–Test (ว 4.2 ป.5/1)",
      method: "Safe Angle Test และ Servo Control Mission",
      criteria: "ใช้ Predict → Run → Observe → Compare เมื่อเปลี่ยน Target Angle",
    },
    {
      topic: "ตรวจหาข้อผิดพลาดและแก้ไข",
      method: "Why Didn’t It Move? และ Exercise 2",
      criteria: "เลือก Stop / Check Pin / Check Angle / Upload ใหม่ จากหลักฐาน แก้ 1 จุด และ Retest ได้",
    },
    { topic: "ความรอบคอบและ Safety", method: "สังเกตพฤติกรรม", criteria: "เปลี่ยนทีละค่า ใช้มุมปลอดภัย และไม่ฝืนกลไก" },
  ],
  takeaways: [
    "โปรแกรม Servo ต้องระบุ Pin/Port ให้ถูกตัว และกำหนด Target Angle ให้ชัด",
    "Target Angle บอกปลายทาง ส่วน Speed บอกเร็ว/ช้า — แต่ละ Joint มี Safe Min–Max ของตัวเอง",
    "เมื่อผลไม่ตรง Prediction ให้ Test → Observe → Check → Fix → Retest ทีละจุด",
  ],
  bridge: {
    nextLabel: "หน่วยที่ 4: มุมของ Robot Arm",
    question: "“สั่ง 90° แล้ว Joint จริงไปถึง 90° พอดีไหม?”",
    teaser: "“คาบหน้าเราจะเทียบมุมที่สั่งกับมุมจริงของแต่ละ Joint”",
  },
};

export const p5LessonPlans: LessonPlan[] = [unit1, unit2, unit3];
