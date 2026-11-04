import { AdapterError, type RobotDevice } from "../modes/robot-control/engineAdapter";

type Item = { id: number; pwm: number; time_ms: number };
type Group = { index: number; items: Item[] };
const integer = (v: unknown, min: number, max: number) => Number.isInteger(v) && Number(v) >= min && Number(v) <= max;
const check = (valid: boolean) => { if (!valid) throw new AdapterError("SAFETY_LIMIT"); };
export const simulatorEnabled = new URLSearchParams(location.search).get("simulator") === "1";

/** In-memory board only: this driver never imports or calls the native bridge. */
export class RobotSimulator {
  readonly devices: RobotDevice[] = [
    { transport: "hid", path: "sim://hid", name: "Simulator HID" },
    { transport: "serial", path: "sim://serial", name: "Simulator Serial" },
  ];
  private positions = new Map<number, number>();
  private groups = new Map<number, Group>();
  private torque = true;
  private halted = false;
  private generation = 0;
  fault: "none" | "timeout" | "disconnect" = "none";
  readonly history: string[] = [];
  private disconnected() { return this.fault === "disconnect"; }
  cancel = async () => { this.generation++; };
  rearm() {
    check(this.fault === "none");
    this.generation++;
    this.torque = true;
    this.halted = false;
  }
  emergencyStop = async () => {
    this.generation++;
    this.halted = true;
    this.torque = false;
    this.history.push("robot.stop", "servo.torque:off");
  };
  private group(value: unknown): Group {
    const g = value as Group;
    check(!!g && integer(g.index, 0, 9999) && Array.isArray(g.items) && g.items.length === 6);
    const ids = new Set<number>();
    for (const item of g.items) {
      check(!!item && integer(item.id, 0, 254) && !ids.has(item.id)
        && integer(item.pwm, 500, 2500) && integer(item.time_ms, 0, 9999));
      ids.add(item.id);
    }
    return structuredClone(g);
  }
  run = async (_mode: "robot-control", action: string, input: unknown) => {
    const data = input as Record<string, unknown>;
    check(!!data && typeof data === "object" && new TextEncoder().encode(JSON.stringify(data)).length <= 65536);
    if (action === "device.list") return { data: { devices: this.fault === "disconnect" ? [] : this.devices, simulated: true } };
    const device = this.devices.find(d => d.transport === data.transport && d.path === (data.device ?? data.port));
    if (!device || this.disconnected()) {
      await this.emergencyStop();
      throw new AdapterError("DEVICE_NOT_FOUND");
    }
    const token = this.generation;
    await new Promise(resolve => setTimeout(resolve, 15));
    if (token !== this.generation) throw new AdapterError("ENGINE_CANCELLED");
    if (this.disconnected()) {
      await this.emergencyStop();
      throw new AdapterError("DEVICE_NOT_FOUND");
    }
    if (this.fault === "timeout") {
      await this.emergencyStop();
      throw new AdapterError("DEVICE_TIMEOUT");
    }
    let received = "";
    let groups: Group[] = [];
    const motion = ["servo.set", "group.execute", "group.run"].includes(action);
    if (motion && (!this.torque || this.halted)) throw new AdapterError("TORQUE_OFF");
    switch (action) {
      case "board.info": received = "SIMULATOR"; break;
      case "servo.set":
        check(integer(data.id, 0, 254) && integer(data.pwm, 500, 2500) && integer(data.time_ms ?? 1000, 0, 9999));
        this.positions.set(Number(data.id), Number(data.pwm)); break;
      case "servo.read":
        check(integer(data.id, 0, 254));
        received = `#${data.id}PRAD=${this.positions.get(Number(data.id)) ?? 1500}!`; break;
      case "servo.torque":
        check(typeof data.enabled === "boolean");
        this.torque = data.enabled as boolean;
        this.halted = false; break;
      case "robot.stop": this.halted = false; break;
      case "group.execute":
      case "group.download": {
        const raw = data.group ? [data.group] : data.groups;
        check(Array.isArray(raw) && raw.length > 0 && raw.length <= (action === "group.execute" ? 1 : 4));
        groups = (raw as unknown[]).map(g => this.group(g));
        check(new Set(groups.map(g => g.index)).size === groups.length);
        for (const g of groups) {
          if (action === "group.download") this.groups.set(g.index, g);
          else for (const item of g.items) this.positions.set(item.id, item.pwm);
        }
        break;
      }
      case "group.run":
      case "group.read": {
        check(integer(data.start, 0, 9999) && integer(data.end, Number(data.start), 9999));
        if (action === "group.run") check(integer(data.times, 1, 9999));
        for (let i = Number(data.start); i <= Number(data.end); i++) {
          const g = this.groups.get(i);
          check(!!g);
          groups.push(g!);
        }
        if (action === "group.run") for (const g of groups) for (const item of g.items) this.positions.set(item.id, item.pwm);
        break;
      }
      default: throw new AdapterError("ACTION_UNKNOWN");
    }
    this.history.push(action);
    if (this.history.length > 1000) this.history.shift();
    return { data: { simulated: true, delivery: "simulator", stop_requested: action === "robot.stop",
      groups, exchange: { received }, torque_enabled: this.torque } };
  };
}
export const robotSimulator = new RobotSimulator();
