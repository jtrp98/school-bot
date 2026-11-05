import { useEffect, useRef, useState } from "react";
import { useTranslation } from "react-i18next";
import { Button } from "../../../components/shared/Button";
import { outline } from "./categoryIcons";
import { elapsedText, type LogLine, type SerialLog } from "./serialLog";

const ICONS = {
  board: outline("M17 7L7 17M17 17H7V7"),
  sent: outline("M7 7h10v10M7 17L17 7"),
  error: outline("M12 2a10 10 0 1 0 0 20 10 10 0 0 0 0-20zM12 8v4M12 16h.01"),
  system: outline("M12 2a10 10 0 1 0 0 20 10 10 0 0 0 0-20zM12 16v-4M12 8h.01"),
};
const InfoIcon = ICONS.system;
const DownIcon = outline("M12 5v14M6 13l6 6 6-6");

const ROW_TEXT: Record<LogLine["kind"], string> = {
  board: "font-normal text-muted",
  sent: "font-semibold text-ink",
  error: "font-bold text-err",
  system: "font-semibold text-ink",
};

const PREFIX_KEYS: Record<LogLine["kind"], string> = {
  board: "blockLab.terminal.prefix.board",
  sent: "blockLab.terminal.prefix.sent",
  error: "blockLab.terminal.prefix.error",
  system: "blockLab.terminal.prefix.system",
};

function LineText({ line }: { line: LogLine }) {
  const { t } = useTranslation();
  if ("text" in line) return <>{line.text}</>;
  if (line.kind === "system") return <>{t(line.key)}</>;
  return (
    <>
      {t(line.key, { message: line.message ?? "" })}
      <span className="ml-[8px] font-body text-[14px] font-medium text-muted">{line.code}</span>
    </>
  );
}

// The log in the design's terminal: board lines in grey, what the child sent and the app's notes
// in ink, errors in red, each with the time since the log began. Lines the child caused (sent,
// errors) always scroll into view; board lines only while "follow" is on, and otherwise count up
// on the "go to bottom" button.
export function TerminalLog({ log, follow }: { log: SerialLog; follow: boolean }) {
  const { t } = useTranslation();
  const box = useRef<HTMLDivElement>(null);
  const [unseen, setUnseen] = useState(0);
  const seen = useRef(0);
  const lastId = log.lines.at(-1)?.id ?? 0;
  const lastKind = log.lines.at(-1)?.kind;

  useEffect(() => {
    const added = lastId - seen.current;
    seen.current = lastId;
    if (added <= 0) return;
    if (follow || lastKind !== "board") {
      box.current?.scrollTo({ top: box.current.scrollHeight });
      setUnseen(0);
    } else {
      setUnseen(count => count + added);
    }
  }, [lastId, lastKind, follow, log.partial]);

  useEffect(() => {
    if (follow) box.current?.scrollTo({ top: box.current.scrollHeight });
  }, [follow, log.partial]);

  const toBottom = () => {
    box.current?.scrollTo({ top: box.current.scrollHeight });
    setUnseen(0);
  };

  const rows = log.partial
    ? [...log.lines, { id: -1, kind: "board" as const, time: Date.now(), text: log.partial }]
    : log.lines;
  return (
    <div className="relative min-h-0 flex-1">
      <div
        ref={box}
        role="log"
        aria-live="polite"
        aria-relevant="additions"
        aria-label={t("blockLab.terminal.logLabel")}
        tabIndex={0}
        className={[
          "box-border h-full overflow-x-hidden overflow-y-auto px-[12px] py-[4px] select-text",
          "focus-visible:outline-[3px] focus-visible:outline-offset-[-3px] focus-visible:outline-blue",
          "focus-visible:[outline-style:solid]",
        ].join(" ")}
      >
        {log.trimmed && (
          <div
            role="note"
            className={[
              "mb-[4px] flex min-h-[32px] items-center gap-[8px] rounded-[8px] border border-solid border-line",
              "bg-sunken px-[8px] text-[14px] leading-[1.7] text-muted",
            ].join(" ")}
          >
            <InfoIcon size={16} aria-hidden="true" />
            {t("blockLab.terminal.trimmed")}
          </div>
        )}
        {rows.map(line => {
          const LineIcon = ICONS[line.kind];
          return (
            <div key={line.id} className="flex min-h-[24px] items-start gap-[8px] text-[16px] leading-[24px]">
              <span className="w-[44px] shrink-0 text-[14px] leading-[24px] text-muted">
                {elapsedText(log, line.time)}
              </span>
              <LineIcon
                size={16}
                aria-hidden="true"
                className={`mt-[4px] shrink-0 ${line.kind === "board" ? "text-muted" : ROW_TEXT[line.kind]}`}
              />
              <span className={`min-w-0 flex-auto font-mono break-words whitespace-pre-wrap ${ROW_TEXT[line.kind]}`}>
                <span className="sr-only">{t(PREFIX_KEYS[line.kind])}</span>
                <LineText line={line} />
              </span>
            </div>
          );
        })}
      </div>
      {!follow && unseen > 0 && (
        <Button
          variant="secondary"
          size="sm"
          className="absolute right-[12px] bottom-[8px] shadow-pop"
          onClick={toBottom}
        >
          <DownIcon size={20} aria-hidden="true" />
          {t("blockLab.terminal.newLines", { lineCount: unseen })}
        </Button>
      )}
    </div>
  );
}
