// lib/lesson-plans/p6.ts
// แผนการสอน ป.6 สัปดาห์ที่ 1–5 (หน่วยการเรียนรู้ที่ 1–5)
// ที่มา: แผนการจัดการเรียนรู้ ป.6 (Google Docs) ปรับตามคอมเมนต์คอลัมน์ K ใน Course Plan
// ชื่อแบบฝึกใช้ระบบเดียวกันทั้งระดับชั้น: Exercise 1, 2 นับใหม่ในแต่ละหน่วย

import type { LessonPlan } from "./types";

const logicP6 = {
  code: "ว 4.2 ป.6/1",
  indicator: "ใช้เหตุผลเชิงตรรกะในการอธิบายและออกแบบวิธีการแก้ปัญหาที่พบในชีวิตประจำวัน",
};

const questionP6 = {
  code: "ท 3.1 ป.6/2",
  indicator: "ตั้งคำถามและตอบคำถามเชิงเหตุผลจากเรื่องที่ฟังและดู",
};

const teamworkP6 = {
  code: "ง 1.1 ป.6/2",
  indicator: "ใช้ทักษะการจัดการในการทำงานและมีทักษะการทำงานร่วมกัน",
};

const unit1: LessonPlan = {
  gradeSlug: "p6",
  week: 1,
  unitLabel: "หน่วยที่ 1",
  title: "Robot in Industry",
  subtitle: "หุ่นยนต์ในโรงงาน และบทบาทของคน",
  minutesPerPeriod: 50,
  summary: [
    "หุ่นยนต์อุตสาหกรรมคือเครื่องจักรที่ตั้งโปรแกรมให้ทำงานตามขั้นตอนได้อัตโนมัติ เหมาะกับงานซ้ำ งานที่ต้องการความแม่นยำหรือความเร็ว งานหนัก หรืองานเสี่ยงต่อคน ไม่จำเป็นต้องมีรูปร่างเหมือนมนุษย์ และทำงานตามโปรแกรมที่มนุษย์ออกแบบ หน่วยนี้เปิดจากของที่เด็กรู้จัก — แขนกลที่เด็กเคยเขียนโปรแกรมตอน ป.5 ก็คือ Articulated Robot ขนาดเล็ก",
    "การเลือกหุ่นยนต์เริ่มจาก “งานต้องการอะไร” ผ่านกรอบ Task → Need → Robot Options → Choose → Reason เด็กรู้จัก Articulated, SCARA, Cartesian, Delta และ Mobile Robot/AGV–AMR จากการ์ดภาพและงานตัวอย่าง โดยไม่ต้องจำชื่อทั้งหมด และเห็นว่ามนุษย์ยังสำคัญในการตัดสินใจ ออกแบบ โปรแกรม บำรุงรักษา และดูแลความปลอดภัย",
  ],
  objectives: [
    "อธิบายความหมายของหุ่นยนต์อุตสาหกรรมและเหตุผลที่โรงงานเลือกใช้หุ่นยนต์ได้",
    "ยกตัวอย่างการใช้หุ่นยนต์ในอุตสาหกรรมรถยนต์ อาหาร และอิเล็กทรอนิกส์ พร้อมบอกจุดเด่นของหุ่นยนต์แต่ละประเภทในระดับพื้นฐานได้",
    "วิเคราะห์ลักษณะงาน เปรียบเทียบตัวเลือก และเลือกประเภทหุ่นยนต์ที่เหมาะพร้อมเหตุผลได้",
    "อธิบายบทบาทของมนุษย์ที่ทำงานร่วมกับหุ่นยนต์ และเชื่อมโยงกับอาชีพที่เกี่ยวข้องได้อย่างน้อย 1 อาชีพ",
    "สนใจเรียนรู้เทคโนโลยี รับฟังเหตุผลของผู้อื่น และตระหนักถึงความปลอดภัย",
  ],
  standards: [
    { ...logicP6, core: "วิเคราะห์ลักษณะงานในโรงงาน และใช้เหตุผลเลือกประเภทหุ่นยนต์ให้เหมาะกับความต้องการของงาน" },
    { ...questionP6, core: "ตั้งและตอบคำถามจากภาพ/คลิปโรงงาน ว่าเหตุใด Robot บางแบบเหมาะกว่า และมนุษย์ยังมีบทบาทอะไร" },
  ],
  integrations: [{ subject: "การงานอาชีพ", detail: "บทบาทของคนในโรงงาน อาชีพด้านหุ่นยนต์และระบบอัตโนมัติ และทักษะที่จำเป็นในอนาคต" }],
  learning: {
    knowledge:
      "ความหมายของหุ่นยนต์อุตสาหกรรม, เหตุผลที่โรงงานใช้หุ่นยนต์, ตัวอย่างในอุตสาหกรรมรถยนต์ อาหาร อิเล็กทรอนิกส์, จุดเด่นของ Articulated / SCARA / Cartesian / Delta / Mobile Robot, ไม่มี Robot แบบเดียวที่เหมาะกับทุกงาน และจุดแข็งของ Robot เทียบกับบทบาทมนุษย์",
    skills: "การวิเคราะห์ Task → Need → Robot Options → Choose → Reason การเปรียบเทียบข้อดีกับความต้องการของงาน และการเชื่อมโยงบทบาทมนุษย์",
    attitude: "เปิดรับเหตุผลที่แตกต่าง ตระหนักถึงความปลอดภัย และเห็นความสำคัญของมนุษย์ในการรับผิดชอบต่อระบบ",
  },
  vocabulary: [
    { term: "Industrial Robot", meaning: "หุ่นยนต์อุตสาหกรรมที่ตั้งโปรแกรมได้" },
    { term: "Articulated", meaning: "แขนกลหลายข้อต่อ ยืดหยุ่นหลายทิศทาง" },
    { term: "SCARA", meaning: "แขนกลที่เหมาะงานประกอบเร็วในแนวระนาบ" },
    { term: "Delta", meaning: "หุ่นยนต์หยิบวางเร็วมาก" },
    { term: "AGV / AMR", meaning: "หุ่นยนต์เคลื่อนที่สำหรับขนส่งในโรงงาน" },
  ],
  materials: [
    "แขนกลของห้องเรียน (ที่ใช้ตอน ป.5) วางให้เห็นหน้าห้อง เป็นตัวอย่าง Articulated Robot ขนาดเล็ก",
    "ภาพหรือคลิปสั้นหุ่นยนต์ในโรงงานรถยนต์ อาหาร และอิเล็กทรอนิกส์",
    "Robot Type Cards แบบภาพ + งานตัวอย่าง · Factory Scenario Cards 3–4 แบบ",
    "ใบงาน Exercise 1: Robot in Industry Match",
  ],
  sessions: [
    {
      number: 1,
      title: "Robot in Industry",
      minutes: 50,
      goal: "นักเรียนเข้าใจบทบาทของหุ่นยนต์ในโรงงาน รู้จักประเภทหุ่นยนต์อุตสาหกรรมเบื้องต้น และใช้เหตุผลเลือกหุ่นยนต์ให้เหมาะกับงาน",
      steps: [
        {
          time: "0–5",
          phase: "เริ่มจากแขนกลที่เรารู้จัก",
          activity: "ดูแขนกลของห้องเรียนที่เคยเขียนโปรแกรมตอน ป.5 แล้วเทียบกับคลิปแขนกลในโรงงานรถยนต์",
          teacher: "ถาม “แขนกลของเรากับในโรงงานเหมือนกันตรงไหน?” แล้วบอกว่าทั้งคู่คือ Articulated Robot",
        },
        {
          time: "5–12",
          phase: "Industrial Robot คืออะไร?",
          activity: "เรียนความหมายของหุ่นยนต์อุตสาหกรรมและลักษณะงานที่โรงงานมักใช้หุ่นยนต์",
          teacher: "ย้ำว่าตั้งโปรแกรมได้ ไม่จำเป็นต้องเหมือนคน และเหมาะกับงานซ้ำ แม่นยำ เร็ว หนัก หรือเสี่ยง",
        },
        {
          time: "12–20",
          phase: "Robot ในอุตสาหกรรมต่าง ๆ",
          activity: "ดูงานจริง: รถยนต์ — เชื่อม/พ่นสี/ประกอบ · อาหาร — หยิบ/จัดเรียง/บรรจุ · อิเล็กทรอนิกส์ — หยิบวางชิ้นส่วนเล็ก",
          teacher: "Model การคิด Industry → Task → What the job needs → Robot Options",
        },
        {
          time: "20–28",
          phase: "ประเภทหุ่นยนต์อุตสาหกรรม",
          activity: "จับคู่การ์ดภาพ Articulated, SCARA, Cartesian, Delta และ Mobile Robot กับงานตัวอย่าง",
          teacher: "ใช้การ์ดภาพ + งานตัวอย่าง ไม่ต้องจำชื่อทั้งหมด ไม่ลง DOF หรือกลไกเชิงลึก",
          note: "หุ่นยนต์ 5 ประเภทใน 8 นาทีค่อนข้างเยอะ — ให้เด็กจับคู่ “ภาพ ↔ งาน” ก็พอ ไม่ต้องท่องชื่อ",
        },
        {
          time: "28–33",
          phase: "Robot + Human",
          activity: "เชื่อมงานของหุ่นยนต์กับคนที่ออกแบบ โปรแกรม ควบคุม บำรุงรักษา และตรวจคุณภาพ พร้อมรู้จักอาชีพตัวอย่าง",
          teacher: "เปรียบเทียบจุดแข็งอย่างสมดุล — Automation ไม่ได้แปลว่า “ไม่มีคน”",
        },
        {
          time: "33–43",
          phase: "Activity: Factory Robot Detective",
          activity: "ทำงานกลุ่มจาก Scenario Card 3 สถานการณ์ เติม Task → Need → Robot Options → Choose → Why → Human Role โดยอย่างน้อย 1 สถานการณ์เปรียบเทียบ Robot 2 แบบ",
          teacher: "ถาม “งานนี้ต้องการอะไรเป็นพิเศษ?” “แบบไหนเหมาะกว่า เพราะอะไร?” “ส่วนไหนยังควรให้คนรับผิดชอบ?”",
        },
        {
          time: "43–45",
          phase: "สรุปและเชื่อมหน่วยที่ 2",
          activity: "สรุปว่าการเลือก Robot ต้องเริ่มจากงาน และบอกบทบาทมนุษย์ 1 บทบาท",
          teacher: "Robot หนึ่งตัวเป็นเพียงส่วนหนึ่งของโรงงาน — คาบหน้าจะดูทั้งระบบ",
        },
        {
          time: "45–50",
          phase: "Exercise 1: Robot in Industry Match",
          activity: "จาก 2 สถานการณ์ ระบุ งาน → ความต้องการ → Robot ที่เป็นไปได้ → เลือก → เหตุผล และตอบว่ามนุษย์ยังมีบทบาทอะไร",
          teacher: "ตรวจว่าเลือกจากความต้องการของงาน",
        },
      ],
    },
  ],
  guides: [
    {
      id: "robot-types",
      title: "จับคู่ประเภทหุ่นยนต์กับงาน",
      blocks: [
        { icon: "🦾", heading: "Articulated", text: "ยืดหยุ่นหลายทิศทาง — เชื่อม พ่นสี ประกอบ (แขนกลห้องเรียนก็เป็นแบบนี้)" },
        { icon: "⚙️", heading: "SCARA", text: "ประกอบชิ้นส่วนเร็วในแนวระนาบ" },
        { icon: "📐", heading: "Cartesian", text: "เคลื่อนตามแนวแกน X–Y–Z" },
        { icon: "🕷️", heading: "Delta", text: "หยิบวางเร็วมาก เช่น จัดเรียงขนม" },
        { icon: "🚚", heading: "Mobile Robot / AGV–AMR", text: "ขนส่งของในโรงงาน" },
      ],
      tip: "ไม่มี Robot แบบเดียวที่ดีที่สุดสำหรับทุกงาน — เริ่มจากถามว่า “งานต้องการอะไร?”",
    },
    {
      id: "robot-human",
      title: "Robot เด่นอะไร คนเด่นอะไร",
      blocks: [
        { icon: "🤖", heading: "Robot", text: "งานซ้ำ · ความเร็ว · ความสม่ำเสมอ · งานหนัก · งานเสี่ยง" },
        { icon: "🧑‍🔧", heading: "Human", text: "ตัดสินใจ · ปรับตัว · แก้ปัญหา · ออกแบบ · โปรแกรม · บำรุงรักษา · ตรวจคุณภาพ · ดูแลความปลอดภัย" },
        { icon: "💼", heading: "อาชีพตัวอย่าง", text: "Robot Programmer · Automation Technician/Engineer · Maintenance · Quality Control" },
      ],
    },
  ],
  assessment: [
    {
      topic: "ความเข้าใจ Robot in Industry",
      method: "ถามตอบ ภาพ/คลิป และ Exercise 1",
      criteria: "อธิบายความหมายของหุ่นยนต์อุตสาหกรรม เหตุผลที่โรงงานใช้ และยกตัวอย่างได้",
    },
    {
      topic: "วิเคราะห์และเลือก Robot (ว 4.2 ป.6/1)",
      method: "Factory Robot Detective และ Exercise 1",
      criteria: "เปรียบเทียบ Robot อย่างน้อย 2 ตัวเลือก เลือกแบบที่เหมาะ และให้เหตุผลจากความต้องการของงาน",
    },
    {
      topic: "บทบาทคนและอาชีพ (ท 3.1 ป.6/2)",
      method: "การอธิบายระหว่าง Activity และ Exercise",
      criteria: "บอกบทบาทของมนุษย์และอาชีพที่เกี่ยวข้องได้อย่างน้อย 1 ตัวอย่าง",
    },
    { topic: "การมีส่วนร่วมและความปลอดภัย", method: "สังเกตพฤติกรรม", criteria: "รับฟังเพื่อนและตระหนักถึงความปลอดภัยในการทำงานกับหุ่นยนต์" },
  ],
  takeaways: [
    "หุ่นยนต์อุตสาหกรรมตั้งโปรแกรมได้ ไม่จำเป็นต้องเหมือนคน และเหมาะกับงานซ้ำ แม่นยำ เร็ว หนัก หรือเสี่ยง",
    "การเลือกหุ่นยนต์เริ่มจากงาน — ไม่มี Robot แบบเดียวที่ดีที่สุดสำหรับทุกงาน",
    "Robot ไม่ได้ดีกว่าคนทุกงาน มนุษย์ยังสำคัญในการตัดสินใจ ออกแบบ ดูแล และรับผิดชอบระบบ",
  ],
  bridge: {
    nextLabel: "หน่วยที่ 2: Industrial Automation",
    question: "“ถ้าในโรงงานมี Robot และเครื่องจักรหลายตัว เราจะทำให้ทั้งหมดทำงานต่อกันได้อย่างไร?”",
    teaser: "“คาบหน้าเราจะขยายจาก Robot หนึ่งตัว ไปสู่ระบบอัตโนมัติและสายการผลิตทั้งสาย”",
  },
};

