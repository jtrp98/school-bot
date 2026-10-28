import type { IconName } from "../../../components/shared/Icons";
import { t as translate } from "../../../i18n";
import type { WorkspaceFieldsContext } from "./workspaceFields";

export type SequenceMode = "all" | "range" | "order";

type PoseGroup = { id: string; name: string; keys: string[]; open: boolean };

// What the group button would act on; passed back to the action so it works on the same selection the user saw.
export type GroupSelection = {
  indices: number[];
  contiguous: boolean;
  extendedGroup: PoseGroup | null;
  singlesCount: number;
  ok: boolean;
};

export type SequenceToolbarView = {
  normalMode: boolean;
  viewMode: "cards" | "table";
  tableViewDisabled: boolean;
  modeOptions: { value: SequenceMode; label: string; selected: boolean; disabled: boolean }[] | null;
  clipboard: { icon: IconName | undefined; text: string; doneLabel: string } | null;
  rangeChip: { text: string; cancelLabel: string; canSaveAsGroup: boolean } | null;
  manageControl: { disabled: boolean; pressed: boolean; tip: string } | null;
  tableToggle: { visible: boolean; pressed: boolean; label: string; iconPath: string; text: string };
  manageBar: {
    text: string;
    emphasized: boolean;
    naming: boolean;
    draft: string;
    makeDisabled: boolean;
    makeTip: string;
    makeLabel: string;
  } | null;
  releaseBar: boolean;
  groupSelection: GroupSelection;
};

const EXPAND_ICON_PATH = "M4 9V4h5M20 9V4h-5M4 15v5h5M20 15v5h-5";
const COLLAPSE_ICON_PATH = "M9 4v5H4M15 4v5h5M9 20v-5H4M15 20v-5h5";

