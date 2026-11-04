import type { Command, ModeID } from "./types";
import { UI_RANGES } from "../modes/robot-control/engineAdapter";
import { JOINTS } from "../modes/robot-control/workspace/types";

export type ProjectMode = "arm" | "code" | "block";

export interface ArmPoseRecord {
  id: string;
  name: string;
  joints: number[];
  time_ms: number;
  joint_times_ms?: number[];
}

export interface ArmGroupRecord {
  id: string;
  name: string;
  pose_ids: string[];
}

export interface ArmProjectContent {
  kind: "arm";
  arm_model: string;
  poses: ArmPoseRecord[];
  groups: ArmGroupRecord[];
  ui_state?: Record<string, unknown>;
}

export interface CodeProjectContent {
  kind: "code";
  language: "python-dsl";
  source: string;
  entry: "main";
  ui_state?: Record<string, unknown>;
}

export interface BlockProjectContent {
  kind: "block";
  workspace: Record<string, unknown>;
  compiled_preview?: {
    commands: Command[];
  };
  ui_state?: Record<string, unknown>;
}

export type ProjectContent =
  | ArmProjectContent
  | CodeProjectContent
  | BlockProjectContent;

export interface ProjectSummaryV2 {
  project_id: string;
  name: string;
  mode: ProjectMode;
  schema_version: number;
  local_revision: number;
  cloud_revision?: number;
  updated_at: string;
  deleted_at?: string;
}

export interface ProjectDocumentV2 extends ProjectSummaryV2 {
  owner_scope: string;
  created_at: string;
  expires_at?: string;
  import_digest?: string;
  content: ProjectContent;
}

export interface TrashItemV2 {
  project_id: string;
  name: string;
  mode: ProjectMode;
  local_revision: number;
  deleted_at: string;
  expires_at: string;
}

export interface ExportEnvelopeV2 {
  format: "schoolbot-project";
  schema_version: 2;
  exported_at: string;
  name: string;
  mode: ProjectMode;
  content: ProjectContent;
}

export interface ExportBundleV2 {
  file_name: string;
  envelope: ExportEnvelopeV2;
  raw_json: string;
}

export interface ImportGuestResultV2 {
  project: ProjectDocumentV2;
  already_imported: boolean;
}

export class ProjectAdapterError extends Error {
  readonly code: string;
  constructor(code: string, message: string) {
    super(message);
    this.name = "ProjectAdapterError";
    this.code = code;
  }
}

const MAX_PROJECT_NAME_LENGTH = 120;
const MAX_CODE_UTF16_UNITS = 16384;
const MAX_BLOCK_COUNT = 1000;
const MAX_BLOCK_DEPTH = 8;
const MAX_BLOCK_COMMANDS = 64;
const MAX_ARM_POSES = 10000;
const MAX_ARM_GROUPS = 256;
const TRASH_RETENTION_MS = 30 * 24 * 60 * 60 * 1000;

const ALLOWED_COMMAND_OPS = new Set([
  "move_right",
  "move_left",
  "move_up",
  "move_down",
  "jump",
  "wait",
  "interact",
]);

const FORBIDDEN_ARM_RUNTIME_KEYS = new Set([
  "connected",
  "verified",
  "is_connected",
  "is_verified",
  "device_verified",
]);

export function modeIdToProjectMode(mode: ModeID): ProjectMode {
  switch (mode) {
    case "robot-control":
      return "arm";
    case "coding-adventure":
      return "code";
    case "block-programming":
      return "block";
  }
}

export function projectModeToModeId(mode: ProjectMode): ModeID {
  switch (mode) {
    case "arm":
      return "robot-control";
    case "code":
      return "coding-adventure";
    case "block":
      return "block-programming";
  }
}

export function validateProjectName(name: string): string {
  if (typeof name !== "string") {
    throw new ProjectAdapterError("PROJECT_SCHEMA_INVALID", "Project name must be a string");
  }
  const trimmed = name.trim();
  if (!trimmed || trimmed.length > MAX_PROJECT_NAME_LENGTH) {
    throw new ProjectAdapterError("PROJECT_SCHEMA_INVALID", "Project name must be 1..120 characters");
  }
  for (let i = 0; i < trimmed.length; i++) {
    const code = trimmed.charCodeAt(i);
    if (code < 0x20 || code === 0x7f) {
      throw new ProjectAdapterError("PROJECT_SCHEMA_INVALID", "Project name contains control characters");
    }
  }
  return trimmed;
}

export function sanitizeExportFileName(name: string): string {
  const cleaned = Array.from(name.trim())
    .map((ch) => {
      if (ch.charCodeAt(0) < 0x20 || '<>:"/\\|?*'.includes(ch)) {
        return "_";
      }
      return ch;
    })
    .join("")
    .trim()
    .replace(/^[. ]+|[. ]+$/g, "");
  const stem = cleaned || "project";
  return `${stem}.schoolbot-project.json`;
}

