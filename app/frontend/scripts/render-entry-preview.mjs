import { readFile, readdir, mkdir, writeFile } from "node:fs/promises";
import { fileURLToPath } from "node:url";
import { createServer } from "vite";
import React from "react";
import { renderToStaticMarkup } from "react-dom/server";

const root = fileURLToPath(new URL("../", import.meta.url));
globalThis.location = { search: "" };
globalThis.localStorage = { getItem: () => null, setItem() {} };
const server = await createServer({
  configFile: false, root, cacheDir: "node_modules/.vite-entry-preview",
  optimizeDeps: { noDiscovery: true, include: [] },
  server: { middlewareMode: true, hmr: false }, appType: "custom",
});
const inertAction = async () => { throw new Error("Static preview has no host adapter"); };
try {
  const { default: i18n } = await server.ssrLoadModule("/src/i18n/index.ts");
  const { EntryScreen } = await server.ssrLoadModule("/src/app/entry/EntryScreen.tsx");
  const { CloseDialog } = await server.ssrLoadModule("/src/app/CloseDialog.tsx");
  const states = ["unactivated", "checking", "valid", "offline-valid", "expired", "denied", "clock_untrusted", "locked", "unavailable"];
  const errors = ["network", "DEVICE_LIMIT", "DEVICE_NOT_ACTIVATED", "DEVICE_PROOF_INVALID", "LICENSE_INVALID", "LICENSE_INACTIVE", "BINDING_REVOKED", "INVALID_ENTITLEMENT", "UNKNOWN_FORBIDDEN", "CREDENTIAL_STORE_FAILED", "CHALLENGE_EXPIRED", "unavailable"];
  const base = {
    busy: false, loginAvailable: false, offlineAvailable: false,
    onActivate: inertAction, onValidate: inertAction, onOffline: inertAction,
    onGuest: inertAction, onLogin: inertAction, onRetryRecovery: inertAction,
  };
  const expiresAt = "2026-10-28T10:00:00Z";
  const panels = [];
  for (const language of ["th", "en", "zh"]) {
    await i18n.changeLanguage(language);
    const fixtures = [
      ...states.map(status => ({ name: status, status: { status, expiresAt } })),
      ...errors.map(error => ({ name: error, status: { status: "locked", error } })),
      { name: "offline-warning", status: { status: "offline-valid", expiresAt, expiringSoon: true } },
      { name: "offline-choice", status: { status: "checking", expiresAt, canUseOffline: true }, offlineAvailable: true },
      { name: "activation-loading", status: { status: "unactivated" }, busy: true },
      { name: "login-loading", status: { status: "valid", expiresAt }, loginAvailable: true, busy: true },
      { name: "login-error", status: { status: "valid", expiresAt }, loginAvailable: true, error: "invalid_credentials" },
      { name: "expiry-recovery", status: { status: "expired", expiresAt }, recovering: true, busy: true },
      { name: "expiry-recovery-failed", status: { status: "expired", expiresAt }, error: "recovery_failed" },
    ];
    for (const { name, ...fixture } of fixtures) {
      panels.push(`<article lang="${language}"><h2>${language} · ${name}</h2>${renderToStaticMarkup(React.createElement(EntryScreen, { ...base, ...fixture }))}</article>`);
    }
    for (const phase of ["confirmation", "saving", "stopping", "closing", "save-unavailable"]) {
      const failure = ["saving", "stopping", "closing"].includes(phase);
      panels.push(`<article lang="${language}" class="preview-close"><h2>${language} · ${phase}</h2>${renderToStaticMarkup(React.createElement(CloseDialog, {
        closeRequested: true, setCloseRequested() {}, closeBusy: false,
        closePhase: failure ? phase : "stopping", closeSaveRequested: phase === "saving",
        closeSaveAvailable: phase !== "save-unavailable",
        closeError: failure ? i18n.t(`entry.close.errors.${phase}`) : "", closeWindow: inertAction,
      }))}</article>`);
    }
  }
  const assets = await readdir(new URL("../dist/assets/", import.meta.url));
  const css = assets.find(name => /^index-.*\.css$/.test(name));
  if (!css) throw new Error("Run npm run build first");
  const icon = await readFile(new URL("../src/assets/school-bot-icon.png", import.meta.url));
  const html = `<!doctype html><html lang="th"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">
<title>ภาพตัวอย่าง Entry และ Close</title><link rel="stylesheet" href="../dist/assets/${css}">
<style>body{margin:0;background:var(--bg);color:var(--ink);font-family:var(--f-body)}
article{margin:24px auto;padding:24px;max-width:960px;background:var(--surface);border:2px solid var(--line);border-radius:var(--r-card)}
article>h2{margin:0 0 16px;font-size:16px}.preview-close .fixed{position:relative;min-height:480px}
article .sb-btn{white-space:normal}header{padding:24px}header img{width:40px;height:40px;vertical-align:middle}</style>
<header><img src="data:image/png;base64,${icon.toString("base64")}" alt=""> ภาพตัวอย่างแบบคงที่จาก test injection — ไม่มี host, network หรือ hardware callbacks</header>
${panels.join("\n")}</html>`;
  await mkdir(new URL("../reports/", import.meta.url), { recursive: true });
  await writeFile(new URL("../reports/entry-preview.html", import.meta.url), html);
  console.log(`สร้าง reports/entry-preview.html แล้ว (${panels.length} ภาพสถานะ, TH/EN/ZH); ยังไม่ได้ทดสอบ interaction หรือ hardware`);
} finally { await server.close(); }
