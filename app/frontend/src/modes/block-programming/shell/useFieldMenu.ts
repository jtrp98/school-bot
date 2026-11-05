import { useCallback, useEffect, useRef, useState, type KeyboardEvent } from "react";
import * as Blockly from "blockly/core";
import { setFieldMenuOpener, type FieldMenuRequest } from "../blockly/fieldMenu";

// Changes that move or remove the field the menu points at.
const MOVING_EVENTS: readonly string[] = [
  Blockly.Events.VIEWPORT_CHANGE,
  Blockly.Events.BLOCK_MOVE,
  Blockly.Events.BLOCK_DRAG,
  Blockly.Events.BLOCK_DELETE,
];

// Arrow steps between options: the colour grid has four columns, lists have one.
function step(key: string, columns: number) {
  if (key === "ArrowDown") return columns;
  if (key === "ArrowUp") return -columns;
  if (columns > 1 && key === "ArrowRight") return 1;
  if (columns > 1 && key === "ArrowLeft") return -1;
  return 0;
}

// The popover a dropdown field opens. While it is open it holds Blockly's ephemeral focus, so
// Blockly's keyboard shortcuts stay out of the way and focus returns to the field afterwards.
export function useFieldMenu(workspace: Blockly.WorkspaceSvg | null) {
  const [request, setRequest] = useState<FieldMenuRequest | null>(null);
  const panel = useRef<HTMLDivElement>(null);

  useEffect(() => {
    setFieldMenuOpener(setRequest);
    return () => setFieldMenuOpener(null);
  }, []);

  useEffect(() => {
    const element = panel.current;
    if (!request || !element) return;
    const focus = Blockly.getFocusManager();
    const giveBack = focus.ephemeralFocusTaken() ? null : focus.takeEphemeralFocus(element);
    const start = element.querySelector<HTMLElement>("[role=option][aria-selected=true]")
      ?? element.querySelector<HTMLElement>("[role=tab][aria-selected=true]")
      ?? element.querySelector<HTMLElement>("[role=option]");
    start?.focus({ preventScroll: true });
    start?.scrollIntoView({ block: "nearest" });
    // The colour picker has tabs and boxes to Tab through; it closes once focus leaves it.
    const tabsInside = request.kind === "color";
    const onPointerDown = (event: PointerEvent) => {
      if (!element.contains(event.target as Node)) setRequest(null);
    };
    const onKeyDown = (event: globalThis.KeyboardEvent) => {
      if (event.key !== "Escape" && (event.key !== "Tab" || tabsInside)) return;
      // Esc belongs to this menu, not to the block list behind it.
      event.preventDefault();
      event.stopPropagation();
      setRequest(null);
    };
    const onFocusOut = (event: FocusEvent) => {
      const next = event.relatedTarget as Node | null;
      if (tabsInside && next && !element.contains(next)) setRequest(null);
    };
    const onChange = (event: Blockly.Events.Abstract) => {
      if (MOVING_EVENTS.includes(event.type) || request.source.isDeadOrDying()) setRequest(null);
    };
    const onResize = () => setRequest(null);
    document.addEventListener("pointerdown", onPointerDown, true);
    document.addEventListener("keydown", onKeyDown, true);
    element.addEventListener("focusout", onFocusOut);
    window.addEventListener("resize", onResize);
    workspace?.addChangeListener(onChange);
    return () => {
      document.removeEventListener("pointerdown", onPointerDown, true);
      document.removeEventListener("keydown", onKeyDown, true);
      element.removeEventListener("focusout", onFocusOut);
      window.removeEventListener("resize", onResize);
      workspace?.removeChangeListener(onChange);
      giveBack?.();
    };
  }, [request, workspace]);

  // The workspace is rebuilt when the language changes, which takes the field with it.
  useEffect(() => () => setRequest(null), [workspace]);

  const pick = useCallback((value: string) => {
    request?.pick(value);
    setRequest(null);
  }, [request]);

  // The colour picker changes the field as the child mixes, and stays open.
  const apply = useCallback((value: string) => request?.pick(value), [request]);

  const moveFocus = useCallback((event: KeyboardEvent<HTMLDivElement>) => {
    const options = [...event.currentTarget.querySelectorAll<HTMLElement>("[role=option]")];
    const index = options.indexOf(document.activeElement as HTMLElement);
    // Arrows on the colour picker's slider or boxes are theirs, not a move between options.
    if (index < 0 && request?.kind === "color") return;
    const columns = request?.kind === "color" ? 4 : 1;
    let next = index + step(event.key, columns);
    if (event.key === "Home") next = 0;
    else if (event.key === "End") next = options.length - 1;
    else if (next === index) return;
    event.preventDefault();
    options[(next + options.length) % options.length]?.focus();
  }, [request]);

  return { request, panel, pick, apply, moveFocus };
}

export type FieldMenuState = ReturnType<typeof useFieldMenu>;
