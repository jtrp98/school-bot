import { HomeDialogs } from "./HomeDialogs";
import { Medal, Sparkles } from "./HomeDecorations";
import { workTitle, FOLDERS, type ScenarioId, scenarios } from "./homeData";
import { t as translate } from "../i18n";

import { Icon } from "../components/shared/Icons";
import { ArmFigure, Bar, Button, RobotChip, TagKind } from "../components/shared";
import { SAMPLE_ROBOT_NAME, STUDENT_NAME } from "../shell";
import { RichText } from "../components/shared/RichText";
import { AppBar } from "../components/shared/AppBar";
import { SearchBox } from "../components/shared/SearchBox";
import { ScenarioBar } from "../components/shared/ScenarioBar";
import heroRobot from "../assets/hero-robot.webp";
import type { useHomeScreen } from "../Home";

type Props = { v: ReturnType<typeof useHomeScreen> };

const pageBodyClass =
  "page-body [flex:1] min-h-0 overflow-auto py-[28px] px-[36px] [@media(max-width:_1100px)]:p-[24px] " +
  "[@media(max-width:_850px)]:py-[20px] [@media(max-width:_850px)]:px-[16px]";
const heroBaseClass =
  "hm-hero relative flex items-center gap-[28px] h-[340px] pt-[20px] pr-[40px] pb-[20px] pl-[28px] " +
  "bg-surface border-2 border-solid border-line rounded-modal shadow-card overflow-hidden " +
  "before:[content:''] before:absolute before:right-[-70px] before:top-[-90px] before:w-[380px] " +
  "before:h-[380px] before:rounded-[50%] before:bg-orange-tint before:opacity-[0.75] after:[content:''] " +
  "after:absolute after:right-[170px] after:bottom-[-130px] after:w-[260px] after:h-[260px] " +
  "after:rounded-[50%] after:bg-blue-tint after:opacity-[0.85] [&_>_*]:relative [&_>_*]:z-[1] " +
  "[&.is-done]:border-ok-line [&.is-done::before]:bg-ok-tint";
const mascotClass =
  "hm-mascot w-[196px] h-full flex items-end justify-center shrink-0 relative before:[content:''] " +
  "before:absolute before:left-[14px] before:right-[14px] before:bottom-[-2px] before:h-[18px] " +
  "before:rounded-[50%] before:bg-[rgba(var(--ink-rgb),_0.1)] [&_img]:relative " +
  "[&_img]:h-[min(292px,_100%)] [&_img]:w-auto [&_img]:block";
const heroTitleClass =
  "hm-htitle m-0 not-italic [font-variant:normal] font-extrabold [font-stretch:normal] text-[38px] " +
  "leading-[1.2] font-heading text-ink";
const heroSubClass =
  "hm-hsub m-0 not-italic [font-variant:normal] font-normal [font-stretch:normal] text-[20px] " +
  "leading-[1.5] font-body text-muted";
const heroProgressClass =
  "hm-prog flex items-center gap-[14px] not-italic [font-variant:normal] font-semibold " +
  "[font-stretch:normal] text-[16px] leading-[1.3] font-body text-muted [&_.sb-bar]:w-[300px] " +
  "[&_.sb-bar]:h-[14px] [&_b]:not-italic [&_b]:[font-variant:normal] [&_b]:font-extrabold " +
  "[&_b]:[font-stretch:normal] [&_b]:text-[20px] [&_b]:leading-none [&_b]:font-heading [&_b]:text-ink";
const recentTitleClass =
  "sb-h2 not-italic [font-variant:normal] font-extrabold [font-stretch:normal] text-[24px] " +
  "leading-[1.25] font-heading";
const savedHintClass =
  "sb-t-body not-italic [font-variant:normal] font-normal [font-stretch:normal] text-[16px] " +
  "leading-[1.5] font-body sb-muted text-muted";
const folderPopClass =
  "sb-pop bg-surface border-2 border-solid border-line-strong rounded-[14px] shadow-pop hm-fpop " +
  "absolute right-0 top-[calc(100%_+_8px)] w-[390px] z-[35] p-[8px] flex flex-col gap-[2px] text-left";
const folderNavHeadClass =
  "sb-navhead not-italic [font-variant:normal] font-bold [font-stretch:normal] text-[12px] leading-none " +
  "font-heading tracking-[.08em] uppercase text-faint pt-[8px] px-[12px] pb-[4px]";