function assertNoForbiddenArmRuntimeState(value: unknown): void {
  if (!value || typeof value !== "object") return;
  if (Array.isArray(value)) {
    for (const item of value) {
      assertNoForbiddenArmRuntimeState(item);
    }
    return;
  }
  const record = value as Record<string, unknown>;
  for (const key of Object.keys(record)) {
    if (FORBIDDEN_ARM_RUNTIME_KEYS.has(key.toLowerCase())) {
      throw new ProjectAdapterError(
        "PROJECT_RUNTIME_STATE_FORBIDDEN",
        `Arm project content must not persist runtime connection/verification key: ${key}`,
      );
    }
    assertNoForbiddenArmRuntimeState(record[key]);
  }
}

export interface ArmWorkspaceSnapshotInput {
  poses: Record<string, { name: string; v: number[]; t: number; ts: number[] }>;
  seq: string[];
  groups?: Array<{ id: string; name: string; keys: string[]; open?: boolean }>;
  counter?: number;
  projName?: string;
  savedSnap?: string;
  savedAt?: string;
  jnames?: string[];
  timeMode?: "same" | "split";
  view?: "cards" | "table";
  loops?: number;
  armModel?: string;
}

export function serializeArmProject(state: ArmWorkspaceSnapshotInput): ArmProjectContent {
  if (!state || typeof state !== "object" || !Array.isArray(state.seq) || !state.poses || typeof state.poses !== "object") {
    throw new ProjectAdapterError("PROJECT_SCHEMA_INVALID", "Arm workspace state requires seq and poses");
  }
  if (state.seq.length === 0 || state.seq.length > MAX_ARM_POSES) {
    throw new ProjectAdapterError("PROJECT_POSE_LIMIT", "Arm pose count must be 1..10000");
  }
  const seenKeys = new Set<string>();
  const poses: ArmPoseRecord[] = [];
  for (const key of state.seq) {
    if (typeof key !== "string" || !key || seenKeys.has(key)) {
      throw new ProjectAdapterError("PROJECT_SCHEMA_INVALID", "Duplicate or invalid pose key in sequence");
    }
    seenKeys.add(key);
    if (!Object.hasOwn(state.poses, key)) {
      throw new ProjectAdapterError("PROJECT_SCHEMA_INVALID", `Missing pose definition for key ${key}`);
    }
    const pose = state.poses[key];
    if (!pose || typeof pose.name !== "string" || !pose.name.trim()) {
      throw new ProjectAdapterError("PROJECT_SCHEMA_INVALID", `Pose ${key} is missing a valid name`);
    }
    if (!Array.isArray(pose.v) || pose.v.length !== 6) {
      throw new ProjectAdapterError("PROJECT_SCHEMA_INVALID", `Pose ${key} requires 6 joint values`);
    }
    for (let j = 0; j < 6; j++) {
      const val = pose.v[j];
      const [min, max] = UI_RANGES[j];
      if (typeof val !== "number" || !Number.isFinite(val) || val < min || val > max) {
        throw new ProjectAdapterError("PROJECT_SCHEMA_INVALID", `Pose ${key} joint ${j} out of range`);
      }
    }
    if (typeof pose.t !== "number" || !Number.isInteger(pose.t) || pose.t < 1000 || pose.t > 9000) {
      throw new ProjectAdapterError("PROJECT_SCHEMA_INVALID", `Pose ${key} time_ms must be 1000..9000`);
    }
    if (!Array.isArray(pose.ts) || pose.ts.length !== 6) {
      throw new ProjectAdapterError("PROJECT_SCHEMA_INVALID", `Pose ${key} requires 6 joint times`);
    }
    for (const jt of pose.ts) {
      if (typeof jt !== "number" || !Number.isInteger(jt) || jt < 1000 || jt > 9000) {
        throw new ProjectAdapterError("PROJECT_SCHEMA_INVALID", `Pose ${key} joint time out of range`);
      }
    }
    poses.push({
      id: key,
      name: pose.name.trim(),
      joints: [...pose.v],
      time_ms: pose.t,
      joint_times_ms: [...pose.ts],
    });
  }

  const groups: ArmGroupRecord[] = [];
  if (Array.isArray(state.groups)) {
    if (state.groups.length > MAX_ARM_GROUPS) {
      throw new ProjectAdapterError("PROJECT_SCHEMA_INVALID", "Too many arm pose groups");
    }
    for (const group of state.groups) {
      if (
        group &&
        typeof group.id === "string" &&
        typeof group.name === "string" &&
        Array.isArray(group.keys) &&
        group.keys.length >= 2 &&
        group.keys.every((k) => seenKeys.has(k))
      ) {
        groups.push({
          id: group.id,
          name: group.name,
          pose_ids: [...group.keys],
        });
      }
    }
  }

  const uiState: Record<string, unknown> = {
    counter: typeof state.counter === "number" && Number.isInteger(state.counter) ? state.counter : poses.length + 1,
    projName: typeof state.projName === "string" ? state.projName : "",
    savedSnap: typeof state.savedSnap === "string" ? state.savedSnap : "",
    savedAt: typeof state.savedAt === "string" ? state.savedAt : "",
    jnames: Array.isArray(state.jnames) && state.jnames.length === 6 ? [...state.jnames] : JOINTS.map((j) => j.name),
    timeMode: state.timeMode === "split" ? "split" : "same",
    view: state.view === "table" ? "table" : "cards",
    loops: typeof state.loops === "number" && Number.isInteger(state.loops) && state.loops >= 1 && state.loops <= 9999 ? state.loops : 1,
  };

  return {
    kind: "arm",
    arm_model: typeof state.armModel === "string" && state.armModel.trim() ? state.armModel.trim() : "uhand-6dof",
    poses,
    groups,
    ui_state: uiState,
  };
}

