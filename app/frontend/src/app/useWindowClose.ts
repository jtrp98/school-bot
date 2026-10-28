import { useCallback, useEffect, useRef, useState, type KeyboardEvent } from "react";

export interface CloseActions {
  save?: () => Promise<void>;
  requestStop: () => Promise<void>;
  close: () => Promise<void>;
}

type ClosePhase = "saving" | "stopping" | "closing";
type CloseFailure = { phase: ClosePhase; cause: unknown };

export function useWindowClose(actions: CloseActions, enabled: boolean) {
  const [closeRequested, setCloseRequestedState] = useState(false);
  const [closeBusy, setCloseBusy] = useState(false);
  const [closeFailure, setCloseFailure] = useState<CloseFailure>();
  const [closePhase, setClosePhase] = useState<ClosePhase>("stopping");
  const pending = useRef(false);
  const [closeSaveRequested, setCloseSaveRequested] = useState(false);
  const closeError = closeFailure ? `entry.close.errors.${closeFailure.phase}` : "";
  const cause = closeFailure?.cause;
  const reason = cause instanceof Error ? cause.message : cause;
  const closeErrorReason = typeof reason === "string" ? reason : "";

  const setCloseRequested = useCallback((requested: boolean) => {
    if (pending.current) return;
    setCloseFailure(undefined);
    setCloseRequestedState(requested);
  }, []);

  async function closeWindow(save = false) {
    if (!enabled || pending.current) return;
    pending.current = true;
    setCloseBusy(true);
    setCloseSaveRequested(save);
    setCloseFailure(undefined);
    setCloseRequestedState(true);
    let phase: ClosePhase = save ? "saving" : "stopping";
    try {
      if (save) {
        setClosePhase("saving");
        if (!actions.save) {
          setCloseFailure({ phase, cause: undefined });
          return;
        }
        await actions.save();
      }
      phase = "stopping";
      setClosePhase(phase);
      await actions.requestStop();
      phase = "closing";
      setClosePhase(phase);
      await actions.close();
      setCloseRequestedState(false);
    } catch (cause) {
      setCloseFailure({ phase, cause });
    } finally {
      pending.current = false;
      setCloseBusy(false);
    }
  }

  return {
    closeRequested, setCloseRequested, closeBusy, closeError, closeErrorReason, closePhase, closeWindow,
    closeSaveAvailable: !!actions.save, closeSaveRequested,
  };
}

export function useCloseDialog(busy: boolean) {
  const panel = useRef<HTMLDivElement>(null);
  useEffect(() => {
    const previous = document.activeElement;
    return () => { if (previous instanceof HTMLElement && previous.isConnected) previous.focus(); };
  }, []);
  useEffect(() => {
    const button = panel.current?.querySelector<HTMLButtonElement>("button:not(:disabled)");
    (button ?? panel.current)?.focus();
  }, [busy]);

  function onKeyDown(event: KeyboardEvent<HTMLDivElement>) {
    if (event.key !== "Tab") return;
    const buttons = [...(panel.current?.querySelectorAll<HTMLButtonElement>("button:not(:disabled)") ?? [])];
    const first = buttons[0];
    const last = buttons.at(-1);
    if (!first) { event.preventDefault(); return; }
    if (event.shiftKey && document.activeElement === first) { event.preventDefault(); last?.focus(); }
    else if (!event.shiftKey && document.activeElement === last) { event.preventDefault(); first.focus(); }
  }

  return { panel, onKeyDown };
}
