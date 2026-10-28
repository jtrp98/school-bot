import { useTranslation } from "react-i18next";
import type { PublicEntitlement } from "./types";

export function LicenseStatus({ status }: { status: PublicEntitlement }) {
  const { t, i18n } = useTranslation();
  const expiry = status.expiresAt ? new Date(status.expiresAt) : undefined;
  const expiresAt = expiry && Number.isFinite(expiry.getTime())
    ? new Intl.DateTimeFormat(i18n.resolvedLanguage, { dateStyle: "medium", timeStyle: "short" }).format(expiry)
    : t("entry.license.expiryUnknown");
  const offline = status.status === "offline-valid";
  const expiryKey = status.status === "valid" ? "entry.license.status" : "entry.license.expiry";
  return (
    <div
      role="status"
      aria-live="polite"
      className={`px-[16px] py-[8px] font-body text-[14px] leading-[28px] ${status.expiringSoon
        ? "bg-orange-tint text-orange-deep" : "bg-blue-tint text-blue-press"}`}
    >
      {t(offline ? "entry.license.offline" : expiryKey, { expiresAt })}
      {status.expiringSoon && <strong className="block">{t("entry.license.expiringSoon")}</strong>}
    </div>
  );
}
