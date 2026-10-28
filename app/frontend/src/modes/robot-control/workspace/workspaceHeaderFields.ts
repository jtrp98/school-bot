import type { MouseEvent } from "react";
import { t as translate } from "../../../i18n";

import type { WorkspaceFieldsContext } from "./workspaceFields";

export function buildHeaderFields(context: WorkspaceFieldsContext) {
 const { screen, S, st, set, isDisc, running, busy, seq, n, sp, linked, needGo, jogOff, lockOff, isSplit, isSame, armDots, armTag, isRange, isOrder, grpOf, logFilters, logItems, sendOff, joints } = context;
 const jointTitle = (): string => {
   if (S.mode === "range") {
     return translate("robot.workspace.messages.selectingRangePressPlayOrBeforeEditingPoses");
   }
   if (S.mode === "order") {
     return translate("robot.workspace.messages.reorderingPressDoneBeforeEditingPoses");
   }
   if (!sp) {
     return translate(S.p0sel
       ? "robot.workspace.selection.viewingZero"
       : "robot.workspace.selection.chooseToEdit");
   }
   const group = grpOf(seq[S.sel]);
   let key = "robot.workspace.editing.viewingPose";
   if (linked) {
     key = group ? "robot.workspace.editing.linkedGroupPose" : "robot.workspace.editing.linkedPose";
   } else if (group) {
     key = "robot.workspace.editing.viewingGroupPose";
   }
   return translate(key, { poseNumber: S.sel + 1, groupName: group?.name });
 };
 const syncText = (): string => {
   if (S.release) {
     if (!sp) return translate("robot.workspace.capture.choosePose");
     return translate("robot.workspace.capture.positioning", { poseNumber: S.sel + 1 });
   }
   if (running) return translate("robot.workspace.messages.playing");
   if (!sp) return translate("robot.workspace.sequence.addToBegin");
   if (S.linked === S.sel) return translate("robot.workspace.position.atPose");
   if (S.linked >= 0) {
     return translate("robot.workspace.position.atOtherPose", { poseNumber: S.linked + 1 });
   }
   return translate("robot.workspace.position.notAtPose");
 };
 return {
pageBg: 'var(--bg)',
notDisc: !isDisc,
isDisc,
reconnectLabel: st === 'connecting' ? translate("robot.workspace.messages.connecting") : translate("robot.workspace.messages.reconnect"),
reconnect: () => { if (screen.props.onReconnect) { screen.props.onReconnect(); return; } set({ st: 'connecting' }); clearTimeout(screen.wait); screen.wait = screen.tm(() => screen.setState({ st: 'ready' }), 1200); },
baudOpen: S.baud,
showBaud: () => set({ baud: true }),
hideBaud: () => set({ baud: false }),
toggleBaud: () => set({ baud: !S.baud }),
closeLog: () => set({ logOpen: false }),
logOpen: S.logOpen,
logFilters,
logItems,
renameDraft: S.renameDraft,
sbShow: S.sb.show,
sbStyle: 'position: absolute; right: 3px; width: 8px; top: ' + S.sb.top + 'px; height: ' + S.sb.h + 'px; border-radius: 999px; background: rgba(var(--ink-rgb), ' + (screen.sbDrag ? 0.55 : 0.32) + '); z-index: 30; cursor: grab; touch-action: none; transition: opacity 0.2s; opacity: ' + (S.sb.vis ? 1 : 0),
sbScroll: (e?: any) => { screen.sEl = e.currentTarget; screen.sbMeasure(true); screen.sbHideSoon(); },
sbEnter: () => { screen.sbOver = true; screen.sbMeasure(true); },
sbLeave: () => { screen.sbOver = false; screen.sbHideSoon(); },
sbDown: (e?: any) => {
        const el = screen.sEl; if (!el) return; e.preventDefault(); e.stopPropagation();
        const y0 = e.clientY, t0 = el.scrollTop, sh = el.scrollHeight, ch = el.clientHeight, h = screen.state.sb.h, k = (sh - ch) / Math.max(1, ch - h - 8);
        screen.sbDrag = true; screen.sbMeasure(true);
        const mv = (ev?: any) => { el.scrollTop = t0 + (ev.clientY - y0) * k; screen.sbMeasure(true); };
        const up = () => { screen.sbDrag = false; window.removeEventListener('pointermove', mv); window.removeEventListener('pointerup', up); screen.sbMeasure(true); screen.sbHideSoon(); };
        window.addEventListener('pointermove', mv); window.addEventListener('pointerup', up);
      },
showSelPanel: !!screen.props.showSelPanel && (!!sp || S.p0sel || !!screen.props.showEmptySel),
armDots,
armTag,
showArmTag: !!screen.props.showArmTag,
joints,
jointActions: screen.jointActions,
selectedPoseActions: screen.selectedPoseActions,
jointPanelActions: screen.jointPanelActions,
poseActions: screen.poseActions,
jogOff,
jogAria: busy || (jogOff && !needGo),
jogDis: jogOff && !((needGo || !!screen.lockKind()) && !(st === 'moving' || st === 'sending' || S.preflight || S.far || S.mode === 'range' || S.mode === 'order' || S.p0sel)),
lockOff,
isSplit,
isSame,
stop: (e?: any) => { if (e && e.stopPropagation) e.stopPropagation(); },
jointTitle: jointTitle(),
jEmptyCls: (!sp && n === 0 && !busy) ? 'aw-jempty' : '',
main6Style: 'flex-shrink: 0; box-sizing: border-box; padding: 12px 24px 0; display: ' + ((S.view === 'table' && S.t6full) ? 'none' : 'flex'),
up6Style: 'margin-left: auto; display: inline-flex; align-items: center; gap: 6px; height: 36px; padding: 0 12px; border-radius: 10px; box-sizing: border-box; font-family: inherit; font-size: 13px; font-weight: 700; white-space: nowrap; cursor: pointer; ' + (sendOff ? 'border: 2px solid var(--off); background: var(--sunken); color: var(--faint); cursor: not-allowed' : 'border: 2px solid var(--line-strong); border-bottom-width: 4px; background: var(--surface); color: var(--blue-ink)'),
relFlag6: (S.release && S.view === 'table' && S.t6full) ? '1' : '0',
f6StripStyle: 'position: absolute; left: 50%; bottom: 14px; transform: translateX(-50%); z-index: 30; display: flex; align-items: center; gap: 10px; background: var(--surface); border-radius: 999px; padding: 5px; box-shadow: ' + (S.release ? '0 0 0 3px var(--orange-line), ' : '') + '0 6px 18px rgba(var(--blue-ink-rgb), 0.18)',
rel6: !!S.release && S.view === 'table' && !!S.t6full && !S.selMode,
jsel6: String(S.jsel),
f6Name: sp ? (sp.name || translate("robot.workspace.messages.unnamedPose")) : (S.p0sel ? translate("robot.workspace.messages.uprightArm") : (n ? translate("robot.workspace.messages.noPoseSelected") : translate("robot.workspace.messages.noPosesYet"))),
f6No: sp ? translate("robot.workspace.messages.poseValue0OfValue1", {value0: (S.sel + 1), value1: n}) : (S.p0sel ? translate("robot.workspace.messages.pose0") : '—'),
yes6: true,
switch6Tip: busy ? translate("robot.workspace.messages.stopTheRobotBeforeChangingRobots") : translate("robot.workspace.messages.chooseARobot"),
switch6Style: 'display: inline-flex; align-items: center; height: 36px; padding: 0 12px; border: 2px solid var(--line-strong); border-bottom-width: 4px; border-radius: 10px; background: var(--surface); font-size: 13px; font-weight: 700; color: ' + (busy ? 'var(--faint); pointer-events: none; opacity: 0.6' : 'var(--blue-ink)') + '; text-decoration: none; white-space: nowrap; box-sizing: border-box',
p0m6Open: (event: MouseEvent<HTMLElement>) => screen.menuActions.openPoseZeroMenu(event, lockOff),
f6Sync: syncText(),
f6SyncStyle: 'font-size: 12px; font-weight: 700; color: ' + (S.release ? 'var(--orange-press)' : running ? 'var(--blue-ink)' : (sp && S.linked === S.sel ? 'var(--ok)' : 'var(--orange-deep)')),
f6StripClick: (e?: any) => { if (!S.preflight) return; if (e && e.stopPropagation) e.stopPropagation(); set({ preflightShakes: (S.preflightShakes || 0) + 1 }); },
f6Prev: () => { if (S.preflight) return; if (!running && n) { const i = Math.max(0, (S.sel < 0 ? 0 : S.sel - 1)); screen.select(i); screen.scrollTo6(i); } },
f6Next: () => { if (S.preflight) return; if (!running && n) { const i = Math.min(n - 1, S.sel + 1); screen.select(i); screen.scrollTo6(i); } },
f6PrevOff: running || !n || S.sel <= 0,
f6NextOff: running || !n || S.sel >= n - 1,
f6BtnStyle: 'width: 44px; height: 44px; border-radius: 999px; border: 2px solid var(--line-strong); border-bottom-width: 4px; background: var(--surface); color: var(--blue-ink); font-family: inherit; font-size: 20px; font-weight: 800; cursor: pointer',
rootClick: (e?: any) => { if (!(!sp && n === 0 && !busy)) return; const t = e && e.target; if (!t || !t.closest) return; if (t.closest('header')) return; if (t.closest('button:not(:disabled), a, select, input:not(:disabled), textarea, [role=button], [role=menuitem], [role=tab], label')) return; screen.nudgeAdd(); },
hasSelectedPose: !!sp,
addCls: (n === 0 && !(lockOff || S.far || S.selMode || isRange)) ? "aw-ring [--rc:var(--orange-rgb)] animate-[awAttn_2.4s_ease-in-out_infinite] motion-reduce:animate-none motion-reduce:shadow-[0_0_0_4px_rgba(var(--rc),.5)]" : '',
hasFar: !!S.far,
farHead: S.far ? translate("robot.workspace.messages.robotWillMakeALargeMoveToPoseValue0", {value0: (S.sel + 1), value1: S.far.sid, value2: S.far.maxd}) : '',
farCancel: () => { screen.pressing = false; set({ far: null }); },
farGo: () => { screen.addLog(translate("blocks.move"), translate("robot.workspace.messages.confirmLargeMovementToPoseValue0", {value0: (S.sel + 1)})); screen.syncTo(S.far && S.far.after ? S.far.after : undefined); },
boxMenuOpen: !!S.boxMenu && !busy,
toggleBoxMenu: () => set({ boxMenu: !S.boxMenu }),
goBox: () => { screen.clearAll(); set({ boxMenu: false, st: 'moving', moveName: translate("robot.workspace.messages.storagePose") }); screen.glide(screen.BOX, [2500, 2500, 2500, 2500, 2500, 2500]); screen.addLog(translate("blocks.move"), translate("robot.workspace.messages.foldArmIntoBox")); screen.wait = screen.tm(() => screen.setState({ st: 'ready', moveName: '', cur: screen.BOX.slice(), linked: -1 }), 2500); },
homeOff: busy,
goHome: () => { screen.clearAll(); set({ st: 'moving', moveName: translate("common.messages.restPose") }); screen.glide(screen.HOME, [2000, 2000, 2000, 2000, 2000, 2000]); screen.addLog(translate("blocks.move"), translate("common.messages.returnToRestPose")); screen.wait = screen.tm(() => screen.setState({ st: 'ready', moveName: '', cur: screen.HOME.slice(), linked: -1 }), 2000); },
selNone: !sp && !S.p0sel,
selIsPose0: S.p0sel,
selIsPose: !!sp && !S.p0sel,
selNo: sp ? S.sel + 1 : '',
selName: sp ? sp.name : '',
isOrderMode: isOrder,
notOrderMode: !isOrder,
 };
}
