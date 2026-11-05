// lib/lesson-plans/p3.ts
// แผนการสอน ป.3 สัปดาห์ที่ 1–3 และ 20 (หน่วยการเรียนรู้ที่ 1–3, 20)
// ที่มา: แผนการจัดการเรียนรู้ ป.3 (Google Docs) ปรับตามคอมเมนต์คอลัมน์ K ใน Course Plan

import type { LessonPlan } from "./types";

const algorithmP3 = {
  code: "ว 4.2 ป.3/1",
  indicator: "แสดงอัลกอริทึมในการทำงานหรือการแก้ปัญหาอย่างง่าย โดยใช้ภาพ สัญลักษณ์ หรือข้อความ",
};

const programP3 = {
  code: "ว 4.2 ป.3/2",
  indicator: "เขียนโปรแกรมอย่างง่าย โดยใช้ซอฟต์แวร์หรือสื่อ และตรวจหาข้อผิดพลาดของโปรแกรม",
};

const unit1: LessonPlan = {
  gradeSlug: "p3",
  week: 1,
  unitLabel: "หน่วยที่ 1",
  title: "Intro Coding & Control",
  subtitle: "Coding, Arduino และลำดับการทำงานของระบบ",
  minutesPerPeriod: 50,
  summary: [
    "Coding คือการกำหนดคำสั่งให้ระบบทำงาน และคำสั่งต้องมีลำดับที่ชัดเจน เพราะการสลับลำดับอาจทำให้ผลลัพธ์เปลี่ยนไป เด็กเคยใช้ Sequence แบบนี้มาแล้ว ทั้งบัตรคำสั่งตอน ป.1 และการวางลำดับท่าใน Zide ตอน ป.2 — วันนี้คือแนวคิดเดียวกัน แต่ให้ Controller เป็นคนทำตาม",
    "Arduino Board ทำหน้าที่เป็น Controller หรือ “สมองของระบบ” ที่รัน Program ตามคำสั่งที่มนุษย์กำหนด โดย Board ไม่ได้คิดเอง ระบบอย่างง่ายมองเป็น Input → Process → Output หน่วยนี้เน้นให้เข้าใจลำดับการทำงานก่อนเข้าสู่ Mixly และการเขียน Program จริงในหน่วยถัดไป",
  ],
  objectives: [
    "อธิบายด้วยภาษาของตนเองว่า Coding คือการกำหนดคำสั่ง และ Sequence คือลำดับก่อน–หลังของคำสั่ง",
    "อธิบายความหมายของ Input → Process → Output และบอกหน้าที่ของแต่ละส่วนจากตัวอย่างง่าย ๆ ได้",
    "อธิบายได้ว่า Arduino Board เป็น Controller ที่รัน Program ตามลำดับคำสั่งเพื่อประมวลผล Input และควบคุม Output",
    "จำแนก Input, Arduino/Process และ Output จากสถานการณ์ระบบอย่างง่ายได้",
    "เรียง System Flow ด้วยบัตร และเรียงคำสั่งสั้น ๆ 3–4 ขั้นให้ Controller ทำงานตามลำดับได้",
    "ร่วมสังเกตและทดลอง รับฟังเพื่อน และใช้อุปกรณ์อย่างระมัดระวัง",
  ],
  standards: [
    { ...algorithmP3, core: "เรียงและสื่อสารลำดับคำสั่งอย่างง่าย พร้อมตรวจว่าลำดับสอดคล้องกับเป้าหมายหรือไม่" },
    { ...programP3, core: "เข้าใจว่าการเขียนโปรแกรมคือการสร้างลำดับคำสั่ง และฝึกตรวจ/แก้ลำดับคำสั่งผ่านบัตรคำสั่งและการสาธิต" },
  ],
  integrations: [
    { subject: "ภาษาไทย", detail: "ท 3.1 ป.3/2 บอกสาระสำคัญจากการฟังและการดู ผ่านการสังเกตการสาธิตและสรุปการทำงาน" },
    { subject: "คณิตศาสตร์", detail: "การเรียงลำดับก่อน–หลังและความสัมพันธ์ของขั้นตอน" },
  ],
  learning: {
    knowledge:
      "Coding และ Sequence, คำสั่งต้องมีลำดับก่อน–หลัง, Arduino Board ในฐานะ Controller, แนวคิด Input → Process → Output และ Program คือชุดคำสั่งที่กำหนดการทำงานของ Controller",
    skills: "การสังเกต การเรียงลำดับคำสั่ง การเปรียบเทียบผลเมื่อสลับลำดับ การจำแนก Input/Process/Output และการอธิบายระบบด้วยภาพหรือบัตร",
    attitude: "สนใจทดลอง กล้าบอกเหตุผล ยอมรับว่าคำสั่งอาจต้องแก้ไข และใช้ Hardware อย่างปลอดภัย",
  },
  vocabulary: [
    { term: "Coding", meaning: "การกำหนดคำสั่งให้ระบบทำงาน" },
    { term: "Sequence", meaning: "ลำดับก่อน–หลังของคำสั่ง" },
    { term: "Controller", meaning: "ตัวควบคุมที่รัน Program — Arduino" },
    { term: "Input / Process / Output", meaning: "สิ่งที่เข้า / การทำงานตาม Program / สิ่งที่เกิดออกมา" },
  ],
  materials: [
    "ชุดสาธิต Button → Arduino → LED ของครู 1 ชุด ต่อวงจรและอัปโหลด Program ไว้ล่วงหน้า (ยังไม่สอน Mixly ในคาบนี้)",
    "บัตรคำสั่ง Sequence 3–4 ขั้น · บัตร Input / Arduino-Process / Output · บัตรลูกศร System Flow กลุ่มละ 1 ชุด",
    "ภาพระบบใกล้ตัว 3 แบบ เช่น รีโมตของเล่น เครื่องกดสบู่อัตโนมัติ ไฟเตือน",
    "ใบงาน Exercise 1: System Flow Detective",
  ],
  sessions: [
    {
      number: 1,
      title: "Coding, Arduino และลำดับการทำงานของระบบ",
      minutes: 50,
      goal: "นักเรียนเข้าใจว่า Coding คือการกำหนดคำสั่งตามลำดับ รู้บทบาทของ Arduino ในฐานะ Controller และอธิบาย Input → Process → Output ของระบบง่าย ๆ ได้",
      steps: [
        {
          time: "0–5",
          phase: "นำเข้าสู่บทเรียน",
          activity: "ชม Arduino Board ตัวจริงและการสาธิต LED ที่ทำงานตาม Program แล้วสังเกตว่า Board อยู่ระหว่าง “คำสั่ง” กับ “สิ่งที่เกิดขึ้น”",
          teacher: "ถาม “ถ้า LED คือสิ่งที่ทำงาน แล้วอะไรเป็นตัวควบคุมมัน?” “Arduino รู้เองไหม หรือมี Program บอก?”",
        },
        {
          time: "5–12",
          phase: "Coding & Sequence — สิ่งที่เราเคยทำแล้ว",
          activity: "เรียงบัตรคำสั่ง 3–4 ขั้น เช่น เริ่ม → เปิดไฟ → รอ → ปิดไฟ แล้วลองสลับบัตร 1 ใบดูว่าผลเปลี่ยนไหม",
          teacher: "เชื่อมความรู้เดิม: “บัตรคำสั่งตอน ป.1 และการวางลำดับท่าใน Zide ตอน ป.2 ก็คือ Sequence แบบเดียวกัน” ไม่สอนซ้ำจากศูนย์",
        },
        {
          time: "12–20",
          phase: "Arduino = Controller",
          activity: "วางป้าย “Controller” ตรงกลางระบบ และเชื่อมว่า Program ที่เราเรียงคำสั่งไว้ถูกรันบน Arduino",
          teacher: "ใช้คำว่า “สมองของระบบ” เป็นตัวช่วยเห็นภาพ แต่ย้ำว่า Board ไม่ได้คิดเอง",
        },
        {
          time: "20–30",
          phase: "รู้จัก Input → Process → Output",
          activity: "ใช้บัตร 3 ช่องกับตัวอย่าง กดปุ่ม → Arduino ทำตาม Program → LED ติด แล้วช่วยกันบอกแต่ละส่วน",
          teacher: "Input = สิ่งที่เข้า · Process = Arduino ทำงานตาม Program ตามลำดับ · Output = สิ่งที่เกิดออกมา",
        },
        {
          time: "30–42",
          phase: "System Flow + Sequence Detective",
          activity:
            "แต่ละกลุ่มเรียงบัตร Input → Arduino/Process → Output และเรียงบัตร Program 3–4 ขั้น คาดเดาผล แล้วนำบัตรมาให้ครูรันที่ชุดสาธิตหน้าห้องทีละกลุ่ม และลองสลับคำสั่ง 1 ใบเพื่อเปรียบเทียบผล",
          teacher: "ถาม “อะไรเกิดก่อน–หลัง?” “ถ้าเปลี่ยนลำดับ ผลจะเหมือนเดิมไหม?” ยังไม่สอนการต่อ Button หรือ Mixly",
          note: "มีชุดสาธิต 1 ชุด จึงให้กลุ่มเรียงบัตรแล้วครูเป็นคนรันให้ดู ถ้ามีชุดเพียงพอ (กลุ่มละ 1 ชุด) ให้กลุ่มกดทดลองเองได้",
        },
        {
          time: "42–50",
          phase: "สรุปและ Exit Ticket",
          activity: "ทำ System Flow Detective 1 สถานการณ์ แล้วเติมประโยค “Arduino เป็นสมองของระบบเพราะ…”",
          teacher: "ตรวจว่าเด็กเข้าใจ Coding = คำสั่ง, Sequence = ลำดับ, Arduino = Controller และระบบทำงานเป็น Input → Process → Output",
          note: "ตัดเหลือ 2 งานท้ายคาบ (System Flow + เติมประโยค) เพราะ 8 นาทีไม่พอสำหรับ 3 งาน",
        },
      ],
    },
  ],
  guides: [
    {
      id: "system-flow",
      title: "Exercise 1 — System Flow Detective",
      intro: "ใช้สถานการณ์ใกล้ตัว เช่น กดปุ่มแล้วไฟติด รีโมตสั่งรถของเล่น หรือระบบเตือนอย่างง่าย",
      blocks: [
        {
          heading: "จัดหรือวาด System Flow",
          flow: [
            { label: "Input", sub: "สิ่งที่เข้า" },
            { label: "Arduino / Process", sub: "ทำตาม Program" },
            { label: "Output", sub: "สิ่งที่เกิดออกมา" },
          ],
        },
        {
          icon: "📝",
          heading: "เติมประโยค",
          text: "“Arduino เป็นสมองของระบบเพราะ…”",
        },
      ],
      tip: "ไม่ลงรายละเอียดชนิด Input, Sensor, pin หรือการต่อวงจร ซึ่งจะเรียนในหน่วยถัดไป",
    },
  ],
  assessment: [
    {
      topic: "ความเข้าใจ Coding, Sequence, Arduino และ IPO",
      method: "ถามตอบระหว่างกิจกรรม + Exit Ticket",
      criteria: "อธิบาย Coding, Sequence, บทบาท Controller และความหมาย Input/Process/Output จากตัวอย่างได้",
    },
    {
      topic: "System Flow และลำดับคำสั่ง (ว 4.2 ป.3/1, ป.3/2)",
      method: "System Flow + Sequence Detective และ Exercise 1",
      criteria: "เรียง Input → Arduino/Process → Output ได้ถูกลำดับ และเรียงคำสั่ง 3–4 ขั้นได้สอดคล้องกับผล",
    },
    {
      topic: "การมีส่วนร่วมและความปลอดภัย",
      method: "สังเกตพฤติกรรม",
      criteria: "ร่วมทดลอง รับฟังเพื่อน ปรับคำสั่งเมื่อจำเป็น และปฏิบัติตามข้อตกลงด้านความปลอดภัย",
    },
  ],
  takeaways: [
    "Coding คือการกำหนดคำสั่ง และ Sequence คือการจัดคำสั่งตามลำดับก่อน–หลัง",
    "Arduino Board เป็น Controller ที่รัน Program ตามลำดับคำสั่งที่มนุษย์กำหนด — ไม่ได้คิดเอง",
    "ระบบอย่างง่ายมองเป็น Input → Process → Output และถ้าลำดับคำสั่งเปลี่ยน ผลลัพธ์อาจเปลี่ยน",
  ],
  bridge: {
    nextLabel: "หน่วยที่ 2: Intro MiniProject",
    question: "“ถ้าเรารู้จัก Input → Process → Output แล้ว ปลายเทอมเราจะใช้ความรู้นี้สร้างอะไรได้บ้าง?”",
    teaser: "“คาบหน้าเราจะดูตัวอย่าง MiniProject ปลายทาง แล้วเริ่มเก็บ Idea ของตัวเอง”",
  },
};

