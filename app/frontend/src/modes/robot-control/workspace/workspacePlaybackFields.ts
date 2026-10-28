import { t as translate } from "../../../i18n";

import type { WorkspaceFieldsContext } from "./workspaceFields";

export function buildPlaybackFields(context: WorkspaceFieldsContext) {
 const { screen, S, displayNames, st, set, IC, running, busy, seq, n, noSeq, sp, linked, lockOff, isSplit, isSame, ord, isRange, isOrder, dragOn, pOn, ms, changed, changeText, robotWhere, tOk, tN, goSelOff, samePick, sameWhy, tiles, rows } = context;
 const sourceNote = (): string => {
   if (S.src === "robot") {
     if (!S.robotSeq) return "";
     if (S.robotRun) return translate("robot.workspace.messages.robotIsPlayingFromItsMemory");
     const key = S.synced
       ? "robot.workspace.upload.sourceMatches"
       : "robot.workspace.upload.sourceDiffers";
     return translate(key, { poseCount: S.robotSeq.n, uploadedAt: S.robotSeq.at });
   }
   if (S.synced) return translate("robot.workspace.messages.appRobotMemory");
   return translate("robot.workspace.messages.appValue0PosesNotUploaded", { value0: n });
 };
 const moveLabel = (showPlayIcon: boolean): string => {
   if (linked) {
     return showPlayIcon
       ? translate("robot.workspace.messages.robotIsAtPoseValue0Label", { value0: S.sel + 1 })
       : translate("robot.workspace.messages.robotIsAlreadyAtThisPose");
   }
   if (st === "moving") return translate("robot.workspace.position.moving");
   const key = showPlayIcon ? "robot.workspace.position.moveToPose" : "robot.workspace.position.goToPose";
   return translate(key, { poseNumber: S.sel + 1 });
 };
 const V = {
srcIsApp: S.src !== 'robot',
srcOpts: [['app', translate("robot.workspace.messages.appPoses"), false, translate("robot.workspace.messages.playPosesInThisAppNoUploadNeeded")], ['robot', translate("robot.workspace.messages.robotMemoryPoses"), !S.robotSeq, S.robotSeq ? translate("robot.workspace.messages.playUploadedPosesFromRobotMemoryValue0", {value0: S.robotSeq.names}) : translate("robot.workspace.messages.noPosesHaveBeenUploadedYet")]].map((o?: any) => ({ label: IC(o[1]).t, ic: IC(o[1]).c, on: (S.src || 'app') === o[0], off: o[2] || running, tip: o[3], pick: () => set({ src: o[0], scopeOpen: false }),
        tab: o[0] === 'app' ? translate("robot.workspace.messages.app") : translate("robot.workspace.messages.board"),
        tabStyle: 'display: inline-flex; align-items: center; gap: 6px; height: 36px; padding: 0 12px; border: none; border-radius: 9px; font-family: inherit; font-size: 14px; font-weight: 700; white-space: nowrap; cursor: ' + ((o[2] || running) ? 'not-allowed' : 'pointer') + '; ' + ((S.src || 'app') === o[0] ? (running ? 'background: var(--sunken); color: var(--faint)' : 'background: var(--surface); color: var(--blue-ink); box-shadow: 0 1px 3px rgba(var(--ink-rgb),.18)') : 'background: transparent; color: ' + ((o[2] || running) ? 'var(--faint)' : 'var(--muted)')),
        style: 'height: 30px; padding: 0 10px; border: none; border-radius: 8px; font-family: inherit; font-size: 13px; font-weight: 700; white-space: nowrap; cursor: pointer; ' + ((S.src || 'app') === o[0] ? 'background: var(--surface); color: var(--blue-ink); box-shadow: 0 1px 3px rgba(var(--ink-rgb),.18)' : 'background: transparent; color: ' + (o[2] ? 'var(--faint)' : 'var(--muted)')) })),
srcNote: sourceNote(),
srcNoteStyle: 'padding-left: 4px; font-size: 11px; font-weight: 600; white-space: nowrap; color: ' + (S.src === 'robot' && !S.synced ? 'var(--orange-deep)' : 'var(--muted)'),
cancelSel: () => set({ selMode: false, picks: [], grpNaming: false }),
toggleOrder: () => set({ mode: isOrder ? 'all' : 'order', picking: false, scopeOpen: false, movePick: -1 }),
endSlotShow: pOn || (isOrder && S.movePick >= 0 && S.movePick !== n - 1) || (dragOn && S.dropGap === n),
endSlotCls: dragOn ? "aw-slotgrow animate-[awSlotGrow_.18s_ease-out_both] overflow-hidden motion-reduce:animate-none" : '',
endSlotText: translate("robot.workspace.messages.pasteAtEnd"),
pClipN: S.pClip ? S.pClip.poses.length : 0,
endSlotLabel: pOn ? translate("robot.workspace.messages.pasteAtTheEndOfTheSequence") : S.movePick >= 0 ? translate("robot.workspace.messages.placePoseValue0AtTheEnd", {value0: (S.poses[seq[S.movePick]] || {}).name}) : '',
scopeOff: running || isOrder || noSeq || st === 'moving' || st === 'sending',
scopeOpen: !!S.scopeOpen,
scopeMenuOpen: !!S.scopeOpen && !isRange && !running,
scopeToggle: () => { if (isRange) set({ mode: 'all', picking: false, scopeOpen: false }); else set({ scopeOpen: !S.scopeOpen }); },
scopeCaret: !isRange,
scopeLabel: isRange ? (S.picking ? translate("robot.workspace.messages.rangeTapEndPoseCancel") : translate("robot.workspace.messages.rangeTapStartPoseCancel")) : (S.rangeOn && n ? translate("robot.workspace.messages.playPosesValue0Value1", {value0: (ord[0] + 1), value1: (ord.length > 1 ? ' → ' + (ord[ord.length - 1] + 1) : '')}) : translate("robot.workspace.messages.playAll")),
scopeTip: isRange ? translate("robot.workspace.messages.cancelRangeSelection") : translate("robot.workspace.messages.playAllPosesOrARangeThisAffectsPlayback"),
scopeStyle: 'height: 34px; padding: 0 12px; border-radius: 10px; font-family: inherit; font-size: 14px; font-weight: 700; display: flex; align-items: center; gap: 6px; white-space: nowrap; cursor: pointer; ' + ((running || isOrder || noSeq) ? 'border: 2px solid var(--off); background: var(--sunken); color: var(--faint); cursor: not-allowed' : ((S.rangeOn || isRange) ? 'border: 2px solid var(--blue-ink); background: var(--blue-tint); color: var(--blue-press)' : 'border: 2px solid var(--line-strong); background: var(--surface); color: var(--blue-ink)')),
redoShow: false,
redoLabel: isRange ? (S.rangeDone ? translate("robot.workspace.messages.chooseAgain") : translate("common.messages.startOver")) : translate("robot.workspace.messages.chooseAgain"),
redoTip: isRange ? translate("robot.workspace.messages.clearTheStartPoseAndChooseAnotherOrPress") : translate("robot.workspace.messages.clearThisRangeAndChooseANewStartPose"),
redoPick: () => { set({ hoverIdx: -1, mode: 'range', picking: false, rangeDone: false, rangeOn: false, scopeOpen: false, timeEdit: -1, renaming: null, movePick: -1 }); },
scopeClearShow: false,
redoOff: running || (isRange && !S.picking && !S.rangeDone),
scopeClearLabel: translate("robot.workspace.messages.backToPlayingAll"),
scopeClear: () => set({ rangeOn: false, scopeOpen: false }),
scopeIsAll: !S.rangeOn,
rangeOnFlag: !!S.rangeOn,
scopeAllLabel: translate("robot.workspace.messages.value0AllValue1Poses", {value0: (S.rangeOn ? '' : '✓ '), value1: n}),
scopeItemAllStyle: 'display: flex; align-items: center; gap: 6px; min-height: 36px; padding: 6px 10px; border: none; border-radius: 8px; text-align: left; font-family: inherit; font-size: 14px; font-weight: 700; cursor: pointer; ' + (!S.rangeOn ? 'background: var(--blue-tint); color: var(--blue-press)' : 'background: var(--surface); color: var(--ink)'),
scopeItemRangeStyle: 'display: flex; flex-direction: column; align-items: flex-start; gap: 1px; min-height: 36px; padding: 6px 10px; border: none; border-radius: 8px; text-align: left; font-family: inherit; font-size: 14px; font-weight: 700; cursor: pointer; ' + (S.rangeOn ? 'background: var(--blue-tint); color: var(--blue-press)' : 'background: var(--surface); color: var(--ink)'),
orderBtnLabel: isOrder ? translate("robot.workspace.messages.doneLabelStatus") : translate("robot.workspace.messages.reorderPoses"),
showLinkBox: !!screen.props.showLinkBox,
showRevert: !!screen.props.showRevert,
onValKey: (e?: any) => { if (e.key === 'Enter') e.target.blur(); else if (e.key === 'Escape') { set({ valDraft: null }); e.target.blur(); } },
linkHead: linked ? translate("robot.workspace.messages.robotIsAtThisPose") : translate("robot.workspace.messages.previewingRobotIsNotAtThisPose"),
linkDesc: linked ? translate("robot.workspace.messages.jointAdjustmentsUpdateThisPoseImmediatelyNoExtraSave") : translate("robot.workspace.messages.value0AdjustWithMovementButtonsOrPressMoveRobot", {value0: robotWhere}),
selT: sp ? sp.t : '',
selTValid: tOk,
selDeleteConfirm: !!S.delAsk,
selTHint: tOk ? translate("robot.workspace.messages.value0SecondsValue1", {value0: (tN / 1000).toFixed(1), value1: (tN < 1500 ? translate("robot.workspace.input.fast") : (tN < 3000 ? translate("robot.workspace.input.normal") : translate("robot.workspace.input.slow")))}) : translate("robot.workspace.messages.mustBeBetween10009000Ms"),
selDurText: translate("robot.workspace.messages.value0SecSlowestServo", {value0: (ms / 1000).toFixed(1)}),
goSel: () => { if (screen.state.goPop >= 0) screen.setState({ goPop: -1 }); const p = screen.selPose(); if (!p) return; const md = screen.maxDiff(S.cur, p.v); if (md.m > (screen.props.farDeg ?? 30)) set({ far: { j: md.j, dir: 0, maxd: Math.round(md.m), sid: screen.SV[md.j].sid } }); else screen.syncTo(); },
hdrGoShow: false,
hdrP0Show: !!S.p0sel,
hdrGoLabel: moveLabel(true),
hdrGoTip: linked ? translate("robot.workspace.messages.robotIsAtThisPoseYouCanAdjustJoints") : translate("robot.workspace.messages.moveTheRobotToTheSelectedPoseLargeMoves"),
hdrGoCls: '',
goPopClose: () => set({ goPop: -1 }),
goPopTitle: translate("robot.workspace.messages.robotIsNotAtPoseValue0", {value0: (S.sel + 1)}),
goPopBody: translate("robot.workspace.messages.moveTheRobotToThisPoseBeforeAdjustingIt"),
goPopHint: translate("robot.workspace.messages.pressTheOrangeOutlinedMoveToPoseValue0Tap", {value0: (S.sel + 1)}),
hdrGoStyle: 'height: 30px; padding: 0 10px; border-radius: 10px; font-family: inherit; font-size: 13px; font-weight: 700; display: flex; align-items: center; gap: 6px; white-space: nowrap; ' + (linked ? 'border: 2px solid transparent; background: var(--ok-tint); color: var(--ok-ink); cursor: default' : ((busy || !sp) ? 'border: 2px solid var(--off); background: var(--sunken); color: var(--faint); cursor: not-allowed' : 'border: 2px solid var(--blue-line); border-bottom: 4px solid var(--blue-line); background: var(--surface); color: var(--blue-ink); cursor: pointer')),
goSelOff,
goSelLabel: moveLabel(false),
goSelLinked: linked,
selChanged: changed,
changeText,
delLabel: S.delAsk ? translate("robot.workspace.messages.pressAgainToDelete") : translate("common.messages.deleteThisPose"),
goOff: busy,

goPose0: () => {
  const difference = screen.maxDiff(S.cur, screen.POSE0.v);
  screen.clearAll();
  set({ st: 'moving', moveName: translate("robot.workspace.messages.pose0") });
  screen.glide(screen.POSE0.v, [2000, 2000, 2000, 2000, 2000, 2000]);
  const key = difference.m > (screen.props.farDeg ?? 30)
    ? "robot.workspace.position.poseZeroLargeMoveLog"
    : "robot.workspace.position.poseZeroLog";
  screen.addLog(translate("blocks.move"), translate(key));
  screen.wait = screen.tm(() => screen.setState({
    st: 'ready',
    moveName: '',
    cur: screen.POSE0.v.slice(),
    linked: -1,
  }), 2000);
},
p0DupShow: !isRange && !isOrder && !running && !S.selMode,
p0DupCls: S.p0sel ? "aw-dup [&.aw-del-on]:opacity-[1] focus-visible:opacity-[1] [&_.aw-dup-lbl]:hidden [&:hover:not(:disabled)]:bg-blue-ink! [&:hover:not(:disabled)]:border-blue-ink! [&:hover:not(:disabled)]:text-white! focus-visible:bg-blue-ink! focus-visible:border-blue-ink! focus-visible:text-white! aw-del-on" : "aw-dup [&.aw-del-on]:opacity-[1] focus-visible:opacity-[1] [&_.aw-dup-lbl]:hidden [&:hover:not(:disabled)]:bg-blue-ink! [&:hover:not(:disabled)]:border-blue-ink! [&:hover:not(:disabled)]:text-white! focus-visible:bg-blue-ink! focus-visible:border-blue-ink! focus-visible:text-white!",
p0Dup: (e?: any) => { if (e && e.stopPropagation) e.stopPropagation(); if (lockOff) return; screen.insertPose(0, Object.assign(JSON.parse(JSON.stringify(screen.POSE0)), { name: translate("robot.workspace.messages.uprightArm"), t: 3000, ts: [3000, 3000, 3000, 3000, 3000, 3000] }), translate("robot.workspace.messages.duplicatePose0AsPose1")); screen.showToast(translate("robot.workspace.messages.duplicatedPose0AsPose1ReadyToEdit"), null, '', null, '', 3000); },
usePose0: () => screen.insertPose(n, JSON.parse(JSON.stringify(screen.POSE0)), translate("robot.workspace.messages.copyPose0AsPoseValue0", {value0: (n + 1)})),
p0Off: true,
p0On: S.p0sel,
isCards: S.view === 'cards',
isTable: S.view === 'table',
modeHintOn: isRange || isOrder,
modeHint: isOrder ? translate("robot.workspace.messages.reorderModeDragPosesToNewPositionsPlaybackUses") : translate("robot.workspace.messages.rangeModeTapStartAndEndPosesPlaybackFollows"),
modeHintStyle: 'font-size: 13px; border-radius: 10px; padding: 6px 10px; ' + (isOrder ? 'background: var(--sunken); color: var(--ink)' : 'background: var(--blue-tint); color: var(--blue-press)'),
sameAskShow: !!S.sameAsk && isSplit && !running,
sameAskChips: (S.seq as string[]).slice(0, 4).map((k?: any, i?: any) => ({ t: translate("robot.workspace.messages.poseValue0", {value0: (i + 1)}), v: samePick(S.poses[k]) })),
sameAskMore: S.seq.length > 4 ? translate("robot.workspace.messages.andValue0MorePoses", {value0: (S.seq.length - 4)}) : '',
sameAskWhy: sameWhy,
sameAskCount: S.seq.length,
sameAskNo: () => set({ sameAsk: false }),
chainLabel: isSame ? translate("robot.workspace.messages.sameDurationForAllJoints") : translate("robot.workspace.messages.perJointDuration"),
noSeq,
tiles,
rows,
servoHeads: screen.SV.map((servo, jointIndex) => {
  const rangeKey = servo.pct
    ? "robot.workspace.input.opening"
    : "robot.workspace.input.angleRange";
  const durationKey = servo.pct
    ? "robot.workspace.input.openingWithDuration"
    : "robot.workspace.input.angleRangeWithDuration";
  return {
    sid: servo.sid,
    name: displayNames[jointIndex],
    sub: translate(isSplit ? durationKey : rangeKey, { minimum: servo.min, maximum: servo.max }),
  };
}),
 };
 return V;
}