export function deserializeArmProject(content: unknown): Record<string, unknown> {
  if (!content || typeof content !== "object") {
    throw new ProjectAdapterError("PROJECT_SCHEMA_INVALID", "Arm project content must be an object");
  }
  assertNoForbiddenArmRuntimeState(content);
  const doc = content as Partial<ArmProjectContent>;
  if (doc.kind !== "arm") {
    throw new ProjectAdapterError("PROJECT_MODE_MISMATCH", "Expected kind === 'arm'");
  }
  if (typeof doc.arm_model !== "string" || !doc.arm_model.trim()) {
    throw new ProjectAdapterError("PROJECT_SCHEMA_INVALID", "arm_model is required");
  }
  if (!Array.isArray(doc.poses) || doc.poses.length === 0 || doc.poses.length > MAX_ARM_POSES) {
    throw new ProjectAdapterError("PROJECT_POSE_LIMIT", "Arm poses must contain 1..10000 items");
  }

  const poses: Record<string, { name: string; v: number[]; t: number; ts: number[] }> = Object.create(null);
  const seq: string[] = [];
  const seenIds = new Set<string>();

  for (const p of doc.poses) {
    if (!p || typeof p.id !== "string" || !p.id.trim() || seenIds.has(p.id)) {
      throw new ProjectAdapterError("PROJECT_SCHEMA_INVALID", "Invalid or duplicate pose id");
    }
    if (typeof p.name !== "string" || !p.name.trim()) {
      throw new ProjectAdapterError("PROJECT_SCHEMA_INVALID", "Pose name is required");
    }
    if (!Array.isArray(p.joints) || p.joints.length !== 6) {
      throw new ProjectAdapterError("PROJECT_SCHEMA_INVALID", "Pose joints must have 6 values");
    }
    for (let j = 0; j < 6; j++) {
      const v = p.joints[j];
      const [min, max] = UI_RANGES[j];
      if (typeof v !== "number" || !Number.isFinite(v) || v < min || v > max) {
        throw new ProjectAdapterError("PROJECT_SCHEMA_INVALID", `Pose ${p.id} joint ${j} out of range`);
      }
    }
    const timeMs = typeof p.time_ms === "number" && Number.isInteger(p.time_ms) ? p.time_ms : 1000;
    if (timeMs < 1000 || timeMs > 9000) {
      throw new ProjectAdapterError("PROJECT_SCHEMA_INVALID", `Pose ${p.id} time_ms out of range`);
    }
    const jointTimes = Array.isArray(p.joint_times_ms) && p.joint_times_ms.length === 6
      ? p.joint_times_ms
      : [timeMs, timeMs, timeMs, timeMs, timeMs, timeMs];
    for (const jt of jointTimes) {
      if (typeof jt !== "number" || !Number.isInteger(jt) || jt < 1000 || jt > 9000) {
        throw new ProjectAdapterError("PROJECT_SCHEMA_INVALID", `Pose ${p.id} joint_times_ms out of range`);
      }
    }
    seenIds.add(p.id);
    seq.push(p.id);
    poses[p.id] = {
      name: p.name,
      v: [...p.joints],
      t: timeMs,
      ts: [...jointTimes],
    };
  }

  const groups: Array<{ id: string; name: string; keys: string[]; open: boolean }> = [];
  if (doc.groups !== undefined) {
    if (!Array.isArray(doc.groups) || doc.groups.length > MAX_ARM_GROUPS) {
      throw new ProjectAdapterError("PROJECT_SCHEMA_INVALID", "Invalid arm groups array");
    }
    for (const g of doc.groups) {
      if (
        !g ||
        typeof g.id !== "string" ||
        !g.id.trim() ||
        typeof g.name !== "string" ||
        !g.name.trim() ||
        !Array.isArray(g.pose_ids) ||
        g.pose_ids.length === 0 ||
        g.pose_ids.some((id) => !seenIds.has(id))
      ) {
        throw new ProjectAdapterError("PROJECT_SCHEMA_INVALID", "Invalid arm group reference");
      }
      groups.push({
        id: g.id,
        name: g.name,
        keys: [...g.pose_ids],
        open: false,
      });
    }
  }

  const ui = (doc.ui_state && typeof doc.ui_state === "object" ? doc.ui_state : {}) as Record<string, unknown>;
  const nextPoseNumbers = seq.map((k) => (/^p\d+$/.test(k) ? Number(k.slice(1)) + 1 : 1));
  const namesValid =
    Array.isArray(ui.jnames) &&
    ui.jnames.length === 6 &&
    ui.jnames.every((x) => typeof x === "string");

  return {
    poses,
    seq,
    groups,
    counter: Math.max(
      typeof ui.counter === "number" && Number.isInteger(ui.counter) ? ui.counter : 1,
      ...nextPoseNumbers,
    ),
    projName: typeof ui.projName === "string" ? ui.projName : "",
    savedSnap: typeof ui.savedSnap === "string" ? ui.savedSnap : "",
    savedAt: typeof ui.savedAt === "string" ? ui.savedAt : "",
    sel: seq.length ? 0 : -1,
    cur: [0, 0, 0, 0, 0, 50],
    jnames: namesValid ? [...(ui.jnames as string[])] : JOINTS.map((joint) => joint.name),
    timeMode: ui.timeMode === "split" ? "split" : "same",
    view: ui.view === "table" ? "table" : "cards",
    loops: typeof ui.loops === "number" && Number.isInteger(ui.loops) && ui.loops >= 1 && ui.loops <= 9999 ? ui.loops : 1,
    loopInf: false,
    src: "app",
    log: [],
    robotSeq: null,
    synced: false,
    linked: -1,
    snap: null,
  };
}

