import { Button, IconBtn } from "../../../components/shared";
import { SimulatorStage } from "./SimulatorStage";
import { JOINTS, MAX_NAME } from "./simulatorData";
import { t as translate } from "../../../i18n";
import { NumericInput } from "../../../components/robot-control/NumericInput";
import { HoldButton } from "../../../components/robot-control/HoldButton";

import type { useArmSimulator } from "../ArmSimulator";

type Props = { v: ReturnType<typeof useArmSimulator> };

const gridClass =
  "arm6 grid [grid-template-columns:minmax(380px,_520px)_minmax(0,_1fr)] gap-[16px] [align-items:start] " +
  "[@media(max-width:_1100px)]:[grid-template-columns:1fr]";
const topGridClass =
  "arm-top grid [grid-template-columns:minmax(0,_1fr)_280px] gap-[16px] [align-items:start] " +
  "[@media(max-width:_1100px)]:[grid-template-columns:1fr]";
const panelClass =
  "arm-panel bg-white border-2 border-solid border-line rounded-[20px] pt-[12px] px-[16px] pb-[16px] " +
  "min-w-0";
const selectedPanelClass =
  "arm-panel bg-white border-2 border-solid border-line rounded-[20px] pt-[12px] px-[16px] pb-[16px] " +
  "min-w-0 selected flex flex-col gap-[10px] [&_.arm-h]:mb-0 [&_.danger]:mt-0 [&_.actions]:mt-0";
const headingClass = "arm-h text-[17px] font-semibold leading-[32px] mt-0 mx-0 mb-[6px]";
const noteClass = "arm-note mt-0 mx-0 mb-[8px] py-[8px] px-[10px] rounded-[10px] bg-sunken text-[13px] text-muted";
const jointCardBaseClass =
  "jcard flex items-center gap-[10px] min-h-[56px] py-[6px] px-[12px] border-2 border-solid border-line " +
  "rounded-[14px] bg-white focus-within:border-blue focus-within:bg-blue-tint [&.off]:opacity-[0.6] " +
  "flex-wrap gap-y-[4px]";
const jointBadgeClass =
  "jbadge shrink-0 w-[36px] h-[30px] flex items-center justify-center border-2 border-solid " +
  "border-blue-line rounded-[9px] bg-white text-blue-press text-[13px] font-bold";
const jointSliderClass = "jslider order-[9] [flex:1_1_100%] min-w-0 h-[24px] [accent-color:var(--blue)] cursor-pointer";
const roundButtonClass =
  "round-btn w-[44px] h-[44px] min-h-0 p-0 shrink-0 rounded-full text-[20px] font-extrabold " +
  "leading-none [&.small]:w-[34px] [&.small]:h-[34px] [&.small]:text-[13px] small";
const valueBoxClass =
  "valbox inline-flex items-center justify-center w-[72px] h-[40px] shrink-0 border-2 border-solid " +
  "border-blue-line rounded-[10px] bg-white font-bold [&_input]:w-[40px] [&_input]:border-[0] " +
  "[&_input]:border-none [&_input]:border-current [&_input]:[outline:0] [&_input]:bg-transparent " +
  "[&_input]:text-right [&_input]:text-ink [&_input]:font-bold focus-within:border-blue " +
  "focus-within:shadow-[0_0_0_3px_var(--blue-tint)]";
const fieldClass =
  "field flex flex-col gap-[4px] text-[12px] font-semibold text-muted [&_>_input[type='text']]:h-[40px] " +
  "[&_>_input[type='text']]:py-0 [&_>_input[type='text']]:px-[10px] [&_>_input[type='text']]:border-2 " +
  "[&_>_input[type='text']]:border-solid [&_>_input[type='text']]:border-blue-line " +
  "[&_>_input[type='text']]:rounded-[10px] [&_>_input[type='text']]:text-ink " +
  "[&_>_input[type='text']]:text-[15px] [&_>_input[type='text']]:font-semibold " +
  "[&_>_input[type='text']:disabled]:bg-sunken [&_>_input[type='text']:disabled]:border-line";
const timeRowClass =
  "time-row flex items-center gap-[6px] py-[6px] px-[8px] rounded-[12px] bg-sunken text-ink text-[13px] " +
  "[&_input]:[flex:1] [&_input]:min-w-0 [&_input]:h-[36px] [&_input]:border-2 [&_input]:border-solid " +
  "[&_input]:border-blue-line [&_input]:rounded-[10px] [&_input]:text-center [&_input]:font-bold " +
  "[&_input]:text-ink";
const primaryClass =
  "primary bg-orange text-white border-orange border-b-orange-press text-[17px] hover:bg-orange " +
  "hover:[filter:brightness(1.07)] hover:border-orange hover:border-b-orange-press disabled:bg-off " +
  "disabled:text-faint disabled:border-off disabled:border-b-line-strong disabled:[filter:none] " +
  "disabled:opacity-[1] wide w-full";
const dangerClass =
  "danger mt-[16px] text-err-ink border-err-line bg-err-tint text-[12px] hover:bg-err-tint " +
  "hover:border-err wide w-full";
