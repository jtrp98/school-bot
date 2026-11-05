import { useId, useRef } from "react";
import { useTranslation } from "react-i18next";
import { Button } from "../../../components/shared/Button";
import { Modal } from "../../../components/shared/Modal";
import { useBlockDialog } from "./useBlockDialog";

// Asked by the workspace menu before every block goes. "Keep them" is the safe choice: it takes
// focus first and answers Esc. The deletion can still be undone, which the dialog says.
export function DeleteAllDialog({
  blockCount,
  onKeep,
  onDelete,
}: {
  blockCount: number;
  onKeep: () => void;
  onDelete: () => void;
}) {
  const { t } = useTranslation();
  const titleId = useId();
  const keep = useRef<HTMLButtonElement>(null);
  const focus = useBlockDialog(keep, onKeep);
  return (
    // Over the blocks panel only, as in the design: the drawer and window bar stay in view.
    <div ref={focus.panel} tabIndex={-1} onKeyDown={focus.onKeyDown} className="absolute inset-0 z-[70] outline-none">
      <Modal
        title={t("blockLab.deleteAll.title", { blockCount })}
        titleId={titleId}
        icon="warn"
        tone="warn"
        width={520}
        actions={
          <>
            <Button ref={keep} variant="secondary" size="md" onClick={onKeep}>
              {t("blockLab.deleteAll.keep")}
            </Button>
            <Button variant="danger" size="md" icon="trash" onClick={onDelete}>
              {t("blockLab.deleteAll.confirm")}
            </Button>
          </>
        }
      >
        <p className="m-0 font-body text-[16px] leading-[1.6]">{t("blockLab.deleteAll.body")}</p>
      </Modal>
    </div>
  );
}
