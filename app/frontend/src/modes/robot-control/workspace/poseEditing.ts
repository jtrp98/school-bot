import type { DragEvent } from "react";
import type { PoseGroup, PoseListState } from "./poseListState";
import { t as translate } from "../../../i18n";
import type { WorkspaceLogic } from "./WorkspaceLogic";
import { poseRangeText } from "./workspaceText";

const pasteToastKey = (moved: boolean, grouped: boolean) => {
  if (moved) return grouped ? "robot.workspace.toast.posesMovedIntoGroup" : "robot.workspace.toast.posesMoved";
  return grouped ? "robot.workspace.toast.posesPastedIntoGroup" : "robot.workspace.toast.posesPasted";
};

export function gCopy(screen: WorkspaceLogic, g?: any) {
  if (!g) return;
  const S = screen.state;
  screen.setState({
    pClip: null,
    pMode: false,
    gClip: { name: g.name, poses: g.keys.map((k?: any) => JSON.parse(JSON.stringify(S.poses[k]))) },
    gFocus: g.id,
    gMenu: null,
  });
  const text = translate("robot.workspace.toast.groupCopied", { name: g.name, count: g.keys.length });
  screen.showToast(text, null, '', null, '', 3500);
}

export function gDelete(screen: WorkspaceLogic, g?: any) {
  if (!g) return;
  const S = screen.state;
  const back: Record<string, any> = { poses: S.poses, seq: S.seq, groups: S.groups, sel: S.sel, linked: S.linked };
  const p2 = Object.assign({}, S.poses);
  g.keys.forEach((k?: any) => delete p2[k]);
  screen.setState({
    poses: p2,
    seq: S.seq.filter((k?: any) => g.keys.indexOf(k) < 0),
    groups: S.groups.filter((x?: any) => x.id !== g.id),
    sel: -1,
    linked: -1,
    gFocus: null,
    gMenu: null,
    synced: false,
  });
  const deleted = { name: g.name, count: g.keys.length };
  screen.addLog('pose', translate("robot.workspace.log.groupDeleted", deleted));
  screen.showToast(
    translate("robot.workspace.toast.groupDeleted", deleted),
    () => screen.setState(Object.assign({ toast: null }, back)),
    "common.messages.undo",
    null,
    '',
    6000,
  );
}

export function gPaste(screen: WorkspaceLogic, at?: any) {
  const S = screen.state, c = S.gClip;
  if (!c) return;
  let nm = translate("robot.workspace.messages.value0Copy", { value0: c.name }), q = 2;
  while (S.groups.some((x?: any) => x.name === nm)) {
    nm = translate("robot.workspace.pose.copyNumbered", { name: c.name, number: q++ });
  }
  const p2 = Object.assign({}, S.poses), s2 = S.seq.slice(), keys: any[] = [];
  let cnt = S.counter;
  c.poses.forEach((p?: any, i?: any) => {
    const k = 'p' + (cnt++);
    p2[k] = JSON.parse(JSON.stringify(p));
    keys.push(k);
    s2.splice(at + i, 0, k);
  });
  const g: Record<string, any> = { id: 'g' + Date.now(), name: nm, keys, open: false };
  const back: Record<string, any> = { poses: S.poses, seq: S.seq, groups: S.groups, counter: S.counter };
  screen.setState({
    poses: p2,
    seq: s2,
    counter: cnt,
    groups: S.groups.concat([g]),
    gFocus: g.id,
    gMenu: null,
    synced: false,
  });
  const pasted = { name: nm, range: poseRangeText(at + 1, keys.length) };
  screen.addLog('pose', translate("robot.workspace.log.groupPasted", pasted));
  screen.showToast(
    translate("robot.workspace.toast.groupPasted", pasted),
    () => screen.setState(Object.assign({ toast: null, gFocus: null }, back)),
    "common.messages.undo",
    null,
    '',
    6000,
  );
}

