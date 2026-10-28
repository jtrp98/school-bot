import type { ModeID } from "../core/types";

export type WorkKind = "arm" | "block";

type WorkPreview = {
  id: string;
  name: string;
  bg: string;
};

export type HomeWork = WorkPreview & {
  mode: ModeID;
  kind: WorkKind;
  meta: string;
  saved?: string;
  a?: number[];
};

export type RobotWork = WorkPreview & {
  robot: string;
  poses: number;
  groups: number;
  saved: string;
  // USB provenance is already included in saved, so the UI omits the usual saved prefix.
  fromUsb?: boolean;
  a: number[];
  fresh?: boolean;
  tag?: string;
};
