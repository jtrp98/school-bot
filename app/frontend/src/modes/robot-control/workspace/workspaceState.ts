import type { LogicProps, WorkspaceLogic } from "./WorkspaceLogic";
import { JOINTS } from "./types";

export function createWorkspaceState(props: LogicProps) {
  const demo = props.demo || 'empty';
  const createPose = (name: string, values: number[], duration: number, times?: number[]) => ({
    name,
    v: values,
    t: duration,
    ts: times || [duration, duration, duration, duration, duration, duration],
  });
  const empty = demo === 'empty';
  const state: WorkspaceLogic["state"] = {
    st: demo === 'playing' ? 'playing' : 'ready',
    cur: (demo === 'release' || demo === 'releaseIdle') ? [0, -25, 45, 10, 0, 60] : [0, 0, 0, 0, 0, 0],
    poses: empty ? {} : {
      above: createPose('เหนือกล่อง', [0, -40, 30, 0, 0, 100], 3000),
      down: createPose('ลงคีบ', [0, -10, 70, -10, 0, 100], 3000, [3000, 3800, 3000, 3000, 3000, 3000]),
      grab: createPose('คีบแล้ว', [0, -10, 70, -10, 0, 20], 3000),
      lift: createPose('ยกขึ้น', [0, -50, 20, 0, 0, 20], 3000, [3000, 3000, 4200, 3000, 3000, 3000])
    },
    seq: empty ? [] : ['above', 'down', 'grab', 'lift'],
    jnames: JOINTS.map((joint) => joint.name),
    renaming: null,
    renameDraft: '',
    sel: empty ? -1 : 0,
    linked: -1,
    runHint: 0,
    nudge: 0,
    p0sel: false,
    snap: null,
    jsel: 1,
    holding: null,
    limitMsg: '',
    moveName: '',
    far: null,
    valDraft: null,
    boxMenu: false,
    timeEdit: -1,
    projName: props.workName || 'แขนหยิบกล่อง',
    savedSnap: null,
    savedAt: '10:20',
    projRenaming: false,
    projDraft: '',
    menuOpen: false,
    netSt: (props && props.net === 'slow') ? 'slow' : ((props && props.net === 'off') ? 'off' : 'ok'),
    netMeas: false,
    netRes: null,
    lang: 'th',
    clearAsk: false,
    toast: null,
    copyPop: null,
    robInfAsk: false,
    release: demo === 'release' || demo === 'releaseIdle',
    relAsk: false,
    relIdle: demo === 'releaseIdle',
    relCount: 0,
    relRead: [],
    reading: false,
    pClip: null,
    pMode: false,
    pctx: null,
    selMode: false,
    picks: [],
    groups: [],
    grpNaming: false,
    grpDraft: '',
    hoverIdx: -1,
    moveMs: null,
    movePick: -1,
    moved: null,
    rangeOn: false,
    scopeOpen: false,
    sb: { show: false, h: 0, top: 0, vis: false },
    timeMode: demo === 'split' ? 'split' : 'same',
    tmConfirm: false,
    view: demo === 'table' || demo === 'split' ? 'table' : 'cards',
    mode: (demo === 'range' || demo === 'playing') && props.showRange ? 'range' : (demo === 'order' ? 'order' : 'all'),
    rs: 1,
    re: 2,
    picking: false,
    dragFrom: -1,
    loops: 1,
    loopInf: false,
    loopMenuOpen: false,
    lap: 0,
    steps: demo === 'playing' ? [1, 2] : [],
    pos: 0,
    elapsed: demo === 'playing' ? 500 : 0,
    preflight: false,
    checked: false,
    checkedOnce: demo === 'playing',
    checkedSnap: null,
    msgPop: null,
    goPop: -1,
    keyPop: null,
    keyTipSeen: false,
    synced: false,
    syncTime: '',
    src: 'app',
    robotSeq: { n: 3, ms: 9000, at: '09:50', names: 'เหนือกล่อง · ลงคีบ · คีบแล้ว' },
    robotRun: false,
    counter: 5,
    delAsk: false,
    menuIdx: -1,
    baud: false,
    logOpen: demo === 'log',
    logFilter: 'all',
    log: [
      { time: '10:02', type: 'conn', text: 'เชื่อมต่อหุ่นแขนกล (COM7)' },
      { time: '10:05', type: 'pose', text: 'เพิ่มท่า 1 “เหนือกล่อง”' },
      { time: '10:08', type: 'pose', text: 'เพิ่มท่า 2 “ลงคีบ”' },
      { time: '10:11', type: 'pose', text: 'แก้ท่า 2: S1 −20° → −10° · S2 60° → 70°' },
      { time: '10:14', type: 'play', text: 'เล่นทั้งหมด 4 ท่า 1 รอบ · ครบ' },
      { time: '10:15', type: 'stop', text: 'กดหยุดทั้งหมด (ระหว่างเล่นท่า 3)' },
      { time: '10:17', type: 'err', text: 'S2 ศอก ถึงขีดจำกัด (+135°)' }
    ]
  };
  if (empty) state.log = state.log.slice(0, 1);
  if (demo === 'viewing' || demo === 'farConfirm') {
    state.sel = 3;
    state.linked = 0;
    state.cur = state.poses.above.v.slice();
  }
  if (demo === 'linked' || demo === 'split' || demo === 'table') {
    state.sel = 1;
    state.linked = 1;
    state.cur = [0, -4, 70, -10, 0, 100];
    state.poses.down = Object.assign({}, state.poses.down, { v: [0, -4, 70, -10, 0, 100] });
    state.snap = {
      key: 'down',
      p: createPose('ลงคีบ', [0, -10, 70, -10, 0, 100], 3000, [3000, 3800, 3000, 3000, 3000, 3000]),
    };
  }
  if (demo === 'farConfirm') state.far = { j: 1, dir: 1, maxd: 50, sid: 'S1' };
  return state;
}
