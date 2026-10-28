# Zide C++ engine

Extracted from the original Zide Qt application and integrated
with `app/frontend/src/modes/robot-control` through the desktop Go host. The engine
remains independently buildable. Desktop builds and installers bundle it under
`engines/zide`; see [INTEGRATION.md](INTEGRATION.md).

## Build and test

Windows requires either CMake + Visual Studio's C++ workload **including Windows SDK**,
or a Zig C/C++ compiler. `scripts/build.ps1` uses the portable Zig in ignored
`.tools/zig-x86_64-windows-0.15.2` when present, otherwise `zig` from `PATH`.
No engine code is written in Go or Zig. Dependencies are vendored, so build does
not fetch code from the internet.
The Windows Zig build explicitly targets `x86_64-windows-gnu` with `-mcpu=baseline`
so release CPU requirements do not depend on the build machine's detected CPU.
The CMake/MSVC path keeps its own toolchain defaults and needs separate validation.

```powershell
./scripts/build-engine.ps1 -Engine zide -Test
node --test engines/zide/tests/ipc.test.mjs
```

Output: `engines/zide/bin/windows-amd64/zide-engine.exe`. No Qt installation, window or bundled
HID DLL is needed. `-OutputDirectory <path>` changes the output location.
`CMakeLists.txt` is also provided for conventional C++ builds and CTest.

On macOS/Linux, `bash scripts/build-engine.sh zide` builds dry-run/compilation support.
Pass an output directory and `--test` to run protocol tests. Native HID/serial
transport in this extraction is Windows-only; other platforms return a clear error.
Those OS builds are not covered by the Windows tests.

## JSON Lines contract

One JSON request per stdin line; exactly one JSON response per line on stdout.
Diagnostics use stderr. Envelope matches School Bot protocol `1.0`; only
`mode: "robot-control"` is supported. Requests are capped at 65536 bytes per line and
request IDs at 128 bytes. Unknown fields, noninteger values, malformed groups and unsafe
PWM values return structured errors. No shell commands or script evaluation are used.

Every response satisfies `contracts/engine/response.schema.json`. A line longer than
65536 bytes gets an `INVALID_REQUEST` error, the rest of that line is discarded unread
and the engine keeps serving the next line. A `request_id` that is not a string of
1..128 bytes is not echoed: the error response carries `request_id: ""`.

### Error codes

Callers decide by `error.code`; `message` is an English diagnostic and may change.

| Code | Sent by | Meaning |
|---|---|---|
| `INVALID_REQUEST` | engine | The line is not JSON, or longer than 65536 bytes |
| `PROTOCOL_INVALID` | engine | Not a request object, unknown envelope field, protocol other than `1.0`, bad `request_id`, wrong `mode` or `action` type |
| `INVALID_INPUT` | engine | Missing, mistyped or out-of-range `data` field, unknown `data` field, malformed group |
| `ACTION_UNKNOWN` | engine | `action` is not one of the actions below |
| `SAFETY_LIMIT` | engine | Well-formed servo command whose PWM **including bias** is outside 500–2500 |
| `PROJECT_INVALID` | engine | `project.import` / `project.export` content cannot be represented |
| `TRANSPORT_ERROR` | engine | Device enumeration or the HID/serial exchange failed; delivery may be partial, do not retry movement automatically |
| `DEVICE_TIMEOUT` | engine | The command was written but a reply-expecting action received no bytes within `read_ms` |
| `ENGINE_BUSY` | host | Another engine request is still running |
| `ENGINE_CANCELLED` | host | The request was cancelled by the user before it finished |
| `ENGINE_TIMEOUT` | host | The engine did not finish within the host's 5-second budget |
| `ENGINE_FAILED` | host | The engine could not start, exited abnormally, or broke the contract; the message ends with a bounded `stderr:` excerpt when the engine wrote one |

