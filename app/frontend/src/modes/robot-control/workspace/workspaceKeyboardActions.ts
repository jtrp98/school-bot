import type { WorkspaceLogic } from "./WorkspaceLogic";

function isTypingTarget(target: EventTarget | null): target is HTMLInputElement | HTMLTextAreaElement {
  const element = target as HTMLElement | null;
  return element?.tagName === "INPUT" || element?.tagName === "TEXTAREA";
}

export function createWorkspaceKeyboardActions(screen: WorkspaceLogic) {
  const shortcut = (event: KeyboardEvent): void => {
    const target = event.target;
    const key = (event.key || "").toLowerCase();
    let typing = isTypingTarget(target);
    if (typing && (key === "c" || key === "x") && isTypingTarget(target)) {
      try {
        if (target.selectionStart === target.selectionEnd) typing = false;
      } catch {
        // Input types without a text selection still keep their keyboard shortcuts.
      }
    }
    if (!(event.ctrlKey || event.metaKey)) return;
    if (key === "s") {
      event.preventDefault();
      if (event.shiftKey) screen.saveAs();
      else screen.saveProj();
    } else if (key === "a" && !typing) {
      event.preventDefault();
      screen.openMerge();
    } else if (key === "o" && !typing) {
      event.preventDefault();
      screen.goWork();
    } else if (key === "h" && !typing && !event.shiftKey) {
      event.preventDefault();
      screen.setState({ menuOpen: false, logOpen: !screen.state.logOpen });
    } else if (key === "n" && !typing && !event.shiftKey) {
      event.preventDefault();
      screen.goNew();
    } else if (key === "c" && !typing && !event.shiftKey && !screen.pBusy()) {
      if (screen.gTarget()) {
        event.preventDefault();
        screen.gCopy(screen.gTarget());
      } else if (screen.pSrcKeys().length) {
        event.preventDefault();
        screen.pCopy(false);
      }
    } else if (key === "x" && !typing && !event.shiftKey && !screen.pBusy() && screen.pSrcKeys().length) {
      event.preventDefault();
      screen.pCopy(true);
    } else if (key === "v" && !typing && (screen.state.gClip || screen.state.pClip) && !screen.busyNow()) {
      event.preventDefault();
      screen.pasteClip(screen.pasteAt());
    }
  };

  const deleteGroup = (event: KeyboardEvent): void => {
    if (isTypingTarget(event.target) || (event.key !== "Delete" && event.key !== "Backspace")) return;
    const group = screen.gTarget();
    if (group && !screen.busyNow()) {
      event.preventDefault();
      screen.gDelete(group);
    }
  };

  const escape = (event: KeyboardEvent): void => {
    const target = event.target as HTMLElement | null;
    if (event.key !== "Escape" || target?.tagName === "INPUT") return;
    const state = screen.state;
    if (state.rangeListOpen) {
      screen.setState({ rangeListOpen: null });
      return;
    }
    if (state.sameAsk) {
      screen.setState({ sameAsk: false });
      return;
    }
    if (state.jMenu != null) {
      screen.setState({ jMenu: null });
      return;
    }
    if (state.ctx) {
      screen.setState({ ctx: null });
      return;
    }
    if (state.pctx) {
      screen.setState({ pctx: null });
      return;
    }
    if (state.far) {
      screen.pressing = false;
      screen.setState({ far: null });
      return;
    }
    if (state.pMode) {
      screen.pDone();
      return;
    }
    if (state.preflight && !state.checked && screen.props.bar && screen.props.bar !== "now") {
      screen.setState({ preflight: false });
      screen.endGroupRun();
      return;
    }
    if (state.menuOpen || state.clearAsk || state.leaveAsk || state.gMenu
      || state.selMode || state.mergeOpen || state.playbackMenu) {
      screen.setState({
        playbackMenu: null,
        mergeOpen: false,
        menuOpen: false,
        clearAsk: false,
        leaveAsk: null,
        gMenu: null,
        gRen: null,
        selMode: false,
        picks: [],
        grpNaming: false,
      });
      if (state.st !== "moving" && state.st !== "playing" && state.st !== "preparing") return;
    }
    if (state.mode === "range") {
      screen.setState({ mode: "all", picking: false, rangeDone: false, hoverIdx: -1 });
      if (state.st !== "moving" && state.st !== "playing" && state.st !== "preparing") return;
    }
    screen.stopAll();
  };

  return { shortcut, deleteGroup, escape };
}
