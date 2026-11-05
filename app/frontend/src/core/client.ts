import { robotSimulator, simulatorEnabled } from "./robotSimulator";
import { EngineError } from "./engineError";
import { t } from "../i18n";
import type { AppInfo, EngineResponse, ModeID, ProgramEngineData } from "./types";
import {
  InMemoryProjectLibraryBackend,
  type ExportBundleV2,
  type ImportGuestResultV2,
  type ProjectContent,
  type ProjectDocumentV2,
  type ProjectMode,
  type ProjectSummaryV2,
  type TrashItemV2,
} from "./projectStoreV2";
import type { LoginResult, PublicEntitlement } from "../app/entry/types";

// Isolate preview so a missing native bridge cannot silently fall back to simulation.
export const preview = simulatorEnabled ||
  new URLSearchParams(location.search).get("preview") === "1";

async function native<T>(method: string, ...args: unknown[]): Promise<T> {
  const { Call } = await import("@wailsio/runtime");
  return Call.ByName(`main.DesktopService.${method}`, ...args) as Promise<T>;
}

const previewProjectBackend = new InMemoryProjectLibraryBackend();
let previewSessionScope = "guest";

export const client = {
  learningScope: (): Promise<string> => preview ? Promise.resolve(`preview:${previewSessionScope}`) : native("LearningScope"),
  learningRequest: async <T>(request: import("../learning/types").LearningRequest): Promise<{ data: T; etag: string }> => {
    if (preview) throw new Error("LEARNING_PREVIEW_UNAVAILABLE");
    return JSON.parse(await native<string>("LearningRequest", JSON.stringify(request))) as { data: T; etag: string };
  },
  onCloseRequested: (listener: () => void): (() => void) => {
    let active = true;
    let unsubscribe: (() => void) | undefined;
    if (!preview) {
      void import("@wailsio/runtime").then(({ Events }) => {
        if (active) unsubscribe = Events.On("school-bot:close-requested", listener);
      });
    }
    return () => {
      active = false;
      unsubscribe?.();
    };
  },
  isFullscreen: async (): Promise<boolean> => {
    if (preview) return false;
    const { Window } = await import("@wailsio/runtime");
    return Window.IsFullscreen();
  },
  setFullscreen: async (fullscreen: boolean): Promise<void> => {
    if (preview) return;
    const { Window } = await import("@wailsio/runtime");
    if (fullscreen) await Window.Fullscreen();
    else await Window.UnFullscreen();
  },
  minimizeWindow: async (): Promise<void> => {
    if (preview) return;
    const { Window } = await import("@wailsio/runtime");
    await Window.Minimise();
  },
  closeWindow: async (): Promise<void> => {
    if (!preview) await native("CloseWindow");
  },
  info: (): Promise<AppInfo> =>
    preview
      ? Promise.resolve({
          version: "preview",
          platform: "browser-preview",
          api_url: "Desktop only",
          runtime: "Browser simulation · no native IPC",
        })
      : native("Info"),
  run: async <TData = ProgramEngineData>(
    mode: ModeID,
    action: string,
    data: unknown,
  ): Promise<EngineResponse<TData>> => {
    if (simulatorEnabled && mode === "robot-control")
      return { protocol: "1.0", request_id: crypto.randomUUID(), status: "success", ...await robotSimulator.run(mode, action, data) } as EngineResponse<TData>;
    if (preview)
      return (await import("./preview")).runPreview(mode, action, data) as EngineResponse<TData>;
    const result = await native<EngineResponse<TData>>(
      "Run",
      mode,
      action,
      JSON.stringify(data),
    );
    if (result.status === "error")
      throw new EngineError(result.error?.code ?? "ENGINE_FAILED", result.error?.message ?? "");
    return result;
  },
  cancel: async () => {
    if (simulatorEnabled) await robotSimulator.cancel();
    else if (!preview) await native("CancelRun");
  },
  save: async (mode: ModeID, data: unknown) => {
    if (preview) localStorage.setItem(`school-bot:project:${mode}`, JSON.stringify(data));
    else await native("SaveProject", mode, JSON.stringify(data));
  },
  load: async (mode: ModeID): Promise<unknown> => {
    const value = preview
      ? localStorage.getItem(`school-bot:project:${mode}`)
      : await native<string>("LoadProject", mode);
    return value ? JSON.parse(value) : null;
  },
  updates: async (): Promise<string> => {
    if (preview)
      throw new Error(t("shell.messages.previewUpdatesUnavailable"));
    return native("CheckUpdates");
  },
  activate: async (key: string): Promise<string> => {
    if (preview)
      throw new Error(t("shell.messages.previewActivationUnavailable"));
    return native("ActivateLicense", key);
  },
  licenseStatus: async (): Promise<PublicEntitlement> => {
    if (preview) {
      return {
        status: "valid",
        expiresAt: new Date(Date.now() + 7 * 24 * 3600 * 1000).toISOString(),
      };
    }
    const raw = await native<unknown>("LicenseStatus");
    return typeof raw === "string"
      ? (JSON.parse(raw) as PublicEntitlement)
      : (raw as PublicEntitlement);
  },
  activateLicense: async (key: string): Promise<PublicEntitlement> => {
    if (preview) {
      throw new Error(t("shell.messages.previewActivationUnavailable"));
    }
    const raw = await native<string>("ActivateLicense", key);
    return JSON.parse(raw) as PublicEntitlement;
  },
  validateLicense: async (): Promise<PublicEntitlement> => {
    if (preview) {
      return {
        status: "valid",
        expiresAt: new Date(Date.now() + 7 * 24 * 3600 * 1000).toISOString(),
      };
    }
    const raw = await native<string>("ValidateLicense", "");
    return JSON.parse(raw) as PublicEntitlement;
  },
  loginSession: async (username: string, password: string): Promise<LoginResult> => {
    if (preview) {
      if (!username.trim() || !password) return "invalid_credentials";
      previewSessionScope = username.trim();
      return "success";
    }
    const res = await native<string>("LoginSession", username, password);
    if (res === "success" || res === "invalid_credentials" || res === "network" || res === "unavailable" || res === "verification_required" || res === "password_reset_required") {
      return res;
    }
    return "unavailable";
  },
  logoutSession: async (): Promise<void> => {
    if (preview) {
      previewSessionScope = "guest";
      return;
    }
    await native("LogoutSession");
  },
  currentSession: async (): Promise<string> => {
    if (preview) return previewSessionScope;
    return native<string>("CurrentSession");
  },
  listProjectsV2: async (mode: ProjectMode | "" = ""): Promise<ProjectSummaryV2[]> => {
    if (preview) {
      return previewProjectBackend.listProjects(previewSessionScope, mode);
    }
    const raw = await native<string>("ListProjectsV2", mode);
    return JSON.parse(raw) as ProjectSummaryV2[];
  },
  createProjectV2: async (name: string, mode: ProjectMode, content: ProjectContent): Promise<ProjectDocumentV2> => {
    if (preview) {
      return previewProjectBackend.createProject(previewSessionScope, name, mode, content);
    }
    const raw = await native<string>("CreateProjectV2", JSON.stringify({ name, mode, content }));
    return JSON.parse(raw) as ProjectDocumentV2;
  },
  getProjectV2: async (projectId: string): Promise<ProjectDocumentV2> => {
    if (preview) {
      return previewProjectBackend.getProject(previewSessionScope, projectId);
    }
    const raw = await native<string>("GetProjectV2", projectId);
    return JSON.parse(raw) as ProjectDocumentV2;
  },
  saveProjectV2: async (
    projectId: string,
    expectedLocalRevision: number,
    name: string,
    mode: ProjectMode,
    content: ProjectContent,
  ): Promise<ProjectDocumentV2> => {
    if (preview) {
      return previewProjectBackend.saveProject(
        previewSessionScope,
        projectId,
        expectedLocalRevision,
        name,
        mode,
        content,
      );
    }
    const raw = await native<string>(
      "SaveProjectV2",
      JSON.stringify({
        project_id: projectId,
        expected_local_revision: expectedLocalRevision,
        name,
        mode,
        content,
      }),
    );
    return JSON.parse(raw) as ProjectDocumentV2;
  },
  copyProjectV2: async (sourceProjectId: string, newName: string): Promise<ProjectDocumentV2> => {
    if (preview) {
      return previewProjectBackend.copyProject(previewSessionScope, sourceProjectId, newName);
    }
    const raw = await native<string>("CopyProjectV2", sourceProjectId, newName);
    return JSON.parse(raw) as ProjectDocumentV2;
  },
  deleteProjectV2: async (projectId: string): Promise<TrashItemV2> => {
    if (preview) {
      return previewProjectBackend.deleteProject(previewSessionScope, projectId);
    }
    const raw = await native<string>("DeleteProjectV2", projectId);
    return JSON.parse(raw) as TrashItemV2;
  },
  listTrashV2: async (): Promise<TrashItemV2[]> => {
    if (preview) {
      return previewProjectBackend.listTrash(previewSessionScope);
    }
    const raw = await native<string>("ListTrashV2");
    return JSON.parse(raw) as TrashItemV2[];
  },
  restoreProjectV2: async (projectId: string): Promise<ProjectDocumentV2> => {
    if (preview) {
      return previewProjectBackend.restoreProject(previewSessionScope, projectId);
    }
    const raw = await native<string>("RestoreProjectV2", projectId);
    return JSON.parse(raw) as ProjectDocumentV2;
  },
  exportProjectV2: async (projectId: string): Promise<ExportBundleV2> => {
    if (preview) {
      return previewProjectBackend.exportProject(previewSessionScope, projectId);
    }
    const raw = await native<string>("ExportProjectV2", projectId);
    return JSON.parse(raw) as ExportBundleV2;
  },
  openExportedProjectV2: async (rawJson: string): Promise<ProjectDocumentV2> => {
    if (preview) {
      return previewProjectBackend.openExportedProject(previewSessionScope, rawJson);
    }
    const raw = await native<string>("OpenExportedProjectV2", rawJson);
    return JSON.parse(raw) as ProjectDocumentV2;
  },
  importGuestProjectV2: async (guestProjectId: string, idempotencyKey: string): Promise<ImportGuestResultV2> => {
    if (preview) {
      return previewProjectBackend.importGuestProject(previewSessionScope, guestProjectId, idempotencyKey);
    }
    const raw = await native<string>("ImportGuestProjectV2", guestProjectId, idempotencyKey);
    return JSON.parse(raw) as ImportGuestResultV2;
  },
};
