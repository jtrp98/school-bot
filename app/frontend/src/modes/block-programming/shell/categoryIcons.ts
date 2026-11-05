import { GenIcon, type IconType } from "react-icons/lib";
import { HAT_ICON_PATHS } from "../blockly/hatFields";
import type { RailCategoryId } from "./railCategories";

// Icons drawn with the exact outline paths of the A2 design (stroke 2.4, round caps), so the rail,
// the list header, the category chips and the workspace notes match it line for line.
export const outline = (d: string): IconType => GenIcon({
  tag: "svg",
  attr: {
    viewBox: "0 0 24 24",
    fill: "none",
    stroke: "currentColor",
    strokeWidth: "2.4",
    strokeLinecap: "round",
    strokeLinejoin: "round",
  },
  child: [{ tag: "path", attr: { d }, child: [] }],
});

export const CATEGORY_ICONS: Record<RailCategoryId, IconType> = {
  start: outline(HAT_ICON_PATHS.start),
  control: outline(HAT_ICON_PATHS.loop),
  logic: outline("M12 3l9 9-9 9-9-9z"),
  math: outline("M7 3v8M3 7h8M13 17h8M14.5 14.5l5 5M19.5 14.5l-5 5"),
  text: outline("M4 7V4h16v3M9 20h6M12 4v16"),
  variable: outline("M21 8l-9-5-9 5v8l9 5 9-5zM3 8l9 5 9-5M12 13v8"),
  list: outline("M8 6h13M8 12h13M8 18h13M3 6h.01M3 12h.01M3 18h.01"),
  function: outline(
    "M9 3H7a2 2 0 0 0-2 2v4a2 2 0 0 1-2 2 2 2 0 0 1 2 2v4a2 2 0 0 0 2 2h2M15 3h2a2 2 0 0 1 2 2v4a2 2 0 0 0 2 2 "
      + "2 2 0 0 0-2 2v4a2 2 0 0 1-2 2h-2",
  ),
  car: outline("M5 17h14v-5l-2-5H7l-2 5zM5 12h14M8 17v2M16 17v2M8 14h.01M16 14h.01"),
  motor: outline("M21 12a9 9 0 1 1-3-6.7M21 3v6h-6"),
  sensor: outline("M5 12.55a11 11 0 0 1 14.08 0M1.42 9a16 16 0 0 1 21.16 0M8.53 16.11a6 6 0 0 1 6.95 0M12 20h.01"),
  display: outline("M3 4h18v12H3zM8 21h8M12 16v5"),
  serial: outline("M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"),
  remote: outline("M8 2h8v20H8zM12 6h.01M10 10h4M10 14h4M10 18h4"),
};
