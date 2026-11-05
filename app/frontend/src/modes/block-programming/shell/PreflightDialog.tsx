import { useId, useRef } from "react";
import { useTranslation } from "react-i18next";
import type { TFunction } from "i18next";
import heroRobot from "../../../assets/hero-robot.webp";
import { Button } from "../../../components/shared/Button";
import { problemBlockId, type PreflightProblem } from "../blockly/preflight";
import { outline } from "./categoryIcons";
import { useBlockDialog } from "./useBlockDialog";

type Kind = PreflightProblem["kind"];

const WARN_PATH = "M10.3 3.9L1.8 18a2 2 0 0 0 1.7 3h17a2 2 0 0 0 1.7-3L13.7 3.9a2 2 0 0 0-3.4 0zM12 9v4M12 17h.01";
const WarnIcon = outline(WARN_PATH);

// The A2 icons per kind of problem, and the codes a child reads out to the teacher.
const KIND_ICONS = {
  emptySlot: outline("M12 5v14M5 12h14"),
  loose: outline(
    "M10 13a5 5 0 0 0 7.5.5l3-3a5 5 0 0 0-7-7l-1.7 1.7M14 11a5 5 0 0 0-7.5-.5l-3 3a5 5 0 0 0 7 7l1.7-1.7",
  ),
  pinShared: outline("M9 2v6M15 2v6M6 8h12v4a6 6 0 0 1-12 0zM12 18v4"),
  timer2: WarnIcon,
  engine: WarnIcon,
} satisfies Record<Kind, unknown>;
const KIND_CODES: Record<Kind, string> = {
  emptySlot: "E-CHK-01",
  loose: "E-CHK-02",
  pinShared: "E-CHK-03",
  timer2: "E-CHK-04",
  engine: "E-CHK-05",
};
const LockIcon = outline("M5 11h14v10H5zM8 11V7a4 4 0 0 1 8 0v4");
const ShowIcon = outline("M9 6l6 6-6 6");

const USE_KEYS: Record<string, string> = {
  input: "blockLab.preflight.use.input",
  output: "blockLab.preflight.use.output",
  tone: "blockLab.preflight.use.tone",
  servo: "blockLab.preflight.use.servo",
  neopixel: "blockLab.preflight.use.neopixel",
  ultrasonic: "blockLab.preflight.use.ultrasonic",
  i2c: "blockLab.preflight.use.i2c",
};

// The design shows three items at once (84px each) and scrolls the rest.
const VISIBLE_ITEMS = 3;

function problemText(t: TFunction, language: string, problem: PreflightProblem) {
  switch (problem.kind) {
    case "emptySlot":
    case "loose":
      return {
        title: t(`blockLab.preflight.${problem.kind}.title`, { blockName: problem.blockName }),
        fix: t(`blockLab.preflight.${problem.kind}.fix`),
      };
    case "pinShared": {
      // An unknown use is the engine's own name for it, shown as sent.
      const uses = problem.uses.map(use => (USE_KEYS[use] ? t(USE_KEYS[use]) : use));
      const list = new Intl.ListFormat(language, { type: "conjunction" }).format(uses);
      return {
        title: t("blockLab.preflight.pinShared.title", { pin: problem.pin, uses: list }),
        fix: t("blockLab.preflight.pinShared.fix"),
      };
    }
    case "timer2":
      return { title: t("blockLab.preflight.timer2.title"), fix: t("blockLab.preflight.timer2.fix") };
    case "engine":
      return { title: t("blockLab.preflight.engine.title"), fix: t("blockLab.preflight.engine.fix") };
  }
}

