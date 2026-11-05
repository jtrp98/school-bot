import { useEffect, type KeyboardEvent } from "react";
import { useTranslation } from "react-i18next";
import { Icon, type IconName } from "../../../components/shared/Icons";
import { languages } from "../../../i18n";
import { BarPopover } from "./BarPopover";
import { Tip } from "./Tip";
import type { BlockProject } from "./useBlockProject";
import { usePopover } from "./usePopover";

type Item = { labelKey: string; icon: IconName; shortcut?: string; run?: () => void };

const NEXT_ITEM: Record<string, (index: number, count: number) => number> = {
  ArrowDown: (index, count) => (index + 1) % count,
  ArrowUp: (index, count) => (index - 1 + count) % count,
  Home: () => 0,
  End: (_index, count) => count - 1,
};

function MenuItem({ item, onDone }: { item: Item; onDone: () => void }) {
  const { t } = useTranslation();
  const ready = !!item.run;
  return (
    <button
      type="button"
      role="menuitem"
      aria-disabled={!ready}
      onClick={() => {
        if (!item.run) return;
        onDone();
        item.run();
      }}
      className={[
        "box-border flex min-h-[44px] w-full items-center gap-[12px] rounded-[12px] border-0 bg-transparent px-[12px]",
        "py-[4px] text-left font-body text-[16px] font-semibold hover:bg-blue-tint focus-visible:bg-blue-tint",
        "focus-visible:outline-none",
        ready ? "cursor-pointer text-ink" : "cursor-default text-muted",
      ].join(" ")}
    >
      <Icon name={item.icon} size={20} />
      <span className="flex flex-col">
        {t(item.labelKey)}
        {!ready && <span className="text-[13px] font-medium">{t("blockLab.menu.notYet")}</span>}
      </span>
      {item.shortcut && (
        <span
          className={[
            "ml-auto rounded-[8px] border border-b-2 border-solid border-line bg-sunken px-[6px] text-[12px]",
            "leading-[1.7] font-bold whitespace-nowrap text-muted",
          ].join(" ")}
        >
          {item.shortcut}
        </span>
      )}
    </button>
  );
}

// The ☰ menu of the design (7 entries). Entries this version cannot do yet stay listed with the
// reason instead of disappearing, so nothing the design promises is silently missing.
export function MainMenu({ project }: { project: BlockProject }) {
  const { t, i18n } = useTranslation();
  const popover = usePopover();
  const fileItems: Item[] = [
    { labelKey: "blockLab.menu.new", icon: "plus", shortcut: "Ctrl+N", run: project.requestNew },
    { labelKey: "blockLab.menu.save", icon: "save", shortcut: "Ctrl+S", run: () => void project.save() },
    { labelKey: "blockLab.menu.saveCopy", icon: "fileCopy", shortcut: "Ctrl+Shift+S" },
    { labelKey: "blockLab.menu.export", icon: "upload" },
  ];
  const helpItems: Item[] = [
    { labelKey: "blockLab.menu.checkConnection", icon: "wifi" },
    { labelKey: "blockLab.menu.about", icon: "info" },
  ];

  useEffect(() => {
    if (popover.open) popover.root.current?.querySelector<HTMLElement>("[role=menuitem]")?.focus();
  }, [popover.open, popover.root]);

  function moveFocus(event: KeyboardEvent<HTMLDivElement>) {
    const next = NEXT_ITEM[event.key];
    if (!next) return;
    event.preventDefault();
    const items = [...event.currentTarget.querySelectorAll<HTMLElement>("[role=menuitem]")];
    items[next(items.indexOf(document.activeElement as HTMLElement), items.length)]?.focus();
  }

  return (
    <div ref={popover.root} className="relative shrink-0">
      <button
        ref={popover.trigger}
        type="button"
        aria-haspopup="menu"
        aria-expanded={popover.open}
        aria-label={t("blockLab.menu.label")}
        onClick={popover.toggle}
        className={[
          "group relative box-border inline-flex h-[44px] w-[44px] shrink-0 cursor-pointer items-center justify-center",
          "rounded-[12px] border-2 border-solid border-line-strong bg-surface p-0 text-ink hover:border-blue",
          "hover:bg-blue-tint hover:text-blue aria-expanded:border-blue aria-expanded:bg-blue aria-expanded:text-white",
        ].join(" ")}
      >
        <Icon name="menu" size={20} />
        {!popover.open && <Tip side="below-left">{t("blockLab.menu.label")}</Tip>}
      </button>
      {popover.open && (
        <BarPopover
          align="left"
          role="menu"
          label={t("blockLab.menu.label")}
          onKeyDown={moveFocus}
          className="w-max min-w-[280px] max-w-[320px] gap-[2px] p-[8px]"
        >
          {fileItems.map(item => <MenuItem key={item.labelKey} item={item} onDone={popover.close} />)}
          <div aria-hidden="true" className="my-[4px] h-px bg-line" />
          <div className="flex min-h-[48px] items-center gap-[12px] px-[12px]">
            <Icon name="globe" size={20} />
            <span className="flex-1 font-body text-[16px] leading-[1.6] font-semibold">{t("language.label")}</span>
            {languages.map(language => (
              <button
                key={language.value}
                type="button"
                aria-pressed={i18n.resolvedLanguage === language.value}
                onClick={() => void i18n.changeLanguage(language.value)}
                className={[
                  "box-border h-[44px] min-w-[44px] cursor-pointer rounded-[12px] border-2 border-solid px-[8px]",
                  "font-body text-[16px] leading-[1.5] font-bold",
                  i18n.resolvedLanguage === language.value
                    ? "border-blue bg-blue-tint text-blue"
                    : "border-line-strong bg-surface text-ink",
                ].join(" ")}
              >
                {language.label}
              </button>
            ))}
          </div>
          <div aria-hidden="true" className="my-[4px] h-px bg-line" />
          {helpItems.map(item => <MenuItem key={item.labelKey} item={item} onDone={popover.close} />)}
        </BarPopover>
      )}
    </div>
  );
}
