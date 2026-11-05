import { useCallback, useEffect, useRef, useState } from "react";
import { useTranslation } from "react-i18next";
import * as Blockly from "blockly/core";
import { defineBlockLabBlocks } from "../blockly/blocks";
import { decorateContextMenu } from "../blockly/contextMenu";
import { BlockLabFlyout } from "../blockly/flyout";
import { applyBlocklyLocale } from "../blockly/locale";
import { BLOCK_LAB_RENDERER } from "../blockly/renderer";
import { blockLabTheme } from "../blockly/theme";
import { CREATE_VARIABLE_CALLBACK } from "../blockly/toolbox";
import {
  canZoomIn,
  canZoomOut,
  fitBlocks,
  MAX_SCALE,
  MIN_SCALE,
  registerZoomShortcuts,
  SCALE_SPEED,
  stepZoom,
} from "../blockly/zoom";

// The zoom level shows briefly after a zoom, then goes, as in the design.
const ZOOM_NOTE_MS = 1600;

type WorkspaceView = { canUndo: boolean; canRedo: boolean; scale: number };
const INITIAL_VIEW: WorkspaceView = { canUndo: false, canRedo: false, scale: 1 };

function readView(workspace: Blockly.WorkspaceSvg): WorkspaceView {
  return {
    canUndo: workspace.getUndoStack().length > 0,
    canRedo: workspace.getRedoStack().length > 0,
    scale: workspace.scale,
  };
}

export function addStartingBlocks(workspace: Blockly.WorkspaceSvg) {
  const setup = workspace.newBlock("base_setup");
  const loop = workspace.newBlock("sb_loop");
  for (const block of [setup, loop]) {
    block.initSvg();
    block.render();
  }
  setup.moveBy(40, 40);
  loop.moveBy(40, 40 + setup.getHeightWidth().height + 40);
}

export function useBlockWorkspace() {
  const { i18n } = useTranslation();
  const container = useRef<HTMLDivElement>(null);
  const [workspace, setWorkspace] = useState<Blockly.WorkspaceSvg | null>(null);
  const [view, setView] = useState(INITIAL_VIEW);
  const [zoomNote, setZoomNote] = useState<number | null>(null);
  // Block text is resolved when a block is created, so a language change rebuilds the workspace
  // from this snapshot instead of losing the child's blocks.
  const snapshot = useRef<object | null>(null);

  useEffect(() => {
    applyBlocklyLocale(i18n.resolvedLanguage);
    defineBlockLabBlocks();
    registerZoomShortcuts();
    decorateContextMenu();
    // Blockly puts its menus in the workspace's own box, which the rounded panel clips; the
    // Block Lab root keeps them inside the theme and free to overflow the panel.
    const root = container.current!.closest("[data-theme='block-lab']");
    if (root) Blockly.common.setParentContainer(root);
    const instance = Blockly.inject(container.current!, {
      toolbox: { kind: "flyoutToolbox", contents: [] },
      renderer: BLOCK_LAB_RENDERER,
      theme: blockLabTheme,
      media: "./blockly-media/",
      trashcan: false,
      // The wheel scrolls and ctrl+wheel zooms. The scrollbars keep Blockly's scroll limits but
      // are hidden in blockLab.css: the design has none.
      move: { scrollbars: true, drag: true, wheel: true },
      zoom: {
        controls: false,
        wheel: true,
        startScale: 1,
        minScale: MIN_SCALE,
        maxScale: MAX_SCALE,
        scaleSpeed: SCALE_SPEED,
      },
      grid: { spacing: 24, length: 1, colour: "var(--line)", snap: true },
      plugins: { flyoutsVerticalToolbox: BlockLabFlyout },
    });
    if (snapshot.current) Blockly.serialization.workspaces.load(snapshot.current, instance);
    else addStartingBlocks(instance);
    instance.registerButtonCallback(CREATE_VARIABLE_CALLBACK, button => {
      Blockly.Variables.createVariableButtonHandler(button.getTargetWorkspace());
    });
    const refresh = () => setView(readView(instance));
    // Only blocks joined under a hat or a function run, so loose blocks are greyed out.
    instance.addChangeListener(Blockly.Events.disableOrphans);
    instance.addChangeListener(refresh);
    const resize = new ResizeObserver(() => Blockly.svgResize(instance));
    resize.observe(container.current!);
    setView(readView(instance));
    setWorkspace(instance);
    return () => {
      snapshot.current = Blockly.serialization.workspaces.save(instance);
      resize.disconnect();
      instance.removeChangeListener(refresh);
      instance.removeChangeListener(Blockly.Events.disableOrphans);
      instance.dispose();
      setWorkspace(null);
    };
  }, [i18n.resolvedLanguage]);

  useEffect(() => {
    if (!workspace) return;
    let timer: number | undefined;
    // Blockly merges back-to-back viewport events and with them their old scale, so the last
    // scale seen is kept here instead.
    let lastScale = workspace.scale;
    const onZoom = (event: Blockly.Events.Abstract) => {
      if (event.type !== Blockly.Events.VIEWPORT_CHANGE) return;
      const scale = workspace.scale;
      if (Math.abs(scale - lastScale) < 0.001) return;
      lastScale = scale;
      setZoomNote(Math.round(scale * 100));
      window.clearTimeout(timer);
      timer = window.setTimeout(() => setZoomNote(null), ZOOM_NOTE_MS);
    };
    workspace.addChangeListener(onZoom);
    return () => {
      workspace.removeChangeListener(onZoom);
      window.clearTimeout(timer);
      setZoomNote(null);
    };
  }, [workspace]);

  const undo = useCallback(() => workspace?.undo(false), [workspace]);
  const redo = useCallback(() => workspace?.undo(true), [workspace]);
  const zoomIn = useCallback(() => workspace && stepZoom(workspace, 1), [workspace]);
  const zoomOut = useCallback(() => workspace && stepZoom(workspace, -1), [workspace]);
  const fit = useCallback(() => workspace && fitBlocks(workspace), [workspace]);

  return {
    container,
    workspace,
    controls: {
      canUndo: view.canUndo,
      canRedo: view.canRedo,
      canZoomIn: canZoomIn(view.scale),
      canZoomOut: canZoomOut(view.scale),
      zoomPercent: Math.round(view.scale * 100),
      zoomNote,
      undo,
      redo,
      zoomIn,
      zoomOut,
      fit,
    },
  };
}

export type WorkspaceControlsState = ReturnType<typeof useBlockWorkspace>["controls"];
