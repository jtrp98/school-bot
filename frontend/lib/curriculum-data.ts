// lib/curriculum-data.ts
// หลักสูตรหลัก ป.1–6 ของ School Bot (ใช้หุ่นยนต์ Robo X)
// ออกแบบให้เป็น "หลักสูตรมาตรฐาน" ที่ใช้ได้ทุกปีโดยไม่ต้องรื้อใหม่
// ปัญหา "ปีแรกไม่มีพื้นฐาน" แก้ด้วย recommendedFoundationCourses (ดู foundation-data.ts)
// ไม่ใช่การหักชั่วโมงออกจากเนื้อหาประจำชั้นแบบเดิม
//
// ทุกจุดที่เขียนว่า TODO คือเนื้อหาที่รอเติม
// เริ่มลงเนื้อหาจริงจาก ป.1 ก่อนตามที่ทีมหลักสูตรสั่ง

export type Tool = "app" | "ps2" | "pc-software" | "wondercode" | "python";

export interface LessonBlock {
  title: string;
  hours: number;
  tools: Tool[];
  summary: string;
}

export interface GradeCurriculum {
  grade: number; // 1-6
  label: string; // "ป.1"
  totalHours: number; // คงที่ 40 ชม./ปี
  status: "ready" | "draft"; // ready = เนื้อหาจริงแล้ว, draft = ยังเป็นโครง/TODO
  recommendedFoundationCourses: string[]; // slug จาก foundation-data.ts — แนะนำเฉพาะเด็กที่ยังไม่มีพื้นฐาน (เช่น ปีแรกของหลักสูตร) ไม่บังคับ ไม่กินชั่วโมงประจำชั้น
  coreBlocks: LessonBlock[];
  extensionActivities: LessonBlock[];
}

