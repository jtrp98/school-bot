import { useCallback, useEffect, useRef, useState } from "react";
import { client } from "../core/client";
import type { LearningAttempt } from "./types";

export function useLearningAttempt(curriculumId: number, activityId: number) {
  const [runs, setRuns] = useState<{ id: number; n_year: number; n_term: string }[]>([]);
  const [runId, setRunId] = useState(0);
  const [attempt, setAttempt] = useState<LearningAttempt>();
  const [answers, setAnswers] = useState<LearningAttempt["answers"]>([]);
  const [seconds, setSeconds] = useState<number | null>(null);
  const [error, setError] = useState(false);
  const [busy, setBusy] = useState(false);
  const current = useRef<{ attempt: LearningAttempt; etag: string; observedAt: number } | undefined>(undefined);
  const writes = useRef<Promise<void>>(Promise.resolve());
  const expired = useRef(false);
  const generation = useRef(0);
  const expiryRetryAt = useRef(0);

  const accept = useCallback((response: { data: LearningAttempt; etag: string }) => {
    current.current = { attempt: response.data, etag: response.etag, observedAt: performance.now() };
    setAttempt(response.data);
    setAnswers(response.data.answers);
    expired.current = response.data.state !== "started";
  }, []);

  useEffect(() => {
    let active = true;
    void client.learningRequest<{ items: { id: number; n_year: number; n_term: string }[] }>({ operation: "courseRuns", id: curriculumId })
      .then(response => { if (active) setRuns(response.data.items); }).catch(() => { if (active) setError(true); });
    return () => { active = false; };
  }, [curriculumId]);

  useEffect(() => {
    generation.current++;
    current.current = undefined;
    setAttempt(undefined);
    setAnswers([]);
    setSeconds(null);
    if (!runId) return;
    let active = true;
    void client.learningScope().then(async scope => {
      const key = `school-bot:attempt:${scope}:${runId}:${activityId}`;
      const saved = localStorage.getItem(key);
      if (saved && Number.isInteger(Number(saved))) {
        const response = await client.learningRequest<LearningAttempt>({ operation: "attempt", id: Number(saved) });
        if (active) accept(response);
      }
    }).catch(() => { if (active) setError(true); });
    return () => { active = false; };
  }, [runId, activityId, accept]);

  const write = useCallback((nextAnswers: LearningAttempt["answers"], submit: boolean) => {
    const task = writes.current.then(async () => {
      const saved = current.current;
      if (!saved || saved.attempt.state !== "started") return;
      const epoch = generation.current;
      setBusy(true);
      setError(false);
      try {
        const response = await client.learningRequest<LearningAttempt>({
          operation: submit ? "submitAttempt" : "answers", id: saved.attempt.id, etag: saved.etag,
          key: crypto.randomUUID(), body: { answers: nextAnswers },
        });
        if (epoch === generation.current) {
          current.current = { attempt: response.data, etag: response.etag, observedAt: performance.now() };
          setAttempt(response.data);
        }
      } catch (failure) {
        if (epoch !== generation.current) return;
        setError(!String(failure).includes("ATTEMPT_EXPIRED"));
        // A lost write response may have committed; recover its current ETag before another write.
        try {
          const response = await client.learningRequest<LearningAttempt>({ operation: "attempt", id: saved.attempt.id });
          if (epoch === generation.current) accept(response);
        } finally {
          expiryRetryAt.current = performance.now() + 5000;
          expired.current = false;
        }
      } finally { setBusy(false); }
    });
    writes.current = task.catch(() => { setError(true); setBusy(false); });
    return task;
  }, [accept]);

  useEffect(() => {
    const interval = window.setInterval(() => {
      const saved = current.current;
      if (!saved || saved.attempt.state !== "started" || !saved.attempt.deadline_at) { setSeconds(null); return; }
      const time = Date.parse(saved.attempt.server_now) + performance.now() - saved.observedAt;
      const remaining = Math.max(0, Math.ceil((Date.parse(saved.attempt.deadline_at) - time) / 1000));
      setSeconds(remaining);
      if (remaining === 0 && !expired.current && performance.now() >= expiryRetryAt.current) {
        expired.current = true;
        void write(saved.attempt.answers, true);
      }
    }, 250);
    return () => window.clearInterval(interval);
  }, [write]);

  async function start() {
    if (!runId || busy) return;
    setBusy(true);
    setError(false);
    try {
      const scope = await client.learningScope();
      const storageKey = `school-bot:attempt:${scope}:${runId}:${activityId}`;
      const requestKey = localStorage.getItem(`${storageKey}:key`) ?? crypto.randomUUID();
      localStorage.setItem(`${storageKey}:key`, requestKey);
      const response = await client.learningRequest<LearningAttempt>({
        operation: "startAttempt", course_run_id: runId, activity_id: activityId, key: requestKey,
      });
      localStorage.setItem(storageKey, String(response.data.id));
      localStorage.removeItem(`${storageKey}:key`);
      accept(response);
    } catch { setError(true); }
    finally { setBusy(false); }
  }

  function answer(questionKey: string, optionKey: string) {
    const next = [...answers.filter(a => a.question_key !== questionKey), { question_key: questionKey, selected_option_keys: [optionKey] }];
    setAnswers(next);
    void write(next, false);
  }

  return { runs, runId, setRunId, attempt, answers, seconds, error, busy, start, answer,
    submit: () => write(answers, true) };
}
