import { useTranslation } from "react-i18next";
import { Icon, type IconName } from "../../../components/shared/Icons";
import { preview } from "../../../core/client";
import { t as translate } from "../../../i18n";
import { linkName, lostBodyKey } from "../connect/boardLabel";
import type { BoardNames } from "../connect/boardNames";
import type { BoardLink } from "../connect/boardDevices";
import { BarPopover } from "./BarPopover";
import { usePopover } from "./usePopover";

type ChipState = "none" | "connected" | "lost" | "unknown";
type ChipText = { chip: string; label: string; title: string; body: string; meta: string };

const ICONS: Record<ChipState, { name: IconName; tone: string }> = {
  none: { name: "unplug", tone: "text-muted" },
  connected: { name: "plug", tone: "text-ok" },
  lost: { name: "unplug", tone: "text-err" },
  unknown: { name: "warn", tone: "text-[var(--warn)]" },
};

function chipText(state: ChipState, link: BoardLink | null, names: BoardNames): ChipText {
  if (!link) {
    return {
      chip: translate("blockLab.board.notConnected"),
      label: translate("blockLab.board.notConnected"),
      title: translate("blockLab.board.notConnected"),
      body: translate("blockLab.board.notConnectedHint"),
      meta: translate("blockLab.board.usbOnly"),
    };
  }
  const values = { boardName: linkName(link, names), port: link.port, chipName: link.chip };
  const meta = translate("blockLab.board.linkMeta", values);
  if (state === "lost") {
    return {
      chip: translate("blockLab.board.lost"),
      label: translate("blockLab.board.label.lost", values),
      title: translate("blockLab.board.lostTitle"),
      body: translate(lostBodyKey(link.followsAcrossPorts)),
      meta,
    };
  }
  if (state === "unknown") {
    return {
      chip: translate("blockLab.board.unknown"),
      label: translate("blockLab.board.label.unknown", values),
      title: translate("blockLab.board.unknownTitle"),
      body: translate("blockLab.board.unknownBody"),
      meta,
    };
  }
  const uno = link.kind === "uno";
  // An unnamed clone is already called by its chip, so the chip is added only after a name.
  const chipAfterName = link.chip && names[link.key];
  return {
    chip: translate(chipAfterName ? "blockLab.board.connectedChipWithChip" : "blockLab.board.connectedChip", values),
    label: translate(uno ? "blockLab.board.label.connectedUno" : "blockLab.board.label.connectedSerial", values),
    title: translate(uno ? "blockLab.board.connectedTitle" : "blockLab.board.connectedTitleSerial"),
    body: translate("blockLab.board.connectedBody"),
    meta: translate(link.chip ? "blockLab.board.connectedMetaWithChip" : "blockLab.board.connectedMeta", values),
  };
}

// The board chip has no frame (as Arm Lab's device chip) and opens a short explanation. A board
// whose list cannot be read is shown as unknown rather than still connected.
export function BoardChip({ link, names, unchecked }: {
  link: BoardLink | null;
  names: BoardNames;
  unchecked: boolean;
}) {
  const { t } = useTranslation();
  const popover = usePopover();
  const state: ChipState = !link ? "none" : link.lost ? "lost" : unchecked ? "unknown" : "connected";
  const text = chipText(state, link, names);
  return (
    <div ref={popover.root} className="relative flex shrink-0 items-center gap-[4px]">
      <button
        ref={popover.trigger}
        type="button"
        aria-haspopup="dialog"
        aria-expanded={popover.open}
        aria-label={text.label}
        onClick={popover.toggle}
        className="group flex h-[44px] cursor-pointer items-center border-0 bg-transparent p-0 text-ink"
      >
        <span
          className={[
            "flex h-[32px] items-center gap-[8px] rounded-full px-[12px] font-body text-[16px]",
            "font-semibold leading-[1.7] whitespace-nowrap group-hover:bg-blue-tint group-focus-visible:bg-blue-tint",
            "group-aria-expanded:bg-blue-tint",
            link ? "bg-surface" : "bg-sunken",
          ].join(" ")}
        >
          <Icon name={ICONS[state].name} size={20} className={ICONS[state].tone} />
          <span role="status">{text.chip}</span>
          <Icon name="chevD" size={16} className="text-muted" />
        </span>
      </button>
      {/* Block Lab runs full screen, which hides the app's preview badge, so the made-up
          ports say here that they are simulated. */}
      {preview && (
        <span
          className={[
            "rounded-full border border-dashed border-muted px-[8px] font-body text-[12px] leading-[1.6]",
            "font-semibold whitespace-nowrap text-muted",
          ].join(" ")}
        >
          {t("blockLab.board.simulated")}
        </span>
      )}
      {popover.open && (
        <BarPopover align="right" role="dialog" label={text.title} className="w-[330px] gap-[8px] p-[16px]">
          <span className="font-body text-[18px] leading-[1.6] font-bold">{text.title}</span>
          <span className="font-body text-[16px] leading-[1.6]">{text.body}</span>
          <span className="font-body text-[14px] leading-[1.6] text-muted">{text.meta}</span>
          <span className="font-body text-[14px] leading-[1.6] text-muted">{t("blockLab.board.tapToClose")}</span>
        </BarPopover>
      )}
    </div>
  );
}
