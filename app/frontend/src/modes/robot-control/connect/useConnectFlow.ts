import { type FlowState, portsOf, nameOf, portOfKey, clock, initialState, startState } from "./connectState";
import { useCallback, useEffect, useRef, useState } from "react";
import { param } from "../../../core/debugParams";
import { useRobot } from "../robotContext";
import { MS, type ErrKey, type ScenarioId } from "./connectData";


















export function useConnectFlow() {
  const rb = useRobot();
  const rbRef = useRef(rb);
  rbRef.current = rb;

  const hold = useRef(!!param("hold")).current;

  const startRef = useRef<ReturnType<typeof startState> | null>(null);
  if (!startRef.current) startRef.current = startState(rb);
  const [s, setS] = useState<FlowState>(startRef.current.state);
  const sRef = useRef<FlowState>(s);

  const patch = useCallback((p: Partial<FlowState>) => {
    sRef.current = { ...sRef.current, ...p };
    setS(sRef.current);
  }, []);

  const timers = useRef(new Map<string, number>());
  const cancel = useCallback((key: string) => {
    const t = timers.current.get(key);
    if (t !== undefined) window.clearTimeout(t);
    timers.current.delete(key);
  }, []);
  const clearAll = useCallback(() => {
    timers.current.forEach((t) => window.clearTimeout(t));
    timers.current.clear();
  }, []);
  const later = useCallback(
    (key: string, fn: () => void, ms: number) => {
      cancel(key);
      timers.current.set(
        key,
        window.setTimeout(() => {
          timers.current.delete(key);
          fn();
        }, ms),
      );
    },
    [cancel],
  );

  const finish = useCallback(() => {
    cancel("go");
    const f = rbRef.current.from;
    rbRef.current.go(f === "works" ? "works" : "workspace");
  }, [cancel]);
  const goLater = useCallback(() => {
    later("go", () => {
      if (sRef.current.phase === "ok") finish();
    }, MS.goBack);
  }, [later, finish]);

  const afterOk = useCallback(
    (key: string) => {
      if (!sRef.current.names[key]) patch({ phase: "naming", nameDraft: "" });
      else {
        patch({ phase: "ok" });
        goLater();
      }
    },
    [patch, goLater],
  );

  const resolveConnect = useCallback(() => {
    const c = sRef.current;
    const P = portsOf(c);
    const robot = P.robots.find((r) => r.port === c.picked);
    let err: ErrKey | null = null;
    if (!robot) err = "noReply";
    else if (c.scenario === "busy" && c.tries < 2) err = "busy";
    else if ((c.scenario === "noReply" || c.scenario === "fail2") && c.tries < 3) err = "noReply";
    if (err || !robot) {
      patch({ phase: "idle", err });
      rbRef.current.setConn("off");
      return;
    }
    patch({ connKey: robot.key, since: clock(), switchTo: null });
    rbRef.current.connectTo({ name: nameOf(sRef.current, robot.key), port: robot.port });
    afterOk(robot.key);
  }, [patch, afterOk]);

  const connect = useCallback(() => {
    const c = sRef.current;
    if (!c.picked || c.phase === "connecting") return;
    cancel("t");
    cancel("go");
    patch({ phase: "connecting", err: null, tries: c.tries + 1, connKey: null, justDisc: null });
    rbRef.current.setConn("connecting");
    later("t", resolveConnect, MS.connect);
  }, [cancel, later, patch, resolveConnect]);

  const finishScan = useCallback(() => {
    const c = sRef.current;
    const found = c.scenario === "none" ? true : c.found;
    patch({
      phase: "idle",
      found,
      picked: c.scenario === "none" && found && !c.picked ? "COM7" : c.picked,
      id: c.id === "wait" ? null : c.id,
    });
  }, [patch]);
  const rescan = useCallback(() => {
    cancel("t");
    patch({ phase: "scanning", err: null });
    later("t", finishScan, MS.scan);
  }, [cancel, later, patch, finishScan]);

  const scheduleId = useCallback(() => {
    later("it", () => {
      patch({ id: "unplug2" });
      if (sRef.current.outcome === "two") return;
      later("it", () => {
        const o = sRef.current.outcome;
        if (o === "timeout") {
          patch({ id: "wait", idTips: true });
          return;
        }
        const p8 = o === "portChanged";
        patch({ id: "found", port8: p8, picked: p8 ? "COM8" : "COM7" });
      }, MS.idStep);
    }, MS.idStep);
  }, [later, patch]);
  const startId = useCallback(() => {
    cancel("it");
    patch({ id: "unplug", picked: null, err: null, port8: false, idTips: false });
    scheduleId();
  }, [cancel, patch, scheduleId]);
  const cancelId = useCallback(() => {
    cancel("it");
    patch({ id: null, idTips: false });
  }, [cancel, patch]);
  const idAct = useCallback(() => {
    if (sRef.current.id === "wait") patch({ idTips: !sRef.current.idTips });
    else startId();
  }, [patch, startId]);

  const doDisconnect = useCallback(
    (folded: boolean) => {
      const c = sRef.current;
      const key = c.connKey;
      patch({
        connKey: null,
        disc: null,
        justDisc: key,
        picked: key ? portOfKey(c, key) : c.picked,
        phase: "idle",
        moving: false,
        discFolded: folded,
        switchTo: null,
      });
      rbRef.current.disconnect();
    },
    [patch],
  );
  const fin = useCallback(
    (fold: boolean) => {
      if (fold) {
        patch({ disc: "folding", fold: true });
        later("dt", () => doDisconnect(true), MS.folding);
      } else doDisconnect(false);
    },
    [patch, later, doDisconnect],
  );
  const startDisc = useCallback(
    (fold: boolean) => {
      const c = sRef.current;
      if (c.moving) {
        patch({ disc: "stopping", fold });
        later("dt", () => {
          patch({ moving: false });
          if (sRef.current.unsaved) patch({ disc: "ask", fold });
          else fin(fold);
        }, MS.stopping);
        return;
      }
      if (c.unsaved) {
        patch({ disc: "ask", fold });
        return;
      }
      patch({ fold });
      fin(fold);
    },
    [patch, later, fin],
  );
  const cancelDisc = useCallback(() => {
    cancel("dt");
    patch({ disc: null });
  }, [cancel, patch]);
  const discNow = useCallback(() => fin(sRef.current.fold), [fin]);
  const uploadThenDisc = useCallback(() => {
    patch({ disc: "uploading" });
    later("dt", () => {
      patch({ unsaved: false });
      fin(sRef.current.fold);
    }, MS.uploading);
  }, [patch, later, fin]);

  const stopAll = useCallback(() => {
    const c = sRef.current;
    cancel("dt");
    patch({ moving: false, disc: c.disc === "ask" ? "ask" : null });
  }, [cancel, patch]);

  const pick = useCallback(
    (port: string) => {
      const c = sRef.current;
      patch({ picked: port, err: null, tries: 0, switchTo: c.connKey ? port : null });
    },
    [patch],
  );
  const toggleOthers = useCallback(() => patch({ showOthers: !sRef.current.showOthers }), [patch]);
  const toggleCheck = useCallback(
    (i: number) => patch({ checks: { ...sRef.current.checks, [i]: !sRef.current.checks[i] } }),
    [patch],
  );
  const setNameDraft = useCallback((v: string) => patch({ nameDraft: v }), [patch]);
  const saveName = useCallback(() => {
    const c = sRef.current;
    const nm = c.nameDraft.trim();
    if (!c.connKey || !nm) return;
    patch({ names: { ...c.names, [c.connKey]: nm }, phase: "ok" });
    rbRef.current.nameRobot(c.connKey, nm);
    rbRef.current.connectTo({ name: nm, port: portOfKey(c, c.connKey) });
    goLater();
  }, [patch, goLater]);
  const skipName = useCallback(() => {
    patch({ phase: "ok" });
    goLater();
  }, [patch, goLater]);

  const kickoff = useCallback(
    (st: FlowState) => {
      if (hold) return;
      if (st.phase === "scanning") later("t", finishScan, MS.scan);
      else if (st.phase === "connecting") later("t", resolveConnect, MS.connect);
      else if (st.phase === "ok") goLater();
      if (st.id === "unplug") scheduleId();
      if (st.disc === "folding") later("dt", () => doDisconnect(true), MS.folding);
    },
    [hold, later, finishScan, resolveConnect, goLater, scheduleId, doDisconnect],
  );
  const syncCtx = useCallback((st: FlowState) => {
    const r = rbRef.current;
    if (st.connKey) r.connectTo({ name: nameOf(st, st.connKey), port: portOfKey(st, st.connKey) });
    else if (st.phase === "connecting") r.setConn("connecting");
    else if (r.conn !== "off") r.disconnect();
  }, []);
  const applyScenario = useCallback(
    (id: ScenarioId) => {
      clearAll();
      const c = sRef.current;
      const st = initialState(id, rbRef.current, true, {
        unsaved: c.unsaved,
        moving: c.moving,
        outcome: c.outcome,
      });
      sRef.current = st;
      setS(st);
      syncCtx(st);
      kickoff(st);
    },
    [clearAll, syncCtx, kickoff],
  );
  const setDev = useCallback(
    (p: Partial<Pick<FlowState, "outcome" | "unsaved" | "moving">>) => patch(p),
    [patch],
  );

  useEffect(() => {
    if (startRef.current?.explicit) syncCtx(sRef.current);
    kickoff(sRef.current);
    return clearAll;
  }, []);

  return {
    s,
    hold,
    actions: {
      pick,
      rescan,
      connect,
      startId,
      cancelId,
      idAct,
      toggleOthers,
      toggleCheck,
      setNameDraft,
      saveName,
      skipName,
      startDisc,
      cancelDisc,
      discNow,
      uploadThenDisc,
      stopAll,
      finish,
      applyScenario,
      setDev,
    },
  };
}
