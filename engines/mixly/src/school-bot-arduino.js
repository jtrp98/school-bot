'use strict';
// School Bot Block Lab blocks for the Arduino UNO, loaded after the upstream Mixly generators.
// Every type here is new (sb_*), never a replacement for an upstream type, so existing .mix
// files keep their meaning. Blocks named after Scratch blocks behave like Scratch (inclusive
// random range, letters counted from 1, contains answers true or false).
(function () {
  const A = Blockly.Arduino;
  const isInteger = text => /^-?\d+$/.test(text);

  // Upstream quote_ adds the quotes without escaping, so a quote or backslash in a text block
  // would end the C++ string early and turn the rest of the text into program code. The
  // engine already refuses control characters; they are escaped too so quote_ is safe alone.
  const octalEscape = character => '\\' + ('00' + character.charCodeAt(0).toString(8)).slice(-3);
  A.quote_ = function (text) {
    const escaped = String(text).replace(/[\\"]/g, '\\$&').replace(/[\x00-\x1f\x7f]/g, octalEscape);
    return '"' + escaped + '"';
  };

  // The loop hat's body becomes loop(); upstream already emits and indents top-level code
  // there, so drop the extra level statementToCode adds.
  A.sb_loop = function () {
    return A.statementToCode(this, 'DO').replace(/^ {2}/gm, '');
  };

  A.sb_forever = function () {
    return 'while (true) {\n' + A.statementToCode(this, 'DO') + '}\n';
  };

  A.sb_repeat = function () {
    const times = A.valueToCode(this, 'TIMES', A.ORDER_RELATIONAL) || '0';
    const counter = A.variableDB_.getDistinctName('i', Blockly.Variables.NAME_TYPE);
    const branch = A.statementToCode(this, 'DO');
    return 'for (int ' + counter + ' = 0; ' + counter + ' < ' + times + '; ' + counter + '++) {\n' +
      branch + '}\n';
  };

  A.sb_wait_until = function () {
    const condition = A.valueToCode(this, 'CONDITION', A.ORDER_NONE) || 'false';
    return 'while (!(' + condition + ')) {\n}\n';
  };

  A.sb_random = function () {
    const from = A.valueToCode(this, 'FROM', A.ORDER_NONE) || '0';
    const to = A.valueToCode(this, 'TO', A.ORDER_ADDITIVE) || '0';
    const end = isInteger(to) ? String(Number(to) + 1) : to + ' + 1';
    return ['random(' + from + ', ' + end + ')', A.ORDER_UNARY_POSTFIX];
  };

  A.sb_text_letter = function () {
    const text = A.valueToCode(this, 'TEXT', A.ORDER_NONE) || '""';
    const position = A.valueToCode(this, 'POSITION', A.ORDER_ADDITIVE) || '1';
    const index = isInteger(position) ? String(Number(position) - 1) : position + ' - 1';
    return ['String(' + text + ').charAt(' + index + ')', A.ORDER_ATOMIC];
  };

  A.sb_text_contains = function () {
    const text = A.valueToCode(this, 'TEXT', A.ORDER_NONE) || '""';
    const part = A.valueToCode(this, 'PART', A.ORDER_NONE) || '""';
    return ['String(' + text + ').indexOf(String(' + part + ')) != -1', A.ORDER_EQUALITY];
  };

  A.sb_variable_change = function () {
    const name = A.variableDB_.getName(this.getFieldValue('VAR'), Blockly.Variables.NAME_TYPE);
    const delta = A.valueToCode(this, 'DELTA', A.ORDER_ADDITIVE) || '0';
    return name + ' = ' + name + ' + ' + delta + ';\n';
  };
})();
