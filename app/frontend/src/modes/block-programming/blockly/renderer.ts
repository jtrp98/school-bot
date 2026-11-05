import * as Blockly from "blockly/core";

export const BLOCK_LAB_RENDERER = "school-bot";

// Geometry from the Block Lab mockup's block component: corner 7, a 24×5 trapezoid notch
// 14px in, a half-ellipse hat 22px high, 28px value slots. Zelos supplies the pill and
// hexagon outputs the design uses for values and conditions. Colours live in blockLab.css.
class BlockLabConstants extends Blockly.zelos.ConstantProvider {
  constructor() {
    super();
    this.CORNER_RADIUS = 7;
    this.NOTCH_WIDTH = 24;
    this.NOTCH_HEIGHT = 5;
    this.NOTCH_OFFSET_LEFT = 14;
    this.START_HAT_HEIGHT = 22;
    this.START_HAT_WIDTH = 120;
    this.MIN_BLOCK_HEIGHT = 44;
    this.FIELD_BORDER_RECT_HEIGHT = 28;
    this.FIELD_DROPDOWN_BORDER_RECT_HEIGHT = 28;
  }

  override makeNotch() {
    const slope = 4;
    const flat = this.NOTCH_WIDTH - 2 * slope;
    const height = this.NOTCH_HEIGHT;
    return {
      type: this.SHAPES.NOTCH,
      width: this.NOTCH_WIDTH,
      height,
      pathLeft: `l ${slope},${height} h ${flat} l ${slope},${-height}`,
      pathRight: `l ${-slope},${height} h ${-flat} l ${-slope},${-height}`,
    };
  }

  override makeStartHat() {
    const width = this.START_HAT_WIDTH;
    const height = this.START_HAT_HEIGHT;
    return { height, width, path: `a ${width / 2},${height} 0 0,1 ${width},0` };
  }
}

class BlockLabRenderer extends Blockly.zelos.Renderer {
  protected override makeConstants_() {
    return new BlockLabConstants();
  }
}

if (!Blockly.registry.hasItem(Blockly.registry.Type.RENDERER, BLOCK_LAB_RENDERER))
  Blockly.blockRendering.register(BLOCK_LAB_RENDERER, BlockLabRenderer);
