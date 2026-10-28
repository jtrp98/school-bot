import { t as translate } from "../../i18n";
export function ArmFigure({
  a = [0, 0, 0, 0, 0, 50],
  w = 360,
  h = 360,
  labels,
  sel,
  mini,
  ghost,
  baseX = 0.32,
}: {
  a?: number[];
  w?: number;
  h?: number;
  labels?: boolean;
  sel?: number;
  mini?: boolean;
  ghost?: string;
  baseX?: number;
}) {
  const k = mini ? 0.6 : 1;
  const bx = w * baseX;
  const by = h - (mini ? 14 : 44);
  const L = [h * 0.3, h * 0.26, h * 0.17];
  const rad = (d: number) => (d * Math.PI) / 180;
  let ang = 0;
  let x = bx;
  let y = by - 22 * k;
  const pts: [number, number][] = [[x, y]];
  for (let i = 0; i < 3; i++) {
    ang += a[i + 1];
    x += L[i] * Math.sin(rad(ang));
    y -= L[i] * Math.cos(rad(ang));
    pts.push([x, y]);
  }
  const poly = pts.map((p) => `${p[0].toFixed(1)},${p[1].toFixed(1)}`).join(" ");
  const sw = mini ? 12 : 26;
  const sw2 = mini ? 7 : 16;
  const hl = (i: number) => sel === i;
  const [gx, gy] = pts[3];
  const spread = 8 + ((a[5] ?? 0) / 100) * 30;
  const gl = mini ? 14 : 30;
  const prong = (s: number) => {
    const t = ang + s * spread;
    return (
      <line
        key={s}
        x1={gx}
        y1={gy}
        x2={gx + gl * Math.sin(rad(t))}
        y2={gy - gl * Math.cos(rad(t))}
        stroke="var(--ink)"
        strokeWidth={mini ? 4 : 8}
        strokeLinecap="round"
      />
    );
  };
  const badge = (i: number, px: number, py: number) => (
    <g key={i}>
      <circle
        cx={px}
        cy={py}
        r={15}
        fill={hl(i) ? "var(--blue-ink)" : "var(--surface)"}
        stroke="var(--blue-ink)"
        strokeWidth={3}
      />
      <text
        x={px}
        y={py + 5}
        textAnchor="middle"
        fontFamily="Plus Jakarta Sans, sans-serif"
        fontWeight={800}
        fontSize={13}
        fill={hl(i) ? "var(--surface)" : "var(--blue-ink)"}
      >
        S{i}
      </text>
    </g>
  );
  return (
    <svg
      width={w}
      height={h}
      viewBox={`0 0 ${w} ${h}`}
      role="img"
      aria-label={translate("common.messages.robotArmIllustration")}
      className={"block"}
    >
      {!mini && <ellipse cx={bx} cy={by + 22} rx={w * 0.26} ry={10} fill="#f3efe9" />}
      {ghost && (
        <polyline
          points={ghost}
          fill="none"
          stroke="#f4610a"
          strokeWidth={sw2}
          strokeLinecap="round"
          strokeLinejoin="round"
          strokeDasharray="2 14"
          opacity={0.8}
        />
      )}
      <polyline points={poly} fill="none" stroke="#9ccdf0" strokeWidth={sw} strokeLinecap="round" strokeLinejoin="round" />
      <polyline points={poly} fill="none" stroke="#1490e0" strokeWidth={sw2} strokeLinecap="round" strokeLinejoin="round" />
      <rect x={bx - 34 * k} y={by - 22 * k} width={68 * k} height={24 * k} rx={8 * k} fill="#1f2430" />
      {!mini && (
        <rect x={bx - 22} y={by - 34} width={44} height={14} rx={6} fill="#fff" stroke={hl(0) ? "#f4610a" : "#0b6aae"} strokeWidth={3} />
      )}
      {!mini &&
        pts.slice(0, 3).map((p, i) => (
          <circle key={i} cx={p[0]} cy={p[1]} r={13} fill="#fff" stroke="#0b6aae" strokeWidth={4} />
        ))}
      {prong(-1)}
      {prong(1)}
      {labels && (
        <>
          {badge(0, bx - 52, by - 4)}
          {badge(1, pts[0][0], pts[0][1])}
          {badge(2, pts[1][0], pts[1][1])}
          {badge(3, pts[2][0], pts[2][1])}
          {badge(4, pts[3][0], pts[3][1] + 24)}
          {badge(5, pts[3][0] + 36 * Math.sin(rad(ang)) + 22, pts[3][1] - 36 * Math.cos(rad(ang)))}
        </>
      )}
    </svg>
  );
}
