import type { IconName } from "../../../components/shared/Icons";
import { t as translate } from "../../../i18n";

import { buildArmStage } from "./armStageView";
import { deriveGroupSelection } from "./groupSelection";
import { buildLogView } from "./logView";
import { buildPlaybackSummary } from "./playbackSummary";
import { derivePoseEditSummary } from "./poseEditSummary";
import { deriveListFlags, groupOfKey } from "./poseListState";
import { createPoseStatus, wrapIndexes } from "./poseStatus";
import type { WorkspaceLogic } from "./WorkspaceLogic";
import type { NativeWorkspaceView } from "./viewTypes";
import { buildWorkspaceFields } from "./workspaceFields";

type IconLabel = { c: IconName | undefined; t: string };

// Everything the field builders read, derived once per render from the screen state. Builders only read from it: the
// state repairs and handlers live in workspaceReconcile.ts and the *Actions modules.
function createWorkspaceViewContext(screen: WorkspaceLogic) {
  const S = screen.state;
  const displayNames: string[] = S.jnames.map((name: string, joint: number) => {
    const spec = screen.SV[joint];
    return name === spec.name ? translate(spec.nameKey) : name;
  });
  const flags = deriveListFlags(S);
  const status = createPoseStatus(screen, S);
  const summary = buildPlaybackSummary(screen, S, status);
  const { sp, linked, busy, lockOff, running, st, n, noSeq } = flags;
  return {
    screen,
    S,
    displayNames,
    set: (patch: Record<string, unknown>) => screen.setState(patch),
    IC: (text: string | undefined): IconLabel => screen.icoSplit(text),
    ...flags,
    isSplit: S.timeMode === "split",
    isSame: S.timeMode !== "split",
    ord: status.order,
    wrapOrd: (from: number, to: number) => wrapIndexes(n, from, to),
    grpOf: (key: string) => groupOfKey(S, key),
    ...summary,
    ...deriveGroupSelection(S),
    ...derivePoseEditSummary(screen, S),
    ...buildArmStage(screen.SV, S.jsel, displayNames),
    ...buildLogView(S),
    pOn: flags.pasteMode,
    leaveBusyNow: running || st === "moving" || st === "sending" || !!S.preflight,
    leaveDirty: !S.projName || S.savedSnap !== screen.snapOf(S),
    busyCtx: running || st === "moving" || st === "sending" || !!S.preflight || S.selMode || S.mode !== "all",
    goSelOff: busy || !sp || linked || !screen.poseOk(sp || screen.POSE0),
    sendOff: lockOff || noSeq || summary.anyBad,
  };
}

export type WorkspaceViewContext = ReturnType<typeof createWorkspaceViewContext>;

export function buildWorkspaceView(screen: WorkspaceLogic) {
  const context = createWorkspaceViewContext(screen);
  const V = buildWorkspaceFields(context);
  const icons: Record<"selTextIc" | "makeLabelIc" | "summaryHintIc" | "jointTitleIc", IconName | undefined> = {
    selTextIc: undefined,
    makeLabelIc: undefined,
    summaryHintIc: undefined,
    jointTitleIc: undefined,
  };
  for (const key of ["selText", "makeLabel", "summaryHint", "jointTitle"] as const) {
    const icon = context.IC(V[key]);
    icons[`${key}Ic`] = icon.c;
    V[key] = icon.t;
  }
  const view = Object.assign(V, icons, {
    dom: screen.dom,
    jointNote: V.jointLockShow ? "" : V.jointTitle,
    navTo: screen.menuActions.navTo,
    leaveNav: screen.menuActions.leaveNav,
    leaveSaveNav: screen.menuActions.leaveSaveNav,
    openExternal: screen.menuActions.openExternal,
    openMainMenu: screen.menuActions.openMainMenu,
  });
  return view as typeof view & NativeWorkspaceView;
}
