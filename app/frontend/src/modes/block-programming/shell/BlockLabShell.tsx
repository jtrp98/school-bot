import { useCallback, useEffect, useRef, useState } from "react";
import { useTranslation } from "react-i18next";
import { hasNoBlocks } from "../blockly/toolbox";
import { ConnectUnoScreen } from "../connect/ConnectUnoScreen";
import { useBoardConnection } from "../connect/useBoardConnection";
import { registerWindowCloseGuard } from "../../../core/windowLifecycle";
import { dropQueuedBlockEngineCalls, leaveBlockEngine, openBlockEngine } from "./blockEngine";
import { BottomDrawer } from "./BottomDrawer";
import { CategoryRail } from "./CategoryRail";
import { BlockListPanel } from "./BlockListPanel";
import { BlockToast } from "./BlockToast";
import { DeleteAllDialog } from "./DeleteAllDialog";
import { DeleteSignalLayer } from "./DeleteSignalLayer";
import type { DrawerTab } from "./DrawerTabs";
import { FieldMenu } from "./FieldMenu";
import { PreflightDialog } from "./PreflightDialog";
import { PressRing } from "./PressRing";
import { ProblemRing } from "./ProblemRing";
import { DEFAULT_CATEGORY, type RailCategoryId } from "./railCategories";
import { SendCard } from "./SendCard";
import { SendGuardDialog } from "./SendGuardDialog";
import { StartHint } from "./StartHint";
import { UnsavedWorkDialog } from "./UnsavedWorkDialog";
import { useBlockList } from "./useBlockList";
import { useBlockProject } from "./useBlockProject";
import { useBlockToast } from "./useBlockToast";
import { useBlockWorkspace } from "./useBlockWorkspace";
import { useDeleteAllConfirm } from "./useDeleteAllConfirm";
import { useDropEffect } from "./useDropEffect";
import { useDropToDelete } from "./useDropToDelete";
import { useFieldMenu } from "./useFieldMenu";
import { useGeneratedCode } from "./useGeneratedCode";
import { useLongPressMenu } from "./useLongPressMenu";
import { usePreflight } from "./usePreflight";
import { useProblemRing } from "./useProblemRing";
import { useBoardSerial } from "./useBoardSerial";
import { useSendToBoard } from "./useSendToBoard";
import { useSettingsBoxTap } from "./useSettingsBoxTap";
import { useStartHint } from "./useStartHint";
import { WindowBar } from "./WindowBar";
import { WorkspaceControls } from "./WorkspaceControls";

