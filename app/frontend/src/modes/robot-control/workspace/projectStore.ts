import { t as translate } from "../../../i18n";
import { UI_RANGES } from "../engineAdapter";
import { JOINTS } from "./types";

const isValidPose = (p: any) => {
  if (!p || typeof p.name !== "string") return false;
  if (!Array.isArray(p.v) || p.v.length !== 6) return false;
  const valuesInRange = Array.from(p.v).every((v, j) => {
    return typeof v === "number" && Number.isFinite(v) && v >= UI_RANGES[j][0] && v <= UI_RANGES[j][1];
  });
  if (!valuesInRange) return false;
  if (!Number.isInteger(p.t) || p.t < 1000 || p.t > 9000) return false;
  if (!Array.isArray(p.ts) || p.ts.length !== 6) return false;
  return Array.from(p.ts).every((t) => typeof t === "number" && Number.isInteger(t) && t >= 1000 && t <= 9000);
};

export function restoreNativeProject(value: unknown): Record<string, any> | null {
  if (value == null) return null;
  const doc = value as { kind?: unknown; version?: unknown; state?: Record<string, any> };
  if (doc.kind !== "zide-workspace" || doc.version !== 1 || !doc.state) {
    throw new Error(translate("robot.workspace.error.projectFormat"));
  }
  const s = doc.state;
  const sequenceValid = Array.isArray(s.seq)
    && s.seq.length <= 10000
    && new Set(s.seq).size === s.seq.length
    && s.poses
    && typeof s.poses === "object";
  if (!sequenceValid) throw new Error(translate("robot.workspace.error.sequenceInvalid"));
  const poses: Record<string, any> = Object.create(null);
  for (const key of s.seq) {
    const p = s.poses[key];
    if (typeof key !== "string" || !Object.hasOwn(s.poses, key) || !isValidPose(p)) {
      throw new Error(translate("robot.workspace.error.poseValuesInvalid"));
    }
    poses[key] = { name: p.name, v: [...p.v], t: p.t, ts: [...p.ts] };
  }
  const groups = Array.isArray(s.groups)
    ? s.groups
      .filter((g: any) => {
        return g
          && typeof g.id === "string"
          && typeof g.name === "string"
          && Array.isArray(g.keys)
          && g.keys.length >= 2
          && g.keys.every((k: unknown) => s.seq.includes(k));
      })
      .map((g: any) => ({ id: g.id, name: g.name, keys: [...g.keys], open: false }))
    : [];
  const nextPoseNumbers = s.seq.map((k: string) => /^p\d+$/.test(k) ? Number(k.slice(1)) + 1 : 1);
  const namesValid = Array.isArray(s.jnames)
    && s.jnames.length === 6
    && s.jnames.every((x: unknown) => typeof x === "string");
  return {
    poses,
    seq: [...s.seq],
    groups,
    counter: Math.max(Number.isInteger(s.counter) ? s.counter : 1, ...nextPoseNumbers),
    projName: typeof s.projName === "string" ? s.projName : translate("robot.workspace.project.restoredName"),
    savedSnap: typeof s.savedSnap === "string" ? s.savedSnap : "",
    savedAt: typeof s.savedAt === "string" ? s.savedAt : "",
    sel: s.seq.length ? 0 : -1,
    cur: [0, 0, 0, 0, 0, 50],
    jnames: namesValid ? s.jnames : JOINTS.map((joint) => joint.name),
    timeMode: s.timeMode === "split" ? "split" : "same",
    view: s.view === "table" ? "table" : "cards",
    loops: Number.isInteger(s.loops) && s.loops >= 1 && s.loops <= 9999 ? s.loops : 1,
    loopInf: false,
    src: "app",
    log: [],
  };
}
