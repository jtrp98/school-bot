import { AppDialogs } from "./AppDialogs";
import { CloseDialog } from "./CloseDialog";
import { EntryGate } from "./entry/EntryGate";
import { LicenseStatus } from "./entry/LicenseStatus";
import { AppNavigation } from "./AppNavigation";
import { Robot, Lessons, CodeLab, Blocks, titles, titleIcons, avatarLetter, ModeBoundary } from "./appData";
import { t as translate } from "../i18n";

import { Suspense, type CSSProperties } from "react";
import { preview } from "../core/client";

import { Icon } from "../components/shared/Icons";
import { Banner, IconBtn } from "../components/shared";
import Home from "../Home";
import schoolBotIcon from "../assets/school-bot-icon.png";
import WindowChromeProvider from "../components/shared/WindowTitlebar";

import { ShellContext, STUDENT_NAME } from "../shell";
import { AppBar } from "../components/shared/AppBar";
import type { useAppScreen } from "../App";

type Props = { v: ReturnType<typeof useAppScreen> };

const windowChromeClass =
  "window-chrome-enabled [&_.window-integrated-header]:pr-0! [&_.window-integrated-header]:[--wails-draggable:drag] " +
  "[&_.window-integrated-header_:is(button,_a,_input,_textarea,_select,_label,_[role=button],_[role=menu]," +
  "_[role=dialog],_[contenteditable=true],_.window-no-drag)]:[--wails-draggable:no-drag] " +
  "[&_.window-integrated-header_img]:[-webkit-user-drag:none] [&_.sh-appbar_>_.sb-grow]:min-w-[24px] " +
  "[&_.aw-window-header_.aw-window-project]:shrink-[1]! [&_.aw-window-header_.aw-window-project]:min-w-0 " +
  "[&_.aw-window-header_.aw-window-project_>_span]:min-w-0 [&_.aw-window-header_.aw-name]:max-w-[min(190px,_16vw)]! " +
  "[&_.aw-window-header_.aw-window-project_input]:max-w-[16vw] " +
  "[&_.aw-window-header_.aw-window-project_[role=status]]:max-w-[min(220px,_20vw)] " +
  "[&_.aw-window-header_.aw-window-project_[role=status]]:overflow-hidden " +
  "[&_.aw-window-header_.aw-window-project_[role=status]]:text-ellipsis [&_.tm-head]:w-[100vw] [&_.tm-head]:min-w-0 " +
  "[&_.tm-head-stat]:overflow-hidden [&_.tm-head-stat]:text-ellipsis [&_.tm-head-stat]:whitespace-nowrap " +
  "[&_.tm-head-stat]:min-w-0 [&_.mw-hdr_.mw-chip]:min-w-0 [&_.mw-hdr_.mw-chip]:overflow-hidden " +
  "[&_.mw-hdr_.mw-chip-name]:overflow-hidden [&_.mw-hdr_.mw-chip-name]:text-ellipsis " +
  "[&_.cn-head_.cn-chip]:max-w-[24vw] [&_.cn-head_.cn-chip]:overflow-hidden [&_.cn-head_.cn-chip]:whitespace-nowrap " +
  "[&_.cn-head_.cn-chip]:text-ellipsis [@media(max-width:_1100px)]:[&_.aw-window-header]:gap-[8px]! " +
  "[@media(max-width:_1100px)]:[&_.aw-window-header]:pl-[16px]! " +
  "[@media(max-width:_1100px)]:[&_.aw-window-header_>_img:first-child]:w-[152px]! " +
  "[@media(max-width:_1100px)]:[&_.aw-window-header_>_img:first-child]:h-auto! " +
  "[@media(max-width:_1100px)]:[&_.aw-window-header_>_img:nth-child(2)]:w-[80px]! " +
  "[@media(max-width:_1100px)]:[&_.aw-window-header_>_img:nth-child(2)]:h-auto! " +
  "[@media(max-width:_1100px)]:[&_.mw-hdr_.mw-back]:py-0 [@media(max-width:_1100px)]:[&_.mw-hdr_.mw-back]:px-[8px] " +
  "[@media(max-width:_1100px)]:[&_.cn-head]:pl-[16px] [@media(max-width:_1100px)]:[&_.cn-head]:gap-[8px] " +
  "[@media(max-width:_1100px)]:[&_.cn-brand]:mr-0 [@media(max-width:_1100px)]:[&_.cn-brand]:gap-[8px] " +
  "[@media(max-width:_1100px)]:[&_.cn-logo-sb]:w-[152px] [@media(max-width:_1100px)]:[&_.cn-logo-arm]:w-[80px]";

