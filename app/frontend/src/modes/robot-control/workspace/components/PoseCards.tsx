import { Icon } from "../../../../components/shared/Icons";
import { PoseCard } from "./PoseCard";
import { t as translate } from "../../../../i18n";
import { Fragment, type ReactNode } from "react";
import { T, arr } from "../tplUtil";
import type { WorkspaceViewProps } from "../viewTypes";

type Props = WorkspaceViewProps<
  "p0Off" | "p0On" | "p0DupShow" | "p0DupCls" | "p0m6Open" | "tiles" | "gRenDraft" | "noClip"
  | "lockOff" | "renameDraft" | "endSlotShow" | "endSlotCls" | "endSlotLabel" | "endSlotText" | "addCls"
  | "addPose" | "addOff" | "addTileLabel" | "dom" | "poseActions"
>;

const TRACK_CLASS_NAME = [
  "[scrollbar-width:thin] [scrollbar-color:#C6D6EA_transparent]",
  "[&::-webkit-scrollbar]:h-[6px] [&::-webkit-scrollbar-thumb]:bg-line-strong",
  "[&::-webkit-scrollbar-thumb]:rounded-full [&_button[data-tile]]:h-[124px]!",
  "[&_button[data-tile]]:w-[128px]! [&_button[data-tile]]:grid!",
  "[&_button[data-tile]]:[grid-template-columns:auto_1fr_auto]",
  "[&_button[data-tile]]:[grid-template-rows:34px_36px_26px]!",
  "[&_button[data-tile]]:[align-items:start]! [&_button[data-tile]]:[justify-items:start]",
  "[&_button[data-tile]]:gap-x-[4px]! [&_button[data-tile]]:gap-y-[6px]!",
  "[&_button[data-tile]]:py-[8px]! [&_button[data-tile]]:px-[10px]!",
  "[&_button[data-tile]_>_[data-more6]]:[grid-column:2]",
  "[&_button[data-tile]_>_[data-more6]]:[justify-self:end]",
  "[&_button[data-tile]_>_span:last-child]:[grid-column:1_/_-1]",
  "[&_button[data-tile]_>_span:last-child]:[align-self:stretch]",
  "[&_button[data-tile]_>_span:last-child]:w-full",
  "[&_button[data-tile]_>_span:last-child_>_span:first-child]:[white-space:normal]!",
  "[&_button[data-tile]_>_span:last-child_>_span:first-child]:[display:-webkit-box]!",
  "[&_button[data-tile]_>_span:last-child_>_span:first-child]:[-webkit-line-clamp:2]",
  "[&_button[data-tile]_>_span:last-child_>_span:first-child]:[-webkit-box-orient:vertical]",
  "[&_button[data-tile]_>_span:last-child_>_span:first-child]:leading-[1.3]",
  "[&_button[data-tile]_>_span:last-child_>_span:first-child]:max-h-[2.6em]",
  "[&_button[data-tile]_>_span:last-child_>_span[role=button]:first-child]:[white-space:normal]!",
  "[&_button[data-tile]_>_span:last-child_>_span[role=button]:first-child]:[display:-webkit-box]!",
  "[&_button[data-tile]_>_span:last-child_>_span[role=button]:first-child]:[-webkit-line-clamp:2]",
  "[&_button[data-tile]_>_span:last-child_>_span[role=button]:first-child]:[-webkit-box-orient:vertical]",
  "[&_button[data-tile]_>_span:last-child_>_span[role=button]:first-child]:leading-[1.3]",
  "[&_button[data-tile]_>_span:last-child_>_span[role=button]:first-child]:max-h-[2.6em]",
  "[&_button[data-tile]_>_span:last-child]:justify-between",
  "[&_.aw-card_>_button:not([data-tile])]:hidden! [&_.aw-card_>_.aw-dup]:hidden!",
  "[&_.aw-card_>_[class*='aw-del']]:hidden! [&_.aw-gcard]:h-[124px]! [&_.aw-gap]:h-[124px]!",
  "[&_.aw-card_input.aw-edit]:left-[8px]! [&_.aw-card_input.aw-edit]:right-[8px]!",
  "[&_.aw-card_input.aw-edit]:top-[42px]! [&_[data-add6]]:h-[124px]! [&_[data-add6]]:flex-col!",
  "[&_[data-add6]]:gap-[6px]! [&_button[data-tile]]:relative!",
  "[&_button[data-tile]_>_[data-more6]]:absolute! [&_button[data-tile]_>_[data-more6]]:right-[8px]",
  "[&_button[data-tile]_>_[data-more6]]:bottom-[8px] [&_button[data-tile]_>_[data-pop]]:[grid-column:3]",
  "[&_button[data-tile]_>_[data-pop]]:[grid-row:1]",
  "[&_button[data-tile]_>_[data-pop]]:[justify-self:end]",
  "[&_button[data-tile]_[data-chk6]]:[grid-column:1] [&_button[data-tile]_[data-chk6]]:w-[26px]",
  "[&_button[data-tile]_[data-chk6]]:h-[26px] [&_button[data-tile]_[data-chk6]]:box-border",
  "[&_button[data-tile]_[data-chk6]]:rounded-[7px] [&_button[data-tile]_[data-chk6]]:border-2",
  "[&_button[data-tile]_[data-chk6]]:border-solid [&_button[data-tile]_[data-chk6]]:border-purple",
  "[&_button[data-tile]_[data-chk6]]:bg-white [&_button[data-tile]_[data-chk6]]:text-transparent",
  "[&_button[data-tile]_[data-chk6]]:text-[14px] [&_button[data-tile]_[data-chk6]]:font-extrabold",
  "[&_button[data-tile]_[data-chk6]]:flex [&_button[data-tile]_[data-chk6]]:items-center",
  "[&_button[data-tile]_[data-chk6]]:justify-center [&_button[data-tile]_[data-chk6='1']]:bg-purple",
  "[&_button[data-tile]_[data-chk6='1']]:border-purple [&_button[data-tile]_[data-chk6='1']]:text-white",
  "[&_button[data-tile]_[data-hdl6]]:[grid-column:2]",
  "[&_button[data-tile]_[data-hdl6]]:[justify-self:start] [&_button[data-tile]_[data-hdl6]]:text-[18px]",
  "[&_button[data-tile]_[data-hdl6]]:leading-[26px] [&_button[data-tile]_[data-hdl6]]:text-muted",
  "[&_button[data-tile]_[data-hdl6]]:[cursor:grab] [&_button[data-tile][data-pk6='0']]:border-2!",
  "[&_button[data-tile][data-pk6='0']]:border-dashed! [&_button[data-tile][data-pk6='0']]:border-faint!",
  "[&_button[data-tile][data-pk6='0']]:bg-white! [&_button[data-tile][data-pk6='0']]:shadow-none!",
  "[&_button[data-tile][data-pk6='0']]:outline-none! [&_button[data-tile][data-pk6='1']]:border-[3px]!",
  "[&_button[data-tile][data-pk6='1']]:border-solid! [&_button[data-tile][data-pk6='1']]:border-purple!",
  "[&_button[data-tile][data-pk6='1']]:bg-purple-tint! [&_button[data-tile][data-pk6='1']]:shadow-none!",
  "[&_button[data-tile][data-pk6='1']]:outline-none! [&_.aw-gcard]:w-[148px]! [&_.aw-gcard]:box-border",
  "[&_.aw-gcard]:grid! [&_.aw-gcard]:[grid-template-columns:1fr]!",
  "[&_.aw-gcard]:[grid-template-rows:34px_36px_26px]! [&_.aw-gcard]:[align-items:start]!",
  "[&_.aw-gcard]:gap-x-[4px]! [&_.aw-gcard]:gap-y-[6px]! [&_.aw-gcard]:pt-[8px]!",
  "[&_.aw-gcard]:px-[9px]! [&_.aw-gcard]:pb-[7px]!",
  "[&_.aw-gcard_>_span:not([data-gtop6])]:[grid-column:1_/_-1]",
  "[&_.aw-gcard_>_span:not([data-gtop6])]:[align-self:start]!",
  "[&_.aw-gcard_>_button:first-of-type]:[justify-self:start]",
  "[&_.aw-gcard_>_button:first-of-type]:h-[30px]! [&_.aw-gcard_>_button:first-of-type]:border-b-4!",
  "[&_.aw-gcard_>_button]:[align-self:end]! [&_.aw-gcard_>_span:not([data-gtop6])]:[flex:0_0_auto]!",
  "[&_.aw-gcard_>_span:not([data-gtop6])_>_span:first-child]:[white-space:normal]!",
  "[&_.aw-gcard_>_span:not([data-gtop6])_>_span:first-child]:[display:-webkit-box]!",
  "[&_.aw-gcard_>_span:not([data-gtop6])_>_span:first-child]:[-webkit-line-clamp:2]",
  "[&_.aw-gcard_>_span:not([data-gtop6])_>_span:first-child]:[-webkit-box-orient:vertical]",
  "[&_.aw-gcard_>_span:not([data-gtop6])_>_span:first-child]:leading-[1.3] [&_[data-gtop6]]:flex",
  "[&_[data-gtop6]]:items-center [&_[data-gtop6]]:gap-[6px] [&_[data-gtop6]_[data-chk6]]:w-[26px]",
  "[&_[data-gtop6]_[data-chk6]]:h-[26px] [&_[data-gtop6]_[data-chk6]]:box-border",
  "[&_[data-gtop6]_[data-chk6]]:rounded-[7px] [&_[data-gtop6]_[data-chk6]]:border-2",
  "[&_[data-gtop6]_[data-chk6]]:border-solid [&_[data-gtop6]_[data-chk6]]:border-purple",
  "[&_[data-gtop6]_[data-chk6]]:bg-white [&_[data-gtop6]_[data-chk6]]:text-transparent",
  "[&_[data-gtop6]_[data-chk6]]:text-[14px] [&_[data-gtop6]_[data-chk6]]:font-extrabold",
  "[&_[data-gtop6]_[data-chk6]]:flex [&_[data-gtop6]_[data-chk6]]:items-center",
  "[&_[data-gtop6]_[data-chk6]]:justify-center [&_[data-gtop6]_[data-chk6='1']]:bg-purple",
  "[&_[data-gtop6]_[data-chk6='1']]:border-purple [&_[data-gtop6]_[data-chk6='1']]:text-white",
  "[&_[data-gtop6]_[data-hdl6]]:text-[18px] [&_[data-gtop6]_[data-hdl6]]:leading-[26px]",
  "[&_[data-gtop6]_[data-hdl6]]:text-muted [&_[data-gtop6]_[data-hdl6]]:[cursor:grab]",
  "[&_.aw-gcard[data-gpk6='0']]:border-2! [&_.aw-gcard[data-gpk6='0']]:border-dashed!",
  "[&_.aw-gcard[data-gpk6='0']]:border-faint! [&_.aw-gcard[data-gpk6='0']]:bg-white!",
  "[&_.aw-gcard[data-gpk6='0']]:outline-none! [&_.aw-gcard[data-gpk6='0']]:shadow-none!",
  "[&_.aw-gcard[data-gpk6='1']]:border-[3px]! [&_.aw-gcard[data-gpk6='1']]:border-solid!",
  "[&_.aw-gcard[data-gpk6='1']]:border-purple! [&_.aw-gcard[data-gpk6='1']]:bg-purple-tint!",
  "[&_.aw-gcard[data-gpk6='1']]:outline-none! [&_.aw-gcard[data-gpk6='1']]:shadow-none!",
  "[&_button[data-tile]_[data-gtag6]]:[grid-column:3]",
  "[&_button[data-tile]_[data-gtag6]]:[justify-self:end]",
  "[&_button[data-tile]_[data-gtag6]]:max-w-[62px] [&_button[data-tile]_[data-gtag6]]:h-[20px]",
  "[&_button[data-tile]_[data-gtag6]]:py-0 [&_button[data-tile]_[data-gtag6]]:px-[6px]",
  "[&_button[data-tile]_[data-gtag6]]:rounded-full [&_button[data-tile]_[data-gtag6]]:bg-purple-tint",
  "[&_button[data-tile]_[data-gtag6]]:text-purple-ink [&_button[data-tile]_[data-gtag6]]:text-[10px]",
  "[&_button[data-tile]_[data-gtag6]]:font-bold [&_button[data-tile]_[data-gtag6]]:leading-[20px]",
  "[&_button[data-tile]_[data-gtag6]]:whitespace-nowrap",
  "[&_button[data-tile]_[data-gtag6]]:overflow-hidden [&_button[data-tile]_[data-gtag6]]:text-ellipsis",
  "[&_button[data-tile][data-gm6='1'][data-pk6='0']]:border-purple-line! [&_[data-add6]]:w-[128px]!",
  "[&_.aw-gcard[data-gin6='1']]:shadow-[0_0_0_4px_var(--purple-line)]!",
  "[&_.aw-gcard[data-gin6='1']]:border-purple! [&_.aw-gcard[data-gin6='1']]:bg-purple-tint!",
  "[&_button[data-tile][data-gin6='1']]:shadow-[0_0_0_4px_var(--purple-line)]!",
  "[&_button[data-tile][data-gin6='1']]:border-purple!",
  "[&_button[data-tile][data-gin6='1']]:bg-purple-tint!",
  "[&_.aw-gcard[data-gpk6='0']_>_button:not(:first-of-type)]:hidden!",
  "[&_.aw-gcard[data-gpk6='1']_>_button:not(:first-of-type)]:hidden! h-[176px]!",
  "[&_button[data-tile]]:[align-content:start]! [&_button[data-tile]_>_span:last-child]:grid!",
  "[&_button[data-tile]_>_span:last-child]:[grid-template-rows:36px_26px]",
  "[&_button[data-tile]_>_span:last-child]:gap-y-[6px]",
  "[&_button[data-tile]_>_span:last-child]:[align-items:start]",
  "[&_button[data-tile]_>_span:last-child_>_:first-child]:h-[36px]",
  "[&_button[data-tile]_>_span:last-child_>_:first-child]:[align-self:start]",
  "[&_button[data-tile]:hover:not([data-pk6])]:border-blue-line! [&_.aw-gcard]:border-2!",
  "[&_.aw-gcard]:border-solid! [&_.aw-gcard]:border-purple-line! [&_.aw-gcard]:border-b-4!",
  "[&_.aw-gcard]:[border-bottom-style:solid]! [&_.aw-gcard]:border-b-purple-line!",
  "[&_.aw-gcard]:rounded-[14px]! [&_.aw-gcard]:bg-white! [&_.aw-gcard]:outline-none!",
  "[&_.aw-gcard]:shadow-[5px_-5px_0_-1px_var(--surface),5px_-5px_0_1px_var(--purple-line)]!",
  "[&_.aw-gcard]:mt-[5px] [&_.aw-gcard:hover]:border-purple-line!",
  "[&_.aw-gcard[data-gst6='sel']]:border-[3px]! [&_.aw-gcard[data-gst6='sel']]:border-solid!",
  "[&_.aw-gcard[data-gst6='sel']]:border-purple! [&_.aw-gcard[data-gst6='sel']]:border-b-[5px]!",
  "[&_.aw-gcard[data-gst6='sel']]:[border-bottom-style:solid]!",
  "[&_.aw-gcard[data-gst6='sel']]:border-b-purple-ink! [&_.aw-gcard[data-gst6='sel']]:bg-purple-tint!",
  "[&_.aw-gcard[data-gst6='sel']]:shadow-[0_0_0_3px_var(--purple-tint),7px_-7px_0_-1px_var(--surface),7px_-7px_0_1px_var(--purple-line)]!",
  "[&_.aw-gcard[data-gst6='play']]:border-[3px]! [&_.aw-gcard[data-gst6='play']]:border-solid!",
  "[&_.aw-gcard[data-gst6='play']]:border-blue-ink! [&_.aw-gcard[data-gst6='play']]:border-b-[5px]!",
  "[&_.aw-gcard[data-gst6='play']]:[border-bottom-style:solid]!",
  "[&_.aw-gcard[data-gst6='play']]:border-b-blue-press! [&_.aw-gcard[data-gst6='play']]:bg-blue-tint!",
  "[&_.aw-gcard[data-gst6='in']]:border-[3px]! [&_.aw-gcard[data-gst6='in']]:border-solid!",
  "[&_.aw-gcard[data-gst6='in']]:border-blue-ink! [&_.aw-gcard[data-gst6='in']]:bg-blue-tint!",
  "[&_.aw-gcard[data-gst6='out']]:border-2! [&_.aw-gcard[data-gst6='out']]:border-dashed!",
  "[&_.aw-gcard[data-gst6='out']]:border-purple-line! [&_.aw-gcard[data-gst6='out']]:opacity-[.7]",
  "[&_.aw-gcard_[data-gr1]]:flex [&_.aw-gcard_[data-gr1]]:items-center",
  "[&_.aw-gcard_[data-gr1]]:gap-[4px] [&_.aw-gcard_[data-gr1]]:min-w-0 [&_.aw-gcard_[data-gr3]]:flex",
  "[&_.aw-gcard_[data-gr3]]:items-center [&_.aw-gcard_[data-gr3]]:gap-[4px]",
  "[&_.aw-gcard_[data-gr3]]:min-w-0 [&_.aw-gcard_[data-gtop6]]:absolute",
  "[&_.aw-gcard_[data-gtop6]]:left-[9px] [&_.aw-gcard_[data-gtop6]]:top-[12px]",
  "[&_.aw-gcard[data-gpk6='0']_[data-gr1]_[data-gbadge6]]:ml-[52px]",
  "[&_.aw-gcard[data-gpk6='1']_[data-gr1]_[data-gbadge6]]:ml-[52px] [&_[data-gbadge6]]:h-[26px]",
  "[&_[data-gbadge6]]:py-0 [&_[data-gbadge6]]:px-[8px] [&_[data-gbadge6]]:rounded-full",
  "[&_[data-gbadge6]]:border-[medium] [&_[data-gbadge6]]:border-none [&_[data-gbadge6]]:border-current",
  "[&_[data-gbadge6]]:bg-blue-tint [&_[data-gbadge6]]:text-ink",
  "[&_[data-gbadge6]]:font-[family-name:inherit] [&_[data-gbadge6]]:text-[12px]",
  "[&_[data-gbadge6]]:font-extrabold [&_[data-gbadge6]]:inline-flex [&_[data-gbadge6]]:items-center",
  "[&_[data-gbadge6]]:gap-[4px] [&_[data-gbadge6]]:cursor-pointer [&_[data-gbadge6]]:whitespace-nowrap",
  "[&_.aw-gcard[data-gst6='sel']_[data-gbadge6]]:bg-purple",
  "[&_.aw-gcard[data-gst6='sel']_[data-gbadge6]]:text-white",
  "[&_.aw-gcard[data-gst6='play']_[data-gbadge6]]:bg-blue-ink",
  "[&_.aw-gcard[data-gst6='play']_[data-gbadge6]]:text-white [&_[data-gplay6]]:w-[34px]",
  "[&_[data-gplay6]]:h-[34px] [&_[data-gplay6]]:box-border [&_[data-gplay6]]:rounded-[50%]",
  "[&_[data-gplay6]]:border-2 [&_[data-gplay6]]:border-solid [&_[data-gplay6]]:border-purple-line",
  "[&_[data-gplay6]]:border-b-4 [&_[data-gplay6]]:bg-purple-tint [&_[data-gplay6]]:text-purple",
  "[&_[data-gplay6]]:flex [&_[data-gplay6]]:items-center [&_[data-gplay6]]:justify-center",
  "[&_[data-gplay6]]:cursor-pointer [&_[data-gplay6]]:p-0 [&_[data-gplay6]:disabled]:border-line",
  "[&_[data-gplay6]:disabled]:bg-sunken [&_[data-gplay6]:disabled]:text-line-strong",
  "[&_[data-gplay6]:disabled]:[cursor:default] [&_[data-gname6]]:text-[13.5px]",
  "[&_[data-gname6]]:font-bold [&_[data-gname6]]:text-purple-ink [&_[data-gname6]]:leading-[1.3]",
  "[&_[data-gname6]]:h-[36px] [&_[data-gname6]]:[display:-webkit-box]",
  "[&_[data-gname6]]:[-webkit-line-clamp:2] [&_[data-gname6]]:[-webkit-box-orient:vertical]",
  "[&_[data-gname6]]:overflow-hidden [&_[data-gname6]]:[word-break:break-word]",
  "[&_[data-gfoot6]]:inline-flex [&_[data-gfoot6]]:items-center [&_[data-gfoot6]]:gap-[4px]",
  "[&_[data-gfoot6]]:text-purple",
  "[&_[data-gfoot6]]:font-[family-name:ui-monospace,Menlo,Consolas,monospace]",
  "[&_[data-gfoot6]]:text-[11px] [&_[data-gfoot6]]:font-bold [&_[data-gfoot6]]:whitespace-nowrap",
  "[&_[data-gmore6]]:w-[28px] [&_[data-gmore6]]:h-[26px] [&_[data-gmore6]]:box-border",
  "[&_[data-gmore6]]:rounded-[8px] [&_[data-gmore6]]:border-2 [&_[data-gmore6]]:border-solid",
  "[&_[data-gmore6]]:border-purple-line [&_[data-gmore6]]:border-b-4 [&_[data-gmore6]]:bg-white",
  "[&_[data-gmore6]]:text-purple-ink [&_[data-gmore6]]:text-[13px] [&_[data-gmore6]]:font-extrabold",
  "[&_[data-gmore6]]:flex [&_[data-gmore6]]:items-center [&_[data-gmore6]]:justify-center",
  "[&_[data-gmore6]]:cursor-pointer [&_[data-gmore6]]:p-0 [&_[data-gmore6]]:font-[family-name:inherit]",
  "[&_.aw-gpop]:w-auto! [&_.aw-gpop]:right-[-3px]! [&_[data-gbar6='first']]:rounded-[999px_0_0_999px]!",
  "[&_[data-gbar6='first']]:mr-[-24px]! [&_[data-gbar6='first']]:relative",
  "[&_[data-gbar6='first']]:z-[1] [&_[data-gbar6='mid']]:rounded-none!",
  "[&_[data-gbar6='mid']]:ml-[-2px]! [&_[data-gbar6='mid']]:mr-[-24px]! [&_[data-gbar6='mid']]:relative",
  "[&_[data-gbar6='mid']]:z-[1] [&_[data-gbar6='last']]:rounded-[0_999px_999px_0]!",
  "[&_[data-gbar6='last']]:ml-[-2px]! flex items-start gap-[6px] overflow-x-auto [overflow-y:hidden]",
  "min-w-0 grow h-[150px] box-border pt-[6px] pr-[6px] pb-[12px] pl-[6px] mt-[-6px] mr-[-6px] mb-0",
  "ml-[-6px]",
].join(" ");

