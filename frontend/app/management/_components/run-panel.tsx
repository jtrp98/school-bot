"use client";

import { useState } from "react";
import { MdSchool } from "react-icons/md";
import { Card, Button, Input, NativeSelect } from "./ui";
import { useExamPolicy } from "../_hooks/use-exam-policy";
import { useLearningStore } from "../_stores/use-learning-store";
import { localized } from "../_types/learning";
import type { LearningManagement } from "../_hooks/use-learning-management";
import { t } from "../messages";

export default function RunPanel({ manager }: { manager: LearningManagement }) {
  const run = useLearningStore((s) => s.run);
  const curriculum = useLearningStore((s) => s.curriculum);
  const proposal = useLearningStore((s) => s.proposal);
  const busy = useLearningStore((s) => s.loading);

  const activities = curriculum?.lessons.flatMap((l) => l.activities).filter((a) => a.type !== "instruction") ?? [];
  const [weights, setWeights] = useState<Record<number, string>>({});
  const exam = useExamPolicy(manager, run?.data.school_bot_course_run_id ?? 0);
  const [learner, setLearner] = useState("");
  const [reason, setReason] = useState("");
  const [quizCounts, setQuizCounts] = useState(false);

  const sum = activities.reduce((total, a) => total + Number(weights[a.id] ?? 0), 0);
  const assessmentActivities = activities.filter((a) => a.type === "quiz" || a.type === "test");

  if (!run) return null;

  return (
    <Card
      title={t("runTitle")}
      icon={<MdSchool />}
      description={`${localized(curriculum?.title ?? "")} · ${run.data.n_year} / ${run.data.n_term}`}
    >
      <div className="flex flex-col gap-4">
        <p className="text-sm font-medium text-navy">
          {t("status")}: <span className="rounded-md bg-sky/20 px-2 py-0.5 text-xs text-navy">{t(`states.${run.data.lifecycle}`)}</span>
        </p>

        {run.data.lifecycle === "planned" && (
          <div className="space-y-3 rounded-xl border border-navy/10 bg-paper-dim/40 p-4">
            <p className="text-xs text-ink/70">{t("weightsHint")}</p>
            <div className="grid gap-3 md:grid-cols-2">
              {activities.map((a) => (
                <label key={a.id} className="block text-xs font-medium text-ink/80">
                  {localized(a.title)} (%)
                  <div className="mt-1">
                    <Input
                      type="number"
                      value={weights[a.id] ?? "0"}
                      onChange={(value) => setWeights({ ...weights, [a.id]: value })}
                      aria-label={localized(a.title)}
                    />
                  </div>
                </label>
              ))}
            </div>

            <label className="flex items-center gap-2 text-xs text-ink/80 cursor-pointer">
              <input
                type="checkbox"
                checked={quizCounts}
                onChange={(e) => setQuizCounts(e.target.checked)}
                className="rounded border-navy/20 text-orange focus:ring-orange"
              />
              {t("quizCounts")}
            </label>

            <div className="flex flex-wrap gap-2 pt-2">
              <Button
                content={t("savePolicy")}
                disabled={busy || sum !== 100}
                onClick={() =>
                  void manager.savePolicy({
                    quiz_counts: quizCounts,
                    formula_version: "weighted-normalized-v1",
                    activity_weights: activities.map((a) => ({
                      activity_id: a.id,
                      weight_bps: Math.round(Number(weights[a.id] ?? 0) * 100),
                    })),
                  })
                }
              />
              <Button content={t("activate")} disabled={busy} onClick={() => void manager.activate()} />
            </div>
          </div>
        )}

        <div className="space-y-3 border-t border-navy/10 pt-3">
          <label className="block text-xs font-medium text-ink/80">
            {t("activity")}
            <div className="mt-1">
              <NativeSelect value={exam.activity} disabled={busy} onChange={(e) => exam.select(e.target.value)}>
                <option value="">{t("chooseActivity")}</option>
                {assessmentActivities.map((a) => (
                  <option key={a.id} value={a.id}>
                    {localized(a.title)}
                  </option>
                ))}
              </NativeSelect>
            </div>
          </label>

          <div className="grid gap-3 md:grid-cols-3">
            <label className="block text-xs font-medium text-ink/80">
              {t("timeLimit")}
              <div className="mt-1">
                <Input type="number" value={exam.time} onChange={exam.setTime} aria-label={t("timeLimit")} />
              </div>
            </label>
            <label className="block text-xs font-medium text-ink/80">
              {t("opens")}
              <div className="mt-1">
                <Input type="datetime-local" value={exam.opens} onChange={exam.setOpens} aria-label={t("opens")} />
              </div>
            </label>
            <label className="block text-xs font-medium text-ink/80">
              {t("closes")}
              <div className="mt-1">
                <Input type="datetime-local" value={exam.closes} onChange={exam.setCloses} aria-label={t("closes")} />
              </div>
            </label>
          </div>

          <Button
            content={t("saveTime")}
            disabled={busy || !exam.etag || (exam.time !== "" && Number(exam.time) <= 0)}
            onClick={() => void exam.save()}
          />
        </div>

        <div className="space-y-3 border-t border-navy/10 pt-3">
          <div className="grid gap-3 md:grid-cols-2">
            <label className="block text-xs font-medium text-ink/80">
              {t("learner")}
              <div className="mt-1">
                <Input type="number" value={learner} onChange={setLearner} aria-label={t("learner")} />
              </div>
            </label>
            <label className="block text-xs font-medium text-ink/80">
              {t("reason")}
              <div className="mt-1">
                <Input value={reason} onChange={setReason} aria-label={t("reason")} maxLength={1000} />
              </div>
            </label>
          </div>
          <Button
            content={t("grantRetake")}
            disabled={busy || !exam.activity || Number(learner) <= 0 || !reason.trim()}
            onClick={() => void manager.retake(Number(exam.activity), Number(learner), reason)}
          />
        </div>

        <div className="border-t border-navy/10 pt-3">
          <Button
            content={t("propose")}
            disabled={busy || !["closing", "closed"].includes(run.data.lifecycle)}
            onClick={() => void manager.propose()}
          />
        </div>

        {proposal && (
          <div className="space-y-2 rounded-xl border border-sky/30 bg-sky/10 p-4" role="status">
            <p className="text-sm font-medium text-navy">
              {t("proposalState")}: {t(`states.${proposal.data.state}`)}
            </p>
            <p className="text-xs text-ink/60">{t("wholeClassHint")}</p>
            <div className="flex flex-wrap gap-2 pt-2">
              <Button
                content={t("confirmLevel")}
                disabled={busy || proposal.data.state !== "proposed"}
                onClick={() => void manager.decide(true, reason)}
              />
              <Button
                content={t("declineLevel")}
                variant="secondary-outline"
                disabled={busy || proposal.data.state !== "proposed" || !reason.trim()}
                onClick={() => void manager.decide(false, reason)}
              />
            </div>
          </div>
        )}
      </div>
    </Card>
  );
}
