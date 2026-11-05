import type { RingBox } from "./useProblemRing";

// The ring sits 4 outside the block, as the design's highlight does.
const GAP = 4;

export function ProblemRing({ box }: { box: RingBox | null }) {
  if (!box) return null;
  return (
    <div
      aria-hidden="true"
      className="pointer-events-none absolute z-[15] box-border rounded-[12px] border-4 border-solid border-blue"
      style={{
        left: box.left - GAP,
        top: box.top - GAP,
        width: box.width + 2 * GAP,
        height: box.height + 2 * GAP,
      }}
    />
  );
}
