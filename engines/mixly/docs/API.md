# Mixly JSON Lines API

Common envelope: [request](../../../contracts/engine/request.schema.json),
[response](../../../contracts/engine/response.schema.json).
Modes: `block-programming`, `coding-adventure`; engine ID is `mixly`.
Each UTF-8 request line produces exactly one response line. stdout is protocol
only; operational errors use the response `error.code`. Stream I/O failures go
to stderr and exit nonzero.

| Action | Data | Result |
| --- | --- | --- |
| `engine.info` | `{}` | Engine manifest and action list |
| `board.list` | `{}` | All 17 families, targets and original configuration |
| `board.info` | `{board}` | One family's targets and configuration |
| `toolchain.status` | `{}` | Availability of fixed executable names |
| `project.validate` | Program data with `xml` or `blocks` | Validates by generating code |
| `program.generate` | Program data with `xml` or `blocks` | `engine`, `board`, `platform`, `language`, `code` |
| `program.compile` | Program data with `source`, `xml` or `blocks` | `delivery`, `compiler`, `compiled`, `uploaded`, bounded diagnostics |
| `program.upload` | Same, plus `port` | Arduino compile/upload or MicroPython `main.py` copy |
| `device.list` | `{}` | Real serial port names; no ports are opened |
| `serial.exchange` | Serial data below | Written byte count and actual received bytes |
| `firmware.flash` | `{board, platform?, port, transport?}` | Executes the upstream command preset; `flashed`, `steps`, `delivery` |

Unknown fields are rejected. Requests cannot select executable paths, compiler
flags, arbitrary FQBNs, firmware files or shell commands. Target/platform names
are selected from the original configuration. If `platform` is omitted, the first
target in lexicographic order is used; set it explicitly for native work.
Native upload/flashing require an explicit platform when a family has multiple targets.
`options` are validated against that target's declared board option values.

## Program data

```json
{
  "board": "arduino_avr",
  "platform": "Arduino/Genuino Uno",
  "blocks": [{
    "type": "inout_digital_write2",
    "values": {
      "PIN": {"type": "math_number", "fields": {"NUM": "13"}},
      "STAT": {"type": "inout_highlow", "fields": {"BOOL": "HIGH"}}
    }
  }]
}
```

Use exactly one of `blocks`, `xml` (the text of a `.mix` file), or `source`.
Raw `source` is accepted only by compile/upload. `options` is an optional map,
for example `{"cpu":"atmega328old"}` for `Arduino Nano`. Raw source is never
evaluated by the JavaScript generator runtime; only the trusted embedded scripts
execute there. External compilers are not an OS security sandbox.

A JSON block has `type`, optional string-valued `fields`, maps `values` and
`statements` to child blocks, optional `next`, `mutation` attributes, `comment`,
`disabled`, and `arguments` (objects with `name` and optional Arduino `type`).
`mutation` supports `items`, `elseif`, `else`, `value` as in Mixly XML. XML
procedure `<arg name="x" type="int"/>` entries become `arguments`. Root variable
IDs are UI metadata; generators use the text names in block fields.

`transport` defaults to `dry-run` for compile/upload, serial and flashing.
Dry-run validates inputs and reports a plan; it returns `compiled:false`,
`uploaded:false`, `written:0` or `flashed:false`, without starting tools or
opening a port. Native access requires `"transport":"native"`.

`program.compile` with native Python uses `python3 -I -m py_compile`, not script
execution. Native Arduino invokes `arduino-cli compile` with a validated target.
Native MicroPython upload invokes `mpremote connect <port> fs cp <file> :main.py`;
dependent device modules must already exist. Compilation temporary files are
removed after completion; this API does not return or persist build artifacts.

## Serial data

```json
{
  "port": "COM3",
  "baud_rate": 115200,
  "write_base64": "aGVsbG8=",
  "read_bytes": 256,
  "timeout_ms": 500,
  "transport": "dry-run"
}
```

8N1; baud defaults to 115200, timeout to 500 ms. Native port names must match
current OS enumeration. Maximum write: 1024 bytes; read: 8192 bytes; read timeout:
1..2000 ms; baud: 50..3000000. A read returns the bytes received by one bounded
read, possibly fewer than requested. Ports close at the end of each request.
Opening serial may change modem control lines on some operating systems.
`device_acknowledged` remains false: sending bytes or receiving arbitrary data
does not establish acknowledgement of a device-specific operation.

## Firmware

Command presets are read from the embedded board configuration, using only
`esptool` or `kflash` under the trusted toolchains directory. The original flash
offsets, baud and reset flags are preserved. Tool/asset existence and port
selection are validated before executing any preset step. Files must resolve
inside `toolchains/firmware/`, including across symlinks. The request cannot
provide a file path. A completed tool process establishes tool-reported success,
not hardware acceptance. Volume-based flashing returns `INVALID_INPUT`.

## Bounds and errors

- 64 KB per input line, 1..128-byte request ID; oversized lines are drained so
  later requests still work. Invalid UTF-8 and unknown envelope fields fail.
- At most 512 blocks, 32 nested block connections, 64 procedure arguments and
  mutation counts of 0..512; XML is checked before tree
  allocation (96 element levels, 4096 elements). DTDs/processing instructions
  other than the initial XML declaration are rejected; external entities are not resolved.
- Each generation uses a fresh VM, a three-second execution deadline, 256 VM
  stack frames and a 128 KB generated-source limit. This is not a memory sandbox.
- Tool execution: two-minute deadline per command and one-second inherited-pipe
  draining budget; combined stdout/stderr retained up to 16 KB. Windows tool trees
  use a kill-on-close job; other platforms only terminate the immediate child. Calls are serial
  within one process. Termination/cancellation is not physical device stop.

Error codes use the shared response schema: `PROTOCOL_INVALID`, `INVALID_REQUEST`,
`INVALID_INPUT`, `PROJECT_INVALID`, `PROGRAM_INVALID`, `ACTION_UNKNOWN`,
`SAFETY_LIMIT`, `TRANSPORT_ERROR`, `ENGINE_FAILED`, `ENGINE_TIMEOUT`.
Clients decide by code and retain original compiler/device diagnostics.
