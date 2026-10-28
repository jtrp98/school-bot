type CloseGuard = () => void | Promise<void>;
const guards = new Set<CloseGuard>();

export function registerWindowCloseGuard(guard: CloseGuard) {
  guards.add(guard);
  return () => { guards.delete(guard); };
}
export async function prepareWindowClose() {
  // Invoke all synchronously before awaiting: each participant first invalidates its queue.
  const tasks = [...guards].map(guard => {
    try { return Promise.resolve(guard()); }
    catch (error) { return Promise.reject(error); }
  });
  const results = await Promise.allSettled(tasks);
  const failed = results.find(result => result.status === "rejected");
  if (failed?.status === "rejected") throw failed.reason;
}
