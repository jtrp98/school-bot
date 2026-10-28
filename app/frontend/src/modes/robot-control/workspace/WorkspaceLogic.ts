import type { IconName } from "../../../components/shared/Icons";
import * as popovers from "./workspacePopovers";
import { createWorkspaceState } from "./workspaceState";
import * as poseEditing from "./poseEditing";
import * as sampleMotion from "./workspaceSampleMotion";
import { buildWorkspaceView } from "./workspaceView";
import { t as translate } from "../../../i18n";
import { SyncComponent } from "./SyncComponent";
import { WorkspaceDom } from "./workspaceDom";
import { JOINTS } from "./types";
import { memoryPlayLogText } from "./workspaceText";
import { createPlaybackBarActions, type PlaybackBarActions } from "./playbackBarActions";
import { createDialogActions, type DialogActions } from "./workspaceDialogActions";
import { createMenuActions, type MenuActions } from "./workspaceMenuActions";
import { createToolbarActions, type ToolbarActions } from "./workspaceToolbarActions";
import { createJointActions, readJointInSample, type JointActions } from "./jointActions";
import { createPoseActions, type PoseActions } from "./poseActions";
import { createSelectedPoseActions, type SelectedPoseActions } from "./selectedPoseActions";
import { createJointPanelActions, type JointPanelActions } from "./jointPanelActions";
import { createPlaybackSlotActions, type PlaybackSlotActions } from "./playbackSlotActions";
import { reconcileRange, reconcileSelection } from "./workspaceReconcile";
import { sampleMergeWorkspaces } from "./workspaceSampleData";
import { createWorkspaceKeyboardActions } from "./workspaceKeyboardActions";
import { createWorkspaceSelectionActions } from "./workspaceSelectionActions";
import type { PlaybackBarState } from "./playbackBar";


export type LogicProps = {
  demo?: string;
  showLinkBox?: boolean;
  showRange?: boolean;
  showRevert?: boolean;
  showArmTag?: boolean;
  showEmptySel?: boolean;
  showSelPanel?: boolean;
  farDeg?: number;
  bar?: "now" | "A" | "C";
  net?: "ok" | "slow" | "off";
  jdens?: "compact" | "full";
  sample4c?: string;

  conn?: "off" | "connecting" | "connected" | "lost";
  workName?: string;

  init?: Record<string, unknown>;
  boot?: string;

  restore?: Record<string, unknown> | null;
  onStash?: (state: Record<string, unknown>) => void;
  onNav?: (kind: string) => void;
  onProjName?: (name: string) => void;
  onOpenMenu?: () => void;
  onReconnect?: () => void;
  onLang?: (lang: "th" | "en" | "zh") => void;
};

export class WorkspaceLogic extends SyncComponent<LogicProps> {
  declare SV: typeof JOINTS;
  declare HOME: number[];
  declare BOX: number[];
  declare timer: number | undefined;
  declare hold: number | undefined;
  declare wait: number | undefined;
  declare toastT: number | undefined;
  declare glideT: number | undefined;
  declare relIdleT: number | undefined;
  declare relCdT: number | undefined;
  declare relSimT: number | undefined;
  declare netT: number | undefined;
  declare newT: number | undefined;
  declare landT: number | undefined;
  declare movedT: number | undefined;
  declare gAnimT: number | undefined;
  declare sbT: number | undefined;
  declare bkT: number | undefined;
  declare runT1: number | undefined;
  declare runT2: number | undefined;
  declare _tfT: number | undefined;
  declare _nudgeT: number | undefined;
  declare _g6t: number | undefined;
  declare _playUpT: number | undefined;
  declare _armPlay: number | undefined;
  declare _quiet: boolean | undefined;
  declare _robInfOk: boolean | undefined;
  declare _lastProj: string | null | undefined;
  declare _lastConn: string | undefined;
  declare _grpPrev: {
    rangeOn: boolean;
    rs: number;
    re: number;
    rk: PlaybackBarState["rk"];
  } | null | undefined;
  declare pressing: boolean | undefined;
  declare holdDir: number | undefined;
  declare sbDrag: boolean | undefined;
  declare sbOver: boolean | undefined;
  declare sEl: HTMLElement | null | undefined;
  declare gl: {
    from: number[];
    to: number[];
    ts: number[];
    t0: number;
  } | null | undefined;
  declare guard6: ((event: MouseEvent | PointerEvent) => void) | undefined;
  declare wheel6: ((event: WheelEvent) => void) | undefined;
  declare onCtrlS: ((event: KeyboardEvent) => void) | undefined;
  declare onGDel: ((event: KeyboardEvent) => void) | undefined;
  declare onEsc: ((event: KeyboardEvent) => void) | undefined;
  declare onBlur: () => void;
  declare cpDown: ((event: Event) => void) | null | undefined;
  declare cpKey: ((event: KeyboardEvent) => void) | null | undefined;
  declare _mpL: ((event: Event) => void) | null | undefined;
  readonly playbackActions: PlaybackBarActions;
  readonly dialogActions: DialogActions;
  readonly menuActions: MenuActions;
  readonly toolbarActions: ToolbarActions;
  readonly jointActions: JointActions;
  readonly poseActions: PoseActions;
  readonly selectedPoseActions: SelectedPoseActions;
  readonly jointPanelActions: JointPanelActions;
  readonly playSlotActions: PlaybackSlotActions;
  // One stable handler for the stop button, Escape and joint inputs. It dispatches to stopAll() at call time so the
  // NativeWorkspace hardware stop override applies, and it does nothing once the workspace has unmounted.
  readonly dom = new WorkspaceDom();
  _lastSel: number | null = null;
  _autoSelT: number | null = null;
  _rkFix = false;
  readonly stopAllHandler = (): void => {
    if (!this._dead) this.stopAll();
  };
  // A getter so the preset's name follows the current language instead of the one active at construction.
  get POSE0(): { name: string; v: number[]; t: number; ts: number[] } {
    return {
      name: translate("robot.workspace.messages.uprightArm"),
      v: [0, 0, 0, 0, 0, 50],
      t: 3000,
      ts: [3000, 3000, 3000, 3000, 3000, 3000],
    };
  }

