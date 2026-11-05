import { useId } from "react";
import { useTranslation } from "react-i18next";
import { Button } from "../../../components/shared/Button";
import { Field } from "../../../components/shared/Field";
import { Icon } from "../../../components/shared/Icons";
import { TimedProgressBar } from "../../../components/shared/TimedProgressBar";
import { PROGRAM_BAUD_RATE } from "./boardDevices";
import { boardName, linkName } from "./boardLabel";
import { MAX_BOARD_NAME_LENGTH } from "./boardNames";
import { StatusCircle } from "./ConnectStatus";
import { OK_RETURN_MS, type ConnectScreenState } from "./useConnectScreen";

function NamingForm({ screen }: { screen: ConnectScreenState }) {
  const { t } = useTranslation();
  const inputId = useId();
  const empty = !screen.nameDraft.trim();
  return (
    <div className="flex items-center gap-[16px]">
      <div className="flex flex-col">
        <span className="text-[20px] leading-[1.3] font-bold text-ok">{t("blockLab.connect.naming.title")}</span>
        <span className="text-[14px] text-muted">{t("blockLab.connect.naming.body")}</span>
      </div>
      <div className="w-[240px]">
        <Field
          id={inputId}
          value={screen.nameDraft}
          maxLength={MAX_BOARD_NAME_LENGTH}
          autoFocus
          placeholder={t("blockLab.connect.naming.placeholder")}
          aria-label={t("blockLab.connect.naming.label")}
          onChange={event => screen.setNameDraft(event.target.value)}
          onKeyDown={event => {
            if (event.key === "Enter" && !event.nativeEvent.isComposing) screen.saveName();
          }}
        />
      </div>
      <Button variant="secondary" size="md" onClick={screen.skipName}>
        {t("blockLab.connect.naming.skip")}
      </Button>
      <Button variant="blue" size="md" aria-disabled={empty || undefined} onClick={screen.saveName}>
        {t("blockLab.connect.naming.save")}
      </Button>
    </div>
  );
}

function ConnectedNow({ screen }: { screen: ConnectScreenState }) {
  const { t } = useTranslation();
  const { link, names } = screen.conn;
  return (
    <>
      <StatusCircle tone="ok" icon="check" />
      <div role="status" className="flex min-w-[360px] flex-col gap-[4px]">
        <span className="text-[20px] leading-[1.3] font-bold text-ok">
          {t("blockLab.connect.ok.title", { boardName: link ? linkName(link, names) : "" })}
        </span>
        <span className="text-[14px] text-muted">
          {t("blockLab.connect.ok.body", { seconds: OK_RETURN_MS / 1000 })}
        </span>
        {/* The bar's length comes from OK_RETURN_MS, set inline by TimedProgressBar. */}
        <div
          className={[
            "h-[8px] overflow-hidden rounded-full bg-line [&_.cn-bar]:h-full [&_.cn-bar]:rounded-full",
            "[&_.cn-bar]:bg-ok [&_.cn-bar]:animate-[cnBar_linear_both] motion-reduce:[&_.cn-bar]:w-full",
            "motion-reduce:[&_.cn-bar]:animate-none",
          ].join(" ")}
        >
          <TimedProgressBar ms={OK_RETURN_MS} hold={false} pct={100} />
        </div>
      </div>
      <Button variant="blue" size="lg" iconRight="arrowR" className="ml-[16px]" onClick={screen.close}>
        {t("blockLab.connect.ok.start")}
      </Button>
    </>
  );
}

function ConnectButton({ screen }: { screen: ConnectScreenState }) {
  const { t } = useTranslation();
  const baudNoteId = useId();
  const { link, names } = screen.conn;
  const picked = screen.pickedBoard;
  const pickedName = picked ? boardName({ ...picked, name: names[picked.key] }) : "";
  let label = t("blockLab.connect.footer.pickFirst");
  if (picked && picked.key === screen.returned?.key) {
    label = t("blockLab.connect.footer.confirmBack", { boardName: pickedName, port: picked.port });
  } else if (picked && link && screen.switching) {
    label = t("blockLab.connect.footer.switch", {
      oldName: linkName(link, names),
      newName: pickedName,
      port: picked.port,
    });
  } else if (picked) {
    label = t("blockLab.connect.footer.connect", { boardName: pickedName, port: picked.port });
  }
  const baudNote = t(picked ? "blockLab.connect.baud.tip" : "blockLab.connect.baud.tipNone");
  return (
    <>
      {/* A disabled select cannot take focus, so the reason is tied to it for screen readers
          and also shown on hover. */}
      <label title={baudNote} className="flex flex-col gap-[4px]">
        <span className="inline-flex items-center gap-[4px] text-[14px] font-semibold text-muted">
          <Icon name="lock" size={16} />
          {t("blockLab.connect.baud.label")}
        </span>
        <select
          disabled
          value="baud"
          aria-label={t("blockLab.connect.baud.selectLabel")}
          aria-describedby={baudNoteId}
          onChange={() => undefined}
          className={[
            "box-border h-[48px] min-w-[192px] cursor-not-allowed rounded-[12px] border-2 border-solid border-line",
            "bg-sunken px-[12px] font-body text-[16px] text-muted",
          ].join(" ")}
        >
          <option value="baud">
            {picked
              ? t("blockLab.connect.baud.value", { baudRate: PROGRAM_BAUD_RATE })
              : t("blockLab.connect.baud.none")}
          </option>
        </select>
        <span id={baudNoteId} hidden>
          {baudNote}
        </span>
      </label>
      <Button variant="blue" size="lg" aria-disabled={!picked || undefined} onClick={screen.connect}>
        {label}
      </Button>
    </>
  );
}

export function ConnectFooter({ screen }: { screen: ConnectScreenState }) {
  const { t } = useTranslation();
  const { link } = screen.conn;
  let content = null;
  if (screen.phase === "naming") content = <NamingForm screen={screen} />;
  else if (screen.phase === "ok") content = <ConnectedNow screen={screen} />;
  else if (link?.lost && !screen.switching && !screen.returned) {
    content = (
      <span
        role="status"
        className={[
          "box-border inline-flex h-[56px] items-center gap-[12px] rounded-[12px] border-2 border-solid border-line",
          "bg-sunken px-[24px] text-[18px] font-bold text-muted",
        ].join(" ")}
      >
        <Icon name="loader" size={20} className="animate-[sbSpin_1s_linear_infinite] motion-reduce:animate-none" />
        {t("blockLab.connect.footer.waitingBack")}
      </span>
    );
  } else if (!link || screen.switching || screen.returned) content = <ConnectButton screen={screen} />;
  return (
    <footer className="box-border h-[96px] shrink-0 border-0 border-t-2 border-solid border-line bg-surface px-[48px]">
      <div className="mx-auto flex h-full max-w-[1200px] items-center gap-[16px]">
        <div className="flex-1" />
        {content}
      </div>
    </footer>
  );
}
