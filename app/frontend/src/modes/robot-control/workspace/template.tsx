import type { ReactNode } from "react";
import type { WorkspaceViewProps } from "./viewTypes";
import { Overlays } from "./components/Overlays";
import { TopBar } from "./components/TopBar";
import { LostBanner } from "./components/LostBanner";
import { StagePanel } from "./components/StagePanel";
import { WorkPanels } from "./components/WorkPanels";
import { Dialogs } from "./components/Dialogs";
import { LogDrawer } from "./components/LogDrawer";

export function Template({ v }: WorkspaceViewProps): ReactNode {
  return (
    <div
      data-root5a="1"
      onClick={v.rootClick}
      ref={v.dom.setRoot}
      className="relative box-border flex w-full select-none flex-col overflow-hidden text-[var(--ink)]"
      style={{
        height: "max(var(--app-viewport-height, 100vh), 780px)",
        background: v.pageBg,
        fontFamily: "'Noto Sans Thai', 'Leelawadee UI', sans-serif",
        transition: "background 0.3s",
      }}
    >
      <Overlays v={v} />
      <TopBar v={v} />
      <LostBanner v={v} />
      <StagePanel v={v} />
      <WorkPanels v={v} />
      <Dialogs v={v} />
      <LogDrawer v={v} />
    </div>
  );
}
