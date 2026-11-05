import * as Blockly from "blockly/core";

// Checked before the list (Blockly's flyout, weight 1): a drag the system cancelled must find
// this area first wherever the pointer is.
const WEIGHT = -1;

// Holds every point outside a box, so Blockly treats everything beyond it (for the workspace:
// the rail, the window bar, the drawer) as one place to drop a block for deleting.
class OutsideRect extends Blockly.utils.Rect {
  constructor(
    private readonly area: OutsideDeleteArea,
    box: DOMRect,
  ) {
    super(box.top, box.bottom, box.left, box.right);
  }

  override contains(x: number, y: number) {
    return this.area.cancelled || !super.contains(x, y);
  }
}

// The design deletes a placed block let go anywhere outside the workspace, as the list does, and
// a function input let go outside the white part of the settings box. `inside` is the element
// whose box counts as inside. A drag the system cancels (pointercancel) goes back where it
// started instead, wherever it was.
export class OutsideDeleteArea extends Blockly.DeleteArea {
  override id = "sbOutsideDeleteArea";
  cancelled = false;

  constructor(
    private readonly workspace: Blockly.WorkspaceSvg,
    private readonly inside: () => Element,
  ) {
    super();
  }

  register() {
    const { Capability } = Blockly.ComponentManager;
    this.workspace.getComponentManager().addComponent({
      component: this,
      weight: WEIGHT,
      capabilities: [Capability.DELETE_AREA, Capability.DRAG_TARGET],
    });
  }

  unregister() {
    this.workspace.getComponentManager().removeComponent(this.id);
  }

  override getClientRect() {
    return new OutsideRect(this, this.inside().getBoundingClientRect());
  }

  override wouldDelete(element: Blockly.IDraggable) {
    return super.wouldDelete(element) && !this.cancelled;
  }

  override shouldPreventMove() {
    return this.cancelled;
  }
}
