import { t as translate } from "../../../i18n";

export function poseRangeText(first: number, count: number): string {
  if (count > 1) return translate("robot.workspace.pose.rangeMulti", { from: first, to: first + count - 1 });
  return translate("robot.workspace.pose.rangeSingle", { from: first });
}

export function playStartLogText(
  scope: { from: number; to: number } | null,
  count: number,
  loops: number | null,
): string {
  if (scope) {
    const values = { from: scope.from, to: scope.to, loops };
    return translate(loops === null ? "robot.workspace.log.playRangeForever" : "robot.workspace.log.playRangeRounds", values);
  }
  return translate(loops === null ? "robot.workspace.log.playAllForever" : "robot.workspace.log.playAllRounds", { count, loops });
}

export function memoryPlayLogText(count: number, at: string, loops: number | null): string {
  const values = { count, at, loops };
  if (loops === null) return translate("robot.workspace.log.memoryPlayForever", values);
  if (loops > 1) return translate("robot.workspace.log.memoryPlayRounds", values);
  return translate("robot.workspace.log.memoryPlay", values);
}
