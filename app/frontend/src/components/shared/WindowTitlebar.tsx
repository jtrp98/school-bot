import { t as translate } from "../../i18n";
import { client } from "../../core/client";
import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useLayoutEffect,
  useMemo,
  useRef,
  useState,
  type ReactNode,
} from "react";
import { createPortal } from "react-dom";


type Props = { desktop: boolean; preview: boolean; escapeBlocked: boolean; onClose: () => void; children: ReactNode };
type ChromeContext = { enabled: boolean; registerSlot: (key: symbol, node: HTMLDivElement | null) => void };
const Chrome = createContext<ChromeContext | null>(null);

const windowControlsSlotClass = [
  "window-controls-slot w-[144px] min-w-[144px] h-[48px] [flex:none] [align-self:center]",
  "pointer-events-none [--wails-draggable:no-drag]",
].join(" ");
const captionControlsBaseClass = [
  "window-caption-controls fixed z-[10000] h-[48px] flex [align-items:stretch] text-muted",
  "[--wails-draggable:no-drag] [&.is-fallback]:bg-surface [&.is-fallback]:rounded-[0_0_0_12px]",
  "[&.is-dark]:text-white [&_button]:flex [&_button]:items-center [&_button]:justify-center",
  "[&_button]:h-[48px] [&_button]:min-h-[48px] [&_button]:w-[48px] [&_button]:p-0 [&_button]:m-0",
  "[&_button]:border-[0] [&_button]:border-none [&_button]:border-current [&_button]:rounded-none",
  "[&_button]:bg-transparent [&_button]:text-[inherit] [&_button]:shadow-none",
  "[&_button]:[transition:none] [&_button]:[--wails-draggable:no-drag]",
  "[&_button:hover:not(:disabled)]:bg-sunken [&_button:hover:not(:disabled)]:text-ink",
  "[&_button:active:not(:disabled)]:[transform:none]",
  "[&_.window-caption-close:hover:not(:disabled)]:bg-err",
  "[&_.window-caption-close:hover:not(:disabled)]:text-surface",
  "[&_button:focus-visible]:[outline:2px_solid_var(--blue)]",
  "[&_button:focus-visible]:[outline-offset:-4px] [&_button:disabled]:opacity-[0.4]",
].join(" ");
const captionErrorClass = [
  "window-caption-error absolute top-[52px] right-[8px] w-[max-content] max-w-[min(600px,_90vw)]",
  "py-[8px] px-[16px] rounded-[12px] bg-surface border-1 border-solid border-err-line text-err-ink",
  "not-italic [font-variant:normal] font-medium [font-stretch:normal] text-[14px] leading-[28px]",
  "font-body",
].join(" ");

