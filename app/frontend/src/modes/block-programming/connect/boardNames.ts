// Names the children give their boards stay on this computer only, so the next class finds
// "group 3's board" in the list without plugging and unplugging cables again.
const STORAGE_KEY = "school-bot:block-lab:board-names";

export const MAX_BOARD_NAME_LENGTH = 20;

export type BoardNames = Readonly<Record<string, string>>;

export function loadBoardNames(): BoardNames {
  try {
    const saved: unknown = JSON.parse(localStorage.getItem(STORAGE_KEY) ?? "{}");
    if (typeof saved !== "object" || saved === null || Array.isArray(saved)) return {};
    const names: Record<string, string> = {};
    for (const [key, name] of Object.entries(saved)) {
      if (typeof name === "string" && name.trim()) names[key] = name.trim().slice(0, MAX_BOARD_NAME_LENGTH);
    }
    return names;
  } catch {
    // Embedded webviews can disable local storage; boards then simply stay unnamed.
    return {};
  }
}

export function saveBoardNames(names: BoardNames) {
  try {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(names));
  } catch {
    // Storage is optional: the name still shows for this session.
  }
}
