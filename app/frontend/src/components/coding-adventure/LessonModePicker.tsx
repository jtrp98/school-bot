import { t } from "../../i18n";
import { Button } from "../shared";
import { Icon } from "../shared/Icons";
import { lessonModes, type LessonMode } from "../../modes/coding-adventure/lessonModes";

type PickerProps = { chooseMode: (mode: LessonMode) => void };

export function LessonModePicker({ chooseMode }: PickerProps) {
  return (
    <section className="mx-auto flex max-w-[1120px] flex-col gap-[32px] p-[32px]">
      <div className="flex flex-col gap-[8px]">
        <h1 className="font-heading text-[28px] font-bold text-ink">{t("lessons.modes.title")}</h1>
        <p className="font-body text-[18px] text-muted">{t("lessons.modes.description")}</p>
      </div>
      <div className="grid grid-cols-3 gap-[24px] [@media(max-width:_850px)]:grid-cols-1">
        {lessonModes.map(mode => (
          <Button
            key={mode.id}
            variant="custom"
            className={
              "flex min-h-[256px] flex-col items-start gap-[16px] rounded-card border-2 border-line " +
              "bg-surface p-[24px] text-left shadow-card hover:border-blue hover:bg-blue-tint " +
              "focus-visible:outline-none focus-visible:shadow-[var(--focus)]"
            }
            onClick={() => chooseMode(mode.id)}
          >
            <span className={
              "flex h-[64px] w-[64px] items-center justify-center rounded-control " +
              (mode.id === "block" ? "bg-orange-tint text-orange-deep" : "") +
              (mode.id === "arm" ? "bg-blue-tint text-blue-ink" : "") +
              (mode.id === "code" ? "bg-purple-tint text-purple-ink" : "")
            }>
              <Icon name={mode.icon} size={36} />
            </span>
            <span className="font-heading text-[24px] font-bold text-ink">{t(mode.title)}</span>
            <span className="font-body text-[18px] text-muted">{t(mode.description)}</span>
            <span className="mt-auto flex items-center gap-[8px] font-body text-[16px] font-bold text-blue-ink">
              {t("lessons.modes.viewLessons")}
              <Icon name="arrowR" size={20} />
            </span>
          </Button>
        ))}
      </div>
    </section>
  );
}

type UnavailableProps = { mode: LessonMode; backToModes: () => void };

export function LessonModeUnavailable({ mode, backToModes }: UnavailableProps) {
  const option = lessonModes.find(option => option.id === mode)!;
  return (
    <section className="flex flex-col items-start gap-[24px] p-[32px]">
      <Button variant="secondary" icon="arrowL" onClick={backToModes}>
        {t("lessons.modes.change")}
      </Button>
      <h1 className="flex items-center gap-[16px] font-heading text-[28px] font-bold text-ink">
        <Icon name={option.icon} size={32} />
        {t(option.title)}
      </h1>
      <p role="status" className="font-body text-[18px] text-muted">
        {t("lessons.modes.unavailable", { modeName: t(option.title) })}
      </p>
    </section>
  );
}
