import { t as translate } from "../../i18n";
import { Icon } from "./Icons";
import { Pill, type PillTone } from "./Pill";

export type RobotChipState = "connected" | "off" | "lost" | "busy" | "moving";

const robotChipClass = [
  "sb-robotchip text-ink inline-flex items-center gap-[10px] min-h-[44px] py-0 px-[14px]",
  "rounded-control bg-sunken border-2 border-solid border-line not-italic [font-variant:normal]",
  "font-bold [font-stretch:normal] text-[15px] leading-none font-body cursor-pointer hover:border-blue",
  "hover:bg-blue-tint",
].join(" ");
const portClass = [
  "sb-t-cap not-italic [font-variant:normal] font-medium [font-stretch:normal] text-[13px]",
  "leading-[1.4] font-body text-muted",
].join(" ");

export function RobotChip({
  state = "off",
  name,
  port,
  onClick,
}: {
  state?: RobotChipState;
  name?: string;
  port?: string;
  onClick?: () => void;
}) {
  const m = {
    connected: ["ok", "robot.workspace.messages.connected"],
    off: ["off", "common.messages.notConnected"],
    lost: ["err", "common.messages.connectionLost"],
    busy: ["warn", "robot.workspace.messages.connecting"],
    moving: ["warn", "robot.workspace.messages.moving"],
  }[state] as [PillTone, string];
  const shown = state === "off"
    ? translate("common.messages.chooseRobot")
    : (name ?? translate("robot.connect.messages.robotArm"));
  return (
    <button
      type="button"
      className={robotChipClass}
      onClick={onClick}
      aria-label={translate("common.messages.robotValue0Value1", {value0: shown, value1: translate(m[1])})}
    >
      <Icon name="bot" size={22} />
      <span>{shown}</span>
      {port && state !== "off" && <span className={portClass}>· {port}</span>}
      <Pill
        tone={m[0]}
        dot={state !== "busy"}
        icon={state === "busy" ? "loader" : undefined}
      >
        {translate(m[1])}
      </Pill>
    </button>
  );
}
