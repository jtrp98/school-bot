// lib/lesson-plans/types.ts
// แผนการสอนแบบบทความ — เก็บเป็นข้อมูลโครงสร้างแทนการแนบไฟล์ Doc/PDF

export interface FlowStep {
  label: string;
  sub?: string;
}

export interface Standard {
  code: string;
  indicator: string;
  core: string;
}

export interface Integration {
  subject: string;
  detail: string;
}

export interface LessonStep {
  time: string;
  phase: string;
  activity: string;
  teacher?: string;
  expected?: string;
  note?: string;
  // อ้างถึงใบงานที่ใช้ในขั้นนี้ เช่น { id: "dream-robot", part: "ตอนที่ 1" }
  worksheet?: { id: string; part?: string };
}

export interface Worksheet {
  id: string;
  title: string;
  file: string; // path ใต้ /public
  parts: string[];
  usage: string;
}

export interface LessonSession {
  number: number;
  title: string;
  minutes: number;
  goal: string;
  steps: LessonStep[];
}

export interface GuideBlock {
  icon?: string;
  heading: string;
  text?: string;
  compare?: string;
  flow?: FlowStep[];
  questions?: string[];
}

export interface LessonGuide {
  id: string;
  title: string;
  intro?: string;
  blocks: GuideBlock[];
  tip?: string;
}

export interface Assessment {
  topic: string;
  method: string;
  criteria: string;
}

export interface LessonPlan {
  gradeSlug: string;
  week: number;
  unitLabel: string;
  title: string;
  subtitle: string;
  minutesPerPeriod: number;
  summary: string[];
  objectives: string[];
  standards: Standard[];
  integrations: Integration[];
  learning: {
    knowledge: string;
    skills: string;
    attitude: string;
  };
  vocabulary?: { term: string; meaning: string }[];
  worksheets?: Worksheet[];
  materials: string[];
  sessions: LessonSession[];
  guides?: LessonGuide[];
  questions?: string[];
  assessment: Assessment[];
  assessmentNote?: string;
  takeaways: string[];
  bridge: {
    nextLabel: string;
    question: string;
    teaser: string;
  };
}
