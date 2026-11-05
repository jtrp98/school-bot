import { lazy, Suspense, useEffect, useId, useRef } from "react";
import { useTranslation } from "react-i18next";
import { Button } from "../../../components/shared/Button";
import { ConnectionMascot } from "../../../components/shared/ConnectionMascot";
import { preview } from "../../../core/client";
import { BoardCards } from "./BoardCards";
import { ConnectFooter } from "./ConnectFooter";
import { ConnectStatus } from "./ConnectStatus";
import { mascotLine, type MascotLine } from "./connectView";
import { NumberedTips } from "./NumberedTips";
import type { BoardConnection } from "./useBoardConnection";
import { useConnectScreen, type ConnectHint } from "./useConnectScreen";

// The sample ports panel is a development aid for the browser preview, so production builds
// leave it out and installed builds never load it.
const PreviewBoardsPanel = import.meta.env.DEV && preview ? lazy(() => import("./PreviewBoardsPanel")) : null;

const SAY_KEYS: Record<MascotLine, { title: string; sub: string }> = {
  lost: { title: "blockLab.connect.say.lost.title", sub: "blockLab.connect.say.lost.sub" },
  returned: { title: "blockLab.connect.say.returned.title", sub: "blockLab.connect.say.returned.sub" },
  back: { title: "blockLab.connect.say.back.title", sub: "blockLab.connect.say.back.sub" },
  naming: { title: "blockLab.connect.say.naming.title", sub: "blockLab.connect.say.naming.sub" },
  ok: { title: "blockLab.connect.say.ok.title", sub: "blockLab.connect.say.ok.sub" },
  connected: { title: "blockLab.connect.say.connected.title", sub: "blockLab.connect.say.connected.sub" },
  switching: { title: "blockLab.connect.say.switching.title", sub: "blockLab.connect.say.switching.sub" },
  scanFailed: { title: "blockLab.connect.say.scanFailed.title", sub: "blockLab.connect.say.scanFailed.sub" },
  scanning: { title: "blockLab.connect.say.scanning.title", sub: "blockLab.connect.say.scanning.sub" },
  disconnected: { title: "blockLab.connect.say.disconnected.title", sub: "blockLab.connect.say.disconnected.sub" },
  none: { title: "blockLab.connect.say.none.title", sub: "blockLab.connect.say.none.sub" },
  search: { title: "blockLab.connect.say.search.title", sub: "blockLab.connect.say.search.sub" },
  one: { title: "blockLab.connect.say.one.title", sub: "blockLab.connect.say.one.sub" },
  oneMaybe: { title: "blockLab.connect.say.oneMaybe.title", sub: "blockLab.connect.say.oneMaybe.sub" },
  many: { title: "blockLab.connect.say.many.title", sub: "blockLab.connect.say.many.sub" },
};

const HINT_KEYS: Record<ConnectHint, string> = {
  waitLost: "blockLab.connect.hint.waitLost",
  inUse: "blockLab.connect.hint.inUse",
  busy: "blockLab.connect.hint.busy",
  pickFirst: "blockLab.connect.hint.pickFirst",
  nameFirst: "blockLab.connect.hint.nameFirst",
};

// "Connect Uno board": shown over the block area (which stays mounted underneath) from the
// window bar's Change button. Choosing a board opens no port; the card only says what USB
// reports, and sending programs or reading Serial are not part of this screen yet.
export function ConnectUnoScreen({ conn, onClose }: { conn: BoardConnection; onClose: () => void }) {
  const { t } = useTranslation();
  const screen = useConnectScreen(conn, onClose);
  const root = useRef<HTMLDivElement>(null);
  const titleId = useId();
  useEffect(() => root.current?.focus(), []);
  const line = SAY_KEYS[mascotLine(screen)];
  const { link, devices, failure } = conn;
  const noBoards = devices !== null && screen.boards.length === 0;
  const showHelp = !link && (noBoards || !!failure);
  const helpTips = [
    t("blockLab.connect.help.tips.cable"),
    t("blockLab.connect.help.tips.light"),
    t("blockLab.connect.help.tips.dataCable"),
  ];
  return (
    <div
      ref={root}
      tabIndex={-1}
      role="region"
      aria-labelledby={titleId}
      className={[
        "absolute inset-x-0 top-[60px] bottom-0 z-[50] flex flex-col bg-bg font-body leading-[1.6] text-ink",
        "outline-none",
      ].join(" ")}
    >
      <header
        className={[
          "box-border flex h-[60px] shrink-0 items-center gap-[16px] border-0 border-b-2 border-solid border-line",
          "bg-surface px-[24px]",
        ].join(" ")}
      >
        <span id={titleId} className="text-[20px] leading-[1.3] font-bold">
          {t("blockLab.connect.title")}
        </span>
        <div className="flex-1" />
        <Button variant="secondary" size="sm" icon="chevL" onClick={onClose}>
          {t("blockLab.connect.back")}
        </Button>
      </header>
      <div className="box-border min-h-0 flex-1 overflow-y-auto px-[48px] pt-[24px] pb-[20px]">
        <div className="mx-auto flex max-w-[1200px] flex-col gap-[16px]">
          <div className="flex items-center gap-[20px]">
            <ConnectionMascot width={64} />
            <div
              role="status"
              className={[
                "flex max-w-[800px] flex-col rounded-[16px] border-2 border-solid border-line bg-surface",
                "px-[20px] py-[12px]",
              ].join(" ")}
            >
              <span className="text-[24px] leading-[1.3] font-bold">
                {t(line.title, { boardCount: screen.boards.length })}
              </span>
              <span className="text-[16px] text-muted">{t(line.sub)}</span>
            </div>
          </div>
          <ConnectStatus screen={screen} />
          <BoardCards screen={screen} />
          {showHelp && (
            <div className="flex flex-col gap-[8px]">
              <span className="text-[18px] font-bold">
                {t(noBoards ? "blockLab.connect.help.title" : "blockLab.connect.help.titleMore")}
              </span>
              <NumberedTips tips={helpTips} look="card" />
            </div>
          )}
        </div>
      </div>
      {screen.hint && (
        <div
          role="status"
          className={[
            "absolute right-[120px] bottom-[112px] z-[5] max-w-[480px] rounded-[12px] bg-ink px-[16px] py-[12px]",
            "text-[16px] text-white shadow-[0_8px_20px_rgba(20,36,61,.2)]",
            "motion-safe:animate-[block-lab-pop-in_.2s_cubic-bezier(.2,.8,.2,1)]",
          ].join(" ")}
        >
          {t(HINT_KEYS[screen.hint])}
        </div>
      )}
      <ConnectFooter screen={screen} />
      {PreviewBoardsPanel && (
        <Suspense fallback={null}>
          <PreviewBoardsPanel onChange={screen.rescan} />
        </Suspense>
      )}
    </div>
  );
}
