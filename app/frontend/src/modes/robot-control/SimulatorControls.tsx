import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Button } from "../../components/shared";
import { robotSimulator, simulatorEnabled } from "../../core/robotSimulator";
import { useRobot } from "./robotContext";

export function SimulatorControls() {
  const { t } = useTranslation();
  const rb = useRobot();
  const [status, setStatus] = useState("ready");
  const enter = () => {
    const url = new URL(location.href);
    url.searchParams.set("preview", "1");
    url.searchParams.set("simulator", "1");
    url.searchParams.set("screen", "connect");
    location.assign(url.href);
  };
  return <aside className="relative z-[70] flex flex-wrap items-center gap-[8px] p-[16px] bg-surface border-b" aria-label={t("robot.hardwareSimulator.title")}>
    {!simulatorEnabled ? <Button variant="secondary" onClick={enter}>{t("robot.hardwareSimulator.open")}</Button> : <>
      <strong>{t("robot.hardwareSimulator.title")}</strong>
      <span role="status">{t(`robot.hardwareSimulator.${status}`)}</span>
      <Button variant="danger" onClick={() => { void robotSimulator.emergencyStop(); rb.loseCable(); setStatus("halted"); }}>{t("robot.hardwareSimulator.stop")}</Button>
      <Button variant="secondary" onClick={() => { robotSimulator.fault = "disconnect"; void robotSimulator.emergencyStop(); rb.loseCable(); setStatus("disconnect"); }}>{t("robot.hardwareSimulator.disconnect")}</Button>
      <Button variant="secondary" onClick={() => { robotSimulator.fault = "timeout"; setStatus("timeout"); }}>{t("robot.hardwareSimulator.timeout")}</Button>
      <Button variant="secondary" onClick={() => { robotSimulator.fault = "none"; setStatus("restored"); }}>{t("robot.hardwareSimulator.restore")}</Button>
      <Button variant="secondary" disabled={robotSimulator.fault !== "none"} onClick={() => { robotSimulator.rearm(); setStatus("ready"); }}>{t("robot.hardwareSimulator.rearm")}</Button>
    </>}
  </aside>;
}