export function pCopy(screen: WorkspaceLogic, cut?: any, keys?: any) {
    const S = screen.state; if (screen.pBusy()) return;
    const ks = (keys || screen.pSrcKeys()).filter((k?: any) => S.poses[k]);
    if (!ks.length) {
      screen.showToast(
        translate(cut ? "robot.workspace.toast.selectPosesToCut" : "robot.workspace.toast.selectPosesToCopy"),
        null,
        '',
        null,
        '',
        3000,
      );
      return;
    }
    const ord = S.seq.filter((k?: any) => ks.indexOf(k) >= 0);
    const clip: Record<string, any> = { poses: ord.map((k?: any) => JSON.parse(JSON.stringify(S.poses[k]))), keys: null };
    if (cut) {
      const back: Record<string, any> = { poses: S.poses, seq: S.seq, groups: S.groups, sel: S.sel, linked: S.linked };
      const seq2 = S.seq.filter((k?: any) => ord.indexOf(k) < 0), p2 = Object.assign({}, S.poses); ord.forEach((k?: any) => delete p2[k]);
      const first = S.seq.indexOf(ord[0]);
      screen.setState({ pClip: clip, gClip: null, pMode: false, poses: p2, seq: seq2, groups: screen.fixGroups(seq2, S.groups.map((g?: any) => Object.assign({}, g, { keys: g.keys.filter((k?: any) => ord.indexOf(k) < 0) })).filter((g?: any) => g.keys.length >= 2)), sel: seq2.length ? Math.max(0, Math.min(first - 1, seq2.length - 1)) : -1, linked: -1, synced: false, picks: [], grpNaming: false, gMenu: null, ctx: null, pctx: null });
      screen.addLog('pose', translate("robot.workspace.log.posesCut", { count: ord.length }));
      screen.showToast(
        translate("robot.workspace.toast.posesCut", { count: ord.length }),
        () => screen.setState(Object.assign({ toast: null, pClip: null }, back)),
        "common.messages.undo",
        null,
        '',
        5000,
      );
      return;
    }
    screen.setState({ pClip: clip, gClip: null, pMode: false, picks: [], grpNaming: false, gMenu: null, ctx: null, pctx: null });
    screen.addLog('pose', translate("robot.workspace.log.posesCopied", { count: ord.length }));
    screen.showToast(translate("robot.workspace.toast.posesCopied", { count: ord.length }), null, '', null, '', 4500);
  }