export function WindowControlsSlot() {
  const chrome = useContext(Chrome);
  const key = useRef(Symbol("window-controls-slot")).current;
  const register = chrome?.registerSlot;
  const ref = useCallback((node: HTMLDivElement | null) => register?.(key, node), [key, register]);
  return chrome?.enabled ? (
    <div ref={ref} className={windowControlsSlotClass} aria-hidden="true" />
  ) : null;
}
function hasEscapeOwner(event: KeyboardEvent) {
  const target = event.target instanceof Element ? event.target : document.activeElement;
  if (target?.closest("input, textarea, select, [contenteditable=true], [role=textbox]")) return true;
  // Keep Escape available for Arm Lab's stop/cancel handler.
  if (document.querySelector(".aw-host")) return true;
  return [...document.querySelectorAll(
    "[role=dialog], [role=alertdialog], [role=menu], [role=listbox], [popover]",
  )].some(
    el => el.getClientRects().length > 0 && el.getAttribute("aria-hidden") !== "true",
  );
}
export default function WindowChromeProvider({ desktop, preview, escapeBlocked, onClose, children }: Props) {
  const enabled = desktop || preview;
  const [slots, setSlots] = useState(new Map<symbol, HTMLDivElement>());
  const registerSlot = useCallback((key: symbol, node: HTMLDivElement | null) => {
    setSlots(current => {
      if (node ? current.get(key) === node : !current.has(key)) return current;
      const next = new Map(current);
      if (node) next.set(key, node);
      else next.delete(key);
      return next;
    });
  }, []);
  const context = useMemo(() => ({ enabled, registerSlot }), [enabled, registerSlot]);
  const slot = [...slots.values()].at(-1);
  const [placement, setPlacement] = useState({ top: 0, right: 0, dark: false });
  useLayoutEffect(() => {
    if (!enabled || !slot) { setPlacement({ top: 0, right: 0, dark: false }); return; }
    const sync = () => {
      const rect = slot.getBoundingClientRect();
      const next = {
        top: Math.max(0, rect.top),
        right: Math.max(0, window.innerWidth - rect.right),
        dark: !!slot.closest(".tm-head"),
      };
      setPlacement(current => (
        current.top === next.top && current.right === next.right && current.dark === next.dark ? current : next
      ));
    };
    sync();
    const observer = new ResizeObserver(sync);
    observer.observe(slot);
    window.addEventListener("resize", sync);
    window.addEventListener("scroll", sync, true);
    return () => {
      observer.disconnect();
      window.removeEventListener("resize", sync);
      window.removeEventListener("scroll", sync, true);
    };
  }, [enabled, slot]);
  const [fullscreen, setFullscreen] = useState(true);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState("");
  const live = useRef({ desktop, escapeBlocked }); live.current = { desktop, escapeBlocked };
  const pending = useRef(false);
  const update = useCallback(async () => {
    if (!live.current.desktop) return;
    setFullscreen(await client.isFullscreen());
  }, []);
  const toggle = useCallback(async (exitOnly = false) => {
    if (!live.current.desktop || pending.current) return;
    pending.current = true;
    setBusy(true);
    setError("");
    try {
      const current = await client.isFullscreen();
      if (current) await client.setFullscreen(false);
      else if (!exitOnly) await client.setFullscreen(true);
      await update();
    } catch (e) {
      setError(translate("shell.messages.fullscreenToggleFailed", { reason: String(e) }));
    } finally {
      pending.current = false;
      setBusy(false);
    }
  }, [update]);
  useEffect(() => {
    if (!desktop) return;
    const resized = () => {
      void update().catch(error => console.warn("Unable to refresh fullscreen state", error));
    };
    resized();
    const onKey = (event: KeyboardEvent) => {
      if (event.key === "F11") {
        event.preventDefault();
        if (!event.repeat) void toggle();
        return;
      }
      if (
        event.key !== "Escape" ||
        event.repeat ||
        event.defaultPrevented ||
        live.current.escapeBlocked ||
        hasEscapeOwner(event)
      ) return;
      queueMicrotask(() => { if (!event.defaultPrevented && !event.cancelBubble) void toggle(true); });
    };
    window.addEventListener("resize", resized);
    window.addEventListener("keydown", onKey);
    return () => {
      window.removeEventListener("resize", resized);
      window.removeEventListener("keydown", onKey);
    };
  }, [desktop, update, toggle]);
  const minimize = async () => {
    if (!desktop) return;
    try {
      await client.minimizeWindow();
    }
    catch (e) { setError(translate("shell.messages.minimizeFailed", { reason: String(e) })); }
  };
  // Render above modal stacking contexts while visually occupying the registered header slot.
  const captionControlsClass =
    `${captionControlsBaseClass}${slot ? "" : " is-fallback"}${placement.dark ? " is-dark" : ""}`;
  const controls = (
    <div
      className={captionControlsClass}
      style={{ top: placement.top, right: placement.right }}
      role="group"
      aria-label={translate("common.messages.windowControls")}
    >
      <button
        type="button"
        className="disabled:opacity-40"
        disabled={!desktop || busy}
        aria-label={translate("common.messages.minimize")}
        title={translate("common.messages.minimize")}
        onClick={() => void minimize()}
      >
        <svg
          width="18"
          height="18"
          viewBox="0 0 24 24"
          fill="none"
          stroke="currentColor"
          strokeWidth="1.6"
          aria-hidden="true"
        >
          <path d="M5 16h14" />
        </svg>
      </button>
      <button
        type="button"
        className="disabled:opacity-40"
        disabled={!desktop || busy}
        aria-label={fullscreen ? translate("common.messages.exitFullScreen") : translate("common.messages.fullScreen")}
        title={fullscreen ? translate("common.messages.exitFullScreenF11") : translate("common.messages.fullScreenF11")}
        onClick={() => void toggle()}
      >
        <svg
          width="18"
          height="18"
          viewBox="0 0 24 24"
          fill="none"
          stroke="currentColor"
          strokeWidth="1.6"
          aria-hidden="true"
        >
          {fullscreen ? (
            <path d="M9 3v6H3m18 0h-6V3M3 15h6v6m6 0v-6h6" />
          ) : (
            <path d="M3 9V3h6m6 0h6v6M3 15v6h6m6 0h6v-6" />
          )}
        </svg>
      </button>
      <button
        type="button"
        className="window-caption-close disabled:opacity-40"
        disabled={!desktop}
        aria-label={translate("window.close")}
        title={translate("window.close")}
        onClick={onClose}
      >
        <svg
          width="18"
          height="18"
          viewBox="0 0 24 24"
          fill="none"
          stroke="currentColor"
          strokeWidth="1.6"
          aria-hidden="true"
        >
          <path d="m6 6 12 12M18 6 6 18" />
        </svg>
      </button>
      {error && <div className={captionErrorClass} role="status">{translate(error)}</div>}
    </div>
  );
  return (
    <Chrome.Provider value={context}>
      {children}
      {enabled && typeof document !== "undefined" && createPortal(controls, document.body)}
    </Chrome.Provider>
  );
}
