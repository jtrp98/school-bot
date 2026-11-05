import { useEffect, useState } from "react";
import { unreadCount, type SerialLog } from "./serialLog";

// How many lines arrived while the Output panel was out of sight (drawer collapsed or the C++
// tab showing), for the number on the Output tab.
export function useOutputUnread(log: SerialLog, visible: boolean): number {
  const [seenUpTo, setSeenUpTo] = useState(log.nextId);
  useEffect(() => {
    if (visible) setSeenUpTo(log.nextId);
  }, [visible, log.nextId]);
  return visible ? 0 : unreadCount(log, seenUpTo);
}
