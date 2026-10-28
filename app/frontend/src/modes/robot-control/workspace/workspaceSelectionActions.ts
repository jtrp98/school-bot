import type { WorkspaceLogic } from "./WorkspaceLogic";

export function createWorkspaceSelectionActions(screen: WorkspaceLogic) {
  const guard = (event: MouseEvent | PointerEvent): void => {
    if (!screen.state.selMode) return;
    const target = event.target as Element | null;
    if (!target?.closest) return;
    const button = target.closest(
      "button, a, select, input, [role=button], [role=menuitem], [role=tab], [role=option], label",
    );
    if (!button) return;
    if (button.closest('[data-sel6ok], [role=table], [data-pop="stop"], [role=dialog], [role=alertdialog]')
      || screen.dom.contains(screen.dom.cardsTrack, button)) return;
    event.preventDefault();
    event.stopPropagation();
    if (event.type === "pointerdown" || (event.type === "click" && Date.now() - (screen._g6t || 0) > 400)) {
      screen._g6t = Date.now();
      if (screen.state.msgPop && screen.state.msgPop.kind !== "sel6") screen.setState({ msgPop: null });
      screen.tm(() => screen.msgPopOpen("sel6"), 0);
    }
  };

  const wheel = (event: WheelEvent): void => {
    const track = screen.dom.cardsTrack;
    if (!track || !screen.dom.contains(track, event.target)) return;
    if (Math.abs(event.deltaY) <= Math.abs(event.deltaX)) return;
    const before = track.scrollLeft;
    track.scrollLeft += event.deltaY;
    if (track.scrollLeft !== before) event.preventDefault();
  };

  return { guard, wheel };
}
