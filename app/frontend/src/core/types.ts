export type ModeID = "robot-control" | "coding-adventure" | "block-programming";
export type Command = {
  op: "move_right" | "move_left" | "jump";
  value?: number;
};
export type ProgramEngineData = {
  commands?: Command[];
  steps?: number;
  simulated?: boolean;
};
export type EngineResponse<TData = ProgramEngineData> = {
  protocol: string;
  request_id: string;
  status: "success" | "error";
  data?: TData;
  error?: { code: string; message: string };
};
export type AppInfo = {
  version: string;
  platform: string;
  api_url: string;
  runtime: string;
};
