export type LearningMode = "code" | "block" | "arm";
export interface LearningRequest {
  operation: string;
  id?: number;
  course_run_id?: number;
  activity_id?: number;
  body?: unknown;
  key?: string;
  etag?: string;
}
export interface LearningGateway {
  request<T>(request: LearningRequest): Promise<{ data: T; etag: string }>;
}
export interface LearningActivity {
  id: number;
  title: string;
  type: "instruction" | "project" | "quiz" | "test";
  mode?: LearningMode;
  config: { instructions?: string; mission_id?: number };
}
export interface LearningMission {
  id: number;
  title: string;
  criteria: { id: number };
}
export interface LearningCurriculum {
  id: number;
  title: string;
  revision: number;
  lessons: { id: number; title: string; activities: LearningActivity[]; missions: LearningMission[] }[];
}
export interface LearningSubmission {
  id: number;
  state: "queued" | "grading" | "awaiting_teacher" | "evaluated" | "error";
  update_date: string;
  assessment?: { id: number; outcome: "passed" | "not_yet"; feedback: string; rule_results: { rule_id: string; matched: boolean }[] };
}
export interface LearningAttempt {
  id: number;
  state: "started" | "submitted" | "expired" | "voided";
  deadline_at: string | null;
  server_now: string;
  questions: { question_key: string; prompt: Record<string, string>; options: { option_key: string; text: Record<string, string> }[] }[];
  answers: { question_key: string; selected_option_keys: string[] }[];
  earned: string | null;
  max_score: string;
}
