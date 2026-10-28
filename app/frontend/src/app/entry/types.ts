export type EntitlementState =
  | "unactivated" | "checking" | "valid" | "offline-valid" | "expired"
  | "denied" | "clock_untrusted" | "locked" | "unavailable";

export type EntryError =
  | "network" | "DEVICE_LIMIT" | "DEVICE_NOT_ACTIVATED" | "DEVICE_PROOF_INVALID"
  | "LICENSE_INVALID" | "LICENSE_INACTIVE" | "BINDING_REVOKED" | "INVALID_ENTITLEMENT"
  | "UNKNOWN_FORBIDDEN" | "CREDENTIAL_STORE_FAILED" | "CHALLENGE_EXPIRED" | "unavailable";

// Presentation seam only: the host owns verification, clocks, persistence and authorization.
export interface PublicEntitlement {
  status: EntitlementState;
  expiresAt?: string;
  expiringSoon?: boolean;
  error?: EntryError;
  canUseOffline?: boolean;
}

export type LoginResult = "success" | "invalid_credentials" | "network" | "unavailable";
export type EntryFeedback = Exclude<LoginResult, "success"> | "operation_failed" | "recovery_failed";

export interface EntryActions {
  activate: (key: string) => Promise<void>;
  validate: () => Promise<void>;
  useOffline?: () => Promise<void>;
  guest: () => Promise<void>;
  login?: (username: string, password: string) => Promise<LoginResult>;
  saveAndRequestStop?: () => Promise<void>;
}

export interface EntryAdapter extends EntryActions {
  readStatus: () => Promise<PublicEntitlement>;
  subscribe: (listener: (status: PublicEntitlement) => void) => () => void;
}

export function allowsEntry(status: PublicEntitlement): boolean {
  return (status.status === "valid" || status.status === "offline-valid") && !!status.expiresAt
    && Number.isFinite(Date.parse(status.expiresAt));
}

// Stop here until a real host public-status adapter exists; v1 activation is not entitlement proof.
export const unavailableEntryAdapter: EntryAdapter = {
  readStatus: async () => ({ status: "unavailable" }),
  subscribe: () => () => {},
  activate: async () => { throw new Error("unavailable"); },
  validate: async () => { throw new Error("unavailable"); },
  guest: async () => { throw new Error("unavailable"); },
};
