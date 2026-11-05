import type { Lesson } from "../_types/content";

// ส่งเฉพาะข้อมูลที่แก้ไขได้ โดยไม่ส่ง provenance แบบอ่านอย่างเดียวกลับเข้า strict API
export function lessonInputs(lessons: Lesson[], newRevision = false): Lesson[] {
  return lessons.map((lesson, li) => ({
    lesson_key: lesson.lesson_key,
    ordinal: li + 1,
    title: lesson.title,
    objective: lesson.objective,
    instructions: lesson.instructions,
    missions: lesson.missions.map((mission, mi) => ({
      mission_key: mission.mission_key,
      ordinal: mi + 1,
      title: mission.title,
      instructions: mission.instructions,
      method: mission.method,
      criteria: {
        revision: mission.criteria.revision,
        rules: mission.criteria.rules,
        source_ref: mission.criteria.source_ref,
        approved_by: newRevision ? null : mission.criteria.approved_by,
        approved_at: newRevision ? null : mission.criteria.approved_at,
      },
    })),
    activities: lesson.activities.map((activity, ai) => {
      if (newRevision && activity.type === "project") {
        let config: { mission_key?: string };
        try {
          config = JSON.parse(activity.config ?? "{}");
        } catch {
          throw new Error("ข้อมูลภารกิจเดิมไม่สมบูรณ์ กรุณาตรวจการอ้างอิงภารกิจก่อนสร้างฉบับใหม่");
        }
        if (!config?.mission_key || !lesson.missions.some((m) => m.mission_key === config.mission_key)) {
          throw new Error("ภารกิจฉบับเดิมยังใช้รหัสฐานข้อมูล กรุณาแปลงการอ้างอิงเป็นรหัสภารกิจถาวรก่อนสร้างฉบับใหม่");
        }
      }
      return {
        activity_key: activity.activity_key,
        ordinal: ai + 1,
        title: activity.title,
        type: activity.type,
        mode: activity.mode,
        indicators: activity.indicators,
        config: activity.config,
      };
    }),
  }));
}
