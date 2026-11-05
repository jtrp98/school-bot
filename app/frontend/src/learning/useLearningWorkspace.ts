import { useCallback, useEffect, useMemo, useState } from "react";
import { client } from "../core/client";
import { OfflineLearning } from "./offlineLearning";
import type { LearningActivity, LearningCurriculum, LearningGateway, LearningMode, LearningSubmission } from "./types";

const gateway: LearningGateway = { request: client.learningRequest };

export function useLearningWorkspace(mode: LearningMode) {
  const [offline, setOffline] = useState<OfflineLearning>();
  const [courses, setCourses] = useState<{ id: number; title: string }[]>([]);
  const [curriculum, setCurriculum] = useState<LearningCurriculum>();
  const [activity, setActivity] = useState<LearningActivity>();
  const [source, setSource] = useState("");
  const [blocks, setBlocks] = useState<{ id: string; enabled: true; type: "jump" | "move"; direction?: string; tiles?: number }[]>([]);
  const [armProjects, setArmProjects] = useState<Awaited<ReturnType<typeof client.listProjectsV2>>>([]);
  const [armId, setArmId] = useState("");
  const [pending, setPending] = useState(0);
  const [results, setResults] = useState<LearningSubmission[]>([]);
  const [error, setError] = useState(false);
  const [busy, setBusy] = useState(false);
  const [connected, setConnected] = useState(navigator.onLine);

  const refresh = useCallback((store: OfflineLearning) => {
    const state = store.read();
    setPending(state.pending.length);
    setResults(state.results);
  }, []);

  useEffect(() => {
    let active = true;
    void client.learningScope().then(scope => {
      if (!active) return;
      const store = new OfflineLearning(scope, localStorage, gateway);
      setOffline(store);
      const cached = store.read().curricula;
      setCourses(cached);
      refresh(store);
      if (navigator.onLine) void gateway.request<{ items: { id: number; title: string }[] }>({ operation: "curricula" })
        .then(response => { if (active) setCourses(response.data.items); })
        .catch(() => { if (active) setError(true); });
    }).catch(() => { if (active) setError(true); });
    return () => { active = false; };
  }, [refresh]);

  const sync = useCallback(async () => {
    if (!offline) return;
    setBusy(true);
    setError(false);
    try {
      await offline.sync();
      const state = offline.read();
      for (const result of state.results.filter(r => r.state === "queued" || r.state === "grading")) {
        const response = await gateway.request<LearningSubmission>({ operation: "submissionResult", id: result.id });
        offline.updateResult(response.data);
      }
      const recent = await gateway.request<{ items: LearningSubmission[] }>({ operation: "submissions" });
      for (const result of recent.data.items) offline.updateResult(result);
      setResults(offline.read().results);
    } catch { setError(true); }
    finally { refresh(offline); setBusy(false); }
  }, [offline, refresh]);

  useEffect(() => {
    const online = () => { setConnected(true); void sync(); };
    const disconnected = () => setConnected(false);
    window.addEventListener("online", online);
    window.addEventListener("offline", disconnected);
    if (navigator.onLine && pending > 0) void sync();
    return () => {
      window.removeEventListener("online", online);
      window.removeEventListener("offline", disconnected);
    };
  }, [sync, pending]);

  async function chooseCourse(id: number) {
    if (!offline) return;
    setBusy(true);
    setError(false);
    try {
      const cached = offline.read().curricula.find(c => c.id === id);
      const course = connected
        ? (await gateway.request<LearningCurriculum>({ operation: "curriculum", id })).data : cached;
      if (!course) throw new Error("CURRICULUM_NOT_CACHED");
      offline.cache(course);
      setCurriculum(course);
      setActivity(undefined);
    } catch { setError(true); }
    finally { setBusy(false); }
  }

  async function submit(content: unknown) {
    if (!offline || !curriculum || !activity) return;
    const mission = curriculum.lessons.flatMap(l => l.missions).find(m => m.id === activity.config.mission_id);
    if (!mission) { setError(true); return; }
    try {
      offline.enqueue({ name: activity.title, mode, content, missionId: mission.id, criteriaId: mission.criteria.id });
      refresh(offline);
      if (connected) await sync();
    } catch { setError(true); }
  }

  async function submitCurrent() {
    try {
      if (mode === "code") await submit({ kind: "code", source });
      else if (mode === "block") await submit({ kind: "block", stacks: [{ x: 0, y: 0, blocks }] });
      else await submit((await client.getProjectV2(armId)).content);
    } catch { setError(true); }
  }
  async function loadArmProjects() {
    try { setArmProjects(await client.listProjectsV2("arm")); }
    catch { setError(true); }
  }
  function addBlock(op: "move_left" | "move_right" | "jump") {
    if (blocks.length >= 64) return;
    setBlocks([...blocks, op === "jump" ? { id: crypto.randomUUID(), enabled: true, type: "jump" }
      : { id: crypto.randomUUID(), enabled: true, type: "move", direction: op, tiles: 1 }]);
  }

  const activities = useMemo(() => curriculum?.lessons.flatMap(l =>
    l.activities.filter(a => !a.mode || a.mode === mode).map(a => ({ ...a, lessonTitle: l.title }))) ?? [], [curriculum, mode]);

  return { courses, curriculum, activity, setActivity, activities, source, setSource, pending, results,
    error, busy, connected, chooseCourse, submit, sync, submitCurrent, blocks, addBlock,
    clearBlocks: () => setBlocks([]), armId, setArmId, armProjects, loadArmProjects };
}