const unit2: LessonPlan = {
  gradeSlug: "p6",
  week: 2,
  unitLabel: "หน่วยที่ 2",
  title: "Industrial Automation",
  subtitle: "จาก Robot หนึ่งตัว สู่สายการผลิตทั้งระบบ",
  minutesPerPeriod: 50,
  summary: [
    "Industrial Automation คือการทำให้เครื่องจักร หุ่นยนต์ อุปกรณ์ควบคุม และกระบวนการทำงานตามลำดับหรือเงื่อนไขที่กำหนดโดยอัตโนมัติ สายการผลิต (Production Line) เป็นลำดับของสถานีงานที่ชิ้นงานเคลื่อนผ่าน เช่น รับวัตถุดิบ → ประกอบ → ตรวจสอบ → บรรจุ → ขนส่ง และต้องประสานกันผ่านการ “ส่งต่อ” (Handoff) — ถ้าสถานีหนึ่งหยุด ทั้งสายอาจได้รับผลกระทบ",
    "Robot Arm เป็นเพียง “หนึ่งสถานี” ในระบบ ส่วน Industry 4.0 คือ Automation ที่เพิ่มการเชื่อมโยงและใช้ข้อมูล Production Line ที่เด็กออกแบบในหน่วยนี้จะถูกเก็บไว้เป็น Blueprint สำหรับสร้างจริงในหน่วยที่ 16 Mini Product Line และปูทางไปหน่วยที่ 13–15 (Conveyor, Quality Inspection)",
  ],
  objectives: [
    "อธิบายความหมายของ Industrial Automation และเหตุผลที่โรงงานใช้ระบบอัตโนมัติได้",
    "อธิบาย Production Line ระบุหน้าที่และการส่งต่อระหว่างสถานี และบทบาทของ Robot Arm ในฐานะหนึ่งสถานีได้",
    "แยกความแตกต่างเบื้องต้นระหว่าง Automation กับ Industry 4.0 ได้",
    "ออกแบบ Production Line อย่างง่าย พร้อมอธิบายลำดับ การส่งต่อ และตำแหน่งที่เหมาะของ Robot Arm ได้",
    "คาดการณ์ผลเมื่อสถานีหนึ่งหยุดหรือทำงานผิดลำดับได้",
    "ทำงานอย่างเป็นระบบ และตระหนักถึงบทบาทของมนุษย์ในการควบคุมระบบอัตโนมัติ",
  ],
  standards: [
    { ...logicP6, core: "อธิบายลำดับการทำงานของระบบอัตโนมัติ วิเคราะห์ความสัมพันธ์ของสถานี และออกแบบ Production Line อย่างง่าย" },
    { ...questionP6, core: "ตั้งและตอบคำถามจากสถานการณ์สายการผลิต อธิบายลำดับและเหตุผลที่ระบบต้องประสานกัน" },
  ],
  integrations: [{ subject: "การงานอาชีพ", detail: "กระบวนการทำงานในโรงงาน และบทบาทของคนกับระบบอัตโนมัติ" }],
  learning: {
    knowledge:
      "Industrial Automation, Production Line และสถานีงาน, Handoff ระหว่างสถานี, Automated Station ต่างจาก Production Line, Robot Arm เป็นหนึ่งสถานี, Automation System = งานเข้า → Control/Process → Output และ Industry 4.0 = Automation + Connectivity + Data",
    skills: "การจัดลำดับและออกแบบสายการผลิต การอธิบาย Handoff การเลือกตำแหน่ง Robot Arm และการคิดเชิงระบบเมื่อส่วนหนึ่งหยุดทำงาน",
    attitude: "ทำงานเป็นขั้นตอน เปิดรับเหตุผลที่แตกต่าง และเห็นความสำคัญของมนุษย์ในการดูแลระบบ",
  },
  vocabulary: [
    { term: "Automation", meaning: "การทำงานตามลำดับหรือเงื่อนไขโดยอัตโนมัติ" },
    { term: "Production Line", meaning: "สายการผลิตที่ประกอบด้วยหลายสถานี" },
    { term: "Station", meaning: "สถานีงานที่ทำหน้าที่หนึ่งในสายการผลิต" },
    { term: "Handoff", meaning: "การส่งต่อชิ้นงานหรือสถานะไปยังสถานีถัดไป" },
    { term: "Industry 4.0", meaning: "Automation ที่เชื่อมโยงและใช้ข้อมูลมากขึ้น" },
  ],
  materials: [
    "ภาพหรือคลิปสั้นของสายการผลิตอัตโนมัติ และภาพ Smart Factory",
    "Production Line Station Cards 5–6 ขั้นของผลิตภัณฑ์ตัวอย่าง 2 ชุด · Problem Card 1–2 แบบ",
    "ของจริงชิ้นเล็ก (เช่น กล่องหรือบล็อก) สำหรับกิจกรรมเล่นบทเป็นสถานีแล้วส่งต่อกันจริง",
    "แฟ้มเก็บ Production Line Blueprint ของแต่ละกลุ่ม — นำกลับมาใช้ในหน่วยที่ 16",
    "ใบงาน Exercise 1: Automation System Check",
  ],
  sessions: [
    {
      number: 1,
      title: "Industrial Automation",
      minutes: 50,
      goal: "นักเรียนอธิบาย Industrial Automation และ Production Line ออกแบบและเล่นบทสายการผลิตอย่างง่าย วิเคราะห์ผลเมื่อสถานีหยุด และเข้าใจ Industry 4.0 ในระดับเบื้องต้น",
      steps: [
        {
          time: "0–5",
          phase: "เชื่อมจาก Robot สู่ System",
          activity: "ดูภาพ/คลิปโรงงานที่มี Robot, Conveyor, เครื่องจักร และคนทำงานร่วมกัน",
          teacher: "ถาม “ถ้าในโรงงานมี Robot หลายตัว จะทำให้ทำงานต่อกันได้อย่างไร?”",
        },
        {
          time: "5–11",
          phase: "Concept: Industrial Automation",
          activity: "เรียนความหมายของ Automation และแยกองค์ประกอบในระบบ",
          teacher: "ย้ำว่า “อัตโนมัติ” ไม่ได้แปลว่าไม่มีคน และไม่จำเป็นต้องใช้ Robot ทุกขั้น",
        },
        {
          time: "11–18",
          phase: "Production Line = งานที่ต่อกันเป็นระบบ",
          activity: "ดูสายการผลิตตัวอย่าง แล้วระบุว่าแต่ละ Station รับอะไร ทำอะไร และส่งต่ออะไร",
          teacher: "Model กรอบ Input → Station 1 → Handoff → Station 2 → … → Output",
        },
        {
          time: "18–26",
          phase: "Role-play: เราคือสายการผลิต",
          activity:
            "นักเรียน 5 คนเล่นบทเป็น Station ยืนเรียงกันและส่งกล่องต่อกันจริง (รับ → ประกอบ → ตรวจ → บรรจุ → ส่ง) จากนั้นครูให้ Station ตรวจสอบ “หยุดทำงาน” แล้วให้ทั้งห้องสังเกตว่าเกิดอะไรขึ้นกับสถานีถัดไป",
          teacher: "ถาม “ชิ้นงานกองอยู่ที่ไหน?” “สถานีไหนว่าง?” “ระบบรู้ได้อย่างไรว่ามีปัญหา?”",
          note: "การเล่นบทส่งของต่อกันจริงช่วยให้เห็น Handoff และ Fault ชัดกว่าการใช้การ์ดอย่างเดียว",
        },
        {
          time: "26–31",
          phase: "Industry 4.0 เบื้องต้น",
          activity: "เปรียบเทียบโรงงาน Automation ปกติกับ Smart Factory ที่ส่งข้อมูลสถานะให้คนมองเห็น",
          teacher: "สรุป Industry 4.0 = Automation + Connectivity + Data ไม่ลง IoT/Cloud/AI เชิงเทคนิค",
        },
        {
          time: "31–42",
          phase: "Activity: Build the Production Line",
          activity: "ออกแบบ Production Line 5–6 ขั้น กำหนดว่าแต่ละ Station “รับอะไร → ทำอะไร → ส่งต่ออะไร” เลือกจุดที่เหมาะกับ Robot Arm และรับ Problem Card 1 ใบเพื่อคาดการณ์ผล",
          teacher: "ถาม “ขั้นไหนต้องมาก่อน/หลัง?” “Robot Arm ควรอยู่สถานีไหน?” — เก็บผลงานเป็น Blueprint ไว้ใช้ในหน่วยที่ 16",
        },
        {
          time: "42–45",
          phase: "สรุปและเชื่อมหน่วยที่ 3",
          activity: "สรุป Automation · Production Line · Industry 4.0",
          teacher: "บอกว่าสถานี Conveyor และ Quality Inspection จะได้สร้างจริงในหน่วยที่ 13–15 และหน่วยที่ 3 จะเปิดดู Robot Arm",
        },
        {
          time: "45–50",
          phase: "Exercise 1: Automation System Check",
          activity: "จากแผนภาพที่กำหนด ระบุหน้าที่และ Handoff เลือก Station ที่เหมาะกับ Robot Arm วิเคราะห์ Fault 1 จุด และเลือกคำอธิบาย Industry 4.0 ที่ถูก",
          teacher: "ตรวจการคิดเชิงระบบ",
        },
      ],
    },
  ],
  guides: [
    {
      id: "production-line",
      title: "Production Line และ Handoff",
      blocks: [
        {
          heading: "ตัวอย่างสายการผลิต",
          flow: [
            { label: "รับวัตถุดิบ" },
            { label: "ประกอบ", sub: "Robot Arm" },
            { label: "ตรวจสอบ" },
            { label: "บรรจุ" },
            { label: "ขนส่ง" },
          ],
        },
        {
          icon: "⚠️",
          heading: "ถ้าสถานีตรวจสอบหยุด",
          text: "ชิ้นงานกองที่สถานีตรวจ · สถานีบรรจุว่าง · ส่งของไม่ทัน",
          questions: ["สถานีไหนได้รับผลกระทบ?", "ระบบควรแจ้งใคร?"],
        },
      ],
      tip: "เก็บ Blueprint ของแต่ละกลุ่มไว้ — หน่วยที่ 16 Mini Product Line จะสร้างจากแบบนี้จริง",
    },
  ],
  assessment: [
    {
      topic: "Automation / Production Line / Industry 4.0",
      method: "ถามตอบ และ Exercise 1",
      criteria: "อธิบาย Automation และ Production Line และแยก Automation กับ Industry 4.0 ได้",
    },
    {
      topic: "ออกแบบ Production Line (ว 4.2 ป.6/1)",
      method: "Build the Production Line และ Exercise 1",
      criteria: "ออกแบบได้สมเหตุสมผล อธิบาย Handoff และเลือกตำแหน่ง Robot Arm ได้",
    },
    {
      topic: "คิดเชิงระบบ (ท 3.1 ป.6/2)",
      method: "Role-play, Problem Card และ Fault Analysis",
      criteria: "คาดการณ์ได้ว่า Fault จะส่งผลต่อสถานีใด พร้อมเหตุผล",
    },
    { topic: "ความเป็นระบบและความปลอดภัย", method: "สังเกตพฤติกรรม", criteria: "ทำงานเป็นขั้นตอนและตระหนักถึงความปลอดภัยของระบบ" },
  ],
  takeaways: [
    "Industrial Automation คือการทำงานตามลำดับ/เงื่อนไขโดยอัตโนมัติ — ไม่ได้แปลว่าไม่มีคน",
    "Production Line คือชุดของ Station ที่ส่งต่อกัน ถ้าส่วนหนึ่งหยุดหรือผิดลำดับ อาจกระทบทั้งสาย",
    "Robot Arm เป็นหนึ่ง Station ส่วน Industry 4.0 เพิ่มการเชื่อมโยงและการใช้ข้อมูล",
  ],
  bridge: {
    nextLabel: "หน่วยที่ 3: Robot Arm Components",
    question: "“Robot Arm หนึ่ง Station ต้องมีส่วนใดบ้าง จึงจะหยิบหรือวางชิ้นงานได้?”",
    teaser: "“คาบหน้าเราจะเปิดดูข้างในแขนกลว่าประกอบด้วยอะไร”",
  },
};

