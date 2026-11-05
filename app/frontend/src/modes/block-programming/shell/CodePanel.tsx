import { useTranslation } from "react-i18next";
import { Icon } from "../../../components/shared/Icons";
import type { GeneratedCode } from "./useGeneratedCode";

const codeClass = "m-0 font-mono text-[14px] leading-[1.6] whitespace-pre text-ink";
const noteClass = "m-0 p-[12px] font-body text-[14px] text-muted";
// The design's code rows: 20px high, numbers 13px in a 28px column.
const rowsClass = "m-0 font-mono text-[14px] leading-[20px] whitespace-pre text-ink";

function ReadOnlyBadge() {
  const { t } = useTranslation();
  return (
    <span
      className={[
        "absolute top-[6px] right-[12px] box-border inline-flex h-[28px] items-center gap-[6px] rounded-[14px]",
        "border-2 border-solid border-line bg-sunken px-[10px] font-body text-[14px] font-semibold whitespace-nowrap",
        "text-muted",
      ].join(" ")}
    >
      <Icon name="lock" size={14} />
      {t("blockLab.drawer.readOnly")}
    </span>
  );
}

// The C++ tab is read-only: the code always follows the blocks. Line numbers sit in their own
// column, outside the copyable text.
export function CodePanel({ code }: { code: GeneratedCode }) {
  const { t } = useTranslation();
  if (code.status === "desktopOnly") return <p className={noteClass}>{t("blockLab.drawer.cppDesktopOnly")}</p>;
  if (code.status === "generating") {
    return <p role="status" className={noteClass}>{t("blockLab.drawer.cppGenerating")}</p>;
  }
  if (code.status === "failed")
    return (
      <div role="alert" className="flex flex-col gap-[8px] p-[12px]">
        <p className="m-0 font-body text-[14px] font-bold text-err">{t("blockLab.drawer.cppFailed")}</p>
        {/* The engine's reason, such as a pin used twice, is diagnostic text and stays as sent. */}
        <pre className={`${codeClass} text-muted`}>{code.detail}</pre>
      </div>
    );
  // A trailing newline would number a line the code block does not show.
  const text = code.code.replace(/\n+$/, "");
  const lines = text.split("\n");
  return (
    <div className="relative flex px-[12px] py-[4px]">
      <ReadOnlyBadge />
      <div aria-hidden="true" className="w-[28px] shrink-0 font-mono text-[13px] leading-[20px] text-muted select-none">
        {lines.map((_, index) => (
          <div key={index}>{index + 1}</div>
        ))}
      </div>
      <pre className={rowsClass}>
        {text}
      </pre>
    </div>
  );
}
