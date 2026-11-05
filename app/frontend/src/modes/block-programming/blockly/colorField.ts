import * as Blockly from "blockly/core";
import i18n from "../../../i18n";
import { hexOf, isMixedColor, rgbOf } from "./colorValue";
import { requestMenu } from "./fieldMenu";

export const COLOR_FIELD = "sb_color";

const SWATCH = 16;
const SWATCH_GAP = 6;
const SWATCH_SHIFT = `translate(${SWATCH + SWATCH_GAP}, 0)`;

// The colour block's field: the generator's colour names as its options, plus any colour the
// child mixed on the wheel or by numbers, shown as its numbers. A swatch in front shows the colour.
class FieldColor extends Blockly.FieldDropdown {
  private swatch: SVGRectElement | null = null;

  protected override doClassValidation_(newValue?: string): string | null {
    if (newValue !== undefined && isMixedColor(newValue)) return newValue;
    return super.doClassValidation_(newValue);
  }

  protected override getText_() {
    const value = this.getValue();
    if (value === null || !isMixedColor(value)) return super.getText_();
    return i18n.t("blockLab.colorPicker.mixedName", rgbOf(value));
  }

  override initView() {
    super.initView();
    if (!this.fieldGroup_) return;
    this.swatch = Blockly.utils.dom.createSvgElement(
      Blockly.utils.Svg.RECT,
      { width: SWATCH, height: SWATCH, rx: 5, ry: 5, class: "sbColorSwatch" },
      this.fieldGroup_,
    );
  }

  protected override render_() {
    super.render_();
    if (!this.swatch || !this.fieldGroup_) return;
    // Everything Blockly drew moves right to make room, keeping any placement of its own.
    for (const child of [...this.fieldGroup_.children]) {
      if (child === this.borderRect_ || child === this.swatch) continue;
      const own = (child.getAttribute("transform") ?? "").replace(SWATCH_SHIFT, "").trim();
      child.setAttribute("transform", `${SWATCH_SHIFT} ${own}`.trim());
    }
    this.size_.width += SWATCH + SWATCH_GAP;
    this.positionBorderRect_();
    const padding = this.getConstants()!.FIELD_BORDER_RECT_X_PADDING;
    this.swatch.setAttribute("x", String(padding));
    this.swatch.setAttribute("y", String((this.size_.height - SWATCH) / 2));
    // Inline, because Blockly's stylesheet paints every rect in a field white.
    this.swatch.style.fill = hexOf(this.getValue()) ?? "none";
  }

  protected override showEditor_(e?: MouseEvent) {
    if (!requestMenu(this, value => this.setValue(value), "color")) super.showEditor_(e);
  }
}

if (!Blockly.registry.hasItem(Blockly.registry.Type.FIELD, COLOR_FIELD))
  Blockly.fieldRegistry.register(COLOR_FIELD, FieldColor);
