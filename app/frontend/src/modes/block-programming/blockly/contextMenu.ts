import * as Blockly from "blockly/core";
import i18n from "../../../i18n";
import { isRequiredBlock } from "./blocks";
import { deletableBlocks, deleteAllBlocks, placedCount } from "./deleteAll";
import { deleteInput, INPUT_TYPE } from "./settingsBox";

type Scope = Blockly.ContextMenuRegistry.Scope;
type RegistryItem = Blockly.ContextMenuRegistry.RegistryItem;

// The design's right-click menu icons, on a 24-unit grid.
const ICONS = {
  undo: "M9 14L4 9l5-5M4 9h10.5a5.5 5.5 0 0 1 5.5 5.5 5.5 5.5 0 0 1-5.5 5.5H11",
  redo: "M15 14l5-5-5-5M20 9H9.5A5.5 5.5 0 0 0 4 14.5 5.5 5.5 0 0 0 9.5 20H13",
  arrange: "M4 4v16M8 6h12v4H8zM8 14h8v4H8z",
  trash: "M3 6h18M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6M8 6V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2M10 11v6M14 11v6",
  copy: "M10 8h10a2 2 0 0 1 2 2v10a2 2 0 0 1-2 2H10a2 2 0 0 1-2-2V10a2 2 0 0 1 2-2z"
    + "M4 16a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h10a2 2 0 0 1 2 2",
  call: "M15 10l5 5-5 5M4 4v7a4 4 0 0 0 4 4h12",
  plus: "M5 12h14M12 5v14",
};

// The built-in entries the design keeps, with their icons. Blockly's comment, collapse and
// switch-off entries stay hidden: the workspace is injected without those options.
const ITEM_ICONS: Readonly<Record<string, (scope: Scope) => string>> = {
  undoWorkspace: () => ICONS.undo,
  redoWorkspace: () => ICONS.redo,
  cleanWorkspace: () => ICONS.arrange,
  workspaceDelete: () => ICONS.trash,
  blockDuplicate: () => ICONS.copy,
  blockDelete: () => ICONS.trash,
};

const MODIFIER_NAMES: Readonly<Record<string, string>> = { Control: "Ctrl", Meta: "Cmd", Shift: "Shift", Alt: "Alt" };
const KEY_NAMES: Readonly<Record<string, string>> = { 8: "Backspace", 46: "Delete" };

// The shortest key combination Blockly binds to the entry on this system, written the way the
// workspace controls write theirs ("Ctrl+Z"). Keys without a modifier, such as Delete, count for
// every system, as in Blockly's own formatting.
function shortcutLabel(name: string | undefined) {
  if (!name) return "";
  const all = Blockly.ShortcutRegistry.registry.getKeyCodesByShortcutName(name).map(key => key.split("+"));
  const thisSystem = all.filter(parts => parts.includes("Meta") === Blockly.utils.userAgent.APPLE);
  const combos = (thisSystem.length ? thisSystem : all).sort((a, b) => a.length - b.length);
  const parts = combos[0] ?? [];
  return parts.map(part => MODIFIER_NAMES[part] ?? KEY_NAMES[part] ?? String.fromCharCode(Number(part))).join("+");
}

function icon(path: string) {
  const svg = Blockly.utils.dom.createSvgElement(Blockly.utils.Svg.SVG, { viewBox: "0 0 24 24", class: "sbMenuIcon" });
  svg.setAttribute("aria-hidden", "true");
  Blockly.utils.dom.createSvgElement(Blockly.utils.Svg.PATH, { d: path }, svg);
  return svg;
}

// `reason` is the second line a disabled entry shows, so the child knows why it does nothing.
function entry(iconPath: string, label: string, shortcut: string, reason: string, danger: boolean) {
  const row = document.createElement("span");
  row.className = danger ? "sbMenuEntry sbMenuDanger" : "sbMenuEntry";
  const text = document.createElement("span");
  text.className = "sbMenuLabel";
  text.textContent = label;
  if (reason) {
    const why = document.createElement("span");
    why.className = "sbMenuReason";
    why.textContent = reason;
    text.append(why);
  }
  row.append(icon(iconPath), text);
  if (!shortcut) return row;
  const keys = document.createElement("span");
  keys.className = "sbMenuKeys";
  keys.textContent = shortcut;
  row.append(keys);
  return row;
}

