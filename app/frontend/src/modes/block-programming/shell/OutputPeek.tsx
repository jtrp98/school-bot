import { useTranslation } from "react-i18next";
import type { TFunction } from "i18next";
import type { LogLine } from "./serialLog";
import { serialFailureCode, type BoardSerial } from "./useBoardSerial";

// The board's and the child's text stay as sent; the app's own lines follow the language.
function lineText(line: LogLine, t: TFunction): string {
  if (line.kind === "system") return t(line.key);
  if (line.kind === "error") return t(line.key, { message: line.message ?? "" });
  return line.text;
}

// With the drawer collapsed on Output, the tab row shows the latest line, or why there is none.
export function OutputPeek({ serial }: { serial: BoardSerial }) {
  const { t } = useTranslation();
  const { log, status, failure } = serial;
  const last = log.lines.at(-1);
  let text = t("blockLab.drawer.peek.notReading");
  if (last) {
    text = t("blockLab.drawer.peek.latest", { message: lineText(last, t), lineCount: log.lines.length });
  } else if (status === "open") {
    text = t("blockLab.terminal.emptyTitle");
  } else if (status === "failed") {
    text = t("blockLab.drawer.peek.unreadable", { errorCode: serialFailureCode(failure) });
  }
  return (
    <span className="min-w-0 truncate text-[16px] leading-[1.7] text-muted" title={text}>
      {text}
    </span>
  );
}
