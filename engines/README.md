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
scripts/
  build-engine.ps1          # selects an engine by ID, then delegates to its script
  build-engine.sh
```

## Build one engine

```powershell
./scripts/build-engine.ps1 -Engine zide -Test
node --test engines/zide/tests/ipc.test.mjs
```

```bash
bash scripts/build-engine.sh zide --test
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
  routing table: robot-control → Zide, coding-adventure/block-programming → demo.
  Adding a source directory or manifest does not authorize executing a new engine.

The existing `app/cmd/demo-engine` remains the PoC runtime for Adventure and Blockly.
Zide remains a separate C++ implementation used by native Robot Control. Desktop
builds collect their runtime files into one app and installer. The root request/
response schema paths are compatibility references to the common envelopes.

Engines:

- [Zide](zide/README.md), C++17, robot-control; routed and bundled by the desktop.
- [Mixly](mixly/README.md), Go with original Mixly JavaScript generators; standalone
  engine only, with its own build/tests. It is not yet routed or bundled by the desktop.

```powershell
./scripts/build-engine.ps1 -Engine mixly -Test
```

Release source/symbol handling and its limits are described in
[Code protection](../docs/CODE-PROTECTION.md). Compiled/embedded files remain
inspectable; code confidentiality is not equivalent to license enforcement.
