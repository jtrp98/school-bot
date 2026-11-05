import { useEffect, useRef, useState, type ChangeEvent, type KeyboardEvent, type PointerEvent } from "react";
import * as Blockly from "blockly/core";
import { NAMED_COLORS, colorValueOf, hslFromRgb, rgbFromHsl, rgbOf, type Rgb } from "../blockly/colorValue";

export type ColorTab = "basic" | "wheel" | "numbers";
export type ColorChannel = keyof Rgb;
export type NumberNote = { kind: "notDigits" | "empty" | "tooHigh"; used: number };

const CHANNELS: readonly ColorChannel[] = ["red", "green", "blue"];
const TABS: readonly ColorTab[] = ["basic", "wheel", "numbers"];
const HUE_STEP = 5;
const HUE_JUMP = 30;
// The wheel answers a press on its ring (between half and the edge of its radius, with a
// little slack outside), not on the colour disc in the middle.
const RING_INNER = 0.5;
const RING_OUTER = 1.08;

// The mixing tab used last opens next time, unless the colour is a basic one.
let lastMixTab: Exclude<ColorTab, "basic"> = "wheel";

type WheelState = { forValue: string; hue: number; lightness: number };
type Draft = { forValue: string; text: Record<ColorChannel, string>; notes: Partial<Record<ColorChannel, NumberNote>> };

const textOf = (rgb: Rgb) => ({ red: String(rgb.red), green: String(rgb.green), blue: String(rgb.blue) });

function hueAt(event: PointerEvent<HTMLElement>, onRingOnly: boolean) {
  const box = event.currentTarget.getBoundingClientRect();
  const dx = event.clientX - (box.left + box.width / 2);
  const dy = event.clientY - (box.top + box.height / 2);
  const reach = Math.hypot(dx, dy) / (box.width / 2);
  if (onRingOnly && (reach < RING_INNER || reach > RING_OUTER)) return null;
  return (Math.atan2(dx, -dy) * 180 / Math.PI + 360) % 360;
}

function parseChannel(text: string): { channel: number; note?: NumberNote } {
  const digits = text.trim().replace(/[^0-9]/g, "");
  const typed = digits === "" ? 0 : parseInt(digits, 10);
  const channel = Math.min(255, typed);
  if (typed > 255) return { channel, note: { kind: "tooHigh", used: channel } };
  if (digits !== text.trim()) return { channel, note: { kind: "notDigits", used: channel } };
  if (digits === "") return { channel, note: { kind: "empty", used: channel } };
  return { channel };
}

