import { Button, Modal } from "../../../components/shared";
import { t as translate } from "../../../i18n";
import { useEffect, useRef, useState } from "react";

import { param } from "../../../core/debugParams";
import { useRobot } from "../robotContext";
import { RichText } from "../../../components/shared/RichText";
import { SAMPLE_ROBOT_NAME } from "../../../shell";
import { CheckIcon, SpinIcon } from "../../../components/robot-control/ConnectionIcons";

type Ph = "lost" | "back" | "homing";
const HOMING_MS = 2000;

const reconnectVeilClass = [
  "rc-veil [--cn-ink:var(--ink)] [--cn-muted:var(--muted)] [--cn-line:var(--line)]",
  "[--cn-blue:var(--blue-ink)] [--cn-green:var(--ok)] motion-reduce:[&_*]:animate-none!",
  "motion-reduce:[&_*]:[transition:none]! absolute left-0 right-0 top-[var(--rc-top,_60px)]",
  "bottom-[var(--rc-bottom,_88px)] bg-[rgba(var(--purple-tint-rgb),_0.82)] flex items-center",
  "justify-center z-[50] font-[family-name:'Noto_Sans_Thai',_'Leelawadee_UI',_sans-serif] text-ink",
  "leading-[normal] [&_*]:box-border [&_*::before]:box-border [&_*::after]:box-border",
  "[&_button]:font-[family-name:inherit] [&_button]:leading-[normal] [&_button]:min-h-0 [&_button]:m-0",
  "[&_button]:font-normal [&_:focus-visible]:[outline:3px_solid_var(--blue-ink)]",
  "[&_:focus-visible]:[outline-offset:2px] [&_.rc-card]:box-border [&_.rc-card]:bg-white",
  "[&_.rc-card]:rounded-[22px] [&_.rc-card]:py-[26px] [&_.rc-card]:px-[28px] [&_.rc-card]:flex",
  "[&_.rc-card]:flex-col [&_.rc-card]:gap-[14px] [&_.rc-card]:shadow-pop",
  "[&_.rc-card]:animate-[rcIn_.22s_ease-out_both] [&_.rc-card]:max-w-[calc(100%_-_32px)]",
  "[&_.rc-card.is-lost]:w-[620px] [&_.rc-card.is-lost]:border-2 [&_.rc-card.is-lost]:border-solid",
  "[&_.rc-card.is-lost]:border-orange-line [&_.rc-card.is-back]:w-[640px] [&_.rc-card.is-back]:border-2",
  "[&_.rc-card.is-back]:border-solid [&_.rc-card.is-back]:border-ok-line [&_.rc-head]:flex",
  "[&_.rc-head]:items-center [&_.rc-head]:gap-[14px] [&_.rc-spin]:animate-[rcSpin_1s_linear_infinite]",
  "[&_.rc-spin]:shrink-0 [&_.rc-tt]:font-[family-name:'Mitr',_'Noto_Sans_Thai',_sans-serif]",
  "[&_.rc-tt]:text-[24px] [&_.rc-tt]:font-medium [&_.rc-tt.is-lost]:text-orange-deep",
  "[&_.rc-tt.is-back]:text-ok-ink [&_.rc-ok]:w-[44px] [&_.rc-ok]:h-[44px] [&_.rc-ok]:rounded-full",
  "[&_.rc-ok]:bg-ok-tint [&_.rc-ok]:flex [&_.rc-ok]:items-center [&_.rc-ok]:justify-center",
  "[&_.rc-ok]:shrink-0 [&_.rc-bd]:text-[16px] [&_.rc-bd]:leading-[1.5]",
  "[&_.rc-bd.is-back]:leading-[1.55] [&_.rc-pills]:flex [&_.rc-pills]:gap-[10px]",
  "[&_.rc-pills]:flex-wrap [&_.rc-pill]:py-[6px] [&_.rc-pill]:px-[12px] [&_.rc-pill]:rounded-full",
  "[&_.rc-pill]:bg-blue-tint [&_.rc-pill]:text-muted [&_.rc-pill]:text-[14px]",
  "[&_.rc-pill]:font-semibold [&_.rc-pill.is-ok]:bg-ok-tint [&_.rc-pill.is-ok]:text-ok-ink",
  "[&_.rc-acts]:flex [&_.rc-acts]:gap-[10px] [&_.rc-acts]:justify-end",
  "[&_.rc-btn]:[transition:transform_.08s_ease,_filter_.12s_ease] [&_.rc-btn]:cursor-pointer",
  "[&_.rc-btn:hover]:[filter:brightness(1.04)] [&_.rc-btn:active]:[transform:translateY(2px)]",
  "[&_.rc-switch]:h-[48px] [&_.rc-switch]:py-0 [&_.rc-switch]:px-[18px] [&_.rc-switch]:border-2",
  "[&_.rc-switch]:border-solid [&_.rc-switch]:border-line-strong [&_.rc-switch]:border-b-4",
  "[&_.rc-switch]:rounded-[12px] [&_.rc-switch]:bg-white [&_.rc-switch]:text-blue-ink",
  "[&_.rc-switch]:text-[15px] [&_.rc-switch]:font-bold [&_.rc-switch]:flex [&_.rc-switch]:items-center",
  "[&_.rc-switch]:no-underline [&_.rc-sim]:h-[48px] [&_.rc-sim]:py-0 [&_.rc-sim]:px-[18px]",
  "[&_.rc-sim]:border-2 [&_.rc-sim]:border-dashed [&_.rc-sim]:border-faint [&_.rc-sim]:rounded-[12px]",
  "[&_.rc-sim]:bg-sunken [&_.rc-sim]:text-muted [&_.rc-sim]:text-[14px] [&_.rc-sim]:font-semibold",
  "[&_.rc-skip]:h-[50px] [&_.rc-skip]:py-0 [&_.rc-skip]:px-[18px] [&_.rc-skip]:border-2",
  "[&_.rc-skip]:border-solid [&_.rc-skip]:border-line-strong [&_.rc-skip]:border-b-4",
  "[&_.rc-skip]:rounded-[12px] [&_.rc-skip]:bg-white [&_.rc-skip]:text-ink [&_.rc-skip]:text-[15px]",
  "[&_.rc-skip]:font-semibold [&_.rc-home]:h-[50px] [&_.rc-home]:py-0 [&_.rc-home]:px-[22px]",
  "[&_.rc-home]:border-[medium] [&_.rc-home]:border-none [&_.rc-home]:border-current",
  "[&_.rc-home]:border-b-4 [&_.rc-home]:[border-bottom-style:solid] [&_.rc-home]:border-b-orange-press",
  "[&_.rc-home]:rounded-[12px] [&_.rc-home]:bg-orange [&_.rc-home]:text-ink [&_.rc-home]:text-[16px]",
  "[&_.rc-home]:font-bold [&_.rc-homing]:flex [&_.rc-homing]:flex-col [&_.rc-homing]:gap-[6px]",
  "[&_.rc-homing-t]:text-[14px] [&_.rc-homing-t]:font-bold [&_.rc-homing-t]:text-orange-deep",
  "[&_.rc-track]:h-[8px] [&_.rc-track]:rounded-full [&_.rc-track]:bg-line-strong",
  "[&_.rc-track]:overflow-hidden [&_.rc-track_>_div]:h-full [&_.rc-track_>_div]:bg-orange",
  "[&_.rc-track_>_div]:rounded-full [&_.rc-track_>_div]:animate-[cnBar_2s_linear_both]",
  "[&_.rc-track_>_div.is-hold]:animate-none [&_button:disabled]:opacity-[1]",
].join(" ");

