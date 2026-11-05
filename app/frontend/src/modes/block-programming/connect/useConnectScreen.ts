import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { isBoard, returnedBoard, type BoardDevice, type BoardLink } from "./boardDevices";
import type { CardStatus } from "./connectView";
import type { BoardConnection } from "./useBoardConnection";
import { useUnplugSearch } from "./useUnplugSearch";

type ConnectPhase = "idle" | "naming" | "ok";
export type ConnectHint = "waitLost" | "inUse" | "busy" | "pickFirst" | "nameFirst";
type DisconnectedBoard = Pick<BoardLink, "key" | "kind" | "chip">;

// The design returns to the blocks by itself 2 seconds after the board is chosen.
export const OK_RETURN_MS = 2000;
const HINT_MS = 2600;

export function useConnectScreen(conn: BoardConnection, onClose: () => void) {
  const boards = useMemo(() => conn.devices?.filter(isBoard) ?? [], [conn.devices]);
  const others = useMemo(() => conn.devices?.filter(device => !isBoard(device)) ?? [], [conn.devices]);
  const search = useUnplugSearch(boards);
  const [phase, setPhase] = useState<ConnectPhase>("idle");
  const [picked, setPicked] = useState<string | null>(null);
  const [showOthers, setShowOthers] = useState(false);
  const [confirmingDisconnect, setConfirmingDisconnect] = useState(false);
  const [disconnected, setDisconnected] = useState<DisconnectedBoard | null>(null);
  const [nameDraft, setNameDraft] = useState("");
  const [hint, setHint] = useState<ConnectHint | null>(null);
  const [lostTips, setLostTips] = useState(false);
  const [cameBack, setCameBack] = useState(false);
  const hintTimer = useRef<number | undefined>(undefined);
  const { link } = conn;

  const found = search.state?.step === "found" ? search.state : null;
  const foundBoard = found ? boards.find(board => board.port === found.port) : undefined;
  // A single UNO on this computer is chosen for the child, as in the design. A lone clone
  // bridge may be some other gadget, so the child picks that one.
  const onlyUno = !link && !search.state && boards.length === 1 && boards[0].kind === "uno" ? boards[0] : undefined;
  const pickedBoard: BoardDevice | undefined = foundBoard ?? boards.find(board => board.key === picked) ?? onlyUno;
  // While the chosen board is lost, any listed card is another board, even on its old port.
  const switching = !!link && !!pickedBoard && (link.lost || pickedBoard.key !== link.key);
  const returned = returnedBoard(link, boards);

  const showHint = useCallback((next: ConnectHint) => {
    window.clearTimeout(hintTimer.current);
    setHint(next);
    hintTimer.current = window.setTimeout(() => setHint(null), HINT_MS);
  }, []);
  useEffect(() => () => window.clearTimeout(hintTimer.current), []);

  // The chosen board's card came back after its cable was out: the design says so and goes
  // back to the blocks by itself.
  const lost = link?.lost ?? false;
  const hasLink = link !== null;
  const wasLost = useRef(lost);
  useEffect(() => {
    if (wasLost.current && !lost && hasLink) {
      setCameBack(true);
      setLostTips(false);
      setPhase(current => (current === "idle" ? "ok" : current));
    }
    if (lost) setCameBack(false);
    wasLost.current = lost;
  }, [lost, hasLink]);

  useEffect(() => {
    if (phase !== "ok") return;
    const timer = window.setTimeout(onClose, OK_RETURN_MS);
    return () => window.clearTimeout(timer);
  }, [phase, onClose]);

  useEffect(() => {
    const onKeyDown = (event: KeyboardEvent) => {
      if (event.key !== "Escape" || event.defaultPrevented) return;
      const target = event.target instanceof Element ? event.target : null;
      // Esc in a text field belongs to the field.
      if (target?.closest("input, textarea, select, [role=menu]")) return;
      // Handled here so the window chrome does not also leave fullscreen.
      event.preventDefault();
      if (confirmingDisconnect) setConfirmingDisconnect(false);
      else onClose();
    };
    document.addEventListener("keydown", onKeyDown);
    return () => document.removeEventListener("keydown", onKeyDown);
  }, [confirmingDisconnect, onClose]);

  const pick = (key: string, status: CardStatus) => {
    if (phase !== "idle") showHint("busy");
    else if (status === "lost") showHint("waitLost");
    else if (status === "inUse") showHint("inUse");
    else {
      setPicked(key);
      setDisconnected(null);
    }
  };

  const connect = () => {
    if (phase !== "idle") return;
    if (!pickedBoard) {
      showHint("pickFirst");
      return;
    }
    conn.connect(pickedBoard);
    search.cancel();
    setPicked(null);
    setDisconnected(null);
    setCameBack(false);
    if (conn.names[pickedBoard.key]) setPhase("ok");
    else {
      setNameDraft("");
      setPhase("naming");
    }
  };

  const saveName = () => {
    if (!link) return;
    if (!nameDraft.trim()) {
      showHint("nameFirst");
      return;
    }
    conn.nameBoard(link.key, nameDraft);
    setPhase("ok");
  };

  const disconnectNow = () => {
    if (!link) return;
    setDisconnected({ key: link.key, kind: link.kind, chip: link.chip });
    setPicked(link.key);
    setConfirmingDisconnect(false);
    setCameBack(false);
    conn.disconnect();
  };

  const startSearch = () => {
    if (phase !== "idle") {
      showHint("busy");
      return;
    }
    setPicked(null);
    setDisconnected(null);
    search.start();
  };

  // Closing the box after a find keeps that board chosen.
  const closeSearch = () => {
    if (foundBoard) setPicked(foundBoard.key);
    search.cancel();
  };

  return {
    conn,
    boards,
    others,
    search,
    phase,
    pickedBoard,
    switching,
    returned,
    showOthers,
    confirmingDisconnect,
    disconnected,
    nameDraft,
    hint,
    lostTips,
    cameBack,
    pick,
    connect,
    saveName,
    skipName: () => setPhase("ok"),
    setNameDraft,
    askDisconnect: () => setConfirmingDisconnect(true),
    cancelDisconnect: () => setConfirmingDisconnect(false),
    disconnectNow,
    toggleOthers: () => setShowOthers(shown => !shown),
    toggleLostTips: () => setLostTips(shown => !shown),
    startSearch,
    closeSearch,
    rescan: () => void conn.rescan(),
    close: onClose,
  };
}

export type ConnectScreenState = ReturnType<typeof useConnectScreen>;