export interface CodeWorkspaceState {
  stageId: number;
  code: string;
}

export function serializeCodeProject(state: CodeWorkspaceState): CodeProjectContent {
  if (!state || typeof state !== "object" || typeof state.code !== "string") {
    throw new ProjectAdapterError("PROJECT_SCHEMA_INVALID", "Code workspace state requires string code");
  }
  if (!state.code.trim()) {
    throw new ProjectAdapterError("PROJECT_SCHEMA_INVALID", "Code source must not be empty");
  }
  if (state.code.length > MAX_CODE_UTF16_UNITS) {
    throw new ProjectAdapterError("PROJECT_CODE_TOO_LARGE", "Code source exceeds 16384 UTF-16 code units");
  }
  const stageId = typeof state.stageId === "number" && Number.isInteger(state.stageId) && state.stageId >= 1
    ? state.stageId
    : 1;
  return {
    kind: "code",
    language: "python-dsl",
    source: state.code,
    entry: "main",
    ui_state: { stage_id: stageId },
  };
}

export function deserializeCodeProject(content: unknown): CodeWorkspaceState {
  if (!content || typeof content !== "object") {
    throw new ProjectAdapterError("PROJECT_SCHEMA_INVALID", "Code project content must be an object");
  }
  const doc = content as Partial<CodeProjectContent>;
  if (doc.kind !== "code") {
    throw new ProjectAdapterError("PROJECT_MODE_MISMATCH", "Expected kind === 'code'");
  }
  if (doc.language !== "python-dsl") {
    throw new ProjectAdapterError("PROJECT_SCHEMA_INVALID", "Unsupported code language");
  }
  if (doc.entry !== undefined && doc.entry !== "main") {
    throw new ProjectAdapterError("PROJECT_SCHEMA_INVALID", "Unsupported code entry point");
  }
  if (typeof doc.source !== "string" || !doc.source.trim()) {
    throw new ProjectAdapterError("PROJECT_SCHEMA_INVALID", "Code source must be a non-empty string");
  }
  if (doc.source.length > MAX_CODE_UTF16_UNITS) {
    throw new ProjectAdapterError("PROJECT_CODE_TOO_LARGE", "Code source exceeds 16384 UTF-16 code units");
  }
  const ui = (doc.ui_state && typeof doc.ui_state === "object" ? doc.ui_state : {}) as Record<string, unknown>;
  const stageId = typeof ui.stage_id === "number" && Number.isInteger(ui.stage_id) && ui.stage_id >= 1
    ? ui.stage_id
    : 1;
  return {
    stageId,
    code: doc.source,
  };
}

export interface BlockWorkspaceState {
  stageId: number;
  workspace: Record<string, unknown>;
  commands: Command[];
}