const unit2: LessonPlan = {
  gradeSlug: "p3",
  week: 2,
  unitLabel: "หน่วยที่ 2",
  title: "Intro MiniProject",
  subtitle: "จากสิ่งที่เรียนสู่สิ่งที่อยากสร้าง",
  minutesPerPeriod: 50,
  summary: [
    "MiniProject คือชิ้นงานปลายทางที่นักเรียนนำความรู้หลายเรื่องมารวมกันเพื่อสร้างระบบตามเป้าหมาย หน่วยนี้ยังไม่ให้ออกแบบ Project ฉบับสมบูรณ์ แต่ช่วยให้เห็น “ภาพปลายทาง” ว่าแต่ละหน่วยที่กำลังจะเรียนคือเครื่องมือที่ค่อย ๆ เพิ่มเข้ามา",
    "นักเรียนดูตัวอย่าง Project เชื่อมกับ Input → Process → Output จากหน่วยที่ 1 แล้วเริ่มสร้าง Idea จากสิ่งที่สนใจหรือปัญหาเล็ก ๆ รอบตัว เก็บ Idea Card ไว้ใน Project Journal แล้วกลับมาทบทวนกลางเทอมหลังหน่วยที่ 12 MiniControl Project ก่อนนำไปสร้างจริงในหน่วยที่ 20",
  ],
  objectives: [
    "อธิบายได้ว่า MiniProject คือการนำความรู้หลายเรื่องมารวมกันเพื่อสร้างชิ้นงานหรือระบบที่มีเป้าหมาย",
    "บอกได้ว่าความรู้ที่จะเรียนต่อ เช่น Sensor, Mixly, Output, Input, Logic เป็น “เครื่องมือ” ที่จะนำมาใช้สร้าง Project",
    "สังเกตตัวอย่าง Project และอธิบายได้ว่ารับข้อมูลหรือเหตุการณ์อะไร และทำอะไรออกมา",
    "ระบุปัญหา ความสนใจ หรือสิ่งที่อยากสร้างอย่างน้อย 1 Idea และบันทึกลง Project Idea Card ได้",
    "วาด Idea Flow 3 ขั้น “เมื่อเกิดอะไรขึ้น → ระบบทำอะไร → ผลที่อยากได้” และอธิบาย Idea สั้น ๆ ได้",
    "เปิดรับ Idea ที่หลากหลาย และเข้าใจว่า Idea แรกเปลี่ยนได้เมื่อเรียนรู้เครื่องมือใหม่",
  ],
  standards: [
    {
      ...algorithmP3,
      core: "แสดงลำดับการทำงานของแนวคิด MiniProject ด้วยภาพหรือข้อความ “เมื่อเกิดอะไรขึ้น → ระบบทำอะไร → เกิดผลอะไร”",
    },
  ],
  integrations: [
    {
      subject: "ภาษาไทย",
      detail: "ท 2.1 ป.3/2 เขียนบรรยายเกี่ยวกับสิ่งใดสิ่งหนึ่งได้ชัดเจน (Project Idea Card) · ท 3.1 ป.3/2 บอกสาระสำคัญจากการฟังและการดู",
    },
    { subject: "ศิลปะ / การออกแบบ", detail: "การสเก็ตช์แนวคิดและสื่อสาร Idea ด้วยภาพอย่างง่าย" },
  ],
  learning: {
    knowledge:
      "ความหมายและบทบาทของ MiniProject, Project มีเป้าหมาย, ความรู้แต่ละหน่วยเป็นเครื่องมือสร้างระบบ, การเชื่อมตัวอย่าง Project กับ Input → Process → Output และ Project เริ่มจาก Problem / Interest / Goal ได้",
    skills: "การวิเคราะห์ตัวอย่าง Project การมองหาปัญหา/ความสนใจ การบันทึก Idea การวาด Idea Flow 3 ขั้น และการสื่อสารความคิดสั้น ๆ",
    attitude: "เปิดรับความคิดหลากหลาย กล้าลองคิดโดยไม่กลัวผิด และเข้าใจว่า Idea พัฒนาได้",
  },
  materials: [
    "ภาพ/คลิปหรือ Prototype ตัวอย่าง MiniProject 3–4 แบบ เช่น ไฟเตือน ประตูเปิด–ปิดอัตโนมัติ เปิดไฟเมื่อมืด",
    "Project Gallery Card · Project Idea Card · Idea Flow Card 3 ช่อง · Sticky Notes",
    "ภาพ Learning Journey ของ ป.3 เรียงตามลำดับสัปดาห์จริง",
    "Project Menu สำหรับนักเรียนที่ยังคิด Idea ไม่ออก และ Project Journal ประจำตัว",
  ],
  sessions: [
    {
      number: 1,
      title: "MiniProject — จากสิ่งที่เรียนสู่สิ่งที่อยากสร้าง",
      minutes: 50,
      goal: "นักเรียนเข้าใจว่า MiniProject คือการรวมความรู้หลายเรื่องเพื่อสร้างระบบ และสร้าง Idea พร้อมลำดับการทำงานอย่างง่ายของตนเอง",
      steps: [
        {
          time: "0–6",
          phase: "เปิดด้วย Project ของจริง",
          activity: "ชม Prototype หรือคลิปสั้น 1–2 แบบ แล้วสังเกตว่า “ชิ้นงานนี้ช่วยอะไร?” “เกิดอะไรขึ้นก่อน?” “สุดท้ายระบบทำอะไร?”",
          teacher: "ย้ำว่าตัวอย่างเป็นภาพปลายทาง ไม่ใช่แบบที่ทุกคนต้องทำตาม",
        },
        {
          time: "6–14",
          phase: "MiniProject คืออะไร?",
          activity: "สรุปร่วมกันว่า MiniProject คือการรวมความรู้หลายเรื่องเพื่อสร้างระบบตามเป้าหมาย แล้วดูตัวอย่าง 1 ชิ้นผ่าน Input → Process → Output",
          teacher: "ย้ำว่าวันนี้ยังไม่ต้องรู้ว่าใช้อุปกรณ์อะไรหรือเขียนโค้ดอย่างไร",
        },
        {
          time: "14–22",
          phase: "เปิดกล่องเครื่องมือของเรา",
          activity:
            "ดู Learning Journey ตามลำดับจริงของเทอม: Sensor ภาพรวม → Mixly → Output → Input → Loop / Sound / Servo → MiniControl Project → เจาะ Sensor ทีละตัว → Logic → Automatic System → MiniProject แล้วจับคู่ว่าเครื่องมือแต่ละอย่างช่วย Project อย่างไร",
          teacher: "อธิบายระดับภาพรวม ไม่ลงรายละเอียดหน่วยที่ยังไม่เรียน",
          note: "ภาพ Learning Journey ต้องเรียงตรงกับลำดับสัปดาห์ในหลักสูตร — หน่วยถัดไปคือ Sensor คืออะไร ก่อนเข้า Mixly และ Output",
        },
        {
          time: "22–33",
          phase: "Project Gallery Challenge",
          activity: "ทำงานกลุ่มกับ Project Gallery Card 2–3 ตัวอย่าง ตอบว่า Project มีเป้าหมายอะไร อะไรเป็นเหตุการณ์เริ่มต้น และระบบทำอะไรออกมา",
          teacher: "ให้เด็กอธิบายด้วยภาษาของตนเองได้ ไม่บังคับศัพท์เทคนิคทุกครั้ง",
        },
        {
          time: "33–44",
          phase: "Idea Hunt + Idea Flow",
          activity: "เลือก 1 สิ่งที่อยากช่วย อยากเตือน หรืออยากสร้าง กรอก Project Idea Card และวาด Idea Flow “เมื่อ… → ระบบจะ… → ผลที่อยากได้…”",
          teacher: "ช่วยคิดจาก Goal ก่อน ไม่บังคับเลือก Sensor หรือ Output ให้ถูก ใช้ Project Menu ช่วยเมื่อคิดไม่ออก",
        },
        {
          time: "44–50",
          phase: "Share, Park & Exit Ticket",
          activity: "เล่า Idea 1 ประโยค เก็บ Card ลง Project Journal และตอบ “ตอนนี้ฉันมี Idea อะไร และต้องเรียนอะไรเพิ่มก่อนสร้างจริง?”",
          teacher: "บอกว่าเราจะกลับมาเปิด Idea Card อีกครั้งหลังหน่วยที่ 12 MiniControl Project เพื่อปรับตามเครื่องมือที่เรียนมา",
        },
      ],
    },
  ],
  guides: [
    {
      id: "idea-card",
      title: "Exercise 1 — Project Idea Card + Idea Flow",
      blocks: [
        {
          icon: "💡",
          heading: "Project Idea Card",
          text: "“ฉันสังเกตว่า… / ฉันอยากลองทำ… / สิ่งนี้น่าจะช่วย…”",
        },
        {
          heading: "Idea Flow 3 ช่อง",
          flow: [
            { label: "เมื่อ…", sub: "เหตุการณ์เริ่มต้น" },
            { label: "ระบบจะ…", sub: "สิ่งที่ระบบทำ" },
            { label: "ผลที่อยากได้…" },
          ],
        },
      ],
      tip: "ไม่ต้องระบุวงจร Sensor หรือ Logic ให้ครบ และไม่ต้องตัดสินใจว่านี่คือ Project สุดท้าย",
    },
    {
      id: "idea-checkpoints",
      title: "เส้นทางของ Idea Card ตลอดเทอม",
      blocks: [
        {
          heading: "Idea ไม่ได้ถูกทิ้งไว้ 15 สัปดาห์",
          flow: [
            { label: "หน่วยที่ 2", sub: "เริ่ม Idea" },
            { label: "หน่วยที่ 12", sub: "กลับมาทบทวนและปรับ" },
            { label: "หน่วยที่ 20", sub: "สร้างจริง" },
          ],
        },
      ],
      tip: "ท้ายหน่วยที่ 12 MiniControl Project ให้เด็กเปิด Idea Card ดูอีกครั้ง แล้วเติมว่าตอนนี้รู้จัก Output / Input / Servo อะไรแล้วบ้างที่ช่วย Idea นี้ได้",
    },
  ],
  assessment: [
    {
      topic: "เข้าใจ MiniProject และ Learning Journey",
      method: "ถามตอบและการอธิบายจากตัวอย่าง",
      criteria: "อธิบายได้ว่า MiniProject รวมความรู้หลายเรื่อง และบอกได้อย่างน้อย 1 ตัวอย่างว่าเครื่องมือที่จะเรียนช่วย Project อย่างไร",
    },
    {
      topic: "วิเคราะห์ตัวอย่างและสร้าง Idea",
      method: "Project Gallery Card + Project Idea Card",
      criteria: "อธิบายตัวอย่าง Project อย่างง่ายได้ และระบุ Idea ของตนเองอย่างน้อย 1 เรื่อง",
    },
    {
      topic: "Idea Flow (ว 4.2 ป.3/1)",
      method: "Idea Flow Card + การเล่าสั้น ๆ",
      criteria: "วาด Idea Flow เป็นลำดับอย่างง่าย และบอกได้ว่าอยากให้เกิดผลอะไร",
    },
    {
      topic: "เปิดรับและพัฒนา Idea",
      method: "สังเกตพฤติกรรม",
      criteria: "รับฟังเพื่อน กล้าปรับ Idea และยอมรับว่าความคิดแรกเปลี่ยนได้",
    },
  ],
  takeaways: [
    "MiniProject คือการนำความรู้หลายหน่วยมารวมกันเพื่อสร้างระบบตามเป้าหมาย",
    "แต่ละหน่วยต่อจากนี้จะเพิ่ม “เครื่องมือ” ใหม่ เช่น Sensor, Output, Input, Logic และ Control",
    "Idea Card ที่เก็บไว้วันนี้จะกลับมาทบทวนกลางเทอม และพัฒนาเป็น Project จริงในหน่วยที่ 20",
  ],
  bridge: {
    nextLabel: "หน่วยที่ 3: Sensor คืออะไร",
    question: "“ถ้าอยากให้ระบบรู้ว่ามืดหรือมีคนเดินมา ระบบจะรับข้อมูลจากรอบตัวได้อย่างไร?”",
    teaser: "“คาบหน้าเราจะรู้จัก Sensor เครื่องมือแรกที่ช่วยให้ระบบรับข้อมูลจากสิ่งรอบตัว”",
  },
};

