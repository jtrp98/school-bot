export type LocalText = Record<string, string>;

export type Versioned<T> = { value: T; etag: string | null };

export type Summary = {
  id: number;
  title: string | LocalText;
  lifecycle: string;
  revision: number;
  curriculum_key?: string;
  bank_key?: string;
};

export type ListPage = {
  items: Summary[];
  next_cursor: string | null;
};

export type Criteria = {
  revision: number;
  rules: string;
  source_ref: string;
  approved_by?: string | null;
  approved_at?: string | null;
};

export type Mission = {
  mission_key: string;
  ordinal: number;
  title: string;
  instructions: string;
  method: string;
  criteria: Criteria;
};

export type Activity = {
  activity_key: string;
  ordinal: number;
  title: string;
  type: string;
  mode?: string | null;
  indicators?: string | null;
  config?: string | null;
};

export type Lesson = {
  lesson_key: string;
  ordinal: number;
  title: string;
  objective: string;
  instructions: string;
  missions: Mission[];
  activities: Activity[];
};

export type Curriculum = Omit<Summary, "title"> & {
  title: string;
  curriculum_key: string;
  level_id: number;
  source_ref: string;
  recommended_grades: string[];
  subjects: { subject_key: string; code: string; title: string }[];
  lessons: Lesson[];
};

export type Question = {
  question_key: string;
  kind: "single_choice" | "boolean";
  prompt: LocalText;
  options: { option_key: string; text: LocalText }[];
  correct_option_keys: string[];
  max_score: string;
};

export type QuestionBank = Omit<Summary, "title"> & {
  bank_key: string;
  title: LocalText;
  source_ref: string;
  questions: Question[];
};

// แสดงข้อความภาษาไทยเป็นหลักตามสัญญาแสดงผลของระบบ
export function contentTitle(title: Summary["title"]): string {
  if (typeof title === "string") return title;
  return title?.th ?? title?.en ?? Object.values(title ?? {})[0] ?? "";
}
