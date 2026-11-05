# Phase 4 Labs — local evidence (2026-11-04)

Implemented directly without BAAN. This report records local simulator verification, not release or hardware acceptance. Phase 4 remains open for native safety and macOS acceptance.

## Try the simulator

From `app/frontend`, run `npm run dev -- --port 5181`, then open:

`http://127.0.0.1:5181/?simulator=1&view=robot-control&screen=connect`

Choose Guest at the explicitly simulated entry screen. Select Simulator HID or Simulator Serial, review/confirm the six-servo profile, and connect. Create poses in the existing workspace, send/play a sequence, upload five poses (4+1), and read servo values. The simulator starts at PWM 1500 for unread joints. Every returned position is simulated, never a sensor measurement.

The simulator toolbar provides emergency stop/torque cutoff, cable-loss and timeout injection. Emergency cutoff cancels pending work and disables torque; restore the connection, explicitly enable simulator torque, then reconnect before resuming. Normal Stop drains pending work but is distinct from emergency cutoff. Faults never automatically retry movement.

Code Lab: open `?simulator=1&view=code-lab`, choose Guest, then run `hero.move_right(2)` and `hero.jump()`. The allowlisted DSL produces a visual trace only; it does not move the arm or evaluate Python/JavaScript.

Block Lab: open `?simulator=1&view=block-programming`, choose Guest, then connect move/jump/repeat blocks and run. It compiles to the same bounded trace. Workspace load validates a temporary headless workspace before replacing the editor.

Explicit `simulator=1` implies preview and routes all program/robot execution to memory. No native Wails/USB calls or native entitlement are provided by the simulator entry adapter. Ordinary native sessions do not fall back to simulation.

## Actual verification on Windows

- `npm run typecheck`: PASS.
- `npm run build`: PASS, including release asset inspection.
- `npm run test:i18n`: 14/14 PASS.
- `node --test app/frontend/tests/phase4-simulator.test.mjs app/frontend/tests/frontend-adapter.test.mjs app/frontend/tests/frontend-workspace.test.mjs app/frontend/tests/frontend-sample-workspace.test.mjs`: 59/59 PASS.
- `./scripts/engines/build.ps1 -Engine zide -Test -OutputDirectory .tmp/phase4-engine`: PASS after running with compiler cache access; initial restricted invocation failed with compiler `Unexpected`. Protocol: 344 checks; shared mock transport scenarios: PASS.
- `ZIDE_ENGINE=.tmp/phase4-engine/zide-engine.exe node --test engines/zide/tests/ipc.test.mjs`: 12/12 PASS against newly compiled source; all IPC requests use dry-run.

The regression covers profile mapping/reversed endpoints, six joint atomic reads, invalid PWM/time/groups, upload progress/failure, stale callbacks, queued motion, cancellation/drain/stop, close/unmount, torque release, HID/Serial simulator, timeout/disconnect, emergency latch/rearm, DSL bounds and Blockly serialization/nesting. Source for Darwin transport was added concurrently in this checkout; it was preserved and its shared mock tests/build were exercised, not claimed as authored here.

## Pending acceptance

- macOS Intel and Apple Silicon native compile/runtime: not run on this Windows machine. The existing CI matrix runs `scripts/verify.ps1` on both architectures; the simulator suite is now included in that script. No remote CI run was triggered.
- Interactive browser QA: in-app browser could not reach localhost (`ERR_CONNECTION_TIMED_OUT`); Chrome computer-use access timed out. No visual/end-to-end browser success is claimed.
- Native engine enforcement of the entire confirmed six-servo profile and physical emergency cutoff remain open. Absolute command validation and existing native stop/read/torque regression are not proof of those requirements.
- No robot was attached, no USB motion/firmware writes occurred, and no physical torque cutoff/stop, calibration or real sensor accuracy was verified. Desktop EXE/installer was not rebuilt.
- `QA-004` in the canonical plan belongs to Phase 5 learning/grading; it was not relabelled or closed. Labs safety coverage is recorded under `QA-003`.

Implementation tasks with locally tested frontend behavior are `done-local`; backend/native and cross-platform QA remain `in-progress`. No gate or external approval was closed.
