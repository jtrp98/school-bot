import { useEffect, useState } from "react";
import * as Blockly from "blockly/core";
import { OutsideDeleteArea } from "../blockly/outsideDeleteArea";
import { watchSettingsBoxes } from "../blockly/settingsBox";

// Turns off the fade of a block that Blockly marks for deleting (blockLab.css), for a block just
// taken from the list that has not been over the workspace yet.
const QUIET_CLASS = "sbQuietDelete";
// Lets Blockly's drag surface out of the workspace's box (blockLab.css).
const LIFT_CLASS = "sbLifted";
// Blockly's shrink effect for a deleted block plays on after the drop.
const LIFT_AFTER_DROP_MS = 260;

type Box = { left: number; top: number; width: number; height: number };
// Window coordinates: the signal is drawn above the whole window, past the blocks panel.
export type DeleteSignal = { pointer: { x: number; y: number }; box: Box };

// While a placed block is dragged where letting go deletes it (the list, or anywhere outside the
// workspace; for a function input, outside the white part of its settings box), it gets a red
// dashed frame and a "let go to delete" tag beside the pointer, so the deletion is not told by
// fading alone. A block just taken from the list shows nothing until it has been over the
// workspace once: letting go of it before then only means not adding it. While any block is
// dragged, `lifted` lets it float over the window bar and the drawer, as in the design.
export function useDropToDelete(workspace: Blockly.WorkspaceSvg | null) {
  const [signal, setSignal] = useState<DeleteSignal | null>(null);
  const [newFromList, setNewFromList] = useState(false);
  const [lifted, setLifted] = useState(false);

  useEffect(() => {
    if (!workspace) return;
    const root = workspace.getInjectionDiv();
    const area = new OutsideDeleteArea(workspace, () => root);
    area.register();
    // One more area for each settings box while it is open; the box's workspace goes when it closes.
    const boxAreas = new Set<OutsideDeleteArea>();
    const allAreas = () => [area, ...boxAreas];
    const stopWatchingBoxes = watchSettingsBoxes(workspace, (_functionId, box) => {
      const boxArea = new OutsideDeleteArea(box, () => box.getParentSvg());
      boxArea.register();
      boxAreas.add(boxArea);
      return () => {
        boxArea.unregister();
        boxAreas.delete(boxArea);
      };
    });
    let liftTimer = 0;
    const lift = (on: boolean) => {
      window.clearTimeout(liftTimer);
      root.classList.toggle(LIFT_CLASS, on);
      setLifted(on);
    };
    let fromList = false;
    let overWorkspace = false;
    const onDown = (event: PointerEvent) => {
      for (const each of allAreas()) each.cancelled = false;
      fromList = event.target instanceof Element && !!event.target.closest(".blocklyFlyout");
      overWorkspace = false;
    };
    // Blockly stops a drag's moves from going further, so they are caught on the way down and
    // read in the next frame, once Blockly has set the block's delete style for that move.
    let pointer: { x: number; y: number } | null = null;
    let frame = 0;
    const onMove = (event: PointerEvent) => {
      pointer = { x: event.clientX, y: event.clientY };
      if (!frame) frame = window.requestAnimationFrame(readDrag);
    };
    const readDrag = () => {
      frame = 0;
      // In the workspace or in a settings box, which has its own drags.
      const dragged = pointer ? root.querySelector(".blocklyDragging") : null;
      if (!dragged) {
        setSignal(null);
        return;
      }
      if (!root.classList.contains(LIFT_CLASS)) lift(true);
      const deleting = dragged.classList.contains("blocklyDraggingDelete");
      if (!deleting) overWorkspace = true;
      const quiet = fromList && !overWorkspace;
      root.classList.toggle(QUIET_CLASS, quiet);
      setNewFromList(quiet);
      if (!deleting || quiet) {
        setSignal(null);
        return;
      }
      const { left, top, width, height } = dragged.getBoundingClientRect();
      if (pointer) setSignal({ pointer, box: { left, top, width, height } });
    };
    const onEnd = () => {
      window.cancelAnimationFrame(frame);
      frame = 0;
      pointer = null;
      root.classList.remove(QUIET_CLASS);
      setSignal(null);
      setNewFromList(false);
      if (!root.classList.contains(LIFT_CLASS)) return;
      liftTimer = window.setTimeout(() => lift(false), LIFT_AFTER_DROP_MS);
    };
    // Ahead of Blockly's handler, which then drops the block where the drag was cancelled.
    const onCancel = () => {
      if (!root.querySelector(".blocklyDragging")) return;
      for (const each of allAreas()) each.cancelled = true;
    };
    const onDragEnd = (event: Blockly.Events.Abstract) => {
      if (event.type !== Blockly.Events.BLOCK_DRAG || (event as Blockly.Events.BlockDrag).isStart) return;
      for (const each of allAreas()) each.cancelled = false;
    };
    workspace.addChangeListener(onDragEnd);
    root.addEventListener("pointerdown", onDown, true);
    window.addEventListener("pointermove", onMove, true);
    window.addEventListener("pointercancel", onCancel, true);
    window.addEventListener("pointerup", onEnd, true);
    window.addEventListener("pointercancel", onEnd, true);
    return () => {
      root.removeEventListener("pointerdown", onDown, true);
      window.removeEventListener("pointermove", onMove, true);
      window.removeEventListener("pointercancel", onCancel, true);
      window.removeEventListener("pointerup", onEnd, true);
      window.removeEventListener("pointercancel", onEnd, true);
      workspace.removeChangeListener(onDragEnd);
      stopWatchingBoxes();
      onEnd();
      lift(false);
      area.unregister();
    };
  }, [workspace]);

  return { signal, newFromList, lifted };
}
