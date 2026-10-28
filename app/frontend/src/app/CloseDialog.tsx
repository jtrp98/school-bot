import { Button, Modal } from "../components/shared";
import { t } from "../i18n";
import { useCloseDialog } from "./useWindowClose";
import type { useWindowClose } from "./useWindowClose";

type Props = ReturnType<typeof useWindowClose>;

export function CloseDialog(props: Props) {
  const { panel, onKeyDown } = useCloseDialog(props.closeBusy);

  return (
    <div
      ref={panel}
      tabIndex={-1}
      onKeyDown={onKeyDown}
    >
      <Modal
        fixed icon="warn" tone="warn" width={620}
        title={t("modal.close.title")} titleId="close-app-title"
        actions={
          <div className="flex flex-wrap justify-end gap-[16px] [&_.sb-btn]:whitespace-normal">
            <Button variant="secondary" size="md" disabled={props.closeBusy}
              onClick={() => props.setCloseRequested(false)}>
              {t("entry.close.cancel")}
            </Button>
            <Button variant="secondary" size="md" disabled={props.closeBusy || !props.closeSaveAvailable}
              onClick={() => void props.closeWindow(true)}>
              {t("entry.close.save")}
            </Button>
            <Button variant="blue" size="md" disabled={props.closeBusy} loading={props.closeBusy}
              onClick={() => void props.closeWindow(props.closeError ? props.closeSaveRequested : false)}>
              {t(props.closeError ? "entry.close.retry" : "entry.close.withoutSave")}
            </Button>
          </div>
        }
      >
        <p className="m-0 font-body text-[16px] leading-[32px] text-muted">{t("modal.close.description")}</p>
        {!props.closeSaveAvailable && <p className="m-0 leading-[32px]">{t("entry.close.saveUnavailable")}</p>}
        {props.closeBusy && <p role="status" aria-live="polite">{t(`entry.close.phases.${props.closePhase}`)}</p>}
        {props.closeError && (
          <p role="alert" className="m-0 leading-[32px] text-err-ink">
            {t(props.closeError)}
            {props.closeErrorReason && (
              <span className="block whitespace-pre-wrap [overflow-wrap:anywhere]">{props.closeErrorReason}</span>
            )}
          </p>
        )}
      </Modal>
    </div>
  );
}
