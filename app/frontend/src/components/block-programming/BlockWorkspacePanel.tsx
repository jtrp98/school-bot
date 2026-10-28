import { t as translate } from "../../i18n";
import type { Ref } from "react";

type Props = {
  container: Ref<HTMLDivElement>;
  busy: boolean;
  save: () => Promise<void>;
  load: () => Promise<void>;
  run: () => Promise<void>;
};

const workspacePanelClass = [
  "panel bg-surface border-2 border-solid border-line rounded-card p-[24px]",
  "[@media(max-width:_1100px)]:p-[16px]",
].join(" ");
const panelHeadingClass = "panel-heading flex items-center justify-between gap-[16px] mb-[16px]";
const actionsClass = "actions flex gap-[8px] flex-wrap mt-[12px] [&_>_button]:[flex:1]";
const runButtonClass = [
  "primary bg-orange text-white border-orange border-b-orange-press text-[17px] hover:bg-orange",
  "hover:[filter:brightness(1.07)] hover:border-orange hover:border-b-orange-press disabled:bg-off",
  "disabled:text-faint disabled:border-off disabled:border-b-line-strong disabled:[filter:none]",
  "disabled:opacity-[1]",
].join(" ");
const blockHostClass = [
  "block-host h-[460px] relative border-2 border-solid border-line rounded-control overflow-hidden",
  "[@media(max-width:_850px)]:h-[400px]",
].join(" ");

export function BlockWorkspacePanel({ container, busy, save, load, run }: Props) {
  return (
    <section className={workspacePanelClass}>
      <div className={panelHeadingClass}>
        <h2>{translate("blocks.messages.buildYourSequence")}</h2>
        <div className={actionsClass}>
          <button onClick={save}>{translate("common.save")}</button>
          <button onClick={load}>{translate("common.load")}</button>
          <button className={runButtonClass} disabled={busy} onClick={run}>
            {busy ? translate("common.messages.checking") : translate("blocks.messages.runBlocks")}
          </button>
        </div>
      </div>
      <div
        ref={container}
        className={blockHostClass}
        aria-label={translate("blocks.messages.visualProgrammingWorkspace")}
      />
    </section>
  );
}
