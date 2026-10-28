import { nav } from "./appData";
import { t } from "../i18n";
import { Icon } from "../components/shared/Icons";
import type { View } from "../shell";

const navItemClass = [
  "sb-nav flex flex-col items-center justify-center gap-[4px] w-[84px] min-h-[72px] rounded-[14px]",
  "border-2 border-solid border-transparent border-b-4 bg-transparent text-muted not-italic",
  "[font-variant:normal] font-bold [font-stretch:normal] text-[13px] leading-[1.2] font-body",
  "cursor-pointer no-underline hover:bg-blue-tint hover:text-blue-ink [&.sb-is-hover]:bg-blue-tint",
  "[&.sb-is-hover]:text-blue-ink [&.sb-on]:bg-blue-tint [&.sb-on]:text-blue-ink",
  "[&.sb-on]:border-blue-line [&.sb-on]:border-b-blue focus-visible:outline-none",
  "focus-visible:shadow-[var(--focus)] [&.sb-is-focus]:outline-none [&.sb-is-focus]:shadow-[var(--focus)]",
].join(" ");

type Props = {
  view: View;
  onNavigate: (view: View) => void;
  section: "learn" | "create";
  wide?: boolean;
};

export function AppNavigation({ view, onNavigate, section, wide = false }: Props) {
  const items = section === "learn" ? nav.slice(0, 2) : nav.slice(2);
  return items.map(item => (
    <button
      key={item.view}
      type="button"
      className={`${navItemClass} ${view === item.view ? "sb-on" : ""}`}
      onClick={() => onNavigate(item.view)}
      aria-current={view === item.view ? "page" : undefined}
    >
      <Icon name={item.icon} size={wide ? 26 : 28} />
      <span className="block whitespace-nowrap">{t(item.label)}</span>
    </button>
  ));
}
