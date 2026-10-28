import { t as translate } from "../../../i18n";
import { useCallback, useEffect, useRef, useState } from "react";
import { param } from "../../../core/debugParams";
import { ScenarioBar } from "../../../components/shared/ScenarioBar";
import { useSampleData, useShell } from "../../../shell";
import { useRobot, type Conn } from "../robotContext";
import { Workspace } from "./Workspace";
import { NativeWorkspace } from "./NativeWorkspace";
import { restoreNativeProject } from "./projectStore";
import { client } from "../../../core/client";
import { SCENARIOS, scenarioOf } from "./scenarios";
import type { LogicProps } from "./WorkspaceLogic";


const ROBOT = { name: "หุ่นกลุ่ม 3", port: "COM7" };
let consumedScenario = false;
let stash: Record<string, any> | null = null;
let nativeStash: Record<string, any> | null = null;

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
const awDevClass = [
  "aw-dev [&_.sb-scenario]:fixed [&_.sb-scenario]:top-[13px] [&_.sb-scenario]:left-[372px]",
  "[&_.sb-scenario]:bottom-auto [&_.sb-scenario]:z-[60] [&_.sb-scenario]:max-w-[calc(100vw_-_760px)]",
  "[&_.sb-scenario-toggle]:min-h-[34px] [&_.sb-scenario-toggle]:py-0 [&_.sb-scenario-toggle]:px-[12px]",
  "[&_.sb-scenario-toggle]:text-[12px] [&_.sb-scenario-toggle]:shadow-none",
  "[&_.sb-scenario-toggle]:max-w-full [&_.sb-scenario-toggle_>_span]:overflow-hidden",
  "[&_.sb-scenario-toggle_>_span]:text-ellipsis [&_.sb-scenario-toggle_>_span]:whitespace-nowrap",
  "[&_.sb-scenario-list]:order-0 [&_.sb-scenario-list]:absolute",
  "[&_.sb-scenario-list]:top-[calc(100%_+_8px)] [&_.sb-scenario-list]:left-0",
  "[&_.sb-scenario-list]:w-[min(660px,_calc(100vw_-_400px))] [&_.sb-scenario-list]:z-[95]",
  "[&_.sb-scenario-list]:max-h-[calc(var(--app-viewport-height,_100vh)_-_80px)]",
].join(" ");