const unit3: LessonPlan = {
  gradeSlug: "p3",
  week: 3,
  unitLabel: "หน่วยที่ 3",
  title: "Sensor คืออะไร",
  subtitle: "ระบบรับข้อมูลจากสิ่งรอบตัว — ภาพรวม",
  minutesPerPeriod: 50,
  summary: [
    "หน่วยนี้เป็น “ภาพรวม” ของ Sensor ก่อนที่เด็กจะเขียน Mixly และก่อนหน่วยที่ 13–16 ที่เจาะ Sensor ทีละตัว Input คือสิ่งที่ระบบรับเข้า อาจมาจากผู้ใช้ เช่น กดปุ่ม หรือจากสิ่งรอบตัว เช่น แสง ระยะ อุณหภูมิ Sensor จึงเป็นอุปกรณ์ Input ชนิดหนึ่งที่ช่วยให้ระบบรับสิ่งที่เกิดขึ้นใน Environment",
    "เชื่อมกับ ป.1 หน่วย Hello Robot ที่เคยเรียนว่า Sensor คือ “ตา/หู” ของ Robot ตอนนี้ลงลึกขึ้นว่า Sensor ตรวจจับหรือวัด แล้วส่งสัญญาณเข้าให้ Arduino โดยไม่ได้คิดหรือตัดสินใจเอง และรู้จัก Digital (สถานะชัดเจน) กับ Analog (ค่าหลายระดับ) ในระดับเบื้องต้น ซึ่งจะกลับมาใช้อีกในหน่วย Digital Output และ Digital Input",
  ],
  objectives: [
    "อธิบายได้ว่า Input คือสิ่งที่ระบบรับเข้า และยกตัวอย่าง Input จากผู้ใช้หรือสิ่งรอบตัวได้",
    "อธิบายได้ว่า Sensor เป็นอุปกรณ์ Input ที่ตรวจจับหรือวัดสิ่งจาก Environment แล้วส่งข้อมูลให้ Controller โดยไม่ได้คิดเอง",
    "ยกตัวอย่าง Sensor ระยะ แสง IR และอุณหภูมิ พร้อมบอกว่าแต่ละชนิดตรวจจับอะไร",
    "แยก Digital และ Analog จากตัวอย่างง่าย ๆ ได้",
    "จับคู่สถานการณ์กับ Sensor ที่เหมาะสม และเรียง System Flow ได้",
    "คาดเดา ทดลอง และสังเกตได้ว่าเมื่อ Environment เปลี่ยน ค่าที่ Sensor อ่านได้อาจเปลี่ยนตาม",
  ],
  standards: [
    {
      ...algorithmP3,
      core: "เรียง System Flow ของระบบ Sensor อย่างง่าย Environment → Sensor (Input) → Arduino/Program (Process) → Output",
    },
    {
      code: "ท 3.1 ป.3/3",
      indicator: "ตั้งคำถามและตอบคำถามเกี่ยวกับเรื่องที่ฟังและดู",
      core: "ตอบคำถาม อธิบายหน้าที่ของ Input/Sensor และแลกเปลี่ยนสิ่งที่สังเกตจากการทดลอง",
    },
  ],
  integrations: [{ subject: "ภาษาไทย", detail: "ท 3.1 ป.3/6 มีมารยาทในการฟัง การดู และการพูด" }],
  learning: {
    knowledge:
      "Input จากผู้ใช้/Environment, Sensor เป็น Input ชนิดหนึ่ง, Environment → Sensor → ข้อมูล → Controller, Sensor ไม่ได้คิดเอง, Distance / Light / IR / Temperature Sensor, Digital และ Analog เบื้องต้น และ System Flow",
    skills: "การจำแนก Input/Output การจับคู่สิ่งที่ต้องตรวจจับกับ Sensor การ Predict ก่อนทดลอง การเปรียบเทียบก่อน–หลัง และการเรียง System Flow",
    attitude: "สนใจทดลอง รับฟังผลจริง ใช้อุปกรณ์อย่างระมัดระวัง และไม่สรุปว่าอุปกรณ์ “คิดเอง”",
  },
  vocabulary: [
    { term: "Input", meaning: "สิ่งที่ระบบรับเข้า" },
    { term: "Sensor", meaning: "อุปกรณ์ Input ที่ตรวจจับหรือวัดสิ่งรอบตัว" },
    { term: "Environment", meaning: "สิ่งแวดล้อมรอบระบบ" },
    { term: "Digital", meaning: "สถานะชัดเจน เช่น มี/ไม่มี" },
    { term: "Analog", meaning: "ค่าเปลี่ยนได้หลายระดับ เช่น สว่างมาก–น้อย" },
  ],
  materials: [
    "Sensor 4 ชนิด: Distance/Ultrasonic, Light/LDR, IR, Temperature พร้อม Arduino ที่เตรียมโปรแกรมไว้ (เลือก 1 ชนิดทดลองจริง)",
    "Sensor Gallery Card 4 ใบ — เก็บไว้ใช้อ้างอิงอีกครั้งในหน่วยที่ 13–16",
    "Slide, Input–Output Card, Predict–Observe Card, System Flow Card และหน้าจอ Serial Monitor",
    "ใบงาน Exercise 1: Sensor Detective (2 ส่วน)",
  ],
  sessions: [
    {
      number: 1,
      title: "Sensor — ระบบรับข้อมูลจากสิ่งรอบตัว",
      minutes: 50,
      goal: "นักเรียนเชื่อมจาก Input ไปสู่ Sensor รู้จัก Sensor หลัก 4 แบบ สังเกตการเปลี่ยนแปลงของค่า จำแนก Digital/Analog เบื้องต้น และเรียง System Flow ได้",
      steps: [
        {
          time: "0–7",
          phase: "ปูพื้นฐาน Input",
          activity: "ดูระบบง่าย ๆ เช่น กดปุ่มแล้วไฟติด หรือรีโมตสั่งรถ แล้วแยก Input → Process → Output",
          teacher: "สรุปว่า Input คือสิ่งที่ระบบรับเข้าเพื่อให้ Controller นำไปใช้ต่อ",
        },
        {
          time: "7–15",
          phase: "จาก Input สู่ Sensor",
          activity: "เปรียบเทียบ “คนกดปุ่ม” กับ “ระบบตรวจพบความมืดเอง” และเชื่อมกับ ป.1 ว่า Sensor คือ “ตา/หู” ของ Robot",
          teacher: "อธิบายว่า Input มาจากผู้ใช้หรือ Environment ได้ และ Sensor ไม่ใช่ Process",
        },
        {
          time: "15–22",
          phase: "พื้นฐานการทำงานของ Sensor",
          activity: "เรียงแนวคิด Environment → Sensor ตรวจจับ/วัด → สัญญาณหรือข้อมูล → Arduino/Controller",
          teacher: "ย้ำว่า Sensor แค่รับการเปลี่ยนแปลงแล้วส่งต่อ ไม่เข้าใจความหมาย ไม่คิด ไม่เลือก",
        },
        {
          time: "22–32",
          phase: "Sensor Gallery + Sensor Match",
          activity: "ดู Sensor Gallery 4 ชนิด จับคู่ว่าแต่ละตัวตรวจจับอะไร แล้วทำ Sensor Match ในใบงาน 2 ข้อทันที",
          teacher: "ถาม “ตัวนี้ช่วยให้ระบบรู้อะไร?” เน้นหน้าที่มากกว่าชื่อรุ่น",
          note: "ย้าย Sensor Match จากท้ายคาบมาไว้ช่วงนี้ เพราะ 5 นาทีท้ายคาบไม่พอสำหรับ 3 ส่วน",
        },
        {
          time: "32–39",
          phase: "Predict → Test → Observe",
          activity: "ทดลอง Light Sensor: ก่อนบังแสง Predict ว่าค่าจะเปลี่ยนไหม แล้วทดลองจริง",
          teacher: "บันทึกแค่ “เหมือนเดิม / เปลี่ยน / มากขึ้น / น้อยลง” ไม่จดค่าหลายตัว",
        },
        {
          time: "39–44",
          phase: "Digital vs Analog เบื้องต้น",
          activity: "Quick Sort ในสไลด์ว่าอะไรเป็นสถานะชัดเจน และอะไรเป็นค่าหลายระดับ",
          teacher: "บอกว่าจะเจอคำนี้อีกในหน่วย Digital Output และ Digital Input ยังไม่ลง ADC หรือ Bit",
        },
        {
          time: "44–50",
          phase: "Exercise 1: Sensor Detective (2 ส่วน)",
          activity: "ทำใบงาน 2 ส่วน: Input Detective และ System Flow พร้อมเติม “Sensor มีหน้าที่ ______”",
          teacher: "ย้ำ “Sensor = Input ที่ตรวจจับ/วัดและส่งต่อ” ส่วน Arduino/Program = Process",
        },
      ],
    },
  ],
  guides: [
    {
      id: "sensor-gallery",
      title: "Sensor Gallery — ใช้อ้างอิงอีกครั้งในหน่วยที่ 13–16",
      blocks: [
        { icon: "📏", heading: "Distance / Ultrasonic", text: "ตรวจจับระยะ — เจาะลึกในหน่วยที่ 13" },
        { icon: "💡", heading: "Light / LDR", text: "ตรวจจับแสง — เจาะลึกในหน่วยที่ 14" },
        { icon: "🔴", heading: "IR", text: "ตรวจจับวัตถุหรือการกีดขวาง — เจาะลึกในหน่วยที่ 15" },
        { icon: "🌡️", heading: "Temperature", text: "วัดอุณหภูมิ — เจาะลึกในหน่วยที่ 16" },
      ],
      tip: "เก็บการ์ด Sensor Gallery ไว้บนบอร์ดห้องเรียน เมื่อถึงหน่วยที่เจาะ Sensor แต่ละตัว ให้ชี้กลับมาที่การ์ดนี้ก่อนเริ่ม",
    },
    {
      id: "sensor-detective",
      title: "Exercise 1 — Sensor Detective",
      blocks: [
        {
          icon: "1️⃣",
          heading: "Sensor Match (ทำช่วง 22–32 นาที)",
          text: "จับคู่สถานการณ์กับ Sensor ที่เหมาะสม 2 ข้อ",
        },
        {
          icon: "2️⃣",
          heading: "Input Detective (ท้ายคาบ)",
          text: "เลือก Input 2 ข้อจากภาพ และบอกว่ามาจากผู้ใช้หรือ Environment",
        },
        {
          icon: "3️⃣",
          heading: "System Flow (ท้ายคาบ)",
          flow: [
            { label: "Environment" },
            { label: "Sensor", sub: "Input" },
            { label: "Arduino / Program", sub: "Process" },
            { label: "Output" },
          ],
        },
      ],
    },
  ],
  assessment: [
    {
      topic: "พื้นฐาน Input และ Sensor",
      method: "ถามตอบ + Sensor Gallery + Exercise 1",
      criteria: "อธิบาย Input ได้ บอกว่า Sensor เป็น Input ชนิดหนึ่ง และบอกสิ่งที่ Sensor ตัวอย่างตรวจจับได้",
    },
    { topic: "Digital / Analog", method: "Quick Sort", criteria: "จำแนกตัวอย่าง Digital และ Analog พื้นฐานได้" },
    {
      topic: "การเลือกและสังเกต (ว 4.2 ป.3/1)",
      method: "Predict–Test–Observe + System Flow",
      criteria: "เลือก Sensor ให้เหมาะกับสถานการณ์ เรียง System Flow ได้ และอธิบายว่าค่าเปลี่ยนตาม Environment",
    },
    {
      topic: "การทดลองอย่างปลอดภัย",
      method: "สังเกตพฤติกรรม",
      criteria: "ร่วมทดลองอย่างระมัดระวัง ไม่ดึงสายหรือแก้อุปกรณ์เอง",
    },
  ],
  takeaways: [
    "Sensor เป็นอุปกรณ์ Input ที่ตรวจจับหรือวัดสิ่งจาก Environment แล้วส่งข้อมูลให้ Controller — ไม่ได้คิดเอง",
    "Sensor แต่ละชนิดตรวจจับสิ่งต่างกัน เช่น ระยะ แสง การมี/ไม่มีวัตถุ และอุณหภูมิ",
    "Digital มีสถานะชัดเจน ส่วน Analog เปลี่ยนได้หลายระดับ · ภาพรวมระบบคือ Environment → Sensor → Arduino/Program → Output",
  ],
  bridge: {
    nextLabel: "หน่วยที่ 4: Intro Mixly",
    question: "“ตอนนี้เรารู้แล้วว่า Arduino ทำงานตาม Program แล้วเราจะเขียน Program ให้ Arduino ได้อย่างไร?”",
    teaser: "“คาบหน้าเราจะเริ่มเขียนโปรแกรมแบบบล็อกด้วย Mixly”",
  },
};

