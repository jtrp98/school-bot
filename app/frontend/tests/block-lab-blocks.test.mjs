import test from "node:test";
import assert from "node:assert/strict";
import { spawnSync } from "node:child_process";
import { existsSync, readFileSync } from "node:fs";
import { createRequire } from "node:module";
import { fileURLToPath } from "node:url";
import { createServer } from "vite";

globalThis.localStorage = { getItem: () => null, setItem() {} };
globalThis.location = { search: "" };
const require = createRequire(import.meta.url);
const Blockly = require("blockly/core");
globalThis.blocklyTestRequire = require;
// Blockly's Node build is CommonJS with exports Node cannot list for ESM, so each "blockly/..."
// import becomes a module re-exporting the one CommonJS instance.
const blocklyFromNode = {
  name: "blockly-from-node",
  enforce: "pre",
  resolveId: id => (id === "blockly/core" || id === "blockly/blocks" || id.startsWith("blockly/msg/") ? `\0${id}` : null),
  load(id) {
    if (!id.startsWith("\0blockly/")) return null;
    const name = id.slice(1);
    const names = Object.keys(require(name)).filter(key => /^[A-Za-z_$][\w$]*$/.test(key) && key !== "default");
    return [
      `const library = globalThis.blocklyTestRequire(${JSON.stringify(name)});`,
      "export default library;",
      ...names.map(key => `export const ${key} = library[${JSON.stringify(key)}];`),
    ].join("\n");
  },
};
const server = await createServer({
  configFile: false,
  root: fileURLToPath(new URL("../", import.meta.url)),
  cacheDir: "node_modules/.vite-block-lab-tests",
  plugins: [blocklyFromNode],
  ssr: { noExternal: ["blockly"] },
  optimizeDeps: { noDiscovery: true, include: [] },
  server: { middlewareMode: true, hmr: false },
  appType: "custom",
});
let BLOCK_LAB_BLOCKS, CORE_BLOCKS, defineBlockLabBlocks, applyBlocklyLocale, toEngineProgram, findBlockProblems, fromEngineProblems;
try {
  ({ BLOCK_LAB_BLOCKS, defineBlockLabBlocks } = await server.ssrLoadModule("/src/modes/block-programming/blockly/blocks/index.ts"));
  ({ CORE_BLOCKS } = await server.ssrLoadModule("/src/modes/block-programming/blockly/blocks/coreBlocks.ts"));
  ({ applyBlocklyLocale } = await server.ssrLoadModule("/src/modes/block-programming/blockly/locale.ts"));
  ({ toEngineProgram } = await server.ssrLoadModule("/src/modes/block-programming/blockly/engineProgram.ts"));
  const preflight = "/src/modes/block-programming/blockly/preflight.ts";
  ({ findBlockProblems, fromEngineProblems } = await server.ssrLoadModule(preflight));
} finally {
  await server.close();
}

const catalog = JSON.parse(readFileSync(new URL("../../../contracts/block-lab/catalog.json", import.meta.url), "utf8"));
const catalogTypes = catalog.categories.flatMap(category => category.blocks)
  .filter(block => block.status !== "deferred")
  .map(block => block.type);
// Blockly's own procedure blocks are kept as they are; Block Lab defines every other type.
const STANDARD_TYPES = ["procedures_defnoreturn", "procedures_defreturn", "procedures_callnoreturn", "procedures_callreturn"];
const target = `${process.platform === "win32" ? "windows" : process.platform}-${process.arch === "x64" ? "amd64" : process.arch}`;
const executable = `mixly-engine${process.platform === "win32" ? ".exe" : ""}`;
const engine = process.env.MIXLY_ENGINE
  ?? fileURLToPath(new URL(`../../../engines/mixly/bin/${target}/${executable}`, import.meta.url));

applyBlocklyLocale("en");
defineBlockLabBlocks();

