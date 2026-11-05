"use client";

import { useCallback, useEffect, useRef, useState } from "react";
import { learningRequest } from "@/lib/api/learning-api";
import { useLearningStore } from "../_stores/use-learning-store";
import type {
  Curriculum,
  ExportStatus,
  GradeMapping,
  Preview,
  Proposal,
  Run,
  Submission,
} from "../_types/learning";

export function useLearningManagement() {
  const [runId, setRunId] = useState("");
  const patch = useLearningStore((s) => s.patch);
  const reset = useLearningStore((s) => s.reset);
  const keys = useRef(new Map<string, string>());
  const epoch = useRef(0);

  const invalidate = useCallback(() => {
    epoch.current++;
    reset();
    keys.current.clear();
  }, [reset]);

  const execute = useCallback(
    async <T,>(action: () => Promise<T>, accept: (result: T) => void) => {
      const generation = epoch.current;
      patch({ loading: true, error: null });
      try {
        const result = await action();
        if (epoch.current === generation) accept(result);
      } catch (error) {
        if (epoch.current === generation) patch({ error });
      } finally {
        if (epoch.current === generation) patch({ loading: false });
      }
    },
    [patch],
  );

  useEffect(() => {
    void execute(
      async () => {
        const curricula = await learningRequest<{ items: { id: number; title: string | Record<string, string> }[] }>("/curricula");
        const groups = await Promise.all(
          curricula.data.items.map(async (c) => {
            const title = typeof c.title === "string" ? c.title : c.title?.th ?? c.title?.en ?? String(c.id);
            const runs = await learningRequest<{
              items: { id: number; n_year: number; n_term: string; classroom_id: number }[];
            }>(`/learning/curricula/${c.id}/course-runs`);
            return runs.data.items.map((r) => ({ ...r, title }));
          }),
        );
        return groups.flat();
      },
      (runs) => patch({ runs }),
    );
  }, [execute, patch]);

  const mutate = useCallback(
    async <T,>(path: string, body: unknown = {}, method = "POST", etag?: string | null) => {
      const fingerprint = JSON.stringify([path, method, body, etag]);
      const key = keys.current.get(fingerprint) ?? crypto.randomUUID();
      keys.current.set(fingerprint, key);
      const response = await learningRequest<T>(path, method, body, etag, key);
      keys.current.delete(fingerprint);
      return response;
    },
    [],
  );

  const load = () => {
    if (!/^[1-9]\d*$/.test(runId)) return;
    epoch.current++;
    patch({
      run: null,
      curriculum: null,
      submissions: [],
      proposal: null,
      mapping: null,
      preview: null,
      exportStatus: null,
    });
    void execute(async () => {
      const run = await learningRequest<Run>(`/course-runs/${runId}`);
      const curriculum = await learningRequest<Curriculum>(`/learning/curricula/${run.data.curriculum_id}`);
      const submissions = await learningRequest<{ items: Submission[] }>("/submissions");
      return { run, curriculum: curriculum.data, submissions: submissions.data.items };
    }, patch);
  };

  const run = useLearningStore((s) => s.run);
  const path = `/course-runs/${run?.data.school_bot_course_run_id}`;

  return {
    runId,
    setRunId,
    load,
    execute,
    mutate,
    invalidate,
    review: (submission: Submission, body: unknown) =>
      execute(
        () => mutate<Submission>(`/submissions/${submission.id}/assessments`, body, "POST", submission.etag),
        (result) => {
          const rows = useLearningStore.getState().submissions;
          patch({ submissions: rows.map((s) => (s.id === result.data.id ? result.data : s)) });
        },
      ),
    activate: () =>
      execute(
        () => mutate<Run>(`${path}/activate`, {}, "POST", run?.etag),
        (result) => patch({ run: result }),
      ),
    savePolicy: (body: unknown) =>
      execute(
        async () => {
          await mutate(`${path}/scoring-policy`, body, "PUT", run?.etag);
          return learningRequest<Run>(path);
        },
        (result) => patch({ run: result }),
      ),
    saveTime: (activityId: number, body: unknown, etag: string | null) =>
      execute(() => mutate(`${path}/activities/${activityId}/policy`, body, "PUT", etag), () => undefined),
    retake: (activityId: number, learnerId: number, reason: string) =>
      execute(() => mutate(`${path}/activities/${activityId}/retakes`, { learner_id: learnerId, reason }), () => undefined),
    propose: () =>
      execute(
        () => mutate<Proposal>(`${path}/level-proposals`),
        (proposal) => patch({ proposal }),
      ),
    decide: (confirm: boolean, reason: string) => {
      const proposal = useLearningStore.getState().proposal;
      if (!proposal) return;
      return execute(
        () =>
          mutate<Proposal>(
            `/level-proposals/${proposal.data.id}/${confirm ? "confirm" : "decline"}`,
            { snapshot_hash: proposal.data.snapshot_hash, reason },
            "POST",
            proposal.etag,
          ),
        (result) => patch({ proposal: result }),
      );
    },
    loadMapping: () =>
      execute(
        () => learningRequest<GradeMapping>(`${path}/grade-mapping`),
        (result) => patch({ mapping: result.data }),
      ),
    map: (candidate: string, activityId: number, reason: string) =>
      execute(async () => {
        const mapping = useLearningStore.getState().mapping;
        const ownedSlot = mapping?.candidates.find((c) => c.candidate_key === candidate)?.owned_slot_id;
        const etag = mapping?.mappings.find((m) => m.grade_slot_id === ownedSlot && m.retired_at === null)?.etag;
        await mutate(`${path}/grade-mapping`, { candidate_key: candidate, activity_id: activityId, reason }, "PUT", etag);
        return learningRequest<GradeMapping>(`${path}/grade-mapping`);
      }, (result) => patch({ mapping: result.data })),
    retireMapping: (id: number, etag: string) =>
      execute(async () => {
        await mutate(`${path}/grade-mapping/${id}`, {}, "DELETE", etag);
        return learningRequest<GradeMapping>(`${path}/grade-mapping`);
      }, (result) => patch({ mapping: result.data })),
    preview: () =>
      execute(
        () => mutate<Preview>(`${path}/score-previews`),
        (preview) => patch({ preview, exportStatus: null }),
      ),
    resolveExport: (decisions: { learner_id: number; action: "keep" | "replace" }[]) => {
      const id = useLearningStore.getState().exportStatus?.export.id;
      if (!id) return;
      return execute(
        () => mutate<Preview>(`/score-exports/${id}/resolutions`, { decisions }),
        (preview) => patch({ preview, exportStatus: null }),
      );
    },
    confirmExport: () => {
      const preview = useLearningStore.getState().preview;
      if (!preview) return;
      return execute(
        async () => {
          const result = await mutate<{ id: number }>(
            `/score-previews/${preview.data.id}/confirm`,
            { snapshot_hash: preview.data.snapshot_hash },
            "POST",
            preview.etag,
          );
          return learningRequest<ExportStatus>(`/score-exports/${result.data.id}`);
        },
        (result) => patch({ exportStatus: result.data }),
      );
    },
    recoverExport: () => {
      const id = useLearningStore.getState().exportStatus?.export.id;
      if (!id) return;
      return execute(
        async () => {
          await mutate(`/score-exports/${id}/process`);
          return learningRequest<ExportStatus>(`/score-exports/${id}`);
        },
        (result) => patch({ exportStatus: result.data }),
      );
    },
  };
}

export type LearningManagement = ReturnType<typeof useLearningManagement>;
