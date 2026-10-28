import { WorkCards } from "./WorkCards";
import { Button } from "../../../components/shared";
import { Icon } from "../../../components/shared/Icons";
import { type ScenarioId, scenarios } from "./worksData";
import { t as translate } from "../../../i18n";

import { ScenarioBar } from "../../../components/shared/ScenarioBar";

import { WindowControlsSlot } from "../../../components/shared/WindowTitlebar";
import type { useWorksScreen } from "./WorksScreen";

type Props = { v: ReturnType<typeof useWorksScreen> };

const rootClass =
  "mw-root [--mw-ink:var(--ink)] [--mw-muted:var(--muted)] [--mw-faint:var(--faint)] " +
  "[--mw-line:var(--line)] [--mw-line-strong:var(--line-strong)] [--mw-blue:var(--blue-ink)] " +
  "[--mw-blue-tint:var(--blue-tint)] [--mw-green:var(--ok)] [--mw-red:var(--err-ink)] w-full " +
  "h-[var(--app-viewport-height,_100vh)] min-h-0 box-border relative flex flex-col overflow-hidden " +
  "bg-bg text-[var(--mw-ink)] font-[family-name:'Noto_Sans_Thai',_'Leelawadee_UI',_sans-serif] " +
  "text-[14px] leading-[normal] [&_*]:box-border [&_*::before]:box-border [&_*::after]:box-border " +
  "[&_button]:min-h-0 [&_button]:[transform:none] [&_button]:[transition:none] " +
  "[&_button]:font-[family-name:inherit] [&_button:active:not(:disabled)]:[transform:none] " +
  "[&_button:focus-visible]:[outline:3px_solid_var(--blue-line)] " +
  "[&_button:focus-visible]:[outline-offset:2px] [&_button:focus-visible]:shadow-none " +
  "[&_input:focus-visible]:[outline:3px_solid_var(--blue-line)] " +
  "[&_input:focus-visible]:[outline-offset:2px] [&_input:focus-visible]:shadow-none " +
  "[&_a:focus-visible]:[outline:3px_solid_var(--blue-line)] [&_a:focus-visible]:[outline-offset:2px] " +
  "[&_a:focus-visible]:shadow-none [&_.mw-search_input:focus-visible]:outline-none " +
  "[&_.mw-edit:focus-visible]:outline-none " +
  "[&_.mw-edit:focus-visible]:shadow-[0_0_0_3px_var(--blue-tint)] " +
  "[&_.mw-rename:focus-visible]:outline-none " +
  "[&_.mw-rename:focus-visible]:shadow-[0_0_0_3px_var(--blue-tint)] " +
  "[&_.mw-rename.is-err:focus-visible]:shadow-[0_0_0_3px_var(--err-line)] [&_.sb-scenario]:left-[16px] " +
  "[&_.sb-scenario]:bottom-[16px]";
const headerClass =
  "mw-hdr h-[60px] shrink-0 py-0 px-[24px] bg-white border-b-2 [border-bottom-style:solid] " +
  "border-b-[var(--mw-line)] flex items-center gap-[12px] window-integrated-header";
const chipBaseClass =
  "mw-chip flex items-center gap-[8px] h-[44px] py-0 px-[12px] rounded-[12px] bg-blue-tint " +
  "border-[medium] border-none border-current text-[var(--mw-ink)] text-[14px] cursor-pointer text-left " +
  "hover:bg-blue-tint [&.is-off_.mw-chip-st]:text-[var(--mw-muted)] " +
  "[&.is-off_.mw-chip-st_i]:bg-[var(--mw-faint)] [&.is-lost_.mw-chip-st]:text-[var(--mw-red)] " +
  "[&.is-lost_.mw-chip-st_i]:bg-err [&.is-busy_.mw-chip-st]:text-orange-deep " +
  "[&.is-busy_.mw-chip-st_i]:bg-star";
const chipNameClass =
  "mw-chip-name text-[15px] font-bold whitespace-nowrap [&_span]:font-medium " +
  "[&_span]:text-[var(--mw-muted)]";