const tileBaseClass =
  "tile [flex:0_0_148px] h-[112px] grid [grid-template-columns:auto_1fr] " +
  "[grid-template-rows:30px_1fr_auto] [align-items:start] [justify-items:start] gap-[4px_8px] py-[8px] " +
  "px-[10px] text-left text-ink border-2 border-solid border-blue-line border-b-4 rounded-[14px] " +
  "bg-white hover:border-blue-line hover:bg-white [&.on]:border-[3px] [&.on]:border-solid " +
  "[&.on]:border-blue [&.on]:border-b-[5px] [&.on]:bg-blue-tint " +
  "[&.playing]:shadow-[0_0_0_4px_var(--blue-tint)] [&.rest]:border-dashed [&.rest]:border-faint " +
  "[&.rest.on]:border-solid [&.rest.on]:border-muted [&.rest.on]:bg-sunken [&.rest_.tile-no]:bg-muted " +
  "[&.add]:border-[3px] [&.add]:border-dashed [&.add]:border-blue-line [&.add]:text-blue [&.add]:flex " +
  "[&.add]:flex-col [&.add]:items-center [&.add]:justify-center [&.add]:gap-[6px] [&.add]:text-[15px]";
const tileNumberClass =
  "tile-no w-[28px] h-[28px] rounded-full flex items-center justify-center bg-blue text-white " +
  "text-[13px] font-bold";
const tileNameClass =
  "tile-name [grid-column:1_/_-1] max-w-full text-[14px] font-bold leading-[1.3] [display:-webkit-box] " +
  "[-webkit-line-clamp:2] [-webkit-box-orient:vertical] overflow-hidden [word-break:break-word]";
const tileFootClass =
  "tile-foot [grid-column:1_/_-1] font-[family-name:ui-monospace,_Menlo,_Consolas,_monospace] " +
  "text-[11px] font-bold text-blue-press";
const noticeClass =
  "notice whitespace-pre-wrap [overflow-wrap:anywhere] border-2 border-solid border-blue-line " +
  "bg-blue-tint text-blue-press rounded-control py-[12px] px-[16px] mt-0 mx-0 mb-[20px] text-[13px] " +
  "[&.error]:text-err-ink [&.error]:bg-err-tint [&.error]:border-err-line";

function tileStateClass(selected: boolean, playing: boolean): string {
  return `${selected ? "on" : ""}  ${playing ? "playing" : ""}`;
}

