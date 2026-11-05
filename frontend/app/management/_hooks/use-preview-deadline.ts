"use client";

import { useEffect, useState } from "react";

export function usePreviewDeadline(expiresAt?: string) {
  const [now, setNow] = useState(() => Date.now());

  useEffect(() => {
    if (!expiresAt) return;
    const timer = window.setInterval(() => setNow(Date.now()), 1000);
    return () => window.clearInterval(timer);
  }, [expiresAt]);

  return !!expiresAt && now >= Date.parse(expiresAt);
}
