import {
  choice,
  command,
  container,
  hat,
  message,
  reporter,
  symbols,
  value,
  variable,
  type BlockJson,
} from "./builders";

const COMPARE = [["=", "EQ"], ["≠", "NEQ"], ["<", "LT"], ["≤", "LTE"], [">", "GT"], ["≥", "GTE"]] as const;
const ARITHMETIC = [
  ["+", "ADD"],
  ["−", "MINUS"],
  ["×", "MULTIPLY"],
  ["÷", "DIVIDE"],
  [message("options_math_mod"), "QUYU"],
  ["^", "POWER"],
] as const;

// Field and input names follow the Mixly Arduino generators in engines/mixly, which turn
// these blocks into C++; a renamed field silently generates the default value instead.
export const CORE_BLOCKS: readonly BlockJson[] = [
  hat("base_setup", "start"),
  hat("sb_loop", "loop"),

  command("controls_delay", "control", [
    value("DELAY_TIME", "Number"),
    choice("UNIT", "delay", ["delay", "delayMicroseconds"]),
  ]),
  {
    ...container("controls_if", "control", [value("IF0", "Boolean")]),
    args1: [{ type: "input_statement", name: "DO0" }],
    // Blockly's own mutator adds the "else if" and "else" inputs with the names Mixly expects.
    mutator: "controls_if_mutator",
  },
  container("controls_whileUntil", "control", [choice("MODE", "while", ["WHILE", "UNTIL"]), value("BOOL", "Boolean")]),
  container("sb_forever", "control"),
  container("controls_for", "control", [
    variable(),
    value("FROM", "Number"),
    value("TO", "Number"),
    value("STEP", "Number"),
  ]),
  container("sb_repeat", "control", [value("TIMES", "Number")]),
  command("controls_flow_statements", "control", [choice("FLOW", "flow", ["BREAK", "CONTINUE"])]),
  command("sb_wait_until", "control", [value("CONDITION", "Boolean")]),
  { ...command("controls_end_program", "control"), nextStatement: undefined },

  reporter("logic_compare", "logic", [value("A"), symbols("OP", COMPARE), value("B")], "Boolean"),
  reporter("logic_operation", "logic", [
    value("A", "Boolean"),
    choice("OP", "logic", ["AND", "OR"]),
    value("B", "Boolean"),
  ], "Boolean"),
  reporter("logic_negate", "logic", [value("BOOL", "Boolean")], "Boolean"),
  reporter("logic_boolean", "logic", [choice("BOOL", "boolean", ["TRUE", "FALSE"])], "Boolean"),

  reporter("math_number", "math", [{ type: "field_number", name: "NUM", value: 0 }], "Number"),
  reporter("math_arithmetic", "math", [value("A", "Number"), symbols("OP", ARITHMETIC), value("B", "Number")], "Number"),
  reporter("sb_random", "math", [value("FROM", "Number"), value("TO", "Number")], "Number"),
  reporter("math_to_int", "math", [
    choice("OP", "round", ["round", "ceil", "floor", "abs", "sqrt"]),
    value("A", "Number"),
  ], "Number"),
  reporter("base_map", "math", [
    value("NUM", "Number"),
    value("fromLow", "Number"),
    value("fromHigh", "Number"),
    value("toLow", "Number"),
    value("toHigh", "Number"),
  ], "Number"),
  reporter("math_constrain", "math", [value("VALUE", "Number"), value("LOW", "Number"), value("HIGH", "Number")], "Number"),

  reporter("text", "text", [{ type: "field_input", name: "TEXT", text: "" }], "String"),
  reporter("text_join", "text", [value("A"), value("B")], "String"),
  reporter("sb_text_letter", "text", [value("POSITION", "Number"), value("TEXT")], "String"),
  reporter("text_length", "text", [value("VAR")], "Number"),
  reporter("sb_text_contains", "text", [value("TEXT"), value("PART")], "Boolean"),

  command("variables_declare", "variable", [
    variable(),
    choice("TYPE", "type", ["int", "float", "String", "boolean"]),
    value("VALUE"),
  ]),
  reporter("variables_get", "variable", [variable()], null),
  command("variables_set", "variable", [variable(), value("VALUE")]),
  command("sb_variable_change", "variable", [variable(), value("DELTA", "Number")]),

  { ...command("procedures_return", "function", [value("VALUE")]), nextStatement: undefined },

  command("serial_print", "serial", [choice("new_line", "print", ["print", "println"]), value("CONTENT")]),
  reporter("serial_readstr", "serial", [], "String"),
  reporter("serial_available", "serial", [], "Boolean"),
];