function measureBlockWorkspace(root: Record<string, unknown>): { count: number; depth: number } {
  let count = 0;
  let maxDepth = 0;

  function visitBlock(block: unknown, depth: number): void {
    if (!block || typeof block !== "object" || Array.isArray(block)) return;
    count++;
    if (depth > maxDepth) maxDepth = depth;
    const rec = block as Record<string, unknown>;
    if (rec.inputs && typeof rec.inputs === "object" && !Array.isArray(rec.inputs)) {
      for (const input of Object.values(rec.inputs as Record<string, unknown>)) {
        if (input && typeof input === "object" && !Array.isArray(input)) {
          const inp = input as Record<string, unknown>;
          if (inp.block) visitBlock(inp.block, depth + 1);
          if (inp.shadow) visitBlock(inp.shadow, depth + 1);
        }
      }
    }
    if (Array.isArray(rec.children)) {
      for (const child of rec.children) {
        visitBlock(child, depth + 1);
      }
    }
    if (rec.next && typeof rec.next === "object" && !Array.isArray(rec.next)) {
      const nextRec = rec.next as Record<string, unknown>;
      if (nextRec.block) {
        visitBlock(nextRec.block, depth);
      } else {
        visitBlock(rec.next, depth);
      }
    }
  }

  const blocksContainer = root.blocks;
  if (Array.isArray(blocksContainer)) {
    for (const b of blocksContainer) visitBlock(b, 1);
  } else if (blocksContainer && typeof blocksContainer === "object") {
    const inner = (blocksContainer as Record<string, unknown>).blocks;
    if (Array.isArray(inner)) {
      for (const b of inner) visitBlock(b, 1);
    } else {
      visitBlock(blocksContainer, 1);
    }
  } else if (typeof root.type === "string") {
    visitBlock(root, 1);
  }
  return { count, depth: maxDepth };
}

function validateCommands(commands: Command[]): Command[] {
  if (!Array.isArray(commands) || commands.length > MAX_BLOCK_COMMANDS) {
    throw new ProjectAdapterError("PROJECT_BLOCK_LIMIT", "Block compiled_preview commands exceed 64");
  }
  return commands.map((cmd) => {
    if (!cmd || typeof cmd.op !== "string" || !ALLOWED_COMMAND_OPS.has(cmd.op)) {
      throw new ProjectAdapterError("PROJECT_SCHEMA_INVALID", `Unsupported block command op: ${String(cmd?.op)}`);
    }
    if (cmd.value !== undefined && (typeof cmd.value !== "number" || !Number.isInteger(cmd.value) || cmd.value < 0)) {
      throw new ProjectAdapterError("PROJECT_SCHEMA_INVALID", "Invalid command value");
    }
    return cmd.value !== undefined ? { op: cmd.op, value: cmd.value } : { op: cmd.op };
  });
}

export function serializeBlockProject(state: BlockWorkspaceState): BlockProjectContent {
  if (!state || typeof state !== "object" || !state.workspace || typeof state.workspace !== "object" || Array.isArray(state.workspace)) {
    throw new ProjectAdapterError("PROJECT_SCHEMA_INVALID", "Block workspace state requires a workspace object");
  }
  const { count, depth } = measureBlockWorkspace(state.workspace);
  if (count > MAX_BLOCK_COUNT || depth > MAX_BLOCK_DEPTH) {
    throw new ProjectAdapterError("PROJECT_BLOCK_LIMIT", "Block workspace exceeds block count (1000) or nesting depth (8)");
  }
  const commands = validateCommands(state.commands ?? []);
  const stageId = typeof state.stageId === "number" && Number.isInteger(state.stageId) && state.stageId >= 1
    ? state.stageId
    : 1;
  return {
    kind: "block",
    workspace: structuredClone(state.workspace),
    compiled_preview: { commands },
    ui_state: { stage_id: stageId },
  };
}

export function deserializeBlockProject(content: unknown): BlockWorkspaceState {
  if (!content || typeof content !== "object") {
    throw new ProjectAdapterError("PROJECT_SCHEMA_INVALID", "Block project content must be an object");
  }
  const doc = content as Partial<BlockProjectContent>;
  if (doc.kind !== "block") {
    throw new ProjectAdapterError("PROJECT_MODE_MISMATCH", "Expected kind === 'block'");
  }
  if (!doc.workspace || typeof doc.workspace !== "object" || Array.isArray(doc.workspace)) {
    throw new ProjectAdapterError("PROJECT_SCHEMA_INVALID", "Block workspace must be an object");
  }
  const { count, depth } = measureBlockWorkspace(doc.workspace);
  if (count > MAX_BLOCK_COUNT || depth > MAX_BLOCK_DEPTH) {
    throw new ProjectAdapterError("PROJECT_BLOCK_LIMIT", "Block workspace exceeds block count (1000) or nesting depth (8)");
  }
  const rawCommands = doc.compiled_preview?.commands ?? [];
  const commands = validateCommands(rawCommands);
  const ui = (doc.ui_state && typeof doc.ui_state === "object" ? doc.ui_state : {}) as Record<string, unknown>;
  const stageId = typeof ui.stage_id === "number" && Number.isInteger(ui.stage_id) && ui.stage_id >= 1
    ? ui.stage_id
    : 1;
  return {
    stageId,
    workspace: structuredClone(doc.workspace),
    commands,
  };
}

