import { useEffect, useId, useRef, useState, type CSSProperties, type KeyboardEvent, type ReactNode } from "react";
import { useTranslation } from "react-i18next";
import { Icon } from "../../../components/shared/Icons";
import { CATEGORY_ICONS } from "./categoryIcons";
import { RAIL_GROUPS, RAIL_ORDER, type RailCategory, type RailCategoryId } from "./railCategories";
import { useRailScroll } from "./useRailScroll";

const scrollButtonClass = [
  "absolute left-0 z-[2] box-border flex h-[44px] w-[112px] cursor-pointer items-center justify-center gap-[4px]",
  "border-0 border-solid border-line-strong bg-blue-tint p-0 font-body text-[14px] leading-[1.6] font-bold",
  "text-blue-press focus-visible:outline-[3px] focus-visible:outline-offset-[-3px] focus-visible:outline-blue",
  "focus-visible:[outline-style:solid]",
].join(" ");

const NEXT_KEYS: Record<string, (index: number) => number> = {
  ArrowUp: index => Math.max(0, index - 1),
  ArrowDown: index => Math.min(RAIL_ORDER.length - 1, index + 1),
  Home: () => 0,
  End: () => RAIL_ORDER.length - 1,
};

// Category rail of the A2 design (CatRailA2): a #DCE3EC rail, the chosen category as a white tab,
// no scrollbar but full-width "scroll" buttons that appear only while there is more to see.
export function CategoryRail({
  selected,
  onSelect,
  isEmpty,
}: {
  selected: RailCategoryId;
  onSelect: (id: RailCategoryId) => void;
  isEmpty: (id: RailCategoryId) => boolean;
}) {
  const { t } = useTranslation();
  const { listRef, canScrollUp, canScrollDown, scrollPage, reveal } = useRailScroll();
  const buttons = useRef(new Map<RailCategoryId, HTMLButtonElement>());
  const [focusable, setFocusable] = useState(selected);

  useEffect(() => {
    setFocusable(selected);
    reveal(buttons.current.get(selected));
  }, [selected, reveal]);

  function moveFocus(event: KeyboardEvent<HTMLDivElement>) {
    const next = NEXT_KEYS[event.key];
    if (!next) return;
    event.preventDefault();
    const id = RAIL_ORDER[next(RAIL_ORDER.indexOf(focusable))];
    setFocusable(id);
    buttons.current.get(id)?.focus();
  }

  return (
    <nav
      aria-label={t("blockLab.rail.label")}
      className="relative w-[112px] shrink-0 overflow-hidden rounded-l-[16px] bg-line text-ink"
    >
      <div
        ref={listRef}
        onKeyDown={moveFocus}
        className="absolute inset-0 box-border overflow-x-hidden overflow-y-auto pb-[4px] [scrollbar-width:none]"
      >
        {RAIL_GROUPS.map((group, index) => (
          <RailGroupSection key={group.labelKey} labelKey={group.labelKey} first={index === 0}>
            {group.categories.map(category => (
              <RailItem
                key={category.id}
                category={category}
                selected={category.id === selected}
                focusable={category.id === focusable}
                empty={isEmpty(category.id)}
                onSelect={onSelect}
                onFocus={setFocusable}
                register={element => {
                  if (element) buttons.current.set(category.id, element);
                  else buttons.current.delete(category.id);
                }}
              />
            ))}
          </RailGroupSection>
        ))}
      </div>
      {canScrollUp && (
        <button
          type="button"
          aria-label={t("blockLab.rail.scrollUpLabel")}
          className={`${scrollButtonClass} top-0 border-b-2`}
          onClick={() => scrollPage(-1)}
        >
          <Icon name="chevU" size={18} />
          {t("blockLab.rail.scrollUp")}
        </button>
      )}
      {canScrollDown && (
        <button
          type="button"
          aria-label={t("blockLab.rail.scrollDownLabel")}
          className={`${scrollButtonClass} bottom-0 border-t-2`}
          onClick={() => scrollPage(1)}
        >
          <Icon name="chevD" size={18} />
          {t("blockLab.rail.scrollDown")}
        </button>
      )}
    </nav>
  );
}

function RailGroupSection({
  labelKey,
  first,
  children,
}: {
  labelKey: string;
  first: boolean;
  children: ReactNode;
}) {
  const { t } = useTranslation();
  const headingId = useId();
  return (
    <div role="group" aria-labelledby={headingId}>
      <div
        id={headingId}
        className={[
          "box-border flex items-center justify-center text-center font-body text-[14px] leading-[1.6] font-bold",
          "text-muted",
          first
            ? "mx-[4px] mt-[4px] h-[28px] px-[4px]"
            : "mx-[8px] mt-[12px] h-[34px] border-0 border-t-2 border-solid border-line-strong pt-[4px]",
        ].join(" ")}
      >
        {t(labelKey)}
      </div>
      {children}
    </div>
  );
}

function RailItem({
  category,
  selected,
  focusable,
  empty,
  onSelect,
  onFocus,
  register,
}: {
  category: RailCategory;
  selected: boolean;
  focusable: boolean;
  empty: boolean;
  onSelect: (id: RailCategoryId) => void;
  onFocus: (id: RailCategoryId) => void;
  register: (element: HTMLButtonElement | null) => void;
}) {
  const { t } = useTranslation();
  const name = t(category.labelKey);
  const CategoryIcon = CATEGORY_ICONS[category.id];
  const colours = { "--cat-fill": category.fill, "--cat-edge": category.edge } as CSSProperties;
  return (
    <button
      ref={register}
      type="button"
      tabIndex={focusable ? 0 : -1}
      aria-pressed={selected}
      aria-label={empty ? t("blockLab.rail.emptyCategory", { categoryName: name }) : name}
      style={colours}
      onClick={() => onSelect(category.id)}
      onFocus={() => onFocus(category.id)}
      className={[
        "mx-[4px] mt-[4px] box-border flex min-h-[60px] w-[104px] cursor-pointer flex-col items-center justify-center",
        "rounded-[12px] border-0 px-[2px] py-[4px] text-center font-body text-[16px] leading-[1.6] font-bold",
        "focus-visible:outline-[3px] focus-visible:outline-offset-[-3px] focus-visible:outline-blue",
        "focus-visible:[outline-style:solid]",
        selected
          ? "bg-surface text-blue shadow-[0_1px_3px_rgba(20,36,61,.18)]"
          : "bg-transparent text-muted hover:bg-blue-tint hover:text-ink",
      ].join(" ")}
    >
      <span
        aria-hidden="true"
        className={[
          "box-border inline-flex h-[26px] w-[26px] items-center justify-center rounded-[9px] border-2",
          "border-[var(--cat-edge)] bg-[var(--cat-fill)] text-ink",
          empty ? "border-dashed" : "border-solid",
        ].join(" ")}
      >
        <CategoryIcon size={16} />
      </span>
      <span className="max-w-full [overflow-wrap:break-word]">{name}</span>
    </button>
  );
}