  constructor(props: LogicProps) {
    super(props);
    this.SV = JOINTS;
    this.HOME = [0, -45, 60, 0, 0, 100];
    this.BOX = [0, 80, -125, -45, 0, 0]; // Simulation only: this fold pose has not been calibrated on hardware.
    this.state = createWorkspaceState(props);
    this.onBlur = () => this.endHold();
    this.playbackActions = createPlaybackBarActions({
      getState: () => this.state,
      setState: (patch) => this.setState(patch),
      showUndoToast: (text, undo, durationMs) =>
        this.showToast(text, undo, translate("common.messages.undo"), null, "", durationMs),
      endGroupRun: () => this.endGroupRun(),
      forgetGroupRun: () => {
        this._grpPrev = null;
      },
    });
    this.dialogActions = createDialogActions(this);
    this.menuActions = createMenuActions(this);
    this.toolbarActions = createToolbarActions(this);
    this.jointActions = createJointActions(this);
    this.poseActions = createPoseActions(this);
    this.selectedPoseActions = createSelectedPoseActions(this);
    this.jointPanelActions = createJointPanelActions(this);
    this.playSlotActions = createPlaybackSlotActions(this);
    this.applyBoot(props);
  }
  componentDidMount() {
    const selection = createWorkspaceSelectionActions(this);
    this.guard6 = selection.guard;
    document.addEventListener('pointerdown', this.guard6, true);
    document.addEventListener('click', this.guard6, true);
    this.wheel6 = selection.wheel;
    document.addEventListener('wheel', this.wheel6, { passive: false });
    const sample = this.props.sample4c;
    const sequence: string[] = this.state.seq;
    if (sample === 'group' && sequence.length >= 3) {
      this.setState({ groups: [{ id: 'g1', name: 'หยิบของ', keys: [sequence[1], sequence[2]], open: false }] });
    }
    if (sample === 'select' && sequence.length >= 3) this.setState({ selMode: true, picks: [sequence[1], sequence[2]] });
    if (!this.props.restore) this.setState({ savedSnap: this.snapOf(this.state) });
    if (sample !== 'saved' && sample !== 'none') this.setState({ savedSnap: '' });
    const keyboard = createWorkspaceKeyboardActions(this);
    this.onCtrlS = keyboard.shortcut;
    this.onGDel = keyboard.deleteGroup;
    window.addEventListener('keydown', this.onGDel);
    this.bkT = this.iv(() => {
      const state = this.state;
      if (state.projName && state.savedSnap !== this.snapOf(state) && state.lastBk !== this.snapOf(state)) {
        const now = new Date();
        const pad = (value: number) => String(value).padStart(2, '0');
        this.setState({
          backupAt: `${pad(now.getHours())}:${pad(now.getMinutes())}:${pad(now.getSeconds())}`,
          lastBk: this.snapOf(state),
        });
      }
    }, 10000);
    if (sample === 'recover') this.setState({ recoverAsk: true });
    if (sample === 'merge') {
      this.setState({ mergeOpen: true, mWork: 'w2', mPicks: ['m3', 'm4', 'm5'], mWhere: 'end' });
    }
    window.addEventListener('keydown', this.onCtrlS);
    window.addEventListener('blur', this.onBlur);
    this.onEsc = keyboard.escape;
    window.addEventListener('keydown', this.onEsc);
    reconcileSelection(this);
    reconcileRange(this);
    this.afterMount();
  }
  componentWillUnmount() { this.beforeUnmount(); clearInterval(this.bkT); this.clearAll(); window.removeEventListener('blur', this.onBlur); if (this.onEsc) window.removeEventListener('keydown', this.onEsc); if (this.onCtrlS) window.removeEventListener('keydown', this.onCtrlS); clearTimeout(this.toastT); }
  sbMeasure(vis?: any) {
    const el = this.sEl; if (!el) return;
    const sh = el.scrollHeight, ch = el.clientHeight;
    if (sh <= ch + 1) { if (this.state.sb.show) this.setState({ sb: { show: false, h: 0, top: 0, vis: false } }); return; }
    const h = Math.max(36, ch * ch / sh), top = (el.scrollTop / (sh - ch)) * (ch - h - 8) + 4;
    this.setState({ sb: { show: true, h, top, vis: vis === undefined ? this.state.sb.vis : vis } });
  }
  sbHideSoon() { clearTimeout(this.sbT); this.sbT = this.tm(() => { if (!this.sbDrag && !this.sbOver) this.setState({ sb: Object.assign({}, this.state.sb, { vis: false }) }); }, 900); }
  clearAll() { clearInterval(this.timer); clearInterval(this.hold); clearTimeout(this.wait); }
  now() { const d = new Date(); return String(d.getHours()).padStart(2, '0') + ':' + String(d.getMinutes()).padStart(2, '0'); }
  addLog(type?: any, text?: any) { this.setState((s?: any) => ({ log: s.log.concat([{ time: this.now(), type, text }]) })); }
  okT(v?: any) { const n = Number(v); return String(v).trim() !== '' && Number.isInteger(n) && n >= 1000 && n <= 9000; }
  okV(j?: any, v?: any) { const n = Number(v); return String(v).trim() !== '' && Number.isInteger(n) && n >= this.SV[j].min && n <= this.SV[j].max; }
  num(v?: any, f?: any) { const n = Number(v); return (String(v).trim() === '' || isNaN(n)) ? f : n; }
  gcodePart(j?: any) {
    const S = this.state, sv = this.SV[j], seq = S.seq || [], k = seq[S.sel], p = S.p0sel ? this.POSE0 : (k != null ? S.poses[k] : null);
    const val = p ? Number(p.v[j]) : Number(S.cur[j]);
    const pw = sv.pct ? 500 + Math.round(val * 20) : 1500 + Math.round(val * 1000 / sv.max);
    const t = p ? (S.timeMode === 'split' && p.ts && this.okT(p.ts[j]) ? Number(p.ts[j]) : this.dur(p)) : 1000;
    return '#' + String(j).padStart(3, '0') + 'P' + Math.max(500, Math.min(2500, pw)) + 'T' + t + '!';
  }
  gcodeId() { const S = this.state, k = (S.seq || [])[S.sel]; return (k != null && !S.p0sel) ? 'G' + String(S.sel + 1).padStart(4, '0') : 'G0000'; }
  gcodeOne(j?: any) { return '{' + this.gcodeId() + this.gcodePart(j) + '}'; }
  gcodeAll() { return '{' + this.gcodeId() + this.SV.map((_?: any, j?: any) => this.gcodePart(j)).join('') + '}'; }
  icoSplit(str?: any) {
    const MAP: Record<string, IconName> = { '📦': 'box', '✂': 'cut', '📋': 'paste', '🗑': 'trash', '📂': 'folder', '💾': 'save', '📄': 'file', '📶': 'wifi', '🕘': 'clock', '📥': 'in', '🔒': 'lock', '🛟': 'buoy', '💻': 'laptop', '🤖': 'robot', '➕': 'plus', '⚠': 'warn', '⧉': 'copy' };
    const t = String(str == null ? '' : str), ch = Array.from(t)[0];
    if (ch && MAP[ch]) return { c: MAP[ch], t: t.slice(ch.length).replace(/^\uFE0F?\s*/, '') };
    return { c: undefined, t };
  }
  dur(p?: any) { if (!p) return 3000; if (this.state.timeMode === 'split') return Math.max.apply(null, p.ts.map((x?: any) => (this.okT(x) ? Number(x) : 3000))); return this.okT(p.t) ? Number(p.t) : 3000; }
  poseOk(p?: any) { const S = this.state; return p.v.every((v?: any, j?: any) => this.okV(j, v)) && (S.timeMode === 'split' ? p.ts.every((x?: any) => this.okT(x)) : this.okT(p.t)); }
  fmt(j?: any, v?: any) { const n = Math.round(this.num(v, 0)); if (this.SV[j].pct) return translate("robot.workspace.messages.openingCount", { count: n }); return (n > 0 ? '+' : (n < 0 ? '−' : '')) + Math.abs(n) + '°'; }
  order() {
    const S = this.state, n = S.seq.length;
    if (!n) return [];
    if (S.mode !== 'range' && !S.rangeOn) return Array.from({ length: n }, (_?: any, i?: any) => i);

    const a = Math.min(Math.max(S.rs, 0), n - 1), b = Math.min(Math.max(S.re, 0), n - 1);
    const out: any[] = []; for (let i = a, g = 0; g < n; i = (i + 1) % n, g++) { out.push(i); if (i === b) break; } return out;
  }
  selPose() { const S = this.state; return S.sel >= 0 ? S.poses[S.seq[S.sel]] : null; }
  maxDiff(a?: any, b?: any) { let m = 0, j = 0; for (let k = 0; k < 5; k++) { const d = Math.abs(this.num(a[k], 0) - this.num(b[k], 0)); if (d > m) { m = d; j = k; } } return { m, j }; }

