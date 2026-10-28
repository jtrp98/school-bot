import { Button } from "../../../components/shared";
import { t as translate } from "../../../i18n";

import { Svg } from "../../../components/robot-control/RobotSvg";
import { useState } from "react";
import { ID_OUTCOMES, type IdOutcome } from "./connectData";

export const WarnIcon = ({ size, stroke, sw = 2.2 }: { size: number; stroke: string; sw?: number }) => (
  <Svg size={size} stroke={stroke} sw={sw}>
    <path d="M12 3 2 20h20z" />
    <path d="M12 10v4M12 17h.01" />
  </Svg>
);

export const UnplugIcon = ({ size, stroke }: { size: number; stroke: string }) => (
  <Svg size={size} stroke={stroke} sw={2.2}>
    <path d="M9 7 7 9a4 4 0 0 0 0 6l2 2M15 7l2 2a4 4 0 0 1 0 6l-2 2" />
    <path d="m4 4 16 16" />
  </Svg>
);

export const FoldIcon = () => (
  <Svg size={16} sw={2.4}>
    <path d="M3 9h18v11H3z" />
    <path d="M12 3v9M8 8l4 4 4-4" />
  </Svg>
);

export function DevPanel({
  outcome,
  unsaved,
  moving,
  onOutcome,
  onUnsaved,
  onMoving,
}: {
  outcome: IdOutcome;
  unsaved: boolean;
  moving: boolean;
  onOutcome: (v: IdOutcome) => void;
  onUnsaved: (v: boolean) => void;
  onMoving: (v: boolean) => void;
}) {
  const [open, setOpen] = useState(false);
  return (
    <div className={"cn-dev"}>
      <Button
        variant="custom"
        type="button"
        className={"cn-dev-toggle"}
        aria-expanded={open}
        onClick={() => setOpen(!open)}
      >
        {translate("robot.connect.debug.title")}{" "}{open ? "▴" : "▾"}
      </Button>
      {open && (
        <div className={"cn-dev-panel"} role="group" aria-label={translate("robot.connect.messages.testTools")}>
          <span>{translate("robot.connect.messages.cableIdentificationResult")}</span>
          <div className={"cn-dev-seg"}>
            {ID_OUTCOMES.map((o) => (
              <Button
                variant="custom"
                key={o.value}
                type="button"
                aria-pressed={outcome === o.value}
                onClick={() => onOutcome(o.value)}
              >
                {translate(o.label)}
              </Button>
            ))}
          </div>
          <label className={"cn-dev-row"}>
            <span>{translate("robot.connect.messages.somePosesAreNotUploaded")}</span>
            <input type="checkbox" checked={unsaved} onChange={(e) => onUnsaved(e.target.checked)} />
          </label>
          <label className={"cn-dev-row"}>
            <span>{translate("robot.connect.messages.robotIsMoving")}</span>
            <input type="checkbox" checked={moving} onChange={(e) => onMoving(e.target.checked)} />
          </label>
          <span className={"cn-dev-note"}>{translate("robot.connect.messages.sampleRobotAndPortDataNoRealSerialConnection")}</span>
        </div>
      )}
    </div>
  );
}
