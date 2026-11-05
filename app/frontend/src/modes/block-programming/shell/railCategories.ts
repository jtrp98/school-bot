import { BLOCK_CATEGORIES, type BlockCategory, type BlockCategoryId } from "../blockly/categories";

// The design lists 14 categories: character blocks belong to the stage, which Block Lab does not
// have, and Block Lab has no Arm Control; its colour stays reserved in categories.ts.
export type RailCategoryId = Exclude<BlockCategoryId, "character" | "arm">;

export type RailCategory = BlockCategory & {
  id: RailCategoryId;
  labelKey: string;
};

export type RailGroup = {
  labelKey: string;
  categories: readonly RailCategory[];
};

const LABEL_KEYS: Record<RailCategoryId, string> = {
  start: "blockLab.categories.start",
  control: "blockLab.categories.control",
  logic: "blockLab.categories.logic",
  math: "blockLab.categories.math",
  text: "blockLab.categories.text",
  variable: "blockLab.categories.variable",
  list: "blockLab.categories.list",
  function: "blockLab.categories.function",
  car: "blockLab.categories.car",
  motor: "blockLab.categories.motor",
  sensor: "blockLab.categories.sensor",
  display: "blockLab.categories.display",
  serial: "blockLab.categories.serial",
  remote: "blockLab.categories.remote",
};

export function railCategory(id: RailCategoryId): RailCategory {
  const colours = BLOCK_CATEGORIES.find(category => category.id === id)!;
  return { ...colours, id, labelKey: LABEL_KEYS[id] };
}

export const RAIL_GROUPS: readonly RailGroup[] = [
  {
    labelKey: "blockLab.rail.groups.basics",
    categories: (["start", "control", "logic", "math", "text"] as const).map(railCategory),
  },
  {
    labelKey: "blockLab.rail.groups.data",
    categories: (["variable", "list", "function"] as const).map(railCategory),
  },
  {
    labelKey: "blockLab.rail.groups.schoolBot",
    categories: (["car", "motor", "sensor", "display", "serial", "remote"] as const).map(railCategory),
  },
];

export const RAIL_ORDER: readonly RailCategoryId[] = RAIL_GROUPS.flatMap(group =>
  group.categories.map(category => category.id),
);

// The design opens new work on Control, the category with the most everyday blocks.
export const DEFAULT_CATEGORY: RailCategoryId = "control";
