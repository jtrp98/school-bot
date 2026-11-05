import { useTranslation } from "react-i18next";
import { Icon } from "../../../components/shared/Icons";
import type { FieldMenuKind, FieldMenuOption } from "../blockly/fieldMenu";
import { ColorPicker } from "./ColorPicker";
import type { FieldMenuState } from "./useFieldMenu";

const LABEL_KEYS: Record<FieldMenuKind, string> = {
  pin: "blockLab.fieldMenu.pin",
  variable: "blockLab.fieldMenu.variable",
  color: "blockLab.fieldMenu.color",
  value: "blockLab.fieldMenu.value",
};

// The design's popover box (as the ☰ menu): 8 from the field, kept 8 inside the window, and
// opening upwards only when there is clearly more room above.
const GAP = 8;
const EDGE = 8;
const MENU_WIDTH = 312;
const MAX_HEIGHT = 400;
const ROOM_BELOW = 240;
// The colour popover is as tall as its wheel tab needs, and opens below only when all of it fits.
const COLOR_HEIGHT = 520;

function placement(anchor: DOMRect, height: number, roomBelow: number) {
  const below = window.innerHeight - anchor.bottom - GAP - EDGE;
  const above = anchor.top - GAP - EDGE;
  const left = Math.max(EDGE, Math.min(anchor.left, window.innerWidth - EDGE - MENU_WIDTH));
  if (below >= roomBelow || below >= above) {
    return { left, top: anchor.bottom + GAP, maxHeight: Math.min(height, below) };
  }
  return { left, bottom: window.innerHeight - anchor.top + GAP, maxHeight: Math.min(height, above) };
}

type Item = Exclude<FieldMenuOption, "separator">;
type OptionProps = { item: Item; selected: boolean; onPick: (value: string) => void };

function ListOption({ item, selected, onPick }: OptionProps) {
  return (
    <button
      type="button"
      role="option"
      aria-selected={selected}
      onClick={() => onPick(item.value)}
      className={[
        "box-border flex min-h-[44px] w-full shrink-0 cursor-pointer items-center gap-[12px] rounded-[12px] border-0",
        "bg-transparent px-[12px] py-[4px] text-left font-body text-[16px] leading-[1.5] font-semibold text-ink",
        "hover:bg-blue-tint focus-visible:bg-blue-tint focus-visible:shadow-none focus-visible:outline-none",
      ].join(" ")}
    >
      {item.action && <Icon name={item.action === "rename" ? "edit" : "trash"} size={20} />}
      <span className="min-w-0 flex-1 [overflow-wrap:anywhere]">{item.label}</span>
      {selected && <Icon name="check" size={20} className="shrink-0 text-blue" />}
    </button>
  );
}

// What a dropdown field opens: the field's own options in the design's menu box, or, for the
// colour field, the colour picker.
export function FieldMenu({ menu }: { menu: FieldMenuState }) {
  const { t } = useTranslation();
  const request = menu.request;
  if (!request) return null;
  const label = t(LABEL_KEYS[request.kind]);
  const color = request.kind === "color";
  return (
    <div
      ref={menu.panel}
      role={color ? "dialog" : "listbox"}
      aria-label={label}
      tabIndex={-1}
      onKeyDown={menu.moveFocus}
      style={color
        ? placement(request.anchor, COLOR_HEIGHT, COLOR_HEIGHT)
        : placement(request.anchor, MAX_HEIGHT, ROOM_BELOW)}
      className={[
        "fixed z-[60] box-border flex flex-col gap-[2px] overflow-y-auto",
        color ? "w-[312px]" : "w-max min-w-[280px] max-w-[312px]",
        "rounded-[16px] border border-solid border-line bg-surface p-[8px] text-left font-body outline-none",
        "shadow-[0_8px_20px_rgba(20,36,61,.2)]",
        "motion-safe:animate-[block-lab-pop-in_.2s_cubic-bezier(.2,.8,.2,1)]",
      ].join(" ")}
    >
      {color ? (
        <ColorPicker
          initial={request.value}
          named={request.options.filter((option): option is Item => option !== "separator")}
          onApply={menu.apply}
        />
      ) : (
        <>
          <div
            aria-hidden="true"
            className="px-[12px] pt-[2px] pb-[4px] text-[14px] leading-[1.6] font-semibold text-muted"
          >
            {label}
          </div>
          {request.options.map((option, index) => option === "separator"
            ? <div key={`separator-${index}`} aria-hidden="true" className="my-[4px] h-px shrink-0 bg-line" />
            : (
              <ListOption
                key={option.value}
                item={option}
                selected={!option.action && option.value === request.value}
                onPick={menu.pick}
              />
            ))}
        </>
      )}
    </div>
  );
}
