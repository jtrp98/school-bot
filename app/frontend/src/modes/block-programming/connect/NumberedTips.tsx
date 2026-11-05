export function NumberedTips({ tips, look }: { tips: string[]; look: "card" | "plain" }) {
  return (
    <div
      className={[
        "grid grid-cols-3 gap-[12px]",
        look === "plain" ? "border-0 border-t border-solid border-line pt-[12px]" : "",
      ].join(" ")}
    >
      {tips.map((tip, index) => (
        <div
          key={tip}
          className={[
            "flex items-start gap-[12px] text-[16px] leading-[1.6]",
            look === "card" ? "rounded-[16px] border-2 border-solid border-line bg-surface p-[16px]" : "",
          ].join(" ")}
        >
          <span
            className={[
              "box-border flex h-[32px] w-[32px] shrink-0 items-center justify-center rounded-full border-2",
              "border-solid border-line bg-sunken font-bold",
            ].join(" ")}
          >
            {index + 1}
          </span>
          <span>{tip}</span>
        </div>
      ))}
    </div>
  );
}