export function validateProjectContentForMode(mode: ProjectMode, content: ProjectContent): void {
  if (!content || typeof content !== "object" || content.kind !== mode) {
    throw new ProjectAdapterError("PROJECT_MODE_MISMATCH", `Project content kind must match mode ${mode}`);
  }
  switch (mode) {
    case "arm":
      deserializeArmProject(content);
      return;
    case "code":
      deserializeCodeProject(content);
      return;
    case "block":
      deserializeBlockProject(content);
      return;
  }
}

export async function safeLoadWithFallback<TState>(
  loadFn: () => Promise<TState>,
  currentState: TState,
): Promise<{ ok: true; state: TState } | { ok: false; state: TState; error: string }> {
  const snapshot = structuredClone(currentState);
  try {
    const next = await loadFn();
    return { ok: true, state: next };
  } catch (err) {
    return {
      ok: false,
      state: snapshot,
      error: err instanceof Error ? err.message : String(err),
    };
  }
}

export async function safeSavePreservingState<TState>(
  saveFn: (state: TState) => Promise<ProjectDocumentV2>,
  currentState: TState,
): Promise<{ ok: true; state: TState; project: ProjectDocumentV2 } | { ok: false; state: TState; error: string }> {
  const snapshot = structuredClone(currentState);
  try {
    const project = await saveFn(snapshot);
    return { ok: true, state: snapshot, project };
  } catch (err) {
    return {
      ok: false,
      state: snapshot,
      error: err instanceof Error ? err.message : String(err),
    };
  }
}

function computeSimpleDigest(mode: ProjectMode, name: string, content: ProjectContent): string {
  const canonical = JSON.stringify({ mode, name: name.trim(), content });
  let h1 = 0xdeadbeef ^ canonical.length;
  let h2 = 0x41c6ce57 ^ canonical.length;
  for (let i = 0; i < canonical.length; i++) {
    const ch = canonical.charCodeAt(i);
    h1 = Math.imul(h1 ^ ch, 2654435761);
    h2 = Math.imul(h2 ^ ch, 1597334677);
  }
  h1 = Math.imul(h1 ^ (h1 >>> 16), 2246822507) ^ Math.imul(h2 ^ (h2 >>> 13), 3266489909);
  h2 = Math.imul(h2 ^ (h2 >>> 16), 2246822507) ^ Math.imul(h1 ^ (h1 >>> 13), 3266489909);
  return (h1 >>> 0).toString(16).padStart(8, "0") + (h2 >>> 0).toString(16).padStart(8, "0");
}

export interface ProjectLibraryBackend {
  listProjects: (ownerScope: string, mode?: ProjectMode | "") => Promise<ProjectSummaryV2[]>;
  createProject: (ownerScope: string, name: string, mode: ProjectMode, content: ProjectContent) => Promise<ProjectDocumentV2>;
  getProject: (ownerScope: string, projectId: string) => Promise<ProjectDocumentV2>;
  saveProject: (
    ownerScope: string,
    projectId: string,
    expectedLocalRevision: number,
    name: string,
    mode: ProjectMode,
    content: ProjectContent,
  ) => Promise<ProjectDocumentV2>;
  copyProject: (ownerScope: string, sourceProjectId: string, newName: string) => Promise<ProjectDocumentV2>;
  deleteProject: (ownerScope: string, projectId: string) => Promise<TrashItemV2>;
  listTrash: (ownerScope: string) => Promise<TrashItemV2[]>;
  restoreProject: (ownerScope: string, projectId: string) => Promise<ProjectDocumentV2>;
  exportProject: (ownerScope: string, projectId: string) => Promise<ExportBundleV2>;
  openExportedProject: (ownerScope: string, rawJson: string) => Promise<ProjectDocumentV2>;
  importGuestProject: (userScope: string, guestProjectId: string, idempotencyKey: string) => Promise<ImportGuestResultV2>;
}

export class InMemoryProjectLibraryBackend implements ProjectLibraryBackend {
  private readonly docs = new Map<string, ProjectDocumentV2>();
  private readonly idempotencyMap = new Map<string, string>();
  private readonly digestMap = new Map<string, string>();
  private clock: () => Date;

  constructor(clock: () => Date = () => new Date()) {
    this.clock = clock;
  }

  setClock(clock: () => Date): void {
    this.clock = clock;
  }

  private key(ownerScope: string, projectId: string): string {
    return `${ownerScope}::${projectId}`;
  }

  private normalizeScope(ownerScope: string): string {
    const trimmed = ownerScope.trim();
    return trimmed || "guest";
  }

