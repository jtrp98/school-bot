import { useTranslation } from "react-i18next";
import { MAX_SCALE, MIN_SCALE } from "../blockly/zoom";
import { outline } from "./categoryIcons";
import { ToolButton } from "./ToolButton";
import type { WorkspaceControlsState } from "./useBlockWorkspace";

const UndoIcon = outline("M3 12a9 9 0 1 0 3-6.7M3 3v6h6");
const RedoIcon = outline("M21 12a9 9 0 1 1-3-6.7M21 3v6h-6");
const ZoomOutIcon = outline("M5 12h14");
const ZoomInIcon = outline("M12 5v14M5 12h14");
const FitIcon = outline("M4 9V4h5M20 9V4h-5M4 15v5h5M20 15v5h-5");

// Blockly's own undo and redo keys, named the way each system writes them.
const isMac = navigator.userAgent.includes("Mac");
const UNDO_KEYS = isMac ? "Cmd+Z" : "Ctrl+Z";
const REDO_KEYS = isMac ? "Cmd+Shift+Z" : "Ctrl+Y";

const Separator = () => <span aria-hidden="true" className="mx-[4px] my-[2px] block h-px bg-line" />;

// Undo, redo, zoom and fit in one frame in the workspace's lower right, with the zoom level
// flashing above it after each zoom.
export function WorkspaceControls({ controls }: { controls: WorkspaceControlsState }) {
  const { t } = useTranslation();
  const zoomNote = controls.zoomNote === null ? "" : t("blockLab.workspace.zoomLevel", { percent: controls.zoomNote });
  const percent = controls.zoomPercent;
  const undoTip = controls.canUndo
    ? t("blockLab.workspace.undoTip", { shortcut: UNDO_KEYS })
    : t("blockLab.workspace.undoUnavailable");
  const redoTip = controls.canRedo
    ? t("blockLab.workspace.redoTip", { shortcut: REDO_KEYS })
    : t("blockLab.workspace.redoUnavailable");
  const zoomOutTip = controls.canZoomOut
    ? t("blockLab.workspace.zoomOutTip", { percent })
    : t("blockLab.workspace.zoomOutUnavailable", { percent: MIN_SCALE * 100 });
  const zoomInTip = controls.canZoomIn
    ? t("blockLab.workspace.zoomInTip", { percent })
    : t("blockLab.workspace.zoomInUnavailable", { percent: MAX_SCALE * 100 });
  return (
    <div className="absolute right-[16px] bottom-[16px] z-[10] flex flex-col items-end gap-[16px]">
      {zoomNote && (
        <span
          aria-hidden="true"
          className={[
            "pointer-events-none inline-flex h-[36px] animate-[block-lab-pop-in_.12s_ease-out] items-center",
            "rounded-[8px] bg-ink px-[12px] font-body text-[16px] leading-[1.6] font-bold whitespace-nowrap",
            "text-white shadow-[0_8px_20px_rgba(20,36,61,.2)] motion-reduce:animate-none",
          ].join(" ")}
        >
          {zoomNote}
        </span>
      )}
      <div
        role="group"
        aria-label={t("blockLab.workspace.controls")}
        className={[
          "box-border flex w-[56px] flex-col gap-[4px] rounded-[16px] border-2 border-solid border-line bg-surface",
          "p-[4px] shadow-[0_8px_20px_rgba(20,36,61,.2)]",
        ].join(" ")}
      >
        <ToolButton
          label={controls.canUndo ? t("common.messages.undo") : undoTip}
          tip={undoTip}
          tipSide="left"
          unavailable={!controls.canUndo}
          onClick={controls.undo}
        >
          <UndoIcon size={24} aria-hidden="true" />
        </ToolButton>
        <ToolButton
          label={controls.canRedo ? t("blockLab.workspace.redo") : redoTip}
          tip={redoTip}
          tipSide="left"
          unavailable={!controls.canRedo}
          onClick={controls.redo}
        >
          <RedoIcon size={24} aria-hidden="true" />
        </ToolButton>
        <Separator />
        <ToolButton
          label={controls.canZoomOut ? t("blockLab.workspace.zoomOut") : zoomOutTip}
          tip={zoomOutTip}
          tipSide="left"
          unavailable={!controls.canZoomOut}
          onClick={controls.zoomOut}
        >
          <ZoomOutIcon size={24} aria-hidden="true" />
        </ToolButton>
        <ToolButton
          label={controls.canZoomIn ? t("blockLab.workspace.zoomIn") : zoomInTip}
          tip={zoomInTip}
          tipSide="left"
          unavailable={!controls.canZoomIn}
          onClick={controls.zoomIn}
        >
          <ZoomInIcon size={24} aria-hidden="true" />
        </ToolButton>
        <ToolButton
          label={t("blockLab.workspace.fit")}
          tip={t("blockLab.workspace.fitTip")}
          tipSide="left"
          onClick={controls.fit}
        >
          <FitIcon size={24} aria-hidden="true" />
        </ToolButton>
      </div>
      <span role="status" className="sr-only">{zoomNote}</span>
    </div>
  );
}
