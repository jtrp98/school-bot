import type { ChangeEvent, KeyboardEvent, MouseEvent } from "react";
import { t as translate } from "../../../i18n";
import type { WorkspaceLogic } from "./WorkspaceLogic";
import { deriveListFlags, groupIndexes, type PoseGroup, type PoseListState } from "./poseListState";

const UNGROUP_UNDO_TOAST_MS = 5000;
const MENU_BUTTON_OFFSET_X = 240;
const MENU_BUTTON_GAP_Y = 4;

// Handlers read the screen state when they run, so one instance serves every render without stale closures.
export function createPoseGroupActions(screen: WorkspaceLogic) {
  const state = (): PoseListState => screen.state;
  const set = (patch: Record<string, unknown>) => screen.setState(patch);
  const groupById = (groupId: string): PoseGroup | undefined => state().groups.find((group) => group.id === groupId);
  const lastIndexOf = (group: PoseGroup) => {
    const indexes = groupIndexes(state(), group);
    return indexes[indexes.length - 1];
  };

  const pickRange = (group: PoseGroup) => {
    const indexes = groupIndexes(state(), group);
    const last = indexes[indexes.length - 1];
    const finish = { picking: false, rangeDone: true, hoverIdx: -1 };
    if (state().picking) set({ re: last, ...finish });
    else set({ rs: indexes[0], re: last, ...finish });
  };

  const ungroup = (group: PoseGroup, extra: Record<string, unknown>) => {
    set({ groups: state().groups.filter((candidate) => candidate !== group), ...extra });
    screen.showToast(
      translate("robot.workspace.messages.ungroupedValue0IntoIndividualPoses", { value0: group.name }),
      () => screen.setState({ groups: screen.state.groups.concat([group]), toast: null }),
      translate("common.messages.undo"),
      null,
      "",
      UNGROUP_UNDO_TOAST_MS,
    );
  };

  const openGroupMenu = (groupId: string, event: MouseEvent<HTMLElement>) => {
    const group = groupById(groupId);
    if (!group) return;
    event.preventDefault();
    event.stopPropagation();
    let point = screen.dom.pointInRoot(event.clientX, event.clientY);
    if (event.type === "click") {
      const button = event.currentTarget.getBoundingClientRect();
      const origin = screen.dom.pointInRoot(button.right, button.bottom);
      point = { x: origin.x - MENU_BUTTON_OFFSET_X, y: origin.y + MENU_BUTTON_GAP_Y };
    }
    set({ ctx: { gid: group.id, x: point.x, y: point.y }, gFocus: group.id, gMenu: null });
  };

  return {
    // A group card focuses the group; a group row folds it. Both pick the group in the selection and range modes.
    tapGroupCard: (groupId: string) => {
      const group = groupById(groupId);
      if (!group) return;
      const current = state();
      const { isRange, isOrder, running } = deriveListFlags(current);
      if (current.selMode) {
        screen.togglePickGroup(group);
        return;
      }
      if (!isRange && !running && !isOrder) {
        set({ gFocus: current.gFocus === group.id ? null : group.id });
        return;
      }
      if (isRange) pickRange(group);
    },
    tapGroupRow: (groupId: string, event: MouseEvent<HTMLElement>) => {
      const group = groupById(groupId);
      const target = event.target as HTMLElement;
      const onControl = !!target.closest("input,button,textarea,select,a,[role=button]");
      if (!group || onControl) return;
      const current = state();
      const { isRange, isOrder, running } = deriveListFlags(current);
      if (current.selMode) {
        screen.togglePickGroup(group);
        return;
      }
      if (!isRange && !running && !isOrder) {
        screen.toggleGroup(group.id);
        return;
      }
      if (isRange) pickRange(group);
    },
    hoverGroup: (groupId: string) => {
      const group = groupById(groupId);
      if (!group) return;
      const current = state();
      const last = lastIndexOf(group);
      if (deriveListFlags(current).isRange && current.picking && current.hoverIdx !== last) set({ hoverIdx: last });
    },
    foldGroup: (groupId: string) => screen.toggleGroup(groupId),
    toggleGroupFromCard: (groupId: string, event: MouseEvent<HTMLElement>) => {
      event.stopPropagation();
      screen.toggleGroup(groupId);
    },
    playGroup: (groupId: string, event?: MouseEvent<HTMLElement>) => {
      event?.stopPropagation();
      const group = groupById(groupId);
      if (!group) return;
      const indexes = groupIndexes(state(), group);
      screen.playGroup(indexes[0], indexes[indexes.length - 1], group.name);
    },
    copyGroup: (groupId: string, event: MouseEvent<HTMLElement>) => {
      event.stopPropagation();
      screen.gCopy(groupById(groupId));
    },
    deleteGroup: (groupId: string, event: MouseEvent<HTMLElement>) => {
      event.stopPropagation();
      screen.gDelete(groupById(groupId));
    },
    pasteAfterGroup: (groupId: string, event: MouseEvent<HTMLElement>) => {
      event.stopPropagation();
      const group = groupById(groupId);
      if (group) screen.pasteClip(lastIndexOf(group) + 1);
    },
    ungroupFromCard: (groupId: string, event: MouseEvent<HTMLElement>) => {
      event.stopPropagation();
      const group = groupById(groupId);
      if (group) ungroup(group, { gMenu: null, gRen: null });
    },
    openGroupMenu,
    toggleGroupMenu: (groupId: string, event: MouseEvent<HTMLElement>) => {
      if (state().ctx) {
        set({ ctx: null });
        return;
      }
      openGroupMenu(groupId, event);
    },
    closeGroupCardMenu: (event: MouseEvent<HTMLElement>) => {
      event.stopPropagation();
      set({ gMenu: null, gRen: null });
    },
    startGroupRename: (groupId: string, event: MouseEvent<HTMLElement>) => {
      event.stopPropagation();
      const group = groupById(groupId);
      if (group) set({ gRen: group.id, gRenDraft: group.name });
    },
    startGroupRowRename: (groupId: string, event: MouseEvent<HTMLElement>) => {
      event.stopPropagation();
      const group = groupById(groupId);
      const current = state();
      if (group && !current.selMode && !deriveListFlags(current).running) {
        set({ gRen: group.id, gRenDraft: group.name, gMenu: null });
      }
    },
    editGroupNameDraft: (event: ChangeEvent<HTMLInputElement>) => set({ gRenDraft: event.target.value }),
    commitGroupName: (groupId: string, event?: MouseEvent<HTMLElement>) => {
      event?.stopPropagation();
      screen.commitGroupName(groupId);
    },
    keyOnGroupName: (groupId: string, event: KeyboardEvent<HTMLElement>) => {
      if (event.key === "Enter") {
        screen.commitGroupName(groupId);
      } else if (event.key === "Escape") {
        event.stopPropagation();
        set({ gRen: null });
      }
    },
  };
}

export type PoseGroupActions = ReturnType<typeof createPoseGroupActions>;
