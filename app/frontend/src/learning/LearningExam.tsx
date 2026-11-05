import { useTranslation } from "react-i18next";
import { Banner, Button } from "../components/shared";
import { useLearningAttempt } from "./useLearningAttempt";

export function LearningExam({ curriculumId, activityId }: { curriculumId: number; activityId: number }) {
  const { t, i18n } = useTranslation();
  const exam = useLearningAttempt(curriculumId, activityId);
  const text = (value: Record<string, string>) => value[i18n.resolvedLanguage ?? "th"] ?? value.th ?? value.en ?? value.zh ?? "";
  const editable = exam.attempt?.state === "started" && exam.seconds !== 0;
  return <section className="flex flex-col gap-[16px]">
    {exam.error && <Banner kind="err" title={t("learning.error")} />}
    {!exam.attempt && <>
      <label>{t("learning.run")}<select value={exam.runId} onChange={event => exam.setRunId(Number(event.target.value))}
        className="min-h-[48px] w-full rounded-control border-2 border-line bg-surface p-[8px]">
        <option value={0}>{t("learning.choose")}</option>
        {exam.runs.map(run => <option key={run.id} value={run.id}>{run.n_year} · {run.n_term}</option>)}
      </select></label>
      <Button disabled={!exam.runId || exam.busy || !navigator.onLine} onClick={() => { void exam.start(); }}>{t("learning.startExam")}</Button>
    </>}
    {exam.seconds !== null && <p role="timer" aria-live="off">{t("learning.timeLeft", { seconds: exam.seconds })}</p>}
    {exam.attempt?.questions.map(question => <fieldset key={question.question_key} disabled={!editable}
      className="rounded-card border-2 border-line p-[16px]">
      <legend className="font-bold">{text(question.prompt)}</legend>
      {question.options.map(option => <label key={option.option_key} className="flex min-h-[48px] items-center gap-[16px]">
        <input type="radio" name={question.question_key} value={option.option_key}
          checked={exam.answers.some(a => a.question_key === question.question_key && a.selected_option_keys.includes(option.option_key))}
          onChange={() => exam.answer(question.question_key, option.option_key)} />{text(option.text)}
      </label>)}
    </fieldset>)}
    {editable && <Button disabled={exam.busy || !navigator.onLine} onClick={() => { void exam.submit(); }}>{t("learning.submitExam")}</Button>}
    {exam.attempt && exam.attempt.state !== "started" && <p role="status">{exam.attempt.earned === null
      ? t("learning.resultsHidden") : t("learning.examResult", { earned: exam.attempt.earned, max: exam.attempt.max_score })}</p>}
    {exam.attempt && exam.attempt.state !== "started" &&
      <Button disabled={exam.busy || !navigator.onLine} onClick={() => { void exam.start(); }}>{t("learning.startExam")}</Button>}
  </section>;
}
