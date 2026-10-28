globalThis.window = globalThis;
globalThis.self = globalThis;
globalThis.console = { log() {}, warn() {}, error() {} };
globalThis.goog = {
  provide(name) {
    let target = globalThis;
    for (const part of name.split('.')) {
      if (!Object.prototype.hasOwnProperty.call(target, part)) target[part] = {};
      target = target[part];
    }
  },
  require() {},
};
globalThis.Mixly = {
  JSFuncs: { getPlatform: () => platformName },
  Boards: { getSelectedBoardKey: () => selectedBoardKey },
};
// Some upstream files initialise canvas tools alongside ordinary generators.
// Keep loading them, but fail explicitly if a request needs browser rendering.
globalThis.document = {
  createElement() {
    return { getContext: () => new Proxy({}, {
      get() { throw new Error('This block requires the Mixly browser canvas'); },
    }) };
  },
};

function makeBlock(node, workspace, valueInput = false, parent = null) {
  const inputs = Object.create(null);
  const fields = Object.assign(Object.create(null), node.fields ?? {});
  const block = {
    type: node.type,
    id: String(workspace.blocks.length),
    workspace,
    isTopBlock: parent === null,
    inputList: [],
    outputConnection: valueInput ? { targetConnection: {} } : null,
    nextConnection: null,
    itemCount_: Number(node.mutation?.items ?? 0),
    elseifCount_: Number(node.mutation?.elseif ?? 0),
    elseCount_: Number(node.mutation?.else ?? 0),
    arguments_: (node.arguments ?? []).map(argument => argument.name),
    argumentstype_: (node.arguments ?? []).map(argument => argument.type || 'long'),
    hasReturnValue_: !['0', 'false'].includes(node.mutation?.value),
    getFieldValue: name => fields[name] ?? '',
    setFieldValue(value, name) { fields[name] = String(value); },
    setTooltip() {},
    setWarningText(message) {
      if (message) workspace.warnings.push(String(message));
    },
    getInputTargetBlock: name => inputs[name] ?? null,
    getInput: name => block.inputList.find(input => input.name === name) ?? null,
    getNextBlock: () => block.nextConnection?.targetBlock() ?? null,
    getParent: () => parent,
    getSurroundParent: () => parent,
    getCommentText: () => node.comment ?? '',
    isEnabled: () => node.disabled !== true,
    isInsertionMarker: () => false,
    getChildren: () => Object.values(inputs),
    getDescendants() {
      return [block, ...Object.values(inputs).flatMap(child => child.getDescendants())];
    },
  };
  workspace.blocks.push(block);
  for (const [name, child] of Object.entries(node.values ?? {})) {
    inputs[name] = makeBlock(child, workspace, true, block);
    block.inputList.push({ name, type: Blockly.INPUT_VALUE, connection: { targetBlock: () => inputs[name] } });
  }
  for (const [name, child] of Object.entries(node.statements ?? {})) {
    inputs[name] = makeBlock(child, workspace, false, block);
    block.inputList.push({ name, type: Blockly.NEXT_STATEMENT, connection: { targetBlock: () => inputs[name] } });
  }
  if (node.next) {
    const next = makeBlock(node.next, workspace, false, parent);
    block.nextConnection = { targetBlock: () => next };
  }
  return block;
}

function generateProgram(nodes, language) {
  const workspace = {
    blocks: [],
    warnings: [],
    options: { oneBasedIndex: true },
    getAllBlocks() { return this.blocks; },
    getAllVariables: () => [],
    getVariableMap: () => ({ getAllVariables: () => [], getVariableById: () => null }),
  };
  const topBlocks = nodes.map(node => makeBlock(node, workspace));
  workspace.getTopBlocks = () => topBlocks;
  const generator = language === 'arduino' ? Blockly.Arduino : Blockly.Python;
  for (const block of workspace.blocks) {
    if (!Object.prototype.hasOwnProperty.call(generator, block.type) ||
        typeof generator[block.type] !== 'function' ||
        ['init', 'finish', 'scrub_', 'quote_', 'scrubNakedValue'].includes(block.type) ||
        Object.prototype.hasOwnProperty.call(Blockly.Generator.prototype, block.type)) {
      throw new Error('Unknown block type: ' + block.type);
    }
  }
  const code = generator.workspaceToCode(workspace);
  if (workspace.warnings.length) throw new Error(workspace.warnings.join('\n'));
  return code;
}

function prepareGenerator(language) {
  const generator = language === 'arduino' ? Blockly.Arduino : Blockly.Python;
  for (const key of Object.keys(generator)) {
    if (typeof generator[key] !== 'function' || key.endsWith('_') ||
        Object.prototype.hasOwnProperty.call(Blockly.Generator.prototype, key) ||
        ['init', 'finish', 'scrubNakedValue'].includes(key)) continue;
    const original = generator[key];
    generator[key] = function(...args) {
      const result = original.apply(this, args);
      if (Array.isArray(result) && this.isTopBlock && !this.outputConnection) {
        this.outputConnection = { targetConnection: null };
      }
      return result;
    };
    Object.assign(generator[key], original);
  }
}
