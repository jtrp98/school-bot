import { useEffect } from "react";
import { useTranslation } from "react-i18next";
import * as Blockly from "blockly/core";

// blockLab.css holds the ring 0.6s, then fades it out once.
const DROP_CLASS = "sbDropped";
const DROP_MS = 900;

// After a block is let go on the workspace, its outline turns into a short blue ring and screen
// readers hear "placed". A block let go on the list is deleted instead and gets neither.
export function useDropEffect(workspace: Blockly.WorkspaceSvg | null) {
  const { t } = useTranslation();

  useEffect(() => {
    if (!workspace) return;
    const timers = new Set<number>();
    const onDrag = (event: Blockly.Events.Abstract) => {
      if (event.type !== Blockly.Events.BLOCK_DRAG) return;
      const drag = event as Blockly.Events.BlockDrag;
      if (drag.isStart || !drag.blockId) return;
      const block = workspace.getBlockById(drag.blockId);
      if (!block) return;
      block.addClass(DROP_CLASS);
      const timer = window.setTimeout(() => {
        timers.delete(timer);
        if (!block.isDeadOrDying()) block.removeClass(DROP_CLASS);
      }, DROP_MS);
      timers.add(timer);
      Blockly.utils.aria.announceDynamicAriaState(t("blockLab.workspace.dropped"));
    };
    workspace.addChangeListener(onDrag);
    return () => {
      workspace.removeChangeListener(onDrag);
      for (const timer of timers) window.clearTimeout(timer);
    };
  }, [workspace, t]);
}
