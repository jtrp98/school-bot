import type { FoundationCourse } from "@/lib/foundation-data";

export interface FoundationSequenceItem {
  href: string;
  title: string;
  badge?: string;
  kind: "resource" | "module";
}

export function buildFoundationSequence(
  course: FoundationCourse,
): FoundationSequenceItem[] {
  const items: FoundationSequenceItem[] = [];

  if (course.slug === "robotics") {
    items.push({
      href: "/foundation/robotics/robox-components",
      title: "องค์ประกอบของ RoboX",
      badge: "PDF",
      kind: "resource",
    });
  }

  course.modules.forEach((module) => {
    items.push({
      href: `/foundation/${course.slug}/${module.slug}`,
      title: module.title,
      kind: "module",
    });
  });

  return items;
}
