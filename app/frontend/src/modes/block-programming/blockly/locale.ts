import * as Blockly from "blockly/core";
import * as englishMessages from "blockly/msg/en";
import * as thaiMessages from "blockly/msg/th";
import * as chineseMessages from "blockly/msg/zh-hans";
import i18n from "../../../i18n";

type MessageTree = { [key: string]: string | MessageTree };

// Block Lab's block text lives in the app catalogs (blockLab.blocks.*) like other UI text.
// Each leaf becomes the Blockly message SB_<PATH>, which block definitions use as %{BKY_SB_...}.
function blockMessages(tree: MessageTree, prefix = "SB"): Record<string, string> {
  const messages: Record<string, string> = {};
  for (const [key, entry] of Object.entries(tree)) {
    const name = `${prefix}_${key.toUpperCase()}`;
    if (typeof entry === "string") messages[name] = entry;
    else Object.assign(messages, blockMessages(entry, name));
  }
  return messages;
}

// Blockly's own wording that Block Lab replaces with the design's, and fills where Blockly's Thai
// catalog is still English: the function block and its settings box, comments a project made
// before may still hold, the screen reader label of switched-off blocks, and the right-click
// menu, whose undo and redo also match the workspace controls. Values keep Blockly's own %1
// placeholders.
const BLOCKLY_WORDING: Readonly<Record<string, string>> = {
  PROCEDURES_DEFNORETURN_TITLE: "blockLab.function.title",
  PROCEDURES_DEFRETURN_TITLE: "blockLab.function.title",
  PROCEDURES_MUTATORCONTAINER_TITLE: "blockLab.function.inputs",
  PROCEDURES_MUTATORARG_TITLE: "blockLab.function.inputName",
  PROCEDURES_ALLOW_STATEMENTS: "blockLab.function.allowStatements",
  PROCEDURES_CALL_DISABLED_DEF_WARNING: "blockLab.function.callDisabledWarning",
  ICON_LABEL_MUTATOR_CLOSED: "blockLab.blockSettings.open",
  ICON_LABEL_MUTATOR_OPEN: "blockLab.blockSettings.close",
  WORKSPACE_LABEL_MUTATOR_WORKSPACE: "blockLab.blockSettings.workspace",
  ICON_LABEL_COMMENT_CLOSED: "blockLab.comment.open",
  ICON_LABEL_COMMENT_OPEN: "blockLab.comment.close",
  BUBBLE_LABEL_COMMENT: "blockLab.comment.bubble",
  ARIA_LABEL_COMMENT: "blockLab.comment.label",
  ARIA_LABEL_COMMENT_COLLAPSE: "blockLab.comment.collapse",
  ARIA_LABEL_COMMENT_EXPAND: "blockLab.comment.expand",
  BLOCK_LABEL_DISABLED: "blockLab.blockState.disabled",
  UNDO: "common.messages.undo",
  REDO: "blockLab.workspace.redo",
  CLEAN_UP: "blockLab.blockMenu.cleanUp",
  DUPLICATE_BLOCK: "blockLab.blockMenu.duplicate",
  DELETE_BLOCK: "blockLab.blockMenu.deleteOne",
  DELETE_X_BLOCKS: "blockLab.blockMenu.deleteMany",
  PROCEDURES_CREATE_DO: "blockLab.blockMenu.createCall",
  VARIABLES_SET_CREATE_GET: "blockLab.blockMenu.createValue",
};

export function applyBlocklyLocale(language: string | undefined) {
  const messages = language === "zh"
    ? chineseMessages
    : language === "en"
      ? englishMessages
      : thaiMessages;
  // Blockly's CJS declarations include a default export that its ESM bundle omits.
  Blockly.setLocale(
    Object.fromEntries(
      Object.entries(messages).filter(([, value]) => typeof value === "string"),
    ) as Record<string, string>,
  );
  const catalog = language === "zh" || language === "en" ? language : "th";
  const blockText = i18n.getResource(catalog, "translation", "blockLab.blocks") as MessageTree;
  Blockly.setLocale(blockMessages(blockText));
  const t = i18n.getFixedT(catalog);
  Blockly.setLocale(Object.fromEntries(Object.entries(BLOCKLY_WORDING).map(([name, key]) => [name, t(key)])));
}