const CARD_CLASS = [
  "aw-card [&_.aw-del]:opacity-0 [&_.aw-dup]:opacity-0 [&:hover_.aw-dup]:opacity-[1]",
  "[&:hover_.aw-del]:opacity-[1] relative inline-flex shrink-0",
].join(" ");
const POSE0_BUTTON_CLASS = [
  "h-[124px] w-[120px] shrink-0 box-border pt-[8px] px-[8px] pb-[40px] rounded-[14px] flex flex-col items-center",
  "justify-start gap-[4px] font-[family-name:inherit] text-left cursor-default bg-sunken border-2 border-dashed",
  "border-faint",
].join(" ");
const POSE0_BADGE_CLASS = [
  "w-[24px] h-[24px] shrink-0 rounded-full flex items-center justify-center text-[12px] font-bold",
  "bg-muted text-white",
].join(" ");
const P0_DUP_CLASS = [
  "absolute top-[44px] right-[6px] min-w-[30px] h-[30px] p-0 flex items-center justify-center gap-[4px]",
  "rounded-full font-[family-name:inherit] text-[12px] font-bold whitespace-nowrap cursor-pointer z-[6]",
  "border-2 border-solid border-line bg-white text-blue-ink [transition:background_0.12s,_color_0.12s]",
].join(" ");
const SLOT_CLASS_PREFIX = "aw-slot hover:bg-blue-tint! hover:border-solid!";
const SLOT_CLASS_SUFFIX = [
  "w-[58px] h-[80px] shrink-0 box-border border-2 border-dashed border-blue-ink rounded-[12px]",
  "bg-blue-tint text-blue-ink font-[family-name:inherit] text-[11px] font-bold flex flex-col",
  "items-center justify-center gap-[2px] cursor-pointer leading-[1.2]",
].join(" ");
const SLOT_GROWING_CLASS = "aw-slotgrow animate-[awSlotGrow_.18s_ease-out_both] "
  + "overflow-hidden motion-reduce:animate-none";