function generate(programs, action = "program.generate") {
  const lines = programs.map((data, index) => JSON.stringify({
    protocol: "1.0",
    request_id: `block-${index}`,
    mode: "block-programming",
    action,
    data: { board: "arduino_avr", platform: "Arduino/Genuino Uno", ...data },
  }));
  const child = spawnSync(engine, [], { input: `${lines.join("\n")}\n`, encoding: "utf8", timeout: 60000, windowsHide: true });
  assert.ifError(child.error);
  assert.equal(child.status, 0, child.stderr);
  return child.stdout.trim().split(/\r?\n/).map(line => JSON.parse(line));
}

function program(blocks) {
  return inWorkspace(blocks, toEngineProgram);
}

function inWorkspace(blocks, read) {
  const workspace = new Blockly.Workspace();
  Blockly.serialization.workspaces.load({ blocks: { languageVersion: 0, blocks } }, workspace);
  const result = read(workspace);
  workspace.dispose();
  return result;
}

// Puts a block where it can run: commands go in the loop, values in a place that takes them.
function placed(json, fields) {
  const block = fields ? { type: json.type, fields } : { type: json.type };
  if (json.previousStatement !== undefined) return block;
  if (json.output === "Boolean") return { type: "controls_if", inputs: { IF0: { block } } };
  if (json.output === "Colour") return { type: "sb_rgb_set", inputs: { COLOR: { block } } };
  return { type: "serial_print", inputs: { CONTENT: { block } } };
}

const inProgram = block => [
  { type: "base_setup", x: 0, y: 0 },
  { type: "sb_loop", x: 0, y: 200, inputs: { DO: { block } } },
];

test("the toolbox offers exactly the catalog's blocks, each with a definition", () => {
  const defined = [...BLOCK_LAB_BLOCKS.map(block => block.type), ...STANDARD_TYPES].sort();
  assert.deepEqual(defined, [...catalogTypes].sort());
  for (const type of catalogTypes) assert.ok(Blockly.Blocks[type], `${type} has no Blockly definition`);
});

test("every block message exists in all three languages", () => {
  const tokens = [...new Set(JSON.stringify(BLOCK_LAB_BLOCKS).match(/%\{BKY_SB_[A-Z0-9_]+\}/g))];
  for (const language of ["th", "en", "zh"]) {
    applyBlocklyLocale(language);
    for (const token of tokens) {
      const name = token.slice(6, -1);
      assert.equal(typeof Blockly.Msg[name], "string", `${language} is missing ${name}`);
    }
  }
  applyBlocklyLocale("en");
});

test("loose blocks and switched-off blocks are left out of the program", () => {
  const { blocks } = program([
    ...inProgram({ type: "sb_led", next: { block: { type: "sb_oled_clear", enabled: false } } }),
    { type: "sb_relay", x: 400, y: 0 },
  ]);
  assert.deepEqual(blocks.map(block => block.type), ["base_setup", "sb_loop"]);
  assert.equal(blocks[1].statements.DO.type, "sb_led");
  assert.equal(blocks[1].statements.DO.next, undefined);
});

test("every catalog block generates Arduino code in the engine", { skip: !existsSync(engine) && "mixly engine not built" }, () => {
  const types = BLOCK_LAB_BLOCKS.filter(json => !["base_setup", "sb_loop", "procedures_return"].includes(json.type));
  const responses = generate(types.map(json => program(inProgram(placed(json)))));
  responses.forEach((response, index) => {
    assert.equal(response.status, "success", `${types[index].type}: ${response.error?.message}`);
    assert.match(response.data.code, /void loop\(\)/);
  });
});

// The engine takes only the values these blocks offer in the fields Mixly pastes into the C++.
test("every choice the core blocks offer generates in the engine", { skip: !existsSync(engine) && "mixly engine not built" }, () => {
  const choices = CORE_BLOCKS.flatMap(json => [...(json.args0 ?? []), ...(json.args1 ?? [])]
    .filter(arg => arg.type === "field_dropdown")
    .flatMap(arg => arg.options.map(([, choice]) => ({ json, fields: { [arg.name]: choice } }))));
  const responses = generate(choices.map(({ json, fields }) => program(inProgram(placed(json, fields)))));
  responses.forEach((response, index) => {
    const { json, fields } = choices[index];
    assert.equal(response.status, "success", `${json.type} ${JSON.stringify(fields)}: ${response.error?.message}`);
  });
});

