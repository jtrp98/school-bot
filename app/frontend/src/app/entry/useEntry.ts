import { useCallback, useEffect, useRef, useState } from "react";
import { allowsEntry, type EntryAdapter, type PublicEntitlement, type LoginResult, type EntryFeedback } from "./types";

export function useEntry(adapter: EntryAdapter) {
  const [status, setStatus] = useState<PublicEntitlement>({ status: "checking" });
  const [entered, setEntered] = useState(false);
  const [hasEntered, setHasEntered] = useState(false);
  const [account, setAccount] = useState<string>();
  const [busy, setBusy] = useState(false);
  const [recovering, setRecovering] = useState(false);
  const [error, setError] = useState<EntryFeedback | "">("");
  const pending = useRef(false);
  const generation = useRef(0);
  const current = useRef(status);
  const admitted = useRef(false);
  const live = useRef(true);

  useEffect(() => {
    live.current = true;
    let active = true;
    let revision = 0;
    setStatus({ status: "checking" });
    current.current = { status: "checking" };
    setEntered(false);
    setError("");
    setHasEntered(false);
    admitted.current = false;
    const receive = (next: PublicEntitlement) => {
      if (!active) return;
      revision++;
      current.current = next;
      setStatus(next);
      if (!allowsEntry(next)) {
        generation.current++;
        if (admitted.current) {
          admitted.current = false;
          setEntered(false);
          setRecovering(true);
          const recovery = adapter.saveAndRequestStop;
          void Promise.resolve().then(() => {
            if (!recovery) throw new Error("unavailable");
            return recovery();
          }).catch(() => {
            if (active) setError("recovery_failed");
          }).finally(() => {
            if (active) setRecovering(false);
          });
        }
      }
    };
    const startRevision = revision;
    const unsubscribe = adapter.subscribe(receive);
    void adapter.readStatus().then(next => {
      if (revision === startRevision) receive(next);
    }).catch(() => {
      if (revision === startRevision) receive({ status: "unavailable" });
    });
    return () => {
      active = false;
      live.current = false;
      generation.current++;
      unsubscribe();
    };
  }, [adapter]);

  const perform = useCallback(async (action: () => Promise<void>) => {
    if (pending.current) return;
    pending.current = true;
    setBusy(true);
    setError("");
    const request = generation.current;
    try { await action(); }
    catch {
      if (live.current && request === generation.current) setError("operation_failed");
    }
    finally {
      pending.current = false;
      if (live.current) setBusy(false);
    }
  }, []);

  const enter = useCallback((username?: string, password?: string) => perform(async () => {
    if (!allowsEntry(current.current) || recovering) return;
    const request = generation.current;
    let result: LoginResult;
    if (username === undefined) {
      await adapter.guest();
      result = "success";
    } else result = await adapter.login?.(username, password ?? "") ?? "unavailable";
    if (!live.current || request !== generation.current || !allowsEntry(current.current)) return;
    if (result !== "success") { setError(result); return; }
    admitted.current = true;
    setAccount(username);
    setHasEntered(true);
    setEntered(true);
  }), [adapter, perform, recovering]);

  const activate = useCallback((key: string) => perform(async () => {
    if (!key || key.length > 256 || /\s/.test(key)) return;
    await adapter.activate(key);
  }), [adapter, perform]);
  const validate = useCallback(() => perform(adapter.validate), [adapter, perform]);
  const useOffline = useCallback(() => perform(async () => {
    if (current.current.status !== "checking" || !current.current.canUseOffline) return;
    await adapter.useOffline?.();
  }), [adapter, perform]);
  const guest = useCallback(() => enter(), [enter]);
  const login = useCallback((username: string, password: string) => enter(username, password), [enter]);
  const retryRecovery = useCallback(() => perform(async () => {
    setRecovering(true);
    try {
      if (!adapter.saveAndRequestStop) throw new Error("unavailable");
      await adapter.saveAndRequestStop();
    } catch {
      if (live.current) setError("recovery_failed");
    } finally {
      if (live.current) setRecovering(false);
    }
  }), [adapter, perform]);

  return {
    status, entered, hasEntered, account, busy: busy || recovering, recovering, error,
    activate, validate, useOffline, guest, login, retryRecovery,
  };
}
