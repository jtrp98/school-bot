import { t as translate } from "../../i18n";
import type { Command } from "../../core/types";
import GameCanvas from "./GameCanvas";

type Props = {
  source: string;
  setSource: (source: string) => void;
  commands: Command[];
  runID: number;
  busy: boolean;
  status: string;
  run: () => Promise<void>;
  save: () => Promise<void>;
  load: () => Promise<void>;
  reset: () => void;
};

const introClass = [
  "intro flex items-center justify-between mb-[24px] [&_p]:text-muted [&_p]:text-[13px]",
  "[@media(max-width:_1100px)]:gap-[16px] [@media(max-width:_850px)]:items-start",
  "[@media(max-width:_850px)]:flex-col [@media(max-width:_850px)]:mb-[16px]",
  "[@media(max-width:_850px)]:gap-0",
].join(" ");
const pillClass = [
  "pill text-[10px] font-bold tracking-[1px] bg-blue-tint border-1 border-solid border-blue-line",
  "text-blue-press rounded-[6px] py-[5px] px-[8px] whitespace-nowrap [&.mint]:bg-ok-tint",
  "[&.mint]:text-ok-ink [&.mint]:border-ok-line mint",
].join(" ");
const layoutClass = [
  "mode-layout grid gap-[24px] [@media(max-width:_1100px)]:gap-[16px] adventure-layout",
  "[grid-template-columns:minmax(280px,_1fr)_minmax(0,_1.7fr)]",
  "[@media(max-width:_850px)]:[grid-template-columns:1fr]",
].join(" ");
const editorPanelClass = [
  "panel bg-surface border-2 border-solid border-line rounded-card p-[24px]",
  "[@media(max-width:_1100px)]:p-[16px]",
].join(" ");
const tinyLabelClass = "tiny-label text-[9px] tracking-[1.5px] text-faint";
const panelHeadingClass = "panel-heading flex items-center justify-between gap-[16px] mb-[16px]";
const editorLabelClass = "editor-label block text-[12px] text-muted mb-[8px]";
const runButtonClass = [
  "primary bg-orange text-white border-orange border-b-orange-press text-[17px] hover:bg-orange",
  "hover:[filter:brightness(1.07)] hover:border-orange hover:border-b-orange-press disabled:bg-off",
  "disabled:text-faint disabled:border-off disabled:border-b-line-strong disabled:[filter:none]",
  "disabled:opacity-[1]",
].join(" ");
const actionsClass = "actions flex gap-[8px] flex-wrap mt-[12px] [&_>_button]:[flex:1]";
const mutedNoteClass = "muted text-muted text-[12px]";
const gamePanelClass = [
  "panel bg-surface border-2 border-solid border-line rounded-card [@media(max-width:_1100px)]:p-[16px]",
  "game-panel pt-[24px] px-[16px] pb-[16px] min-w-0",
].join(" ");
const noticeClass = [
  "notice whitespace-pre-wrap [overflow-wrap:anywhere] border-2 border-solid border-blue-line",
  "bg-blue-tint text-blue-press rounded-control py-[12px] px-[16px] mt-0 mx-0 mb-[20px] text-[13px]",
  "[&.error]:text-err-ink [&.error]:bg-err-tint [&.error]:border-err-line",
].join(" ");

export function AdventurePlayground({
  source,
  setSource,
  commands,
  runID,
  busy,
  status,
  run,
  save,
  load,
  reset,
}: Props) {
  return (
    <>
      <div className={introClass}>
        <p>{translate("adventure.messages.aFewLinesCanTakeYouSomewhereNew")}</p>
        <span className={pillClass}>{translate("adventure.messages.localGamePhaser")}</span>
      </div>
      <div className={layoutClass}>
        <section className={editorPanelClass}>
          <div className={panelHeadingClass}>
            <h2>{translate("adventure.messages.yourProgram")}</h2>
            <span className={tinyLabelClass}>{translate("adventure.messages.teachingDsl")}</span>
          </div>
          <label className={editorLabelClass} htmlFor="source">
            {translate("adventure.messages.heroCommands")}
          </label>
          <textarea
            id="source"
            spellCheck={false}
            value={source}
            onChange={(event) => setSource(event.target.value)}
            maxLength={16384}
          />
          <div className={actionsClass}>
            <button
              className={runButtonClass}
              disabled={busy}
              onClick={run}
            >
              {busy ? translate("common.messages.checking") : translate("adventure.messages.runProgram")}
            </button>
            <button
              onClick={reset}
            >{translate("adventure.messages.resetScene")}</button>
          </div>
          <div className={actionsClass}>
            <button onClick={save}>{translate("common.save")}</button>
            <button onClick={load}>{translate("common.load")}</button>
          </div>
          <p className={mutedNoteClass}>
            {translate("adventure.messages.supportedMoveRight110MoveLeft110")}
          </p>
        </section>
        <section className={gamePanelClass}>
          <div className={panelHeadingClass}>
            <h2>{translate("adventure.messages.adventurePlayground")}</h2>
            <span className={tinyLabelClass}>{translate("adventure.messages.30FpsTargetUnmeasured")}</span>
          </div>
          <GameCanvas commands={commands} runID={runID} />
        </section>
      </div>
      <div className={noticeClass} role="status">
        {translate(status)}
      </div>
    </>
  );
}