  bump(j?: any, dir?: any) {
    const S = this.state, k = S.seq[S.sel], p = S.poses[k];
    if (!p) return;
    const sv = this.SV[j], cur = S.cur.slice();
    const nv = Math.max(sv.min, Math.min(sv.max, cur[j] + dir * sv.step));
    const hit = nv === sv.min || nv === sv.max;
    cur[j] = nv;
    const v = p.v.slice(); v[j] = nv;
    const p2 = Object.assign({}, S.poses); p2[k] = Object.assign({}, p, { v });
    const joint = sv.sid + ' ' + this.state.jnames[j];
    const value = this.fmt(j, nv);
    this.setState({
      cur,
      poses: p2,
      jsel: j,
      synced: false,
      limitMsg: hit ? translate("robot.workspace.limit.reached", { joint, value }) : '',
    });
    if (hit) {
      this.msgPopOpen('limit', {
        title: translate("robot.workspace.limit.title", { joint }),
        body: translate(dir < 0 ? "robot.workspace.limit.minBody" : "robot.workspace.limit.maxBody", { value }),
        anchorSelector: 'button[aria-label^="' + sv.sid + ' "][onpointerdown], .aw-jcard button[aria-label^="' + sv.sid + ' "]',
        pick: dir < 0 ? 0 : 1,
      });
      this.endHold();
      this.addLog('err', translate("robot.workspace.log.limitReached", { joint, value }));
    }
  }
  stepVal(j?: any, dir?: any, big?: any) {
    const S = this.state, sv = this.SV[j], p = this.selPose(); if (!p) return;
    const base = S.linked === S.sel ? S.cur[j] : this.num(p.v[j], 0);
    const d = sv.pct ? (big ? 20 : 5) : (big ? 10 : 1);
    this._quiet = true; this.typeVal(j, Math.round(base) + dir * d); this._quiet = false;
  }
  setAbs(j?: any, nv?: any) {
    const S = this.state, k = S.seq[S.sel], p = S.poses[k];
    if (!p) return;
    const sv = this.SV[j], cur = S.cur.slice(); cur[j] = nv;
    const v = p.v.slice(); v[j] = nv;
    const p2 = Object.assign({}, S.poses); p2[k] = Object.assign({}, p, { v });
    this.setState({ cur, poses: p2, jsel: j, synced: false, valDraft: null });
    if (!this._quiet) {
      this.addLog('pose', translate("robot.workspace.log.typedValue", {
        joint: sv.sid + ' ' + S.jnames[j],
        value: this.fmt(j, nv),
        number: S.sel + 1,
      }));
    }
  }
  // typed value: same safety path as jogging (go to pose first, confirm if far)
  typeVal(j?: any, raw?: any) {
    const S = this.state, sv = this.SV[j], n = Number(String(raw).replace(/[^0-9+\-.]/g, ''));
    { const kk = this.lockKind(); if (kk) { this.setState({ valDraft: null }); this.msgPopOpen(kk); return; } }
    if (String(raw).trim() === '' || isNaN(n) || S.sel < 0) { this.setState({ valDraft: null }); return; }
    const nv = Math.max(sv.min, Math.min(sv.max, Math.round(n)));
    const clampedText = { joint: sv.sid, min: this.fmt(j, sv.min), max: this.fmt(j, sv.max), value: this.fmt(j, nv) };
    this.setState({ limitMsg: nv !== Math.round(n) ? translate("robot.workspace.limit.clamped", clampedText) : '' });
    if (nv !== Math.round(n)) {
      this.msgPopOpen('limit', {
        title: translate("robot.workspace.limit.rangeTitle", clampedText),
        body: translate("robot.workspace.limit.clampedBody", clampedText),
        anchorSelector: 'input[aria-label^="' + sv.sid + '"]',
      });
    }
    if (S.st !== 'ready' && S.st !== 'stopped' && S.st !== 'done') { this.setState({ valDraft: null }); return; }
    if (S.linked !== S.sel) { this.setState({ valDraft: { j, s: String(raw) } }); this.msgPopOpen('goType'); return; }
    if (S.st !== 'ready') this.setState({ st: 'ready' });
    this.setAbs(j, nv);
  }
  beginHold(j?: any, dir?: any) { clearInterval(this.hold); this.setState({ holding: j }); this.bump(j, dir); this.hold = this.iv(() => this.bump(j, dir), 140); }
  timesOf(p?: any) { const t = this.num(p.t, 3000); return this.state.timeMode === 'split' ? p.ts.map((x?: any) => Math.max(100, this.num(x, t))) : [t, t, t, t, t, t]; }
  glide(values: Array<number | string>, times: number[]): void {
    sampleMotion.glide(this, values, times);
  }
  freezeGlide(): void {
    sampleMotion.freezeGlide(this);
  }
  syncTo(after?: () => void): void {
    sampleMotion.syncTo(this, after);
  }
  lockKind() {
    const S = this.state, st = S.st;
    if (st === 'disconnected' || st === 'connecting') return 'disc';
    if (st === 'preparing' || st === 'playing' || st === 'paused') return 'run';
    if (S.sel < 0 && !S.p0sel) return S.seq.length === 0 ? 'nopose' : 'nosel';
    return '';
  }
  msgPopOpen(kind?: any, o?: any) { return popovers.msgPopOpen(this, kind, o); }
  keyPopOpen(el?: any) { return popovers.keyPopOpen(this, el); }
  needGoMsg() { this.msgPopOpen('go'); }
  startHold(j?: any, dir?: any) {
    const S = this.state;
    { const kk = this.lockKind(); if (kk) { this.msgPopOpen(kk); return; } }
    if (S.sel < 0 || S.st !== 'ready' && S.st !== 'stopped' && S.st !== 'done') return;
    if (S.linked !== S.sel) { this.needGoMsg(); return; }
    if (S.st !== 'ready') this.setState({ st: 'ready' });
    this.pressing = true;
    if (S.linked === S.sel) { this.beginHold(j, dir); return; }
    const p = this.selPose(), md = this.maxDiff(S.cur, p.v);
    const lim = this.props.farDeg ?? 30;
    if (md.m > lim) { this.setState({ far: { j, dir, maxd: Math.round(md.m), sid: this.SV[md.j].sid } }); return; }
    this.syncTo(() => { if (this.pressing) this.beginHold(j, dir); });
  }
  endHold() { this.pressing = false; clearInterval(this.hold); if (this.state.holding !== null) this.setState({ holding: null }); }
  logEditOf(S?: any) {
    if (!S.snap) return;
    const i = S.seq.indexOf(S.snap.key), p = S.poses[S.snap.key], o = S.snap.p;
    if (i < 0 || !p) return;
    const ch: any[] = [];
    p.v.forEach((v?: any, j?: any) => { if (this.num(v, 0) !== this.num(o.v[j], 0)) ch.push(this.SV[j].sid + ' ' + this.fmt(j, o.v[j]) + ' → ' + this.fmt(j, v)); });
    if (String(p.t) !== String(o.t) || p.ts.join() !== o.ts.join()) ch.push(translate("robot.workspace.log.changeTime"));
    if (p.name !== o.name) ch.push(translate("robot.workspace.log.changeName", { from: o.name, to: p.name }));
    if (ch.length) this.addLog('pose', translate("robot.workspace.log.poseEdited", { number: i + 1, changes: ch.join(' · ') }));
  }
  select(i?: any) {
    if (this.state.valDraft) this.setState({ valDraft: null });
    const S = this.state;
    if (S.sel === i && !S.p0sel) return;
    this.logEditOf(S);
    this.setState({ sel: i, p0sel: false, snap: (S.linked === i && S.snap && S.snap.key === S.seq[i]) ? S.snap : (S.linked === i ? { key: S.seq[i], p: JSON.parse(JSON.stringify(S.poses[S.seq[i]])) } : null), far: null, delAsk: false, menuIdx: -1, timeEdit: -1, limitMsg: '' });
  }
  play(): void {
    sampleMotion.play(this);
  }
  toggleGroup(id?: any) {
    const g = this.state.groups.find((x?: any) => x.id === id); if (!g) return;
    clearTimeout(this.gAnimT);
    const fast = typeof window !== 'undefined' && window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    const setOpen = (v?: any, mode?: any) => this.setState({ gMenu: null, gRen: null, groups: this.state.groups.map((x?: any) => x.id === id ? Object.assign({}, x, { open: v }) : x), gAnim: mode ? { id, mode } : null });
    if (!g.open) { setOpen(true, fast ? null : 'in'); if (!fast) this.gAnimT = this.tm(() => this.setState({ gAnim: null }), 260 + g.keys.length * 45); }
    else if (fast) setOpen(false, null);
    else { this.setState({ gAnim: { id, mode: 'out' }, gMenu: null }); this.gAnimT = this.tm(() => { setOpen(false, 'pop'); this.gAnimT = this.tm(() => this.setState({ gAnim: null }), 260); }, 200 + g.keys.length * 30); }
  }
  commitGroupName(id?: any) {
    const v = (this.state.gRenDraft || '').trim().slice(0, 20); const g = this.state.groups.find((x?: any) => x.id === id);
    if (!g) return; if (!v || v === g.name) { this.setState({ gRen: null, gMenu: null }); return; }
    if (this.state.groups.some((x?: any) => x.id !== id && x.name === v)) {
      this.showToast(translate("robot.workspace.toast.groupNameTaken", { name: v }), null, '', null, '', 3000);
      return;
    }
    this.setState({ gRen: null, gMenu: null, groups: this.state.groups.map((x?: any) => x.id === id ? Object.assign({}, x, { name: v }) : x) });
    this.addLog('order', translate("robot.workspace.log.groupRenamed", { from: g.name, to: v }));
  }
  nudgeAdd() { clearTimeout(this._nudgeT); this.setState({ nudge: 0 }); this._nudgeT = this.tm(() => { this.setState({ nudge: 1 }); this._nudgeT = this.tm(() => this.setState({ nudge: 0 }), 600); }, 20); }
  goWork() { const S = this.state; const dirty = !S.projName || S.savedSnap !== this.snapOf(S); const busy = ['preparing', 'playing', 'paused', 'moving', 'sending'].indexOf(S.st) >= 0 || S.preflight; this.setState({ menuOpen: false, leaveAsk: { kind: 'work', to: translate("robot.workspace.messages.myProjects") } }); if (!dirty && !busy) { this.setState({ leaveAsk: null }); this.navTo('work'); } }
  goNew() { const S = this.state; const dirty = !S.projName || S.savedSnap !== this.snapOf(S); const busy = ['preparing', 'playing', 'paused', 'moving', 'sending'].indexOf(S.st) >= 0 || S.preflight; this.setState({ menuOpen: false, leaveAsk: { kind: 'new', to: translate("home.create.title") } }); if (!dirty && !busy) { this.setState({ leaveAsk: null }); this.navTo('new'); } }
  openMerge() { const S = this.state; if (['preparing', 'playing', 'paused', 'moving', 'sending'].indexOf(S.st) >= 0 || S.selMode) return; this.setState({ menuOpen: false, mergeOpen: true, mWork: null, fdFolder: 0 }); }
  mergeAll() {
    const S = this.state, mW = this.MW.find((w) => w.id === S.mWork); if (!mW || !mW.poses.length) return;
    const picks = mW.poses, at = S.seq.length; const p2 = Object.assign({}, S.poses); let c = S.counter; const keys: any[] = [], byG: Record<string, any> = {};
    picks.forEach((m?: any) => { const k = 'p' + (c++); p2[k] = { name: m.name, v: m.v.slice(), t: m.t, ts: [m.t, m.t, m.t, m.t, m.t, m.t] }; keys.push(k); if (m.g) (byG[m.g] = byG[m.g] || []).push(k); });
    const s2 = S.seq.slice().concat(keys);
    const names = S.groups.map((g?: any) => g.name); const ng = Object.keys(byG).filter((g?: any) => byG[g].length >= 2).map((g?: any, ix?: any) => { let nm = g, q = 2; while (names.indexOf(nm) >= 0) nm = g + ' ' + (q++); names.push(nm); return { id: 'gm' + Date.now() + ix, name: nm, keys: byG[g], open: false }; });
    const back: Record<string, any> = { poses: S.poses, seq: S.seq, groups: S.groups, counter: S.counter, sel: S.sel };
    this.setState({ mergeOpen: false, poses: p2, seq: s2, counter: c, groups: this.fixGroups(s2, S.groups).concat(ng), sel: at, synced: false });
    const merged = { count: picks.length, name: mW.name, from: at + 1, to: at + picks.length };
    this.addLog('pose', translate("robot.workspace.log.importedPoses", merged));
    this.showToast(
      translate("robot.workspace.toast.importedPoses", merged),
      () => this.setState(Object.assign({ toast: null }, back)),
      "common.messages.undo",
      null,
      '',
      6000,
    );
  }
  playSrc() {
    const S = this.state; if (S.src !== 'robot') { this.play(); return; }
    const R = S.robotSeq; if (!R) return;
    if (S.loopInf && !this._robInfOk) { this.setState({ robInfAsk: true }); return; }
    this.clearAll();
    this.setState({ st: 'playing', robotRun: true, steps: [], pos: 0, robInfAsk: false });
    this.addLog('play', memoryPlayLogText(R.n, R.at, S.loopInf ? null : S.loops));
    if (!S.loopInf) {
      this.wait = this.tm(() => {
        this.setState({ st: 'done', robotRun: false });
        this.addLog('play', translate("robot.workspace.log.memoryPlayDone"));
      }, R.ms * Math.max(1, S.loops || 1));
    }
  }
  playGroup(a?: any, b?: any, nm?: any) {
    const S = this.state;
    const prev = this._grpPrev || { rangeOn: S.rangeOn, rs: S.rs, re: S.re, rk: S.rk || null };
    this._grpPrev = prev;
    this.setState({ rangeOn: true, mode: 'all', rs: a, re: b, rk: null, grpRun: nm, selMode: false, picks: [] });
    if (this.isChecked(S)) this.tm(() => this.play(), 0); else { this.clearAll(); this.setState({ preflight: true, checked: false }); }
  }
  togglePickGroup(g?: any) { const S = this.state; if (S.grpNaming) return; const all = g.keys.every((x?: any) => S.picks.indexOf(x) >= 0); this.setState({ picks: all ? S.picks.filter((x?: any) => g.keys.indexOf(x) < 0) : S.picks.filter((x?: any) => g.keys.indexOf(x) < 0).concat(g.keys) }); }
  gCopy(g?: any) { return poseEditing.gCopy(this, g); }
  gDelete(g?: any) { return poseEditing.gDelete(this, g); }
  gPaste(at?: any) { return poseEditing.gPaste(this, at); }
  busyNow() { const st = this.state.st; return ['preparing', 'playing', 'paused', 'moving', 'sending'].indexOf(st) >= 0 || this.state.preflight || this.state.selMode || this.state.mode !== 'all'; }
  gTarget() { const S = this.state; return S.groups.find((x?: any) => x.id === S.gFocus) || null; }
  pBusy() { const S = this.state; return ['preparing', 'playing', 'paused', 'moving', 'sending'].indexOf(S.st) >= 0 || !!S.preflight || S.mode !== 'all'; }
  pSrcKeys() { const S = this.state; if (S.selMode) return S.seq.filter((k?: any) => S.picks.indexOf(k) >= 0); const g = this.gTarget(); if (g) return g.keys.slice(); if (S.sel >= 0 && !S.p0sel && S.seq[S.sel]) return [S.seq[S.sel]]; return []; }
  pCopy(cut?: any, keys?: any) { return poseEditing.pCopy(this, cut, keys); }
  pDone() { const S = this.state; if (S.pClip && S.pClip.keys) { this.setState({ pMode: false, pClip: null }); this.showToast(translate("robot.workspace.toast.cutCancelled"), null, '', null, '', 2500); } else this.setState({ pMode: false }); }
  pasteClip(at?: any) { if (this.state.pClip) this.pPaste(at); else this.gPaste(at); }
  pPaste(at?: any) { return poseEditing.pPaste(this, at); }
  pasteAt() { const S = this.state, g = this.gTarget(); if (g) return S.seq.indexOf(g.keys[g.keys.length - 1]) + 1; if (S.sel >= 0) { const gg = S.groups.find((x?: any) => x.keys.indexOf(S.seq[S.sel]) >= 0); return gg ? S.seq.indexOf(gg.keys[gg.keys.length - 1]) + 1 : S.sel + 1; } return S.seq.length; }
  endGroupRun() { if (!this._grpPrev) return; const p = this._grpPrev; this._grpPrev = null; this.setState({ rangeOn: p.rangeOn, rs: p.rs, re: p.re, rk: p.rk, grpRun: null }); }
  stopAll() {
    const S = this.state;
    const running = S.st === "preparing" || S.st === "playing" || S.st === "paused";
    const order = this.order();
    const stoppedAt = running && order.length ? order[S.pos % order.length] : -1;
    if (S.release) this.relFinish("stop");
    this.freezeGlide();
    this.clearAll();
    this.endHold();
    this.setState({ st: "stopped", preflight: false, far: null, linked: -1, robotRun: false });
    if (this.state.mode === "range") this.setState({ mode: "all", picking: false, rangeDone: false, hoverIdx: -1 });
    this.endGroupRun();
    this.addLog("stop", stoppedAt >= 0
      ? translate("robot.workspace.log.stopAllDuringPose", { number: stoppedAt + 1 })
      : translate("robot.workspace.log.stopAll"));
  }
  tick(): void {
    sampleMotion.tick(this);
  }
  relOpen() { if (this.state.release) return; this.setState({ relAsk: true, jMenu: null, menuOpen: false }); }
  relStart(): void {
    sampleMotion.releaseStart(this);
  }
  relPoke(): void {
    sampleMotion.releasePoke(this);
  }
  relEndAsk(): void {
    sampleMotion.releaseEndAsk(this);
  }
  relCancelEnd(): void {
    sampleMotion.releaseCancelEnd(this);
  }
  relFinish(reason?: string): void {
    sampleMotion.releaseFinish(this, reason);
  }
  get MW() { return sampleMergeWorkspaces(); }
  readJoint(joint: number, event?: { stopPropagation(): void }): void { readJointInSample(this, joint, event); }
  setPose(k?: any, patch?: any) { return poseEditing.setPose(this, k, patch); }
  focusName6(k?: any) {
    const S = this.state, i = S.seq.indexOf(k); if (i < 0) return;
    if (S.view === 'table') {
      this.tm(() => {
        if (!this.dom.tileInput(k)) return;
        this.dom.scrollTileIntoView(k, 'auto');
        this.dom.focusTileInput(k);
      }, 40);
      return;
    }
    this.setState({ renaming: { k: 'p', i }, renameDraft: S.poses[k].name });
    this.tm(() => {
      this.dom.scrollTileIntoView(k, 'auto');
      this.dom.selectCardRenameInput();
    }, 60);
  }
  scrollTo6(i?: any) {
    const k = this.state.seq[i];
    this.tm(() => this.dom.scrollTileIntoView(k), 60);
  }
  insertPose(at?: any, pose?: any, msg?: any, joinKey?: any) { return poseEditing.insertPose(this, at, pose, msg, joinKey); }
  playSnap(S?: any) { return JSON.stringify([S.poses, S.seq, S.groups]); }
  isChecked(S?: any) { return !!S.checkedOnce && (S.checkedSnap == null || S.checkedSnap === this.playSnap(S)); }
  snapOf(S?: any) { return JSON.stringify([S.projName, S.poses, S.seq, S.jnames, S.groups, S.loops, S.loopInf]); }
  now2() { const d = new Date(); return String(d.getHours()).padStart(2, '0') + ':' + String(d.getMinutes()).padStart(2, '0'); }
  openCopy(title?: any, code?: any, js?: any) { return popovers.openCopy(this, title, code, js); }
  closeCopyL() { if (this.cpDown) window.removeEventListener('pointerdown', this.cpDown, true); if (this.cpKey) window.removeEventListener('keydown', this.cpKey); this.cpDown = this.cpKey = null; }
  closeCopy() { this.closeCopyL(); if (this.state.copyPop) this.setState({ copyPop: null }); }
  showToast(text?: any, act?: any, actLabel?: any, act2?: any, act2Label?: any, ms?: any) { clearTimeout(this.toastT); this.setState({ toast: { text, act, actLabel, act2, act2Label } }); this.toastT = this.tm(() => this.setState({ toast: null }), ms || 5000); }
  saveAs() {
    const S = this.state;
    const baseName = S.projName || translate("robot.workspace.project.defaultName");
    this.setState({
      menuOpen: false,
      projRenaming: true,
      saveAsMode: true,
      projDraft: translate("robot.workspace.messages.value0Copy", { value0: baseName }),
    });
  }
  saveProj() {
    const S = this.state;
    if (!S.projName) { this.setState({ projRenaming: true, projDraft: '', saveAfterName: true }); return; }
    this.setState({ savedSnap: this.snapOf(S), savedAt: this.now2() });
    this.showToast(translate("robot.workspace.toast.projectSaved", { name: S.projName }), null, '', null, '', 2500);
  }
  fixGroups(seq?: any, groups?: any) { return poseEditing.fixGroups(seq, groups); }
  removeAt(i?: any) { return poseEditing.removeAt(this, i); }
  over6(e?: any, i?: any, gCard?: any, axis?: any) { return poseEditing.over6(this, e, i, gCard, axis); }
  drop6() { return poseEditing.drop6(this); }
  moveTo(from?: any, gap?: any) { return poseEditing.moveTo(this, from, gap); }
  undoMove() { return poseEditing.undoMove(this); }
  moveIdx(from?: any, to?: any) { return poseEditing.moveIdx(this, from, to); }
  commitRename() { return poseEditing.commitRename(this); }

