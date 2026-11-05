import { useId, useRef } from "react";
import { useTranslation } from "react-i18next";
import { Button } from "../../../components/shared/Button";
import { Modal } from "../../../components/shared/Modal";
import { useBlockDialog } from "./useBlockDialog";
import type { SendGuardReason } from "./useSendToBoard";

const BODY_KEYS: Record<SendGuardReason, string> = {
  new: "blockLab.send.guard.body.new",
  leave: "blockLab.send.guard.body.leave",
  close: "blockLab.send.guard.body.close",
  board: "blockLab.send.guard.body.board",
};

// Shown instead of starting over, leaving, closing the app or changing the board while a send
// runs. It offers no way to go ahead: that would cancel the upload halfway, so the child waits
// for the send card to finish. "OK" is the only choice and takes focus; Esc answers it too.
export function SendGuardDialog({ reason, onClose }: { reason: SendGuardReason; onClose: () => void }) {
  const { t } = useTranslation();
  const titleId = useId();
  const ok = useRef<HTMLButtonElement>(null);
  const focus = useBlockDialog(ok, onClose);
  return (
    // Above every layer of Block Lab, the send card included, like the unsaved-work dialog.
    <div
      ref={focus.panel}
      tabIndex={-1}
      onKeyDown={focus.onKeyDown}
      className="absolute inset-0 z-[70] outline-none"
    >
      <Modal
        title={t("blockLab.send.guard.title")}
        titleId={titleId}
        icon="warn"
        tone="warn"
        width={520}
        actions={
          <Button ref={ok} variant="blue" size="md" onClick={onClose}>
            {t("blockLab.send.guard.ok")}
          </Button>
        }
      >
        <p className="m-0 font-body text-[16px] leading-[1.6]">{t(BODY_KEYS[reason])}</p>
      </Modal>
    </div>
  );
}
