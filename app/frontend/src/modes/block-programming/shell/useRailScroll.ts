import { useCallback, useEffect, useRef, useState } from "react";

const prefersReducedMotion = () => window.matchMedia("(prefers-reduced-motion: reduce)").matches;
const BUTTON_CLEARANCE = 52;

// The rail has no scrollbar in the design; up/down buttons appear only while there is more to see.
export function useRailScroll() {
  const listRef = useRef<HTMLDivElement>(null);
  const [edges, setEdges] = useState({ up: false, down: false });

  const measure = useCallback(() => {
    const list = listRef.current;
    if (!list) return;
    setEdges({
      up: list.scrollTop > 1,
      down: list.scrollTop + list.clientHeight < list.scrollHeight - 1,
    });
  }, []);

  useEffect(() => {
    const list = listRef.current;
    if (!list) return;
    measure();
    const resize = new ResizeObserver(measure);
    resize.observe(list);
    list.addEventListener("scroll", measure, { passive: true });
    return () => {
      resize.disconnect();
      list.removeEventListener("scroll", measure);
    };
  }, [measure]);

  const scrollPage = useCallback((direction: 1 | -1) => {
    const list = listRef.current;
    if (!list) return;
    list.scrollBy({
      top: direction * list.clientHeight * 0.6,
      behavior: prefersReducedMotion() ? "auto" : "smooth",
    });
  }, []);

  // The chosen category always comes into view, clear of the 44px scroll buttons (+8 gap).
  const reveal = useCallback((element: HTMLElement | undefined) => {
    const list = listRef.current;
    if (!list || !element) return;
    const top = element.offsetTop;
    const bottom = top + element.offsetHeight;
    let target = list.scrollTop;
    if (top < list.scrollTop + BUTTON_CLEARANCE) target = Math.max(0, top - BUTTON_CLEARANCE);
    else if (bottom > list.scrollTop + list.clientHeight - BUTTON_CLEARANCE) {
      target = bottom - list.clientHeight + BUTTON_CLEARANCE;
    }
    list.scrollTo({ top: target, behavior: prefersReducedMotion() ? "auto" : "smooth" });
  }, []);

  return { listRef, canScrollUp: edges.up, canScrollDown: edges.down, scrollPage, reveal };
}
