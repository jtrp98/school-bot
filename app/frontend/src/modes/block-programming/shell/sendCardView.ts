import { preview } from "../../../core/client";
import type { SendFailure, SendState, SendStep } from "./sendOutcome";

export type ShownSendState = Exclude<SendState, { status: "idle" }>;

// What a send card says, as catalog keys and values; SendCard draws it.
export type SendCardView = {
  role: "status" | "alert";
  mascot: "search" | "robot" | null;
  mark: "spinner" | "tick" | null;
  titleKey: string;
  bodyKey: string;
  tipKeys: string[];
  step: SendStep | null;
  // The code a child reads out to the teacher (docs/TEACHER-SETUP.md lists them).
  errorCode: string | null;
  // The tool's own text, shown as sent for the teacher.
  output: string | null;
  previewNote: boolean;
};

const FAILURES: Record<SendFailure, { errorCode: string; tipKeys: string[] }> = {
  compile: {
    errorCode: "E-BUILD",
    tipKeys: ["blockLab.send.failed.compile.tips.check", "blockLab.send.failed.compile.tips.teacher"],
  },
  upload: {
    errorCode: "E-USB",
    tipKeys: [
      "blockLab.send.failed.upload.tips.cable",
      "blockLab.send.failed.upload.tips.monitor",
      "blockLab.send.failed.upload.tips.again",
    ],
  },
  compileTimeout: {
    errorCode: "E-TIME",
    tipKeys: ["blockLab.send.failed.compileTimeout.tips.again", "blockLab.send.failed.compileTimeout.tips.teacher"],
  },
  timeout: {
    errorCode: "E-TIME",
    tipKeys: ["blockLab.send.failed.timeout.tips.cable", "blockLab.send.failed.timeout.tips.again"],
  },
  toolchain: {
    errorCode: "E-TOOL",
    tipKeys: [
      "blockLab.send.failed.toolchain.tips.install",
      "blockLab.send.failed.toolchain.tips.open",
      "blockLab.send.failed.toolchain.tips.again",
    ],
  },
  busy: {
    errorCode: "E-BUSY",
    tipKeys: ["blockLab.send.failed.busy.tips.again"],
  },
  engine: {
    errorCode: "E-APP",
    tipKeys: ["blockLab.send.failed.engine.tips.again", "blockLab.send.failed.engine.tips.teacher"],
  },
  uploadEngine: {
    errorCode: "E-APP",
    tipKeys: ["blockLab.send.failed.engine.tips.again", "blockLab.send.failed.engine.tips.teacher"],
  },
  robotPort: {
    errorCode: "E-ARM",
    tipKeys: ["blockLab.send.failed.robotPort.tips.pick", "blockLab.send.failed.robotPort.tips.replug"],
  },
};

const FAILURE_TEXT: Record<SendFailure, { titleKey: string; bodyKey: string }> = {
  compile: { titleKey: "blockLab.send.failed.compile.title", bodyKey: "blockLab.send.failed.compile.body" },
  upload: { titleKey: "blockLab.send.failed.upload.title", bodyKey: "blockLab.send.failed.upload.body" },
  compileTimeout: {
    titleKey: "blockLab.send.failed.compileTimeout.title",
    bodyKey: "blockLab.send.failed.compileTimeout.body",
  },
  timeout: { titleKey: "blockLab.send.failed.timeout.title", bodyKey: "blockLab.send.failed.timeout.body" },
  toolchain: { titleKey: "blockLab.send.failed.toolchain.title", bodyKey: "blockLab.send.failed.toolchain.body" },
  busy: { titleKey: "blockLab.send.failed.busy.title", bodyKey: "blockLab.send.failed.busy.body" },
  engine: { titleKey: "blockLab.send.failed.engine.title", bodyKey: "blockLab.send.failed.engine.body" },
  uploadEngine: { titleKey: "blockLab.send.failed.engine.title", bodyKey: "blockLab.send.failed.upload.body" },
  robotPort: { titleKey: "blockLab.send.failed.robotPort.title", bodyKey: "blockLab.send.failed.robotPort.body" },
};

const CANCELLED_TEXT: Record<SendStep, { titleKey: string; bodyKey: string }> = {
  compile: { titleKey: "blockLab.send.cancelled.compile.title", bodyKey: "blockLab.send.cancelled.compile.body" },
  upload: { titleKey: "blockLab.send.cancelled.upload.title", bodyKey: "blockLab.send.cancelled.upload.body" },
};

const PLAIN: Omit<SendCardView, "titleKey" | "bodyKey"> = {
  role: "status",
  mascot: null,
  mark: null,
  tipKeys: [],
  step: null,
  errorCode: null,
  output: null,
  previewNote: false,
};

export function sendCardView(state: ShownSendState): SendCardView {
  switch (state.status) {
    case "noBoard":
      return {
        ...PLAIN,
        mascot: "search",
        titleKey: "blockLab.send.noBoard.title",
        bodyKey: "blockLab.send.noBoard.body",
        tipKeys: [
          "blockLab.send.noBoard.tips.cable",
          state.chosen ? "blockLab.send.noBoard.tips.wait" : "blockLab.send.noBoard.tips.pick",
        ],
        errorCode: "E-USB",
      };
    case "compiling":
      return {
        ...PLAIN,
        mascot: "robot",
        mark: "spinner",
        titleKey: "blockLab.send.compiling.title",
        bodyKey: "blockLab.send.compiling.body",
        step: "compile",
      };
    case "uploading":
      return {
        ...PLAIN,
        mascot: "robot",
        mark: "spinner",
        titleKey: "blockLab.send.uploading.title",
        bodyKey: "blockLab.send.uploading.body",
        step: "upload",
      };
    case "stopping":
      return {
        ...PLAIN,
        mascot: "robot",
        mark: "spinner",
        titleKey: "blockLab.send.stopping.title",
        bodyKey: "blockLab.send.stopping.body",
        step: "upload",
      };
    case "cancelled":
      return { ...PLAIN, ...CANCELLED_TEXT[state.step] };
    case "sent":
      return {
        ...PLAIN,
        mascot: "robot",
        mark: "tick",
        titleKey: "blockLab.send.sent.title",
        bodyKey: "blockLab.send.sent.body",
        previewNote: preview,
      };
    case "failed":
      return {
        ...PLAIN,
        role: "alert",
        mascot: "robot",
        ...FAILURE_TEXT[state.failure],
        ...FAILURES[state.failure],
        output: state.output || null,
      };
  }
}
