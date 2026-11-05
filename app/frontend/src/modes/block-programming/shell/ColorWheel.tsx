import type { CSSProperties } from "react";
import { useTranslation } from "react-i18next";
import { hexOf, hueNameOf } from "../blockly/colorValue";
import type { ColorPickerState } from "./useColorPicker";

// The design's wheel: 192 across, the handle riding the ring 76 from the centre, the chosen
// colour in a 112 disc in the middle.
const WHEEL = 192;
const HANDLE_ORBIT = 76;
const HANDLE = 30;

// Inline, because a class name split over two lines is invisible to Tailwind's scanner.
const HUE_RING = {
  background: "conic-gradient(from 0deg, hsl(0,100%,50%), hsl(60,100%,50%), hsl(120,100%,50%), " +
    "hsl(180,100%,50%), hsl(240,100%,50%), hsl(300,100%,50%), hsl(360,100%,50%))",
};

function handlePosition(hue: number) {
  const angle = hue * Math.PI / 180;
  return {
    left: Math.round(WHEEL / 2 + HANDLE_ORBIT * Math.sin(angle) - HANDLE / 2),
    top: Math.round(WHEEL / 2 - HANDLE_ORBIT * Math.cos(angle) - HANDLE / 2),
  };
}

export function ColorWheel({ picker }: { picker: ColorPickerState }) {
  const { t } = useTranslation();
  const { hue, lightness } = picker.wheel;
  const hueName = t(`blockLab.colorPicker.hues.${hueNameOf(hue)}`);
  const pureHue = `hsl(${hue},100%,50%)`;
  const track = { "--track": `linear-gradient(to right,#000000,${pureHue},#FFFFFF)` } as CSSProperties;
  return (
    <div className="flex flex-col items-center gap-[4px] pt-[8px] pb-[2px]">
      <div
        role="slider"
        tabIndex={0}
        aria-label={t("blockLab.colorPicker.wheel.hueLabel")}
        aria-valuemin={0}
        aria-valuemax={359}
        aria-valuenow={hue}
        aria-valuetext={t("blockLab.colorPicker.wheel.hueText", { hue, hueName })}
        onPointerDown={picker.wheel.onPointerDown}
        onPointerMove={picker.wheel.onPointerMove}
        onPointerUp={picker.wheel.onPointerUp}
        onPointerCancel={picker.wheel.onPointerCancel}
        onKeyDown={picker.wheel.onKeyDown}
        style={HUE_RING}
        className={[
          "relative h-[192px] w-[192px] shrink-0 cursor-pointer touch-none rounded-full",
          "focus-visible:outline-[3px] focus-visible:outline-offset-[3px] focus-visible:outline-blue",
          "focus-visible:[outline-style:solid]",
        ].join(" ")}
      >
        <span
          aria-hidden="true"
          style={{ background: hexOf(picker.value) ?? "transparent" }}
          className={[
            "pointer-events-none absolute top-[40px] left-[40px] box-border h-[112px] w-[112px] rounded-full",
            "border-2 border-solid border-ink",
          ].join(" ")}
        />
        <span
          aria-hidden="true"
          style={{ ...handlePosition(hue), background: pureHue }}
          className={[
            "pointer-events-none absolute box-border h-[30px] w-[30px] rounded-full border-[3px] border-solid",
            "border-ink shadow-[0_0_0_2px_#ffffff]",
          ].join(" ")}
        />
      </div>
      <span className="text-[14px] leading-[1.6] font-semibold text-ink">
        {t("blockLab.colorPicker.wheel.hue", { hue, hueName })}
      </span>
      <span className="text-[14px] leading-[1.6] text-muted">{t("blockLab.colorPicker.wheel.hint")}</span>
      <label
        htmlFor="block-lab-lightness"
        className="self-start px-[8px] text-[16px] leading-[1.5] font-semibold text-ink"
      >
        {t("blockLab.colorPicker.wheel.lightness", { lightness })}
      </label>
      <input
        id="block-lab-lightness"
        type="range"
        min={0}
        max={100}
        step={1}
        value={lightness}
        aria-valuetext={t("blockLab.colorPicker.wheel.lightnessText", { lightness })}
        onChange={picker.lightness.onChange}
        onPointerDown={picker.lightness.onPointerDown}
        onPointerUp={picker.lightness.onPointerUp}
        onPointerCancel={picker.lightness.onPointerCancel}
        style={track}
        className={[
          "m-0 h-[44px] w-[272px] cursor-pointer appearance-none bg-transparent p-0",
          "focus-visible:rounded-[12px] focus-visible:outline-[3px] focus-visible:outline-offset-2",
          "focus-visible:outline-blue focus-visible:[outline-style:solid]",
          "[&::-webkit-slider-runnable-track]:h-[20px] [&::-webkit-slider-runnable-track]:rounded-[10px]",
          "[&::-webkit-slider-runnable-track]:border-2 [&::-webkit-slider-runnable-track]:border-solid",
          "[&::-webkit-slider-runnable-track]:border-ink [&::-webkit-slider-runnable-track]:[background:var(--track)]",
          "[&::-webkit-slider-thumb]:-mt-[8px] [&::-webkit-slider-thumb]:box-border",
          "[&::-webkit-slider-thumb]:h-[32px] [&::-webkit-slider-thumb]:w-[32px]",
          "[&::-webkit-slider-thumb]:appearance-none [&::-webkit-slider-thumb]:rounded-full",
          "[&::-webkit-slider-thumb]:border-[3px] [&::-webkit-slider-thumb]:border-solid",
          "[&::-webkit-slider-thumb]:border-ink [&::-webkit-slider-thumb]:bg-surface",
        ].join(" ")}
      />
      <span role="status" className="text-[16px] leading-[1.5] font-bold text-ink">
        {t("blockLab.colorPicker.wheel.rgb", picker.rgb)}
      </span>
    </div>
  );
}
