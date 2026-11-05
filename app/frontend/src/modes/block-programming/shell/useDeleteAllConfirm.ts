import { useCallback, useEffect, useState } from "react";
import { setDeleteAllConfirm } from "../blockly/deleteAll";

type Request = { blockCount: number; deleteAll: () => void };

// The question the workspace menu's "delete all blocks" asks before it deletes anything.
export function useDeleteAllConfirm() {
  const [request, setRequest] = useState<Request | null>(null);

  useEffect(() => {
    setDeleteAllConfirm((blockCount, deleteAll) => setRequest({ blockCount, deleteAll }));
    return () => setDeleteAllConfirm(null);
  }, []);

  const keep = useCallback(() => setRequest(null), []);
  const confirm = useCallback(() => {
    setRequest(null);
    request?.deleteAll();
  }, [request]);

  return { blockCount: request?.blockCount ?? null, keep, confirm };
}

export type DeleteAllConfirm = ReturnType<typeof useDeleteAllConfirm>;
