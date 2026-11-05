import { useEffect, useState } from "react";
import * as Blockly from "blockly/core";
import { placedInputAt, settingsBoxAt } from "../blockly/settingsBox";

// The design opens the menu after 500 ms instead of Blockly's 750, and shows a filling ring
// from 120 ms so the child sees that holding does something.
const MENU_MS = 500;
const RING_MS = 120;
// Moving further than this before the menu opens turns the press into a drag or a scroll.
const MOVE_LIMIT_PX = 10;

export type PressRingSpot = { x: number; y: number };

// `input`: the function input a press in a settings box is on, which opens that input's menu.
type Press = { down: PointerEvent; input: Blockly.BlockSvg | null; ringTimer: number; menuTimer: number };

// Placed blocks and the empty workspace. The list drags its blocks out instead.
function opensMenu(target: EventTarget | null) {
  if (!(target instanceof Element)) return false;
  if (target.closest(".blocklyFlyout, .blocklyBubbleCanvas")) return false;
  return !!target.closest(".blocklyMainBackground, .blocklyBlockCanvas");
}

function placedInputUnder(workspace: Blockly.WorkspaceSvg, target: EventTarget | null) {
  if (!(target instanceof Element) || !target.closest(".blocklyBubbleCanvas")) return null;
  const box = settingsBoxAt(workspace, target);
  return box ? placedInputAt(box, target) : null;
}

// Blockly gives the blocks in a settings box no menu at all; the menu shows only "delete this
// input" there (contextMenu.ts).
function showInputMenu(input: Blockly.BlockSvg, event: PointerEvent) {
  input.contextMenu = true;
  input.showContextMenu(event);
}

// Press and hold on a placed block or the empty workspace, with a finger or a pen, opens the
// same menu as a right click. On an input placed in a settings box, a hold with any pointer, or a
// right click, opens its "delete this input" menu: the way to remove it without dragging.
export function useLongPressMenu(workspace: Blockly.WorkspaceSvg | null) {
  const [ring, setRing] = useState<PressRingSpot | null>(null);

  useEffect(() => {
    if (!workspace) return;
    const root = workspace.getInjectionDiv();
    let press: Press | null = null;
    const cancel = () => {
      if (!press) return;
      window.clearTimeout(press.ringTimer);
      window.clearTimeout(press.menuTimer);
      press = null;
      setRing(null);
    };
    // Blockly's own long press would open the menu again at 750 ms, and the gesture it ends
    // would otherwise go on to a click when the finger lifts.
    const open = () => {
      const held = press;
      cancel();
      Blockly.Touch.longStop();
      if (held?.input && !held.input.workspace.isDragging()) {
        held.input.workspace.cancelCurrentGesture();
        showInputMenu(held.input, held.down);
      } else if (held && !workspace.isDragging()) {
        workspace.getGesture(held.down)?.handleRightClick(held.down);
      }
    };
    const onDown = (event: PointerEvent) => {
      cancel();
      if (!event.isPrimary) return;
      const input = placedInputUnder(workspace, event.target);
      // Once Blockly has handled the right click itself.
      if (input && event.button === 2) {
        window.setTimeout(() => showInputMenu(input, event));
        return;
      }
      if (!input && (event.pointerType === "mouse" || !opensMenu(event.target))) return;
      const box = root.getBoundingClientRect();
      const spot = { x: event.clientX - box.left, y: event.clientY - box.top };
      press = {
        down: event,
        input,
        ringTimer: window.setTimeout(() => setRing(spot), RING_MS),
        menuTimer: window.setTimeout(open, MENU_MS),
      };
    };
    const onMove = (event: PointerEvent) => {
      if (!press || event.pointerId !== press.down.pointerId) return;
      const moved = Math.hypot(event.clientX - press.down.clientX, event.clientY - press.down.clientY);
      if (moved > MOVE_LIMIT_PX) cancel();
    };
    root.addEventListener("pointerdown", onDown, true);
    window.addEventListener("pointermove", onMove, true);
    window.addEventListener("pointerup", cancel, true);
    window.addEventListener("pointercancel", cancel, true);
    return () => {
      cancel();
      root.removeEventListener("pointerdown", onDown, true);
      window.removeEventListener("pointermove", onMove, true);
      window.removeEventListener("pointerup", cancel, true);
      window.removeEventListener("pointercancel", cancel, true);
    };
  }, [workspace]);

  return ring;
}
