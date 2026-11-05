import { useTranslation } from "react-i18next";
import { outline } from "./categoryIcons";
import type { HintPlace } from "./useStartHint";

const ArrowUp = outline("M12 19V5M5 12l7-7 7 7");

// "Drag blocks under loop", shown under the loop hat of an empty program. It scales with the
// workspace like the blocks it points at and lets the pointer through to them.
export function StartHint({ place }: { place: HintPlace | null }) {
  const { t } = useTranslation();
  if (!place) return null;
  return (
    <div
      role="note"
      style={{ left: place.left, top: place.top, transform: `scale(${place.scale})` }}
      className={[
        "pointer-events-none absolute z-[5] box-border w-[340px] origin-top-left rounded-[12px] border-[3px]",
        "border-dashed border-blue bg-blue-tint px-[14px] py-[10px] font-body leading-[1.6]",
      ].join(" ")}
    >
      <div className="flex items-center gap-[8px] text-[18px] font-bold text-ink">
        <ArrowUp size={20} aria-hidden="true" />
        {t("blockLab.workspace.hint.title")}
      </div>
      <div className="text-[16px] text-muted">{t("blockLab.workspace.hint.tap")}</div>
    </div>
  );
}
