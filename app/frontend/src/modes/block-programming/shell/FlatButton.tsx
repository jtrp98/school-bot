import type { ReactNode } from "react";
import { Button, type ButtonProps } from "../../../components/shared/Button";
import { Icon } from "../../../components/shared/Icons";
import { UnavailableButton } from "./UnavailableButton";

type FlatButtonProps = Omit<ButtonProps, "variant" | "size" | "icon"> & {
  icon?: ReactNode;
  tone?: "following" | "paused";
  // The Output header's buttons sit 12 from their edge; the board cards' 16.
  inset?: "tight" | "roomy";
  // Given a reason, the button is unavailable and says why (UnavailableButton).
  reason?: string;
};

// The design's flat 44px button (TerminalA2 and ConnectUnoA2): a 2px frame without the raised
// edge of the app's buttons, so a button that is switched on does not look like a key held down.
export function FlatButton({
  icon,
  tone,
  inset = "tight",
  reason,
  loading,
  onClick,
  children,
  ...rest
}: FlatButtonProps) {
  const className = [
    "inline-flex min-h-[44px] shrink-0 cursor-pointer items-center justify-center gap-[6px] rounded-[12px]",
    "border-2 border-solid py-0 font-body text-[16px] leading-[1.6] font-semibold whitespace-nowrap",
    inset === "tight" ? "px-[12px]" : "px-[16px]",
    "text-ink focus-visible:outline-[3px] focus-visible:outline-offset-2 focus-visible:outline-blue",
    "focus-visible:[outline-style:solid] aria-busy:pointer-events-none",
    "aria-disabled:cursor-not-allowed aria-disabled:border-line aria-disabled:bg-off aria-disabled:text-muted",
    tone === "following"
      ? "border-blue bg-blue-tint"
      : tone === "paused"
        ? "border-[var(--warn)] bg-[var(--warn-tint)]"
        : "border-line-strong bg-surface hover:bg-blue-tint aria-disabled:hover:bg-off",
  ].join(" ");
  const content = (
    <>
      {loading ? (
        <Icon name="loader" size={20} className="animate-[sbSpin_1s_linear_infinite] motion-reduce:animate-none" />
      ) : (
        icon
      )}
      {children}
    </>
  );
  if (reason !== undefined) {
    return (
      <UnavailableButton {...rest} variant="custom" className={className} reason={reason}>
        {content}
      </UnavailableButton>
    );
  }
  return (
    <Button {...rest} variant="custom" className={className} loading={loading} onClick={onClick}>
      {content}
    </Button>
  );
}
