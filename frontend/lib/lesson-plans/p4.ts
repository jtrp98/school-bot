// lib/lesson-plans/p4.ts
// แผนการสอน ป.4 สัปดาห์ที่ 1–4 (หน่วยการเรียนรู้ที่ 1–4)
// ที่มา: แผนการจัดการเรียนรู้ ป.4 (Google Docs) ปรับตามคอมเมนต์คอลัมน์ K ใน Course Plan
// ชื่อแบบฝึกใช้ระบบเดียวกันทั้งระดับชั้น: Exercise 1, 2 นับใหม่ในแต่ละหน่วย

import type { LessonPlan } from "./types";

const logicP4 = {
  code: "ว 4.2 ป.4/1",
  indicator: "ใช้เหตุผลเชิงตรรกะในการแก้ปัญหา การอธิบายการทำงาน การคาดการณ์ผลลัพธ์จากปัญหาอย่างง่าย",
};

const questionP4 = {
  code: "ท 3.1 ป.4/4",
  indicator: "ตั้งคำถามและตอบคำถามเชิงเหตุผลจากเรื่องที่ฟังและดู",
};

const unit1: LessonPlan = {
  gradeSlug: "p4",
  week: 1,
  unitLabel: "หน่วยที่ 1",
  title: "What Innovation",
  subtitle: "Innovation เริ่มจากปัญหา",
  minutesPerPeriod: 50,
  summary: [
    "นวัตกรรมสำหรับ ป.4 ไม่ได้หมายถึงของใหม่ที่ซับซ้อนหรือราคาแพงเสมอไป แต่คือแนวคิด วิธีการ หรือสิ่งที่ช่วยแก้ปัญหาและสร้างคุณค่าให้ผู้ใช้ จะเป็นการสร้างสิ่งใหม่หรือปรับสิ่งเดิมให้ดีขึ้นก็ได้",
    "หน่วยนี้ต่อยอดจาก MiniProject ตอน ป.3 โดยเปลี่ยนจุดเริ่มจาก “อยากสร้างอะไร” เป็น “มีปัญหาอะไร ใครเจอ และอยากให้ดีขึ้นอย่างไร” ผ่านกรอบ Problem → User → Need → Idea → Better ปัญหาที่เด็กเก็บไว้ใน Innovation Journal วันนี้จะถูกนำกลับมาใช้จริงในหน่วยที่ 11 Innovation Problem",
  ],
  objectives: [
    "อธิบายได้ว่านวัตกรรมเป็นแนวคิด วิธีการ หรือสิ่งที่ช่วยแก้ปัญหาและสร้างคุณค่าให้ผู้ใช้ โดยไม่จำเป็นต้องเป็นสิ่งใหม่ทั้งหมด",
    "อธิบายหลักคิดได้ว่า Innovation เริ่มจาก Problem/Need คำนึงถึง User และควรทำให้บางอย่าง Better ขึ้น",
    "สังเกตสถานการณ์ใกล้ตัวและระบุ Problem, User และ Need ได้อย่างสอดคล้องกัน",
    "เสนอ Idea เบื้องต้นที่สัมพันธ์กับ Need อธิบายว่าอะไร Better ขึ้น และเปรียบเทียบ Idea อย่างง่ายได้",
    "รับฟังมุมมองของผู้อื่น และเห็นคุณค่าของการใช้เทคโนโลยีอย่างรับผิดชอบ",
  ],
  standards: [
    { ...logicP4, core: "สังเกตและวิเคราะห์ปัญหาใกล้ตัว ระบุผู้ใช้และความต้องการ แล้วอธิบายแนวคิดที่อาจช่วยแก้ปัญหา" },
    {
      code: "ท 3.1 ป.4/3",
      indicator: "พูดแสดงความรู้ ความคิดเห็น และความรู้สึกเกี่ยวกับเรื่องที่ฟังและดู",
      core: "อภิปรายตัวอย่าง Innovation นำเสนอ Idea และอธิบายว่าช่วยแก้ปัญหาหรือทำให้สิ่งเดิมดีขึ้นอย่างไร",
    },
  ],
  integrations: [
    { subject: "ภาษาไทย", detail: "ท 3.1 ป.4/6 มารยาทในการฟัง การดู และการพูด" },
    { subject: "สังคมศึกษา", detail: "ส 2.1 ป.4/1 การปฏิบัติตนเป็นสมาชิกที่ดีของชุมชน" },
  ],
  learning: {
    knowledge:
      "ความหมายของ Innovation, “ของใหม่” กับ “สิ่งที่สร้างคุณค่า”, หลักคิด Problem/Need – User/Context – Idea – Better/Value, การปรับของเดิมให้ดีขึ้นก็เป็น Innovation ได้ และบทบาทของ Sensor ในนวัตกรรมบางประเภท",
    skills: "การสังเกตปัญหา การวิเคราะห์ User และ Need การแยกปัญหาออกจากวิธีแก้ การเปรียบเทียบ Idea และการสื่อสารเหตุผล",
    attitude: "เห็นคุณค่าของการแก้ปัญหาอย่างมีเป้าหมาย เคารพความต้องการของผู้ใช้ และใช้เทคโนโลยีอย่างรับผิดชอบ",
  },
  vocabulary: [
    { term: "Innovation", meaning: "แนวคิด วิธีการ หรือสิ่งที่แก้ปัญหาและสร้างคุณค่า" },
    { term: "Problem / Need", meaning: "ปัญหา / สิ่งที่ผู้ใช้ต้องการ" },
    { term: "User", meaning: "ผู้ใช้หรือคนที่เจอปัญหา" },
    { term: "Better", meaning: "สิ่งที่ดีขึ้น เช่น ง่ายขึ้น ปลอดภัยขึ้น ประหยัดขึ้น" },
  ],
  materials: [
    "Slide ภาพนวัตกรรมใกล้ตัว เช่น ประตูอัตโนมัติ ถังขยะเปิดอัตโนมัติ ระบบรดน้ำ ไฟอัตโนมัติ",
    "ภาพ “ของเดิม → ของที่ปรับดีขึ้น” เช่น ถังขยะฝาเปิดมือ → แบบเหยียบ → แบบอัตโนมัติ",
    "Situation Card · Problem–User–Need–Idea–Better Card · Idea Comparison Card",
    "Innovation Journal ประจำตัว สำหรับเก็บ Problem Hunter ไว้ใช้ในหน่วยที่ 11",
  ],
  sessions: [
    {
      number: 1,
      title: "Innovation เริ่มจากปัญหา",
      minutes: 50,
      goal: "นักเรียนอธิบายหลักคิดของ Innovation ระบุ Problem–User–Need เสนอ Idea และบอกได้ว่าอะไร Better ขึ้น",
      steps: [
        {
          time: "0–5",
          phase: "เชื่อมประสบการณ์",
          activity: "ชมภาพนวัตกรรมใกล้ตัว 3–4 ตัวอย่าง และเชื่อมกับ MiniProject ตอน ป.3 ว่าเราเคยสร้างระบบมาแล้ว",
          teacher: "ถาม “ก่อนมีสิ่งนี้ เราเจอปัญหาอะไร?” “สิ่งนี้ช่วยให้ดีขึ้นตรงไหน?”",
        },
        {
          time: "5–12",
          phase: "สร้าง Concept",
          activity: "ร่วมสร้าง Concept ว่า Innovation คือสิ่งที่แก้ Problem/Need และสร้างคุณค่าให้ User พร้อมดูตัวอย่าง “ของเดิม → ปรับให้ดีขึ้น”",
          teacher: "ย้ำว่า “ใหม่กว่า” หรือ “ซับซ้อนกว่า” ไม่ได้แปลว่า “ดีกว่า” เสมอไป",
        },
        {
          time: "12–22",
          phase: "วิเคราะห์ตัวอย่าง",
          activity: "วิเคราะห์ประตูอัตโนมัติ ไฟอัตโนมัติ หรือระบบรดน้ำ ด้วยกรอบ Problem → User → Need → Idea → Better",
          teacher: "ถามทีละส่วน และให้ลองเทียบ Idea 2 แบบที่แก้ปัญหาเดียวกัน",
        },
        {
          time: "22–35",
          phase: "Activity: Innovation Detective",
          activity: "ทำงานกลุ่มกับ Situation Card เติม 5 ช่อง และเลือกว่าดีขึ้นด้านใด",
          teacher: "ช่วยแยก Problem ออกจาก Solution ถาม “มี Idea ที่ง่ายกว่านี้แต่ช่วยได้เหมือนกันไหม?”",
        },
        {
          time: "35–42",
          phase: "แลกเปลี่ยนและเปรียบเทียบ",
          activity: "แต่ละกลุ่มนำเสนอสั้น ๆ แล้วเปรียบเทียบ 2 Idea ที่แก้ปัญหาเดียวกัน",
          teacher: "ชี้ว่า Idea ที่เหมาะขึ้นกับ User และ Context",
        },
        {
          time: "42–45",
          phase: "เชื่อมสู่หน่วยถัดไป",
          activity: "สังเกตตัวอย่างนวัตกรรมที่ใช้ Sensor และบอกว่า Sensor ช่วยระบบ “รู้” อะไร",
          teacher: "ย้ำว่านวัตกรรมไม่จำเป็นต้องมี Sensor แต่หลักสูตรนี้จะศึกษาแบบที่ใช้ Sensor",
        },
        {
          time: "45–50",
          phase: "Exercise 1: Problem Hunter",
          activity: "เลือกปัญหาใกล้ตัว 1 เรื่อง เติม 3 ช่องแรก Problem – User – Need ในคาบ แล้วเก็บไว้ใน Innovation Journal",
          teacher: "ช่อง Idea และ Better ให้เติมเป็นการบ้านหรือเติมต่อในหน่วยที่ 11",
          note: "5 ช่องใน 5 นาทีแน่นเกินไป จึงให้กรอก 3 ช่องแรกในคาบ — และหน่วยที่ 11 จะกลับมาใช้ Problem Hunter นี้ต่อ แบบเดียวกับ Idea Card ของ ป.3",
        },
      ],
    },
  ],
  guides: [
    {
      id: "framework",
      title: "Problem → User → Need → Idea → Better",
      blocks: [
        {
          heading: "กรอบคิดของหน่วย",
          flow: [
            { label: "Problem" },
            { label: "User" },
            { label: "Need" },
            { label: "Idea" },
            { label: "Better" },
          ],
        },
        {
          icon: "🗑️",
          heading: "ตัวอย่าง: ถังขยะ",
          text: "ฝาเปิดมือ → แบบเหยียบ → แบบอัตโนมัติ",
          compare: "แต่ละขั้นทำให้ Better อย่างไร? ซับซ้อนขึ้นแปลว่าดีกว่าทุกครั้งไหม?",
        },
      ],
      tip: "ใช้คำถามช่วยคิด “ใครได้ประโยชน์?” “ดีขึ้นตรงไหน?” “ถ้าไม่มี Idea นี้ ปัญหายังอยู่ไหม?”",
    },
    {
      id: "problem-hunter",
      title: "Exercise 1 — Problem Hunter (เก็บใน Innovation Journal)",
      blocks: [
        { icon: "1️⃣", heading: "ในคาบ", text: "Problem — ปัญหาคืออะไร · User — ใครเจอ · Need — อยากให้ดีขึ้นอย่างไร" },
        { icon: "2️⃣", heading: "การบ้าน / หน่วยที่ 11", text: "Idea — แนวคิดที่อาจช่วย · Better — ใช้แล้วอะไรดีขึ้น" },
      ],
    },
  ],
  assessment: [
    {
      topic: "ความเข้าใจ Innovation",
      method: "ถามตอบระหว่างวิเคราะห์ตัวอย่าง",
      criteria: "อธิบายได้ว่า Innovation ต้องตอบ Problem/Need และสร้างคุณค่า โดยไม่จำเป็นต้องใหม่ทั้งหมด",
    },
    {
      topic: "Problem–User–Need (ว 4.2 ป.4/1)",
      method: "Innovation Detective และ Exercise 1",
      criteria: "ระบุ Problem, User และ Need ได้สัมพันธ์กัน และไม่สับสนระหว่างปัญหากับวิธีแก้",
    },
    {
      topic: "เสนอ Idea (ท 3.1 ป.4/3)",
      method: "การ์ดกิจกรรมและการนำเสนอ",
      criteria: "เสนอ Idea ที่สัมพันธ์กับ Need อธิบายว่าอะไร Better และให้เหตุผลว่าเหมาะกับ User/Context",
    },
    { topic: "การมีส่วนร่วม", method: "สังเกตพฤติกรรม", criteria: "รับฟังแนวคิดผู้อื่นและสื่อสารอย่างเหมาะสม" },
  ],
  takeaways: [
    "Innovation คือแนวคิด วิธีการ หรือสิ่งที่ช่วยแก้ Problem/Need และสร้างคุณค่าให้ User — การปรับของเดิมให้ดีขึ้นก็นับ",
    "กรอบคิด Problem → User → Need → Idea → Better · Idea ที่เหมาะไม่จำเป็นต้องซับซ้อนที่สุด",
    "Sensor เป็นหนึ่งในเครื่องมือที่ช่วยให้นวัตกรรมรับข้อมูลจากรอบตัว แต่ไม่ใช่ทุกนวัตกรรมต้องมี",
  ],
  bridge: {
    nextLabel: "หน่วยที่ 2: Sensor",
    question: "“ถ้านวัตกรรมต้อง ‘รู้’ ว่ามืด มีคนมา หรือร้อนเกินไป ระบบจะรู้ได้อย่างไร?”",
    teaser: "“คาบหน้าเราจะเลือก Sensor จากสิ่งที่ระบบต้องการรู้”",
  },
};

