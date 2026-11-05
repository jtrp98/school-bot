import { useTranslation } from "react-i18next";
import { Toast } from "../../../components/shared/Toast";
import type { BlockToastState } from "./useBlockToast";

// Lower left of the workspace, beside the list when it is open, so it covers neither the list
// nor the controls on the right. The live region stays mounted so each new note is announced.
export function BlockToast({ toast, left }: { toast: BlockToastState; left: number }) {
  const { t } = useTranslation();
  const note = toast.toast;
  return (
    <div aria-live="polite" style={{ left: left + 16 }} className="absolute bottom-[16px] z-[50]">
      {note && (
        <Toast actionLabel={note.canUndo ? t("common.messages.undo") : undefined} onAction={toast.undo}>
          {note.text}
        </Toast>
      )}
    </div>
  );
}
