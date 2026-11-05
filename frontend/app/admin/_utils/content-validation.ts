import type { Lesson } from "../_types/content";

export interface MetadataValues {
  title: string;
  level_id: number;
  source_ref: string;
  grades: string;
}

export interface QuestionValues {
  prompt: string;
  kind: "single_choice" | "boolean";
  max_score: string;
  options: string[];
  correct: number;
}

export interface ImportRow {
  source_row_key: string;
  source_position: number;
  source_approved: boolean;
  lesson: {
    lesson_key: string;
    ordinal: number;
    title: string;
    objective: string;
    instructions: string;
    missions: unknown[];
    activities: unknown[];
  };
}

export interface ImportPayload {
  source_ref: string;
  rows: ImportRow[];
}

export function validateMetadata(values: MetadataValues): string | null {
  if (!values.title.trim()) return "กรุณาระบุชื่อหลักสูตร";
  if (values.title.length > 200) return "ชื่อหลักสูตรต้องไม่เกิน 200 ตัวอักษร";
  if (!values.level_id || values.level_id <= 0) return "กรุณาเลือกระดับหลักสูตร";
  if (!values.source_ref.trim()) return "กรุณาระบุแหล่งอ้างอิงเนื้อหา";
  if (values.source_ref.length > 2048) return "แหล่งอ้างอิงต้องไม่เกิน 2048 ตัวอักษร";
  return null;
}

export function validateQuestion(values: QuestionValues): string | null {
  if (!values.prompt.trim()) return "กรุณาระบุโจทย์ข้อสอบ";
  if (values.prompt.length > 8000) return "โจทย์ต้องไม่เกิน 8000 ตัวอักษร";

  const num = Number(values.max_score);
  if (!/^\d+(\.\d{1,6})?$/.test(values.max_score) || isNaN(num) || num <= 0 || num >= 1e12) {
    return "คะแนนเต็มต้องเป็นตัวเลขจำนวนบวก";
  }

  if (!values.options || values.options.length < 2 || values.options.length > 8) {
    return "ตัวเลือกต้องมีระหว่าง 2 ถึง 8 ข้อ";
  }

  if (values.kind === "boolean" && values.options.length !== 2) {
    return "ข้อสอบประเภทถูก/ผิดต้องมี 2 ตัวเลือก";
  }

  for (let i = 0; i < values.options.length; i++) {
    if (!values.options[i].trim()) {
      return `กรุณาระบุข้อความในตัวเลือกที่ ${i + 1}`;
    }
  }

  if (values.correct < 0 || values.correct >= values.options.length) {
    return "กรุณาเลือกคำตอบที่ถูกต้อง";
  }

  return null;
}

export function validateLessons(lessons: Lesson[]): string | null {
  if (!lessons || lessons.length === 0) {
    return "กรุณาเพิ่มบทเรียนอย่างน้อย 1 บทเรียน";
  }

  for (let i = 0; i < lessons.length; i++) {
    const l = lessons[i];
    if (!l.title?.trim() || !l.objective?.trim() || !l.instructions?.trim()) {
      return `บทเรียนที่ ${i + 1}: กรุณากรอกชื่อบทเรียน เป้าหมาย และคำแนะนำให้ครบถ้วน`;
    }
    if (Array.isArray(l.missions)) {
      for (let j = 0; j < l.missions.length; j++) {
        const m = l.missions[j];
        if (!m.title?.trim() || !m.instructions?.trim() || !m.criteria?.rules) {
          return `บทเรียนที่ ${i + 1} ภารกิจที่ ${j + 1}: กรุณากรอกชื่อภารกิจ สิ่งที่ต้องทำ และเกณฑ์ให้ครบถ้วน`;
        }
      }
    }
  }

  return null;
}

export function parseAndValidateImport(data: unknown): ImportPayload {
  if (!data || typeof data !== "object") {
    throw new Error("ไฟล์นำเข้าไม่ถูกต้อง");
  }
  const obj = data as Record<string, unknown>;
  if (typeof obj.source_ref !== "string" || !obj.source_ref.trim()) {
    throw new Error("ไฟล์นำเข้าต้องระบุ source_ref");
  }
  if (!Array.isArray(obj.rows) || obj.rows.length === 0) {
    throw new Error("ไฟล์นำเข้าต้องมีรายการแถว (rows)");
  }
  if (obj.rows.length > 1000) {
    throw new Error("ไฟล์นำเข้ามีรายการเกินขีดจำกัด 1000 แถว");
  }

  for (let i = 0; i < obj.rows.length; i++) {
    const r = obj.rows[i] as Record<string, unknown>;
    if (typeof r.source_row_key !== "string" || !r.source_row_key) {
      throw new Error(`แถวที่ ${i + 1} ไม่มี source_row_key`);
    }
    if (typeof r.source_position !== "number" || r.source_position <= 0) {
      throw new Error(`แถวที่ ${i + 1} ข้อมูล source_position ไม่ถูกต้อง`);
    }
    if (typeof r.source_approved !== "boolean") {
      throw new Error(`แถวที่ ${i + 1} ข้อมูล source_approved ต้องเป็น true/false`);
    }
    if (!r.lesson || typeof r.lesson !== "object") {
      throw new Error(`แถวที่ ${i + 1} ไม่มีข้อมูลบทเรียน (lesson)`);
    }
    const l = r.lesson as Record<string, unknown>;
    if (typeof l.title !== "string" || !l.title.trim()) {
      throw new Error(`แถวที่ ${i + 1} บทเรียนต้องมีชื่อ`);
    }
  }

  return data as ImportPayload;
}
