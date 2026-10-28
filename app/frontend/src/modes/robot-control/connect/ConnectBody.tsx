import { Button, Modal } from "../../../components/shared";
import { TimedProgressBar } from "../../../components/shared/TimedProgressBar";
import { DISC_BUSY_KEYS } from "./connectPresentation";
import { t as translate } from "../../../i18n";

import { DEEP_STEPS, ERR_TEXT, HELP_TIPS, WAIT_TIPS } from "./connectData";
import { FoldIcon, UnplugIcon, WarnIcon } from "./parts";
import { CheckIcon, RefreshIcon } from "../../../components/robot-control/ConnectionIcons";
import { familyOf, nameOf, portOfKey } from "./connectState";

import type { ConnectView } from "./ConnectView";

const connectedSinceKey = (unsaved: boolean, moving: boolean) => {
  if (unsaved) {
    return moving
      ? "robot.connect.messages.connectedSinceUnuploadedMoving"
      : "robot.connect.messages.connectedSinceUnuploaded";
  }
  return moving
    ? "robot.connect.messages.connectedSinceInSyncMoving"
    : "robot.connect.messages.connectedSinceInSync";
};

export function ConnectBody({ v }: { v: Pick<Parameters<typeof ConnectView>[0]["v"],
  "s" | "hold" | "a" | "P" | "busy" | "connKey" | "noRobots" | "id" | "hasError" | "showHelp" | "discMs" | "idle"> }) {
const { s, hold, a, P, busy, connKey, noRobots, id, hasError, showHelp, discMs, idle } = v;
const connBox = connKey && (s.phase === "idle" || !!s.disc) && (
    <section className="cn-conn cn-in" aria-label={translate("robot.connect.messages.connectedRobot")}>
      <div className="cn-conn-top">
        <span className="cn-conn-ic">
          <CheckIcon size={26} stroke="var(--ok)" sw={3.2} />
        </span>
        <div className="cn-col cn-conn-txt">
          <span className="cn-mitr cn-conn-t">
            {translate("robot.connect.messages.connectedNamePort", {
    name: nameOf(s, connKey),
    port: portOfKey(s, connKey),
  })}
          </span>
          <span className="cn-conn-s">
            {translate(connectedSinceKey(s.unsaved, s.moving), { since: s.since })}
          </span>
        </div>
        {idle && (
          <>
            <Button
              variant="custom"
              type="button"
              className="cn-btn cn-disc"
              onClick={() => a.startDisc(false)}
            >
              {translate("robot.connect.disconnect")}
            </Button>
            <Button
              variant="custom"
              type="button"
              className="cn-btn cn-go"
              onClick={a.finish}
            >
              {translate("robot.connect.messages.backToYourProject")}
            </Button>
          </>
        )}
      </div>
      {idle && (
        <div className="cn-fold">
          <span>{translate("robot.connect.messages.classFinished")}</span>
          <Button
            variant="custom"
            type="button"
            className="cn-btn cn-fold-b"
            onClick={() => a.startDisc(true)}
          >
            <FoldIcon />
            {translate("robot.connect.messages.foldTheArmIntoItsBoxThenDisconnect")}
          </Button>
          <span className="cn-fold-n">{translate("robot.connect.messages.clearTheAreaAroundTheArmFirst")}</span>
        </div>
      )}
      {s.disc === "ask" && (
        <Modal
          layout="inline"
          className="cn-ask cn-in"
          label={translate("robot.connect.messages.confirmDisconnect")}
        >
          <WarnIcon size={30} stroke="var(--orange-deep)" />
          <div className="cn-col cn-ask-txt">
            <span className="cn-ask-t">
              {s.unsaved
                ? translate("robot.connect.messages.somePosesHaveNotBeenUploadedToTheRobot")
                : translate("robot.connect.messages.disconnect")}
            </span>
            <span className="cn-ask-b">
              {s.unsaved ? translate("robot.connect.messages.appPosesWillBeKeptIfYouDisconnectNow") : ""}
            </span>
          </div>
          <Button
            variant="custom"
            type="button"
            className="cn-btn cn-ask-cancel"
            onClick={a.cancelDisc}
          >
            {translate("common.cancel")}
          </Button>
          {s.unsaved && (
            <Button
              variant="custom"
              type="button"
              className="cn-btn cn-ask-up"
              onClick={a.uploadThenDisc}
            >
              {translate("robot.connect.messages.uploadThenDisconnect")}
            </Button>
          )}
          <Button
            variant="custom"
            type="button"
            className="cn-btn cn-ask-now"
            onClick={a.discNow}
          >
            {s.fold
              ? translate("robot.connect.messages.foldAndDisconnect")
              : translate("robot.connect.messages.disconnectNow")}
          </Button>
        </Modal>
      )}
      {(s.disc === "stopping" || s.disc === "folding" || s.disc === "uploading") && (
        <div className="cn-busy cn-in" role="status">
          <span className="cn-busy-t">{translate(DISC_BUSY_KEYS[s.disc])}</span>
          <div className="cn-track">
            <TimedProgressBar key={s.disc} ms={discMs} hold={hold} pct={62} />
          </div>
        </div>
      )}
    </section>
  );

  const cards = P.robots.map((r) => {
    const on = s.picked === r.port;
    const inUse = connKey === r.key;
    const wasDisc = s.justDisc === r.key && !connKey;
    const ours = s.id === "found" && r.key === "COM7";
    const named = !!s.names[r.key];
    let tag = "";
    let tagOk = false;
    if (inUse) {
      tag = translate("robot.connect.messages.cardTagInUse");
      tagOk = true;
    } else if (wasDisc) tag = translate("robot.connect.messages.cardTagDisconnected");
    else if (ours) {
      tag = s.port8
        ? translate("robot.connect.messages.cardTagOursBack", { port: r.port })
        : translate("robot.connect.messages.cardTagOurs");
      tagOk = true;
    }
    const label = inUse
      ? translate("robot.connect.messages.inUse")
      : on
        ? translate("robot.connect.messages.selected")
        : translate("robot.connect.messages.useThisRobot");
    return (
      <div
        key={r.key}
        className={inUse
          ? "cn-card is-use"
          : on
            ? "cn-card is-on"
            : "cn-card"}
      >
        <span className="cn-card-txt">
          <span className="cn-card-n">{named ? s.names[r.key] : translate("robot.connect.messages.robotArm")}</span>
          <span className="cn-card-s">
            {translate(
              named ? "robot.connect.messages.portLabel" : "robot.connect.messages.portUnnamed",
              { port: r.port },
            )}
          </span>
        </span>
        {tag && <span className={tagOk ? "cn-tag is-ok" : "cn-tag"}>{tag}</span>}
        <Button
          variant="custom"
          type="button"
          className="cn-pick"
          disabled={busy || inUse}
          aria-pressed={on}
          aria-label={translate("robot.connect.messages.cardAriaLabel", {
            action: label,
            name: named ? s.names[r.key] : translate("robot.connect.messages.robotArm"),
            port: r.port,
          })}
          onClick={() => a.pick(r.port)}
        >
          {label}
        </Button>
      </div>
    );
  });

  const body = (
    <>
      {connBox}

      {!!s.justDisc && !connKey && s.phase === "idle" && !s.err && (
        <div className="cn-done cn-in" role="status">
          <UnplugIcon size={28} stroke="var(--muted)" />
          <div className="cn-col cn-grow">
            <span className="cn-done-t">
              {translate(
    s.discFolded ? "robot.connect.messages.disconnectedNameFolded" : "robot.connect.messages.disconnectedName",
    { name: nameOf(s, s.justDisc) },
  )}
            </span>
            <span className="cn-done-s">
              {translate("robot.connect.messages.youCanOpenCodeLabToKeepUsingThe")}
            </span>
          </div>
        </div>
      )}

      <div className="cn-sec">
        <span className="cn-sec-t">{translate("robot.connect.messages.robotsFoundOnThisDevice")}</span>
        <span className="cn-sec-n">
          {s.phase === "scanning"
            ? translate("robot.connect.messages.searching")
            : translate("robot.connect.messages.updatesWhenACableIsConnectedOrDisconnected")}
        </span>
        <div className="cn-grow" />
        <Button
          variant="custom"
          type="button"
          className="cn-btn cn-rescan"
          disabled={busy}
          onClick={a.rescan}
        >
          <RefreshIcon />
          {translate("robot.connect.messages.searchAgain")}
        </Button>
      </div>

      {!noRobots && (
        <>
          <div className="cn-cards">{cards}</div>
          {familyOf(s.scenario) === "many" && !s.id && s.phase === "idle" && !connKey && (
            <Button
              variant="custom"
              type="button"
              className="cn-btn cn-idbtn"
              disabled={busy}
              onClick={a.startId}
            >
              {translate("robot.connect.messages.notSureWhichRobotIdentifyItByUnpluggingIts")}
            </Button>
          )}
          {id && (
            <div className={id.warn ? "cn-id is-warn cn-in" : "cn-id cn-in"} role="status">
              <span className="cn-id-no">{id.no}</span>
              <div className="cn-id-txt">
                <span className="cn-id-t">{translate(id.title)}</span>
                <span className="cn-id-b">{translate(id.body)}</span>
              </div>
              {((s.id === "unplug2" && s.outcome === "two") || s.id === "wait") && (
                <Button
                  variant="custom"
                  type="button"
                  className="cn-btn cn-id-act"
                  aria-expanded={s.id === "wait" ? s.idTips : undefined}
                  onClick={a.idAct}
                >
                  {s.id === "wait"
                    ? translate(s.idTips
                      ? "robot.connect.messages.idTipsHide"
                      : "robot.connect.messages.idTipsShow")
                    : translate("common.messages.startOver")}
                </Button>
              )}
              <Button
                variant="custom"
                type="button"
                className="cn-btn cn-id-cancel"
                onClick={a.cancelId}
              >
                {s.id === "found" ? translate("common.messages.close") : translate("common.cancel")}
              </Button>
            </div>
          )}
          {s.id === "wait" && s.idTips && (
            <div className="cn-tips cn-in">
              {WAIT_TIPS.map((t) => (
                <div key={translate(t)} className="cn-tip">
                  {translate(t)}
                </div>
              ))}
            </div>
          )}
        </>
      )}

      {noRobots && (
        <div className="cn-empty">
          <span className="cn-mitr cn-empty-t">
            {s.phase === "scanning"
              ? translate("robot.connect.messages.searching")
              : translate("robot.connect.messages.noRobotsFoundOnThisDevice")}
          </span>
          <span className="cn-empty-s">
            {translate("robot.connect.messages.followTheStepsBelowThenPressSearchAgain")}
          </span>
        </div>
      )}

      <div className="cn-others">
        <Button
          variant="custom"
          type="button"
          className="cn-others-t"
          aria-expanded={s.showOthers}
          onClick={a.toggleOthers}
        >
          {translate("robot.connect.messages.value0OtherDevicesUnlikelyToBeRobotsValue1Usually", {
            value0: s.showOthers ? "▾ " : "▸ ",
            value1: P.others.length,
          })}
        </Button>
        {s.showOthers && (
          <div className="cn-cards">
            {P.others.map((o) => {
              const on = s.picked === o.port;
              const label = on
                ? translate("robot.connect.messages.selected")
                : translate("robot.connect.messages.useThisPort");
              return (
                <div key={o.port} className={on ? "cn-card is-other is-on" : "cn-card is-other"}>
                  <span className="cn-card-n is-other">{o.name}</span>
                  <span className="cn-card-s">{translate("robot.connect.messages.portLabel", { port: o.port })}</span>
                  <Button
                    variant="custom"
                    type="button"
                    className="cn-pick"
                    disabled={busy}
                    aria-label={translate("robot.connect.messages.value0Value1PortValue2", {
                      value0: label,
                      value1: o.name,
                      value2: o.port,
                    })}
                    onClick={() => a.pick(o.port)}
                  >
                    {label}
                  </Button>
                </div>
              );
            })}
          </div>
        )}
      </div>

      {hasError && s.err && (
        <div className="cn-err cn-in" role="alert">
          <div className="cn-err-top">
            <WarnIcon size={36} stroke="var(--err-ink)" sw={2} />
            <div className="cn-err-txt">
              <span className="cn-err-t">{translate(ERR_TEXT[s.err].title)}</span>
              <span className="cn-err-b">{translate(ERR_TEXT[s.err].body)}</span>
            </div>
            {s.tries >= 2 && (
              <span
                className="cn-code"
                title={translate("robot.connect.messages.showThisCodeToYourTeacherToIdentifyThe")}
              >
                <span className="cn-code-a">{translate("robot.connect.messages.showThisCodeToYourTeacher")}</span>
                <span className="cn-code-b">{`${ERR_TEXT[s.err].code} · ${s.picked ?? ""}`}</span>
              </span>
            )}
            <Button
              variant="custom"
              type="button"
              className="cn-btn cn-retry"
              onClick={a.connect}
            >
              {translate("robot.connect.messages.tryAgain")}
            </Button>
          </div>
          {s.tries >= 2 && (
            <div className="cn-deep">
              <span className="cn-deep-t">
                {translate("robot.connect.messages.attemptStillFailing", { tries: s.tries })}
              </span>
              <div className="cn-steps">
                {DEEP_STEPS[s.err].map((d, i) => {
                  const done = !!s.checks[i];
                  return (
                    <Button
                      variant="custom"
                      key={translate(d[0])}
                      type="button"
                      className={done ? "cn-step is-done" : "cn-step"}
                      aria-pressed={done}
                      onClick={() => a.toggleCheck(i)}
                    >
                      <span className="cn-step-bx">{done ? "✓" : String(i + 1)}</span>
                      <span className="cn-step-txt">
                        <span className="cn-step-a">{translate(d[0])}</span>
                        <span className="cn-step-b">{translate(d[1])}</span>
                      </span>
                    </Button>
                  );
                })}
              </div>
            </div>
          )}
        </div>
      )}

      {showHelp && (
        <div className="cn-help">
          <span className="cn-help-t">
            {noRobots
              ? translate("robot.connect.messages.canTFindYourRobotTryTheseSteps")
              : translate("robot.connect.messages.moreChecks")}
          </span>
          <div className="cn-help-g">
            {HELP_TIPS.map((t, i) => (
              <div key={translate(t)} className="cn-help-c">
                <span className="cn-help-n">{i + 1}</span>
                <span className="cn-help-x">{translate(t)}</span>
              </div>
            ))}
          </div>
        </div>
      )}
    </>
  );
return body;
}
