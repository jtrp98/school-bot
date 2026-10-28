import { Button, IconBtn } from "../../../components/shared";
import { t as translate } from "../../../i18n";
import { metaOf } from "./worksData";
import type { useWorksScreen } from "./WorksScreen";

const cardBaseClass =
  "mw-card relative min-h-[104px] rounded-[18px] bg-white border-2 border-solid border-[var(--mw-line)] " +
  "[transition:box-shadow_0.12s,_transform_0.12s] hover:shadow-pop hover:[transform:translateY(-2px)] " +
  "[&.is-open]:border-[3px] [&.is-open]:border-solid [&.is-open]:border-[var(--mw-blue)] " +
  "[&.is-fresh]:border-[3px] [&.is-fresh]:border-solid [&.is-fresh]:border-[var(--mw-green)] " +
  "[&.is-open.is-fresh]:border-[3px] [&.is-open.is-fresh]:border-solid " +
  "[&.is-open.is-fresh]:border-[var(--mw-blue)] [&.is-missing]:border-2 [&.is-missing]:border-dashed " +
  "[&.is-missing]:border-line-strong [&.is-wait]:border-[3px] [&.is-wait]:border-dashed " +
  "[&.is-wait]:border-blue-line [&.is-wait:hover]:shadow-none [&.is-wait:hover]:[transform:none] " +
  "[&_.mw-info]:pointer-events-none [&_.mw-info_input]:[pointer-events:auto] " +
  "[&_.mw-info_button]:[pointer-events:auto] [&:hover_.mw-acts]:opacity-[1] " +
  "[&:focus-within_.mw-acts]:opacity-[1]";
const usbCardClass = `${cardBaseClass} is-wait`;
const waitTopClass =
  "mw-wait-top h-[56px] rounded-[14px_14px_0_0] bg-[var(--mw-blue-tint)] flex items-center " +
  "justify-center gap-[10px] text-[var(--mw-blue)] text-[15px] font-bold";
const spinClass =
  "mw-spin w-[22px] h-[22px] rounded-[50%] border-[3px] border-solid border-blue-line " +
  "border-t-[var(--mw-blue)] animate-[mw-rot_0.8s_linear_infinite] " +
  "motion-reduce:[animation-duration:3s]";
const missTopClass =
  "mw-miss-top h-[84px] rounded-[14px_14px_0_0] bg-sunken flex items-center justify-center gap-[10px] " +
  "text-[var(--mw-muted)] [&_.mw-q]:text-[26px] [&_.mw-mt]:flex [&_.mw-mt]:flex-col " +
  "[&_.mw-mt]:leading-[1.4] [&_.mw-mt_b]:text-[15px] [&_.mw-mt_b]:font-bold [&_.mw-mt_span]:text-[12px]";
const hitClass =
  "mw-hit absolute inset-0 z-[1] w-full p-0 border-[medium] border-none border-current " +
  "rounded-[inherit] bg-transparent cursor-pointer hover:bg-transparent hover:[filter:none]";
const badgeOpenClass =
  "mw-badge-open h-[22px] py-0 px-[10px] rounded-full text-[12px] font-bold inline-flex items-center " +
  "leading-none bg-[var(--mw-blue-tint)] text-[var(--mw-blue)] border-[1.5px] border-solid " +
  "border-[var(--mw-blue)]";
const badgeTagClass =
  "mw-badge-tag h-[22px] py-0 px-[10px] rounded-full text-[12px] font-bold inline-flex items-center " +
  "leading-none bg-sunken text-[var(--mw-muted)] border-[1.5px] border-solid border-[var(--mw-line)]";
const renameBaseClass =
  "mw-rename h-[34px] py-0 px-[8px] border-2 border-solid border-[var(--mw-blue)] rounded-[8px] " +
  "text-[16px] font-bold text-[var(--mw-ink)] bg-white min-w-0 w-full [&.is-err]:border-err " +
  "focus:outline-none focus:shadow-[0_0_0_3px_var(--blue-tint)] " +
  "[&.is-err:focus]:shadow-[0_0_0_3px_var(--err-line)]";
const nameClass = "mw-nm text-[17px] font-bold whitespace-nowrap overflow-hidden text-ellipsis leading-[28px]";
const savedErrClass =
  "mw-saved text-[12px] text-[var(--mw-faint)] whitespace-nowrap overflow-hidden text-ellipsis " +
  "[&.is-err]:text-[var(--mw-red)] [&.is-err]:font-semibold is-err";
const metaClass = "mw-meta text-[13px] text-[var(--mw-muted)] whitespace-nowrap overflow-hidden text-ellipsis";
const missActsClass =
  "mw-miss-acts flex gap-[6px] [&_button]:[flex:1_1_0] [&_button]:h-[34px] [&_button]:rounded-[9px] " +
  "[&_button]:text-[13px] [&_button]:py-[1px] [&_button]:px-[6px]";
