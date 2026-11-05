# Architecture

```text
React UI (one active mode)
  ├── Robot Simulator
  ├── Phaser Game
  └── Blockly
       │ core/client.ts
       ▼
Go DesktopService (thin Wails adapter)
  ├── local project store
  ├── HTTP API client (local PoC) → C# Api → Application → Domain
  │                                       ↑
  │                               Infrastructure store adapter
  └── Engine Manager ──JSON Lines──┬── robot-control → C++ Zide
                                  ├── adventure program.run → demo-engine
                                  ├── blocks program.generate / project.validate / device.list /
                                  │   program.compile / program.upload → mixly
                                  └── blocks serial.open / read / write / close → mixly session
                                      (one long-lived process outside the request slot)
```

## Boundaries

Standalone engine sources live in `engines/<engine-id>/`; each owns its language,
dependencies, build scripts, tests, docs and platform outputs. Root build-engine
scripts only dispatch by ID. Shared manifest metadata lives in `contracts/engine/`.
Mode and engine identity are separate; they are not a one-to-one mapping.
See `engines/README.md` for the workspace convention. The host's trusted registry
routes Robot Control to Zide, Coding Adventure to the demo engine and Block Lab's code
generation, port list (which opens no port), compile, on Windows upload, and the chosen
board's Serial monitor (one long-lived engine process) to Mixly.

Frontend modes never choose executable paths or access license credentials outside the call.
`internal/engine` has no Wails imports. A replacement Host can reuse protocol/engine unchanged.
Mode ownership is separate from engine implementation language. The manager locates
built-in binary paths only beside the running executable, including `engines/zide`;
the working directory is never searched, so a development run must use the built
executable in the build output directory. It never accepts frontend executable paths.

`Run` creates a process per request, handles one response, and terminates on cancellation or its
route's deadline: five seconds, or two minutes for Block Lab's `program.compile` and
`program.upload` (counted once the Serial session has let go of the port).
Input capped at 64 KB, stdout at 1 MB, diagnostic output at 8 KB. No shell is used.
Only one request can be active. No networking or filesystem API is exposed by the teaching DSL.
Block Lab's Serial monitor is the one exception to a process per request: its
`serial.open`/`read`/`write`/`close` go to one long-lived engine process outside that slot
(`internal/engine/session.go`), five seconds per request, killed on a timeout or failure, and
ended by `serial.close`, before an upload, after 15 seconds without a request, and by Cancel
once the slot is free; it refuses `serial.open` while a compile or upload runs.
Cancel waits until the current child exits and releases its slot. An already-active
`robot.stop` is allowed to finish despite navigation/close cancellation; its five-second
deadline remains in effect. So is a Block Lab `program.upload` (two-minute deadline): an upload
stopped half way leaves the board without a whole program, so only `StopUpload`, Block Lab's
confirmed stop, ends one, and a page that lost its own guard cannot cut it off. `StopUpload` ends
an upload and nothing else, so a late stop cannot end the next request, and `UploadRunning` lets
the close dialog say why closing waits. While an upload holds the slot, a Robot Control
`robot.stop` gets `ENGINE_BUSY` and a Cancel waits, so a physical stop can wait up to about two
minutes behind it. Block Lab keeps the child in Block Lab until its send ends, and the host
refuses uploads to a port Robot Control used (`internal/engine/armports.go`).
`cmd.WaitDelay` bounds inherited-pipe draining to one second
after child exit or context cancellation. Robot Control
then submits a separate physical `$DST!` request; cancellation alone is not a stop.
Output beyond the caps is discarded without failing the child's writes, so a verbose engine is not
killed by a full pipe. Host failures are returned as responses with an `error.code`
(`ENGINE_BUSY`, `ENGINE_CANCELLED`, `ENGINE_TIMEOUT`, `ENGINE_FAILED`); the host also rejects engine
responses that break `contracts/engine/response.schema.json`. The frontend decides by code, not by
message text; codes are listed in `engines/zide/docs/API.md`.

Native Robot Control starts disconnected and probes a selected device before
accepting a six-joint calibration profile. UI samples/previews do not call hardware.
The native workspace serializes motion, invalidates pending work on stop/unmount,
and reports transmission separately from measured position or verified board storage.
See `engines/zide/docs/INTEGRATION.md` for its supported operations and limits.

This is **not a sandbox for arbitrary executable student code**. Child-tree cleanup, package verification,
dynamic discovery, persistent sessions beyond the Serial monitor, event streaming and protocol
capability negotiation are follow-ups.

## C# API

- Domain: immutable license/release records and invariants; no persistence/web dependencies
- Application: use cases, DTOs, `IManagementStore`, injected `TimeProvider`
- Infrastructure: process-local Memory Store; no EF dependency in Domain/Application
- Api: composition root, transport validation, auth/rate limits, error→HTTP mapping

No production business secret is embedded in Desktop. License activation key is sent per call and not persisted.
Admin key exists only in API environment. No CORS is required for the native Go client.
API ปัจจุบันใช้ HTTP บน localhost สำหรับพัฒนา. เป้าหมาย online คือ Desktop/Web→HTTP Gateway→gRPC SchoolBot→Application→Domain; browser ผ่าน same-origin BFF. ยังไม่มี gRPC integration ที่พร้อมใช้; ต้องกำหนด RPC/HTTP mapping, trusted JWT tenant และ opaque-session boundary ก่อน. Login HTTP เดิมของ Gateway/decoder และ engine JSON Lines/local hardware คงเดิม ไม่เปิด direct public SchoolBot fallback.
Desktop distribution builds embed the public API address with `SCHOOL_BOT_BUILD_API_URL`;
`SCHOOL_BOT_API_URL` remains a runtime override for development. See `DEPLOYMENT.md`.

## Offline and storage

Mode execution and Project Save/Load never call the API. User config directory `SchoolBotPoC` holds
one JSON project per mode plus a random hexadecimal device ID. Project reads check the
file size before allocation and retain a 1 MB bounded read against concurrent growth. Write uses temporary file + sync + rename.
This is a local file store, not encrypted student-data storage or a sync database.

Browser preview explicitly opts into simulation with `?preview=1`. Project data uses localStorage there;
Native behavior and performance must be measured in the Desktop executable, not preview.

## Product lifecycle

Wails is pinned to a Beta. Dependency updates require rerunning bridge, build and real-device checks.
The starter intentionally does not install unverified update packages or enforce online licensing.
API outage never blocks local learning. Signed offline entitlements and revocation policy need a separate design.