export const curriculum: GradeCurriculum[] = [
  {
    grade: 1,
    label: "ป.1",
    totalHours: 40,
    status: "draft",
    recommendedFoundationCourses: [],
    coreBlocks: [
      {
        title: "หุ่นยนต์คืออะไร / รู้จัก Robo X",
        hours: 6,
        tools: ["app"],
        summary:
          "TODO: แนะนำชิ้นส่วนหุ่นยนต์ จุดต่อเซนเซอร์ และความปลอดภัยในการใช้งาน",
      },
      {
        title: "บังคับแขนกลด้วยมือ (App / จอย PS2)",
        hours: 14,
        tools: ["app", "ps2"],
        summary: "TODO: กดปุ่มสั่งงาน สังเกตการเคลื่อนไหว ไม่มีการเขียนโค้ด",
      },
      {
        title: "ดูหุ่นยนต์ฉลาดได้ยังไง (สาธิต AI vision เบื้องต้น)",
        hours: 8,
        tools: ["app"],
        summary:
          "TODO: ครูสาธิตจำสี/จำหน้าให้ดู เด็กสังเกตและอภิปราย ไม่ต้องเขียนโปรแกรม",
      },
      {
        title: "โครงงานปิดเทอม: ผู้บังคับการแขนกลตัวน้อย",
        hours: 12,
        tools: ["app"],
        summary: "TODO: รวมทักษะที่เรียนมาในกิจกรรมเล่นเป็นทีม",
      },
    ],
    extensionActivities: [
      {
        title: "กิจกรรมเสริม: วาดภาพ \"หุ่นยนต์ในฝัน\"",
        hours: 0,
        tools: [],
        summary: "TODO: กิจกรรมไม่ใช้อุปกรณ์ สำหรับเวลาเหลือ/วันฝนตก",
      },
    ],
  },
  {
    grade: 2,
    label: "ป.2",
    totalHours: 40,
    status: "draft",
    recommendedFoundationCourses: [],
    coreBlocks: [
      {
        title: "บังคับแขนกลขั้นสูง + รู้จักพอร์ตเซนเซอร์",
        hours: 14,
        tools: ["app", "ps2"],
        summary: "TODO",
      },
      {
        title: "PC Software: สั่งงานท่าทางสำเร็จรูป",
        hours: 18,
        tools: ["pc-software"],
        summary: "TODO: ทดลองเรียกใช้ action group ที่มีอยู่แล้ว ยังไม่สร้างเอง",
      },
      {
        title: "TODO: ขยายเนื้อหาให้ครบ 40 ชม.",
        hours: 8,
        tools: [],
        summary: "TODO: รอออกแบบต่อหลังจาก ป.1 เสร็จสมบูรณ์",
      },
    ],
    extensionActivities: [],
  },
  {
    grade: 3,
    label: "ป.3",
    totalHours: 40,
    status: "draft",
    recommendedFoundationCourses: [],
    coreBlocks: [
      {
        title: "PC Software: สร้าง Action Group เอง",
        hours: 12,
        tools: ["pc-software"],
        summary:
          "TODO: เรียงลำดับท่าทางเป็นโปรแกรมง่ายๆ ปูพื้นแนวคิด sequencing",
      },
      {
        title: "เริ่มเขียนโปรแกรมด้วย WonderCode (บล็อกพื้นฐาน)",
        hours: 14,
        tools: ["wondercode"],
        summary:
          "TODO: ไฟ RGB, จอแมทริกซ์, ปุ่มกด — ยังไม่แตะพิกัด/เซนเซอร์ซับซ้อน",
      },
      {
        title: "TODO: ขยายเนื้อหาให้ครบ 40 ชม.",
        hours: 14,
        tools: [],
        summary: "TODO: รอออกแบบต่อหลังจาก ป.1 เสร็จสมบูรณ์",
      },
    ],
    extensionActivities: [],
  },
  {
    grade: 4,
    label: "ป.4",
    totalHours: 40,
    status: "draft",
    recommendedFoundationCourses: ["robotics", "ai"],
    coreBlocks: [
      {
        title: "WonderCode: เซนเซอร์พื้นฐานครบชุด",
        hours: 14,
        tools: ["wondercode"],
        summary: "TODO: อัลตราโซนิก, เสียง, สีเบื้องต้น",
      },
      {
        title: "เริ่มรู้จัก WonderCam (กล้อง AI)",
        hours: 8,
        tools: ["wondercode"],
        summary: "TODO: จำสีอย่างเดียว ยังไม่ทำ tracking",
      },
      {
        title: "TODO: ขยายเนื้อหาให้ครบ 40 ชม.",
        hours: 18,
        tools: [],
        summary: "TODO: รอออกแบบต่อหลังจาก ป.1 เสร็จสมบูรณ์",
      },
    ],
    extensionActivities: [],
  },
  {
    grade: 5,
    label: "ป.5",
    totalHours: 40,
    status: "draft",
    recommendedFoundationCourses: ["robotics", "ai"],
    coreBlocks: [
      {
        title: "AI Vision: จำสี + ติดตามสี",
        hours: 10,
        tools: ["wondercode"],
        summary: "TODO",
      },
      {
        title: "AI Vision: จำแท็ก (AprilTag) + คัดแยกของ",
        hours: 10,
        tools: ["wondercode"],
        summary: "TODO",
      },
      {
        title: "TODO: ขยายเนื้อหาให้ครบ 40 ชม.",
        hours: 20,
        tools: [],
        summary: "TODO: รอออกแบบต่อหลังจาก ป.1 เสร็จสมบูรณ์",
      },
    ],
    extensionActivities: [],
  },
  {
    grade: 6,
    label: "ป.6",
    totalHours: 40,
    status: "draft",
    recommendedFoundationCourses: ["robotics", "ai"],
    coreBlocks: [
      {
        title: "AI Vision ขั้นสูง: จำหน้า + ติดตามหน้า",
        hours: 10,
        tools: ["wondercode"],
        summary: "TODO",
      },
      {
        title: "แอบดูโค้ด Python (Auto Transcode)",
        hours: 8,
        tools: ["wondercode", "python"],
        summary:
          "TODO: ใช้ปุ่มแปลงบล็อกเป็น Python ให้ดูโครงสร้างโค้ด ไม่ลงรายละเอียดทฤษฎี",
      },
      {
        title: "TODO: ขยายเนื้อหาให้ครบ 40 ชม.",
        hours: 22,
        tools: [],
        summary: "TODO: รอออกแบบต่อหลังจาก ป.1 เสร็จสมบูรณ์",
      },
    ],
    extensionActivities: [],
  },
];

export const toolLabel: Record<Tool, string> = {
  app: "แอปมือถือ",
  ps2: "จอย PS2",
  "pc-software": "โปรแกรม PC (Action Editor)",
  wondercode: "WonderCode (Scratch)",
  python: "Python Editor",
};
