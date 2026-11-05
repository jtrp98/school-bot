import { param } from "./debugParams";

// Browser preview only: a made-up set of serial ports for Block Lab's board screen. Nothing
// here touches a real port; the preview panel plugs and unplugs these entries in memory.
type PreviewPort = {
  port: string;
  usb: boolean;
  vid?: string;
  pid?: string;
  serial_number?: string;
  kind: "uno" | "usb-serial" | "other";
  chip?: string;
};

export type PreviewSample = {
  id: string;
  device: PreviewPort;
  altPort?: string;
  plugged: boolean;
  moved: boolean;
};

// Cheap CP210x bridges often report the same serial number on every board.
const SHARED_SERIAL_CLONE: Omit<PreviewPort, "port"> = {
  usb: true,
  vid: "10C4",
  pid: "EA60",
  serial_number: "0001",
  kind: "usb-serial",
  chip: "CP210x",
};

const SAMPLES: Omit<PreviewSample, "plugged" | "moved">[] = [
  {
    id: "uno-a",
    device: { port: "COM3", usb: true, vid: "2341", pid: "0043", serial_number: "PREVIEW-UNO-A", kind: "uno" },
    altPort: "COM8",
  },
  {
    id: "uno-b",
    device: { port: "COM4", usb: true, vid: "2341", pid: "0043", serial_number: "PREVIEW-UNO-B", kind: "uno" },
    altPort: "COM10",
  },
  {
    id: "clone",
    device: { port: "COM5", usb: true, vid: "1A86", pid: "7523", kind: "usb-serial", chip: "CH340" },
    altPort: "COM9",
  },
  { id: "cp-a", device: { ...SHARED_SERIAL_CLONE, port: "COM11" }, altPort: "COM13" },
  { id: "cp-b", device: { ...SHARED_SERIAL_CLONE, port: "COM12" }, altPort: "COM14" },
  { id: "serial", device: { port: "COM1", usb: false, kind: "other" } },
  { id: "usb-other", device: { port: "COM6", usb: true, vid: "0483", pid: "5740", kind: "other" } },
];

// `?devices=none|one|many|clone|clones` picks which sample ports start plugged in.
const PRESETS: Record<string, string[]> = {
  none: ["serial"],
  one: ["uno-a", "serial"],
  many: ["uno-a", "uno-b", "clone", "serial", "usb-other"],
  clone: ["clone", "serial"],
  clones: ["cp-a", "cp-b", "serial"],
};

const startPlugged = PRESETS[param("devices") ?? "one"] ?? PRESETS.one;
let samples: PreviewSample[] = SAMPLES.map(sample => ({
  ...sample,
  plugged: startPlugged.includes(sample.id),
  moved: false,
}));

export const previewPortOf = (sample: PreviewSample) =>
  sample.moved && sample.altPort ? sample.altPort : sample.device.port;

export const previewSamples = (): readonly PreviewSample[] => samples;

export function setPreviewSample(id: string, change: Partial<Pick<PreviewSample, "plugged" | "moved">>) {
  samples = samples.map(sample => (sample.id === id ? { ...sample, ...change } : sample));
}

export function previewDeviceList() {
  const devices = samples
    .filter(sample => sample.plugged)
    .map(sample => ({ ...sample.device, port: previewPortOf(sample) }));
  return { engine: "preview", simulated: true, ports: devices.map(device => device.port), devices };
}