export function pPaste(screen: WorkspaceLogic, at?: any) {
    const S = screen.state, c = S.pClip; if (!c || screen.pBusy()) return;
    const back: Record<string, any> = { poses: S.poses, seq: S.seq, groups: S.groups, counter: S.counter, sel: S.sel, linked: S.linked, pClip: S.pClip, pMode: S.pMode, rs: S.rs, re: S.re, synced: S.synced };
    const n0 = S.seq.length; at = Math.max(0, Math.min(at == null ? n0 : at, n0));
    let poses = Object.assign({}, S.poses), cnt = S.counter, groups = S.groups.map((g?: any) => Object.assign({}, g, { keys: g.keys.slice() })), seq: any, newKeys: any, tgt = null;
    if (c.keys) {
      const cut = S.seq.filter((k?: any) => c.keys.indexOf(k) >= 0);
      if (!cut.length) {
        screen.setState({ pClip: null, pMode: false });
        screen.showToast(translate("robot.workspace.toast.cutPosesGone"), null, '', null, '', 3000);
        return;
      }
      const before = S.seq.slice(0, at).filter((k?: any) => cut.indexOf(k) >= 0).length;
      const rest = S.seq.filter((k?: any) => cut.indexOf(k) < 0), at2 = at - before;
      const whole = groups.filter((g?: any) => g.keys.every((k?: any) => cut.indexOf(k) >= 0));
      groups = groups.map((g?: any) => whole.indexOf(g) >= 0 ? g : Object.assign(g, { keys: g.keys.filter((k?: any) => cut.indexOf(k) < 0) }));
      tgt = (at2 > 0 && at2 < rest.length) ? (groups.find((g?: any) => whole.indexOf(g) < 0 && g.keys.indexOf(rest[at2 - 1]) >= 0 && g.keys.indexOf(rest[at2]) >= 0) || null) : null;
      seq = rest.slice(); seq.splice(at2, 0, ...cut); newKeys = cut;
      if (seq.join() === S.seq.join()) {
        screen.setState({ pClip: null, pMode: false });
        screen.showToast(translate("robot.workspace.toast.pasteSamePlace"), null, '', null, '', 2500);
        return;
      }
      if (tgt) { tgt.keys.splice(tgt.keys.indexOf(rest[at2 - 1]) + 1, 0, ...cut); groups = groups.filter((g?: any) => whole.indexOf(g) < 0); }
    } else {
      newKeys = [];
      c.poses.forEach((p?: any) => {
        const k = 'p' + (cnt++);
        const name = translate("robot.workspace.messages.value0Copy", { value0: p.name });
        poses[k] = Object.assign(JSON.parse(JSON.stringify(p)), { name });
        newKeys.push(k);
      });
      seq = S.seq.slice(); seq.splice(at, 0, ...newKeys);
      tgt = (at > 0 && at < n0) ? (groups.find((g?: any) => g.keys.indexOf(S.seq[at - 1]) >= 0 && g.keys.indexOf(S.seq[at]) >= 0) || null) : null;
      if (tgt) tgt.keys.splice(tgt.keys.indexOf(S.seq[at - 1]) + 1, 0, ...newKeys);
    }
    const g2 = screen.fixGroups(seq, groups), first = seq.indexOf(newKeys[0]), mx = Math.max(seq.length - 1, 0);
    screen.setState({ poses, seq, groups: g2, counter: cnt, synced: false, sel: first, p0sel: false, linked: -1, snap: null, far: null, delAsk: false, menuIdx: -1, timeEdit: -1, gFocus: null, gMenu: null, pctx: null, rs: Math.min(S.rs, mx), re: Math.min(S.re, mx), pClip: c.keys ? null : c, pMode: false, newKey: newKeys[0] });
    clearTimeout(screen.newT); screen.newT = screen.tm(() => screen.setState({ newKey: null }), 4000);
    screen.tm(() => screen.dom.scrollTileIntoView(newKeys[0]), 60);
    const pasted = { count: newKeys.length, range: poseRangeText(first + 1, newKeys.length), group: tgt ? tgt.name : '' };
    screen.addLog('pose', translate(c.keys ? "robot.workspace.log.posesMoved" : "robot.workspace.log.posesPasted", pasted));
    screen.showToast(
      translate(pasteToastKey(!!c.keys, !!tgt), pasted),
      () => screen.setState(Object.assign({ toast: null }, back)),
      "common.messages.undo",
      null,
      '',
      6000,
    );
  }

export function setPose(screen: WorkspaceLogic, k?: any, patch?: any) {
    const S = screen.state, p2 = Object.assign({}, S.poses);
    p2[k] = Object.assign({}, p2[k], patch);
    const i = S.seq.indexOf(k);
    const unlink = patch.v && i === S.linked;
    screen.setState({ poses: p2, synced: false, linked: unlink ? -1 : S.linked });
  }

