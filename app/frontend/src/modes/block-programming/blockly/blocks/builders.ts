import { blockStyleName, type BlockCategoryId } from "../categories";
import { hatIcon, PILL_FIELD, type HatIcon } from "../hatFields";

export type BlockJson = { type: string } & Record<string, unknown>;
type Arg = Record<string, unknown>;

// Pin rules from the engine (engines/mixly/docs/API.md): 0 and 1 carry upload and Serial,
// 13 flashes on reset so only the LED may use it, and only 5 and 6 keep PWM beside Servo and tone.
export const ANALOG_PINS = ["A0", "A1", "A2", "A3", "A4", "A5"];
export const IO_PINS = ["2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", ...ANALOG_PINS];
export const LED_PINS = [...IO_PINS, "13"];
export const PWM_PINS = ["5", "6"];

// Block text comes from the app catalogs (blockLab.blocks.*), registered as Blockly messages.
export const message = (key: string) => `%{BKY_SB_${key.toUpperCase()}}`;

export const pin = (name = "PIN", pins: readonly string[] = IO_PINS): Arg => ({
  type: "field_dropdown",
  name,
  options: pins.map(value => [value, value]),
});

export const choice = (name: string, group: string, values: readonly string[]): Arg => ({
  type: "field_dropdown",
  name,
  options: values.map(value => [message(`options_${group}_${value}`), value]),
});

export const symbols = (name: string, options: readonly (readonly [string, string])[]): Arg => ({
  type: "field_dropdown",
  name,
  options,
});

export const value = (name: string, check?: string): Arg => (check
  ? { type: "input_value", name, check }
  : { type: "input_value", name });

export const variable = (name = "VAR"): Arg => ({ type: "field_variable", name, variable: null });

const statementInput = (name = "DO"): Arg => ({ type: "input_statement", name });

export function command(type: string, category: BlockCategoryId, args: Arg[] = []): BlockJson {
  return {
    type,
    message0: message(type),
    args0: args,
    inputsInline: true,
    previousStatement: null,
    nextStatement: null,
    style: blockStyleName(category),
  };
}

export function reporter(type: string, category: BlockCategoryId, args: Arg[], output: string | null): BlockJson {
  return {
    type,
    message0: message(type),
    args0: args,
    inputsInline: true,
    output,
    style: blockStyleName(category),
  };
}

// A block that wraps a stack of commands under its title, like "repeat" or "if".
export function container(type: string, category: BlockCategoryId, args: Arg[] = []): BlockJson {
  return {
    ...command(type, category, args),
    message1: message("common_do"),
    args1: [statementInput()],
  };
}

// The start and loop hats: nothing attaches above them. The icon and the note are unnamed
// decorations, so they are not saved and do not reach the generator.
export function hat(type: string, icon: HatIcon): BlockJson {
  return {
    type,
    message0: `%1 ${message(type)} %2`,
    args0: [hatIcon(icon), { type: PILL_FIELD, text: message(`${type}_note`), class: "sbPillText" }],
    message1: message("common_do"),
    args1: [statementInput()],
    style: blockStyleName("start", true),
  };
}
