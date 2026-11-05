"use client";

import { create } from "zustand";
import type {
  Curriculum,
  ExportStatus,
  GradeMapping,
  Preview,
  Proposal,
  Run,
  Submission,
  Versioned,
} from "../_types/learning";

type LearningState = {
  curriculum: Curriculum | null;
  run: Versioned<Run> | null;
  submissions: Submission[];
  proposal: Versioned<Proposal> | null;
  mapping: GradeMapping | null;
  preview: Versioned<Preview> | null;
  exportStatus: ExportStatus | null;
  loading: boolean;
  error: unknown;
  runs: { id: number; title: string; n_year: number; n_term: string; classroom_id: number }[];
  patch: (value: Partial<Omit<LearningState, "patch" | "reset">>) => void;
  reset: () => void;
};

const initial = {
  curriculum: null,
  run: null,
  submissions: [],
  proposal: null,
  mapping: null,
  preview: null,
  exportStatus: null,
  runs: [],
  loading: false,
  error: null,
};

export const useLearningStore = create<LearningState>((set) => ({
  ...initial,
  patch: (value) => set(value),
  reset: () => set(initial),
}));
