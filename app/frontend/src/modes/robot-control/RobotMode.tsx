import { t as translate } from "../../i18n";
import { useEffect, useState } from "react";
import "./armlab-fonts";

import { param } from "../../core/debugParams";
import { useSampleData, useShell } from "../../shell";
import { IconBtn } from "../../components/shared";
import ArmSimulator from "./ArmSimulator";
import ArmWorkspace from "./workspace/ArmWorkspace";
import ConnectScreen from "./connect/ConnectScreen";
import NativeConnectScreen from "./connect/NativeConnectScreen";
import NativeReconnectOverlay from "./connect/NativeReconnectOverlay";
import ReconnectOverlay from "./connect/ReconnectOverlay";
import { RobotProvider, useRobot, type Screen } from "./robotContext";
import TeacherMenu from "./teacher/TeacherMenu";
import WorksScreen from "./works/WorksScreen";

const screens: Screen[] = ["connect", "works", "workspace", "teacher"];

function ArmLabScreens() {
  const { screen, conn, sample } = useRobot();
  const { setArmScreen } = useShell();
  useEffect(() => setArmScreen(screen), [screen, setArmScreen]);
  return (
    <div className={"arm-lab [flex:1] flex flex-col min-h-0 relative [&:has(>_.aw-host)]:min-h-[max(var(--app-viewport-height,_100vh),_780px)]"}>
      {screen === "connect" && (sample ? <ConnectScreen /> : <NativeConnectScreen />)}
      {screen === "works" && <WorksScreen />}
      {screen === "workspace" && <ArmWorkspace />}
      {screen === "teacher" && <TeacherMenu />}
      {conn === "lost" && screen === "workspace" && (sample ? <ReconnectOverlay /> : <NativeReconnectOverlay />)}
    </div>
  );
}

export default function RobotMode() {

  const sample = useSampleData();
  const [view, setView] = useState<"design" | "simulator">("design");
  const asked = param("screen") as Screen | undefined;

  const start: Screen = asked && screens.includes(asked) ? asked : "works";
  return (
    <div className={"arm-lab-root relative flex flex-col min-h-[calc(var(--app-viewport-height,_100vh)_-_0px)]"}>
      {view === "design" ? (
        <RobotProvider
          start={start}
          initialConn={!sample || start === "connect" ? "off" : "connected"}
        >
          <ArmLabScreens />
        </RobotProvider>
      ) : (
        <div className={"arm-lab-sim pt-[26px] px-[36px] pb-[90px]"}>
          <ArmSimulator />
        </div>
      )}
      {sample && (
      <div className={"arm-lab-switch fixed right-[16px] bottom-[16px] z-[60] shadow-card rounded-control"}>
        <IconBtn
          icon={view === "design" ? "terminal" : "arm"}
          label={
            view === "design"
              ? translate("robot.simulator.messages.openSimulatorCommandsEnabled")
              : translate("robot.simulator.messages.backToArmLab")
          }
          onClick={() => setView(view === "design" ? "simulator" : "design")}
        />
      </div>
      )}
    </div>
  );
}
