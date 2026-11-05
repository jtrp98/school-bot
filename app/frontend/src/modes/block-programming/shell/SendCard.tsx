import { useEffect, useRef, type KeyboardEvent, type ReactNode, type RefObject } from "react";
import { useTranslation } from "react-i18next";
import heroRobot from "../../../assets/hero-robot.webp";
import { Button } from "../../../components/shared/Button";
import { ConnectionMascot } from "../../../components/shared/ConnectionMascot";
import { Icon } from "../../../components/shared/Icons";
import { sendCardView, type ShownSendState } from "./sendCardView";
import { CardNote, StepBar, TipList, ToolOutput } from "./SendCardParts";
import { isBuilding } from "./sendOutcome";
import type { SendToBoard } from "./useSendToBoard";

// The drawer's send button carries this attribute, so a card can tell that focus is still on it.
const SEND_BUTTON = "[data-block-lab-send]";

type FocusRule = "result" | "question" | "ifLost";

// A result or a question takes focus from the send button, from the card itself, or from
// nowhere (a replaced card's button). A running step takes it only from nowhere, so a second
// Enter on "send" never lands on "cancel". Focus the child moved elsewhere stays there.
function useCardFocus(panel: RefObject<HTMLDivElement | null>, rule: FocusRule, shownAs: string) {
  useEffect(() => {
    const active = document.activeElement;
    const lost = !active || active === document.body;
    const onSend = active instanceof Element && active.closest(SEND_BUTTON) !== null;
    const inCard = !!active && !!panel.current?.contains(active);
    const take = rule === "ifLost" ? lost : lost || onSend || inCard;
    if (take) panel.current?.querySelector<HTMLButtonElement>("button")?.focus();
  }, [panel, rule, shownAs]);
}

function Actions({ children }: { children: ReactNode }) {
  return <div className="flex flex-wrap items-center justify-start gap-[12px]">{children}</div>;
}

function CardActions({ state, send, onPickBoard }: {
  state: ShownSendState;
  send: SendToBoard;
  onPickBoard: () => void;
}) {
  const { t } = useTranslation();
  const retry = (
    <Button variant="blue" size="md" onClick={() => void send.start()}>
      {t("blockLab.send.retry")}
    </Button>
  );
  const close = (
    <Button variant="secondary" size="md" onClick={send.close}>
      {t("blockLab.send.close")}
    </Button>
  );
  const pickBoard = (
    <Button variant="blue" size="md" onClick={onPickBoard}>
      {t("blockLab.send.noBoard.pick")}
    </Button>
  );
  switch (state.status) {
    case "noBoard":
      return <Actions>{state.chosen ? retry : pickBoard}{close}</Actions>;
    case "compiling":
      return (
        <Actions>
          <Button variant="secondary" size="md" onClick={send.cancel}>
            {t("blockLab.send.compiling.cancel")}
          </Button>
        </Actions>
      );
    case "uploading":
      if (state.confirmingStop) {
        return (
          <Actions key="confirm">
            <Button variant="blue" size="md" onClick={send.keepSending}>
              {t("blockLab.send.uploading.keep")}
            </Button>
            <Button variant="secondary" size="md" onClick={send.stopSending}>
              {t("blockLab.send.uploading.stop")}
            </Button>
          </Actions>
        );
      }
      return (
        <Actions>
          <Button variant="secondary" size="md" onClick={send.cancel}>
            {t("blockLab.send.uploading.cancel")}
          </Button>
        </Actions>
      );
    case "stopping":
      return null;
    case "cancelled":
      return <Actions>{state.step === "upload" ? <>{retry}{close}</> : close}</Actions>;
    case "sent":
      return (
        <Actions>
          <Button variant="blue" size="md" onClick={send.close}>
            {t("blockLab.send.sent.done")}
          </Button>
        </Actions>
      );
    case "failed":
      return <Actions>{state.failure === "robotPort" ? pickBoard : retry}{close}</Actions>;
  }
}

function SendCardPanel({ state, send, onPickBoard }: {
  state: ShownSendState;
  send: SendToBoard;
  onPickBoard: () => void;
}) {
  const { t } = useTranslation();
  const panel = useRef<HTMLDivElement>(null);
  const view = sendCardView(state);
  const building = isBuilding(state);
  const confirming = state.status === "uploading" && state.confirmingStop;
  const rule: FocusRule = confirming ? "question" : building ? "ifLost" : "result";
  useCardFocus(panel, rule, confirming ? "confirm" : state.status);
  // Esc closes a finished card; a running one stays until the build ends.
  const onKeyDown = (event: KeyboardEvent<HTMLDivElement>) => {
    if (event.key !== "Escape" || building) return;
    event.preventDefault();
    event.stopPropagation();
    send.close();
  };
  return (
    <div
      ref={panel}
      role={view.role}
      aria-label={t(view.titleKey)}
      onKeyDown={onKeyDown}
      className={[
        "pointer-events-auto box-border flex min-h-0 w-[420px] flex-col gap-[12px] overflow-y-auto",
        "rounded-[16px] border border-solid border-line bg-surface p-[16px] font-body text-ink",
        "shadow-[0_8px_20px_rgba(var(--ink-rgb),.2)]",
        "motion-safe:animate-[block-lab-pop-in_.2s_cubic-bezier(.2,.8,.2,1)]",
      ].join(" ")}
    >
      <div className="flex items-start gap-[14px]">
        {view.mascot === "search" && <ConnectionMascot width={72} />}
        {view.mascot === "robot" && (
          <img src={heroRobot} alt="" className="h-[96px] w-[68px] shrink-0 object-contain" />
        )}
        <div className="min-w-0 flex-1">
          <div className="flex items-center gap-[12px] text-[18px] leading-[1.5] font-bold">
            {view.mark === "spinner" && (
              <Icon
                name="loader"
                size={24}
                className="text-blue animate-[sbSpin_.8s_linear_infinite] motion-reduce:animate-none"
              />
            )}
            {view.mark === "tick" && <Icon name="check" size={24} className="text-ok" />}
            <span className="min-w-0">{t(view.titleKey)}</span>
          </div>
          <div className="mt-[4px] text-[16px] leading-[1.6]">{t(view.bodyKey)}</div>
        </div>
      </div>
      {view.tipKeys.length > 0 && <TipList tips={view.tipKeys.map(key => t(key))} />}
      {view.step && <StepBar step={view.step} />}
      {view.previewNote && <CardNote tone="info">{t("blockLab.send.sent.previewNote")}</CardNote>}
      {confirming && <CardNote tone="warn">{t("blockLab.send.uploading.confirmStop")}</CardNote>}
      {view.output && <ToolOutput text={view.output} />}
      {view.errorCode && (
        <div className="text-[14px] leading-[1.6] text-muted">
          {t("blockLab.send.errorCode", { errorCode: view.errorCode })}
        </div>
      )}
      <CardActions state={state} send={send} onPickBoard={onPickBoard} />
    </div>
  );
}

// The send cards of the design: floating on the right, 8px above the drawer, without a
// backdrop, so the blocks stay usable while a build runs. The live region stays mounted so each
// new card is announced.
export function SendCard({ send, onPickBoard }: { send: SendToBoard; onPickBoard: () => void }) {
  const { state } = send;
  return (
    <div
      aria-live="polite"
      className={[
        "pointer-events-none absolute right-[36px] bottom-[8px] z-[50] flex max-h-[calc(100%_-_76px)]",
        "flex-col",
      ].join(" ")}
    >
      {state.status !== "idle" && (
        <SendCardPanel key={state.status} state={state} send={send} onPickBoard={onPickBoard} />
      )}
    </div>
  );
}
