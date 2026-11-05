import type { Ref } from "react";
import { useTranslation } from "react-i18next";
import { Button } from "../../../components/shared/Button";
import { Icon } from "../../../components/shared/Icons";
import { WindowControlsSlot } from "../../../components/shared/WindowTitlebar";
import blockLabLogo from "../../../assets/logo-block-lab.svg";
import schoolBotLogo from "../../../assets/logo-school-bot.svg";
import { useShell } from "../../../shell";
import type { BoardConnection } from "../connect/useBoardConnection";
import { leaveBlockEngine } from "./blockEngine";
import { BoardChip } from "./BoardChip";
import { MainMenu } from "./MainMenu";
import { ProjectName } from "./ProjectName";
import { Tip } from "./Tip";
import type { BlockProject } from "./useBlockProject";

const Divider = () => <span aria-hidden="true" className="h-[60px] w-px shrink-0 self-stretch bg-line" />;

// Home › ☰ › logos | space | board chip + Change | job name | window buttons. There is no
// block/code tab: this version has no code mode.
export function WindowBar({
  project,
  board,
  connectOpen,
  changeButton,
  onChangeBoard,
}: {
  project: BlockProject;
  board: BoardConnection;
  connectOpen: boolean;
  changeButton: Ref<HTMLButtonElement>;
  onChangeBoard: () => void;
}) {
  const { t } = useTranslation();
  const { leaveMode } = useShell();
  return (
    <header
      className={[
        "window-integrated-header relative z-[56] box-border flex h-[60px] shrink-0 items-center gap-[12px]",
        "border-b-2 border-solid border-b-line border-x-0 border-t-0 bg-surface pl-[16px] text-ink select-none",
      ].join(" ")}
    >
      <button
        type="button"
        aria-label={t("nav.home")}
        onClick={() => project.guard("leave", () => {
          leaveBlockEngine();
          leaveMode("home");
        })}
        className={[
          "group relative box-border inline-flex h-[44px] w-[44px] shrink-0 cursor-pointer items-center justify-center",
          "rounded-[12px] border-2 border-solid border-line-strong bg-surface p-0 text-ink hover:border-blue",
          "hover:bg-blue-tint hover:text-blue active:border-blue active:bg-blue active:text-white",
        ].join(" ")}
      >
        <Icon name="home" size={20} />
        <Tip side="below-left">{t("nav.home")}</Tip>
      </button>
      <MainMenu project={project} />
      <div className="flex shrink-0 items-center gap-[6px]">
        <img src={schoolBotLogo} alt="School Bot Studio" width="124" height="22" className="block h-[22px] w-[124px]" />
        <img src={blockLabLogo} alt="Block Lab" width="83" height="20" className="-ml-[3px] block h-[20px] w-[83px]" />
      </div>
      <Divider />
      <div aria-hidden="true" className="min-w-0 flex-1" />
      <Divider />
      <div className="flex shrink-0 items-center gap-[4px]">
        <BoardChip link={board.link} names={board.names} unchecked={board.failure !== null} />
        {/* With the cable out, the board screen is where the child confirms the same board again. */}
        {board.link?.lost && (
          <Button
            variant="secondary"
            size="sm"
            aria-disabled={connectOpen || undefined}
            onClick={connectOpen ? undefined : onChangeBoard}
            className="group shrink-0 text-err!"
          >
            {t("blockLab.board.reconnect")}
            <Tip side="below-right">{t("blockLab.board.reconnectTip")}</Tip>
          </Button>
        )}
        <Button
          ref={changeButton}
          variant="secondary"
          size="sm"
          aria-disabled={connectOpen || undefined}
          aria-label={t("blockLab.board.changeLabel")}
          onClick={connectOpen ? undefined : onChangeBoard}
          className="group shrink-0"
        >
          {t("blockLab.board.change")}
          <Tip side="below-right">{t(connectOpen ? "blockLab.board.changeHere" : "blockLab.board.changeLabel")}</Tip>
        </Button>
      </div>
      <Divider />
      <ProjectName project={project} />
      <Divider />
      <WindowControlsSlot />
    </header>
  );
}