const chipStatusClass =
  "mw-chip-st inline-flex items-center gap-[5px] text-[13px] font-semibold whitespace-nowrap " +
  "text-ok-ink [&_i]:w-[9px] [&_i]:h-[9px] [&_i]:rounded-full [&_i]:bg-[var(--mw-green)]";
const pathClass =
  "mw-path inline-flex items-center gap-[6px] h-[28px] py-0 px-[10px] rounded-[8px] bg-white border-2 " +
  "border-solid border-[var(--mw-line)] font-bold text-[var(--mw-ink)] [&.mw-path]:box-content";
const folderLinkClass =
  "mw-folder h-[28px] py-0 px-[8px] border-[medium] border-none border-current bg-transparent " +
  "text-[var(--mw-blue)] text-[13px] font-bold cursor-pointer underline hover:bg-transparent";
const searchClass =
  "mw-search flex items-center gap-[8px] h-[44px] w-[220px] py-0 px-[12px] border-2 border-solid " +
  "border-[var(--mw-line-strong)] rounded-[12px] bg-white [&_input]:[flex:1_1_0] [&_input]:min-w-0 " +
  "[&_input]:border-[medium] [&_input]:border-none [&_input]:border-current [&_input]:outline-none " +
  "[&_input]:font-[family-name:inherit] [&_input]:text-[15px] [&_input]:text-[var(--mw-ink)] " +
  "[&_input]:bg-transparent [&_input]:py-[1px] [&_input]:px-[2px] [&_input]:shadow-none " +
  "focus-within:border-[var(--mw-blue)] focus-within:shadow-[0_0_0_3px_var(--blue-tint)]";
const openFileClass =
  "mw-ibtn border-2 border-solid border-[var(--mw-line-strong)] bg-white text-[var(--mw-ink)] font-bold " +
  "cursor-pointer [&:hover:not(:disabled)]:bg-[var(--mw-blue-tint)] " +
  "[&:hover:not(:disabled)]:border-[var(--mw-blue)] [&:hover:not(:disabled)]:text-[var(--mw-blue)] " +
  "[&.mw-red:hover:not(:disabled)]:bg-err-tint [&.mw-red:hover:not(:disabled)]:border-err " +
  "[&.mw-red:hover:not(:disabled)]:text-[var(--mw-red)] disabled:opacity-[1] " +
  "disabled:cursor-not-allowed mw-open-file inline-flex items-center gap-[8px] h-[44px] py-0 px-[16px] " +
  "border-b-4 rounded-[12px] text-[15px] disabled:bg-sunken disabled:text-[var(--mw-faint)]";
const gridClass =
  "mw-grid grid [grid-template-columns:repeat(auto-fill,_minmax(300px,_1fr))] gap-[16px] " +
  "[align-content:start] shrink-0";
const newCardClass =
  "mw-new min-h-[104px] border-[3px] border-dashed border-blue-line rounded-[18px] bg-white flex " +
  "flex-row items-center justify-center gap-[14px] cursor-pointer text-[var(--mw-blue)] py-[1px] " +
  "px-[6px] hover:bg-[var(--mw-blue-tint)] hover:border-[var(--mw-blue)]";
const newPlusClass =
  "mw-new-plus w-[48px] h-[48px] shrink-0 rounded-full bg-[var(--mw-blue)] text-white flex items-center " +
  "justify-center text-[30px] font-semibold leading-none";
const nameCardClass =
  "mw-name-card min-h-[104px] border-[3px] border-solid border-[var(--mw-blue)] rounded-[18px] bg-white " +
  "p-[20px] flex flex-col gap-[12px] shadow-[0_0_0_4px_var(--blue-tint)] [&_h2]:m-0 [&_h2]:text-[18px] " +
  "[&_h2]:font-bold";
const editInputClass =
  "mw-edit h-[48px] py-0 px-[14px] border-2 border-solid border-blue-line rounded-[12px] text-[17px] " +
  "font-semibold text-[var(--mw-ink)] bg-white min-w-0 w-full focus:outline-none " +
  "focus:border-[var(--mw-blue)] focus:shadow-[0_0_0_3px_var(--blue-tint)]";
const pathNoteBaseClass =
  "mw-path-note text-[13px] leading-[1.5] font-semibold text-[var(--mw-muted)] " +
  "[&.is-err]:text-[var(--mw-red)]";
