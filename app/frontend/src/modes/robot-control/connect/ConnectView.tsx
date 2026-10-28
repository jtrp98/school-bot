import { Button } from "../../../components/shared";
import { TimedProgressBar } from "../../../components/shared/TimedProgressBar";
import { ConnectBody } from "./ConnectBody";

import { t as translate } from "../../../i18n";
import type { ReactNode } from "react";
import logoArmLab from "../../../assets/logo-arm-lab.svg";
import logoSchoolBot from "../../../assets/logo-school-bot.svg";

import { WindowControlsSlot } from "../../../components/shared/WindowTitlebar";
import { ScenarioBar } from "../../../components/shared/ScenarioBar";
import { BAUD, MS, SCENARIOS } from "./connectData";
import { DevPanel } from "./parts";
import { ConnectionMascot } from "../../../components/shared/ConnectionMascot";
import { CheckIcon } from "../../../components/robot-control/ConnectionIcons";
import { nameOf } from "./connectState";

import type { useConnectScreen } from "./ConnectScreen";

const CONNECT_VIEW_ROOT_CLASS = [
  "cn-root [--cn-ink:var(--ink)] [--cn-muted:var(--muted)] [--cn-line:var(--line)]",
  "[--cn-blue:var(--blue-ink)] [--cn-green:var(--ok)] relative box-border w-full",
  "h-[var(--app-viewport-height,_100vh)] [flex:none] bg-bg",
  "font-[family-name:'Noto_Sans_Thai',_'Leelawadee_UI',_sans-serif] text-[var(--cn-ink)]",
  "leading-[normal] flex flex-col [&_*]:box-border [&_*::before]:box-border [&_*::after]:box-border",
  "[&_button]:font-[family-name:inherit] [&_button]:leading-[normal]",
  "[&_select]:font-[family-name:inherit] [&_select]:leading-[normal]",
  "[&_input]:font-[family-name:inherit] [&_input]:leading-[normal] [&_button]:min-h-0 [&_button]:m-0",
  "[&_button]:font-normal [&_button:disabled]:opacity-[1]",
  "[&_:focus-visible]:[outline:3px_solid_var(--blue-ink)] [&_:focus-visible]:[outline-offset:2px]",
  "[&_.cn-in]:animate-[cnIn_.2s_ease-out_both] [&_.cn-bar]:animate-[cnBar_2s_linear_both]",
  "[&_.cn-bar.is-hold]:animate-none [&_.cn-btn]:[transition:transform_.08s_ease,_filter_.12s_ease]",
  "[&_.cn-btn]:cursor-pointer [&_.cn-btn:hover:not(:disabled)]:[filter:brightness(1.04)]",
  "[&_.cn-btn:active:not(:disabled)]:[transform:translateY(2px)]",
  "[&_.cn-mitr]:font-[family-name:'Mitr',_'Noto_Sans_Thai',_sans-serif] [&_.cn-name:focus]:outline-none",
  "[&_.cn-name:focus]:border-blue-ink! [&_.cn-name:focus]:shadow-[0_0_0_3px_var(--blue-tint)]",
  "motion-reduce:[&_*]:animate-none! motion-reduce:[&_*]:[transition:none]! [&_.cn-head]:h-[76px]",
  "[&_.cn-head]:shrink-0 [&_.cn-head]:py-0 [&_.cn-head]:px-[24px] [&_.cn-head]:bg-white",
  "[&_.cn-head]:border-b-2 [&_.cn-head]:[border-bottom-style:solid] [&_.cn-head]:border-b-line",
  "[&_.cn-head]:flex [&_.cn-head]:items-center [&_.cn-head]:gap-[12px] [&_.cn-brand]:flex",
  "[&_.cn-brand]:items-center [&_.cn-brand]:gap-[10px] [&_.cn-brand]:mr-[12px]",
  "[&_.cn-logo-sb]:w-[192px] [&_.cn-logo-sb]:h-[34px] [&_.cn-logo-sb]:object-contain",
  "[&_.cn-logo-sb]:[object-position:0_50%] [&_.cn-logo-sb]:shrink-0 [&_.cn-logo-arm]:w-[100px]",
  "[&_.cn-logo-arm]:h-[25px] [&_.cn-logo-arm]:shrink-0 [&_.cn-logo-arm]:block [&_.cn-logo-arm]:ml-[2px]",
  "[&_.cn-chip]:py-[8px] [&_.cn-chip]:px-[12px] [&_.cn-chip]:rounded-full [&_.cn-chip]:text-[14px]",
  "[&_.cn-chip]:font-semibold [&_.cn-chip]:bg-blue-tint [&_.cn-chip]:text-muted",
  "[&_.cn-chip.is-on]:bg-ok-tint [&_.cn-chip.is-on]:text-ok-ink [&_.cn-grow]:grow [&_.cn-stopbox]:flex",
  "[&_.cn-stopbox]:flex-col [&_.cn-stopbox]:items-center [&_.cn-stopbox]:gap-[2px]",
  "[&_.cn-stop]:h-[50px] [&_.cn-stop]:py-0 [&_.cn-stop]:px-[22px] [&_.cn-stop]:rounded-[14px]",
  "[&_.cn-stop]:text-[17px] [&_.cn-stop]:font-bold [&_.cn-stop]:border-[medium]",
  "[&_.cn-stop]:border-none [&_.cn-stop]:border-current [&_.cn-stop]:border-b-4",
  "[&_.cn-stop]:[border-bottom-style:solid] [&_.cn-stop]:border-b-err-ink [&_.cn-stop]:bg-err",
  "[&_.cn-stop]:text-white [&_.cn-stop:disabled]:border-b-[0] [&_.cn-stop:disabled]:bg-off",
  "[&_.cn-stop:disabled]:text-faint [&_.cn-stop:disabled]:cursor-not-allowed",
  "[&_.cn-stopnote]:text-[11px] [&_.cn-stopnote]:text-muted [&_.cn-main]:grow [&_.cn-main]:min-h-0",
  "[&_.cn-main]:overflow-y-auto [&_.cn-main]:pt-[28px] [&_.cn-main]:px-[120px] [&_.cn-main]:pb-[16px]",
  "[&_.cn-main]:flex [&_.cn-main]:flex-col [&_.cn-main]:gap-[18px] [&_.cn-main_>_*]:shrink-0",
  "[&_.cn-sayrow]:flex [&_.cn-sayrow]:items-center [&_.cn-sayrow]:gap-[20px] [&_.cn-say]:bg-white",
  "[&_.cn-say]:border-2 [&_.cn-say]:border-solid [&_.cn-say]:border-line [&_.cn-say]:rounded-[18px]",
  "[&_.cn-say]:py-[14px] [&_.cn-say]:px-[22px] [&_.cn-say]:max-w-[760px] [&_.cn-say-t]:text-[24px]",
  "[&_.cn-say-t]:font-medium [&_.cn-say-t]:block [&_.cn-say-s]:text-[16px] [&_.cn-say-s]:text-muted",
  "[&_.cn-conn]:flex [&_.cn-conn]:flex-col [&_.cn-conn]:gap-[12px] [&_.cn-conn]:bg-ok-tint",
  "[&_.cn-conn]:border-2 [&_.cn-conn]:border-solid [&_.cn-conn]:border-ok-line",
  "[&_.cn-conn]:rounded-[20px] [&_.cn-conn]:py-[16px] [&_.cn-conn]:px-[20px] [&_.cn-conn-top]:flex",
  "[&_.cn-conn-top]:items-center [&_.cn-conn-top]:gap-[16px] [&_.cn-conn-ic]:w-[48px]",
  "[&_.cn-conn-ic]:h-[48px] [&_.cn-conn-ic]:rounded-full [&_.cn-conn-ic]:bg-white [&_.cn-conn-ic]:flex",
  "[&_.cn-conn-ic]:items-center [&_.cn-conn-ic]:justify-center [&_.cn-conn-ic]:shrink-0",
  "[&_.cn-col]:flex [&_.cn-col]:flex-col [&_.cn-conn-txt]:grow [&_.cn-conn-txt]:min-w-0",
  "[&_.cn-conn-t]:text-[22px] [&_.cn-conn-t]:font-medium [&_.cn-conn-t]:text-ok-ink",
  "[&_.cn-conn-s]:text-[14px] [&_.cn-conn-s]:text-ok-ink [&_.cn-disc]:h-[52px] [&_.cn-disc]:py-0",
  "[&_.cn-disc]:px-[20px] [&_.cn-disc]:border-2 [&_.cn-disc]:border-solid [&_.cn-disc]:border-ok-line",
  "[&_.cn-disc]:border-b-4 [&_.cn-disc]:rounded-[14px] [&_.cn-disc]:bg-white [&_.cn-disc]:text-ink",
  "[&_.cn-disc]:text-[16px] [&_.cn-disc]:font-bold [&_.cn-disc]:whitespace-nowrap [&_.cn-go]:h-[56px]",
  "[&_.cn-go]:py-0 [&_.cn-go]:px-[28px] [&_.cn-go]:border-[medium] [&_.cn-go]:border-none",
  "[&_.cn-go]:border-current [&_.cn-go]:border-b-[5px] [&_.cn-go]:[border-bottom-style:solid]",
  "[&_.cn-go]:border-b-ok-ink [&_.cn-go]:rounded-[16px] [&_.cn-go]:bg-ok [&_.cn-go]:text-white",
  "[&_.cn-go]:text-[18px] [&_.cn-go]:font-bold [&_.cn-go]:flex [&_.cn-go]:items-center",
  "[&_.cn-go]:gap-[8px] [&_.cn-go]:no-underline [&_.cn-go]:whitespace-nowrap [&_.cn-fold]:flex",
  "[&_.cn-fold]:items-center [&_.cn-fold]:gap-[10px] [&_.cn-fold]:text-[14px] [&_.cn-fold]:text-ok-ink",
  "[&_.cn-fold]:border-t-1 [&_.cn-fold]:[border-top-style:dashed] [&_.cn-fold]:border-t-ok-line",
  "[&_.cn-fold]:pt-[10px] [&_.cn-fold-b]:h-[36px] [&_.cn-fold-b]:py-0 [&_.cn-fold-b]:px-[14px]",
  "[&_.cn-fold-b]:border-2 [&_.cn-fold-b]:border-solid [&_.cn-fold-b]:border-ok-line",
  "[&_.cn-fold-b]:rounded-[10px] [&_.cn-fold-b]:bg-white [&_.cn-fold-b]:text-ok-ink",
  "[&_.cn-fold-b]:text-[14px] [&_.cn-fold-b]:font-bold [&_.cn-fold-b]:flex [&_.cn-fold-b]:items-center",
  "[&_.cn-fold-b]:gap-[6px] [&_.cn-fold-n]:text-blue-ink [&_.cn-ask]:flex [&_.cn-ask]:items-center",
  "[&_.cn-ask]:gap-[14px] [&_.cn-ask]:bg-orange-tint [&_.cn-ask]:border-2 [&_.cn-ask]:border-solid",
  "[&_.cn-ask]:border-orange-line [&_.cn-ask]:rounded-[14px] [&_.cn-ask]:py-[12px]",
  "[&_.cn-ask]:px-[16px] [&_.cn-ask-txt]:grow [&_.cn-ask-t]:text-[16px] [&_.cn-ask-t]:font-bold",
  "[&_.cn-ask-t]:text-orange-deep [&_.cn-ask-b]:text-[14px] [&_.cn-ask-b]:text-orange-deep",
  "[&_.cn-ask-cancel]:h-[46px] [&_.cn-ask-cancel]:py-0 [&_.cn-ask-cancel]:px-[16px]",
  "[&_.cn-ask-cancel]:border-2 [&_.cn-ask-cancel]:border-solid [&_.cn-ask-cancel]:border-orange-line",
  "[&_.cn-ask-cancel]:border-b-4 [&_.cn-ask-cancel]:rounded-[12px] [&_.cn-ask-cancel]:bg-white",
  "[&_.cn-ask-cancel]:text-orange-deep [&_.cn-ask-cancel]:text-[15px] [&_.cn-ask-cancel]:font-semibold",
  "[&_.cn-ask-up]:h-[46px] [&_.cn-ask-up]:py-0 [&_.cn-ask-up]:px-[16px] [&_.cn-ask-up]:border-2",
  "[&_.cn-ask-up]:border-solid [&_.cn-ask-up]:border-blue-line [&_.cn-ask-up]:border-b-4",
  "[&_.cn-ask-up]:rounded-[12px] [&_.cn-ask-up]:bg-white [&_.cn-ask-up]:text-blue-ink",
  "[&_.cn-ask-up]:text-[15px] [&_.cn-ask-up]:font-bold [&_.cn-ask-now]:h-[46px] [&_.cn-ask-now]:py-0",
  "[&_.cn-ask-now]:px-[18px] [&_.cn-ask-now]:border-[medium] [&_.cn-ask-now]:border-none",
  "[&_.cn-ask-now]:border-current [&_.cn-ask-now]:border-b-4",
  "[&_.cn-ask-now]:[border-bottom-style:solid] [&_.cn-ask-now]:border-b-blue-press",
  "[&_.cn-ask-now]:rounded-[12px] [&_.cn-ask-now]:bg-blue-ink [&_.cn-ask-now]:text-white",
  "[&_.cn-ask-now]:text-[15px] [&_.cn-ask-now]:font-bold [&_.cn-busy]:flex [&_.cn-busy]:flex-col",
  "[&_.cn-busy]:gap-[6px] [&_.cn-busy]:bg-white [&_.cn-busy]:rounded-[12px] [&_.cn-busy]:py-[10px]",
  "[&_.cn-busy]:px-[14px] [&_.cn-busy-t]:text-[15px] [&_.cn-busy-t]:font-bold [&_.cn-busy-t]:text-ink",
  "[&_.cn-track]:h-[8px] [&_.cn-track]:rounded-full [&_.cn-track]:bg-line-strong",
  "[&_.cn-track]:overflow-hidden [&_.cn-track_>_div]:h-full [&_.cn-track_>_div]:rounded-full",
  "[&_.cn-track_>_div]:bg-ok [&_.cn-done]:flex [&_.cn-done]:items-center [&_.cn-done]:gap-[14px]",
  "[&_.cn-done]:bg-white [&_.cn-done]:border-2 [&_.cn-done]:border-solid [&_.cn-done]:border-line",
  "[&_.cn-done]:rounded-[16px] [&_.cn-done]:py-[12px] [&_.cn-done]:px-[18px] [&_.cn-done-t]:text-[17px]",
  "[&_.cn-done-t]:font-bold [&_.cn-done-s]:text-[14px] [&_.cn-done-s]:text-muted [&_.cn-sec]:flex",
  "[&_.cn-sec]:items-center [&_.cn-sec]:gap-[12px] [&_.cn-sec-t]:text-[17px] [&_.cn-sec-t]:font-bold",
  "[&_.cn-sec-n]:text-[13px] [&_.cn-sec-n]:text-muted [&_.cn-rescan]:h-[40px] [&_.cn-rescan]:py-0",
  "[&_.cn-rescan]:px-[14px] [&_.cn-rescan]:border-2 [&_.cn-rescan]:border-solid",
  "[&_.cn-rescan]:border-line-strong [&_.cn-rescan]:border-b-4 [&_.cn-rescan]:rounded-[12px]",
  "[&_.cn-rescan]:bg-white [&_.cn-rescan]:text-blue-ink [&_.cn-rescan]:text-[14px]",
  "[&_.cn-rescan]:font-bold [&_.cn-rescan]:flex [&_.cn-rescan]:items-center [&_.cn-rescan]:gap-[6px]",
  "[&_.cn-cards]:flex [&_.cn-cards]:gap-[10px] [&_.cn-cards]:flex-wrap [&_.cn-card]:flex",
  "[&_.cn-card]:items-center [&_.cn-card]:gap-[12px] [&_.cn-card]:py-[10px] [&_.cn-card]:px-[12px]",
  "[&_.cn-card]:rounded-[12px] [&_.cn-card]:border-2 [&_.cn-card]:border-solid [&_.cn-card]:border-line",
  "[&_.cn-card]:bg-white [&_.cn-card.is-on]:border-blue-ink [&_.cn-card.is-on]:bg-blue-tint",
  "[&_.cn-card.is-use]:border-ok [&_.cn-card.is-use]:bg-ok-tint [&_.cn-card.is-other]:py-[8px]",
  "[&_.cn-card.is-other]:px-[12px] [&_.cn-card-txt]:flex [&_.cn-card-txt]:flex-col",
  "[&_.cn-card-txt]:leading-[1.3] [&_.cn-card-n]:text-[15px] [&_.cn-card-n]:font-bold",
  "[&_.cn-card-s]:text-[12px] [&_.cn-card-s]:text-muted [&_.cn-card-n.is-other]:text-[14px]",
  "[&_.cn-card-n.is-other]:font-semibold [&_.cn-tag]:text-[12px] [&_.cn-tag]:font-bold",
  "[&_.cn-tag]:rounded-full [&_.cn-tag]:py-[2px] [&_.cn-tag]:px-[10px] [&_.cn-tag]:bg-blue-tint",
  "[&_.cn-tag]:text-muted [&_.cn-tag.is-ok]:bg-ok-tint [&_.cn-tag.is-ok]:text-ok-ink",
  "[&_.cn-pick]:h-[30px] [&_.cn-pick]:py-0 [&_.cn-pick]:px-[10px] [&_.cn-pick]:border-2",
  "[&_.cn-pick]:border-solid [&_.cn-pick]:border-line [&_.cn-pick]:rounded-[9px] [&_.cn-pick]:bg-white",
  "[&_.cn-pick]:text-ink [&_.cn-pick]:text-[12px] [&_.cn-pick]:font-semibold",
  "[&_.cn-pick]:cursor-pointer [&_.cn-pick:disabled]:[cursor:default]",
  "[&_.cn-idbtn]:[align-self:flex-start] [&_.cn-idbtn]:h-[44px] [&_.cn-idbtn]:py-0",
  "[&_.cn-idbtn]:px-[16px] [&_.cn-idbtn]:border-2 [&_.cn-idbtn]:border-solid",
  "[&_.cn-idbtn]:border-line-strong [&_.cn-idbtn]:border-b-4 [&_.cn-idbtn]:rounded-[12px]",
  "[&_.cn-idbtn]:bg-white [&_.cn-idbtn]:text-blue-ink [&_.cn-idbtn]:text-[15px] [&_.cn-idbtn]:font-bold",
  "[&_.cn-id]:flex [&_.cn-id]:items-center [&_.cn-id]:gap-[16px] [&_.cn-id]:rounded-[16px]",
  "[&_.cn-id]:py-[12px] [&_.cn-id]:px-[18px] [&_.cn-id]:bg-blue-tint [&_.cn-id]:border-2",
  "[&_.cn-id]:border-solid [&_.cn-id]:border-blue-ink [&_.cn-id.is-warn]:bg-orange-tint",
  "[&_.cn-id.is-warn]:border-orange-line [&_.cn-id-no]:w-[36px] [&_.cn-id-no]:h-[36px]",
  "[&_.cn-id-no]:rounded-full [&_.cn-id-no]:text-white [&_.cn-id-no]:flex [&_.cn-id-no]:items-center",
  "[&_.cn-id-no]:justify-center [&_.cn-id-no]:font-bold [&_.cn-id-no]:shrink-0",
  "[&_.cn-id-no]:bg-blue-ink [&_.cn-id.is-warn_.cn-id-no]:bg-orange [&_.cn-id-txt]:flex",
  "[&_.cn-id-txt]:flex-col [&_.cn-id-txt]:grow [&_.cn-id-t]:text-[17px] [&_.cn-id-t]:font-bold",
  "[&_.cn-id-t]:text-blue-press [&_.cn-id.is-warn_.cn-id-t]:text-orange-deep [&_.cn-id-b]:text-[14px]",
  "[&_.cn-id-b]:text-ink [&_.cn-id-act]:h-[44px] [&_.cn-id-act]:py-0 [&_.cn-id-act]:px-[16px]",
  "[&_.cn-id-act]:border-2 [&_.cn-id-act]:border-solid [&_.cn-id-act]:border-blue-line",
  "[&_.cn-id-act]:border-b-4 [&_.cn-id-act]:rounded-[12px] [&_.cn-id-act]:bg-white",
  "[&_.cn-id-act]:text-blue-ink [&_.cn-id-act]:text-[14px] [&_.cn-id-act]:font-bold",
  "[&_.cn-id-act]:whitespace-nowrap [&_.cn-id-cancel]:h-[44px] [&_.cn-id-cancel]:py-0",
  "[&_.cn-id-cancel]:px-[16px] [&_.cn-id-cancel]:border-2 [&_.cn-id-cancel]:border-solid",
  "[&_.cn-id-cancel]:border-line-strong [&_.cn-id-cancel]:border-b-4 [&_.cn-id-cancel]:rounded-[12px]",
  "[&_.cn-id-cancel]:bg-white [&_.cn-id-cancel]:text-ink [&_.cn-id-cancel]:text-[14px]",
  "[&_.cn-id-cancel]:font-semibold [&_.cn-tips]:grid",
  "[&_.cn-tips]:[grid-template-columns:repeat(3,_minmax(0,_1fr))] [&_.cn-tips]:gap-[12px]",
  "[&_.cn-tip]:bg-white [&_.cn-tip]:border-2 [&_.cn-tip]:border-solid [&_.cn-tip]:border-line",
  "[&_.cn-tip]:rounded-[14px] [&_.cn-tip]:p-[12px] [&_.cn-tip]:text-[14px] [&_.cn-tip]:leading-[1.45]",
  "[&_.cn-empty]:[box-sizing:content-box] [&_.cn-empty]:h-[180px] [&_.cn-empty]:border-[3px]",
  "[&_.cn-empty]:border-dashed [&_.cn-empty]:border-line-strong [&_.cn-empty]:rounded-[22px]",
  "[&_.cn-empty]:bg-white [&_.cn-empty]:flex [&_.cn-empty]:flex-col [&_.cn-empty]:items-center",
  "[&_.cn-empty]:justify-center [&_.cn-empty]:gap-[6px] [&_.cn-empty-t]:text-[24px]",
  "[&_.cn-empty-t]:font-medium [&_.cn-empty-s]:text-[15px] [&_.cn-empty-s]:text-muted",
  "[&_.cn-others]:flex [&_.cn-others]:flex-col [&_.cn-others]:gap-[8px]",
  "[&_.cn-others-t]:[align-self:flex-start] [&_.cn-others-t]:h-[34px] [&_.cn-others-t]:py-0",
  "[&_.cn-others-t]:px-[6px] [&_.cn-others-t]:border-[medium] [&_.cn-others-t]:border-none",
  "[&_.cn-others-t]:border-current [&_.cn-others-t]:bg-transparent [&_.cn-others-t]:text-muted",
  "[&_.cn-others-t]:text-[14px] [&_.cn-others-t]:font-semibold [&_.cn-err]:flex [&_.cn-err]:flex-col",
  "[&_.cn-err]:gap-[12px] [&_.cn-err]:bg-err-tint [&_.cn-err]:rounded-[16px] [&_.cn-err]:py-[14px]",
  "[&_.cn-err]:px-[18px] [&_.cn-err-top]:flex [&_.cn-err-top]:items-center [&_.cn-err-top]:gap-[16px]",
  "[&_.cn-err-txt]:flex [&_.cn-err-txt]:flex-col [&_.cn-err-txt]:grow [&_.cn-err-t]:text-[18px]",
  "[&_.cn-err-t]:font-bold [&_.cn-err-t]:text-err-ink [&_.cn-err-b]:text-[15px]",
  "[&_.cn-err-b]:text-err-ink [&_.cn-code]:flex [&_.cn-code]:flex-col [&_.cn-code]:items-center",
  "[&_.cn-code]:py-[6px] [&_.cn-code]:px-[12px] [&_.cn-code]:rounded-[10px] [&_.cn-code]:bg-white",
  "[&_.cn-code]:border-2 [&_.cn-code]:border-dashed [&_.cn-code]:border-err-line",
  "[&_.cn-code-a]:text-[11px] [&_.cn-code-a]:text-err-ink",
  "[&_.cn-code-b]:font-[family-name:ui-monospace,_Menlo,_Consolas,_monospace]",
  "[&_.cn-code-b]:text-[16px] [&_.cn-code-b]:font-bold [&_.cn-code-b]:text-err-ink",
  "[&_.cn-retry]:h-[50px] [&_.cn-retry]:py-0 [&_.cn-retry]:px-[20px] [&_.cn-retry]:border-[medium]",
  "[&_.cn-retry]:border-none [&_.cn-retry]:border-current [&_.cn-retry]:border-b-4",
  "[&_.cn-retry]:[border-bottom-style:solid] [&_.cn-retry]:border-b-blue-press",
  "[&_.cn-retry]:rounded-[14px] [&_.cn-retry]:bg-blue-ink [&_.cn-retry]:text-white",
  "[&_.cn-retry]:text-[16px] [&_.cn-retry]:font-bold [&_.cn-deep]:flex [&_.cn-deep]:flex-col",
  "[&_.cn-deep]:gap-[8px] [&_.cn-deep]:border-t-1 [&_.cn-deep]:[border-top-style:dashed]",
  "[&_.cn-deep]:border-t-err-line [&_.cn-deep]:pt-[10px] [&_.cn-deep-t]:text-[15px]",
  "[&_.cn-deep-t]:font-bold [&_.cn-deep-t]:text-err-ink [&_.cn-steps]:grid",
  "[&_.cn-steps]:[grid-template-columns:repeat(2,_minmax(0,_1fr))] [&_.cn-steps]:gap-[8px]",
  "[&_.cn-step]:flex [&_.cn-step]:items-center [&_.cn-step]:gap-[10px] [&_.cn-step]:py-[10px]",
  "[&_.cn-step]:px-[12px] [&_.cn-step]:rounded-[12px] [&_.cn-step]:cursor-pointer [&_.cn-step]:text-ink",
  "[&_.cn-step]:border-2 [&_.cn-step]:border-solid [&_.cn-step]:border-err-line [&_.cn-step]:bg-white",
  "[&_.cn-step]:text-left [&_.cn-step.is-done]:border-ok-line [&_.cn-step.is-done]:bg-ok-tint",
  "[&_.cn-step-bx]:w-[28px] [&_.cn-step-bx]:h-[28px] [&_.cn-step-bx]:rounded-[8px]",
  "[&_.cn-step-bx]:shrink-0 [&_.cn-step-bx]:flex [&_.cn-step-bx]:items-center",
  "[&_.cn-step-bx]:justify-center [&_.cn-step-bx]:font-bold [&_.cn-step-bx]:bg-err-tint",
  "[&_.cn-step-bx]:text-err-ink [&_.cn-step.is-done_.cn-step-bx]:bg-ok",
  "[&_.cn-step.is-done_.cn-step-bx]:text-white [&_.cn-step-txt]:flex [&_.cn-step-txt]:flex-col",
  "[&_.cn-step-txt]:text-left [&_.cn-step-a]:text-[15px] [&_.cn-step-a]:font-bold",
  "[&_.cn-step-b]:text-[13px] [&_.cn-step-b]:text-muted [&_.cn-help]:flex [&_.cn-help]:flex-col",
  "[&_.cn-help]:gap-[10px] [&_.cn-help-t]:text-[15px] [&_.cn-help-t]:font-bold [&_.cn-help-g]:grid",
  "[&_.cn-help-g]:[grid-template-columns:repeat(3,_minmax(0,_1fr))] [&_.cn-help-g]:gap-[16px]",
  "[&_.cn-help-c]:bg-white [&_.cn-help-c]:border-2 [&_.cn-help-c]:border-solid",
  "[&_.cn-help-c]:border-line [&_.cn-help-c]:border-b-4 [&_.cn-help-c]:rounded-[16px]",
  "[&_.cn-help-c]:p-[14px] [&_.cn-help-c]:flex [&_.cn-help-c]:gap-[12px] [&_.cn-help-c]:items-center",
  "[&_.cn-help-n]:w-[32px] [&_.cn-help-n]:h-[32px] [&_.cn-help-n]:rounded-full",
  "[&_.cn-help-n]:bg-blue-tint [&_.cn-help-n]:flex [&_.cn-help-n]:items-center",
  "[&_.cn-help-n]:justify-center [&_.cn-help-n]:font-bold [&_.cn-help-n]:shrink-0",
  "[&_.cn-help-x]:text-[15px] [&_.cn-help-x]:leading-[1.4] [&_.cn-foot]:h-[112px] [&_.cn-foot]:shrink-0",
  "[&_.cn-foot]:border-t-2 [&_.cn-foot]:[border-top-style:solid] [&_.cn-foot]:border-t-line",
  "[&_.cn-foot]:bg-white [&_.cn-foot]:py-0 [&_.cn-foot]:px-[120px] [&_.cn-foot]:flex",
  "[&_.cn-foot]:items-center [&_.cn-foot]:gap-[16px] [&_.cn-foot.is-ok]:bg-ok-tint",
  "[&_.cn-nameform]:flex [&_.cn-nameform]:items-center [&_.cn-nameform]:gap-[14px]",
  "[&_.cn-nameform-t]:text-[20px] [&_.cn-nameform-t]:font-semibold [&_.cn-nameform-t]:text-ok-ink",
  "[&_.cn-nameform-s]:text-[13px] [&_.cn-nameform-s]:text-ok-ink [&_.cn-name]:h-[52px]",
  "[&_.cn-name]:w-[240px] [&_.cn-name]:border-2 [&_.cn-name]:border-solid [&_.cn-name]:border-ok-line",
  "[&_.cn-name]:rounded-[12px] [&_.cn-name]:py-0 [&_.cn-name]:px-[14px] [&_.cn-name]:text-[18px]",
  "[&_.cn-name]:font-bold [&_.cn-name]:text-ink [&_.cn-name]:bg-white [&_.cn-skip]:h-[52px]",
  "[&_.cn-skip]:py-0 [&_.cn-skip]:px-[16px] [&_.cn-skip]:border-2 [&_.cn-skip]:border-solid",
  "[&_.cn-skip]:border-ok-line [&_.cn-skip]:border-b-4 [&_.cn-skip]:rounded-[14px]",
  "[&_.cn-skip]:bg-white [&_.cn-skip]:text-ok-ink [&_.cn-skip]:text-[15px] [&_.cn-skip]:font-semibold",
  "[&_.cn-save]:h-[56px] [&_.cn-save]:py-0 [&_.cn-save]:px-[24px] [&_.cn-save]:rounded-[16px]",
  "[&_.cn-save]:text-[17px] [&_.cn-save]:font-bold [&_.cn-save]:border-[medium]",
  "[&_.cn-save]:border-none [&_.cn-save]:border-current [&_.cn-save]:border-b-[5px]",
  "[&_.cn-save]:[border-bottom-style:solid] [&_.cn-save]:border-b-ok-ink [&_.cn-save]:bg-ok",
  "[&_.cn-save]:text-white [&_.cn-save:disabled]:border-b-[0] [&_.cn-save:disabled]:bg-off",
  "[&_.cn-save:disabled]:text-faint [&_.cn-save:disabled]:cursor-not-allowed [&_.cn-ok-ic]:w-[44px]",
  "[&_.cn-ok-ic]:h-[44px] [&_.cn-ok-ic]:rounded-full [&_.cn-ok-ic]:bg-white [&_.cn-ok-ic]:flex",
  "[&_.cn-ok-ic]:items-center [&_.cn-ok-ic]:justify-center [&_.cn-ok-txt]:flex [&_.cn-ok-txt]:flex-col",
  "[&_.cn-ok-txt]:gap-[6px] [&_.cn-ok-txt]:min-w-[360px] [&_.cn-ok-t]:text-[22px]",
  "[&_.cn-ok-t]:font-semibold [&_.cn-ok-t]:text-ok-ink [&_.cn-ok-s]:text-[14px]",
  "[&_.cn-ok-s]:text-ok-ink [&_.cn-ok-track]:h-[6px] [&_.cn-ok-track]:rounded-full",
  "[&_.cn-ok-track]:bg-ok-line [&_.cn-ok-track]:overflow-hidden [&_.cn-ok-track_>_div]:h-full",
  "[&_.cn-ok-track_>_div]:bg-ok [&_.cn-ok-track_>_div]:rounded-full [&_.cn-ok-go]:h-[56px]",
  "[&_.cn-ok-go]:py-0 [&_.cn-ok-go]:px-[32px] [&_.cn-ok-go]:border-[medium] [&_.cn-ok-go]:border-none",
  "[&_.cn-ok-go]:border-current [&_.cn-ok-go]:border-b-[5px] [&_.cn-ok-go]:[border-bottom-style:solid]",
  "[&_.cn-ok-go]:border-b-ok-ink [&_.cn-ok-go]:rounded-[16px] [&_.cn-ok-go]:bg-ok",
  "[&_.cn-ok-go]:text-white [&_.cn-ok-go]:text-[18px] [&_.cn-ok-go]:font-bold [&_.cn-ok-go]:flex",
  "[&_.cn-ok-go]:items-center [&_.cn-ok-go]:no-underline [&_.cn-ok-go]:ml-[16px] [&_.cn-wait]:h-[56px]",
  "[&_.cn-wait]:py-0 [&_.cn-wait]:px-[32px] [&_.cn-wait]:border-[medium] [&_.cn-wait]:border-none",
  "[&_.cn-wait]:border-current [&_.cn-wait]:rounded-[16px] [&_.cn-wait]:bg-blue-tint",
  "[&_.cn-wait]:text-blue-ink [&_.cn-wait]:text-[18px] [&_.cn-wait]:font-bold",
  "[&_.cn-wait]:[cursor:progress] [&_.cn-baud]:flex [&_.cn-baud]:flex-col [&_.cn-baud]:gap-[3px]",
  "[&_.cn-baud]:[align-self:flex-end] [&_.cn-baud]:mb-[24px] [&_.cn-baud-l]:text-[12px]",
  "[&_.cn-baud-l]:font-semibold [&_.cn-baud-l]:text-muted [&_.cn-baud_select]:h-[56px]",
  "[&_.cn-baud_select]:min-w-[190px] [&_.cn-baud_select]:py-0 [&_.cn-baud_select]:px-[12px]",
  "[&_.cn-baud_select]:border-2 [&_.cn-baud_select]:border-solid [&_.cn-baud_select]:border-off",
  "[&_.cn-baud_select]:rounded-[12px] [&_.cn-baud_select]:bg-sunken [&_.cn-baud_select]:text-muted",
  "[&_.cn-baud_select]:text-[16px] [&_.cn-baud_select]:font-bold [&_.cn-baud_select]:cursor-not-allowed",
  "[&_.cn-connect]:h-[56px] [&_.cn-connect]:[align-self:flex-end] [&_.cn-connect]:mb-[24px]",
  "[&_.cn-connect]:py-0 [&_.cn-connect]:px-[32px] [&_.cn-connect]:rounded-[16px]",
  "[&_.cn-connect]:text-[18px] [&_.cn-connect]:font-bold [&_.cn-connect]:border-[medium]",
  "[&_.cn-connect]:border-none [&_.cn-connect]:border-current [&_.cn-connect]:border-b-[5px]",
  "[&_.cn-connect]:[border-bottom-style:solid] [&_.cn-connect]:border-b-blue-press",
  "[&_.cn-connect]:bg-blue-ink [&_.cn-connect]:text-white [&_.cn-connect:disabled]:border-b-[0]",
  "[&_.cn-connect:disabled]:bg-off [&_.cn-connect:disabled]:text-faint",
  "[&_.cn-connect:disabled]:cursor-not-allowed [&_.sb-scenario]:left-[16px]",
  "[&_.sb-scenario]:bottom-[16px] [&_.cn-dev]:fixed [&_.cn-dev]:top-[20px] [&_.cn-dev]:right-[190px]",
  "[&_.cn-dev]:z-[60] [&_.cn-dev]:flex [&_.cn-dev]:flex-col [&_.cn-dev]:items-end [&_.cn-dev]:gap-[8px]",
  "[&_.cn-dev]:font-[family-name:inherit] [&_.cn-dev-toggle]:h-[36px] [&_.cn-dev-toggle]:py-0",
  "[&_.cn-dev-toggle]:px-[12px] [&_.cn-dev-toggle]:border-2 [&_.cn-dev-toggle]:border-dashed",
  "[&_.cn-dev-toggle]:border-faint [&_.cn-dev-toggle]:rounded-full [&_.cn-dev-toggle]:bg-sunken",
  "[&_.cn-dev-toggle]:text-muted [&_.cn-dev-toggle]:text-[13px] [&_.cn-dev-toggle]:font-semibold",
  "[&_.cn-dev-toggle]:inline-flex [&_.cn-dev-toggle]:items-center [&_.cn-dev-toggle]:gap-[6px]",
  "[&_.cn-dev-toggle]:cursor-pointer [&_.cn-dev-panel]:w-[300px] [&_.cn-dev-panel]:bg-white",
  "[&_.cn-dev-panel]:border-2 [&_.cn-dev-panel]:border-solid [&_.cn-dev-panel]:border-line-strong",
  "[&_.cn-dev-panel]:rounded-[14px] [&_.cn-dev-panel]:p-[12px] [&_.cn-dev-panel]:flex",
  "[&_.cn-dev-panel]:flex-col [&_.cn-dev-panel]:gap-[10px] [&_.cn-dev-panel]:shadow-pop",
  "[&_.cn-dev-panel]:text-[13px] [&_.cn-dev-seg]:flex [&_.cn-dev-seg]:flex-wrap",
  "[&_.cn-dev-seg]:gap-[6px] [&_.cn-dev-seg_button]:h-[30px] [&_.cn-dev-seg_button]:py-0",
  "[&_.cn-dev-seg_button]:px-[10px] [&_.cn-dev-seg_button]:border-2 [&_.cn-dev-seg_button]:border-solid",
  "[&_.cn-dev-seg_button]:border-line [&_.cn-dev-seg_button]:rounded-[9px]",
  "[&_.cn-dev-seg_button]:bg-white [&_.cn-dev-seg_button]:text-ink [&_.cn-dev-seg_button]:text-[12px]",
  "[&_.cn-dev-seg_button]:font-semibold [&_.cn-dev-seg_button[aria-pressed='true']]:border-blue-ink",
  "[&_.cn-dev-seg_button[aria-pressed='true']]:bg-blue-tint",
  "[&_.cn-dev-seg_button[aria-pressed='true']]:text-blue-ink [&_.cn-dev-row]:flex",
  "[&_.cn-dev-row]:items-center [&_.cn-dev-row]:justify-between [&_.cn-dev-row]:gap-[10px]",
  "[&_.cn-dev-note]:text-[11px] [&_.cn-dev-note]:text-muted",
].join(" ");

