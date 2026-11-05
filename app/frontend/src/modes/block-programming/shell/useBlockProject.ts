import { useCallback, useEffect, useRef, useState } from "react";
import * as Blockly from "blockly/core";
import { client } from "../../../core/client";
import { isRequiredBlock } from "../blockly/blocks";
import { useShell } from "../../../shell";
import { addStartingBlocks } from "./useBlockWorkspace";

// The host keeps one Block Lab project. Schema 2 holds the Blockly workspace; the old demo's
// schema 1 cannot describe these blocks, so it is ignored instead of loaded.
const SCHEMA = 2;
const COMPARE_DELAY_MS = 300;

type SavedProject = { schema?: number; name?: string; workspace?: object };
export type SaveState = "unsaved" | "saved" | "dirty" | "failed";
// What the child was about to do when unsaved work stopped them: start over, leave the page or
// close the app.
export type UnsavedReason = "new" | "leave" | "close";
type Confirm = { reason: UnsavedReason; proceed: () => void };

const snapshotOf = (workspace: Blockly.WorkspaceSvg, name: string) =>
  JSON.stringify({ name, workspace: Blockly.serialization.workspaces.save(workspace) });

// The menu no longer collapses or switches off blocks, so a project saved with such blocks opens
// with them back to normal, where the child can see and change them again.
function openUp(workspace: Blockly.WorkspaceSvg) {
  const manual = Blockly.constants.MANUALLY_DISABLED;
  for (const block of workspace.getAllBlocks(false)) {
    if (block.isCollapsed()) block.setCollapsed(false);
    if (block.hasDisabledReason(manual)) block.setDisabledReason(false, manual);
  }
}

// At most the starting hats: nothing the child made would be lost.
const isUntouched = (workspace: Blockly.WorkspaceSvg) =>
  workspace.getAllBlocks(false).every(block => isRequiredBlock(block.type))
  && workspace.getVariableMap().getAllVariables().length === 0;

// `holdBack` is asked first by every way out (new job, leave, close); answering true means it
// has taken over (a send to the board is running) and nothing goes ahead.
export function useBlockProject(
  workspace: Blockly.WorkspaceSvg | null,
  holdBack: (reason: UnsavedReason) => boolean,
) {
  const [name, setName] = useState("");
  const [saveState, setSaveState] = useState<SaveState>("unsaved");
  const [savedAt, setSavedAt] = useState<Date | null>(null);
  const [confirm, setConfirm] = useState<Confirm | null>(null);
  const loaded = useRef(false);
  // "Has unsaved changes" compares the content with the last save, so loading, undoing back
  // and rebuilding the workspace for another language never mark the project as edited.
  const savedSnapshot = useRef<string | null>(null);
  const nameRef = useRef(name);
  nameRef.current = name;

  const compare = useCallback(() => {
    if (!workspace || savedSnapshot.current === null) return;
    setSaveState(snapshotOf(workspace, nameRef.current) === savedSnapshot.current ? "saved" : "dirty");
  }, [workspace]);

  useEffect(() => {
    if (!workspace || loaded.current) return;
    loaded.current = true;
    let live = true;
    client.load("block-programming").then(value => {
      const saved = value as SavedProject | null;
      if (!live || saved?.schema !== SCHEMA || !saved.workspace) return;
      Blockly.serialization.workspaces.load(saved.workspace, workspace);
      openUp(workspace);
      workspace.clearUndo();
      const restoredName = saved.name ?? "";
      setName(restoredName);
      savedSnapshot.current = snapshotOf(workspace, restoredName);
      setSaveState("saved");
    }, () => undefined);
    return () => {
      live = false;
    };
  }, [workspace]);

  useEffect(() => {
    if (!workspace) return;
    let timer: number | undefined;
    const onChange = (event: Blockly.Events.Abstract) => {
      if (event.isUiEvent) return;
      window.clearTimeout(timer);
      timer = window.setTimeout(compare, COMPARE_DELAY_MS);
    };
    workspace.addChangeListener(onChange);
    return () => {
      window.clearTimeout(timer);
      workspace.removeChangeListener(onChange);
    };
  }, [workspace, compare]);

  useEffect(compare, [name, compare]);

  const save = useCallback(async () => {
    if (!workspace) return false;
    const snapshot = snapshotOf(workspace, nameRef.current);
    try {
      await client.save("block-programming", {
        schema: SCHEMA,
        name: nameRef.current,
        workspace: Blockly.serialization.workspaces.save(workspace),
      });
    } catch {
      setSaveState("failed");
      return false;
    }
    savedSnapshot.current = snapshot;
    setSavedAt(new Date());
    setSaveState("saved");
    return true;
  }, [workspace]);

  // Starting over replaces the blocks; the saved copy stays until the next save overwrites it.
  const startNew = useCallback(() => {
    if (!workspace) return;
    workspace.clear();
    addStartingBlocks(workspace);
    workspace.clearUndo();
    savedSnapshot.current = null;
    setName("");
    setSavedAt(null);
    setSaveState("unsaved");
  }, [workspace]);

  // Saved work, or nothing beyond the two hats, goes ahead at once; otherwise the child chooses.
  const guard = useCallback((reason: UnsavedReason, proceed: () => void) => {
    if (holdBack(reason)) return;
    const safe = !workspace || saveState === "saved" || (saveState === "unsaved" && isUntouched(workspace));
    if (safe) proceed();
    else setConfirm({ reason, proceed });
  }, [workspace, saveState, holdBack]);

  const requestNew = useCallback(() => guard("new", startNew), [guard, startNew]);

  const keepWorking = useCallback(() => setConfirm(null), []);
  // Asked again: a send may have started while the dialog was open.
  const discardAndContinue = useCallback(() => {
    setConfirm(null);
    if (confirm && !holdBack(confirm.reason)) confirm.proceed();
  }, [confirm, holdBack]);
  // A failed save keeps the dialog open; the window bar and the dialog both say it failed.
  const saveAndContinue = useCallback(async () => {
    if (!(await save())) return;
    setConfirm(null);
    if (confirm && !holdBack(confirm.reason)) confirm.proceed();
  }, [save, confirm, holdBack]);

  // While Block Lab is open, closing the window asks about unsaved work here instead of in the
  // app's general close dialog.
  const { setCloseGuard, closeApp } = useShell();
  const latestGuard = useRef(guard);
  latestGuard.current = guard;
  useEffect(() => {
    setCloseGuard(() => latestGuard.current("close", closeApp));
    return () => setCloseGuard(null);
  }, [setCloseGuard, closeApp]);

  useEffect(() => {
    const onKey = (event: KeyboardEvent) => {
      if (!(event.ctrlKey || event.metaKey) || event.altKey) return;
      const key = event.key.toLowerCase();
      if (key === "s" && !event.shiftKey) {
        event.preventDefault();
        void save();
      } else if (key === "n" && !event.shiftKey) {
        event.preventDefault();
        requestNew();
      }
    };
    window.addEventListener("keydown", onKey);
    return () => window.removeEventListener("keydown", onKey);
  }, [save, requestNew]);

  return {
    name,
    rename: setName,
    saveState,
    savedAt,
    save,
    requestNew,
    guard,
    confirmReason: confirm?.reason ?? null,
    keepWorking,
    discardAndContinue,
    saveAndContinue,
  };
}

export type BlockProject = ReturnType<typeof useBlockProject>;
