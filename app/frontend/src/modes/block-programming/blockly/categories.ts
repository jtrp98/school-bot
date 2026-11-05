// The Blockly theme and the design check sheet both read this list, so a category's colour
// is defined once. Order follows the category rail in the design.
export type BlockCategoryId =
  | "start"
  | "control"
  | "logic"
  | "math"
  | "text"
  | "variable"
  | "list"
  | "function"
  | "arm"
  | "car"
  | "motor"
  | "sensor"
  | "display"
  | "serial"
  | "remote"
  | "character";

export type BlockCategory = {
  id: BlockCategoryId;
  fill: string;
  edge: string;
};

export const BLOCK_CATEGORIES: readonly BlockCategory[] = [
  { id: "start", fill: "#FFD23F", edge: "#9C7A00" },
  { id: "control", fill: "#FFAB3D", edge: "#A8610A" },
  { id: "logic", fill: "#62D4C8", edge: "#137A70" },
  { id: "math", fill: "#6CCB5F", edge: "#2F7A26" },
  { id: "text", fill: "#B79CFF", edge: "#5F3FC2" },
  { id: "variable", fill: "#FF8F70", edge: "#B03A1C" },
  { id: "list", fill: "#FF86A8", edge: "#B02C57" },
  { id: "function", fill: "#F08AD3", edge: "#9A2F85" },
  { id: "arm", fill: "#C9E04A", edge: "#6E7F0B" },
  { id: "car", fill: "#D58CF2", edge: "#8239A8" },
  { id: "motor", fill: "#5BAEFF", edge: "#1D63B8" },
  { id: "sensor", fill: "#5CCBEA", edge: "#17789A" },
  { id: "display", fill: "#7FE0A8", edge: "#1F8050" },
  { id: "serial", fill: "#8FA6FF", edge: "#3D4FC0" },
  { id: "remote", fill: "#D2A679", edge: "#7A4E1F" },
  { id: "character", fill: "#FFBE94", edge: "#A4521C" },
];

export const blockStyleName = (id: BlockCategoryId, hat = false) =>
  hat ? `sb-${id}-hat` : `sb-${id}`;
