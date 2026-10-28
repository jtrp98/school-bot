import { Button } from "../../../components/shared";

import { t as translate } from "../../../i18n";

import type { SimulatorView } from "./SimulatorView";

const armChipClass =
  "arm-chip h-[52px] flex items-center gap-[12px] py-0 px-[14px] border-2 border-solid border-line " +
  "rounded-[16px] bg-white";

const stageCardClass =
  "stage-card relative flex flex-col items-center pt-[14px] px-[14px] pb-[16px] rounded-[22px] " +
  "bg-[radial-gradient(_120%_90%_at_50%_30%,_#f2f8ff_0%,_#dcebfb_70%,_#cfe2f8_100%_)] [&_svg]:block [&_svg]:w-full " +
  "[&_svg]:max-h-[360px]";

const restBtnClass =
  "rest-btn absolute top-[10px] right-[10px] z-[2] min-h-[30px] h-[30px] py-0 px-[10px] text-[13px] rounded-[10px]";

const poseStripClass =
  "pose-strip flex items-center gap-[10px] mt-[6px] p-[5px] bg-white rounded-full " +
  "shadow-[0_6px_18px_rgba(24,_102,_201,_0.18)]";

const roundBtnClass =
  "round-btn w-[44px] h-[44px] min-h-0 p-0 shrink-0 rounded-full text-[20px] font-extrabold leading-none " +
  "[&.small]:w-[34px] [&.small]:h-[34px] [&.small]:text-[13px]";

const poseStripNameClass =
  "pose-strip-name flex flex-col items-center min-w-[200px] leading-[1.25] py-0 px-[4px] [&_b]:max-w-[260px] " +
  "[&_b]:overflow-hidden [&_b]:text-ellipsis [&_b]:whitespace-nowrap [&_b]:text-[18px] [&_span]:text-[12px] " +
  "[&_span]:font-semibold [&_span]:text-muted";

const playBarClass =
  "play-bar relative flex flex-col gap-[10px] p-[12px] bg-white border-2 border-solid border-line rounded-[18px]";

const playProgressClass =
  "play-progress absolute left-0 right-0 top-[-2px] h-[4px] bg-blue-line rounded-[18px_18px_0_0] overflow-hidden " +
  "[&_>_div]:h-full [&_>_div]:bg-blue [&_>_div]:[transition:width_0.1s_linear]";

const lookCheckClass =
  "look-check flex items-center gap-[10px] text-[14px] cursor-pointer [&_input]:w-[22px] [&_input]:h-[22px] " +
  "[&_input]:[accent-color:var(--blue)]";

const loopsClass =
  "loops inline-flex items-center gap-[6px] text-[14px] font-semibold [&_b]:min-w-[14px] [&_b]:text-center " +
  "[&_b]:text-[17px]";

const stopBtnClass =
  "stop-btn h-[56px] py-0 px-[22px] border-[0] border-none border-current border-b-[5px] " +
  "[border-bottom-style:solid] border-b-err-press rounded-[16px] bg-err text-white text-[18px] hover:bg-err";

const playBtnClass =
  "primary bg-orange text-white border-orange border-b-orange-press hover:bg-orange hover:[filter:brightness(1.07)] " +
  "hover:border-orange hover:border-b-orange-press disabled:bg-off disabled:text-faint disabled:border-off " +
  "disabled:border-b-line-strong disabled:[filter:none] disabled:opacity-[1] play-btn [flex:1] h-[56px] " +
  "rounded-[16px] border-b-[5px] text-[18px]";

type Props = {
  v: Pick<Parameters<typeof SimulatorView>[0]["v"],
    "poses" | "sel" | "busy" | "playing" | "stopped" | "looked" | "setLooked" | "loops" | "setLoops"
    | "shown" | "atRobot" | "go" | "play" | "stop" | "goRest" | "a" | "ready">;
};

