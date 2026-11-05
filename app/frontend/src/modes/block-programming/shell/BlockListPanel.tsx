import type { CSSProperties, ReactNode } from "react";
import { useTranslation } from "react-i18next";
import { Icon } from "../../../components/shared/Icons";
import { CATEGORY_ICONS, outline } from "./categoryIcons";
import { railCategory, type RailCategoryId } from "./railCategories";
import { ToolButton } from "./ToolButton";
import type { BlockListState } from "./useBlockList";

// The design's "hide list" and "show list" icons: a sidebar with an arrow.
const SIDEBAR_PATH = "M5 3h14a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2zM9 3v18";
const HideIcon = outline(`${SIDEBAR_PATH}M16 15l-3-3 3-3`);
const ShowIcon = outline(`${SIDEBAR_PATH}M14 9l3 3-3 3`);
// The Start note follows the two hats, as in the design's list.
const START_NOTE_GAP = 6;

function CategoryChip({ id, size, empty }: { id: RailCategoryId; size: "header" | "button"; empty?: boolean }) {
  const category = railCategory(id);
  const CategoryIcon = CATEGORY_ICONS[id];
  return (
    <span
      aria-hidden="true"
      style={{ "--cat-fill": category.fill, "--cat-edge": category.edge } as CSSProperties}
      className={[
        "box-border inline-flex shrink-0 items-center justify-center border-2 border-[var(--cat-edge)]",
        "bg-[var(--cat-fill)] text-ink",
        size === "header" ? "h-[32px] w-[32px] rounded-[10px]" : "h-[26px] w-[26px] rounded-[9px]",
        empty ? "border-dashed" : "border-solid",
      ].join(" ")}
    >
      <CategoryIcon size={size === "header" ? 20 : 16} />
    </span>
  );
}

function Note({ icon, children }: { icon: "info" | "lock"; children: ReactNode }) {
  return (
    <div
      role="note"
      className="flex items-start gap-[8px] px-[2px] py-[6px] font-body text-[16px] leading-[1.6] text-muted"
    >
      <Icon name={icon} size={20} className="mt-[4px]" />
      <span>{children}</span>
    </div>
  );
}

// The list's own chrome around Blockly's flyout: header, "more above/below" fades, the delete
// zone while a placed block is dragged over it, and the notes for empty or already-placed
// categories. Blocks themselves are drawn by the flyout underneath.
export function BlockListPanel({
  list,
  category,
  empty,
  deleteZone,
  onPick,
}: {
  list: BlockListState;
  category: RailCategoryId;
  empty: boolean;
  // The list is a place to let go of a block for deleting, shown while a placed block is over it.
  deleteZone: boolean;
  onPick: (id: RailCategoryId) => void;
}) {
  const { t } = useTranslation();
  if (!list.shown) {
    return (
      <div
        className={[
          "absolute top-[12px] left-[12px] z-[24] rounded-[16px] border-2 border-solid border-line bg-surface p-[4px]",
          "shadow-[0_8px_20px_rgba(20,36,61,.2)]",
        ].join(" ")}
      >
        <ToolButton
          label={t("blockLab.list.show")}
          tip={t("blockLab.list.show")}
          tipSide="below-left"
          aria-expanded="false"
          onClick={list.show}
        >
          <ShowIcon size={24} aria-hidden="true" />
        </ToolButton>
      </div>
    );
  }
  // The panel's frame (right edge, rounded corners, shadow) is drawn here once, over the header
  // and Blockly's list alike, so nothing shows where they meet. With nothing in Blockly's
  // list this panel also draws the list surface itself.
  return (
    <div
      role="region"
      aria-label={t("blockLab.list.label")}
      style={{ width: list.width }}
      className={[
        "pointer-events-none absolute top-0 bottom-0 left-0 z-[20] rounded-r-[16px] border-0 border-r-2",
        "border-solid border-line shadow-[8px_0_20px_-8px_rgba(20,36,61,.24)]",
        empty ? "bg-surface" : "",
      ].join(" ")}
    >
      <div
        className={[
          "pointer-events-auto box-border flex h-[52px] items-center gap-[10px] rounded-tr-[14px] bg-surface",
          "py-[4px] pr-[4px] pl-[10px]",
        ].join(" ")}
      >
        <CategoryChip id={category} size="header" empty={empty} />
        <span className="min-w-0 flex-1 truncate font-body text-[18px] leading-[1.5] font-bold text-ink">
          {t(railCategory(category).labelKey)}
        </span>
        <ToolButton
          label={t("blockLab.list.hide")}
          tip={t("blockLab.list.hideHint")}
          tipSide="below-right"
          aria-expanded="true"
          onClick={list.hide}
        >
          <HideIcon size={24} aria-hidden="true" />
        </ToolButton>
      </div>
      {empty && (
        <div className="pointer-events-auto flex flex-col gap-[10px] p-[8px_10px] font-body text-[16px] leading-[1.6]">
          <div className="flex items-start gap-[8px] font-bold text-ink">
            <Icon name="info" size={20} className="mt-[3px]" />
            {t("blockLab.emptyCategory.title")}
          </div>
          <div className="text-muted">{t("blockLab.emptyCategory.hint")}</div>
          {(["sensor", "control"] as const).map(target => (
            <button
              key={target}
              type="button"
              onClick={() => onPick(target)}
              aria-label={t("blockLab.emptyCategory.goTo", { categoryName: t(railCategory(target).labelKey) })}
              className={[
                "inline-flex min-h-[44px] w-full cursor-pointer items-center justify-start gap-[6px] rounded-[12px] border-2",
                "border-b-4 border-solid border-line-strong bg-surface px-[12px] font-body text-[16px] font-semibold",
                "leading-[1.6] text-ink hover:bg-blue-tint",
              ].join(" ")}
            >
              <CategoryChip id={target} size="button" />
              {t(railCategory(target).labelKey)}
            </button>
          ))}
        </div>
      )}
      {category === "start" && list.hatsPlaced && (
        <div className="absolute right-0 left-0 px-[14px]" style={{ top: list.contentBottom + START_NOTE_GAP }}>
          <Note icon="lock">{t("blockLab.startPlaced.hint")}</Note>
        </div>
      )}
      {list.fades.top && (
        <div aria-hidden="true" className="absolute top-[52px] right-0 left-0 h-[28px] bg-gradient-to-b from-white/90 to-white/0" />
      )}
      {list.fades.bottom && (
        <div
          aria-hidden="true"
          className={[
            "absolute right-0 bottom-0 left-0 box-border flex h-[44px] items-end justify-center bg-gradient-to-t",
            "from-white/90 to-white/0 pb-[4px] text-muted",
          ].join(" ")}
        >
          <Icon name="chevD" size={20} />
        </div>
      )}
      {deleteZone && (
        <div
          className={[
            "absolute inset-0 z-[5] box-border flex flex-col items-center justify-center gap-[12px]",
            "border-[3px] border-dashed border-err bg-[#FDECEC]",
          ].join(" ")}
        >
          <Icon name="trash" size={56} className="text-err" />
          <span className="font-body text-[20px] font-bold text-ink">{t("blockLab.list.dropToDelete")}</span>
        </div>
      )}
    </div>
  );
}
