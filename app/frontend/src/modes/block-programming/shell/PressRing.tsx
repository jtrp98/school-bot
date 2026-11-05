import type { PressRingSpot } from "./useLongPressMenu";

// The ring that fills under a finger held on the workspace, until the menu opens. Reduced motion
// shows it full at once.
export function PressRing({ at }: { at: PressRingSpot | null }) {
  if (!at) return null;
  return (
    <svg
      aria-hidden="true"
      width="80"
      height="80"
      viewBox="0 0 80 80"
      className="pointer-events-none absolute z-[75]"
      style={{ left: at.x - 40, top: at.y - 40 }}
    >
      <circle cx="40" cy="40" r="32" fill="none" className="stroke-white" strokeOpacity={0.9} strokeWidth={11} />
      <circle cx="40" cy="40" r="32" fill="none" className="stroke-ink" strokeOpacity={0.25} strokeWidth={6} />
      <circle
        cx="40"
        cy="40"
        r="32"
        fill="none"
        className="stroke-ink [stroke-dasharray:201.06] motion-safe:animate-[block-lab-press-ring_.38s_linear_forwards]"
        strokeWidth={6}
        strokeLinecap="round"
        transform="rotate(-90 40 40)"
      />
    </svg>
  );
}
