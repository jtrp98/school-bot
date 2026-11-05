import * as Blockly from "blockly/core";

// The design zooms from 50% to 200% in 25% steps; ctrl+wheel still zooms smoothly by this speed.
const ZOOM_LEVELS = [0.5, 0.75, 1, 1.25, 1.5, 1.75, 2];
export const MIN_SCALE = ZOOM_LEVELS[0];
export const MAX_SCALE = ZOOM_LEVELS[ZOOM_LEVELS.length - 1];
export const SCALE_SPEED = 1.25;
const EPSILON = 0.001;

// "Fit" leaves 24 around the blocks, and on the right also the controls (16 + 56 + 16).
const FIT_MARGIN = 24;
const FIT_RIGHT = 16 + 56 + 16;

export const canZoomIn = (scale: number) => scale < MAX_SCALE - EPSILON;
export const canZoomOut = (scale: number) => scale > MIN_SCALE + EPSILON;

// Zooms around the middle of what the child can see; the view already leaves out the list.
function zoomTo(workspace: Blockly.WorkspaceSvg, scale: number) {
  const metrics = workspace.getMetrics();
  const steps = Math.log(scale / workspace.scale) / Math.log(workspace.options.zoomOptions.scaleSpeed);
  const centreX = metrics.absoluteLeft + metrics.viewWidth / 2;
  const centreY = metrics.absoluteTop + metrics.viewHeight / 2;
  workspace.zoom(centreX, centreY, steps);
}

export function stepZoom(workspace: Blockly.WorkspaceSvg, direction: 1 | -1) {
  const scale = workspace.scale;
  const target = direction > 0
    ? ZOOM_LEVELS.find(level => level > scale + EPSILON)
    : [...ZOOM_LEVELS].reverse().find(level => level < scale - EPSILON);
  if (target !== undefined) zoomTo(workspace, target);
}

// Shows every block, never larger than 100%, centred in the space the controls leave free.
export function fitBlocks(workspace: Blockly.WorkspaceSvg) {
  const box = workspace.getBlocksBoundingBox();
  const metrics = workspace.getMetrics();
  const width = metrics.viewWidth - FIT_MARGIN - FIT_RIGHT;
  const height = metrics.viewHeight - FIT_MARGIN * 2;
  const blocksWidth = box.right - box.left;
  const blocksHeight = box.bottom - box.top;
  if (blocksWidth <= 0 || blocksHeight <= 0) {
    workspace.setScale(1);
    return;
  }
  const fitted = Math.min(width / blocksWidth, height / blocksHeight);
  const scale = Math.min(1, Math.max(MIN_SCALE, fitted));
  workspace.setScale(scale);
  const left = FIT_MARGIN + width / 2 - (box.left + blocksWidth / 2) * scale;
  const top = FIT_MARGIN + height / 2 - (box.top + blocksHeight / 2) * scale;
  workspace.scroll(left, top);
}

const { KeyCodes } = Blockly.utils;
const shift = (keyCode: number) => Blockly.ShortcutRegistry.registry.createSerializedKey(keyCode, [KeyCodes.SHIFT]);

// + − 0 on the workspace, beside Blockly's own arrows, undo and redo. Not while a field is
// being edited or a block is being dragged.
type ZoomShortcut = {
  name: string;
  keyCodes: (number | string)[];
  run: (workspace: Blockly.WorkspaceSvg) => void;
};
const ZOOM_SHORTCUTS: ZoomShortcut[] = [
  {
    name: "sb_zoom_in",
    keyCodes: [KeyCodes.EQUALS, shift(KeyCodes.EQUALS), KeyCodes.NUM_PLUS],
    run: workspace => stepZoom(workspace, 1),
  },
  {
    name: "sb_zoom_out",
    keyCodes: [KeyCodes.DASH, KeyCodes.NUM_MINUS],
    run: workspace => stepZoom(workspace, -1),
  },
  { name: "sb_zoom_fit", keyCodes: [KeyCodes.ZERO, KeyCodes.NUM_ZERO], run: fitBlocks },
];

export function registerZoomShortcuts() {
  const registry = Blockly.ShortcutRegistry.registry;
  for (const shortcut of ZOOM_SHORTCUTS) {
    if (registry.getRegistry()[shortcut.name]) continue;
    registry.register({
      name: shortcut.name,
      keyCodes: shortcut.keyCodes,
      preconditionFn: workspace => !workspace.isFlyout
        && !workspace.isDragging()
        && !Blockly.getFocusManager().ephemeralFocusTaken(),
      callback: (workspace, event) => {
        shortcut.run(workspace);
        event.preventDefault();
        return true;
      },
    });
  }
}