The desktop host also forwards the demo engine's `PROGRAM_INVALID`, `COMMAND_UNKNOWN` and
`STEP_LIMIT`. The full list is the `errorCode` enum in the response schema.

```powershell
'{"protocol":"1.0","request_id":"test-1","mode":"robot-control","action":"servo.set","data":{"id":0,"pwm":1500,"time_ms":1000}}' |
  ./engines/zide/bin/windows-amd64/zide-engine.exe
```

For device commands, default `transport: "dry-run"` returns compiled command text plus full 64-byte HID
reports as arrays of bytes, with `simulated: true`. It never opens an output device.
The example compiles `#000P1500T1000!`.

Every device command accepts:

| Option | Meaning |
|---|---|
| `transport` | `dry-run` (default), `hid`, or `serial` |
| `device` | Exact HID path returned by `device.list`; optional only if exactly one Zide board exists |
| `port` | Serial COM1–COM9999; required for serial |
| `baud` | Default 115200; one of 9600, 19200, 38400, 57600, 115200, 128000, 230400, 256000, 460800, 512000, 921600 |
| `read_ms` | Response collection window 1–1500 ms; default 1000 |

`engine.info`, `project.import`, `project.export` and `device.list` reject all transport
options (`transport`, `device`, `port`, `baud`, `read_ms`). They report `transport: "none"`
and `simulated: false`; metadata/project operations do not simulate a device, while
`device.list` returns actual OS discovery. Windows HID discovery uses SetupAPI
interface paths and hardware IDs without opening HID output handles.

`device.list` enumerates devices without sending commands. `board.info` with an
explicit transport sends `$DRS!` and requires a response. Device paths cannot select
arbitrary files. HID is restricted to VID `0483` / PID `5750` and 64-byte reports.

| Action | Data fields in addition to transport options |
|---|---|
| `engine.info` | none; returns supported actions and limits |
| `device.list` | none |
| `servo.set` | `id`, `pwm`, optional `time_ms` (1000), `bias` (0) |
| `servo.read` | `id` |
| `servo.torque` | `enabled`, optional `id` 0–255 (default 255 broadcasts) |
| `robot.stop` | none; transmits `$DST!`; the response reports `stop_requested`, never a confirmed stop |
| `board.info` | none; transmits `$DRS!` |
| `group.execute` | `group` or `groups` with exactly one live group |
| `group.download` | `groups`: 1–8 groups per request, or a single `group` |
| `group.run` | `start`, `end` (0–9999, `end` ≥ `start`), `times` (1–9999) |
| `group.read` | `start`, `end` (0–9999, `end` ≥ `start`) |
| `project.import` | `content`: Zide INI text, up to 48000 bytes |
| `project.export` | `groups`: 1–128 groups; returns INI text |

A group is `{"index":0,"items":[{"id":0,"pwm":1500,"time_ms":1000}]}`.
Use explicit group indices (0–9999). At most 32 unique servo IDs per group.
Servo IDs are 0–254, PWM 500–2500 including bias, bias −500–500, time 0–9999 ms.
An empty download compiles the original `$$$$!` empty-row marker.

### Response `data` fields

Every success carries `engine`, `language`, `simulated` and `transport`. Device actions add:

| Field | Meaning |
|---|---|
| `commands` | Compiled command strings, in send order |
| `hid_reports` | Dry-run only: 64-byte HID reports as byte arrays |
| `delivery` | `dry-run` or `written` |
| `acknowledged` | Non-dry-run only; always `false`, since a write is never a motion confirmation |
| `exchange` | Non-dry-run only: `received` text, `received_hex`, `bytes_written`, `bytes_read` |
| `runs_on_device` | `group.run` only: `true` when the board was sent the command, `false` for dry-run |
| `stop_requested` | `robot.stop` only: always `true`; the stop request was issued, not confirmed |

