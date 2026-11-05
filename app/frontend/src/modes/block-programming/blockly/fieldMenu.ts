import * as Blockly from "blockly/core";

const PIN_FIELDS = new Set(["PIN", "PIN_A", "PIN_B", "TRIG", "ECHO"]);

export type FieldMenuKind = "pin" | "variable" | "color" | "value";
// action marks the variable menu's own "rename" and "delete" entries.
export type FieldMenuOption = { value: string; label: string; action?: "rename" | "delete" } | "separator";
export type FieldMenuRequest = {
  kind: FieldMenuKind;
  anchor: DOMRect;
  options: FieldMenuOption[];
  value: string | null;
  source: Blockly.Block;
  pick: (value: string) => void;
};

// The shell's React popover registers here; without one (the design check sheet) the fields
// fall back to Blockly's own dropdown.
let openMenu: ((request: FieldMenuRequest) => void) | null = null;
export function setFieldMenuOpener(opener: typeof openMenu) {
  openMenu = opener;
}

function optionLabel(label: string | Blockly.ImageProperties | HTMLElement) {
  if (typeof label === "string") return label;
  if (label instanceof HTMLElement) return label.textContent ?? "";
  return label.alt;
}

const ACTIONS: Readonly<Record<string, "rename" | "delete">> = {
  [Blockly.RENAME_VARIABLE_ID]: "rename",
  [Blockly.DELETE_VARIABLE_ID]: "delete",
};

function readOptions(field: Blockly.FieldDropdown): FieldMenuOption[] {
  const options: FieldMenuOption[] = [];
  for (const option of field.getOptions(false)) {
    if (option === "separator") {
      options.push(option);
      continue;
    }
    const action = ACTIONS[option[1]];
    // The variable actions sit apart from the variables themselves.
    const firstAction = action && !options.some(item => item !== "separator" && item.action);
    if (firstAction && options.length) options.push("separator");
    options.push({ value: option[1], label: optionLabel(option[0]), action });
  }
  return options;
}

function kindOf(field: Blockly.FieldDropdown): FieldMenuKind {
  if (field instanceof Blockly.FieldVariable) return "variable";
  return PIN_FIELDS.has(field.name ?? "") ? "pin" : "value";
}

export function requestMenu(
  field: Blockly.FieldDropdown,
  pick: (value: string) => void,
  kind: FieldMenuKind = kindOf(field),
) {
  const source = field.getSourceBlock();
  const element = field.getSvgRoot();
  if (!openMenu || !source || !element) return false;
  openMenu({
    kind,
    anchor: element.getBoundingClientRect(),
    options: readOptions(field),
    value: field.getValue(),
    source,
    pick,
  });
  return true;
}

// FieldVariable's own handler takes a menu item; only the value is read from it, which keeps
// its "rename" and "delete" entries working from the React menu.
const asMenuItem = (value: string) => ({ getValue: () => value }) as unknown as Blockly.MenuItem;
const NO_MENU = null as unknown as Blockly.Menu;

// Block Lab's dropdown: the same options and stored values, opened as the design's popover.
class BlockLabDropdown extends Blockly.FieldDropdown {
  protected override showEditor_(e?: MouseEvent) {
    if (!requestMenu(this, value => this.setValue(value))) super.showEditor_(e);
  }
}

class BlockLabVariableField extends Blockly.FieldVariable {
  protected override showEditor_(e?: MouseEvent) {
    if (!requestMenu(this, value => this.onItemSelected_(NO_MENU, asMenuItem(value)))) super.showEditor_(e);
  }
}

// Registered over Blockly's own names, so the block definitions keep "field_dropdown" and
// "field_variable" and nothing they save changes.
export function registerBlockLabFields() {
  Blockly.fieldRegistry.register("field_dropdown", BlockLabDropdown);
  Blockly.fieldRegistry.register("field_variable", BlockLabVariableField);
}
