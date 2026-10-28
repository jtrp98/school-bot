import { t as translate } from "../../i18n";

export function SchoolBotMascot({ w, h, cheer }: { w: number; h: number; cheer?: boolean }) {
  return (
    <svg
      width={w}
      height={h}
      viewBox="0 0 112 160"
      role="img"
      aria-label={translate("lessons.messages.schoolBotMascot")}
      className="block"
    >
      <ellipse cx="56" cy="152" rx="34" ry="6.5" fill="rgba(31,36,48,.14)" />
      <rect x="36" y="122" width="14" height="24" rx="6" fill="#1F2430" />
      <rect x="62" y="122" width="14" height="24" rx="6" fill="#1F2430" />
      <rect x="30" y="140" width="26" height="10" rx="5" fill="#1F2430" />
      <rect x="56" y="140" width="26" height="10" rx="5" fill="#1F2430" />
      <path d="M26 94 C10 98 8 114 14 122" fill="none" stroke="#1F2430" strokeWidth="9" strokeLinecap="round" />
      <circle cx="15" cy="124" r="7" fill="#F4610A" stroke="#1F2430" strokeWidth="3" />
      {cheer ? (
        <>
          <path d="M86 94 C104 86 106 66 100 50" fill="none" stroke="#1F2430" strokeWidth="9" strokeLinecap="round" />
          <circle cx="99" cy="46" r="8" fill="#F4610A" stroke="#1F2430" strokeWidth="3" />
        </>
      ) : (
        <>
          <path d="M86 94 C102 98 104 114 98 122" fill="none" stroke="#1F2430" strokeWidth="9" strokeLinecap="round" />
          <circle cx="97" cy="124" r="7" fill="#F4610A" stroke="#1F2430" strokeWidth="3" />
        </>
      )}
      <rect x="24" y="74" width="64" height="56" rx="18" fill="#F4610A" stroke="#C74A00" strokeWidth="3" />
      <rect x="40" y="90" width="32" height="26" rx="9" fill="#fff" />
      <circle cx="56" cy="103" r="6.5" fill="#1490E0" />
      <line x1="56" y1="24" x2="56" y2="10" stroke="#1F2430" strokeWidth="4" strokeLinecap="round" />
      <circle cx="56" cy="8" r="6.5" fill="#F4610A" stroke="#1F2430" strokeWidth="3" />
      <circle cx="19" cy="52" r="6" fill="#1490E0" stroke="#1F2430" strokeWidth="3" />
      <circle cx="93" cy="52" r="6" fill="#1490E0" stroke="#1F2430" strokeWidth="3" />
      <rect x="20" y="24" width="72" height="58" rx="24" fill="#fff" stroke="#1F2430" strokeWidth="3.5" />
      <rect x="30" y="35" width="52" height="36" rx="15" fill="#1F2430" />
      <circle cx="45" cy="51" r="5.5" fill="#8FD4FF" />
      <circle cx="67" cy="51" r="5.5" fill="#8FD4FF" />
      <path d="M47 61 Q56 69 65 61" fill="none" stroke="#8FD4FF" strokeWidth="3.5" strokeLinecap="round" />
    </svg>
  );
}
