import { useCallback, useEffect, useRef, useState } from "react";

// A popover in the window bar or the terminal: a press anywhere outside closes it; Esc or a close
// from inside it gives focus back to what opened it (the button, or the field that was typed in).
export function usePopover() {
  const [open, setOpen] = useState(false);
  const root = useRef<HTMLDivElement>(null);
  const trigger = useRef<HTMLButtonElement>(null);
  const opener = useRef<HTMLElement | null>(null);
  const giveFocusBack = () => (opener.current ?? trigger.current)?.focus();
  const remember = () => {
    opener.current = document.activeElement instanceof HTMLElement ? document.activeElement : null;
  };

  useEffect(() => {
    if (!open) return;
    const onPointerDown = (event: PointerEvent) => {
      if (!root.current?.contains(event.target as Node)) setOpen(false);
    };
    const onKeyDown = (event: KeyboardEvent) => {
      if (event.key !== "Escape") return;
      event.stopPropagation();
      setOpen(false);
      giveFocusBack();
    };
    document.addEventListener("pointerdown", onPointerDown, true);
    document.addEventListener("keydown", onKeyDown, true);
    return () => {
      document.removeEventListener("pointerdown", onPointerDown, true);
      document.removeEventListener("keydown", onKeyDown, true);
    };
  }, [open]);

  const toggle = useCallback(() => {
    remember();
    setOpen(current => !current);
  }, []);
  const show = useCallback(() => {
    remember();
    setOpen(true);
  }, []);
  const close = useCallback(() => {
    setOpen(false);
    giveFocusBack();
  }, []);
  return { open, toggle, show, close, root, trigger };
}
