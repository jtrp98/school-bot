import { EngineError } from "./engineError";
import { t } from "../i18n";
import type { AppInfo, EngineResponse, ModeID, ProgramEngineData } from "./types";

// Isolate preview so a missing native bridge cannot silently fall back to simulation.
export const preview =
  new URLSearchParams(location.search).get("preview") === "1";
async function native<T>(method: string, ...args: unknown[]): Promise<T> {
  const { Call } = await import("@wailsio/runtime");
  return Call.ByName(`main.DesktopService.${method}`, ...args) as Promise<T>;
}

export const client = {
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
    if (!preview) await native("CancelRun");
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
};