// The colour popover's state: the colour shown, the open tab, the wheel's hue and lightness and
// the numbers being typed. Every change goes straight to the block's field through `apply`.
export function useColorPicker(initial: string | null, apply: (value: string) => void) {
  const [value, setValue] = useState(initial ?? "OFF");
  const [tab, setTab] = useState<ColorTab>(initial !== null && initial in NAMED_COLORS ? "basic" : lastMixTab);
  const [wheelState, setWheelState] = useState<WheelState | null>(null);
  const [draftState, setDraftState] = useState<Draft | null>(null);
  const dragging = useRef(false);
  const rgb = rgbOf(value);

  // Greys have no hue of their own, so the wheel keeps the hue it had.
  const derived = hslFromRgb(rgb);
  const wheel = wheelState?.forValue === value ? wheelState : {
    forValue: value,
    hue: derived.saturation < 0.05 ? wheelState?.hue ?? 0 : derived.hue,
    lightness: derived.lightness,
  };
  const draft = draftState?.forValue === value ? draftState : { forValue: value, text: textOf(rgb), notes: {} };

  const change = (next: string) => {
    setValue(next);
    apply(next);
  };

  const pickTab = (next: ColorTab) => {
    setTab(next);
    if (next !== "basic") lastMixTab = next;
  };

  // Left and right arrows move along the tabs, as in a tab list.
  const onTabKey = (event: KeyboardEvent<HTMLElement>) => {
    const step = event.key === "ArrowRight" ? 1 : event.key === "ArrowLeft" ? -1 : 0;
    if (!step) return;
    event.preventDefault();
    event.stopPropagation();
    const next = TABS[(TABS.indexOf(tab) + step + TABS.length) % TABS.length];
    pickTab(next);
    event.currentTarget.querySelector<HTMLElement>(`[data-tab=${next}]`)?.focus();
  };

  const mix = (hue: number, lightness: number) => {
    const turned = ((Math.round(hue) % 360) + 360) % 360;
    const level = Math.max(0, Math.min(100, Math.round(lightness)));
    const next = colorValueOf(rgbFromHsl(turned, level));
    setWheelState({ forValue: next, hue: turned, lightness: level });
    change(next);
  };

  // A drag on the wheel or the slider is one change in the undo history, not one per step.
  const startDrag = () => {
    dragging.current = true;
    Blockly.Events.setGroup(true);
  };
  const endDrag = () => {
    if (!dragging.current) return;
    dragging.current = false;
    Blockly.Events.setGroup(false);
  };
  // The popover can close in the middle of a drag; the undo group must not stay open after it.
  useEffect(() => () => {
    if (dragging.current) Blockly.Events.setGroup(false);
  }, []);

  const wheelHandlers = {
    onPointerDown: (event: PointerEvent<HTMLElement>) => {
      const hue = hueAt(event, true);
      if (hue === null) return;
      event.preventDefault();
      event.currentTarget.setPointerCapture(event.pointerId);
      startDrag();
      mix(hue, wheel.lightness);
    },
    onPointerMove: (event: PointerEvent<HTMLElement>) => {
      if (dragging.current) mix(hueAt(event, false) ?? wheel.hue, wheel.lightness);
    },
    onPointerUp: endDrag,
    onPointerCancel: endDrag,
    // ← ↓ and → ↑ turn 5° (Shift: 30°), Page Up/Down 30°, Home goes to red and End to 355°.
    onKeyDown: (event: KeyboardEvent<HTMLElement>) => {
      const step = event.shiftKey ? HUE_JUMP : HUE_STEP;
      const turns: Record<string, number> = {
        ArrowRight: wheel.hue + step,
        ArrowUp: wheel.hue + step,
        ArrowLeft: wheel.hue - step,
        ArrowDown: wheel.hue - step,
        PageUp: wheel.hue + HUE_JUMP,
        PageDown: wheel.hue - HUE_JUMP,
        Home: 0,
        End: 355,
      };
      if (!(event.key in turns)) return;
      event.preventDefault();
      event.stopPropagation();
      mix(turns[event.key], wheel.lightness);
    },
  };

  const lightnessHandlers = {
    onChange: (event: ChangeEvent<HTMLInputElement>) => mix(wheel.hue, Number(event.target.value)),
    onPointerDown: startDrag,
    onPointerUp: endDrag,
    onPointerCancel: endDrag,
  };

  const commitChannel = (channel: ColorChannel) => {
    const parsed = CHANNELS.map(name => parseChannel(draft.text[name]));
    const mixed = { red: parsed[0].channel, green: parsed[1].channel, blue: parsed[2].channel };
    const next = colorValueOf(mixed);
    const notes = { ...draft.notes, [channel]: parsed[CHANNELS.indexOf(channel)].note };
    setDraftState({ forValue: next, text: textOf(mixed), notes });
    change(next);
  };

  const numberHandlers = (channel: ColorChannel) => ({
    onChange: (event: ChangeEvent<HTMLInputElement>) =>
      setDraftState({ ...draft, text: { ...draft.text, [channel]: event.target.value } }),
    onBlur: () => commitChannel(channel),
    onKeyDown: (event: KeyboardEvent<HTMLInputElement>) => {
      if (event.key === "Enter") {
        event.preventDefault();
        commitChannel(channel);
      }
      // Arrows and Home/End edit the number here instead of moving through the popover.
      if (event.key !== "Escape" && event.key !== "Tab") event.stopPropagation();
    },
  });

  return {
    value,
    rgb,
    tab,
    pickTab,
    onTabKey,
    pickBasic: change,
    wheel: { hue: wheel.hue, lightness: wheel.lightness, ...wheelHandlers },
    lightness: lightnessHandlers,
    numbers: { channels: CHANNELS, text: draft.text, notes: draft.notes, handlers: numberHandlers },
  };
}

export type ColorPickerState = ReturnType<typeof useColorPicker>;
