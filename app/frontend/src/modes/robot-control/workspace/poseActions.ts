import type { WorkspaceLogic } from "./WorkspaceLogic";
import { createPoseDragActions } from "./poseDragActions";
import { createPoseGroupActions } from "./poseGroupActions";
import { createPoseListActions } from "./poseListActions";
import { createPoseRowActions } from "./poseRowActions";

export function createPoseActions(screen: WorkspaceLogic) {
  return {
    ...createPoseListActions(screen),
    ...createPoseGroupActions(screen),
    ...createPoseDragActions(screen),
    ...createPoseRowActions(screen),
  };
}

export type PoseActions = ReturnType<typeof createPoseActions>;