const cancelClass =
  "mw-cancel [flex:1_1_0] h-[44px] border-2 border-solid border-[var(--mw-line-strong)] rounded-[12px] " +
  "bg-white text-[var(--mw-ink)] text-[15px] font-bold cursor-pointer py-[1px] px-[6px] hover:bg-white " +
  "hover:border-[var(--mw-line-strong)]";
const createClass =
  "mw-create [flex:1.4_1_0] h-[44px] rounded-[12px] text-[15px] font-bold py-[1px] px-[6px] " +
  "border-[medium] border-none border-current border-b-4 [border-bottom-style:solid] " +
  "border-b-blue-press bg-[var(--mw-blue)] text-white cursor-pointer " +
  "[&:hover:not(:disabled)]:bg-[var(--mw-blue)] [&:hover:not(:disabled)]:border-blue-press " +
  "disabled:opacity-[1] disabled:border-2 disabled:border-solid disabled:border-off disabled:bg-sunken " +
  "disabled:text-[var(--mw-faint)] disabled:cursor-not-allowed";
const noneClass = "mw-none p-[24px] text-center text-[16px] text-[var(--mw-muted)]";
const emptyClass = "mw-empty py-[8px] px-[4px] text-[16px] text-[var(--mw-muted)]";
const toastClass =
  "mw-toast fixed left-[50%] bottom-[32px] [transform:translateX(-50%)] flex items-center gap-[12px] " +
  "pt-[8px] pr-[8px] pb-[8px] pl-[16px] rounded-[12px] bg-[var(--mw-ink)] text-white text-[15px] " +
  "font-semibold whitespace-nowrap shadow-pop z-[70] max-w-[calc(100vw_-_32px)] " +
  "[&_>_span]:overflow-hidden [&_>_span]:text-ellipsis [&_button]:h-[34px] [&_button]:py-0 " +
  "[&_button]:px-[12px] [&_button]:border-2 [&_button]:border-solid [&_button]:border-star " +
  "[&_button]:rounded-[9px] [&_button]:bg-transparent [&_button]:text-star [&_button]:text-[14px] " +
  "[&_button]:font-bold [&_button]:cursor-pointer [&_button]:shrink-0 " +
  "[&_button:hover]:bg-[rgba(var(--star-rgb),_0.15)]";

const mainClass =
  "mw-main [flex:1_1_0] min-h-0 pt-[24px] px-[32px] pb-[40px] flex flex-col gap-[20px] overflow-y-auto";

