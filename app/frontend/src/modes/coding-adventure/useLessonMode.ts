import { useState } from "react";
import type { LessonMode } from "./lessonModes";

export function useLessonMode() {
  const [mode, setMode] = useState<LessonMode | null>(null);

  function chooseMode(next: LessonMode) {
    setMode(next);
  }

  function backToModes() {
    setMode(null);
  }

  return { mode, chooseMode, backToModes };
}
