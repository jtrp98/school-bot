import { useState, useEffect, useCallback } from "react";
import { client } from "../core/client";
import { t as translate } from "../i18n";
import { Modal, Button, Banner } from "../components/shared";
import type { AppInfo, EnvironmentInfo } from "../core/types";

type Props = {
  open: boolean;
  onClose: () => void;
  onUpdateInfo?: (info: AppInfo) => void;
};

const devBodyClass =
  "flex flex-col gap-[12px] max-h-[calc(var(--app-viewport-height,_100vh)_-_240px)] overflow-auto text-ink";

const optionRowClass =
  "flex items-start gap-[10px] p-[10px] rounded-control border-2 border-solid border-line cursor-pointer hover:border-blue-line transition-colors";

const optionActiveRowClass =
  "flex items-start gap-[10px] p-[10px] rounded-control border-2 border-solid border-blue-tint-active bg-blue-tint cursor-pointer";

const labelClass = "flex flex-col gap-[2px] grow min-w-0";

const titleClass = "font-heading font-bold text-[14px] leading-[1.3]";

const urlClass = "text-[12px] text-muted font-mono break-all";

const inputClass =
  "w-full px-[10px] py-[6px] rounded-[6px] border border-solid border-line font-mono text-[13px] bg-surface focus:outline-none focus:border-blue-line";

const msgClass =
  "notice whitespace-pre-wrap [overflow-wrap:anywhere] border-2 border-solid border-blue-line bg-blue-tint text-blue-press rounded-control py-[8px] px-[12px] text-[13px] m-0";

const errClass =
  "notice whitespace-pre-wrap [overflow-wrap:anywhere] border-2 border-solid border-err-line bg-err-tint text-err-ink rounded-control py-[8px] px-[12px] text-[13px] m-0";

