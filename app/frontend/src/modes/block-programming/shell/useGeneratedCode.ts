import { useEffect, useState } from "react";
import type * as Blockly from "blockly/core";
import { preview } from "../../../core/client";
import { EngineError } from "../../../core/engineError";
import { toEngineProgram } from "../blockly/engineProgram";
import { runBlockEngine } from "./blockEngine";

export type GeneratedCode =
  | { status: "desktopOnly" }
  | { status: "generating" }
  | { status: "ready"; code: string }
  | { status: "failed"; detail: string };

// Waits for a pause in editing so dragging a block does not start the engine on every step.
const GENERATE_DELAY_MS = 400;

// The C++ comes from the mixly engine through the host, which the browser preview does not have.
export function useGeneratedCode(workspace: Blockly.WorkspaceSvg | null, visible: boolean): GeneratedCode {
  const [code, setCode] = useState<GeneratedCode>(preview ? { status: "desktopOnly" } : { status: "generating" });

  useEffect(() => {
    if (!workspace || !visible || preview) return;
    let live = true;
    let latest = 0;
    let timer: number | undefined;
    const generate = () => {
      const request = ++latest;
      runBlockEngine<{ code?: string }>("program.generate", toEngineProgram(workspace))
        .then(response => {
          if (live && request === latest) setCode({ status: "ready", code: response.data?.code ?? "" });
        })
        .catch((error: unknown) => {
          if (!live || request !== latest) return;
          setCode({ status: "failed", detail: error instanceof EngineError ? error.detail : String(error) });
        });
    };
    const onChange = (event: Blockly.Events.Abstract) => {
      if (event.isUiEvent) return;
      window.clearTimeout(timer);
      timer = window.setTimeout(generate, GENERATE_DELAY_MS);
    };
    workspace.addChangeListener(onChange);
    generate();
    return () => {
      live = false;
      window.clearTimeout(timer);
      workspace.removeChangeListener(onChange);
    };
  }, [workspace, visible]);

  return code;
}
