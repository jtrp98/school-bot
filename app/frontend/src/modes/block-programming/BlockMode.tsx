import { compile } from "./blockCompiler";
import { BlockWorkspacePanel } from "../../components/block-programming/BlockWorkspacePanel";
import { BlockResultPanel } from "../../components/block-programming/BlockResultPanel";
import { useEffect, useRef, useState } from "react";
import * as Blockly from "blockly/core";
import * as englishMessages from "blockly/msg/en";
import * as thaiMessages from "blockly/msg/th";
import * as chineseMessages from "blockly/msg/zh-hans";
import { useTranslation } from "react-i18next";
import i18n, { t as translate } from "../../i18n";
import { client } from "../../core/client";

// Blockly's CJS declarations include a default export that its ESM bundle omits.
Blockly.setLocale(
  Object.fromEntries(
    Object.entries(
      i18n.resolvedLanguage === "zh"
        ? chineseMessages
        : i18n.resolvedLanguage === "en"
          ? englishMessages
          : thaiMessages,
    ).filter(([, value]) => typeof value === "string"),
  ) as Record<string, string>,
);
if (!Blockly.Blocks["arm_move"])
  Blockly.defineBlocksWithJsonArray([
    {
      type: "arm_move",
      message0: "move %1 %2 tiles",
      args0: [
        {
          type: "field_dropdown",
          name: "DIRECTION",
          options: [
            ["right", "move_right"],
            ["left", "move_left"],
          ],
        },
        {
          type: "field_number",
          name: "TILES",
          value: 2,
          min: 1,
          max: 10,
          precision: 1,
        },
      ],
      previousStatement: null,
      nextStatement: null,
      colour: 158,
    },
    {
      type: "arm_jump",
      message0: "jump",
      previousStatement: null,
      nextStatement: null,
      colour: 25,
    },
    {
      type: "arm_repeat",
      message0: "repeat %1 times %2 %3",
      args0: [
        {
          type: "field_number",
          name: "TIMES",
          value: 2,
          min: 1,
          max: 10,
          precision: 1,
        },
        { type: "input_dummy" },
        { type: "input_statement", name: "DO" },
      ],
      previousStatement: null,
      nextStatement: null,
      colour: 220,
    },
  ]);

const introClass = [
  "intro flex items-center justify-between mb-[24px] [&_p]:text-muted [&_p]:text-[13px]",
  "[@media(max-width:_1100px)]:gap-[16px] [@media(max-width:_850px)]:items-start",
  "[@media(max-width:_850px)]:flex-col [@media(max-width:_850px)]:mb-[16px]",
  "[@media(max-width:_850px)]:gap-0",
].join(" ");
const pillClass = [
  "pill text-[10px] font-bold tracking-[1px] bg-blue-tint border-1 border-solid border-blue-line",
  "text-blue-press rounded-[6px] py-[5px] px-[8px] whitespace-nowrap [&.mint]:bg-ok-tint",
  "[&.mint]:text-ok-ink [&.mint]:border-ok-line mint",
].join(" ");
const mutedNoteClass = "muted text-muted text-[12px]";

function localizeWorkspace(workspace: Blockly.WorkspaceSvg) {
  const labels: Record<string, string[]> = {
    arm_move: ["blocks.move", "blocks.tiles"],
    arm_jump: ["blocks.jump"],
    arm_repeat: ["blocks.repeat", "blocks.times"],
  };
  const blocks = [
    ...workspace.getAllBlocks(false),
    ...(workspace.getFlyout()?.getWorkspace().getAllBlocks(false) ?? []),
  ];
  for (const block of blocks) {
    block.inputList
      .flatMap(input => input.fieldRow)
      .filter(field => field instanceof Blockly.FieldLabel)
      .forEach((field, index) => {
        const key = labels[block.type]?.[index];
        if (key) field.setValue(translate(key));
      });
    const direction = block.getField("DIRECTION");
    if (direction instanceof Blockly.FieldDropdown) {
      const value = direction.getValue();
      direction.setOptions([
        [translate("blocks.direction.right"), "move_right"],
        [translate("blocks.direction.left"), "move_left"],
      ]);
      direction.setValue(value);
    }
  }
}

