import type { LearningCurriculum, LearningGateway, LearningMode, LearningSubmission } from "./types";

interface PendingSubmission {
  id: string;
  projectKey: string;
  name: string;
  mode: LearningMode;
  content: unknown;
  missionId: number;
  criteriaId: number;
  projectRevisionId?: number;
}
interface OfflineState {
  version: 1;
  curricula: LearningCurriculum[];
  pending: PendingSubmission[];
  results: LearningSubmission[];
}

export class OfflineLearning {
  private running?: Promise<void>;
  private readonly key: string;

  constructor(scope: string, private readonly storage: Pick<Storage, "getItem" | "setItem">,
    private readonly gateway: LearningGateway, private readonly uuid = () => crypto.randomUUID()) {
    if (!scope || scope.length > 512) throw new Error("INVALID_LEARNING_SCOPE");
    this.key = `school-bot:learning:${scope}`;
  }

  read(): OfflineState {
    const raw = this.storage.getItem(this.key);
    if (!raw) return { version: 1, curricula: [], pending: [], results: [] };
    const value: unknown = JSON.parse(raw);
    if (!value || typeof value !== "object" || !("version" in value) || value.version !== 1 ||
        !("curricula" in value) || !Array.isArray(value.curricula) ||
        !("pending" in value) || !Array.isArray(value.pending) ||
        !("results" in value) || !Array.isArray(value.results)) throw new Error("INVALID_LEARNING_CACHE");
    return value as OfflineState;
  }

  private write(state: OfflineState) {
    const raw = JSON.stringify(state);
    if (raw.length > 2 * 1024 * 1024 || state.pending.length > 100) throw new Error("LEARNING_CACHE_FULL");
    this.storage.setItem(this.key, raw);
  }

  cache(curriculum: LearningCurriculum) {
    const state = this.read();
    state.curricula = [...state.curricula.filter(c => c.id !== curriculum.id), curriculum];
    this.write(state);
  }

  enqueue(input: Omit<PendingSubmission, "id" | "projectKey" | "projectRevisionId">): string {
    const state = this.read();
    const id = this.uuid();
    state.pending.push({ ...input, id, projectKey: this.uuid() });
    this.write(state);
    return id;
  }

  sync(): Promise<void> {
    if (this.running) return this.running;
    this.running = this.flush().finally(() => { this.running = undefined; });
    return this.running;
  }

  updateResult(result: LearningSubmission) {
    const state = this.read();
    state.results = [...state.results.filter(r => r.id !== result.id), result].slice(-100);
    this.write(state);
  }

  private async flush() {
    for (;;) {
      let state = this.read();
      const item = state.pending[0];
      if (!item) return;
      if (!item.projectRevisionId) {
        const uploaded = await this.gateway.request<{ project_revision_id: number }>({
          operation: "project", key: item.projectKey,
          body: { mode: item.mode, name: item.name, content: item.content },
        });
        if (!Number.isInteger(uploaded.data.project_revision_id) || uploaded.data.project_revision_id <= 0)
          throw new Error("INVALID_PROJECT_RECEIPT");
        state = this.read();
        const current = state.pending.find(p => p.id === item.id);
        if (!current) throw new Error("LEARNING_QUEUE_CHANGED");
        current.projectRevisionId = uploaded.data.project_revision_id;
        this.write(state);
        item.projectRevisionId = uploaded.data.project_revision_id;
      }
      const submitted = await this.gateway.request<LearningSubmission>({
        operation: "submission", key: item.id,
        body: { mission_id: item.missionId, criteria_id: item.criteriaId, project_revision_id: item.projectRevisionId },
      });
      if (!Number.isInteger(submitted.data.id) || submitted.data.id <= 0) throw new Error("INVALID_SUBMISSION_RECEIPT");
      state = this.read();
      state.pending = state.pending.filter(p => p.id !== item.id);
      state.results = [...state.results.filter(r => r.id !== submitted.data.id), submitted.data].slice(-100);
      this.write(state);
    }
  }
}
