import { t as translate } from "../../../i18n";
import { useEffect, useRef, useState } from "react";
import { useRobot } from "../robotContext";
import { nominalProfile, UI_RANGES, validateProfile, type RobotDevice } from "../engineAdapter";
import { engineErrorText } from "../engineErrorText";
import { NativeConnectView } from "./NativeConnectView";

export const SERVO_FIELDS = ["id", "pwmMin", "pwmMax"] as const;

export function useNativeConnectScreen() {
  const rb = useRobot();
  const [devices, setDevices] = useState<RobotDevice[]>([]);
  const [selected, setSelected] = useState("");
  const [profile, setProfile] = useState(
    () => rb.robot?.profile ?? nominalProfile(translate("robot.connect.profile.defaultName")),
  );
  const [confirmed, setConfirmed] = useState(false);
  const [name, setName] = useState(rb.robot?.name ?? "");
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState("");
  const mounted = useRef(true);
  const inputsLocked = busy || rb.conn === "connected";
  const renameProfile = (value: string) => {
    setProfile({ ...profile, name: value });
    setConfirmed(false);
  };
  const rangeText = (index: number) => translate("robot.connect.messages.displayRange", {
    min: UI_RANGES[index][0],
    max: UI_RANGES[index][1],
  });
  const updateServo = (index: number, field: (typeof SERVO_FIELDS)[number], value: number) => {
    setProfile({
      ...profile,
      servos: profile.servos.map((servo, i) => (i === index ? { ...servo, [field]: value } : servo)),
    });
    setConfirmed(false);
  };
  const scan = async () => {
    setBusy(true);
    setError("");
    try {
      const found = await rb.discover();
      if (mounted.current) {
        setDevices(found);
        setSelected(found[0] ? `${found[0].transport}:${found[0].path}` : "");
        setConfirmed(false);
      }
    } catch (e) {
      if (mounted.current) setError(engineErrorText(e));
    } finally {
      if (mounted.current) setBusy(false);
    }
  };
  useEffect(() => {
    mounted.current = true;
    void scan();
    return () => {
      mounted.current = false;
    };
  }, []);
  const connect = async () => {
    const device = devices.find(d => `${d.transport}:${d.path}` === selected);
    if (!device || busy || !confirmed) return;
    setBusy(true);
    setError("");
    try {
      const checkedProfile = validateProfile(profile);
      await rb.probe(device);
      if (!mounted.current) return;
      const robotName = name.trim() || device.name || translate("robot.connect.messages.robotArm");
      rb.connectTo({ name: robotName, port: device.path, device, profile: checkedProfile });
      rb.go(rb.from === "works" ? "works" : "workspace");
    } catch (e) { if (mounted.current) setError(engineErrorText(e)); }
    finally { if (mounted.current) setBusy(false); }
  };
  return {
    rb,
    devices,
    selected,
    setSelected,
    profile,
    confirmed,
    setConfirmed,
    name,
    setName,
    busy,
    error,
    setError,
    inputsLocked,
    renameProfile,
    rangeText,
    updateServo,
    scan,
    connect,
  };
}

export default function NativeConnectScreen() {
  return <NativeConnectView v={useNativeConnectScreen()} />;
}
