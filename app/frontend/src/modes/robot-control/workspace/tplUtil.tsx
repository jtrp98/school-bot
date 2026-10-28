import type { ReactNode } from "react";
import armPhoto from "../../../assets/arm-photo.jpg";
import armLabLogo from "../../../assets/logo-arm-lab.svg";
import schoolBotLogo from "../../../assets/logo-school-bot.svg";

export const IMG = { armPhoto, armLab: armLabLogo, schoolBot: schoolBotLogo };

export const T = (value: unknown): ReactNode => {
  if (value === undefined || value === null || typeof value === "boolean") return null;
  return <span className="sc-interp">{String(value)}</span>;
};

export const arr = <A,>(x: A[] | undefined | null): A[] => (Array.isArray(x) ? x : []);

type Styled = { ref: (el: HTMLElement | null) => void };
const lastCss = new WeakMap<HTMLElement, string>();
const cache = new Map<string, (el: HTMLElement | null) => void>();

// cssText preserves shorthand/longhand order and !important until the remaining callers use TSX styles.
export function sx(css: string | undefined | null): Styled {
  const text = css ?? "";
  let fn = cache.get(text);
  if (!fn) {
    fn = (el) => {
      if (!el || lastCss.get(el) === text) return;
      lastCss.set(el, text);
      el.style.cssText = text;
    };
    if (cache.size > 800) cache.clear();
    cache.set(text, fn);
  }
  return { ref: fn };
}