export default function BlockMode() {
  const { i18n: language } = useTranslation();
  const container = useRef<HTMLDivElement>(null);
  const workspace = useRef<Blockly.WorkspaceSvg | null>(null);
  const [status, setStatus] = useState(
    "common.messages.dragBlocksFromTheToolboxConnectThemIntoOne",
  );
  const [result, setResult] = useState("");
  const [busy, setBusy] = useState(false);
  useEffect(() => {
    const instance = Blockly.inject(container.current!, {
      toolbox: {
        kind: "flyoutToolbox",
        contents: [
          { kind: "block", type: "arm_move" },
          { kind: "block", type: "arm_jump" },
          { kind: "block", type: "arm_repeat" },
        ],
      },
      media: "./blockly-media/",
      trashcan: true,
      scrollbars: true,
      zoom: { controls: true, wheel: true, startScale: 0.9 },
      grid: { spacing: 24, length: 2, colour: "var(--line-strong)", snap: true },
      maxBlocks: 1000,
    });
    workspace.current = instance;
    const resize = new ResizeObserver(() => Blockly.svgResize(instance));
    resize.observe(container.current!);
    const block = instance.newBlock("arm_move");
    block.initSvg();
    block.render();
    block.moveBy(240, 64);
    const localizeCreatedBlocks = (event: Blockly.Events.Abstract) => {
      if (event.type === Blockly.Events.BLOCK_CREATE) localizeWorkspace(instance);
    };
    instance.addChangeListener(localizeCreatedBlocks);
    return () => {
      resize.disconnect();
      instance.removeChangeListener(localizeCreatedBlocks);
      instance.dispose();
      workspace.current = null;
    };
  }, []);
  useEffect(() => {
    const messages = language.resolvedLanguage === "zh"
      ? chineseMessages
      : language.resolvedLanguage === "en"
        ? englishMessages
        : thaiMessages;
    Blockly.setLocale(
      Object.fromEntries(
        Object.entries(messages).filter(([, value]) => typeof value === "string"),
      ) as Record<string, string>,
    );
    if (workspace.current) localizeWorkspace(workspace.current);
  }, [language.resolvedLanguage]);
  async function run() {
    if (!workspace.current) return;
    setBusy(true);
    try {
      const commands = compile(workspace.current);
      const response = await client.run("block-programming", "program.run", {
        commands,
      });
      setResult(JSON.stringify(response.data, null, 2));
      setStatus(
        "blocks.run.result",
      );
    } catch (error) {
      setStatus(String(error));
    } finally {
      setBusy(false);
    }
  }
  async function save() {
    try {
      if (!workspace.current) return;
      await client.save("block-programming", {
        schema: 1,
        workspace: Blockly.serialization.workspaces.save(workspace.current),
      });
      setStatus("common.messages.blockProjectSavedLocally");
    } catch (error) {
      setStatus(String(error));
    }
  }
  async function load() {
    try {
      const value = (await client.load("block-programming")) as {
        schema?: number;
        workspace?: object;
      } | null;
      if (!value) {
        setStatus("common.messages.noSavedBlockProjectYet");
        return;
      }
      if (value.schema !== 1 || !value.workspace || !workspace.current)
        throw new Error("Invalid saved project");
      if (new TextEncoder().encode(JSON.stringify(value.workspace)).length > 262144)
        throw new Error("Block workspace exceeds 256 KiB");
      const candidate = new Blockly.Workspace();
      try {
        Blockly.serialization.workspaces.load(value.workspace, candidate);
        if (candidate.getAllBlocks(false).length > 1000) throw new Error("Workspace exceeds 1000 blocks");
        compile(candidate);
      } finally { candidate.dispose(); }
      Blockly.serialization.workspaces.load(value.workspace, workspace.current);
      setStatus("common.messages.savedBlockProjectLoaded");
    } catch (error) {
      setStatus(String(error));
    }
  }
  return (
    <>
      <div className={introClass}>
        <p>{translate("blocks.messages.thinkItConnectItTryIt")}</p>
        <span className={pillClass}>{translate("blocks.messages.localWorkspaceBlockly")}</span>
      </div>
      <BlockWorkspacePanel container={container} busy={busy} save={save} load={load} run={run} />
      <BlockResultPanel status={status} result={result} />
      <p className={mutedNoteClass}>
        {translate("blocks.workspace.scope")}
      </p>
    </>
  );
}
