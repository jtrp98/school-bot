import { createContext, useCallback, useContext, useEffect, useMemo, useRef, useState, type ReactNode } from "react";
import { client } from "../../core/client";
import { SAMPLE_ROBOT_NAME, SAMPLE_ROBOT_NAME_SECOND, SAMPLE_WORK_NAME, useSampleData } from "../../shell";
import { registerWindowCloseGuard } from "../../core/windowLifecycle";
import { AdapterError, ZideTransport, validateProfile, type RobotDevice, type RobotProfile } from "./engineAdapter";
import { engineErrorText } from "./engineErrorText";

export type Screen = "connect" | "works" | "workspace" | "teacher";
export type Conn = "off" | "connecting" | "connected" | "lost";
export type Robot = { name: string; port: string; device?: RobotDevice; profile?: RobotProfile };
export type RobotApi = {
  screen: Screen; go: (screen: Screen) => void; from: Screen; conn: Conn; robot: Robot | null;
  sample: boolean; error: string | null; busy: () => boolean;
  setConn: (conn: Conn) => void; connectTo: (robot: Robot) => void;
  discover: () => Promise<RobotDevice[]>; probe: (device: RobotDevice) => Promise<void>;
  run: (action: string, data?: Record<string, unknown>) => Promise<unknown>;
  stop: (relock?: boolean) => Promise<void>; disconnect: () => void; loseCable: () => void;
  names: Record<string, string>; nameRobot: (port: string, name: string) => void;
  workName: string; setWorkName: (name: string) => void;
};
const Ctx = createContext<RobotApi | null>(null);
export function useRobot(): RobotApi {
  const value = useContext(Ctx);
  if (!value) throw new Error("useRobot must be used inside <RobotProvider>");
  return value;
}
export function RobotProvider({
  start = "workspace",
  initialConn = "off",
  children,
}: {
  start?: Screen;
  initialConn?: Conn;
  children: ReactNode;
}) {
  const sample = useSampleData();
  const [screen, setScreen] = useState<Screen>(start);
  const [from, setFrom] = useState<Screen>("workspace");
  const [conn, setConnection] = useState<Conn>(sample ? initialConn : "off");
  const connRef = useRef(conn); connRef.current = conn;
  const changeConnection = useCallback((next: Conn) => { connRef.current = next; setConnection(next); }, []);
  const [robot, setRobot] = useState<Robot | null>(
    sample && initialConn !== "off" ? { name: SAMPLE_ROBOT_NAME, port: "COM7" } : null,
  );
  const robotRef = useRef(robot); robotRef.current = robot;
  const verified = useRef("");
  const probeGeneration = useRef(0);
  const mounted = useRef(true);
  const [error, setError] = useState<string | null>(null);
  const failed = useRef<((error: unknown) => void)>(() => {});
  failed.current = e => { if (mounted.current) { setError(engineErrorText(e)); changeConnection("lost"); } };
  const transport = useMemo(() => new ZideTransport(client, sample, e => failed.current(e)), [sample]);
  const [names, setNames] = useState<Record<string, string>>(
    sample ? { COM7: SAMPLE_ROBOT_NAME, COM9: SAMPLE_ROBOT_NAME_SECOND } : {},
  );
  const [workName, setWorkName] = useState(sample ? SAMPLE_WORK_NAME : "");
  const go = useCallback((next: Screen) => { setScreen(current => { setFrom(current); return next; }); }, []);
  const setConn = useCallback((next: Conn) => {
    if (sample || next !== "connected") changeConnection(next);
  }, [sample, changeConnection]);
  const connectTo = useCallback((r: Robot) => {
    if (!sample) {
      if (!r.device || verified.current !== `${r.device.transport}:${r.device.path}` || !r.profile) return;
      r = { ...r, profile: validateProfile(r.profile) };
    }
    robotRef.current = r; setRobot(r); changeConnection("connected"); setError(null);
  }, [sample]);
  const loseCable = useCallback(() => changeConnection("lost"), [changeConnection]);
  const run = useCallback(async (action: string, data: Record<string, unknown> = {}) => {
    const target = robotRef.current;
    if (connRef.current !== "connected" || !target?.device || !target.profile) {
      throw new AdapterError("ROBOT_NOT_SELECTED");
    }
    return transport.command(target.device, action, data);
  }, [transport]);
  const discover = useCallback(() => transport.discover(), [transport]);
  const probe = useCallback(async (device: RobotDevice) => {
    const generation = ++probeGeneration.current;
    changeConnection("connecting"); setError(null); verified.current = "";
    try {
      await transport.probe(device);
      if (!mounted.current || generation !== probeGeneration.current) throw new AdapterError("CONNECTION_SUPERSEDED");
      verified.current = `${device.transport}:${device.path}`;
    }
    catch (e) { if (mounted.current) { changeConnection("lost"); setError(engineErrorText(e)); } throw e; }
  }, [transport]);
  const stop = useCallback(async (relock = false) => {
    if (sample) return;
    probeGeneration.current++;
    const device = robotRef.current?.device;
    if (!device) return;
    await transport.stop(device, relock);
  }, [sample, transport]);
  const disconnect = useCallback(() => {
    if (sample) { changeConnection("off"); return; }
    changeConnection("off");
    void stop()
      .then(() => {
        if (mounted.current) {
          setRobot(null);
          robotRef.current = null;
          verified.current = "";
        }
      })
      .catch(error => console.warn("Unable to stop while disconnecting", error));
  }, [sample, stop]);
  const nameRobot = useCallback((port: string, name: string) => setNames(n => ({ ...n, [port]: name })), []);
  useEffect(() => {
    if (sample) return;
    return registerWindowCloseGuard(async () => {
      changeConnection("off"); // Block new commands while shutdown drains.
      await stop();
    });
  }, [sample, stop, changeConnection]);
  useEffect(() => {
    mounted.current = true;
    if (sample) return;
    // A connection is a last verified target; requests open/close USB independently.
    const timer = window.setInterval(() => {
      const selected = robotRef.current?.device;
      if (connRef.current !== "connected" || !selected || !transport.idle) return;
      void transport.discover().then(devices => {
        const stillConnected = mounted.current
          && connRef.current === "connected"
          && robotRef.current?.device === selected;
        const present = devices.some(d => d.transport === selected.transport && d.path === selected.path);
        if (stillConnected && !present) failed.current(new AdapterError("DEVICE_NOT_FOUND"));
      }).catch(() => undefined); // A busy engine during discovery does not imply a cable failure.
    }, 3000);
    return () => {
      mounted.current = false;
      window.clearInterval(timer);
      void stop().catch(error => console.warn("Unable to stop during robot context cleanup", error));
    };
  }, [sample, transport, stop]);
  const value = useMemo<RobotApi>(() => ({
    screen, go, from, conn, robot, sample, error,
    busy: () => !transport.idle,
    setConn, connectTo, discover, probe, run, stop, disconnect, loseCable,
    names, nameRobot, workName, setWorkName,
  }), [
    screen, go, from, conn, robot, sample, error, transport,
    setConn, connectTo, discover, probe, run, stop, disconnect, loseCable,
    names, nameRobot, workName,
  ]);
  return <Ctx.Provider value={value}>{children}</Ctx.Provider>;
}
