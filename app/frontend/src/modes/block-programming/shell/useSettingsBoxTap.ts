import { useEffect } from "react";
import * as Blockly from "blockly/core";
import { addInputFromTray, INPUT_TYPE, watchSettingsBoxes } from "../blockly/settingsBox";

// A tap on the input in a function's settings box tray adds it after the last input, so a child
// who cannot drag can still give the function inputs.
export function useSettingsBoxTap(workspace: Blockly.WorkspaceSvg | null) {
  useEffect(() => {
    if (!workspace) return;
    return watchSettingsBoxes(workspace, (_functionId, box) => {
      const tray = box.getFlyout()?.getWorkspace();
      if (!tray) return;
      const onTray = (event: Blockly.Events.Abstract) => {
        const click = event as Blockly.Events.Click;
        if (event.type !== Blockly.Events.CLICK || click.targetType !== "block" || !click.blockId) return;
        const source = tray.getBlockById(click.blockId)?.getRootBlock();
        if (source?.type === INPUT_TYPE) addInputFromTray(box, source);
      };
      tray.addChangeListener(onTray);
      return () => tray.removeChangeListener(onTray);
    });
  }, [workspace]);
}