  netClass(mbps?: any, ping?: any) { if (mbps == null || mbps < 0.15) return 'off'; return (mbps >= 5 && ping < 300) ? 'ok' : 'slow'; }
  netTest() {
    const S = this.state; if (S.netMeas) return;
    if (S.netSt === 'off') { this.netPop('[data-net="menu"]'); return; }
    this.setState({ netMeas: true, netRes: null });
    clearTimeout(this.netT);
    this.netT = this.tm(() => { const sim = this.props.net === 'slow' ? { mbps: 2.1, ping: 420 } : (this.props.net === 'off' ? { mbps: null, ping: null } : { mbps: 48, ping: 40 }); this.setState({ netMeas: false, netRes: sim, netSt: this.netClass(sim.mbps, sim.ping) }); }, 1500);
  }
  netPop(q?: any) { return popovers.netPop(this, q); }
  renderVals() { return buildWorkspaceView(this); }

  _dead = false;
  _tms = new Set<number>();
  _ivs = new Set<number>();
  tm(fn: () => void, ms?: number): number {
    const id: number = window.setTimeout(() => {
      this._tms.delete(id);
      if (!this._dead) fn();
    }, ms);
    this._tms.add(id);
    return id;
  }
  iv(fn: () => void, ms?: number): number {
    const id: number = window.setInterval(() => {
      if (!this._dead) fn();
    }, ms);
    this._ivs.add(id);
    return id;
  }
  q<E extends HTMLElement = HTMLElement>(sel: string): E | null { return this.dom.query<E>(sel); }
  qa(sel: string): HTMLElement[] { return this.dom.queryAll(sel); }
  rootW(): number { const el = this.dom.root; return el ? el.clientWidth : 1440; }
  rootH(): number { const el = this.dom.root; return el ? el.clientHeight : 900; }

