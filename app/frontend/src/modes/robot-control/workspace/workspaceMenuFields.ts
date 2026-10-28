import i18n, { languages, t as translate, type Language } from "../../../i18n";
import type { IconName } from "../../../components/shared/Icons";
import type { WorkspaceFieldsContext } from "./workspaceFields";

export type NetworkState = "ok" | "slow" | "off" | "measuring";

export type TopBarView = {
  project: { renaming: boolean; draft: string; name: string; saved: boolean };
  network: {
    state: NetworkState;
    chipText: string;
    chipLabel: string;
    detail: string;
    showHeaderChip: boolean;
  };
  menuOpen: boolean;
  leaveDirectly: boolean;
  mergeDisabled: boolean;
  clearDisabled: boolean;
  languages: { value: Language; label: string; selected: boolean }[];
};

type MenuPosition = { left: number; top: number };

export type ContextMenusView = {
  pose0Menu: MenuPosition | null;
  groupMenu: (MenuPosition & { title: string; icon: IconName; viewLabel: string; disabled: boolean }) | null;
  poseMenu: (MenuPosition & { title: string; disabled: boolean; pasteDisabled: boolean }) | null;
  mainMenuBackdrop: boolean;
  groupInlineMenuBackdrop: boolean;
  jointMenuBackdrop: boolean;
};

type NetworkResult = { mbps: number | null; ping: number | null } | null;

const GROUP_MENU_MARGIN = { right: 250, bottom: 260 };
const POSE_MENU_MARGIN = { right: 250, bottom: 270 };
const POSE0_MENU = { width: 260, right: 270, bottom: 150 };

function networkDetail(state: NetworkState, netState: string, result: NetworkResult): string {
  if (state === "measuring") return translate("robot.workspace.messages.measuringSpeed");
  if (netState === "off") return translate("robot.workspace.network.robotWorks");
  if (!result || result.mbps == null) return translate("robot.workspace.network.testSpeed");
  const key = netState === "slow" ? "robot.workspace.network.measuredDetailSlow" : "robot.workspace.network.measuredDetail";
  return translate(key, { mbps: result.mbps, ping: result.ping });
}

function networkChipText(netState: string): string {
  if (netState === "slow") return translate("robot.workspace.messages.slowInternet");
  if (netState === "off") return translate("robot.workspace.messages.noInternet");
  return translate("robot.workspace.messages.internetConnected");
}

export function buildMenuFields(context: WorkspaceFieldsContext) {
  const { screen, S, seq, running, isRange, busy, noSeq, busyCtx, leaveBusyNow, leaveDirty } = context;
  const measuring: boolean = S.netMeas;
  const state: NetworkState = measuring ? "measuring" : S.netSt;
  const chipText = measuring ? "" : networkChipText(S.netSt);
  const groupMenuState: { gid: string; x: number; y: number } | null = S.ctx;
  const poseMenuState: { i: number; x: number; y: number } | null = S.pctx;
  const pose0MenuState: { x: number; y: number } | null = S.p0m6;
  const group = groupMenuState
    ? (S.groups as { id: string; name: string; open: boolean; keys: string[] }[]).find((g) => g.id === groupMenuState.gid)
    : undefined;
  const menuPoseKey = poseMenuState ? seq[poseMenuState.i] : undefined;
  const rootWidth = screen.rootW();
  const rootHeight = screen.rootH();

  const topBar: TopBarView = {
    project: {
      renaming: !!S.projRenaming,
      draft: S.projDraft ?? "",
      name: S.projName || translate("robot.workspace.messages.untitledProject"),
      saved: !!S.projName,
    },
    network: {
      state,
      chipText,
      chipLabel: measuring ? translate("robot.workspace.messages.measuringSpeed") : chipText,
      detail: networkDetail(state, S.netSt, S.netRes),
      showHeaderChip: !measuring && S.netSt !== "ok",
    },
    menuOpen: !!S.menuOpen,
    leaveDirectly: !leaveBusyNow && !leaveDirty,
    mergeDisabled: running || S.selMode || isRange,
    clearDisabled: noSeq || busy || isRange,
    languages: languages.map((language) => ({
      value: language.value,
      label: language.label,
      selected: i18n.resolvedLanguage === language.value,
    })),
  };

  const contextMenus: ContextMenusView = {
    pose0Menu: pose0MenuState
      ? {
        left: Math.max(8, Math.min(pose0MenuState.x - POSE0_MENU.width, rootWidth - POSE0_MENU.right)),
        top: Math.min(pose0MenuState.y, rootHeight - POSE0_MENU.bottom),
      }
      : null,
    groupMenu: groupMenuState
      ? {
        left: Math.min(groupMenuState.x, rootWidth - GROUP_MENU_MARGIN.right),
        top: Math.min(groupMenuState.y, rootHeight - GROUP_MENU_MARGIN.bottom),
        title: group?.name ?? "",
        icon: "box",
        viewLabel: group?.open
          ? translate("robot.workspace.messages.hidePosesInGroup")
          : translate("robot.workspace.messages.viewGroupPosesValue0Poses", { value0: group ? group.keys.length : 0 }),
        disabled: busyCtx,
      }
      : null,
    poseMenu: poseMenuState && menuPoseKey
      ? {
        left: Math.min(poseMenuState.x, rootWidth - POSE_MENU_MARGIN.right),
        top: Math.min(poseMenuState.y, rootHeight - POSE_MENU_MARGIN.bottom),
        title: S.poses[menuPoseKey]
          ? translate("robot.workspace.messages.poseValue0Value1", {
            value0: poseMenuState.i + 1,
            value1: S.poses[menuPoseKey].name,
          })
          : "",
        disabled: busyCtx,
        pasteDisabled: busyCtx || !(S.gClip || S.pClip),
      }
      : null,
    mainMenuBackdrop: !!S.menuOpen,
    groupInlineMenuBackdrop: !!S.gMenu,
    jointMenuBackdrop: S.jMenu != null,
  };

  return { topBar, contextMenus, menuActions: screen.menuActions };
}
