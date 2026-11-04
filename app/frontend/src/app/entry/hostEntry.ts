import { client } from "../../core/client";
import { prepareWindowClose } from "../../core/windowLifecycle";
import {
  allowsEntry,
  type EntitlementState,
  type EntryAdapter,
  type EntryError,
  type LoginResult,
  type PublicEntitlement,
} from "./types";

const VALID_STATES = new Set<EntitlementState>([
  "unactivated", "checking", "valid", "offline-valid", "expired",
  "denied", "clock_untrusted", "locked", "unavailable",
]);

const VALID_ERRORS = new Set<EntryError>([
  "network", "DEVICE_LIMIT", "DEVICE_NOT_ACTIVATED", "DEVICE_PROOF_INVALID",
  "LICENSE_INVALID", "LICENSE_INACTIVE", "BINDING_REVOKED", "INVALID_ENTITLEMENT",
  "UNKNOWN_FORBIDDEN", "CREDENTIAL_STORE_FAILED", "CHALLENGE_EXPIRED", "unavailable",
]);

export function normalizePublicEntitlement(
  raw: unknown,
  nowMs: number = Date.now(),
): PublicEntitlement {
  let parsed: unknown = raw;
  if (typeof raw === "string") {
    try {
      parsed = JSON.parse(raw);
    } catch {
      return { status: "unavailable", error: "unavailable" };
    }
  }
  if (!parsed || typeof parsed !== "object") {
    return { status: "unavailable", error: "unavailable" };
  }
  const obj = parsed as Record<string, unknown>;
  const rawStatus = typeof obj.status === "string" ? obj.status : "unavailable";
  const status: EntitlementState = VALID_STATES.has(rawStatus as EntitlementState)
    ? (rawStatus as EntitlementState)
    : "unavailable";
  const expiresAt = typeof obj.expiresAt === "string"
    ? obj.expiresAt
    : typeof obj.expires_at === "string"
      ? obj.expires_at
      : undefined;
  const result: PublicEntitlement = { status };
  if (expiresAt) {
    result.expiresAt = expiresAt;
  }
  if (typeof obj.expiringSoon === "boolean") {
    result.expiringSoon = obj.expiringSoon;
  } else if (expiresAt && (status === "valid" || status === "offline-valid")) {
    const expiryMs = Date.parse(expiresAt);
    const remainingMs = expiryMs - nowMs;
    if (Number.isFinite(remainingMs) && remainingMs > 0 && remainingMs <= 24 * 3600 * 1000) {
      result.expiringSoon = true;
    }
  }
  const rawError = typeof obj.error === "string"
    ? obj.error
    : typeof obj.error_code === "string"
      ? obj.error_code
      : undefined;
  if (rawError && VALID_ERRORS.has(rawError as EntryError)) {
    result.error = rawError as EntryError;
  }
  if (typeof obj.canUseOffline === "boolean") {
    result.canUseOffline = obj.canUseOffline;
  }
  return result;
}

export function parseHostLicenseError(
  err: unknown,
  currentStatus: PublicEntitlement = { status: "unactivated" },
): PublicEntitlement | null {
  let code = "";
  let message = "";
  if (err && typeof err === "object" && typeof (err as { code?: unknown }).code === "string") {
    code = (err as { code: string }).code.trim();
    message = typeof (err as { message?: unknown }).message === "string"
      ? (err as { message: string }).message
      : "";
  } else {
    const text = err instanceof Error ? err.message : typeof err === "string" ? err : "";
    const match = /^\[([A-Z_]+)\]\s*(.*)$/.exec(text.trim());
    if (match) {
      code = match[1];
      message = match[2];
    }
  }
  if (!code) return null;

  const isFirstRun = currentStatus.status === "unactivated";
  switch (code) {
    case "DEVICE_LIMIT":
    case "LICENSE_INVALID":
    case "LICENSE_INACTIVE":
    case "DEVICE_NOT_ACTIVATED":
    case "DEVICE_PROOF_INVALID":
      return {
        status: isFirstRun ? "unactivated" : "denied",
        error: code as EntryError,
      };
    case "BINDING_REVOKED":
    case "UNKNOWN_FORBIDDEN":
      return {
        status: "denied",
        error: code as EntryError,
      };
    case "INVALID_ENTITLEMENT":
    case "CREDENTIAL_STORE_FAILED":
    case "CHALLENGE_EXPIRED":
      return {
        status: isFirstRun ? "unactivated" : "locked",
        error: code as EntryError,
      };
    case "CLOCK_UNTRUSTED":
      return {
        status: "clock_untrusted",
      };
    case "STORE_UNAVAILABLE":
    case "NETWORK": {
      if (allowsEntry(currentStatus)) {
        return {
          status: "offline-valid",
          expiresAt: currentStatus.expiresAt,
          expiringSoon: currentStatus.expiringSoon,
          error: "network",
        };
      }
      if (message.toLowerCase().includes("credential store")) {
        return {
          status: isFirstRun ? "unactivated" : "locked",
          error: "CREDENTIAL_STORE_FAILED",
        };
      }
      return {
        status: isFirstRun ? "unactivated" : "locked",
        error: "network",
      };
    }
    case "CONFIG_UNAVAILABLE":
    case "VERIFIER_UNAVAILABLE":
      return {
        status: "unavailable",
        error: "unavailable",
      };
    default:
      return null;
  }
}