const railBaseClass =
  "sb-rail w-[104px] shrink-0 bg-surface border-r-2 [border-right-style:solid] border-r-line " +
  "flex flex-col items-center gap-[8px] py-[14px] px-0 [&.sb-wide]:w-[264px] [&.sb-wide]:[align-items:stretch] " +
  "[&.sb-wide]:py-[14px] [&.sb-wide]:px-[12px] [&.sb-wide]:shadow-pop [&.sb-wide_.sb-nav]:flex-row " +
  "[&.sb-wide_.sb-nav]:justify-start [&.sb-wide_.sb-nav]:w-auto [&.sb-wide_.sb-nav]:min-h-[56px] " +
  "[&.sb-wide_.sb-nav]:py-0 [&.sb-wide_.sb-nav]:px-[16px] [&.sb-wide_.sb-nav]:gap-[14px] " +
  "[&.sb-wide_.sb-nav]:text-[17px]";

const sideRailClass =
  `${railBaseClass} sh-rail h-[var(--app-viewport-height,_100vh)] overflow-y-auto ` +
  "[overflow-x:hidden] [scrollbar-width:none] [&_.sb-nav]:p-0";

const menuRailClass =
  `${railBaseClass} sb-wide sh-wide fixed left-0 top-0 bottom-0 z-[41] ` +
  "h-[var(--app-viewport-height,_100vh)] overflow-y-auto";

const logoClass =
  "sb-logo inline-flex items-center gap-[10px] not-italic [font-variant:normal] font-extrabold " +
  "[font-stretch:normal] text-[18px] leading-[1.1] font-heading text-ink";

const logoSubClass =
  "sh-logo-sub not-italic [font-variant:normal] font-semibold [font-stretch:normal] text-[12px] leading-[1.2] " +
  "font-body text-muted";

const railDividerClass =
  "sb-divider h-[2px] bg-line border-[0] border-none border-current " +
  "m-0 sh-rail-hr w-[56px] shrink-0";

const windowLayoutClass = "window-layout [--app-viewport-height:100vh] h-[100vh] overflow-hidden";

const brandIconClass = "sh-brand-icon w-[36px] h-[36px] object-contain [flex:none]";

const srOnlyClass = "sr-only m-0 absolute w-[1px] h-[1px] overflow-hidden [clip:rect(0_0_0_0)] whitespace-nowrap";

const simBadgeClass =
  "sh-sim w-[84px] py-[6px] px-[4px] rounded-[8px] bg-orange-tint text-orange-deep not-italic [font-variant:normal] " +
  "font-extrabold [font-stretch:normal] text-[10px] leading-[1.25] font-heading tracking-[0.06em] text-center";

const navButtonClass =
  "sb-nav flex flex-col items-center justify-center gap-[4px] w-[84px] min-h-[72px] rounded-[14px] border-2 " +
  "border-solid border-transparent border-b-4 bg-transparent text-muted not-italic [font-variant:normal] font-bold " +
  "[font-stretch:normal] text-[13px] leading-[1.2] font-body cursor-pointer no-underline hover:bg-blue-tint " +
  "hover:text-blue-ink [&.sb-is-hover]:bg-blue-tint [&.sb-is-hover]:text-blue-ink [&.sb-on]:bg-blue-tint " +
  "[&.sb-on]:text-blue-ink [&.sb-on]:border-blue-line [&.sb-on]:border-b-blue focus-visible:outline-none " +
  "focus-visible:shadow-[var(--focus)] [&.sb-is-focus]:outline-none [&.sb-is-focus]:shadow-[var(--focus)]";

const avatarClass =
  "sb-avatar w-[40px] h-[40px] rounded-[50%] bg-orange-tint text-orange-deep inline-flex items-center justify-center " +
  "not-italic [font-variant:normal] font-extrabold [font-stretch:normal] text-[16px] leading-none font-body shrink-0";

