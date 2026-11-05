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
| `toolchain.status` | `{}` | Availability of fixed executable names; where `arduino-cli` was found and whether `arduino:avr` is installed |
| `project.validate` | Program data with `xml` or `blocks` | `engine`, `board`, `platform`, `problems` (see School Bot blocks) |
| `program.generate` | Program data with `xml` or `blocks` | `engine`, `board`, `platform`, `language`, `code` |
| `program.compile` | Program data with `source`, `xml` or `blocks` | Build result (see Compile and upload) |
| `program.upload` | Same, plus `port`; Arduino also `vid` and `pid` | Build result: Arduino compile then verified upload, or MicroPython `main.py` copy |
| `device.list` | `{}` | Real serial port names and what USB says about each; opens no port |
| `serial.exchange` | Serial data below | Written byte count and actual received bytes |
| `serial.open` / `read` / `write` / `close` | See Serial session | A port kept open across the requests of one engine process |
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

Upstream generators paste field text into the C++ and turn comments into `//` lines, so no
field name or value, mutation name or value, argument name or type, or comment, in any block
(values, statements and `next` chains alike), may contain a control character U+0000–U+001F
or U+007F, and a comment may not end in a backslash; such projects fail with
`PROJECT_INVALID`. A line break there would start a new line of C++ (an `#include` of a local
file whose text the compiler echoes in its errors), and a trailing backslash would join the
next generated line to the comment. On `arduino_avr`, text blocks escape `"` and `\` inside
the C++ string literal.

On Arduino boards the fields that upstream generators paste into the C++ as they are take only
the values their blocks offer, or the project fails with `PROJECT_INVALID` (`arduinoPastedFields`
in `project.go`): `math_number.NUM` is a number literal; `controls_delay.UNIT` is `delay` or
`delayMicroseconds`; `math_to_int.OP` is `round`, `ceil`, `floor`, `abs` or `sqrt`;
`variables_declare.TYPE` and argument types are `int`, `long`, `float`, `boolean` or `String`
(argument types may be empty, and procedure `TYPE` may also be empty or `void`);
`serial_select` is `Serial` and `serial_print.new_line` is `print` or `println`; variable,
function and argument names contain no comma, the one character Blockly's name cleaning keeps
besides letters, digits and `_`. `sb_*` generators check their own fields. Other upstream blocks
are not covered, so the compiler is no sandbox for a hostile project that uses them; the host
routes compile and upload for Block Lab's own block types only.

## School Bot blocks (`arduino_avr` only)

`src/school-bot-arduino.js`, `src/school-bot-devices.js` and `src/school-bot-modules.js`
add `sb_*` generators for Block Lab after the upstream ones. They never replace an
upstream type, so `.mix` files keep their meaning. Other boards reject `sb_*` types as
unknown. The toolbox contract is `contracts/block-lab/catalog.json`; exact output is
pinned by `testdata/school-bot/*.ino` (`go test -run TestSchoolBotGolden -update` after
an intended change, then review the diff).

Pins are dropdown field values. I/O pins are `2`..`12` and `A0`..`A5`: 0 and 1 carry
upload and Serial, and 13 flashes on every reset, so only `sb_led` may use 13. Analog
pins are `A0`..`A5`. PWM outputs (`sb_led_brightness`, the fan's `PIN_A`) use 5 or 6,
the only PWM pins that keep working beside Servo (Timer1: 9, 10) and tone/IR (Timer2:
3, 11). Generation also fails, with the reason, when a program uses one pin for two
kinds of use (input, output, NeoPixel, servo, tone, ultrasonic, or A4/A5 while an I2C
module is present), or uses the buzzer tone and the IR receiver together. These checks
keep children's programs on wiring that works; they are not a security boundary.

`project.validate` succeeds with these causes in `problems` instead, so Block Lab can list
them all before sending; `problems` is `[]` when the program generates. Each pin appears
once, with every use in the order found:

```json
{"problems": [
  {"code": "PIN_SHARED", "pin": "7", "uses": ["input", "output"]},
  {"code": "TIMER2_SHARED", "uses": ["tone", "ir"]}
]}
```

Uses are `input`, `output`, `tone`, `servo`, `neopixel`, `ultrasonic` and `i2c` (A4/A5);
Timer2 uses are `tone` and `ir`. Any other invalid program still fails with
`PROGRAM_INVALID`, and `program.generate`/`compile`/`upload` fail on these causes too.

| Type | Fields | Inputs | Output |
| --- | --- | --- | --- |
| `sb_loop` | — | statement `DO` | body of `loop()` |
| `sb_forever`, `sb_repeat`, `sb_wait_until` | — | `DO`; `TIMES`; `CONDITION` | `while (true)`, `for` from 0, empty `while (!(c))` |
| `sb_random` | — | `FROM`, `TO` | `random(from, to + 1)` (inclusive like Scratch) |
| `sb_text_letter`, `sb_text_contains` | — | `TEXT` + `POSITION` (from 1); `TEXT` + `PART` | `charAt(position - 1)`; `indexOf(...) != -1` |
| `sb_variable_change` | `VAR` | `DELTA` | `x = x + delta;` |
| `sb_button`, `sb_touch`, `sb_reed_switch`, `sb_sound`, `sb_light`, `sb_line_tracker`, `sb_collision`, `sb_vibration` | `PIN` I/O | — | `digitalRead`, `pinMode(INPUT)` in setup |
| `sb_potentiometer`, `sb_soil_moisture`, `sb_water_level`, `sb_grayscale`, `sb_thermistor` | `PIN` analog | — | `analogRead` |
| `sb_ir_obstacle`, `sb_tilt`, `sb_pir`, `sb_mq7`, `sb_mq3`, `sb_flame` | `PIN`, `MODE` `DIGITAL`/`ANALOG` | — | `digitalRead` or `analogRead` |
| `sb_active_buzzer`, `sb_relay`, `sb_led` | `PIN` (`sb_led` also 13), `STATE` `HIGH`/`LOW` | — | `digitalWrite`, `pinMode(OUTPUT)` |
| `sb_led_brightness` | `PIN` 5/6 | `BRIGHTNESS` 0..100 % (clamped) | `analogWrite(pin, 0..255)` |
| `sb_fan`, `sb_fan_stop` | `PIN_A` 5/6, `PIN_B`, `DIRECTION` `FORWARD`/`REVERSE` | `SPEED` 0..100 % (clamped) | L9110 drive / both pins low |
| `sb_buzzer_tone`, `sb_buzzer_tone_for`, `sb_buzzer_stop` | `PIN` | `FREQUENCY`, `DURATION` ms | `tone`, `noTone` |
| `sb_servo` | `PIN` | `ANGLE` | `Servo servo_<pin>` |
| `sb_rgb_setup` | `PIN`, `COUNT` 1..60, `BRIGHTNESS` 0..100 % | — | NeoPixel `rgb_display_<pin>`, `begin` and strip-wide `setBrightness` in setup |
| `sb_rgb_set` | `PIN` | `PIXEL` (from 1), `COLOR` | `setPixelColor`, `show` |
| `sb_rgb_color`, `sb_rgb_named_color` | —; `COLOR` `RED GREEN BLUE YELLOW CYAN MAGENTA WHITE OFF` or a mixed colour `#RRGGBB` (capital hex) | `RED`, `GREEN`, `BLUE` | packed 24-bit colour (`0xRRGGBB` for a mixed colour) |
| `sb_oled_print`, `sb_oled_clear` | `LINE` `1`..`4` | `TEXT` | U8g2 SSD1306 128×64 I2C page mode, four remembered lines |
| `sb_ultrasonic` | `TRIG`, `ECHO` | — | distance in cm (`checkdistance_<trig>_<echo>()`) |
| `sb_mpu6050` | `VALUE` `ACCEL_X/Y/Z` (0.01 g), `GYRO_X/Y/Z` (°/s) | — | MPU6050 registers over I2C 0x68; keeps the last values if the sensor does not answer |
| `sb_ir_received`, `sb_ir_button` | `PIN`; `BUTTON` hex code | — | IRremote legacy decode; `irValue == code` |
| `sb_dht11` | `PIN`, `VALUE` `TEMPERATURE`/`HUMIDITY` | — | `dht.h` from the ZLTech library |
| `sb_gesture_is`, `sb_gesture_light` | `DIRECTION` `DIR_*`; `CHANNEL` `RED GREEN BLUE AMBIENT` | — | APDS9960 (ZLTech library) |
| `sb_color_sensor` | `CHANNEL` `RED GREEN BLUE CLEAR` | — | Adafruit_TCS34725 with ZLTech's `getR/getG/getB/getC` |

The Arduino libraries these need ship with the engine in `libraries/` (see Compile and
upload): Servo, Adafruit_NeoPixel, U8g2, IRremote and the ZLTech `dht`, `APDS9960` and
`Adafruit_TCS34725` forks. `Wire` comes with the Arduino AVR core.

`transport` defaults to `dry-run` for compile/upload, serial and flashing.
Dry-run validates inputs and reports a plan; it returns `compiled:false`,
`uploaded:false`, `written:0` or `flashed:false`, without starting tools or
opening a port. Native access requires `"transport":"native"`.

`program.compile` with native Python uses `python3 -I -m py_compile`, not script
execution. Native MicroPython upload invokes `mpremote connect <port> fs cp <file> :main.py`;
dependent device modules must already exist. Python temporary files are removed after
completion. Native Arduino is described next.

## Compile and upload (Arduino)

Block Lab's host sends, with `board`, `platform` and `transport` fixed by the host:

```json
{"board": "arduino_avr", "platform": "Arduino/Genuino Uno", "transport": "native",
 "blocks": [...]}
{"board": "arduino_avr", "platform": "Arduino/Genuino Uno", "transport": "native",
 "blocks": [...], "port": "COM5", "vid": "1A86", "pid": "7523"}
```

The first is `program.compile`, the second `program.upload`. An Arduino upload needs `port`
(at most 256 bytes) and the `vid` and `pid` that `device.list` reported for the picked board
(four hex digits each, any case), in dry-run too. Any other action, and non-Arduino uploads,
refuse `vid`/`pid` with `INVALID_INPUT`. Both actions answer with the build result:

```json
{"engine": "mixly", "board": "arduino_avr", "platform": "Arduino/Genuino Uno",
 "compiler": "arduino-cli", "delivery": "native", "compiled": true, "uploaded": true,
 "output": "Sketch uses 1752 bytes (5%) of program storage space. ..."}
```

`delivery` is `dry-run` (nothing ran, `compiled` and `uploaded` false) or `native`. `output` is
the combined compile and upload output, at most 16 KB from each tool. Native steps:

1. Generate (three-second deadline) and find `arduino-cli` (see Toolchains).
2. For upload, list the ports as `device.list` does: `port` must be listed with kind `uno` or
   `usb-serial` and the request's `vid`:`pid`, otherwise `TRANSPORT_ERROR`. A clone's bridge
   chip also sits in other devices, so this proves the board was not swapped for another
   kind of device, not that it is an UNO.
3. Write the sketch to the per-user build folder `<temp>/school-bot-mixly/<fqbn>/` (for the UNO
   `arduino_avr_uno`; `program/program.ino` and `build/`), created with mode 0700. It is reused
   between requests, so an upload after a compile, or a small change, rebuilds only what
   changed (U8g2's fonts take tens of seconds from scratch). On macOS and Linux the folders
   must be real folders owned by this account and closed to others; on Windows real folders,
   not links. A lock file keeps a second compile or upload out until this one ends
   (`ENGINE_BUSY`); the OS releases it if the engine is killed.
4. `arduino-cli compile --fqbn <target> --build-path <build> --library <engine>/libraries/<name>...
   <sketch>`, 110-second deadline for `program.compile` and 75 seconds inside `program.upload`
   (Block Lab compiles first, so the upload's compile is mostly cached). Every folder in `libraries/` beside the engine executable is
   passed with `--library`, which wins over libraries the teacher installed; only that folder
   is read, never one from the request, and a missing or empty folder is `ENGINE_FAILED`.
   A `compiling` marker beside `build/` exists while the compile runs and stays if the compile is
   killed (by its deadline, or with the engine when the host cancels). The next compile then
   deletes the object files whose ELF section table is incomplete, with their `.d` files, and the
   `.a` archives, since a half-written object looks current to arduino-cli; finished objects stay,
   so a slow PC keeps what it compiled before its deadline.
5. Upload only: list the ports again as in step 2, then
   `arduino-cli upload --fqbn <target> --input-dir <build> --port <port> --protocol serial
   --verify <sketch>`, 30-second deadline. avrdude reads the flash back and compares it;
   `uploaded` becomes true only after that succeeded.

Each request stays under the host's two-minute deadline: 3 + 110 seconds for compile, 3 + 75 +
30 seconds for upload, plus one second of pipe draining per tool. Errors:

| Code | When |
| --- | --- |
| `TOOLCHAIN_MISSING` | `arduino-cli` not found, or a compile failed within 30 seconds and the `arduino:avr` core is not on disk |
| `COMPILE_FAILED` | `arduino-cli compile` exited non-zero; the message holds its output (at most 16 KB) |
| `UPLOAD_FAILED` | `arduino-cli upload` or its verify exited non-zero; the message holds its output |
| `TRANSPORT_ERROR` | The port is missing, not an UNO or USB-serial bridge, or has other USB IDs |
| `ENGINE_TIMEOUT` | A tool passed its deadline; after an upload timeout the board may hold a partial program |
| `ENGINE_BUSY` | Another School Bot compile or upload holds the build folder |
| `ENGINE_FAILED` | Bundled libraries missing, build folder unusable, or a tool that could not start |

## Toolchains

`arduino-cli` is looked for in fixed places only, never PATH, the working directory or
a request, in this order:

1. `toolchains/arduino-cli[.exe]` beside the engine, with `toolchains/arduino-cli.yaml` if present.
2. The CLI inside Arduino IDE 2.2 or later (`<install>/resources/app/lib/backend/resources/arduino-cli`):
   on Windows `%LOCALAPPDATA%\Programs` then `%ProgramFiles%`, each with the folder
   `Arduino IDE` (the IDE's `.exe` installer) then `arduino-ide` (its `.msi`, which winget uses); on
   macOS `/Applications/Arduino IDE.app/Contents/Resources` then the same under
   `~/Applications`. The IDE's `~/.arduinoIDE/arduino-cli.yaml` is passed when present, so
   compiles use the teacher's sketchbook and library folders. Linux has no fixed install.

The host starts the engine without the user's environment, so these folders come from the OS
(Windows known folders, the macOS account's home) and are handed to arduino-cli as
`USERPROFILE`/`LOCALAPPDATA`/`APPDATA` or `HOME`. Every run also sets
`ARDUINO_UPDATER_ENABLE_NOTIFICATION=false` so the CLI does not check for a newer release.
A request cannot choose the CLI, but compiles do use the teacher's own Arduino install
(libraries, `platform.local.txt`, hooks), which School Bot does not vet.

`toolchain.status` asks the CLI for its data folder (`arduino-cli config dump --format json`;
arduino-cli 1.x leaves defaults out of the dump, so then also
`arduino-cli config get directories.data --format json`; 10-second deadline each, so the host
must allow at least 20 seconds) and looks for
`packages/arduino/hardware/avr/*/platform.txt` there. It does not run `core list`, which
downloads the board and library indexes (about 60 MB) wherever they are missing:

```json
{"engine": "mixly", "tools": [{"name": "arduino-cli", "available": true}, ...],
 "arduino": {"found": true, "source": "arduino-ide", "avr_core": true}}
