import { t as translate } from "../../i18n";
import { useState } from "react";
import { Icon } from "./Icons";

export type Scenario = { id: string; label: string };

const scenarioRootClass = [
  "sb-scenario fixed left-[116px] bottom-[16px] z-[60] flex flex-col gap-[8px] items-start",
  "max-w-[calc(100vw_-_140px)]",
].join(" ");
const scenarioToggleClass = [
  "sb-scenario-toggle inline-flex items-center gap-[8px] min-h-[44px] py-0 px-[14px]",
  "rounded-[var(--r-pill)] border-2 border-dashed border-line-strong bg-surface text-muted not-italic",
  "[font-variant:normal] font-semibold [font-stretch:normal] text-[14px] leading-none font-body",
  "shadow-card cursor-pointer hover:border-blue hover:text-blue-ink hover:bg-blue-tint",
].join(" ");
const scenarioListClass = [
  "sb-scenario-list order-[-1] flex flex-wrap gap-[6px] p-[10px] bg-surface border-2 border-solid",
  "border-line-strong rounded-card shadow-pop max-h-[50vh] overflow-auto [&_button]:min-h-[44px]",
  "[&_button]:py-0 [&_button]:px-[14px] [&_button]:rounded-control [&_button]:border-2",
  "[&_button]:border-solid [&_button]:border-line-strong [&_button]:bg-surface [&_button]:text-ink",
  "[&_button]:not-italic [&_button]:[font-variant:normal] [&_button]:font-semibold",
  "[&_button]:[font-stretch:normal] [&_button]:text-[14px] [&_button]:leading-none [&_button]:font-body",
  "[&_button]:cursor-pointer [&_button:hover]:border-blue [&_button:hover]:bg-blue-tint",
  "[&_button.on]:bg-blue-ink [&_button.on]:border-blue-ink [&_button.on]:text-white",
].join(" ");

export function ScenarioBar({
  title = "common.messages.previewScenario",
  items,
  value,
  onChange,
}: {
  title?: string;
  items: readonly Scenario[];
  value: string;
  onChange: (id: string) => void;
}) {
  const [open, setOpen] = useState(false);
  const current = items.find((i) => i.id === value);
  return (
    <div className={scenarioRootClass} data-open={open}>
      <button
        type="button"
        className={scenarioToggleClass}
        aria-expanded={open}
        onClick={() => setOpen(!open)}
      >
        <Icon name="sliders" size={18} />
        <span>
          {translate(title)}
          {current ? ` · ${translate(current.label)}` : ""}
        </span>
        <Icon name={open ? "chevD" : "chevU"} size={18} />
      </button>
      {open && (
        <div className={scenarioListClass} role="group" aria-label={translate(title)}>
          {items.map((i) => (
            <button
              key={i.id}
              type="button"
              aria-pressed={i.id === value}
              className={i.id === value ? "on" : undefined}
              onClick={() => onChange(i.id)}
            >
              {translate(i.label)}
            </button>
          ))}
        </div>
      )}
    </div>
  );
}