`engine.info` returns `actions`, `hardware_platforms` (platforms with native device transport)
and `limits`: `servos_per_group`, `pwm_min`, `pwm_max`, `time_ms_max`, `command_bytes`.

For compatibility with the old four-slider simulator, `servo.set` also accepts
`servo: 1..4` plus `angle: 0..180`, mapped linearly to PWM 500..2500. This is a
nominal software adapter, **not measured arm calibration**. The future six-joint UI
should supply a calibrated ID/PWM profile instead of treating UI angles as physical
angles. This engine does not change that UI's joint limits or servo IDs.

`project.import/export` handles the numeric `[group]` section only. Empty Qt slots
`P0000T0000` are skipped. UI layout, bias settings outside the group, custom command
rows, IR/PS2/voice mappings and combine metadata are not silently interpreted.
Unsupported group commands fail explicitly; export does not claim full INI fidelity.
Export folds per-item bias into the target PWM; it does not retain separate calibration metadata.

## Device lifecycle and safety semantics

Each request opens and closes its device. Device operations have a four-second
budget, paced 200 ms between commands and 1 ms between HID packets. Large download
batches may reach the deadline; use smaller batches or a short read window.
CHECK_FW waits up to 1500 ms on every HID open before passthrough. A bootloader
response refuses transmission because TRAN_DATA would write flash. As in the source
application, silence implies application mode; this is not firmware authentication.

`delivery: "written"` means OS transmission completed, with `acknowledged: false`.
It does not assert motion completion or storage verification. No automatic movement
retry is performed after errors. Responses include device text, byte counts, and
`received_hex` for exact raw byte preservation. Text fragments are concatenated
without inserting line breaks.

`group.run` starts execution on the board. Killing/cancelling the engine does not
stop that execution. A physical stop requires an explicit `robot.stop` request to
the same device; loss of communication may prevent it from arriving. No physical
emergency-stop or hardware safety certification is claimed.

Firmware erase/write/upgrade, persistent sessions/events, completion acknowledgements,
macOS hardware transport are outside this extraction. Native frontend integration
is provided for Windows HID/serial; macOS retains explicit simulation.

## Source lineage and validation

- `actiongroup.cpp`, `actiongroupitem.cpp`: padded servo/group strings.
- `formsteerctl.cpp`: PWM, bias and duration bounds.
- `mainwindow.cpp`: live/download group suffixes and HID chunking.
- `bootloader.cpp`: report ID 2, TRAN_DATA header, big-endian address field.
- `html/app.js` and the source HID report: 56-byte chunk threshold, CHECK_FW guard,
  response fragments and queue pacing.
- Original HIDAPI Windows implementation and all alternative licenses are vendored;
  narrow correctness/lifecycle fixes are documented in the engine's notices.

Protocol golden tests and executable IPC tests run without sending any hardware
commands. Real USB/serial motion, unplug/replug behavior, stopping a real moving
arm and calibration still require device acceptance testing.

`scripts/build-engine.ps1 -Engine zide -Test` and `node --test engines/zide/tests/ipc.test.mjs`
must both pass. Tests cover all command builders, HID boundaries, INI roundtrips, JSON input
rejection, multi-request recovery and the request size bound. The build refuses a locked/in-use output with an instruction to use `-OutputDirectory`;
it does not stop the running engine. The build writes the binary,
a fresh copy of `engine.json` and the license bundle to `engines/zide/bin/windows-amd64`;
`engine.json` there is regenerated on every build, so an older copy means the engine was not rebuilt.
Go manager routing, frontend controller behavior and package layout are described
in [INTEGRATION.md](INTEGRATION.md).

Build dependencies: [nlohmann/json v3.12.0](https://github.com/nlohmann/json/releases/tag/v3.12.0),
[Zig 0.15.2 downloads](https://ziglang.org/download/). Windows serial configuration
follows [Microsoft's communications settings guidance](https://learn.microsoft.com/en-us/windows/win32/devio/modification-of-communications-resource-settings).
