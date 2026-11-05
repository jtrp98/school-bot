import * as Blockly from "blockly/core";
import catalog from "../../../../../../contracts/block-lab/catalog.json";
import i18n from "../../../i18n";
import type { BlockCategoryId } from "./categories";

type CatalogBlock = { type: string; group?: string; status?: string };
type FlyoutItem = Blockly.utils.toolbox.FlyoutItemInfo;

export const CREATE_VARIABLE_CALLBACK = "SB_CREATE_VARIABLE";

// The toolbox is built from the contract the engine is tested against, so every block a child
// can drag has a generator.
const CATALOG_BLOCKS = new Map(
  catalog.categories.map(category => [
    category.id,
    (category.blocks as CatalogBlock[]).filter(block => block.status !== "deferred"),
  ]),
);

const SENSOR_GROUP_KEYS: Record<string, string> = {
  buttons: "blockLab.sensorGroups.buttons",
  motion: "blockLab.sensorGroups.motion",
  light: "blockLab.sensorGroups.light",
  sound: "blockLab.sensorGroups.sound",
  distance: "blockLab.sensorGroups.distance",
  environment: "blockLab.sensorGroups.environment",
  actuators: "blockLab.sensorGroups.actuators",
};
const HEADING_GAP_ABOVE = 28;
const HEADING_GAP_BELOW = 8;

const number = (value: number) => ({ shadow: { type: "math_number", fields: { NUM: value } } });
const text = () => ({ shadow: { type: "text", fields: { TEXT: i18n.t("blockLab.defaults.text") } } });

// Starting values, so a block dragged from the list already does something sensible.
const STARTING_INPUTS: Record<string, () => Record<string, unknown>> = {
  controls_delay: () => ({ DELAY_TIME: number(1000) }),
  controls_for: () => ({ FROM: number(1), TO: number(10), STEP: number(1) }),
  sb_repeat: () => ({ TIMES: number(10) }),
  logic_compare: () => ({ A: number(0), B: number(0) }),
  math_arithmetic: () => ({ A: number(1), B: number(1) }),
  sb_random: () => ({ FROM: number(1), TO: number(10) }),
  math_to_int: () => ({ A: number(0) }),
  base_map: () => ({
    NUM: number(512),
    fromLow: number(0),
    fromHigh: number(1023),
    toLow: number(0),
    toHigh: number(255),
  }),
  math_constrain: () => ({ VALUE: number(0), LOW: number(0), HIGH: number(255) }),
  text_join: () => ({ A: text(), B: text() }),
  sb_text_letter: () => ({ POSITION: number(1), TEXT: text() }),
  text_length: () => ({ VAR: text() }),
  sb_text_contains: () => ({ TEXT: text(), PART: text() }),
  variables_declare: () => ({ VALUE: number(0) }),
  variables_set: () => ({ VALUE: number(0) }),
  sb_variable_change: () => ({ DELTA: number(1) }),
  serial_print: () => ({ CONTENT: text() }),
  sb_fan: () => ({ SPEED: number(50) }),
  sb_buzzer_tone: () => ({ FREQUENCY: number(262) }),
  sb_buzzer_tone_for: () => ({ FREQUENCY: number(262), DURATION: number(500) }),
  sb_rgb_set: () => ({ PIXEL: number(1), COLOR: { shadow: { type: "sb_rgb_named_color" } } }),
  sb_rgb_color: () => ({ RED: number(255), GREEN: number(0), BLUE: number(0) }),
  sb_servo: () => ({ ANGLE: number(90) }),
  sb_oled_print: () => ({ TEXT: text() }),
  sb_led_brightness: () => ({ BRIGHTNESS: number(50) }),
};

// The A2 list spaces blocks 12px apart.
const BLOCK_GAP = 12;

const blockItem = (type: string, extra: Record<string, unknown> = {}): FlyoutItem => ({
  kind: "block",
  type,
  gap: BLOCK_GAP,
  ...(STARTING_INPUTS[type] ? { inputs: STARTING_INPUTS[type]() } : {}),
  ...extra,
}) as FlyoutItem;

function variableFlyout(workspace: Blockly.WorkspaceSvg): FlyoutItem[] {
  const button: FlyoutItem = {
    kind: "button",
    text: Blockly.Msg.NEW_VARIABLE,
    callbackkey: CREATE_VARIABLE_CALLBACK,
  };
  const variables = workspace.getVariableMap().getAllVariables();
  if (!variables.length) return [button];
  const last = variables[variables.length - 1];
  const fields = { VAR: { id: last.getId() } };
  return [
    button,
    ...(CATALOG_BLOCKS.get("variable") ?? []).map(block => blockItem(block.type, { fields })),
  ];
}

// Blockly lists one "define" block per kind and a call block per defined function; Block Lab
// replaces Blockly's conditional return with the plain one the Mixly generator understands.
function functionFlyout(workspace: Blockly.WorkspaceSvg): FlyoutItem[] {
  const standard = Blockly.Procedures.flyoutCategory(workspace);
  return [
    ...standard.filter(item => !("type" in item) || item.type !== "procedures_ifreturn"),
    blockItem("procedures_return"),
  ];
}

function sensorFlyout(): FlyoutItem[] {
  const items: FlyoutItem[] = [];
  let group: string | undefined;
  for (const block of CATALOG_BLOCKS.get("sensor") ?? []) {
    if (block.group && block.group !== group) {
      // A2 keeps a heading close to its blocks and leaves room above later ones for the divider
      // the list draws (flyout.ts).
      if (group) items.push({ kind: "sep", gap: String(HEADING_GAP_ABOVE) } as FlyoutItem);
      group = block.group;
      items.push({
        kind: "label",
        text: i18n.t(SENSOR_GROUP_KEYS[group]),
        "web-class": "sbListHeading",
      } as FlyoutItem);
      items.push({ kind: "sep", gap: String(HEADING_GAP_BELOW) } as FlyoutItem);
    }
    items.push(blockItem(block.type));
  }
  return items;
}

export function categoryFlyout(id: BlockCategoryId, workspace: Blockly.WorkspaceSvg): FlyoutItem[] {
  if (id === "variable") return variableFlyout(workspace);
  if (id === "function") return functionFlyout(workspace);
  if (id === "sensor") return sensorFlyout();
  return (CATALOG_BLOCKS.get(id) ?? []).map(block => blockItem(block.type));
}

// Categories with nothing to offer in this version.
export const hasNoBlocks = (id: BlockCategoryId) => !CATALOG_BLOCKS.get(id)?.length;
