


import { t as translate } from "../i18n";
import { Icon } from "../components/shared/Icons";
import { Button, Modal } from "../components/shared";
import { STUDENT_NAME } from "../shell";

import type { HomeView } from "./HomeView";

const bodyTextClass =
  "sb-t-body not-italic [font-variant:normal] font-normal [font-stretch:normal] text-[16px] leading-[1.5] " +
  "font-body sb-muted text-muted";

const kindCardClass =
  "sb-card bg-surface border-2 border-solid border-line rounded-card shadow-card " +
  "[transition:border-color_.12s,transform_.12s] [&.hm-wcard]:h-[252px] [&.hm-wcard]:min-h-0 [&.hm-wcard]:p-0 " +
  "[&.hm-wcard]:flex [&.hm-wcard]:flex-col [&.hm-wcard]:[align-items:stretch] [&.hm-wcard]:overflow-hidden " +
  "[&.hm-wcard]:text-left [&.hm-kind]:flex [&.hm-kind]:flex-col [&.hm-kind]:items-start [&.hm-kind]:gap-[8px] " +
  "[&.hm-kind]:p-[20px] [&.hm-kind]:text-left [&.hm-kind]:min-h-[190px] [&.hm-kind]:bg-surface " +
  "[&.hm-kind:hover]:bg-blue-tint motion-reduce:[&.hm-wcard]:[transition:none] sb-card-hit cursor-pointer block " +
  "text-[inherit] no-underline [font-weight:inherit] hover:border-blue hover:[transform:translateY(-2px)] " +
  "focus-visible:outline-none focus-visible:shadow-[var(--focus)] active:[transform:translateY(1px)] hm-kind " +
  "[&_b]:not-italic [&_b]:[font-variant:normal] [&_b]:font-bold [&_b]:[font-stretch:normal] [&_b]:text-[20px] " +
  "[&_b]:leading-[1.3] [&_b]:font-heading";

const kindIconBaseClass =
  "hm-kicon w-[64px] h-[64px] rounded-[16px] flex items-center justify-center [&.is-blue]:bg-blue-tint " +
  "[&.is-blue]:text-blue-ink [&.is-orange]:bg-orange-tint [&.is-orange]:text-orange-deep";

const toastClass =
  "sb-toast inline-flex items-center gap-[14px] min-h-[52px] pt-[8px] pr-[10px] pb-[8px] pl-[18px] rounded-control " +
  "bg-ink text-white not-italic [font-variant:normal] font-semibold [font-stretch:normal] text-[16px] leading-[1.3] " +
  "font-body shadow-pop [&_.sb-btn]:min-h-[44px]";

type Props = { v: Pick<Parameters<typeof HomeView>[0]["v"], "kindOpen" | "setKindOpen" | "toast" | "startNew">;  };

export function HomeDialogs({ v }: Props) {
 const { kindOpen, setKindOpen, toast, startNew } = v;
 return (<>
{kindOpen && (
        <Modal
          fixed
          width={640}
          tone="info"
          title={translate("modal.projectType.title")}
          titleId="kind-title"
          actions={
            <Button variant="secondary" size="md" onClick={() => setKindOpen(false)}>
              {translate("common.cancel")}
            </Button>
          }
        >
          <div className={bodyTextClass}>
            {translate("home.messages.aNewProjectFor")}{STUDENT_NAME}{translate("home.messages.howWouldYouLikeToStart")}
          </div>
          <div className={"hm-kinds grid [grid-template-columns:1fr_1fr] gap-[16px]"}>
            <button
              type="button"
              className={kindCardClass}
              onClick={() => startNew("block-programming")}
            >
              <span className={`${kindIconBaseClass} is-orange`}>
                <Icon name="block" size={38} />
              </span>
              <b>{"Block Lab"}</b>
              <span className={bodyTextClass}>{translate("home.messages.connectBlocksForTheRobotToFollowStepBy")}</span>
            </button>
            <button
              type="button"
              className={kindCardClass}
              onClick={() => startNew("robot-control")}
            >
              <span className={`${kindIconBaseClass} is-blue`}>
                <Icon name="arm" size={38} />
              </span>
              <b>{"Arm Lab"}</b>
              <span className={bodyTextClass}>{translate("home.messages.arrangeArmPosesThenPressPlay")}</span>
            </button>
          </div>
        </Modal>
      )}
{toast && (
        <div className={"hm-toast fixed left-[50%] bottom-[32px] [transform:translateX(-50%)] z-[60]"}>
          <div className={toastClass} role="status">
            <span>{translate(toast)}</span>
          </div>
        </div>
      )}
</>);
}
