"use client";

import Link from "next/link";
import { MdSchool, MdArrowBack } from "react-icons/md";
import { Card, Button, NativeSelect, ApiErrorText } from "./_components/ui";
import { useLearningManagement } from "./_hooks/use-learning-management";
import { useLearningStore } from "./_stores/use-learning-store";
import ReviewPanel from "./_components/review-panel";
import RunPanel from "./_components/run-panel";
import GradePanel from "./_components/grade-panel";
import { t } from "./messages";

export default function SchoolBotManagementPage() {
  const manager = useLearningManagement();
  const run = useLearningStore((s) => s.run);
  const error = useLearningStore((s) => s.error);
  const busy = useLearningStore((s) => s.loading);
  const runs = useLearningStore((s) => s.runs);

  return (
    <div className="min-h-screen bg-paper pb-16 pt-8">
      <div className="mx-auto max-w-5xl px-4 sm:px-6 space-y-6">
        <div className="flex items-center justify-between">
          <Link
            href="/"
            className="inline-flex items-center gap-1.5 text-xs font-medium text-navy/70 hover:text-navy transition"
          >
            <MdArrowBack className="text-base" />
            <span>กลับหน้าหลัก</span>
          </Link>
          <span className="rounded-full bg-orange/15 px-3 py-1 text-xs font-semibold text-navy">
            School Bot · ครูผู้สอน
          </span>
        </div>

        <Card title={t("title")} icon={<MdSchool />} description="ระบบจัดการรอบการเรียน นโยบายประเมินผล และส่งคะแนน">
          <div className="flex flex-col gap-4">
            <label className="block text-xs font-medium text-ink/80">
              {t("courseRun")}
              <div className="mt-1">
                <NativeSelect
                  value={manager.runId}
                  onChange={(e) => manager.setRunId(e.target.value)}
                  aria-label={t("courseRun")}
                >
                  <option value="">{t("loadRun")}</option>
                  {runs.map((r) => (
                    <option key={r.id} value={r.id}>
                      {r.title} · {r.n_year} / {r.n_term} · ห้องเรียน #{r.classroom_id}
                    </option>
                  ))}
                </NativeSelect>
              </div>
            </label>

            <div>
              <Button content={t("loadRun")} pending={busy} disabled={!manager.runId} onClick={manager.load} />
            </div>

            <ApiErrorText error={error} />
          </div>
        </Card>

        {run && (
          <div className="space-y-6">
            <RunPanel key={run.data.school_bot_course_run_id} manager={manager} />
            <ReviewPanel key={`review-${run.data.school_bot_course_run_id}`} manager={manager} />
            <GradePanel key={`grade-${run.data.school_bot_course_run_id}`} manager={manager} />
          </div>
        )}
      </div>
    </div>
  );
}
