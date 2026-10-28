import { t as translate } from "../../../../i18n";
import type { ReactNode } from "react";
import { T } from "../tplUtil";
import type { DialogActions } from "../workspaceDialogActions";
import type { MergeDialogView } from "../workspaceDialogFields";

const PANE_ROW_CLASS = "flex items-center gap-[8px] border-b-1 [border-bottom-style:solid] border-b-line px-[12px] py-[8px]";
const PATH_BOX_CLASS = "box-border flex h-[28px] items-center rounded-[4px] border-1 border-solid px-[8px]";

const SCRIM_CLASS = [
  "absolute inset-0 z-[70] flex items-center justify-center bg-[rgba(var(--ink-rgb),_0.25)]",
  "font-[family-name:'Segoe_UI',_'Leelawadee_UI',_Tahoma,_sans-serif]",
].join(" ");

const WINDOW_CLASS = [
  "box-border flex h-[500px] w-[780px] flex-col overflow-hidden rounded-[8px] border-1 border-solid border-muted",
  "bg-white text-[13px] text-ink shadow-pop",
].join(" ");

const SIDEBAR_CLASS = [
  "flex w-[190px] shrink-0 flex-col gap-[1px] bg-sunken px-[4px] py-[6px]",
  "border-r-1 [border-right-style:solid] border-r-line",
].join(" ");

const FILE_ROW_CLASS = [
  "flex h-[28px] w-full cursor-default items-center border-1 border-solid px-[10px] text-left",
  "font-[family-name:inherit] text-[13px] text-ink",
].join(" ");

export function MergeProjectDialog({ merge, actions }: { merge: MergeDialogView; actions: DialogActions }): ReactNode {
  const canOpen = merge.selectedFileName !== "";
  return (
    <div className={SCRIM_CLASS}>
      <div
        role="dialog"
        aria-label={translate("robot.workspace.messages.openChooseAProjectToAppendItsPoses")}
        className={WINDOW_CLASS}
      >
        <div className="flex h-[34px] items-center bg-sunken pl-[12px] border-b-1 [border-bottom-style:solid] border-b-line">
          <span className="[flex:1_1_0] text-[12px]">{translate("robot.workspace.messages.open")}</span>
          <button
            type="button"
            onClick={actions.closeMerge}
            aria-label={translate("common.messages.close")}
            className="h-[34px] w-[46px] cursor-pointer border-none bg-transparent font-[family-name:inherit] text-[14px]"
          >
            ✕
          </button>
        </div>
        <div className={PANE_ROW_CLASS}>
          <span className="text-muted">←  →  ↑</span>
          <span className={`${PATH_BOX_CLASS} [flex:1_1_0] gap-[6px] border-line`}>{T(merge.breadcrumb)}</span>
          <span className={`${PATH_BOX_CLASS} w-[200px] border-line text-muted`}>
            {translate("robot.workspace.messages.search")} {T(merge.folderName)}
          </span>
        </div>
        <div className="flex min-h-0 [flex:1_1_0]">
          <div className={SIDEBAR_CLASS}>
            {merge.folders.map((folder, index) => (
              <button
                key={index}
                type="button"
                onClick={() => actions.pickMergeFolder(index)}
                className={`h-[28px] cursor-pointer rounded-[4px] border-none px-[8px] text-left font-[family-name:inherit] text-[13px] text-ink ${
                  folder.selected ? "bg-blue-line" : "bg-transparent"
                }`}
              >
                {T(folder.icon)} {T(folder.name)}
              </button>
            ))}
          </div>
          <div className="flex min-w-0 [flex:1_1_0] flex-col">
            <div className="flex h-[28px] items-center px-[10px] text-[12px] text-muted border-b-1 [border-bottom-style:solid] border-b-line">
              <span className="[flex:1_1_0]">{translate("robot.workspace.messages.name")}</span>
              <span className="w-[150px]">{translate("robot.workspace.messages.modifiedDate")}</span>
              <span className="w-[80px]">{translate("robot.workspace.messages.type")}</span>
              <span className="w-[60px] text-right">{translate("robot.workspace.messages.size")}</span>
            </div>
            <div className="overflow-auto px-0 py-[2px] [flex:1_1_0]">
              {merge.files.map((file) => (
                <button
                  key={file.id}
                  type="button"
                  onClick={() => actions.pickMergeFile(file.id)}
                  onDoubleClick={() => actions.openMergeFile(file.id)}
                  aria-pressed={file.selected}
                  className={`${FILE_ROW_CLASS} ${
                    file.selected ? "border-blue-line bg-blue-tint" : "border-transparent bg-transparent"
                  }`}
                >
                  <span className="flex [flex:1_1_0] items-center gap-[6px] overflow-hidden whitespace-nowrap">
                    <span aria-hidden="true">📄</span>
                    {T(file.name)}
                  </span>
                  <span className="w-[150px] text-muted">{T(file.date)}</span>
                  <span className="w-[80px] text-muted">{translate("robot.workspace.messages.iniFile")}</span>
                  <span className="w-[60px] text-right text-muted">{T(file.size)}</span>
                </button>
              ))}
              {merge.files.length === 0 ? (
                <div className="p-[24px] text-center text-muted">
                  {translate("robot.workspace.messages.noItemsMatchYourSearch")}
                </div>
              ) : null}
            </div>
          </div>
        </div>
        <div className="flex items-center gap-[8px] bg-sunken px-[12px] py-[10px] border-t-1 [border-top-style:solid] border-t-line">
          <span className="w-[70px] text-right">{translate("robot.workspace.messages.fileName")}</span>
          <span className={`${PATH_BOX_CLASS} [flex:1_1_0] border-faint bg-white`}>{T(merge.selectedFileName)}</span>
          <span className={`${PATH_BOX_CLASS} w-[200px] border-faint bg-white`}>
            {translate("robot.workspace.messages.schoolBotProjectsIni")}
          </span>
        </div>
        <div className="flex justify-end gap-[8px] bg-sunken px-[12px] pb-[12px]">
          <button
            type="button"
            onClick={actions.confirmMerge}
            disabled={!canOpen}
            className={`h-[30px] w-[90px] rounded-[4px] border-1 border-solid font-[family-name:inherit] text-[13px] ${
              canOpen
                ? "cursor-pointer border-blue-press bg-blue-ink text-surface"
                : "cursor-default border-line bg-sunken text-faint"
            }`}
          >
            {translate("common.open")}
          </button>
          <button
            type="button"
            onClick={actions.closeMerge}
            className="h-[30px] w-[90px] cursor-pointer rounded-[4px] border-1 border-solid border-faint bg-sunken font-[family-name:inherit] text-[13px]"
          >
            {translate("common.cancel")}
          </button>
        </div>
        <div className="bg-orange-tint px-[12px] py-[6px] text-[12px] text-orange-deep border-t-1 [border-top-style:solid] border-t-star">
          {translate("robot.workspace.messages.simulationSystemFilePickerPressOpenToAppendAll")}
        </div>
      </div>
    </div>
  );
}