  async listProjects(ownerScope: string, mode?: ProjectMode | ""): Promise<ProjectSummaryV2[]> {
    const scope = this.normalizeScope(ownerScope);
    const items: ProjectSummaryV2[] = [];
    for (const doc of this.docs.values()) {
      if (doc.owner_scope !== scope || doc.deleted_at) continue;
      if (mode && doc.mode !== mode) continue;
      items.push({
        project_id: doc.project_id,
        name: doc.name,
        mode: doc.mode,
        schema_version: doc.schema_version,
        local_revision: doc.local_revision,
        cloud_revision: doc.cloud_revision,
        updated_at: doc.updated_at,
      });
    }
    items.sort((a, b) => b.updated_at.localeCompare(a.updated_at));
    return items;
  }

  async createProject(
    ownerScope: string,
    name: string,
    mode: ProjectMode,
    content: ProjectContent,
  ): Promise<ProjectDocumentV2> {
    const scope = this.normalizeScope(ownerScope);
    const validName = validateProjectName(name);
    validateProjectContentForMode(mode, content);
    const now = this.clock().toISOString();
    const projectId = crypto.randomUUID();
    const doc: ProjectDocumentV2 = {
      project_id: projectId,
      owner_scope: scope,
      name: validName,
      mode,
      schema_version: 2,
      local_revision: 1,
      created_at: now,
      updated_at: now,
      content: structuredClone(content),
    };
    this.docs.set(this.key(scope, projectId), doc);
    return structuredClone(doc);
  }

  async getProject(ownerScope: string, projectId: string): Promise<ProjectDocumentV2> {
    const scope = this.normalizeScope(ownerScope);
    const doc = this.docs.get(this.key(scope, projectId));
    if (!doc || doc.deleted_at) {
      throw new ProjectAdapterError("PROJECT_NOT_FOUND", `Project ${projectId} not found`);
    }
    return structuredClone(doc);
  }

  async saveProject(
    ownerScope: string,
    projectId: string,
    expectedLocalRevision: number,
    name: string,
    mode: ProjectMode,
    content: ProjectContent,
  ): Promise<ProjectDocumentV2> {
    const scope = this.normalizeScope(ownerScope);
    const existing = this.docs.get(this.key(scope, projectId));
    if (!existing || existing.deleted_at) {
      throw new ProjectAdapterError("PROJECT_NOT_FOUND", `Project ${projectId} not found`);
    }
    if (existing.mode !== mode) {
      throw new ProjectAdapterError("PROJECT_MODE_MISMATCH", "Cannot change project mode");
    }
    if (existing.local_revision !== expectedLocalRevision) {
      throw new ProjectAdapterError("PROJECT_REVISION_CONFLICT", "Project local_revision conflict");
    }
    const validName = validateProjectName(name);
    validateProjectContentForMode(mode, content);
    const updated: ProjectDocumentV2 = {
      ...existing,
      name: validName,
      local_revision: existing.local_revision + 1,
      updated_at: this.clock().toISOString(),
      content: structuredClone(content),
    };
    this.docs.set(this.key(scope, projectId), updated);
    return structuredClone(updated);
  }

  async copyProject(ownerScope: string, sourceProjectId: string, newName: string): Promise<ProjectDocumentV2> {
    const source = await this.getProject(ownerScope, sourceProjectId);
    const targetName = newName.trim() ? validateProjectName(newName) : validateProjectName(`${source.name} (Copy)`);
    return this.createProject(ownerScope, targetName, source.mode, source.content);
  }

  async deleteProject(ownerScope: string, projectId: string): Promise<TrashItemV2> {
    const scope = this.normalizeScope(ownerScope);
    const existing = this.docs.get(this.key(scope, projectId));
    if (!existing || existing.deleted_at) {
      throw new ProjectAdapterError("PROJECT_NOT_FOUND", `Project ${projectId} not found`);
    }
    const deletedAtDate = this.clock();
    const expiresAtDate = new Date(deletedAtDate.getTime() + TRASH_RETENTION_MS);
    const deletedAt = deletedAtDate.toISOString();
    const expiresAt = expiresAtDate.toISOString();
    existing.deleted_at = deletedAt;
    existing.expires_at = expiresAt;
    existing.updated_at = deletedAt;
    return {
      project_id: existing.project_id,
      name: existing.name,
      mode: existing.mode,
      local_revision: existing.local_revision,
      deleted_at: deletedAt,
      expires_at: expiresAt,
    };
  }

