import { Icon } from "./Icons";

const tagKindBaseClass = [
  "sb-tag-kind inline-flex min-h-[26px] items-center gap-[6px] rounded-full px-[10px] py-0",
  "font-body text-[13px] font-bold leading-none",
].join(" ");

function tagKindToneClass(kind: "arm" | "block" | "quiz"): string {
  return kind === "arm"
    ? "bg-blue-tint text-blue-ink"
    : kind === "block"
      ? "bg-orange-tint text-orange-deep"
      : "bg-purple-tint text-purple-ink";
}

export function TagKind({ kind }: { kind: "arm" | "block" | "quiz" }) {
  const m = { arm: ["arm", "Arm Lab"], block: ["block", "Block Lab"], quiz: ["quiz", "common.messages.quiz"] } as const;
  return (
    <span className={`${tagKindBaseClass} sb-tag-${kind} ${tagKindToneClass(kind)}`}>
      <Icon name={m[kind][0]} size={16} />
      {m[kind][1]}
    </span>
  );
}
