import { t as translate } from "../../../i18n";
import type { BoardKind } from "./boardDevices";
import type { BoardNames } from "./boardNames";

type Labelled = { name: string | null | undefined; kind: BoardKind; chip?: string };

// The child's own name for a board wins; an unnamed one is called by what USB says it is. A
// clone's bridge chip sits in other gadgets too, so it is not called an UNO outright. These
// nouns also sit inside other sentences, so each reads right both first and mid-sentence.
export function boardName({ name, kind, chip }: Labelled): string {
  if (name) return name;
  if (kind === "uno") return translate("blockLab.connect.card.unnamedUno");
  if (kind === "other") return translate("blockLab.connect.card.unnamedOther");
  return chip
    ? translate("blockLab.connect.card.unnamedChip", { chipName: chip })
    : translate("blockLab.connect.card.unnamedUsbSerial");
}

export const linkName = (link: { key: string; kind: BoardKind; chip?: string }, names: BoardNames): string =>
  boardName({ name: names[link.key], kind: link.kind, chip: link.chip });

// A board found by its serial number comes back in any socket; one found by its port name
// only in the same socket.
export const lostBodyKey = (followsAcrossPorts: boolean): string =>
  followsAcrossPorts ? "blockLab.board.lostBody.anySocket" : "blockLab.board.lostBody.sameSocket";

export function boardDetail({ name, kind, chip, port }: Labelled & { port: string }): string {
  if (kind === "usb-serial" && name && chip) {
    return translate("blockLab.connect.card.portChip", { port, chipName: chip });
  }
  if (kind === "usb-serial" && !name) return translate("blockLab.connect.card.portMaybeUno", { port });
  return translate(name ? "blockLab.connect.card.port" : "blockLab.connect.card.portUnnamed", { port });
}
