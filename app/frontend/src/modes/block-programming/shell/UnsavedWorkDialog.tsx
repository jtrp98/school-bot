import type { KeyboardEvent } from "react";
import { useTranslation } from "react-i18next";
import { useCloseDialog } from "../../../app/useWindowClose";
import { Button } from "../../../components/shared/Button";
import { Modal } from "../../../components/shared/Modal";
import type { BlockProject, UnsavedReason } from "./useBlockProject";

const BODY_KEYS: Record<UnsavedReason, string> = {
  new: "blockLab.project.confirm.body.new",
  leave: "blockLab.project.confirm.body.leave",
  close: "blockLab.project.confirm.body.close",
};

// Asked before starting over, leaving Block Lab or closing the app would drop blocks that were
// never saved. The safe choice (keep working) is first, takes focus and answers Esc; Tab stays
// inside, and focus goes back where it was when the dialog closes, as in the app's close dialog.
export function UnsavedWorkDialog({ project, reason }: { project: BlockProject; reason: UnsavedReason }) {
  const { t } = useTranslation();
  const focus = useCloseDialog(false);
  const title = project.name
    ? t("blockLab.project.confirm.titleNamed", { jobName: project.name })
    : t("blockLab.project.confirm.title");
  const onKeyDown = (event: KeyboardEvent<HTMLDivElement>) => {
    if (event.key !== "Escape") {
      focus.onKeyDown(event);
      return;
    }
    event.preventDefault();
    event.stopPropagation();
    project.keepWorking();
  };
  return (
    // Above every workspace layer, the toast and field menu included.
    <div
      ref={focus.panel}
      tabIndex={-1}
      onKeyDown={onKeyDown}
      className="absolute inset-0 z-[70] flex items-center justify-center bg-[rgba(20,36,61,.35)] outline-none"
    >
      <Modal
        title={title}
        icon="warn"
        tone="warn"
        width={520}
        actions={
          <>
            <Button variant="secondary" size="md" onClick={project.keepWorking}>
              {t("blockLab.project.confirm.keep")}
            </Button>
            <Button variant="secondary" size="md" onClick={project.discardAndContinue}>
              {t("blockLab.project.confirm.discard")}
            </Button>
            <Button variant="blue" size="md" icon="save" onClick={() => void project.saveAndContinue()}>
              {t("blockLab.menu.save")}
            </Button>
          </>
        }
      >
        <p className="m-0 font-body text-[16px] leading-[1.6]">{t(BODY_KEYS[reason])}</p>
        {project.saveState === "failed" && (
          <p role="alert" className="m-0 font-body text-[16px] leading-[1.6] font-semibold text-err">
            {t("blockLab.project.saveFailed")}
          </p>
        )}
      </Modal>
    </div>
  );
}
