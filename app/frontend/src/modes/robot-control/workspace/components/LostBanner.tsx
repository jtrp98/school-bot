import { t as translate } from "../../../../i18n";
import type { ReactNode } from "react";
import { T } from "../tplUtil";
import type { WorkspaceViewProps } from "../viewTypes";

type Props = WorkspaceViewProps<
  "notDisc" | "isDisc" | "nativeHardware" | "robotPort" | "nativeLost" | "reconnect" | "reconnectLabel"
>;

export function LostBanner({ v }: Props): ReactNode {
  return (
    <>
      {v.notDisc ? (
        <>

        </>
      ) : null}
      {v.isDisc ? (
        <>
        <header className={"h-[60px] shrink-0 box-border py-0 px-[24px] bg-err-ink text-white flex items-center gap-[16px]"} >
          <svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="var(--surface)" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
            <path d="M12 3 2 20h20z" />
            <path d="M12 10v4M12 17h.01" />
          </svg>
          <div className={"flex flex-col grow"} >
            <span className={"text-[18px] font-bold"} >
              {v.nativeHardware ? translate("robot.workspace.messages.deviceValue0IsNotReadyConnectBeforeSendingCommands", {value0: v.robotPort}) : translate("robot.workspace.messages.robotConnectionCom7LostScreenControlsCannotCommandOr")}
            </span>
            <span className={"text-[14px]"} >
              {v.nativeHardware && !v.nativeLost ? translate("robot.workspace.messages.selectADeviceAndConfirmItsProfileARobot") : translate("robot.workspace.messages.theRobotMayStillBeRunningDisconnectItsPower")}
            </span>
          </div>
          <button type="button" data-pop="reconnect" onClick={v.reconnect} className={"inline-flex items-center justify-center leading-[1.2] h-[50px] py-0 px-[22px] border-[medium] border-none border-current border-b-4 [border-bottom-style:solid] border-b-line rounded-[14px] bg-white text-err-ink font-[family-name:inherit] text-[16px] font-bold cursor-pointer"} >
            {T(v.reconnectLabel)}
          </button>
        </header>
        </>
      ) : null}
    </>
  );
}
