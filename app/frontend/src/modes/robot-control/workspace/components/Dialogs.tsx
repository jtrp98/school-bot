import type { ReactNode } from "react";
import type { WorkspaceViewProps } from "../viewTypes";
import { PopoverNotices } from "./PopoverNotices";
import { ProjectDialogs } from "./ProjectDialogs";
import { ReleaseDialogs } from "./ReleaseDialogs";
import { ToastBars } from "./ToastBars";

type Props = WorkspaceViewProps<"dialogs" | "dialogActions" | "projectDialogs" | "sameAskShow" | "sameAskNo">;

export function Dialogs({ v }: Props): ReactNode {
  return (
    <>
      <ToastBars v={v} />
      <ProjectDialogs v={v} />
      {v.sameAskShow ? (
        <div onClick={v.sameAskNo} aria-hidden="true" className="absolute inset-0 z-[35]" />
      ) : null}
      <ReleaseDialogs v={v} />
      <PopoverNotices v={v} />
    </>
  );
}