export function SimulatorStage({ v }: Props) {
 const {
   poses, sel, busy, playing, stopped, looked, setLooked, loops, setLoops, shown, atRobot, go, play, stop, goRest,
   a, ready,
 } = v;
 return (<>
<aside
  className={"arm-left flex flex-col gap-[10px] sticky top-[16px] [@media(max-width:_1100px)]:[position:static]"}
  aria-label={translate("common.messages.robotAndPlayback")}
>
          <div className={armChipClass}>
            <span className={"chip-name text-[15px] font-bold [&_.muted]:font-medium [&_.muted]:text-[13px]"}>
              {translate("robot.simulator.messages.simulatedRobot")}{" "}
              <span className={"muted text-muted text-[12px]"}>
                {translate("robot.simulator.messages.noUsbNeeded")}
              </span>
            </span>
            <span className={"chip-ok ml-auto inline-flex items-center text-[13px] font-semibold text-ok-ink"}>
              <i className={"status-dot w-[7px] h-[7px] inline-block rounded-[50%] mr-[6px] bg-ok"} />
              {stopped ? translate("robot.simulator.messages.stopped") : translate("robot.simulator.messages.ready")}
            </span>
          </div>
          <div className={stageCardClass}>
            <Button
              variant="custom"
              type="button"
              className={restBtnClass}
              disabled={busy || !looked}
              title={looked
                ? translate("common.messages.restPoseTheRobotSPowerOnPose")
                : translate("robot.simulator.messages.checkAreaAroundRobotIsClearFirst")}
              onClick={goRest}
            >{translate("common.messages.returnToRestPose")}</Button>
            <svg
              viewBox="0 0 600 400"
              role="img"
              aria-label={translate("robot.simulator.messages.robotArmInPoseValue0", {value0: shown.name})}
            >
              <ellipse cx="300" cy="348" rx="135" ry="17" fill="#1866c9" opacity=".12" />
              <path d="M80 350H520" stroke="#a9c4e6" strokeWidth="2" />
              <rect x="247" y="316" width="106" height="30" rx="10" fill="#1f3a5f" />
              <rect x="270" y="290" width="60" height="32" rx="8" fill="#3a5576" />
              <g transform={`translate(300 291) rotate(${(a[0] - 90) * 0.45})`}>
                <g transform={`rotate(${(a[1] - 90) * 0.55})`}>
                  <rect x="-15" y="-129" width="30" height="129" rx="15" fill="#5aa0f2" />
                  <circle r="21" fill="#14243d" />
                  <circle r="9" fill="#c6d6ea" />
                  <g transform={`translate(0 -120) rotate(${(a[2] - 90) * 0.7})`}>
                    <rect x="-13" y="-108" width="26" height="108" rx="13" fill="#1866c9" />
                    <circle r="18" fill="#14243d" />
                    <circle r="7" fill="#c6d6ea" />
                    <g transform="translate(0 -101)">
                      <rect x="-17" y="-20" width="34" height="27" rx="7" fill="#1f3a5f" />
                      <path
                        d={`M-12 -15 L-${16 + a[3] / 6} -43 M12 -15 L${16 + a[3] / 6} -43`}
                        stroke="#1f3a5f"
                        strokeWidth="9"
                        strokeLinecap="round"
                      />
                    </g>
                  </g>
                </g>
              </g>
            </svg>
            <div className={poseStripClass}>
              <Button
                variant="custom"
                type="button"
                className={roundBtnClass}
                aria-label={translate("common.messages.previousPose")}
                disabled={sel === 0 || busy}
                onClick={() => go(-1)}
              >
                ‹
              </Button>
              <span className={poseStripNameClass}>
                <b>{shown.name}</b>
                <span>
                  {sel === 0
                    ? translate("robot.simulator.messages.initialPose")
                    : translate("robot.simulator.messages.poseValue0Value1", {
                      value0: sel,
                      value1: poses.length,
                    })}
                  {" · "}
                  {atRobot
                    ? translate("robot.simulator.messages.robotIsAtThisPose")
                    : translate("robot.simulator.messages.previewOnlyRobotHasNotMoved")}
                </span>
              </span>
              <Button
                variant="custom"
                type="button"
                className={roundBtnClass}
                aria-label={translate("common.messages.nextPose")}
                disabled={sel >= poses.length || busy}
                onClick={() => go(1)}
              >
                ›
              </Button>
            </div>
          </div>
          <div
            className={playBarClass}
            role="region"
            aria-label={translate("common.messages.playAndStop")}
          >
            {playing !== undefined && (
              <div
                className={playProgressClass}
                role="progressbar"
                aria-label={translate("common.messages.playbackProgress")}
                aria-valuemin={0}
                aria-valuemax={poses.length}
                aria-valuenow={playing}
              >
                <div style={{ width: `${(playing / poses.length) * 100}%` }} />
              </div>
            )}
            <label className={lookCheckClass}>
              <input
                type="checkbox"
                checked={looked}
                onChange={(event) => setLooked(event.target.checked)}
              />
              <span>
                <strong>{translate("common.messages.beforeTheRobotMoves")}</strong>
                {" "}
                {translate("common.messages.areaAroundRobotIsClearOfHandsAndObjects")}
              </span>
            </label>
            <div className={"play-row flex items-center gap-[10px]"}>
              <span className={loopsClass}>{translate("common.messages.repeat")}<Button
                variant="custom"
                type="button"
                className={`${roundBtnClass} small`}
                aria-label={translate("common.messages.fewerRepetitions")}
                disabled={busy || loops <= 1}
                onClick={() => setLoops(loops - 1)}
              >
                  −
                </Button>
                <b aria-live="polite">{loops}</b>
                <Button
                  variant="custom"
                  type="button"
                  className={`${roundBtnClass} small`}
                  aria-label={translate("common.messages.moreRepetitions")}
                  disabled={busy || loops >= 9}
                  onClick={() => setLoops(loops + 1)}
                >
                  +
                </Button>{translate("common.messages.times")}</span>
              <Button
                variant="custom"
                type="button"
                className={stopBtnClass}
                onClick={stop}
              >
                {translate("robot.simulator.messages.stop")}
              </Button>
              <Button
                variant="custom"
                type="button"
                className={playBtnClass}
                disabled={!ready || poses.length === 0}
                title={looked ? undefined : translate("robot.simulator.messages.checkAreaAroundRobotIsClearFirst")}
                onClick={play}
              >
                {playing !== undefined
                  ? translate("robot.simulator.messages.playingPoseValue0", {value0: playing})
                  : translate("robot.simulator.messages.playValue0Poses", {value0: poses.length})}
              </Button>
            </div>
          </div>
        </aside>
</>);
}
