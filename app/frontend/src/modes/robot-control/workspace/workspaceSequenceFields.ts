import { t as translate } from "../../../i18n";

import type { WorkspaceFieldsContext } from "./workspaceFields";

export function buildSequenceFields(context: WorkspaceFieldsContext) {
 const { screen, S, st, set, isDisc, n, noSeq, lockOff, isSame, badIdx, anyBad, isRange, summary, summaryHint, hintBad, barMode, isRobot, runTitleNow, runSubNow, slotLabel, slotOff, slotPause, totalMs, doneMs, vPreflight, vRunning, vStopped, vControls, sendOff } = context;
 const uploadTip = (): string => {
   if (st === "sending") {
     return translate("robot.workspace.messages.uploadingToRobotMemoryKeepTheCableConnectedUntil");
   }
   if (isDisc) return translate("robot.workspace.upload.connectFirst");
   if (noSeq) return translate("robot.workspace.upload.addPoseFirst");
   if (anyBad) {
     return translate("robot.workspace.upload.invalidPoses", {
       poseNumbers: badIdx.map((index) => index + 1).join(", "),
     });
   }
   if (lockOff) return translate("robot.workspace.upload.waitUntilStopped");
   if (S.rangeOn) {
     const key = S.synced
       ? "robot.workspace.upload.alreadyStoredAll"
       : "robot.workspace.upload.descriptionAll";
     return translate(key, { poseCount: n });
   }
   return translate(S.synced ? "robot.workspace.upload.alreadyStored" : "robot.workspace.upload.description");
 };
 return {
p0Cells: screen.POSE0.v.map((v?: any, j?: any) => ({ v: screen.fmt(j, v) })),
sendOff,
sendTip: uploadTip(),

send: () => { screen.clearAll(); set({ st: 'sending' }); screen.wait = screen.tm(() => { screen.setState({ robotSeq: { n: screen.state.seq.length, ms: screen.state.seq.reduce((a?: any, k?: any) => a + screen.dur(screen.state.poses[k]), 0) * screen.state.loops, at: screen.now(), names: screen.state.seq.slice(0, 3).map((k?: any) => screen.state.poses[k].name).join(' · ') + (screen.state.seq.length > 3 ? ' …' : '') } }); screen.setState({ st: 'ready', synced: true, syncTime: screen.now() }); screen.addLog('send', translate("robot.workspace.messages.uploadValue0PosesToRobotMemory", {value0: n})); screen.showToast(translate("robot.workspace.messages.uploadedToRobotMemoryYouCanDisconnectUsbAnd"), null, '', null, '', 3500); }, 1500); },
timeSegShow: true,
tmOff: lockOff || isRange || S.selMode,
tmWrapStyle: 'display: flex; align-items: center; gap: 6px' + ((isRange || S.selMode) ? '; opacity: 0.5' : ''),
tmTip: (isRange || S.selMode) ? translate("robot.workspace.messages.exitRangeOrMultiSelectModeBeforeChangingDuration") : (isSame ? translate("robot.workspace.input.linkedHint") : translate("robot.workspace.input.separateHint")),
addOff: lockOff || !!S.far || S.selMode || isRange,

addRowShow: true,
addRowNo: n + 1,
addPose: () => {
  screen.tm(() => {
    const element = screen.dom.tableScroll;
    if (element) element.scrollTop = element.scrollHeight;
  }, 60);
  const duration = 3000;
  const first = n === 0;
  const key = first ? "robot.workspace.sequence.addFirstLog" : "robot.workspace.sequence.addCurrentLog";
  screen.insertPose(n, {
    name: translate("robot.workspace.messages.poseValue0Label", { value0: n + 1 }),
    v: first ? [0, 0, 0, 0, 0, 0] : S.cur.slice(),
    t: duration,
    ts: [duration, duration, duration, duration, duration, duration],
  }, translate(key, { poseNumber: n + 1 }));
  set({ mode: 'all' });
},
addTileLabel: translate("common.messages.addPose"),
addHint: n === 0 ? translate("robot.workspace.messages.doesNotMoveTheRobotFirstPoseStartsAt") : translate("robot.workspace.messages.doesNotMoveTheRobotAppendItsCurrentPose"),
variantStyle: 'flex-grow: 1; min-height: 56px; box-sizing: border-box; border-radius: 16px; padding: 8px 16px; display: flex; align-items: center; gap: 12px; ' + (vPreflight ? 'background: var(--orange-tint)' : (vRunning ? 'background: var(--blue-tint)' : (vStopped ? 'background: var(--blue-tint)' : 'background: var(--sunken)'))),
vPreflight,
vRunning,
vStopped,
vControls,
srcIsRobotTab: isRobot,
playTip: hintBad ? summaryHint : '',
barNow: barMode === 'now',
barNew: barMode !== 'now',
barA: barMode === 'A',
barC: barMode === 'C',
summary,
summaryHint,
summaryHintStyle: 'font-size: 13px; ' + (hintBad ? 'color: var(--err-ink); font-weight: 700' : 'color: var(--muted)'),
runTitle: runTitleNow,
runSub: runSubNow,
runPct: st === 'preparing' ? 0 : Math.min(100, Math.round(doneMs / totalMs * 100)),
stoppedTitle: translate("robot.workspace.messages.stoppedRobotHoldsThisPositionLabel"),
playSlot: () => { if (Date.now() - (screen._playUpT || 0) < 600) return; screen.playSlotActions.activate(); },
playDown: () => { if (screen.state.renaming) { screen._armPlay = Date.now(); screen.commitRename(); } },
playUp: () => { const a = screen._armPlay; screen._armPlay = 0; const slot = screen.playSlotActions.current(); if (!a || Date.now() - a > 1500 || slot.off || slot.kind === 'none') return; screen._playUpT = Date.now(); screen.playSlotActions.activate(); },
playSlotOff: slotOff,
playSlotLabel: slotLabel,
slotIsPause: slotPause,
slotIsPlay: !slotPause,


stopAll: screen.stopAllHandler,
 };
}
