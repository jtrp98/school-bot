import { t as translate } from "../../i18n";
import type { ReactNode } from "react";
import { IconBtn } from "./IconBtn";
import { useShell } from "../../shell";
import { WindowControlsSlot } from "./WindowTitlebar";

const headerClass = [
  "sb-appbar h-[64px] shrink-0 flex items-center gap-[14px] py-0 px-[20px] bg-surface border-b-2",
  "[border-bottom-style:solid] border-b-line sh-appbar relative z-[5] window-integrated-header",
].join(" ");
const leadClass = "sb-row flex items-center sh-lead gap-[8px] shrink-0";
const titleClass = [
  "sh-ttl flex items-center gap-[8px] grow-0 not-italic [font-variant:normal] font-extrabold",
  "[font-stretch:normal] text-[22px] leading-[1.2] font-heading text-ink whitespace-nowrap",
].join(" ");
const searchClass = "sb-grow [flex:1] min-w-0";
const rightClass = "sb-row flex items-center sh-right gap-[12px] shrink-0";

export function AppBar({
  title,
  search,
  right,
}: {
  title: ReactNode;
  search?: ReactNode;
  right?: ReactNode;
}) {
  const { openMenu } = useShell();
  return (
    <header className={headerClass}>
      <div className={leadClass}>
        <IconBtn icon="menu" label={translate("nav.open")} flat onClick={openMenu} />
        <h1 className={titleClass}>{title}</h1>
      </div>
      <div className={searchClass}>{search}</div>
      {right && <div className={rightClass}>{right}</div>}
      <WindowControlsSlot />
    </header>
  );
}