export function SimulatorView({ v }: Props) {
  const joints = JOINTS.map((joint) => ({ sid: joint.sid, name: translate(joint.nameKey) }));
  const {
    poses, sel, setSel, status, busy, playing, looked, shown, editable, patchSelected, setJoint,
    jog, clearHold, holdStart, addPose, removePose, sendSelected, save, load, a, ready,
  } = v;
  return (
    <>
      <div className={gridClass}>
        <SimulatorStage v={v} />

        <div className={"arm-right flex flex-col gap-[16px] min-w-0"}>
          <div className={topGridClass}>
            <section className={panelClass} aria-label={translate("common.messages.moveTheArm")}>
              <h2 className={headingClass}>{translate("robot.workspace.joints.title")}</h2>
              {sel === 0 && (
                <p className={noteClass}>{translate("robot.simulator.messages.theRestPoseIsReadOnlyUseAddPose")}</p>
              )}
              <div className={"joint-list flex flex-col gap-[8px]"}>
                {joints.map((joint, index) => (
                  <div
                    className={`${jointCardBaseClass} ${editable ? "" : "off"}`}
                    key={joint.sid}
                  >
                    <span className={jointBadgeClass}>{joint.sid}</span>
                    <span className={"jname [flex:1] min-w-0 text-[14px] font-bold"}>{joint.name}</span>
                    <input
                      className={jointSliderClass}
                      type="range"
                      min="0"
                      max="180"
                      step="1"
                      aria-label={`${joint.name} (${joint.sid})`}
                      disabled={!editable}
                      value={a[index]}
                      onChange={(event) =>
                        setJoint(index, Number(event.target.value))
                      }
                    />
                    <HoldButton
                      className={roundButtonClass}
                      aria-label={translate("robot.simulator.messages.decreaseValue0", {value0: joint.name})}
                      disabled={!editable}
                      onPointerDown={() => holdStart(index, -1)}
                      onHoldEnd={clearHold}
                      onClick={(event) => event.detail === 0 && jog(index, -1)}
                    >
                      ◀
                    </HoldButton>
                    <label className={valueBoxClass}>
                      <NumericInput
                        aria-label={translate("robot.simulator.messages.value0Angle", {value0: joint.name})}
                        disabled={!editable}
                        value={a[index]}
                        onChange={(event) => {
                          const next = Number(event.target.value);
                          if (event.target.value !== "" && Number.isFinite(next))
                            setJoint(index, next);
                        }}
                      />
                      °
                    </label>
                    <HoldButton
                      className={roundButtonClass}
                      aria-label={translate("robot.simulator.messages.increaseValue0", {value0: joint.name})}
                      disabled={!editable}
                      onPointerDown={() => holdStart(index, 1)}
                      onHoldEnd={clearHold}
                      onClick={(event) => event.detail === 0 && jog(index, 1)}
                    >
                      ▶
                    </HoldButton>
                  </div>
                ))}
              </div>
            </section>

            <section className={selectedPanelClass} aria-label={translate("common.messages.selectedPose")}>
              <h2 className={headingClass}>{translate("common.messages.selectedPose")}</h2>
              <label className={fieldClass}>
                <span>{translate("common.messages.poseName")}</span>
                <input
                  type="text"
                  maxLength={MAX_NAME}
                  disabled={!editable}
                  value={shown.name}
                  onChange={(event) => patchSelected({ name: event.target.value })}
                />
              </label>
              <div className={fieldClass}>
                <span>{translate("common.messages.moveDuration")}</span>
                <span className={timeRowClass}>
                  <IconBtn
                    variant="custom"
                    type="button"
                    className={roundButtonClass}
                    label={translate("common.messages.decreaseDurationBy100Ms")}
                    disabled={!editable}
                    onClick={() =>
                      patchSelected({ ms: Math.max(100, shown.ms - 100) })
                    }
                  >
                    −
                  </IconBtn>
                  <NumericInput
                    aria-label={translate("robot.simulator.messages.moveDurationMs")}
                    disabled={!editable}
                    value={shown.ms}
                    onChange={(event) => {
                      const next = Number(event.target.value);
                      if (event.target.value !== "" && Number.isFinite(next))
                        patchSelected({
                          ms: Math.min(5000, Math.max(100, Math.round(next))),
                        });
                    }}
                  />
                  <IconBtn
                    variant="custom"
                    type="button"
                    className={roundButtonClass}
                    label={translate("common.messages.increaseDurationBy100Ms")}
                    disabled={!editable}
                    onClick={() =>
                      patchSelected({ ms: Math.min(5000, shown.ms + 100) })
                    }
                  >
                    +
                  </IconBtn>{"ms"}</span>
                <small className={"muted text-muted text-[12px]"}>
                  {translate("robot.simulator.messages.1000Ms1Second")}
                </small>
              </div>
              <Button
                variant="custom"
                type="button"
                className={primaryClass}
                disabled={!ready}
                title={looked ? undefined : translate("robot.simulator.messages.checkAreaAroundRobotIsClearFirst")}
                onClick={sendSelected}
              >
                {busy && playing === undefined
                  ? translate("robot.simulator.messages.sending")
                  : translate(
                    sel === 0
                      ? "robot.simulator.messages.moveRobotToRestPose"
                      : "robot.simulator.messages.moveRobotToThisPose",
                  )}
              </Button>
              <div className={"actions flex gap-[8px] flex-wrap mt-[12px] [&_>_button]:[flex:1]"}>
                <Button
                  variant="custom"
                  type="button"
                  disabled={busy}
                  onClick={save}
                >
                  {translate("robot.simulator.messages.saveProject")}
                </Button>
                <Button
                  variant="custom"
                  type="button"
                  disabled={busy}
                  onClick={load}
                >
                  {translate("robot.simulator.messages.openSavedProject")}
                </Button>
              </div>
              <Button
                variant="custom"
                type="button"
                className={dangerClass}
                disabled={!editable}
                onClick={removePose}
              >{translate("common.messages.deleteThisPose")}</Button>
            </section>
          </div>

          <section className={panelClass} aria-label={translate("robot.workspace.sequence.title")}>
            <h2 className={headingClass}>{translate("robot.workspace.sequence.title")}</h2>
            <div className={"pose-row flex gap-[12px] overflow-x-auto pt-[4px] px-[4px] pb-[10px]"}>
              <Button
                variant="custom"
                type="button"
                className={`${tileBaseClass} rest ${sel === 0 ? "on" : ""}`}
                disabled={busy}
                aria-pressed={sel === 0}
                onClick={() => setSel(0)}
              >
                <span className={tileNumberClass}>0</span>
                <span className={tileNameClass}>{translate("common.messages.restPose")}</span>
                <span className={tileFootClass}>{translate("robot.simulator.messages.readOnly")}</span>
              </Button>
              {poses.map((pose, index) => (
                <Button
                  variant="custom"
                  type="button"
                  key={pose.id}
                  className={`${tileBaseClass} ${tileStateClass(sel === index + 1, playing === index + 1)}`}
                  disabled={busy && playing === undefined}
                  aria-pressed={sel === index + 1}
                  onClick={() => setSel(index + 1)}
                >
                  <span className={tileNumberClass}>{index + 1}</span>
                  <span className={tileNameClass}>{pose.name}</span>
                  <span className={tileFootClass}>{pose.ms}{" ms"}</span>
                </Button>
              ))}
              <Button
                variant="custom"
                type="button"
                className={`${tileBaseClass} add`}
                disabled={busy || poses.length >= 100}
                onClick={addPose}
              >
                <span className="plus-dot">+</span>{translate("common.messages.addPose")}</Button>
            </div>
          </section>
        </div>
      </div>
      <div className={noticeClass} role="status">
        {translate(status)}
      </div>
    </>
  );
}