const GAP_CLASS = [
  "aw-gap [&:hover_.aw-ins]:opacity-[1] relative inline-flex shrink-0 w-[12px] h-[80px] items-center",
  "justify-center",
].join(" ");
const ADD_FIRST_CLASS = [
  "aw-addfirst disabled:border-line-strong! disabled:bg-sunken! disabled:text-faint!",
  "disabled:cursor-not-allowed! [&:disabled>span:first-child]:bg-line!",
  "[transition:background-color_.12s,border-color_.12s] [&:hover:not(:disabled)]:bg-blue-tint!",
  "[&:hover:not(:disabled)]:border-blue-ink! [&:hover:not(:disabled)]:border-solid!",
  "[&:focus-visible:not(:disabled)]:bg-blue-tint! [&:focus-visible:not(:disabled)]:border-blue-ink!",
  "[&:focus-visible:not(:disabled)]:border-solid! focus-visible:[outline:3px_solid_var(--blue-tint)]",
  "focus-visible:[outline-offset:2px] [&:active:not(:disabled)]:bg-blue-ink!",
  "[&:active:not(:disabled)]:text-white! [&:active:not(:disabled)]:border-blue-ink!",
  "[&:active:not(:disabled)]:border-b-blue-press! [&:active:not(:disabled)]:border-b-4!",
  "[&:active:not(:disabled)>span:first-child]:bg-white!",
  "[&:active:not(:disabled)>span:first-child]:text-blue-ink!",
  "[&:active:not(:disabled)]:[transform:translateY(1px)] [&.aw-ring]:[animation-name:awAttnIn]",
  "motion-reduce:[&.aw-ring]:animate-none",
  "motion-reduce:[&.aw-ring]:shadow-[inset_0_0_0_3px_rgba(var(--rc),.5)]",
  "h-[80px] w-[128px] shrink-0 box-border border-2 border-dashed border-blue-line rounded-[14px] bg-blue-tint",
  "flex items-center justify-center gap-[10px] font-[family-name:inherit] text-blue-ink cursor-pointer",
].join(" ");
const GROUP_ANIMATION_CLASS = {
  in: "aw-gin animate-[awGin_.26s_cubic-bezier(.2,.8,.2,1)_both] overflow-hidden motion-reduce:animate-none",
  out: "aw-gout animate-[awGout_.2s_ease-in_both] overflow-hidden pointer-events-none motion-reduce:animate-none",
};

