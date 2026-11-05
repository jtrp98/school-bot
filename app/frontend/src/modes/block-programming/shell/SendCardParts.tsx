import type { ReactNode } from "react";
import { useTranslation } from "react-i18next";
import { Icon } from "../../../components/shared/Icons";
import type { SendStep } from "./sendOutcome";

const STEP_KEYS: Record<SendStep, string> = {
  compile: "blockLab.send.steps.compile",
  upload: "blockLab.send.steps.upload",
};

type SegmentState = "done" | "active" | "waiting";

function StepSegment({ state }: { state: SegmentState }) {
  return (
    <div
      className={[
        "relative box-border h-[14px] flex-1 overflow-hidden rounded-[7px] border-2 border-solid border-line-strong",
        "bg-blue-tint",
      ].join(" ")}
    >
      {state === "done" && <div className="absolute inset-0 bg-blue" />}
      {state === "active" && (
        <div
          className={[
            "absolute inset-y-0 left-0 w-2/5 bg-blue",
            "motion-safe:animate-[block-lab-step-slide_1.4s_ease-in-out_infinite] motion-reduce:w-full",
            "motion-reduce:opacity-50",
          ].join(" ")}
        />
      )}
    </div>
  );
}

// Two steps, compile then upload. The tools report no progress within a step, so the running
// step moves without a number instead of showing a percentage that measures nothing.
export function StepBar({ step }: { step: SendStep }) {
  const { t } = useTranslation();
  const label = t(STEP_KEYS[step]);
  const compiling = step === "compile";
  return (
    <div>
      <div
        role="progressbar"
        aria-label={t("blockLab.send.progressLabel")}
        aria-valuemin={1}
        aria-valuemax={2}
        aria-valuenow={compiling ? 1 : 2}
        aria-valuetext={label}
        className="flex gap-[6px]"
      >
        <StepSegment state={compiling ? "active" : "done"} />
        <StepSegment state={compiling ? "waiting" : "active"} />
      </div>
      <div className="mt-[8px] text-[16px] leading-[1.6] text-muted">{label}</div>
    </div>
  );
}

export function TipList({ tips }: { tips: string[] }) {
  return (
    <ol className="m-0 flex list-none flex-col gap-[8px] p-0">
      {tips.map((tip, index) => (
        <li key={tip} className="flex items-start gap-[12px] text-[16px] leading-[1.6]">
          <span
            aria-hidden="true"
            className={[
              "box-border inline-flex h-[28px] w-[28px] shrink-0 items-center justify-center rounded-full border-2",
              "border-solid border-line-strong bg-blue-tint text-[16px] font-bold",
            ].join(" ")}
          >
            {index + 1}
          </span>
          <span className="pt-[1px]">{tip}</span>
        </li>
      ))}
    </ol>
  );
}

export function CardNote({ tone, children }: { tone: "info" | "warn"; children: ReactNode }) {
  return (
    <div
      role="note"
      className={[
        "flex items-start gap-[12px] rounded-[12px] border-2 border-solid px-[12px] py-[8px] text-[16px]",
        "leading-[1.6] font-semibold",
        tone === "warn" ? "border-[var(--warn)] bg-[var(--warn-tint)]" : "border-line-strong bg-blue-tint",
      ].join(" ")}
    >
      <Icon name={tone === "warn" ? "warn" : "info"} size={20} className="mt-[3px]" />
      <span>{children}</span>
    </div>
  );
}

// The compiler's or avrdude's own words, for the teacher: the only way to see why a build
// failed today. Shown as sent, never translated, and folded away from the child.
export function ToolOutput({ text }: { text: string }) {
  const { t } = useTranslation();
  return (
    <details className="rounded-[12px] border-2 border-solid border-line">
      <summary className="cursor-pointer px-[12px] py-[11px] text-[14px] leading-[1.6] font-bold text-blue-ink">
        {t("blockLab.send.toolOutput")}
      </summary>
      <pre
        className={[
          "m-0 max-h-[160px] overflow-auto border-0 border-t-2 border-solid border-line bg-sunken px-[12px] py-[8px]",
          "font-mono text-[13px] leading-[1.5] break-words whitespace-pre-wrap text-ink select-text",
        ].join(" ")}
      >
        {text}
      </pre>
    </details>
  );
}
