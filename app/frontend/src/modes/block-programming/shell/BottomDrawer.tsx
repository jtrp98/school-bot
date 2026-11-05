import { useId } from "react";
import { useTranslation } from "react-i18next";
import { Button } from "../../../components/shared/Button";
import { BoardTerminal } from "./BoardTerminal";
import { outline } from "./categoryIcons";
import { CodePanel } from "./CodePanel";
import { drawerPanelId, drawerTabId, DrawerTabs, type DrawerTab } from "./DrawerTabs";
import { OutputPeek } from "./OutputPeek";
import { ToolButton } from "./ToolButton";
import type { BoardSerial } from "./useBoardSerial";
import type { GeneratedCode } from "./useGeneratedCode";
import { useOutputUnread } from "./useOutputUnread";

const CollapseIcon = outline("M6 9l6 6 6-6");
const ExpandIcon = outline("M6 15l6-6 6 6");

// The drawer under the workspace: Output and C++ tabs, "send to board" and collapse. Both
// panels stay mounted while hidden so the terminal keeps its state when the child switches tabs.
// "Send to board" starts the send (useSendToBoard); while the check or a build runs it shows that
// it is busy, and during a build it is also marked disabled.
export function BottomDrawer({
  boardConnected,
  serial,
  open,
  tab,
  code,
  checking,
  sending,
  onToggle,
  onTab,
  onSend,
}: {
  boardConnected: boolean;
  serial: BoardSerial;
  open: boolean;
  tab: DrawerTab;
  code: GeneratedCode;
  checking: boolean;
  sending: boolean;
  onToggle: () => void;
  onTab: (next: DrawerTab) => void;
  onSend: () => void;
}) {
  const { t } = useTranslation();
  const base = useId();
  const toggleLabel = t(open ? "blockLab.drawer.collapse" : "blockLab.drawer.expand");
  const unread = useOutputUnread(serial.log, open && tab === "output");
  const pickTab = (next: DrawerTab) => {
    onTab(next);
    if (!open) onToggle();
  };
  const panelClass = "min-h-0 flex-1 border-0 border-t border-solid border-t-line";
  return (
    <section
      aria-label={t("blockLab.drawer.label")}
      className={[
        "flex shrink-0 flex-col rounded-[16px] border-2 border-solid border-line bg-surface",
        open ? "h-[284px]" : "h-[60px]",
      ].join(" ")}
    >
      <div className="box-border flex h-[56px] shrink-0 items-center gap-[8px] px-[10px]">
        <DrawerTabs base={base} tab={tab} unread={unread} onTab={pickTab} />
        {/* On a narrow window the latest line gives way to the buttons, as in the design. */}
        <div className="flex min-w-0 grow justify-end px-[8px] max-[999px]:invisible">
          {!open && tab === "output" && <OutputPeek serial={serial} />}
        </div>
        <Button
          variant="blue"
          size="md"
          icon="upload"
          loading={checking || sending}
          aria-disabled={sending || undefined}
          data-block-lab-send=""
          onClick={onSend}
        >
          {t("blockLab.drawer.send")}
        </Button>
        <ToolButton
          label={toggleLabel}
          tip={toggleLabel}
          tipSide="above-right"
          aria-expanded={open}
          onClick={onToggle}
        >
          {open ? <CollapseIcon size={24} aria-hidden="true" /> : <ExpandIcon size={24} aria-hidden="true" />}
        </ToolButton>
      </div>
      <div
        role="tabpanel"
        id={drawerPanelId(base, "output")}
        aria-labelledby={drawerTabId(base, "output")}
        hidden={!open || tab !== "output"}
        className={`${panelClass} overflow-hidden rounded-b-[14px]`}
      >
        <BoardTerminal boardConnected={boardConnected} serial={serial} />
      </div>
      <div
        role="tabpanel"
        id={drawerPanelId(base, "cpp")}
        aria-labelledby={drawerTabId(base, "cpp")}
        tabIndex={0}
        hidden={!open || tab !== "cpp"}
        className={`${panelClass} overflow-auto rounded-b-[14px] select-text`}
      >
        <CodePanel code={code} />
      </div>
    </section>
  );
}