export function insertPose(screen: WorkspaceLogic, at?: any, pose?: any, msg?: any, joinKey?: any) {
    if (at >= screen.state.seq.length) screen.tm(() => screen.dom.scrollSequenceToEnd(), 120);
    const S = screen.state, key = 'p' + S.counter, p2 = Object.assign({}, S.poses), s2 = S.seq.slice();
    p2[key] = pose; s2.splice(at, 0, key);
    const gIn = S.groups.map((g?: any) => { const a = s2.indexOf(g.keys[0]), b = s2.indexOf(g.keys[g.keys.length - 1]); return ((at > a && at < b) || (joinKey && g.keys.indexOf(joinKey) >= 0)) ? Object.assign({}, g, { keys: g.keys.concat([key]).sort((x?: any, y?: any) => s2.indexOf(x) - s2.indexOf(y)) }) : g; });
    screen.logEditOf(S);
    const isCur = pose.v.every((v?: any, j?: any) => screen.num(v, 0) === S.cur[j]);
    screen.setState({ groups: gIn, poses: p2, seq: s2, counter: S.counter + 1, synced: false, sel: at, p0sel: false, linked: isCur ? at : (S.linked >= at ? S.linked + 1 : S.linked), snap: isCur ? { key, p: JSON.parse(JSON.stringify(pose)) } : null, far: null, delAsk: false });
    screen.addLog('pose', msg);
    clearTimeout(screen.newT); screen.setState({ newKey: key, renaming: screen.state.view === 'table' ? null : { k: 'p', i: at }, renameDraft: pose.name, menuIdx: -1 }); screen.newT = screen.tm(() => screen.setState({ newKey: null }), 4000);
    screen.tm(() => {
      screen.dom.scrollTileIntoView(key);
      screen.dom.focusTileInput(key);
    }, 60);
  }

export function fixGroups(seq?: any, groups?: any) {
    return groups.map((g?: any) => Object.assign({}, g, { keys: g.keys.filter((k?: any) => seq.indexOf(k) >= 0) })).filter((g?: any) => {
      if (g.keys.length < 2) return false;
      const ix = g.keys.map((k?: any) => seq.indexOf(k)).sort((a?: any, b?: any) => a - b);
      for (let q = 1; q < ix.length; q++) if (ix[q] !== ix[q - 1] + 1) return false;
      return true;
    }).map((g?: any) => Object.assign({}, g, { keys: g.keys.slice().sort((a?: any, b?: any) => seq.indexOf(a) - seq.indexOf(b)) }));
  }

export function removeAt(screen: WorkspaceLogic, i?: any) {
    const S = screen.state, s2 = S.seq.slice(), k = s2[i], nm = S.poses[k].name; s2.splice(i, 1);
    const fix = (x?: any) => (x === i ? -1 : (x > i ? x - 1 : x));
    screen.setState({ seq: s2, synced: false, sel: fix(S.sel), linked: fix(S.linked), snap: S.snap && S.snap.key === k ? null : S.snap, delAsk: false, rs: Math.min(S.rs, Math.max(s2.length - 1, 0)), re: Math.min(S.re, Math.max(s2.length - 1, 0)) });
    const g0 = S.groups, g2 = screen.fixGroups(s2, screen.state.groups);
    const lost = g0.filter((g?: any) => !g2.some((x?: any) => x.id === g.id));
    screen.setState({ groups: g2 });
    screen.addLog('pose', translate("robot.workspace.log.poseDeleted", { number: i + 1, name: nm }));
    const back: Record<string, any> = { seq: S.seq, groups: g0, sel: S.sel, linked: S.linked, rs: S.rs, re: S.re, synced: S.synced };
    const deleted = { name: nm, group: lost.length ? lost[0].name : '' };
    screen.showToast(
      translate(lost.length ? "robot.workspace.toast.poseDeletedGroupSplit" : "robot.workspace.toast.poseDeleted", deleted),
      () => {
        screen.setState(Object.assign({ toast: null }, back));
        screen.addLog('pose', translate("robot.workspace.log.poseDeleteUndone", { name: nm }));
      },
      "common.messages.undo",
      null,
      '',
      6000,
    );
  }

