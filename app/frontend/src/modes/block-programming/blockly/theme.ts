import * as Blockly from "blockly/core";
import { BLOCK_CATEGORIES, blockStyleName } from "./categories";

// Blockly parses theme colours itself and rejects CSS variables, so these mirror the
// Block Lab tokens in blockLab.css, which also sets the block text colour.
const INK: readonly number[] = [20, 36, 61];

// The design paints dropdowns as the category colour under a 12% ink wash.
function inkWash(hex: string) {
  const mixed = [1, 3, 5].map((offset, index) =>
    Math.round(parseInt(hex.slice(offset, offset + 2), 16) * 0.88 + INK[index] * 0.12),
  );
  return `#${mixed.map(value => value.toString(16).padStart(2, "0")).join("")}`;
}

const blockStyles: Record<string, Blockly.Theme.BlockStyle> = {};
const categoryStyles: Record<string, Blockly.Theme.CategoryStyle> = {};
for (const category of BLOCK_CATEGORIES) {
  const style = {
    colourPrimary: category.fill,
    colourSecondary: inkWash(category.fill),
    colourTertiary: category.edge,
    hat: "",
  };
  blockStyles[blockStyleName(category.id)] = style;
  blockStyles[blockStyleName(category.id, true)] = { ...style, hat: "cap" };
  categoryStyles[category.id] = { colour: category.fill };
}

// Blockly's own blocks that Block Lab keeps (procedures, the parts of the "if" mutator) name its
// standard styles, so those names take the matching category colours.
blockStyles.procedure_blocks = blockStyles[blockStyleName("function", true)];
blockStyles.logic_blocks = blockStyles[blockStyleName("control")];

export const blockLabTheme = new Blockly.Theme("school-bot-block-lab", blockStyles, categoryStyles, {
  workspaceBackgroundColour: "#FFFFFF",
  flyoutBackgroundColour: "#FFFFFF",
  flyoutOpacity: 1,
  scrollbarColour: "#C6D6EA",
  insertionMarkerColour: "#C9CFD8",
  insertionMarkerOpacity: 1,
  cursorColour: "#1866C9",
  selectedGlowColour: "#1866C9",
});
blockLabTheme.setFontStyle({
  family: "'IBM Plex Sans Thai', system-ui, sans-serif",
  weight: "700",
  // Blockly sizes block text in points: 12pt is the design's 16px.
  size: 12,
});
