import { t as translate } from "../../i18n";
import { Icon } from "./Icons";
import { IconBtn } from "./IconBtn";

const searchClass = [
  "sh-search flex items-center gap-[10px] h-[48px] w-full max-w-[460px] pt-0 pr-[2px] pb-0",
  "pl-[14px] border-2 border-solid border-line-strong rounded-control bg-surface text-muted",
  "hover:border-blue focus-within:border-blue focus-within:shadow-[0_0_0_4px_var(--blue-tint)]",
  "[&_input]:[flex:1] [&_input]:min-w-0 [&_input]:border-[0] [&_input]:border-none",
  "[&_input]:border-current [&_input]:[outline:0] [&_input]:shadow-none [&_input]:bg-transparent",
  "[&_input]:not-italic [&_input]:[font-variant:normal] [&_input]:font-medium",
  "[&_input]:[font-stretch:normal] [&_input]:text-[17px] [&_input]:leading-[1.2]",
  "[&_input]:font-body [&_input]:text-ink [&_input::placeholder]:text-faint",
  "[&_.sb-iconbtn]:w-[44px] [&_.sb-iconbtn]:h-[44px] [&_.sb-iconbtn]:border-[0]",
  "[&_.sb-iconbtn]:border-none [&_.sb-iconbtn]:border-current",
].join(" ");

export function SearchBox({
  placeholder,
  value,
  onChange,
}: {
  placeholder: string;
  value: string;
  onChange: (next: string) => void;
}) {
  return (
    <label className={searchClass}>
      <Icon name="search" size={22} />
      <input
        type="text"
        value={value}
        placeholder={placeholder}
        aria-label={placeholder}
        onChange={(event) => onChange(event.target.value)}
      />
      {value && (
        <IconBtn
          icon="x"
          size={22}
          label={translate("common.messages.clearSearch")}
          flat
          onClick={() => onChange("")}
        />
      )}
    </label>
  );
}
