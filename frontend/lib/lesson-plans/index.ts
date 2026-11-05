// lib/lesson-plans/index.ts
import { p1LessonPlans } from "./p1";
import { p2LessonPlans } from "./p2";
import { p3LessonPlans } from "./p3";
import { p4LessonPlans } from "./p4";
import { p5LessonPlans } from "./p5";
import { p6LessonPlans } from "./p6";
import type { LessonPlan } from "./types";

export type { LessonPlan } from "./types";

export const lessonPlans: LessonPlan[] = [
  ...p1LessonPlans,
  ...p2LessonPlans,
  ...p3LessonPlans,
  ...p4LessonPlans,
  ...p5LessonPlans,
  ...p6LessonPlans,
];

export function findLessonPlan(gradeSlug: string, week: number) {
  return lessonPlans.find((plan) => plan.gradeSlug === gradeSlug && plan.week === week);
}

export function lessonHref(gradeSlug: string, week: number) {
  return `/curriculum/${gradeSlug}/week-${week}`;
}
