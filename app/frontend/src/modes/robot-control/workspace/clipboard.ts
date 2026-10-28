// Clipboard access can be missing or denied inside the embedded WebView. The copied text is always shown on screen
// as well, so a failed copy must not interrupt the workflow.
export function copyTextToClipboard(text: string): void {
  try {
    navigator.clipboard.writeText(text).catch(() => undefined);
  } catch {
    // navigator.clipboard is undefined outside secure contexts.
  }
}
