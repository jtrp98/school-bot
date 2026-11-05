import { LessonModePicker, LessonModeUnavailable } from "../../components/coding-adventure/LessonModePicker";
import LearnMap from "./LearnMap";
import { useLessonMode } from "./useLessonMode";
import { LearningWorkspace } from "../../learning/LearningWorkspace";
import { useSampleData } from "../../shell";
import { param } from "../../core/debugParams";

type Props = ReturnType<typeof useLessonMode>;

export function LessonsModeView({ mode, chooseMode, backToModes }: Props) {
  const sample = useSampleData() || param("preview") === "1";
  if (!mode) return <LessonModePicker chooseMode={chooseMode} />;
  if (!sample) return <LearningWorkspace mode={mode} back={backToModes} />;
  if (mode === "arm") return <LearnMap onChooseMode={backToModes} />;
  return <LessonModeUnavailable mode={mode} backToModes={backToModes} />;
}

export default function LessonsMode() {
  return <LessonsModeView {...useLessonMode()} />;
}
