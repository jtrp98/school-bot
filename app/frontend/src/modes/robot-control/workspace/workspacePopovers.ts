import { t as translate } from "../../../i18n";
import { hasSeenTableKeysTip, markTableKeysTipSeen } from "./tableKeyTipStorage";
import type { WorkspaceLogic } from "./WorkspaceLogic";
import type { CopiedCommandRow } from "./viewTypes";

export type MessagePopoverKind = "disc" | "run" | "nosel" | "go" | "goType" | "nopose" | "limit" | "net" | "sel6";

export type MessagePopoverOptions = {
  title?: string;
  body?: string;
  anchorSelector?: string;
  pick?: 0 | 1;
  red?: boolean;
};

export type MessagePopoverState = {
  kind: MessagePopoverKind;
  // A translation key for fixed messages, already-translated text for messages built from live values.
  title: string;
  body: string;
  red: boolean;
  x: number;
  y: number;
  opensUp: boolean;
  anchor: { left: number; top: number; width: number; height: number };
  shakes: number;
};

export type KeyPopoverState = { x: number; y: number };

export type CopyPopoverState = {
  title: string;
  code: string;
  rows: CopiedCommandRow[];
  open: boolean;
};

type PopoverSpec = {
  title: string;
  body: string;
  anchorSelector?: string;
  red?: boolean;
  pick?: 0 | 1;
};

const POPOVER_WIDTH = 330;
const POPOVER_HEIGHT = 150;
const KEY_TIP_HEIGHT = 150;
const MIN_VISIBLE_ANCHOR_WIDTH = 40;
const FALLBACK_VIEWPORT = { width: 1440, height: 900 };

function describePopover(kind: MessagePopoverKind, selected: number, options: MessagePopoverOptions): PopoverSpec {
  switch (kind) {
    case "disc":
      return {
        title: "robot.workspace.popover.disconnectedTitle",
        body: "robot.workspace.popover.disconnectedBody",
        anchorSelector: '[data-pop="reconnect"]',
      };
    case "run":
      return {
        title: "robot.workspace.messages.playingSequence",
        body: "robot.workspace.popover.runningBody",
        anchorSelector: '[data-pop="stop"]',
      };
    case "nosel":
      return {
        title: "robot.workspace.messages.noPoseSelected",
        body: "robot.workspace.popover.noSelectionBody",
        anchorSelector: '[data-pose-index="1"]',
      };
    case "go":
      return {
        title: translate("robot.workspace.popover.notAtPoseTitle", { number: selected + 1 }),
        body: "robot.workspace.popover.notAtPoseBody",
        anchorSelector: '[data-pop="go"]',
      };
    case "goType":
      return {
        title: "robot.workspace.popover.valueNotUsableTitle",
        body: translate("robot.workspace.popover.valueNotUsableBody", { number: selected + 1 }),
        anchorSelector: '[data-pop="go"]',
      };
    case "nopose":
      return {
        title: "robot.workspace.messages.noPosesYet",
        body: "robot.workspace.popover.noPoseBody",
        anchorSelector: '[data-pop="add"]',
      };
    case "limit":
      return {
        title: options.title ?? "",
        body: options.body ?? "",
        red: true,
        anchorSelector: options.anchorSelector,
        pick: options.pick,
      };
    case "net":
      return {
        title: options.title ?? "",
        body: options.body ?? "",
        red: !!options.red,
        anchorSelector: options.anchorSelector,
      };
    case "sel6":
      return {
        title: "robot.workspace.popover.saveFirstTitle",
        body: "robot.workspace.popover.saveFirstBody",
        anchorSelector: "[data-exit6]",
      };
  }
}

function findAnchor(host: WorkspaceLogic, spec: PopoverSpec): HTMLElement | null {
  const { anchorSelector, pick } = spec;
  if (!anchorSelector) return null;
  const first: HTMLElement | null = host.q(anchorSelector);
  if (pick === undefined) return first;
  // Joint buttons are rendered in several panels; only the visible one can anchor the limit message.
  const visible = Array.from<HTMLElement>(host.qa(anchorSelector)).filter(
    (button) => button.offsetWidth >= MIN_VISIBLE_ANCHOR_WIDTH,
  );
  if (!visible.length) return first;
  return pick ? visible[visible.length - 1] : visible[0];
}

function viewportSize() {
  return {
    width: window.innerWidth || FALLBACK_VIEWPORT.width,
    height: window.innerHeight || FALLBACK_VIEWPORT.height,
  };
}

type PopoverDismissTarget = EventTarget | null;

function isInsideJointCard(target: PopoverDismissTarget): boolean {
  const candidate = target as { closest?: (selector: string) => unknown } | null;
  if (!candidate || typeof candidate.closest !== "function") return false;
  return !!candidate.closest(".aw-jcard button, .aw-jcard input, .aw-jcard label");
}

function removeMessagePopoverListener(screen: WorkspaceLogic) {
  if (!screen._mpL) return;
  window.removeEventListener("pointerdown", screen._mpL, true);
  window.removeEventListener("keydown", screen._mpL, true);
  screen._mpL = null;
}

