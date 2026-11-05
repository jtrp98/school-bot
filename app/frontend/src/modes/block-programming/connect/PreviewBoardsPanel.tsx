import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Button } from "../../../components/shared/Button";
import { previewPortOf, previewSamples, setPreviewSample, type PreviewSample } from "../../../core/previewBoards";

const sampleLabel = ({ device }: PreviewSample) =>
  [device.kind, device.chip, device.serial_number].filter(Boolean).join(" · ");

// Browser preview only: plugs, unplugs and moves the made-up ports so the board screen's
// one / many / none / cable-out cases can be tried without hardware. Nothing reaches a port.
export default function PreviewBoardsPanel({ onChange }: { onChange: () => void }) {
  const { t } = useTranslation();
  const [open, setOpen] = useState(false);
  const [samples, setSamples] = useState(previewSamples);
  const change = (id: string, next: Parameters<typeof setPreviewSample>[1]) => {
    setPreviewSample(id, next);
    setSamples(previewSamples());
    onChange();
  };
  return (
    <div className="absolute bottom-[112px] left-[16px] z-[6] flex flex-col items-start gap-[8px]">
      {open && (
        <div
          role="group"
          aria-label={t("blockLab.connect.preview.title")}
          className={[
            "flex w-[360px] flex-col gap-[8px] rounded-[16px] border-2 border-dashed border-line-strong bg-surface",
            "p-[12px] text-[14px] shadow-[0_8px_20px_rgba(20,36,61,.2)]",
          ].join(" ")}
        >
          <span className="font-bold">{t("blockLab.connect.preview.title")}</span>
          <span className="text-muted">{t("blockLab.connect.preview.note")}</span>
          {samples.map(sample => (
            <div key={sample.id} className="flex items-center gap-[8px]">
              <label className="flex flex-1 cursor-pointer items-center gap-[8px]">
                <input
                  type="checkbox"
                  checked={sample.plugged}
                  onChange={event => change(sample.id, { plugged: event.target.checked })}
                  className="h-[20px] w-[20px]"
                />
                <span className="flex flex-col leading-[1.3]">
                  <span className="font-mono font-bold">{previewPortOf(sample)}</span>
                  <span className="font-mono text-[12px] text-muted">{sampleLabel(sample)}</span>
                </span>
              </label>
              {sample.altPort && (
                <Button
                  variant="secondary"
                  size="sm"
                  aria-pressed={sample.moved}
                  onClick={() => change(sample.id, { moved: !sample.moved })}
                >
                  {t("blockLab.connect.preview.movePort")}
                </Button>
              )}
            </div>
          ))}
        </div>
      )}
      <Button variant="secondary" size="sm" icon="usb" aria-expanded={open} onClick={() => setOpen(shown => !shown)}>
        {t("blockLab.connect.preview.toggle")}
      </Button>
    </div>
  );
}
