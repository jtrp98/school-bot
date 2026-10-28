import { t as translate } from "../../../../i18n";
import { Fragment, type ReactNode } from "react";
import type { LogType } from "../logView";
import { T, arr } from "../tplUtil";
import type { WorkspaceViewProps } from "../viewTypes";

type Props = WorkspaceViewProps<
  "logOpen" | "nativeHardware" | "robotPort" | "closeLog" | "logFilters" | "logItems" | "menuActions"
>;

const TAG_TONE: Record<LogType, string> = {
  conn: "bg-blue-tint text-ink",
  pose: "bg-blue-tint text-blue-press",
  move: "bg-orange-tint text-orange-deep",
  order: "bg-blue-tint text-blue-press",
  play: "bg-ok-tint text-ok-ink",
  stop: "bg-off text-ink",
  err: "bg-err-tint text-err-ink",
  send: "bg-blue-tint text-blue-press",
  time: "bg-blue-tint text-ink",
};

function filterClass(on: boolean): string {
  const tone = on ? "border-blue-ink bg-blue-ink text-white" : "border-line bg-surface text-ink";
  return `h-[32px] py-0 px-[12px] rounded-full font-[family-name:inherit] text-[13px] font-semibold cursor-pointer `
    + `border-2 border-solid ${tone}`;
}

export function LogDrawer({ v }: Props): ReactNode {
  return (
    <>
      {v.logOpen ? (
        <>
        <aside
          aria-label={translate("robot.workspace.messages.activityHistory")}
          className="absolute top-[68px] right-[24px] bottom-[84px] w-[400px] box-border bg-white border-2
            border-solid border-line-strong rounded-[18px] shadow-pop z-[30] flex flex-col
            overflow-hidden"
        >
          <div
            className="flex items-center gap-[10px] py-[14px] px-[16px] border-b-2 [border-bottom-style:solid]
              border-b-sunken"
          >
            <span className={"flex flex-col grow"} >
              <span className={"font-[family-name:'Mitr',_'Noto_Sans_Thai',_sans-serif] text-[18px] font-medium"} >
                {translate("robot.workspace.messages.activityHistory")}
              </span>
              <span className={"text-[12px] text-muted"} >
                {v.nativeHardware
                  ? translate("robot.workspace.messages.robotArmValue0Today", { value0: v.robotPort })
                  : translate("robot.workspace.messages.group3RobotArmCom7Today")}
              </span>
            </span>
            <button
              type="button"
              onClick={v.closeLog}
              aria-label={translate("robot.workspace.messages.closeHistory")}
              className="w-[40px] h-[40px] border-2 border-solid border-line rounded-[10px] bg-white text-ink
                font-[family-name:inherit] text-[18px] cursor-pointer"
            >
              ×
            </button>
          </div>
          <div
            role="group"
            aria-label={translate("robot.workspace.messages.filterHistory")}
            className="flex gap-[6px] py-[10px] px-[16px] flex-wrap"
          >
            {arr(v.logFilters).map((filter) => (
              <Fragment key={filter.value}>
                <button
                  type="button"
                  onClick={() => v.menuActions.chooseLogFilter(filter.value)}
                  aria-pressed={filter.on}
                  className={filterClass(filter.on)}
                >
                  {T(filter.label)}
                </button>
              </Fragment>
            ))}
          </div>
          <ol
            className="m-0 pt-0 px-[16px] pb-[12px] [list-style:none] flex flex-col gap-[6px] overflow-y-auto
              grow"
          >
            {arr(v.logItems).map((l, $i0) => (
              <Fragment key={$i0}>
                <li className={"flex gap-[10px] items-start py-[8px] px-[10px] rounded-[10px] bg-sunken"} >
                  <span
                    className="text-[12px] text-muted [font-variant-numeric:tabular-nums] shrink-0 w-[40px]
                      pt-[2px]"
                  >
                    {T(l.time)}
                  </span>
                  <span
                    className={`text-[11px] font-bold py-[2px] px-[8px] rounded-full shrink-0 whitespace-nowrap
                      ${TAG_TONE[l.tone]}`}
                  >
                    {T(l.tag)}
                  </span>
                  <span className={"text-[13px] leading-[1.45] min-w-0"} >
                    {T(l.text)}
                  </span>
                </li>
              </Fragment>
            ))}
          </ol>
          <div
            className="py-[10px] px-[16px] border-t-2 [border-top-style:solid] border-t-sunken text-[11px]
              text-muted leading-[1.45]"
          >
            {translate("robot.workspace.messages.recordedByGroupAndRobotWithoutStudentNamesA")}
          </div>
        </aside>
        </>
      ) : null}
    </>
  );
}