export function over6(
  screen: WorkspaceLogic,
  event: DragEvent<HTMLElement>,
  index: number,
  groupCard: PoseGroup | null,
  axis: "x" | "y",
): void {
  const state: PoseListState = screen.state;
  const sequence = state.seq;
  let into: string | null = null;
  let gap = index;
  try {
    const rect = event.currentTarget.getBoundingClientRect();
    const position = axis === "x"
      ? (event.clientX - rect.left) / rect.width
      : (event.clientY - rect.top) / rect.height;
    if (groupCard && !state.dragG6 && position > 0.25 && position < 0.75) into = groupCard.id;
    else {
      const after = position > 0.5;
      if (groupCard && !groupCard.open) {
        const end = sequence.indexOf(groupCard.keys[groupCard.keys.length - 1]);
        gap = after ? end + 1 : sequence.indexOf(groupCard.keys[0]);
      } else gap = after ? index + 1 : index;
    }
  } catch {
    // A detached drag target falls back to the current row index instead of interrupting the drag.
  }
  if (!into && !state.dragG6) {
    const containingGroup = state.groups.find((group) => {
      const first = sequence.indexOf(group.keys[0]);
      const last = sequence.indexOf(group.keys[group.keys.length - 1]);
      return group.open && gap > first && gap <= last;
    });
    if (containingGroup) into = containingGroup.id;
  }
  if (gap !== state.dropGap || into !== state.dropInto6) {
    const targetGroup = state.groups.find((group) => group.id === into);
    screen.setState({
      dropGap: into && !targetGroup?.open ? -1 : gap,
      dropInto6: into,
    });
  }
}

export function drop6(screen: WorkspaceLogic) {
    const S = screen.state, f = S.dragFrom; screen.setState({ dragFrom: -1, dropGap: -1, dragG6: null, dropInto6: null });
    if (f < 0) return;
    const seq = S.seq, block = S.dragG6 ? S.groups.find((g?: any) => g.id === S.dragG6) : null;
    const moving = block ? block.keys.slice() : [seq[f]];
    const arr = seq.filter((k?: any) => moving.indexOf(k) < 0);
    const intoG = S.dropInto6 ? S.groups.find((g?: any) => g.id === S.dropInto6) : null;
    let at: any;
    if (intoG && S.dropGap < 0) at = arr.indexOf(intoG.keys.filter((k?: any) => moving.indexOf(k) < 0).slice(-1)[0]) + 1;
    else { if (S.dropGap < 0) return; at = S.dropGap - seq.slice(0, S.dropGap).filter((k?: any) => moving.indexOf(k) >= 0).length; }
    if (block) { const H = S.groups.find((g?: any) => g.id !== block.id && (() => { const a = arr.indexOf(g.keys[0]), b = arr.indexOf(g.keys[g.keys.length - 1]); return at > a && at <= b; })()); if (H) at = arr.indexOf(H.keys[0]); }
    const s2 = arr.slice(0, at).concat(moving, arr.slice(at));
    if (s2.join() === seq.join() && !intoG) return;
    const k = moving[0], gOld = block ? null : S.groups.find((g?: any) => g.keys.indexOf(k) >= 0);
    let groups = S.groups.map((g?: any) => Object.assign({}, g, { keys: g.keys.slice() }));
    if (!block) {
      const join = intoG && (!gOld || intoG.id !== gOld.id) ? intoG.id : (gOld && intoG && intoG.id === gOld.id ? gOld.id : null);
      groups = groups.map((g?: any) => { let ks = g.keys.filter((x?: any) => x !== k); if (g.id === join || (gOld && g.id === gOld.id && join === gOld.id)) ks = ks.concat([k]); return Object.assign({}, g, { keys: ks.sort((a?: any, b?: any) => s2.indexOf(a) - s2.indexOf(b)) }); });
    }
    const kept = groups.filter((g?: any) => g.keys.length >= 2);
    const map = (x?: any) => (x < 0 ? x : s2.indexOf(seq[x]));
    screen.setState({ seq: s2, groups: kept, sel: map(S.sel), linked: map(S.linked), synced: false, landed: { key: k, from: f } });
    clearTimeout(screen.landT); screen.landT = screen.tm(() => screen.setState({ landed: null }), 1200);
    const nm = S.poses[k].name;
    if (block) {
      screen.addLog('order', translate("robot.workspace.log.groupMoved", { name: block.name, position: at + 1 }));
    } else if (intoG && (!gOld || intoG.id !== gOld.id)) {
      const joined = { name: nm, group: intoG.name };
      screen.addLog('order', translate("robot.workspace.log.poseAddedToGroup", joined));
      screen.showToast(translate("robot.workspace.toast.poseAddedToGroup", joined), null, '', null, '', 3000);
    } else if (gOld && !kept.some((g?: any) => g.id === gOld.id)) {
      screen.showToast(translate("robot.workspace.toast.groupSplit", { group: gOld.name }), null, '', null, '', 3500);
    } else if (gOld && !kept.find((g?: any) => g.id === gOld.id).keys.includes(k)) {
      screen.showToast(translate("robot.workspace.toast.poseRemovedFromGroup", { name: nm, group: gOld.name }), null, '', null, '', 3000);
    }
    if (!block) screen.focusName6(k);
  }

