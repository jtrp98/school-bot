import { useTranslation } from "react-i18next";
import { Button } from "../../../components/shared/Button";
import { Icon } from "../../../components/shared/Icons";
import { FlatButton } from "../shell/FlatButton";
import { boardDetail, boardName } from "./boardLabel";
import { boardCards, type BoardCardView, type CardStatus, type CardTag } from "./connectView";
import { UnplugSearchBox } from "./UnplugSearchBox";
import type { ConnectScreenState } from "./useConnectScreen";

const CARD_LOOK: Record<CardStatus, string> = {
  lost: "border-2 border-dashed border-err bg-sunken",
  inUse: "border-2 border-solid border-ok bg-surface",
  picked: "border-2 border-solid border-blue bg-blue-tint shadow-[0_0_0_1px_var(--blue)]",
  free: "border-2 border-solid border-line bg-surface",
};

const TAG_LOOK: Record<CardTag, string> = {
  lost: "border-err text-err",
  returned: "border-ok text-ok",
  inUse: "border-ok text-ok",
  disconnected: "border-muted text-muted",
  ours: "border-ok text-ok",
  oursMoved: "border-ok text-ok",
};

const TAG_KEYS: Record<CardTag, string> = {
  lost: "blockLab.connect.card.tag.lost",
  returned: "blockLab.connect.card.tag.returned",
  inUse: "blockLab.connect.card.tag.inUse",
  disconnected: "blockLab.connect.card.tag.disconnected",
  ours: "blockLab.connect.card.tag.ours",
  oursMoved: "blockLab.connect.card.tag.oursMoved",
};

const PICK_KEYS: Record<CardStatus, { label: string; aria: string }> = {
  lost: { label: "blockLab.connect.card.waitingBack", aria: "blockLab.connect.card.pickAria.waitingBack" },
  inUse: { label: "blockLab.connect.card.inUse", aria: "blockLab.connect.card.pickAria.inUse" },
  picked: { label: "blockLab.connect.card.picked", aria: "blockLab.connect.card.pickAria.picked" },
  free: { label: "blockLab.connect.card.pick", aria: "blockLab.connect.card.pickAria.pick" },
};

function BoardCard({ card, busy, onPick }: {
  card: BoardCardView;
  busy: boolean;
  onPick: (key: string, status: CardStatus) => void;
}) {
  const { t } = useTranslation();
  const name = boardName(card);
  const off = busy || card.status === "lost" || card.status === "inUse";
  const showsCheck = card.tag === "inUse" || card.tag === "ours" || card.tag === "oursMoved";
  return (
    <div
      className={[
        "box-border flex min-w-[360px] items-center gap-[12px] rounded-[16px] px-[16px] py-[12px]",
        CARD_LOOK[card.status],
      ].join(" ")}
    >
      <Icon name="chip" size={32} className={card.status === "lost" ? "text-muted" : "text-blue"} />
      <span className="flex flex-col leading-[1.6]">
        <span className="text-[18px] font-bold">{name}</span>
        <span className="text-[14px] text-muted">{boardDetail(card)}</span>
      </span>
      {card.tag && (
        <span
          className={[
            "box-border inline-flex h-[28px] items-center gap-[4px] rounded-full border-2 border-solid bg-surface",
            "px-[12px] text-[14px] font-bold whitespace-nowrap",
            TAG_LOOK[card.tag],
          ].join(" ")}
        >
          {card.tag === "lost" && <Icon name="unplug" size={16} />}
          {card.tag === "returned" && <Icon name="plug" size={16} />}
          {showsCheck && <Icon name="check" size={16} />}
          {t(TAG_KEYS[card.tag], { port: card.port })}
        </span>
      )}
      <div className="flex-1" />
      {/* The card itself shows which board is picked; its button keeps the same flat look. */}
      <FlatButton
        inset="roomy"
        aria-disabled={off || undefined}
        aria-pressed={card.status === "picked"}
        aria-label={t(PICK_KEYS[card.status].aria, { boardName: name, port: card.port })}
        onClick={() => onPick(card.key, card.status)}
      >
        {t(PICK_KEYS[card.status].label)}
      </FlatButton>
    </div>
  );
}

// Ports that are not boards are listed for reference only: v1 has no way to use one.
export function BoardCards({ screen }: { screen: ConnectScreenState }) {
  const { t } = useTranslation();
  const cards = boardCards(screen);
  const { scanning, link } = screen.conn;
  const canSearch = screen.boards.length > 1 && !screen.search.state && screen.phase === "idle" && !link;
  const noCards = cards.length === 0 && screen.search.state?.step !== "plugBack";
  return (
    <>
      <div className="flex items-center gap-[12px]">
        <span className="text-[18px] font-bold">{t("blockLab.connect.list.title")}</span>
        <span className="text-[14px] text-muted">
          {t(scanning ? "blockLab.connect.list.searching" : "blockLab.connect.list.autoUpdate")}
        </span>
        <div className="flex-1" />
        <Button variant="secondary" size="sm" icon="refresh" loading={scanning} onClick={screen.rescan}>
          {t("blockLab.connect.list.rescan")}
        </Button>
      </div>
      {cards.length > 0 && (
        <div className="flex flex-wrap gap-[12px]">
          {cards.map(card => (
            <BoardCard key={card.id} card={card} busy={screen.phase !== "idle"} onPick={screen.pick} />
          ))}
        </div>
      )}
      {canSearch && (
        <Button variant="secondary" size="md" className="self-start" onClick={screen.startSearch}>
          {t("blockLab.connect.search.start")}
        </Button>
      )}
      {screen.search.state && <UnplugSearchBox screen={screen} />}
      {noCards && (
        <div
          className={[
            "box-border flex h-[160px] shrink-0 flex-col items-center justify-center gap-[8px] rounded-[16px]",
            "border-2 border-dashed border-line-strong bg-surface",
          ].join(" ")}
        >
          <Icon name="plug" size={32} className="text-muted" />
          <span className="text-[24px] leading-[1.3] font-bold">
            {t(scanning ? "blockLab.connect.list.searching" : "blockLab.connect.empty.title")}
          </span>
          <span className="text-[16px] text-muted">{t("blockLab.connect.empty.body")}</span>
        </div>
      )}
      {screen.others.length > 0 && (
        <div className="flex flex-col gap-[8px]">
          <Button
            variant="ghost"
            size="sm"
            icon={screen.showOthers ? "chevD" : "chevR"}
            aria-expanded={screen.showOthers}
            className="self-start"
            onClick={screen.toggleOthers}
          >
            {t("blockLab.connect.others.toggle", { deviceCount: screen.others.length })}
          </Button>
          {screen.showOthers && (
            <ul className="m-0 flex list-none flex-wrap gap-[12px] p-0">
              {screen.others.map(device => (
                <li
                  key={device.port}
                  className={[
                    "flex flex-col rounded-[12px] border-2 border-solid border-line bg-surface",
                    "px-[12px] py-[8px]",
                  ].join(" ")}
                >
                  <span className="text-[16px] font-semibold">
                    {t(device.usb ? "blockLab.connect.others.usbName" : "blockLab.connect.others.serialName")}
                  </span>
                  <span className="text-[14px] text-muted">
                    {t("blockLab.connect.card.port", { port: device.port })}
                  </span>
                  <span className="text-[14px] text-muted">
                    {t(device.usb ? "blockLab.connect.others.usbReason" : "blockLab.connect.others.serialReason")}
                  </span>
                </li>
              ))}
            </ul>
          )}
        </div>
      )}
    </>
  );
}