const locateClass =
  "mw-ibtn border-2 border-solid border-[var(--mw-line-strong)] bg-white font-bold cursor-pointer " +
  "[&:hover:not(:disabled)]:bg-[var(--mw-blue-tint)] [&:hover:not(:disabled)]:border-[var(--mw-blue)] " +
  "[&:hover:not(:disabled)]:text-[var(--mw-blue)] [&.mw-red:hover:not(:disabled)]:bg-err-tint " +
  "[&.mw-red:hover:not(:disabled)]:border-err [&.mw-red:hover:not(:disabled)]:text-[var(--mw-red)] " +
  "disabled:opacity-[1] disabled:cursor-not-allowed mw-locate border-b-[3px] text-[var(--mw-blue)]";
const forgetClass =
  "mw-ibtn border-2 border-solid border-[var(--mw-line-strong)] bg-white font-bold cursor-pointer " +
  "[&:hover:not(:disabled)]:bg-[var(--mw-blue-tint)] [&:hover:not(:disabled)]:border-[var(--mw-blue)] " +
  "[&:hover:not(:disabled)]:text-[var(--mw-blue)] [&.mw-red:hover:not(:disabled)]:bg-err-tint " +
  "[&.mw-red:hover:not(:disabled)]:border-err [&.mw-red:hover:not(:disabled)]:text-[var(--mw-red)] " +
  "disabled:opacity-[1] disabled:cursor-not-allowed mw-forget text-[var(--mw-muted)]";
const actsClass = "mw-acts absolute z-[2] top-[8px] right-[8px] flex gap-[6px] opacity-0 [transition:opacity_0.12s]";
const actClass =
  "mw-ibtn border-2 border-solid border-[var(--mw-line-strong)] bg-white cursor-pointer " +
  "[&:hover:not(:disabled)]:bg-[var(--mw-blue-tint)] [&:hover:not(:disabled)]:border-[var(--mw-blue)] " +
  "[&:hover:not(:disabled)]:text-[var(--mw-blue)] [&.mw-red:hover:not(:disabled)]:bg-err-tint " +
  "[&.mw-red:hover:not(:disabled)]:border-err [&.mw-red:hover:not(:disabled)]:text-[var(--mw-red)] " +
  "disabled:opacity-[1] disabled:cursor-not-allowed mw-act w-[34px] h-[34px] p-0 rounded-[9px] " +
  "text-[var(--mw-muted)] text-[15px] font-normal leading-none";
const actDangerClass =
  "mw-ibtn border-2 border-solid border-[var(--mw-line-strong)] bg-white cursor-pointer " +
  "[&:hover:not(:disabled)]:bg-[var(--mw-blue-tint)] [&:hover:not(:disabled)]:border-[var(--mw-blue)] " +
  "[&:hover:not(:disabled)]:text-[var(--mw-blue)] [&.mw-red:hover:not(:disabled)]:bg-err-tint " +
  "[&.mw-red:hover:not(:disabled)]:border-err [&.mw-red:hover:not(:disabled)]:text-[var(--mw-red)] " +
  "disabled:opacity-[1] disabled:cursor-not-allowed mw-red mw-act w-[34px] h-[34px] p-0 rounded-[9px] " +
  "text-[var(--mw-muted)] text-[15px] font-normal leading-none";

