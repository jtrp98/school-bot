"use client";

import { useState } from "react";
import { learningRequest } from "@/lib/api/learning-api";
import type { LearningManagement } from "./use-learning-management";

type Policy = {
  effective_policy: { time_limit_seconds: number | null };
  opens_at: string | null;
  closes_at: string | null;
};

const local = (date: string | null) =>
  date
    ? new Date(Date.parse(date) - new Date(date).getTimezoneOffset() * 60000)
        .toISOString()
        .slice(0, 16)
    : "";

export function useExamPolicy(manager: LearningManagement, runId: number) {
  const [activity, setActivity] = useState("");
  const [time, setTime] = useState("");
  const [opens, setOpens] = useState("");
  const [closes, setCloses] = useState("");
  const [etag, setEtag] = useState<string | null>(null);

  const select = (id: string) => {
    setActivity(id);
    setEtag(null);
    setTime("");
    setOpens("");
    setCloses("");
    if (id) {
      void manager.execute(
        () => learningRequest<Policy>(`/course-runs/${runId}/activities/${id}`),
        (response) => {
          setEtag(response.etag);
          setTime(String(response.data.effective_policy.time_limit_seconds ?? ""));
          setOpens(local(response.data.opens_at));
          setCloses(local(response.data.closes_at));
        },
      );
    }
  };

  return {
    activity,
    select,
    time,
    setTime,
    opens,
    setOpens,
    closes,
    setCloses,
    etag,
    save: () =>
      manager.saveTime(
        Number(activity),
        {
          time_limit_seconds: time ? Number(time) : null,
          opens_at: opens ? new Date(opens).toISOString() : null,
          closes_at: closes ? new Date(closes).toISOString() : null,
        },
        etag,
      ),
  };
}