const folderOptBaseClass =
  "hm-opt flex items-center gap-[12px] w-full min-h-[60px] py-[8px] px-[12px] border-[0] border-none " +
  "border-current rounded-[10px] bg-transparent text-left cursor-pointer not-italic " +
  "[font-variant:normal] font-semibold [font-stretch:normal] text-[16px] leading-[1.3] font-body " +
  "text-ink hover:bg-blue-tint [&.is-on]:bg-blue-tint [&.is-on_.hm-oico]:bg-surface " +
  "[&.is-on_.hm-oico]:text-blue-ink [&.hm-other]:text-blue-ink [&.hm-other]:font-bold " +
  "[&.hm-other_.hm-oico]:bg-blue-tint [&.hm-other_.hm-oico]:text-blue-ink";
const folderIconClass =
  "hm-oico w-[40px] h-[40px] rounded-[10px] bg-sunken text-muted flex items-center justify-center " +
  "shrink-0";
const folderCaptionClass =
  "sb-t-cap not-italic [font-variant:normal] font-medium [font-stretch:normal] text-[13px] " +
  "leading-[1.4] font-body text-muted";
const folderDividerClass = "sb-divider h-[2px] bg-line border-[0] border-none border-current my-[6px] mx-[4px]";
const folderTextClass = "sb-grow [flex:1] min-w-0 sb-col flex flex-col gap-0";
const fixedFolderClass =
  "hm-fixed inline-flex items-center gap-[8px] min-h-[44px] py-0 px-[16px] rounded-control border-2 " +
  "border-solid border-line bg-surface not-italic [font-variant:normal] font-bold [font-stretch:normal] " +
  "text-[16px] leading-none font-body";
const viewAllClass =
  "sb-link text-blue-ink underline [text-underline-offset:3px] hover:text-blue-press " +
  "focus-visible:outline-none focus-visible:shadow-[var(--focus)] focus-visible:rounded-[4px] hm-all " +
  "inline-flex items-center gap-[6px] min-h-[44px] py-0 px-[8px] border-[0] border-none border-current " +
  "bg-transparent not-italic [font-variant:normal] font-bold [font-stretch:normal] text-[17px] " +
  "leading-none font-body hover:bg-blue-tint hover:rounded-control";
const gridClass =
  "hm-grid grid [grid-template-columns:repeat(4,_minmax(0,_1fr))] gap-[16px] relative " +
  "[@media(max-width:_1100px)]:[grid-template-columns:repeat(2,_minmax(0,_1fr))]";
const newCardBaseClass =
  "hm-new h-[252px] flex flex-col items-center justify-center gap-[12px] border-[3px] border-dashed " +
  "border-orange rounded-card bg-surface text-orange-press not-italic [font-variant:normal] " +
  "font-extrabold [font-stretch:normal] text-[22px] leading-[1.2] font-heading cursor-pointer p-0 " +
  "[transition:background_0.12s] hover:bg-orange-tint hover:border-orange [&.is-attn]:bg-orange-tint " +
  "[&.is-attn]:shadow-[0_0_0_5px_rgba(var(--orange-rgb),_0.2)] motion-reduce:[transition:none]";
const newPlusClass =
  "hm-newplus w-[64px] h-[64px] rounded-[50%] bg-orange text-white flex items-center justify-center " +
  "border-b-4 [border-bottom-style:solid] border-b-orange-press";
const workCardClass =
  "sb-card bg-surface border-2 border-solid border-line rounded-card shadow-card " +
  "[transition:border-color_.12s,transform_.12s] [&.hm-wcard]:h-[252px] [&.hm-wcard]:min-h-0 " +
  "[&.hm-wcard]:p-0 [&.hm-wcard]:flex [&.hm-wcard]:flex-col [&.hm-wcard]:[align-items:stretch] " +
  "[&.hm-wcard]:overflow-hidden [&.hm-wcard]:text-left [&.hm-kind]:flex [&.hm-kind]:flex-col " +
  "[&.hm-kind]:items-start [&.hm-kind]:gap-[8px] [&.hm-kind]:p-[20px] [&.hm-kind]:text-left " +
  "[&.hm-kind]:min-h-[190px] [&.hm-kind]:bg-surface [&.hm-kind:hover]:bg-blue-tint " +
  "motion-reduce:[&.hm-wcard]:[transition:none] sb-card-hit cursor-pointer block text-[inherit] " +
  "no-underline [font-weight:inherit] hover:border-blue hover:[transform:translateY(-2px)] " +
  "focus-visible:outline-none focus-visible:shadow-[var(--focus)] active:[transform:translateY(1px)] " +
  "hm-wcard";
