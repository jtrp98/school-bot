import { useId, useState, type KeyboardEvent } from "react";
import { useTranslation } from "react-i18next";
import { Button } from "../../../components/shared/Button";
import { Icon } from "../../../components/shared/Icons";
import { outline } from "./categoryIcons";
import { Tip } from "./Tip";
import { usePopover } from "./usePopover";

const SendIcon = outline("M22 2L11 13M22 2l-7 20-4-9-9-4z");
// 60 characters stay inside the engine's 256-byte write even in Thai (3 bytes each) plus "\n".
const MAX_MESSAGE_CHARS = 60;
const HISTORY_SIZE = 20;

// Why the row cannot send: no board chosen, or the board's Serial is not being read.
export type InputBlock = "noBoard" | "notReading" | null;

const BLOCK_TEXT = {
  noBoard: {
    placeholder: "blockLab.terminal.placeholderOff",
    tip: "blockLab.terminal.sendUnavailable",
    explain: "blockLab.terminal.explainBody",
  },
  notReading: {
    placeholder: "blockLab.terminal.placeholderNotReading",
    tip: "blockLab.terminal.sendNotReading",
    explain: "blockLab.terminal.explainBodyNotReading",
  },
};

// Why "Send" cannot send now, or null when it can.
function sendTipKey(blocked: InputBlock, hasText: boolean) {
  if (blocked) return BLOCK_TEXT[blocked].tip;
  return hasText ? null : "blockLab.terminal.sendEmpty";
}

// The terminal's typing row: Enter or "Send" writes the line to the board (not while an input
// method is composing), ↑/↓ bring back what was sent. When it cannot send it stays focusable and
// explains why; a failed send keeps the text so the child can try again.
type TerminalInputProps = { blocked: InputBlock; onSend: (text: string) => Promise<boolean> };

export function TerminalInput({ blocked, onSend }: TerminalInputProps) {
  const { t } = useTranslation();
  const explain = usePopover();
  const helpId = useId();
  const [draft, setDraft] = useState("");
  const [history, setHistory] = useState<string[]>([]);
  const [recalled, setRecalled] = useState(-1);
  const [live, setLive] = useState("");
  const text = draft.trim();
  const ready = blocked === null && text !== "";
  const blockText = blocked ? BLOCK_TEXT[blocked] : null;
  const tipKey = sendTipKey(blocked, text !== "");
  const tip = tipKey ? t(tipKey) : "";

  const send = async () => {
    if (blocked) {
      explain.show();
      return;
    }
    if (!text) {
      setLive(t("blockLab.terminal.sendEmpty"));
      return;
    }
    if (!(await onSend(text))) return;
    setHistory(previous => [...previous, text].slice(-HISTORY_SIZE));
    setRecalled(-1);
    setDraft("");
    setLive(t("blockLab.terminal.sentLive"));
  };

  const recall = (direction: -1 | 1) => {
    if (!history.length || (direction === 1 && recalled < 0)) return;
    const start = recalled < 0 ? history.length : recalled;
    const next = start + direction;
    const index = next >= history.length ? -1 : Math.max(0, next);
    setRecalled(index);
    setDraft(index < 0 ? "" : history[index]);
  };

  const onKeyDown = (event: KeyboardEvent<HTMLInputElement>) => {
    if (event.key === "Enter" && !event.nativeEvent.isComposing) {
      event.preventDefault();
      void send();
    } else if (event.key === "ArrowUp" || event.key === "ArrowDown") {
      event.preventDefault();
      recall(event.key === "ArrowUp" ? -1 : 1);
    }
  };

  return (
    <div
      ref={explain.root}
      className={[
        "relative flex h-[45px] shrink-0 items-center border-0 border-t border-solid border-line",
        blocked ? "bg-sunken" : "bg-surface",
      ].join(" ")}
    >
      {explain.open && blockText && (
        <div
          className={[
            "absolute bottom-[52px] left-[12px] z-[30] box-border flex w-[440px] max-w-[calc(100%-24px)] flex-col",
            "gap-[8px] rounded-[16px] border border-solid border-line bg-surface p-[16px] text-left",
            "shadow-[0_8px_20px_rgba(20,36,61,.2)]",
            "motion-safe:animate-[block-lab-pop-in_.2s_cubic-bezier(.2,.8,.2,1)]",
          ].join(" ")}
        >
          <span className="text-[18px] leading-[1.6] font-bold">{t("blockLab.terminal.explainTitle")}</span>
          <span className="text-[16px] leading-[1.6]">{t(blockText.explain)}</span>
          <Button variant="secondary" size="sm" className="self-start" onClick={explain.close}>
            {t("blockLab.terminal.explainClose")}
          </Button>
        </div>
      )}
      <span
        aria-hidden="true"
        className={[
          "inline-flex h-[44px] w-[44px] shrink-0 items-center justify-center",
          blocked ? "text-muted" : "text-blue",
        ].join(" ")}
      >
        <Icon name="chevR" size={24} />
      </span>
      <input
        type="text"
        value={draft}
        maxLength={MAX_MESSAGE_CHARS}
        autoComplete="off"
        spellCheck={false}
        readOnly={blocked !== null}
        aria-disabled={blocked !== null || undefined}
        aria-label={t("blockLab.terminal.inputLabel")}
        aria-describedby={helpId}
        placeholder={t(blockText ? blockText.placeholder : "blockLab.terminal.placeholderReady")}
        onChange={event => {
          setDraft(event.target.value);
          setRecalled(-1);
          explain.close();
        }}
        onKeyDown={onKeyDown}
        className={[
          "box-border h-[44px] min-w-0 flex-1 border-0 bg-transparent px-[8px] font-mono text-[16px]",
          "leading-[24px] text-ink outline-none placeholder:text-muted placeholder:opacity-100 select-text",
          "focus-visible:shadow-[inset_0_0_0_2px_var(--blue),inset_0_0_0_5px_#D0EBFD]",
          blocked ? "cursor-not-allowed" : "",
        ].join(" ")}
      />
      <button
        ref={explain.trigger}
        type="button"
        aria-disabled={!ready || undefined}
        aria-label={ready ? t("blockLab.terminal.sendReady") : tip}
        onClick={() => void send()}
        className={[
          "group relative box-border inline-flex h-[44px] shrink-0 items-center gap-[8px] border-0",
          "px-[20px] text-[16px] leading-[1.6] font-bold",
          "focus-visible:outline-[3px] focus-visible:outline-offset-2 focus-visible:outline-blue",
          "focus-visible:[outline-style:solid]",
          ready
            ? "cursor-pointer bg-blue text-white"
            : "cursor-not-allowed border-l border-solid border-line bg-sunken text-muted",
        ].join(" ")}
      >
        <SendIcon size={20} aria-hidden="true" />
        {t("blockLab.terminal.send")}
        {tip && <Tip side="above-right">{tip}</Tip>}
      </button>
      <span id={helpId} hidden>
        {t(blockText ? blockText.explain : "blockLab.terminal.helpReady")}
      </span>
      <span role="status" className="sr-only">
        {explain.open && blockText ? t(blockText.tip) : live}
      </span>
    </div>
  );
}
