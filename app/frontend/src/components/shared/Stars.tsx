import { t as translate } from "../../i18n";
import { Icon } from "./Icons";

export function Stars({
  n,
  total = 3,
  px = 20,
}: {
  n: number;
  total?: number;
  px?: number;
}) {
  return (
    <span className={"sb-stars inline-flex gap-[2px]"} role="img" aria-label={translate("common.messages.value0OfValue1Stars", {value0: n, value1: total})}>
      {Array.from({ length: total }, (_, i) => (
        <Icon
          key={i}
          name={i < n ? "star" : "starO"}
          size={px}

          className={i < n ? "sb-star-on text-star" : "sb-star-off text-faint"}
        />
      ))}
    </span>
  );
}
