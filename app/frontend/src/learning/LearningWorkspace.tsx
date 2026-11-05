import { useTranslation } from "react-i18next";
import { Banner, Button } from "../components/shared";
import type { LearningMode } from "./types";
import { useLearningWorkspace } from "./useLearningWorkspace";
import { LearningExam } from "./LearningExam";

export function LearningWorkspace({ mode, back }: { mode: LearningMode; back: () => void }) {
  const { t } = useTranslation();
  const workspace = useLearningWorkspace(mode);
  const { blocks, armId, armProjects } = workspace;

  return <section className="mx-auto flex max-w-[1120px] flex-col gap-[24px] p-[24px] text-ink">
    <header className="flex flex-wrap items-center justify-between gap-[16px]">
      <div><h1 className="font-heading text-[28px] font-bold">{t("learning.title")}</h1>
        <p>{t(workspace.connected ? "learning.online" : "learning.offline")}</p></div>
      <Button variant="secondary" onClick={back}>{t("learning.back")}</Button>
    </header>
    {workspace.error && <Banner kind="err" title={t("learning.error")} />}
    <div className="flex flex-wrap items-end gap-[16px]">
      <label className="flex flex-col gap-[8px]">{t("learning.curriculum")}
        <select className="min-h-[48px] rounded-control border-2 border-line bg-surface p-[8px]"
          value={workspace.curriculum?.id ?? ""} disabled={workspace.busy}
          onChange={event => { void workspace.chooseCourse(Number(event.target.value)); }}>
          <option value="" disabled>{t("learning.choose")}</option>
          {workspace.courses.map(course => <option key={course.id} value={course.id}>{course.title}</option>)}
        </select>
      </label>
      <Button variant="secondary" disabled={workspace.busy || !workspace.connected} onClick={() => { void workspace.sync(); }}>
        {t("learning.sync", { count: workspace.pending })}
      </Button>
    </div>
    {workspace.courses.length === 0 && <p>{t("learning.noCourses")}</p>}
    <div className="grid grid-cols-[minmax(240px,1fr)_2fr] gap-[24px] max-[640px]:grid-cols-1">
      <nav aria-label={t("learning.activities")} className="flex flex-col gap-[8px]">
        {workspace.activities.map(activity => <Button key={activity.id} variant="secondary"
          aria-pressed={activity.id === workspace.activity?.id} onClick={() => workspace.setActivity(activity)}>
          {activity.lessonTitle} · {activity.title}
        </Button>)}
      </nav>
      <main className="flex flex-col gap-[16px] rounded-card border-2 border-line bg-surface p-[24px]">
        {workspace.activity ? <>
          <h2 className="font-heading text-[24px] font-bold">{workspace.activity.title}</h2>
          {workspace.activity.type === "instruction" && <p className="whitespace-pre-wrap">{workspace.activity.config.instructions}</p>}
          {workspace.activity.type === "project" && <>
            {mode === "code" && <label className="flex flex-col gap-[8px]">{t("learning.program")}
              <textarea className="min-h-[240px] rounded-control border-2 border-line p-[16px] font-mono"
                maxLength={16384} value={workspace.source} onChange={event => workspace.setSource(event.target.value)} />
            </label>}
            {mode === "block" && <>
              <div className="flex flex-wrap gap-[8px]">
                {(["move_right", "move_left", "jump"] as const).map(op => <Button key={op} variant="secondary"
                  disabled={blocks.length >= 64} onClick={() => workspace.addBlock(op)}>
                  {t(`learning.${op}`)}
                </Button>)}
                <Button variant="ghost" onClick={workspace.clearBlocks}>{t("learning.clear")}</Button>
              </div>
              <ol>{blocks.map(block => <li key={block.id}>{t(`learning.${block.type === "jump" ? "jump" : block.direction}`)}</li>)}</ol>
            </>}
            {mode === "arm" && <>
              <Button variant="secondary" onClick={() => { void workspace.loadArmProjects(); }}>{t("learning.savedProjects")}</Button>
              <label>{t("learning.project")}<select value={armId} onChange={e => workspace.setArmId(e.target.value)}
                className="min-h-[48px] w-full rounded-control border-2 border-line bg-surface p-[8px]">
                <option value="">{t("learning.choose")}</option>
                {armProjects.map(project => <option key={project.project_id} value={project.project_id}>{project.name}</option>)}
              </select></label>
            </>}
            <Button disabled={workspace.busy || (mode === "arm" && !armId)} onClick={() => { void workspace.submitCurrent(); }}>
              {t(workspace.connected ? "learning.submit" : "learning.queue")}
            </Button>
          </>}
          {(workspace.activity.type === "quiz" || workspace.activity.type === "test") &&
            <LearningExam key={workspace.activity.id} curriculumId={workspace.curriculum!.id} activityId={workspace.activity.id} />}
        </> : <p>{t("learning.chooseActivity")}</p>}
      </main>
    </div>
    <section aria-live="polite"><h2 className="font-heading text-[24px]">{t("learning.results")}</h2>
      {workspace.results.map(result => <p key={result.id}>
        {t(`learning.state.${result.state}`)} {result.assessment && t(`learning.outcome.${result.assessment.outcome}`)}
      </p>)}
    </section>
  </section>;
}