function ProblemRow({ problem, number, onShow }: {
  problem: PreflightProblem;
  number: number;
  onShow: (problem: PreflightProblem) => void;
}) {
  const { t, i18n } = useTranslation();
  const { title, fix } = problemText(t, i18n.language, problem);
  const Icon = KIND_ICONS[problem.kind];
  const code = KIND_CODES[problem.kind];
  // The engine's diagnostic stays as sent, for the teacher. A block edited away while the
  // engine answered leaves nothing to show, so that row is not a button.
  const detail = problem.kind === "engine" ? problem.detail : undefined;
  const showable = problemBlockId(problem) !== undefined;
  const body = (
    <>
      <span
        className={[
          "box-border inline-flex h-[28px] w-[28px] shrink-0 items-center justify-center rounded-[50%]",
          "border-2 border-solid border-[var(--warn)] bg-[var(--warn-tint)] text-[16px] leading-[1.2] font-bold",
        ].join(" ")}
      >
        {number}
      </span>
      <span
        className={[
          "box-border inline-flex h-[40px] w-[40px] shrink-0 items-center justify-center rounded-[12px]",
          "border-2 border-solid border-blue-line bg-bg",
        ].join(" ")}
      >
        <Icon size={20} aria-hidden="true" />
      </span>
      <span className="min-w-0 flex-1">
        <span className="block font-bold">{title}</span>
        <span className="block text-muted">{t("blockLab.preflight.fixLine", { fix })}</span>
        {detail && <span className="block font-mono text-[14px] break-words text-muted">{detail}</span>}
      </span>
      <span className="flex shrink-0 flex-col items-end gap-[2px] text-[14px] leading-[1.5]">
        <span className="text-muted">{code}</span>
        {showable && (
          <span className="inline-flex items-center gap-[2px] font-bold text-blue-ink">
            {t("blockLab.preflight.show")}
            <ShowIcon size={16} aria-hidden="true" />
          </span>
        )}
      </span>
    </>
  );
  const rowClass = [
    "box-border flex min-h-[76px] w-full items-center gap-[12px] rounded-[12px] border-2 border-b-4 border-solid",
    "border-line bg-surface px-[12px] py-[8px] text-left font-body text-[16px] leading-[1.6] text-ink",
  ].join(" ");
  if (!showable) return <li className={`mb-[8px] ${rowClass}`}>{body}</li>;
  return (
    <li className="mb-[8px]">
      <button
        type="button"
        aria-label={t("blockLab.preflight.itemLabel", { number, title, fix })}
        onClick={() => onShow(problem)}
        className={`${rowClass} cursor-pointer hover:bg-blue-tint`}
      >
        {body}
      </button>
    </li>
  );
}

// What to fix before sending: every problem at once, none can be skipped. A row closes the
// list and shows its block; Esc, the backdrop and "back" close it.
export function PreflightDialog({ problems, onBack, onShow }: {
  problems: PreflightProblem[];
  onBack: () => void;
  onShow: (problem: PreflightProblem) => void;
}) {
  const { t } = useTranslation();
  const titleId = useId();
  const back = useRef<HTMLButtonElement>(null);
  const focus = useBlockDialog(back, onBack);
  const count = problems.length;
  const more = count - VISIBLE_ITEMS;
  return (
    // Over the window bar (z 56) and the blocks, not the drawer, like the design.
    <div className="absolute inset-0 z-[60] flex items-center justify-center">
      <div aria-hidden="true" onClick={onBack} className="absolute inset-0 bg-[rgba(var(--ink-rgb),.35)]" />
      <div
        ref={focus.panel}
        role="alertdialog"
        aria-modal="true"
        aria-labelledby={titleId}
        tabIndex={-1}
        onKeyDown={focus.onKeyDown}
        className={[
          "relative box-border flex max-h-[calc(100%_-_32px)] w-[520px] max-w-[calc(100%_-_32px)] flex-col gap-[12px]",
          "rounded-[16px] bg-surface p-[24px] font-body text-ink shadow-pop outline-none",
        ].join(" ")}
      >
        <div className="flex items-center gap-[12px]">
          <span
            aria-hidden="true"
            className={[
              "box-border flex h-[40px] w-[40px] shrink-0 items-center justify-center rounded-[12px]",
              "border-2 border-solid border-[var(--warn)] bg-[var(--warn-tint)] text-[var(--warn)]",
            ].join(" ")}
          >
            <WarnIcon size={20} />
          </span>
          <h2 id={titleId} className="m-0 min-w-0 text-[24px] leading-[1.3] font-bold">
            {t("blockLab.preflight.title")}
          </h2>
        </div>
        <div className="flex items-center gap-[16px]">
          <img src={heroRobot} alt="" className="h-[88px] w-[88px] shrink-0 object-contain" />
          <div className="min-w-0 flex-1">
            <div role="status" className="text-[18px] leading-[1.6] font-bold">
              {t("blockLab.preflight.summary", { count })}
            </div>
            <div className="mt-[2px] text-[16px] leading-[1.6] text-muted">{t("blockLab.preflight.sub")}</div>
          </div>
        </div>
        <ul
          aria-label={t("blockLab.preflight.listLabel", { count })}
          className="m-0 max-h-[252px] min-h-0 list-none overflow-y-auto p-0 pr-[2px]"
        >
          {problems.map((problem, index) => (
            <ProblemRow key={index} problem={problem} number={index + 1} onShow={onShow} />
          ))}
        </ul>
        {more > 0 && (
          <div className="-mt-[8px] text-[14px] leading-[1.6] font-bold text-blue-ink">
            {t("blockLab.preflight.more", { count: more })}
          </div>
        )}
        <div className="flex items-center gap-[12px]">
          <LockIcon size={24} aria-hidden="true" className="shrink-0" />
          <div className="min-w-0 flex-1 text-[16px] leading-[1.6]">
            <b>{t("blockLab.preflight.locked")}</b>
            <br />
            <span className="text-muted">{t("blockLab.preflight.lockedNote")}</span>
          </div>
          <Button ref={back} variant="blue" size="md" icon="undo" onClick={onBack}>
            {t("blockLab.preflight.back")}
          </Button>
        </div>
      </div>
    </div>
  );
}
