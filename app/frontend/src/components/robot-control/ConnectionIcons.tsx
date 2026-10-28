import { Svg } from "./RobotSvg";

export const CheckIcon = ({ size, stroke, sw }: { size: number; stroke?: string; sw: number }) => (
  <Svg size={size} stroke={stroke} sw={sw}>
    <path d="m5 12 5 5 9-10" />
  </Svg>
);

export const RefreshIcon = () => (
  <Svg size={16} sw={2.6}>
    <path d="M20 11a8 8 0 1 0-2.3 5.7" />
    <path d="M20 4v7h-7" />
  </Svg>
);

export const SpinIcon = ({ size, stroke, sw }: { size: number; stroke: string; sw: number }) => (
  <Svg size={size} stroke={stroke} sw={sw} className={"rc-spin"}>
    <path d="M12 3a9 9 0 1 0 9 9" />
  </Svg>
);
