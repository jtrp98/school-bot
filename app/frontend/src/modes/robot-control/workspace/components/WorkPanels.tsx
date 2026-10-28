import type { ReactNode } from "react";
import { sx } from "../tplUtil";
import type { WorkspaceViewProps } from "../viewTypes";
import { JointPanel } from "./JointPanel";
import { PoseSequence } from "./PoseSequence";

type Props = WorkspaceViewProps;

export function WorkPanels({ v }: Props): ReactNode {
  return (
    <>
      <div onMouseEnter={v.sbEnter} onMouseLeave={v.sbLeave} className={"grow min-h-0 relative ml-[584px]"} >
        {v.sbShow ? (
          <>
          <div className="aw-sbthumb" onPointerDown={v.sbDown} aria-hidden="true" {...sx(v.sbStyle)} />
          </>
        ) : null}
        <div className={"aw-noscroll [scrollbar-width:none] [&::-webkit-scrollbar]:hidden [&::-webkit-scrollbar]:w-0 [&::-webkit-scrollbar]:h-0 h-full overflow-y-auto pb-[12px] box-border flex flex-col"} onScroll={v.sbScroll} onMouseEnter={v.sbScroll} >
          <div {...sx(v.main6Style)}>
            <JointPanel v={v} />
          </div>
          <PoseSequence v={v} />
        </div>
      </div>
    </>
  );
}
