import { useCallback, useEffect, useRef, useState } from "react";
import * as Blockly from "blockly/core";
import { preview } from "../../../core/client";
import { EngineError } from "../../../core/engineError";
import { toEngineProgram } from "../blockly/engineProgram";
import {
  findBlockProblems,
  fromEngineProblems,
  problemBlockId,
  type EngineProblem,
  type PreflightProblem,
} from "../blockly/preflight";
import { runBlockEngine } from "./blockEngine";

type PreflightState =
  | { status: "idle" }
  | { status: "checking" }
  | { status: "problems"; problems: PreflightProblem[] };

const IDLE: PreflightState = { status: "idle" };

// The browser preview has no engine, so there only the checks on the blocks themselves run.
async function wiringProblems(workspace: Blockly.WorkspaceSvg): Promise<PreflightProblem[]> {
  if (preview) return [];
  try {
    const response = await runBlockEngine<{ problems?: EngineProblem[] }>(
      "project.validate",
      toEngineProgram(workspace),
    );
    const problems = response.data?.problems;
    // An engine that answers without the list (an older build) has not checked the wiring.
    if (!Array.isArray(problems)) return [{ kind: "engine" }];
    return fromEngineProblems(workspace, problems);
  } catch (error) {
    // The engine's reason is diagnostic text, shown as sent for the teacher.
    return [{ kind: "engine", detail: error instanceof EngineError ? error.detail : String(error) }];
  }
}

// The check before sending. It runs again on every press of "send to board". With problems
// it lists them and answers false, so nothing is sent; without, it answers true and the send
// goes on (useSendToBoard).
export function usePreflight(workspace: Blockly.WorkspaceSvg | null) {
  const [state, setState] = useState<PreflightState>(IDLE);
  // The block "show me" pointed at, ringed until the child changes something (useProblemRing).
  const [pointedAt, setPointedAt] = useState<string | null>(null);
  const latest = useRef(0);
  // Enter or Space still reach the button while it shows the check running.
  const running = useRef(false);

  const check = useCallback(async (): Promise<boolean> => {
    if (!workspace || running.current) return false;
    running.current = true;
    setPointedAt(null);
    const request = ++latest.current;
    setState({ status: "checking" });
    try {
      const problems = [...findBlockProblems(workspace), ...await wiringProblems(workspace)];
      if (request !== latest.current) return false;
      setState(problems.length ? { status: "problems", problems } : IDLE);
      return problems.length === 0;
    } finally {
      running.current = false;
    }
  }, [workspace]);

  const close = useCallback(() => {
    latest.current++;
    setState(IDLE);
  }, []);

  // Closes the list, then shows the block in the middle of the workspace with focus on it,
  // after the dialog has handed focus back.
  const show = useCallback((problem: PreflightProblem) => {
    close();
    const id = problemBlockId(problem);
    const block = id ? workspace?.getBlockById(id) : null;
    if (!workspace || !block) return;
    requestAnimationFrame(() => {
      if (block.isDeadOrDying()) return;
      workspace.centerOnBlock(block.id);
      Blockly.getFocusManager().focusNode(block);
      setPointedAt(block.id);
    });
  }, [close, workspace]);

  // A rebuilt workspace (language change) takes the listed blocks with it.
  useEffect(() => close, [workspace, close]);

  const clearPointer = useCallback(() => setPointedAt(null), []);

  return { state, check, close, show, pointedAt, clearPointer };
}

export type Preflight = ReturnType<typeof usePreflight>;
