export type BasicResourceType = "lesson" | "test";

export interface BasicSessionResource {
  slug: string;
  title: string;
  periodLabel: string;
  type: BasicResourceType;
  pdfPath: string;
  slidesPdfPath?: string;
  unitTitle?: string;
  topics?: string[];
}

export interface BasicSession {
  slug: "basic-1" | "basic-2";
  title: string;
  subtitle: string;
  description: string;
  resources: BasicSessionResource[];
}

export const basicSessions: BasicSession[] = [
  {
    slug: "basic-1",
    title: "Basic 1",
    subtitle: "Session 1",
    description:
      "รวมแผนการเรียนและแบบทดสอบของชุด Basic 1 ตามลำดับคาบเรียนจากไฟล์ PDF ในโฟลเดอร์ public/pdf/Basic 1",
    resources: [
      {
        slug: "basic1-periods-1-4",
        title: "แผนการเรียน",
        periodLabel: "คาบเรียนที่ 1-4",
        type: "lesson",
        pdfPath: "/pdf/Basic 1/(Basic1) คาบเรียนที่ 1-4.pdf",
        slidesPdfPath: "/pdf/Slides/basic-1-periods-1-4.pdf",
        unitTitle: "Robotic & AI Technology",
        topics: [
          "Robotics คืออะไร และความหมายของวิทยาการหุ่นยนต์",
          "องค์ประกอบสำคัญของระบบหุ่นยนต์",
          "AI คืออะไร และ 4 คุณสมบัติหลักของ AI",
          "หลักการทำงานของ AI พร้อมประเภทของ AI",
          "ข้อมูลดิจิทัลและจริยธรรมการใช้ AI",
        ],
      },
      {
        slug: "basic1-test-1-4",
        title: "แบบทดสอบ",
        periodLabel: "คาบเรียนที่ 1-4",
        type: "test",
        pdfPath: "/pdf/Basic 1/(Basic1) แบบทดสอบคาบเรียนที่ 1-4.pdf",
      },
      {
        slug: "basic1-periods-5-14",
        title: "แผนการเรียน",
        periodLabel: "คาบเรียนที่ 5-14",
        type: "lesson",
        pdfPath: "/pdf/Basic 1/(Basic1) คาบเรียนที่ 5-14.pdf",
        unitTitle: "AI Literacy",
        topics: [
          "Machine Learning คืออะไร และประเภทหลักของการเรียนรู้",
          "AI Bias และผลกระทบจากอคติของข้อมูล",
          "Prompt Thinking และการเขียน Prompt อย่างมีโครงสร้าง",
          "รู้จักเครื่องมือ AI เช่น ChatGPT, Gemini, Grok และ Suno",
          "Data Literacy และการทำโปรเจกต์ด้วย Google Teachable Machine",
        ],
      },
      {
        slug: "basic1-test-5-14",
        title: "แบบทดสอบ",
        periodLabel: "คาบเรียนที่ 5-14",
        type: "test",
        pdfPath: "/pdf/Basic 1/(Basic1) แบบทดสอบคาบเรียนที่ 5-14.pdf",
      },
      {
        slug: "basic1-periods-15-20",
        title: "แผนการเรียน",
        periodLabel: "คาบเรียนที่ 15-20",
        type: "lesson",
        pdfPath: "/pdf/Basic 1/(Basic1) คาบเรียนที่ 15-20.pdf",
        unitTitle: "Industry Robotic / Robotic Arm Technology",
        topics: [
          "แขนกลหุ่นยนต์คืออะไร และบทบาทในงานอุตสาหกรรม",
          "ส่วนประกอบหลักของ Robotic Arm และโครงสร้างแขนกล",
          "Degrees of Freedom และการเคลื่อนที่เชิงตำแหน่ง/เชิงมุม",
          "ระบบพิกัดคาร์ทีเซียนและองศาอิสระในการหมุน",
          "ระบบควบคุม เซนเซอร์ และอุปกรณ์เสริมของแขนกล",
        ],
      },
      {
        slug: "basic1-test-15-20",
        title: "แบบทดสอบ",
        periodLabel: "คาบเรียนที่ 15-20",
        type: "test",
        pdfPath: "/pdf/Basic 1/(Basic1) แบบทดสอบคาบเรียนที่ 15-20.pdf",
      },
      {
        slug: "basic1-periods-21-32",
        title: "แผนการเรียน",
        periodLabel: "คาบเรียนที่ 21-32",
        type: "lesson",
        pdfPath: "/pdf/Basic 1/(Basic1) คาบเรียนที่ 21-32.pdf",
        topics: [
          "ฝึกควบคุมหุ่นยนต์ผ่านการใช้งานจริง",
          "พัฒนาความแม่นยำของการเคลื่อนที่และลำดับคำสั่ง",
          "เชื่อมการทำงานระหว่างอุปกรณ์ควบคุมกับภารกิจ",
        ],
      },
      {
        slug: "basic1-test-21-32",
        title: "แบบทดสอบ",
        periodLabel: "คาบเรียนที่ 21-32",
        type: "test",
        pdfPath: "/pdf/Basic 1/(Basic1) แบบทดสอบคาบเรียนที่ 21-32.pdf",
      },
      {
        slug: "basic1-periods-33-36",
        title: "แผนการเรียน",
        periodLabel: "คาบเรียนที่ 33-36",
        type: "lesson",
        pdfPath: "/pdf/Basic 1/(Basic1) คาบเรียนที่ 33-36.pdf",
        topics: [
          "สรุปทักษะสำคัญจากหน่วยก่อนหน้า",
          "ประยุกต์ใช้ความรู้กับภารกิจหรือกิจกรรมต่อยอด",
          "เตรียมความพร้อมก่อนเข้าส่วนถัดไปของหลักสูตร",
        ],
      },
    ],
  },
  {
    slug: "basic-2",
    title: "Basic 2",
    subtitle: "Session 2",
    description:
      "รวมแผนการเรียนของชุด Basic 2 ตามลำดับคาบเรียนจากไฟล์ PDF ในโฟลเดอร์ public/pdf/Basic 2",
    resources: [
      {
        slug: "basic2-periods-1-4",
        title: "แผนการเรียน",
        periodLabel: "คาบเรียนที่ 1-4",
        type: "lesson",
        pdfPath: "/pdf/Basic 2/(Basic2) คาบเรียนที่ 1-4.pdf",
        slidesPdfPath: "/pdf/Slides/basic-2-periods-1-4.pdf",
        unitTitle: "Game base Learning",
        topics: [
          "เรียนรู้ผ่านเกมและภารกิจแบบทีม",
          "ฝึกการวางแผน แก้ปัญหา และออกแบบกติกา",
          "เข้าใจระบบ การทำงานเป็นลำดับ และการสะท้อนผล",
        ],
      },
      {
        slug: "basic2-periods-5-12",
        title: "แผนการเรียน",
        periodLabel: "คาบเรียนที่ 5-12",
        type: "lesson",
        pdfPath: "/pdf/Basic 2/(Basic2) คาบเรียนที่ 5-12.pdf",
        topics: [
          "พัฒนาทักษะพื้นฐานการควบคุมหุ่นยนต์ให้คล่องขึ้น",
          "ฝึกความแม่นยำของการเคลื่อนที่และการจัดลำดับงาน",
          "เชื่อมการทดลองกับโจทย์ภารกิจจริง",
        ],
      },
      {
        slug: "basic2-periods-13-20",
        title: "แผนการเรียน",
        periodLabel: "คาบเรียนที่ 13-20",
        type: "lesson",
        pdfPath: "/pdf/Basic 2/(Basic2) คาบเรียนที่ 13-20.pdf",
        unitTitle: "การควบคุม XArm ผ่าน Application",
        topics: [
          "เชื่อมต่อและควบคุม XArm ผ่านแอปพลิเคชัน",
          "เข้าใจการใช้งานปุ่ม คำสั่ง และ Slider บนแอป",
          "สร้าง แก้ไข และเรียกใช้ Action Group",
          "เปรียบเทียบการควบคุมผ่านแอปกับวิธีอื่น",
        ],
      },
      {
        slug: "basic2-periods-21-30",
        title: "แผนการเรียน",
        periodLabel: "คาบเรียนที่ 21-30",
        type: "lesson",
        pdfPath: "/pdf/Basic 2/(Basic2) คาบเรียนที่ 21-30.pdf",
        unitTitle: "Multi control Robotic Arm",
        topics: [
          "ต่อยอดการควบคุมแขนกลหลายรูปแบบ",
          "ฝึกประสานการทำงานของอุปกรณ์ควบคุมกับภารกิจ",
          "เพิ่มความแม่นยำ ความเร็ว และการแก้ปัญหาระหว่างใช้งาน",
        ],
      },
      {
        slug: "basic2-periods-31-34",
        title: "แผนการเรียน",
        periodLabel: "คาบเรียนที่ 31-34",
        type: "lesson",
        pdfPath: "/pdf/Basic 2/(Basic2) คาบเรียนที่ 31-34.pdf",
        unitTitle: "Computational Thinking",
        topics: [
          "เข้าใจแนวคิดเชิงคำนวณและการแยกปัญหา",
          "ฝึก Pattern Recognition และ Abstraction",
          "ออกแบบ Algorithm และ Flowchart สำหรับการแก้ปัญหา",
        ],
      },
      {
        slug: "basic2-periods-34-38",
        title: "แผนการเรียน",
        periodLabel: "คาบเรียนที่ 34-38",
        type: "lesson",
        pdfPath: "/pdf/Basic 2/(Basic2) คาบเรียนที่ 34-38.pdf",
        unitTitle: "Final Project (PBL)",
        topics: [
          "ตั้งโจทย์ วางแผน และออกแบบโครงงาน",
          "สร้าง ทดสอบ และปรับปรุงชิ้นงาน",
          "นำเสนอผลงานและสะท้อนการเรียนรู้จากโครงงาน",
        ],
      },
    ],
  },
];