export interface HostEntryDependencies {
  licenseStatus: () => Promise<PublicEntitlement>;
  activateLicense: (key: string) => Promise<PublicEntitlement>;
  validateLicense: () => Promise<PublicEntitlement>;
  loginSession: (username: string, password: string) => Promise<LoginResult>;
  logoutSession: () => Promise<void>;
  stopAndSaveActiveWork: () => Promise<void>;
}

const defaultDependencies: HostEntryDependencies = {
  licenseStatus: () => client.licenseStatus(),
  activateLicense: (key) => client.activateLicense(key),
  validateLicense: () => client.validateLicense(),
  loginSession: (username, password) => client.loginSession(username, password),
  logoutSession: () => client.logoutSession(),
  stopAndSaveActiveWork: async () => {
    await prepareWindowClose();
    await client.cancel();
  },
};

export interface HostEntryController extends EntryAdapter {
  logout: () => Promise<void>;
  notifyStatus: (status: PublicEntitlement) => void;
}

export function createHostEntryAdapter(
  deps: Partial<HostEntryDependencies> = {},
): HostEntryController {
  const resolved: HostEntryDependencies = { ...defaultDependencies, ...deps };
  const listeners = new Set<(status: PublicEntitlement) => void>();
  let lastStatus: PublicEntitlement = { status: "checking" };

  const emit = (next: PublicEntitlement) => {
    lastStatus = next;
    for (const listener of [...listeners]) {
      listener(next);
    }
  };

  return {
    readStatus: async () => {
      try {
        const raw = await resolved.licenseStatus();
        const next = normalizePublicEntitlement(raw);
        lastStatus = next;
        return next;
      } catch (err) {
        const mapped = parseHostLicenseError(err, lastStatus);
        if (mapped) {
          lastStatus = mapped;
          return mapped;
        }
        throw err;
      }
    },
    subscribe: (listener) => {
      listeners.add(listener);
      return () => {
        listeners.delete(listener);
      };
    },
    activate: async (key: string) => {
      const trimmed = key.trim();
      if (!trimmed || trimmed.length > 256 || /\s/.test(trimmed)) {
        throw new Error("Invalid license key");
      }
      try {
        const raw = await resolved.activateLicense(trimmed);
        const next = normalizePublicEntitlement(raw);
        emit(next);
      } catch (err) {
        const mapped = parseHostLicenseError(err, lastStatus);
        if (mapped) {
          emit(mapped);
          return;
        }
        throw err;
      }
    },
    validate: async () => {
      try {
        const raw = await resolved.validateLicense();
        const next = normalizePublicEntitlement(raw);
        emit(next);
      } catch (err) {
        const mapped = parseHostLicenseError(err, lastStatus);
        if (mapped) {
          emit(mapped);
          return;
        }
        throw err;
      }
    },
    useOffline: async () => {
      if (lastStatus.status === "checking" && lastStatus.canUseOffline && lastStatus.expiresAt) {
        emit({
          status: "offline-valid",
          expiresAt: lastStatus.expiresAt,
          expiringSoon: lastStatus.expiringSoon,
        });
        return;
      }
      const raw = await resolved.licenseStatus();
      const next = normalizePublicEntitlement(raw);
      emit(next);
    },
    guest: async () => {
      await resolved.logoutSession();
    },
    login: async (username: string, password: string): Promise<LoginResult> => {
      const trimmed = username.trim();
      if (!trimmed || !password) {
        return "invalid_credentials";
      }
      return resolved.loginSession(trimmed, password);
    },
    logout: async () => {
      await resolved.logoutSession();
    },
    saveAndRequestStop: async () => {
      await resolved.stopAndSaveActiveWork();
    },
    notifyStatus: (status: PublicEntitlement) => {
      emit(status);
    },
  };
}

export const hostEntryAdapter: HostEntryController = createHostEntryAdapter();