export function WorkCards({ v }: { v: ReturnType<typeof useWorksScreen> }) {
  const {
    shown,
    usb,
    missing,
    workName,
    robotName,
    live,
    renameId,
    renameVal,
    setRenameVal,
    renameState,
    onRenameKey,
    finishRename,
    openWork,
    locate,
    removeWork,
    startRename,
    copyWork,
  } = v;
  return (
    <>
          {usb && (
            <div
              className={usbCardClass}
              role="status"
              aria-busy="true"
              aria-label={translate("robot.works.messages.openingAUsbFile")}
            >
              <div className={waitTopClass}>
                <span className={spinClass} aria-hidden="true" />
                {translate("robot.works.messages.copyingFromUsbE")}
              </div>
              <div className={"mw-info relative p-[16px] flex flex-col gap-[6px]"}>
                <div className={"mw-bar bg-off w-[130px] h-[30px] rounded-full"}  />
                <div className={"mw-bar rounded-[8px] bg-off w-[170px] h-[24px]"}  />
                <div className={"mw-bar h-[16px] rounded-[8px] bg-off w-[110px]"}  />
              </div>
            </div>
          )}

          {shown.map((w) => {
            const isMissing = missing.includes(w.id);
            const isOpen = !isMissing && w.name === workName;
            const mine = w.robot === robotName;
            const renaming = renameId === w.id;
            const stateClass = isMissing
              ? " is-missing"
              : isOpen
                ? " is-open"
                : w.fresh
                  ? " is-fresh"
                  : "";
            const cls = `${cardBaseClass}${stateClass}`;
            const savedText = w.fromUsb ? w.saved : translate("robot.works.messages.savedLine", { when: w.saved });
            const roboLabel = translate(
              mine && live ? "robot.works.messages.robotOfWorkLive" : "robot.works.messages.robotOfWork",
              { robot: w.robot },
            );
            return (
              <div key={w.id} className={cls} title={roboLabel}>
                {isMissing && (
                  <div className={missTopClass}>
                    <span className="mw-q" aria-hidden="true">❔</span>
                    <span className="mw-mt">
                      <b>{translate("robot.works.messages.fileNotFound")}</b>
                      <span>{translate("robot.works.messages.theFileMayHaveBeenMovedRenamedOrDeleted")}</span>
                    </span>
                  </div>
                )}
                {!isMissing && !renaming && (
                  <Button
                    variant="custom"
                    type="button"
                    className={hitClass}
                    aria-label={translate("common.messages.openProjectValue0", {value0: w.name})}
                    onClick={() => openWork(w)}
                  />
                )}
                <div className={"mw-info relative p-[16px] flex flex-col gap-[6px]"}>
                  {(isOpen || w.tag) && (
                    <span className={"mw-badges flex items-center gap-[6px]"}>
                      {isOpen && (
                        <span className={badgeOpenClass}>{translate("robot.works.messages.currentlyOpen")}</span>
                      )}
                      {w.tag && <span className={badgeTagClass}>{w.tag}</span>}
                    </span>
                  )}
                  {renaming ? (
                    <input
                      type="text"
                      className={`${renameBaseClass}${renameState !== "ok" ? " is-err" : ""}`}
                      value={renameVal}
                      maxLength={30}
                      aria-label={translate("robot.works.projectName")}
                      aria-invalid={renameState !== "ok" || undefined}
                      autoFocus
                      onFocus={(event) => event.currentTarget.select()}
                      onChange={(event) => setRenameVal(event.target.value)}
                      onKeyDown={onRenameKey}
                      onBlur={() => finishRename(renameState === "ok")}
                    />
                  ) : (
                    <span className={nameClass}>{w.name}</span>
                  )}
                  {renaming && renameState !== "ok" ? (
                    <span className={savedErrClass} role="alert">
                      {renameState === "dup"
                        ? translate("robot.works.messages.aProjectWithThisNameAlreadyExists")
                        : renameState === "bad"
                          ? translate("robot.works.messages.thisNameContainsInvalidCharacters")
                          : translate("robot.works.messages.enterAProjectName")}
                    </span>
                  ) : (
                    <span className={metaClass}>
                      {isMissing
                        ? translate("robot.works.messages.previouslyAtSchoolbotValue0Ini", {value0: w.name})
                        : `${metaOf(w)} · ${savedText}`}
                    </span>
                  )}
                  {isMissing && (
                    <span className={missActsClass}>
                      <Button
                        variant="custom"
                        type="button"
                        className={locateClass}
                        onClick={() => locate(w)}
                      >{translate("robot.works.messages.locateFile")}</Button>
                      <Button
                        variant="custom"
                        type="button"
                        className={forgetClass}
                        onClick={() => removeWork(w, "list")}
                      >{translate("robot.works.messages.removeFromList")}</Button>
                    </span>
                  )}
                </div>
                {!isMissing && (
                  <div className={actsClass}>
                    <IconBtn
                      variant="custom"
                      type="button"
                      className={actClass}
                      label={translate("robot.works.messages.renameProjectValue0", {value0: w.name})}
                      title={translate("robot.works.messages.renameAlsoChangesTheFileName")}
                      onClick={() => startRename(w)}
                    >
                      ✎
                    </IconBtn>
                    <IconBtn
                      variant="custom"
                      type="button"
                      className={actClass}
                      label={translate("robot.works.messages.duplicateProjectValue0", {value0: w.name})}
                      title={translate("robot.works.messages.duplicate")}
                      onClick={() => copyWork(w)}
                    >
                      ⧉
                    </IconBtn>
                    <IconBtn
                      variant="custom"
                      type="button"
                      className={actDangerClass}
                      disabled={isOpen}
                      label={isOpen
                        ? translate("robot.works.messages.cannotDeleteOpenProjectValue0", {value0: w.name})
                        : translate("robot.works.messages.moveProjectValue0ToTrash", {value0: w.name})}
                      title={isOpen
                        ? translate("robot.works.messages.openProjectsCannotBeDeleted")
                        : translate("robot.works.messages.moveToTrashRecoverable")}
                      onClick={() => removeWork(w, "trash")}
                    >
                      🗑
                    </IconBtn>
                  </div>
                )}
              </div>
            );
          })}
    </>
  );
}
