import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Button } from "../../../components/shared/Button";
import { outline } from "./categoryIcons";
import { TAB_ICONS } from "./DrawerTabs";
import { railCategory } from "./railCategories";
import { TerminalInput, type InputBlock } from "./TerminalInput";
import { TerminalLog } from "./TerminalLog";
import { UnavailableButton } from "./UnavailableButton";
import { FlatButton } from "./FlatButton";
import { serialFailureCode, type BoardSerial } from "./useBoardSerial";

const OutputIcon = TAB_ICONS.output;
const PlugIcon = outline("M9 2v6M15 2v6M6 8h12v4a6 6 0 0 1-12 0zM12 18v4");
const WarnIcon = outline(
  "M10.3 3.9L1.8 18a2 2 0 0 0 1.7 3h17a2 2 0 0 0 1.7-3L13.7 3.9a2 2 0 0 0-3.4 0zM12 9v4M12 17h.01",
);
const FollowIcon = outline("M12 17V3M6 11l6 6 6-6M19 21H5");
const TrashIcon = outline("M4 7h16M10 11v6M14 11v6M6 7l1 13h10l1-13M9 7V4h6v3");

const FAILURE_TEXT = {
  unreadable: { title: "blockLab.terminal.unreadable.title", body: "blockLab.terminal.unreadable.body" },
  robotPort: { title: "blockLab.terminal.robotPort.title", body: "blockLab.terminal.robotPort.body" },
};

// What the log area shows when it has no lines yet.
function EmptyPanel({ boardConnected, serial }: { boardConnected: boolean; serial: BoardSerial }) {
  const { t } = useTranslation();
  if (serial.status === "failed") {
    const text = FAILURE_TEXT[serial.failure];
    return (
      <div
        className="flex min-h-0 flex-1 items-center gap-[14px] bg-[var(--warn-tint)] px-[16px] text-[16px] leading-[1.6]"
      >
        <WarnIcon size={28} aria-hidden="true" className="shrink-0 text-[var(--warn)]" />
        <div className="min-w-0 flex-1">
          <div className="font-bold">
            {t(text.title)}{" "}
            <span className="text-[14px] font-medium text-muted">{serialFailureCode(serial.failure)}</span>
          </div>
          <div>{t(text.body)}</div>
        </div>
        <Button variant="secondary" size="md" onClick={() => void serial.open()}>
          {t("blockLab.terminal.unreadable.retry")}
        </Button>
      </div>
    );
  }
  if (boardConnected && serial.status !== "open") {
    const label = (
      <>
        <PlugIcon size={20} aria-hidden="true" />
        {t("blockLab.terminal.manual.open")}
      </>
    );
    return (
      <div className="flex min-h-0 flex-1 items-center gap-[16px] px-[20px] text-[16px] leading-[1.6]">
        {serial.paused ? (
          <UnavailableButton variant="blue" size="lg" reason={t("blockLab.terminal.waitSend")}>
            {label}
          </UnavailableButton>
        ) : (
          <Button variant="blue" size="lg" loading={serial.status === "opening"} onClick={() => void serial.open()}>
            {label}
          </Button>
        )}
        <div className="text-muted">
          <div>{t("blockLab.terminal.manual.idle")}</div>
          <div>{t("blockLab.terminal.manual.restart")}</div>
        </div>
      </div>
    );
  }
  return (
    <div className="flex min-h-0 flex-1 items-center gap-[14px] px-[20px] text-[16px] leading-[1.6]">
      <OutputIcon size={32} aria-hidden="true" className="shrink-0 text-muted" />
      <div>
        <div className="font-bold">{t("blockLab.terminal.emptyTitle")}</div>
        <div className="text-muted">
          {t("blockLab.terminal.emptyHint", { categoryName: t(railCategory("serial").labelKey) })}
        </div>
      </div>
    </div>
  );
}

// The Output tab as the design's terminal (TerminalA2): the chosen board's Serial, read once the
// child asks (useBoardSerial), with "follow" and "clear" at the top and a typing row below.
export function BoardTerminal({ boardConnected, serial }: { boardConnected: boolean; serial: BoardSerial }) {
  const { t } = useTranslation();
  const [follow, setFollow] = useState(true);
  const hasLines = serial.log.lines.length > 0 || serial.log.partial !== "";
  const failed = serial.status === "failed";
  const blocked: InputBlock = !boardConnected ? "noBoard" : serial.status === "open" ? null : "notReading";
  return (
    <div className="flex h-full flex-col bg-surface font-body text-ink">
      <div
        className="flex h-[48px] shrink-0 items-center gap-[8px] border-0 border-b border-solid border-line px-[12px]"
      >
        <span className="text-[16px] font-bold whitespace-nowrap">{t("blockLab.terminal.title")}</span>
        {boardConnected && (
          <span
            className={[
              "inline-flex h-[30px] items-center gap-[6px] rounded-full border-2 border-solid px-[10px] text-[14px]",
              "font-semibold whitespace-nowrap text-ink",
              failed ? "border-[var(--warn)] bg-[var(--warn-tint)]" : "border-ink bg-surface",
            ].join(" ")}
          >
            {failed ? <WarnIcon size={16} aria-hidden="true" /> : <PlugIcon size={16} aria-hidden="true" />}
            {t(failed ? "blockLab.terminal.badge.unreadable" : "blockLab.terminal.badge.uno")}
          </span>
        )}
        <div className="flex-1" />
        {boardConnected && hasLines && serial.status !== "open" && !serial.paused && (
          <FlatButton
            icon={<PlugIcon size={20} aria-hidden="true" />}
            loading={serial.status === "opening"}
            onClick={() => void serial.open()}
          >
            {t("blockLab.terminal.reopen")}
          </FlatButton>
        )}
        <FlatButton
          icon={<FollowIcon size={20} aria-hidden="true" />}
          tone={follow ? "following" : "paused"}
          aria-pressed={follow}
          onClick={() => setFollow(on => !on)}
        >
          {t(follow ? "blockLab.terminal.follow" : "blockLab.terminal.followOff")}
        </FlatButton>
        <FlatButton
          icon={<TrashIcon size={20} aria-hidden="true" />}
          aria-label={hasLines ? t("blockLab.terminal.clearLabel") : undefined}
          reason={hasLines ? undefined : t("blockLab.terminal.nothingYet")}
          onClick={serial.clear}
        >
          {t("blockLab.terminal.clear")}
        </FlatButton>
      </div>
      {hasLines ? (
        <TerminalLog log={serial.log} follow={follow} />
      ) : (
        <EmptyPanel boardConnected={boardConnected} serial={serial} />
      )}
      <TerminalInput blocked={blocked} onSend={serial.send} />
    </div>
  );
}
