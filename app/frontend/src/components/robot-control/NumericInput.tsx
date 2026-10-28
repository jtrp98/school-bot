import type { ComponentPropsWithRef } from "react";

type Props = Omit<ComponentPropsWithRef<"input">, "type" | "inputMode">;

export function NumericInput({ id, ...props }: Props) {
  return <input id={id} type="text" inputMode="numeric" {...props} />;
}