  applyBoot(props: LogicProps) {
    this._dead = false;
    const st = this.state;
    if (props.restore) Object.assign(st, props.restore);
    if (props.init) Object.assign(st, props.init);
    if (props.conn && props.conn !== 'connected') st.st = props.conn === 'connecting' ? 'connecting' : 'disconnected';
    this._lastProj = st.projName;
    this._lastConn = props.conn || 'connected';
  }
  afterMount() {
    this._dead = false;
    const a = this.props.boot;
    if (a === 'upload') this.tm(() => this.renderVals().send(), 0);
    else if (a === 'play') this.tm(() => { this.setState({ st: 'playing' }); this.tick(); }, 0);
    else if (a === 'dirty') this.setState({ savedSnap: '' });
    else if (a === 'netHdr') this.tm(() => this.netPop('[data-net="hdr"]'), 60);
  }
  componentDidUpdate() {
    const p = this.props, S = this.state;
    if (p.onProjName && S.projName !== this._lastProj) { this._lastProj = S.projName; if (S.projName) p.onProjName(S.projName); }
    const c = p.conn || 'connected';
    if (c !== this._lastConn) { this._lastConn = c; this.applyConn(c); }
    reconcileSelection(this);
    reconcileRange(this);
  }

  applyConn(c: string) {
    const S = this.state;
    if (c === 'connected') {
      if (S.st === 'disconnected' || S.st === 'connecting') this.setState({ st: 'ready', linked: -1, snap: null });
      return;
    }
    this.freezeGlide(); this.clearAll(); this.endHold();
    clearInterval(this.relSimT); clearInterval(this.relCdT); clearTimeout(this.relIdleT);
    this.setState({ st: c === 'connecting' ? 'connecting' : 'disconnected', preflight: false, far: null, release: false, relAsk: false, relCount: 0, robotRun: false, linked: -1, snap: null, picking: false, hoverIdx: -1, boxMenu: false, msgPop: null, ...(S.mode === 'range' ? { mode: 'all', rangeDone: false } : {}) });
  }
  navTo(kind: string) { if (this.props.onNav) this.props.onNav(kind); if (kind === 'new') this.newProject(); }
  leaveNav() { const k = this.state.leaveAsk && this.state.leaveAsk.kind; this.setState({ leaveAsk: null }); if (k) this.navTo(k); }
  leaveSaveNav() { this.saveProj(); this.leaveNav(); }
  openExternal(url: string): void {
    try {
      window.open(url, '_blank', 'noopener');
    } catch (error) {
      // The external help site is optional, so a blocked popup must not interrupt editing.
      console.warn("Unable to open external help", error);
    }
  }