const loadingNoticeClass =
  "notice whitespace-pre-wrap [overflow-wrap:anywhere] border-2 border-solid border-blue-line bg-blue-tint " +
  "text-blue-press rounded-control py-[12px] px-[16px] text-[13px] [&.error]:text-err-ink [&.error]:bg-err-tint " +
  "[&.error]:border-err-line sh-loading my-[28px] mx-[36px]";

const pageClass =
  "page [flex:1] min-w-0 h-[var(--app-viewport-height,_100vh)] flex flex-col [&.is-full]:overflow-auto";

const pageBodyClass =
  "page-body [flex:1] min-h-0 overflow-auto py-[28px] px-[36px] [@media(max-width:_1100px)]:p-[24px] " +
  "[@media(max-width:_850px)]:py-[20px] [@media(max-width:_850px)]:px-[16px]";

const lessonsBodyClass = "page-body flex flex-1 min-h-0 min-w-0 flex-col overflow-hidden";

const scrimClass =
  "sb-scrim inset-0 bg-[rgba(var(--ink-rgb),.45)] flex items-center justify-center z-[40] sh-scrim fixed";

const menuHeadClass =
  "sb-row flex items-center gap-[12px] sh-wide-head pt-[2px] pr-0 pb-[10px] pl-[6px] border-b-2 " +
  "[border-bottom-style:solid] border-b-line mb-[4px]";

const navHeadClass =
  "sb-navhead not-italic [font-variant:normal] font-bold [font-stretch:normal] text-[12px] leading-none font-heading " +
  "tracking-[.08em] uppercase text-faint pt-[12px] px-[16px] pb-[4px]";

const langLabelClass =
  "sb-row flex items-center sh-lang-label gap-[8px] not-italic [font-variant:normal] font-bold [font-stretch:normal] " +
  "text-[14px] leading-none font-body text-muted mb-[8px]";

const whoClass =
  "sb-row flex items-center sh-who pt-[10px] pr-[6px] pb-[10px] pl-[12px] rounded-[14px] bg-sunken gap-[10px]";

const captionClass =
  "sb-t-cap not-italic [font-variant:normal] font-medium [font-stretch:normal] " +
  "text-[13px] leading-[1.4] font-body text-muted";

const popClass =
  "sb-pop bg-surface border-2 border-solid border-line-strong rounded-[14px] shadow-pop sh-prof fixed left-[112px] " +
  "bottom-[16px] w-[330px] z-[51] p-[8px] flex flex-col gap-[2px] [&.is-wide]:left-[274px]";

const profDividerClass =
  "sb-divider h-[2px] bg-line border-[0] border-none border-current sh-prof-hr mt-[2px] mx-[4px] mb-[8px]";

const menuItemClass =
  "sh-mi flex items-center gap-[14px] w-full min-h-[52px] py-0 px-[14px] border-[0] border-none border-current " +
  "rounded-[10px] bg-transparent text-left not-italic [font-variant:normal] font-semibold [font-stretch:normal] " +
  "text-[17px] leading-[1.3] font-body text-ink hover:bg-blue-tint [&_.icon:first-child]:text-blue-ink " +
  "[&_small]:ml-auto [&_small]:not-italic [&_small]:[font-variant:normal] [&_small]:font-medium " +
  "[&_small]:[font-stretch:normal] [&_small]:text-[13px] " +
  "[&_small]:leading-none [&_small]:font-body [&_small]:text-muted";

const msgClass =
  "sh-msg fixed top-[76px] left-[50%] [transform:translateX(-50%)] z-[70] w-[min(640px,_calc(100vw_-_160px))] " +
  "shadow-pop rounded-card";

const msgPreClass =
  "sh-msg-pre m-0 whitespace-pre-wrap [overflow-wrap:anywhere] [font-style:inherit] [font-variant:inherit] " +
  "[font-weight:inherit] [font-stretch:inherit] text-[inherit] leading-[inherit] font-[family-name:inherit]";

