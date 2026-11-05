// lib/foundation-data.ts
// คอร์สปรับพื้นฐานของ School Bot แยกตามหัวข้อจริงของเนื้อหา
// ใช้เป็นคอร์สเสริมก่อนเข้าเนื้อหาประจำชั้น ป.1-6
// (ดู recommendedFoundationCourses ใน curriculum-data.ts)
//
// - Foundation to Robotics: เนื้อหาจาก Schoolbot Basic Sem1 + Sem2
// - Foundation to AI: เนื้อหาจาก Schoolbot Basic Sem1
// - Foundation to Scratch: บทเปิดจาก Schoolbot Basic Sem2 หน่วย Computational Thinking
// - Foundation to Python: ยังรอเนื้อหาต้นทาง

export interface FoundationModule {
  slug: string;
  title: string;
  hours: number;
  contentFile: string; // path relative to /content
}

export interface FoundationCourse {
  slug: "robotics" | "ai" | "scratch" | "python";
  title: string;
  subtitle: string;
  status: "ready" | "draft" | "coming-soon";
  sourceNote: string;
  modules: FoundationModule[];
}

export const foundationCourses: FoundationCourse[] = [
  {
    slug: "robotics",
    title: "Foundation to Robotics",
    subtitle:
      "รู้จักหุ่นยนต์ Robo X ตั้งแต่แนวคิดพื้นฐาน เกมอุ่นเครื่อง การควบคุมผ่าน PC และแอป Wonderbot ไปจนถึงลีกการแข่งขันและโครงงานปิดท้าย",
    status: "ready",
    sourceNote:
      "ไมเกรตจาก Schoolbot Basic Sem1 และ Sem2 (เดิมเป็นเลเวลรวม ป.4-6) — เขียนไว้สำหรับเด็กที่ยังไม่มีพื้นฐานมาก่อน",
    modules: [
      { slug: "robotics-101", title: "Robotics คืออะไร?", hours: 1, contentFile: "foundation/robotics/01-robotics-101.md" },
      { slug: "robotic-arm-intro", title: "แขนกลหุ่นยนต์ (Robotic Arm) คืออะไร?", hours: 2, contentFile: "foundation/robotics/02-robotic-arm-intro.md" },
      { slug: "motion-and-angles", title: "การเคลื่อนไหวของแขนกลและการหมุนเชิงมุม", hours: 2, contentFile: "foundation/robotics/03-motion-and-angles.md" },
      { slug: "arm-components", title: "ส่วนประกอบของ Robotic Arm", hours: 2, contentFile: "foundation/robotics/04-arm-components.md" },
      { slug: "pc-software-control", title: "การควบคุม Robo X ผ่านซอฟต์แวร์ PC", hours: 4, contentFile: "foundation/robotics/05-pc-software-control.md" },
      { slug: "mission-builder", title: "กิจกรรม: Mission Builder (Action Group)", hours: 1, contentFile: "foundation/robotics/06-mission-builder.md" },
      { slug: "obstacle-avoidance", title: "กิจกรรม: Obstacle Avoidance", hours: 1, contentFile: "foundation/robotics/07-obstacle-avoidance.md" },
      { slug: "combine-master", title: "กิจกรรม: Combine Master (ผสาน Action Group)", hours: 1, contentFile: "foundation/robotics/08-combine-master.md" },
      { slug: "debug-master", title: "กิจกรรม: Debug Master (ตามล่าบัก)", hours: 1, contentFile: "foundation/robotics/09-debug-master.md" },
      { slug: "sorting-line", title: "กิจกรรม: Sorting Line (สายการผลิต)", hours: 1, contentFile: "foundation/robotics/10-sorting-line.md" },
      { slug: "precision-pick", title: "กิจกรรม: Precision Pick (หยิบเล็กให้แม่น)", hours: 1, contentFile: "foundation/robotics/11-precision-pick.md" },
      { slug: "free-build", title: "กิจกรรม: Free Build (ออกแบบด้วยตัวเอง)", hours: 1, contentFile: "foundation/robotics/12-free-build.md" },
      { slug: "final-showcase", title: "กิจกรรม: Final Showcase (เวทีนำเสนอ)", hours: 1, contentFile: "foundation/robotics/13-final-showcase.md" },
      { slug: "pbl-inspire-ideate", title: "โครงงานปิดท้าย 1: Inspire & Ideate", hours: 1, contentFile: "foundation/robotics/14-pbl-inspire-ideate.md" },
      { slug: "pbl-plan-design", title: "โครงงานปิดท้าย 2: Plan & Design", hours: 1, contentFile: "foundation/robotics/15-pbl-plan-design.md" },
      { slug: "pbl-build-test", title: "โครงงานปิดท้าย 3: Build & Test", hours: 1, contentFile: "foundation/robotics/16-pbl-build-test.md" },
      { slug: "pbl-present-reflect", title: "โครงงานปิดท้าย 4: Present & Reflect", hours: 1, contentFile: "foundation/robotics/17-pbl-present-reflect.md" },
      { slug: "game-build-impossible", title: "กิจกรรม: Build the Impossible", hours: 1, contentFile: "foundation/robotics/18-game-build-impossible.md" },
      { slug: "silent-builder", title: "กิจกรรม: Silent Builder Challenge", hours: 1, contentFile: "foundation/robotics/19-silent-builder.md" },
      { slug: "factory-frenzy", title: "กิจกรรม: Factory Frenzy", hours: 1, contentFile: "foundation/robotics/20-factory-frenzy.md" },
      { slug: "grand-mission", title: "กิจกรรม: Grand Mission Tournament", hours: 1, contentFile: "foundation/robotics/21-grand-mission.md" },
      { slug: "connecting-future", title: "Connecting the Future", hours: 2, contentFile: "foundation/robotics/22-connecting-future.md" },
      { slug: "app-slider-explore", title: "App Slider Control: สำรวจและควบคุม Robo X ผ่าน Slider", hours: 1, contentFile: "foundation/robotics/23-app-slider-explore.md" },
      { slug: "app-slider-challenge", title: "App Slider Control: ภารกิจ Challenge + กราฟ Slider + เปรียบเทียบ", hours: 1, contentFile: "foundation/robotics/24-app-slider-challenge.md" },
      { slug: "action-designer", title: "The Action Designer", hours: 1, contentFile: "foundation/robotics/25-action-designer.md" },
      { slug: "edit-refine", title: "Edit & Refine", hours: 1, contentFile: "foundation/robotics/26-edit-refine.md" },
      { slug: "mission-possible", title: "Mission Possible", hours: 1, contentFile: "foundation/robotics/27-mission-possible.md" },
      { slug: "robot-show", title: "Robot Show & Design Thinking", hours: 1, contentFile: "foundation/robotics/28-robot-show.md" },
      { slug: "championship-kickoff", title: "Championship Kickoff", hours: 1, contentFile: "foundation/robotics/29-championship-kickoff.md" },
      { slug: "speed-run", title: "Speed Run", hours: 1, contentFile: "foundation/robotics/30-speed-run.md" },
      { slug: "precision-code", title: "Precision Code", hours: 1, contentFile: "foundation/robotics/31-precision-code.md" },
      { slug: "app-maestro", title: "App Maestro", hours: 1, contentFile: "foundation/robotics/32-app-maestro.md" },
      { slug: "relay-race", title: "Relay Race", hours: 1, contentFile: "foundation/robotics/33-relay-race.md" },
      { slug: "debug-battle", title: "Debug Battle", hours: 1, contentFile: "foundation/robotics/34-debug-battle.md" },
      { slug: "blind-pilot", title: "Blind Pilot", hours: 1, contentFile: "foundation/robotics/35-blind-pilot.md" },
      { slug: "system-design", title: "System Design", hours: 1, contentFile: "foundation/robotics/36-system-design.md" },
      { slug: "grand-final-sem2", title: "Grand Final", hours: 1, contentFile: "foundation/robotics/37-grand-final.md" },
      { slug: "award-ceremony", title: "Award Ceremony + Reflection", hours: 1, contentFile: "foundation/robotics/38-award-ceremony.md" },
      { slug: "sem2-pbl-inspire-ideate", title: "Sem2 PBL 1: Inspire & Ideate", hours: 1, contentFile: "foundation/robotics/39-pbl-inspire-ideate.md" },
      { slug: "sem2-pbl-plan-design", title: "Sem2 PBL 2: Plan & Design", hours: 1, contentFile: "foundation/robotics/40-pbl-plan-design.md" },
      { slug: "sem2-pbl-build-test", title: "Sem2 PBL 3: Build & Test", hours: 1, contentFile: "foundation/robotics/41-pbl-build-test.md" },
      { slug: "sem2-pbl-present-reflect", title: "Sem2 PBL 4: Present & Reflect", hours: 1, contentFile: "foundation/robotics/42-pbl-present-reflect.md" },
    ],
  },
  {
    slug: "ai",
    title: "Foundation to AI",
    subtitle:
      "เข้าใจว่า AI คืออะไร เรียนรู้ได้ยังไง มีจริยธรรมอะไรบ้าง แล้วลงมือใช้ AI จริงทั้งเขียน Prompt และฝึกโมเดลด้วยตัวเอง",
    status: "ready",
    sourceNote:
      "ไมเกรตจาก Schoolbot Basic Sem1 (เดิมเป็นเลเวลรวม ป.4-6) — เขียนไว้สำหรับเด็กที่ยังไม่มีพื้นฐานมาก่อน",
    modules: [
      { slug: "what-is-ai", title: "AI คืออะไร?", hours: 1, contentFile: "foundation/ai/01-what-is-ai.md" },
      { slug: "ai-ethics-types", title: "AI กับจริยธรรมและประเภทของ AI", hours: 1, contentFile: "foundation/ai/02-ai-ethics-types.md" },
      { slug: "digital-data", title: "ข้อมูลดิจิทัล", hours: 1, contentFile: "foundation/ai/03-digital-data.md" },
      { slug: "machine-learning", title: "Machine Learning คืออะไร?", hours: 2, contentFile: "foundation/ai/04-machine-learning.md" },
      { slug: "prompt-thinking", title: "Prompt Thinking และ AI อัจฉริยะ", hours: 2, contentFile: "foundation/ai/05-prompt-thinking.md" },
      { slug: "ai-tools-prompting", title: "AI Tool และการใช้งานชุดคำสั่ง Prompt", hours: 4, contentFile: "foundation/ai/06-ai-tools-prompting.md" },
      { slug: "data-literacy", title: "Data Literacy: การสร้างและวิเคราะห์ข้อมูล", hours: 2, contentFile: "foundation/ai/07-data-literacy.md" },
    ],
  },
  {
    slug: "scratch",
    title: "Foundation to Scratch",
    subtitle: "ปูพื้นฐานแนวคิดเชิงคำนวณก่อนต่อยอดสู่การเขียนโปรแกรมแบบบล็อกด้วย WonderCode",
    status: "ready",
    sourceNote: "คัดจาก Schoolbot Basic Sem2 หน่วย Computational Thinking เพื่อใช้เป็นบทเปิดของคอร์ส Scratch",
    modules: [
      { slug: "decomposition", title: "Decomposition: การแยกย่อยปัญหา", hours: 1, contentFile: "foundation/scratch/01-decomposition.md" },
      { slug: "pattern-abstraction", title: "Pattern Recognition และ Abstraction", hours: 1, contentFile: "foundation/scratch/02-pattern-abstraction.md" },
      { slug: "algorithm-flowchart", title: "Algorithm และ Flowchart", hours: 2, contentFile: "foundation/scratch/03-algorithm-flowchart.md" },
    ],
  },
  {
    slug: "python",
    title: "Foundation to Python",
    subtitle: "เร็วๆ นี้",
    status: "coming-soon",
    sourceNote: "",
    modules: [],
  },
];
