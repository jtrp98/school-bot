export type RobotDevice = { transport: "hid" | "serial"; path: string; name: string };
export type ServoCalibration = { id: number; pwmMin: number; pwmMax: number };
export type RobotProfile = { name: string; servos: ServoCalibration[] };
// Apply calibration because UI coordinates are not measured mechanical angles.
export const UI_RANGES = [[-180, 180], [-135, 135], [-135, 135], [-135, 135], [-135, 135], [0, 100]] as const;
/** A failure the UI shows from its code, so the text follows the selected language. `values` fill the sentence. */
export class AdapterError extends Error {
  readonly code: string;
  readonly values: Record<string, unknown>;

  constructor(code: string, values: Record<string, unknown> = {}) {
    super(code);
    this.code = code;
    this.values = values;
  }
}
export const nominalProfile = (name = "Default profile (needs calibration)"): RobotProfile => ({
  name,
  servos: UI_RANGES.map((_, id) => ({ id, pwmMin: 500, pwmMax: 2500 })),
});
const validPwm = (pwm: number) => Number.isInteger(pwm) && pwm >= 500 && pwm <= 2500;
const validServo = (servo: ServoCalibration, seen: Set<number>) =>
  !!servo
  && Number.isInteger(servo.id)
  && servo.id >= 0
  && servo.id <= 254
  && !seen.has(servo.id)
  && validPwm(servo.pwmMin)
  && validPwm(servo.pwmMax)
  && servo.pwmMin !== servo.pwmMax;
