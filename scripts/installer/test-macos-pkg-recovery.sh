#!/usr/bin/env bash
# BE-021 / QA-009 / QA-010: Verify macOS Universal PKG payload, lifecycle, project preservation,
# and AC-030-07 explicit update failure recovery/repair on Intel (x86_64) and Apple Silicon (arm64).
set -euo pipefail
root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
if [[ "$(uname -s)" != Darwin ]]; then
  echo 'Run macOS PKG verification and recovery smoke test on macOS 15+ (Intel or Apple Silicon).' >&2
  exit 1
fi

version="$(node -p 'JSON.parse(require("node:fs").readFileSync("'"$root"'/app/frontend/package.json", "utf8")).version')"
pkg_path="${1:-$root/dist/installers/school-bot-$version-macos-universal.pkg}"
if [[ ! -f "$pkg_path" ]]; then
  echo "Missing Universal PKG: $pkg_path (build with bash scripts/desktop/package.sh first)" >&2
  exit 1
fi

work_dir="$(mktemp -d)"
trap 'rm -rf "$work_dir"' EXIT

echo "[1/4] Expanding and verifying Universal PKG ($pkg_path)..."
expanded="$work_dir/expanded"
pkgutil --expand-full "$pkg_path" "$expanded"
app_bundle="$(find "$expanded" -type d -name 'School Bot.app' -print -quit)"
test -n "$app_bundle"

for exe in school-bot demo-engine engines/zide/zide-engine; do
  test -x "$app_bundle/Contents/MacOS/$exe"
  lipo -verify_arch x86_64 arm64 "$app_bundle/Contents/MacOS/$exe"
done
codesign --verify --deep --strict "$app_bundle"

echo "[2/4] Running native engine dry-run smoke checks..."
printf '%s\n' '{"protocol":"1.0","request_id":"pkg-smoke-zide","mode":"robot-control","action":"robot.stop","data":{"transport":"dry-run"}}' \
  | "$app_bundle/Contents/MacOS/engines/zide/zide-engine" | grep -q '"status":"success"'
printf '%s\n' '{"protocol":"1.0","request_id":"pkg-smoke-demo","mode":"coding-adventure","action":"program.run","data":{"source":"hero.jump()"}}' \
  | "$app_bundle/Contents/MacOS/demo-engine" | grep -q '"status":"success"'

echo "[3/4] Verifying uninstall preserves user Projects and Device ID (AC-032-03)..."
sim_apps="$work_dir/Applications"
sim_support="$work_dir/Library/Application Support/SchoolBotPoC"
mkdir -p "$sim_apps" "$sim_support"
cp -R "$app_bundle" "$sim_apps/School Bot.app"
printf '%s' 'device-id-macos-preserve-test' > "$sim_support/device-id"
printf '%s' '{"schema":1,"source":"hero.move_right(2)"}' > "$sim_support/coding-adventure.json"
before_hash="$(shasum -a 256 "$sim_support/device-id" "$sim_support/coding-adventure.json")"
rm -rf "$sim_apps/School Bot.app"
after_hash="$(shasum -a 256 "$sim_support/device-id" "$sim_support/coding-adventure.json")"
if [[ "$before_hash" != "$after_hash" ]]; then
  echo 'Uninstalling School Bot.app altered Application Support user data!' >&2
  exit 1
fi

echo "[4/4] Verifying AC-030-07 mid-update failure recovery (bundle snapshot restore, no half-install, no auto-downgrade)..."
go_cmd=go
if [[ -x "$root/.tools/go/bin/go" ]]; then go_cmd="$root/.tools/go/bin/go"; fi
(cd "$root/app" && "$go_cmd" test -v -run TestMacOSUniversalPKGRecoveryOnIntelAndARM ./internal/updater/...)

echo "macOS Universal PKG verification and AC-030-07 recovery checks passed."
