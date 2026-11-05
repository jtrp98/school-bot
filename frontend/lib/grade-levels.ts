export interface GradeLevel {
  slug: "basic" | "intermediate" | "advance";
  label: string;
  grades: number[];
}

export const gradeLevels: GradeLevel[] = [
  {
    slug: "basic",
    label: "Basic",
    grades: [1, 2],
  },
  {
    slug: "intermediate",
    label: "Intermediate",
    grades: [3, 4],
  },
  {
    slug: "advance",
    label: "Advance",
    grades: [5, 6],
  },
];

export function findGradeLevelByGrade(grade: number) {
  return gradeLevels.find((level) => level.grades.includes(grade));
}
