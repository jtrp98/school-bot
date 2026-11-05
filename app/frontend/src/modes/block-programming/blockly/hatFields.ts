import * as Blockly from "blockly/core";

export const PILL_FIELD = "sb_pill";

// The start and loop hats' icons from the design, on a 24-unit grid. The rail draws the Start and
// Control categories with the same two icons.
export const HAT_ICON_PATHS = {
  start: "M5 21V4M5 4h11l-2 4 2 4H5",
  loop: "M17 2l4 4-4 4M3 11V9a3 3 0 0 1 3-3h15M7 22l-4-4 4-4M21 13v2a3 3 0 0 1-3 3H3",
};
export type HatIcon = keyof typeof HAT_ICON_PATHS;

const ICON_SIZE = 20;
// An image cannot read CSS variables, so the stroke repeats the Block Lab ink token.
const ICON_INK = "#14243D";

export function hatIcon(icon: HatIcon) {
  const svg = [
    `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="${ICON_INK}"`,
    ` stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round"><path d="${HAT_ICON_PATHS[icon]}"/></svg>`,
  ].join("");
  return {
    type: "field_image",
    src: `data:image/svg+xml,${encodeURIComponent(svg)}`,
    width: ICON_SIZE,
    height: ICON_SIZE,
    alt: "",
  };
}

const PILL_HEIGHT = 28;
const PILL_PADDING = 10;
const TICK_SIZE = 16;
const TICK_GAP = 4;
const TICK_PATH = "M5 12l5 5L20 7";

// A start or loop hat in the block list while the program already holds one: it shows as placed
// and cannot be added again, since a second setup() would not compile.
export function isPlacedInProgram(block: Blockly.Block) {
  const list = block.workspace as Blockly.WorkspaceSvg;
  return list.isFlyout && !!list.targetWorkspace?.getBlocksByType(block.type, false).length;
}

// The note beside a hat's name ("do once"). In the block list, while the program holds that hat,
// the note turns into the design's "✓ placed" badge.
class FieldPill extends Blockly.FieldLabel {
  private tick: SVGPathElement | null = null;

  private get placed() {
    const block = this.getSourceBlock();
    return !!block && isPlacedInProgram(block);
  }

  override initView() {
    super.initView();
    this.createBorderRect_();
    if (!this.fieldGroup_ || !this.borderRect_) return;
    // The text is drawn after the background so it stays on top.
    this.fieldGroup_.appendChild(this.getTextElement());
    Blockly.utils.dom.addClass(this.borderRect_, "sbPillRect");
    if (!this.placed) return;
    Blockly.utils.dom.addClass(this.fieldGroup_, "sbPillPlaced");
    this.tick = Blockly.utils.dom.createSvgElement(Blockly.utils.Svg.PATH, { d: TICK_PATH }, this.fieldGroup_);
    Blockly.utils.dom.addClass(this.tick, "sbPillTick");
  }

  protected override getDisplayText_() {
    return this.placed ? Blockly.Msg.SB_HAT_PLACED : super.getDisplayText_();
  }

  protected override updateSize_() {
    const textWidth = this.textElement_ ? Blockly.utils.dom.getTextWidth(this.textElement_) : 0;
    const lead = this.tick ? TICK_SIZE + TICK_GAP : 0;
    this.size_ = new Blockly.utils.Size(PILL_PADDING * 2 + lead + textWidth, PILL_HEIGHT);
    this.positionTextElement_(PILL_PADDING + lead, textWidth);
    this.positionBorderRect_();
    const scale = TICK_SIZE / 24;
    this.tick?.setAttribute(
      "transform",
      `translate(${PILL_PADDING}, ${(PILL_HEIGHT - TICK_SIZE) / 2}) scale(${scale})`,
    );
  }

  protected override positionBorderRect_() {
    super.positionBorderRect_();
    this.borderRect_?.setAttribute("rx", String(PILL_HEIGHT / 2));
    this.borderRect_?.setAttribute("ry", String(PILL_HEIGHT / 2));
  }
}

if (!Blockly.registry.hasItem(Blockly.registry.Type.FIELD, PILL_FIELD))
  Blockly.fieldRegistry.register(PILL_FIELD, FieldPill);
