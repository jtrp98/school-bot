import type { ReactNode } from "react";
import { T } from "../tplUtil";
import type { SequenceToolbarView } from "../workspaceToolbarFields";
import { ToolbarButton } from "./ToolbarButton";

export function TableToggleButton({
  toggle,
  onToggle,
  describeState,
}: {
  toggle: SequenceToolbarView["tableToggle"];
  onToggle: () => void;
  describeState: boolean;
}): ReactNode {
  return (
    <ToolbarButton
      variant="secondary"
      onClick={onToggle}
      aria-pressed={describeState ? toggle.pressed : undefined}
      aria-label={toggle.label}
      title={describeState ? toggle.label : undefined}
    >
      <svg
        width="14"
        height="14"
        viewBox="0 0 24 24"
        fill="none"
        stroke="currentColor"
        strokeWidth="2.4"
        strokeLinecap="round"
        strokeLinejoin="round"
        aria-hidden="true"
        className="mr-[6px] inline-block align-[-.15em]"
      >
        <path d={toggle.iconPath} />
      </svg>
      {T(toggle.text)}
    </ToolbarButton>
  );
}
