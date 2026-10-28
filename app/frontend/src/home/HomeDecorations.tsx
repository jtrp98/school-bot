

import { Icon, type IconName } from "../components/shared/Icons";

const medalBaseClass = [
  "hm-medal w-[52px] h-[52px] rounded-[50%] flex items-center justify-center [&.is-got]:bg-award",
  "[&.is-got]:text-award-ink [&.is-got]:border-b-[3px] [&.is-got]:[border-bottom-style:solid]",
  "[&.is-got]:border-b-award-line [&.is-lock]:bg-sunken [&.is-lock]:text-faint [&.is-lock]:border-2",
  "[&.is-lock]:border-dashed [&.is-lock]:border-line-strong",
].join(" ");

export function Medal({ icon, got, label }: { icon: IconName; got: boolean; label: string }) {
  return (
    <span
      className={`${medalBaseClass} ${got ? "is-got" : "is-lock"}`}
      role="img"
      aria-label={label}
    >
      <Icon name={icon} size={26} />
    </span>
  );
}

export function Sparkles() {
  return (
    <>
      <span className={"hm-deco absolute z-[1] right-[70px] top-[34px] text-star"} >
        <Icon name="sparkle" size={44} />
      </span>
      <span className={"hm-deco absolute z-[1] right-[200px] top-[62px] text-orange"} >
        <Icon name="star" size={28} />
      </span>
      <span className={"hm-deco absolute z-[1] right-[40px] bottom-[70px] text-blue"} >
        <Icon name="sparkle" size={30} />
      </span>
      <span className={"hm-deco absolute z-[1] right-[160px] bottom-[48px] text-star"} >
        <Icon name="star" size={22} />
      </span>
    </>
  );
}
