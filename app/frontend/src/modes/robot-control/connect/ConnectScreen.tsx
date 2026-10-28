import { ConnectView } from "./ConnectView";
import { sayText, idInfo } from "./connectPresentation";
import { t as translate } from "../../../i18n";

import { useSampleData } from "../../../shell";

import { MS } from "./connectData";

import { nameOf, portsOf } from "./connectState";
import { useConnectFlow } from "./useConnectFlow";

export function useConnectScreen() {
  const { s, hold, actions: a } = useConnectFlow();
  const sample = useSampleData();
  const P = portsOf(s);
  const n = P.robots.length;

  const busy =
    s.phase === "scanning" || s.phase === "connecting" || s.phase === "ok" || s.phase === "naming" || !!s.disc;
  const pickedRobot = P.robots.find((r) => r.port === s.picked) ?? null;
  const pickedOther = P.others.find((o) => o.port === s.picked) ?? null;
  const pickedName = pickedRobot
    ? `${nameOf(s, pickedRobot.key)} (${pickedRobot.port})`
    : pickedOther
      ? translate("robot.connect.messages.portLabel", { port: pickedOther.port })
      : "";
  const hasPick = !!(pickedRobot || pickedOther);

  const [sayT, sayS] = sayText(s, n);
  const connKey = s.connKey;
  const noRobots = n === 0 && s.id !== "unplug2";
  const id = idInfo(s);
  const showConnectBtn =
    (s.phase === "idle" || s.phase === "scanning") && !s.disc && (!connKey || !!s.switchTo);
  const hasError = !!s.err && s.phase === "idle";
  const showHelp = (noRobots || (!!s.err && s.tries < 2)) && !connKey;
  const discMs = s.disc === "folding" ? MS.folding : s.disc === "uploading" ? MS.uploading : MS.stopping;
  const idle = !!connKey && !s.disc;
  const footOk = s.phase === "ok" || s.phase === "naming";

  return {
    s, hold, a, sample, P, busy, pickedRobot, pickedOther, pickedName, hasPick, sayT, sayS,
    connKey, noRobots, id, showConnectBtn, hasError, showHelp, discMs, idle, footOk,
  };
}

export default function ConnectScreen() { return <ConnectView v={useConnectScreen()} />; }
