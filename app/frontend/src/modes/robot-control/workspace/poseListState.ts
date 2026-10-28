import { deriveWorkspaceFlags, type FlagState } from "./workspaceFlags";

export type PoseGroup = { id: string; name: string; keys: string[]; open: boolean };

export type PoseClip = { keys: string[] | null; poses: unknown[] };

export type PoseRename = { k: "p"; i: number } | { k: "j"; j: number };

export type PoseListState = FlagState & {
  groups: PoseGroup[];
  picks: string[];
  selMode: boolean;
  grpNaming: boolean;
  gFocus: string | null;
  gMenu: string | null;
  gRen: string | null;
  gAnim: { id: string; mode: "in" | "out" | "pop" } | null;
  picking: boolean;
  rangeDone: boolean;
  rs: number;
  re: number;
  hoverIdx: number;
  dragFrom: number;
  dropGap: number;
  movePick: number;
  landed: { key: string; from: number } | null;
  pClip: PoseClip | null;
  pMode: boolean;
  menuIdx: number;
  delAsk: boolean;
  timeEdit: number;
  renaming: PoseRename | null;
  holding: number | null;
  cur: number[];
  view: "cards" | "table";
  timeMode: "same" | "split";
  ctx: { gid: string; x: number; y: number } | null;
  dropInto6: string | null;
  dragG6: string | null;
};

export function groupOfKey(state: PoseListState, key: string): PoseGroup | null {
  return state.groups.find((group) => group.keys.includes(key)) ?? null;
}

// Mode flags shared by every handler and view of the pose list, so both read "what is the user doing" the same way.
export function deriveListFlags(state: PoseListState) {
  const flags = deriveWorkspaceFlags(state);
  const isRange = state.mode === "range";
  const isOrder = state.mode === "order";
  const pasteMode = !!state.pClip && !!state.pMode && !isOrder && !isRange && !state.selMode && !flags.running;
  const dragOn = isOrder && state.dragFrom >= 0;
  return { ...flags, isRange, isOrder, pasteMode, dragOn };
}

export function groupIndexes(state: PoseListState, group: PoseGroup): number[] {
  return group.keys.map((key) => state.seq.indexOf(key));
}
