import { t as translate } from "../../i18n";
import { Button, Modal, RobotChip, Toast } from "../shared";
import type { useLearnMapScreen } from "../../modes/coding-adventure/LearnMap";
import { SchoolBotMascot } from "../shared/SchoolBotMascot";
import { SAMPLE_ROBOT_NAME } from "../../shell";

const runScrimClass = "sb-scrim absolute inset-0 bg-[rgba(var(--ink-rgb),.45)] flex items-center justify-center z-[40]";

const runModalClass =
  "sb-modal bg-surface rounded-modal shadow-pop p-[28px] flex flex-col border-2 border-solid border-line " +
  "w-[560px] max-w-[calc(100%_-_32px)] gap-[18px]";

const progressNoticeClass =
  "lm-proto [align-self:flex-start] bg-orange-tint text-orange-deep border-2 border-dashed border-orange-deep " +
  "rounded-full py-[3px] px-[14px] not-italic [font-variant:normal] font-extrabold [font-stretch:normal] " +
  "text-[15px] leading-[1.4] font-body";

const eyebrowClass =
  "lm-eyebrow not-italic [font-variant:normal] font-extrabold [font-stretch:normal] text-[16px] leading-[1.3] " +
  "font-body text-blue-ink";

const runTitleClass =
  "lm-runtitle not-italic [font-variant:normal] font-extrabold [font-stretch:normal] text-[28px] leading-[1.3] " +
  "font-heading";

const runBodyClass =
  "lm-runbody not-italic [font-variant:normal] font-medium [font-stretch:normal] text-[18px] leading-[1.55] " +
  "font-body text-muted";

const bigBtnClass =
  "lm-bigbtn [&.lm-bigbtn]:min-h-[64px] [&.lm-bigbtn]:text-[22px] [&.lm-bigbtn]:rounded-[14px]";

const noRobotHintClass =
  "sb-t-body not-italic [font-variant:normal] font-normal [font-stretch:normal] text-[16px] leading-[1.5] " +
  "font-body sb-muted text-muted";

const howToClass =
  "lm-howto flex items-center gap-[14px] py-[12px] px-[14px] rounded-[14px] bg-sunken border-2 border-dashed " +
  "border-line-strong [&_.t]:not-italic [&_.t]:[font-variant:normal] [&_.t]:font-bold [&_.t]:[font-stretch:normal] " +
  "[&_.t]:text-[17px] [&_.t]:leading-[1.4] [&_.t]:font-body [&_.b]:not-italic [&_.b]:[font-variant:normal] " +
  "[&_.b]:font-medium [&_.b]:[font-stretch:normal] [&_.b]:text-[17px] [&_.b]:leading-[1.5] [&_.b]:font-body";

const connectRowLabelClass =
  "sb-t-sm not-italic [font-variant:normal] font-normal [font-stretch:normal] text-[14px] leading-[1.45] " +
  "font-body sb-muted text-muted";

const toastLayerClass =
  "lm-toast absolute left-0 right-0 bottom-[28px] flex justify-center z-[55] pointer-events-none " +
  "[&_.sb-toast]:[pointer-events:auto]";

type Props = Pick<ReturnType<typeof useLearnMapScreen>,
  "setRun"
  | "conn"
  | "noRobot"
  | "setNoRobot"
  | "toast"
  | "s"
  | "ps"
  | "showRun"
  | "finishRun"
  | "connectRobot"
>;

export function LessonDialogs({
  setRun, conn, noRobot, setNoRobot, toast, s, ps, showRun, finishRun, connectRobot,
}: Props) {

  return (<>
{showRun && s && (
        <div className={runScrimClass}>
          <div className={runModalClass} role="dialog" aria-modal="true" aria-labelledby="lm-run-title" >
            <span className={progressNoticeClass}>{translate("lessons.run.progressNotice")}</span>
            <div className={"sb-row flex items-center gap-[18px]"} >
              <SchoolBotMascot w={84} h={120} />
              <div className={"sb-col flex flex-col gap-[4px]"} >
                <span className={eyebrowClass}>
                  {ps === "done"
                    ? translate("lessons.messages.playAgain")
                    : translate("lessons.messages.missionStarted")}
                </span>
                <span className={runTitleClass} id="lm-run-title">{translate(s.t)}</span>
              </div>
            </div>
            <div className={runBodyClass}>
              {translate(
              ps === "done" ? "lessons.messages.missionScreenIntroReplay" : "lessons.messages.missionScreenIntro",
              { kind: translate(s.kind) },
            )}
            </div>
            <div className={"sb-col flex flex-col gap-[10px]"} >
              <Button variant="ok" icon="check" block className={bigBtnClass} autoFocus onClick={finishRun}>
                {ps === "done"
                  ? translate("lessons.messages.finishThisRound")
                  : translate("lessons.messages.completeMissionDemo")}
              </Button>
              <Button variant="secondary" size="md" icon="map" block onClick={() => setRun(false)}>
                {translate("lessons.messages.backToMap")}
              </Button>
            </div>
          </div>
        </div>
      )}
{noRobot && (
        <Modal
          icon="plug"
          tone="warn"
          width={580}
          title={translate("modal.robotRequired.title")}
          titleId="lm-norobot-title"
          actions={
            <>
              <Button
                variant="secondary"
                size="md"
                disabled={conn === "connecting"}
                onClick={() => setNoRobot(false)}
              >
                {translate("common.messages.notNow")}
              </Button>
              <Button size="md" icon="plug" loading={conn === "connecting"} autoFocus onClick={connectRobot}>
                {translate(conn === "connecting"
                  ? "robot.workspace.messages.connecting"
                  : "common.messages.chooseRobot")}
              </Button>
            </>
          }
        >
          <div className={noRobotHintClass}>
            {translate("lessons.messages.thisLevelNeedsARealRobotConnectYourGroup")}
          </div>
          <div className={howToClass}>
            <SchoolBotMascot w={56} h={80} />
            <div className={"sb-grow [flex:1] min-w-0 sb-col flex flex-col gap-[6px]"} >
              <span className="t">{translate("lessons.messages.followTheseSteps")}</span>
              <span className="b">
                {translate("lessons.messages.1ConnectTheRobotSUsbCable")}<br />
                {translate("lessons.messages.2PressChooseRobotAndSelectYourGroupS")}
              </span>
            </div>
          </div>
          <div className={"sb-row flex items-center gap-[10px]"} >
            <span className={connectRowLabelClass}>{translate("common.messages.currentRobot")}</span>
            <RobotChip
              state={conn === "connecting"
                ? "busy"
                : conn === "connected"
                  ? "connected"
                  : "off"}
              name={SAMPLE_ROBOT_NAME}
              onClick={connectRobot}
            />
          </div>
        </Modal>
      )}
{toast && (
        <div className={toastLayerClass}>
          <Toast>{translate(toast)}</Toast>
        </div>
      )}
</>);
}