const unit2: LessonPlan = {
  gradeSlug: "p4",
  week: 2,
  unitLabel: "หน่วยที่ 2",
  title: "Sensor",
  subtitle: "เลือก Sensor จากสิ่งที่ระบบต้องการรู้",
  minutesPerPeriod: 50,
  summary: [
    "เด็กรู้จัก Sensor มาแล้วตั้งแต่ ป.3 ว่าเป็น Input ที่ตรวจจับหรือวัดสิ่งรอบตัวโดยไม่ได้คิดเอง หน่วยนี้จึงเริ่มด้วย Quick Recall สั้น ๆ แล้วใช้เวลาส่วนใหญ่กับทักษะใหม่ คือ “เลือก Sensor จากสิ่งที่ระบบต้องการรู้”",
    "กรอบคิดคือ System Need → What to Sense → Sensor → Application เริ่มจากคำถาม “ระบบต้องการรู้อะไร?” ไม่ใช่ “จะใช้ Sensor อะไร?” และเห็นว่า Sensor เดียวกันใช้ได้หลายงาน ส่วนปัญหาเดียวกันก็อาจเลือก Sensor ต่างกันได้",
  ],
  objectives: [
    "อธิบายได้ว่า Sensor เป็น Input ที่ตรวจจับหรือวัดสิ่งรอบตัวแล้วเปลี่ยนเป็นข้อมูลให้ระบบ โดยไม่ได้คิดเอง",
    "จำแนก Sensor ตามสิ่งที่ตรวจจับได้ เช่น แสง ระยะ วัตถุ เสียง อุณหภูมิ และยกตัวอย่างการใช้งานได้",
    "วิเคราะห์สถานการณ์ด้วยกรอบ System Need → What to Sense → Sensor → Application แล้วเลือก Sensor พร้อมเหตุผลได้",
    "อธิบายได้ว่า Sensor เดียวกันใช้ได้หลายงาน และปัญหาเดียวกันอาจใช้ Sensor ต่างกันได้",
    "มีส่วนร่วมในการสังเกตและแลกเปลี่ยน และใช้อุปกรณ์อย่างระมัดระวัง",
  ],
  standards: [
    { ...logicP4, core: "วิเคราะห์ว่าระบบต้องการข้อมูลอะไร แล้วเลือก Sensor ให้เหมาะกับสถานการณ์พร้อมเหตุผล" },
    { ...questionP4, core: "ตั้งคำถามและอธิบายเหตุผลในการเลือก Sensor จากสิ่งที่ระบบต้องการตรวจจับ" },
  ],
  integrations: [
    { subject: "ภาษาไทย", detail: "ท 3.1 ป.4/6 มารยาทในการฟัง การดู และการพูด" },
    { subject: "วิทยาศาสตร์", detail: "การสังเกตสิ่งแวดล้อมและการเปลี่ยนแปลงของแสง ระยะ เสียง และอุณหภูมิ" },
  ],
  learning: {
    knowledge:
      "Sensor เป็น Input, Environment → Detect/Measure → Data/Signal, Sensor แต่ละชนิดรับข้อมูลได้เฉพาะบางประเภท, การนำ Sensor ไปใช้ใน Smart Home, Safety, Agriculture และ Robot และหลักเลือก Sensor ด้วย System Need → What to Sense → Sensor → Application",
    skills: "การจำแนก Sensor ตามสิ่งที่ตรวจจับ การวิเคราะห์ว่าสถานการณ์ต้องการข้อมูลอะไร การเลือกและเปรียบเทียบทางเลือก และการอธิบายเหตุผล",
    attitude: "สนใจทดลอง รับฟังความคิดเห็น และไม่สรุปผลก่อนสังเกตข้อมูล",
  },
  materials: [
    "Arduino/Controller และ Sensor ตัวอย่าง 3–5 ชนิด เช่น Light, Ultrasonic/Distance, IR, Sound, Temperature",
    "Sensor Card · Application Card · Situation Card · แผนภาพ System Need → What to Sense → Sensor → Application",
    "Program อ่านค่า Sensor ที่เตรียมไว้ล่วงหน้า (ไม่ต่อวงจรในคาบ)",
    "ใบงาน Exercise 1: Sensor Match",
  ],
  sessions: [
    {
      number: 1,
      title: "Sensor รับข้อมูลอะไร?",
      minutes: 50,
      goal: "นักเรียนทบทวนบทบาท Sensor อย่างรวดเร็ว แล้วเลือก Sensor ให้เหมาะกับ Application ด้วยกรอบ System Need → What to Sense → Sensor → Application",
      steps: [
        {
          time: "0–8",
          phase: "Quick Recall จาก ป.3",
          activity: "ดูการ์ด Sensor Gallery จาก ป.3 แล้วตอบเร็ว ๆ ว่า Sensor คือ Input แบบไหน ตรวจจับอะไร และ “คิดเองไหม?” พร้อมเรียง Environment → Detect/Measure → Data/Signal",
          teacher: "ทบทวนเร็ว ไม่สอนซ้ำจากศูนย์ ย้ำว่า Sensor ไม่ได้คิด ตัดสินใจ หรือเป็น AI เอง",
          note: "เนื้อหาซ้อนกับ ป.3 หน่วยที่ 3 จึงย่อช่วงทบทวนเหลือ 8 นาที แล้วให้เวลากิจกรรมเลือก Sensor มากขึ้น",
        },
        {
          time: "8–13",
          phase: "ประเภท Sensor และการใช้งาน",
          activity: "จำแนก Sensor ตามสิ่งที่ตรวจจับ (แสง ระยะ วัตถุ เสียง อุณหภูมิ) และจับคู่กับ Application",
          teacher: "เน้น “มันตรวจจับอะไร” ไม่ใช่จำชื่อรุ่น — Sound Sensor เป็นตัวใหม่ที่ ป.3 ยังไม่เคยเรียน",
        },
        {
          time: "13–18",
          phase: "หลักการเลือก Sensor",
          activity: "ครูพาคิดตัวอย่าง “อยากให้ไฟทำงานเมื่อมืด” → ต้องรู้ระดับแสง → Light Sensor → Automatic Light",
          teacher: "ย้ำ “อย่าเริ่มจากจะใช้ Sensor อะไร ให้เริ่มจากระบบต้องการรู้อะไร”",
        },
        {
          time: "18–34",
          phase: "Activity: Which Sensor Should We Use?",
          activity: "ทำงานกลุ่มกับ Situation Card เติม 4 ช่อง และเขียนเหตุผลในการเลือก Sensor",
          teacher: "ถามเป็นลำดับ “ระบบต้องการรู้อะไร?” → “ต้องตรวจจับอะไร?” → “Sensor ตัวไหนทำได้?”",
        },
        {
          time: "34–42",
          phase: "Activity: Same Sensor, Different Jobs",
          activity: "รับ Sensor Card 1 ชนิด แล้วคิด Application อย่างน้อย 2 แบบ เปรียบเทียบกับกลุ่มอื่น",
          teacher: "ปิดท้ายด้วยการสาธิตว่าเมื่อ Environment เปลี่ยน Reading อาจเปลี่ยน เพื่อปูหน่วยที่ 3",
        },
        {
          time: "42–45",
          phase: "สรุปและเชื่อมต่อ",
          activity: "สรุป System Need → What to Sense → Sensor → Data → Application",
          teacher: "ปูหน่วยที่ 3 ว่า Sensor ตรวจจับอย่างไร และ Reading เปลี่ยนอย่างไร",
        },
        {
          time: "45–50",
          phase: "Exercise 1: Sensor Match",
          activity: "ทำแบบฝึกเดี่ยว 4–5 สถานการณ์ เติม 4 ช่องและเหตุผลสั้น ๆ",
          teacher: "ตรวจว่าเลือกจากข้อมูลที่ระบบต้องการ ไม่ใช่จำชื่อ Project",
        },
      ],
    },
  ],
  guides: [
    {
      id: "choose-sensor",
      title: "เลือก Sensor เริ่มจากสิ่งที่ระบบต้องการรู้",
      blocks: [
        {
          heading: "กรอบคิด",
          flow: [
            { label: "System Need", sub: "ระบบต้องการรู้อะไร" },
            { label: "What to Sense", sub: "ต้องตรวจจับอะไร" },
            { label: "Sensor" },
            { label: "Application" },
          ],
        },
        {
          icon: "💡",
          heading: "ตัวอย่าง",
          text: "อยากให้ไฟทำงานเมื่อมืด → ต้องรู้ระดับแสง → Light Sensor → Automatic Light",
        },
        {
          icon: "🔁",
          heading: "Same Sensor, Different Jobs",
          text: "Ultrasonic ใช้ได้ทั้งประตูอัตโนมัติ เตือนถอยรถ และถังขยะเปิดเอง",
        },
      ],
      tip: "ตรวจก่อนสอนว่าชุดอุปกรณ์ของโรงเรียนมี Sound Sensor หรือไม่ ถ้าไม่มีให้ใช้ภาพแทน",
    },
  ],
  assessment: [
    {
      topic: "บทบาทและประเภท Sensor",
      method: "ถามตอบและการจัดกลุ่ม Sensor",
      criteria: "อธิบายได้ว่า Sensor ตรวจจับ/วัดแล้วส่งข้อมูลให้ระบบ และจำแนกตามสิ่งที่ตรวจจับได้",
    },
    {
      topic: "เลือก Sensor (ว 4.2 ป.4/1)",
      method: "Which Sensor Should We Use? และ Exercise 1",
      criteria: "เลือก Sensor ให้สัมพันธ์กับสิ่งที่ระบบต้องการรู้ และบอกเหตุผลได้",
    },
    {
      topic: "เปรียบเทียบการเลือก (ท 3.1 ป.4/4)",
      method: "Same Sensor, Different Jobs",
      criteria: "อธิบายได้ว่า Sensor เดียวกันใช้หลายงานได้ และเปรียบเทียบทางเลือกอย่างมีเหตุผล",
    },
    { topic: "การมีส่วนร่วมและความปลอดภัย", method: "สังเกตพฤติกรรม", criteria: "ร่วมกิจกรรมและปฏิบัติตามข้อตกลงด้านความปลอดภัย" },
  ],
  takeaways: [
    "Sensor เป็น Input ที่ตรวจจับหรือวัดสิ่งรอบตัว แล้วเปลี่ยนเป็นข้อมูลให้ระบบ — ไม่ได้คิดหรือเป็น AI เอง",
    "การเลือก Sensor เริ่มจากคำถาม “ระบบต้องการรู้อะไร?” ตามกรอบ System Need → What to Sense → Sensor → Application",
    "Sensor เดียวกันใช้ได้หลายงาน และปัญหาเดียวกันอาจใช้ Sensor ต่างกันได้",
  ],
  bridge: {
    nextLabel: "หน่วยที่ 3: How Sensors Sense",
    question: "“ตัวเลขที่ Sensor ส่งมาเกิดขึ้นได้อย่างไร และแต่ละค่าบอกอะไรเรา?”",
    teaser: "“คาบหน้าเราจะอ่าน Sensor Reading จริง และดูว่าเบื้องหลัง Sensor ตรวจจับอย่างไร”",
  },
};

