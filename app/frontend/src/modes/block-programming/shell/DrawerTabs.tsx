import { useRef, type KeyboardEvent } from "react";
import { useTranslation } from "react-i18next";
import { CATEGORY_ICONS, outline } from "./categoryIcons";

export type DrawerTab = "output" | "cpp";

const TABS: readonly DrawerTab[] = ["output", "cpp"];
export const TAB_ICONS = {
  // Output shows what the board sends over Serial, so it wears the Serial category's icon.
  output: CATEGORY_ICONS.serial,
  cpp: outline("M8 7l-5 5 5 5M16 7l5 5-5 5M14 4l-4 16"),
};
const TAB_LABEL_KEYS = { output: "blockLab.drawer.output", cpp: "blockLab.drawer.cpp" };

export const drawerTabId = (base: string, tab: DrawerTab) => `${base}-tab-${tab}`;
export const drawerPanelId = (base: string, tab: DrawerTab) => `${base}-panel-${tab}`;

// The drawer's tabs on a grey rail: arrows, Home and End move between them, as ARIA tabs do.
// The Output tab counts the lines that came while it was out of sight.
export function DrawerTabs({ base, tab, unread, onTab }: {
  base: string;
  tab: DrawerTab;
  unread: number;
  onTab: (next: DrawerTab) => void;
}) {
  const { t } = useTranslation();
  const buttons = useRef<Partial<Record<DrawerTab, HTMLButtonElement | null>>>({});
  const move = (event: KeyboardEvent<HTMLButtonElement>) => {
    const index = TABS.indexOf(tab);
    const targets: Record<string, DrawerTab> = {
      ArrowLeft: TABS[(index + TABS.length - 1) % TABS.length],
      ArrowRight: TABS[(index + 1) % TABS.length],
      Home: TABS[0],
      End: TABS[TABS.length - 1],
    };
    const next = targets[event.key];
    if (!next) return;
    event.preventDefault();
    onTab(next);
    buttons.current[next]?.focus();
  };
  return (
    <div
      role="tablist"
      aria-label={t("blockLab.drawer.tabs")}
      className="flex shrink-0 items-center gap-[4px] rounded-[12px] bg-line p-[4px]"
    >
      {TABS.map(id => {
        const TabIcon = TAB_ICONS[id];
        const selected = id === tab;
        const badge = id === "output" && unread > 0;
        return (
          <button
            key={id}
            ref={element => {
              buttons.current[id] = element;
            }}
            type="button"
            role="tab"
            id={drawerTabId(base, id)}
            aria-selected={selected}
            aria-controls={drawerPanelId(base, id)}
            aria-label={badge ? t("blockLab.drawer.outputUnread", { lineCount: unread }) : undefined}
            tabIndex={selected ? 0 : -1}
            onClick={() => onTab(id)}
            onKeyDown={move}
            className={[
              "box-border inline-flex h-[44px] cursor-pointer items-center gap-[8px] rounded-[12px] border-0 px-[12px]",
              "font-body text-[16px] leading-[1.6] font-bold whitespace-nowrap focus-visible:outline-[3px]",
              "focus-visible:outline-offset-2 focus-visible:outline-blue focus-visible:[outline-style:solid]",
              selected
                ? "bg-surface text-blue shadow-[0_1px_3px_rgba(20,36,61,.18)]"
                : "bg-transparent text-muted hover:bg-blue-tint hover:text-ink",
            ].join(" ")}
          >
            <TabIcon size={20} aria-hidden="true" />
            {t(TAB_LABEL_KEYS[id])}
            {badge && (
              <span
                aria-hidden="true"
                className={[
                  "box-border inline-flex h-[24px] min-w-[24px] items-center justify-center rounded-[12px] bg-blue",
                  "px-[6px] text-[14px] leading-none font-bold text-white tabular-nums",
                ].join(" ")}
              >
                {unread}
              </span>
            )}
          </button>
        );
      })}
    </div>
  );
}
