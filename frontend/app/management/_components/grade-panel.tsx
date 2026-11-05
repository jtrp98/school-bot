"use client";

import { useState } from "react";
import { MdFactCheck, MdDeleteOutline } from "react-icons/md";
import { Card, Button, Input, NativeSelect, DataTable } from "./ui";
import { useLearningStore } from "../_stores/use-learning-store";
import { localized } from "../_types/learning";
import type { LearningManagement } from "../_hooks/use-learning-management";
import { usePreviewDeadline } from "../_hooks/use-preview-deadline";
import { t } from "../messages";

export default function GradePanel({ manager }: { manager: LearningManagement }) {
  const curriculum = useLearningStore((s) => s.curriculum);
  const mapping = useLearningStore((s) => s.mapping);
  const preview = useLearningStore((s) => s.preview);
  const expired = usePreviewDeadline(preview?.data.expires_at);
  const status = useLearningStore((s) => s.exportStatus);
  const busy = useLearningStore((s) => s.loading);

  const [candidate, setCandidate] = useState("");
  const [activity, setActivity] = useState("");
  const [reason, setReason] = useState("");
  const [acknowledged, acknowledge] = useState(false);
  const [resolutions, setResolutions] = useState<Record<string, "keep" | "replace">>({});

  const activities = curriculum?.lessons.flatMap((l) => l.activities).filter((a) => a.type !== "instruction") ?? [];

  return (
    <Card title={t("gradeTitle")} icon={<MdFactCheck />}>
      <div className="flex flex-col gap-4">
        <p className="text-xs text-ink/70">{t("gradeHint")}</p>
        <div>
          <Button content={t("loadMapping")} disabled={busy} onClick={() => void manager.loadMapping()} />
        </div>

        {mapping && (
          <div className="space-y-3 rounded-xl border border-navy/10 bg-paper-dim/40 p-4">
            <label className="block text-xs font-medium text-ink/80">
              {t("destination")}
              <div className="mt-1">
                <NativeSelect value={candidate} onChange={(e) => setCandidate(e.target.value)}>
                  <option value="">{t("chooseDestination")}</option>
                  {mapping.candidates.map((c, index) => (
                    <option key={c.candidate_key} value={c.candidate_key}>
                      {c.category} · {index + 1}
                    </option>
                  ))}
                </NativeSelect>
              </div>
            </label>

            <label className="block text-xs font-medium text-ink/80">
              {t("activity")}
              <div className="mt-1">
                <NativeSelect value={activity} onChange={(e) => setActivity(e.target.value)}>
                  <option value="">{t("chooseActivity")}</option>
                  {activities.map((a) => (
                    <option key={a.id} value={a.id}>
                      {localized(a.title)}
                    </option>
                  ))}
                </NativeSelect>
              </div>
            </label>

            <label className="block text-xs font-medium text-ink/80">
              {t("ownershipReason")}
              <div className="mt-1">
                <Input value={reason} onChange={setReason} aria-label={t("ownershipReason")} maxLength={1000} />
              </div>
            </label>

            <Button
              content={t("saveMapping")}
              disabled={busy || !candidate || !activity || !reason.trim()}
              onClick={() => void manager.map(candidate, Number(activity), reason)}
            />
          </div>
        )}

        {mapping && (
          <DataTable
            rows={mapping.mappings.filter((m) => !m.retired_at)}
            rowKey={(m) => m.id}
            columns={[
              {
                id: "activity",
                header: t("activity"),
                cell: (m) => localized(activities.find((a) => a.id === m.activity_id)?.title ?? ""),
              },
              {
                id: "target",
                header: t("destination"),
                cell: (m) => mapping.slots.find((s) => s.id === m.grade_slot_id)?.category,
              },
              {
                id: "actions",
                header: t("retireMapping"),
                cell: (m) => (
                  <Button
                    iconOnly
                    icon={<MdDeleteOutline />}
                    aria-label={t("retireMapping")}
                    title={t("retireMapping")}
                    content={t("retireMapping")}
                    variant="ghost"
                    disabled={busy}
                    onClick={() => void manager.retireMapping(m.id, m.etag)}
                  />
                ),
              },
            ]}
          />
        )}

        <div className="pt-2">
          <Button
            content={t("preview")}
            disabled={busy}
            onClick={() => {
              acknowledge(false);
              void manager.preview();
            }}
          />
        </div>

        {preview && (
          <div className="space-y-3 rounded-xl border border-navy/10 bg-paper-dim/40 p-4">
            <p className="text-xs text-ink/70">
              {t("expires", { time: new Date(preview.data.expires_at).toLocaleString("th-TH") })}
            </p>

            <DataTable
              rows={preview.data.learners}
              rowKey={(r) => r.learner_id}
              columns={[
                { id: "learner", header: t("learner"), cell: (r) => r.learner_id },
                {
                  id: "source",
                  header: t("sourceScore"),
                  cell: (r) => r.cells.map((c) => `${c.source.earned}/${c.source.max}`).join(", "),
                },
                {
                  id: "old",
                  header: t("oldScore"),
                  cell: (r) =>
                    [
                      ...r.cells.map((c) => c.target?.current_value ?? t("emptyScore")),
                      ...(r.skipped_cells ?? []).map((c) => c.old_score ?? t("emptyScore")),
                    ].join(", "),
                },
                {
                  id: "target",
                  header: t("targetScore"),
                  cell: (r) => (r.cells.length ? r.cells.map((c) => c.integer_target).join(", ") : t("skipRetain")),
                },
                { id: "skipped", header: t("skippedCells"), cell: (r) => r.skipped_cells?.length ?? 0 },
                { id: "state", header: t("status"), cell: (r) => (r.state === "ready" ? t("ready") : t("blocked")) },
                {
                  id: "reason",
                  header: t("reason"),
                  cell: (r) =>
                    [
                      ...new Set([
                        ...r.reasons.map((reason) => (reason.startsWith("SOURCE_") ? t("missingScore") : t("destinationNeedsReview"))),
                        ...(r.skipped_cells?.length ? [t("skipRetain")] : []),
                      ]),
                    ].join(", "),
                },
              ]}
            />

            <label className="flex items-center gap-2 text-xs text-ink/80 cursor-pointer pt-2">
              <input
                type="checkbox"
                checked={acknowledged}
                onChange={(e) => acknowledge(e.target.checked)}
                className="rounded border-navy/20 text-orange focus:ring-orange"
              />
              {t("confirmHint")}
            </label>

            <Button
              content={t("confirmExport")}
              disabled={
                busy ||
                !acknowledged ||
                !!status ||
                preview.data.learners.some((l) => l.state !== "ready") ||
                expired
              }
              onClick={() => void manager.confirmExport()}
            />
          </div>
        )}

        {status && (
          <div className="space-y-3 rounded-xl border border-navy/10 bg-paper-dim/40 p-4">
            <p role="status" className="text-sm font-medium text-navy">
              {t("exportState")}: <span className="text-orange">{t(`states.${status.export.state}`)}</span>
            </p>

            <DataTable
              rows={status.items}
              rowKey={(r) => r.id}
              columns={[
                { id: "learner", header: t("learner"), cell: (r) => r.learner_id },
                { id: "state", header: t("status"), cell: (r) => t(`states.${r.state}`) },
                {
                  id: "receipt",
                  header: t("receipt"),
                  cell: (r) =>
                    r.state === "skipped"
                      ? t("noWrite")
                      : r.receipt_id
                      ? t("receiptSaved")
                      : r.last_error_code
                      ? t("requiresReview")
                      : t("waiting"),
                },
                {
                  id: "resolution",
                  header: t("resolveDecision"),
                  cell: (r) =>
                    status.export.state === "conflict" && ["queued", "conflict"].includes(r.state) ? (
                      <NativeSelect
                        aria-label={`${t("resolveDecision")} ${r.learner_id}`}
                        value={resolutions[`${status.export.id}:${r.learner_id}`] ?? "keep"}
                        onChange={(e) => {
                          const action = e.target.value === "replace" ? "replace" : "keep";
                          setResolutions((values) => ({ ...values, [`${status.export.id}:${r.learner_id}`]: action }));
                        }}
                      >
                        <option value="keep">{t("keepExisting")}</option>
                        <option value="replace">{t("replaceScore")}</option>
                      </NativeSelect>
                    ) : null,
                },
              ]}
            />

            {status.export.state === "conflict" && (
              <div className="space-y-2 pt-2">
                <p className="text-xs text-ink/70">{t("resolveHint")}</p>
                <Button
                  content={t("previewResolution")}
                  disabled={
                    busy ||
                    status.items.some((r) => ["processing", "unknown"].includes(r.state)) ||
                    !status.items.some((r) => ["queued", "conflict"].includes(r.state))
                  }
                  onClick={() => {
                    acknowledge(false);
                    void manager.resolveExport(
                      status.items
                        .filter((r) => ["queued", "conflict"].includes(r.state))
                        .map((r) => ({
                          learner_id: r.learner_id,
                          action: resolutions[`${status.export.id}:${r.learner_id}`] ?? "keep",
                        })),
                    );
                  }}
                />
              </div>
            )}

            <Button
              content={t("recoverExport")}
              disabled={
                busy ||
                ["applied", "completed_with_skips", "superseded", "blocked"].includes(status.export.state) ||
                (status.export.state === "conflict" && !status.items.some((r) => ["unknown", "processing"].includes(r.state)))
              }
              onClick={() => void manager.recoverExport()}
            />
          </div>
        )}
      </div>
    </Card>
  );
}
