import { useTranslation } from "react-i18next";
import { Icon } from "../../../components/shared/Icons";
import type { DeleteSignal } from "./useDropToDelete";

// The tag sits up and to the right of the pointer, clear of a finger and the cursor's arrow, and
// is kept inside the window. Its size is the design's Thai tag.
const TAG_OFFSET = { x: 16, y: -52 };
const TAG_SIZE = { width: 176, height: 44 };
const EDGE = 8;
const FRAME_GAP = 6;

const clamp = (value: number, low: number, high: number) => Math.max(low, Math.min(high, value));

export function DeleteSignalLayer({ signal }: { signal: DeleteSignal | null }) {
  const { t } = useTranslation();
  if (!signal) return null;
  const { pointer, box } = signal;
  const tagLeft = clamp(pointer.x + TAG_OFFSET.x, EDGE, window.innerWidth - TAG_SIZE.width);
  const tagTop = clamp(pointer.y + TAG_OFFSET.y, EDGE, window.innerHeight - TAG_SIZE.height);
  return (
    <div aria-hidden="true" className="pointer-events-none fixed inset-0 z-[80]">
      <div
        className="absolute box-border rounded-[12px] border-[3px] border-dashed border-err"
        style={{
          left: box.left - FRAME_GAP,
          top: box.top - FRAME_GAP,
          width: box.width + 2 * FRAME_GAP,
          height: box.height + 2 * FRAME_GAP,
        }}
      />
      <div
        className={[
          "absolute box-border inline-flex h-[36px] items-center gap-[8px] rounded-[12px] bg-err px-[12px]",
          "font-body text-[16px] leading-none font-bold whitespace-nowrap text-white",
          "shadow-[0_8px_20px_rgba(20,36,61,.2)]",
        ].join(" ")}
        style={{ left: tagLeft, top: tagTop }}
      >
        <Icon name="trash" size={20} />
        {t("blockLab.workspace.releaseToDelete")}
      </div>
    </div>
  );
}
