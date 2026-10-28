import { t as translate } from "../../../../i18n";
import type { ReactNode } from "react";
import type { WorkspaceViewProps } from "../viewTypes";
import { SequenceToolbar } from "./SequenceToolbar";
import { PoseCards } from "./PoseCards";
import { PoseTable } from "./PoseTable";

type Props = WorkspaceViewProps;

export function PoseSequence({ v }: Props): ReactNode {
  const fill = v.isTable ? "flex-[1_1_0] min-h-[200px]" : "shrink-0";
  return (
    <section
      aria-label={translate("robot.workspace.sequence.title")}
      className={`relative mt-[12px] mx-[24px] mb-0 bg-surface border-2 border-solid border-line rounded-[20px]
        pt-[10px] px-[16px] pb-[12px] flex flex-col gap-[8px] ${fill}`}
    >
      <SequenceToolbar v={v} />
      <div className={"flex [align-items:stretch] gap-[14px] [flex:1_1_auto] min-h-0"} >
        <div className={"relative grow min-w-0 min-h-0 flex flex-col"} >
          {v.isCards ? <PoseCards v={v} /> : null}
          {v.isTable ? <PoseTable v={v} /> : null}
        </div>
      </div>
    </section>
  );
}
