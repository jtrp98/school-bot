import { useId } from "react";
import { Button, type ButtonProps } from "../../../components/shared/Button";

// aria-disabled keeps the button focusable so keyboard and screen reader users can still reach
// the reason, which a disabled button would hide.
export function UnavailableButton({ reason, children, ...rest }: Omit<ButtonProps, "onClick"> & { reason: string }) {
  const reasonId = useId();
  return (
    <>
      <Button {...rest} aria-disabled="true" aria-describedby={reasonId} title={reason}>
        {children}
      </Button>
      <span id={reasonId} hidden>
        {reason}
      </span>
    </>
  );
}
