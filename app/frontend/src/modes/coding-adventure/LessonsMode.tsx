import { LessonModePicker, LessonModeUnavailable } from "../../components/coding-adventure/LessonModePicker";
import LearnMap from "./LearnMap";
import { useLessonMode } from "./useLessonMode";

type Props = ReturnType<typeof useLessonMode>;

export function LessonsModeView({ mode, chooseMode, backToModes }: Props) {
  if (!mode) return <LessonModePicker chooseMode={chooseMode} />;
  if (mode === "arm") return <LearnMap onChooseMode={backToModes} />;
  return <LessonModeUnavailable mode={mode} backToModes={backToModes} />;
}

export default function LessonsMode() {
  return <LessonsModeView {...useLessonMode()} />;
}
