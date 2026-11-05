import { create } from "zustand";
import type { Curriculum, ListPage, QuestionBank, Versioned } from "../_types/content";

interface ContentState {
  curricula: ListPage;
  banks: ListPage;
  levels: { id: number; title: string }[];
  curriculum: Versioned<Curriculum> | null;
  bank: Versioned<QuestionBank> | null;
  busy: boolean;
  error: string | null;
  patch: (value: Partial<Omit<ContentState, "patch" | "reset">>) => void;
  reset: () => void;
}

const initial = {
  curricula: { items: [], next_cursor: null },
  banks: { items: [], next_cursor: null },
  levels: [],
  curriculum: null,
  bank: null,
  busy: false,
  error: null,
};

// แยกสถานะของบทเรียนและคลังข้อสอบออกจากกัน และล้างข้อมูลเมื่อสลับหน้าเพื่อความถูกต้อง
export const useContentStore = create<ContentState>((set) => ({
  ...initial,
  patch: (value) => set(value),
  reset: () => set(initial),
}));
