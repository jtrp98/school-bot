# Zide engine (C++17)

Headless extraction from `sb-app-zide`, intended for School Bot's Robot Control mode.
Integrated with the native Robot Control frontend through the trusted Go host.
It can also be built and exercised independently using dry-run requests.

```powershell
# From repository root
./scripts/build-engine.ps1 -Engine zide -Test
node --test engines/zide/tests/ipc.test.mjs
```

Executable on Windows x64: `bin/windows-amd64/zide-engine.exe`.
Each engine owns its build scripts, build intermediates, outputs, tests, dependencies,
and documentation. To build this engine directly, run `engines/zide/scripts/build.ps1`.
Send protocol 1.0 JSON Lines on stdin;
responses are JSON Lines on stdout. Device transport defaults to dry-run.

Includes servo/action-group commands, group INI import/export, Windows HID and
serial I/O, and bootloader passthrough protection. Uses the original project's
HIDAPI Windows source with documented fixes. No Qt runtime is required.

Full contract, examples, source lineage, lifecycle and limits:
[docs/API.md](docs/API.md).
Dependency notices: [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).

Both commands above must pass. Tests never exercise hardware writes or motion.
Firmware upgrades and macOS hardware transport are not included in this extraction.

Desktop wiring and operation: [INTEGRATION.md](docs/INTEGRATION.md).
