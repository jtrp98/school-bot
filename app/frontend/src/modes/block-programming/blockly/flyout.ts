import * as Blockly from "blockly/core";

// The A2 block list is 340 wide. A longer device block widens it (up to 560, as the design
// allows) instead of being cut off.
export const LIST_WIDTH = 340;
const MAX_LIST_WIDTH = 560;
// Blockly starts items 8px in from the left and top; the design lines blocks up 16px from the
// edge and 10px under the 52px list header that React draws over the flyout.
const EXTRA_INDENT = 8;
const HEADER_SPACE = 54;
// A2 draws a 2px line over every sub-group heading but the first, 16 in from the list's edges,
// in the space toolbox.ts leaves above the heading.
const DIVIDER_INSET = 16;
const DIVIDER_ABOVE = 12;
const HEADING_CLASS = "sbListHeading";

const notInList = () => false;

function isHeading(element: Blockly.IBoundedElement): element is Blockly.FlyoutButton {
  return element instanceof Blockly.FlyoutButton
    && element.isLabel()
    && element.getSvgRoot().classList.contains(HEADING_CLASS);
}

function addDivider(heading: Blockly.FlyoutButton) {
  return Blockly.utils.dom.createSvgElement(
    Blockly.utils.Svg.RECT,
    {
      class: "sbListDivider",
      x: DIVIDER_INSET - heading.getPosition().x,
      y: -DIVIDER_ABOVE,
      height: 2,
    },
    heading.getSvgRoot(),
  );
}

// In the list a tap adds the whole block (addFromList). Blockly would otherwise let a tap edit a
// value, open a dropdown or a gear box on the list's own copy, since the list does not close.
function tapsAddWholeBlock(block: Blockly.BlockSvg) {
  for (const part of block.getDescendants(false)) {
    for (const input of part.inputList) {
      for (const field of input.fieldRow) field.isClickableInFlyout = notInList;
    }
    for (const icon of part.getIcons()) icon.isClickableInFlyout = notInList;
  }
}

// Blockly's vertical flyout with the A2 list geometry. Only placement and size change here;
// which blocks appear and what they generate still come from the catalog and definitions.
export class BlockLabFlyout extends Blockly.VerticalFlyout {
  onDeleteZone?: (active: boolean) => void;
  onWidth?: (width: number) => void;
  // A2 rounds the list's outer corners at 16.
  override readonly CORNER_RADIUS = 16;
  // Drawn inside their headings, so they go when the list is cleared; sized once the width is known.
  private dividers: SVGRectElement[] = [];

  // The list stays at 100% while the workspace zooms; a dragged block takes the workspace's size.
  override getFlyoutScale() {
    return 1;
  }

  protected override layout_(contents: Blockly.FlyoutItem[]) {
    super.layout_(contents);
    this.dividers = [];
    let headings = 0;
    for (const item of contents) {
      const element = item.getElement();
      element.moveBy(EXTRA_INDENT, HEADER_SPACE);
      if (element instanceof Blockly.BlockSvg) tapsAddWholeBlock(element);
      else if (isHeading(element) && headings++ > 0) this.dividers.push(addDivider(element));
    }
  }

  protected override reflowInternal_() {
    super.reflowInternal_();
    const width = Math.min(MAX_LIST_WIDTH, Math.max(LIST_WIDTH, this.width_));
    if (width !== this.width_) {
      this.width_ = width;
      this.position();
      this.targetWorkspace.resizeContents();
      this.targetWorkspace.recordDragTargets();
    }
    for (const divider of this.dividers) divider.setAttribute("width", String(this.width_ - 2 * DIVIDER_INSET));
    this.onWidth?.(this.width_);
  }

  override onDragEnter(element: Blockly.IDraggable) {
    super.onDragEnter(element);
    this.onDeleteZone?.(this.wouldDelete(element));
  }

  override onDragExit(element: Blockly.IDraggable) {
    super.onDragExit(element);
    this.onDeleteZone?.(false);
  }

  override onDrop(element: Blockly.IDraggable) {
    super.onDrop(element);
    this.onDeleteZone?.(false);
  }
}