function deleteLabel({ block }: Scope) {
  if (!block) return Blockly.Msg.DELETE_BLOCK;
  const stays = new Set(block.getNextBlock()?.getDescendants(false) ?? []);
  const count = placedCount(block.getDescendants(false).filter(part => !stays.has(part)));
  return count === 1 ? Blockly.Msg.DELETE_BLOCK : Blockly.Msg.DELETE_X_BLOCKS.replace("%1", String(count));
}

const deleteAllLabel = ({ workspace }: Scope) =>
  i18n.t("blockLab.blockMenu.deleteAll", { blockCount: workspace ? placedCount(deletableBlocks(workspace)) : 0 });

const OWN_LABELS: Readonly<Record<string, (scope: Scope) => string>> = {
  blockDelete: deleteLabel,
  workspaceDelete: deleteAllLabel,
};

// Why an entry is switched off, when it is: the same tests as the entries' own conditions.
const DISABLED_REASONS: Readonly<Record<string, (scope: Scope) => string | null>> = {
  undoWorkspace: ({ workspace }) =>
    workspace?.getUndoStack().length ? null : "blockLab.blockMenu.reason.nothingToUndo",
  redoWorkspace: ({ workspace }) =>
    workspace?.getRedoStack().length ? null : "blockLab.blockMenu.reason.nothingToRedo",
  cleanWorkspace: ({ workspace }) =>
    workspace?.getTopBlocks(false).length ? null : "blockLab.blockMenu.reason.noBlocks",
  workspaceDelete: ({ workspace }) =>
    workspace && deletableBlocks(workspace).length ? null : "blockLab.blockMenu.reason.noBlocks",
  blockDuplicate: ({ block }) => (isPlacedHat(block) ? "blockLab.blockMenu.reason.onlyOne" : null),
};

const isPlacedHat = (block: Blockly.BlockSvg | undefined) =>
  !!block && !block.isInFlyout && isRequiredBlock(block.type);

// What an entry shows for a block that is not an ordinary placed block, or null for the
// entry's own test.
function specialState(id: string, block: Blockly.BlockSvg | undefined) {
  if (!block) return null;
  // A settings box offers only "delete this input" (registerDeleteInput).
  if (block.workspace.isMutator) return "hidden";
  if (!isPlacedHat(block)) return null;
  // A start or loop hat exists once: "duplicate" stays in its menu, switched off with the reason,
  // beside "delete"; the list offers a deleted hat again.
  if (id === "blockDuplicate") return "disabled";
  return id === "blockDelete" ? null : "hidden";
}

const DANGER_ITEMS = new Set(["blockDelete", "workspaceDelete"]);

const OWN_CALLBACKS: Readonly<Record<string, (scope: Scope) => void>> = {
  workspaceDelete: ({ workspace }) => {
    if (workspace) deleteAllBlocks(workspace);
  },
};

// Blockly keeps a single stack where it is; the design arranges as soon as there is any block.
const OWN_PRECONDITIONS: Readonly<Record<string, (scope: Scope) => "enabled" | "disabled">> = {
  cleanWorkspace: ({ workspace }) => (workspace?.getTopBlocks(false).length ? "enabled" : "disabled"),
};

const decorated = new WeakSet<RegistryItem>();

const DELETE_INPUT_ITEM = "sbDeleteInput";

// The menu of an input placed in a function's settings box: the way to remove it without
// dragging it out (useLongPressMenu opens it on a long press or a right click).
function registerDeleteInput() {
  const registry = Blockly.ContextMenuRegistry.registry;
  if (registry.getItem(DELETE_INPUT_ITEM)) return;
  registry.register({
    id: DELETE_INPUT_ITEM,
    scopeType: Blockly.ContextMenuRegistry.ScopeType.BLOCK,
    weight: 0,
    displayText: () => entry(ICONS.trash, i18n.t("blockLab.blockMenu.deleteInput"), "", "", true),
    preconditionFn: ({ block }) =>
      block?.type === INPUT_TYPE && block.workspace.isMutator && !block.isInFlyout ? "enabled" : "hidden",
    callback: ({ block }) => {
      if (block) deleteInput(block);
    },
  });
}

