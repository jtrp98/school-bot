import type { EntryAdapter } from "./types";

// This presentation adapter grants no native entitlement; the simulator client never calls the host.
export const simulatorEntryAdapter: EntryAdapter = {
  readStatus: async () => ({ status: "offline-valid", expiresAt: "2099-01-01T00:00:00Z" }),
  subscribe: () => () => {},
  guest: async () => {},
  activate: async () => { throw new Error("Simulator only"); },
  validate: async () => {},
};
