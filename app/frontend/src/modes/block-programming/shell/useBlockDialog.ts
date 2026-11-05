import { useEffect, type KeyboardEvent, type RefObject } from "react";
import * as Blockly from "blockly/core";
import { useCloseDialog } from "../../../app/useWindowClose";

// A dialog over the workspace: Tab stays inside and focus goes back to the opener, as in the
// app's close dialog, and Esc takes the safe way out. It also holds Blockly's ephemeral focus
// while open, so Blockly's keyboard shortcuts (delete, copy) cannot act on a block behind it.
// `start` is the safe choice, focused first.
export function useBlockDialog(start: RefObject<HTMLButtonElement | null>, onEscape: () => void) {
  const dialog = useCloseDialog(false);
  const { panel } = dialog;
  useEffect(() => {
    const focus = Blockly.getFocusManager();
    const giveBack = panel.current && !focus.ephemeralFocusTaken() ? focus.takeEphemeralFocus(panel.current) : null;
    start.current?.focus();
    return () => giveBack?.(false);
  }, [panel, start]);
  const onKeyDown = (event: KeyboardEvent<HTMLDivElement>) => {
    if (event.key !== "Escape") {
      dialog.onKeyDown(event);
      return;
    }
    event.preventDefault();
    event.stopPropagation();
    onEscape();
  };
  return { panel, onKeyDown };
}
