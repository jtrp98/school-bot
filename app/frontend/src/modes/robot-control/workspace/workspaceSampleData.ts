import { t as translate } from "../../../i18n";

export type SamplePose = { id: string; name: string; v: number[]; t: number; g?: string };
export type SampleWorkspace = { id: string; name: string; poses: SamplePose[] };

const SAMPLE_POSE_MS = 3000;

function pose(id: string, nameKey: string, v: number[], groupKey?: string): SamplePose {
  return { id, name: translate(nameKey), v, t: SAMPLE_POSE_MS, ...(groupKey ? { g: translate(groupKey) } : {}) };
}

// Other projects offered by the merge dialog in Sample mode. Names are translated when asked for, so they follow the
// current language.
export function sampleMergeWorkspaces(): SampleWorkspace[] {
  return [
    {
      id: "w2",
      name: translate("robot.workspace.messages.waveHello"),
      poses: [
        pose("m1", "robot.workspace.messages.raiseArm", [0, 20, -40, 0, 0, 50]),
        pose("m2", "robot.workspace.messages.waveLeft", [-30, 20, -40, 30, 0, 50], "robot.workspace.messages.wave"),
        pose("m3", "robot.workspace.messages.waveRight", [30, 20, -40, -30, 0, 50], "robot.workspace.messages.wave"),
        pose("m4", "robot.workspace.messages.greetingPose", [0, -10, 60, 40, 0, 0]),
        pose("m5", "common.messages.returnToRestPose", [0, 0, 0, 0, 0, 0]),
      ],
    },
    {
      id: "w3",
      name: translate("robot.workspace.messages.sortColoredBlocks"),
      poses: [
        pose("n1", "robot.workspace.messages.pickRed", [-40, -50, 60, 20, 0, 100], "robot.workspace.messages.pickUp"),
        pose("n2", "robot.workspace.messages.liftRed", [-40, 0, 30, 0, 0, 100], "robot.workspace.messages.pickUp"),
        pose("n3", "robot.workspace.messages.placeRed", [40, -50, 60, 20, 0, 0]),
      ],
    },
    {
      id: "w4",
      name: translate("robot.workspace.messages.passTheBall"),
      poses: [
        pose("q1", "robot.workspace.messages.receiveBall", [0, -60, 70, 10, 0, 100]),
        pose("q2", "robot.workspace.messages.swing", [60, 40, -20, -10, 0, 0]),
      ],
    },
  ];
}
