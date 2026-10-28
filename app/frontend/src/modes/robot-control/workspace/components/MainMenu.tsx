import { Button } from "../../../../components/shared/Button";
import { Icon } from "../../../../components/shared/Icons";
import { t as translate } from "../../../../i18n";
import type { ReactNode } from "react";
import { T } from "../tplUtil";
import type { WorkspaceViewProps } from "../viewTypes";
import type { NetworkState } from "../workspaceMenuFields";
import { MenuItem } from "./MenuItem";

type Props = WorkspaceViewProps<"topBar" | "menuActions" | "netTest">;

const NETWORK_CHIP_TONE: Record<NetworkState, { chip: string; dot: string }> = {
  ok: { chip: "border-ok bg-ok-tint text-ok", dot: "bg-ok" },
  slow: { chip: "border-star bg-orange-tint text-orange-deep", dot: "bg-star" },
  off: { chip: "border-err bg-err-tint text-err-ink", dot: "bg-err" },
  measuring: { chip: "border-line-strong bg-sunken text-muted", dot: "bg-line-strong" },
};

const DIVIDER_CLASS = "mx-0 my-[4px] h-[2px] bg-sunken";

export function networkChipClass(state: NetworkState): string {
  return [
    "box-border inline-flex shrink-0 items-center gap-[6px] rounded-full border-2 border-solid whitespace-nowrap",
    "font-[family-name:inherit] text-[12px] font-bold",
    NETWORK_CHIP_TONE[state].chip,
  ].join(" ");
}

export function networkDotClass(state: NetworkState): string {
  return `h-[8px] w-[8px] shrink-0 rounded-[50%] ${NETWORK_CHIP_TONE[state].dot}`;
}

export function MainMenu({ v }: Props): ReactNode {
  const { leaveDirectly, mergeDisabled, clearDisabled, network, languages } = v.topBar;
  const actions = v.menuActions;
  const measuring = network.state === "measuring";
  const newProjectItem = (
    <MenuItem
      icon="plus"
      shortcut="Ctrl+N"
      onClick={() => actions.leaveTo("new", !leaveDirectly)}
    >
      {translate("home.create.title")}
    </MenuItem>
  );
  return (
    <div
      role="menu"
      aria-label={translate("robot.workspace.messages.menu")}
      className={[
        "absolute top-[46px] right-0 z-[60] box-border flex w-[280px] flex-col gap-[2px] rounded-[14px]",
        "border-2 border-solid border-line-strong bg-white p-[8px] shadow-pop",
      ].join(" ")}
    >
      <MenuItem icon="home" onClick={() => actions.leaveTo("home", true)}>
        {translate("nav.label")}
      </MenuItem>
      <div className={DIVIDER_CLASS} />
      <MenuItem icon="folder" shortcut="Ctrl+O" onClick={() => actions.leaveTo("work", !leaveDirectly)}>
        {translate("robot.workspace.messages.myProjects")}
      </MenuItem>
      {leaveDirectly ? newProjectItem : null}
      <MenuItem icon="in" shortcut="Ctrl+A" onClick={actions.openMerge} disabled={mergeDisabled}>
        {translate("robot.workspace.messages.appendPosesFromAnotherProject")}
      </MenuItem>
      <MenuItem icon="save" shortcut="Ctrl+S" onClick={actions.save}>
        {translate("common.save")}
      </MenuItem>
      <MenuItem icon="file" shortcut="Ctrl+Shift+S" onClick={actions.saveAs}>
        {translate("robot.workspace.messages.saveAs")}
      </MenuItem>
      {leaveDirectly ? null : newProjectItem}
      <MenuItem icon="clock" shortcut="Ctrl+H" onClick={actions.openActivityLog}>
        {translate("robot.workspace.messages.activityLog")}
      </MenuItem>
      <MenuItem
        tone="danger"
        detail={translate("robot.workspace.messages.deleteAllPoses")}
        onClick={actions.askClear}
        disabled={clearDisabled}
      >
        <span aria-hidden="true" className="mr-[.3em] inline-flex w-[1.15em] shrink-0 justify-center">
          ↺
        </span>
        {translate("common.messages.startOver")}
      </MenuItem>
      <div className={DIVIDER_CLASS} />
      <div className="flex items-center gap-[8px] px-[10px] py-[4px]">
        <span className="flex grow items-center gap-[10px] text-[15px] font-semibold">
          <Icon name="globe" className="mr-[.3em] inline-block size-[1.15em] align-[-.2em]" />
          {translate("language.label")}
        </span>
        {languages.map((language) => (
          <Button
            key={language.value}
            variant={language.selected ? "blue" : "secondary"}
            size="sm"
            aria-pressed={language.selected}
            onClick={() => actions.setLanguage(language.value)}
            className="h-[30px]! min-h-0! rounded-[8px]! px-[10px]! text-[13px]! border-b-2!"
          >
            {T(language.label)}
          </Button>
        ))}
      </div>
      <div className={DIVIDER_CLASS} />
      <MenuItem
        icon="lock"
        detail={translate("common.messages.pinRequired")}
        onClick={() => actions.leaveTo("teacher", !leaveDirectly)}
      >
        {translate("robot.teacher.title")}
      </MenuItem>
      <button
        type="button"
        role="menuitem"
        onClick={v.netTest}
        aria-busy={measuring}
        className={[
          "box-border flex min-h-[40px] w-full cursor-pointer flex-col gap-[2px] rounded-[9px] border-none px-[10px] py-[6px]",
          "text-left font-[family-name:inherit] text-[15px] font-semibold text-ink hover:bg-blue-tint!",
          measuring ? "bg-blue-tint" : "bg-surface",
        ].join(" ")}
      >
        <span className="flex w-full items-center gap-[10px]">
          <Icon name="wifi" className="mr-[.3em] inline-block size-[1.15em] align-[-.2em]" />{" "}
          {translate("robot.workspace.messages.checkConnection")}
          <span
            data-net="menu"
            role="status"
            aria-label={network.chipLabel}
            onClick={actions.explainNetworkFromMenu}
            className={[
              networkChipClass(network.state),
              "ml-auto h-[26px]",
              measuring ? "w-[40px] justify-center p-0" : "px-[10px]",
            ].join(" ")}
          >
            {measuring ? (
              <span
                aria-hidden="true"
                className={[
                  "box-border h-[10px] w-[10px] rounded-[50%] border-2 border-solid border-line-strong border-t-blue-ink",
                  "animate-[awNetSpin_.8s_linear_infinite] motion-reduce:animate-none",
                ].join(" ")}
              />
            ) : (
              <span aria-hidden="true" className={networkDotClass(network.state)} />
            )}
            {T(network.chipText)}
          </span>
        </span>
        <span className="pl-[30px] text-[12px] font-medium text-muted">{T(network.detail)}</span>
      </button>
      <MenuItem
        size="md"
        tone="muted"
        detail="schoolbot.app"
        onClick={() => actions.openExternal("https://www.schoolbot.app")}
      >
        {translate("robot.workspace.messages.aboutSchoolBot")}
      </MenuItem>
    </div>
  );
}