const unit20: LessonPlan = {
  gradeSlug: "p3",
  week: 20,
  unitLabel: "หน่วยที่ 20",
  title: "MiniProject",
  subtitle: "ออกแบบ สร้าง ทดสอบ และปรับปรุง",
  minutesPerPeriod: 50,
  summary: [
    "MiniProject เป็นช่วงที่นักเรียนนำความรู้จากหลายหน่วยกลับมารวมกันเป็นระบบที่มีเป้าหมาย โดยย้อนกลับไปดู Idea จากหน่วยที่ 2 (ที่ทบทวนแล้วหลังหน่วยที่ 12) แล้วเลือกพัฒนาจาก Project Menu หรือ My Idea เพื่อไม่ต้องเริ่มจากหน้ากระดาษเปล่า",
    "นักเรียนกำหนด Goal เลือก Input และ Output วาง Logic เป็น System Flow ต่อวงจร เขียน Program ด้วย Mixly และทำงานตามวงจร Predict → Build/Code → Run → Observe → Compare → Debug → Try Again โดยคุม Scope ไว้ที่ 1 Input + 1 Output หลัก + 1 Logic แล้วปรับปรุงอย่างน้อย 1 จุดให้พร้อมเข้าสู่ Final Challenge",
  ],
  objectives: [
    "อธิบายความสัมพันธ์ระหว่าง Goal → Input → Program/Logic → Output ของ Project ตนเองได้",
    "อธิบายได้ว่า Test, Debug และ Improve เป็นขั้นตอนที่ใช้พัฒนาระบบให้เข้าใกล้ Goal",
    "เลือก Project และจัดทำ Project Blueprint ที่ระบุ Goal, Input, Output และ System Flow ได้",
    "ต่อวงจรและเขียน Program ด้วย Mixly ตาม Blueprint อย่างปลอดภัย",
    "ทดสอบระบบ บันทึกผล ระบุปัญหา และปรับแก้ Program หรือ Hardware อย่างมีเหตุผล",
    "ปรับปรุงชิ้นงานอย่างน้อย 1 จุด และทำงานร่วมกับเพื่อนตามบทบาทอย่างรับผิดชอบ",
  ],
  standards: [
    { ...algorithmP3, core: "ออกแบบ Project Blueprint และ System Flow โดยกำหนด Goal, Input, Program/Logic และ Output ก่อนลงมือสร้าง" },
    { ...programP3, core: "เขียน Program ด้วย Mixly จากแบบที่วางไว้ ทดลอง ตรวจหาข้อผิดพลาด ปรับแก้ และทดสอบซ้ำ" },
  ],
  integrations: [
    { subject: "ภาษาไทย", detail: "การเขียนและอธิบายแนวคิดของ Project อย่างเป็นลำดับ" },
    { subject: "ศิลปะ / การออกแบบ", detail: "การสเก็ตช์ชิ้นงานและจัดวางองค์ประกอบ" },
    { subject: "คณิตศาสตร์", detail: "การเปรียบเทียบค่าจาก Sensor เวลา หรือ Threshold ตาม Project ที่เลือก" },
  ],
  learning: {
    knowledge:
      "องค์ประกอบของ Project และ Automatic System, Goal / Input / Output / Program-Logic, Sequence/Condition, แนวคิด Prototype และความหมายของ Test, Debug, Improve",
    skills: "การกำหนด Goal การวาง System Flow การต่อวงจร การเขียน Program ด้วย Mixly การ Predict/Test/Observe การ Debug การบันทึก Test Log และการปรับปรุงแล้วทดสอบซ้ำ",
    attitude: "รับผิดชอบต่ออุปกรณ์ อดทนเมื่อระบบยังไม่ทำงาน ร่วมมือตามบทบาท และปรับงานจากหลักฐาน",
  },
  vocabulary: [
    { term: "Goal", meaning: "เป้าหมายของระบบ" },
    { term: "Blueprint", meaning: "แบบแผนก่อนลงมือสร้าง" },
    { term: "Prototype", meaning: "ชิ้นงานต้นแบบ" },
    { term: "Debug", meaning: "หาและแก้ข้อผิดพลาด" },
    { term: "Core Function", meaning: "หน้าที่หลักที่ระบบต้องทำได้" },
  ],
  materials: [
    "Arduino Board, Breadboard, สาย Jumper และอุปกรณ์ที่เรียนมาแล้ว (LED/RGB, Buzzer, Servo, Button, Distance, Light, IR, Temperature Sensor)",
    "Mixly และ Serial Monitor ตามความจำเป็นของ Project",
    "Project Kit แยกตาม Project Menu พร้อม Wiring Guide และ Starter Block ของแต่ละ Project — เตรียมให้ครบก่อนคาบ 2 เพราะช่วง Build + Code + Run + Debug มีเวลาแน่น",
    "Project Menu, Project Journal (Idea จากหน่วยที่ 2), Role Card, Component Card, System Flow Card",
    "ใบงาน Exercise 1–3: Blueprint Checkpoint, Test & Debug Log, Project Ready Check",
  ],
  sessions: [
    {
      number: 1,
      title: "เลือกปัญหาและออกแบบระบบ",
      minutes: 50,
      goal: "นักเรียนเลือก Project ที่เป็นไปได้ กำหนด Goal และสร้าง Project Blueprint ก่อนลงมือสร้าง",
      steps: [
        {
          time: "0–5",
          phase: "กลับไปหา Idea เดิม",
          activity: "เปิด Project Journal ดู Idea จากหน่วยที่ 2 และที่ปรับไว้หลังหน่วยที่ 12 ว่ายังอยากทำหรือไม่",
          teacher: "ย้ำว่าเปลี่ยน Idea ได้ เป้าหมายคือ Project ที่สร้างได้จริง",
        },
        {
          time: "5–12",
          phase: "Project Menu + My Idea",
          activity: "เลือก 1 Project ต่อกลุ่ม เช่น Smart Light, Distance Alert, Auto Gate/Trash Bin, Object Alert, Temperature Warning",
          teacher: "คุม Scope: 1 Input + 1 Output หลัก + 1 Logic/Sequence และตรวจว่าอุปกรณ์มีจริง",
        },
        {
          time: "12–20",
          phase: "กำหนด Goal และ Success",
          activity: "เขียน “เมื่อ… ระบบจะ… เพื่อ…” และกำหนดว่าแบบไหนถือว่าสำเร็จ",
          teacher: "ถาม “เราจะรู้ได้อย่างไรว่าระบบทำงานตามที่ต้องการ?”",
        },
        {
          time: "20–30",
          phase: "วาง System Flow",
          activity: "เรียง Goal → Input → Program/Logic → Output ด้วยการ์ดหรือภาพ",
          teacher: "ช่วยแยกหน้าที่ Sensor, Arduino/Program และ Output โดยไม่ออกแบบแทน",
        },
        {
          time: "30–40",
          phase: "ออกแบบก่อนสร้าง",
          activity: "สเก็ตช์ชิ้นงาน วางตำแหน่งอุปกรณ์ และเรียง Sequence/Condition",
          teacher: "Teacher Checkpoint: Goal ชัด? Input/Output เหมาะ? Flow เป็นไปได้? Scope ไม่ใหญ่เกิน?",
        },
        {
          time: "40–50",
          phase: "Exercise 1: Project Blueprint Checkpoint",
          activity: "ทำ Blueprint ให้ครบ 5 ส่วน และให้สมาชิกแต่ละคนเขียน “ระบบของเราจะทำงานเมื่อ…”",
          teacher: "ใช้เป็น Gate ก่อน Build — ถ้า Scope ใหญ่เกินให้ลดฟังก์ชันก่อน",
        },
      ],
    },
    {
      number: 2,
      title: "Build → Code → Test → Debug",
      minutes: 50,
      goal: "นักเรียนสร้างระบบหลักตาม Blueprint เขียน Program ด้วย Mixly ทดลอง และ Debug จากผลที่สังเกตได้",
      steps: [
        {
          time: "0–5",
          phase: "Safety + Build Plan",
          activity: "ทบทวน Blueprint แบ่งบทบาท Builder / Coder / Tester / Recorder และเรียงว่าจะต่อส่วนใดก่อน",
          teacher: "ย้ำ “หยุดระบบก่อนแก้สาย” และเริ่มจาก Core Function",
        },
        {
          time: "5–15",
          phase: "Build Hardware",
          activity: "ต่อวงจรทีละส่วนตาม Wiring Guide และให้เพื่อนตรวจไขว้ก่อนจ่ายไฟ",
          teacher: "ตรวจ Power/GND/Pin ชี้จุดให้เด็กตรวจเอง",
        },
        {
          time: "15–25",
          phase: "Code Core Function",
          activity: "เริ่มจาก Starter Block ของ Project แล้วสร้าง Program ใน Mixly ตาม Blueprint และอัปโหลด",
          teacher: "ถาม “Block นี้สัมพันธ์กับส่วนไหนของ Flow?”",
          note: "หลายกลุ่มทำพร้อมกันเวลาค่อนข้างแน่น — Starter Block และ Wiring Guide ที่เตรียมไว้จะช่วยให้ทุกกลุ่มถึงช่วงทดสอบทัน",
        },
        {
          time: "25–33",
          phase: "First Run",
          activity: "คาดเดาผลก่อน Run แล้วทดลองจริงและเทียบกับ Success Criteria",
          teacher: "สังเกตทีละส่วน: Input ได้ข้อมูลไหม? Program ทำตามเงื่อนไขไหม? Output ตอบสนองไหม?",
        },
        {
          time: "33–40",
          phase: "Debug Cycle",
          activity: "เลือกปัญหาทีละ 1 จุด ตรวจ Hardware หรือ Program แก้ทีละอย่าง แล้ว Try Again",
          teacher: "ย้ำว่าการ Debug เป็นส่วนปกติของการสร้าง Project",
        },
        {
          time: "40–50",
          phase: "Exercise 2: Test & Debug Log",
          activity: "บันทึก สิ่งที่คาด → สิ่งที่เกิดจริง → จุดที่สงสัย → สิ่งที่แก้ → ผลหลังแก้ และวงว่าแก้ Hardware, Program หรือทั้งสอง",
          teacher: "ถ้ายังไม่สำเร็จ ให้ระบุ Next Step สำหรับคาบที่ 3",
        },
      ],
    },
    {
      number: 3,
      title: "Improve & Ready for Final Challenge",
      minutes: 50,
      goal: "นักเรียนทำให้ Core Function ทำงานสม่ำเสมอ ปรับปรุงอย่างน้อย 1 จุดจากผลการทดสอบ และเตรียม Prototype สำหรับ Final Challenge",
      steps: [
        {
          time: "0–5",
          phase: "Restart Check",
          activity: "เปิด Test & Debug Log ต่อระบบกลับ และทดสอบ Core Function 1 รอบ",
          teacher: "ไม่เพิ่มฟังก์ชันใหม่จนกว่าระบบหลักจะทำงาน",
        },
        {
          time: "5–15",
          phase: "Stabilize Core",
          activity: "แก้ปัญหาค้างและทดสอบซ้ำจนระบบทำงานตาม Goal ได้สม่ำเสมอขึ้น",
          teacher: "ช่วยแยกปัญหาเป็น Input / Program / Output",
        },
        {
          time: "15–28",
          phase: "Choose One Improvement",
          activity: "เลือกปรับ 1 จุด เช่น Threshold, Timing, ตำแหน่ง Sensor หรือการตอบสนองของ Output",
          teacher: "ถาม “สิ่งไหนทำให้ Project ใช้งานดีขึ้นจริง?”",
        },
        {
          time: "28–40",
          phase: "Improve → Retest",
          activity: "ปรับตามแผน แล้วทดสอบก่อน–หลังอย่างน้อย 2 รอบ",
          teacher: "ใช้หลักฐานเปรียบเทียบ และหยุด Extension หากกระทบ Core Function",
        },
        {
          time: "40–50",
          phase: "Exercise 3: Project Ready Check",
          activity: "ทำ Checklist รายกลุ่มและคำตอบรายบุคคลสั้น ๆ ว่า Goal, Input, Logic, Output คืออะไร ทดสอบและปรับอะไร",
          teacher: "ยังไม่ต้องนำเสนออย่างเป็นทางการ — Demo และ Presentation อยู่ในหน่วยที่ 21",
        },
      ],
    },
  ],
  guides: [
    {
      id: "project-menu",
      title: "Project Menu และ Scope มาตรฐาน",
      blocks: [
        {
          icon: "📋",
          heading: "ตัวอย่างใน Project Menu",
          text: "Smart Light · Distance Alert · Auto Gate / Trash Bin · Object Alert · Temperature Warning",
        },
        {
          heading: "Scope ที่เล็กแต่ทำงานได้",
          flow: [{ label: "1 Input" }, { label: "1 Logic / Sequence" }, { label: "1 Output หลัก" }],
        },
        {
          icon: "👥",
          heading: "บทบาทในกลุ่ม (สลับได้)",
          text: "Builder · Coder · Tester · Recorder",
        },
      ],
      tip: "ไม่ให้ความสวยงามมาก่อนความถูกต้อง ความปลอดภัย และการทำงานตาม Goal — Extension เพิ่มเมื่อ Core Function ทำงานแล้วเท่านั้น",
    },
    {
      id: "debug-cycle",
      title: "วงจรพัฒนาระบบ",
      blocks: [
        {
          heading: "ทำซ้ำจนเข้าใกล้ Goal",
          flow: [
            { label: "Predict" },
            { label: "Build / Code" },
            { label: "Run" },
            { label: "Observe" },
            { label: "Debug" },
            { label: "Try Again" },
          ],
        },
        {
          icon: "⚠️",
          heading: "Safety",
          text: "ตรวจวงจรก่อนจ่ายไฟ หยุดระบบก่อนแก้สาย ระวัง Servo และชิ้นส่วนเคลื่อนที่ ถ้า Board ร้อนหรือมีกลิ่นผิดปกติให้หยุดและแจ้งครู",
        },
      ],
    },
  ],
  assessment: [
    {
      topic: "ความเข้าใจระบบและวงจรพัฒนา",
      method: "ถามตอบ + Project Ready Check",
      criteria: "อธิบาย Goal, Input, Logic, Output และ Test/Debug/Improve ได้สอดคล้องกับ Project",
    },
    {
      topic: "การออกแบบ Project (ว 4.2 ป.3/1)",
      method: "Exercise 1: Blueprint Checkpoint",
      criteria: "Blueprint มี Goal ชัด เลือก Input/Output เหมาะสม และ Flow อยู่ใน Scope ที่สร้างได้",
    },
    {
      topic: "Build & Code (ว 4.2 ป.3/2)",
      method: "สังเกตการทำงาน + Prototype + Mixly Program",
      criteria: "ต่อวงจรและสร้าง Program ตาม Blueprint อย่างปลอดภัย และ Core Function ทำงานหรือมีความคืบหน้าที่ตรวจได้",
    },
    {
      topic: "Test, Debug & Improve",
      method: "Exercise 2 + Exercise 3",
      criteria: "มีหลักฐานการทดสอบ แก้ทีละจุด และปรับปรุงอย่างน้อย 1 จุดโดยอธิบายผลก่อน–หลังได้",
    },
    {
      topic: "การทำงานร่วมกันและความปลอดภัย",
      method: "แบบสังเกตพฤติกรรม 3 คาบ",
      criteria: "รับผิดชอบบทบาท ใช้อุปกรณ์ตามข้อตกลง และหยุดระบบก่อนแก้สาย",
    },
  ],
  takeaways: [
    "MiniProject เริ่มจาก Goal ที่ชัด แล้วจึงเลือก Input, Program/Logic และ Output ให้สัมพันธ์กัน",
    "สร้าง Core Function ที่เล็กแต่ทำงานได้ก่อน แล้วจึงพิจารณา Extension",
    "การ Test และ Debug ไม่ใช่ความล้มเหลว แต่เป็นวิธีใช้หลักฐานทำให้ระบบดีขึ้น",
  ],
  bridge: {
    nextLabel: "หน่วยที่ 21: Final Challenge",
    question: "“Prototype ของเราพร้อมแล้ว ถ้าเพื่อนมาลองใช้จริง ระบบจะทำงานได้ตาม Goal ไหม?”",
    teaser: "“สัปดาห์หน้าคือ Final Challenge — Final Test, Demo, Peer Try, Presentation และ Reflection”",
  },
};

export const p3LessonPlans: LessonPlan[] = [unit1, unit2, unit3, unit20];