const unit3: LessonPlan = {
  gradeSlug: "p6",
  week: 3,
  unitLabel: "หน่วยที่ 3",
  title: "Robot Arm Components",
  subtitle: "Link, Actuator, DOF และ End Effector",
  minutesPerPeriod: 50,
  summary: [
    "เด็กเรียนส่วนประกอบพื้นฐาน (Base, Joint, Servo, Gripper, Controller, End Effector) และ Control Flow มาแล้วใน ป.5 หน่วยที่ 1 หน่วยนี้จึงทบทวนสั้น ๆ แล้วเน้นของใหม่: Link (ชิ้นส่วนแข็งระหว่างข้อต่อ), Actuator (คำกว้างของตัวขับเคลื่อน ซึ่ง Servo เป็นชนิดหนึ่ง) และ DOF — จำนวนการเคลื่อนที่อิสระที่สั่งแยกกันได้",
    "DOF มากขึ้นช่วยเพิ่มความยืดหยุ่น แต่ไม่ได้ดีกว่าหรือจำเป็นกับทุกงาน การออกแบบเริ่มจาก Task → Movement Needed → DOF → End Effector และเลือก End Effector (Gripper, Suction Cup, เครื่องมือเฉพาะงาน) ให้เหมาะกับชิ้นงาน ก่อนเปรียบเทียบแขนกลอุตสาหกรรมกับแขนกลห้องเรียนด้วย 3 เกณฑ์: ขนาด/กำลัง ความเร็ว/ความแม่นยำ และความปลอดภัย",
  ],
  objectives: [
    "ระบุและอธิบายหน้าที่ของ Base, Link, Joint, Actuator, Controller และ End Effector ได้",
    "อธิบายความหมายของ DOF ในระดับพื้นฐานและยกตัวอย่างการเคลื่อนที่อิสระของ Robot Arm ได้",
    "อธิบายความต่างระหว่าง End Effector กับ Gripper และปัจจัยที่ใช้เลือก End Effector ได้",
    "วิเคราะห์ Task → Movement Needed → DOF → End Effector และคาดการณ์ผลเมื่อบางส่วนทำงานผิดปกติได้",
    "เปรียบเทียบแขนกลอุตสาหกรรมกับแขนกลห้องเรียนด้วย 3 เกณฑ์หลักได้",
    "ใช้แขนกลอย่างปลอดภัยและทำงานร่วมกับผู้อื่นได้",
  ],
  standards: [
    { ...logicP6, core: "อธิบายความสัมพันธ์ Controller → Actuator → Joint/Link → End Effector วิเคราะห์ DOF และเปรียบเทียบแขนกลสองบริบท" },
    { ...teamworkP6, core: "จัดการงานกลุ่ม แลกเปลี่ยนเหตุผล ตัดสินใจร่วมกัน และใช้อุปกรณ์ตามข้อตกลงด้านความปลอดภัย" },
  ],
  integrations: [
    { subject: "วิทยาศาสตร์", detail: "การเคลื่อนที่ แรง และการถ่ายทอดการเคลื่อนที่ผ่านข้อต่อ" },
    { subject: "ภาษาไทย", detail: "การอธิบายเหตุผลจากภาพ/คลิป/แบบจำลอง และการอภิปรายเปรียบเทียบ" },
  ],
  learning: {
    knowledge:
      "ส่วนประกอบ Base, Link, Joint, Actuator, Controller, End Effector; DOF = การเคลื่อนที่อิสระที่สั่งแยกกันได้; End Effector ≠ Gripper เสมอไป; ปัจจัยเลือก End Effector; Program → Controller → Actuator → Joint/Link → End Effector และความต่างของแขนกลอุตสาหกรรมกับห้องเรียน",
    skills: "การสังเกตและระบุส่วนประกอบ การทำ Move Map การวิเคราะห์ Task → Movement → DOF → End Effector การคาดการณ์ What if? และการเปรียบเทียบด้วยเกณฑ์เดียวกัน",
    attitude: "สังเกตอย่างเป็นระบบ ใช้อุปกรณ์อย่างปลอดภัย และเห็นความสำคัญของการเลือกส่วนประกอบให้เหมาะกับงาน",
  },
  vocabulary: [
    { term: "Link", meaning: "ชิ้นส่วนแข็งของแขนที่เชื่อมระหว่างข้อต่อ" },
    { term: "Actuator", meaning: "ตัวขับเคลื่อน เช่น มอเตอร์ — Servo เป็นชนิดหนึ่ง" },
    { term: "DOF (Degree of Freedom)", meaning: "จำนวนการเคลื่อนที่อิสระที่สั่งแยกกันได้" },
    { term: "End Effector", meaning: "เครื่องมือที่ปลายแขน — Gripper, Suction Cup หรือเครื่องมือเฉพาะงาน" },
  ],
  materials: [
    "Robot Arm ของห้องเรียน/แบบจำลอง — ครูตรวจและบันทึกจำนวน DOF ของแขนกลห้องเรียนไว้ล่วงหน้า เพื่อเฉลย Move Map ได้ตรง",
    "Servo/Motor ตัวอย่าง · Gripper/End Effector ตัวอย่าง (ถ้ามี)",
    "ภาพ/คลิป Robot Arm อุตสาหกรรม · Component Cards · DOF Motion Cards · End Effector Cards · Task Cards",
    "ตารางเปรียบเทียบ Industrial vs Classroom Robot Arm · ใบงาน Exercise 1–2",
  ],
  sessions: [
    {
      number: 1,
      title: "Structure, Joint & DOF — แขนกลขยับได้อย่างไร",
      minutes: 50,
      goal: "นักเรียนทบทวนส่วนประกอบจาก ป.5 อย่างรวดเร็ว แล้วเข้าใจของใหม่ คือ Link, Actuator และ DOF พร้อมอธิบายเส้นทางจากคำสั่งสู่การเคลื่อนที่",
      steps: [
        { time: "0–5", phase: "เชื่อมจากหน่วยที่ 2", activity: "โฟกัสที่ Robot Arm หนึ่ง Station ใน Production Line", teacher: "วันนี้จะ “เปิดดูข้างใน” แขนกล" },
        {
          time: "5–10",
          phase: "Quick Recall จาก ป.5",
          activity: "ชี้ Base, Joint, Servo, Gripper, Controller และ End Effector บนแขนกลจริงให้เร็วที่สุด พร้อมเรียง Control Flow ที่เคยเรียน",
          teacher: "ทบทวนเร็ว ไม่สอนซ้ำ",
          note: "เนื้อหาส่วนประกอบซ้อนกับ ป.5 หน่วยที่ 1 เกือบทั้งหมด จึงย่อเหลือ Recall 5 นาที แล้วใช้เวลากับของใหม่",
        },
        {
          time: "10–18",
          phase: "ของใหม่: Link + Actuator",
          activity: "รู้จัก Link (ชิ้นส่วนแข็งระหว่างข้อต่อ) และ Actuator (ตัวขับเคลื่อน — คำกว้างที่รวม Servo และมอเตอร์ชนิดอื่น)",
          teacher: "เปรียบเทียบง่าย ๆ Link ≈ ท่อนแขน · Joint ≈ ข้อศอก · Actuator ≈ กล้ามเนื้อ แต่ย้ำว่าไม่เหมือนแขนคนทุกอย่าง",
        },
        {
          time: "18–27",
          phase: "DOF = การเคลื่อนที่อิสระที่ควบคุมได้",
          activity: "สังเกตการขยับทีละแบบ เช่น หมุนฐาน ยก/ลดแขน ก้ม/เงยข้อมือ แล้วตอบว่า “แบบไหนสั่งแยกจากแบบอื่นได้?” จึงนับเป็น DOF",
          teacher: "“หนึ่งการขยับที่สั่งแยกได้ = หนึ่งอิสระ” — ดูจากการเคลื่อนที่จริง ไม่จำจากจำนวน Joint อย่างเดียว",
        },
        { time: "27–33", phase: "Controller → Actuator → Joint/Link", activity: "ติดตามคำสั่ง “ยกแขนขึ้น”: Controller ส่งคำสั่ง → Actuator ทำงาน → Joint เคลื่อน → Link เปลี่ยนตำแหน่ง", teacher: "Controller ไม่ได้ยกชิ้นงานเอง" },
        {
          time: "33–42",
          phase: "Activity: Component Detective & Move Map",
          activity: "หาส่วนประกอบบนแขนกล วาดลูกศรการขยับที่ครูสาธิต 2–3 แบบ ทำ Move Map ว่า “ส่วนไหนขยับ → ขยับแบบไหน → สั่งแยกได้ไหม” แล้วสรุป DOF และตอบ What if 1 ข้อ",
          teacher: "ถาม “ถ้า Actuator ที่ยกแขนไม่ทำงาน งานอะไรจะทำไม่ได้?”",
        },
        { time: "42–45", phase: "สรุป Structure → Motion", activity: "สรุป Controller → Actuator → Joint/Link → การเคลื่อนที่ และ DOF", teacher: "เชื่อมคาบ 2: ปลายแขนทำงานกับชิ้นงานอย่างไร" },
        { time: "45–50", phase: "Exercise 1: Function & Motion Check", activity: "จับคู่ส่วนประกอบกับหน้าที่ เรียง Flow และดู Motion Diagram นับ DOF จากสถานการณ์ใหม่", teacher: "ใช้ภาพที่ไม่ซ้ำกับ Activity" },
      ],
    },
    {
      number: 2,
      title: "End Effector & Comparison — แขนกลทำงานกับชิ้นงานอย่างไร",
      minutes: 50,
      goal: "นักเรียนแยก End Effector กับ Gripper วิเคราะห์ Task → Movement → DOF → End Effector และเปรียบเทียบแขนกลอุตสาหกรรมกับแขนกลห้องเรียนได้",
      steps: [
        { time: "0–5", phase: "จากการเคลื่อนที่สู่การทำงาน", activity: "ตั้งคำถาม “ขยับได้แล้ว แต่จะหยิบ จับ ดูด หรือใช้เครื่องมือกับชิ้นงานได้อย่างไร?”", teacher: "ทบทวน Flow สั้น ๆ" },
        { time: "5–10", phase: "Quick Review: จากคำสั่งถึงปลายแขน", activity: "ชี้ตาม Move → Grip → Release ผ่าน Program → Controller → Actuator → Joint/Link → End Effector", teacher: "ไม่สอน Controller ซ้ำ" },
        { time: "10–20", phase: "End Effector ≠ Gripper เสมอไป", activity: "เปรียบเทียบ Gripper, Suction Cup และเครื่องมือเฉพาะงาน แล้วจับคู่กับงาน", teacher: "เลือกตามรูปร่าง วัสดุ น้ำหนัก ความเปราะบาง และงานที่ต้องทำ" },
        {
          time: "20–35",
          phase: "Activity: End Effector Mission",
          activity: "จาก Task Card 2 งาน คิดตาม Task → Movement Needed → DOF → End Effector และอธิบายเหตุผลของทีม",
          teacher: "ท้าทาย “ถ้าเพิ่ม DOF อีก 1 แบบ จะช่วยงานนี้จริงไหม?” — DOF มากขึ้นไม่ได้ดีกว่าเสมอ",
        },
        { time: "35–42", phase: "Industrial vs Classroom Robot Arm", activity: "เปรียบเทียบด้วย 3 เกณฑ์: ขนาด/กำลัง · ความเร็ว/ความแม่นยำ · ความปลอดภัย และระบุ 1 สิ่งที่เหมือนกัน", teacher: "ระบบควบคุมหรือความทนทานเป็นตัวอย่างเสริมเมื่อมีเวลา" },
        { time: "42–45", phase: "สรุปและเชื่อมหน่วยที่ 4", activity: "สรุปส่วนประกอบ DOF End Effector และความต่างของแขนกลสองแบบ", teacher: "ปูหน่วยที่ 4: ปลายแขนอยู่ตรงไหน — พิกัด X–Y–Z" },
        { time: "45–50", phase: "Exercise 2: Spot, Explain & Compare", activity: "ตัดสินว่า End Effector ที่ให้มาเหมาะกับงานไหม เติมตารางเปรียบเทียบ 3 เกณฑ์ และเขียน 1 ประโยคว่าเหมือนกันตรงไหน", teacher: "ใช้สถานการณ์ใหม่" },
      ],
    },
  ],
  guides: [
    {
      id: "task-to-effector",
      title: "Task → Movement Needed → DOF → End Effector",
      blocks: [
        {
          heading: "ลำดับการคิด",
          flow: [{ label: "Task" }, { label: "Movement Needed" }, { label: "DOF" }, { label: "End Effector" }],
        },
        { icon: "📦", heading: "หยิบกล่องจากสายพาน", text: "ต้องหมุนฐาน + ยก/ลด → Gripper" },
        { icon: "🪟", heading: "ย้ายแผ่นพื้นผิวเรียบ", text: "ต้องยก/ลดและวางให้ตรง → Suction Cup" },
      ],
      tip: "DOF มากขึ้นเพิ่มทางเลือกในการเคลื่อนที่ แต่ไม่จำเป็นต้องดีกว่าหรือจำเป็นกับทุกงาน",
    },
    {
      id: "compare-arms",
      title: "Industrial vs Classroom Robot Arm",
      blocks: [
        { icon: "💪", heading: "ขนาด / กำลัง", text: "อุตสาหกรรมรับภาระได้มากกว่า" },
        { icon: "⚡", heading: "ความเร็ว / ความแม่นยำ", text: "อุตสาหกรรมเร็วและแม่นยำกว่า" },
        { icon: "🛡️", heading: "ความปลอดภัย", text: "อุตสาหกรรมต้องมีมาตรการเข้มกว่า เช่น รั้วกั้นและปุ่มหยุดฉุกเฉิน" },
        { icon: "🤝", heading: "สิ่งที่เหมือนกัน", text: "มีโครงสร้าง ข้อต่อ ตัวขับเคลื่อน Controller และ End Effector" },
      ],
    },
  ],
  assessment: [
    {
      topic: "ส่วนประกอบ DOF และ End Effector",
      method: "ถามตอบ Component Detective, End Effector Mission และ Exercise 1–2",
      criteria: "อธิบายหน้าที่ส่วนประกอบ DOF ระดับพื้นฐาน และแยก End Effector กับ Gripper ได้",
    },
    {
      topic: "วิเคราะห์ Task → Movement → Robot Function (ว 4.2 ป.6/1)",
      method: "Move Map + End Effector Mission",
      criteria: "เชื่อม Task กับ Movement/DOF ได้สมเหตุสมผล และคาดการณ์ผลเมื่อบางส่วนหยุดทำงานได้",
    },
    {
      topic: "เปรียบเทียบแขนกลสองแบบ",
      method: "ตารางเปรียบเทียบ + Exercise 2",
      criteria: "เปรียบเทียบได้อย่างน้อย 2 จาก 3 เกณฑ์ และระบุ 1 สิ่งที่เหมือนกัน",
    },
    {
      topic: "การทำงานร่วมกันและความปลอดภัย (ง 1.1 ป.6/2)",
      method: "สังเกตพฤติกรรม",
      criteria: "ร่วมมือ ตัดสินใจร่วมกัน และหยุดระบบก่อนสัมผัสส่วนเคลื่อนที่",
    },
  ],
  takeaways: [
    "Robot Arm ประกอบด้วย Base, Link, Joint, Actuator, Controller และ End Effector ที่ทำงานร่วมกัน",
    "DOF คือจำนวนการเคลื่อนที่อิสระที่สั่งแยกกันได้ — มากขึ้นช่วยให้ยืดหยุ่น แต่ไม่จำเป็นกับทุกงาน",
    "เลือก End Effector ตามงานและชิ้นงาน (Gripper เป็นแค่ชนิดหนึ่ง) · แขนกลอุตสาหกรรมกับห้องเรียนหลักการเหมือนกัน แต่ต่างด้านกำลัง ความเร็ว และความปลอดภัย",
  ],
  bridge: {
    nextLabel: "หน่วยที่ 4: Robot Coordinate & Position",
    question: "“รู้แล้วว่าแขนกลขยับอย่างไร แล้วจะบอกได้อย่างไรว่าปลายแขนอยู่ตรงไหนในพื้นที่ 3 มิติ?”",
    teaser: "“คาบหน้าเราจะต่อยอดพิกัด X–Y จาก ป.5 ไปสู่ X–Y–Z”",
  },
};

