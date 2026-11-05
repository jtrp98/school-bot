import { useTranslation } from "react-i18next";
import type { ColorPickerState, NumberNote } from "./useColorPicker";

const NOTE_KEYS: Record<NumberNote["kind"], string> = {
  notDigits: "blockLab.colorPicker.numbers.notDigits",
  empty: "blockLab.colorPicker.numbers.empty",
  tooHigh: "blockLab.colorPicker.numbers.tooHigh",
};

// R G B as numbers 0–255. A number is used on Enter or when the child leaves the box; anything
// that is not one is fixed up and the line under the box says what was used.
export function ColorNumbers({ picker }: { picker: ColorPickerState }) {
  const { t } = useTranslation();
  const { channels, text, notes, handlers } = picker.numbers;
  return (
    <div className="flex flex-col gap-[6px] px-[4px] pt-[8px] pb-[2px]">
      {channels.map(channel => {
        const note = notes[channel];
        return (
          <div key={channel} className="flex flex-col gap-[2px]">
            <div className="flex items-center gap-[12px]">
              <label
                htmlFor={`block-lab-color-${channel}`}
                className="flex-1 text-[16px] leading-[1.5] font-semibold text-ink"
              >
                {t(`blockLab.colorPicker.numbers.${channel}`)}
              </label>
              <input
                id={`block-lab-color-${channel}`}
                type="text"
                inputMode="numeric"
                maxLength={4}
                aria-label={t(`blockLab.colorPicker.numbers.${channel}Label`)}
                aria-describedby={`block-lab-color-${channel}-note`}
                value={text[channel]}
                {...handlers(channel)}
                className={[
                  "box-border h-[44px] w-[88px] rounded-[12px] border-2 border-solid border-line-strong bg-surface",
                  "px-[12px] text-right font-body text-[16px] font-semibold text-ink outline-none",
                  "focus:border-blue focus:shadow-[0_0_0_3px_#d0ebfd]",
                ].join(" ")}
              />
            </div>
            <span
              id={`block-lab-color-${channel}-note`}
              role="status"
              className="text-[14px] leading-[1.6] font-semibold text-ink"
            >
              {note ? t(NOTE_KEYS[note.kind], { number: note.used }) : ""}
            </span>
          </div>
        );
      })}
      <span className="text-[14px] leading-[1.6] text-muted">{t("blockLab.colorPicker.numbers.hint")}</span>
    </div>
  );
}
