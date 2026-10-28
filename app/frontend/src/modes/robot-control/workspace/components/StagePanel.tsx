import { t as translate } from "../../../../i18n";
import type { ReactNode } from "react";
import type { WorkspaceViewProps } from "../viewTypes";
import { ArmStage } from "./ArmStage";
import { PlaybackControls } from "./PlaybackControls";

type Props = WorkspaceViewProps;

export function StagePanel({ v }: Props): ReactNode {
  return (
    <>
      <aside data-stage6="1" aria-label={translate("common.messages.robotAndPlayback")} className={"absolute left-[24px] bottom-[16px] w-[560px] z-[20] flex flex-col gap-[10px]"} style={{ top: v.isDisc ? "132px" : "72px" }}>
        <ArmStage v={v} />
        <PlaybackControls v={v} />
      </aside>
    </>
  );
}
