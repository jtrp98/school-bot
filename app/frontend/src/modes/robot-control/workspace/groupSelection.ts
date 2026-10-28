import { t as translate } from "../../../i18n";
import { groupOfKey, type PoseGroup, type PoseListState } from "./poseListState";

export type GroupSelectionState = PoseListState & { grpDraft: string };

// What the "make a group" toolbar can do with the poses picked in selection mode.
export function deriveGroupSelection(state: GroupSelectionState) {
  const { seq, picks, groups } = state;
  const pickIdx = picks.map((key) => seq.indexOf(key)).filter((index) => index >= 0).sort((a, b) => a - b);
  const pickContig = pickIdx.length >= 2
    && pickIdx.every((index, position) => position === 0 || index === pickIdx[position - 1] + 1);
  const pickedGroups = new Set<PoseGroup>();
  for (const key of picks) {
    const group = groupOfKey(state, key);
    if (group) pickedGroups.add(group);
  }
  const pickGs = [...pickedGroups];
  const pickSingles = picks.filter((key) => !groupOfKey(state, key)).length;
  const pickPartial = pickGs.some((group) => !group.keys.every((key) => picks.includes(key)));
  const pickExt = pickIdx.length >= 2 && pickGs.length === 1 && !pickPartial && pickSingles > 0;
  const pickNew = pickIdx.length >= 2 && pickGs.length === 0;
  const moveNote = translate("robot.workspace.messages.followingPosesWillMoveNextToPoseValue0", {
    value0: pickIdx[0] + 1,
  });
  return {
    pickIdx,
    pickContig,
    pickGs,
    pickSingles,
    pickPartial,
    pickExt,
    extG: pickExt ? pickGs[0] : null,
    pickMoveNote: pickIdx.length >= 2 && !pickContig ? ` ${moveNote}` : "",
    pickOk: pickNew || pickExt,
    grpDup: !!state.grpNaming && groups.some((group) => group.name === (state.grpDraft || "").trim()),
  };
}
