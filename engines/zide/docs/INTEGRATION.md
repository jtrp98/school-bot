# Robot Control desktop integration

Zide is integrated into Arm Lab. One desktop installer bundles the
application and both built-in engines. The engine source remains self-contained.

```text
React Robot Control
  → core/client.ts (Wails Run / CancelRun)
  → Go DesktopService
  → trusted Engine Manager routing
  → engines/zide/zide-engine[.exe]
  → Windows HID or Serial
```

Adventure and Blockly route to the existing demo engine. Frontends cannot supply
executable paths or choose an arbitrary executable. Registry entries are host code;
engine.json documents metadata and is not an executable trust decision.
The host also keeps a per-mode action allowlist (`modeActions` in
`app/internal/engine/manager.go`) and rejects any other action before starting an
engine, so the webview cannot reach engine actions its UI does not use.

## Use and configuration

Build the desktop with `scripts/build-desktop.ps1 -SkipInstall` on Windows when
dependencies are installed. The script builds and places the C++ engine plus its
manifest, hash and notices in `app/bin/engines/zide/`. The same folder is installed
under the application's installation directory by the MSI. macOS builds include
the engine in `Contents/MacOS/engines/zide/` but native hardware transport remains
Windows-only. Installer signing remains the existing release-pipeline responsibility.

Open Arm Lab → connect → choose an enumerated HID/Serial target. Configure the six
servo IDs and the PWM value at each end of each UI range. Reversed PWM endpoints
support reversed servo direction. The defaults are a nominal example, not measured
arm calibration. Confirm that the profile matches the device before connecting.
A `$DRS!` board probe must actually receive a response; connecting never moves the arm.

The app starts disconnected with an empty native workspace. Preview/scenario views
retain their sample data separately and never dispatch hardware requests. The old
four-slider simulator explicitly requests dry-run. Connection state means the last
verified selected target; each engine request opens/closes its own device. Idle
discovery checks detect an unplugged target; operation errors also surface loss.

## Supported operations

- Send a pose through `group.execute`; only start the visual prediction after the
  engine reports successful OS transmission. The picture is not measured telemetry.
- Joint edits are coalesced and sent one at a time. A stop discards pending edits
  and invalidates callbacks so a late response cannot start another motion.
- Play an application sequence by sending one group per pose, then waiting its
  configured duration. Normal completion describes dispatched steps and elapsed
  durations, not physical completion feedback.
- Upload all poses in batches of four, reporting progress only after each response.
  A failure aborts later batches. Uploaded data is labeled unverified because board
  storage is not read back for verification.
- Run uploaded groups on the board. No completion event is available; the UI keeps
  board execution active until an explicit stop rather than inventing completion.
- Read servo responses by matching the configured ID and validating PWM against
  the profile, then converting it back to UI coordinates. Incomplete, missing or
  invalid sensor replies leave pose data unchanged and report an error.
- Explicit torque release/lock. There is no simulated drift or timed automatic
  torque behavior on the native path.
- Save/load a versioned validated native project through the existing local Go
  project store. Sample stash and native stash are separate.

## Stop, lifetime and limits

The host permits one request at a time with a five-second timeout. The C++ transport
budget is four seconds. HID opens include a bootloader check that can take 1.5 s;
manual updates therefore coalesce rather than repeatedly retrying a busy engine.

Stop first invalidates local playback/queued updates, cancels and reaps the active
child, then sends `$DST!` to the selected device. Reopening HID for that stop still
waits for CHECK_FW silence (about 1.5 s) before `$DST!` can be written, in addition
to cancellation/drain and OS/device delays. CHECK_FW remains before passthrough
because skipping it could write TRAN_DATA to bootloader flash. The host protects an
already-active `robot.stop` from concurrent navigation/close cancellation, while
retaining its five-second deadline. Inherited pipes can add at most one second of
drain after child exit/kill. A successful write is not proof
that a moving arm stopped. Communication loss can prevent delivery; physical
emergency-stop provisions are still required for actual hardware acceptance.

Navigation/unmount discards pending callbacks and attempts a stop; it does not
automatically replay a pose on reconnect. Native pause/resume is explicitly
unavailable; use stop and restart. HOME, BOX and POSE0 motion shortcuts remain
unavailable until actual calibrated presets are provided. Firmware upgrades are
not part of this integration.

## Verification

Automated checks use fake bridges for frontend lifecycle/error cases and dry-run
for the real C++ process. The Go integration test exercises the actual trusted
registry and an injected test resolver against the bundled C++ executable. No test
transmits a hardware motion command. Device movement, calibration, unplug/replug,
and emergency-stop behavior need acceptance testing with the physical robot.

Hardware acceptance must separately check:

- CHECK_FW/stop latency from a running motion, including cancellation and reopening HID.
- Serial stale input after cancellation: verify the next servo read belongs to the new request.
- Serial target identity and firmware-mode behavior before changing port or probe policy.
- A kill/timeout between HID reports of a download, followed by stop: verify the board
  does not append the stop to an incomplete buffered command or continue queued motion.
- Unplug/replug during a partial write and broadcast torque lock/release on the selected board.

These cases do not justify changing serial purge, CHECK_FW ordering or board buffering
assumptions without physical evidence.

```powershell
node --test app/frontend/tests/frontend-adapter.test.mjs app/frontend/tests/frontend-workspace.test.mjs
./scripts/verify.ps1 -SkipInstall
./scripts/package-desktop.ps1 -SkipBuild
```

MSI install/repair/uninstall testing belongs on a disposable Windows account/CI;
building or inspecting a package does not assert that those steps have run locally.
