import type { CSSProperties } from "react";

export function TimedProgressBar({ ms, hold, pct }: { ms: number; hold: boolean; pct: number }) {
  const style: CSSProperties = hold ? { width: `${pct}%` } : { animationDuration: `${ms}ms` };
  return <div className={hold ? "cn-bar is-hold" : "cn-bar"} style={style} />;
}

