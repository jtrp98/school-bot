import { titles } from "./appData";
import { CloseDialog } from "./CloseDialog";
import { t as translate } from "../i18n";

import { client, preview } from "../core/client";

import { Banner, Button, Modal } from "../components/shared";

import type { AppView } from "./AppView";

const settingsBodyClass =
  "sh-settings flex flex-col gap-[12px] max-h-[calc(var(--app-viewport-height,_100vh)_-_260px)] overflow-auto " +
  "[&_p]:m-0";

const settingsRowClass =
  "sh-settings-row flex items-center gap-[12px] flex-wrap [&_.sb-grow]:min-w-[200px]";

const bodyTextClass =
  "sb-t-body not-italic [font-variant:normal] font-normal [font-stretch:normal] text-[16px] leading-[1.5] " +
  "font-body sb-muted text-muted";

const bodyPlainClass =
  "sb-t-body not-italic [font-variant:normal] font-normal [font-stretch:normal] text-[16px] leading-[1.5] font-body";

const hintClass =
  "sb-t-sm not-italic [font-variant:normal] font-normal [font-stretch:normal] text-[14px] leading-[1.45] font-body " +
  "sb-muted text-muted";

const settingsMsgClass =
  "notice whitespace-pre-wrap [overflow-wrap:anywhere] border-2 border-solid border-blue-line bg-blue-tint " +
  "text-blue-press rounded-control py-[12px] px-[16px] text-[13px] [&.error]:text-err-ink [&.error]:bg-err-tint " +
  "[&.error]:border-err-line sh-settings-msg m-0";

type Props = { v: Pick<Parameters<typeof AppView>[0]["v"],
  "info" | "message" | "cloudBusy" | "settings" | "setSettings" | "pending" | "setPending"
  | "closeRequested" | "setCloseRequested" | "closeBusy" | "closeError" | "closeErrorReason"
  | "closeWindow" | "closePhase" | "closeSaveAvailable" | "closeSaveRequested" | "cloud" | "confirmSwitch">;  };

// Preview reports a non-numeric marker, which must not be shown as a release number.
function formatVersion(version?: string): string {
  if (!version) return "";
  return /^\d/.test(version) ? `v${version}` : version;
}

export function AppDialogs({ v }: Props) {
 const {
   info, message, cloudBusy, settings, setSettings, pending, setPending, closeRequested,
   cloud, confirmSwitch,
 } = v;
 return (<>
{settings && (
          <Modal
            fixed
            icon="cog"
            tone="info"
            width={620}
            title={translate("shell.messages.appConnection")}
            titleId="settings-title"
            actions={
              <Button variant="secondary" size="md" onClick={() => setSettings(false)}>
                {translate("entry.close.cancel")}
              </Button>
            }
          >
            <div className={settingsBodyClass}>
              {preview && (
                <Banner kind="warn" title={translate("shell.messages.browserSimulation")}>
                  {translate("shell.messages.browserPreviewUsesAUiSimulatorNativeJsonIpc")}
                </Banner>
              )}
              <p className={bodyPlainClass}>
                {info?.runtime} · {info?.platform} · {formatVersion(info?.version)}
              </p>
              <p className={hintClass}>
                {translate("shell.messages.apiAddressOffline", {
    api: info?.api_url ?? translate("common.messages.notConnected"),
  })}
              </p>
              <div className={settingsRowClass}>
                <Button
                  variant="secondary"
                  size="md"
                  icon="refresh"
                  loading={cloudBusy}
                  disabled={cloudBusy}
                  onClick={() => cloud(client.updates)}
                >{translate("shell.messages.checkUpdateMetadata")}</Button>
              </div>
              {message && (
                <pre className={settingsMsgClass} role="status">
                  {translate(message)}
                </pre>
              )}
            </div>
          </Modal>
        )}
{pending && (
          <Modal
            fixed
            icon="warn"
            tone="warn"
            width={520}
            title={translate("modal.leave.title")}
            titleId="switch-title"
            actions={
              <>
                <Button
                  variant="secondary"
                  size="md"
                  autoFocus
                  onClick={() => setPending(undefined)}
                >{translate("modal.leave.stay")}</Button>
                <Button variant="primary" size="md" onClick={confirmSwitch}>
                  {translate("modal.leave.go", { page: translate(titles[pending.view]) })}
                </Button>
              </>
            }
          >
            <p className={bodyTextClass}>{translate("modal.leave.description")}</p>
          </Modal>
        )}
{closeRequested && <CloseDialog {...v} />}
</>);
}