const unit4: LessonPlan = {
  gradeSlug: "p6",
  week: 4,
  unitLabel: "หน่วยที่ 4",
  title: "Robot Coordinate & Position",
  subtitle: "X–Y–Z, Workspace, Home และ Pick/Place Point",
  minutesPerPeriod: 50,
  summary: [
    "หน่วยนี้ต่อยอดจาก ป.5 หน่วยที่ 2 ที่เด็กใช้พิกัด X–Y บนกริดและรู้ว่า Home ไม่ใช่ 0° ของทุก Joint คราวนี้เพิ่มแกน Z เพื่อบอกตำแหน่งปลายแขนในพื้นที่ 3 มิติ พิกัดต้องอ่านพร้อมจุดอ้างอิงและทิศของแกนเสมอ Workspace คือบริเวณที่ปลายแขนเข้าถึงได้ — ไม่ใช่ทุกพิกัดที่ Robot ไปถึง และ Home Position ไม่จำเป็นต้องเป็นจุดกำเนิด (0,0,0)",
    "แขนกลห้องเรียนสั่งงานด้วยมุม Servo ไม่ได้สั่งด้วยพิกัด X–Y–Z โดยตรง หน่วยนี้จึงเชื่อมสองภาษาเข้าด้วยกันด้วย Teach Point: วัดตำแหน่ง Pick/Place จริงบนโต๊ะ แล้วขยับแขนไปที่จุดนั้นด้วยความเร็วต่ำและบันทึกมุม Servo ไว้ เพื่อให้ตำแหน่งที่วางแผนนำไปใช้กับเครื่องจริงได้",
  ],
  objectives: [
    "อธิบายแกน X–Y–Z จุดอ้างอิง และพิกัด (X, Y, Z) ในการระบุตำแหน่งของ Robot Arm ได้",
    "อธิบาย Workspace และ Home Position และแยก Home Position กับ Origin ได้",
    "อ่านพิกัดจากแผนภาพหรือวัดจากโต๊ะจริง และบอกได้ว่าการย้ายตำแหน่งเกี่ยวกับแกนใด",
    "ตรวจว่าจุดอยู่ใน Workspace และกำหนด Home, Pick Point และ Place Point ได้อย่างสมเหตุสมผล",
    "บันทึก Teach Point โดยเชื่อมตำแหน่ง Pick/Place กับมุม Servo ของแขนกลจริงได้",
    "ตรวจพบและแก้ข้อผิดพลาดของตำแหน่ง และตระหนักถึงพื้นที่ปลอดภัยก่อนทดสอบ",
  ],
  standards: [
    { ...logicP6, core: "อธิบายตำแหน่งปลายแขนด้วยพิกัด X–Y–Z ตรวจว่าจุดอยู่ใน Workspace และกำหนด Home / Pick / Place อย่างเป็นระบบ" },
    { ...teamworkP6, core: "ทำงานกลุ่มวัดตำแหน่งจริง กำหนดจุด ตรวจสอบความถูกต้อง และใช้อุปกรณ์ตามข้อตกลงด้านความปลอดภัย" },
  ],
  integrations: [
    { subject: "คณิตศาสตร์", detail: "การอ่านตำแหน่งจากแกนและค่าพิกัด การวัดระยะด้วยไม้บรรทัด และการใช้จุดอ้างอิง" },
    { subject: "ภาษาไทย", detail: "การอธิบายตำแหน่ง ขั้นตอน และเหตุผลจากแผนภาพ" },
  ],
  learning: {
    knowledge:
      "แกน X–Y–Z และพิกัด (X, Y, Z), Origin และทิศของแกน, Workspace = บริเวณที่ปลายแขนเข้าถึงได้, Home Position ≠ Origin เสมอไป, Pick Point และ Place Point, Teach Point = ตำแหน่งที่บันทึกเป็นมุม Servo และ “ตำแหน่งอยู่ที่ไหน” ต่างจาก “เคลื่อนที่ไปอย่างไร”",
    skills: "การอ่าน/วัดพิกัด การระบุแกนที่เปลี่ยน การตรวจ Workspace การเติม Position Table การบันทึก Teach Point และการตรวจแก้ข้อผิดพลาดของตำแหน่ง",
    attitude: "ทำงานเป็นขั้นตอน ตรวจตำแหน่งก่อนใช้งาน และตระหนักถึงพื้นที่ปลอดภัย",
  },
  vocabulary: [
    { term: "Origin", meaning: "จุดอ้างอิงของระบบพิกัด" },
    { term: "Workspace", meaning: "บริเวณที่ปลายแขนเข้าถึงได้" },
    { term: "Home Position", meaning: "ตำแหน่งเริ่ม/กลับที่กำหนด — ไม่จำเป็นต้องเป็น (0,0,0)" },
    { term: "Pick / Place Point", meaning: "จุดที่ปลายแขนต้องไปหยิบ / วาง" },
    { term: "Teach Point", meaning: "ตำแหน่งที่บันทึกไว้เป็นชุดมุม Servo จากการขยับแขนไปยังจุดนั้น" },
  ],
  materials: [
    "Robot Arm ของห้องเรียน พร้อมวิธีขยับทีละ Joint ที่ความเร็วต่ำ (Zide หรือโปรแกรมที่ใช้ใน ป.5)",
    "แผ่นพิกัดบนโต๊ะ: ติดเทปจุด Origin และลูกศรทิศ X–Y ใต้แขนกล · ไม้บรรทัด/ตลับเมตรสำหรับวัด X, Y และความสูง Z",
    "แผนภาพแกน X–Y–Z · Workspace Diagram · Home/Pick/Place Point Cards · Error Cards",
    "Position Table แบบ “Point → (X, Y, Z) → มุม Servo ของแต่ละ Joint”",
    "ใบงาน Exercise 1: Read the Position และ Exercise 2: Position Error Check",
  ],
  sessions: [
    {
      number: 1,
      title: "X–Y–Z, Workspace & Home — ปลายแขนอยู่ตรงไหน?",
      minutes: 50,
      goal: "นักเรียนอ่านพิกัด X–Y–Z อธิบายจุดอ้างอิง Workspace และ Home Position และตรวจว่าจุดอยู่ในขอบเขตที่ Robot เข้าถึงได้",
      steps: [
        {
          time: "0–5",
          phase: "เชื่อมจากหน่วยที่ 3 และ ป.5",
          activity: "ทบทวนว่าตอน ป.5 เราใช้ (X, Y) บนกริดมองจากด้านบนบอกตำแหน่ง แล้วตั้งคำถามว่า “ถ้าปลายแขนยกสูงขึ้น เราจะบอกได้อย่างไร?”",
          teacher: "อ้างกลับ ป.5 หน่วยที่ 2 ตรง ๆ: X–Y และ Home ≠ 0° — วันนี้เพิ่มแกน Z",
        },
        {
          time: "5–12",
          phase: "Coordinate System: X–Y–Z",
          activity: "สังเกตแกน X, Y, Z บนแผ่นพิกัดจริงใต้แขนกล แล้วอ่านตำแหน่งตัวอย่าง 2–3 จุด",
          teacher: "พิกัดต้องอ่านพร้อมทิศของแกนที่กำหนด ไม่จำว่า X ต้องเป็นทิศใดในโลกจริงเสมอ",
        },
        {
          time: "12–20",
          phase: "Reference Point & Reading Position",
          activity: "เปรียบเทียบจุด A และ B ว่าแกนใดเปลี่ยน แกนใดคงเดิม",
          teacher: "หลักคิด “เริ่มจากจุดอ้างอิง → ดูทิศ X/Y/Z → อ่านทีละแกน”",
        },
        {
          time: "20–27",
          phase: "Workspace",
          activity: "จำแนกจุดตัวอย่างว่า Reachable / Not Reachable",
          teacher: "พิกัดที่เขียนได้ไม่ได้แปลว่า Robot ไปถึงทุกจุด — ไม่ลงสมการ Reach",
        },
        {
          time: "27–33",
          phase: "Home Position ≠ Origin เสมอไป",
          activity: "เปรียบเทียบ Home Position กับ Origin บนแผนภาพ และอธิบายว่าทำไม Robot ต้องมีตำแหน่งเริ่ม/กลับที่รู้แน่นอน",
          teacher: "Origin = จุดอ้างอิงของระบบพิกัด · Home = จุดที่ระบบกำหนดให้เริ่มหรือกลับ",
        },
        {
          time: "33–40",
          phase: "Activity: Coordinate Hunt & Reach Check",
          activity: "อ่านพิกัด Target Cards 3–4 จุด ระบุว่าอยู่ใน/นอก Workspace หาจุด Home และเปรียบเทียบ 2 จุดว่าแกนใดเปลี่ยน",
          teacher: "ถาม “จุดนี้ Robot ไปถึงได้ไหม เพราะอะไร?”",
        },
        {
          time: "40–45",
          phase: "สรุป Position Language",
          activity: "สรุป X–Y–Z, Workspace และ Home",
          teacher: "เชื่อมคาบ 2: ใช้พิกัดกำหนดจุดหยิบและจุดวางบนโต๊ะจริง",
        },
        {
          time: "45–50",
          phase: "Exercise 1: Read the Position",
          activity: "อ่านพิกัด 2 จุด ระบุแกนที่เปลี่ยน จำแนกจุดใน/นอก Workspace และเลือกข้อความที่ถูกเรื่อง Home กับ Origin",
          teacher: "ใช้แผนภาพใหม่",
        },
      ],
    },
    {
      number: 2,
      title: "Pick & Place Position — จากพิกัดบนโต๊ะสู่ Teach Point",
      minutes: 50,
      goal: "นักเรียนวัดและกำหนด Home, Pick, Place บนโต๊ะจริง ตรวจด้วย 3 Position Checks และบันทึกเป็น Teach Point (มุม Servo) ที่แขนกลห้องเรียนใช้ได้",
      steps: [
        {
          time: "0–5",
          phase: "ทบทวนภาษาตำแหน่ง",
          activity: "ทบทวน X–Y–Z, Workspace และ Home จากตัวอย่าง 1 ชุด",
          teacher: "คาบนี้เน้น “ไปที่จุดไหน” ยังไม่ลงเส้นทางหรือความเร็ว",
        },
        {
          time: "5–10",
          phase: "Pick Point & Place Point",
          activity: "ดูวัตถุ A และกล่อง B บนโต๊ะ แล้วระบุว่าจุดไหนคือ Pick Point และ Place Point",
          teacher: "Pick/Place คือจุดเป้าหมายของปลายแขน ไม่ใช่ชื่อวัตถุ",
        },
        {
          time: "10–16",
          phase: "Position Table + 3 Position Checks",
          activity: "อ่าน Position Table ตัวอย่างที่มีข้อผิดพลาด (X/Y สลับ, จุดนอก Workspace, Home ถูกตั้งเป็น Origin) แล้วใช้ 3 Checks หาจุดผิด",
          teacher: "3 Checks: แกน/จุดอ้างอิงถูกไหม → อยู่ใน Workspace ไหม → ชื่อจุดตรงหน้าที่ไหม",
        },
        {
          time: "16–28",
          phase: "Activity: วัดตำแหน่งจริงบนโต๊ะ",
          activity: "แต่ละกลุ่มวางวัตถุและกล่องบนแผ่นพิกัด ใช้ไม้บรรทัดวัด X, Y จาก Origin และวัดความสูง Z ของจุดหยิบ/วาง แล้วเติม Position Table และตรวจด้วย 3 Checks",
          teacher: "ถาม “วัดจาก Origin หรือยัง?” “ทุกจุดอยู่ใน Workspace ไหม?”",
          note: "เพิ่มการวัดตำแหน่งจริงอย่างน้อย 1 กิจกรรม เพื่อไม่ให้หน่วยนี้เป็นงานบนกระดาษอย่างเดียว",
        },
        {
          time: "28–40",
          phase: "Teach Point: จากพิกัดสู่มุม Servo",
          activity:
            "ครูหรือตัวแทนกลุ่มขยับแขนกลทีละ Joint ด้วยความเร็วต่ำไปที่ Pick Point ที่วัดไว้ แล้วบันทึกมุม Servo ของแต่ละ Joint ลงตาราง “Point → (X, Y, Z) → มุม Servo” ทำซ้ำกับ Place Point และ Home",
          teacher: "ย้ำว่าแขนกลห้องเรียนสั่งด้วยมุม Servo — Teach Point คือวิธีแปลงตำแหน่งที่วางแผนให้เป็นคำสั่งที่เครื่องทำได้จริง",
          note: "หยุดระบบก่อนจัดวางวัตถุหรือแก้ตำแหน่งด้วยมือ และไม่สั่งไปยังจุดที่ไม่แน่ใจว่าอยู่ใน Workspace",
        },
        {
          time: "40–45",
          phase: "สรุปและเชื่อมหน่วยที่ 5",
          activity: "สรุป: พิกัดบอกตำแหน่ง · Workspace ตรวจว่าเข้าถึงได้ · Home คือจุดเริ่ม/กลับ · Teach Point เก็บตำแหน่งเป็นมุม Servo",
          teacher: "ปูหน่วยที่ 5: รู้แล้วว่าต้องไปจุดไหน ต่อไปคือ “เคลื่อนไปอย่างไร”",
        },
        {
          time: "45–50",
          phase: "Exercise 2: Position Error Check",
          activity: "เติมพิกัด Pick/Place ที่หายไป ตรวจ Workspace และหา Error 1 จุดพร้อมแก้ไข",
          teacher: "ใช้แผนภาพชุดใหม่",
        },
      ],
    },
  ],
  guides: [
    {
      id: "three-checks",
      title: "3 Position Checks ก่อนใช้ตำแหน่ง",
      blocks: [
        {
          heading: "ตรวจตามลำดับ",
          flow: [
            { label: "แกน / จุดอ้างอิงถูก" },
            { label: "อยู่ใน Workspace" },
            { label: "ชื่อจุดตรงหน้าที่", sub: "Home / Pick / Place" },
          ],
        },
        { icon: "🔀", heading: "Error: X/Y สลับ", compare: "Robot ไปผิดฝั่งของโต๊ะ" },
        { icon: "🚫", heading: "Error: จุดนอก Workspace", compare: "Robot ไปไม่ถึงหรือฝืนกลไก" },
        { icon: "🏠", heading: "Error: ตั้ง Home เป็น Origin โดยไม่มีข้อมูล", compare: "Robot อาจกลับไปท่าที่ไม่ปลอดภัย" },
      ],
    },
    {
      id: "teach-point",
      title: "Teach Point — เชื่อมพิกัดกับมุม Servo",
      blocks: [
        {
          heading: "จากแผนสู่เครื่องจริง",
          flow: [
            { label: "วัดพิกัดบนโต๊ะ", sub: "(X, Y, Z)" },
            { label: "ขยับแขนไปที่จุด", sub: "ความเร็วต่ำ" },
            { label: "บันทึกมุม Servo" },
            { label: "ใช้ซ้ำได้" },
          ],
        },
        {
          icon: "📋",
          heading: "ตัวอย่างตาราง",
          text: "Pick → (12, 8, 3) → Base 70° · Shoulder 110° · Elbow 95° · Gripper เปิด",
        },
      ],
      tip: "หน่วยที่ 5 จะใช้ Teach Point เหล่านี้ทดสอบ Motion Plan จริงที่ความเร็วต่ำ",
    },
  ],
  assessment: [
    {
      topic: "พิกัด Workspace และ Home",
      method: "ถามตอบ Coordinate Hunt และ Exercise 1",
      criteria: "อธิบายแกน X–Y–Z จุดอ้างอิง Workspace และแยก Home กับ Origin ได้",
    },
    { topic: "อ่านและวัดพิกัด", method: "Coordinate Hunt + วัดตำแหน่งจริง", criteria: "อ่าน/วัดพิกัดได้ และบอกได้ว่าแกนใดเปลี่ยน" },
    {
      topic: "กำหนดและตรวจ Home / Pick / Place (ว 4.2 ป.6/1)",
      method: "Position Table + Exercise 2",
      criteria: "กำหนดจุดได้สอดคล้องกับโต๊ะจริง ตรวจ Workspace และแก้ Error ได้อย่างน้อย 1 จุดพร้อมเหตุผล",
    },
    { topic: "Teach Point", method: "ตาราง Point → พิกัด → มุม Servo", criteria: "บันทึกมุม Servo ของ Pick, Place และ Home ได้ครบ" },
    {
      topic: "การทำงานร่วมกันและความปลอดภัย (ง 1.1 ป.6/2)",
      method: "สังเกตพฤติกรรม",
      criteria: "ทำงานเป็นขั้นตอน ตรวจ Workspace ก่อนใช้ตำแหน่ง และหยุดระบบก่อนแก้ตำแหน่งด้วยมือ",
    },
  ],
  takeaways: [
    "พิกัด (X, Y, Z) ต้องอ่านพร้อมจุดอ้างอิงและทิศของแกน — Workspace บอกว่าจุดไหนไปถึงได้",
    "Origin คือจุดอ้างอิง ส่วน Home คือจุดเริ่ม/กลับที่กำหนด ไม่จำเป็นต้องเป็น (0,0,0)",
    "แขนกลห้องเรียนสั่งด้วยมุม Servo จึงใช้ Teach Point บันทึกตำแหน่ง Pick/Place เป็นชุดมุมที่ใช้ซ้ำได้",
  ],
  bridge: {
    nextLabel: "หน่วยที่ 5: Robot Motion",
    question: "“รู้แล้วว่าต้องไปที่จุดไหน แต่ควรเคลื่อนจากจุดหนึ่งไปอีกจุดหนึ่งอย่างไรให้ปลอดภัยและแม่นยำ?”",
    teaser: "“คาบหน้าเราจะวางแผนการเคลื่อนที่แบบจุดต่อจุด และทดสอบกับ Teach Point ที่ความเร็วต่ำ”",
  },
};