function listenForMessagePopoverDismiss(screen: WorkspaceLogic) {
  removeMessagePopoverListener(screen);
  // Registered on the next tick so the click that opened the popover does not dismiss it.
  screen.tm(() => {
    screen._mpL = (event: Event) => {
      if (event.type === "keydown" && (event as KeyboardEvent).key !== "Escape") return;
      const popover: MessagePopoverState | null = screen.state.msgPop;
      const jointCardWhileLocked = event.type === "pointerdown"
        && isInsideJointCard(event.target)
        && !!screen.lockKind()
        && !!popover
        && popover.kind !== "limit";
      if (jointCardWhileLocked) return;
      removeMessagePopoverListener(screen);
      screen.setState({ msgPop: null });
    };
    window.addEventListener("pointerdown", screen._mpL, true);
    window.addEventListener("keydown", screen._mpL, true);
  }, 0);
}

export function msgPopOpen(screen: WorkspaceLogic, kind: MessagePopoverKind, options: MessagePopoverOptions = {}) {
  if (typeof document === "undefined") return;
  const current: MessagePopoverState | null = screen.state.msgPop;
  if (current && current.kind === kind) {
    screen.setState({ msgPop: { ...current, shakes: current.shakes + 1 } });
    return;
  }
  const spec = describePopover(kind, screen.state.sel, options);
  const anchor = findAnchor(screen, spec);
  if (!anchor) return;
  if (kind === "go") {
    const card = anchor.closest(".aw-card") ?? anchor;
    try {
      card.scrollIntoView({ block: "nearest", inline: "nearest" });
    } catch {
      // A detached anchor must not stop the explanation from showing.
    }
  }
  const rect = anchor.getBoundingClientRect();
  const viewport = viewportSize();
  const alignRight = rect.left + rect.width / 2 > viewport.width / 2;
  const x = Math.max(8, Math.min(alignRight ? rect.right - POPOVER_WIDTH : rect.left, viewport.width - POPOVER_WIDTH - 8));
  const opensUp = rect.bottom + POPOVER_HEIGHT > viewport.height;
  const popover: MessagePopoverState = {
    kind,
    title: spec.title,
    body: spec.body,
    red: !!spec.red,
    x,
    y: opensUp ? Math.max(8, viewport.height - rect.top + 12) : rect.bottom + 12,
    opensUp,
    anchor: { left: rect.left, top: rect.top, width: rect.width, height: rect.height },
    shakes: 0,
  };
  screen.setState({ msgPop: popover });
  listenForMessagePopoverDismiss(screen);
}

export function keyPopOpen(screen: WorkspaceLogic, anchor: HTMLElement | null) {
  const state = screen.state;
  if (state.keyTipSeen || state.keyPop || !anchor || !anchor.getBoundingClientRect) return;
  if (hasSeenTableKeysTip()) {
    screen.setState({ keyTipSeen: true });
    return;
  }
  const rect = anchor.getBoundingClientRect();
  const viewport = viewportSize();
  const x = Math.max(8, Math.min(rect.left - 8, viewport.width - POPOVER_WIDTH - 8));
  const opensUp = rect.bottom + 10 + KEY_TIP_HEIGHT > viewport.height;
  const keyPop: KeyPopoverState = { x, y: opensUp ? Math.max(8, rect.top - 10 - KEY_TIP_HEIGHT) : rect.bottom + 10 };
  screen.setState({ keyTipSeen: true, keyPop });
  markTableKeysTipSeen();
}

export function openCopy(screen: WorkspaceLogic, title: string, code: string, joints: number[]) {
  clearTimeout(screen.toastT);
  screen.closeCopyL();
  const rows: CopiedCommandRow[] = joints.map((joint) => {
    const parts = /#(\d+)P(\d+)T(\d+)/.exec(screen.gcodePart(joint)) ?? [];
    return {
      sid: screen.SV[joint].sid,
      name: screen.state.jnames[joint],
      id: "#" + (parts[1] ?? ""),
      p: parts[2] ?? "",
      t: parts[3] ?? "",
    };
  });
  const copyPop: CopyPopoverState = { title, code, rows, open: false };
  screen.setState({ toast: null, copyPop });
  screen.tm(() => {
    screen.cpDown = (event: Event) => {
      const box: HTMLElement | null = screen.q("[data-cpbox]");
      if (box && box.contains(event.target as Node | null)) return;
      screen.closeCopy();
    };
    screen.cpKey = (event: KeyboardEvent) => {
      if (event.key === "Escape") screen.closeCopy();
    };
    window.addEventListener("pointerdown", screen.cpDown, true);
    window.addEventListener("keydown", screen.cpKey);
  }, 0);
}

export function netPop(screen: WorkspaceLogic, anchorSelector: string) {
  const state = screen.state;
  const result: { mbps: number | null; ping: number | null } | null = state.netRes;
  const offline = state.netSt === "off";
  const measured = !offline && !!result && result.mbps != null;
  const title = translate(offline ? "robot.workspace.popover.offlineTitle" : "robot.workspace.popover.slowTitle");
  const bodyKey = offline
    ? "robot.workspace.popover.offlineBody"
    : measured ? "robot.workspace.popover.slowBodyMeasured" : "robot.workspace.popover.slowBody";
  const body = translate(bodyKey, measured && result ? { mbps: result.mbps, ping: result.ping } : undefined);
  screen.msgPopOpen("net", { title, body, anchorSelector, red: offline });
}
