"use client";

import { useState } from "react";
import { MdRateReview } from "react-icons/md";
import { Card, Button, Input, NativeSelect, DataTable } from "./ui";
import { useLearningStore } from "../_stores/use-learning-store";
import { localized } from "../_types/learning";
import type { LearningManagement } from "../_hooks/use-learning-management";
import ProjectEvidence from "./project-evidence";
import { t } from "../messages";

export default function ReviewPanel({ manager }: { manager: LearningManagement }) {
  const curriculum = useLearningStore((s) => s.curriculum);
  const submissions = useLearningStore((s) => s.submissions);
  const loading = useLearningStore((s) => s.loading);

  const [selectedId, select] = useState<number | null>(null);
  const [feedback, setFeedback] = useState("");
  const [reason, setReason] = useState("");
  const [checked, setChecked] = useState<Record<string, boolean>>({});
  const [outcome, setOutcome] = useState("not_yet");

  const missions = curriculum?.lessons.flatMap((l) => l.missions) ?? [];
  const selected = submissions.find((s) => s.id === selectedId);
  const mission = missions.find((m) => m.id === selected?.mission_id);
  const items = mission?.criteria.rules.items ?? [];
  const rows = submissions.filter((s) => missions.some((m) => m.id === s.mission_id));

  return (
    <Card title={t("reviewTitle")} icon={<MdRateReview />} description={t("latestWork")}>
      <div className="flex flex-col gap-4">
        <DataTable
          rows={rows}
          rowKey={(s) => s.id}
          loading={loading}
          emptyText={t("noWork")}
          columns={[
            { id: "learner", header: t("learner"), cell: (s) => s.learner_id },
            {
              id: "mission",
              header: t("mission"),
              cell: (s) => localized(missions.find((m) => m.id === s.mission_id)?.title ?? ""),
            },
            { id: "state", header: t("status"), cell: (s) => t(`states.${s.state}`) },
            {
              id: "actions",
              header: t("review"),
              cell: (s) => (
                <Button
                  content={t("review")}
                  icon={<MdRateReview />}
                  iconOnly
                  aria-label={t("review")}
                  title={t("review")}
                  variant="ghost"
                  onClick={() => {
                    select(s.id);
                    setFeedback(s.assessment?.feedback ?? "");
                    setChecked({});
                    setReason("");
                  }}
                />
              ),
            },
          ]}
        />

        {selected && mission && (
          <section
            aria-label={t("review")}
            className="flex flex-col gap-3 rounded-xl border border-navy/15 bg-paper-dim/40 p-5"
          >
            <p className="font-display font-medium text-navy text-sm">
              {localized(mission.title)} · {t("learner")} {selected.learner_id}
            </p>
            <p className="text-xs text-ink/70">{t("submittedWork")}</p>
            <ProjectEvidence content={selected.project_content} />

            <div className="space-y-1.5 pt-2">
              {items.map((item) => (
                <label key={item.id} className="flex items-center gap-2 text-xs text-ink/80 cursor-pointer">
                  <input
                    type="checkbox"
                    checked={checked[item.id] ?? false}
                    onChange={(e) => setChecked({ ...checked, [item.id]: e.target.checked })}
                    className="rounded border-navy/20 text-orange focus:ring-orange"
                  />
                  {item.instruction}
                </label>
              ))}
            </div>

            <div className="space-y-3 pt-2">
              <label className="block text-xs font-medium text-ink/80">
                {t("outcome")}
                <div className="mt-1">
                  <NativeSelect value={outcome} onChange={(e) => setOutcome(e.target.value)}>
                    <option value="not_yet">{t("notYet")}</option>
                    <option value="passed">{t("passed")}</option>
                  </NativeSelect>
                </div>
              </label>

              <label className="block text-xs font-medium text-ink/80">
                {t("feedback")}
                <div className="mt-1">
                  <Input
                    multiline
                    value={feedback}
                    onChange={setFeedback}
                    aria-label={t("feedback")}
                    maxLength={4000}
                  />
                </div>
              </label>

              {selected.assessment && (
                <label className="block text-xs font-medium text-ink/80">
                  {t("correctionReason")}
                  <div className="mt-1">
                    <Input
                      value={reason}
                      onChange={setReason}
                      aria-label={t("correctionReason")}
                      maxLength={1000}
                    />
                  </div>
                </label>
              )}

              <Button
                content={t("saveReview")}
                disabled={
                  loading ||
                  items.length === 0 ||
                  !feedback.trim() ||
                  (!!selected.assessment && !reason.trim())
                }
                onClick={() =>
                  void manager.review(selected, {
                    outcome,
                    feedback,
                    rule_results: items.map((item) => ({
                      rule_id: item.id,
                      matched: checked[item.id] ?? false,
                    })),
                    ...(selected.assessment ? { supersedes_id: selected.assessment.id, reason } : {}),
                  })
                }
              />
            </div>
          </section>
        )}
      </div>
    </Card>
  );
}
