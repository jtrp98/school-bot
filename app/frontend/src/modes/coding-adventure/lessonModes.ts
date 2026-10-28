import type { IconName } from "../../components/shared/Icons";

export type LessonMode = "block" | "arm" | "code";

type LessonModeOption = {
  id: LessonMode;
  title: string;
  description: string;
  icon: IconName;
};

export const lessonModes: readonly LessonModeOption[] = [
  {
    id: "block",
    title: "lessons.modes.block.title",
    description: "lessons.modes.block.description",
    icon: "block",
  },
  {
    id: "arm",
    title: "lessons.modes.arm.title",
    description: "lessons.modes.arm.description",
    icon: "arm",
  },
  {
    id: "code",
    title: "lessons.modes.code.title",
    description: "lessons.modes.code.description",
    icon: "terminal",
  },
];
