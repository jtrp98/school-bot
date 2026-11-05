import type { BoardKind } from "./boardDevices";
import type { ConnectScreenState } from "./useConnectScreen";

// What the mascot says, picked in the order the design checks its states. Each case is one
// whole sentence pair in the catalogs.
export type MascotLine =
  | "lost"
  | "returned"
  | "back"
  | "naming"
  | "ok"
  | "connected"
  | "switching"
  | "scanFailed"
  | "scanning"
  | "disconnected"
  | "none"
  | "search"
  | "one"
  | "oneMaybe"
  | "many";

export function mascotLine(screen: ConnectScreenState): MascotLine {
  const { link, failure, devices } = screen.conn;
  if (link?.lost) return screen.returned ? "returned" : "lost";
  if (screen.cameBack) return "back";
  if (screen.phase !== "idle") return screen.phase;
  // A list that cannot be read says nothing about the chosen board, as the board chip shows.
  if (failure) return "scanFailed";
  if (link) return screen.switching ? "switching" : "connected";
  if (devices === null) return "scanning";
  if (screen.disconnected) return "disconnected";
  if (screen.boards.length === 0) return "none";
  if (screen.search.state) return "search";
  if (screen.boards.length > 1) return "many";
  return screen.boards[0].kind === "uno" ? "one" : "oneMaybe";
}

export type CardStatus = "lost" | "inUse" | "picked" | "free";
export type CardTag = "lost" | "returned" | "inUse" | "disconnected" | "ours" | "oursMoved";

export type BoardCardView = {
  id: string;
  key: string;
  port: string;
  kind: BoardKind;
  chip?: string;
  name: string | null;
  status: CardStatus;
  tag: CardTag | null;
};

function tagOf(screen: ConnectScreenState, card: Pick<BoardCardView, "key" | "port" | "status">): CardTag | null {
  if (card.status === "lost") return "lost";
  if (card.status === "inUse") return "inUse";
  if (card.key === screen.returned?.key) return "returned";
  if (screen.disconnected?.key === card.key && !screen.conn.link) return "disconnected";
  const found = screen.search.state?.step === "found" ? screen.search.state : null;
  if (found?.port === card.port) return found.portChanged ? "oursMoved" : "ours";
  return null;
}

// The chosen board's card stays in the row while its cable is out, so the child sees which
// one is missing, until a board with its USB ids is listed in its socket again; every other
// card is a port the engine listed just now. After a failed scan the row is the last list read,
// so no card claims the board is in use.
export function boardCards(screen: ConnectScreenState): BoardCardView[] {
  const { link, names, failure } = screen.conn;
  const linkedKey = link && !link.lost && !failure ? link.key : null;
  const listed = screen.boards.map(board => {
    const status: CardStatus =
      board.key === linkedKey ? "inUse" : board.key === screen.pickedBoard?.key ? "picked" : "free";
    return { id: board.key, key: board.key, port: board.port, kind: board.kind, chip: board.chip, status };
  });
  // Another gadget can take the lost board's port name, so the ghost card has its own id.
  const lost = link?.lost && !screen.returned
    ? [{
      id: `lost:${link.key}`,
      key: link.key,
      port: link.port,
      kind: link.kind,
      chip: link.chip,
      status: "lost" as const,
    }]
    : [];
  return [...lost, ...listed].map(card => ({ ...card, name: names[card.key] ?? null, tag: tagOf(screen, card) }));
}

export const clockText = (time: number) => {
  const date = new Date(time);
  return `${String(date.getHours()).padStart(2, "0")}:${String(date.getMinutes()).padStart(2, "0")}`;
};
