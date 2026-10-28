import { t as translate } from "../../i18n";
type Props = { status: string; result: string };

const resultNoticeClass = [
  "notice whitespace-pre-wrap [overflow-wrap:anywhere] border-2 border-solid border-blue-line",
  "bg-blue-tint text-blue-press rounded-control py-[12px] px-[16px] mt-0 mx-0 mb-[20px] text-[13px]",
  "[&.error]:text-err-ink [&.error]:bg-err-tint [&.error]:border-err-line",
].join(" ");
const tracePanelClass = [
  "panel bg-surface border-2 border-solid border-line rounded-card p-[24px]",
  "[@media(max-width:_1100px)]:p-[16px] trace mt-[16px] [&_pre]:text-[12px] [&_pre]:whitespace-pre-wrap",
].join(" ");

export function BlockResultPanel({ status, result }: Props) {
  return (
    <>
      <div className={resultNoticeClass} role="status">{translate(status)}</div>
      {result && (
        <details className={tracePanelClass}>
          <summary>{translate("blocks.messages.engineCommandTrace")}</summary>
          <pre>{result}</pre>
        </details>
      )}
    </>
  );
}