export function WorksView({ v }: Props) {
  const {
    go,
    robot,
    sample,
    scenario,
    naming,
    setNaming,
    setRenameId,
    q,
    setQ,
    toast,
    setToast,
    usb,
    robotName,
    say,
    applyScenario,
    createWork,
    openUsb,
    needle,
    shown,
    hasWorks,
    nameState,
    pathText,
    chipCls,
    chipSt,
    onNamingKey,
  } = v;
  return (
    <div className={rootClass}>
      <header className={headerClass}>
        <h1 className={"mw-h1 m-0 text-[24px] font-bold whitespace-nowrap"}>{translate("robot.works.title")}</h1>
        <div className={"mw-spacer grow"} />
        <Button
          variant="custom"
          type="button"
          className={`${chipBaseClass} ${chipCls}`}
          aria-label={translate(
            "robot.works.messages.robotValue0Value1Value2OpenConnectionPage",
            { value0: robotName, value1: robot ? ` ${robot.port}` : "", value2: chipSt },
          )}
          onClick={() => go("connect")}
        >
          <span className={chipNameClass}>
            {robotName}
            {robot && <span> · {robot.port}</span>}
          </span>
          <span className={chipStatusClass}>
            <i />
            {chipSt}
          </span>
        </Button>
        <WindowControlsSlot />
      </header>

      <main className={mainClass}>
        <div className={"mw-head flex items-center gap-[16px] shrink-0"}>
          <div className={"mw-title flex flex-col gap-[4px]"}>
            <span className={"mw-where flex items-center gap-[8px] text-[14px] text-[var(--mw-muted)]"}>
              {translate("common.messages.projectsSavedIn")}{" "}
              <span className={pathClass}>{translate("robot.works.messages.documentsSchoolbot")}</span>
              <Button
                variant="custom"
                type="button"
                className={folderLinkClass}
                onClick={() => say(translate("robot.works.messages.toastOpenFolder"))}
              >{translate("robot.works.messages.openFolder")}</Button>
            </span>
          </div>
          <div className={"mw-spacer grow"} />
          {hasWorks && (
            <label className={searchClass}>
              <Icon name="search" size={18} className="text-muted" />
              <input
                type="text"
                value={q}
                onChange={(event) => setQ(event.target.value)}
                placeholder={translate("home.search.placeholder")}
                aria-label={translate("home.search.placeholder")}
              />
            </label>
          )}
          <Button
            variant="custom"
            type="button"
            className={openFileClass}
            disabled={usb}
            onClick={openUsb}
            title={translate("robot.works.messages.openAFileFromUsbOrFromYourTeacher")}
          >
            {usb ? translate("robot.works.messages.openingFile") : translate("robot.works.messages.openAFile")}
          </Button>
        </div>

        <div className={gridClass}>
          {naming == null ? (
            <Button
              variant="custom"
              type="button"
              className={newCardClass}
              onClick={() => {
                setRenameId(null);
                setNaming("");
              }}
            >
              <span
                className={newPlusClass}
                aria-hidden="true"
              >＋</span>
              <span className={"mw-new-tx flex flex-col items-start gap-[2px] text-left"}>
                <span className={"mw-new-t text-[20px] font-bold"}>{translate("home.create.title")}</span>
                <span className={"mw-new-s text-[13px] text-[var(--mw-muted)] font-normal"}>
                  {translate("robot.works.messages.startWithTheArmUpright")}
                </span>
              </span>
            </Button>
          ) : (
            <div className={nameCardClass}>
              <h2>{translate("robot.works.messages.nameYourNewProject")}</h2>
              <input
                type="text"
                className={editInputClass}
                value={naming}
                maxLength={30}
                placeholder={translate("robot.works.messages.forExamplePickUpABox")}
                aria-label={translate("robot.works.messages.newProjectName")}
                aria-invalid={nameState === "dup" || nameState === "bad" || undefined}
                autoFocus
                onChange={(event) => setNaming(event.target.value)}
                onKeyDown={onNamingKey}
              />
              <span
                className={`${pathNoteBaseClass}${nameState === "bad" || nameState === "dup" ? " is-err" : ""}`}
                role={nameState === "bad" || nameState === "dup" ? "alert" : undefined}
              >
                {pathText}
              </span>
              <div className={"mw-name-acts flex gap-[8px] mt-auto"}>
                <Button
                  variant="custom"
                  type="button"
                  className={cancelClass}
                  onClick={() => setNaming(null)}
                >{translate("common.cancel")}</Button>
                <Button
                  variant="custom"
                  type="button"
                  className={createClass}
                  disabled={nameState !== "ok"}
                  onClick={createWork}
                >{translate("robot.works.messages.createProject")}</Button>
              </div>
            </div>
          )}

          <WorkCards v={v} />
        </div>

        {hasWorks && shown.length === 0 && (
          <div className={noneClass}>
            {needle
              ? translate("robot.works.messages.noProjectsMatchValue0TryAnotherSearch", {value0: needle})
              : translate("common.messages.noProjectsYet")}
          </div>
        )}
        {!hasWorks && naming == null && !usb && (
          <div className={emptyClass}>
            {translate("robot.works.messages.noProjectsInTheSchoolbotFolderSelectNewProject")}
          </div>
        )}
      </main>

      {toast && (
        <div className={toastClass} role="status">
          <span>{toast.text}</span>
          {toast.undo && (
            <Button
              variant="custom"
              type="button"
              onClick={() => {
                toast.undo?.();
                setToast(null);
              }}
            >{translate("common.messages.undo")}</Button>
          )}
        </div>
      )}

      {sample && (
        <ScenarioBar
          items={scenarios}
          value={scenario}
          onChange={(id) => applyScenario(id as ScenarioId)}
        />
      )}
    </div>
  );
}