```

`source` is `toolchains` or `arduino-ide`. When the CLI is found but cannot say where its data
is, `avr_core` is false, `problem_code` is `ENGINE_TIMEOUT`, `TOOL_FAILED` or
`CONFIG_UNREADABLE`, and `problem` says why (at most 300 characters). The bundled libraries
are not checked here; compile reads them from `libraries/` beside the engine.

## Devices

`device.list` enumerates ports from the OS (SetupAPI/registry on Windows, IOKit on macOS,
sysfs on Linux) and opens none, so it never resets a board or disturbs a port another
program holds. On Linux it lists only `ttyACM*` and `ttyUSB*`, because the serial library's
own Linux list opens every `/dev/ttyS*` to probe it. The macOS build needs cgo for IOKit
(`scripts/build.sh` enables it):

```json
{"engine": "mixly", "ports": ["COM3", "COM5"], "devices": [
  {"port": "COM3", "usb": true, "vid": "2341", "pid": "0043", "serial_number": "8573...", "kind": "uno"},
  {"port": "COM5", "usb": true, "vid": "1A86", "pid": "7523", "kind": "usb-serial", "chip": "CH340"}
]}
```

`kind` is `uno` for the UNO's USB IDs in the Arduino AVR core (`2341:0043`, `2341:0001`,
`2A03:0043`, `2341:0243`), `usb-serial` for a bridge UNO clones use (`chip` is `CH340`,
`CH341`, `CH9102`, `CP210x` or `FTDI`), and `other` otherwise. Those bridges are in other
devices too, so `usb-serial` means "may be an UNO", not proof. Where the OS gives no USB
details, every port is `other`. `ports` lists the same names in the same order. When the
OS enumeration fails the action returns `TRANSPORT_ERROR` instead of a list of bare names,
so a caller never reads "no board" while one is plugged in.

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

8N1; baud defaults to 115200, timeout to 500 ms. Native port names must be in the current
`device.list` (on Windows: ports of the SetupAPI "Ports" class, not every SERIALCOMM entry). Maximum write: 1024 bytes; read: 8192 bytes; read timeout:
1..2000 ms; baud: 50..3000000. A read returns the bytes received by one bounded
read, possibly fewer than requested. Ports close at the end of each request.
Opening serial may change modem control lines on some operating systems.
`device_acknowledged` remains false: sending bytes or receiving arbitrary data
does not establish acknowledgement of a device-specific operation.

## Serial session

`serial.exchange` opens and closes the port in one request. A board's Serial monitor keeps it
open instead: the host runs one engine process with its stdin open and sends these requests
to it in turn (opening the port again for every read would restart an UNO each time, since its
reset follows DTR, and lose what it printed in between). The port closes on `serial.close` or
when stdin closes.

| Action | Data | Result |
| --- | --- | --- |
| `serial.open` | `{port, vid, pid, baud_rate?, transport?}` | Opens the port; `open`, `port`, `baud_rate` |
| `serial.read` | `{}` | Bytes received since the last read (`received_base64`, at most 16 KB), `open`, `dropped`, `problem` |
| `serial.write` | `{write_base64}` | Writes 1–256 bytes; `written` |
| `serial.close` | `{}` | Closes the port if one is open; `open: false` |

`serial.open` opens a port only with `transport: "native"` (the desktop host sets it); without it
the request is checked and answers `open: false`, `delivery: "dry-run"`. With it, the port is
checked as upload does (listed now, kind `uno` or `usb-serial`, the request's `vid`:`pid`,
otherwise `TRANSPORT_ERROR`), then opened 8N1 at `baud_rate` (default 9600, the rate Block Lab
programs use; 300–2000000, otherwise `SAFETY_LIMIT`) with DTR and RTS raised, so an UNO may
restart its program; so does any other device behind a CH340, which is why the host refuses a
port Robot Control drove in this run of the app. A port another program holds is
`TRANSPORT_ERROR`. Asking to open the port already open answers its state; another port while
one is open is `INVALID_INPUT`. Bytes that arrive in the first 250 ms after opening are dropped:
they are the noise of the UNO's reset, which would show as broken characters before the
program's first line (its bootloader waits about a second before the program prints). A board
that does not reset on DTR loses at most that much of its output. Received bytes wait in a
64 KB buffer: when nobody reads, the oldest go first and the next read says `dropped: true`. When the port fails by itself (the cable
came out), reading stops, the engine closes the port, `open` becomes false and `problem` says why; `serial.write` is then
`TRANSPORT_ERROR` until the port is opened again. `serial.read` and `serial.close` with no port
open answer `open: false`.

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
- At most 512 blocks, 32 levels of blocks inside blocks (a `next` row does not nest), 64 procedure arguments and
  mutation counts of 0..512; XML is checked before tree
  allocation (96 element levels, 4096 elements). DTDs/processing instructions
  other than the initial XML declaration are rejected; external entities are not resolved.
- Each generation uses a fresh VM, a three-second execution deadline, 4096 VM
  stack frames and a 128 KB generated-source limit. This is not a memory sandbox.
- Tool execution: Arduino compile 110 seconds alone or 75 seconds before an upload, upload 30 seconds, other tools two minutes per
  command (10 seconds for the `toolchain.status` settings query, whose stdout may be up to
  1 MB) and one-second inherited-pipe draining budget; combined stdout/stderr retained up to
  16 KB per command. Windows tool trees use a kill-on-close job; other platforms only terminate
  the immediate child. Calls are serial within one process. Termination/cancellation is not
  physical device stop.

Error codes use the shared response schema: `PROTOCOL_INVALID`, `INVALID_REQUEST`,
`INVALID_INPUT`, `PROJECT_INVALID`, `PROGRAM_INVALID`, `ACTION_UNKNOWN`,
`SAFETY_LIMIT`, `TRANSPORT_ERROR`, `TOOLCHAIN_MISSING`, `COMPILE_FAILED`, `UPLOAD_FAILED`,
`ENGINE_BUSY`, `ENGINE_FAILED`, `ENGINE_TIMEOUT`.
Clients decide by code and retain original compiler/device diagnostics.
