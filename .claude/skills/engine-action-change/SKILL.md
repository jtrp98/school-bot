---
name: engine-action-change
description: Cross-layer checklist for adding, changing or routing an engine action in school-bot (engine validation + docs/API.md, contracts, the host's trusted mode→engine routing and per-mode action allowlist, frontend client/types/preview, error codes, tests). Use when a mode needs a new engine capability, a payload changes, or a mode is re-routed to another engine (for example Block Lab → mixly).
---

# Engine action change (school-bot)

AGENTS.md: "การเปลี่ยน action/payload ต้องปรับ engine validation, contract/types, ผู้เรียก และ tests ที่เกี่ยวข้องให้ตรงกัน". These are the places that must move together. Confirm each `file:line` before editing, because these notes can drift.

## 1. Engine (`engines/<id>/`)

- Validate the action and payload at the engine boundary: types, ranges, sizes, required fields. Reject unknown fields where the engine already does.
- Keep stdout for protocol JSON Lines only and send diagnostics to stderr. Echo `request_id`.
- Default device transport is dry-run. Executables come only from the engine's trusted `toolchains/` beside the binary, never from the payload.
- Document the action in `engines/<id>/docs/API.md`: payload, response, limits, error codes, and the dry-run versus native behaviour.
- Tests: zide uses `tests/protocol_test.cpp` and `tests/ipc.test.mjs`; mixly uses the `*_test.go` files and `tests/ipc.test.mjs`. Add a case for the new action and its rejection paths.

## 2. Shared contract (`contracts/engine/`)

- The envelope (`protocol`, `request_id`, `mode`, `action`, `data`, response status/error) is shared. Actions belong to each engine, so do not add a shared action enum.
- Change `request.schema.json` or `response.schema.json` only for envelope-level changes, such as progress events or new error codes. The host rejects responses that break `response.schema.json`, and zide's `tests/schema-check.mjs` validates against it.

## 3. Host (`app/internal/engine/`, `app/desktopservice.go`)

- Routing: `engineForMode()` in `app/internal/engine/manager.go` maps each mode to an engine id and a binary path beside the executable. On 2026-11-02: `robot-control` → zide, `coding-adventure` and `block-programming` → `demo-engine`; `mixly` is not routed.
- Allowlist: `modeActions` in the same file. The webview is untrusted, so an action missing here is unreachable even if the engine implements it.
- Budget: one active request, a 5-second deadline, and caps of 64 KB in, 1 MB out and 8 KB diagnostics. Long operations such as Arduino compile/upload (minutes) or serial monitoring need an explicit, reviewed per-action deadline and a cancellation design. Do not raise the global timeout silently.
- Bundling: build and desktop-packaging scripts must place the new engine binary (plus any `toolchains/`) beside the app, and its notices and licenses must ship too (`scripts/build-desktop.*`, `scripts/package-desktop.*`, the installer).
- Tests: `app/internal/engine/*_test.go` and `app/desktopservice_test.go`.

## 4. Frontend (`app/frontend/src/`)

- Call only `client.run(mode, action, data)` in `core/client.ts`. It goes to `DesktopService.Run` natively and to `core/preview.ts` when the URL has `?preview=1`.
- Types: `core/types.ts` (`ModeID`, `EngineResponse`, payload types). Add typed payload and response shapes and never use `any`.
- Preview: add a simulated branch in `core/preview.ts` that is clearly a simulation. It must never reach native.
- Errors: branch on `EngineError.code`, never on message text. Map codes to i18n keys (see the `i18n-strings` skill).
- Sample/preview handlers that fake success stay separate from the native path (AGENTS.md).

## 5. Docs and checks

- Update `engines/<id>/docs/API.md`, `engines/README.md` or `docs/ARCHITECTURE.md` when routing or guarantees change.
- Run the `verify-changes` skill: engine build with `-Test`, host `go test ./...` and `go vet ./...`, frontend typecheck/build, and i18n when UI text changed.
- Have `hardware-safety-reviewer` review anything that can reach a device or a native tool, and `rules-reviewer` review the whole diff.
