import type { WorkspaceLogic } from "./WorkspaceLogic";

export type CopiedCommandRow = {
  sid: string;
  name: string;
  id: string;
  p: string;
  t: string;
};

export type NativeWorkspaceView = {
  nativeHardware?: boolean;
  nativeConnected?: boolean;
  nativeLost?: boolean;
  nativeProfile?: string;
  robotPort?: string;
};

export type WorkspaceView = ReturnType<WorkspaceLogic["renderVals"]>;

export type WorkspaceViewProps<K extends keyof WorkspaceView = keyof WorkspaceView> = {
  v: Pick<WorkspaceView, K>;
};
