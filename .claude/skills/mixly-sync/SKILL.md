---
name: mixly-sync
description: Bring Mixly source changes from the sb-app-mixly repo into school-bot's engines/mixly snapshot with the repository's importer (import-source.ps1), review the manifest/hash and notice changes, and run the engine tests. Use when generators, board config or Blockly core changed upstream in sb-app-mixly (e.g. new SB_* blocks, UNO-only config, Thai merge) or when engines/mixly/third_party must be refreshed.
---

# Mixly source sync (sb-app-mixly → engines/mixly)

`engines/mixly/third_party/mixly/` is a hashed snapshot of `C:\src\sb-app-mixly\resources\app\src`. Never hand-edit it. Compatibility code belongs in `engines/mixly/src/bootstrap.js` or Go.

sb-app-mixly is a read-only prototype (user decision, 2026-11-02). Never change it to get a School Bot behaviour. School Bot generator changes, such as `sb_loop` or new `SB_*` blocks, go in an overlay inside `engines/mixly`, outside `third_party/`. Sync only to refresh upstream Mixly code.

## Before

1. Make sure the user wants the sync. It rewrites vendored files and `source-manifest.json`.
2. Check the source repo state: `git -C C:\src\sb-app-mixly status --short` and `git -C C:\src\sb-app-mixly log --oneline -5`. Uncommitted changes there, such as the Thai merge seen on 2026-11-02, would be imported as they are. Ask the user whether those should be committed first.
3. Record the baseline: `git -C C:\src\school-bot status --short engines/mixly`.

## Run (PowerShell, repo root)

```powershell
./engines/mixly/scripts/import-source.ps1 -SourceDirectory C:/src/sb-app-mixly
./scripts/build-engine.ps1 -Engine mixly -Test
```

The build needs Go at the version in `engines/mixly/go.mod` (≥ 1.26; portable in `.tools/go`) and Node. If either is missing, stop and report it. Pass `-OutputDirectory` only as an absolute path, because a relative path resolves inside `engines/mixly`.

## Review

- `git diff --stat engines/mixly`: expect changes only under `third_party/mixly/` and `source-manifest.json`. List the added, removed and changed files.
- The importer copies generators, board profiles/configuration and board pages for load order. It excludes application shell, server keys, toolchains and unrelated UI/media. If a needed file did not come across, report it. Do not copy it by hand.
- Run `generator_test.go` and `tests/ipc.test.mjs` for UNO. Generated C++ for `tests/fixtures/avr-serial.mix` must stay stable unless the change intends otherwise.
- Update `engines/mixly/THIRD-PARTY-NOTICES.md` and `licenses/` if new third-party code arrived (for example ZLTech libraries: check their licenses).
- Update `engines/mixly/README.md` or `docs/API.md` only if behaviour or limits changed.

## Report (Thai)

Source commit/state, number of files changed, test results, notices touched, and anything that was excluded or failed.