const thumbClass =
  "hm-thumb h-[128px] shrink-0 relative flex items-center justify-center overflow-hidden " +
  "[&_>_svg]:absolute [&_>_svg]:right-0 [&_>_svg]:top-0 [&_>_svg]:h-full [&_>_svg]:w-auto";
const thumbBlockClass = "hm-thumb-block flex items-center justify-center w-full h-full bg-orange-tint text-orange-deep";
const workBodyClass =
  "hm-wbody pt-[12px] px-[14px] pb-[14px] flex flex-col gap-[6px] min-w-0 " +
  "[&_.sb-tag-kind]:whitespace-nowrap [&_.sb-tag-kind]:shrink-0";
const workNameClass =
  "hm-wname not-italic [font-variant:normal] font-bold [font-stretch:normal] text-[19px] leading-[1.3] " +
  "font-body text-ink sb-ell whitespace-nowrap overflow-hidden text-ellipsis";
const workMetaClass =
  "sb-t-sm not-italic [font-variant:normal] font-normal [font-stretch:normal] text-[14px] " +
  "leading-[1.45] font-body sb-muted text-muted sb-nowrap sb-ell whitespace-nowrap overflow-hidden " +
  "text-ellipsis";
const workSavedClass =
  "sb-t-cap not-italic [font-variant:normal] font-medium [font-stretch:normal] text-[13px] " +
  "leading-[1.4] font-body sb-ell whitespace-nowrap overflow-hidden text-ellipsis hm-wsaved text-faint";
const emptyCardClass =
  "hm-empty [grid-column:2_/_-1] h-[252px] flex items-center gap-[24px] py-0 px-[32px] bg-surface " +
  "border-2 border-solid border-line rounded-card";
const emptyIconClass =
  "hm-eico w-[76px] h-[76px] rounded-[50%] bg-orange-tint text-orange-deep flex items-center " +
  "justify-center shrink-0";
const emptyTitleClass =
  "sb-h2 not-italic [font-variant:normal] font-extrabold [font-stretch:normal] text-[24px] " +
  "leading-[1.25] font-heading";
const emptyTextClass =
  "hm-etext not-italic [font-variant:normal] font-medium [font-stretch:normal] text-[20px] " +
  "leading-[1.5] font-body text-muted [&_b]:text-orange-press";
const asideClass =
  "hm-aside w-[300px] shrink-0 flex flex-col gap-[16px] [@media(max-width:_1100px)]:w-full " +
  "[@media(max-width:_1100px)]:flex-row [@media(max-width:_1100px)]:[&_>_*]:[flex:1]";
const panelClass =
  "hm-panel bg-surface border-2 border-solid border-line rounded-card p-[20px] shadow-card flex " +
  "flex-col gap-[14px] [&_h3]:m-0";
const panelTitleClass =
  "sb-h3 not-italic [font-variant:normal] font-bold [font-stretch:normal] text-[20px] leading-[1.3] " +
  "font-heading";
const starTileClass =
  "hm-startile w-[52px] h-[52px] rounded-[14px] bg-orange-tint text-orange-deep flex items-center " +
  "justify-center shrink-0";
const starNumClass =
  "hm-starnum not-italic [font-variant:normal] font-extrabold [font-stretch:normal] text-[30px] " +
  "leading-none font-heading";
const tipPanelClass =
  "hm-panel border-2 border-solid rounded-card p-[20px] flex flex-col [&_h3]:m-0 hm-tip bg-blue-tint " +
  "border-blue-line shadow-none gap-[8px] [&_p]:m-0 [&_p]:not-italic [&_p]:[font-variant:normal] " +
  "[&_p]:font-medium [&_p]:[font-stretch:normal] [&_p]:text-[17px] [&_p]:leading-[1.6] [&_p]:font-body " +
  "[&_p]:text-ink";
const tipTitleClass =
  "sb-h3 not-italic [font-variant:normal] font-bold [font-stretch:normal] text-[20px] leading-[1.3] " +
  "font-heading text-blue-press";

