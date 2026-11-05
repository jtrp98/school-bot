import { runBlockEngine } from "../shell/blockEngine";

export type BoardKind = "uno" | "usb-serial" | "other";

// One serial port as the engine's device.list reports it. Listing opens no port, so a card
// says only what USB told the OS, never that the board answered.
export type BoardDevice = {
  key: string;
  // The key is the board's own serial number, so it is still this board in another socket.
  followsAcrossPorts: boolean;
  port: string;
  usb: boolean;
  vid?: string;
  pid?: string;
  serialNumber?: string;
  kind: BoardKind;
  chip?: string;
};

// Block Lab's programs open Serial at 9600 (the catalog has no block that changes it), so the
// screen shows that speed; it follows the program and is not a setting.
export const PROGRAM_BAUD_RATE = 9600;

const KINDS: readonly BoardKind[] = ["uno", "usb-serial", "other"];

type ListedPort = Omit<BoardDevice, "key" | "followsAcrossPorts">;

const isRecord = (value: unknown): value is Record<string, unknown> =>
  typeof value === "object" && value !== null && !Array.isArray(value);

const optionalText = (value: unknown): string | undefined =>
  typeof value === "string" && value !== "" ? value : undefined;

function parsePort(value: unknown): ListedPort | null {
  if (!isRecord(value) || typeof value.port !== "string" || value.port === "") return null;
  const kind = KINDS.find(known => known === value.kind) ?? "other";
  return {
    port: value.port,
    usb: value.usb === true,
    vid: optionalText(value.vid),
    pid: optionalText(value.pid),
    serialNumber: optionalText(value.serial_number),
    kind,
    chip: kind === "usb-serial" ? optionalText(value.chip) : undefined,
  };
}

// A port key carries the USB IDs, so a different gadget later plugged into that socket does
// not inherit the board's name.
function boardKey(port: ListedPort, serialIsUnique: boolean): string {
  const ids = `${port.vid ?? ""}:${port.pid ?? ""}`;
  if (serialIsUnique) return `${ids}:${port.serialNumber}`;
  return port.usb ? `${ids}@${port.port}` : port.port;
}

// The key follows one physical board. Its USB serial number moves with it to another socket,
// but many clone bridges (CP210x, FTDI) report the same serial on every board, so a serial
// shared by two ports here says nothing and the port is used instead.
function withKeys(ports: ListedPort[]): BoardDevice[] {
  const serials = new Map<string, number>();
  for (const { serialNumber } of ports) {
    if (serialNumber) serials.set(serialNumber, (serials.get(serialNumber) ?? 0) + 1);
  }
  return ports.map(port => {
    const serialIsUnique = !!port.serialNumber && serials.get(port.serialNumber) === 1;
    return { ...port, key: boardKey(port, serialIsUnique), followsAcrossPorts: serialIsUnique };
  });
}

export function parseDeviceList(data: unknown): BoardDevice[] {
  if (!isRecord(data) || !Array.isArray(data.devices)) {
    throw new Error("device.list returned no devices array");
  }
  return withKeys(data.devices.map(parsePort).filter((port): port is ListedPort => port !== null));
}

export const isBoard = (device: Pick<BoardDevice, "kind">): boolean => device.kind !== "other";

export async function listBoardDevices(): Promise<BoardDevice[]> {
  const response = await runBlockEngine<unknown>("device.list", {});
  return parseDeviceList(response.data);
}

// The board the child chose. Nothing is opened on it: "connected" means "this is our board",
// and "lost" means its card is gone from the list (the cable is out).
export type BoardLink = BoardDevice & {
  since: number;
  lost: boolean;
};

// Only a board with the same USB ids counts as ours coming back: another gadget on the same
// COM name, or a list where the OS gave no USB details, is not. A serial shared with a newly
// plugged clone turns the key into a port key, so the same port and serial still match.
function isLinkedBoard(link: BoardLink, device: BoardDevice): boolean {
  if (!isBoard(device) || device.vid !== link.vid || device.pid !== link.pid) return false;
  if (device.key === link.key) return true;
  return device.port === link.port && device.serialNumber === link.serialNumber;
}

// The link takes the matched card as it is now, so its key, port and whether it follows the
// board to another socket always describe the current list. A board without a serial of its own
// that comes back may be another gadget on the same bridge, such as the robot arm's controller
// in that socket, so it stays lost until the child chooses its card again.
export function followLink(link: BoardLink | null, devices: readonly BoardDevice[]): BoardLink | null {
  if (!link) return link;
  const match = devices.find(device => isLinkedBoard(link, device));
  if (!match || (link.lost && !link.followsAcrossPorts)) return link.lost ? link : { ...link, lost: true };
  if (!link.lost && match.key === link.key && match.port === link.port) return link;
  return { ...link, ...match, lost: false };
}

// The listed card that looks like the lost board back in its socket, which followLink leaves for
// the child to confirm.
export const returnedBoard = (link: BoardLink | null, devices: readonly BoardDevice[]): BoardDevice | undefined =>
  link?.lost ? devices.find(device => isLinkedBoard(link, device)) : undefined;
