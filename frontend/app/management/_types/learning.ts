export type LocalText = string | Record<string, string>;

export type Versioned<T> = { data: T; etag: string | null };

export type Activity = {
  id: number;
  title: LocalText;
  type: string;
  mode?: string;
  config: { mission_id?: number };
};

export type Mission = {
  id: number;
  title: LocalText;
  criteria: {
    id: number;
    rules: { items?: { id: string; instruction: string }[] };
  };
};

export type Curriculum = {
  id: number;
  title: LocalText;
  lessons: {
    id: number;
    title: LocalText;
    activities: Activity[];
    missions: Mission[];
  }[];
};

export type Run = {
  school_bot_course_run_id: number;
  curriculum_id: number;
  classroom_id: number;
  n_year: number;
  n_term: string;
  lifecycle: string;
  update_date: string;
};

export type Submission = {
  id: number;
  learner_id: number;
  mission_id: number;
  criteria_id: number;
  project_revision_id: number;
  project_content?: Record<string, unknown>;
  project_mode?: string;
  evidence?: Record<string, unknown>;
  state: string;
  etag: string;
  assessment?: {
    id: number;
    outcome: string;
    feedback: string;
    rule_results: string;
  };
};

export type Proposal = {
  id: number;
  snapshot_hash: string;
  state: string;
  snapshot: string;
  to_level_id: number;
  average_score: number;
};

export type GradeMapping = {
  mappings: {
    id: number;
    activity_id: number;
    grade_slot_id: number;
    retired_at: string | null;
    etag: string;
  }[];
  slots: {
    id: number;
    category: string;
    score_identifier: string;
  }[];
  candidates: {
    candidate_key: string;
    owned_slot_id?: number;
    category: string;
    target_snapshot: string;
  }[];
};

export type Preview = {
  id: number;
  snapshot_hash: string;
  expires_at: string;
  state: string;
  learners: {
    learner_id: number;
    state: string;
    reasons: string[];
    skipped_cells?: {
      mapping_id: number;
      grade_slot_id: number;
      reason_code: string;
      old_score: number | null;
    }[];
    cells: {
      integer_target: number;
      raw_target: number;
      source: { earned: number; max: number };
      target?: { current_value: number | null };
      slot: { id: number; category: string };
    }[];
  }[];
};

export type ExportStatus = {
  export: { id: number; state: string };
  items: {
    id: number;
    learner_id: number;
    state: string;
    last_error_code?: string;
    receipt_id?: number;
  }[];
};

export function localized(text: LocalText, locale = "th"): string {
  if (typeof text === "string") return text;
  return text[locale] ?? text.th ?? text.en ?? Object.values(text)[0] ?? "";
}
