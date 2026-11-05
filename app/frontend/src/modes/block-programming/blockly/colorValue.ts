// What the colour field stores: one of the generator's colour names, or a colour the child mixed
// on the wheel or by numbers as #RRGGBB in capitals (engines/mixly/docs/API.md).
export type Rgb = { red: number; green: number; blue: number };

export const NAMED_COLORS: Readonly<Record<string, string>> = {
  RED: "#FF0000",
  GREEN: "#00FF00",
  BLUE: "#0000FF",
  YELLOW: "#FFFF00",
  CYAN: "#00FFFF",
  MAGENTA: "#FF00FF",
  WHITE: "#FFFFFF",
  OFF: "#000000",
};

const MIXED = /^#[0-9A-F]{6}$/;

export const isMixedColor = (value: string) => MIXED.test(value);

export function hexOf(value: string | null) {
  if (value === null) return null;
  return NAMED_COLORS[value] ?? (isMixedColor(value) ? value : null);
}

export function rgbOf(value: string | null): Rgb {
  const hex = hexOf(value) ?? "#000000";
  return {
    red: parseInt(hex.slice(1, 3), 16),
    green: parseInt(hex.slice(3, 5), 16),
    blue: parseInt(hex.slice(5, 7), 16),
  };
}

const twoDigits = (channel: number) => channel.toString(16).padStart(2, "0").toUpperCase();

// A mix that lands on a named colour is stored by name, so the basic tab shows it as picked.
export function colorValueOf({ red, green, blue }: Rgb) {
  const hex = `#${twoDigits(red)}${twoDigits(green)}${twoDigits(blue)}`;
  return Object.keys(NAMED_COLORS).find(name => NAMED_COLORS[name] === hex) ?? hex;
}

// The wheel works in HSL at full saturation: hue 0–359 degrees, lightness 0–100 %.
export function rgbFromHsl(hue: number, lightness: number): Rgb {
  const level = lightness / 100;
  const chroma = 1 - Math.abs(2 * level - 1);
  const second = chroma * (1 - Math.abs(((hue / 60) % 2) - 1));
  const sector = Math.floor(hue / 60) % 6;
  const [red, green, blue] = [
    [chroma, second, 0],
    [second, chroma, 0],
    [0, chroma, second],
    [0, second, chroma],
    [second, 0, chroma],
    [chroma, 0, second],
  ][sector];
  const offset = level - chroma / 2;
  const channel = (part: number) => Math.max(0, Math.min(255, Math.round((part + offset) * 255)));
  return { red: channel(red), green: channel(green), blue: channel(blue) };
}

export function hslFromRgb({ red, green, blue }: Rgb) {
  const [r, g, b] = [red / 255, green / 255, blue / 255];
  const max = Math.max(r, g, b);
  const min = Math.min(r, g, b);
  const spread = max - min;
  const lightness = (max + min) / 2;
  let hue = 0;
  if (spread > 0) {
    if (max === r) hue = ((g - b) / spread) % 6;
    else if (max === g) hue = (b - r) / spread + 2;
    else hue = (r - g) / spread + 4;
    hue = (Math.round(hue * 60) + 360) % 360;
  }
  const saturation = spread === 0 ? 0 : spread / (1 - Math.abs(2 * lightness - 1));
  return { hue, saturation, lightness: Math.round(lightness * 100) };
}

// The design names the hue in twelve 30° steps, starting at red.
export const HUE_NAMES = [
  "red", "orange", "yellow", "lime", "green", "mint", "cyan", "azure", "blue", "purple", "magenta", "pink",
] as const;

export const hueNameOf = (hue: number) => HUE_NAMES[Math.round(hue / 30) % 12];
