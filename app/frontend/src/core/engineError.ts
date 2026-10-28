/** An error response from the engine or the desktop host. Callers branch on `code`, never on the text. */
export class EngineError extends Error {
  readonly code: string;
  /** Diagnostic text from the engine or host; not a localized UI message. */
  readonly detail: string;

  constructor(code: string, detail: string) {
    super(`${code}: ${detail}`);
    this.code = code;
    this.detail = detail;
  }
}
