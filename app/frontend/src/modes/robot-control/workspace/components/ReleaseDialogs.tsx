import { Button } from "../../../../components/shared/Button";
import { Modal } from "../../../../components/shared/Modal";
import { t as translate } from "../../../../i18n";
import type { ReactNode } from "react";
import { T } from "../tplUtil";
import type { WorkspaceViewProps } from "../viewTypes";

type Props = WorkspaceViewProps<"dialogs" | "dialogActions">;

const HAND_STROKE = {
  stroke: "var(--ink)",
  strokeWidth: "2.2",
  strokeLinecap: "round",
  strokeLinejoin: "round",
  fill: "none",
} as const;

function SupportHand({ transform }: { transform: string }): ReactNode {
  return (
    <g transform={transform} {...HAND_STROKE}>
      <path
        d="M3 16 L8.6 10.6 C8.9 10.2 9.4 10 10 10 H13 A2 2 0 0 1 14.45 13.39 L18.65 9.49 A2 2 0 0 1 21.4 12.4 L16.8 16.8 C16.1 17.6 15.1 18 14 18 H10 C9.4 18 8.9 18.2 8.6 18.6 L7 20 Z"
        fill="var(--surface)"
        stroke="none"
      />
      <path d="M11 14h2a2 2 0 1 0 0-4h-3c-.6 0-1.1.2-1.4.6L3 16" />
      <path d="m7 20 1.6-1.4c.3-.4.8-.6 1.4-.6h4c1.1 0 2.1-.4 2.8-1.2l4.6-4.4a2 2 0 0 0-2.75-2.91l-4.2 3.9" />
      <path d="m2 15 6 6" />
    </g>
  );
}

function SupportArmIllustration(): ReactNode {
  return (
    <svg width="112" height="96" viewBox="0 0 112 96" fill="none" aria-hidden="true" className="mx-auto">
      <circle cx="56" cy="50" r="44" fill="var(--blue-tint)" />
      <rect x="34" y="78" width="44" height="9" rx="3" fill="var(--ink)" stroke="var(--ink)" strokeWidth="2.5" />
      <path d="M55.8 76.38 L44 46 L84 30" stroke="var(--blue-ink)" strokeWidth="8" strokeLinecap="round" strokeLinejoin="round" />
      <circle cx="44" cy="46" r="4.5" fill="var(--ink)" />
      <circle cx="56" cy="76" r="4" fill="var(--surface)" stroke="var(--ink)" strokeWidth="2.5" />
      <path d="M84 30 l8 -7 M84 30 l10 2" stroke="var(--ink)" strokeWidth="3.5" strokeLinecap="round" />
      <SupportHand transform="translate(16 37) scale(1.25 1.25)" />
      <SupportHand transform="translate(100 22) scale(-1.25 1.25)" />
      <g transform="translate(6 6)">
        <circle cx="10" cy="11" r="12" fill="var(--surface)" stroke="var(--orange)" strokeWidth="2.5" />
        <path d="M6 10 V7 a4 4 0 0 1 7.5 -2" stroke="var(--ink)" strokeWidth="2.2" strokeLinecap="round" />
        <rect x="4" y="10" width="12" height="9" rx="2" fill="var(--ink)" />
      </g>
    </svg>
  );
}

export function ReleaseDialogs({ v }: Props): ReactNode {
  const { releaseConfirmOpen, releaseCountdown, releaseCountdownSteps, loopFromBoardConfirmOpen } = v.dialogs;
  const actions = v.dialogActions;
  return (
    <>
      {releaseConfirmOpen ? (
        <Modal
          fixed
          width={480}
          title={translate("robot.workspace.messages.supportTheArmFirst")}
          titleId="release-confirm-title"
          actions={(
            <>
              <Button variant="secondary" size="md" onClick={actions.cancelReleaseConfirm}>
                {translate("common.cancel")}
              </Button>
              <Button variant="blue" size="md" autoFocus onClick={actions.confirmRelease}>
                {translate("robot.workspace.messages.armSupportedReleaseJoints")}
              </Button>
            </>
          )}
        >
          <SupportArmIllustration />
          <div className="text-[16px] leading-[1.6] text-ink">
            {translate("robot.workspace.messages.releasingJointsRemovesTorqueTheArmMayFallImmediately")}
            <br />
            {translate("robot.workspace.messages.pleaseUse")} <b>{translate("robot.workspace.messages.bothHands")}</b>{" "}
            {translate("robot.workspace.messages.toSupportTheArmBeforePressingRelease")}
          </div>
        </Modal>
      ) : null}
      {releaseCountdown !== null ? (
        <div className="fixed inset-0 z-[60] flex items-center justify-center bg-[rgba(var(--ink-rgb),_0.35)]">
          <div role="alertdialog" className="box-border w-[400px] rounded-[22px] bg-white p-[24px] text-center">
            <div className="text-[20px] font-bold">{translate("robot.workspace.messages.supportTheArmJointsWillLockIn")}</div>
            <div className="text-[88px] leading-[1.1] font-bold text-ink">{T(releaseCountdown)}</div>
            <div className="mx-0 mt-[-4px] mb-[10px] text-[15px] font-bold text-muted">
              {translate("robot.workspace.messages.seconds")}
            </div>
            <div className="mb-[10px] flex justify-center gap-[6px]">
              {releaseCountdownSteps.map((lit, index) => (
                <span
                  key={index}
                  className={`h-[10px] w-[56px] rounded-[5px] ${lit ? "bg-orange" : "bg-line-strong"}`}
                />
              ))}
            </div>
            <div className="text-muted">{translate("robot.workspace.messages.theArmWillHoldItsCurrentPose")}</div>
            <Button variant="secondary" size="md" className="mt-[14px]" onClick={actions.keepJointsReleased}>
              {translate("robot.workspace.messages.cancelKeepUsing")}
            </Button>
          </div>
        </div>
      ) : null}
      {loopFromBoardConfirmOpen ? (
        <Modal
          fixed
          width={460}
          title={translate("robot.workspace.messages.loopContinuouslyFromBoard")}
          titleId="loop-from-board-title"
          actions={(
            <>
              <Button variant="secondary" size="md" onClick={actions.cancelLoopFromBoard}>
                {translate("common.cancel")}
              </Button>
              <Button variant="blue" size="md" onClick={actions.confirmLoopFromBoard}>
                {translate("robot.workspace.messages.understoodPlayNow")}
              </Button>
            </>
          )}
        >
          <div className="text-[15px] leading-[1.7] text-ink">
            {translate("robot.workspace.messages.theRobotWillKeepLoopingFromItsMemoryEven")}
            <br />
            {translate("robot.workspace.messages.stopUsing")} <b>{translate("common.stop")}</b>{" "}
            {translate("robot.workspace.messages.onScreenWhileConnectedTheBoardButtonOrThe")}
          </div>
        </Modal>
      ) : null}
    </>
  );
}