export function HomeView({ v }: Props) {
  const {
    shell, sample, scenario, query, setQuery, folder, otherFolder, folderOpen, setFolderOpen,
    setKindOpen, pickScenario, pickFolder, browseFolder, heroKind, hero, stars, folders, current,
    all, needle, shown,
  } = v;
  return (
    <>
      <AppBar
        title={translate("nav.home")}
        search={
          <SearchBox
            placeholder={translate("home.search.placeholder")}
            value={query}
            onChange={setQuery}
          />
        }
        right={
          <RobotChip
            state={sample ? "connected" : "off"}
            name={SAMPLE_ROBOT_NAME}
            port="COM7"
            onClick={() => shell.go("robot-control", "connect")}
          />
        }
      />
      <div className={pageBodyClass}>
        <div className={"hm-cols flex gap-[20px] items-start [@media(max-width:_1100px)]:flex-col"}>
          <div className={"hm-left [flex:1] min-w-0 flex flex-col gap-[20px]"}>
            <section
              className={`${heroBaseClass} ${heroKind === "done" ? "is-done" : ""}`}
              aria-label={translate("home.messages.today")}
            >
              <div className={mascotClass}>
                <img src={heroRobot} alt="" width={207} height={292} />
              </div>
              <div className={"hm-hbody flex flex-col gap-[14px] min-w-0 [flex:1] items-start"}>
                <h2 className={heroTitleClass}>{translate(hero.title, { name: STUDENT_NAME })}</h2>
                <p className={heroSubClass}>{translate(hero.sub)}</p>
                {hero.pct != null && (
                  <div className={heroProgressClass}>
                    {heroKind === "done" && (
                      <Icon name="check" size={20} />
                    )}
                    <span>{translate("home.messages.unit3RobotArms")}</span>
                    <Bar pct={hero.pct} kind={heroKind === "done" ? "ok" : undefined} />
                    <b>{hero.pct}%</b>
                  </div>
                )}
                <Button iconRight="play" onClick={() => shell.go("coding-adventure")}>
                  {translate(hero.btn)}
                </Button>
              </div>
              {heroKind === "done" && <Sparkles />}
            </section>

            <div className={"sb-row flex items-center hm-rh gap-[12px] flex-wrap"}>
              <h2 className={recentTitleClass}>{translate("home.recent.title")}</h2>
              <div className={"sb-grow [flex:1] min-w-0"} />
              <span className={savedHintClass}>{translate("common.messages.projectsSavedIn")}</span>
              {sample ? (
                <div className={"hm-fwrap relative"}>
                  <Button
                    variant="secondary"
                    size="sm"
                    icon="folder"
                    iconRight="chevD"
                    className={folderOpen ? "sb-is-hover" : undefined}
                    aria-haspopup="listbox"
                    aria-expanded={folderOpen}
                    aria-label={translate("home.messages.changeProjectFolderValue0", {value0: current.path})}
                    onClick={() => setFolderOpen(!folderOpen)}
                  >
                    {current.path}
                  </Button>
                  {folderOpen && (
                    <>
                      <div
                        className={"hm-fscrim fixed inset-0 z-[34]"}
                        aria-hidden="true"
                        onClick={() => setFolderOpen(false)}
                      />
                      <div
                        className={folderPopClass}
                        role="listbox"
                        aria-label={translate("home.messages.chooseFolder")}
                      >
                        <div className={folderNavHeadClass}>{translate("home.messages.recentFolders")}</div>
                        {folders.map((f, i) => {
                          const on = otherFolder ? i === folders.length - 1 : i === folder;
                          return (
                            <button
                              key={f.path}
                              type="button"
                              role="option"
                              aria-selected={on}
                              className={`${folderOptBaseClass} ${on ? "is-on" : ""}`}
                              onClick={() =>
                                i < FOLDERS.length ? pickFolder(i) : setFolderOpen(false)
                              }
                            >
                              <span className={folderIconClass}>
                                <Icon name={f.icon} size={24} />
                              </span>
                              <span className={folderTextClass}>
                                <b>{f.path}</b>
                                <span className={folderCaptionClass}>{translate(f.count)}</span>
                              </span>
                              {on && (
                                <span className={"text-blue-ink"} >
                                  <Icon name="check" size={24} />
                                </span>
                              )}
                            </button>
                          );
                        })}
                        <hr className={folderDividerClass}  />
                        <button
                          type="button"
                          className={`${folderOptBaseClass} hm-other`}
                          onClick={browseFolder}
                        >
                          <span className={folderIconClass}>
                            <Icon name="folder" size={24} />
                          </span>
                          <span>{translate("home.messages.chooseAnotherFolder")}</span>
                        </button>
                      </div>
                    </>
                  )}
                </div>
              ) : (
                <span className={fixedFolderClass}>
                  <Icon name="folder" size={20} />{translate("home.messages.thisDevice")}</span>
              )}
              {all.length > 0 && (
                <button
                  type="button"
                  className={viewAllClass}
                  onClick={() => shell.go("robot-control", "works")}
                >{translate("home.messages.viewAll")}{" "}<Icon name="arrowR" size={20} />
                </button>
              )}
            </div>

            <div className={gridClass}>
              <button
                type="button"
                className={`${newCardBaseClass} ${all.length === 0 ? "is-attn" : ""}`}
                aria-haspopup="dialog"
                onClick={() => setKindOpen(true)}
              >
                <span className={newPlusClass}>
                  <Icon name="plus" size={34} />
                </span>{translate("home.create.title")}</button>
              {shown.map((w) => (
                <button
                  type="button"
                  key={w.id}
                  className={workCardClass}
                  aria-label={translate("common.messages.openProjectValue0", {value0: workTitle(w)})}
                  onClick={() => shell.go(w.mode)}
                >
                  <div className={thumbClass} style={{ background: w.bg }}>
                    {w.kind === "arm" ? (
                      <ArmFigure
                        a={[w.a![0], w.a![1], w.a![2], w.a![3], 0, 50]}
                        w={244}
                        h={190}
                        mini
                      />
                    ) : (
                      <span className={thumbBlockClass}>
                        <Icon name="block" size={60} />
                      </span>
                    )}
                  </div>
                  <div className={workBodyClass}>
                    <div className={workNameClass}>{workTitle(w)}</div>
                    <div className={"sb-row flex items-center gap-[8px]"} >
                      <TagKind kind={w.kind} />
                      <span className={workMetaClass}>
                        {translate(w.meta)}
                      </span>
                    </div>
                    {w.saved && (
                      <div className={workSavedClass}>{translate(w.saved)}</div>
                    )}
                  </div>
                </button>
              ))}
              {shown.length === 0 && (
                <div className={emptyCardClass}>
                  <span className={emptyIconClass}>
                    <Icon name={needle ? "search" : "hand"} size={40} />
                  </span>
                  <div className={"sb-col flex flex-col gap-[6px]"} >
                    <div className={emptyTitleClass}>
                      {needle
                        ? translate("home.messages.noMatchingProjects")
                        : translate("common.messages.noProjectsYet")}
                    </div>
                    <div className={emptyTextClass}>
                      {needle ? (
                        translate("common.messages.tryADifferentSearch")
                      ) : (
                        <RichText
                          i18nKey="home.messages.createFirstProjectHint"
                          values={{ create: translate("home.create.title") }}
                          components={{ bold: <b /> }}
                        />
                      )}
                    </div>
                  </div>
                  <div className={"sb-grow [flex:1] min-w-0"} />
                  {!needle && (
                    <span className={"hm-point text-orange"}>
                      <Icon name="arrowL" size={56} />
                    </span>
                  )}
                </div>
              )}
            </div>
          </div>

          <aside className={asideClass}>
            <section
              className={panelClass}
              aria-label={translate("home.achievements.title")}
            >
              <h3 className={panelTitleClass}>{translate("home.achievements.title")}</h3>
              <div className={"sb-row flex items-center gap-[14px]"} >
                <span className={starTileClass}>
                  <Icon name="star" size={30} />
                </span>
                <div>
                  <b className={starNumClass}>{stars}</b>{" "}
                  <span className={"sb-muted text-muted text-[16px]"} >{translate("home.messages.starsThisWeek")}</span>
                </div>
              </div>
              <div className={"sb-row flex items-center gap-[10px]"} >
                <Medal
                  icon="badge"
                  got={stars > 0}
                  label={stars > 0
                    ? translate("home.messages.earnedMedal")
                    : translate("home.messages.medalNotEarnedYet")}
                />
                <Medal
                  icon="crown"
                  got={stars > 0}
                  label={stars > 0
                    ? translate("home.messages.earnedAward")
                    : translate("home.messages.awardNotEarnedYet")}
                />
                <Medal icon="quiz" got={false} label={translate("home.messages.secretAwardLocked")} />
              </div>
            </section>
            <section
              className={tipPanelClass}
              aria-label={translate("home.messages.tip")}
            >
              <div className={"sb-row flex items-center hm-tip-head gap-[8px] text-blue-press"}>
                <Icon name="sparkle" size={24} />
                <h3 className={tipTitleClass} >{translate("home.tip.title")}</h3>
              </div>
              <p>{translate("home.messages.beforePressingPlayAlwaysCheckThatNoHandsOr")}</p>
            </section>
          </aside>
        </div>
      </div>

      <HomeDialogs v={v} />



      {sample && (
        <ScenarioBar
          items={scenarios}
          value={scenario}
          onChange={(id) => pickScenario(id as ScenarioId)}
        />
      )}
    </>
  );
}