  newProject() {
    const nm = translate("robot.workspace.project.defaultName");
    this.setState({ poses: {}, seq: [], groups: [], sel: -1, p0sel: false, linked: -1, snap: null, rangeOn: false, mode: 'all', synced: false, picks: [], selMode: false, projName: nm, menuOpen: false, leaveAsk: null, steps: [], pos: 0, elapsed: 0, jnames: this.SV.map((x: any) => x.name), log: [{ time: this.now(), type: 'conn', text: translate("robot.workspace.log.newProjectStarted") }] });
    this.setState({ savedSnap: this.snapOf(this.state), savedAt: this.now2() });
  }

  snapshotForStash() {
    const S = this.state;
    const keep = ['poses', 'seq', 'groups', 'counter', 'jnames', 'projName', 'savedSnap', 'savedAt', 'robotSeq', 'synced', 'syncTime', 'loops', 'loopInf', 'src', 'log', 'view', 'timeMode', 'sel', 'jsel', 'cur', 'rangeOn', 'rs', 're', 'rk', 'lang', 'netSt', 'netRes', 'jdens6', 't6full', 'checkedOnce', 'checkedSnap'];
    const o: any = {};
    keep.forEach((k) => { o[k] = S[k]; });
    o.linked = S.st === 'ready' ? S.linked : -1;
    o.snap = S.st === 'ready' ? S.snap : null;
    return o;
  }
  beforeUnmount() {
    if (this.props.onStash && !this._dead) this.props.onStash(this.snapshotForStash());
    this._dead = true;
    this._tms.forEach((id) => clearTimeout(id));
    this._ivs.forEach((id) => clearInterval(id));
    this._tms.clear(); this._ivs.clear();
    clearTimeout(this.glideT); clearTimeout(this.relIdleT); clearInterval(this.relCdT); clearInterval(this.relSimT); clearTimeout(this.netT); clearTimeout(this.newT); clearTimeout(this.landT); clearTimeout(this.movedT); clearTimeout(this.gAnimT); clearTimeout(this.sbT); clearTimeout(this._tfT); clearTimeout(this._nudgeT); clearTimeout(this.runT1); clearTimeout(this.runT2);
    if (this.guard6) { document.removeEventListener('pointerdown', this.guard6, true); document.removeEventListener('click', this.guard6, true); }
    if (this.wheel6) document.removeEventListener('wheel', this.wheel6);
    if (this.onGDel) window.removeEventListener('keydown', this.onGDel);
    this.closeCopyL();
    if (this._mpL) { window.removeEventListener('pointerdown', this._mpL, true); window.removeEventListener('keydown', this._mpL, true); this._mpL = null; }
  }
}

