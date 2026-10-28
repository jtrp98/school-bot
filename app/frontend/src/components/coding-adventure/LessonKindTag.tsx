import { t as translate } from "../../i18n";
import { Icon } from "../shared/Icons";
import type { Stage } from "../../modes/coding-adventure/LearnMap";

const kindTagBaseClass = [
  "sb-tag-kind lm-tag inline-flex min-h-[36px] items-center gap-[6px] rounded-full border-2",
  "border-solid px-[14px] py-0 font-body text-[15px] font-bold leading-none",
].join(" ");

function kindToneClass(area: Stage["area"]): string {
  return area === "arm"
    ? "border-blue-line bg-blue-tint text-blue-ink"
    : area === "block"
      ? "border-orange-line bg-orange-tint text-orange-deep"
      : "border-purple-line bg-purple-tint text-purple-ink";
}

export function KindTag({ s }: { s: Stage }) {
  return (
    <span className={`${kindTagBaseClass} sb-tag-${s.area} ${kindToneClass(s.area)}`}>
      <Icon name={s.ic} size={18} />
      {translate(s.kind)}
    </span>
  );
}
