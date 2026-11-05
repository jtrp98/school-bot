---
name: verify-changes
description: Pick and run the checks school-bot's AGENTS.md requires for the files that changed (frontend typecheck/build, i18n test, robot adapter/workspace tests, Go host test/vet, mixly/zide engine builds with tests, scripts tests), check prerequisites first, and report the results honestly in Thai. Use after any code, i18n, contract or script change and before handing work to the user.
---

# Verify changes (school-bot)

Run only the checks that the changed files need. Never claim a check passed unless you ran it and saw it pass. Never run hardware, upload, firmware or desktop-launch steps as part of verification.

## 1. List what changed

From the repo root: `git status --porcelain=v1 -uall` and `git diff --stat` (and `git diff --cached --stat`). Group the paths by the table below.

## 2. Map paths to checks

| Changed paths | Run (working directory) | Needs |
|---|---|---|
| `app/frontend/**` (TS, TSX, CSS, config) | `npm run typecheck` then `npm run build` (`app/frontend`) | `node_modules` (`npm ci`) |
| Any UI text, i18n key or `app/frontend/src/i18n/locales/*.json` | `npm run test:i18n` (`app/frontend`) | `node_modules` |
| Robot behaviour: `app/frontend/src/modes/robot-control/**`, adapter code in `src/core/**` | `node --test app/frontend/tests/frontend-adapter.test.mjs app/frontend/tests/frontend-workspace.test.mjs app/frontend/tests/frontend-sample-workspace.test.mjs` (repo root) | Node ≥ 22.13 (`stripTypeScriptTypes`) |
| Go host: `app/*.go`, `app/internal/**`, `app/cmd/**`, `app/go.mod` | `go test ./...` and `go vet ./...` (`app`) | Go ≥ version in `app/go.mod`; portable `.tools/go/bin/go.exe` is preferred by the scripts |
| `engines/mixly/**` | `./scripts/build-engine.ps1 -Engine mixly -Test` (repo root, PowerShell; an `-OutputDirectory` must be absolute) | Go ≥ version in `engines/mixly/go.mod`, Node |
| `engines/zide/**` | `./scripts/build-engine.ps1 -Engine zide -Test` then `node --test engines/zide/tests/ipc.test.mjs` | CMake + C++17 toolchain; set `ZIDE_ENGINE` if the binary is not in `engines/zide/bin/<os>-<arch>/` |
| `contracts/**` | Host Go tests + both engine test runs above | as above |
| `scripts/**` | the matching `scripts/tests/*.test.ps1`; for packaging changes `scripts/package-source.ps1 -OutputDirectory <tmp>` | PowerShell |
| Integration or release work | `./scripts/verify.ps1 -SkipInstall -OutputDirectory "$env:TEMP\school-bot-verify"` | everything above; check no School Bot process is using the output first |
| Docs or comments only | No build. Check the diff and every link or path the docs mention | — |

Known gaps (verify again before relying on them):
- `scripts/verify.ps1` ends with `dotnet test` inside `web/`, but `web/` is not in this checkout, so the full script fails at its last step. Report that step as "ข้าม: ไม่มี web/" and do not count it as a pass.
- `verify.ps1` does not run `frontend-sample-workspace.test.mjs` or the mixly engine tests. Run them yourself when their areas change.

## 3. Check prerequisites before running

- Go: portable go1.26.10 is at `.tools/go/bin/go.exe` (installed 2026-11-02, not on PATH). The PowerShell scripts find it themselves. In Git Bash, prefix `PATH="/c/src/school-bot/.tools/go/bin:$PATH" GOPATH='C:\src\school-bot\.tools/gopath' GOCACHE='C:\src\school-bot\.tools/gocache'`.
- Go checks in `app` need `app/frontend/dist`, because of `//go:embed`. Run `npm run build` first. The verify script passes `-ldflags "-X schoolbot.local/app.appVersion=<version>"` to `go test`; do the same.
- Windows PowerShell 5.1 (no `pwsh` here) aborts a script when `go` writes `go: downloading` to stderr on a cold module cache. Warm the cache in Git Bash first.
- `go test` in `app` creates an untracked `go.work.sum` at the repo root. Report it; do not commit it unasked.
- Frontend: does `app/frontend/node_modules` exist? If not, ask the user before running `npm ci`, because it downloads packages.
- Zide: `cmake --version` and a C++ compiler (MSVC through `vswhere`).
- A missing tool is reported as `ตรวจไม่ได้: <เหตุผล>`. It is never a pass.

## 4. Do not

- Overwrite a binary the user may have open. Use `-OutputDirectory` for builds.
- Kill user processes, install, uninstall, or run hardware actions.
- Use git commands that change state.

## 5. Report in Thai

```
แก้: <ไฟล์/ส่วนที่เปลี่ยน สั้นๆ>
ตรวจแล้ว:
- <คำสั่ง> → ผ่าน | ไม่ผ่าน (<บรรทัด error ที่สำคัญ>)
ตรวจไม่ได้:
- <คำสั่ง> → <เหตุผล เช่น ไม่มี Go / ไม่มี node_modules / ไม่มี web/>
ยังไม่ได้ทดสอบ: UI ในแอปจริง (WebView2) / ฮาร์ดแวร์จริง / EXE-installer ยังไม่ rebuild
```

Keep build/test results separate from manual UI or hardware testing. Never imply that hardware or the real WebView2 app was tested when only builds ran.
