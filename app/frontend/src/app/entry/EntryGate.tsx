import { useTranslation } from "react-i18next";
import type { ReactNode } from "react";
import schoolBotIcon from "../../assets/school-bot-icon.png";
import { WindowControlsSlot } from "../../components/shared/WindowTitlebar";
import { EntryScreen } from "./EntryScreen";
import type { EntryAdapter } from "./types";
import type { useEntry } from "./useEntry";

export function EntryGate({ entry, adapter, language, preview }: {
  entry: ReturnType<typeof useEntry>;
  adapter: EntryAdapter;
  language: ReactNode;
  preview: boolean;
}) {
  const { t } = useTranslation();
  return (
    <div className="flex h-full min-h-0 flex-col bg-bg">
      <header className="window-integrated-header flex min-h-[64px] shrink-0 items-center gap-[16px] bg-surface px-[16px] max-sm:flex-wrap">
        <img src={schoolBotIcon} alt="" className="h-[40px] w-[40px] object-contain" />
        <span className="font-heading text-[20px] font-bold max-sm:sr-only">{t("entry.brand")}</span>
        <div className="ml-auto min-w-0 max-sm:order-last max-sm:w-full">{language}</div>
        <WindowControlsSlot />
      </header>
      {preview && (
        <p role="status" className="m-0 bg-orange-tint px-[16px] py-[8px] text-orange-deep">
          {t("shell.messages.browserSimulation")}
        </p>
      )}
      <main className="min-h-0 flex-1 overflow-auto [&_.sb-btn]:whitespace-normal [&_.sb-input]:motion-reduce:transition-none">
        <EntryScreen
          status={entry.status}
          busy={entry.busy}
          recovering={entry.recovering}
          error={entry.error}
          loginAvailable={!!adapter.login}
          offlineAvailable={!!adapter.useOffline}
          onActivate={entry.activate}
          onValidate={entry.validate}
          onOffline={entry.useOffline}
          onGuest={entry.guest}
          onLogin={entry.login}
          onRetryRecovery={entry.retryRecovery}
        />
      </main>
    </div>
  );
}