export function DevEnvironmentDialog({ open, onClose, onUpdateInfo }: Props) {
  const [envInfo, setEnvInfo] = useState<EnvironmentInfo | null>(null);
  const [selectedEnv, setSelectedEnv] = useState<"local" | "dev" | "prod" | "custom">("local");
  const [customURL, setCustomURL] = useState("");
  const [busy, setBusy] = useState(false);
  const [statusMsg, setStatusMsg] = useState<string | null>(null);
  const [errMsg, setErrMsg] = useState<string | null>(null);

  const refreshEnvironment = useCallback(async () => {
    try {
      const info = await client.getEnvironment();
      setEnvInfo(info);
      setSelectedEnv(info.active_env);
      if (info.active_env === "custom") {
        setCustomURL(info.current_url);
      }
    } catch {
      // Ignored if unavailable in preview
    }
  }, []);

  useEffect(() => {
    if (open) {
      setStatusMsg(null);
      setErrMsg(null);
      void refreshEnvironment();
    }
  }, [open, refreshEnvironment]);

  if (!open) return null;

  const handleApply = async () => {
    setBusy(true);
    setStatusMsg(null);
    setErrMsg(null);
    try {
      const targetCustom = selectedEnv === "custom" ? customURL.trim() : "";
      const updatedInfo = await client.setEnvironment(selectedEnv, targetCustom);
      onUpdateInfo?.(updatedInfo);
      setStatusMsg(translate("devEnv.success", { url: updatedInfo.api_url }));
      await refreshEnvironment();
    } catch (err) {
      const message = err instanceof Error ? err.message : String(err);
      setErrMsg(translate("devEnv.error", { error: message }));
    } finally {
      setBusy(false);
    }
  };

  const handleReset = async () => {
    setBusy(true);
    setStatusMsg(null);
    setErrMsg(null);
    try {
      const updatedInfo = await client.resetEnvironment();
      onUpdateInfo?.(updatedInfo);
      setStatusMsg(translate("devEnv.success", { url: updatedInfo.api_url }));
      await refreshEnvironment();
    } catch (err) {
      const message = err instanceof Error ? err.message : String(err);
      setErrMsg(translate("devEnv.error", { error: message }));
    } finally {
      setBusy(false);
    }
  };

  const presets = envInfo?.presets ?? {
    local: "http://localhost:8080",
    dev: "https://api-dev.schoolbright.co",
    prod: "https://api.schoolbright.co",
  };

  return (
    <Modal
      fixed
      icon="cog"
      tone="warn"
      width={560}
      title={translate("devEnv.title")}
      titleId="dev-env-title"
      actions={
        <>
          <Button variant="secondary" size="md" onClick={handleReset} disabled={busy}>
            {translate("devEnv.reset")}
          </Button>
          <Button variant="secondary" size="md" onClick={onClose} disabled={busy}>
            {translate("devEnv.close")}
          </Button>
          <Button variant="primary" size="md" onClick={handleApply} loading={busy} disabled={busy}>
            {translate("devEnv.apply")}
          </Button>
        </>
      }
    >
      <div className={devBodyClass}>
        <Banner kind="warn" title={translate("devEnv.warning")}>
          {translate("devEnv.description")}
        </Banner>

        <div
          role="radio"
          aria-checked={selectedEnv === "local"}
          tabIndex={0}
          className={selectedEnv === "local" ? optionActiveRowClass : optionRowClass}
          onClick={() => setSelectedEnv("local")}
        >
          <input
            type="radio"
            name="env"
            checked={selectedEnv === "local"}
            onChange={() => setSelectedEnv("local")}
            className="mt-[3px]"
          />
          <div className={labelClass}>
            <span className={titleClass}>{translate("devEnv.local")}</span>
            <span className={urlClass}>{presets.local}</span>
          </div>
        </div>

        <div
          role="radio"
          aria-checked={selectedEnv === "dev"}
          tabIndex={0}
          className={selectedEnv === "dev" ? optionActiveRowClass : optionRowClass}
          onClick={() => setSelectedEnv("dev")}
        >
          <input
            type="radio"
            name="env"
            checked={selectedEnv === "dev"}
            onChange={() => setSelectedEnv("dev")}
            className="mt-[3px]"
          />
          <div className={labelClass}>
            <span className={titleClass}>{translate("devEnv.dev")}</span>
            <span className={urlClass}>{presets.dev}</span>
          </div>
        </div>

        <div
          role="radio"
          aria-checked={selectedEnv === "prod"}
          tabIndex={0}
          className={selectedEnv === "prod" ? optionActiveRowClass : optionRowClass}
          onClick={() => setSelectedEnv("prod")}
        >
          <input
            type="radio"
            name="env"
            checked={selectedEnv === "prod"}
            onChange={() => setSelectedEnv("prod")}
            className="mt-[3px]"
          />
          <div className={labelClass}>
            <span className={titleClass}>{translate("devEnv.prod")}</span>
            <span className={urlClass}>{presets.prod}</span>
          </div>
        </div>

        <div
          role="radio"
          aria-checked={selectedEnv === "custom"}
          tabIndex={0}
          className={selectedEnv === "custom" ? optionActiveRowClass : optionRowClass}
          onClick={() => setSelectedEnv("custom")}
        >
          <input
            type="radio"
            name="env"
            checked={selectedEnv === "custom"}
            onChange={() => setSelectedEnv("custom")}
            className="mt-[3px]"
          />
          <div className={labelClass}>
            <span className={titleClass}>{translate("devEnv.custom")}</span>
            {selectedEnv === "custom" && (
              <input
                type="text"
                value={customURL}
                placeholder={translate("devEnv.customPlaceholder")}
                onChange={(e) => setCustomURL(e.target.value)}
                onClick={(e) => e.stopPropagation()}
                className={inputClass}
              />
            )}
          </div>
        </div>

        {statusMsg && (
          <pre className={msgClass} role="status">
            {statusMsg}
          </pre>
        )}
        {errMsg && (
          <pre className={errClass} role="alert">
            {errMsg}
          </pre>
        )}
      </div>
    </Modal>
  );
}