// The design also opens the focused block's or the workspace's menu with Alt+M, beside
// Blockly's own Shift+F10, menu key and Ctrl+Enter.
function addMenuKey() {
  const shortcuts = Blockly.ShortcutRegistry.registry;
  const { KeyCodes } = Blockly.utils;
  const altM = shortcuts.createSerializedKey(KeyCodes.M, [KeyCodes.ALT]);
  const menu = Blockly.ShortcutItems.names.MENU;
  if (!shortcuts.getShortcutNamesByKeyCode(altM)?.includes(menu)) shortcuts.addKeyMapping(altM, menu, true);
}

type MenuOption = Blockly.ContextMenuRegistry.ContextMenuOption | Blockly.ContextMenuRegistry.LegacyContextMenuOption;
type CustomMenu = (this: Blockly.BlockSvg, options: MenuOption[]) => void;

const FUNCTION_HATS = ["procedures_defnoreturn", "procedures_defreturn"];
const arranged = new WeakSet<object>();

// A function's hat offers "make a call block" and one "make a value" per input. Blockly adds
// them below the other entries; the design puts them first, with its icons and a line below.
function arrangeFunctionMenus() {
  for (const type of FUNCTION_HATS) {
    const definition = Blockly.Blocks[type] as { customContextMenu?: CustomMenu } | undefined;
    const own = definition?.customContextMenu;
    if (!definition || !own || arranged.has(definition)) continue;
    definition.customContextMenu = function (options) {
      const before = options.length;
      own.call(this, options);
      const added = options.splice(before).map((option, index): MenuOption => {
        if (option.separator || typeof option.text !== "string") return option;
        return {
          id: `sbFunctionMenu${index}`,
          scope: { block: this },
          weight: 0,
          enabled: option.enabled,
          callback: option.callback,
          text: entry(index === 0 ? ICONS.call : ICONS.plus, option.text, "", "", false),
        };
      });
      if (!added.length) return;
      const line = { separator: true as const, id: "sbFunctionMenuLine", scope: { block: this }, weight: 0 };
      options.unshift(...added, line);
    };
    arranged.add(definition);
  }
}

// Entries the design's menu does not have and no workspace option hides: help opens a web page,
// and putting a block's values on one row or on separate lines changes only how it looks.
const LEFT_OUT = ["blockHelp", "blockInline"];

// Blockly's right-click menu with the design's entries, icons, key chips, red delete entries and
// the reason a disabled entry gives.
export function decorateContextMenu() {
  arrangeFunctionMenus();
  addMenuKey();
  registerDeleteInput();
  const registry = Blockly.ContextMenuRegistry.registry;
  for (const id of LEFT_OUT) {
    if (registry.getItem(id)) registry.unregister(id);
  }
  for (const [id, iconFor] of Object.entries(ITEM_ICONS)) {
    const item = registry.getItem(id);
    if (!item || decorated.has(item) || !("displayText" in item) || !item.displayText) continue;
    const label = OWN_LABELS[id] ?? item.displayText;
    const shortcut = shortcutLabel(item.associatedKeyboardShortcut);
    const precondition: ((scope: Scope, menuOpenEvent: Event) => string) | undefined =
      OWN_PRECONDITIONS[id] ?? item.preconditionFn;
    item.preconditionFn = (scope, menuOpenEvent) =>
      specialState(id, scope.block) ?? precondition?.(scope, menuOpenEvent) ?? "enabled";
    item.callback = OWN_CALLBACKS[id] ?? item.callback;
    item.displayText = scope => {
      const text = typeof label === "function" ? label(scope) : label;
      if (typeof text !== "string") return text;
      const reasonKey = DISABLED_REASONS[id]?.(scope) ?? null;
      const reason = reasonKey ? i18n.t(reasonKey) : "";
      return entry(iconFor(scope), text, shortcut, reason, DANGER_ITEMS.has(id) && !reasonKey);
    };
    decorated.add(item);
  }
}