export function validateProfile(value: unknown): RobotProfile {
  const p = value as RobotProfile;
  const hasName = !!p && typeof p.name === "string" && !!p.name.trim();
  const hasShape = hasName && Array.isArray(p.servos) && p.servos.length === 6;
  if (!hasShape) throw new AdapterError("PROFILE_INVALID");
  const seen = new Set<number>();
  for (const s of p.servos) {
    if (!validServo(s, seen)) throw new AdapterError("SERVO_CALIBRATION_INVALID");
    seen.add(s.id);
  }
  return { name: p.name.trim(), servos: p.servos.map(s => ({ id: s.id, pwmMin: s.pwmMin, pwmMax: s.pwmMax })) };
}
export function servoItem(profile: RobotProfile, j: number, value: number, time: number) {
  profile = validateProfile(profile);
  const limits = UI_RANGES[j], servo = profile.servos[j];
  const valueInRange = !!limits && Number.isFinite(value) && value >= limits[0] && value <= limits[1];
  const timeInRange = Number.isInteger(time) && time >= 0 && time <= 9999;
  if (!servo || !valueInRange || !timeInRange) throw new AdapterError("JOINT_VALUE_OUT_OF_RANGE");
  return { id: servo.id, pwm: Math.round(servo.pwmMin + (value - limits[0]) / (limits[1] - limits[0]) * (servo.pwmMax - servo.pwmMin)), time_ms: time };
}
export const poseGroup = (profile: RobotProfile, values: number[], times: number[], index = 0) => {
  const complete = values.length === 6 && times.length === 6;
  const numeric = Array.from(values).every(Number.isFinite) && Array.from(times).every(Number.isInteger);
  const indexInRange = Number.isInteger(index) && index >= 0 && index <= 9999;
  if (!complete || !numeric || !indexInRange) throw new AdapterError("POSE_INVALID");
  return { index, items: values.map((v, j) => servoItem(profile, j, v, times[j])) };
};
export function parseServoPosition(profile: RobotProfile, j: number, reply: unknown): number {
  const servo = validateProfile(profile).servos[j], limits = UI_RANGES[j];
  const text = (reply as { exchange?: { received?: unknown } })?.exchange?.received;
  if (!servo || !limits || typeof text !== "string") throw new AdapterError("POSITION_UNAVAILABLE");
  const values = [...text.matchAll(/#(\d{1,3})P(?:RAD=)?(\d{3,4})!/g)].filter(m => Number(m[1]) === servo.id);
  if (!values.length) throw new AdapterError("POSITION_NOT_REPORTED", { id: servo.id });
  const pwm = Number(values[values.length - 1][2]);
  const outsideProfile = pwm < Math.min(servo.pwmMin, servo.pwmMax) || pwm > Math.max(servo.pwmMin, servo.pwmMax);
  if (outsideProfile) throw new AdapterError("POSITION_OUT_OF_PROFILE", { pwm, id: servo.id });
  return Math.round(limits[0] + (pwm - servo.pwmMin) / (servo.pwmMax - servo.pwmMin) * (limits[1] - limits[0]));
}
export const transportData = (device: RobotDevice) => device.transport === "hid" ? { transport: "hid", device: device.path } : { transport: "serial", port: device.path, baud: 115200 };
/** A bridge resolves only for a successful response; engine and host failures reject with a coded error. */
export type EngineReply = { data?: unknown };
export type Bridge = { run: (mode: "robot-control", action: string, data: unknown) => Promise<EngineReply>; cancel: () => Promise<void> };
export const errorCode = (error: unknown): string | undefined => {
  const code = (error as { code?: unknown } | null | undefined)?.code;
  return typeof code === "string" ? code : undefined;
};
// A busy or cancelled request changed nothing on the robot, so it is not a lost connection.
const isInterruption = (error: unknown) => ["ENGINE_BUSY", "ENGINE_CANCELLED"].includes(errorCode(error) ?? "");
const transportBusy = () => Object.assign(new Error("robot transport is busy"), { code: "ENGINE_BUSY" });
/** One request at a time. Stop drains the cancelled request before physical $DST!. */
export class ZideTransport {
  private active: Promise<unknown> | null = null;
  private stopTask: Promise<void> | null = null;
  private relockRequested = false;
  constructor(private bridge: Bridge, private sample: boolean, private onFailure: (error: unknown) => void) { }
  get idle() { return !this.active && !this.stopTask; }
  private async request(action: string, data: Record<string, unknown>) {
    if (this.sample) throw new AdapterError("SAMPLE_MODE");
    if (this.active) throw transportBusy();
    const task = this.bridge.run("robot-control", action, data).then(result => result.data);
    this.active = task;
    try { return await task; }
    finally { if (this.active === task) this.active = null; }
  }
  async discover(): Promise<RobotDevice[]> {
    if (this.stopTask) throw new AdapterError("ROBOT_STOPPING");
    const data = await this.request("device.list", {}) as { devices?: RobotDevice[] };
    return (data?.devices ?? []).filter(d => d && ["hid", "serial"].includes(d.transport) && typeof d.path === "string" && d.path.length > 0);
  }
  async probe(device: RobotDevice) {
    if (this.stopTask) throw new AdapterError("ROBOT_STOPPING");
    // Probe with board.info because it requires an actual controller response.
    const data = await this.request("board.info", { ...transportData(device), read_ms: 1000 }) as { simulated?: boolean };
    if (data?.simulated !== false) throw new AdapterError("DEVICE_NOT_CONFIRMED");
  }
  async command(device: RobotDevice, action: string, data: Record<string, unknown> = {}) {
    if (this.stopTask) throw new AdapterError("ROBOT_STOPPING");
    if (this.active) throw transportBusy();
    try { return await this.request(action, { ...data, ...transportData(device), read_ms: action === "servo.read" || action === "group.read" ? 1000 : 1 }); }
    catch (e) { if (!this.stopTask && !isInterruption(e)) this.onFailure(e); throw e; }
  }
  stop(device: RobotDevice, relock = false): Promise<void> {
    if (this.sample) return Promise.reject(new AdapterError("SAMPLE_MODE"));
    this.relockRequested ||= relock;
    if (this.stopTask) return this.stopTask;
    this.stopTask = (async () => {
      try {
        // Cancellation alone cannot stop motion or a sequence already running on the board.
        await this.bridge.cancel();
        // A failed request must finish draining before the physical stop is attempted too.
        await this.active?.catch(() => undefined);
        await this.request("robot.stop", { ...transportData(device), read_ms: 1 });
        if (this.relockRequested) await this.request("servo.torque", { ...transportData(device), enabled: true, read_ms: 1 });
      } catch (e) { this.onFailure(e); throw e; }
      finally { this.stopTask = null; this.relockRequested = false; }
    })();
    return this.stopTask;
  }
}