const unit5: LessonPlan = {
  gradeSlug: "p6",
  week: 5,
  unitLabel: "หน่วยที่ 5",
  title: "Robot Motion",
  subtitle: "Point-to-Point, Speed, Accuracy และ Safety",
  minutesPerPeriod: 50,
  summary: [
    "หน่วยที่ 4 ตอบว่า “ต้องไปที่ไหน” หน่วยนี้ตอบว่า “ต้องเคลื่อนตามลำดับอย่างไร” ผ่านการเคลื่อนที่แบบ Point-to-Point (PTP) ที่มองงานเป็นลำดับ Target Points เช่น Home → Pick → Place → Home เส้นทางระหว่างจุดขึ้นกับระบบควบคุม จึงไม่ควรสมมติว่าเป็นเส้นตรงเสมอ ถ้าช่วงไหนเสี่ยงก็เพิ่ม Safe Point/Waypoint และบางงานอาจใช้ Approach/Retreat Point เข้าใกล้และถอยออกจากชิ้นงาน",
    "เด็กเคยทำการเคลื่อนที่แบบนี้มาแล้วตอน ป.5 — หน่วยที่ 8 ใช้ for loop เพิ่มมุมทีละขั้นเพื่อคุมความเร็ว และหน่วยที่ 9 บันทึกท่าแล้ว Playback หน่วยนี้จึงใช้วิธีเดิมทดสอบ Motion Plan บนแขนกลจริงด้วย Teach Point จากหน่วยที่ 4 ที่ความเร็วต่ำ และยึดหลัก “เร็วกว่าไม่ได้แปลว่าดีกว่าเสมอ” — Motion Plan ที่ดีต้องสำเร็จ แม่นยำพอ และปลอดภัย",
  ],
  objectives: [
    "อธิบายการเคลื่อนที่แบบ Point-to-Point และแยก Position กับ Motion ได้",
    "อธิบายบทบาทของ Target Point, ลำดับการเคลื่อนที่, Safe Point/Waypoint, ความเร็ว ความแม่นยำ และความปลอดภัยได้",
    "เรียงและออกแบบลำดับ Point-to-Point จาก Home, Pick, Place และ Safe Point ได้อย่างสมเหตุสมผล",
    "เลือกระดับความเร็วให้เหมาะกับแต่ละช่วง และอธิบายผลต่อความแม่นยำและความปลอดภัยได้",
    "ตรวจและปรับ Motion Plan ที่มีปัญหา และทดสอบบนแขนกลจริงที่ความเร็วต่ำได้",
    "ทำงานเป็นระบบ ทดสอบด้วยความเร็วต่ำก่อน และปฏิบัติตามข้อตกลงด้านความปลอดภัย",
  ],
  standards: [
    {
      ...logicP6,
      core: "ออกแบบลำดับการเคลื่อนที่แบบ Point-to-Point เลือกความเร็วให้เหมาะกับช่วงงาน และตรวจ/ปรับ Motion Plan ให้ปลอดภัย",
    },
  ],
  integrations: [
    { subject: "คณิตศาสตร์", detail: "การเรียงลำดับจุด การเปรียบเทียบความเร็วและตำแหน่ง" },
    { subject: "วิทยาศาสตร์", detail: "การเคลื่อนที่และผลของความเร็วต่อการควบคุมและความปลอดภัย" },
    { subject: "ภาษาไทย", detail: "การอธิบายลำดับการเคลื่อนที่และเหตุผลในการเลือกความเร็ว" },
  ],
  learning: {
    knowledge:
      "Point-to-Point = เคลื่อนจาก Target Point หนึ่งไปอีกจุดตามลำดับ, Position vs Motion, Motion Sequence, Safe Point/Waypoint, Approach/Retreat Point, ความเร็ว Low/Medium/High, Accuracy และหลัก Speed–Accuracy–Safety",
    skills: "การเรียง Target Points การเพิ่ม Safe Point เมื่อมีเหตุผล การเลือกความเร็วตามช่วงงาน การเปรียบเทียบ Motion Plans การตรวจแก้ Motion Error และการทดสอบที่ความเร็วต่ำ",
    attitude: "ให้ความสำคัญกับความปลอดภัยมากกว่าความเร็ว และทดสอบอย่างระมัดระวังก่อนใช้งานจริง",
  },
  vocabulary: [
    { term: "Point-to-Point (PTP)", meaning: "การเคลื่อนจาก Target Point หนึ่งไปยังอีกจุดตามลำดับ" },
    { term: "Target Point", meaning: "จุดเป้าหมายที่ปลายแขนต้องไปถึง" },
    { term: "Safe Point / Waypoint", meaning: "จุดผ่านที่เพิ่มเพื่อเลี่ยงพื้นที่เสี่ยง" },
    { term: "Approach / Retreat", meaning: "จุดเข้าใกล้ก่อนทำงาน / จุดถอยออกหลังทำงาน" },
    { term: "Accuracy", meaning: "ความใกล้เคียงของตำแหน่งจริงกับตำแหน่งเป้าหมาย" },
  ],
  materials: [
    "Robot Arm ของห้องเรียน พร้อม Teach Point (Home / Pick / Place) ที่บันทึกไว้จากหน่วยที่ 4",
    "ค่าความเร็ว Low / Medium / High ของแขนกลโรงเรียน — ครูกำหนดและทดสอบล่วงหน้าเป็นขนาดก้าวมุมและ delay ใน for loop แบบที่เรียนใน ป.5 หน่วยที่ 8 (ตัวอย่างเริ่มต้น: Low = เพิ่มทีละ 1° แล้วรอสั้น ๆ ทุกก้าว)",
    "วัตถุ Pick–Place · สิ่งกีดขวางจำลอง/บล็อก · ปุ่มหยุดหรือจุดปิดระบบที่เข้าถึงง่าย",
    "Motion Sequence Cards · Target Point Map · Speed Cards · Safe Point Cards · Approach/Retreat Challenge Card · Motion Plan Table · Motion Error Cards",
    "ใบงาน Exercise 1: Motion Sequence Detective และ Exercise 2: Motion Plan Check",
  ],
  sessions: [
    {
      number: 1,
      title: "Point-to-Point & Motion Sequence — จากตำแหน่งสู่การเคลื่อนที่",
      minutes: 50,
      goal: "นักเรียนอธิบาย Point-to-Point แยก Position กับ Motion เรียง Target Points เป็น Motion Sequence และเลือกใช้ Safe Point เมื่อจำเป็น",
      steps: [
        {
          time: "0–5",
          phase: "เชื่อมจากหน่วยที่ 4 และ ป.5",
          activity: "ดู Position Table และ Teach Point จากหน่วยที่ 4 แล้วนึกถึง ป.5 หน่วยที่ 9 ที่เคยบันทึกท่าแล้ว Playback ตามลำดับ",
          teacher: "ถาม “รู้แล้วว่าต้องไปจุดไหน แต่ควรไปจุดไหนก่อน–หลัง?”",
        },
        {
          time: "5–11",
          phase: "Position vs Motion",
          activity: "เปรียบเทียบรายการพิกัด Home/Pick/Place กับลูกศรลำดับ Home → Pick → Place → Home",
          teacher: "Position = อยู่ที่ไหน · Motion = ไปตามลำดับใด",
        },
        {
          time: "11–18",
          phase: "Point-to-Point & Target Points",
          activity: "ติดตาม A → B → C และเปรียบเทียบกับการข้ามจุดหรือสลับลำดับ",
          teacher: "ไม่สมมติว่าเส้นทางระหว่างสองจุดเป็นเส้นตรงเสมอ",
        },
        {
          time: "18–24",
          phase: "Motion Sequence — ลำดับจุดสำคัญ",
          activity: "จัด Point Cards เป็นลำดับภารกิจ แล้วเปรียบเทียบกับลำดับผิด เช่น Place ก่อน Pick",
          teacher: "จุดเดียวกันแต่ลำดับต่าง อาจทำให้งานไม่สำเร็จ",
        },
        {
          time: "24–30",
          phase: "Safe Point / Waypoint และ Approach / Retreat",
          activity: "เปรียบเทียบแผนตรงกับแผนที่เพิ่ม Safe Point ก่อนผ่านสิ่งกีดขวาง และดูตัวอย่าง Approach/Retreat",
          teacher: "เพิ่มจุดเมื่อช่วยให้ปลอดภัยขึ้น ไม่จำเป็นต้องเพิ่มทุกภารกิจ",
        },
        {
          time: "30–42",
          phase: "Activity: Point-to-Point Route Builder",
          activity: "จาก Motion Map ที่มีสิ่งกีดขวาง เรียง Target Point Cards ให้ภารกิจสำเร็จ ตัดสินใจว่าจะเพิ่ม Safe Point หรือไม่ พร้อมเหตุผล — กลุ่มที่เร็วรับ Approach/Retreat Challenge",
          teacher: "ถาม “ช่วงไหนเสี่ยงชน?” “Safe Point นี้จำเป็นจริงไหม?”",
        },
        {
          time: "42–45",
          phase: "สรุป Point-to-Point Motion",
          activity: "สรุป Target Points ตามลำดับ และ Safe Point ใช้เมื่อช่วยลดความเสี่ยง",
          teacher: "เชื่อมคาบ 2: ต้องเลือก “เร็วแค่ไหน” ด้วย",
        },
        {
          time: "45–50",
          phase: "Exercise 1: Motion Sequence Detective",
          activity: "หา Error ของลำดับ 1 จุดและแก้ ระบุช่วงเสี่ยง และตัดสินว่า Safe Point ที่ให้มา “จำเป็น / ไม่จำเป็น / ควรย้าย”",
          teacher: "ไม่ต้องออกแบบแผนใหม่ทั้งชุด",
        },
      ],
    },
    {
      number: 2,
      title: "Speed, Accuracy & Safety — เร็วแค่ไหนจึงเหมาะ?",
      minutes: 50,
      goal: "นักเรียนเลือกความเร็วให้เหมาะกับแต่ละช่วง อธิบาย Speed–Accuracy–Safety และทดสอบ Motion Plan บนแขนกลจริงที่ความเร็วต่ำ",
      steps: [
        {
          time: "0–5",
          phase: "ทบทวน Motion Sequence",
          activity: "ดู Home → Safe Point → Pick → Place → Home แล้วระบุช่วงที่ต้องเข้าใกล้ชิ้นงานอย่างระมัดระวัง",
          teacher: "Motion Plan ยังไม่ครบจนกว่าจะเลือกความเร็ว",
        },
        {
          time: "5–12",
          phase: "Speed Setting — เร็วไม่เท่ากันทุกช่วง",
          activity: "เปรียบเทียบ Speed Cards Low / Medium / High กับช่วงพื้นที่โล่ง เข้าใกล้ Pick และเข้าใกล้ Place",
          teacher: "บอกให้ชัดว่า Low/Medium/High บนแขนกลของเราคือก้าวมุมและ delay เท่าไร — แบบเดียวกับ for loop ใน ป.5 หน่วยที่ 8",
        },
        {
          time: "12–20",
          phase: "Accuracy",
          activity: "ดูตัวอย่างปลายแขนหยุดตรง Target, ใกล้ Target และเลย Target แล้วอภิปรายว่าแบบใดทำให้งานสำเร็จ",
          teacher: "“เร็ว = ไม่แม่น” ไม่ใช่กฎเสมอ แต่ความเร็วเกินความเหมาะสมทำให้หยุดใกล้ชิ้นงานยากขึ้น",
        },
        {
          time: "20–27",
          phase: "Safety First — ทดสอบช้าก่อน",
          activity: "ใช้ Safety Check กับ Motion Plan ใหม่ แล้วเปรียบเทียบแผน A (เร็วแต่เสี่ยง) กับแผน B (ช้ากว่าแต่เหมาะกับงาน)",
          teacher: "Model “Check → Low-speed Test → Observe → Adjust” และไม่ให้แข่งความเร็ว",
        },
        {
          time: "27–40",
          phase: "Activity: Speed–Accuracy–Safety Mission (ทดสอบจริง)",
          activity:
            "เลือก Low/Medium/High ให้แต่ละช่วง ตรวจว่าต้องเพิ่ม Safe Point หรือไม่ แล้วทดสอบลำดับ Teach Point บนแขนกลจริงที่ความเร็ว Low ด้วยวิธีที่เคยใช้ใน ป.5 (for loop / Playback) บันทึกว่าไปถึง Target ตรงไหม",
          teacher: "ถาม “ช่วงไหนต้องการความแม่นยำมาก?” “แผนที่เร็วกว่าเหมาะกว่าเสมอไหม?” — ทดสอบเฉพาะที่ความเร็วต่ำตามที่ครูกำหนด",
          note: "เว้นมือและร่างกายออกจากพื้นที่เคลื่อนที่ และหยุดระบบก่อนจัดวางวัตถุหรือแก้ตำแหน่งด้วยมือ",
        },
        {
          time: "40–45",
          phase: "สรุปและเชื่อมหน่วยที่ 6",
          activity: "สรุปว่า Motion Plan ที่ดี = ลำดับ Target Points + ความเร็วที่เหมาะ + Safety Check",
          teacher: "ปูหน่วยที่ 6 Industrial Sensor และบอกว่าหน่วยที่ 18 Cycle Time จะกลับมาคิดเรื่องความเร็วกับประสิทธิภาพอีกครั้ง",
        },
        {
          time: "45–50",
          phase: "Exercise 2: Motion Plan Check",
          activity: "เลือกช่วงที่ความเร็วไม่เหมาะ 1 จุดและปรับ ระบุความเสี่ยง 1 ข้อ และแก้ Motion Plan พร้อมเหตุผล",
          teacher: "ตรวจว่าใช้เหตุผล Speed–Accuracy–Safety",
        },
      ],
    },
  ],
  guides: [
    {
      id: "motion-plan",
      title: "Motion Plan ที่ดี",
      blocks: [
        {
          heading: "ลำดับตัวอย่าง",
          flow: [
            { label: "Home" },
            { label: "Safe Point" },
            { label: "Approach", sub: "ช้า" },
            { label: "Pick" },
            { label: "Retreat" },
            { label: "Place", sub: "ช้า" },
            { label: "Home" },
          ],
        },
        {
          icon: "🐢",
          heading: "ทดสอบช้าก่อนเสมอ",
          flow: [{ label: "Check" }, { label: "Low-speed Test" }, { label: "Observe" }, { label: "Adjust" }],
        },
      ],
      tip: "Safe Point และ Approach/Retreat ใช้เมื่อมีเหตุผล ไม่ต้องเพิ่มทุกภารกิจ — และ Motion Plan ที่ดีไม่จำเป็นต้องเร็วที่สุด",
    },
    {
      id: "speed-levels",
      title: "ความเร็วกับแขนกลของโรงเรียน",
      blocks: [
        { icon: "🐢", heading: "Low", text: "ก้าวมุมเล็ก รอนาน — ใช้ตอนทดสอบครั้งแรกและเข้าใกล้ Pick/Place" },
        { icon: "🚶", heading: "Medium", text: "ใช้ในพื้นที่โล่งเมื่อทดสอบลำดับผ่านแล้ว" },
        { icon: "🏃", heading: "High", text: "ใช้เฉพาะเมื่อปลอดภัยและงานต้องการ — ไม่ใช่เป้าหมายของหน่วยนี้" },
      ],
      tip: "ครูกำหนดค่าก้าวมุมและ delay ของแต่ละระดับให้ชัดก่อนสอน เพื่อให้กิจกรรมทำบนเครื่องจริงได้",
    },
  ],
  assessment: [
    {
      topic: "Point-to-Point และ Speed–Accuracy–Safety",
      method: "ถามตอบ Route Builder, Mission และ Exercise 1–2",
      criteria: "อธิบาย Point-to-Point, Position/Motion, Safe Point และหลัก Speed–Accuracy–Safety ได้",
    },
    {
      topic: "ออกแบบ Motion Sequence (ว 4.2 ป.6/1)",
      method: "Route Builder + Exercise 1",
      criteria: "เรียง Target Points ได้ถูกลำดับ และเพิ่ม Safe Point เมื่อมีเหตุผล",
    },
    {
      topic: "เลือกความเร็วและปรับแผน",
      method: "Speed–Accuracy–Safety Mission + Exercise 2",
      criteria: "เลือกความเร็วเหมาะกับช่วงงาน อธิบายผล และปรับ Motion Plan ได้อย่างน้อย 1 จุด",
    },
    {
      topic: "การทดสอบจริงและความปลอดภัย",
      method: "สังเกตการทดสอบบนแขนกลจริง",
      criteria: "ทดสอบที่ความเร็วต่ำ เว้นมือออกจากพื้นที่เคลื่อนที่ และหยุดระบบก่อนแก้ตำแหน่ง",
    },
  ],
  takeaways: [
    "Point-to-Point คือการเคลื่อนไปยัง Target Points ตามลำดับ — Position บอกว่าไปที่ไหน Motion บอกว่าไปอย่างไร",
    "เส้นทางระหว่างจุดไม่จำเป็นต้องเป็นเส้นตรง ช่วงเสี่ยงเพิ่ม Safe Point ได้ และบางงานใช้ Approach/Retreat",
    "เร็วกว่าไม่ได้แปลว่าดีกว่าเสมอ — Check → Low-speed Test → Observe → Adjust ก่อนใช้งานจริง",
  ],
  bridge: {
    nextLabel: "หน่วยที่ 6: Industrial Sensor",
    question: "“Robot เคลื่อนตามจุดได้แล้ว แต่ถ้าชิ้นงานยังมาไม่ถึง หรือวางผิดตำแหน่ง Robot จะรู้ได้อย่างไร?”",
    teaser: "“คาบหน้าเราจะใช้ Sensor ในโรงงานช่วยให้ระบบรับรู้สิ่งรอบตัว”",
  },
};

export const p6LessonPlans: LessonPlan[] = [unit1, unit2, unit3, unit4, unit5];
