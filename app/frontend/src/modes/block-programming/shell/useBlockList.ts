import { useCallback, useEffect, useRef, useState } from "react";
import * as Blockly from "blockly/core";
import { addFromList } from "../blockly/addFromList";
import { isRequiredBlock, REQUIRED_BLOCKS } from "../blockly/blocks";
import { LIST_WIDTH, type BlockLabFlyout } from "../blockly/flyout";
import { categoryFlyout } from "../blockly/toolbox";
import type { RailCategoryId } from "./railCategories";
import type { BlockToastState } from "./useBlockToast";

// Variables and functions are made by the child, so their lists change while the category is open.
const VARIABLE_EVENTS: readonly string[] = [
  Blockly.Events.VAR_CREATE,
  Blockly.Events.VAR_RENAME,
  Blockly.Events.VAR_DELETE,
];

// A start or loop hat came into the program or left it, so the list shows it as placed or as
// one to add. A hat dragged out of the list is created as the drag starts; the list then waits
// for the drop instead of being rebuilt under the drag.
function hatChanged(event: Blockly.Events.Abstract, workspace: Blockly.WorkspaceSvg) {
  const type = createdOrDeletedType(event);
  return !!type && isRequiredBlock(type) && !workspace.isDragging();
}

function createdOrDeletedType(event: Blockly.Events.Abstract) {
  if (event.type === Blockly.Events.BLOCK_DELETE) return (event as Blockly.Events.BlockDelete).oldJson?.type;
  if (event.type === Blockly.Events.BLOCK_CREATE) return (event as Blockly.Events.BlockCreate).json?.type;
  return undefined;
}

function listChanged(event: Blockly.Events.Abstract, workspace: Blockly.WorkspaceSvg) {
  if (VARIABLE_EVENTS.includes(event.type) || hatChanged(event, workspace)) return true;
  // A function appears in the list once the child lets go of its "define" block.
  return event.type === Blockly.Events.BLOCK_DRAG && !(event as Blockly.Events.BlockDrag).isStart;
}

function isWorkspaceClick(event: Blockly.Events.Abstract) {
  return event.type === Blockly.Events.CLICK && (event as Blockly.Events.Click).targetType === "workspace";
}

type Fades = { top: boolean; bottom: boolean };
// contentBottom: where the list's blocks end, in list pixels, so a note can follow them.
type ListView = { fades: Fades; contentBottom: number };
const NO_VIEW: ListView = { fades: { top: false, bottom: false }, contentBottom: 0 };

const ADD_FROM_LIST_SHORTCUT = "sb_add_from_list";

// Blockly's own dropdowns and editors, which close themselves on Esc.
const BLOCKLY_POPUPS = ".blocklyDropDownDiv[style*='display: block'], .blocklyWidgetDiv[style*='display: block']";

function readView(flyout: BlockLabFlyout): ListView {
  const metrics = flyout.getWorkspace().getMetricsManager();
  const view = metrics.getViewMetrics(true);
  const content = metrics.getContentMetrics(true);
  const contentEnd = content.top + content.height;
  return {
    fades: { top: view.top > content.top + 1, bottom: view.top + view.height < contentEnd - 1 },
    contentBottom: contentEnd - view.top,
  };
}