// Block Lab is one screen; the board screen is drawn over it so the workspace keeps its state.
export default function BlockLabShell() {
  // Declared before the hooks below so their first engine calls find Block Lab open.
  useEffect(() => {
    openBlockEngine();
    const removeCloseGuard = registerWindowCloseGuard(dropQueuedBlockEngineCalls);
    return () => {
      removeCloseGuard();
      leaveBlockEngine();
    };
  }, []);
  const { t } = useTranslation();
  const [category, setCategory] = useState<RailCategoryId>(DEFAULT_CATEGORY);
  const [drawerOpen, setDrawerOpen] = useState(true);
  const [drawerTab, setDrawerTab] = useState<DrawerTab>("output");
  const [connectOpen, setConnectOpen] = useState(false);
  const changeBoardButton = useRef<HTMLButtonElement>(null);
  const board = useBoardConnection(connectOpen);
  const workspace = useBlockWorkspace();
  const toast = useBlockToast(workspace.workspace);
  const list = useBlockList(workspace.workspace, category, toast);
  const listInset = list.shown ? list.width : 0;
  const hint = useStartHint(workspace.workspace, listInset);
  useDropEffect(workspace.workspace);
  const fieldMenu = useFieldMenu(workspace.workspace);
  const code = useGeneratedCode(workspace.workspace, drawerOpen && drawerTab === "cpp");
  const deleteAll = useDeleteAllConfirm();
  const pressRing = useLongPressMenu(workspace.workspace);
  const dropDelete = useDropToDelete(workspace.workspace);
  useSettingsBoxTap(workspace.workspace);
  const preflight = usePreflight(workspace.workspace);
  const problemRing = useProblemRing(workspace.workspace, preflight.pointedAt, preflight.clearPointer);
  const serial = useBoardSerial(board.link);
  const send = useSendToBoard(workspace.workspace, board, preflight, serial);
  const project = useBlockProject(workspace.workspace, send.holdBack);
  const blocks = workspace.workspace;
  const { holdBack, close: closeSendCard } = send;
  const openConnect = useCallback(() => {
    if (holdBack("board")) return;
    blocks?.hideChaff();
    closeSendCard();
    setConnectOpen(true);
  }, [blocks, holdBack, closeSendCard]);
  const closeConnect = useCallback(() => {
    setConnectOpen(false);
    changeBoardButton.current?.focus();
  }, []);
  const boardConnected = board.link !== null && !board.link.lost;
  const pickCategory = (id: RailCategoryId) => {
    setCategory(id);
    list.show();
  };
  return (
    <div className="relative flex h-[var(--app-viewport-height,_100vh)] flex-col bg-bg">
      {project.confirmReason && <UnsavedWorkDialog project={project} reason={project.confirmReason} />}
      {send.guard && <SendGuardDialog reason={send.guard} onClose={send.closeGuard} />}
      <FieldMenu menu={fieldMenu} />
      {/* Everything above the drawer. The check before sending covers it and leaves the drawer in view;
          the send cards float at its bottom right, just above the drawer. */}
      <div className="relative flex min-h-0 flex-1 flex-col">
        <WindowBar
          project={project}
          board={board}
          connectOpen={connectOpen}
          changeButton={changeBoardButton}
          onChangeBoard={openConnect}
        />
        {/* The board screen covers the blocks instead of replacing them: Blockly keeps its size,
            blocks and undo history, and comes back exactly as it was. */}
        <div
          inert={connectOpen}
          className={[
            "flex min-h-0 flex-1 flex-col px-[24px] pt-[12px] pb-[12px]",
            connectOpen ? "invisible" : "",
          ].join(" ")}
        >
          <section
            aria-label={t("blockLab.workspace.label")}
            className={[
              "relative flex min-h-0 flex-1 rounded-[16px] bg-surface",
              // A dragged block floats over the window bar and the drawer; the panel stops clipping.
              dropDelete.lifted ? "z-[70] overflow-visible" : "overflow-hidden",
            ].join(" ")}
          >
            <CategoryRail selected={category} onSelect={pickCategory} isEmpty={hasNoBlocks} />
            <div className="relative min-w-0 flex-1">
              <div ref={workspace.container} className="absolute inset-0" />
              <StartHint place={hint} />
              <BlockListPanel
                list={list}
                category={category}
                empty={hasNoBlocks(category)}
                deleteZone={list.deleting && !dropDelete.newFromList}
                onPick={pickCategory}
              />
              <BlockToast toast={toast} left={listInset} />
              <WorkspaceControls controls={workspace.controls} />
              <PressRing at={pressRing} />
              <ProblemRing box={problemRing} />
            </div>
            {deleteAll.blockCount !== null && (
              <DeleteAllDialog blockCount={deleteAll.blockCount} onKeep={deleteAll.keep} onDelete={deleteAll.confirm} />
            )}
            {/* The border is a layer on top so it never shifts Blockly's drag coordinates. */}
            <div
              aria-hidden="true"
              className="pointer-events-none absolute inset-0 z-[20] rounded-[16px] border-2 border-solid border-line"
            />
          </section>
        </div>
        {preflight.state.status === "problems" && (
          <PreflightDialog problems={preflight.state.problems} onBack={preflight.close} onShow={preflight.show} />
        )}
        {!connectOpen && <SendCard send={send} onPickBoard={openConnect} />}
      </div>
      <div inert={connectOpen} className={["shrink-0 px-[24px] pb-[16px]", connectOpen ? "invisible" : ""].join(" ")}>
        <BottomDrawer
          boardConnected={boardConnected}
          serial={serial}
          open={drawerOpen}
          tab={drawerTab}
          code={code}
          checking={preflight.state.status === "checking"}
          sending={send.busy}
          onToggle={() => setDrawerOpen(open => !open)}
          onTab={setDrawerTab}
          onSend={() => void send.start()}
        />
      </div>
      {connectOpen && <ConnectUnoScreen conn={board} onClose={closeConnect} />}
      <DeleteSignalLayer signal={dropDelete.signal} />
    </div>
  );
}