export default function ReconnectOverlay() {
  const { robot, names, setConn, disconnect, go } = useRobot();
  const [ph, setPh] = useState<Ph>(() => {
    const q = param("rc");
    return q === "back" || q === "homing" ? q : "lost";
  });
  const hold = useRef(!!param("hold")).current;
  const timer = useRef<number | undefined>(undefined);

  const name = (robot && (names[robot.port] || robot.name)) || SAMPLE_ROBOT_NAME;
  const poseCount = 4;

  const done = () => setConn("connected");

  useEffect(() => {
    if (ph === "homing" && !hold) timer.current = window.setTimeout(done, HOMING_MS);
    return () => window.clearTimeout(timer.current);
  }, []);

  const goHome = () => {
    setPh("homing");
    window.clearTimeout(timer.current);
    timer.current = window.setTimeout(done, HOMING_MS);
  };
  const switchRobot = () => {
    disconnect();
    go("connect");
  };

  if (ph === "lost") {
    return (
      <div className={reconnectVeilClass}>
        <div className="rc-card is-lost" role="alert">
          <div className="rc-head">
            <SpinIcon size={34} stroke="var(--orange)" sw={3} />
            <span className="rc-tt is-lost">
              {translate("robot.connect.messages.robotDisconnectedWaitingForReconnection")}
            </span>
          </div>
          <span className="rc-bd">
            <RichText
              i18nKey="robot.connect.messages.reconnectCableFor"
              values={{ name }}
              components={{ bold: <strong /> }}
            />
          </span>
          <div className="rc-pills">
            <span className="rc-pill is-ok">
              {translate("robot.connect.messages.posesKept", { count: poseCount })}
            </span>
            <span className="rc-pill">
              {translate("robot.connect.messages.movementControlsAreDisabledUntilReconnected")}
            </span>
          </div>
          <div className="rc-acts">
            <Button
              variant="custom"
              type="button"
              className="rc-btn rc-switch"
              onClick={switchRobot}
            >
              {translate("robot.connect.messages.useADifferentRobot")}
            </Button>
            <Button
              variant="custom"
              type="button"
              className="rc-btn rc-sim"
              onClick={() => setPh("back")}
            >
              {translate("robot.connect.messages.simulationReconnectCable")}
            </Button>
          </div>
        </div>
      </div>
    );
  }

  const homing = ph === "homing";
  return (
    <div className={reconnectVeilClass}>
      <Modal
        layout="inline"
        className="rc-card is-back"
        label={translate("robot.connect.messages.robotReconnected")}
      >
        <div className="rc-head">
          <span className="rc-ok">
            <CheckIcon size={26} stroke="var(--ok)" sw={3.2} />
          </span>
          <span className="rc-tt is-back">{translate("robot.connect.messages.reconnected", { name })}</span>
        </div>
        <span className="rc-bd is-back">
          <RichText i18nKey="robot.connect.messages.reconnectedWarning" components={{ bold: <strong /> }} />
          <br />
          {translate("robot.connect.messages.returnToTheRestPoseBeforeContinuingToAvoid")}
        </span>
        {homing && (
          <div className="rc-homing" role="status">
            <span className="rc-homing-t">{translate("robot.connect.messages.returningToRestPoseKeepHandsAway")}</span>
            <div className="rc-track">
              <div className={hold ? "is-hold" : undefined} style={hold ? { width: "58%" } : undefined} />
            </div>
          </div>
        )}
        {!homing && (
          <div className="rc-acts">
            <Button
              variant="custom"
              type="button"
              className="rc-btn rc-skip"
              onClick={done}
            >
              {translate("robot.connect.messages.skipIMSureTheRobotHasNotMoved")}
            </Button>
            <Button
              variant="custom"
              type="button"
              className="rc-btn rc-home"
              autoFocus
              onClick={goHome}
            >
              {translate("robot.connect.messages.moveRobotToRestPose")}
            </Button>
          </div>
        )}
      </Modal>
    </div>
  );
}
