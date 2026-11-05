type CloseGuard = () => void | Promise<void>;
// `note` is the catalog key the close dialog shows while the window waits for this guard.
type RegisteredGuard = { guard: CloseGuard; note?: string };
const guards = new Set<RegisteredGuard>();

export function registerWindowCloseGuard(guard: CloseGuard, note?: string) {
  const entry = { guard, note };
  guards.add(entry);
  return () => { guards.delete(entry); };
}
export async function prepareWindowClose(onWait?: (note: string) => void) {
  const entries = [...guards];
  // Invoke all synchronously before awaiting: each participant first invalidates its queue.
  const tasks = entries.map(({ guard }) => {
    try { return Promise.resolve(guard()); }
    catch (error) { return Promise.reject(error); }
  });
  const note = entries.find(entry => entry.note)?.note;
  if (note) onWait?.(note);
  const results = await Promise.allSettled(tasks);
  const failed = results.find(result => result.status === "rejected");
  if (failed?.status === "rejected") throw failed.reason;
}