const unit3: LessonPlan = {
  gradeSlug: "p4",
  week: 3,
  unitLabel: "หน่วยที่ 3",
  title: "How Sensors Sense",
  subtitle: "Sensor Reading มาจากไหน",
  minutesPerPeriod: 50,
  summary: [
    "หน่วยนี้ต่อยอดจาก ป.3 ที่รู้ว่า Analog คือค่าหลายระดับ และ Digital คือสถานะชัดเจน ให้ลึกขึ้นด้วยการอ่าน Sensor Reading จริง จำแนกจากรูปแบบข้อมูล (ไม่ใช่จากชื่อ Sensor) และเข้าใจว่า “Reading ต้องมีบริบท” — ค่าเดียวกันยังบอกความหมายไม่ได้ จนกว่าจะรู้ว่าเป็น Sensor อะไรและอยู่ใน Environment แบบไหน",
    "จากนั้นเรียนหลักการตรวจจับของ Light, Ultrasonic และ IR Sensor แบบง่าย เด็กเคยใช้ Distance Sensor (ป.3 หน่วยที่ 13) และ IR Sensor (ป.3 หน่วยที่ 15) มาแล้ว จึงให้เล่าสิ่งที่เคยทำก่อน แล้วยกระดับว่าเบื้องหลังทำงานอย่างไร และทิศทางค่ามาก/น้อยขึ้นกับ Module ที่ใช้",
  ],
  objectives: [
    "เปรียบเทียบข้อมูลแบบ Analog และ Digital จากลักษณะค่า/สถานะที่อ่านได้จริง",
    "อธิบายหลักการตรวจจับแบบง่ายของ Light, Ultrasonic/Distance และ IR Sensor ได้",
    "อ่านและจำแนก Sensor Reading และอธิบายความหมายโดยอ้างอิง Sensor และ Environment ได้",
    "คาดการณ์และทดลองการเปลี่ยนแปลงของ Reading เมื่อเปลี่ยน Environment ทีละปัจจัย",
    "อธิบายความสัมพันธ์ Environment → วิธีที่ Sensor ตรวจจับ → Sensor Reading จากผลที่สังเกตได้",
    "ทดลองอย่างเป็นระบบ และยอมปรับคำอธิบายเมื่อผลจริงต่างจากที่คาด",
  ],
  standards: [
    { ...logicP4, core: "คาดการณ์ว่า Reading จะเปลี่ยนอย่างไรเมื่อ Environment เปลี่ยน และเปรียบเทียบผลที่คาดกับผลจริง" },
    { ...questionP4, core: "ตั้งคำถามและอธิบายว่า Sensor Reading หรือผลการทดลองบอกอะไร โดยอ้างอิงสิ่งที่สังเกตได้" },
  ],
  integrations: [
    { subject: "วิทยาศาสตร์", detail: "การสังเกตการเปลี่ยนแปลงของแสง การสะท้อน และระยะทาง" },
    { subject: "คณิตศาสตร์", detail: "การอ่านและเปรียบเทียบค่าตัวเลขหลายระดับ" },
  ],
  learning: {
    knowledge:
      "Analog และ Digital จากค่า/สถานะจริง, ความหมายของ Sensor Reading, หลักการตรวจจับของ Light (แสง), Ultrasonic (เสียงสะท้อน) และ IR (อินฟราเรด) และความสัมพันธ์ Environment → Sensor → Reading",
    skills: "การอ่านและจำแนก Reading การคาดการณ์ การทดลองโดยเปลี่ยนตัวแปรทีละอย่าง และการอธิบายจากหลักฐาน",
    attitude: "ทดลองอย่างเป็นระบบ ซื่อสัตย์ต่อข้อมูล และรับฟังผลที่ต่างจากที่คาด",
  },
  vocabulary: [
    { term: "Sensor Reading", meaning: "ค่าหรือสถานะที่ระบบอ่านได้จาก Sensor" },
    { term: "Analog Reading", meaning: "ค่าที่เปลี่ยนได้หลายระดับ" },
    { term: "Digital Reading", meaning: "สถานะที่ระบบกำหนด เช่น 0/1" },
    { term: "Echo", meaning: "เสียงสะท้อนที่ Ultrasonic ใช้วัดระยะ" },
  ],
  materials: [
    "Arduino/Controller พร้อม Light, Ultrasonic/Distance และ IR Sensor — ถ้าชุดไม่ครบทุกกลุ่ม ให้จัดเป็นสถานีหมุนเวียน",
    "Mixly หรือโปรแกรมอ่านค่า Sensor · Slide หลักการ · Analog/Digital Card · Sensor Reading Card",
    "Sensor Experiment Sheet และ Reading ตัวอย่างสำรองกรณี Hardware มีปัญหา",
    "ใบงาน Exercise 1: Read the Sensor และ Exercise 2: How Does It Sense?",
  ],
  sessions: [
    {
      number: 1,
      title: "Analog & Digital — อ่านข้อมูลจาก Sensor",
      minutes: 50,
      goal: "นักเรียนอ่าน Sensor Reading จริง จำแนกรูปแบบข้อมูล และอธิบายความหมายเบื้องต้นจากบริบทของ Sensor/Environment",
      steps: [
        {
          time: "0–5",
          phase: "ทบทวนจาก ป.3",
          activity: "ดูตัวอย่าง “มี/ไม่มี” และ “สว่างหลายระดับ” แล้วทบทวนว่าแบบใดเป็น Digital และ Analog",
          teacher: "วันนี้จะอ่าน Reading จริงให้ลึกกว่า ป.3",
        },
        {
          time: "5–12",
          phase: "Sensor Reading คืออะไร",
          activity: "ดู Reading เช่น 820, 650, 410 และ 0/1 แล้วบอกว่าเป็น “ค่า” หรือ “สถานะ”",
          teacher: "อธิบายว่า Reading คือค่าหรือสถานะที่ Program อ่านได้จาก Sensor",
        },
        {
          time: "12–20",
          phase: "Analog vs Digital แบบ ป.4",
          activity: "เปรียบเทียบข้อมูลสองแบบ และสรุปว่าดูจากรูปแบบ Reading ไม่ใช่ชื่อ Sensor",
          teacher: "ย้ำว่าบาง Module มีทั้ง Analog Output และ Digital Output",
        },
        {
          time: "20–27",
          phase: "Reading ต้องมีบริบท",
          activity: "ดู Reading “620” หรือ “1” แล้วอภิปรายว่ายังสรุปไม่ได้จนกว่าจะรู้ Sensor และ Environment",
          teacher: "สอนหลักคิด Sensor/Environment → Reading → Meaning",
        },
        {
          time: "27–33",
          phase: "ตัวอย่างก่อนทำ Activity",
          activity: "ทำตามครู 2–3 ตัวอย่าง: ดู Sensor/Environment → ดูรูปแบบ → จำแนก → อธิบายความหมาย",
          teacher: "Model การคิด เช่น Light Sensor ให้ 220, 480, 760 = Analog",
        },
        {
          time: "33–45",
          phase: "Activity: Read the Sensor",
          activity: "ทำงานกับ Reading Card หรือค่าจริง 2–3 ตัวอย่าง จำแนกและอธิบายว่า Reading บอกอะไร",
          teacher: "ถาม “ดูจากอะไรว่าเป็น Analog หรือ Digital?” “ต้องรู้อะไรเพิ่มก่อนแปลความหมาย?”",
        },
        {
          time: "45–50",
          phase: "Exercise 1: Read the Sensor",
          activity: "อ่าน Reading 2–3 ชุดพร้อมบริบท ระบุ Analog/Digital และเขียนว่า “ข้อมูลนี้กำลังบอกอะไร”",
          teacher: "ตั้งคำถามต่อ “Reading เหล่านี้เกิดจากการตรวจจับอย่างไร?” เพื่อเข้าคาบ 2",
        },
      ],
    },
    {
      number: 2,
      title: "How Sensors Sense — ค่าพวกนี้มาจากไหน?",
      minutes: 50,
      goal: "นักเรียนอธิบายหลักการตรวจจับแบบง่ายของ Light, Ultrasonic และ IR คาดการณ์และทดลองการเปลี่ยนของ Reading และเชื่อม Environment กับ Reading ได้",
      steps: [
        {
          time: "0–5",
          phase: "เล่าจากที่เคยทำใน ป.3",
          activity: "เล่าว่าตอน ป.3 เคยใช้ Distance Sensor (หน่วยที่ 13) และ IR Sensor (หน่วยที่ 15) ทำอะไร และสังเกตเห็นอะไร",
          teacher: "ให้เด็กเล่าก่อนอธิบายหลักการ เพื่อให้เห็นว่าบทนี้ยกระดับจากสิ่งที่เคยใช้",
        },
        {
          time: "5–10",
          phase: "Concept: Sensor ตรวจจับ/วัด",
          activity: "สังเกต Flow Environment → Sensor ตรวจจับ/วัด → Sensor Reading และเทียบว่า Sensor ต่างชนิดใช้วิธีต่างกัน",
          teacher: "อธิบายระดับภาพรวม ไม่ลงวงจรภายใน",
        },
        {
          time: "10–20",
          phase: "ตัวอย่างหลักการ Sensor",
          activity: "Light — ตอบสนองต่อปริมาณแสง · Ultrasonic — ส่งคลื่นเสียงความถี่สูงแล้วรับเสียงสะท้อน เวลาที่กลับมาสัมพันธ์กับระยะ · IR — ส่ง/รับแสงอินฟราเรดเพื่อตรวจจับวัตถุ",
          teacher: "ย้ำว่าทิศทางค่ามาก/น้อยขึ้นกับวงจร/Module และ IR แต่ละแบบอาจมี Output ต่างกัน",
        },
        {
          time: "20–35",
          phase: "Activity: Predict → Test → Observe",
          activity: "เลือก Sensor 1–2 แบบ เปลี่ยน Environment ทีละปัจจัย Predict ก่อนทดลอง และจดผล 2–3 ครั้ง",
          teacher: "ถาม “อะไรเปลี่ยน?” → “Sensor ตรวจจับอะไร?” → “Reading เปลี่ยนอย่างไร?”",
          note: "15 นาทีพอดีถ้ามีชุด Sensor ครบทุกกลุ่ม ถ้าไม่ครบให้หมุนสถานี Light / Ultrasonic / IR",
        },
        {
          time: "35–42",
          phase: "Sensor Reading บอกอะไรเรา?",
          activity: "เลือกผลทดลอง 1 ตัวอย่างแล้วอธิบายว่า Reading บอกอะไรเกี่ยวกับ Environment",
          teacher: "ให้ตีความจากข้อมูลจริงของอุปกรณ์ที่ใช้",
        },
        {
          time: "42–45",
          phase: "สรุปและเชื่อมหน่วยที่ 4",
          activity: "สรุป Environment → Sensor ตรวจจับ/วัด → Sensor Reading",
          teacher: "ปูหน่วยที่ 4: เก็บหลายค่า ทำตาราง/กราฟ และหา Pattern",
        },
        {
          time: "45–50",
          phase: "Exercise 2: How Does It Sense?",
          activity: "ดูสถานการณ์ 2–3 ข้อ แล้วเติม “Environment เปลี่ยนอะไร → Sensor ตรวจจับอย่างไร → Reading บอกอะไร”",
          teacher: "ตรวจเหตุผลจากสิ่งที่เรียนและสังเกต",
        },
      ],
    },
  ],
  guides: [
    {
      id: "how-they-sense",
      title: "หลักการตรวจจับแบบง่าย",
      blocks: [
        { icon: "💡", heading: "Light Sensor", text: "ส่วนตรวจจับตอบสนองต่อปริมาณแสง ทำให้ Reading เปลี่ยน" },
        { icon: "🔊", heading: "Ultrasonic", text: "ส่งคลื่นเสียงความถี่สูงแล้วรับเสียงสะท้อน — เวลาที่เสียงกลับมาสัมพันธ์กับระยะทาง" },
        { icon: "🔴", heading: "IR Sensor", text: "ส่ง/รับแสงอินฟราเรด ใช้การเปลี่ยนของแสงที่รับหรือสะท้อนเพื่อตรวจจับวัตถุ" },
      ],
      tip: "ไม่ลงสูตรคำนวณหรือวงจรภายใน และไม่สอนกฎตายตัวว่า “มืดต้องค่ามาก” เพราะขึ้นกับ Module",
    },
    {
      id: "reading-context",
      title: "Reading ต้องมีบริบท",
      blocks: [
        {
          heading: "ลำดับการตีความ",
          flow: [{ label: "Sensor / Environment" }, { label: "Reading" }, { label: "Meaning" }],
        },
        {
          icon: "❓",
          heading: "“620” แปลว่าอะไร?",
          text: "ยังตอบไม่ได้ จนกว่าจะรู้ว่าเป็น Sensor อะไร ตรวจจับอะไร และอยู่ในสถานการณ์แบบไหน",
        },
      ],
    },
  ],
  assessment: [
    {
      topic: "Analog/Digital และหลักการ Sensor",
      method: "ถามตอบ Read the Sensor และ Exercise 1–2",
      criteria: "เปรียบเทียบ Analog/Digital จากข้อมูลจริง และอธิบายหลักการตรวจจับของ Sensor ตัวอย่างได้",
    },
    {
      topic: "อ่านและจำแนก Reading (ท 3.1 ป.4/4)",
      method: "Sensor Reading Card + Exercise 1",
      criteria: "จำแนกจากรูปแบบข้อมูล และอธิบายความหมายโดยอ้างอิงบริบทได้",
    },
    {
      topic: "คาดการณ์และทดลอง (ว 4.2 ป.4/1)",
      method: "Predict → Test → Observe + Exercise 2",
      criteria: "คาดการณ์ก่อนทดลอง เปลี่ยนทีละปัจจัย และอธิบาย Environment → Reading จากผลจริง",
    },
    { topic: "การทดลองและความปลอดภัย", method: "สังเกตพฤติกรรม", criteria: "ทำงานเป็นระบบ ยอมรับผลจริง และปฏิบัติตามข้อตกลง" },
  ],
  takeaways: [
    "Analog คือค่าหลายระดับ Digital คือสถานะชัดเจน — จำแนกจากรูปแบบ Reading และตีความโดยดู Sensor กับ Environment ประกอบ",
    "Sensor แต่ละชนิดตรวจจับด้วยหลักการต่างกัน เช่น แสง เสียงสะท้อน หรืออินฟราเรด",
    "เมื่อ Environment เปลี่ยน Reading อาจเปลี่ยนตาม — ทดลองที่ดีต้องเปลี่ยนทีละปัจจัย",
  ],
  bridge: {
    nextLabel: "หน่วยที่ 4: Sensor + Data",
    question: "“ถ้า Sensor ส่งค่ามาเรื่อย ๆ เราจะเก็บ ดู และใช้ค่าพวกนี้อย่างไร?”",
    teaser: "“คาบหน้าเราจะเก็บข้อมูลหลายค่า ทำตาราง/กราฟ หา Pattern และหาค่าจุดตัด”",
  },
};

