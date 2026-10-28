import type { PointerEventHandler } from "react";
import { Button, type ButtonProps } from "../shared/Button";

type Props = Omit<ButtonProps, "type" | "onPointerUp" | "onPointerLeave" | "onPointerCancel"> & {
  onHoldEnd: PointerEventHandler<HTMLButtonElement>;
};

export function HoldButton({ onHoldEnd, ...props }: Props) {
  return <Button variant="secondary" {...props} onPointerUp={onHoldEnd} onPointerLeave={onHoldEnd} onPointerCancel={onHoldEnd} />;
}
