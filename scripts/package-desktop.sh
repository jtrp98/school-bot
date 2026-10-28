#!/usr/bin/env bash
set -euo pipefail
root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
if [[ "$(uname -s)" != Darwin ]]; then
  echo 'Build the macOS .pkg on a Mac with Xcode Command Line Tools.' >&2
  exit 1
fi
node "$root/scripts/check-notices.mjs"
skip_build=false
build_flags=(--universal)
while [[ $# -gt 0 ]]; do
  case "$1" in
    --skip-build) skip_build=true ;;
    --skip-install) build_flags+=(--skip-install) ;;
    *) echo 'Usage: bash scripts/package-desktop.sh [--skip-build] [--skip-install]' >&2; exit 1 ;;
  esac
  shift
done
if [[ "$skip_build" == false ]]; then bash "$root/scripts/build-desktop.sh" "${build_flags[@]}"; fi
bundle="$root/app/bin/darwin-universal/School Bot.app"
for executable in school-bot demo-engine engines/zide/zide-engine; do
  test -x "$bundle/Contents/MacOS/$executable"
  lipo -verify_arch x86_64 arm64 "$bundle/Contents/MacOS/$executable"
done
codesign --verify --deep --strict "$bundle"
version="$(/usr/libexec/PlistBuddy -c 'Print :CFBundleShortVersionString' "$bundle/Contents/Info.plist")"
output_dir="$root/dist/installers"
mkdir -p "$output_dir"
installer="$output_dir/school-bot-$version-macos-universal.pkg"
productbuild --component "$bundle" /Applications \
  --product "$root/app/build/macos/installer-requirements.plist" \
  --identifier local.schoolbot.poc.installer --version "$version" "$installer"
(cd "$output_dir" && shasum -a 256 "$(basename "$installer")" > "$(basename "$installer").sha256")
echo "Desktop installer ready: $installer"
echo 'Development .pkg is unsigned and contains an ad-hoc signed app. Developer ID signing and notarization are required before external distribution.'