  async listTrash(ownerScope: string): Promise<TrashItemV2[]> {
    const scope = this.normalizeScope(ownerScope);
    const nowMs = this.clock().getTime();
    const items: TrashItemV2[] = [];
    for (const [mapKey, doc] of this.docs.entries()) {
      if (doc.owner_scope !== scope || !doc.deleted_at || !doc.expires_at) continue;
      if (nowMs >= Date.parse(doc.expires_at)) {
        this.docs.delete(mapKey);
        continue;
      }
      items.push({
        project_id: doc.project_id,
        name: doc.name,
        mode: doc.mode,
        local_revision: doc.local_revision,
        deleted_at: doc.deleted_at,
        expires_at: doc.expires_at,
      });
    }
    items.sort((a, b) => b.deleted_at.localeCompare(a.deleted_at));
    return items;
  }

  async restoreProject(ownerScope: string, projectId: string): Promise<ProjectDocumentV2> {
    const scope = this.normalizeScope(ownerScope);
    const mapKey = this.key(scope, projectId);
    const existing = this.docs.get(mapKey);
    if (!existing || !existing.deleted_at || !existing.expires_at) {
      throw new ProjectAdapterError("PROJECT_NOT_FOUND", `Trashed project ${projectId} not found`);
    }
    if (this.clock().getTime() >= Date.parse(existing.expires_at)) {
      this.docs.delete(mapKey);
      throw new ProjectAdapterError("PROJECT_TRASH_EXPIRED", "Project trash retention of 30 days has expired");
    }
    delete existing.deleted_at;
    delete existing.expires_at;
    existing.updated_at = this.clock().toISOString();
    return structuredClone(existing);
  }

  async exportProject(ownerScope: string, projectId: string): Promise<ExportBundleV2> {
    const doc = await this.getProject(ownerScope, projectId);
    validateProjectContentForMode(doc.mode, doc.content);
    const envelope: ExportEnvelopeV2 = {
      format: "schoolbot-project",
      schema_version: 2,
      exported_at: this.clock().toISOString(),
      name: doc.name,
      mode: doc.mode,
      content: structuredClone(doc.content),
    };
    return {
      file_name: sanitizeExportFileName(doc.name),
      envelope,
      raw_json: JSON.stringify(envelope, null, 2),
    };
  }

  async openExportedProject(ownerScope: string, rawJson: string): Promise<ProjectDocumentV2> {
    let parsed: unknown;
    try {
      parsed = JSON.parse(rawJson);
    } catch {
      throw new ProjectAdapterError("PROJECT_EXPORT_INVALID", "Invalid JSON in exported project file");
    }
    if (!parsed || typeof parsed !== "object") {
      throw new ProjectAdapterError("PROJECT_EXPORT_INVALID", "Exported project must be a JSON object");
    }
    const env = parsed as Partial<ExportEnvelopeV2>;
    if (env.format !== "schoolbot-project" || env.schema_version !== 2 || !env.mode || !env.content) {
      throw new ProjectAdapterError("PROJECT_EXPORT_INVALID", "Unsupported export format or schema_version");
    }
    const validName = validateProjectName(env.name ?? "");
    validateProjectContentForMode(env.mode, env.content);
    const created = await this.createProject(ownerScope, validName, env.mode, env.content);
    created.import_digest = computeSimpleDigest(env.mode, validName, env.content);
    this.docs.set(this.key(this.normalizeScope(ownerScope), created.project_id), structuredClone(created));
    return created;
  }

  async importGuestProject(
    userScope: string,
    guestProjectId: string,
    idempotencyKey: string,
  ): Promise<ImportGuestResultV2> {
    const normalizedUser = userScope.trim();
    if (!normalizedUser || normalizedUser === "guest") {
      throw new ProjectAdapterError("PROJECT_IMPORT_UNAUTHORIZED", "Authenticated userScope is required for import");
    }
    const trimmedKey = idempotencyKey.trim();
    if (!trimmedKey) {
      throw new ProjectAdapterError("PROJECT_SCHEMA_INVALID", "idempotencyKey is required for guest import");
    }
    const guestDoc = await this.getProject("guest", guestProjectId);
    const digest = computeSimpleDigest(guestDoc.mode, guestDoc.name, guestDoc.content);

    const idemLookup = `${normalizedUser}::idem::${trimmedKey}`;
    const digestLookup = `${normalizedUser}::digest::${guestProjectId}::${digest}`;
    const existingId = this.idempotencyMap.get(idemLookup) ?? this.digestMap.get(digestLookup);
    if (existingId) {
      const existingDoc = this.docs.get(this.key(normalizedUser, existingId));
      if (existingDoc && !existingDoc.deleted_at) {
        return { project: structuredClone(existingDoc), already_imported: true };
      }
    }

    const imported = await this.createProject(normalizedUser, guestDoc.name, guestDoc.mode, guestDoc.content);
    imported.cloud_revision = 1;
    imported.import_digest = digest;
    this.docs.set(this.key(normalizedUser, imported.project_id), structuredClone(imported));
    this.idempotencyMap.set(idemLookup, imported.project_id);
    this.digestMap.set(digestLookup, imported.project_id);
    return { project: imported, already_imported: false };
  }
}