export function ConnectView({ v }: { v: ReturnType<typeof useConnectScreen> }) {
  const {
    s,
    hold,
    a,
    sample,
    pickedRobot,
    pickedOther,
    pickedName,
    hasPick,
    sayT,
    sayS,
    connKey,
    showConnectBtn,
    footOk,
  } = v;
  const stopNote = !connKey
    ? translate("robot.connect.messages.connectARobotFirst")
    : s.moving
      ? translate("robot.connect.messages.stopNoteMoving")
      : translate("robot.connect.messages.stopNoteAnytime");
  const header = (
    <header className="cn-head window-integrated-header">
      <div className="cn-brand">
        <img className="cn-logo-sb" src={logoSchoolBot} alt={"School Bot Studio"} width={192} height={34} />
        <img className="cn-logo-arm" src={logoArmLab} alt={"Arm Lab"} width={100} height={25} />
      </div>
      <span className={connKey ? "cn-chip is-on" : "cn-chip"}>
        {connKey
          ? translate("robot.connect.messages.connectedToValue0", { value0: nameOf(s, connKey) })
          : translate("modal.robotRequired.title")}
      </span>
      <div className="cn-grow" />
      <div className="cn-stopbox">
        <Button
          variant="custom"
          type="button"
          className="cn-btn cn-stop"
          disabled={!connKey}
          onClick={a.stopAll}
        >
          {translate("robot.connect.messages.stopAll")}
        </Button>
        <span className="cn-stopnote">{stopNote}</span>
      </div>
      <WindowControlsSlot />
    </header>
  );

  const body = <ConnectBody v={v} />;

  let foot: ReactNode = null;
  const okName = connKey ? nameOf(s, connKey) : "";
  if (s.phase === "naming") {
    foot = (
      <div className="cn-nameform cn-in">
        <div className="cn-col">
          <span className="cn-mitr cn-nameform-t">{translate("robot.connect.messages.foundItGiveThisRobotAName")}</span>
          <span className="cn-nameform-s">
            {translate("robot.connect.messages.thisNameWillAppearNextTimeSoYouCan")}
          </span>
        </div>
        <input
          type="text"
          className="cn-name"
          value={s.nameDraft}
          maxLength={20}
          placeholder={translate("robot.connect.messages.forExampleGroup3Robot")}
          aria-label={translate("robot.connect.robotName")}
          autoFocus
          onChange={(e) => a.setNameDraft(e.target.value)}
          onKeyDown={(e) => {
            if (e.key === "Enter") a.saveName();
          }}
        />
        <Button
          variant="custom"
          type="button"
          className="cn-btn cn-skip"
          onClick={a.skipName}
        >
          {translate("robot.connect.messages.skipForNow")}
        </Button>
        <Button
          variant="custom"
          type="button"
          className="cn-btn cn-save"
          disabled={!s.nameDraft.trim()}
          onClick={a.saveName}
        >
          {translate("robot.connect.messages.saveNameAndStart")}
        </Button>
      </div>
    );
  } else if (s.phase === "ok") {
    foot = (
      <>
        <span className="cn-ok-ic">
          <CheckIcon size={26} stroke="var(--ok)" sw={3.4} />
        </span>
        <div className="cn-ok-txt">
          <span className="cn-mitr cn-ok-t">
            {translate("robot.connect.messages.connectedNameDone", { name: okName })}
          </span>
          <span className="cn-ok-s">{translate("robot.connect.messages.openingArmControlsIn2Seconds")}</span>
          <div className="cn-ok-track">
            <TimedProgressBar ms={MS.goBack} hold={hold} pct={55} />
          </div>
        </div>
        <Button
          variant="custom"
          type="button"
          className="cn-btn cn-ok-go"
          onClick={a.finish}
        >
          {translate("robot.connect.messages.startNow")}
        </Button>
      </>
    );
  } else if (s.phase === "connecting") {
    foot = (
      <Button
        variant="custom"
        type="button"
        className="cn-wait"
        disabled
      >
        {translate("robot.connect.messages.waitingForARobotResponse")}
      </Button>
    );
  } else if (showConnectBtn) {
    const baudKey = pickedRobot?.key;
    const baudVal = pickedRobot
      ? `${BAUD[baudKey ?? ""] ?? "115200"} bps`
      : pickedOther
        ? translate("robot.connect.messages.baudUnspecified")
        : translate("robot.connect.messages.baudSelectRobotFirst");
    const baudTip = pickedRobot
      ? translate("robot.connect.messages.baudTipRobot")
      : pickedOther
        ? translate("robot.connect.messages.baudTipOther")
        : translate("robot.connect.messages.baudTipNone");
    const label =
      s.switchTo && connKey
        ? translate("robot.connect.messages.disconnectValue0ConnectValue1", {
          value0: nameOf(s, connKey),
          value1: pickedName,
        })
        : hasPick
          ? translate("robot.connect.messages.connectValue0", {value0: pickedName})
          : translate("robot.connect.messages.selectARobotFirst");
    foot = (
      <>
        <label className="cn-baud" title={baudTip}>
          <span className="cn-baud-l">{translate("robot.connect.messages.baudRateSetAutomaticallyForTheRobot")}</span>
          <select
            disabled
            aria-label={translate("robot.connect.messages.baudRateIsSetForTheSelectedRobotRead")}
            value="v"
            onChange={() => {}}
          >
            <option value="v">{baudVal}</option>
          </select>
        </label>
        <Button
          variant="custom"
          type="button"
          className="cn-btn cn-connect"
          disabled={!hasPick || s.phase === "scanning"}
          onClick={a.connect}
        >
          {label}
        </Button>
      </>
    );
  }

  return (
    <div className={CONNECT_VIEW_ROOT_CLASS}>
      {header}
      <main className="cn-main">
        <div className="cn-sayrow">
          <ConnectionMascot />
          <div className="cn-say" role="status">
            <span className="cn-mitr cn-say-t">{translate(sayT)}</span>
            <span className="cn-say-s">{translate(sayS)}</span>
          </div>
        </div>
        {body}
      </main>
      <footer className={footOk ? "cn-foot is-ok" : "cn-foot"}>
        <div className="cn-grow" />
        {foot}
      </footer>
      {sample && (
        <>
          <ScenarioBar items={SCENARIOS} value={s.scenario} onChange={(v) => a.applyScenario(v as typeof s.scenario)} />
          <DevPanel
            outcome={s.outcome}
            unsaved={s.unsaved}
            moving={s.moving}
            onOutcome={(outcome) => a.setDev({ outcome })}
            onUnsaved={(unsaved) => a.setDev({ unsaved })}
            onMoving={(moving) => a.setDev({ moving })}
          />
        </>
      )}
    </div>
  );

}