export function buildToolbarFields(context: WorkspaceFieldsContext) {
  const { screen, S, n, seq, lockOff, isRange, isOrder, wrapOrd, grpOf } = context;
  const { pickIdx, pickContig, pickGs, pickSingles, pickExt, extG, pickOk, grpDup } = context;
  const tableFull: boolean = S.t6full;
  const naming: boolean = S.grpNaming;
  const draft: string = S.grpDraft;
  const pickedGroups: PoseGroup[] = pickGs;
  const scatteredSelection = pickIdx.length >= 2 && !pickContig;
  const makeDisabled = !pickOk || (naming && (!draft.trim() || grpDup));

  const groupSelection: GroupSelection = {
    indices: pickIdx,
    contiguous: pickContig,
    extendedGroup: pickExt ? extG : null,
    singlesCount: pickSingles,
    ok: pickOk,
  };

  const manageText = (): string => {
    if (naming && scatteredSelection) {
      const moving = pickIdx
        .filter((index: number, order: number) => index !== pickIdx[0] + order)
        .map((index: number) => index + 1);
      return translate("robot.workspace.messages.poseValue0WillMoveAfterPoseValue1", {
        value0: moving.join(", "),
        value1: pickIdx[0] + 1,
      });
    }
    if (pickExt && extG) {
      const key = pickContig ? "robot.workspace.toolbar.addToGroup" : "robot.workspace.toolbar.addToGroupMoving";
      return translate(key, { count: pickSingles, name: extG.name });
    }
    if (pickedGroups.length === 1 && !pickSingles) {
      return translate("robot.workspace.toolbar.groupSelectedAddSingles", { name: pickedGroups[0].name });
    }
    if (pickedGroups.length >= 2) return translate("robot.workspace.groups.oneGroupOnly");
    return translate("robot.workspace.groups.moveOrSelect");
  };

  const makeTip = (): string => {
    if (grpDup) return translate("robot.workspace.messages.aGroupWithThisNameAlreadyExists");
    if (!pickOk) return translate("robot.workspace.groups.chooseTwoFirst");
    return scatteredSelection ? translate("robot.workspace.toolbar.moveAfterPose", { number: pickIdx[0] + 1 }) : "";
  };

  const rangeChipText = (): string => {
    if (S.rangeDone) {
      const count = wrapOrd(S.rs, S.re).length;
      return S.re !== S.rs
        ? translate("robot.workspace.toolbar.rangeChosen", { from: S.rs + 1, to: S.re + 1, count })
        : translate("robot.workspace.toolbar.rangeChosenSingle", { from: S.rs + 1 });
    }
    return S.picking
      ? translate("robot.workspace.toolbar.rangePickEnd", { from: S.rs + 1 })
      : translate("robot.workspace.playback.tapStart");
  };

  const canSaveRange = (): boolean => {
    if (!(isRange && S.rangeDone) || S.re <= S.rs) return false;
    for (let index = S.rs; index <= S.re; index++) if (grpOf(seq[index])) return false;
    return true;
  };

  const clipboard: { keys?: string[]; poses: unknown[] } | null = S.pClip;
  const showClipboard = !S.selMode && !isRange && !isOrder && !!clipboard && !!S.pMode;
  const showRange = !!screen.props.showRange;
  const modeLabels: Record<SequenceMode, string> = {
    all: translate("robot.workspace.messages.all"),
    range: translate("robot.workspace.messages.selectRange"),
    order: translate("robot.workspace.messages.reorder"),
  };
  const modeValues: SequenceMode[] = ["all", "range", "order"];

  const toolbar: SequenceToolbarView = {
    normalMode: !S.selMode && !(S.release && S.view === "table" && tableFull),
    viewMode: S.view,
    tableViewDisabled: S.selMode || isRange,
    modeOptions: showRange
      ? modeValues.map((value) => ({
        value,
        label: modeLabels[value],
        selected: S.mode === value,
        disabled: lockOff,
      }))
      : null,
    clipboard: showClipboard && clipboard
      ? {
        icon: clipboard.keys ? "cut" : "paste",
        text: translate(
          clipboard.keys ? "robot.workspace.toolbar.clipboardMove" : "robot.workspace.toolbar.clipboardPaste",
          { count: clipboard.poses.length },
        ),
        doneLabel: clipboard.keys
          ? translate("robot.workspace.messages.cancelCut")
          : translate("robot.workspace.messages.doneLabel"),
      }
      : null,
    rangeChip: isRange
      ? {
        text: rangeChipText(),
        cancelLabel: translate("robot.workspace.messages.rangeSelectionCanceledPlayAll"),
        canSaveAsGroup: canSaveRange(),
      }
      : null,
    manageControl: showRange
      ? null
      : {
        disabled: lockOff || n < 2 || isRange,
        pressed: isOrder,
        tip: n < 2
          ? translate("robot.workspace.messages.fewerThan2PosesNoReorderingNeeded")
          : translate(isOrder ? "robot.workspace.sequence.normalMode" : "robot.workspace.sequence.moveInstructions"),
      },
    tableToggle: {
      visible: S.view === "table",
      pressed: tableFull,
      label: tableFull
        ? translate("robot.workspace.messages.shrinkTableAndShowJointPanel")
        : translate("robot.workspace.messages.expandTableHideJointPanel"),
      iconPath: tableFull ? COLLAPSE_ICON_PATH : EXPAND_ICON_PATH,
      text: tableFull
        ? translate("robot.workspace.messages.collapseLabel")
        : translate("robot.workspace.messages.expand"),
    },
    manageBar: S.selMode
      ? {
        text: manageText(),
        emphasized: (naming && scatteredSelection) || pickExt || pickedGroups.length > 0,
        naming,
        draft,
        makeDisabled,
        makeTip: makeTip(),
        makeLabel: naming
          ? translate("robot.workspace.messages.confirm")
          : translate(pickExt ? "robot.workspace.groups.addToGroup" : "robot.workspace.messages.groupPosesLabel"),
      }
      : null,
    releaseBar: !!S.release && S.view === "table" && tableFull && !S.selMode,
    groupSelection,
  };

  return { toolbar, toolbarActions: screen.toolbarActions };
}