export default function ArmWorkspace() {
  const robot = useRobot();
  const shell = useShell();
  const sample = useSampleData();

  const [scn, setScn] = useState(() => {
    if (consumedScenario) return "";
    consumedScenario = true;
    return param("scenario") ?? "";
  });
  const [fresh, setFresh] = useState(() => sample ? stash : nativeStash);
  const [loaded, setLoaded] = useState(sample || !!nativeStash);
  const [loadError, setLoadError] = useState("");
  useEffect(() => {
    if (sample || nativeStash) return;
    let live = true;
    void client.load("robot-control").then(value => {
      if (live) { nativeStash = restoreNativeProject(value); setFresh(nativeStash); }
    }).catch(e => { if (live) setLoadError(String(e)); }).finally(() => { if (live) setLoaded(true); });
    return () => { live = false; };
  }, [sample]);
  const [picked, setPicked] = useState(false);
  const sc = scenarioOf(sample ? scn : "empty");

  // the scenario decides the robot connection; until the app's state follows, show the wanted one
  const [want, setWant] = useState<Conn | null>(sc.conn ?? null);
  useEffect(() => {
    if (!sample || !want) return;
    if (want === "off") {
      if (robot.conn !== "off") robot.disconnect();
    } else {
      if (robot.conn === "off" || robot.conn === "connecting") robot.connectTo(robot.robot ?? ROBOT);
      if (want === "lost" && robot.conn !== "lost") robot.loseCable();
    }
    if (robot.conn === want) setWant(null);
  }, [want, robot]);
  const conn = sample ? want ?? robot.conn : robot.conn;

  const live = useRef({ robot, shell });
  live.current = { robot, shell };
  const reconnect = useCallback(() => {
    const { robot: r } = live.current;
    if (!r.sample) { r.go("connect"); return; }
    if (r.conn === "lost") {
      r.setConn("connecting");
      window.setTimeout(() => live.current.robot.setConn("connected"), 1200);
    } else r.go("connect");
  }, []);
  const onNav = useCallback((kind: string) => {
    const { robot: r } = live.current;
    if (kind === "work") r.go("works");
    else if (kind === "teacher") r.go("teacher");
    // the workspace's own leave dialog already covered unsaved work / a moving robot
    else if (kind === "home") live.current.shell.go("home", undefined, true);
    else if (kind === "connect") r.go("connect");

  }, []);

  const pickScenario = (id: string) => {
    if (!sample) return;
    setPicked(true);
    setScn(id);
    setWant(scenarioOf(id).conn ?? "connected");
  };

  const props: LogicProps = {
    farDeg: 30,
    bar: "A",
    net: "ok",
    jdens: "compact",
    sample4c: "none",
    ...sc.props,
    init: { lang: shell.lang, ...sc.init },
    boot: sc.boot,
    conn,
    workName: robot.workName,
    restore: !scn && !picked ? fresh : null,
    onStash: (s) => {
      if (sample) stash = s;
      else nativeStash = s;
    },
    onNav,
    onProjName: (name) => live.current.robot.setWorkName(name),
    onOpenMenu: () => live.current.shell.openMenu(),
    onReconnect: reconnect,
    onLang: (l) => live.current.shell.setLang(l),
  };

  const hostRef = useRef<HTMLDivElement>(null);
  useEffect(() => {
    const host = hostRef.current;
    const arm = host?.closest<HTMLElement>(".arm-lab") ?? host?.parentElement;
    const root = host?.firstElementChild as HTMLElement | null;
    if (!arm || !root) return;
    if (conn !== "lost") {
      arm.style.removeProperty("--rc-top");
      arm.style.removeProperty("--rc-bottom");
      return;
    }
    const measure = () => {
      const r = root.getBoundingClientRect();
      const headers = root.querySelectorAll<HTMLElement>(":scope > header");
      const head = headers[headers.length - 1];
      const bar = root.querySelector<HTMLElement>("[data-bar6]");
      if (head) arm.style.setProperty("--rc-top", `${Math.round(head.getBoundingClientRect().bottom - r.top)}px`);
      if (bar) arm.style.setProperty("--rc-bottom", `${Math.round(r.bottom - bar.getBoundingClientRect().top + 8)}px`);
    };
    measure();
    const id = window.setInterval(measure, 300);
    window.addEventListener("resize", measure);
    return () => {
      window.clearInterval(id);
      window.removeEventListener("resize", measure);
      arm.style.removeProperty("--rc-top");
      arm.style.removeProperty("--rc-bottom");
    };
  }, [conn]);

  return (
    <div className={"aw-host [display:contents]"} ref={hostRef} data-disc={conn !== "connected" ? "" : undefined}>
      {!loaded ? (
        <p role="status">{translate("robot.workspace.messages.loadingLocalProject")}</p>
      ) : sample ? (
        <Workspace key={scn} {...props} />
      ) : (
        <NativeWorkspace {...props} robotApi={robot} />
      )}
      {loadError && (
        <div className={reconnectVeilClass}>
          <div className="rc-card" role="alert">
            <p>{translate("robot.workspace.messages.couldNotLoadProjectReason", { reason: loadError })}</p>
            <p>{translate("robot.workspace.messages.checkTheLocalProjectFileBeforeOverwritingItYou")}</p>
            <button type="button" className="rc-btn rc-switch" onClick={() => setLoadError("")}>
              {translate("robot.workspace.messages.gotIt")}
            </button>
          </div>
        </div>
      )}
      {sample && (
        <div className={awDevClass}>
          <ScenarioBar items={SCENARIOS} value={scn} onChange={pickScenario} />
        </div>
      )}
    </div>
  );
}