const unit4: LessonPlan = {
  gradeSlug: "p4",
  week: 4,
  unitLabel: "หน่วยที่ 4",
  title: "Sensor + Data",
  subtitle: "จาก Reading สู่ Data, Pattern และ Threshold",
  minutesPerPeriod: 50,
  summary: [
    "Sensor Reading หนึ่งค่าคือข้อมูลหนึ่งจุด เมื่อเก็บหลายค่าภายใต้สถานการณ์ที่ชัดเจนจะเกิดเป็น Data Set ซึ่งช่วยให้เห็นสิ่งที่ค่าเดียวบอกไม่ได้ ผู้เรียนจะใช้ Serial Monitor ดูค่าที่ Arduino ส่งมา และใช้ Serial Plotter ดูเป็นกราฟ แล้วเชื่อม Data Set → Table → Graph → Pattern",
    "เมื่อเห็น Pattern แล้ว จึงเรียนค่าจุดตัด (Threshold) แบบค่อยเป็นค่อยไป 3 ระดับ: แยกข้อมูลเป็น 2 กลุ่ม → เลือกค่าจุดตัดระหว่างกลุ่ม → ทดลองใช้กับ Reading ใหม่ โดยยังไม่เขียน if/else ซึ่งจะเรียนในหน่วยที่ 5",
  ],
  objectives: [
    "อธิบายได้ว่า Reading หนึ่งค่าคือข้อมูลหนึ่งจุด หลายค่ารวมเป็น Data Set และบอกหน้าที่ของ Serial Monitor/Plotter ได้",
    "อธิบายได้ว่า Threshold คือค่าจุดตัดที่ช่วยแบ่งข้อมูลออกเป็นกลุ่มหรือสถานะ",
    "อ่านและบันทึก Sensor Reading จากหลายสถานการณ์ลงตารางอย่างเป็นระบบ",
    "นำ Data Set ไปแสดงหรืออ่านจากกราฟ เปรียบเทียบ และอธิบาย Pattern ได้",
    "เลือกและเปรียบเทียบ Threshold จาก Pattern พร้อมเหตุผล และทดลองใช้กับ Reading ใหม่ได้",
    "บันทึกข้อมูลตามผลจริง และยอมรับว่าข้อมูลแต่ละชุดอาจต่างกัน",
  ],
  standards: [
    {
      code: "ว 4.2 ป.4/4",
      indicator: "รวบรวม ประเมิน นำเสนอข้อมูลและสารสนเทศ โดยใช้ซอฟต์แวร์ที่หลากหลาย เพื่อแก้ปัญหาในชีวิตประจำวัน",
      core: "เก็บ Reading ใช้ Serial Monitor/Plotter จัดเป็นตาราง/กราฟ เปรียบเทียบ Pattern และเลือก Threshold จาก Data Set",
    },
    { ...questionP4, core: "อธิบาย Pattern ตั้งคำถามจากข้อมูล และให้เหตุผลในการเลือก Threshold" },
  ],
  integrations: [
    {
      subject: "คณิตศาสตร์",
      detail: "การอ่านและนำเสนอข้อมูลด้วยตารางและแผนภูมิแท่ง (สาระสถิติ ป.4) · การเปรียบเทียบจำนวน",
    },
    { subject: "วิทยาศาสตร์", detail: "การเก็บข้อมูลจากการทดลองอย่างเป็นระบบ" },
  ],
  learning: {
    knowledge:
      "Sensor Reading และ Data Set, หน้าที่ของ Serial Monitor และ Serial Plotter, ตาราง/กราฟช่วยเปรียบเทียบและเห็น Pattern และ Threshold คือค่าจุดตัดที่ควรเลือกและทดสอบจากข้อมูลจริง",
    skills: "การเก็บและบันทึก Data Set การจัดตาราง การอ่าน/สร้างกราฟ การหา Pattern และการเลือก–ทดสอบ Threshold",
    attitude: "บันทึกตามผลจริง ไม่เลือกตัวเลขให้ตรงกับคำตอบที่คาด และรับฟังข้อมูลจากเพื่อน",
  },
  vocabulary: [
    { term: "Data Point", meaning: "ข้อมูลหนึ่งจุด เช่น Reading หนึ่งค่า" },
    { term: "Data Set", meaning: "ชุดข้อมูลหลายจุดที่เก็บอย่างมีเงื่อนไข" },
    { term: "Serial Monitor", meaning: "หน้าต่างดูค่าที่ Arduino ส่งมายังคอมพิวเตอร์" },
    { term: "Serial Plotter", meaning: "เครื่องมือแสดงค่าที่ส่งมาเป็นกราฟ" },
    { term: "Pattern", meaning: "แนวโน้มที่เห็นจากหลายจุด" },
    { term: "Threshold", meaning: "ค่าจุดตัดที่ใช้แบ่งข้อมูลเป็น 2 ฝั่ง" },
  ],
  materials: [
    "Arduino/Controller พร้อม Light Sensor เป็นตัวอย่างหลัก",
    "Mixly หรือโปรแกรมอ่านค่า + Serial Monitor/Plotter — ตรวจก่อนว่า Mixly มี Serial Plotter หรือไม่ ถ้าไม่มีให้ใช้ Serial Plotter ของ Arduino IDE หรือวาดกราฟจากตารางแทน",
    "ตารางบันทึกข้อมูล · Graph Sheet · Data Set ตัวอย่างและ Data Set สำรอง",
    "ใบงาน Exercise 1: Sensor Data Sheet และ Exercise 2: Threshold Check",
  ],
  sessions: [
    {
      number: 1,
      title: "จาก Sensor Reading สู่ Data",
      minutes: 50,
      goal: "นักเรียนอธิบายได้ว่า Reading หลายค่ารวมเป็น Data Set รู้หน้าที่ของ Serial Monitor/Plotter เก็บข้อมูลอย่างเป็นระบบ และใช้ตาราง/กราฟอธิบาย Pattern ได้",
      steps: [
        { time: "0–5", phase: "เชื่อมจากหน่วยที่ 3", activity: "ดู Reading จากหน่วยที่ 3 แล้วตอบว่าค่าเดียวบอกอะไรได้ และอะไรยังบอกไม่ได้", teacher: "ถ้าอยากเห็นแนวโน้ม เราต้องดูหลายค่าร่วมกัน" },
        { time: "5–10", phase: "จาก Reading สู่ Data Set", activity: "จัด Reading หลายค่าเป็น Situation → Reading", teacher: "สรุป 1 Reading = 1 Data Point · หลาย Data Point = Data Set ต้องระบุสถานการณ์เสมอ" },
        { time: "10–16", phase: "Serial Monitor", activity: "สังเกต Arduino ส่ง Reading มายังคอมพิวเตอร์ แล้วดูตัวเลขใน Serial Monitor", teacher: "เกริ่นปัญหาก่อนให้ชื่อเครื่องมือ" },
        { time: "16–22", phase: "Serial Plotter", activity: "ดูค่าชุดเดียวกันใน Serial Plotter แล้วเปรียบเทียบว่าอะไรอ่านง่ายกว่า", teacher: "Monitor เหมาะกับอ่านตัวเลข Plotter เหมาะกับดูการเปลี่ยนแปลง" },
        { time: "22–28", phase: "Data → Table → Graph → Pattern", activity: "ดูครูจัด Data Set 4–5 ค่าลงตาราง Plot จุดบนกราฟ แล้วอ่านแนวโน้มร่วมกัน", teacher: "Pattern คือแนวโน้มจากหลายจุด ไม่ใช่การเดาจากค่าเดียว" },
        {
          time: "28–40",
          phase: "Activity: Light Data Lab",
          activity: "ทดลอง Light Sensor 4–5 สถานการณ์ เปลี่ยนความสว่างทีละระดับ และบันทึก Situation + Reading",
          teacher: "ถาม “ค่าไหนใกล้กัน?” “เส้นใน Plotter เปลี่ยนตรงไหน?” ไม่กำหนดว่ามืดต้องมีค่ามากหรือน้อย",
        },
        {
          time: "40–45",
          phase: "Activity: Data to Graph",
          activity: "นำ Data Set ของกลุ่มไปทำกราฟอย่างง่าย แล้วเขียน Pattern 1 ประโยค",
          teacher: "เน้นความถูกต้องมากกว่าความสวยงาม",
          note: "คาบนี้เนื้อหาเยอะ (Monitor + Plotter + Table + Graph + Lab) — ถ้าเวลาไม่พอ ให้ย้าย Data to Graph ไปทำต้นคาบ 2 แล้วใช้ Exercise 1 ปิดคาบนี้",
        },
        { time: "45–50", phase: "Exercise 1: Sensor Data Sheet", activity: "เติมตาราง Situation → Reading จาก Data Set 3–4 ค่า Plot จุด และเขียน Pattern 1 ประโยค", teacher: "ไม่ใช้กฎสำเร็จรูป ให้สรุปจากข้อมูลจริง" },
      ],
    },
    {
      number: 2,
      title: "จาก Pattern ไปสู่ Threshold",
      minutes: 50,
      goal: "นักเรียนใช้ Pattern ผ่าน 3 ขั้น: แยกข้อมูลเป็น 2 กลุ่ม เลือก Threshold และทดลองใช้กับ Reading ใหม่พร้อมเหตุผล",
      steps: [
        { time: "0–5", phase: "ทบทวนจากคาบ 1", activity: "ดู Data Set หรือกราฟจากคาบก่อน (หรือทำ Data to Graph ที่ค้างไว้)", teacher: "วันนี้จะใช้ Pattern ช่วยแบ่งข้อมูลทีละขั้น" },
        { time: "5–12", phase: "ระดับ 1: แยกข้อมูลเป็น 2 กลุ่ม", activity: "ดูตัวอย่างที่แยกชัด เช่น มืด 210, 260, 300 และสว่าง 720, 780, 830 แล้วจัดค่าให้ถูกกลุ่ม", teacher: "เริ่มจากโจทย์ที่ต่างชัด ยังไม่ต้องเลือกจุดตัด" },
        { time: "12–18", phase: "รู้จัก Threshold", activity: "ดูช่องว่างระหว่างสองกลุ่ม แล้วเรียนว่าค่าจุดตัดเรียกว่า Threshold", teacher: "Threshold ไม่ใช่สูตร และไม่จำเป็นต้องมีคำตอบเดียว" },
        { time: "18–26", phase: "ระดับ 2: หาเส้นแบ่ง", activity: "เลือก Threshold 1 ค่าในช่วงระหว่างสองกลุ่ม และอธิบายเหตุผล", teacher: "ยอมรับหลายคำตอบถ้ายังแบ่งสองกลุ่มได้ถูก" },
        { time: "26–34", phase: "ตัวอย่างก่อนทำ Activity", activity: "ดูครูทำครบ: แยก 2 กลุ่ม → หา Gap → เลือก Threshold → ใช้กับ Reading ใหม่ 1–2 ค่า", teacher: "Model ด้วยตัวเลขง่าย เช่น เลือก 500 เพราะอยู่ระหว่างสองกลุ่ม" },
        {
          time: "34–42",
          phase: "ระดับ 3: Threshold Hunt",
          activity: "ใช้ Data Set ใหม่ เปรียบเทียบ Threshold 2–3 ค่า เลือกค่าที่เหมาะที่สุดพร้อมเหตุผล แล้วใช้กับ Reading ใหม่ 2–3 ค่า",
          teacher: "ถาม “สองกลุ่มอยู่ช่วงไหน?” → “ค่าไหนใช้ได้?” → “ค่าไหนเหมาะที่สุด?” → “Reading ใหม่อยู่ฝั่งไหน?”",
        },
        { time: "42–45", phase: "สรุปและเชื่อมหน่วยที่ 5", activity: "สรุป 3 ขั้น: แยกกลุ่ม → หา Threshold → ทดลองกับค่าใหม่", teacher: "หน่วยที่ 5 จะนำ Reading ไปเทียบ Threshold เพื่อให้ Program เลือก Output" },
        { time: "45–50", phase: "Exercise 2: Threshold Check", activity: "ทำ 3 ระดับสั้น ๆ: แยก 2 กลุ่ม → เลือก Threshold พร้อมเหตุผล → เปรียบเทียบ 2–3 ค่าและใช้กับ Reading ใหม่", teacher: "ตรวจว่าเลือกจาก Pattern ของข้อมูลจริง" },
      ],
    },
  ],
  guides: [
    {
      id: "data-flow",
      title: "จาก Reading ถึง Threshold",
      blocks: [
        {
          heading: "คาบที่ 1",
          flow: [{ label: "Reading" }, { label: "Data Set" }, { label: "Table" }, { label: "Graph" }, { label: "Pattern" }],
        },
        {
          heading: "คาบที่ 2 — Threshold 3 ระดับ",
          flow: [
            { label: "ระดับ 1", sub: "แยก 2 กลุ่ม" },
            { label: "ระดับ 2", sub: "เลือกจุดตัด" },
            { label: "ระดับ 3", sub: "ทดลองกับค่าใหม่" },
          ],
        },
        {
          icon: "🔢",
          heading: "ตัวอย่าง",
          text: "มืด 210, 260, 300 · สว่าง 720, 780, 830 → Threshold = 500 → Reading ใหม่ 650 อยู่ฝั่งสว่าง",
        },
      ],
      tip: "Threshold ต้องมาจากข้อมูลของระบบนั้น ๆ — ห้องคนละห้องหรือ Module คนละตัวอาจได้ค่าต่างกัน",
    },
  ],
  assessment: [
    {
      topic: "Data และ Threshold",
      method: "ถามตอบระหว่างสาธิต และ Exercise 2",
      criteria: "อธิบาย Data Point / Data Set หน้าที่ Monitor/Plotter และ Threshold เป็นค่าจุดตัดได้",
    },
    {
      topic: "เก็บและบันทึกข้อมูล (ว 4.2 ป.4/4)",
      method: "Light Data Lab และ Exercise 1",
      criteria: "บันทึก Situation และ Reading ได้ครบ สอดคล้องกับผลจริง",
    },
    { topic: "Pattern / Graph", method: "Graph Sheet และการอภิปราย", criteria: "แสดงกราฟอย่างง่าย และบอก Pattern ได้อย่างน้อย 1 ประเด็น" },
    {
      topic: "เลือกและทดสอบ Threshold (ท 3.1 ป.4/4)",
      method: "Threshold Hunt และ Exercise 2",
      criteria: "เปรียบเทียบและเลือก Threshold จาก Pattern พร้อมเหตุผล และใช้จัด Reading ใหม่ได้",
    },
    { topic: "ความเป็นระบบและความปลอดภัย", method: "สังเกตพฤติกรรม", criteria: "บันทึกตามผลจริง ทำงานเป็นขั้นตอน และปฏิบัติตามข้อตกลง" },
  ],
  takeaways: [
    "Reading หนึ่งค่าคือข้อมูลหนึ่งจุด หลายค่าที่เก็บอย่างเป็นระบบรวมเป็น Data Set ดูได้ด้วย Serial Monitor/Plotter",
    "ตารางและกราฟช่วยให้เห็น Pattern — Pattern ต้องสรุปจากหลายจุด ไม่ใช่เดาจากค่าเดียว",
    "Threshold คือค่าจุดตัดที่เลือกจาก Pattern ของข้อมูลจริง แล้วทดลองใช้กับ Reading ใหม่",
  ],
  bridge: {
    nextLabel: "หน่วยที่ 5: One Sensor One Action",
    question: "“ถ้าเรารู้ Threshold แล้ว จะทำให้ Program ตัดสินใจเองได้ไหมว่าควรเปิดไฟหรือไม่?”",
    teaser: "“คาบหน้าเราจะใช้ if/else เทียบ Reading กับ Threshold เพื่อสั่ง Output”",
  },
};

export const p4LessonPlans: LessonPlan[] = [unit1, unit2, unit3, unit4];
