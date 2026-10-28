import { t as translate } from "../../../i18n";
import { WorkspaceLogic, type LogicProps } from "./WorkspaceLogic";
import { Template } from "./template";
import { client } from "../../../core/client";
import { registerWindowCloseGuard } from "../../../core/windowLifecycle";
import type { RobotApi } from "../robotContext";
import { parseServoPosition, poseGroup, servoItem, validateProfile, type RobotProfile } from "../engineAdapter";
import { engineErrorText } from "../engineErrorText";
import { buildNativeWorkspaceView, type NativeViewActions } from "./nativeWorkspaceView";
import type { FlagPose } from "./workspaceFlags";

type NativeProps = LogicProps & { robotApi: RobotApi };

export class NativeWorkspace extends WorkspaceLogic {
  declare props: Readonly<NativeProps>;
  private generation = 0;
  private sending = false;
  private stopping = false;
  private saving = false;
  private pending = new Map<number, { value: number; key: string }>();
  private jointTimer?: number;
  private removeCloseGuard?: () => void;
  private lastTarget: RobotApi["robot"] | undefined;
  private readonly nativeActions: NativeViewActions = {
    unsupportedPreset: () =>
      this.showToast(translate("robot.workspace.messages.thisPresetIsNotCalibratedForThisRobotCreate")),
    send: () => void this.uploadNative(),
    readAll: () => void this.readNative([0, 1, 2, 3, 4, 5]),
    readNew: () => void this.readNative([0, 1, 2, 3, 4, 5], true),
    pauseUnsupported: () =>
      this.showToast(translate("robot.workspace.messages.robotDoesNotSupportPauseResumeStopAllThen")),
    ignorePress: () => undefined,
    netTest: () => this.showToast(translate("robot.workspace.messages.usbControlWorksWithoutInternet")),
  };
  constructor(props: NativeProps) {
    super(props);
    this.state.robotSeq = null; this.state.synced = false; this.state.linked = -1; this.state.snap = null;
    this.state.log = props.restore?.log ?? []; this.state.savedAt = props.restore?.savedAt ?? "";
  }
  private connected() { return this.props.robotApi.conn === "connected" && !this.props.robotApi.sample; }
  private profile(): RobotProfile {
    const profile = this.props.robotApi.robot?.profile;
    if (!profile) throw new Error(translate("robot.workspace.error.profileRequired"));
    return validateProfile(profile);
  }
  private available() {
    if (this.connected() && ["ready", "done", "stopped"].includes(this.state.st)
      && !this.sending && !this.stopping && !this.pending.size && !this.props.robotApi.busy()) return true;
    this.showToast(translate(
      this.connected() ? "robot.workspace.toast.commandInProgress" : "robot.workspace.toast.connectFirst"));
    return false;
  }
  private fail(error: unknown) {
    if (this._dead) return;
    this.setState({
      st: this.connected() ? "ready" : "disconnected",
      linked: -1,
      snap: null,
      moveMs: null,
      moveName: "",
      robotRun: false,
    });
    this.addLog("err", engineErrorText(error));
    const reason = engineErrorText(error);
    this.showToast(translate("robot.workspace.toast.commandFailed", { reason }), null, "", null, "", 8000);
  }
  private invalidate() {
    this.generation++; this.pending.clear(); window.clearTimeout(this.jointTimer);
    this.clearAll(); this.endHold(); this.freezeGlide();
    if (!this._dead) this.setState({ reading: false });
  }
  private editable() {
    const s = this.state;
    return this.connected() && !this.stopping && ["ready", "stopped", "done"].includes(s.st)
      && !s.release && !s.preflight && !s.far && s.mode === "all" && s.sel >= 0 && s.sel === s.linked && !s.p0sel;
  }
  override insertPose(at: number, pose: FlagPose, message: string, joinKey?: string) {
    super.insertPose(at, pose, message, joinKey);
    // Equality with the last commanded display is not a measurement of this arm.
    this.setState({ linked: -1, snap: null });
  }
  override setAbs(j: number, raw: number) {
    if (!this.editable()) return;
    const s = this.state, key = s.seq[s.sel], pose = s.poses[key], joint = this.SV[j];
    const value = Math.max(joint.min, Math.min(joint.max, Math.round(raw)));
    if (!Number.isFinite(value) || !pose) return;
    const values = [...pose.v]; values[j] = value;
    this.setState({
      poses: { ...s.poses, [key]: { ...pose, v: values } },
      synced: false,
      jsel: j,
      valDraft: null,
    });
    this.pending.set(j, { value, key }); window.clearTimeout(this.jointTimer);
    this.jointTimer = window.setTimeout(() => void this.flushJoints(), 100);
  }
  override bump(j: number, dir: number) {
    const p = this.selPose();
    if (p) this.setAbs(j, Number(p.v[j]) + dir * this.SV[j].step);
  }
  override stepVal(j: number, dir: number, big?: boolean) {
    const p = this.selPose();
    if (p) this.typeVal(j, Number(p.v[j]) + dir * (this.SV[j].pct
      ? (big ? 20 : 5)
      : (big ? 10 : 1)));
  }
  private async flushJoints() {
    if (this.sending || this.stopping || !this.connected() || !this.pending.size) return;
    if (this.props.robotApi.busy()) { this.jointTimer = window.setTimeout(() => void this.flushJoints(), 100); return; }
    const token = this.generation; this.sending = true;
    try {
      const profile = this.profile();
      while (this.pending.size && token === this.generation && this.connected()) {
        const [j, next] = this.pending.entries().next().value!; this.pending.delete(j);
        const pose = this.state.poses[next.key];
        if (!pose) continue;
        await this.props.robotApi.run("servo.set",
          servoItem(profile, j, next.value, Math.round(this.timesOf(pose)[j])));
        if (token !== this.generation || this._dead) return;
        const cur = [...this.state.cur]; cur[j] = next.value; this.setState({ cur });
      }
    } catch (e) { this.pending.clear(); if (token === this.generation) this.fail(e); }
    finally { this.sending = false; }
  }
  private async move(
    values: number[], times: number[], name: string, done: Record<string, unknown>, after?: () => void,
  ) {
    if (this.state.release) {
      this.showToast(translate("robot.workspace.toast.lockBeforeSendPose"));
      return;
    }
    if (!this.available()) return;
    const token = this.generation; this.sending = true; this.clearAll();
    this.setState({ st: "moving", moveName: name, far: null, linked: -1 });
    try {
      const group = poseGroup(this.profile(), values, times);
      await this.props.robotApi.run("group.execute", { group });
      if (token !== this.generation || this._dead || !this.connected()) return;
      this.glide(values, times); // Start visual prediction only after OS transmission succeeds.
      this.addLog("move", translate("robot.workspace.log.poseValuesSent", { pose: name }));
      this.wait = this.tm(() => {
        if (token !== this.generation) return;
        this.setState({ st: "ready", moveName: "", cur: [...values], ...done }); after?.();
      }, Math.max(...times));
    } catch (e) { if (token === this.generation) this.fail(e); }
    finally { this.sending = false; }
  }
  override syncTo(after?: () => void) {
    const s = this.state, p = this.selPose(), i = s.sel, key = s.seq[i]; if (!p) return;
    void this.move(
      [...p.v],
      this.timesOf(p),
      translate("robot.workspace.pose.numbered", { number: i + 1, name: p.name }),
      { linked: i, snap: { key, p: JSON.parse(JSON.stringify(p)) } },
      after,
    );
  }
  override play() { void this.playNative(); }
  override tick() { /* The native pose loop owns timing and transmission. */ }
  private async playNative() {
    if (this.state.release) {
      this.showToast(translate("robot.workspace.toast.lockBeforePlay"));
      return;
    }
    if (!this.available()) return;
    const s = this.state, order: number[] = this.order();
    if (!order.length || order.some(i => !this.poseOk(s.poses[s.seq[i]]))) return;
    const steps: number[] = [];
    for (let n = 0; n < (s.loopInf ? 1 : s.loops); n++) steps.push(...order);
    if (!steps.length) return;
    const token = ++this.generation; this.clearAll();
    this.setState({
      st: "preparing",
      steps,
      pos: 0,
      elapsed: 0,
      lap: 0,
      far: null,
      preflight: false,
      linked: -1,
      snap: null,
      robotRun: false,
    });
    const next = async () => {
      if (token !== this.generation || this._dead || !this.connected()) return;
      const pos = this.state.pos, p = this.state.poses[this.state.seq[steps[pos]]], times = this.timesOf(p);
      this.sending = true;
      try { await this.props.robotApi.run("group.execute", { group: poseGroup(this.profile(), p.v, times) }); }
      catch (e) { if (token === this.generation) this.fail(e); return; }
      finally { this.sending = false; }
      if (token !== this.generation || this._dead || !this.connected()) return;
      this.setState({ st: "playing" }); this.glide(p.v, times);
      const started = performance.now();
      this.timer = this.iv(() => {
        if (token !== this.generation) return;
        const elapsed = performance.now() - started;
        if (elapsed < Math.max(...times)) { this.setState({ elapsed }); return; }
        clearInterval(this.timer); const following = pos + 1;
        if (following >= steps.length && !s.loopInf) {
          this.setState({ st: "done", pos: following, elapsed: 0, cur: [...p.v], linked: steps[pos], snap: null });
          this.addLog("play", translate("robot.workspace.log.sequenceSent"));
          this.endGroupRun();
          return;
        }
        this.setState({
          st: "preparing",
          pos: following >= steps.length ? 0 : following,
          elapsed: 0,
          lap: following >= steps.length ? this.state.lap + 1 : this.state.lap,
        });
        void next();
      }, 50);
    };
    await next();
  }
  override playSrc() { if (this.state.src !== "robot") return this.play(); void this.playBoard(); }
  private async playBoard() {
    if (this.state.release) {
      this.showToast(translate("robot.workspace.toast.lockBeforePlay"));
      return;
    }
    const s = this.state;
    if (!s.robotSeq || !this.available()) return;
    if (s.loopInf) {
      this.showToast(translate("robot.workspace.toast.memoryPlayNeedsLoopCount"));
      return;
    }
    const token = this.generation; this.sending = true;
    this.setState({ st: "preparing", preflight: false, linked: -1 });
    try {
      await this.props.robotApi.run("group.run", {
        start: 0,
        end: s.robotSeq.n - 1,
        times: Math.min(9999, Math.max(1, s.loops)),
      });
      if (token !== this.generation || this._dead) return;
      this.setState({ st: "playing", robotRun: true, steps: [], pos: 0 });
      this.addLog("play", translate("robot.workspace.log.memoryPlaySent"));
      this.showToast(translate("robot.workspace.toast.memoryPlaySent"), null, "", null, "", 8000);
    } catch (e) { if (token === this.generation) this.fail(e); }
    finally { this.sending = false; }
  }
  private async uploadNative() {
    if (this.state.release) {
      this.showToast(translate("robot.workspace.toast.lockBeforeUpload"));
      return;
    }
    if (!this.available()) return;
    const s = this.state, keys: string[] = [...s.seq];
    if (!keys.length || keys.length > 10000 || keys.some(k => !this.poseOk(s.poses[k]))) return;
    const token = ++this.generation; this.sending = true;
    this.setState({ st: "sending", synced: false, robotSeq: null, uploadProgress: 0 });
    try {
      const profile = this.profile();
      for (let i = 0; i < keys.length; i += 4) {
        if (token !== this.generation || !this.connected()) return;
        const groups = keys.slice(i, i + 4).map((key, j) =>
          poseGroup(profile, s.poses[key].v, this.timesOf(s.poses[key]), i + j));
        await this.props.robotApi.run("group.download", { groups });
        if (token !== this.generation || this._dead) return;
        this.setState({ uploadProgress: Math.min(keys.length, i + groups.length) });
      }
      this.setState({
        st: "ready",
        synced: false,
        robotSeq: {
          n: keys.length,
          ms: keys.reduce((sum, key) => sum + this.dur(s.poses[key]), 0),
          at: this.now(),
          names: keys.slice(0, 3).map(key => s.poses[key].name).join(" · "),
        },
      });
      this.addLog("send", translate("robot.workspace.log.uploaded", { count: keys.length }));
      this.showToast(translate("robot.workspace.toast.uploaded"), null, "", null, "", 8000);
    } catch (e) { if (token === this.generation) this.fail(e); }
    finally { this.sending = false; }
  }
  private async stopNative() {
    if (this.stopping) return;
    const released = this.state.release; this.stopping = true; this.invalidate();
    this.setState({ st: "preparing", preflight: false, far: null, linked: -1, snap: null, robotRun: false });
    try {
      await this.props.robotApi.stop(released);
      if (this._dead) return;
      this.setState({ st: "stopped", release: false, relRead: [], moveName: "", moveMs: null, reading: false });
      this.addLog("stop", translate("robot.workspace.log.stopSent"));
      this.endGroupRun();
    } catch (e) { this.fail(e); }
    finally { this.stopping = false; }
  }
  override stopAll() { void this.stopNative(); }
  override relStart() { void this.torque(false); }
  override relFinish() { void this.torque(true); }
  override relEndAsk() { void this.torque(true); }
  private async torque(enabled: boolean) {
    if (!this.available()) return;
    const token = this.generation; this.sending = true; this.setState({ st: "preparing", relAsk: false });
    try {
      this.profile(); await this.props.robotApi.run("servo.torque", { enabled });
      if (token !== this.generation || this._dead) return;
      this.setState({
        st: "ready", release: !enabled, linked: -1, snap: null, relRead: [], relCount: 0, relIdle: false,
      });
      this.addLog("move", translate(enabled ? "robot.workspace.log.lockSent" : "robot.workspace.log.unlockSent"));
    } catch (e) { if (token === this.generation) this.fail(e); }
    finally { this.sending = false; }
  }
  override gcodePart(j: number) {
    if (!this.props.robotApi.robot?.profile) return translate("robot.workspace.native.profileUnconfirmed");
    const s = this.state, p = s.p0sel ? this.POSE0 : this.selPose(),
      item = servoItem(this.profile(), j, Number(p?.v[j] ?? s.cur[j]), Math.round(p ? this.timesOf(p)[j] : 1000));
    return `#${String(item.id).padStart(3, "0")}P${String(item.pwm).padStart(4, "0")}T${
      String(item.time_ms).padStart(4, "0")}!`;
  }
  override applyConn(conn: string) { this.invalidate(); super.applyConn(conn); }
  override componentDidUpdate() {
    const target = this.props.robotApi.robot;
    if (target !== this.lastTarget) {
      if (this.lastTarget !== undefined) this.invalidate();
      this.lastTarget = target;
    }
    super.componentDidUpdate();
  }
  override readJoint(joint: number) { void this.readNative([joint]); }
  private async readNative(joints: number[], create = false) {
    if (!this.available()) return;
    const s = this.state, key = s.seq[s.sel], selected = s.sel, pose = s.poses[key];
    if ((!pose || s.p0sel) && !create) {
      this.showToast(translate("robot.workspace.toast.selectPoseBeforeRead"));
      return;
    }
    const token = this.generation, values = [...s.cur]; this.sending = true;
    this.setState({ reading: true, st: "preparing" });
    try {
      const profile = this.profile();
      for (const j of joints) {
        const reply = await this.props.robotApi.run("servo.read", { id: profile.servos[j].id });
        if (token !== this.generation || this._dead || !this.connected()) return;
        values[j] = parseServoPosition(profile, j, reply);
      }
      if (create) {
        const fromRobot = {
          name: translate("robot.workspace.pose.fromRobot"),
          v: values,
          t: 3000,
          ts: values.map(() => 3000),
        };
        this.insertPose(s.seq.length, fromRobot, translate("robot.workspace.log.poseFromRobot"));
      }
      else {
        const v = [...pose.v]; joints.forEach(j => { v[j] = values[j]; });
        this.setState({ poses: { ...this.state.poses, [key]: { ...pose, v } }, synced: false });
      }
      this.setState({
        cur: values,
        st: "ready",
        reading: false,
        linked: joints.length === 6 && !create && !s.release ? selected : -1,
        relRead: [...new Set([...(s.relRead || []), ...joints])],
      });
      this.addLog("pose", translate("robot.workspace.log.servoReadCommitted"));
      this.showToast(translate("robot.workspace.toast.servoReadCommitted"));
    } catch (e) { if (token === this.generation) { this.setState({ reading: false }); this.fail(e); } }
    finally { this.sending = false; }
  }
  override afterMount() {
    this._dead = false;
    if (!this.props.restore?.savedAt) this.setState({ savedSnap: "", savedAt: "" });
    this.removeCloseGuard = registerWindowCloseGuard(async () => {
      const released = this.state.release;
      this.invalidate();
      await this.props.robotApi.stop(released);
    });
  }
  override saveProj() { void this.persist(); }
  private async persist() {
    if (this.saving) return false;
    this.saving = true;
    const snapshot = this.snapshotForStash(), snap = this.snapOf(this.state), at = this.now();
    try {
      await client.save("robot-control", {
        kind: "zide-workspace",
        version: 1,
        state: { ...snapshot, savedSnap: snap, savedAt: at },
      });
      if (!this._dead) {
        this.setState({ savedSnap: snap, savedAt: at, menuOpen: false });
        this.showToast(translate("robot.workspace.toast.projectSavedLocally"));
      }
      return true;
    } catch (e) {
      if (!this._dead) this.showToast(translate("robot.workspace.toast.saveFailed", { reason: String(e) }));
      return false;
    }
    finally { this.saving = false; }
  }
  override saveAs() { this.showToast(translate("robot.workspace.toast.singleProjectSave")); }
  override leaveSaveNav() { void this.persist().then(ok => { if (ok && !this._dead) this.leaveNav(); }); }
  override snapshotForStash() {
    return {
      ...super.snapshotForStash(),
      robotSeq: null,
      synced: false,
      linked: -1,
      snap: null,
      checkedOnce: false,
      checkedSnap: null,
    };
  }
  override beforeUnmount() {
    this.removeCloseGuard?.();
    // A completed OS write can still leave a long servo movement running.
    // Stop the selected target on exit even when no request remains in flight.
    const stopOnExit = this.connected() || this.sending || this.pending.size || this.state.release
      || ["moving", "playing", "preparing", "sending"].includes(this.state.st), released = this.state.release;
    this.invalidate();
    if (stopOnExit) {
      void this.props.robotApi.stop(released).catch(error => console.warn("Unable to stop on workspace exit", error));
    }
    super.beforeUnmount();
  }
  override renderVals() {
    return buildNativeWorkspaceView(super.renderVals(), {
      robot: this.props.robotApi.robot,
      connection: this.props.robotApi.conn,
      connected: this.connected(),
      status: this.state.st,
      linked: this.state.linked === this.state.sel && this.state.sel >= 0,
      savedAt: this.state.savedAt,
      dirty: this.state.savedSnap !== this.snapOf(this.state),
    }, this.nativeActions);
  }
  render() { return <Template v={this.renderVals()} />; }
}
