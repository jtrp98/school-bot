# Engine workspace

Each production engine lives in `engines/<engine-id>/` and can use its own language
and build toolchain. An engine is independent of the desktop UI and other engines.

```text
engines/
  README.md
  zide/
    engine.json             # identity, version, supported protocol/modes
    README.md
    CMakeLists.txt          # engine-specific build system
    src/                    # C++ implementation
    include/
    third_party/            # this engine's pinned dependencies and licenses
    tests/
    scripts/build.ps1       # builds this engine only
    scripts/build.sh
    docs/                   # API, integration and limits
    .build/<os>-<arch>/      # ignored compiler intermediates
    bin/<os>-<arch>/         # ignored distributable binary + manifest + licenses
contracts/engine/
  manifest.schema.json      # shared engine metadata structure
  request.schema.json       # common envelope; actions belong to each engine
  response.schema.json
scripts/engines/
  build.ps1                 # selects an engine by ID, then delegates to its script
  build.sh
```

## Build one engine

```powershell
./scripts/engines/build.ps1 -Engine zide -Test
node --test engines/zide/tests/ipc.test.mjs
```

```bash
bash scripts/engines/build.sh zide --test
# Engine-specific direct build and test:
bash engines/zide/scripts/build.sh /tmp/zide-output --test
```

Build scripts at repository root contain no engine-specific compiler or dependency
logic. To add another engine, add a sibling directory with a unique lowercase ID,
an `engine.json`, and build scripts with the same arguments. Do not copy Zide code
or introduce an empty placeholder engine to register future work.

## Ownership and contracts

- Mode IDs describe UI profiles; engine IDs describe implementations. Keep them
  separate: one mode may use different engines and one engine may support several modes.
- `contracts/engine/manifest.schema.json` defines shared metadata. The request and
  response schemas in the same folder define the common envelopes, which
  use `protocol`, `request_id`, `mode`, `action`, `data` and matching responses.
- Each engine documents and validates its own actions/payloads. Do not grow a shared
  action enum with every vendor/device-specific command.
- Dependencies stay with their engine until there is a demonstrated reason to share
  them. C++ is not required for future engines.
- Frontends call the host and never choose executable paths. The host owns a trusted
  per-action routing table (`app/internal/engine/routes.go`): robot-control → Zide,
  coding-adventure `program.run` → demo, block-programming `program.generate` /
  `project.validate` → mixly with the board fixed to Arduino UNO, its `device.list`
  (port names and USB IDs, no port opened) → mixly, and `program.compile` / `program.upload`
  → mixly with transport fixed to native, Block Lab's own block types only (no C++ source, and
  the engine refuses field values the blocks do not offer where they reach the C++) and a
  two-minute deadline; upload is routed on Windows only, where cancel kills avrdude too, and the
  host refuses it (`SAFETY_LIMIT`) to a serial port Robot Control drove until a Block Lab port
  list no longer shows that port, because a clone UNO and the arm's controller can share one
  USB bridge chip. Block Lab's Serial monitor (`serial.open`/`read`/`write`/`close`) → one
  long-lived mixly process outside the request slot (`app/internal/engine/session.go`), with
  the same port refusal; the host ends it before an upload and when the mode is left.
  Adding a source directory or manifest does not authorize executing a new engine.

The existing `app/cmd/demo-engine` remains the PoC runtime for Adventure.
Zide remains a separate C++ implementation used by native Robot Control. Desktop
builds collect their runtime files into one app and installer. The root request/
response schema paths are compatibility references to the common envelopes.

Engines:

- [Zide](zide/README.md), C++17, robot-control; routed and bundled by the desktop.
- [Mixly](mixly/README.md), Go with original Mixly JavaScript generators; block-programming
  code generation, port list, compile and (Windows only) upload, routed and bundled by the desktop.

```powershell
./scripts/engines/build.ps1 -Engine mixly -Test
```

Release source/symbol handling and its limits are described in
[Code protection](../docs/CODE-PROTECTION.md). Compiled/embedded files remain
inspectable; code confidentiality is not equivalent to license enforcement.