test("if branches and functions keep their shape through the engine", { skip: !existsSync(engine) && "mixly engine not built" }, () => {
  const branches = program(inProgram({
    type: "controls_if",
    extraState: { elseIfCount: 1, hasElse: true },
    inputs: { IF0: { block: { type: "logic_boolean" } }, IF1: { block: { type: "logic_boolean" } } },
  }));
  assert.deepEqual(branches.blocks[1].statements.DO.mutation, { elseif: "1", else: "1" });
  const functions = program([
    ...inProgram({
      type: "serial_print",
      inputs: { CONTENT: { block: { type: "procedures_callreturn", extraState: { name: "twice", params: ["x"] } } } },
    }),
    {
      type: "procedures_defreturn",
      x: 400,
      y: 0,
      extraState: { params: [{ name: "x" }] },
      fields: { NAME: "twice" },
      inputs: { RETURN: { block: { type: "math_number", fields: { NUM: 2 } } } },
    },
  ]);
  const [ifCode, functionCode] = generate([branches, functions]).map(response => {
    assert.equal(response.status, "success", response.error?.message);
    return response.data.code;
  });
  assert.match(ifCode, /else if/);
  assert.match(functionCode, /twice\(long x\)/);
});

// A collapsed block carries Blockly's summary label as an extra field (only on the rendered
// workspace, so it is added here by hand); the engine must generate the same code without it.
test("a collapsed block generates the same code as an expanded one", { skip: !existsSync(engine) && "mixly engine not built" }, () => {
  const expanded = program(inProgram({
    type: "controls_delay",
    inputs: { DELAY_TIME: { block: { type: "math_number", fields: { NUM: 500 } } } },
  }));
  const collapsed = structuredClone(expanded);
  collapsed.blocks[1].statements.DO.fields._TEMP_COLLAPSED_FIELD = "wait 500 ms";
  const [plain, folded] = generate([expanded, collapsed]).map(response => {
    assert.equal(response.status, "success", response.error?.message);
    return response.data.code;
  });
  assert.equal(folded, plain);
});

test("the check before sending lists empty slots and loose blocks, not what is switched off", () => {
  const ids = problems => problems.map(problem => `${problem.kind}:${problem.blockId}`);
  const found = inWorkspace([
    ...inProgram({
      type: "controls_if",
      id: "if-empty",
      inputs: { IF0: { block: { type: "logic_negate", id: "not-empty" } } },
      next: {
        block: { type: "controls_delay", id: "wait", inputs: { DELAY_TIME: { shadow: { type: "math_number" } } } },
      },
    }),
    {
      type: "procedures_defnoreturn",
      x: 400,
      y: 0,
      inputs: { STACK: { block: { type: "procedures_return", id: "return" } } },
    },
    { type: "sb_relay", id: "loose", x: 400, y: 200, inputs: {} },
    { type: "sb_led", id: "parked", x: 400, y: 300, enabled: false },
    { type: "controls_if", id: "off", x: 400, y: 400, enabled: false },
  ], findBlockProblems);
  // The "if" is filled by "not", whose own slot is empty; a shadow counts as filled, and an
  // empty "return" returns nothing.
  assert.deepEqual(ids(found), ["emptySlot:not-empty", "loose:loose"]);
  assert.equal(found[1].blockName.length > 0, true);
});

const needsEngine = { skip: !existsSync(engine) && "mixly engine not built" };

test("wiring the engine refuses points at a block that uses the pin", needsEngine, () => {
  const blocks = inProgram({
    type: "serial_print",
    inputs: { CONTENT: { block: { type: "sb_button", id: "button", fields: { PIN: "7" } } } },
    next: { block: { type: "sb_led", id: "led", fields: { PIN: "7" } } },
  });
  const [response] = generate([program(blocks)], "project.validate");
  assert.equal(response.status, "success", response.error?.message);
  const found = inWorkspace(blocks, workspace => fromEngineProblems(workspace, response.data.problems));
  assert.deepEqual(found, [{ kind: "pinShared", blockId: "button", pin: "7", uses: ["input", "output"] }]);
});
