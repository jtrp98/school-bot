import { t as translate } from "../../../i18n";
import type { WorkspaceLogic } from "./WorkspaceLogic";
import { playStartLogText } from "./workspaceText";

export function glide(screen: WorkspaceLogic, values: Array<number | string>, times: number[]): void {
  const from: number[] = screen.state.cur.slice();
  const to = values.map((value) => screen.num(value, 0));
  screen.gl = { from, to, ts: times, t0: Date.now() };
  screen.setState({ cur: to, moveMs: times, moveFrom: from });
  clearTimeout(screen.glideT);
  screen.glideT = screen.tm(() => {
    screen.gl = null;
    screen.setState({ moveMs: null });
  }, Math.max(...times) + 30);
}

export function freezeGlide(screen: WorkspaceLogic): void {
  const glide = screen.gl;
  if (!glide) return;
  const elapsed = Date.now() - glide.t0;
  const cur = glide.to.map((value, joint) => {
    const progress = Math.min(1, elapsed / glide.ts[joint]);
    return Math.round(glide.from[joint] + (value - glide.from[joint]) * progress);
  });
  screen.gl = null;
  clearTimeout(screen.glideT);
  screen.setState({ cur, moveMs: null });
}

export function syncTo(screen: WorkspaceLogic, after?: () => void): void {
  const state = screen.state;
  const pose = screen.selPose();
  if (!pose) return;
  const index: number = state.sel;
  const duration = Math.max(1000, screen.dur(pose));
  screen.clearAll();
  screen.setState({
    st: "moving",
    moveName: translate("robot.workspace.pose.numbered", { number: index + 1, name: pose.name }),
    far: null,
  });
  screen.glide(pose.v, screen.timesOf(pose).map((time: number) => Math.max(time, 1000)));
  screen.addLog("move", translate("robot.workspace.log.moveToPose", { number: index + 1, name: pose.name }));
  screen.wait = screen.tm(() => {
    const currentPose = screen.selPose();
    screen.setState({
      st: "ready",
      moveName: "",
      cur: currentPose.v.map((value: number | string) => screen.num(value, 0)),
      linked: index,
      snap: { key: screen.state.seq[index], p: JSON.parse(JSON.stringify(currentPose)) },
    });
    after?.();
  }, duration);
}

export function play(screen: WorkspaceLogic): void {
  const order: number[] = screen.order();
  const steps: number[] = [];
  for (let loop = 0; loop < (screen.state.loopInf ? 1 : screen.state.loops); loop++) {
    order.forEach((index) => steps.push(index));
  }
  screen.clearAll();
  screen.logEditOf(screen.state);
  screen.setState({ st: "preparing", steps, pos: 0, elapsed: 0, lap: 0, far: null });
  const ranged = screen.state.mode === "range" || screen.state.rangeOn;
  screen.addLog("play", playStartLogText(
    ranged ? { from: order[0] + 1, to: order[order.length - 1] + 1 } : null,
    order.length,
    screen.state.loopInf ? null : screen.state.loops,
  ));
  screen.wait = screen.tm(() => {
    screen.setState({ st: "playing" });
    const firstPose = screen.state.poses[screen.state.seq[steps[0]]];
    if (firstPose) screen.glide(firstPose.v, screen.timesOf(firstPose));
    screen.tick();
  }, 900);
}

export function tick(screen: WorkspaceLogic): void {
  clearInterval(screen.timer);
  screen.timer = screen.iv(() => {
    const state = screen.state;
    const duration = screen.dur(state.poses[state.seq[state.steps[state.pos]]]);
    let elapsed: number = state.elapsed + 100;
    let position: number = state.pos;
    let lap: number = state.lap || 0;
    if (elapsed >= duration) {
      position += 1;
      elapsed = 0;
    }
    if (state.loopInf && position >= state.steps.length) {
      position = 0;
      lap += 1;
    }
    if (position >= state.steps.length) {
      clearInterval(screen.timer);
      const lastIndex: number = state.steps[state.steps.length - 1];
      const lastPose = state.poses[state.seq[lastIndex]];
      screen.setState({
        st: "done",
        pos: position,
        elapsed: 0,
        cur: lastPose.v.map((value: number | string) => screen.num(value, 0)),
        linked: lastIndex,
        snap: state.sel === lastIndex
          ? { key: state.seq[lastIndex], p: JSON.parse(JSON.stringify(lastPose)) }
          : state.snap,
      });
      screen.addLog("play", translate("robot.workspace.log.playFinished", { number: lastIndex + 1 }));
      screen.endGroupRun();
      return;
    }
    screen.setState({ elapsed, pos: position, lap });
    if (position !== state.pos) {
      const nextPose = state.poses[state.seq[state.steps[position]]];
      if (nextPose) screen.glide(nextPose.v, screen.timesOf(nextPose));
    }
  }, 100);
}

export function releaseStart(screen: WorkspaceLogic): void {
  const state = screen.state;
  screen.clearAll();
  const from: number[] = state.cur.slice();
  const drift = [0, 15, 15, 10, 0, -40];
  const to = from.map((value, joint) =>
    Math.max(screen.SV[joint].min, Math.min(screen.SV[joint].max, value + drift[joint])));
  screen.setState({ relAsk: false, release: true, relRead: [], relIdle: false, relCount: 0, linked: -1 });
  screen.addLog("move", translate("robot.workspace.log.jointsReleased"));
  let tick = 0;
  clearInterval(screen.relSimT);
  screen.relSimT = screen.iv(() => {
    tick++;
    const progress = Math.min(1, tick / 20);
    screen.setState({ cur: from.map((value, joint) => Math.round(value + (to[joint] - value) * progress)) });
    if (progress >= 1) clearInterval(screen.relSimT);
  }, 100);
  screen.relPoke();
}

export function releasePoke(screen: WorkspaceLogic): void {
  clearTimeout(screen.relIdleT);
  screen.relIdleT = screen.tm(() => {
    if (screen.state.release) screen.setState({ relIdle: true });
  }, 120000);
}

export function releaseEndAsk(screen: WorkspaceLogic): void {
  clearInterval(screen.relCdT);
  screen.setState({ relIdle: false, relCount: 3 });
  screen.relCdT = screen.iv(() => {
    const count: number = screen.state.relCount - 1;
    if (count <= 0) {
      clearInterval(screen.relCdT);
      screen.relFinish("");
    } else screen.setState({ relCount: count });
  }, 1000);
}

export function releaseCancelEnd(screen: WorkspaceLogic): void {
  clearInterval(screen.relCdT);
  screen.setState({ relCount: 0, relIdle: false });
  screen.relPoke();
}

export function releaseFinish(screen: WorkspaceLogic, reason?: string): void {
  clearInterval(screen.relCdT);
  clearInterval(screen.relSimT);
  clearTimeout(screen.relIdleT);
  if (!screen.state.release) return;
  screen.setState({ release: false, relCount: 0, relIdle: false, relRead: [], relAsk: false });
  screen.addLog("move", translate(reason === "stop"
    ? "robot.workspace.log.jointsLockedByStop"
    : "robot.workspace.log.jointsLocked"));
}
