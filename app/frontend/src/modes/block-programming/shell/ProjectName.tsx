import { useRef, useState, type KeyboardEvent } from "react";
import { useTranslation } from "react-i18next";
import { Tip } from "./Tip";
import type { BlockProject } from "./useBlockProject";

const NAME_LIMIT = 30;

function useSaveStatus(project: BlockProject) {
  const { t, i18n } = useTranslation();
  const clock = new Intl.DateTimeFormat(i18n.resolvedLanguage, { hour: "2-digit", minute: "2-digit" });
  const time = project.savedAt ? clock.format(project.savedAt) : "";
  if (project.saveState === "failed") return { text: t("blockLab.project.saveFailed"), tone: "text-err" };
  if (project.saveState === "unsaved") return { text: t("blockLab.project.unsaved"), tone: "text-muted" };
  const edited = project.saveState === "dirty";
  // A project opened from an earlier session has no save time in this one.
  if (!time) return { text: t(edited ? "blockLab.project.openedEdited" : "blockLab.project.opened"), tone: "text-ok" };
  return { text: t(edited ? "blockLab.project.savedEdited" : "blockLab.project.saved", { time }), tone: "text-ok" };
}

// Job name and save state, right-aligned under each other. Enter keeps a new name, Esc cancels,
// and an empty name quietly keeps the old one (as in Arm Lab).
export function ProjectName({ project }: { project: BlockProject }) {
  const { t } = useTranslation();
  const [draft, setDraft] = useState<string | null>(null);
  const done = useRef(false);
  const button = useRef<HTMLButtonElement>(null);
  const status = useSaveStatus(project);
  const shown = project.name || t("blockLab.job.untitled");

  function finish(commit: boolean) {
    if (done.current || draft === null) return;
    done.current = true;
    const next = draft.trim();
    if (commit && next) project.rename(next);
    setDraft(null);
    window.setTimeout(() => button.current?.focus());
  }

  function onKey(event: KeyboardEvent<HTMLInputElement>) {
    if (event.key === "Enter") finish(true);
    if (event.key === "Escape") {
      event.stopPropagation();
      finish(false);
    }
  }

  return (
    <div className="flex min-w-[150px] flex-[0_1_auto] flex-col items-end justify-center text-right">
      {draft !== null ? (
        <input
          type="text"
          autoFocus
          value={draft}
          maxLength={NAME_LIMIT}
          placeholder={t("blockLab.job.placeholder")}
          aria-label={t("blockLab.job.label")}
          onChange={event => setDraft(event.target.value)}
          onKeyDown={onKey}
          onBlur={() => finish(true)}
          className={[
            "box-border h-[44px] w-[260px] max-w-full rounded-[8px] border-0 bg-blue-tint px-[8px] text-right",
            "font-body text-[16px] leading-[1.7] font-bold text-blue focus:outline-none",
            "focus-visible:shadow-[0_0_0_2px_#1866C9,0_0_0_5px_#D0EBFD]",
          ].join(" ")}
        />
      ) : (
        <button
          ref={button}
          type="button"
          aria-label={t("blockLab.job.rename", { jobName: shown })}
          onClick={() => {
            done.current = false;
            setDraft(project.name);
          }}
          className={[
            "group relative block max-w-full cursor-text rounded-[8px] border-0 border-b-2 border-solid border-b-transparent",
            "bg-transparent px-[8px] py-[2px] text-right font-body text-[16px] leading-[1.7] font-bold text-ink",
            "hover:border-b-blue hover:bg-blue-tint hover:text-blue focus-visible:border-b-blue",
            "focus-visible:bg-blue-tint focus-visible:text-blue focus-visible:outline-none",
          ].join(" ")}
        >
          <span className="block truncate">{shown}</span>
          <Tip side="below-right">{t("blockLab.job.renameHint")}</Tip>
        </button>
      )}
      <span
        role="status"
        className={`block max-w-full truncate px-[8px] font-body text-[14px] leading-[1.7] font-semibold ${status.tone}`}
      >
        {status.text}
      </span>
    </div>
  );
}
