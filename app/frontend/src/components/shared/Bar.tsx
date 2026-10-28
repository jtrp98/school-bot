
const barBaseClass = [
  "sb-bar h-[12px] rounded-[var(--r-pill)] bg-off overflow-hidden [&>i]:block [&>i]:h-full",
  "[&>i]:rounded-[var(--r-pill)] [&>i]:bg-orange [&.sb-blue>i]:bg-blue [&.sb-ok>i]:bg-ok",
].join(" ");

export function Bar({ pct, kind }: { pct: number; kind?: "blue" | "ok" }) {
  return (
    <div
      className={`${barBaseClass} ${kind ?? ""}`}
      role="progressbar"
      aria-valuenow={Math.round(pct)}
      aria-valuemin={0}
      aria-valuemax={100}
    >
      <i style={{ width: `${pct}%` }} />
    </div>
  );
}
