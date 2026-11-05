import { useCallback, useEffect, useState } from "react";
import type { BoardDevice } from "./boardDevices";
import { advanceUnplugSearch, startUnplugSearch, type UnplugSearch } from "./unplugSearch";

// After this long without the board coming back, the box asks whether the cable is in yet.
const SLOW_RETURN_MS = 15000;

export function useUnplugSearch(boards: readonly BoardDevice[]) {
  const [search, setSearch] = useState<UnplugSearch | null>(null);
  const [slow, setSlow] = useState(false);
  const [tips, setTips] = useState(false);

  useEffect(() => {
    setSearch(current => current && advanceUnplugSearch(current, boards));
  }, [boards]);

  const waitingForReturn = search?.step === "plugBack";
  useEffect(() => {
    if (!waitingForReturn) return;
    const timer = window.setTimeout(() => setSlow(true), SLOW_RETURN_MS);
    return () => window.clearTimeout(timer);
  }, [waitingForReturn]);

  const start = useCallback(() => {
    setSearch(startUnplugSearch(boards));
    setSlow(false);
    setTips(false);
  }, [boards]);

  const cancel = useCallback(() => {
    setSearch(null);
    setSlow(false);
    setTips(false);
  }, []);

  const toggleTips = useCallback(() => setTips(shown => !shown), []);

  return {
    state: search,
    slow: waitingForReturn && slow,
    tips,
    start,
    cancel,
    toggleTips,
  };
}
