import { AppView } from "./app/AppView";
import { initialView, syncUrl } from "./app/appData";
import { t as translate, languages as langs } from "./i18n";
import { useTranslation } from "react-i18next";
import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { client, preview } from "./core/client";
import { param } from "./core/debugParams";
import type { AppInfo } from "./core/types";
import { Seg } from "./components/shared";

import { prepareWindowClose } from "./core/windowLifecycle";
import { useWindowClose, type CloseActions } from "./app/useWindowClose";
import { SAMPLE_STARS, STUDENT_CLASS, WEEK_STARS, useSampleData, type Lang, type ShellApi, type View } from "./shell";

const langSegClass = [
  "sh-seg-full [&_.sb-seg]:flex [&_.sb-seg]:w-full [&_.sb-seg_button]:[flex:1] [&_.sb-seg_button]:py-0",
  "[&_.sb-seg_button]:px-[8px]",
].join(" ");

export function useAppScreen(closeActions?: CloseActions) {
  const [view, setView] = useState<View>(initialView);
  const [nonce, setNonce] = useState(0);

  const [armScreen, setArmScreen] = useState<string>(() => param("screen") ?? "works");
  const [info, setInfo] = useState<AppInfo>();
  const [message, setMessage] = useState("");
  const [cloudBusy, setCloudBusy] = useState(false);
  const [settings, setSettings] = useState(false);
  const [menuOpen, setMenuOpen] = useState(false);
  const [profile, setProfile] = useState(false);
  const { i18n } = useTranslation();
  const lang = i18n.resolvedLanguage as Lang;
  const [pending, setPending] = useState<{ view: View; screen?: string }>();
  const desktop = !!info && info.platform !== "browser-preview" && !preview;
  const close = useWindowClose(closeActions ?? {
    requestStop: async onWait => {
      await prepareWindowClose(onWait);
      // The host's cancel waits for a running upload. It may still run when the page that
      // started it reloaded or crashed and took its own close guard and note along.
      if (await client.uploadRunning()) onWait("blockLab.send.guard.closeWait");
      await client.cancel();
    },
    close: client.closeWindow,
  }, desktop || !!closeActions);
  const { closeRequested, setCloseRequested, closeBusy, closeWindow } = close;
  const closeGuard = useRef<(() => void) | null>(null);
  // A mode's close guard keeps the closeApp it was handed, so closeApp reads the latest
  // closeWindow (and the state it closes over) through this ref.
  const latestCloseWindow = useRef(closeWindow);
  latestCloseWindow.current = closeWindow;
  // Home holds no work. A mode's own guard asks about its work; otherwise the app's dialog does.
  function requestClose() {
    if (closeBusy) return;
    if (view === "home") void closeWindow();
    else if (closeGuard.current) closeGuard.current();
    else setCloseRequested(true);
  }
  const sample = useSampleData();
  const stars = sample ? SAMPLE_STARS : WEEK_STARS;
  const classLine = `${sample ? `${STUDENT_CLASS} · ` : ""}${translate("profile.stars", { count: stars })}`;

  useEffect(() => {
    client
      .info()
      .then(setInfo)
      .catch(() =>
        setMessage(
          "Native bridge unavailable. Use the desktop executable, or open /?preview=1 for explicit UI simulation.",
        ),
      );
  }, []);
  useEffect(() => {
    document.documentElement.lang = lang;
  }, [lang]);
  useEffect(() => {
    function onKey(event: KeyboardEvent) {
      if (event.key !== "Escape") return;
      if (!(closeRequested || settings || pending || profile || menuOpen)) return;
      // App overlays own this Escape; do not pass it to a mode's stop shortcut.
      event.preventDefault();
      event.stopImmediatePropagation();
      if (closeRequested) {
        if (!closeBusy) setCloseRequested(false);
      }
      else if (settings) setSettings(false);
      else if (pending) setPending(undefined);
      else if (profile) setProfile(false);
      else if (menuOpen) setMenuOpen(false);
    }
    window.addEventListener("keydown", onKey, true);
    return () => window.removeEventListener("keydown", onKey, true);
  }, [settings, profile, pending, menuOpen, closeRequested, closeBusy]);

  useEffect(() => {
    if (!desktop) return;
    return client.onCloseRequested(requestClose);
  }, [desktop, closeBusy, view]);

  async function cloud(action: () => Promise<string>) {
    setCloudBusy(true);
    setMessage("");
    try {
      const result = JSON.parse(await action());
      setMessage(JSON.stringify(result, null, 2));
    } catch (error) {
      setMessage(String(error));
    } finally {
      setCloudBusy(false);
    }
  }
  const setLang = useCallback((next: Lang) => { void i18n.changeLanguage(next); }, [i18n]);
  const apply = useCallback((next: View, screen?: string) => {
    syncUrl(screen);
    setArmScreen(screen ?? "works");
    setView(next);
    setNonce((n) => n + 1);
  }, []);
  const go = useCallback(
    (next: View, screen?: string, confirmed?: boolean) => {
      setProfile(false);
      setMenuOpen(false);
      if (next === view && !screen) return;
      // Home and Arm Lab's project list hold no unsaved work, so only leaving a mode mid-work needs confirmation.
      if (confirmed || view === "home" || (view === "robot-control" && armScreen === "works")) apply(next, screen);
      else setPending({ view: next, screen });
    },
    [view, armScreen, apply],
  );
  // The host has one engine slot, so a mode's running request is cancelled before the next
  // view opens instead of holding the slot until its deadline.
  const leaving = useRef(false);
  const leaveMode = useCallback(async (next: View, screen?: string) => {
    // A second click while the cancel is in flight would open the next view twice.
    if (leaving.current) return;
    leaving.current = true;
    try {
      await client.cancel().catch(error => console.warn("Unable to cancel before switching modes", error));
      apply(next, screen);
    } finally {
      leaving.current = false;
    }
  }, [apply]);
  async function confirmSwitch() {
    if (!pending) return;
    await leaveMode(pending.view, pending.screen);
    setPending(undefined);
  }

  const shell = useMemo<ShellApi>(
    () => ({
      go,
      leaveMode: (next: View, screen?: string) => void leaveMode(next, screen),
      menuOpen,
      openMenu: () => {
        setProfile(false);
        setMenuOpen(true);
      },
      closeMenu: () => setMenuOpen(false),
      profileOpen: profile,
      openProfile: () => setProfile(true),
      closeProfile: () => setProfile(false),
      openSettings: () => {
        setProfile(false);
        setMenuOpen(false);
        setSettings(true);
      },
      lang,
      setLang,
      setArmScreen,
      setCloseGuard: (guard: (() => void) | null) => {
        closeGuard.current = guard;
      },
      closeApp: () => void latestCloseWindow.current(),
    }),
    [go, leaveMode, menuOpen, profile, lang, setLang],
  );

  const langName = langs.find((l) => l.value === lang)?.label ?? lang;
  const langSeg = (
    <div className={langSegClass}>
      <Seg value={lang} options={langs} onChange={setLang} label={translate("language.label")} />
    </div>
  );
  const full = (view === "robot-control" && armScreen !== "works") || view === "block-programming";

  return {
    view,
    nonce,
    info,
    message,
    setMessage,
    cloudBusy,
    settings,
    setSettings,
    menuOpen,
    setMenuOpen,
    profile,
    setProfile,
    pending,
    setPending,
    ...close,
    requestClose,
    desktop,
    classLine,
    cloud,
    go,
    confirmSwitch,
    shell,
    langName,
    langSeg,
    full,
  };
}

export default function App(props: { closeActions?: CloseActions }) {
  return <AppView v={useAppScreen(props.closeActions)} />;
}