function SlotArrowIcon(): ReactNode {
  return (
    <svg
      width="18"
      height="18"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="2.8"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
    >
      <path d="M12 4v14M6 12l6 6 6-6" />
    </svg>
  );
}

export function PoseCards({ v }: Props): ReactNode {
  const actions = v.poseActions;
  return (
    <div className={"flex items-start gap-[10px] h-[136px] shrink-0"}>
      <span className={CARD_CLASS}>
        <button
          type="button"
          onClick={actions.selectPoseZero}
          disabled={v.p0Off}
          aria-pressed={v.p0On}
          aria-label={translate("robot.workspace.messages.pose0UprightArmSystemInitialPose")}
          className={POSE0_BUTTON_CLASS}
        >
          <span className={POSE0_BADGE_CLASS}>
            0
          </span>
          <svg width="30" height="40" viewBox="0 0 44 44" aria-hidden="true" className={"shrink-0"}>
            <rect x="10" y="36" width="24" height="5" rx="2" fill="var(--blue-line)" />
            <line x1="22" y1="36" x2="22" y2="8" stroke="var(--muted)" strokeWidth="5" strokeLinecap="round" />
          </svg>
          <span className={"flex flex-col items-start min-w-0"}>
            <span className={"text-[14px] font-bold text-ink"}>
              {translate("robot.workspace.messages.pose0")}{" "}
              <Icon name="lock" className={"inline-block size-[1.15em] align-[-.2em] mr-[.3em]"} />
            </span>
            <span className={"text-[12px] text-muted whitespace-nowrap"}>
              {translate("robot.workspace.messages.uprightArm")}
            </span>
          </span>
        </button>
        {v.p0DupShow ? (
          <>
            <button
              type="button"
              className={`${v.p0DupCls ?? ""} ${P0_DUP_CLASS}`}
              data-p0b6="1"
              onClick={v.p0m6Open}
              aria-haspopup="menu"
              aria-label={translate("robot.workspace.messages.duplicatePose0AsPose1")}
              title={translate("robot.workspace.messages.duplicatePose0AsPose1FirstInThe")}
            >
              <svg
                width="15"
                height="15"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                strokeWidth="2.4"
                strokeLinecap="round"
                strokeLinejoin="round"
                aria-hidden="true"
              >
                <rect x="8" y="8" width="12" height="12" rx="2" />
                <path d="M16 8V5a1 1 0 0 0-1-1H5a1 1 0 0 0-1 1v10a1 1 0 0 0 1 1h3" />
              </svg>
              <span
                className={"aw-p06lbl [[data-p0b6]_&]:[display:inline]! [[data-p0b6]_&]:text-[12px]"}
              >
                {translate("robot.workspace.messages.useThisPose")}
              </span>
            </button>
          </>
        ) : null}
      </span>
      <div className={"w-[2px] h-[112px] bg-line rounded-[2px] shrink-0"} />
      <div className={TRACK_CLASS_NAME} ref={v.dom.setCardsTrack}>
        {arr(v.tiles).map((tile) => (
          <Fragment key={tile.key}>
            {tile.visible ? (
              <div
                className={`${tile.groupAnimation ? GROUP_ANIMATION_CLASS[tile.groupAnimation] : ""}
                  flex items-start gap-[6px]`}
                style={tile.animationDelayMs === null ? undefined : { animationDelay: `${tile.animationDelayMs}ms` }}
              >
                {tile.slotShow ? (
                  <button
                    type="button"
                      className={`${SLOT_CLASS_PREFIX} ${tile.slotGrowing ? SLOT_GROWING_CLASS : ""}
                        ${SLOT_CLASS_SUFFIX}`}
                    onClick={() => actions.pasteOrMoveBefore(tile.index)}
                    onDragOver={actions.allowDropInGap}
                    onDrop={actions.dropInGap}
                    aria-label={tile.slotLabel}
                  >
                    <SlotArrowIcon />
                    {T(tile.slotText)}
                  </button>
                ) : null}
                {tile.showArrow ? (
                  <span className={GAP_CLASS}>
                    <svg
                      width="12"
                      height="80"
                      viewBox="2 0 14 80"
                      fill="none"
                      stroke="var(--faint)"
                      strokeWidth="2.6"
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      aria-hidden="true"
                      className={"shrink-0"}
                    >
                      <path d="M3 40h12M10 35l5 5-5 5" />
                    </svg>
                  </span>
                ) : null}
                <PoseCard v={v} tile={tile} />
              </div>
            ) : null}
          </Fragment>
        ))}
        {v.endSlotShow ? (
          <span className={"flex items-start gap-[6px] shrink-0"}>
            <button
              type="button"
              className={`${SLOT_CLASS_PREFIX} ${v.endSlotCls ?? ""} ${SLOT_CLASS_SUFFIX}`}
              onClick={() => actions.pasteOrMoveBefore(arr(v.tiles).length)}
              onDragOver={actions.allowDropInGap}
              onDrop={actions.dropInGap}
              aria-label={v.endSlotLabel}
            >
              <SlotArrowIcon />
              {T(v.endSlotText)}
            </button>
          </span>
        ) : null}
        <button
          type="button"
          className={`${ADD_FIRST_CLASS} ${v.addCls ?? ""}`}
          onClick={v.addPose}
          disabled={v.addOff}
          data-add6="1"
        >
          <span
            className="w-[32px] h-[32px] rounded-full bg-blue-ink text-white flex items-center justify-center
              shrink-0"
          >
            <svg
              width="18"
              height="18"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="3"
              strokeLinecap="round"
              aria-hidden="true"
            >
              <path d="M12 5v14M5 12h14" />
            </svg>
          </span>
          <span className={"flex flex-col items-start leading-[1.3]"}>
            <span className={"text-[15px] font-bold"}>
              {T(v.addTileLabel)}
            </span>
          </span>
        </button>
      </div>
    </div>
  );
}