export function moveTo(screen: WorkspaceLogic, from?: any, gap?: any) {
    const S = screen.state; if (from < 0) return;
    const to = gap > from ? gap - 1 : gap; if (to === from) { screen.setState({ movePick: -1 }); return; }
    const k = S.seq[from], prevSeq = S.seq.slice();
    screen.moveIdx(from, to);
    clearTimeout(screen.movedT);
    screen.setState({ movePick: -1, landed: { key: k, from }, moved: { key: k, from, to, name: S.poses[k].name, prevSeq, prevSel: S.sel, prevLinked: S.linked } });
    screen.focusName6(k);
    clearTimeout(screen.landT); screen.landT = screen.tm(() => screen.setState({ landed: null }), 1200);
    screen.movedT = screen.tm(() => screen.setState({ moved: null }), 6000);
  }

export function undoMove(screen: WorkspaceLogic) {
    const m = screen.state.moved; if (!m) return;
    clearTimeout(screen.movedT);
    screen.setState({ seq: m.prevSeq, sel: m.prevSel, linked: m.prevLinked, moved: null, movePick: -1, synced: false });
    screen.addLog('order', translate("robot.workspace.log.moveUndone", { name: m.name, position: m.from + 1 }));
  }

export function moveIdx(screen: WorkspaceLogic, from?: any, to?: any) {
    const S = screen.state; if (to < 0 || to >= S.seq.length || from === to) return;
    const s2 = S.seq.slice(), [k] = s2.splice(from, 1); s2.splice(to, 0, k);
    const map = (x?: any) => (x < 0 ? x : S.seq[x] && s2.indexOf(S.seq[x]));
    const g0 = S.groups.length, g2 = screen.fixGroups(s2, S.groups);
    screen.setState({ seq: s2, synced: false, sel: map(S.sel), linked: map(S.linked), dragFrom: -1, groups: g2 });
    if (g2.length < g0) screen.showToast(translate("robot.workspace.toast.poseLeftGroupSplit"), null, '', null, '', 4000);
    screen.addLog('order', translate("robot.workspace.log.poseMoved", { name: S.poses[k].name, from: from + 1, to: to + 1 }));
  }

export function commitRename(screen: WorkspaceLogic) {
    const S = screen.state, r = S.renaming; if (!r) return;
    const v = S.renameDraft.trim().slice(0, 24);
    if (r.k === 'j') {
      const def = screen.SV[r.j].name, nv = v || def, old = S.jnames[r.j];
      if (nv !== old) {
        const jn = S.jnames.slice();
        jn[r.j] = nv;
        screen.setState({ jnames: jn, renaming: null });
        screen.addLog('pose', translate("robot.workspace.log.jointRenamed", { joint: screen.SV[r.j].sid, from: old, to: nv }));
      }
      else screen.setState({ renaming: null });
    } else {
      const k = S.seq[r.i], p = S.poses[k];
      if (p && v && v !== p.name) {
        screen.setPose(k, { name: v });
        screen.addLog('pose', translate("robot.workspace.log.poseRenamed", { number: r.i + 1, from: p.name, to: v }));
      }
      screen.setState({ renaming: null });
    }
  }
