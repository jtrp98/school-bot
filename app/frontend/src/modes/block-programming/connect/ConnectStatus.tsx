import type { AriaRole, ReactNode } from "react";
import { useTranslation } from "react-i18next";
import { Button } from "../../../components/shared/Button";
import { Icon, type IconName } from "../../../components/shared/Icons";
import { Modal } from "../../../components/shared/Modal";
import { boardName, linkName, lostBodyKey } from "./boardLabel";
import { clockText } from "./connectView";
import { NumberedTips } from "./NumberedTips";
import type { ConnectScreenState } from "./useConnectScreen";

type Tone = "ok" | "err" | "plain";

const FRAME_TONE: Record<Tone, string> = { ok: "border-ok", err: "border-err", plain: "border-line" };

// "stacked" holds a row plus something under it (tips or the disconnect question).
function StatusFrame({ tone, role, label, stacked, children }: {
  tone: Tone;
  role?: AriaRole;
  label?: string;
  stacked?: boolean;
  children: ReactNode;
}) {
  return (
    <section
      role={role}
      aria-label={label}
      className={[
        "flex rounded-[16px] border-2 border-solid bg-surface px-[20px] py-[16px]",
        stacked ? "flex-col gap-[12px]" : "items-center gap-[16px]",
        FRAME_TONE[tone],
      ].join(" ")}
    >
      {children}
    </section>
  );
}

export function StatusCircle({ tone, icon }: { tone: "ok" | "err"; icon: IconName }) {
  return (
    <span
      className={[
        "box-border flex h-[48px] w-[48px] shrink-0 items-center justify-center rounded-full border-2 border-solid",
        tone === "ok" ? "border-ok text-ok" : "border-err text-err",
      ].join(" ")}
    >
      <Icon name={icon} size={24} />
    </span>
  );
}

function StatusText({ title, body, small }: { title: ReactNode; body: ReactNode; small?: boolean }) {
  return (
    <div className="flex min-w-0 flex-1 flex-col">
      <span className={small ? "text-[18px] font-bold" : "text-[20px] leading-[1.3] font-bold"}>{title}</span>
      {body}
    </div>
  );
}

export function ConnectStatus({ screen }: { screen: ConnectScreenState }) {
  const { t } = useTranslation();
  const { link, names, failure } = screen.conn;
  const name = link ? linkName(link, names) : "";
  const lostTips = [
    t("blockLab.connect.cableTips.pushIn"),
    t(link?.followsAcrossPorts ? "blockLab.connect.cableTips.otherSocket" : "blockLab.connect.cableTips.newCard"),
    t("blockLab.connect.cableTips.light"),
  ];
  const lostBody = lostBodyKey(!!link?.followsAcrossPorts);
  const disconnected = !link && screen.phase === "idle" ? screen.disconnected : null;
  // A failed scan leaves the board unchecked; the error below stands in for "connected".
  const showConnected = link && !link.lost && !failure && !screen.cameBack && screen.phase === "idle";
  return (
    <>
      {link?.lost && (
        <StatusFrame tone="err" role="alert" stacked>
          <div className="flex items-center gap-[16px]">
            <StatusCircle tone="err" icon="unplug" />
            <StatusText
              title={t(screen.returned ? "blockLab.connect.lost.titleReturned" : "blockLab.connect.lost.title", {
                boardName: name,
              })}
              body={<span className="text-[16px]">{t(lostBody)}</span>}
            />
            <Button variant="secondary" size="md" aria-expanded={screen.lostTips} onClick={screen.toggleLostTips}>
              {t(screen.lostTips ? "blockLab.connect.tips.hide" : "blockLab.connect.tips.show")}
            </Button>
          </div>
          {screen.lostTips && <NumberedTips tips={lostTips} look="plain" />}
        </StatusFrame>
      )}
      {screen.cameBack && link && !link.lost && (
        <StatusFrame tone="ok" role="status">
          <StatusCircle tone="ok" icon="check" />
          <StatusText
            title={t("blockLab.connect.returned.title")}
            body={<span className="text-[16px]">{t("blockLab.connect.returned.body")}</span>}
          />
        </StatusFrame>
      )}
      {showConnected && (
        <StatusFrame tone="ok" label={t("blockLab.connect.connected.label")} stacked>
          <div className="flex items-center gap-[16px]">
            <StatusCircle tone="ok" icon="check" />
            <StatusText
              title={t("blockLab.connect.connected.title", { boardName: name, port: link.port })}
              body={
                <span className="text-[14px] text-muted">
                  {t("blockLab.connect.connected.since", { time: clockText(link.since) })}
                </span>
              }
            />
            {!screen.confirmingDisconnect && (
              <>
                <Button variant="secondary" size="md" onClick={screen.askDisconnect}>
                  {t("blockLab.connect.connected.disconnect")}
                </Button>
                <Button variant="blue" size="lg" iconRight="arrowR" onClick={screen.close}>
                  {t("blockLab.connect.connected.backToWork")}
                </Button>
              </>
            )}
          </div>
          {screen.confirmingDisconnect && (
            <Modal
              layout="inline"
              label={t("blockLab.connect.disconnect.label")}
              className={[
                "flex items-center gap-[16px] rounded-[12px] border-2 border-solid border-[var(--warn)]",
                "bg-[var(--warn-tint)] px-[16px] py-[12px]",
              ].join(" ")}
            >
              <Icon name="warn" size={24} className="text-[var(--warn)]" />
              <div className="flex flex-1 flex-col text-[16px]">
                <span className="font-bold">{t("blockLab.connect.disconnect.title")}</span>
                <span>{t("blockLab.connect.disconnect.body")}</span>
              </div>
              <Button variant="secondary" size="md" autoFocus onClick={screen.cancelDisconnect}>
                {t("common.cancel")}
              </Button>
              <Button variant="blue" size="md" onClick={screen.disconnectNow}>
                {t("blockLab.connect.disconnect.confirm")}
              </Button>
            </Modal>
          )}
        </StatusFrame>
      )}
      {disconnected && (
        <StatusFrame tone="plain" role="status">
          <Icon name="unplug" size={24} className="text-muted" />
          <StatusText
            small
            title={t("blockLab.connect.disconnected.title", {
              boardName: boardName({ ...disconnected, name: names[disconnected.key] }),
            })}
            body={<span className="text-[16px] text-muted">{t("blockLab.connect.disconnected.body")}</span>}
          />
        </StatusFrame>
      )}
      {failure && (
        <StatusFrame tone="err" role="alert">
          <Icon name="warn" size={32} className="text-err" />
          <StatusText
            small
            title={t("blockLab.connect.scanFailed.title")}
            body={
              <>
                <span className="text-[16px]">{t("blockLab.connect.scanFailed.body")}</span>
                <span className="font-mono text-[14px] break-words text-muted">{failure.detail}</span>
              </>
            }
          />
          <span
            className={[
              "flex shrink-0 flex-col items-center rounded-[8px] border-2 border-dashed border-err bg-surface",
              "px-[12px] py-[4px]",
            ].join(" ")}
          >
            <span className="text-[12px] text-muted">{t("blockLab.connect.scanFailed.codeLabel")}</span>
            <span className="font-mono text-[16px] font-bold">{failure.code}</span>
          </span>
          <Button variant="blue" size="md" onClick={screen.rescan}>
            {t("blockLab.connect.scanFailed.retry")}
          </Button>
        </StatusFrame>
      )}
    </>
  );
}
