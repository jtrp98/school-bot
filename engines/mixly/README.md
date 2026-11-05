# Mixly engine

Standalone School Bot engine with ID `mixly`, derived from the local
`sb-app-mixly` source. It uses the original Mixly JavaScript generators in a
Goja runtime embedded in a Go executable. The generators are not rewritten as
Go templates. Electron and Node.js are not needed to run the built engine.
Its module, dependencies, source snapshot, tests and builds are independent of
`app/` and Zide. Goja is needed to execute the existing JavaScript; the serial
dependency replaces Electron's native serial module.

The desktop host routes `block-programming` here with `board` and `platform` fixed to
`arduino_avr` / `Arduino/Genuino Uno`: code generation (`program.generate`,
`project.validate`), `device.list` (which only enumerates ports and opens none), and
`program.compile`/`program.upload` with `transport` fixed to `native`, School Bot blocks
only (no raw `source`; fields the generators paste into the C++ take only the values their
blocks offer, see `docs/API.md`), and for upload the port with the picked board's USB IDs
(never a port Robot Control drove while that port is still listed). The Serial monitor
(`serial.open`/`read`/`write`/`close`) goes to one long-lived engine process with the same
port and USB ID checks, at 9600 baud, and the host closes it before an upload.
`serial.exchange` and the other device actions stay unrouted: the page could otherwise write
raw bytes to any serial port, the robot arm's included. The desktop build and installer ship this engine in
`engines/mixly/` with its `libraries/`. The browser preview has no engine.
No hardware operation is performed by building or running the automated tests.

## Build and verify

Requires Go **1.26 or newer** (the repository's portable SDK is supported), and
Node.js to run the IPC tests. `go.mod`/`go.sum` pin the engine's dependencies.

```powershell
# From the repository root:
./scripts/build-engine.ps1 -Engine mixly -Test
# Use an isolated output if another build is running:
./scripts/build-engine.ps1 -Engine mixly -Test -OutputDirectory ./engines/mixly/bin/check
```

```bash
bash scripts/build-engine.sh mixly --test
# Cross-compile a standalone Mac binary (run tests on the matching platform):
ENGINE_ARCH=arm64 bash scripts/build-engine.sh mixly /tmp/mixly-arm64
```

Default output: `bin/<os>-<arch>/mixly-engine[.exe]`, manifest, executable SHA256,
notices, license texts and the Arduino `libraries/` (C/C++ sources on purpose: arduino-cli
compiles them). No loose generator sources, source maps, SDK or debug symbols are copied
into this payload. Platform builds must run on the
target OS; `ENGINE_ARCH` selects its architecture. The engine is not yet signed.

```powershell
'{"protocol":"1.0","request_id":"boards","mode":"block-programming","action":"board.list","data":{}}' |
  ./engines/mixly/bin/windows-amd64/mixly-engine.exe
```

## Implemented engine operations

- Original generators and configuration for all 17 board families present in
  the supplied source: AVR, ESP32/ESP8266 variants, MicroPython, micro:bit/Mithon,
  MixGo AI, desktop Python and Skulpt profiles.
- Code generation from a JSON block tree or Blockly `.mix` XML, with variables,
  loops, conditionals, procedure arguments, next/statement/value connections,
  comments, disabled blocks and shadow precedence.
- Arduino compile/upload using an Arduino CLI from fixed locations (see below);
  Python syntax compilation using trusted CPython; MicroPython main-file upload
  using trusted mpremote.
- Serial port listing, bounded read/write exchanges, and a Serial monitor session that
  keeps the picked board's port open across requests of one engine process (docs/API.md,
  "Serial session").
- ESP32-family and K210 firmware command presets from the upstream board config,
  executed as argument arrays without a shell. Every firmware asset and executable
  is checked before the first erase command.
- JSON Lines 1.0, matching IDs, validation, bounded input/output and dry-run defaults.

Action payloads, limits and examples: [API](docs/API.md).

## Native toolchains

Native operations require tools installed by the developer/distributor under
`toolchains/` **beside the built engine**, never executable paths from a request
or project. Recognised filenames are `arduino-cli`, `python3`, `mpremote`,
`esptool`, `kflash` (`.exe` on Windows). Dependent runtimes and libraries must
be supplied there too. Arduino CLI may use a distributor-provided
`toolchains/arduino-cli.yaml` for core/library locations.

School Bot does not ship Arduino CLI. Without `toolchains/arduino-cli`, the engine
uses the one inside an installed Arduino IDE 2.2 or later (fixed install folders only,
never PATH; on Windows both the `.exe` installer's `Arduino IDE` folder and the `.msi`/winget
`arduino-ide` folder) with the IDE's own settings. A request cannot choose the CLI, but
compiles then use the teacher's own Arduino install, which School Bot does not vet; see
[API](docs/API.md#toolchains).

The Arduino libraries the Block Lab blocks need ship as source in `libraries/` (Servo,
Adafruit_NeoPixel, U8g2, IRremote and the ZLTech `dht`, `APDS9960` and `Adafruit_TCS34725`
forks; versions and licenses in [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md)). The build
copies the folder beside the engine and every compile passes each library with `--library`,
so they win over libraries the teacher installed. Vendored copies drop examples, docs and
images only; refresh them by copying the upstream library again with the same trimming and
updating the notices.

Firmware assets retain the upstream relative structure under
`toolchains/firmware/<board-id>/build/`; shared font assets use
`toolchains/firmware/micropython_common/build/`. Nothing downloads automatically.
`toolchain.status` reports executable availability and, for Arduino, whether the
`arduino:avr` core is installed; it does not check libraries or other packages.
Compile and upload deadlines, the per-user build folder and the error codes are in
[API](docs/API.md#compile-and-upload-arduino).
Missing native dependencies produce an error, never a simulated success. The
supplied source has no compiler/Electron distribution; it is not a ready-to-use
toolchain bundle.

## Remaining integration limits

Importing every board family's generators does not establish compatibility of
every upstream block. Browser canvas/font rasterisation, custom mutators beyond
the documented representation, editor-only controls, Skulpt execution, turtle/
AVR simulation, Python execution, cloud library management, automatic module
deployment and volume-copy micro:bit firmware installation are not implemented.
Blocks requiring unavailable browser facilities fail explicitly. Python compile
checks CPython syntax; it is not a MicroPython target build. MicroPython upload
copies `main.py` only and does not reset or run the board.

Arduino compile alone has a 110-second deadline; upload compiles within 75 seconds and then
uploads within 30, inside the host's two-minute budget for each action; upload passes `--verify`, so avrdude reads the flash
back. A cut-off upload can leave a partial program running until the next upload. Tool
cancellation
terminates the immediate child and bounds pipe draining. Windows launches tools
suspended and assigns them to a job that kills descendants on closure; other
platforms still need descendant containment. Hardware acceptance remains
unverified. Cancellation is not a physical stop.
OS serial write completion does not verify device acknowledgement or motion.

## Refresh the source snapshot

```powershell
./engines/mixly/scripts/import-source.ps1 -SourceDirectory C:/path/to/sb-app-mixly
./scripts/build-engine.ps1 -Engine mixly -Test
```

The importer selects generator dependencies, board profiles/configuration and
board pages used for script load order. It excludes application shell code,
server keys, toolchain binaries and unrelated UI/media. Imported files retain
their original content; compatibility code lives in `src/bootstrap.js` and Go.
Review imported file/hash changes and corresponding notices before distribution.