// The block list beside the rail: which blocks it holds comes from the catalog (toolbox.ts);
// this hook only shows, hides and measures it, and adds a block when a child taps one.
export function useBlockList(
  workspace: Blockly.WorkspaceSvg | null,
  category: RailCategoryId,
  toast: Pick<BlockToastState, "added" | "onlyOne">,
) {
  const [shown, setShown] = useState(true);
  const [width, setWidth] = useState(LIST_WIDTH);
  const [deleting, setDeleting] = useState(false);
  const [view, setView] = useState<ListView>(NO_VIEW);
  const [version, setVersion] = useState(0);
  // Both hats are in the program, so the start category says each can be used once.
  const [hatsPlaced, setHatsPlaced] = useState(true);
  const flyout = workspace?.getFlyout() as BlockLabFlyout | null | undefined;
  const { added, onlyOne } = toast;

  useEffect(() => {
    if (!flyout) return;
    flyout.onWidth = setWidth;
    flyout.onDeleteZone = setDeleting;
    return () => {
      flyout.onWidth = undefined;
      flyout.onDeleteZone = undefined;
    };
  }, [flyout]);

  // A newly picked category starts at its top; a list refreshed in place (a new variable) stays.
  const listedCategory = useRef<RailCategoryId | null>(null);
  useEffect(() => {
    if (!workspace || !flyout) return;
    const contents = categoryFlyout(category, workspace);
    if (shown && contents.length) {
      flyout.show(contents);
      if (listedCategory.current !== category) flyout.scrollToStart();
      listedCategory.current = category;
    } else {
      flyout.hide();
    }
    // An empty category's note is drawn at the design's list width.
    if (!contents.length) setWidth(LIST_WIDTH);
    setView(shown && contents.length ? readView(flyout) : NO_VIEW);
    setHatsPlaced(REQUIRED_BLOCKS.every(type => workspace.getBlocksByType(type, false).length > 0));
  }, [workspace, flyout, category, shown, version]);

  useEffect(() => {
    if (!workspace || !flyout) return;
    const onMain = (event: Blockly.Events.Abstract) => {
      if (listChanged(event, workspace)) setVersion(current => current + 1);
      if (isWorkspaceClick(event)) setShown(false);
    };
    const listWorkspace = flyout.getWorkspace();
    const take = (source: Blockly.BlockSvg) => {
      // A block that cannot be dragged out cannot be added by a tap either.
      if (!source.isMovable()) {
        if (isRequiredBlock(source.type)) onlyOne(source.type);
        return;
      }
      const result = addFromList(workspace, source);
      if (result) added(result);
    };
    const onList = (event: Blockly.Events.Abstract) => {
      if (event.type === Blockly.Events.VIEWPORT_CHANGE) setView(readView(flyout));
      const click = event as Blockly.Events.Click;
      if (event.type !== Blockly.Events.CLICK || click.targetType !== "block" || !click.blockId) return;
      // A tap on a value inside a list block reports that value; the child meant the whole block.
      const source = listWorkspace.getBlockById(click.blockId)?.getRootBlock();
      if (source) take(source);
    };
    // Keyboard focus can sit on a block or on one of its fields; either way it is the whole block.
    const focusedListBlock = (node: Blockly.IFocusableNode | undefined) => {
      const block = node instanceof Blockly.Field ? node.getSourceBlock() : node;
      return block instanceof Blockly.BlockSvg && block.workspace === listWorkspace ? block.getRootBlock() : null;
    };
    // Enter or Space on a block in the list adds it the way a tap does, ahead of Blockly's own
    // keyboard insert, so both ways put the block in the same place.
    Blockly.ShortcutRegistry.registry.register({
      name: ADD_FROM_LIST_SHORTCUT,
      keyCodes: [Blockly.utils.KeyCodes.ENTER, Blockly.utils.KeyCodes.SPACE],
      allowCollision: true,
      preconditionFn: (_target, scope) => !!focusedListBlock(scope.focusedNode),
      callback: (_target, keyEvent, _shortcut, scope) => {
        const source = focusedListBlock(scope.focusedNode);
        if (!source) return false;
        keyEvent.preventDefault();
        take(source);
        return true;
      },
    });
    workspace.addChangeListener(onMain);
    listWorkspace.addChangeListener(onList);
    return () => {
      Blockly.ShortcutRegistry.registry.unregister(ADD_FROM_LIST_SHORTCUT);
      workspace.removeChangeListener(onMain);
      listWorkspace.removeChangeListener(onList);
    };
  }, [workspace, flyout, added, onlyOne]);

  const hide = useCallback(() => setShown(false), []);
  const show = useCallback(() => setShown(true), []);

  useEffect(() => {
    if (!shown) return;
    const onKey = (event: KeyboardEvent) => {
      if (event.key !== "Escape" || event.defaultPrevented) return;
      const target = event.target as Element | null;
      // Esc already belongs to text fields, Blockly's dropdowns and open dialogs.
      if (target?.closest("input, textarea, [role=dialog], [role=alertdialog], [role=menu]")) return;
      if (document.querySelector(BLOCKLY_POPUPS)) return;
      setShown(false);
    };
    window.addEventListener("keydown", onKey);
    return () => window.removeEventListener("keydown", onKey);
  }, [shown]);

  return { shown, width, deleting, hatsPlaced, fades: view.fades, contentBottom: view.contentBottom, hide, show };
}

export type BlockListState = ReturnType<typeof useBlockList>;
