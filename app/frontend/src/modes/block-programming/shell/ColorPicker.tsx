import { useTranslation } from "react-i18next";
import { NAMED_COLORS, hexOf, isMixedColor } from "../blockly/colorValue";
import type { FieldMenuOption } from "../blockly/fieldMenu";
import { ColorNumbers } from "./ColorNumbers";
import { ColorWheel } from "./ColorWheel";
import { useColorPicker, type ColorTab } from "./useColorPicker";

type Named = Exclude<FieldMenuOption, "separator">;

const TAB_KEYS: Record<ColorTab, { label: string; aria: string }> = {
  basic: { label: "blockLab.colorPicker.tabs.basic", aria: "blockLab.colorPicker.tabs.basicLabel" },
  wheel: { label: "blockLab.colorPicker.tabs.wheel", aria: "blockLab.colorPicker.tabs.wheelLabel" },
  numbers: { label: "blockLab.colorPicker.tabs.numbers", aria: "blockLab.colorPicker.tabs.numbersLabel" },
};

function Swatch({ hex, size }: { hex: string | null; size: "preview" | "option" }) {
  return (
    <span
      aria-hidden="true"
      style={{ background: hex ?? "transparent" }}
      className={[
        "block shrink-0 box-border border-2 border-solid border-ink",
        size === "preview" ? "h-[36px] w-[36px] rounded-[10px]" : "h-[28px] w-[28px] rounded-[8px]",
      ].join(" ")}
    />
  );
}

function BasicColor({ item, selected, onPick }: { item: Named; selected: boolean; onPick: (value: string) => void }) {
  return (
    <button
      type="button"
      role="option"
      aria-selected={selected}
      aria-label={item.label}
      title={item.label}
      onClick={() => onPick(item.value)}
      className={[
        "box-border flex h-[64px] w-[60px] cursor-pointer flex-col items-center justify-center gap-[2px]",
        "rounded-[12px] px-0 py-[4px] font-body text-[14px] leading-[1.4] font-semibold text-ink hover:bg-blue-tint",
        "focus-visible:outline-[3px] focus-visible:outline-offset-2 focus-visible:outline-blue",
        "focus-visible:shadow-none focus-visible:[outline-style:solid]",
        selected ? "border-[3px] border-solid border-blue bg-blue-tint" : "border-0 bg-transparent",
      ].join(" ")}
    >
      <Swatch hex={NAMED_COLORS[item.value] ?? null} size="option" />
      <span aria-hidden="true" className="whitespace-nowrap">{item.label}</span>
    </button>
  );
}

// The colour field's popover (BlocksPaneA2 1.7): the colour now, then three ways to pick one,
// the eight basic colours, a hue wheel with a lightness slider, or R G B numbers.
export function ColorPicker({ initial, named, onApply }: {
  initial: string | null;
  named: Named[];
  onApply: (value: string) => void;
}) {
  const { t } = useTranslation();
  const picker = useColorPicker(initial, onApply);
  const now = isMixedColor(picker.value)
    ? t("blockLab.colorPicker.mixedName", picker.rgb)
    : named.find(item => item.value === picker.value)?.label;
  return (
    <>
      <div className="flex items-center gap-[12px] px-[4px] pt-[2px] pb-[6px]">
        <Swatch hex={hexOf(picker.value)} size="preview" />
        <span className="flex min-w-0 flex-col">
          <span className="text-[14px] leading-[1.6] font-semibold text-muted">{t("blockLab.fieldMenu.colorNow")}</span>
          <span role="status" className="text-[16px] leading-[1.5] font-bold text-ink">{now}</span>
        </span>
      </div>
      <div
        role="tablist"
        aria-label={t("blockLab.colorPicker.tabs.label")}
        onKeyDown={picker.onTabKey}
        className="flex gap-[4px] rounded-[12px] bg-line p-[4px]"
      >
        {(Object.keys(TAB_KEYS) as ColorTab[]).map(tab => (
          <button
            key={tab}
            type="button"
            role="tab"
            data-tab={tab}
            aria-selected={picker.tab === tab}
            aria-label={t(TAB_KEYS[tab].aria)}
            tabIndex={picker.tab === tab ? 0 : -1}
            onClick={() => picker.pickTab(tab)}
            className={[
              "box-border inline-flex h-[44px] min-w-0 flex-1 cursor-pointer items-center justify-center",
              "rounded-[12px] border-0 px-[6px] font-body text-[16px] leading-[1.5] font-bold whitespace-nowrap",
              "focus-visible:outline-[3px] focus-visible:outline-offset-2 focus-visible:outline-blue",
              "focus-visible:[outline-style:solid]",
              picker.tab === tab
                ? "bg-surface text-blue shadow-[0_1px_3px_rgba(20,36,61,.18)]"
                : "bg-transparent text-muted hover:text-ink",
            ].join(" ")}
          >
            {t(TAB_KEYS[tab].label)}
          </button>
        ))}
      </div>
      {picker.tab === "basic" && (
        <div
          role="listbox"
          aria-label={t("blockLab.colorPicker.basicLabel")}
          className="grid grid-cols-[repeat(4,60px)] justify-center gap-x-[6px] gap-y-[8px] pt-[8px] pb-[2px]"
        >
          {named.map(item => (
            <BasicColor
              key={item.value}
              item={item}
              selected={item.value === picker.value}
              onPick={picker.pickBasic}
            />
          ))}
        </div>
      )}
      {picker.tab === "wheel" && <ColorWheel picker={picker} />}
      {picker.tab === "numbers" && <ColorNumbers picker={picker} />}
    </>
  );
}
