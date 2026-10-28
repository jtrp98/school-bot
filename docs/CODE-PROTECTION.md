# Protecting installed code

The goal is to avoid exposing frontend/backend source code on end-user computers.
This is code confidentiality, separate from license activation; no offline-license
duration or license enforcement is defined here.

## What the installed product contains

One installer contains the Desktop app and all enabled engines. Source ownership
remains separate under app/ and engines/<id>/.

- The React/TypeScript source is built to minified JavaScript/CSS with sourcemaps
  explicitly disabled. The Go app embeds frontend/dist in its executable.
  Original TS/TSX files are not required on a user computer.
  The frontend build checks its output and fails if original source files, PDBs,
  source maps, map references or development directories enter the embedded assets.
- Go desktop/host and C++ engines are native executables. Release Go builds use
  -trimpath and -s -w; the C++ build strips debug symbols and suppresses HIDAPI
  exports from the executable. Symbols/intermediates are developer artifacts.
- The C# web API runs on the server, as already designed; its source and deployment
  files do not belong in the Desktop installer.
- Installers select runtime files explicitly. Source code, source maps, PDBs,
  compiler objects, headers, tests, .git, development tools and private keys must
  not be added to user installation payloads. Keep required third-party notices.
- scripts/package-source.ps1 intentionally creates a developer source archive;
  it is not an installer and must not be sent to end users as the product package.

## Limits and placement of proprietary logic

The owner of a computer can inspect local processes and recover the JavaScript
that a WebView must execute. Embedding, minification, obfuscation or encrypting
assets with a key shipped in the same app cannot guarantee confidentiality.
Native binaries also remain subject to reverse engineering.

Keep privileged validation, valuable algorithms and secrets out of frontend code.
Offline robot/safety logic belongs in compiled engines or host code, with the
understanding that native code raises inspection cost rather than preventing it.
Logic that must never reach the user's machine must execute on a controlled server;
that portion then requires a network connection. Do not embed server secrets in any
local binary. Disabling developer tools can reduce casual inspection but is not an
authorization or confidentiality boundary.

No claim of complete anti-copy, anti-tampering or anti-decompilation protection is
made. Signed updates/installers establish publisher/integrity only when verified.
Device-bound licensing is a separate future requirement if copying-and-running
the installation on another machine also needs to be restricted.

## Verification

Run these on Windows; each must pass before a release payload is packaged:

- `npm run build` in `app/frontend` runs `scripts/check-release-assets.mjs`, which fails when
  original source files, PDBs, source maps, map references or development directories are
  in the embedded assets.
- `scripts/build-engine.ps1 -Engine zide -Test` and
  `node --test engines/zide/tests/ipc.test.mjs` run protocol and IPC tests in dry-run only;
  no hardware command is sent.
- Inspect the PE headers of the built executables: the C++ engine has no export table and no
  CodeView/PDB record (a zero-length reproducible-build debug entry is allowed), and the Go
  executables have no PE debug directory.
- macOS builds are not covered by the Windows checks and need their own inspection.

References: [Vite build sourcemap/minification options](https://vite.dev/config/build-options.html),
[Wails v3 embedded asset server](https://v3.wails.io/contributing/asset-server/),
[Microsoft executable file signing](https://learn.microsoft.com/en-us/windows/win32/secbp/understanding-pe-signatures).
