import { useEffect, useRef } from "react";
import { useTranslation } from "react-i18next";
import * as Blockly from "blockly/core";
import { t as translate } from "../../i18n";
import { applyBlocklyLocale } from "./blockly/locale";
import { BLOCK_LAB_RENDERER } from "./blockly/renderer";
import { defineSheetBlocks, fillSheet } from "./blockly/sheetBlocks";
import { blockLabTheme } from "./blockly/theme";

const sheetClass = "h-[960px] bg-surface border-2 border-solid border-line rounded-card overflow-hidden";

// Design check sheet for the Block Lab renderer. BlockMode loads it only in development builds.
export default function BlockSheet() {
  const { i18n } = useTranslation();
  const container = useRef<HTMLDivElement>(null);
  useEffect(() => {
    // Block text is resolved when a block is created, so the locale must be set first and
    // the sheet rebuilt when the language changes.
    applyBlocklyLocale(i18n.resolvedLanguage);
    defineSheetBlocks();
    const workspace = Blockly.inject(container.current!, {
      renderer: BLOCK_LAB_RENDERER,
      theme: blockLabTheme,
      readOnly: true,
      media: "./blockly-media/",
      move: { scrollbars: true, drag: true, wheel: true },
      grid: { spacing: 24, length: 1, colour: "var(--line)" },
    });
    fillSheet(workspace);
    workspace.scroll(24, 24);
    const resize = new ResizeObserver(() => Blockly.svgResize(workspace));
    resize.observe(container.current!);
    return () => {
      resize.disconnect();
      workspace.dispose();
    };
  }, [i18n.resolvedLanguage]);
  return (
    <div
      ref={container}
      className={sheetClass}
      aria-label={translate("blocks.messages.visualProgrammingWorkspace")}
    />
  );
}
