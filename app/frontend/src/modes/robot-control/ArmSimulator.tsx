import { SimulatorView } from "./simulator/SimulatorView";
import { type Pose, REST, restPose, DEFAULT_MS, MAX_NAME, clampAngle, sleep } from "./simulator/simulatorData";

import { useEffect, useRef, useState } from "react";
import { client } from "../../core/client";
import { t as translate } from "../../i18n";

export function useArmSimulator() {
  const nextId = useRef(2);
  const [poses, setPoses] = useState<Pose[]>([
    {
      id: 1,
      name: translate("robot.simulator.messages.defaultPoseName", { number: 1 }),
      angles: [...REST],
      ms: DEFAULT_MS,
    },
  ]);

  const [sel, setSel] = useState(1);
  const [robotAt, setRobotAt] = useState<number[]>();
  const [status, setStatus] = useState(translate("robot.simulator.messages.statusInitial"));
  const [busy, setBusy] = useState(false);
  const [playing, setPlaying] = useState<number>();
  const [stopped, setStopped] = useState(false);
  const [looked, setLooked] = useState(false);
  const [loops, setLoops] = useState(1);
  const generation = useRef(0);
  const hold = useRef<{ delay?: number; tick?: number }>({});
  useEffect(
    () => () => {
      generation.current++;
      clearHold();
    },
    [],
  );

  const shown = sel === 0 ? restPose() : poses[sel - 1];
  const editable = sel > 0 && !busy;
  const atRobot =
    !!robotAt && robotAt.every((angle, index) => angle === shown.angles[index]);

  function patchSelected(patch: Partial<Pose>) {
    setPoses((list) =>
      list.map((pose, index) => (index === sel - 1 ? { ...pose, ...patch } : pose)),
    );
  }
  function setJoint(index: number, value: number) {
    if (sel === 0) return;
    setPoses((list) =>
      list.map((pose, i) =>
        i === sel - 1
          ? {
              ...pose,
              angles: pose.angles.map((angle, j) =>
                j === index ? clampAngle(value) : angle,
              ),
            }
          : pose,
      ),
    );
  }
  function jog(index: number, direction: 1 | -1) {
    setPoses((list) =>
      list.map((pose, i) =>
        i === sel - 1
          ? {
              ...pose,
              angles: pose.angles.map((angle, j) =>
                j === index ? clampAngle(angle + direction) : angle,
              ),
            }
          : pose,
      ),
    );
  }
  function clearHold() {
    window.clearTimeout(hold.current.delay);
    window.clearInterval(hold.current.tick);
    hold.current = {};
  }
  function holdStart(index: number, direction: 1 | -1) {
    clearHold();
    jog(index, direction);
    hold.current.delay = window.setTimeout(() => {
      hold.current.tick = window.setInterval(() => jog(index, direction), 70);
    }, 350);
  }

  function addPose() {
    const source = shown;
    const id = nextId.current++;
    setPoses((list) => [
      ...list,
      {
        id,
        name: translate("robot.simulator.messages.defaultPoseName", { number: list.length + 1 }),
        angles: [...source.angles],
        ms: source.ms,
      },
    ]);
    setSel(poses.length + 1);
    setStatus(translate("robot.simulator.messages.statusPoseAdded"));
  }
  function removePose() {
    if (sel === 0) return;
    setPoses((list) => list.filter((_, index) => index !== sel - 1));
    setSel(Math.max(0, sel - 1));
    setStatus(translate("robot.simulator.messages.statusPoseDeleted"));
  }
  function go(delta: number) {
    setSel((current) => Math.min(poses.length, Math.max(0, current + delta)));
  }

  async function sendAngles(angles: number[], run: number) {
    for (const [index, angle] of angles.entries()) {
      if (run !== generation.current) return false;
      await client.run("robot-control", "servo.set", {
        transport: "dry-run",
        servo: index + 1,
        angle,
      });
    }
    if (run !== generation.current) return false;
    setRobotAt([...angles]);
    return true;
  }
  async function sendSelected() {
    setStopped(false);
    setBusy(true);
    const run = ++generation.current;
    try {
      if (await sendAngles(shown.angles, run))
        setStatus(translate("robot.simulator.messages.statusSent"));
    } catch (error) {
      if (run === generation.current) setStatus(String(error));
    } finally {
      if (run === generation.current) setBusy(false);
    }
  }
  async function play() {
    if (poses.length === 0) {
      setStatus(translate("robot.simulator.messages.statusNothingToPlay"));
      return;
    }
    setStopped(false);
    setBusy(true);
    const run = ++generation.current;
    try {
      for (let round = 0; round < loops; round++) {
        for (const [index, pose] of poses.entries()) {
          if (run !== generation.current) return;
          setPlaying(index + 1);
          setSel(index + 1);
          if (!(await sendAngles(pose.angles, run))) return;
          await sleep(pose.ms);
        }
      }
      if (run === generation.current)
        setStatus(translate("robot.simulator.messages.statusPlayed", { poses: poses.length, loops }));
    } catch (error) {
      if (run === generation.current) setStatus(String(error));
    } finally {
      if (run === generation.current) {
        setBusy(false);
        setPlaying(undefined);
      }
    }
  }
  async function stop() {
    generation.current++;
    setBusy(false);
    setPlaying(undefined);
    setStopped(true);
    await client.cancel().catch(error => console.warn("Unable to cancel simulator request", error));
    setStatus(translate("robot.simulator.messages.statusStopped"));
  }
  async function goRest() {
    setSel(0);
    setStopped(false);
    setBusy(true);
    const run = ++generation.current;
    try {
      if (await sendAngles(REST, run)) setStatus(translate("robot.simulator.messages.statusAtRest"));
    } catch (error) {
      if (run === generation.current) setStatus(String(error));
    } finally {
      if (run === generation.current) setBusy(false);
    }
  }

  async function save() {
    try {
      await client.save("robot-control", {
        schema: 2,
        poses: poses.map(({ name, angles, ms }) => ({ name, angles, ms })),
      });
      setStatus(translate("robot.simulator.messages.statusSaved"));
    } catch (error) {
      setStatus(String(error));
    }
  }
  async function load() {
    try {
      const value = (await client.load("robot-control")) as {
        schema?: number;
        angles?: number[];
        poses?: { name?: unknown; angles?: unknown; ms?: unknown }[];
      } | null;
      if (!value) {
        setStatus(translate("robot.simulator.messages.statusNothingSaved"));
        return;
      }
      const validAngles = (angles: unknown): angles is number[] =>
        Array.isArray(angles) &&
        angles.length === 4 &&
        angles.every(
          (angle) => Number.isInteger(angle) && angle >= 0 && angle <= 180,
        );
      let loaded: Pose[];
      if (value.schema === 1 && validAngles(value.angles)) {
        loaded = [
          {
            id: 0,
            name: translate("robot.simulator.messages.defaultPoseName", { number: 1 }),
            angles: value.angles,
            ms: DEFAULT_MS,
          },
        ];
      } else if (
        value.schema === 2 &&
        Array.isArray(value.poses) &&
        value.poses.length <= 100
      ) {
        loaded = value.poses.map((pose, index) => {
          if (
            !validAngles(pose.angles) ||
            typeof pose.name !== "string" ||
            typeof pose.ms !== "number" ||
            !Number.isFinite(pose.ms)
          )
            throw new Error(translate("robot.simulator.messages.errorBadSaveFile"));
          return {
            id: 0,
            name: pose.name.slice(0, MAX_NAME)
              || translate("robot.simulator.messages.defaultPoseName", { number: index + 1 }),
            angles: pose.angles,
            ms: Math.min(5000, Math.max(100, Math.round(pose.ms))),
          };
        });
      } else throw new Error(translate("robot.simulator.messages.errorBadSaveFile"));
      setPoses(loaded.map((pose) => ({ ...pose, id: nextId.current++ })));
      setSel(loaded.length ? 1 : 0);
      setStatus(translate("robot.simulator.messages.statusOpened", { poses: loaded.length }));
    } catch (error) {
      setStatus(String(error));
    }
  }

  const a = shown.angles;
  const ready = looked && !busy;
  return {
    poses, sel, setSel, status, busy, playing, stopped, looked, setLooked, loops, setLoops,
    shown, editable, atRobot, patchSelected, setJoint, jog, clearHold, holdStart, addPose,
    removePose, go, sendSelected, play, stop, goRest, save, load, a, ready,
  };
}

export default function ArmSimulator() { return <SimulatorView v={useArmSimulator()} />; }
