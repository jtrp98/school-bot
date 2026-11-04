import { client } from "../../core/client";
import { prepareWindowClose } from "../../core/windowLifecycle";
import type { EntryAdapter, LoginResult, PublicEntitlement } from "./types";

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
      const next = await resolved.licenseStatus();
      lastStatus = next;
      return next;
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
      const next = await resolved.activateLicense(trimmed);
      emit(next);
    },
    validate: async () => {
      const next = await resolved.validateLicense();
      emit(next);
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
      const next = await resolved.licenseStatus();
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
