import { useTranslation } from "react-i18next";
import { Button, Field } from "../../components/shared";
import { Icon } from "../../components/shared/Icons";
import { LicenseStatus } from "./LicenseStatus";
import { type EntryActions, type EntryFeedback, type PublicEntitlement } from "./types";
import { useEntryForm } from "./useEntryForm";

export interface EntryScreenProps {
  status: PublicEntitlement;
  busy: boolean;
  recovering?: boolean;
  error?: EntryFeedback | "";
  loginAvailable: boolean;
  offlineAvailable: boolean;
  onActivate: EntryActions["activate"];
  onValidate: EntryActions["validate"];
  onOffline: () => Promise<void>;
  onGuest: () => Promise<void>;
  onRetryRecovery: () => Promise<void>;
  onLogin: (username: string, password: string) => Promise<void>;
}

export function EntryScreen(props: EntryScreenProps) {
  const { status, busy, recovering, error, loginAvailable, offlineAvailable } = props;
  const { t } = useTranslation();
  const {
    key, setKey, username, setUsername, password, setPassword, setActivating, heading,
    valid, checking, unavailable, firstRun, validKey, disabled, activate, login,
  } = useEntryForm(props);

  return (
    <section
      className="mx-auto box-border flex w-full max-w-[720px] flex-col gap-[24px] p-[24px] font-body text-ink"
      aria-busy={busy || checking}
    >
      <h1 ref={heading} tabIndex={-1} className="m-0 font-heading text-[28px] leading-[48px]">
        {t(valid ? "entry.choice.title" : firstRun ? "entry.activation.title" : "entry.lock.title")}
      </h1>
      {(valid || status.expiresAt) && <LicenseStatus status={status} />}
      {!valid && (
        <div role={checking ? "status" : "alert"} aria-live={checking ? "polite" : undefined}>
          {checking && <Icon name="loader" className="animate-spin motion-reduce:animate-none" />}
          <p className="m-0 text-[16px] leading-[32px]">{t(`entry.states.${status.status}`)}</p>
        </div>
      )}
      {status.error && (
        <p role="alert" className="m-0 text-[16px] leading-[32px] text-err-ink">
          {t(status.error === "network" && !firstRun
            ? "entry.errors.onlineCheckFailed" : `entry.errors.${status.error}`)}
        </p>
      )}
      {recovering && <p role="status">{t("entry.recovery.pending")}</p>}
      {error && <p role="alert" className="m-0 text-err-ink leading-[32px]">{t(`entry.feedback.${error}`)}</p>}
      {error === "recovery_failed" && (
        <Button variant="secondary" disabled={busy} onClick={() => void props.onRetryRecovery()}>
          {t("entry.recovery.retry")}
        </Button>
      )}
      {valid ? (
        <div className="grid grid-cols-1 gap-[24px] sm:grid-cols-2">
          <section className="flex flex-col gap-[16px] rounded-card border-2 border-solid border-line p-[24px]">
            <h2 className="m-0 text-[24px] leading-[40px]">{t("entry.choice.guestTitle")}</h2>
            <p className="m-0 text-[16px] leading-[32px]">{t("entry.choice.guestDescription")}</p>
            <Button variant="blue" disabled={busy} onClick={() => void props.onGuest()}>
              {t("entry.choice.guest")}
            </Button>
          </section>
          <form
            className="flex flex-col gap-[16px] rounded-card border-2 border-solid border-line p-[24px]"
            onSubmit={event => { event.preventDefault(); void login(); }}
          >
            <h2 className="m-0 text-[24px] leading-[40px]">{t("entry.choice.loginTitle")}</h2>
            <Field
              id="entry-username"
              label={t("entry.choice.username")}
              value={username}
              onChange={event => setUsername(event.target.value)}
              autoComplete="username"
              disabled={busy || !loginAvailable}
            />
            <Field
              id="entry-password"
              type="password"
              label={t("entry.choice.password")}
              value={password}
              onChange={event => setPassword(event.target.value)}
              autoComplete="current-password"
              disabled={busy || !loginAvailable}
            />
            {!loginAvailable && <p role="status" className="m-0 leading-[32px]">{t("entry.feedback.unavailable")}</p>}
            <Button
              type="submit"
              variant="blue"
              disabled={busy || !loginAvailable || !username.trim() || !password}
              loading={busy}
            >
              {t(busy ? "entry.choice.loading" : "entry.choice.login")}
            </Button>
          </form>
        </div>
      ) : firstRun ? (
        <form className="flex flex-col gap-[16px]" onSubmit={event => { event.preventDefault(); void activate(); }}>
          <Field
            id="entry-license-key"
            type="password"
            label={t("entry.activation.keyLabel")}
            hint={t("entry.activation.hint")}
            aria-describedby="entry-key-help"
            maxLength={256}
            autoComplete="off"
            spellCheck={false}
            value={key}
            disabled={disabled}
            onChange={event => setKey(event.target.value)}
          />
          <p id="entry-key-help" className="m-0 leading-[32px]">{t("entry.activation.online")}</p>
          <Button type="submit" variant="blue" disabled={disabled || !validKey} aria-disabled={disabled || !validKey}
            loading={busy}>
            {t(busy ? "entry.activation.loading" : "entry.activation.submit")}
          </Button>
        </form>
      ) : (
        <div className="flex flex-wrap gap-[16px]">
          <Button variant="blue" disabled={busy} loading={busy} onClick={() => void props.onValidate()}>
            {t("entry.lock.retry")}
          </Button>
          {!checking && !unavailable && (
            <Button variant="secondary" disabled={busy} onClick={() => setActivating(true)}>
              {t("entry.lock.reactivate")}
            </Button>
          )}
          {checking && status.canUseOffline && status.expiresAt && offlineAvailable && (
            <Button variant="secondary" disabled={busy} onClick={() => void props.onOffline()}>
              {t("entry.lock.useOffline")}
            </Button>
          )}
        </div>
      )}
    </section>
  );
}
