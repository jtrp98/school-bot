import { Button, type ButtonProps } from "../../../../components/shared/Button";
import type { ReactNode } from "react";

type ToolbarButtonHeight = 30 | 36 | 40;

// The shared Button's smallest size is 44px tall; the sequence toolbar needs compact rows, so only its box is overridden.
const HEIGHT_CLASS: Record<ToolbarButtonHeight, string> = {
  30: "h-[30px]! min-h-0! shrink-0 gap-[6px]! rounded-[9px]! border-b-4! px-[10px]! text-[13px]!",
  36: "h-[36px]! min-h-0! shrink-0 gap-[6px]! rounded-[10px]! px-[12px]! text-[13px]!",
  40: "h-[40px]! min-h-0! shrink-0 gap-[6px]! rounded-[12px]! px-[14px]! text-[14px]!",
};

export function ToolbarButton({
  height = 36,
  className,
  ...rest
}: Omit<ButtonProps, "size"> & { height?: ToolbarButtonHeight }): ReactNode {
  return <Button size="sm" {...rest} className={`${HEIGHT_CLASS[height]} ${className ?? ""}`} />;
}
