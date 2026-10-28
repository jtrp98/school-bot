const TABLE_KEYS_TIP_STORAGE_KEY = "sb_tblkeys";

export function hasSeenTableKeysTip(): boolean {
  try {
    return window.localStorage.getItem(TABLE_KEYS_TIP_STORAGE_KEY) === "1";
  } catch {
    // Blocked storage only means the tip may appear again on the next launch.
    return false;
  }
}

export function markTableKeysTipSeen(): void {
  try {
    window.localStorage.setItem(TABLE_KEYS_TIP_STORAGE_KEY, "1");
  } catch {
    // The in-memory keyTipSeen flag still keeps the tip to one showing per session.
  }
}
