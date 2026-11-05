import { param } from "./debugParams";
import { EngineError } from "./engineError";

// Browser preview only: a made-up board for Block Lab's Serial monitor. No port is opened; it
// "prints" a numbered line every second and answers what the child sends.
// `?serial=fail` makes opening fail as if another program held the port.
const ACTIONS = ["serial.open", "serial.read", "serial.write", "serial.close"];

let openedAt = 0;
let printed = 0;
let replies: string[] = [];

const encode = (text: string) => {
  let binary = "";
  for (const byte of new TextEncoder().encode(text)) binary += String.fromCharCode(byte);
  return btoa(binary);
};

const state = (open: boolean) => ({
  engine: "preview",
  open,
  port: open ? "COM3" : "",
  baud_rate: 9600,
  simulated: true,
});

export const isPreviewSerial = (action: string) => ACTIONS.includes(action);

// Stands in for the host closing the session when the child leaves Block Lab.
export function closePreviewSerial() {
  openedAt = 0;
  replies = [];
}

export function previewSerial(action: string, input: unknown) {
  const data = typeof input === "object" && input !== null ? (input as Record<string, unknown>) : {};
  switch (action) {
    case "serial.open":
      if (param("serial") === "fail") {
        throw new EngineError("TRANSPORT_ERROR", "preview: another program is using the board's port");
      }
      openedAt = Date.now();
      printed = 0;
      replies = [];
      return state(true);
    case "serial.read": {
      if (!openedAt) return state(false);
      const due = Math.floor((Date.now() - openedAt) / 1000);
      let text = "";
      for (; printed < due; printed++) text += `preview: line ${printed + 1} from the simulated board\r\n`;
      text += replies.join("");
      replies = [];
      return text ? { ...state(true), received_base64: encode(text) } : state(true);
    }
    case "serial.write": {
      if (!openedAt) throw new EngineError("TRANSPORT_ERROR", "preview: the board's serial port is not open");
      const bytes = Uint8Array.from(atob(String(data.write_base64 ?? "")), character => character.charCodeAt(0));
      replies.push(`preview: the board got ${new TextDecoder().decode(bytes).trim()}\r\n`);
      return { ...state(true), written: bytes.length };
    }
    default:
      closePreviewSerial();
      return state(false);
  }
}