export function AppView({ v }: Props) {
  const {
    view, nonce, message, setMessage, settings, menuOpen, setMenuOpen, profile, setProfile,
    pending, closeRequested, setCloseRequested, closeBusy, desktop, classLine, closeWindow,
    go, shell, langName, langSeg, full, entry, entryAdapter, licenseBar, licenseBarHeight,
  } = v;
  return (
    <ShellContext.Provider value={shell}>
      <WindowChromeProvider
        desktop={desktop}
        preview={preview}
        escapeBlocked={!entry.entered || settings || !!pending || profile || menuOpen || closeRequested}
        onClose={() => {
          if (closeBusy) return;
          if (!entry.entered || view === "home") void closeWindow();
          else setCloseRequested(true);
        }}
      >
      <div className={`${windowLayoutClass} ${(desktop || preview) ? windowChromeClass : ""} ${entry.entered
        ? "grid grid-rows-[auto_minmax(0,1fr)]" : ""}`}
        style={entry.entered ? {
          "--app-viewport-height": `calc(100vh - ${licenseBarHeight}px)`,
        } as CSSProperties : undefined}
      >
      {!entry.entered && <EntryGate entry={entry} adapter={entryAdapter} language={langSeg} preview={preview} />}
      {!entry.entered && closeRequested && <CloseDialog {...v} />}
      {entry.entered && (
        <div ref={licenseBar} className="min-w-0 [overflow-wrap:anywhere]">
          <LicenseStatus status={entry.status} />
          <div className="bg-surface px-[16px] py-[8px] font-body text-[14px] leading-[28px]">
            {entry.account
              ? translate("entry.choice.account", { username: entry.account })
              : translate("entry.choice.guestTitle")}
          </div>
        </div>
      )}
      {entry.hasEntered && (
      <div
        inert={!entry.entered}
        className={`app-shell relative h-[var(--app-viewport-height,_100vh)] overflow-hidden ${entry.entered ? "flex" : "hidden"}`}
      >

        {!full && (
          <nav
            className={sideRailClass}
            aria-label={translate("nav.label")}
          >
            <div className={"sh-logo pt-[2px] px-0 pb-[8px]"}>
              <span className={logoClass} aria-hidden="true">
                <img className={brandIconClass} src={schoolBotIcon} alt="" width="36" height="36" />
              </span>
              <span className={srOnlyClass}>{"School Bot"}</span>
            </div>
            <AppNavigation view={view} onNavigate={go} section="learn" />
            <hr className={railDividerClass} />
            <AppNavigation view={view} onNavigate={go} section="create" />
            <div className={"sb-grow [flex:1] min-w-0"} />
            {preview && (
              <span
                className={simBadgeClass}
                title={translate("shell.messages.browserPreviewUsesAUiSimulatorNativeJsonIpc")}
              >{translate("shell.messages.browserSimulation")}</span>
            )}
            <button
              type="button"
              className={navButtonClass}
              aria-label={translate("shell.messages.languageValue0OpenMenuToChange", {value0: langName})}
              onClick={shell.openMenu}
            >
              <Icon name="globe" size={28} />
              <span>{langName}</span>
            </button>
            <button
              type="button"
              className={`${navButtonClass} ${profile && !menuOpen ? "sb-is-hover" : ""}`}
              aria-haspopup="menu"
              aria-expanded={profile && !menuOpen}
              aria-label={translate("shell.messages.profileValue0", {value0: STUDENT_NAME})}
              onClick={() => (profile ? setProfile(false) : setProfile(true))}
            >
              <span className={avatarClass}>{avatarLetter}</span>
              <span>{translate("profile.label")}</span>
            </button>
          </nav>
        )}

        <main className={`${pageClass} ${full ? "is-full" : ""}`}>
          {view === "home" ? (
            <Home />
          ) : (
            <ModeBoundary key={`${view}-${nonce}`}>
              <Suspense
                fallback={
                  <div className={loadingNoticeClass} role="status">
                    {translate("common.messages.loadingThisWorkspace")}
                  </div>
                }
              >
                {view === "robot-control" ? (
                  <Robot />
                ) : (
                  <>
                    <AppBar
                      title={
                        <>
                          {titleIcons[view] && (
                            <Icon name={titleIcons[view]!} size={26} />
                          )}
                          {translate(titles[view])}
                        </>
                      }
                    />
                    <div className={view === "coding-adventure" ? lessonsBodyClass : pageBodyClass}>
                      {view === "coding-adventure" && <Lessons />}
                      {view === "code-lab" && <CodeLab />}
                      {view === "block-programming" && <Blocks />}
                    </div>
                  </>
                )}
              </Suspense>
            </ModeBoundary>
          )}
        </main>

        {menuOpen && (
          <>
            <div
              className={scrimClass}
              aria-hidden="true"
              onClick={() => setMenuOpen(false)}
            />
            <nav
              className={menuRailClass}
              aria-label={translate("shell.messages.mainMenuExpanded")}
            >
              <div className={menuHeadClass}>
                <span className={logoClass}>
                  <img className={brandIconClass} src={schoolBotIcon} alt="" width="36" height="36" />
                  <span>{"School Bot"}<br />
                    <span className={logoSubClass}>{"Academy"}</span>
                  </span>
                </span>
                <div className={"sb-grow [flex:1] min-w-0"} />
                <IconBtn
                  icon="chevL"
                  label={translate("nav.close")}
                  flat
                  autoFocus
                  onClick={() => setMenuOpen(false)}
                />
              </div>
              <div className={navHeadClass}>{translate("shell.messages.learn")}</div>
              <AppNavigation view={view} onNavigate={go} section="learn" wide />
              <div className={navHeadClass}>{translate("shell.messages.create")}</div>
              <AppNavigation view={view} onNavigate={go} section="create" wide />
              <div className={"sb-grow [flex:1] min-w-0"} />
              <div className={"sh-lang pt-[8px] px-[4px] pb-[12px]"}>
                <div className={langLabelClass}>
                  <Icon name="globe" size={20} />{translate("language.label")}</div>
                {langSeg}
              </div>
              <div className={whoClass}>
                <span className={avatarClass}>{avatarLetter}</span>
                <div className={"sb-grow [flex:1] min-w-0 sb-col flex flex-col gap-0"} >
                  <b>{STUDENT_NAME}</b>
                  <span className={captionClass}>{translate(classLine)}</span>
                </div>
                <IconBtn
                  icon="more"
                  label={translate("profile.menu")}
                  flat
                  aria-haspopup="menu"
                  aria-expanded={profile}
                  onClick={() => setProfile(!profile)}
                />
              </div>
            </nav>
          </>
        )}

        {profile && (
          <>
            <div
              className={"sh-pop-scrim fixed inset-0 z-[50]"}
              aria-hidden="true"
              onClick={() => setProfile(false)}
            />
            <div
              className={`${popClass} ${menuOpen ? "is-wide" : ""}`}
              role="menu"
              aria-label={translate("profile.menu")}
            >
              <div className={"sb-row flex items-center gap-[12px] sh-prof-who pt-[8px] px-[14px] pb-[10px]"}>
                <span className={avatarClass}>{avatarLetter}</span>
                <div className={"sb-grow [flex:1] min-w-0 sb-col flex flex-col gap-0"} >
                  <b>{STUDENT_NAME}</b>
                  <span className={captionClass}>{translate(classLine)}</span>
                </div>
              </div>
              <hr className={profDividerClass} />
              <div className={"sh-prof-lang pt-0 px-[14px] pb-[10px]"}>
                <div className={langLabelClass}>
                  <Icon name="globe" size={20} />{translate("language.label")}</div>
                {langSeg}
              </div>
              <hr className={profDividerClass} />
              <button
                type="button"
                role="menuitem"
                className={menuItemClass}
                onClick={shell.openSettings}
              >
                <Icon name="cog" size={24} />{translate("settings.title")}</button>
              <button
                type="button"
                role="menuitem"
                className={menuItemClass}
                onClick={() => go("robot-control", "teacher")}
              >
                <Icon name="lock" size={24} />
                {translate("robot.teacher.title")}
                <small>{translate("common.messages.pinRequired")}</small>
              </button>
            </div>
          </>
        )}

        {message && !settings && (
          <div className={msgClass}>
            <Banner
              kind="info"
              title={translate("shell.messages.systemMessage")}
              actions={
                <IconBtn
                  icon="x"
                  label={translate("lessons.messages.dismissMessage")}
                  flat
                  onClick={() => setMessage("")}
                />
              }
            >
              <pre className={msgPreClass} role="status">
                {translate(message)}
              </pre>
            </Banner>
          </div>
        )}

        {entry.entered && <AppDialogs v={v} />}



      </div>
      )}
      </div>
      </WindowChromeProvider>
    </ShellContext.Provider>
  );
}
