import { t as translate } from "../../../i18n";
import { copyTextToClipboard } from "./clipboard";
import type { WorkspaceFieldsContext } from "./workspaceFields";

export function buildActionFields(context: WorkspaceFieldsContext) {
 const { screen, S, set, isDisc, busy, seq, sp, pOn } = context;
 const saveStateText = (): string => {
   if (!S.projName) return translate("robot.workspace.messages.unsaved");
   const key = S.savedSnap === screen.snapOf(S)
     ? "robot.workspace.save.sampleClean"
     : "robot.workspace.save.sampleDirty";
   return translate(key, { savedAt: S.savedAt });
 };
 const selectionText = (): string => {
   const { pickIdx, pickGs, pickSingles, pickPartial, pickExt, extG, pickContig, grpDup } = context;
   if (S.grpNaming) {
     return translate(grpDup
       ? "robot.workspace.messages.thisNameAlreadyExists"
       : "robot.workspace.messages.namePoseGroup");
   }
   if (pickIdx.length === 0) {
     return translate(S.groups.length
       ? "robot.workspace.groups.selectAnyOrGroup"
       : "robot.workspace.groups.selectTwo");
   }
   if (pickGs.length > 1) return translate("robot.workspace.messages.selectOneGroupAtATime");
   if (pickPartial) return translate("robot.workspace.messages.selectTheEntirePoseGroup");
   if (pickIdx.length < 2) return translate("robot.workspace.selection.oneMore");
   if (pickExt) {
     const key = pickContig
       ? "robot.workspace.selection.extendGroup"
       : "robot.workspace.selection.extendGroupReordered";
     return translate(key, {
       poseCount: pickSingles,
       groupName: extG?.name,
       firstPoseNumber: pickIdx[0] + 1,
     });
   }
   const key = pickContig
     ? "robot.workspace.selection.selectedPoses"
     : "robot.workspace.selection.selectedPosesReordered";
   return translate(key, { poseCount: pickIdx.length, firstPoseNumber: pickIdx[0] + 1 });
 };
 return {
robotName: translate("robot.workspace.messages.group3Robot"),
netTest: () => screen.netTest(),
saveStateText: saveStateText(),
pasteTopShow: pOn,
noClip: !(S.gClip || S.pClip),
hdrMenuOpen: S.jMenu === 'all',
jointLockShow: S.mode === 'range' || S.mode === 'order',
jointNote: S.mode === 'range' || S.mode === 'order' ? translate("common.messages.locked") : '',
readOff: busy || isDisc,
readBack: () => {
        if (!sp) return;
        const k0 = seq[S.sel], old = sp.v.slice(), i0 = S.sel;
        set({ reading: true });
        screen.tm(() => {
          const real = screen.state.cur.map((x?: any, j?: any) => screen.SV[j].pct ? Math.round(x) : Math.round(x + (j === 1 ? 2 : (j === 2 ? -1 : 0))));
          screen.setPose(k0, { v: real });
          screen.setState({ reading: false, cur: real.slice(), linked: i0 });
          screen.addLog('pose', translate("robot.workspace.messages.readRobotPositionsIntoPoseValue0", {value0: (i0 + 1)}));
          const txt = screen.SV.map((sv?: any, j?: any) => sv.sid + '=' + real[j]).join(' ');
          screen.showToast(translate("robot.workspace.messages.readRobotPositionsIntoPoseValue0Label", {value0: (i0 + 1)}), () => { screen.setPose(k0, { v: old }); screen.setState({ toast: null }); }, translate("common.messages.undo"), () => { copyTextToClipboard(txt); screen.showToast(translate("robot.workspace.messages.valuesCopiedValue0", {value0: txt}), () => { screen.setPose(k0, { v: old }); screen.setState({ toast: null }); }, translate("common.messages.undo"), null, '', 5000); }, translate("robot.workspace.messages.copyValues"), 6000);
        }, 800);
      },
relOpen: () => screen.relOpen(),
relOn: S.release,
relEnd: () => screen.relEndAsk(),
relAllLabel: sp && !S.p0sel ? translate("robot.workspace.messages.readAllJointsIntoPoseValue0", {value0: (S.sel + 1)}) : (S.p0sel ? translate("robot.workspace.selection.poseZeroReadOnly") : translate("robot.workspace.messages.noPoseSelected")),
relAllOff: !sp || S.p0sel,
relAllTip: S.p0sel ? translate("robot.workspace.messages.pose0IsReadOnlyUseCaptureAsNew") : !sp ? translate("robot.workspace.messages.lockJointsThenSelectAPoseInTheSequence") : translate("robot.workspace.messages.captureAllCurrentJointPositionsIntoPoseValue0Replace", {value0: (S.sel + 1)}),
relAll: () => {
        if (!sp || S.p0sel) return;
        const k0 = seq[S.sel], old = sp.v.slice(), i0 = S.sel, real = screen.state.cur.map((x?: any) => Math.round(x));
        const nCh = real.filter((v?: any, j?: any) => v !== screen.num(old[j], 0)).length;
        screen.setPose(k0, { v: real }); screen.relPoke();
        screen.setState({ relRead: [0, 1, 2, 3, 4, 5] });
        screen.addLog('pose', translate("robot.workspace.messages.captureAllJointsIntoPoseValue0", {value0: (i0 + 1)}));
        screen.showToast(translate("robot.workspace.messages.capturedPoseValue0Value1JointsChanged", {value0: (i0 + 1), value1: nCh}), () => { screen.setPose(k0, { v: old }); screen.setState({ toast: null, relRead: [] }); }, translate("common.messages.undo"), null, '', 5000);
      },
relNew: () => {
        const at = S.sel >= 0 ? S.sel + 1 : seq.length, real = screen.state.cur.map((x?: any) => Math.round(x));
        screen.insertPose(at, { name: translate("robot.workspace.messages.newPoseLabel"), v: real, t: 3000, ts: [3000, 3000, 3000, 3000, 3000, 3000] }, translate("robot.workspace.messages.addPoseValue0FromArmPositionJointsReleased", {value0: (at + 1)}));
        screen.relPoke(); screen.setState({ relRead: [] });
        screen.showToast(translate("robot.workspace.messages.addedPoseValue0FromTheCurrentArmPosition", {value0: (at + 1)}), null, '', null, '', 4000);
      },
xyzNote: translate("robot.workspace.messages.distanceFromRobotBaseCenter"),
selMode: S.selMode,
selText: selectionText(),
grpNaming: S.grpNaming,
grpDraft: S.grpDraft,
gRenDraft: S.gRenDraft || '',
makeLabel: S.grpNaming ? translate("robot.workspace.messages.createGroup") : translate("robot.workspace.messages.groupPosesLabel"),
 };
}
